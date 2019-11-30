using ICU4N.Globalization;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections;

namespace ICU4N.Dev.Test.Lang
{
    public class TestUScript : TestFmwk
    {
        /**
    * Constructor
    */
        public TestUScript()
        {
        }

        [Test]
        public void TestGetScriptOfCharsWithScriptExtensions()
        {
            /* test characters which have Script_Extensions */
            if (!(
                UScript.Common == UScript.GetScript(0x0640) &&
                UScript.Inherited == UScript.GetScript(0x0650) &&
                UScript.Arabic == UScript.GetScript(0xfdf2))
            )
            {
                Errln("UScript.getScript(character with Script_Extensions) failed");
            }
        }

        [Test]
        public void TestHasScript()
        {
            if (!(
                !UScript.HasScript(0x063f, UScript.Common) &&
                UScript.HasScript(0x063f, UScript.Arabic) &&  /* main Script value */
                !UScript.HasScript(0x063f, UScript.Syriac) &&
                !UScript.HasScript(0x063f, UScript.Thaana))
            )
            {
                Errln("UScript.hasScript(U+063F, ...) is wrong");
            }
            if (!(
                !UScript.HasScript(0x0640, UScript.Common) &&  /* main Script value */
                UScript.HasScript(0x0640, UScript.Arabic) &&
                UScript.HasScript(0x0640, UScript.Syriac) &&
                !UScript.HasScript(0x0640, UScript.Thaana))
            )
            {
                Errln("UScript.hasScript(U+0640, ...) is wrong");
            }
            if (!(
                !UScript.HasScript(0x0650, UScript.Inherited) &&  /* main Script value */
                UScript.HasScript(0x0650, UScript.Arabic) &&
                UScript.HasScript(0x0650, UScript.Syriac) &&
                !UScript.HasScript(0x0650, UScript.Thaana))
            )
            {
                Errln("UScript.hasScript(U+0650, ...) is wrong");
            }
            if (!(
                !UScript.HasScript(0x0660, UScript.Common) &&  /* main Script value */
                UScript.HasScript(0x0660, UScript.Arabic) &&
                !UScript.HasScript(0x0660, UScript.Syriac) &&
                UScript.HasScript(0x0660, UScript.Thaana))
            )
            {
                Errln("UScript.hasScript(U+0660, ...) is wrong");
            }
            if (!(
                !UScript.HasScript(0xfdf2, UScript.Common) &&
                UScript.HasScript(0xfdf2, UScript.Arabic) &&  /* main Script value */
                !UScript.HasScript(0xfdf2, UScript.Syriac) &&
                UScript.HasScript(0xfdf2, UScript.Thaana))
            )
            {
                Errln("UScript.hasScript(U+FDF2, ...) is wrong");
            }
            if (UScript.HasScript(0x0640, 0xaffe))
            {
                // An unguarded implementation might go into an infinite loop.
                Errln("UScript.hasScript(U+0640, bogus 0xaffe) is wrong");
            }
        }

        [Test]
        public void TestGetScriptExtensions()
        {
            BitArray scripts = new BitArray(UScript.CodeLimit);

            /* invalid code points */
            if (UScript.GetScriptExtensions(-1, scripts) != UScript.Unknown || scripts.Cardinality() != 1 ||
                    !scripts.Get(UScript.Unknown))
            {
                Errln("UScript.getScriptExtensions(-1) is not {UNKNOWN}");
            }
            if (UScript.GetScriptExtensions(0x110000, scripts) != UScript.Unknown || scripts.Cardinality() != 1 ||
                    !scripts.Get(UScript.Unknown))
            {
                Errln("UScript.getScriptExtensions(0x110000) is not {UNKNOWN}");
            }

            /* normal usage */
            if (UScript.GetScriptExtensions(0x063f, scripts) != UScript.Arabic || scripts.Cardinality() != 1 ||
                    !scripts.Get(UScript.Arabic))
            {
                Errln("UScript.getScriptExtensions(U+063F) is not {ARABIC}");
            }
            if (UScript.GetScriptExtensions(0x0640, scripts) > -3 || scripts.Cardinality() < 3 ||
               !scripts.Get(UScript.Arabic) || !scripts.Get(UScript.Syriac) || !scripts.Get(UScript.Mandaic)
            )
            {
                Errln("UScript.getScriptExtensions(U+0640) failed");
            }
            if (UScript.GetScriptExtensions(0xfdf2, scripts) != -2 || scripts.Cardinality() != 2 ||
                    !scripts.Get(UScript.Arabic) || !scripts.Get(UScript.Thaana))
            {
                Errln("UScript.getScriptExtensions(U+FDF2) failed");
            }
            if (UScript.GetScriptExtensions(0xff65, scripts) != -6 || scripts.Cardinality() != 6 ||
                    !scripts.Get(UScript.Bopomofo) || !scripts.Get(UScript.Yi))
            {
                Errln("UScript.getScriptExtensions(U+FF65) failed");
            }
        }

