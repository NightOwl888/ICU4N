﻿using ICU4N.Support.Collections;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using UiListItem = ICU4N.Text.LocaleDisplayNames.UiListItem;


namespace ICU4N.Dev.Test.Util
{
    public class ULocaleCollationTest : TestFmwk
    {
        public class ServiceFacade : IServiceFacade
        {
            private readonly Func<ULocale, object> create;

            public ServiceFacade(Func<ULocale, object> create)
            {
                this.create = create;
            }

            public object Create(ULocale req)
            {
                return create(req);
            }
        }

        public class Registrar : IRegistrar
        {
            private readonly Func<ULocale, object, object> register;
            private readonly Func<object, bool> unregister;

            public Registrar(Func<ULocale, object, object> register, Func<object, bool> unregister)
            {
                this.register = register;
                this.unregister = unregister;
            }

            public object Register(ULocale loc, object prototype)
            {
                return register != null ? register(loc, prototype) : null;
            }

            public bool Unregister(object key)
            {
                return unregister != null ? unregister(key) : false;
            }
        }

        [Test]
        public void TestCollator()
        {
            CheckService("ja_JP_YOKOHAMA", new ServiceFacade(create: (req) =>
            {
                return Collator.GetInstance(req);
            }), null, new Registrar(register: (loc, prototype) =>
            {
                return Collator.RegisterInstance((Collator)prototype, loc);
            }, unregister: (key) =>
            {
                return Collator.Unregister(key);
            }));
        }


        /**
         * Interface used by checkService defining a protocol to create an
         * object, given a requested locale.
         */
        internal interface IServiceFacade
        {
            Object Create(ULocale requestedLocale);
        }

        /**
         * Interface used by checkService defining a protocol to get a
         * contained subobject, given its parent object.
         */
        internal interface ISubobject
        {
            Object Get(Object parent);
        }

        /**
         * Interface used by checkService defining a protocol to register
         * and unregister a service object prototype.
         */
        internal interface IRegistrar
        {
            Object Register(ULocale loc, Object prototype);
            bool Unregister(Object key);
        }



        /**
         * Compare two locale IDs.  If they are equal, return 0.  If `string'
         * starts with `prefix' plus an additional element, that is, string ==
         * prefix + '_' + x, then return 1.  Otherwise return a value < 0.
         */
        internal static int Loccmp(String str, String prefix)
        {
            int slen = str.Length,
                    plen = prefix.Length;
            /* 'root' is "less than" everything */
            if (prefix.Equals("root"))
            {
                return str.Equals("root") ? 0 : 1;
            }
            // ON JAVA (only -- not on C -- someone correct me if I'm wrong)
            // consider "" to be an alternate name for "root".
            if (plen == 0)
            {
                return slen == 0 ? 0 : 1;
            }
            if (!str.StartsWith(prefix, StringComparison.Ordinal)) return -1; /* mismatch */
            if (slen == plen) return 0;
            if (str[plen] == '_') return 1;
            return -2; /* false match, e.g. "en_USX" cmp "en_US" */
        }

        /**
         * Check the relationship between requested locales, and report problems.
         * The caller specifies the expected relationships between requested
         * and valid (expReqValid) and between valid and actual (expValidActual).
         * Possible values are:
         * "gt" strictly greater than, e.g., en_US > en
         * "ge" greater or equal,      e.g., en >= en
         * "eq" equal,                 e.g., en == en
         */
        internal void Checklocs(String label,
                String req,
                CultureInfo validLoc,
                CultureInfo actualLoc,
                String expReqValid,
                String expValidActual)
        {
            String valid = validLoc.ToString();
            String actual = actualLoc.ToString();
            int reqValid = Loccmp(req, valid);
            int validActual = Loccmp(valid, actual);
            bool reqOK = (expReqValid.Equals("gt") && reqValid > 0) ||
                    (expReqValid.Equals("ge") && reqValid >= 0) ||
                    (expReqValid.Equals("eq") && reqValid == 0);
            bool valOK = (expValidActual.Equals("gt") && validActual > 0) ||
                    (expValidActual.Equals("ge") && validActual >= 0) ||
                    (expValidActual.Equals("eq") && validActual == 0);
            if (reqOK && valOK)
            {
                Logln("Ok: " + label + "; req=" + req + ", valid=" + valid +
                        ", actual=" + actual);
            }
            else
            {
                Errln("FAIL: " + label + "; req=" + req + ", valid=" + valid +
                        ", actual=" + actual +
                        (reqOK ? "" : "\n  req !" + expReqValid + " valid") +
                        (valOK ? "" : "\n  val !" + expValidActual + " actual"));
            }
        }

