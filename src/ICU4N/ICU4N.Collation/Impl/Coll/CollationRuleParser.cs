using ICU4N.Lang;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Impl.Coll
{
    public sealed class CollationRuleParser
    {
        /** Special reset positions. */
        internal enum Position
        {
            FIRST_TERTIARY_IGNORABLE,
            LAST_TERTIARY_IGNORABLE,
            FIRST_SECONDARY_IGNORABLE,
            LAST_SECONDARY_IGNORABLE,
            FIRST_PRIMARY_IGNORABLE,
            LAST_PRIMARY_IGNORABLE,
            FIRST_VARIABLE,
            LAST_VARIABLE,
            FIRST_REGULAR,
            LAST_REGULAR,
            FIRST_IMPLICIT,
            LAST_IMPLICIT,
            FIRST_TRAILING,
            LAST_TRAILING
        }
        internal static readonly Position[] POSITION_VALUES = (Position[])Enum.GetValues(typeof(Position));

        /**
         * First character of contractions that encode special reset positions.
         * U+FFFE cannot be tailored via rule syntax.
         *
         * The second contraction character is POS_BASE + Position.
         */
        internal static readonly char POS_LEAD = (char)0xfffe;
        /**
         * Base for the second character of contractions that encode special reset positions.
         * Braille characters U+28xx are printable and normalization-inert.
         * @see POS_LEAD
         */
        internal static readonly char POS_BASE = (char)0x2800;

        public abstract class Sink // ICU4N specific - made public because it has public subclass
        {
            /**
             * Adds a reset.
             * strength=UCOL_IDENTICAL for &str.
             * strength=UCOL_PRIMARY/UCOL_SECONDARY/UCOL_TERTIARY for &[before n]str where n=1/2/3.
             */
            internal abstract void AddReset(CollationStrength strength, ICharSequence str);
            /**
             * Adds a relation with strength and prefix | str / extension.
             */
            internal abstract void AddRelation(CollationStrength strength, ICharSequence prefix,
                    ICharSequence str, string extension); // ICU4N specific - changed extension from ICharSequence to string

            internal virtual void SuppressContractions(UnicodeSet set) { }

            internal virtual void Optimize(UnicodeSet set) { }
        }

        internal interface IImporter
        {
            string GetRules(string localeID, string collationType);
        }

        /**
         * Constructor.
         * The Sink must be set before parsing.
         * The Importer can be set, otherwise [import locale] syntax is not supported.
         */
        internal CollationRuleParser(CollationData baseData)
        {
            this.baseData = baseData;
        }

        /**
         * Sets the pointer to a Sink object.
         * The pointer is aliased: Pointer copy without cloning or taking ownership.
         */
        internal void SetSink(Sink sinkAlias)
        {
            sink = sinkAlias;
        }

        /**
         * Sets the pointer to an Importer object.
         * The pointer is aliased: Pointer copy without cloning or taking ownership.
         */
        internal void SetImporter(IImporter importerAlias)
        {
            importer = importerAlias;
        }

        internal void Parse(string ruleString, CollationSettings outSettings)
        {
            settings = outSettings;
            Parse(ruleString);
        }

        private static readonly int UCOL_DEFAULT = -1;
        private static readonly int UCOL_OFF = 0;
        private static readonly int UCOL_ON = 1;

        /** UCOL_PRIMARY=0 .. UCOL_IDENTICAL=15 */
        private static readonly int STRENGTH_MASK = 0xf;
        private static readonly int STARRED_FLAG = 0x10;
        private static readonly int OFFSET_SHIFT = 8;

        private static readonly string BEFORE = "[before";

        // In C++, we parse into temporary UnicodeString objects named "raw" or "str".
        // In Java, we reuse this StringBuilder.
        private readonly StringBuilderCharSequence rawBuilder = new StringBuilderCharSequence();

        private void Parse(string ruleString)
        {
            rules = ruleString;
            ruleIndex = 0;

            while (ruleIndex < rules.Length)
            {
                char c = rules[ruleIndex];
                if (PatternProps.IsWhiteSpace(c))
                {
                    ++ruleIndex;
                    continue;
                }
                switch (c)
                {
                    case '&':
                        ParseRuleChain();
                        break;
                    case '[':
                        ParseSetting();
                        break;
                    case '#': // starts a comment, until the end of the line
                        ruleIndex = SkipComment(ruleIndex + 1);
                        break;
                    case '@': // is equivalent to [backwards 2]
                        settings.SetFlag(CollationSettings.BACKWARD_SECONDARY, true);
                        ++ruleIndex;
                        break;
                    case '!':  // '!' used to turn on Thai/Lao character reversal
                               // Accept but ignore. The root collator has contractions
                               // that are equivalent to the character reversal, where appropriate.
                        ++ruleIndex;
                        break;
                    default:
                        SetParseError("expected a reset or setting or comment");
                        break;
                }
            }
        }

        private void ParseRuleChain()
        {
            CollationStrength resetStrength = (CollationStrength)ParseResetAndPosition();
            bool isFirstRelation = true;
            for (; ; )
            {
                int result = ParseRelationOperator();
                if (result < 0)
                {
                    if (ruleIndex < rules.Length && rules[ruleIndex] == 0x23)
                    {
                        // '#' starts a comment, until the end of the line
                        ruleIndex = SkipComment(ruleIndex + 1);
                        continue;
                    }
                    if (isFirstRelation)
                    {
                        SetParseError("reset not followed by a relation");
                    }
                    return;
                }
                CollationStrength strength = (CollationStrength)(result & STRENGTH_MASK);
                if (resetStrength < CollationStrength.Identical)
                {
                    // reset-before rule chain
                    if (isFirstRelation)
                    {
                        if (strength != resetStrength)
                        {
                            SetParseError("reset-before strength differs from its first relation");
                            return;
                        }
                    }
                    else
                    {
                        if (strength < resetStrength)
                        {
                            SetParseError("reset-before strength followed by a stronger relation");
                            return;
                        }
                    }
                }
                int i = ruleIndex + (result >> OFFSET_SHIFT);  // skip over the relation operator
                if ((result & STARRED_FLAG) == 0)
                {
                    ParseRelationStrings(strength, i);
                }
                else
                {
                    ParseStarredCharacters(strength, i);
                }
                isFirstRelation = false;
            }
        }

        private CollationStrength ParseResetAndPosition()
        {
            int i = SkipWhiteSpace(ruleIndex + 1);
            int j;
            char c;
            CollationStrength resetStrength;
            if (rules.RegionMatches(i, BEFORE, 0, BEFORE.Length) &&
                    (j = i + BEFORE.Length) < rules.Length &&
                    PatternProps.IsWhiteSpace(rules[j]) &&
                    ((j = SkipWhiteSpace(j + 1)) + 1) < rules.Length &&
                    0x31 <= (c = rules[j]) && c <= 0x33 &&
                    rules[j + 1] == 0x5d)
            {
                // &[before n] with n=1 or 2 or 3
                resetStrength = CollationStrength.Primary + (c - 0x31);
                i = SkipWhiteSpace(j + 2);
            }
            else
            {
                resetStrength = CollationStrength.Identical;
            }
            if (i >= rules.Length)
            {
                SetParseError("reset without position");
                return (CollationStrength)UCOL_DEFAULT;
            }
            if (rules[i] == 0x5b)
            {  // '['
                i = ParseSpecialPosition(i, rawBuilder.StringBuilder);
            }
            else
            {
                i = ParseTailoringString(i, rawBuilder.StringBuilder);
            }
            try
            {
                sink.AddReset(resetStrength, rawBuilder);
            }
            catch (Exception e)
            {
                SetParseError("adding reset failed", e);
                return (CollationStrength)UCOL_DEFAULT;
            }
            ruleIndex = i;
            return resetStrength;
        }

        private int ParseRelationOperator()
        {
            ruleIndex = SkipWhiteSpace(ruleIndex);
            if (ruleIndex >= rules.Length) { return UCOL_DEFAULT; }
            int strength;
            int i = ruleIndex;
            char c = rules[i++];
            switch (c)
            {
                case '<':
                    if (i < rules.Length && rules[i] == 0x3c)
                    {  // <<
                        ++i;
                        if (i < rules.Length && rules[i] == 0x3c)
                        {  // <<<
                            ++i;
                            if (i < rules.Length && rules[i] == 0x3c)
                            {  // <<<<
                                ++i;
                                strength = (int)CollationStrength.Quaternary;
                            }
                            else
                            {
                                strength = (int)CollationStrength.Tertiary;
                            }
                        }
                        else
                        {
                            strength = (int)CollationStrength.Secondary;
                        }
                    }
                    else
                    {
                        strength = (int)CollationStrength.Primary;
                    }
                    if (i < rules.Length && rules[i] == 0x2a)
                    {  // '*'
                        ++i;
                        strength |= STARRED_FLAG;
                    }
                    break;
                case ';': // same as <<
                    strength = (int)CollationStrength.Secondary;
                    break;
                case ',': // same as <<<
                    strength = (int)CollationStrength.Tertiary;
                    break;
                case '=':
                    strength = (int)CollationStrength.Identical;
                    if (i < rules.Length && rules[i] == 0x2a)
                    {  // '*'
                        ++i;
                        strength |= STARRED_FLAG;
                    }
                    break;
                default:
                    return UCOL_DEFAULT;
            }
            return ((i - ruleIndex) << OFFSET_SHIFT) | strength;
        }

        private void ParseRelationStrings(CollationStrength strength, int i)
        {
            // Parse
            //     prefix | str / extension
            // where prefix and extension are optional.
            StringCharSequence prefix = new StringCharSequence("");
            string extension = "";
            i = ParseTailoringString(i, rawBuilder.StringBuilder);
            char next = (i < rules.Length) ? rules[i] : (char)0;
            if (next == 0x7c)
            {  // '|' separates the context prefix from the string.
                prefix = new StringCharSequence(rawBuilder.ToString());
                i = ParseTailoringString(i + 1, rawBuilder.StringBuilder);
                next = (i < rules.Length) ? rules[i] : (char)0;
            }
            // str = rawBuilder (do not modify rawBuilder any more in this function)
            if (next == 0x2f)
            {  // '/' separates the string from the extension.
                StringBuilder extBuilder = new StringBuilder();
                i = ParseTailoringString(i + 1, extBuilder);
                extension = extBuilder.ToString();
            }
            if (prefix.Length != 0)
            {
                int prefix0 = prefix.String.CodePointAt(0);
                int c = rawBuilder.StringBuilder.CodePointAt(0);
                if (!nfc.HasBoundaryBefore(prefix0) || !nfc.HasBoundaryBefore(c))
                {
                    SetParseError("in 'prefix|str', prefix and str must each start with an NFC boundary");
                    return;
                }
            }
            try
            {
                sink.AddRelation(strength, prefix, rawBuilder, extension);
            }
            catch (Exception e)
            {
                SetParseError("adding relation failed", e);
                return;
            }
            ruleIndex = i;
        }

        private void ParseStarredCharacters(CollationStrength strength, int i)
        {
            StringCharSequence empty = new StringCharSequence("");
            i = ParseString(SkipWhiteSpace(i), rawBuilder.StringBuilder);
            if (rawBuilder.Length == 0)
            {
                SetParseError("missing starred-relation string");
                return;
            }
            int prev = -1;
            int j = 0;
            for (; ; )
            {
                while (j < rawBuilder.Length)
                {
                    int cp = rawBuilder.StringBuilder.CodePointAt(j);
                    if (!nfd.IsInert(cp))
                    {
                        SetParseError("starred-relation string is not all NFD-inert");
                        return;
                    }
                    try
                    {
                        sink.AddRelation(strength, empty, UTF16.ValueOf(cp).ToCharSequence(), empty.String);
                    }
                    catch (Exception e)
                    {
                        SetParseError("adding relation failed", e);
                        return;
                    }
                    j += Character.CharCount(cp);
                    prev = cp;
                }
                if (i >= rules.Length || rules[i] != 0x2d)
                {  // '-'
                    break;
                }
                if (prev < 0)
                {
                    SetParseError("range without start in starred-relation string");
                    return;
                }
                i = ParseString(i + 1, rawBuilder.StringBuilder);
                if (rawBuilder.Length == 0)
                {
                    SetParseError("range without end in starred-relation string");
                    return;
                }
                int c = rawBuilder.StringBuilder.CodePointAt(0);
                if (c < prev)
                {
                    SetParseError("range start greater than end in starred-relation string");
                    return;
                }
                // range prev-c
                while (++prev <= c)
                {
                    if (!nfd.IsInert(prev))
                    {
                        SetParseError("starred-relation string range is not all NFD-inert");
                        return;
                    }
                    if (IsSurrogate(prev))
                    {
                        SetParseError("starred-relation string range contains a surrogate");
                        return;
                    }
                    if (0xfffd <= prev && prev <= 0xffff)
                    {
                        SetParseError("starred-relation string range contains U+FFFD, U+FFFE or U+FFFF");
                        return;
                    }
                    try
                    {
                        sink.AddRelation(strength, empty, UTF16.ValueOf(prev).ToCharSequence(), empty.String);
                    }
                    catch (Exception e)
                    {
                        SetParseError("adding relation failed", e);
                        return;
                    }
                }
                prev = -1;
                j = Character.CharCount(c);
            }
            ruleIndex = SkipWhiteSpace(i);
        }

        private int ParseTailoringString(int i, StringBuilder raw)
        {
            i = ParseString(SkipWhiteSpace(i), raw);
            if (raw.Length == 0)
            {
                SetParseError("missing relation string");
            }
            return SkipWhiteSpace(i);
        }

        private int ParseString(int i, StringBuilder raw)
        {
            raw.Length = 0;
            while (i < rules.Length)
            {
                char c = rules[i++];
                if (IsSyntaxChar(c))
                {
                    if (c == 0x27)
                    {  // apostrophe
                        if (i < rules.Length && rules[i] == 0x27)
                        {
                            // Double apostrophe, encodes a single one.
                            raw.Append((char)0x27);
                            ++i;
                            continue;
                        }
                        // Quote literal text until the next single apostrophe.
                        for (; ; )
                        {
                            if (i == rules.Length)
                            {
                                SetParseError("quoted literal text missing terminating apostrophe");
                                return i;
                            }
                            c = rules[i++];
                            if (c == 0x27)
                            {
                                if (i < rules.Length && rules[i] == 0x27)
                                {
                                    // Double apostrophe inside quoted literal text,
                                    // still encodes a single apostrophe.
                                    ++i;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            raw.Append(c);
                        }
                    }
                    else if (c == 0x5c)
                    {  // backslash
                        if (i == rules.Length)
                        {
                            SetParseError("backslash escape at the end of the rule string");
                            return i;
                        }
                        int cp = rules.CodePointAt(i);
                        raw.AppendCodePoint(cp);
                        i += Character.CharCount(cp);
                    }
                    else
                    {
                        // Any other syntax character terminates a string.
                        --i;
                        break;
                    }
                }
                else if (PatternProps.IsWhiteSpace(c))
                {
                    // Unquoted white space terminates a string.
                    --i;
                    break;
                }
                else
                {
                    raw.Append(c);
                }
            }
            for (int j = 0; j < raw.Length;)
            {
                int c = raw.CodePointAt(j);
                if (IsSurrogate(c))
                {
                    SetParseError("string contains an unpaired surrogate");
                    return i;
                }
                if (0xfffd <= c && c <= 0xffff)
                {
                    SetParseError("string contains U+FFFD, U+FFFE or U+FFFF");
                    return i;
                }
                j += Character.CharCount(c);
            }
            return i;
        }

        // TODO: Widen UTF16.isSurrogate(char16) to take an int.
        private static bool IsSurrogate(int c)
        {
            return (c & 0xfffff800) == 0xd800;
        }

        private static readonly string[] positions = {
                "first tertiary ignorable",
                "last tertiary ignorable",
                "first secondary ignorable",
                "last secondary ignorable",
                "first primary ignorable",
                "last primary ignorable",
                "first variable",
                "last variable",
                "first regular",
                "last regular",
                "first implicit",
                "last implicit",
                "first trailing",
                "last trailing"
            };

        /**
         * Sets str to a contraction of U+FFFE and (U+2800 + Position).
         * @return rule index after the special reset position
         * @throws ParseException 
         */
        private int ParseSpecialPosition(int i, StringBuilder str)
        {
            int j = ReadWords(i + 1, rawBuilder.StringBuilder);
            if (j > i && rules[j] == 0x5d && rawBuilder.Length != 0)
            {  // words end with ]
                ++j;
                string raw = rawBuilder.ToString();
                str.Length = 0;
                for (int pos = 0; pos < positions.Length; ++pos)
                {
                    if (raw.Equals(positions[pos]))
                    {
                        str.Append(POS_LEAD).Append((char)(POS_BASE + pos));
                        return j;
                    }
                }
                if (raw.Equals("top"))
                {
                    str.Append(POS_LEAD).Append((char)(POS_BASE + (int)Position.LAST_REGULAR));
                    return j;
                }
                if (raw.Equals("variable top"))
                {
                    str.Append(POS_LEAD).Append((char)(POS_BASE + (int)Position.LAST_VARIABLE));
                    return j;
                }
            }
            SetParseError("not a valid special reset position");
            return i;
        }

        private void ParseSetting()
        {
            int i = ruleIndex + 1;
            int j = ReadWords(i, rawBuilder.StringBuilder);
            if (j <= i || rawBuilder.Length == 0)
            {
                SetParseError("expected a setting/option at '['");
            }
            // startsWith() etc. are available for String but not CharSequence/StringBuilder.
            string raw = rawBuilder.ToString();
            if (rules[j] == 0x5d)
            {  // words end with ]
                ++j;
                if (raw.StartsWith("reorder", StringComparison.Ordinal) &&
                        (raw.Length == 7 || raw[7] == 0x20))
                {
                    ParseReordering(raw);
                    ruleIndex = j;
                    return;
                }
                if (raw.Equals("backwards 2"))
                {
                    settings.SetFlag(CollationSettings.BACKWARD_SECONDARY, true);
                    ruleIndex = j;
                    return;
                }
                string v;
                int valueIndex = raw.LastIndexOf((char)0x20);
                if (valueIndex >= 0)
                {
                    v = raw.Substring(valueIndex + 1);
                    raw = raw.Substring(0, valueIndex - 0); // ICU4N: Checked 2nd parameter
                }
                else
                {
                    v = "";
                }
                if (raw.Equals("strength") && v.Length == 1)
                {
                    int value = UCOL_DEFAULT;
                    char c = v[0];
                    if (0x31 <= c && c <= 0x34)
                    {  // 1..4
                        value = (int)CollationStrength.Primary + (c - 0x31);
                    }
                    else if (c == 0x49)
                    {  // 'I'
                        value = (int)CollationStrength.Identical;
                    }
                    if (value != UCOL_DEFAULT)
                    {
                        settings.Strength = (CollationStrength)value;
                        ruleIndex = j;
                        return;
                    }
                }
                else if (raw.Equals("alternate"))
                {
                    int value = UCOL_DEFAULT;
                    if (v.Equals("non-ignorable"))
                    {
                        value = 0;  // UCOL_NON_IGNORABLE
                    }
                    else if (v.Equals("shifted"))
                    {
                        value = 1;  // UCOL_SHIFTED
                    }
                    if (value != UCOL_DEFAULT)
                    {
                        settings.SetAlternateHandlingShifted(value > 0);
                        ruleIndex = j;
                        return;
                    }
                }
                else if (raw.Equals("maxVariable"))
                {
                    int value = UCOL_DEFAULT;
                    if (v.Equals("space"))
                    {
                        value = CollationSettings.MAX_VAR_SPACE;
                    }
                    else if (v.Equals("punct"))
                    {
                        value = CollationSettings.MAX_VAR_PUNCT;
                    }
                    else if (v.Equals("symbol"))
                    {
                        value = CollationSettings.MAX_VAR_SYMBOL;
                    }
                    else if (v.Equals("currency"))
                    {
                        value = CollationSettings.MAX_VAR_CURRENCY;
                    }
                    if (value != UCOL_DEFAULT)
                    {
                        settings.SetMaxVariable(value, 0);
                        settings.VariableTop = baseData.GetLastPrimaryForGroup(
                            ReorderCodes.First + value);
                        Debug.Assert(settings.VariableTop != 0);
                        ruleIndex = j;
                        return;
                    }
                }
                else if (raw.Equals("caseFirst"))
                {
                    int value = UCOL_DEFAULT;
                    if (v.Equals("off"))
                    {
                        value = UCOL_OFF;
                    }
                    else if (v.Equals("lower"))
                    {
                        value = CollationSettings.CASE_FIRST;  // UCOL_LOWER_FIRST
                    }
                    else if (v.Equals("upper"))
                    {
                        value = CollationSettings.CASE_FIRST_AND_UPPER_MASK;  // UCOL_UPPER_FIRST
                    }
                    if (value != UCOL_DEFAULT)
                    {
                        settings.CaseFirst=value;
                        ruleIndex = j;
                        return;
                    }
                }
                else if (raw.Equals("caseLevel"))
                {
                    int value = GetOnOffValue(v);
                    if (value != UCOL_DEFAULT)
                    {
                        settings.SetFlag(CollationSettings.CASE_LEVEL, value > 0);
                        ruleIndex = j;
                        return;
                    }
                }
                else if (raw.Equals("normalization"))
                {
                    int value = GetOnOffValue(v);
                    if (value != UCOL_DEFAULT)
                    {
                        settings.SetFlag(CollationSettings.CHECK_FCD, value > 0);
                        ruleIndex = j;
                        return;
                    }
                }
                else if (raw.Equals("numericOrdering"))
                {
                    int value = GetOnOffValue(v);
                    if (value != UCOL_DEFAULT)
                    {
                        settings.SetFlag(CollationSettings.NUMERIC, value > 0);
                        ruleIndex = j;
                        return;
                    }
                }
                else if (raw.Equals("hiraganaQ"))
                {
                    int value = GetOnOffValue(v);
                    if (value != UCOL_DEFAULT)
                    {
                        if (value == UCOL_ON)
                        {
                            SetParseError("[hiraganaQ on] is not supported");
                        }
                        ruleIndex = j;
                        return;
                    }
                }
                else if (raw.Equals("import"))
                {
                    // BCP 47 language tag -> ICU locale ID
                    ULocale localeID;
                    try
                    {
                        localeID = new ULocale.Builder().SetLanguageTag(v).Build();
                    }
                    catch (Exception e)
                    {
                        SetParseError("expected language tag in [import langTag]", e);
                        return;
                    }
                    // localeID minus all keywords
                    string baseID = localeID.GetBaseName();
                    // @collation=type, or length=0 if not specified
                    string collationType = localeID.GetKeywordValue("collation");
                    if (importer == null)
                    {
                        SetParseError("[import langTag] is not supported");
                    }
                    else
                    {
                        String importedRules;
                        try
                        {
                            importedRules =
                                importer.GetRules(baseID,
                                        collationType != null ? collationType : "standard");
                        }
                        catch (Exception e)
                        {
                            SetParseError("[import langTag] failed", e);
                            return;
                        }
                        string outerRules = rules;
                        int outerRuleIndex = ruleIndex;
                        try
                        {
                            Parse(importedRules);
                        }
                        catch (Exception e)
                        {
                            ruleIndex = outerRuleIndex;  // Restore the original index for error reporting.
                            SetParseError("parsing imported rules failed", e);
                        }
                        rules = outerRules;
                        ruleIndex = j;
                    }
                    return;
                }
            }
            else if (rules[j] == 0x5b)
            {  // words end with [
                UnicodeSet set = new UnicodeSet();
                j = ParseUnicodeSet(j, set);
                if (raw.Equals("optimize"))
                {
                    try
                    {
                        sink.Optimize(set);
                    }
                    catch (Exception e)
                    {
                        SetParseError("[optimize set] failed", e);
                    }
                    ruleIndex = j;
                    return;
                }
                else if (raw.Equals("suppressContractions"))
                {
                    try
                    {
                        sink.SuppressContractions(set);
                    }
                    catch (Exception e)
                    {
                        SetParseError("[suppressContractions set] failed", e);
                    }
                    ruleIndex = j;
                    return;
                }
            }
            SetParseError("not a valid setting/option");
        }

        private void ParseReordering(string raw) // ICU4N specific - changed raw from ICharSequence to string
        {
            int i = 7;  // after "reorder"
            if (i == raw.Length)
            {
                // empty [reorder] with no codes
                settings.ResetReordering();
                return;
            }
            // Parse the codes in [reorder aa bb cc].
            List<int> reorderCodes = new List<int>();
            while (i < raw.Length)
            {
                ++i;  // skip the word-separating space
                int limit = i;
                while (limit < raw.Length && raw[limit] != ' ') { ++limit; }
                string word = raw.SubSequence(i, limit).ToString();
                int code = GetReorderCode(word);
                if (code < 0)
                {
                    SetParseError("unknown script or reorder code");
                    return;
                }
                reorderCodes.Add(code);
                i = limit;
            }
            if (reorderCodes.Count == 0)
            {
                settings.ResetReordering();
            }
            else
            {
                int[] codes = new int[reorderCodes.Count];
                int j = 0;
                foreach (int code in reorderCodes) { codes[j++] = code; }
                settings.SetReordering(baseData, codes);
            }
        }

        private static readonly string[] gSpecialReorderCodes = {
        "space", "punct", "symbol", "currency", "digit"
    };

        /**
         * Gets a script or reorder code from its string representation.
         * @return the script/reorder code, or
         * -1 if not recognized
         */
        public static int GetReorderCode(string word)
        {
            for (int i = 0; i < gSpecialReorderCodes.Length; ++i)
            {
                if (word.Equals(gSpecialReorderCodes[i], StringComparison.OrdinalIgnoreCase))
                {
                    return ReorderCodes.First + i;
                }
            }
            try
            {
                int script = UCharacter.GetPropertyValueEnum(UProperty.SCRIPT, word);
                if (script >= 0)
                {
                    return script;
                }
            }
            catch (IcuArgumentException e)
            {
                // fall through
            }
            if (word.Equals("others", StringComparison.OrdinalIgnoreCase))
            {
                return ReorderCodes.Others;  // same as Zzzz = USCRIPT_UNKNOWN 
            }
            return -1;
        }

        private static int GetOnOffValue(string s)
        {
            if (s.Equals("on"))
            {
                return UCOL_ON;
            }
            else if (s.Equals("off"))
            {
                return UCOL_OFF;
            }
            else
            {
                return UCOL_DEFAULT;
            }
        }

        private int ParseUnicodeSet(int i, UnicodeSet set)
        {
            // Collect a UnicodeSet pattern between a balanced pair of [brackets].
            int level = 0;
            int j = i;
            for (; ; )
            {
                if (j == rules.Length)
                {
                    SetParseError("unbalanced UnicodeSet pattern brackets");
                    return j;
                }
                char c = rules[j++];
                if (c == 0x5b)
                {  // '['
                    ++level;
                }
                else if (c == 0x5d)
                {  // ']'
                    if (--level == 0) { break; }
                }
            }
            try
            {
                set.ApplyPattern(rules.Substring(i, j - i)); // ICU4N: Corrected 2nd parameter
            }
            catch (Exception e)
            {
                SetParseError("not a valid UnicodeSet pattern: " + e.ToString());
            }
            j = SkipWhiteSpace(j);
            if (j == rules.Length || rules[j] != 0x5d)
            {
                SetParseError("missing option-terminating ']' after UnicodeSet pattern");
                return j;
            }
            return ++j;
        }

        private int ReadWords(int i, StringBuilder raw)
        {
            raw.Length = 0;
            i = SkipWhiteSpace(i);
            for (; ; )
            {
                if (i >= rules.Length) { return 0; }
                char c = rules[i];
                if (IsSyntaxChar(c) && c != 0x2d && c != 0x5f)
                {  // syntax except -_
                    if (raw.Length == 0) { return i; }
                    int lastIndex = raw.Length - 1;
                    if (raw[lastIndex] == ' ')
                    {  // remove trailing space
                        raw.Length = lastIndex;
                    }
                    return i;
                }
                if (PatternProps.IsWhiteSpace(c))
                {
                    raw.Append(' ');
                    i = SkipWhiteSpace(i + 1);
                }
                else
                {
                    raw.Append(c);
                    ++i;
                }
            }
        }

        private int SkipComment(int i)
        {
            // skip to past the newline
            while (i < rules.Length)
            {
                char c = rules[i++];
                // LF or FF or CR or NEL or LS or PS
                if (c == 0xa || c == 0xc || c == 0xd || c == 0x85 || c == 0x2028 || c == 0x2029)
                {
                    // Unicode Newline Guidelines: "A readline function should stop at NLF, LS, FF, or PS."
                    // NLF (new line function) = CR or LF or CR+LF or NEL.
                    // No need to collect all of CR+LF because a following LF will be ignored anyway.
                    break;
                }
            }
            return i;
        }

        private void SetParseError(string reason)
        {
            throw MakeParseException(reason);
        }

        private void SetParseError(string reason, Exception e)
        {
            FormatException newExc = MakeParseException(reason + ": " + e.ToString(), e);
            //newExc.initCause(e);
            throw newExc;
        }

        private FormatException MakeParseException(string reason)
        {
            return new FormatException(AppendErrorContext(reason) /*, ruleIndex*/);
        }

        private FormatException MakeParseException(string reason, Exception innerException)
        {
            return new FormatException(AppendErrorContext(reason) /*, ruleIndex*/, innerException);
        }

        private static readonly int U_PARSE_CONTEXT_LEN = 16;

        // C++ setErrorContext()
        private string AppendErrorContext(string reason)
        {
            // Note: This relies on the calling code maintaining the ruleIndex
            // at a position that is useful for debugging.
            // For example, at the beginning of a reset or relation etc.
            StringBuilder msg = new StringBuilder(reason);
            msg.Append(" at index ").Append(ruleIndex);
            // We are not counting line numbers.

            msg.Append(" near \"");
            // before ruleIndex
            int start = ruleIndex - (U_PARSE_CONTEXT_LEN - 1);
            if (start < 0)
            {
                start = 0;
            }
            else if (start > 0 && char.IsLowSurrogate(rules[start]))
            {
                ++start;
            }
            msg.Append(rules, start, ruleIndex);

            msg.Append('!');
            // starting from ruleIndex
            int length = rules.Length - ruleIndex;
            if (length >= U_PARSE_CONTEXT_LEN)
            {
                length = U_PARSE_CONTEXT_LEN - 1;
                if (char.IsHighSurrogate(rules[ruleIndex + length - 1]))
                {
                    --length;
                }
            }
            msg.Append(rules, ruleIndex, ruleIndex + length);
            return msg.Append('\"').ToString();
        }

        /**
         * ASCII [:P:] and [:S:]:
         * [\u0021-\u002F \u003A-\u0040 \u005B-\u0060 \u007B-\u007E]
         */
        private static bool IsSyntaxChar(int c)
        {
            return 0x21 <= c && c <= 0x7e &&
                    (c <= 0x2f || (0x3a <= c && c <= 0x40) ||
                    (0x5b <= c && c <= 0x60) || (0x7b <= c));
        }

        private int SkipWhiteSpace(int i)
        {
            while (i < rules.Length && PatternProps.IsWhiteSpace(rules[i]))
            {
                ++i;
            }
            return i;
        }

        private Normalizer2 nfd = Normalizer2.GetNFDInstance();
        private Normalizer2 nfc = Normalizer2.GetNFCInstance();

        private string rules;
        private readonly CollationData baseData;
        private CollationSettings settings;

        private Sink sink;
        private IImporter importer;

        private int ruleIndex;
    }
}