        [Test]
        public void TestScriptMetadataAPI()
        {
            /* API & code coverage. */
            String sample = UScript.GetSampleString(UScript.Latin);
            if (sample.Length != 1 || UScript.GetScript(sample[0]) != UScript.Latin)
            {
                Errln("UScript.getSampleString(Latn) failed");
            }
            sample = UScript.GetSampleString(UScript.InvalidCode);
            if (sample.Length != 0)
            {
                Errln("UScript.getSampleString(invalid) failed");
            }

            if (UScript.GetUsage(UScript.Latin) != ScriptUsage.Recommended ||
                    // Unicode 10 gives up on "aspirational".
                    UScript.GetUsage(UScript.Yi) != ScriptUsage.LimitedUse ||
                    UScript.GetUsage(UScript.Cherokee) != ScriptUsage.LimitedUse ||
                    UScript.GetUsage(UScript.Coptic) != ScriptUsage.Excluded ||
                    UScript.GetUsage(UScript.Cirth) != ScriptUsage.NotEncoded ||
                    UScript.GetUsage(UScript.InvalidCode) != ScriptUsage.NotEncoded ||
                    UScript.GetUsage(UScript.CodeLimit) != ScriptUsage.NotEncoded)
            {
                Errln("UScript.getUsage() failed");
            }

            if (UScript.IsRightToLeft(UScript.Latin) ||
                    UScript.IsRightToLeft(UScript.Cirth) ||
                    !UScript.IsRightToLeft(UScript.Arabic) ||
                    !UScript.IsRightToLeft(UScript.Hebrew))
            {
                Errln("UScript.isRightToLeft() failed");
            }

            if (UScript.BreaksBetweenLetters(UScript.Latin) ||
                    UScript.BreaksBetweenLetters(UScript.Cirth) ||
                    !UScript.BreaksBetweenLetters(UScript.Han) ||
                    !UScript.BreaksBetweenLetters(UScript.Thai))
            {
                Errln("UScript.breaksBetweenLetters() failed");
            }

            if (UScript.IsCased(UScript.Cirth) ||
                    UScript.IsCased(UScript.Han) ||
                    !UScript.IsCased(UScript.Latin) ||
                    !UScript.IsCased(UScript.Greek))
            {
                Errln("UScript.isCased() failed");
            }
        }

        /**
         * Maps a special script code to the most common script of its encoded characters.
         */
        private static int GetCharScript(int script)
        {
            switch (script)
            {
                case UScript.HanWithBopomofo:
                case UScript.SimplifiedHan:
                case UScript.TraditionalHan:
                    return UScript.Han;
                case UScript.Japanese:
                    return UScript.Hiragana;
                case UScript.Jamo:
                case UScript.Korean:
                    return UScript.Hangul;
                case UScript.SymbolsEmoji:
                    return UScript.Symbols;
                default:
                    return script;
            }
        }

