properties {
    [string]$baseDirectory         = resolve-path "../."
    [string]$artifactsDirectory    = "$baseDirectory/_artifacts"
    [string]$sourceDirectory       = "$baseDirectory/src"
    [string]$testDirectory         = "$baseDirectory/tests"
    [string]$toolsDirectory        = "$baseDirectory/.tools"
    [string]$nugetPackageDirectory = "$artifactsDirectory/NuGetPackages"
    [string]$testResultsDirectory  = "$artifactsDirectory/TestResults"
    [string]$solutionFile          = "$baseDirectory/ICU4N.sln"
    [string]$versionScriptFile     = "$baseDirectory/.build/version.ps1"
    [string]$testResultsFileName   = "TestResults.trx"
    [string]$icu4nResourcesProjectFile = "$sourceDirectory/ICU4N.Resources/ICU4N.Resources.csproj"
    [string]$icu4nResourcesNETFramework40ProjectFile = "$sourceDirectory/ICU4N.Resources.NETFramework4.0/ICU4N.Resources.NETFramework4.0.csproj"

    [string]$packageVersion        = ""  
    [string]$assemblyVersion       = ""
    [string]$informationalVersion  = ""
    [string]$fileVersion           = ""
    [string]$configuration         = "Release"
    [string]$platform              = "Any CPU"
    [bool]$backupFiles             = $true
    [string]$minimumSdkVersion     = "9.0.100"

    #test parameters
    [string]$testPlatforms         = "x64"
}

$backedUpFiles = New-Object System.Collections.ArrayList
$versionInfo = @{}

task default -depends Pack

task Clean -description "This task cleans up the build directory" {
    Remove-Item $nugetPackageDirectory -Force -Recurse -ErrorAction SilentlyContinue
    Remove-Item $testResultsDirectory -Force -Recurse -ErrorAction SilentlyContinue
    Get-ChildItem $baseDirectory -Include *.bak -Recurse | foreach ($_) {Remove-Item $_.FullName}
    Ensure-Directory-Exists $nugetPackageDirectory #For some strange reason, nbgv tool won't install without this directory present
}

task CheckSDK -description "This task makes sure the correct SDK version is installed" {
    # Check prerequisites
    $sdkVersion = ((& dotnet --version) | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command was not found. Please install .NET $minimumSdkVersion or higher SDK and make sure it is in your PATH."
    }
    $releaseVersion = if ($sdkVersion.Contains('-')) { "$sdkVersion".Substring(0, "$sdkVersion".IndexOf('-')) } else { $sdkVersion }
    if ([version]$releaseVersion -lt ([version]$minimumSdkVersion)) {
        throw "Minimum .NET SDK $minimumSdkVersion required. Current SDK version is $releaseVersion. Please install the required SDK before running the command."
    }
}

task Init -depends CheckSDK -description "This tasks makes sure the build environment is correctly setup" {  

    # Get the version info
    $versionInfoString = Invoke-Expression -Command "$versionScriptFile -PackageVersion ""$packageVersion"" -AssemblyVersion ""$assemblyVersion"" -InformationalVersion ""$informationalVersion"" -FileVersion ""$fileVersion"""
    Write-Host $versionInfoString

    # parse the version numbers and put them into a hashtable
    $versionInfoSplit = $versionInfoString -split '\r?\n' # split $a into lines, whether it has CRLF or LF-only line endings
    foreach ($line in $versionInfoSplit) {
        $kvp = $line -split '\:\s+?'
        $versionInfo.Add($kvp[0], $($kvp[1]).Trim())
    }
    $localInformationalVersion = $versionInfo['InformationalVersion']
    $localFileVersion = $versionInfo['FileVersion']
    $localAssemblyVersion = $versionInfo['AssemblyVersion']
    $localPackageVersion = $versionInfo['PackageVersion']

    Write-Host "Base Directory: $(Normalize-FileSystemSlashes "$baseDirectory")"
    Write-Host "Artifacts Directory: $(Normalize-FileSystemSlashes "$artifactsDirectory")"
    Write-Host "Source Directory: $(Normalize-FileSystemSlashes "$sourceDirectory")"
    Write-Host "Test Directory: $(Normalize-FileSystemSlashes "$testDirectory")"
    Write-Host "Tools Directory: $(Normalize-FileSystemSlashes "$toolsDirectory")"
    Write-Host "NuGet Package Directory: $(Normalize-FileSystemSlashes "$nugetPackageDirectory")"
    Write-Host "Test Results Directory: $(Normalize-FileSystemSlashes "$testResultsDirectory")"
    Write-Host "AssemblyVersion: $localAssemblyVersion"
    Write-Host "Package Version: $localPackageVersion"
    Write-Host "File Version: $localFileVersion"
    Write-Host "InformationalVersion Version: $localInformationalVersion"
    Write-Host "Configuration: $configuration"
    
    Ensure-Directory-Exists "$artifactsDirectory"
}

