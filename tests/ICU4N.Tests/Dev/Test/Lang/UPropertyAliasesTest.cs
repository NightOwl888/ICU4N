using ICU4N.Globalization;
using J2N;
using NUnit.Framework;
using System;

namespace ICU4N.Dev.Test.Lang
{
    public class UPropertyAliasesTest : TestFmwk
    {
        public UPropertyAliasesTest() { }

        /// <summary>
        /// Test the property names and property value names API.
        /// </summary>
        [Test]
        public void TestPropertyNames()
        {
            int v, rev;
            UProperty p;
            NameChoice choice;
            for (p = 0; ; ++p)
            {
                bool sawProp = false;
                for (choice = 0; ; ++choice)
                {
                    string name = null;
                    try
                    {
                        name = UChar.GetPropertyName(p, choice);
                        if (!sawProp) Log("prop " + p + ":");
                        string n = (name != null) ? ("\"" + name + '"') : "null";
                        Log(" " + choice + "=" + n);
                        sawProp = true;
                    }
                    catch (ArgumentException e)
                    {
                        if (choice > 0) break;
                    }
                    if (name != null)
                    {
                        /* test reverse mapping */
                        rev = UChar.GetPropertyEnum(name);
                        if (rev != (int)p)
                        {
                            Errln("Property round-trip failure: " + p + " -> " +
                                  name + " -> " + rev);
                        }
                    }
                }
                if (sawProp)
                {
                    /* looks like a valid property; check the values */
                    string pname = UChar.GetPropertyName(p, NameChoice.Long);
                    int max = 0;
                    if (p == UProperty.Canonical_Combining_Class)
                    {
                        max = 255;
                    }
                    else if (p == UProperty.General_Category_Mask)
                    {
                        /* it's far too slow to iterate all the way up to
                           the real max, U_GC_P_MASK */
                        max = 0x1000; // U_GC_NL_MASK;
                    }
                    else if (p == UProperty.Block)
                    {
                        /* UBlockCodes, unlike other values, start at 1 */
                        max = 1;
                    }
                    Logln("");
                    for (v = -1; ; ++v)
                    {
                        bool sawValue = false;
                        for (choice = 0; ; ++choice)
                        {
                            string vname = null;
                            try
                            {
                                vname = UChar.GetPropertyValueName(p, v, choice);
                                string n = (vname != null) ? ("\"" + vname + '"') : "null";
                                if (!sawValue) Log(" " + pname + ", value " + v + ":");
                                Log(" " + choice + "=" + n);
                                sawValue = true;
                            }
                            catch (ArgumentException e)
                            {
                                if (choice > 0) break;
                            }
                            if (vname != null)
                            {
                                /* test reverse mapping */
                                rev = UChar.GetPropertyValueEnum(p, vname);
                                if (rev != v)
                                {
                                    Errln("Value round-trip failure (" + pname +
                                          "): " + v + " -> " +
                                          vname + " -> " + rev);
                                }
                            }
                        }
                        if (sawValue)
                        {
                            Logln("");
                        }
                        if (!sawValue && v >= max) break;
                    }
                }
                if (!sawProp)
                {
                    if (p >= UPropertyConstants.String_Limit)
                    {
                        break;
                    }
                    else if (p >= UPropertyConstants.Double_Limit)
                    {
                        p = UPropertyConstants.String_Start - 1;
                    }
                    else if (p >= UPropertyConstants.Mask_Limit)
                    {
                        p = UPropertyConstants.Double_Start - 1;
                    }
                    else if (p >= UPropertyConstants.Int_Limit)
                    {
                        p = UPropertyConstants.Mask_Start - 1;
                    }
                    else if (p >= UPropertyConstants.Binary_Limit)
                    {
                        p = UPropertyConstants.Int_Start - 1;
                    }
                }
            }

            int i = UChar.GetIntPropertyMinValue(
                                            UProperty.Canonical_Combining_Class);
            try
            {
                for (; i <= UChar.GetIntPropertyMaxValue(
                                              UProperty.Canonical_Combining_Class);
                     i++)
                {
                    UChar.GetPropertyValueName(
                                              UProperty.Canonical_Combining_Class,
                                              i, NameChoice.Long);
                }
            }
            catch (ArgumentException e)
            {
                Errln("0x" + i.ToHexString()
                      + " should have a null property value name");
            }
        }

