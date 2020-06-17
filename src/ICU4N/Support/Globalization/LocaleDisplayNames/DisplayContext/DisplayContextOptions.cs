// Port of text.DisplayContext of ICU4J to DisplayContextOptions and enum properties

using ICU4N.Util;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Specifies display context options when data is loaded from resources.
    /// </summary>
    /// <draft>ICU 60</draft>
    // We don't seal this class so it is possible to use inheritance to pass additional settings,
    // similar to ICU4J's implementation.
    public class DisplayContextOptions : IFreezable<DisplayContextOptions>
    {
        /// <summary>
        /// Capitalization of a date, date symbol, or display name based on context.
        /// </summary>
        /// <draft>ICU 60</draft>
        public Capitalization Capitalization { get; set; } = Capitalization.None;

        /// <summary>
        /// Dialect handling for locale display names.
        /// </summary>
        /// <draft>ICU 60</draft>
        public DialectHandling DialectHandling { get; set; } = DialectHandling.StandardNames;

        /// <summary>
        /// Display length of locale names.
        /// </summary>
        /// <draft>ICU 60</draft>
        public DisplayLength DisplayLength { get; set; } = DisplayLength.Full;

        /// <summary>
        /// Fallback substitute handling of localized text
        /// when the underlying data set does not contain the value.
        /// </summary>
        /// <draft>ICU 60</draft>
        public SubstituteHandling SubstituteHandling { get; set; } = SubstituteHandling.Substitute;

        /// <summary>
        /// Makes the current instance immutable.
        /// </summary>
        /// <returns>The immutable instance.</returns>
        /// <draft>ICU 60</draft>
        public DisplayContextOptions Freeze()
        {
            IsFrozen = true;
            return this;
        }

        /// <summary>
        /// Gets a value indicating whether the current instance is frozen.
        /// </summary>
        /// <draft>ICU 60</draft>
        public bool IsFrozen { get; private set; } = false;

        /// <summary>
        /// Gets a clone of the current instance that is writable.
        /// </summary>
        /// <returns>The writable clone.</returns>
        /// <draft>ICU 60</draft>
        public DisplayContextOptions CloneAsThawed()
        {
            var clone = (DisplayContextOptions)base.MemberwiseClone();
            clone.IsFrozen = false;
            return clone;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is DisplayContextOptions other)
            {
                return Capitalization.Equals(other.Capitalization) &&
                    DialectHandling.Equals(other.DialectHandling) &&
                    DisplayLength.Equals(other.DisplayLength) &&
                    SubstituteHandling.Equals(other.SubstituteHandling);
            }
            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;
                hashCode = (hashCode * 397) * Capitalization.GetHashCode();
                hashCode = (hashCode * 397) * DialectHandling.GetHashCode();
                hashCode = (hashCode * 397) * DisplayLength.GetHashCode();
                hashCode = (hashCode * 397) * SubstituteHandling.GetHashCode();
                return hashCode;
            }
        }
    }
}
