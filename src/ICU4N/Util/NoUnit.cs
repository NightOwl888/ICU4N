using ICU4N.Numerics;

namespace ICU4N.Util
{
    /// <summary>
    /// Dimensionless unit for percent and permille.
    /// </summary>
    /// <seealso cref="NumberFormatter"/>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    internal class NoUnit : MeasureUnit // ICU4N TODO: API - this was public in ICU4J
    {
        //private static final long serialVersionUID = 2467174286237024095L;

        /// <summary>
        /// Constant for the base unit (dimensionless and no scaling).
        /// </summary>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static readonly NoUnit BASE = (NoUnit)MeasureUnit.InternalGetInstance("none", "base");

        /// <summary>
        /// Constant for the percent unit, or 1/100 of a base unit.
        /// </summary>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static readonly NoUnit PERCENT = (NoUnit)MeasureUnit.InternalGetInstance("none", "percent");

        /// <summary>
        /// Constant for the permille unit, or 1/100 of a base unit.
        /// </summary>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static readonly NoUnit PERMILLE = (NoUnit)MeasureUnit.InternalGetInstance("none", "permille");

        /// <summary>
        /// Package local constructor. This class is not designed for subclassing
        /// by ICU users.
        /// </summary>
        /// <param name="subType">The unit subtype.</param>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        internal NoUnit(string subType)
#pragma warning disable CS0618 // Type or member is obsolete
            : base("none", subType)
#pragma warning restore CS0618 // Type or member is obsolete
        {
        }
    }
}