        /// <summary>
        /// Test the property names and property value names API.
        /// </summary>
        [Test] // ICU4N Specific
        public void TestPropertyNamesUsingTry()
        {
            int v, rev;
            UProperty p;
            NameChoice choice;
            for (p = 0; ; ++p)
            {
                bool sawProp = false;
                for (choice = 0; ; ++choice)
                {
                    string name = null;
                    if (UChar.TryGetPropertyName(p, choice, out ReadOnlySpan<char> nameSpan))
                    {
                        name = nameSpan.IsEmpty ? null : nameSpan.ToString();
                        if (!sawProp) Log("prop " + p + ":");
                        string n = (name != null) ? ("\"" + name + '"') : "null";
                        Log(" " + choice + "=" + n);
                        sawProp = true;
                    }
                    else
                    {
                        if (choice > 0) break;
                    }
                    if (name != null)
                    {
                        /* test reverse mapping */
                        //rev = UChar.GetPropertyEnum(name);
                        UChar.TryGetPropertyEnum(name, out rev);
                        if (rev != (int)p)
                        {
                            Errln("Property round-trip failure: " + p + " -> " +
                                  name + " -> " + rev);
                        }
                    }
                }
                if (sawProp)
                {
                    /* looks like a valid property; check the values */
                    string pname = null;
                    if (UChar.TryGetPropertyName(p, NameChoice.Long, out ReadOnlySpan<char> pnameSpan))
                    {
                        pname = pnameSpan.ToString();
                    }
                    int max = 0;
                    if (p == UProperty.Canonical_Combining_Class)
                    {
                        max = 255;
                    }
                    else if (p == UProperty.General_Category_Mask)
                    {
                        /* it's far too slow to iterate all the way up to
                           the real max, U_GC_P_MASK */
                        max = 0x1000; // U_GC_NL_MASK;
                    }
                    else if (p == UProperty.Block)
                    {
                        /* UBlockCodes, unlike other values, start at 1 */
                        max = 1;
                    }
                    Logln("");
                    for (v = -1; ; ++v)
                    {
                        bool sawValue = false;
                        for (choice = 0; ; ++choice)
                        {
                            string vname = null;
                            if (UChar.TryGetPropertyValueName(p, v, choice, out ReadOnlySpan<char> vnameSpan))
                            {
                                vname = vnameSpan.ToString();
                                string n = "\"" + vname + '"';
                                if (!sawValue) Log(" " + pname + ", value " + v + ":");
                                Log(" " + choice + "=" + n);
                                sawValue = true;
                            }
                            else
                            {
                                if (choice > 0) break;
                            }
                            if (vname != null)
                            {
                                /* test reverse mapping */
                                UChar.TryGetPropertyValueEnum(p, vname, out rev);
                                if (rev != v)
                                {
                                    Errln("Value round-trip failure (" + pname +
                                          "): " + v + " -> " +
                                          vname.ToString() + " -> " + rev);
                                }
                            }
                        }
                        if (sawValue)
                        {
                            Logln("");
                        }
                        if (!sawValue && v >= max) break;
                    }
                }
                if (!sawProp)
                {
                    if (p >= UPropertyConstants.String_Limit)
                    {
                        break;
                    }
                    else if (p >= UPropertyConstants.Double_Limit)
                    {
                        p = UPropertyConstants.String_Start - 1;
                    }
                    else if (p >= UPropertyConstants.Mask_Limit)
                    {
                        p = UPropertyConstants.Double_Start - 1;
                    }
                    else if (p >= UPropertyConstants.Int_Limit)
                    {
                        p = UPropertyConstants.Mask_Start - 1;
                    }
                    else if (p >= UPropertyConstants.Binary_Limit)
                    {
                        p = UPropertyConstants.Int_Start - 1;
                    }
                }
            }

            int i = UChar.GetIntPropertyMinValue(
                                            UProperty.Canonical_Combining_Class);

            for (; i <= UChar.GetIntPropertyMaxValue(
                                            UProperty.Canonical_Combining_Class);
                    i++)
            {
                if (!UChar.TryGetPropertyValueName(
                                            UProperty.Canonical_Combining_Class,
                                            i, NameChoice.Long, out ReadOnlySpan<char> valueName))
                {
                    Errln("0x" + i.ToHexString()
                        + " should have a null property value name");
                    break;
                }
            }
        }

        [Test]
        public void TestUnknownPropertyNames()
        {
            try
            {
                int p = UChar.GetPropertyEnum("??");
                Errln("UCharacter.getPropertyEnum(??) returned " + p +
                      " rather than throwing an exception");
            }
            catch (ArgumentException e)
            {
                // ok
            }
            try
            {
                int p = UChar.GetPropertyValueEnum(UProperty.Line_Break, "?!");
                Errln("UCharacter.getPropertyValueEnum(UProperty.LINE_BREAK, ?!) returned " + p +
                      " rather than throwing an exception");
            }
            catch (ArgumentException e)
            {
                // ok
            }
        }
    }
}
