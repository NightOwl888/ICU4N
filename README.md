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

   This is the default and recommended way to use ICU4N resources. Using satellite assemblies to manage resources allows you to include/exclude specific *cultures*.

2. Using Resource Files

   Using resource files allows you to finely tune the amount of data that is distributed with your project by including/excluding specific *features* as well as *cultures*.

   > **NOTE:** ICU4N does not shadow copy these files. It is not recommended to use resource files to deploy resources if you are using an "always up" service (such as ASP.NET) and intend to use xCopy deployment, since the resource files may be locked by your application when you try to overwrite them.

### Default Satellite Assemblies

By default, ICU4N includes a transient dependency on [ICU4N.Resources](https://www.nuget.org/packages/ICU4N.Resources/), which includes satellite assemblies for all features and languages. For most projects, this should suffice.

It is recommended to use the default set of data for class libraries that are deployed via NuGet to be shared, and to only consider using custom subsets of data for executable projects. This gives every consumer of a shared library a chance to customize the ICU resource data.

> **NOTE:** For SDK-Style projects that target `net40` or `net403`, the transient dependency is on [ICU4N.Resources.NETFramework4.0](https://www.nuget.org/packages/ICU4N.Resources.NETFramework4.0/). This package contains exactly the same files as [ICU4N.Resources](https://www.nuget.org/packages/ICU4N.Resources/) and only exists to work around the fact that NuGet doesn't support a single target framework to deploy satellite assemblies to targets below `net45` as well as targets that support `netstandard1.0` (which supports `net45` and higher).

### Custom Satellite Assemblies

ICU4N contains more than 750 cultures which use more than 20MB of disk space.

It is possible to reduce the distribution size by excluding cultures that you don't intend to support. To ship a subset of satellite assemblies, there are 2 options:

- **Option 1:** Re-package a Subset of Satellite Assemblies - Currently, there is no built-in support. The recommended way is to download the matching version of [ICU4N.Resources](https://www.nuget.org/packages/ICU4N.Resources/), use a zip utility to unzip the package, and create a NuGet package with a custom name using the subset of satellite assemblies desired.
- **Option 2:** Change your build script to delete the `<culture name>/ICU4N.resources.dll` files *after* the build and *before* packing and/or publishing them.

The satellite assemblies are located in folders named like `<culture name>/ICU4N.resources.dll`.

> **IMPORTANT:** The neutral culture satellite assembly files contain shared resource data for all of the specific cultures. If you include one or more specific cultures, such as `fr-CA/ICU4N.resources.dll`, you must also include the neutral culture `fr/ICU4N.resources.dll`.

> **IMPORTANT:** There is a common satellite assembly named `ICU4N.resources.dll` that sits in the assembly directory. This file must always be included for ICU4N to function when using satellite assemblies.

When including custom resource data with ICU4N, be sure to exclude the transitive dependencies from ICU4N as described in [Removing the default Transient Dependency on ICU4N.Resources](removing-the-default-transient-dependency-on-ICU4N.Resources).

### Custom Resource Files

Resource files can be used to reduce the total amount of resource data even further by excluding both *features* and *cultures* that are not being used. Resource files are automatically detected if they are in the `<assembly_directory>/data/<icuversion>/<feature>` directory. The default set of resource files can be [downloaded from Maven](https://repo1.maven.org/maven2/com/ibm/icu/icu4j/) for the ICU4J version corresponding to this version of ICU4N. Simply download the main `icu4j.jar` file. This file can be extracted with a Zip utility. The resource data can be found in the `/com/ibm/icu/impl/data/` directory.

Reducing resource data is an advanced topic. See the [ICU Data](https://unicode-org.github.io/icu/userguide/icu_data/) topic to decide the best approach for which resources to include.

Resources will be detected automatically if they are in the `/data/` directory. Note that including the versioned subdirectory (such as `icudt60b`) is required.

When including custom resource data with ICU4N, be sure to exclude the transitive dependencies from ICU4N as described in [Removing the default Transient Dependency on ICU4N.Resources](removing-the-default-transient-dependency-on-ICU4N.Resources).

### Removing the default Transient Dependency on ICU4N.Resources

To deploy custom resources via NuGet with your project, you must remove the transient dependency on the [ICU4N.Resources](https://www.nuget.org/packages/ICU4N.Resources/) package so the full set of resources isn't accidentally deployed to projects that consume yours. This can be done using the [ExcludeAssets](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#controlling-dependency-assets) flag called `buildTransitive`.

```xml
<ItemGroup>
    <PackageReference Include="ICU4N" Version="60.1.0-alpha.401" ExcludeAssets="buildTransitive" />
</ItemGroup>
```

### Legacy Non-SDK-Style Projects

For projects that are using a .NET SDK lower than .NET 5.0 and/or are using a non-SDK style project (i.e. a project that specifies ToolsVersion="4.0" or lower in the `<Project>` element) support for resources is not automatic. You must manually add a package reference to one of:

- **Option 1:** [ICU4N.Resources](https://www.nuget.org/packages/ICU4N.Resources/)
- **Option 2:** [ICU4N.Resources.NETFramework4.0](https://www.nuget.org/packages/ICU4N.Resources.NETFramework4.0/)
- **Option 3:** A custom NuGet package with a subset of resources.

The ability for consuming projects to minimize resource files beyond the set of resources specified by the original package author for non-SDK style projects is not supported.

## Building and Testing

To build the project from source, see the [Building and Testing documentation](https://github.com/NightOwl888/ICU4N/blob/main/docs/building-and-testing.md).

## Saying Thanks

If you find this library to be useful, please star us [on GitHub](https://github.com/NightOwl888/ICU4N) and consider a sponsorship so we can continue bringing you great free tools like this one. It really would make a big difference!

[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)