        /**
         * Use reflection to call getLocale() on the given object to
         * determine both the valid and the actual locale.  Verify these
         * for correctness.
         */
        internal void CheckObject(String requestedLocale, Object obj,
                String expReqValid, String expValidActual)
        {
            Type[] getLocaleParams = new Type[] { typeof(ULocale.Type) };
            try
            {
                Type cls = obj.GetType();
                MethodInfo getLocale = cls.GetMethod("GetLocale", getLocaleParams);
                ULocale valid = (ULocale)getLocale.Invoke(obj, new Object[] {
                    ULocale.VALID_LOCALE });
                ULocale actual = (ULocale)getLocale.Invoke(obj, new Object[] {
                    ULocale.ACTUAL_LOCALE });
                // ICU4N TODO: If we subclass CultureInfo, we can just
                // check valid vs actual rather than calling ToLocale() which
                // changes the case and format of the requestedLocale
                Checklocs(cls.Name, requestedLocale,
                        valid.ToLocale(), actual.ToLocale(),
                        expReqValid, expValidActual);
            }

            // Make the following exceptions _specific_ -- do not
            // catch(Exception), since that will catch the exception
            // that Errln throws.
            catch (MissingMethodException e1)
            {
                // no longer an error, Currency has no getLocale
                // Errln("FAIL: reflection failed: " + e1);
                //} catch(SecurityException e2) { // ICU4N TODO: convert these exceptions..?
                //    Errln("FAIL: reflection failed: " + e2);
                //} catch(IllegalAccessException e3) {
                //    Errln("FAIL: reflection failed: " + e3);
            }
            catch (ArgumentException e4)
            {
                Errln("FAIL: reflection failed: " + e4);
            }
            catch (TargetInvocationException e5)
            {
                // no longer an error, Currency has no getLocale
                // Errln("FAIL: reflection failed: " + e5);
            }
        }

        /**
         * Verify the correct getLocale() behavior for the given service.
         * @param requestedLocale the locale to request.  This MUST BE
         * FAKE.  In other words, it should be something like
         * en_US_FAKEVARIANT so this method can verify correct fallback
         * behavior.
         * @param svc a factory object that can create the object to be
         * tested.  This isn't necessary here (one could just pass in the
         * object) but is required for the overload of this method that
         * takes a Registrar.
         */
        internal void CheckService(String requestedLocale, ServiceFacade svc)
        {
            CheckService(requestedLocale, svc, null, null);
        }

        /**
         * Verify the correct getLocale() behavior for the given service.
         * @param requestedLocale the locale to request.  This MUST BE
         * FAKE.  In other words, it should be something like
         * en_US_FAKEVARIANT so this method can verify correct fallback
         * behavior.
         * @param svc a factory object that can create the object to be
         * tested.
         * @param sub an object that can be used to retrieve a subobject
         * which should also be tested.  May be null.
         * @param reg an object that supplies the registration and
         * unregistration functionality to be tested.  May be null.
         */
        internal void CheckService(String requestedLocale, IServiceFacade svc,
                ISubobject sub, IRegistrar reg)
        {
            ULocale req = new ULocale(requestedLocale);
            Object obj = svc.Create(req);
            CheckObject(requestedLocale, obj, "gt", "ge");
            if (sub != null)
            {
                Object subobj = sub.Get(obj);
                CheckObject(requestedLocale, subobj, "gt", "ge");
            }
            if (reg != null)
            {
                Logln("Info: Registering service");
                Object key = reg.Register(req, obj);
                Object objReg = svc.Create(req);
                CheckObject(requestedLocale, objReg, "eq", "eq");
                if (sub != null)
                {
                    Object subobj = sub.Get(obj);
                    // Assume subobjects don't come from services, so
                    // their metadata should be structured normally.
                    CheckObject(requestedLocale, subobj, "gt", "ge");
                }
                Logln("Info: Unregistering service");
                if (!reg.Unregister(key))
                {
                    Errln("FAIL: unregister failed");
                }
                Object objUnreg = svc.Create(req);
                CheckObject(requestedLocale, objUnreg, "gt", "ge");
            }
        }