task PackResourceStubs -depends Clean, Init -description "This task builds the resource stubs" {
    Ensure-Directory-Exists "$nugetPackageDirectory"

    # This task runs before the restore event of the main build, so it is not possible to
    # actually include the resource files here. This just builds stubs NuGet packages to satisfy
    # the dependencies of ICU4N.csproj. We pack again after building ICU4N.csproj and this second
    # build will contain the resource files. Note the resources are not actually required by the build,
    # but they are required at runtime (or at least the root ICU4N.resources.dll file is).

    $localPackageVersion = $versionInfo['PackageVersion']

    Exec {
        &dotnet pack "$icu4nResourcesProjectFile" `
            --configuration $configuration `
            --output "$nugetPackageDirectory" `
            /p:PackageVersion="$localPackageVersion" `
            /p:SkipGitVersioning=true `
            /p:PackResourceStubs=true
    }

    Exec {
        &dotnet pack "$icu4nResourcesNETFramework40ProjectFile" `
            --configuration $configuration `
            --output "$nugetPackageDirectory" `
            /p:PackageVersion="$localPackageVersion" `
            /p:SkipGitVersioning=true `
            /p:PackResourceStubs=true
    }
}

task Compile -depends Clean, Init, PackResourceStubs -description "This task compiles the solution" {

    $localInformationalVersion = $versionInfo['InformationalVersion']
    $localFileVersion = $versionInfo['FileVersion']
    $localAssemblyVersion = $versionInfo['AssemblyVersion']
    $localPackageVersion = $versionInfo['PackageVersion']

    Exec {
        &dotnet build "$solutionFile" `
            --configuration "$configuration" `
            /p:Platform="$platform" `
            /p:InformationalVersion="$localInformationalVersion" `
            /p:FileVersion="$localFileVersion" `
            /p:AssemblyVersion="$localAssemblyVersion" `
            /p:PackageVersion="$localPackageVersion" `
            /p:TestAllTargetFrameworks=true `
            /p:PortableDebugTypeOnly=true `
            /p:SkipGitVersioning=true `
            /p:ReferenceICUResources=true
    }

    # Remove our package stubs from the NuGet cache. They may interfere with the real
    # packages if they are also requested on this machine.
    DeleteCachedNuGetPackage -PackageId "ICU4N.Resources" -Version "$localPackageVersion"
    DeleteCachedNuGetPackage -PackageId "ICU4N.Resources.NETFramework4.0" -Version "$localPackageVersion"
}

task Pack -depends Compile -description "This task creates the NuGet packages" {
    Ensure-Directory-Exists "$nugetPackageDirectory"

    $localPackageVersion = $versionInfo['PackageVersion']

    Exec {
        &dotnet pack "$solutionFile" `
            --configuration $configuration `
            --output "$nugetPackageDirectory" `
            --no-build `
            --no-restore `
            /p:PackageVersion="$localPackageVersion" `
            /p:SkipGitVersioning=true
    }
    
    Exec {
        &dotnet pack "$icu4nResourcesProjectFile" `
            --configuration $configuration `
            --output "$nugetPackageDirectory" `
            /p:PackageVersion="$localPackageVersion" `
            /p:SkipGitVersioning=true
    }
    
    Exec {
        &dotnet pack "$icu4nResourcesNETFramework40ProjectFile" `
            --configuration $configuration `
            --output "$nugetPackageDirectory" `
            /p:PackageVersion="$localPackageVersion" `
            /p:SkipGitVersioning=true
    }
}

