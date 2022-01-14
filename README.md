International Components for Unicode 
=========

[![Nuget](https://img.shields.io/nuget/dt/ICU4N)](https://www.nuget.org/packages/ICU4N)
[![Azure DevOps builds (main)](https://img.shields.io/azure-devops/build/ICU4N/44041e22-bd88-42a2-ad29-ee6859a5010e/1/main)](https://dev.azure.com/ICU4N/ICU4N/_build?definitionId=1&_a=summary)
[![GitHub](https://img.shields.io/github/license/NightOwl888/ICU4N)](https://github.com/NightOwl888/ICU4N/blob/master/LICENSE.txt)
[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)

ICU4N is a set of .NET libraries providing Unicode and Globalization support for software applications, a .NET port of the popular [ICU4J project](http://site.icu-project.org).

This is a port of ICU4J, version 60.1. We have ported about 40% of the code, and while we aren't planning to add any additional features it is still a work in progress.

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


There are several other features too numerous to mention, but APIs are currently in flux between releases. We have over 1600 passing tests, most failures are just due to missing dependencies.


## NuGet Packages

```
Install-Package ICU4N -Pre
```

We have setup the project structure similar to ICU4J, however this may change in the future. Here is a list of the packages available on NuGet:

- [ICU4N (main package)](https://www.nuget.org/packages/ICU4N/)
- [ICU4N.Collation](https://www.nuget.org/packages/ICU4N.Collation/)
- [ICU4N.CurrencyData](https://www.nuget.org/packages/ICU4N.CurrencyData/)
- [ICU4N.LanguageData](https://www.nuget.org/packages/ICU4N.LanguageData/)
- [ICU4N.RegionData](https://www.nuget.org/packages/ICU4N.RegionData/)
- [ICU4N.Transliterator](https://www.nuget.org/packages/ICU4N.Transliterator/)

We are looking into the best way to allow end users to be able to provide their own data distributions for smaller deployment artifacts.

## Documentation

We have converted most of the documentation comments so Visual Studio Intellisense works. However, for full API docs the best source at the moment is the [ICU4J API Reference](http://icu-project.org/apiref/icu4j/). There is also a lot of other great info on the ICU project's web site, such as the [Feature Comparison Page](http://site.icu-project.org/charts/comparison).

## Building and Testing

To build the project from source, see the [Building and Testing documentation](https://github.com/NightOwl888/ICU4N/blob/main/docs/building-and-testing.md).

## Saying Thanks

If you find this library to be useful, please star us [on GitHub](https://github.com/NightOwl888/ICU4N) and consider a sponsorship so we can continue bringing you great free tools like this one. It really would make a big difference!

[![GitHub Sponsors](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/NightOwl888)