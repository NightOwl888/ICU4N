using ICU4N.Lang;
using ICU4N.Support;
using NUnit.Framework;
using System;
using UProperty = ICU4N.Lang.UProperty;

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
                        name = UCharacter.GetPropertyName(p, choice);
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
                        rev = UCharacter.GetPropertyEnum(name);
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
                    string pname = UCharacter.GetPropertyName(p, NameChoice.Long);
                    int max = 0;
                    if (p == UProperty.CANONICAL_COMBINING_CLASS)
                    {
                        max = 255;
                    }
                    else if (p == UProperty.GENERAL_CATEGORY_MASK)
                    {
                        /* it's far too slow to iterate all the way up to
                           the real max, U_GC_P_MASK */
                        max = 0x1000; // U_GC_NL_MASK;
                    }
                    else if (p == UProperty.BLOCK)
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
                                vname = UCharacter.GetPropertyValueName(p, v, choice);
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
                                rev = UCharacter.GetPropertyValueEnum(p, vname);
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
                    if (p >= UProperty.STRING_LIMIT)
                    {
                        break;
                    }
                    else if (p >= UProperty.DOUBLE_LIMIT)
                    {
                        p = UProperty.STRING_START - 1;
                    }
                    else if (p >= UProperty.MASK_LIMIT)
                    {
                        p = UProperty.DOUBLE_START - 1;
                    }
                    else if (p >= UProperty.INT_LIMIT)
                    {
                        p = UProperty.MASK_START - 1;
                    }
                    else if (p >= UProperty.BINARY_LIMIT)
                    {
                        p = UProperty.INT_START - 1;
                    }
                }
            }

            int i = UCharacter.GetIntPropertyMinValue(
                                            UProperty.CANONICAL_COMBINING_CLASS);
            try
            {
                for (; i <= UCharacter.GetIntPropertyMaxValue(
                                              UProperty.CANONICAL_COMBINING_CLASS);
                     i++)
                {
                    UCharacter.GetPropertyValueName(
                                              UProperty.CANONICAL_COMBINING_CLASS,
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
                    if (UCharacter.TryGetPropertyName(p, choice, out name))
                    {
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
                        rev = UCharacter.GetPropertyEnum(name);
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
                    string pname;
                    UCharacter.TryGetPropertyName(p, NameChoice.Long, out pname);
                    int max = 0;
                    if (p == UProperty.CANONICAL_COMBINING_CLASS)
                    {
                        max = 255;
                    }
                    else if (p == UProperty.GENERAL_CATEGORY_MASK)
                    {
                        /* it's far too slow to iterate all the way up to
                           the real max, U_GC_P_MASK */
                        max = 0x1000; // U_GC_NL_MASK;
                    }
                    else if (p == UProperty.BLOCK)
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
                            if (UCharacter.TryGetPropertyValueName(p, v, choice, out vname))
                            {
                                string n = (vname != null) ? ("\"" + vname + '"') : "null";
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
                                UCharacter.TryGetPropertyValueEnum(p, vname, out rev);
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
                    if (p >= UProperty.STRING_LIMIT)
                    {
                        break;
                    }
                    else if (p >= UProperty.DOUBLE_LIMIT)
                    {
                        p = UProperty.STRING_START - 1;
                    }
                    else if (p >= UProperty.MASK_LIMIT)
                    {
                        p = UProperty.DOUBLE_START - 1;
                    }
                    else if (p >= UProperty.INT_LIMIT)
                    {
                        p = UProperty.MASK_START - 1;
                    }
                    else if (p >= UProperty.BINARY_LIMIT)
                    {
                        p = UProperty.INT_START - 1;
                    }
                }
            }

            int i = UCharacter.GetIntPropertyMinValue(
                                            UProperty.CANONICAL_COMBINING_CLASS);

            for (; i <= UCharacter.GetIntPropertyMaxValue(
                                            UProperty.CANONICAL_COMBINING_CLASS);
                    i++)
            {
                string valueName;
                if (!UCharacter.TryGetPropertyValueName(
                                            UProperty.CANONICAL_COMBINING_CLASS,
                                            i, NameChoice.Long, out valueName))
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
                int p = UCharacter.GetPropertyEnum("??");
                Errln("UCharacter.getPropertyEnum(??) returned " + p +
                      " rather than throwing an exception");
            }
            catch (ArgumentException e)
            {
                // ok
            }
            try
            {
                int p = UCharacter.GetPropertyValueEnum(UProperty.LINE_BREAK, "?!");
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