        [Test]
        public void TestNameList()
        {
            string[][][] tests = {
                        /* name in French, name in self, minimized, modified */
                        new string[][] {new string[] {"fr-Cyrl-BE", "fr-Cyrl-CA"},
                            new string[] {"Français (cyrillique, Belgique)", "Français (cyrillique, Belgique)", "fr_Cyrl_BE", "fr_Cyrl_BE"},
                            new string[] {"Français (cyrillique, Canada)", "Français (cyrillique, Canada)", "fr_Cyrl_CA", "fr_Cyrl_CA"},
                        },
                        new string[][] {new string[] {"en", "de", "fr", "zh"},
                            new string[] {"Allemand", "Deutsch", "de", "de"},
                            new string[] {"Anglais", "English", "en", "en"},
                            new string[] {"Chinois", "中文", "zh", "zh"},
                            new string[] {"Français", "Français", "fr", "fr"},
                        },
                        // some non-canonical names
                        new string[][] {new string[] {"iw", "iw-US", "no", "no-Cyrl", "in", "in-YU"},
                            new string[] {"Hébreu (États-Unis)", "עברית (ארצות הברית)", "iw_US", "iw_US"},
                            new string[] {"Hébreu (Israël)", "עברית (ישראל)", "iw", "iw_IL"},
                            new string[] {"Indonésien (Indonésie)", "Indonesia (Indonesia)", "in", "in_ID"},
                            new string[] {"Indonésien (Serbie)", "Indonesia (Serbia)", "in_YU", "in_YU"},
                            new string[] {"Norvégien (cyrillique)", "Norsk (kyrillisk)", "no_Cyrl", "no_Cyrl"},
                            new string[] {"Norvégien (latin)", "Norsk (latinsk)", "no", "no_Latn"},
                        },
                        new string[][] {new string[] {"zh-Hant-TW", "en", "en-gb", "fr", "zh-Hant", "de", "de-CH", "zh-TW"},
                            new string[] {"Allemand (Allemagne)", "Deutsch (Deutschland)", "de", "de_DE"},
                            new string[] {"Allemand (Suisse)", "Deutsch (Schweiz)", "de_CH", "de_CH"},
                            new string[] {"Anglais (États-Unis)", "English (United States)", "en", "en_US"},
                            new string[] {"Anglais (Royaume-Uni)", "English (United Kingdom)", "en_GB", "en_GB"},
                            new string[] {"Chinois (traditionnel)", "中文（繁體）", "zh_Hant", "zh_Hant"},
                            new string[] {"Français", "Français", "fr", "fr"},
                        },
                        new string[][] {new string[] {"zh", "en-gb", "en-CA", "fr-Latn-FR"},
                            new string[] {"Anglais (Canada)", "English (Canada)", "en_CA", "en_CA"},
                            new string[] {"Anglais (Royaume-Uni)", "English (United Kingdom)", "en_GB", "en_GB"},
                            new string[] {"Chinois", "中文", "zh", "zh"},
                            new string[] {"Français", "Français", "fr", "fr"},
                        },
                        new string[][] {new string[] {"en-gb", "fr", "zh-Hant", "zh-SG", "sr", "sr-Latn"},
                            new string[] {"Anglais (Royaume-Uni)", "English (United Kingdom)", "en_GB", "en_GB"},
                            new string[] {"Chinois (simplifié, Singapour)", "中文（简体，新加坡）", "zh_SG", "zh_Hans_SG"},
                            new string[] {"Chinois (traditionnel, Taïwan)", "中文（繁體，台灣）", "zh_Hant", "zh_Hant_TW"},
                            new string[] {"Français", "Français", "fr", "fr"},
                            new string[] {"Serbe (cyrillique)", "Српски (ћирилица)", "sr", "sr_Cyrl"},
                            new string[] {"Serbe (latin)", "Srpski (latinica)", "sr_Latn", "sr_Latn"},
                        },
                        new string[][] {new string[] {"fr-Cyrl", "fr-Arab"},
                            new string[] {"Français (arabe)", "Français (arabe)", "fr_Arab", "fr_Arab"},
                            new string[] {"Français (cyrillique)", "Français (cyrillique)", "fr_Cyrl", "fr_Cyrl"},
                        },
                        new string[][] {new string[] {"fr-Cyrl-BE", "fr-Arab-CA"},
                            new string[] {"Français (arabe, Canada)", "Français (arabe, Canada)", "fr_Arab_CA", "fr_Arab_CA"},
                            new string[] {"Français (cyrillique, Belgique)", "Français (cyrillique, Belgique)", "fr_Cyrl_BE", "fr_Cyrl_BE"},
                        }
                };
            ULocale french = ULocale.FRENCH;
            LocaleDisplayNames names = LocaleDisplayNames.GetInstance(french,
                    DisplayContext.CAPITALIZATION_FOR_UI_LIST_OR_MENU);
            foreach (DisplayContextType type in Enum.GetValues(typeof(DisplayContextType)))
            {
                Logln("Contexts: " + names.GetContext(type).ToString());
            }
            Collator collator = Collator.GetInstance(french);

            foreach (String[][] test in tests)
            {
                // ICU4N TODO: LinkedHashSet needed ?
                ISet<ULocale> list = new HashSet<ULocale>(); // LinkedHashSet<ULocale>();
                List<UiListItem> expected = new List<UiListItem>();
                foreach (String item in test[0])
                {
                    list.Add(new ULocale(item));
                }
                for (int i = 1; i < test.Length; ++i)
                {
                    String[] rawRow = test[i];
                    expected.Add(new UiListItem(new ULocale(rawRow[2]), new ULocale(rawRow[3]), rawRow[0], rawRow[1]));
                }
                IList<UiListItem> newList = names.GetUiList(list, false, collator);
                if (!expected.SequenceEqual(newList))
                {
                    if (expected.Count != newList.Count)
                    {
                        Errln(CollectionUtil.ToString(list) + ": wrong size" + expected + ", " + newList);
                    }
                    else
                    {
                        Errln(CollectionUtil.ToString(list));
                        for (int i = 0; i < expected.Count; ++i)
                        {
                            assertEquals(i + "", expected[i], newList[i]);
                        }
                    }
                }
                else
                {
                    assertEquals(CollectionUtil.ToString(list), expected, newList);
                }
            }
        }

        [Test]
        public void TestIllformedLocale()
        {
            ULocale french = ULocale.FRENCH;
            Collator collator = Collator.GetInstance(french);
            LocaleDisplayNames names = LocaleDisplayNames.GetInstance(french,
                    DisplayContext.CAPITALIZATION_FOR_UI_LIST_OR_MENU);
            foreach (String malformed in new string[] { "en-a", "$", "ü--a", "en--US" })
            {
                try
                {
                    ISet<ULocale> supported = ImmutableHashSet.Create(new ULocale(malformed)); //Collections.singleton(new ULocale(malformed));
                    names.GetUiList(supported, false, collator);
                    assertNull("Failed to detect bogus locale «" + malformed + "»", supported);
                }
                catch (IllformedLocaleException e)
                {
                    Logln("Successfully detected ill-formed locale «" + malformed + "»:" + e.ToString());
                }
            }
        }
    }
}