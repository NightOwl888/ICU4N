using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Text;
using J2N.Collections.Generic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCG = J2N.Collections.Generic;

namespace ICU4N.Util
{
    /// <summary>
    /// A unit such as length, mass, volume, currency, etc.  A unit is
    /// coupled with a numeric amount to produce a Measure. MeasureUnit objects are immutable.
    /// All subclasses must guarantee that. (However, subclassing is discouraged.)
    /// </summary>
    internal class MeasureUnit // ICU4N TODO: API - this was public in ICU4J
    {
        private static readonly object cacheLock = new object(); // ICU4N specific

        //private static final long serialVersionUID = -1839973855554750484L;

        // Cache of MeasureUnits.
        // All access to the cache or cacheIsPopulated flag must be synchronized on class MeasureUnit,
        // i.e. from synchronized static methods. Beware of non-static methods.
        private static readonly IDictionary<string, IDictionary<string, MeasureUnit>> cache

            = new Dictionary<string, IDictionary<string, MeasureUnit>>();
        private static bool cacheIsPopulated = false;

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected internal readonly string type;

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected readonly string subType;

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected MeasureUnit(string type, string subType)
        {
            this.type = type;
            this.subType = subType;
        }

        /// <summary>
        /// Get the type, such as "length".
        /// </summary>
        /// <stable>ICU 53</stable>
#pragma warning disable CS0618 // Type or member is obsolete
        public virtual string Type => type;

        /// <summary>
        /// Get the subType, such as "foot".
        /// </summary>
        /// <stable>ICU 53</stable>
        public virtual string Subtype => subType;


        /// <inheritdoc/>
        /// <stable>ICU 53</stable>
        public override int GetHashCode()
        {
            return 31 * type.GetHashCode() + subType.GetHashCode();
        }

        /// <inheritdoc/>
        /// <stable>ICU 53</stable>
        public override bool Equals(object rhs)
        {
            if (rhs == this)
            {
                return true;
            }
            if (!(rhs is MeasureUnit c))
            {
                return false;
            }
            return type.Equals(c.Type) && subType.Equals(c.Subtype);
        }

        /// <inheritdoc/>
        /// <stable>ICU 53</stable>
        public override string ToString()
        {
            return type + "-" + subType;
        }


        /// <summary>
        /// Get all of the available units' types. Returned set is unmodifiable.
        /// </summary>
        /// <stable>ICU 53</stable>
        public static ICollection<string> GetAvailableTypes()
        {
            lock (cacheLock)
            {
                PopulateCache();
                return cache.Keys.AsReadOnly();
            }
        }

        /// <summary>
        /// For the given type, return the available units.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The available units for type. Returned set is unmodifiable.</returns>
        /// <stable>ICU 53</stable>
        public static ICollection<MeasureUnit> GetAvailable(string type)
        {
            lock (cacheLock)
            {
                PopulateCache();
                // Train users not to modify returned set from the start giving us more
                // flexibility for implementation.
                return cache.TryGetValue(type, out IDictionary<string, MeasureUnit> units) || units == null ? Collection.EmptySet<MeasureUnit>()
                        : new JCG.HashSet<MeasureUnit>(units.Values).AsReadOnly();
            }
        }

        /// <summary>
        /// Get all of the available units. Returned set is unmodifiable.
        /// </summary>
        /// <stable>ICU 53</stable>
        public static ICollection<MeasureUnit> GetAvailable()
        {
            lock (cacheLock)
            {
                ISet<MeasureUnit> result = new JCG.HashSet<MeasureUnit>();
                foreach (string type in new HashSet<string>(MeasureUnit.GetAvailableTypes()))
                {
                    foreach (MeasureUnit unit in MeasureUnit.GetAvailable(type))
                    {
                        result.Add(unit);
                    }
                }
                // Train users not to modify returned set from the start giving us more
                // flexibility for implementation.
                return result.AsReadOnly();
            }
        }

        /// <summary>
        /// Create a <see cref="MeasureUnit"/> instance (creates a singleton instance).
        /// <para/>
        /// Normally this method should not be used, since there will be no formatting data
        /// available for it, and it may not be returned by <see cref="GetAvailable()"/>.
        /// However, for special purposes (such as CLDR tooling), it is available.
        /// </summary>
        /// <param name="type">The type, such as "length".</param>
        /// <param name="subType">The subType, such as "foot".</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="subType"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is "currency" and <paramref name="type"/> or
        /// <paramref name="subType"/> contain non-ASCII digits or hyphens.</exception>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public static MeasureUnit InternalGetInstance(string type, string subType)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (subType is null)
                throw new ArgumentNullException(nameof(subType));
            if (!"currency".Equals(type, StringComparison.Ordinal))
            {
                if (!ASCII.IsSupersetOf(type) || !ASCII_HYPHEN_DIGITS.IsSupersetOf(subType))
                {
                    throw new ArgumentException("The type or subType are invalid.");
                }
            }
            IFactory factory;
            if ("currency".Equals(type, StringComparison.Ordinal))
            {
                factory = CURRENCY_FACTORY;
            }
            else if ("duration".Equals(type, StringComparison.Ordinal))
            {
                factory = TIMEUNIT_FACTORY;
            }
            else if ("none".Equals(type, StringComparison.Ordinal))
            {
                factory = NOUNIT_FACTORY;
            }
            else
            {
                factory = UNIT_FACTORY;
            }
            return AddUnit(type, subType, factory);
        }

