using J2N.Collections.Generic.Extensions;
using System;
using System.Collections.Generic;

namespace ICU4N.Util
{
    /// <summary>
    /// Measurement unit for time units.
    /// </summary>
    ///// <seealso cref="TimeUnitAmount"/>
    /// <stable>ICU 4.0</stable>
    internal class TimeUnit : MeasureUnit // ICU4N TODO: API - this was public in ICU4J
    {
        //private static final long serialVersionUID = -2839973855554750484L;

        /**
         * Here for serialization backward compatibility only.
         */
        private readonly int index;

        internal TimeUnit(string type, string code)
#pragma warning disable CS0618 // Type or member is obsolete
            : base(type, code)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            index = 0;
        }

        private static readonly IList<TimeUnit> values = new TimeUnit[] { Second, Minute, Hour, Day, Week, Month, Year }.AsReadOnly();

        /// <summary>
        /// Gets the available values.
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public static IList<TimeUnit> Values => values;


        // ICU4N TODO: Serialization
        //private object WriteReplace() // throws ObjectStreamException
        //{
        //    return new MeasureUnitProxy(type, subType);
        //}

        //// For backward compatibility only
        //private Object ReadResolve() //throws ObjectStreamException
        //{
        //    // The old index field used to uniquely identify the time unit.
        //    switch (index)
        //    {
        //        case 6:
        //            return Second;
        //        case 5:
        //            return Minute;
        //        case 4:
        //            return Hour;
        //        case 3:
        //            return Day;
        //        case 2:
        //            return Week;
        //        case 1:
        //            return Month;
        //        case 0:
        //            return Year;
        //        default:
        //            throw new Exception("Bad index: " + index); //new InvalidObjectException("Bad index: " + index);
        //    }
        //}
    }
}
