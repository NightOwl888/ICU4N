International Components for Unicode 
=========

[![Nuget](https://img.shields.io/nuget/dt/ICU4N)](https://www.nuget.org/packages/ICU4N)
[![Azure DevOps builds (main)](https://img.shields.io/azure-devops/build/ICU4N/44041e22-bd88-42a2-ad29-ee6859a5010e/1/main)](https://dev.azure.com/ICU4N/ICU4N/_build?definitionId=1&_a=summary)
[![GitHub](https://img.shields.io/github/license/NightOwl888/ICU4N)](https://github.com/NightOwl888/ICU4N/blob/main/LICENSE.txt)
[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)

ICU4N is a set of .NET libraries providing Unicode and Globalization support for software applications, a .NET port of the popular [ICU4J project](http://site.icu-project.org).

This is a port of ICU4J, version 60.1. We have ported about 50% of the code, and while we aren't planning to add any additional features it is still a work in progress.

## Features

Here are some of the major features that have been ported:

1. [BreakIterator](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/BreakIterator.html)
2. [RuleBasedBreakIterator](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/RuleBasedBreakIterator.html)
2. [Normalizer](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/Normalizer.html)
3. [Normalizer2](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/Normalizer2.html)
4. [FilteredNormalizer2](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/FilteredNormalizer2.html)
5. [UnicodeSet](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/UnicodeSet.html)
6. [Collator](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/Collator.html)
7. [RuleBasedCollator](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/RuleBasedCollator.html)
8. [Transliterator](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/Transliterator.html)
9. [RuleBasedTransliterator](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/RuleBasedTransliterator.html)
10. [RuleBasedNumberFormat](https://unicode-org.github.io/icu-docs/apidoc/released/icu4j/index.html?com/ibm/icu/text/RuleBasedNumberFormat.html) - only formatting from number > string is supported by calling members of the `ICU4N.Text.FormatNumberRuleBased` class or by using them as extension methods from [System.Byte](https://learn.microsoft.com/en-us/dotnet/api/system.byte), [System.Int16](https://learn.microsoft.com/en-us/dotnet/api/system.int16), [System.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32), [System.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64), [System.Int128](https://learn.microsoft.com/en-us/dotnet/api/system.int128), [System.Numerics.BigInteger](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.biginteger), [System.SByte](https://learn.microsoft.com/en-us/dotnet/api/system.sbyte), [System.IntPtr](https://learn.microsoft.com/en-us/dotnet/api/system.intptr), [System.UInt16](https://learn.microsoft.com/en-us/dotnet/api/system.uint16), [System.UInt32](https://learn.microsoft.com/en-us/dotnet/api/system.uint32), [System.UInt64](https://learn.microsoft.com/en-us/dotnet/api/system.uint64), [System.UInt128](https://learn.microsoft.com/en-us/dotnet/api/system.uint128), [System.Half](https://learn.microsoft.com/en-us/dotnet/api/system.half), [System.Single](https://learn.microsoft.com/en-us/dotnet/api/system.single), [System.Double](https://learn.microsoft.com/en-us/dotnet/api/system.Double), or [System.Decimal](https://learn.microsoft.com/en-us/dotnet/api/system.decimal).


There are several other features too numerous to mention, but APIs are currently in flux between releases. We have over 4200 passing tests, most failures are just due to missing dependencies.


## NuGet Packages

```
Install-Package ICU4N -Pre
```

The package structure is as follows:

- [ICU4N (main package)](https://www.nuget.org/packages/ICU4N/)
- [ICU4N.Resources](https://www.nuget.org/packages/ICU4N.Resources/) - See the documentation for [Managing Resources](managing-resources)
- [ICU4N.Resources.NETFramework4.0](https://www.nuget.org/packages/ICU4N.Resources.NETFramework4.0/) - See the documentation for [Managing Resources](managing-resources)

## Documentation

We have converted most of the documentation comments so Visual Studio Intellisense works. However, for full API docs the best source at the moment is the [ICU4J API Reference](http://icu-project.org/apiref/icu4j/). There is also a lot of other great info on the ICU project's web site, such as the [Feature Comparison Page](http://site.icu-project.org/charts/comparison).

## Managing Resources

> **IMPORTANT:** The version of resources that is used must be at the same version as ICU4N.

There are 2 ways to deploy resources with ICU4N.

1. Using Satellite Assemblies

   This is the default and recommended way to use ICU4N resources. Using satellite assemblies to manage resources allows you to include specific neutral *cultures* or include them all by default.

2. Using Resource Files

   Using resource files allows you to finely tune the amount of data that is distributed with your project by including/excluding specific *features* as well as *cultures*.

   > **NOTE:** ICU4N does not shadow copy these files. It is not recommended to use resource files to deploy resources if you are using an "always up" service (such as ASP.NET) and intend to use xCopy deployment, since the resource files may be locked by your application when you try to overwrite them.

### Default Satellite Assemblies

By default, ICU4N includes a dependency on [ICU4N.Resources](https://www.nuget.org/packages/ICU4N.Resources/), which includes satellite assemblies for all features and languages. For most projects, this should suffice.

For class libraries that are deployed in public systems such as NuGet, it is recommended to use the default set of resource data (the full set) to allow end users to utilize built in features of the .NET SDK to exclude resources that don't apply to them.

> **NOTE:** For projects that target .NET Framwork prior to version `net462`, the dependency is on [ICU4N.Resources.NETFramework4.0](https://www.nuget.org/packages/ICU4N.Resources.NETFramework4.0/) instead. This package contains exactly the same files as [ICU4N.Resources](https://www.nuget.org/packages/ICU4N.Resources/) and only exists to work around the fact that NuGet doesn't support a single target framework to deploy satellite assemblies to targets below `net45` as well as targets that support `netstandard2.0` (which supports `net462` and higher).

### Filtering Satellite Assemblies

ICU4N contains more than 750 cultures which use more than 18MB of disk space. If you are publishing an application and wish to reduce the distribution size of your application, ICU4N supports the [SatelliteResourceLanguages](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#satelliteresourcelanguages). Although ICU provides support for both specific and neutral cultures at runtime, the satellite assemblies are packaged as .NET *neutral culture* packages to eliminate issues with ICU locale names that .NET doesn't recognize. All *specific cultures* (such as `en-GB` or `fr-CA`) are packaged in the satellite assembly with the corresponding *neutral culture* (in this case `en` and `fr` respectively).

There are 4 special cases where locale names and culture names differ between ICU4N locale names and the satellite assembly names. For these locales, the .NET culture name must be used to filter the satellite assemblies even though the ICU locale name can be specified to the `UCultureInfo` class.

| Language Name             | ICU4N Locale Name | .NET Culture Name |
| :------------------------ | :---------------- | :---------------- |
| Quechua                   | qu                | quz               |
| Cantonese                 | yue               | zh                |
| Cantonese (Simplified)    | yue-Hans          | zh-Hans           |
| Cantonese (Traditional)   | yue-Hant          | zh-Hant           |

#### Example

Using the `SatelliteResourceLanguages` property to *only* include the languages English, Spanish, and French in your distribution.

```xml
<PropertyGroup>
  <SatelliteResourceLanguages>en;es;fr</SatelliteResourceLanguages>
</PropertyGroup>
```

This will enable ICU4N to support all variants of these languages for all ICU features and exclude the resources for any other language from the distribution.

> **NOTE:** `SatelliteResourceLanguages` applies to *all* resources for dependencies of your project, not just those in ICU4N.Resources. If another library includes support for a *specific culture* and you want to use it in your application, you should include that specific culture name in `SatelliteResourceLanguages` even though it does not specifically apply to ICU4N.

### Custom Satellite Assemblies

In addition to filtering out cultures, ICU4N supports adding new cultures by compiling and packaging a new set of satellite assemblies to deploy with your application. The tools to do this are still a work in progress and there is not yet an official procedure for compiling custom satellite assemblies. You must follow the same naming conventions for resource files for ICU4N to discover any new files that are added, but do note these are still in flux and may change from one release of ICU4N to the next. The version of resources used must **exactly match** the version of ICU4N. Unfortunately, Microsoft's documentation on creating custom satellite assemblies is extremely out-of-date so it is recommended to use an LLM such as ChatGPT to get some direction on how to accomplish this using the .NET SDK rather than the old way of using al.exe.

#### Excluding ICU4N.Resources

To replace the satellite assemblies that are shipped with ICU4N, use the [`ExcludeAssets` feature of MSBuild](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#controlling-dependency-assets) to exclude the default set of satellite assemblies from the build.

```xml
    <PackageReference Include="ICU4N.Resources"
                      Version="<the specific version of ICU4N.Resources>"
                      ExcludeAssets="all" />
```

> **NOTE:** At the time of this writing, all versions of MSBuild have a bug where satellite assemblies that use 3-letter language codes are not copied to the build and/or publish output. ICU4N.Resources includes a patch in the `ICU4N.Resources.CopyPatch.targets` file. It is recommended that you use a NuGet package and include this file along with a `.targets` file named `$(PackageId).targets` that includes `ICU4N.Resources.CopyPatch.targets` and the properties it requires in its `buildTransitive` folder.

### Custom Resource Files

Resource files can be used to reduce the total amount of resource data even further by excluding both *features* and *cultures* that are not being used. Resource files are automatically detected if they are in the `<assembly_directory>/data/<icuversion>/<feature>` directory. The default set of resource files can be [downloaded from Maven](https://repo1.maven.org/maven2/com/ibm/icu/icu4j/) for the ICU4J version corresponding to this version of ICU4N. Simply download the main `icu4j.jar` file. This file can be extracted with a Zip utility. The resource data can be found in the `/com/ibm/icu/impl/data/` directory.

Reducing resource data is an advanced topic. See the [ICU Data](https://unicode-org.github.io/icu/userguide/icu_data/) topic to decide the best approach for which resources to include.

Resources will be detected automatically if they are in the `/data/` directory. Note that including the versioned subdirectory (such as `icudt60b`) is required.

When including custom resource data with ICU4N, be sure to exclude the resources from ICU4N.Resources as described in [Excluding ICU4N.Resources](excluding-ICU4N.Resources).

## Building and Testing

To build the project from source, see the [Building and Testing documentation](https://github.com/NightOwl888/ICU4N/blob/main/docs/building-and-testing.md).

## Saying Thanks

If you find this library to be useful, please star us [on GitHub](https://github.com/NightOwl888/ICU4N) and consider a sponsorship so we can continue bringing you great free tools like this one. It really would make a big difference!

[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)