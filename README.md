International Components for Unicode 
=========

[![Nuget](https://img.shields.io/nuget/dt/ICU4N)](https://www.nuget.org/packages/ICU4N)
[![Azure DevOps builds (master)](https://img.shields.io/azure-devops/build/ICU4N/44041e22-bd88-42a2-ad29-ee6859a5010e/1/master)](https://dev.azure.com/ICU4N/ICU4N/_build?definitionId=1&_a=summary)
[![GitHub](https://img.shields.io/github/license/NightOwl888/ICU4N)](https://github.com/NightOwl888/ICU4N/blob/master/LICENSE.txt)

ICU4N is a set of .NET libraries providing Unicode and Globalization support for software applications, a .NET port of the popular [ICU4J project](http://site.icu-project.org).

This is a port of ICU4J, version 60.1. We have ported about 40% of the code so far, and it is still a work in progress. Here are some of the major features that are functional:

## Functioning

1. BreakIterator
2. RuleBasedBreakIterator
2. Normalizer
3. Normalizer2
4. FilteredNormalizer2
5. UnicodeSet
6. Collator
7. RuleBasedCollator
8. Transliterator


There are several other features too numerous to mention that are also functioning, but APIs are currently in flux between releases. There are also some known gaps in conversion between CultureInfo and ULocale. We have over 1600 passing tests, most failures are just due to missing dependencies.

## Partially Functioning

1. ULocale (we recommend using `System.Globalization.CultureInfo` only to set cultures/locales for the time being)
2. MessageFormat (only supports ChoiceFormat currently)


## NuGet Packages

```
Install-Package ICU4N -Pre
```

We have setup the project structure similar to ICU4J. Here is a list of the packages available on NuGet:

- [ICU4N (main package)](https://www.nuget.org/packages/ICU4N/)
- [ICU4N.Collation](https://www.nuget.org/packages/ICU4N.Collation/)
- [ICU4N.CurrencyData](https://www.nuget.org/packages/ICU4N.CurrencyData/)
- [ICU4N.LanguageData](https://www.nuget.org/packages/ICU4N.LanguageData/)
- [ICU4N.RegionData](https://www.nuget.org/packages/ICU4N.RegionData/)
- [ICU4N.Transliterator](https://www.nuget.org/packages/ICU4N.Transliterator/)

## Documentation

We have converted most of the documentation comments so intellisense works. However, for full API docs the best source at the moment is the [ICU4J API Reference](http://icu-project.org/apiref/icu4j/). There is also a lot of other great info on the ICU project's web site, such as the [Feature Comparison Page](http://site.icu-project.org/charts/comparison).