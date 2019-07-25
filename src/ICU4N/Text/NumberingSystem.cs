using ICU4N.Impl;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="NumberingSystem"/> is the base class for all number
    /// systems. This class provides the interface for setting different numbering
    /// system types, whether it be a simple alternate digit system such as
    /// Thai digits or Devanagari digits, or an algorithmic numbering system such
    /// as Hebrew numbering or Chinese numbering.
    /// </summary>
    /// <author>John Emmons</author>
    /// <stable>ICU 4.2</stable>
    public class NumberingSystem
    {
        private static readonly string[] OTHER_NS_KEYWORDS = { "native", "traditional", "finance" };

        /// <summary>
        /// For convenience, an instance representing the <em>latn</em> numbering system, which
        /// corresponds to digits in the ASCII range '0' through '9'.
        /// </summary>
        /// <draft>ICU 60</draft>
        public static readonly NumberingSystem Latin = LookupInstanceByName("latn");

        /// <summary>
        /// Default constructor.  Returns a numbering system that uses the Western decimal
        /// digits 0 through 9.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public NumberingSystem()
        {
            radix = 10;
            algorithmic = false;
            desc = "0123456789";
            name = "latn";
        }

        /// <summary>
        /// Factory method for creating a numbering system.
        /// </summary>
        /// <param name="radix_in">The radix for this numbering system.  ICU currently
        /// supports only numbering systems whose radix is 10.</param>
        /// <param name="isAlgorithmic_in">Specifies whether the numbering system is algorithmic
        /// (true) or numeric (false).</param>
        /// <param name="desc_in">String used to describe the characteristics of the numbering
        /// system.  For numeric systems, this string contains the digits used by the
        /// numbering system, in order, starting from zero.  For algorithmic numbering
        /// systems, the string contains the name of the RBNF ruleset in the locale's
        /// NumberingSystemRules section that will be used to format numbers using
        /// this numbering system.</param>
        /// <stable>ICU 4.2</stable>
        public static NumberingSystem GetInstance(int radix_in, bool isAlgorithmic_in, string desc_in)
        {
            return GetInstance(null, radix_in, isAlgorithmic_in, desc_in);
        }

        /// <summary>
        /// Factory method for creating a numbering system.
        /// </summary>
        /// <param name="name_in">The string representing the name of the numbering system.</param>
        /// <param name="radix_in">The radix for this numbering system.  ICU currently
        /// supports only numbering systems whose radix is 10.</param>
        /// <param name="isAlgorithmic_in">Specifies whether the numbering system is algorithmic
        /// (true) or numeric (false).</param>
        /// <param name="desc_in">String used to describe the characteristics of the numbering
        /// system.  For numeric systems, this string contains the digits used by the
        /// numbering system, in order, starting from zero.  For algorithmic numbering
        /// systems, the string contains the name of the RBNF ruleset in the locale's
        /// NumberingSystemRules section that will be used to format numbers using
        /// this numbering system.</param>
        /// <stable>ICU 4.6</stable>
        private static NumberingSystem GetInstance(string name_in, int radix_in, bool isAlgorithmic_in, string desc_in)
        {
            if (radix_in < 2)
            {
                throw new ArgumentException("Invalid radix for numbering system");
            }

            if (!isAlgorithmic_in)
            {
                if (desc_in.CodePointCount(0, desc_in.Length) != radix_in || !IsValidDigitString(desc_in))
                {
                    throw new ArgumentException("Invalid digit string for numbering system");
                }
            }
            NumberingSystem ns = new NumberingSystem();
            ns.radix = radix_in;
            ns.algorithmic = isAlgorithmic_in;
            ns.desc = desc_in;
            ns.name = name_in;
            return ns;
        }

        /// <summary>
        /// Returns the default numbering system for the specified locale.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public static NumberingSystem GetInstance(CultureInfo inLocale)
        {
            return GetInstance(ULocale.ForLocale(inLocale));
        }

        /// <summary>
        /// Returns the default numbering system for the specified <see cref="ULocale"/>.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public static NumberingSystem GetInstance(ULocale locale)
        {
            // Check for @numbers
            bool nsResolved = true;
            string numbersKeyword = locale.GetKeywordValue("numbers");
            if (numbersKeyword != null)
            {
                foreach (string keyword in OTHER_NS_KEYWORDS)
                {
                    if (numbersKeyword.Equals(keyword))
                    {
                        nsResolved = false;
                        break;
                    }
                }
            }
            else
            {
                numbersKeyword = "default";
                nsResolved = false;
            }

            if (nsResolved)
            {
                NumberingSystem ns = GetInstanceByName(numbersKeyword);
                if (ns != null)
                {
                    return ns;
                }
                // If the @numbers keyword points to a bogus numbering system name,
                // we return the default for the locale.
                numbersKeyword = "default";
            }

            // Attempt to get the numbering system from the cache
            string baseName = locale.GetBaseName();
            // TODO: Caching by locale+numbersKeyword could yield a large cache.
            // Try to load for each locale the mappings from OTHER_NS_KEYWORDS and default
            // to real numbering system names; can we get those from supplemental data?
            // Then look up those mappings for the locale and resolve the keyword.
            string key = baseName + "@numbers=" + numbersKeyword;
            LocaleLookupData localeLookupData = new LocaleLookupData(locale, numbersKeyword);
            return cachedLocaleData.GetInstance(key, localeLookupData);
        }

        internal class LocaleLookupData
        {
            public readonly ULocale locale;
            public readonly string numbersKeyword;

            internal LocaleLookupData(ULocale locale, string numbersKeyword)
            {
                this.locale = locale;
                this.numbersKeyword = numbersKeyword;
            }
        }

        internal static NumberingSystem LookupInstanceByLocale(LocaleLookupData localeLookupData)
        {
            ULocale locale = localeLookupData.locale;
            ICUResourceBundle rb;
            try
            {
                rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.ICU_BASE_NAME, locale);
                rb = rb.GetWithFallback("NumberElements");
            }
            catch (MissingManifestResourceException)
            {
                return new NumberingSystem();
            }

            string numbersKeyword = localeLookupData.numbersKeyword;
            string resolvedNumberingSystem = null;
            for (; ; )
            {
                try
                {
                    resolvedNumberingSystem = rb.GetStringWithFallback(numbersKeyword);
                    break;
                }
                catch (MissingManifestResourceException)
                { // Fall back behavior as defined in TR35
                    if (numbersKeyword.Equals("native") || numbersKeyword.Equals("finance"))
                    {
                        numbersKeyword = "default";
                    }
                    else if (numbersKeyword.Equals("traditional"))
                    {
                        numbersKeyword = "native";
                    }
                    else
                    {
                        break;
                    }
                }
            }

            NumberingSystem ns = null;
            if (resolvedNumberingSystem != null)
            {
                ns = GetInstanceByName(resolvedNumberingSystem);
            }

            if (ns == null)
            {
                ns = new NumberingSystem();
            }
            return ns;
        }

        /// <summary>
        /// Returns the default numbering system for the default <see cref="ULocale.Category.FORMAT"/>
        /// </summary>
        /// <seealso cref="ULocale.Category.FORMAT"/>
        /// <stable>ICU 4.2</stable>
        public static NumberingSystem GetInstance()
        {
            return GetInstance(ULocale.GetDefault(ULocale.Category.FORMAT));
        }

        /// <summary>
        /// Returns a numbering system from one of the predefined numbering systems
        /// known to ICU.  Numbering system names are based on the numbering systems
        /// defined in CLDR.  To get a list of available numbering systems, use the
        /// <see cref="GetAvailableNames()"/> method.
        /// </summary>
        /// <param name="name">The name of the desired numbering system.  Numbering system
        /// names often correspond with the name of the script they are associated
        /// with.  For example, "thai" for Thai digits, "hebr" for Hebrew numerals.</param>
        /// <stable>ICU 4.2</stable>
        public static NumberingSystem GetInstanceByName(string name)
        {
            // Get the numbering system from the cache.
            return cachedStringData.GetInstance(name, null /* unused */);
        }

        private static NumberingSystem LookupInstanceByName(string name)
        {
            int radix;
            bool isAlgorithmic;
            string description;
            try
            {
                UResourceBundle numberingSystemsInfo = UResourceBundle.GetBundleInstance(ICUData.ICU_BASE_NAME, "numberingSystems");
                UResourceBundle nsCurrent = numberingSystemsInfo.Get("numberingSystems");
                UResourceBundle nsTop = nsCurrent.Get(name);

                description = nsTop.GetString("desc");
                UResourceBundle nsRadixBundle = nsTop.Get("radix");
                UResourceBundle nsAlgBundle = nsTop.Get("algorithmic");
                radix = nsRadixBundle.GetInt32();
                int algorithmic = nsAlgBundle.GetInt32();

                isAlgorithmic = (algorithmic == 1);

            }
            catch (MissingManifestResourceException)
            {
                return null;
            }

            return GetInstance(name, radix, isAlgorithmic, description);
        }

        /// <summary>
        /// Returns a string array containing a list of the names of numbering systems
        /// currently known to ICU.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public static string[] GetAvailableNames()
        {

            UResourceBundle numberingSystemsInfo = UResourceBundle.GetBundleInstance(ICUData.ICU_BASE_NAME, "numberingSystems");
            UResourceBundle nsCurrent = numberingSystemsInfo.Get("numberingSystems");
            UResourceBundle temp;

            string nsName;
            IList<string> output = new List<string>();

            foreach (var rb in nsCurrent)
            {
                temp = rb;
                nsName = temp.Key;
                output.Add(nsName);
            }
            return output.ToArray();
        }

        /// <summary>
        /// Convenience method to determine if a given digit string is valid for use as a
        /// descriptor of a numeric ( non-algorithmic ) numbering system.  In order for
        /// a digit string to be valid, it must contain exactly ten Unicode code points.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public static bool IsValidDigitString(string str)
        {
            int numCodepoints = str.CodePointCount(0, str.Length);
            return (numCodepoints == 10);
        }

        /// <summary>
        /// Returns the radix of the current numbering system.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public virtual int Radix
        {
            get { return radix; }
        }

        /// <summary>
        /// Returns the description string of the current numbering system.
        /// The description string describes the characteristics of the numbering
        /// system.  For numeric systems, this string contains the digits used by the
        /// numbering system, in order, starting from zero.  For algorithmic numbering
        /// systems, the string contains the name of the RBNF ruleset in the locale's
        /// NumberingSystemRules section that will be used to format numbers using
        /// this numbering system.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public virtual string Description
        {
            get { return desc; }
        }

        /// <summary>
        /// Returns the string representing the name of the numbering system.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public virtual string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Returns the numbering system's algorithmic status.  If true,
        /// the numbering system is algorithmic and uses an RBNF formatter to
        /// format numerals.  If false, the numbering system is numeric and
        /// uses a fixed set of digits.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public virtual bool IsAlgorithmic
        {
            get { return algorithmic; }
        }

        private string desc;
        private int radix;
        private bool algorithmic;
        private string name;


        /// <summary>
        /// Cache to hold the NumberingSystems by Locale.
        /// </summary>
        private static CacheBase<string, NumberingSystem, LocaleLookupData> cachedLocaleData =
                new AnonymousSoftCache<string, NumberingSystem, LocaleLookupData>(
                    createInstance: (key, localeLookupData) => { return LookupInstanceByLocale(localeLookupData); });


        /// <summary>
        /// Cache to hold the NumberingSystems by name.
        /// </summary>
        private static CacheBase<string, NumberingSystem, object> cachedStringData =
                   new AnonymousSoftCache<string, NumberingSystem, object>(
                       createInstance: (key, localeLookupData) => { return LookupInstanceByName(key); });
    }
}