        /// <summary>
        /// For ICU use only.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public static MeasureUnit ResolveUnitPerUnit(MeasureUnit unit, MeasureUnit perUnit)
        {
            return unitPerUnitToSingleUnit.TryGetValue(Pair.Of(unit, perUnit), out MeasureUnit value) ? value : null;
        }

        internal static readonly UnicodeSet ASCII = new UnicodeSet('a', 'z').Freeze();
        internal static readonly UnicodeSet ASCII_HYPHEN_DIGITS = new UnicodeSet('-', '-', '0', '9', 'a', 'z').Freeze();

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected internal interface IFactory
        {
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            MeasureUnit Create(string type, string subType);
        }

        private sealed class DefaultUnitFactory : IFactory
        {
            public MeasureUnit Create(string type, string subType)
            {
                return new MeasureUnit(type, subType);
            }
        }

        private sealed class DefaultCurrencyFactory : IFactory
        {
            public MeasureUnit Create(string type, string subType)
            {
                return new Currency(subType);
            }
        }

        private sealed class DefaultTimeUnitFactory : IFactory
        {
            public MeasureUnit Create(string type, string subType)
            {
                return new TimeUnit(type, subType);
            }
        }

        private sealed class DefaultNoUnitFactory : IFactory
        {
            public MeasureUnit Create(string type, string subType)
            {
                return new NoUnit(subType);
            }
        }

        internal static readonly IFactory UNIT_FACTORY = new DefaultUnitFactory();

        internal static readonly IFactory CURRENCY_FACTORY = new DefaultCurrencyFactory();

        internal static readonly IFactory TIMEUNIT_FACTORY = new DefaultTimeUnitFactory();

        internal static readonly IFactory NOUNIT_FACTORY = new DefaultNoUnitFactory();

        /// <summary>
        /// Sink for enumerating the available measure units.
        /// </summary>
        private sealed class MeasureUnitSink : ResourceSink
        {
            public override void Put(ResourceKey key, ResourceValue value, bool noFallback)
            {
                IResourceTable unitTypesTable = value.GetTable();
                for (int i2 = 0; unitTypesTable.GetKeyAndValue(i2, key, value); ++i2)
                {
                    // Skip "compound" and "coordinate" since they are treated differently from the other units
                    if (key.ContentEquals("compound") || key.ContentEquals("coordinate"))
                    {
                        continue;
                    }

                    string unitType = key.ToString();
                    IResourceTable unitNamesTable = value.GetTable();
                    for (int i3 = 0; unitNamesTable.GetKeyAndValue(i3, key, value); ++i3)
                    {
                        string unitName = key.ToString();
                        InternalGetInstance(unitType, unitName);
                    }
                }
            }
        }

        /// <summary>
        /// Sink for enumerating the currency numeric codes.
        /// </summary>
        private sealed class CurrencyNumericCodeSink : ResourceSink
        {
            public override void Put(ResourceKey key, ResourceValue value, bool noFallback)
            {
                IResourceTable codesTable = value.GetTable();
                for (int i1 = 0; codesTable.GetKeyAndValue(i1, key, value); ++i1)
                {
                    InternalGetInstance("currency", key.ToString());
                }
            }
        }

