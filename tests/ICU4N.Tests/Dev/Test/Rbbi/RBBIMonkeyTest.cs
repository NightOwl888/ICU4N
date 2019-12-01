using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Threading;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Rbbi
{
    /// <summary>
    /// RBBI Monkey Test. Ported from ICU4C test/intltest/rbbimonkeytest.cpp.
    /// This is the newer, data driven monkey test. It is completely separate from the
    /// older class RBBITestMonkey.
    /// </summary>
    public class RBBIMonkeyTest : TestFmwk
    {
        //  class CharClass    Represents a single character class from the source break rules.
        //                     Inherits from UObject because instances are adopted by UHashtable, which ultimately
        //                     deletes them using hash's object deleter function.

        internal class CharClass
        {
            internal String fName;
            internal String fOriginalDef;    // set definition as it appeared in user supplied rules.
            internal String fExpandedDef;    // set definition with any embedded named sets replaced by their defs, recursively.
            internal UnicodeSet fSet;
            internal CharClass(String name, String originalDef, String expandedDef, UnicodeSet set)
            {
                fName = name;
                fOriginalDef = originalDef;
                fExpandedDef = expandedDef;
                fSet = set;
            }
        }


        // class BreakRule    Struct-like class represents a single rule from a set of break rules.
        //                    Each rule has the set definitions expanded, and
        //                    is compiled to a regular expression.

        internal class BreakRule
        {
            internal String fName;                   // Name of the rule.
            internal String fRule;                   // Rule expression, excluding the name, as written in user source.
            internal String fExpandedRule;           // Rule expression after expanding the set definitions.
            internal Regex fRuleMatcher;             // Regular expression that matches the rule.
        };


        // class BreakRules    represents a complete set of break rules, possibly tailored,
        //                     compiled from testdata break rules.

        internal class BreakRules
        {
            internal BreakRules(RBBIMonkeyImpl monkeyImpl)
            {
                fMonkeyImpl = monkeyImpl;
                fBreakRules = new List<BreakRule>();
                fType = BreakIterator.KIND_TITLE;
                fCharClasses = new Dictionary<String, CharClass>();
                fCharClassList = new List<CharClass>();
                fDictionarySet = new UnicodeSet();

                // Match an alpha-numeric identifier in a rule. Will be a set name.
                // Use negative look-behind to exclude non-identifiers, mostly property names or values.
                fSetRefsMatcher = new Regex(
                        "(?<!\\{[ \\t]{0,4})" +
                        "(?<!=[ \\t]{0,4})" +
                        "(?<!\\[:[ \\t]{0,4})" +
                        "(?<!\\\\)" +
                        "(?<![A-Za-z0-9_])" +
                        "([A-Za-z_][A-Za-z0-9_]*)",     // The char class name
                        RegexOptions.Compiled);

                // Match comments and blank lines. Matches will be replaced with "", stripping the comments from the rules.
                fCommentsMatcher = new Regex("" +
                        "(^|(?<=;))" +                // Start either at start of line, or just after a ';' (look-behind for ';')
                        "(?:[ \\t]*)+" +                //   Match white space.
                        "(?:(#.*)?)+" +                //   Optional # plus whatever follows
                        "$",                        //   new-line at end of line.
                        RegexOptions.Compiled);

                // Match (initial parse) of a character class definition line.
                fClassDefMatcher = new Regex("" +
                        "[ \\t]*" +                    // leading white space
                        "([A-Za-z_][A-Za-z0-9_]*)" +             // The char class name
                        "[ \\t]*=[ \\t]*" +                    //   =
                        "(.*?)" +                               // The char class UnicodeSet expression
                        "[ \\t]*;$",                            // ; <end of line>
                        RegexOptions.Compiled);

                // Match (initial parse) of a break rule line.
                fRuleDefMatcher = new Regex("" +
                        "[ \\t]*" +                     // leading white space
                        "([A-Za-z_][A-Za-z0-9_.]*)" +             // The rule name
                        "[ \\t]*:[ \\t]*" +                     //   :
                        "(.*?)" +                               // The rule definition
                        "[ \\t]*;$",                             // ; <end of line>
                        RegexOptions.Compiled);

                // Match a property expression, either [:xxx:] or \p{...}
                fPropertyMatcher = new Regex("" +
                        "\\[:.*?:]|\\\\(?:p|P)\\{.*?\\}",
                        RegexOptions.Compiled);
            }

            /**
             * Create the expanded definition for this char class,
             * replacing any set references with the corresponding definition.
             */
            internal CharClass AddCharClass(String name, String definition)
            {
                StringBuffer expandedDef = new StringBuffer();
                Match match = fSetRefsMatcher.Match(definition); // Reset
                int lastEnd = 0;
                if (match.Success)
                {
                    do
                    {
                        string sname = match.Groups[/*"ClassName"*/ 1].Value;
                        CharClass snameClass = fCharClasses.Get(sname);
                        string expansionForName = snameClass != null ? snameClass.fExpandedDef : sname;

                        expandedDef.Append(definition.Substring(lastEnd, match.Index - lastEnd)); // Append replacement
                        expandedDef.Append(expansionForName);

                        lastEnd = match.Index + match.Length;
                    } while ((match = match.NextMatch()).Success);
                }
                expandedDef.Append(definition.Substring(lastEnd)); // Append tail

                String expandedDefString = expandedDef.ToString();

                if (fMonkeyImpl.fDumpExpansions)
                {
                    Console.Out.Write("addCharClass(\"{0}\"\n", name);
                    Console.Out.Write("             {0}\n", definition);
                    Console.Out.Write("expandedDef: {0}\n", expandedDefString);
                }

                // Verify that the expanded set definition is valid.

                UnicodeSet s;
                try
                {
                    s = new UnicodeSet(expandedDefString, UnicodeSet.IgnoreSpace);
                }
                catch (ArgumentException e)
                {
                    Console.Error.Write("{0}: error {1} creating UnicodeSet {2}", fMonkeyImpl.fRuleFileName, e.ToString(), name);
                    throw e;
                }

                // Get an expanded equivalent pattern from the UnicodeSet.
                // This removes set difference operators, which would fail if passed through to Java regex.

                StringBuffer expandedPattern = new StringBuffer();
                s.GeneratePattern(expandedPattern, true);
                expandedDefString = expandedPattern.ToString();
                if (fMonkeyImpl.fDumpExpansions)
                {
                    Console.Out.Write("expandedDef2: {0}\n", expandedDefString);
                }

                CharClass cclass = new CharClass(name, definition, expandedDefString, s);
                CharClass previousClass;
                fCharClasses.TryGetValue(name, out previousClass);
                fCharClasses[name] = cclass;

                if (previousClass != null)
                {
                    // TODO: decide whether or not to allow redefinitions.
                    //       Can be convenient in some cases.
                    // String msg = String.format("{0}: Redefinition of character class {1}\n",
                    //         fMonkeyImpl.fRuleFileName, cclass.fName);
                    // Console.Error.WriteLine(msg);
                    // throw new ArgumentException(msg);
                }
                return cclass;

            }


            internal void AddRule(String name, String definition)
            {
                BreakRule thisRule = new BreakRule();
                StringBuffer expandedDefsRule = new StringBuffer();
                thisRule.fName = name;
                thisRule.fRule = definition;

                // Expand the char class definitions within the rule.
                var match = fSetRefsMatcher.Match(definition); // Reset
                int lastEnd = 0;
                if (match.Success)
                {
                    do
                    {
                        string sname = match.Groups[/*"ClassName"*/ 1].Value;
                        CharClass nameClass = fCharClasses[sname];
                        if (nameClass == null)
                        {
                            Console.Error.Write("char class \"{0}\" unrecognized in rule \"{1}\"\n", sname, definition);
                        }
                        string expansionForName = nameClass != null ? nameClass.fExpandedDef : sname;

                        expandedDefsRule.Append(definition.Substring(lastEnd, match.Index - lastEnd)); // Append replacement
                        expandedDefsRule.Append(expansionForName);

                        lastEnd = match.Index + match.Length;
                    } while ((match = match.NextMatch()).Success);
                }
                expandedDefsRule.Append(definition.Substring(lastEnd)); // Append tail

                // Replace any property expressions, \p{...} or [:...:] with an equivalent expansion,
                // obtained from ICU UnicodeSet. Need to do this substitution because Java regex
                // does not recognize all properties, and because Java's definitions are likely
                // older than ICU's.

                StringBuffer expandedRule = new StringBuffer();
                var expandedDefsRuleString = expandedDefsRule.ToString();
                match = fPropertyMatcher.Match(expandedDefsRuleString); // Reset
                lastEnd = 0;
                if (match.Success)
                {
                    do
                    {
                        String prop = match.Value;
                        UnicodeSet propSet = new UnicodeSet("[" + prop + "]");
                        StringBuffer propExpansion = new StringBuffer();
                        propSet.GeneratePattern(propExpansion, true);
                        expandedRule.Append(expandedDefsRuleString.Substring(lastEnd, match.Index - lastEnd)); // Append replacement
                        expandedRule.Append(propExpansion.ToString());

                        lastEnd = match.Index + match.Length;
                    } while ((match = match.NextMatch()).Success);
                }
                expandedRule.Append(expandedDefsRuleString.Substring(lastEnd)); // Append tail


                //   Replace any [^negated sets] with equivalent flattened sets generated by
                //   ICU UnicodeSet. [^ ...] in Java Regex character classes does not apply
                //   to any nested classes. Variable substitution in rules produces
                //   nested sets that [^negation] needs to apply to.

                StringBuffer ruleWithFlattenedSets = new StringBuffer();
                int idx = 0;
                while (idx < expandedRule.Length)
                {
                    int setOpenPos = expandedRule.IndexOf("[^", idx);
                    if (setOpenPos < 0)
                    {
                        break;
                    }
                    if (setOpenPos > idx)
                    {
                        // Move anything from the source rule preceding the [^ into the processed rule, unchanged.
                        ruleWithFlattenedSets.Append(expandedRule.ToString(idx, setOpenPos - idx));
                    }
                    int nestingLevel = 1;
                    bool haveNesting = false;
                    int setClosePos;
                    for (setClosePos = setOpenPos + 2; nestingLevel > 0 && setClosePos < expandedRule.Length; ++setClosePos)
                    {
                        char c = expandedRule[setClosePos];
                        if (c == '\\')
                        {
                            ++setClosePos;
                        }
                        else if (c == '[')
                        {
                            ++nestingLevel;
                            haveNesting = true;
                        }
                        else if (c == ']')
                        {
                            --nestingLevel;
                        }
                    }
                    if (haveNesting && nestingLevel == 0)
                    {
                        // Found one, a negated set that includes interior nested sets.
                        // Create an ICU UnicodeSet from the source pattern, and obtain an
                        // equivalent flattened pattern from that.
                        UnicodeSet uset = new UnicodeSet(expandedRule.ToString(setOpenPos, setClosePos - setOpenPos), true);
                        uset.GeneratePattern(ruleWithFlattenedSets, true);
                    }
                    else
                    {
                        // The [^ set definition did not include any nested sets.
                        // Copy the original definition without change.
                        // Java regular expressions will handle it without needing to recast it.
                        if (nestingLevel > 0)
                        {
                            // Error case of an unclosed character class expression.
                            // Java regex will also eventually flag the error.
                            Console.Error.Write("No closing ] found in rule {0}\n", name);
                        }
                        ruleWithFlattenedSets.Append(expandedRule.ToString(setOpenPos, setClosePos - setOpenPos));
                    }
                    idx = setClosePos;
                }

                if (idx < expandedRule.Length)
                {
                    ruleWithFlattenedSets.Append(expandedRule.ToString(idx, expandedRule.Length - idx));
                }

                thisRule.fExpandedRule = ruleWithFlattenedSets.ToString();

                // Replace the divide sign (\u00f7) with a regular expression named capture.
                // When running the rules, a match that includes this group means we found a break position.

                // thisRule.fExpandedRule = thisRule.fExpandedRule.replace("÷", "(?<BreakPosition>)");
                thisRule.fExpandedRule = thisRule.fExpandedRule.Replace("÷", "()");
                if (thisRule.fExpandedRule.IndexOf('÷') != -1)
                {
                    String msg = String.Format("{0} Rule {1} contains multiple ÷ signs", fMonkeyImpl.fRuleFileName, name);
                    Console.Error.WriteLine(msg);
                    throw new ArgumentException(msg);
                }

                // UAX break rule set definitions can be empty, just [].
                // Regular expression set expressions don't accept this. Substitute with [a&&[^a]], which
                // also matches nothing.

                thisRule.fExpandedRule = thisRule.fExpandedRule.Replace("[]", "[a&&[^a]]");

                // Change Unicode escape syntax for compatibility with Java regular expressions (Java 7 or newer)
                //    \udddd     => \x{dddd}
                //    \U00hhhhhh => \x{hhhhhh}

                //thisRule.fExpandedRule = Regex.Replace(thisRule.fExpandedRule, "\\\\u([0-9A-Fa-f]{4})", "\\\\x{$1}");
                //thisRule.fExpandedRule = Regex.Replace(thisRule.fExpandedRule, "\\\\U00([0-9A-Fa-f]{6})", "\\\\x{$1}");

                //// Java 6 compatibility troubles - there is no syntax for escaping a supplementary character
                //// within a regular expression character class. Put them in as unescaped literal chars.
                //StringBuilder sb = new StringBuilder(thisRule.fExpandedRule);
                //while (true)
                //{
                //    int where = sb.IndexOf("\\U00");
                //    if (where < 0)
                //    {
                //        break;
                //    }
                //    string cp = HexToCodePoint(sb.ToString(where + 2, (where + 10) - (where + 2)));
                //    sb.Replace(where, where + 10, cp);
                //}
                //thisRule.fExpandedRule = sb.ToString();

                // Escape any literal '#' in the rule expression. Without escaping, these introduce a comment.
                // UnicodeSet._generatePattern() inserts un-escaped "#"s

                thisRule.fExpandedRule = thisRule.fExpandedRule.Replace("#", "\\#");
                if (fMonkeyImpl.fDumpExpansions)
                {
                    Console.Out.Write("fExpandedRule: {0}\n", thisRule.fExpandedRule);
                }

                // Compile a regular expression for this rule.
                try
                {
                    // In .NET, only BMP characters are recognized. So, we convert all UTF32 code points ranges into
                    // alternating UTF16 combination groups. We separated that functionality into a subclass of Regex
                    // to avoid polluting this class with lots of extra regex fixup code.
                    thisRule.fRuleMatcher = new Utf32Regex(@"\A(?:" + thisRule.fExpandedRule + ")", RegexOptions.Compiled);
                }
                catch (ArgumentException e)
                {
                    Console.Error.Write("{0}: Error creating regular expression for rule {1}. Expansion is \n\"{2}\"",
                            fMonkeyImpl.fRuleFileName, name, thisRule.fExpandedRule);
                    throw e;
                }
                // Put this new rule into the vector of all Rules.

                fBreakRules.Add(thisRule);
            }

            internal bool SetKeywordParameter(String keyword, String value)
            {
                if (keyword.Equals("locale"))
                {
                    fLocale = new ULocale(value);
                    return true;
                }
                if (keyword.Equals("type"))
                {
                    if (value.Equals("grapheme"))
                    {
                        fType = BreakIterator.KIND_CHARACTER;
                    }
                    else if (value.Equals("word"))
                    {
                        fType = BreakIterator.KIND_WORD;
                    }
                    else if (value.Equals("line"))
                    {
                        fType = BreakIterator.KIND_LINE;
                    }
                    else if (value.Equals("sentence"))
                    {
                        fType = BreakIterator.KIND_SENTENCE;
                    }
                    else
                    {
                        String msg = String.Format("{0}: Unrecognized break type {1}", fMonkeyImpl.fRuleFileName, value);
                        Console.Error.WriteLine(msg);
                        throw new ArgumentException(msg);
                    }
                    return true;
                }
                return false;
            }


            internal RuleBasedBreakIterator CreateICUBreakIterator()
            {
                BreakIterator bi;
                switch (fType)
                {
                    case BreakIterator.KIND_CHARACTER:
                        bi = (BreakIterator.GetCharacterInstance(fLocale));
                        break;
                    case BreakIterator.KIND_WORD:
                        bi = (BreakIterator.GetWordInstance(fLocale));
                        break;
                    case BreakIterator.KIND_LINE:
                        bi = (BreakIterator.GetLineInstance(fLocale));
                        break;
                    case BreakIterator.KIND_SENTENCE:
                        bi = (BreakIterator.GetSentenceInstance(fLocale));
                        break;
                    default:
                        String msg = String.Format("{0}: Bad break iterator type of {1}", fMonkeyImpl.fRuleFileName, fType);
                        Console.Error.WriteLine(msg);
                        throw new ArgumentException(msg);
                }
                return (RuleBasedBreakIterator)bi;

            }

            internal void CompileRules(String rules)
            {
                int lineNumber = 0;

                foreach (string l in Regex.Split(rules, "\\r?\\n"))
                {
                    string line = l;
                    ++lineNumber;
                    // Strip comment lines.
                    line = fCommentsMatcher.Replace(line, "", 1); // Replace first
                    if (line == string.Empty)
                    {
                        continue;
                    }

                    // Recognize character class definition and keyword lines
                    Match match = fClassDefMatcher.Match(line);
                    if (match.Success && match.Length == line.Length) // Matches()
                    {
                        string className = match.Groups[/*"ClassName"*/ 1].Value;
                        string classDef = match.Groups[/*"ClassDef"*/ 2].Value;
                        if (fMonkeyImpl.fDumpExpansions)
                        {
                            Console.Out.Write("scanned class: {0} = {1}\n", className, classDef);
                        }
                        if (SetKeywordParameter(className, classDef))
                        {
                            // The scanned item was "type = ..." or "locale = ...", etc.
                            //   which are not actual character classes.
                            continue;
                        }
                        AddCharClass(className, classDef);
                        continue;
                    }

                    // Recognize rule lines.
                    match = fRuleDefMatcher.Match(line);
                    if (match.Success && match.Length == line.Length) // Matches()
                    {
                        string ruleName = match.Groups[/*"RuleName"*/ 1].Value;
                        string ruleDef = match.Groups[/*"RuleDef"*/ 2].Value;
                        if (fMonkeyImpl.fDumpExpansions)
                        {
                            Console.Out.Write("scanned rule: {0} : {1}\n", ruleName, ruleDef);
                        }
                        AddRule(ruleName, ruleDef);
                        continue;
                    }

                    string msg = string.Format("Unrecognized line in rule file {0}:{1} \"{2}\"",
                            fMonkeyImpl.fRuleFileName, lineNumber, line);
                    Console.Error.WriteLine(msg);
                    throw new ArgumentException(msg);
                }

                // Build the vector of char classes, omitting the dictionary class if there is one.
                // This will be used when constructing the random text to be tested.

                // Also compute the "other" set, consisting of any characters not included in
                // one or more of the user defined sets.

                UnicodeSet otherSet = new UnicodeSet(0, 0x10ffff);

                foreach (var el in fCharClasses)
                {
                    string ccName = el.Key;
                    CharClass cclass = el.Value;

                    // Console.Out.Write("    Adding {0}\n", ccName);
                    if (!ccName.Equals(cclass.fName))
                    {
                        throw new ArgumentException(
                                String.Format("{0}: internal error, set names ({1}, {2}) inconsistent.\n",
                                        fMonkeyImpl.fRuleFileName, ccName, cclass.fName));
                    }
                    otherSet.RemoveAll(cclass.fSet);
                    if (ccName.Equals("dictionary"))
                    {
                        fDictionarySet = cclass.fSet;
                    }
                    else
                    {
                        fCharClassList.Add(cclass);
                    }
                }

                if (!otherSet.IsEmpty)
                {
                    // Console.Out.Write("have an other set.\n");
                    CharClass cclass = AddCharClass("__Others", otherSet.ToPattern(true));
                    fCharClassList.Add(cclass);
                }

            }

            internal CharClass GetClassForChar(int c)
            {
                foreach (CharClass cc in fCharClassList)
                {
                    if (cc.fSet.Contains(c))
                    {
                        return cc;
                    }
                }
                return null;
            }


            internal RBBIMonkeyImpl fMonkeyImpl;        // Pointer back to the owning MonkeyImpl instance.
            internal List<BreakRule> fBreakRules;        // Contents are of type (BreakRule *).

            internal IDictionary<String, CharClass> fCharClasses;       // Key is the set name.
                                                                        //                                          // Value is the corresponding CharClass
            internal List<CharClass> fCharClassList;     // Char Classes, same contents as fCharClasses values,

            internal UnicodeSet fDictionarySet;     // Dictionary set, empty if none is defined.
            internal ULocale fLocale;
            internal int fType;              // BreakItererator.KIND_WORD, etc.


            internal Regex fSetRefsMatcher;
            internal Regex fCommentsMatcher;
            internal Regex fClassDefMatcher;
            internal Regex fRuleDefMatcher;
            internal Regex fPropertyMatcher;
        }


        // class MonkeyTestData    represents a randomly synthesized test data string together
        //                         with the expected break positions obtained by applying
        //                         the test break rules.

        internal class MonkeyTestData
        {

            internal void Set(BreakRules rules, ICU_Rand rand)
            {
                int dataLength = 1000;   // length of test data to generate, in code points.

                // Fill the test string with random characters.
                // First randomly pick a char class, then randomly pick a character from that class.
                // Exclude any characters from the dictionary set.

                // System.out.println("Populating Test Data");
                fRandomSeed = rand.GetSeed();         // Save initial seed for use in error messages,
                                                      // allowing recreation of failing data.
                fBkRules = rules;
                StringBuilder newString = new StringBuilder();
                for (int n = 0; n < dataLength;)
                {
                    int charClassIndex = rand.Next() % rules.fCharClassList.Count;
                    CharClass cclass = rules.fCharClassList[charClassIndex];
                    if (cclass.fSet.Count == 0)
                    {
                        // Some rules or tailorings do end up with empty char classes.
                        continue;
                    }
                    int charIndex = rand.Next() % cclass.fSet.Count;
                    int c = cclass.fSet[charIndex];
                    if (/*Character.isBmpCodePoint(c)*/ c <= 0x0ffff && char.IsLowSurrogate((char)c) &&
                            newString.Length > 0 && char.IsHighSurrogate(newString[newString.Length - 1]))
                    {
                        // Character classes may contain unpaired surrogates, e.g. Grapheme_Cluster_Break = Control.
                        // Don't let random unpaired surrogates combine in the test data because they might
                        // produce an unwanted dictionary character.
                        continue;
                    }

                    if (!rules.fDictionarySet.Contains(c))
                    {
                        newString.AppendCodePoint(c);
                        ++n;
                    }
                }
                fString = newString.ToString();

                // Init the expectedBreaks, actualBreaks and ruleForPosition.
                // Expected and Actual breaks are one longer than the input string; a true value
                // will indicate a boundary preceding that position.

                fActualBreaks = new bool[fString.Length + 1];
                fExpectedBreaks = new bool[fString.Length + 1];
                fRuleForPosition = new int[fString.Length + 1];
                f2ndRuleForPos = new int[fString.Length + 1];

                // Apply reference rules to find the expected breaks.

                fExpectedBreaks[0] = true;       // Force an expected break before the start of the text.
                                                 // ICU always reports a break there.
                                                 // The reference rules do not have a means to do so.
                int strIdx = 0;
                while (strIdx < fString.Length)
                {
                    BreakRule matchingRule = null;
                    bool hasBreak = false;
                    int ruleNum = 0;
                    int matchStart = 0;
                    int matchEnd = 0;
                    Match match = null;
                    for (ruleNum = 0; ruleNum < rules.fBreakRules.Count; ruleNum++)
                    {
                        BreakRule rule = rules.fBreakRules[ruleNum];
                        //rule.fRuleMatcher.reset(fString.substring(strIdx));
                        //if (rule.fRuleMatcher.lookingAt())
                        //match = rule.fRuleMatcherLookingAt.Match(fString.Substring(strIdx));
                        match = rule.fRuleMatcher.Match(fString.Substring(strIdx), 0);
                        if (match.Success /*&& match.Index == 0*/) // LookingAt
                        {
                            // A candidate rule match, check further to see if we take it or continue to check other rules.
                            // Matches of zero or one code point count only if they also specify a break.
                            matchStart = strIdx;
                            //matchEnd = strIdx + rule.fRuleMatcher.end();
                            //hasBreak = BreakGroupStart(rule.fRuleMatcher) >= 0;
                            matchEnd = strIdx + match.Index + match.Length;
                            hasBreak = BreakGroupStart(match) >= 0;
                            if (hasBreak ||
                                    (matchStart < fString.Length && fString.OffsetByCodePoints(matchStart, 1) < matchEnd))
                            {
                                matchingRule = rule;
                                break;
                            }
                        }
                    }
                    if (matchingRule == null)
                    {
                        // No reference rule matched. This is an error in the rules that should never happen.
                        String msg = String.Format("{0}: No reference rules matched at position {1}. ",
                                rules.fMonkeyImpl.fRuleFileName, strIdx);
                        Console.Error.WriteLine(msg);
                        Dump(strIdx);
                        throw new ArgumentException(msg);
                    }
                    //if (matchingRule.fRuleMatcher.group().Length == 0)
                    if (matchingRule.fRuleMatcher.GetGroupNumbers().Length == 0)
                    {
                        // Zero length rule match. This is also an error in the rule expressions.
                        String msg = String.Format("{0}:{1}: Zero length rule match at {2}.",
                                rules.fMonkeyImpl.fRuleFileName, matchingRule.fName, strIdx);
                        Console.Error.WriteLine(msg);
                        Dump(strIdx);
                        throw new ArgumentException(msg);
                    }

                    // Record which rule matched over the length of the match.
                    for (int i = matchStart; i < matchEnd; i++)
                    {
                        if (fRuleForPosition[i] == 0)
                        {
                            fRuleForPosition[i] = ruleNum;
                        }
                        else
                        {
                            f2ndRuleForPos[i] = ruleNum;
                        }
                    }

                    // Break positions appear in rules as a matching named capture of zero length at the break position,
                    //   the adjusted pattern contains (?<BreakPosition>)
                    if (hasBreak)
                    {
                        //int breakPos = strIdx + BreakGroupStart(matchingRule.fRuleMatcher);
                        int breakPos = strIdx + BreakGroupStart(match);
                        fExpectedBreaks[breakPos] = true;
                        // Console.Out.Write("recording break at {0}\n", breakPos);
                        // For the next iteration, pick up applying rules immediately after the break,
                        // which may differ from end of the match. The matching rule may have included
                        // context following the boundary that needs to be looked at again.
                        strIdx = breakPos;
                    }
                    else
                    {
                        // Original rule didn't specify a break.
                        // Continue applying rules starting on the last code point of this match.
                        int updatedStrIdx = fString.OffsetByCodePoints(matchEnd, -1);
                        if (updatedStrIdx == matchStart)
                        {
                            // Match was only one code point, no progress if we continue.
                            // Shouldn't get here, case is filtered out at top of loop.
                            throw new ArgumentException(String.Format("{0}: Rule {1} internal error.",
                                    rules.fMonkeyImpl.fRuleFileName, matchingRule.fName));
                        }
                        strIdx = updatedStrIdx;
                    }
                }
            }

            // Helper function to find the starting index of a match of the "BreakPosition" named capture group.
            // @param m: a Java regex Matcher that has completed a matching operation.
            // @return m.start("BreakPosition),
            //         or -1 if there is no such group, or the group did not participate in the match.
            //
            // TODO: this becomes m.start("BreakPosition") with Java 8.
            //       In the mean time, assume that the only zero-length capturing group in
            //       a reference rule expression is the "BreakPosition" that corresponds to a "÷".

            internal static int BreakGroupStart(Match m)
            {
                for (int groupNum = 1; groupNum <= m.Groups.Count; ++groupNum)
                {
                    String group = m.Groups[groupNum].Value;
                    //if (group == null)
                    if (!m.Groups[groupNum].Success)
                    {
                        continue;
                    }
                    if (group.Equals(""))
                    {
                        // assert(m.end(groupNum) == m.end("BreakPosition"));
                        //return m.start(groupNum);
                        return m.Groups[groupNum].Index;
                    }
                }
                return -1;
            }

            internal void Dump(int around)
            {
                Console.Out.Write("\n"
                        + "         char                        break  Rule                     Character\n"
                        + "   pos   code   class                 R I   name                     name\n"
                        + "---------------------------------------------------------------------------------------------\n");

                int start;
                int end;

                if (around == -1)
                {
                    start = 0;
                    end = fString.Length;
                }
                else
                {
                    // Display context around a failure.
                    try
                    {
                        start = fString.OffsetByCodePoints(around, -30);
                    }
                    catch (Exception e)
                    {
                        start = 0;
                    }
                    try
                    {
                        end = fString.OffsetByCodePoints(around, +30);
                    }
                    catch (Exception e)
                    {
                        end = fString.Length;
                    }
                }

                for (int charIdx = start; charIdx < end; charIdx = fString.OffsetByCodePoints(charIdx, 1))
                {
                    int c = fString.CodePointAt(charIdx);
                    CharClass cc = fBkRules.GetClassForChar(c);

                    BreakRule rule = fBkRules.fBreakRules[fRuleForPosition[charIdx]];
                    String secondRuleName = "";
                    if (f2ndRuleForPos[charIdx] > 0)
                    {
                        secondRuleName = fBkRules.fBreakRules[f2ndRuleForPos[charIdx]].fName;
                    }
                    String cName = UCharacterName.Instance.GetName(c, UCharacterNameChoice.ExtendedCharName);

                    Console.Out.Write("  {0:d4} {1:x6}   {2}  {3} {4}   {5} {6}    {7}\n",
                            charIdx, c, cc.fName.PadRight(20, ' '),
                            fExpectedBreaks[charIdx] ? '*' : '.',
                            fActualBreaks[charIdx] ? '*' : '.',
                            rule.fName.PadRight(10, ' '), secondRuleName.PadRight(10, ' '), cName
                            );
                }

            }

            internal void ClearActualBreaks()
            {
                Arrays.Fill(fActualBreaks, false);
            }


            internal int fRandomSeed;        // The initial seed value from the random number generator.
            internal BreakRules fBkRules;           // The break rules used to generate this data.
            internal String fString;            // The text.
            internal bool[] fExpectedBreaks;  // Breaks as found by the reference rules.
                                              //     Parallel to fString. true if break preceding.
            internal bool[] fActualBreaks;    // Breaks as found by ICU break iterator.
            internal int[] fRuleForPosition; // Index into BreakRules.fBreakRules of rule that applied at each position.
                                             // Also parallel to fString.
            internal int[] f2ndRuleForPos;   // As above. A 2nd rule applies when the preceding rule
                                             //   didn't cause a break, and a subsequent rule match starts
                                             //   on the last code point of the preceding match.

        }


        // class RBBIMonkeyImpl     holds (some indirectly) everything associated with running a monkey
        //                          test for one set of break rules.
        //

        internal class RBBIMonkeyImpl : ThreadJob
        {

            internal void Setup(String ruleFile)
            {
                fRuleFileName = ruleFile;
                OpenBreakRules(ruleFile);
                fRuleSet = new BreakRules(this);
                fRuleSet.CompileRules(fRuleCharBuffer);
                fBI = fRuleSet.CreateICUBreakIterator();
                fTestData = new MonkeyTestData();
            }

            internal void OpenBreakRules(String fileName)
            {
                StringBuilder testFileBuf = new StringBuilder();
                Stream @is = null;
                String filePath = "ICU4N.Dev.Test.Rbbi.break_rules." + fileName;
                try
                {
                    @is = typeof(RBBIMonkeyImpl).GetTypeInfo().Assembly.GetManifestResourceStream(filePath);
                    if (@is == null)
                    {
                        Errln("Could not open test data file " + fileName);
                        return;
                    }
                    StreamReader isr = new StreamReader(@is, Encoding.UTF8);
                    try
                    {
                        int c;
                        int count = 0;
                        for (; ; )
                        {
                            c = isr.Read();
                            if (c < 0)
                            {
                                break;
                            }
                            count++;
                            if (c == 0xFEFF && count == 1)
                            {
                                // BOM in the test data file. Discard it.
                                continue;
                            }
                            testFileBuf.AppendCodePoint(c);
                        }
                    }
                    finally
                    {
                        isr.Dispose();
                    }
                }
                catch (IOException e)
                {
                    try
                    {
                        @is.Dispose();
                    }
                    catch (IOException ignored)
                    {
                    }
                    Errln(e.ToString());
                }
                fRuleCharBuffer = testFileBuf.ToString();  /* the file as a String */
            }

            internal class MonkeyException : Exception
            {
                private static readonly long serialVersionUID = 1L;
                public int fPosition;    // Position of the failure in the test data.
                internal MonkeyException(String description, int pos)
                        : base(description)
                {
                    fPosition = pos;
                }
            }

            public override void Run()
            {
                int errorCount = 0;
                if (fBI == null)
                {
                    fErrorMsgs.Append("Unable to run test because fBI is null.\n");
                    return;
                }
                for (long loopCount = 0; fLoopCount < 0 || loopCount < fLoopCount; loopCount++)
                {
                    try
                    {
                        fTestData.Set(fRuleSet, fRandomGenerator);
                        // fTestData.dump(-1);
                        TestForwards();
                        TestPrevious();
                        TestFollowing();
                        TestPreceding();
                        TestIsBoundary();
                    }
                    catch (MonkeyException e)
                    {
                        String formattedMsg = String.Format(
                                "{0} at index {1}. VM Arguments to reproduce: -Drules={2} -Dseed={3} -Dloop=1 -Dverbose=1 \"\n",
                                e.Message, e.fPosition, fRuleFileName, fTestData.fRandomSeed);
                        Console.Error.Write(formattedMsg);
                        if (fVerbose)
                        {
                            fTestData.Dump(e.fPosition);
                        }
                        fErrorMsgs.Append(formattedMsg);
                        if (++errorCount > 10)
                        {
                            return;
                        }
                    }
                    if (fLoopCount < 0 && loopCount % 100 == 0)
                    {
                        Console.Error.Write(".");
                    }
                }
            }

            internal enum CheckDirection
            {
                FORWARD,
                REVERSE
            }

            internal void TestForwards()
            {
                fTestData.ClearActualBreaks();
                fBI.SetText(fTestData.fString);
                int previousBreak = -2;
                for (int bk = fBI.First(); bk != BreakIterator.Done; bk = fBI.Next())
                {
                    if (bk <= previousBreak)
                    {
                        throw new MonkeyException("Break Iterator Stall", bk);
                    }
                    if (bk < 0 || bk > fTestData.fString.Length)
                    {
                        throw new MonkeyException("Boundary out of bounds", bk);
                    }
                    fTestData.fActualBreaks[bk] = true;
                }
                CheckResults("testForwards", CheckDirection.FORWARD);
            }


            internal void TestFollowing()
            {
                fTestData.ClearActualBreaks();
                fBI.SetText(fTestData.fString);
                int nextBreak = -1;
                for (int i = -1; i < fTestData.fString.Length; ++i)
                {
                    int bk = fBI.Following(i);
                    if (bk == BreakIterator.Done && i == fTestData.fString.Length)
                    {
                        continue;
                    }
                    if (bk == nextBreak && bk > i)
                    {
                        // i is in the gap between two breaks.
                        continue;
                    }
                    if (i == nextBreak && bk > nextBreak)
                    {
                        fTestData.fActualBreaks[bk] = true;
                        nextBreak = bk;
                        continue;
                    }
                    throw new MonkeyException("following(i)", i);
                }
                CheckResults("testFollowing", CheckDirection.FORWARD);
            }


            internal void TestPrevious()
            {
                fTestData.ClearActualBreaks();
                fBI.SetText(fTestData.fString);
                int previousBreak = int.MaxValue;
                for (int bk = fBI.Last(); bk != BreakIterator.Done; bk = fBI.Previous())
                {
                    if (bk >= previousBreak)
                    {
                        throw new MonkeyException("Break Iterator Stall", bk);
                    }
                    if (bk < 0 || bk > fTestData.fString.Length)
                    {
                        throw new MonkeyException("Boundary out of bounds", bk);
                    }
                    fTestData.fActualBreaks[bk] = true;
                }
                CheckResults("testPrevius", CheckDirection.REVERSE);
            }


            /**
             * Given an index into a string, if it refers to the trail surrogate of a surrogate pair,
             * adjust it to point to the lead surrogate, which is the start of the code point.
             * @param s the String.
             * @param i the initial index
             * @return the adjusted index
             */
            private int GetChar32Start(String s, int i)
            {
                if (i > 0 && i < s.Length &&
                        char.IsLowSurrogate(s[i]) && char.IsHighSurrogate(s[i - 1]))
                {
                    --i;
                }
                return i;
            }


            internal void TestPreceding()
            {
                fTestData.ClearActualBreaks();
                fBI.SetText(fTestData.fString);
                int nextBreak = fTestData.fString.Length + 1;
                for (int i = fTestData.fString.Length + 1; i >= 0; --i)
                {
                    int bk = fBI.Preceding(i);
                    // System.err.printf("testPreceding() i:%d  bk:%d  nextBreak:%d\n", i, bk, nextBreak);
                    if (bk == BreakIterator.Done && i == 0)
                    {
                        continue;
                    }
                    if (bk == nextBreak && bk < i)
                    {
                        // i is in the gap between two breaks.
                        continue;
                    }
                    if (i < fTestData.fString.Length && GetChar32Start(fTestData.fString, i) < i)
                    {
                        // i indexes to a trailing surrogate.
                        // Break Iterators treat an index to either half as referring to the supplemental code point,
                        // with preceding going to some preceding code point.
                        if (fBI.Preceding(i) != fBI.Preceding(GetChar32Start(fTestData.fString, i)))
                        {
                            throw new MonkeyException("preceding of trailing surrogate error", i);
                        }
                        continue;
                    }
                    if (i == nextBreak && bk < nextBreak)
                    {
                        fTestData.fActualBreaks[bk] = true;
                        nextBreak = bk;
                        continue;
                    }
                    throw new MonkeyException("preceding(i)", i);
                }
                CheckResults("testPreceding", CheckDirection.REVERSE);

            }


            internal void TestIsBoundary()
            {
                fTestData.ClearActualBreaks();
                fBI.SetText(fTestData.fString);
                for (int i = fTestData.fString.Length; i >= 0; --i)
                {
                    if (fBI.IsBoundary(i))
                    {
                        fTestData.fActualBreaks[i] = true;
                    }
                }
                CheckResults("testForwards", CheckDirection.FORWARD);
            }


            internal void CheckResults(String msg, CheckDirection direction)
            {
                if (direction == CheckDirection.FORWARD)
                {
                    for (int i = 0; i <= fTestData.fString.Length; ++i)
                    {
                        if (fTestData.fExpectedBreaks[i] != fTestData.fActualBreaks[i])
                        {
                            throw new MonkeyException(msg, i);
                        }
                    }
                }
                else
                {
                    for (int i = fTestData.fString.Length; i >= 0; i--)
                    {
                        if (fTestData.fExpectedBreaks[i] != fTestData.fActualBreaks[i])
                        {
                            throw new MonkeyException(msg, i);
                        }
                    }
                }

            }

            internal String fRuleCharBuffer;         // source file contents of the reference rules.
            internal BreakRules fRuleSet;
            internal RuleBasedBreakIterator fBI;
            internal MonkeyTestData fTestData;
            internal ICU_Rand fRandomGenerator;
            internal String fRuleFileName;
            internal bool fVerbose;                 // True to do long dump of failing data.
            internal int fLoopCount;
            internal int fErrorCount;

            internal bool fDumpExpansions;          // Debug flag to output expanded form of rules and sets.
            internal StringBuilder fErrorMsgs = new StringBuilder();

        }

        //  Test parameters, specified via Java properties.
        //
        //  rules=file_name   Name of file containing the reference rules.
        //  seed=nnnnn        Random number starting seed.
        //                    Setting the seed allows errors to be reproduced.
        //  loop=nnn          Looping count.  Controls running time.
        //                    -1:  run forever.
        //                     0 or greater:  run length.
        //  expansions        debug option, show expansions of rules and sets.
        //  verbose           Display details of the failure.
        //
        // Parameters are passed to the JVM on the command line, or
        // via the Eclipse Run Configuration settings, arguments tab, VM parameters.
        // For example,
        //      -ea -Drules=line.txt -Dloop=-1
        //
        [Test]
        [Ignore("ICU4N TODO: Fix this")]
        public void TestMonkey()
        {
            fail("TODO: Rule regex never matches the test data, so we never actually test anything (even though it passes).");

            String[] tests = {"grapheme.txt", "word.txt", "line.txt", "sentence.txt", "line_normal.txt",
                    "line_normal_cj.txt", "line_loose.txt", "line_loose_cj.txt", "word_POSIX.txt"
            };

            String testNameFromParams = GetProperty("rules");

            if (testNameFromParams != null)
            {
                tests = new String[] { testNameFromParams };
            }

            int loopCount = GetIntProperty("loop", IsQuick() ? 100 : 5000);
            bool dumpExpansions = GetBooleanProperty("expansions", false);
            bool verbose = GetBooleanProperty("verbose", false);
            int seed = GetIntProperty("seed", 1);

            List<RBBIMonkeyImpl> startedTests = new List<RBBIMonkeyImpl>();

            // Monkey testing is multi-threaded.
            // Each set of break rules to be tested is run in a separate thread.
            // Each thread/set of rules gets a separate RBBIMonkeyImpl object.

            foreach (String testName in tests)
            {
                Logln(String.Format("beginning testing of {0}", testName));

                RBBIMonkeyImpl test = new RBBIMonkeyImpl
                {
                    fDumpExpansions = dumpExpansions,
                    fVerbose = verbose,
                    fRandomGenerator = new ICU_Rand(seed),
                    fLoopCount = loopCount
                };
                test.Setup(testName);

                test.Start();
                startedTests.Add(test);
            }

            StringBuilder errors = new StringBuilder();
            foreach (RBBIMonkeyImpl test in startedTests)
            {
#if !NETCOREAPP1_0
                try
                {
#endif
                    test.Join();
                    errors.Append(test.fErrorMsgs);
#if !NETCOREAPP1_0
                }
                catch (ThreadInterruptedException e)
                {
                    errors.Append(e + "\n");
                }
#endif
            }
            String errorMsgs = errors.ToString();
            assertEquals(errorMsgs, "", errorMsgs);

        }
    }
}