        [Test]
        public void TestScriptMetadata()
        {
            UnicodeSet rtl = new UnicodeSet("[[:bc=R:][:bc=AL:]-[:Cn:]-[:sc=Common:]]");
            // So far, sample characters are uppercase.
            // Georgian is special.
            UnicodeSet cased = new UnicodeSet("[[:Lu:]-[:sc=Common:]-[:sc=Geor:]]");
            for (int sc = 0; sc < UScript.CodeLimit; ++sc)
            {
                String sn = UScript.GetShortName(sc);
                ScriptUsage usage = UScript.GetUsage(sc);
                String sample = UScript.GetSampleString(sc);
                UnicodeSet scriptSet = new UnicodeSet();
                scriptSet.ApplyInt32PropertyValue(UProperty.Script, sc);
                if (usage == ScriptUsage.NotEncoded)
                {
                    assertTrue(sn + " not encoded, no sample", sample.Length == 0);  // Java 6: sample.isEmpty()
                    assertFalse(sn + " not encoded, not RTL", UScript.IsRightToLeft(sc));
                    assertFalse(sn + " not encoded, not LB letters", UScript.BreaksBetweenLetters(sc));
                    assertFalse(sn + " not encoded, not cased", UScript.IsCased(sc));
                    assertTrue(sn + " not encoded, no characters", scriptSet.IsEmpty);
                }
                else
                {
                    assertFalse(sn + " encoded, has a sample character", sample.Length == 0);  // Java 6: sample.isEmpty()
                    int firstChar = sample.CodePointAt(0);
                    int charScript = GetCharScript(sc);
                    assertEquals(sn + " script(sample(script))",
                                 charScript, UScript.GetScript(firstChar));
                    assertEquals(sn + " RTL vs. set", rtl.Contains(firstChar), UScript.IsRightToLeft(sc));
                    assertEquals(sn + " cased vs. set", cased.Contains(firstChar), UScript.IsCased(sc));
                    assertEquals(sn + " encoded, has characters", sc == charScript, !scriptSet.IsEmpty);
                    if (UScript.IsRightToLeft(sc))
                    {
                        rtl.RemoveAll(scriptSet);
                    }
                    if (UScript.IsCased(sc))
                    {
                        cased.RemoveAll(scriptSet);
                    }
                }
            }
            assertEquals("no remaining RTL characters", "[]", rtl.ToPattern(true));
            assertEquals("no remaining cased characters", "[]", cased.ToPattern(true));

            assertTrue("Hani breaks between letters", UScript.BreaksBetweenLetters(UScript.Han));
            assertTrue("Thai breaks between letters", UScript.BreaksBetweenLetters(UScript.Thai));
            assertFalse("Latn does not break between letters", UScript.BreaksBetweenLetters(UScript.Latin));
        }