        /// <summary>
        /// Populate the <see cref="MeasureUnit"/> cache with all types from the data.
        /// Population is done lazily, in response to <see cref="GetAvailable()"/>
        /// or other API that expects to see all of the <see cref="MeasureUnit"/>s.
        /// <para/>
        /// At static initialization time the <see cref="MeasureUnit"/>s cache is populated
        /// with public static instances <see cref="GForce"/>, <see cref="MeterPerSecondSquared"/>, etc.) only.
        /// Adding of others is deferred until later to avoid circular static init
        /// dependencies with classes <see cref="Currency"/> and <see cref="TimeUnit"/>.
        /// <para/>
        /// Synchronization: this function must be called from static synchronized methods only.
        /// </summary>
        /// <internal/>
        private static void PopulateCache()
        {
            if (cacheIsPopulated)
            {
                return;
            }
            cacheIsPopulated = true;

            /*  Schema:
             *
             *  units{
             *    duration{
             *      day{
             *        one{"{0} ден"}
             *        other{"{0} дена"}
             *      }
             */

            // Load the unit types.  Use English, since we know that that is a superset.
            ICUResourceBundle rb1 = (ICUResourceBundle)UResourceBundle.GetBundleInstance(
                    ICUData.IcuUnitBaseName,
                    "en");
            rb1.GetAllItemsWithFallback("units", new MeasureUnitSink());

            // Load the currencies
            ICUResourceBundle rb2 = (ICUResourceBundle)UResourceBundle.GetBundleInstance(
                    ICUData.IcuBaseName,
                    "currencyNumericCodes",
                    ICUResourceBundle.IcuDataAssembly);
            rb2.GetAllItemsWithFallback("codeMap", new CurrencyNumericCodeSink());
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected static MeasureUnit AddUnit(string type, string unitName, IFactory factory)
        {
            lock (cacheLock)
            {
                if (!cache.TryGetValue(type, out IDictionary<string, MeasureUnit> tmp) || tmp == null)
                {
                    cache[type] = tmp = new Dictionary<string, MeasureUnit>();
                }
                else
                {
                    // "intern" the type by setting to first item's type.
                    type = tmp.First().Value.type;
                }
                if (!tmp.TryGetValue(unitName, out MeasureUnit unit) || unit == null)
                {
                    tmp[unitName] = unit = factory.Create(type, unitName);
                }
                return unit;
            }
        }


        /*
         * Useful constants. Not necessarily complete: see <see cref="GetAvailable()"/>.
         */

        // All code between the "Start generated MeasureUnit constants" comment and
        // the "End generated MeasureUnit constants" comment is auto generated code
        // and must not be edited manually. For instructions on how to correctly
        // update this code, refer to:
        // http://site.icu-project.org/design/formatting/measureformat/updating-measure-unit
        //
        // Start generated MeasureUnit constants


        /// <summary>
        /// Constant for unit of acceleration: g-force.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit GForce = MeasureUnit.InternalGetInstance("acceleration", "g-force");

        /// <summary>
        /// Constant for unit of acceleration: meter-per-second-squared.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit MeterPerSecondSquared = MeasureUnit.InternalGetInstance("acceleration", "meter-per-second-squared");

        /// <summary>
        /// Constant for unit of angle: arc-minute.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit ArcMinute = MeasureUnit.InternalGetInstance("angle", "arc-minute");

        /// <summary>
        /// Constant for unit of angle: arc-second.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit ArcSecond = MeasureUnit.InternalGetInstance("angle", "arc-second");

        /// <summary>
        /// Constant for unit of angle: degree.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Degree = MeasureUnit.InternalGetInstance("angle", "degree");

        /// <summary>
        /// Constant for unit of angle: radian.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Radian = MeasureUnit.InternalGetInstance("angle", "radian");

        /// <summary>
        /// Constant for unit of angle: revolution.</summary>
        /// <stable>ICU 56</stable>
        public static readonly MeasureUnit RevolutionAngle = MeasureUnit.InternalGetInstance("angle", "revolution");

        /// <summary>
        /// Constant for unit of area: acre.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Acre = MeasureUnit.InternalGetInstance("area", "acre");

        /// <summary>
        /// Constant for unit of area: hectare.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Hectare = MeasureUnit.InternalGetInstance("area", "hectare");

        /// <summary>
        /// Constant for unit of area: square-centimeter.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit SquareCentimeter = MeasureUnit.InternalGetInstance("area", "square-centimeter");

        /// <summary>
        /// Constant for unit of area: square-foot.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit SquareFoot = MeasureUnit.InternalGetInstance("area", "square-foot");

        /// <summary>
        /// Constant for unit of area: square-inch.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit SquareInch = MeasureUnit.InternalGetInstance("area", "square-inch");

        /// <summary>
        /// Constant for unit of area: square-kilometer.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit SquareKilometer = MeasureUnit.InternalGetInstance("area", "square-kilometer");

        /// <summary>
        /// Constant for unit of area: square-meter.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit SquareMeter = MeasureUnit.InternalGetInstance("area", "square-meter");

        /// <summary>
        /// Constant for unit of area: square-mile.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit SquareMile = MeasureUnit.InternalGetInstance("area", "square-mile");

        /// <summary>
        /// Constant for unit of area: square-yard.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit SquareYard = MeasureUnit.InternalGetInstance("area", "square-yard");

        /// <summary>
        /// Constant for unit of concentr: karat.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Karat = MeasureUnit.InternalGetInstance("concentr", "karat");

        /// <summary>
        /// Constant for unit of concentr: milligram-per-deciliter.</summary>
        /// <stable>ICU 57</stable>
        public static readonly MeasureUnit MilligramPerDeciliter = MeasureUnit.InternalGetInstance("concentr", "milligram-per-deciliter");

        /// <summary>
        /// Constant for unit of concentr: millimole-per-liter.</summary>
        /// <stable>ICU 57</stable>
        public static readonly MeasureUnit MillimolePerLiter = MeasureUnit.InternalGetInstance("concentr", "millimole-per-liter");

        /// <summary>
        /// Constant for unit of concentr: part-per-million.</summary>
        /// <stable>ICU 57</stable>
        public static readonly MeasureUnit PartPerMillion = MeasureUnit.InternalGetInstance("concentr", "part-per-million");

        /// <summary>
        /// Constant for unit of consumption: liter-per-100kilometers</summary>
        /// <stable>ICU 56</stable>
        public static readonly MeasureUnit LiterPer100kilometers = MeasureUnit.InternalGetInstance("consumption", "liter-per-100kilometers");

        /// <summary>
        /// Constant for unit of consumption: liter-per-kilometer.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit LiterPerKilometer = MeasureUnit.InternalGetInstance("consumption", "liter-per-kilometer");

        /// <summary>
        /// Constant for unit of consumption: mile-per-gallon.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit MilePerGallon = MeasureUnit.InternalGetInstance("consumption", "mile-per-gallon");

        /// <summary>
        /// Constant for unit of consumption: mile-per-gallon-imperial.</summary>
        /// <stable>ICU 57</stable>
        public static readonly MeasureUnit MilePerGallonImperial = MeasureUnit.InternalGetInstance("consumption", "mile-per-gallon-imperial");

        // at-draft ICU 58, withdrawn
        // public static final MeasureUnit East = MeasureUnit.InternalGetInstance("coordinate", "east");
        // public static final MeasureUnit North = MeasureUnit.InternalGetInstance("coordinate", "north");
        // public static final MeasureUnit South = MeasureUnit.InternalGetInstance("coordinate", "south");
        // public static final MeasureUnit West = MeasureUnit.InternalGetInstance("coordinate", "west");

        /// <summary>
        /// Constant for unit of digital: bit.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Bit = MeasureUnit.InternalGetInstance("digital", "bit");

        /// <summary>
        /// Constant for unit of digital: byte.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Byte = MeasureUnit.InternalGetInstance("digital", "byte");

        /// <summary>
        /// Constant for unit of digital: gigabit.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Gigabit = MeasureUnit.InternalGetInstance("digital", "gigabit");

        /// <summary>
        /// Constant for unit of digital: gigabyte.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Gigabyte = MeasureUnit.InternalGetInstance("digital", "gigabyte");

        /// <summary>
        /// Constant for unit of digital: kilobit.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Kilobit = MeasureUnit.InternalGetInstance("digital", "kilobit");

        /// <summary>
        /// Constant for unit of digital: kilobyte.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Kilobyte = MeasureUnit.InternalGetInstance("digital", "kilobyte");

        /// <summary>
        /// Constant for unit of digital: megabit.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Megabit = MeasureUnit.InternalGetInstance("digital", "megabit");

        /// <summary>
        /// Constant for unit of digital: megabyte.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Megabyte = MeasureUnit.InternalGetInstance("digital", "megabyte");

        /// <summary>
        /// Constant for unit of digital: terabit.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Terabit = MeasureUnit.InternalGetInstance("digital", "terabit");

        /// <summary>
        /// Constant for unit of digital: terabyte.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Terabyte = MeasureUnit.InternalGetInstance("digital", "terabyte");

        /// <summary>
        /// Constant for unit of duration: century.</summary>
        /// <stable>ICU 56</stable>
        public static readonly MeasureUnit Century = MeasureUnit.InternalGetInstance("duration", "century");

        /// <summary>
        /// Constant for unit of duration: day.</summary>
        /// <stable>ICU 4.0</stable>
        public static readonly TimeUnit Day = (TimeUnit)MeasureUnit.InternalGetInstance("duration", "day");

        /// <summary>
        /// Constant for unit of duration: hour.</summary>
        /// <stable>ICU 4.0</stable>
        public static readonly TimeUnit Hour = (TimeUnit)MeasureUnit.InternalGetInstance("duration", "hour");

        /// <summary>
        /// Constant for unit of duration: microsecond.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Microsecond = MeasureUnit.InternalGetInstance("duration", "microsecond");

        /// <summary>
        /// Constant for unit of duration: millisecond.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Millisecond = MeasureUnit.InternalGetInstance("duration", "millisecond");

        /// <summary>
        /// Constant for unit of duration: minute.</summary>
        /// <stable>ICU 4.0</stable>
        public static readonly TimeUnit Minute = (TimeUnit)MeasureUnit.InternalGetInstance("duration", "minute");

        /// <summary>
        /// Constant for unit of duration: month.</summary>
        /// <stable>ICU 4.0</stable>
        public static readonly TimeUnit Month = (TimeUnit)MeasureUnit.InternalGetInstance("duration", "month");

        /// <summary>
        /// Constant for unit of duration: nanosecond.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Nanosecond = MeasureUnit.InternalGetInstance("duration", "nanosecond");

        /// <summary>
        /// Constant for unit of duration: second.</summary>
        /// <stable>ICU 4.0</stable>
        public static readonly TimeUnit Second = (TimeUnit)MeasureUnit.InternalGetInstance("duration", "second");

        /// <summary>
        /// Constant for unit of duration: week.</summary>
        /// <stable>ICU 4.0</stable>
        public static readonly TimeUnit Week = (TimeUnit)MeasureUnit.InternalGetInstance("duration", "week");

        /// <summary>
        /// Constant for unit of duration: year.</summary>
        /// <stable>ICU 4.0</stable>
        public static readonly TimeUnit Year = (TimeUnit)MeasureUnit.InternalGetInstance("duration", "year");

        /// <summary>
        /// Constant for unit of electric: ampere.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Ampere = MeasureUnit.InternalGetInstance("electric", "ampere");

        /// <summary>
        /// Constant for unit of electric: milliampere.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Milliampere = MeasureUnit.InternalGetInstance("electric", "milliampere");

        /// <summary>
        /// Constant for unit of electric: ohm.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Ohm = MeasureUnit.InternalGetInstance("electric", "ohm");

        /// <summary>
        /// Constant for unit of electric: volt.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Volt = MeasureUnit.InternalGetInstance("electric", "volt");

        /// <summary>
        /// Constant for unit of energy: calorie.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Calorie = MeasureUnit.InternalGetInstance("energy", "calorie");

        /// <summary>
        /// Constant for unit of energy: foodcalorie.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Foodcalorie = MeasureUnit.InternalGetInstance("energy", "foodcalorie");

        /// <summary>
        /// Constant for unit of energy: joule.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Joule = MeasureUnit.InternalGetInstance("energy", "joule");

        /// <summary>
        /// Constant for unit of energy: kilocalorie.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Kilocalorie = MeasureUnit.InternalGetInstance("energy", "kilocalorie");

        /// <summary>
        /// Constant for unit of energy: kilojoule.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Kilojoule = MeasureUnit.InternalGetInstance("energy", "kilojoule");

        /// <summary>
        /// Constant for unit of energy: kilowatt-hour.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit KilowattHour = MeasureUnit.InternalGetInstance("energy", "kilowatt-hour");

        /// <summary>
        /// Constant for unit of frequency: gigahertz.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Gigahertz = MeasureUnit.InternalGetInstance("frequency", "gigahertz");

        /// <summary>
        /// Constant for unit of frequency: hertz.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Hertz = MeasureUnit.InternalGetInstance("frequency", "hertz");

        /// <summary>
        /// Constant for unit of frequency: kilohertz.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Kilohertz = MeasureUnit.InternalGetInstance("frequency", "kilohertz");

        /// <summary>
        /// Constant for unit of frequency: megahertz.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Megahertz = MeasureUnit.InternalGetInstance("frequency", "megahertz");

        /// <summary>
        /// Constant for unit of length: astronomical-unit.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit AstronomicalUnit = MeasureUnit.InternalGetInstance("length", "astronomical-unit");

        /// <summary>
        /// Constant for unit of length: centimeter.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Centimeter = MeasureUnit.InternalGetInstance("length", "centimeter");

        /// <summary>
        /// Constant for unit of length: decimeter.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Decimeter = MeasureUnit.InternalGetInstance("length", "decimeter");

        /// <summary>
        /// Constant for unit of length: fathom.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Fathom = MeasureUnit.InternalGetInstance("length", "fathom");

        /// <summary>
        /// Constant for unit of length: foot.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Foot = MeasureUnit.InternalGetInstance("length", "foot");

        /// <summary>
        /// Constant for unit of length: furlong.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Furlong = MeasureUnit.InternalGetInstance("length", "furlong");

        /// <summary>
        /// Constant for unit of length: inch.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Inch = MeasureUnit.InternalGetInstance("length", "inch");

        /// <summary>
        /// Constant for unit of length: kilometer.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Kilometer = MeasureUnit.InternalGetInstance("length", "kilometer");

        /// <summary>
        /// Constant for unit of length: light-year.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit LightYear = MeasureUnit.InternalGetInstance("length", "light-year");

        /// <summary>
        /// Constant for unit of length: meter.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Meter = MeasureUnit.InternalGetInstance("length", "meter");

        /// <summary>
        /// Constant for unit of length: micrometer.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Micrometer = MeasureUnit.InternalGetInstance("length", "micrometer");

        /// <summary>
        /// Constant for unit of length: mile.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Mile = MeasureUnit.InternalGetInstance("length", "mile");

        /// <summary>
        /// Constant for unit of length: mile-scandinavian.</summary>
        /// <stable>ICU 56</stable>
        public static readonly MeasureUnit MileScandinavian = MeasureUnit.InternalGetInstance("length", "mile-scandinavian");

        /// <summary>
        /// Constant for unit of length: millimeter.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Millimeter = MeasureUnit.InternalGetInstance("length", "millimeter");

        /// <summary>
        /// Constant for unit of length: nanometer.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Nanometer = MeasureUnit.InternalGetInstance("length", "nanometer");

        /// <summary>
        /// Constant for unit of length: nautical-mile.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit NauticalMile = MeasureUnit.InternalGetInstance("length", "nautical-mile");

        /// <summary>
        /// Constant for unit of length: parsec.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Parsec = MeasureUnit.InternalGetInstance("length", "parsec");

        /// <summary>
        /// Constant for unit of length: picometer.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Picometer = MeasureUnit.InternalGetInstance("length", "picometer");

        /// <summary>
        /// Constant for unit of length: point.</summary>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static readonly MeasureUnit Point = MeasureUnit.InternalGetInstance("length", "point");

        /// <summary>
        /// Constant for unit of length: yard.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Yard = MeasureUnit.InternalGetInstance("length", "yard");

        /// <summary>
        /// Constant for unit of light: lux.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Lux = MeasureUnit.InternalGetInstance("light", "lux");

        /// <summary>
        /// Constant for unit of mass: carat.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Carat = MeasureUnit.InternalGetInstance("mass", "carat");

        /// <summary>
        /// Constant for unit of mass: gram.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Gram = MeasureUnit.InternalGetInstance("mass", "gram");

        /// <summary>
        /// Constant for unit of mass: kilogram.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Kilogram = MeasureUnit.InternalGetInstance("mass", "kilogram");

        /// <summary>
        /// Constant for unit of mass: metric-ton.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit MetricTon = MeasureUnit.InternalGetInstance("mass", "metric-ton");

        /// <summary>
        /// Constant for unit of mass: microgram.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Microgram = MeasureUnit.InternalGetInstance("mass", "microgram");

        /// <summary>
        /// Constant for unit of mass: milligram.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Milligram = MeasureUnit.InternalGetInstance("mass", "milligram");

        /// <summary>
        /// Constant for unit of mass: ounce.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Ounce = MeasureUnit.InternalGetInstance("mass", "ounce");

        /// <summary>
        /// Constant for unit of mass: ounce-troy.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit OunceTroy = MeasureUnit.InternalGetInstance("mass", "ounce-troy");

        /// <summary>
        /// Constant for unit of mass: pound.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Pound = MeasureUnit.InternalGetInstance("mass", "pound");

        /// <summary>
        /// Constant for unit of mass: stone.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Stone = MeasureUnit.InternalGetInstance("mass", "stone");

        /// <summary>
        /// Constant for unit of mass: ton.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Ton = MeasureUnit.InternalGetInstance("mass", "ton");

        /// <summary>
        /// Constant for unit of power: gigawatt.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Gigawatt = MeasureUnit.InternalGetInstance("power", "gigawatt");

        /// <summary>
        /// Constant for unit of power: horsepower.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Horsepower = MeasureUnit.InternalGetInstance("power", "horsepower");

        /// <summary>
        /// Constant for unit of power: kilowatt.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Kilowatt = MeasureUnit.InternalGetInstance("power", "kilowatt");

        /// <summary>
        /// Constant for unit of power: megawatt.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Megawatt = MeasureUnit.InternalGetInstance("power", "megawatt");

        /// <summary>
        /// Constant for unit of power: milliwatt.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Milliwatt = MeasureUnit.InternalGetInstance("power", "milliwatt");

        /// <summary>
        /// Constant for unit of power: watt.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Watt = MeasureUnit.InternalGetInstance("power", "watt");

        /// <summary>
        /// Constant for unit of pressure: hectopascal.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Hectopascal = MeasureUnit.InternalGetInstance("pressure", "hectopascal");

        /// <summary>
        /// Constant for unit of pressure: inch-hg.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit InchHg = MeasureUnit.InternalGetInstance("pressure", "inch-hg");

        /// <summary>
        /// Constant for unit of pressure: millibar.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Millibar = MeasureUnit.InternalGetInstance("pressure", "millibar");

        /// <summary>
        /// Constant for unit of pressure: millimeter-of-mercury.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit MillimeterOfMercury = MeasureUnit.InternalGetInstance("pressure", "millimeter-of-mercury");

        /// <summary>
        /// Constant for unit of pressure: pound-per-square-inch.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit PoundPerSquareInch = MeasureUnit.InternalGetInstance("pressure", "pound-per-square-inch");

        /// <summary>
        /// Constant for unit of speed: kilometer-per-hour.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit KilometerPerHour = MeasureUnit.InternalGetInstance("speed", "kilometer-per-hour");

        /// <summary>
        /// Constant for unit of speed: knot.</summary>
        /// <stable>ICU 56</stable>
        public static readonly MeasureUnit Knot = MeasureUnit.InternalGetInstance("speed", "knot");

        /// <summary>
        /// Constant for unit of speed: meter-per-second.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit MeterPerSecond = MeasureUnit.InternalGetInstance("speed", "meter-per-second");

        /// <summary>
        /// Constant for unit of speed: mile-per-hour.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit MilePerHour = MeasureUnit.InternalGetInstance("speed", "mile-per-hour");

        /// <summary>
        /// Constant for unit of temperature: celsius.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Celsius = MeasureUnit.InternalGetInstance("temperature", "celsius");

        /// <summary>
        /// Constant for unit of temperature: fahrenheit.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Fahrenheit = MeasureUnit.InternalGetInstance("temperature", "fahrenheit");

        /// <summary>
        /// Constant for unit of temperature: generic.</summary>
        /// <stable>ICU 56</stable>
        public static readonly MeasureUnit GenericTemperature = MeasureUnit.InternalGetInstance("temperature", "generic");

        /// <summary>
        /// Constant for unit of temperature: kelvin.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Kelvin = MeasureUnit.InternalGetInstance("temperature", "kelvin");

        /// <summary>
        /// Constant for unit of volume: acre-foot.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit AcreFoot = MeasureUnit.InternalGetInstance("volume", "acre-foot");

        /// <summary>
        /// Constant for unit of volume: bushel.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Bushel = MeasureUnit.InternalGetInstance("volume", "bushel");

        /// <summary>
        /// Constant for unit of volume: centiliter.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Centiliter = MeasureUnit.InternalGetInstance("volume", "centiliter");

        /// <summary>
        /// Constant for unit of volume: cubic-centimeter.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit CubicCentimeter = MeasureUnit.InternalGetInstance("volume", "cubic-centimeter");

        /// <summary>
        /// Constant for unit of volume: cubic-foot.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit CubicFoot = MeasureUnit.InternalGetInstance("volume", "cubic-foot");

        /// <summary>
        /// Constant for unit of volume: cubic-inch.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit CubicInch = MeasureUnit.InternalGetInstance("volume", "cubic-inch");

        /// <summary>
        /// Constant for unit of volume: cubic-kilometer.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit CubicKilometer = MeasureUnit.InternalGetInstance("volume", "cubic-kilometer");

        /// <summary>
        /// Constant for unit of volume: cubic-meter.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit CubicMeter = MeasureUnit.InternalGetInstance("volume", "cubic-meter");

        /// <summary>
        /// Constant for unit of volume: cubic-mile.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit CubicMile = MeasureUnit.InternalGetInstance("volume", "cubic-mile");

        /// <summary>
        /// Constant for unit of volume: cubic-yard.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit CubicYard = MeasureUnit.InternalGetInstance("volume", "cubic-yard");

        /// <summary>
        /// Constant for unit of volume: cup.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Cup = MeasureUnit.InternalGetInstance("volume", "cup");

        /// <summary>
        /// Constant for unit of volume: cup-metric.</summary>
        /// <stable>ICU 56</stable>
        public static readonly MeasureUnit CupMetric = MeasureUnit.InternalGetInstance("volume", "cup-metric");

        /// <summary>
        /// Constant for unit of volume: deciliter.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Deciliter = MeasureUnit.InternalGetInstance("volume", "deciliter");

        /// <summary>
        /// Constant for unit of volume: fluid-ounce.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit FluidOunce = MeasureUnit.InternalGetInstance("volume", "fluid-ounce");

        /// <summary>
        /// Constant for unit of volume: gallon.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Gallon = MeasureUnit.InternalGetInstance("volume", "gallon");

        /// <summary>
        /// Constant for unit of volume: gallon-imperial.</summary>
        /// <stable>ICU 57</stable>
        public static readonly MeasureUnit GallonImperial = MeasureUnit.InternalGetInstance("volume", "gallon-imperial");

        /// <summary>
        /// Constant for unit of volume: hectoliter.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Hectoliter = MeasureUnit.InternalGetInstance("volume", "hectoliter");

        /// <summary>
        /// Constant for unit of volume: liter.</summary>
        /// <stable>ICU 53</stable>
        public static readonly MeasureUnit Liter = MeasureUnit.InternalGetInstance("volume", "liter");

        /// <summary>
        /// Constant for unit of volume: megaliter.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Megaliter = MeasureUnit.InternalGetInstance("volume", "megaliter");

        /// <summary>
        /// Constant for unit of volume: milliliter.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Milliliter = MeasureUnit.InternalGetInstance("volume", "milliliter");

        /// <summary>
        /// Constant for unit of volume: pint.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Pint = MeasureUnit.InternalGetInstance("volume", "pint");

        /// <summary>
        /// Constant for unit of volume: pint-metric.</summary>
        /// <stable>ICU 56</stable>
        public static readonly MeasureUnit PintMetric = MeasureUnit.InternalGetInstance("volume", "pint-metric");

        /// <summary>
        /// Constant for unit of volume: quart.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Quart = MeasureUnit.InternalGetInstance("volume", "quart");

        /// <summary>
        /// Constant for unit of volume: tablespoon.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Tablespoon = MeasureUnit.InternalGetInstance("volume", "tablespoon");

        /// <summary>
        /// Constant for unit of volume: teaspoon.</summary>
        /// <stable>ICU 54</stable>
        public static readonly MeasureUnit Teaspoon = MeasureUnit.InternalGetInstance("volume", "teaspoon");



        private static readonly Dictionary<Pair<MeasureUnit, MeasureUnit>, MeasureUnit> unitPerUnitToSingleUnit =
                new Dictionary<Pair<MeasureUnit, MeasureUnit>, MeasureUnit>
                {
                    [Pair.Of(MeasureUnit.Liter, MeasureUnit.Kilometer)] = MeasureUnit.LiterPerKilometer,
                    [Pair.Of(MeasureUnit.Pound, MeasureUnit.SquareInch)] = MeasureUnit.PoundPerSquareInch,
                    [Pair.Of(MeasureUnit.Mile, (MeasureUnit)MeasureUnit.Hour)] = MeasureUnit.MilePerHour,
                    [Pair.Of(MeasureUnit.Milligram, MeasureUnit.Deciliter)] = MeasureUnit.MilligramPerDeciliter,
                    [Pair.Of(MeasureUnit.Mile, MeasureUnit.GallonImperial)] = MeasureUnit.MilePerGallonImperial,
                    [Pair.Of(MeasureUnit.Kilometer, (MeasureUnit)MeasureUnit.Hour)] = MeasureUnit.KilometerPerHour,
                    [Pair.Of(MeasureUnit.Mile, MeasureUnit.Gallon)] = MeasureUnit.MilePerGallon,
                    [Pair.Of(MeasureUnit.Meter, (MeasureUnit)MeasureUnit.Second)] = MeasureUnit.MeterPerSecond,
                };

        // End generated MeasureUnit constants
        /* Private */

        private object WriteReplace() //throws ObjectStreamException
        {
            return new MeasureUnitProxy(type, subType);
        }

        private protected sealed class MeasureUnitProxy //: Externalizable // ICU4N TODO: Serialization
        {
            //private static final long serialVersionUID = -3910681415330989598L;

            private string type;
            private string subType;

            public MeasureUnitProxy(string type, string subType)
            {
                this.type = type;
                this.subType = subType;
            }

            // Must have public constructor, to enable Externalizable
            public MeasureUnitProxy()
            {
            }

            // ICU4N TODO: Serialization
            //@Override
            //        public void writeExternal(ObjectOutput out) throws IOException
            //{
            //            out.writeByte(0); // version
            //            out.writeUTF(type);
            //            out.writeUTF(subType);
            //            out.writeShort(0); // allow for more data.
            //}

            //@Override
            //        public void readExternal(ObjectInput in) throws IOException, ClassNotFoundException {
            //            /* byte version = */ in.readByte(); // version
            //type = in.readUTF();
            //subType = in.readUTF();
            //// allow for more data from future version
            //int extra = in.readShort();
            //if (extra > 0)
            //{
            //    byte[] extraBytes = new byte[extra];
            //                in.read(extraBytes, 0, extra);
            //}
            //        }

            //        private Object readResolve() throws ObjectStreamException
            //{
            //            return MeasureUnit.InternalGetInstance(type, subType);
            //}
        }
    }
}
