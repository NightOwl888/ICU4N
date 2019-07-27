International Components for Unicode 
=========


ICU4N is a set of .NET libraries providing Unicode and Globalization support for software applications, a .NET port of the popular [ICU4J project](http://site.icu-project.org).

This is a port of (currently 60.1) of ICU4J. We have ported about 40% of the code so far, and it is still a work in progress. Here are some of the major features that are ported:

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

We have setup the project structure similar to ICU4J. Currently, here is a list of the packages available on NuGet:

1. ICU4N (main package)
2. ICU4N.Collation
3. ICU4N.CurrencyData
4. ICU4N.LanguageData
5. ICU4N.RegionData
6. ICU4N.Transliterator

## Documentation

We have converted most of the documentation comments so intellisense works. However, for full API docs the best source at the moment is the [ICU4J API Reference](http://icu-project.org/apiref/icu4j/). There is also a lot of other great info on the ICU project's web site, such as the [Feature Comparison Page](http://site.icu-project.org/charts/comparison).