        [Test]
        public void TestScriptNames()
        {
            for (int i = 0; i < UScript.CodeLimit; i++)
            {
                String name = UScript.GetName(i);
                if (name.Equals(""))
                {
                    Errln("FAILED: getName for code : " + i);
                }
                String shortName = UScript.GetShortName(i);
                if (shortName.Equals(""))
                {
                    Errln("FAILED: getName for code : " + i);
                }
            }
        }
        [Test]
        public void TestAllCodepoints()
        {
            int code;
            //String oldId="";
            //String oldAbbrId="";
            for (int i = 0; i <= 0x10ffff; i++)
            {
                code = UScript.InvalidCode;
                code = UScript.GetScript(i);
                if (code == UScript.InvalidCode)
                {
                    Errln("UScript.getScript for codepoint 0x" + Hex(i) + " failed");
                }
                String id = UScript.GetName(code);
                if (id.IndexOf("INVALID", StringComparison.Ordinal) >= 0)
                {
                    Errln("UScript.getScript for codepoint 0x" + Hex(i) + " failed");
                }
                String abbr = UScript.GetShortName(code);
                if (abbr.IndexOf("INV", StringComparison.Ordinal) >= 0)
                {
                    Errln("UScript.getScript for codepoint 0x" + Hex(i) + " failed");
                }
            }
        }
        [Test]
        public void TestNewCode()
        {
            /*
             * These script codes were originally added to ICU pre-3.6, so that ICU would
             * have all ISO 15924 script codes. ICU was then based on Unicode 4.1.
             * These script codes were added with only short names because we don't
             * want to invent long names ourselves.
             * Unicode 5 and later encode some of these scripts and give them long names.
             * Whenever this happens, the long script names here need to be updated.
             */
            String[] expectedLong = new String[]{
                "Balinese", "Batak", "Blis", "Brahmi", "Cham", "Cirt", "Cyrs",
                "Egyd", "Egyh", "Egyptian_Hieroglyphs",
                "Geok", "Hans", "Hant", "Pahawh_Hmong", "Old_Hungarian", "Inds",
                "Javanese", "Kayah_Li", "Latf", "Latg",
                "Lepcha", "Linear_A", "Mandaic", "Maya", "Meroitic_Hieroglyphs",
                "Nko", "Old_Turkic", "Old_Permic", "Phags_Pa", "Phoenician",
                "Miao", "Roro", "Sara", "Syre", "Syrj", "Syrn", "Teng", "Vai", "Visp", "Cuneiform",
                "Zxxx", "Unknown",
                "Carian", "Jpan", "Tai_Tham", "Lycian", "Lydian", "Ol_Chiki", "Rejang", "Saurashtra", "SignWriting", "Sundanese",
                "Moon", "Meetei_Mayek",
                /* new in ICU 4.0 */
                "Imperial_Aramaic", "Avestan", "Chakma", "Kore",
                "Kaithi", "Manichaean", "Inscriptional_Pahlavi", "Psalter_Pahlavi", "Phlv",
                "Inscriptional_Parthian", "Samaritan", "Tai_Viet",
                "Zmth", "Zsym",
                /* new in ICU 4.4 */
                "Bamum", "Lisu", "Nkgb", "Old_South_Arabian",
                /* new in ICU 4.6 */
                "Bassa_Vah", "Duployan", "Elbasan", "Grantha", "Kpel",
                "Loma", "Mende_Kikakui", "Meroitic_Cursive",
                "Old_North_Arabian", "Nabataean", "Palmyrene", "Khudawadi", "Warang_Citi",
                /* new in ICU 4.8 */
                "Afak", "Jurc", "Mro", "Nushu", "Sharada", "Sora_Sompeng", "Takri", "Tangut", "Wole",
                /* new in ICU 49 */
                "Anatolian_Hieroglyphs", "Khojki", "Tirhuta",
                /* new in ICU 52 */
                "Caucasian_Albanian", "Mahajani",
                /* new in ICU 54 */
                "Ahom", "Hatran", "Modi", "Multani", "Pau_Cin_Hau", "Siddham",
                // new in ICU 58
                "Adlam", "Bhaiksuki", "Marchen", "Newa", "Osage", "Hanb", "Jamo", "Zsye",
                // new in ICU 60
                "Masaram_Gondi", "Soyombo", "Zanabazar_Square"
            };
            String[] expectedShort = new String[]{
                "Bali", "Batk", "Blis", "Brah", "Cham", "Cirt", "Cyrs", "Egyd", "Egyh", "Egyp",
                "Geok", "Hans", "Hant", "Hmng", "Hung", "Inds", "Java", "Kali", "Latf", "Latg",
                "Lepc", "Lina", "Mand", "Maya", "Mero", "Nkoo", "Orkh", "Perm", "Phag", "Phnx",
                "Plrd", "Roro", "Sara", "Syre", "Syrj", "Syrn", "Teng", "Vaii", "Visp", "Xsux",
                "Zxxx", "Zzzz",
                "Cari", "Jpan", "Lana", "Lyci", "Lydi", "Olck", "Rjng", "Saur", "Sgnw", "Sund",
                "Moon", "Mtei",
                /* new in ICU 4.0 */
                "Armi", "Avst", "Cakm", "Kore",
                "Kthi", "Mani", "Phli", "Phlp", "Phlv", "Prti", "Samr", "Tavt",
                "Zmth", "Zsym",
                /* new in ICU 4.4 */
                "Bamu", "Lisu", "Nkgb", "Sarb",
                /* new in ICU 4.6 */
                "Bass", "Dupl", "Elba", "Gran", "Kpel", "Loma", "Mend", "Merc",
                "Narb", "Nbat", "Palm", "Sind", "Wara",
                /* new in ICU 4.8 */
                "Afak", "Jurc", "Mroo", "Nshu", "Shrd", "Sora", "Takr", "Tang", "Wole",
                /* new in ICU 49 */
                "Hluw", "Khoj", "Tirh",
                /* new in ICU 52 */
                "Aghb", "Mahj",
                /* new in ICU 54 */
                "Ahom", "Hatr", "Modi", "Mult", "Pauc", "Sidd",
                // new in ICU 58
                "Adlm", "Bhks", "Marc", "Newa", "Osge", "Hanb", "Jamo", "Zsye",
                // new in ICU 60
                "Gonm", "Soyo", "Zanb"
            };
            if (expectedLong.Length != (UScript.CodeLimit - UScript.Balinese))
            {
                Errln("need to add new script codes in lang.TestUScript.java!");
                return;
            }
            int j = 0;
            int i = 0;
            for (i = UScript.Balinese; i < UScript.CodeLimit; i++, j++)
            {
                String name = UScript.GetName(i);
                if (name == null || !name.Equals(expectedLong[j]))
                {
                    Errln("UScript.getName failed for code" + i + name + "!=" + expectedLong[j]);
                }
                name = UScript.GetShortName(i);
                if (name == null || !name.Equals(expectedShort[j]))
                {
                    Errln("UScript.getShortName failed for code" + i + name + "!=" + expectedShort[j]);
                }
            }
            for (i = 0; i < expectedLong.Length; i++)
            {
                int[] ret = UScript.GetCode(expectedShort[i]);
                if (ret.Length > 1)
                {
                    Errln("UScript.getCode did not return expected number of codes for script" + expectedShort[i] + ". EXPECTED: 1 GOT: " + ret.Length);
                }
                if (ret[0] != (UScript.Balinese + i))
                {
                    Errln("UScript.getCode did not return expected code for script" + expectedShort[i] + ". EXPECTED: " + (UScript.Balinese + i) + " GOT: %i\n" + ret[0]);
                }
            }
        }
    }
}