task Test -depends Pack -description "This task runs the tests" {

    pushd $baseDirectory
    $testProjects = Get-ChildItem -Path "$testDirectory/**/*.csproj" -Recurse
    popd

    $testProjects = $testProjects | Sort-Object -Property FullName
    Ensure-Directory-Exists $testResultsDirectory

    foreach ($testProject in $testProjects) {
        $testName = $testProject.Directory.Name
    
        # Call the target to get the configured test frameworks for this project. We only read the first line because MSBuild adds extra output.
        $frameworksString = $(dotnet build "$testProject" --verbosity minimal --nologo --no-restore /t:PrintTargetFrameworks /p:TestProjectsOnly=true)[0].Trim()
    
        #Write-Host "Test Framework String: $frameworksString"
        if ($frameworksString -eq 'none') {
            Write-Host "Skipping project '$testProject' because it is not marked with `<IsTestProject`>true`<`/IsTestProject`> and/or it contains no test frameworks for the current environment." -ForegroundColor DarkYellow
            continue
        }
    
        [string[]]$frameworks = $frameworksString -split '\s*;\s*'
        foreach ($framework in $frameworks) {

            $testPlatformArray = $testPlatforms -split '\s*[;,]\s*'
            foreach ($testPlatform in $testPlatformArray) {

                $testResultDirectory = "$testResultsDirectory/$framework/$testPlatform/$testName"
                Ensure-Directory-Exists $testResultDirectory

                Write-Host "Running tests for: $testName,$framework,$testPlatform" -ForegroundColor Green
                &dotnet test "$testProject" `
                    --configuration $configuration `
                    --framework $framework `
                    --no-build `
                    --no-restore `
                    --blame-hang-timeout 10minutes `
                    --blame-hang-dump-type mini `
                    --results-directory "$testResultDirectory" `
                    --logger:"trx;LogFileName=$testResultsFileName" `
                    -- RunConfiguration.TargetPlatform=$testPlatform
                #	--logger:"console;verbosity=normal"
            }
            Write-Host ""
            Write-Host "See the .trx logs in $(Normalize-FileSystemSlashes "$testResultsDirectory/$framework") for more details." -ForegroundColor DarkCyan
        }
    }
}

function Backup-File([string]$path) {
    if ($backupFiles -eq $true) {
        Copy-Item $path "$path.bak" -Force
        $backedUpFiles.Insert(0, $path)
    } else {
        Write-Host "Ignoring backup of file $path" -ForegroundColor DarkRed
    }
}

function Restore-File([string]$path) {
    if ($backupFiles -eq $true) {
        if (Test-Path "$path.bak") {
            Move-Item "$path.bak" $path -Force
        }
        $backedUpFiles.Remove($path)
    }
}

function Ensure-Directory-Exists([string] $path)
{
    if (!(Test-Path $path)) {
        New-Item $path -ItemType Directory
    }
}

function New-TemporaryDirectory {
    $parent = [System.IO.Path]::GetTempPath()
    [string] $name = [System.Guid]::NewGuid()
    New-Item -ItemType Directory -Path (Join-Path $parent $name)
}

function Normalize-FileSystemSlashes([string]$path) {
    $sep = [System.IO.Path]::DirectorySeparatorChar
    return $($path -replace '/',$sep -replace '\\',$sep)
}

function DeleteCachedNuGetPackage([string]$packageId, [string]$version) {
    # The user may have overridden the default .nuget package directory location using NUGET_PACKAGES env variable.
    [string]$nugetCacheDir = $env:NUGET_PACKAGES
    if ([string]::IsNullOrEmpty($nugetCacheDir)) {
        $nugetCacheDir = "~/.nuget/packages"
    }
    
    # Define the NuGet package cache directory
    $cacheDir = Join-Path $nugetCacheDir "/$packageId/$version"

    Write-Host $cacheDir

    # Check if the package directory exists
    if (Test-Path $cacheDir) {
        # Remove the specific version of the package
        Remove-Item -Recurse -Force $cacheDir
        Write-Host "Version '$version' of package '$packageId' has been deleted from the NuGet cache." -ForegroundColor DarkGreen
    } else {
        Write-Host "Package '$packageId' version '$version' not found in the cache." -ForegroundColor DarkYellow
    }
}