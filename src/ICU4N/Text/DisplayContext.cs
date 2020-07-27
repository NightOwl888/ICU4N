using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;

namespace ICU4N.Text
{
    /// <summary>
    /// Display context settings.
    /// Note, the specific numeric values are internal and may change.
    /// </summary>
    /// <stable>ICU 51</stable>
    public enum DisplayContext
    {
        // ICU4N TODO: update docs below from en_GB to en-GB ?
        /**
         * ================================
         * Settings for DIALECT_HANDLING (use one)
         */
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.DialectHandling"/>:
        /// use standard names when generating a locale name,
        /// e.g. en_GB displays as 'English (United Kingdom)'.
        /// </summary>
        /// <stable>ICU 51</stable>
        StandardNames,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.DialectHandling"/>:
        /// use dialect names, when generating a locale name,
        /// e.g. en_GB displays as 'British English'.
        /// </summary>
        /// <stable>ICU 51</stable>
        DialectNames,
        /**
         * ================================
         * Settings for CAPITALIZATION (use one)
         */
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.Capitalization"/>:
        /// The capitalization context to be used is unknown (this is the default value).
        /// </summary>
        /// <stable>ICU 51</stable>
        CapitalizationNone,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.Capitalization"/>:
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for the middle of a sentence.
        /// </summary>
        /// <stable>ICU 51</stable>
        CapitalizationForMiddleOfSentence,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.Capitalization"/>:
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for the beginning of a sentence.
        /// </summary>
        /// <stable>ICU 51</stable>
        CapitalizationForBeginningOfSentence,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.Capitalization"/>:
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for a user-interface list or menu item.
        /// </summary>
        /// <stable>ICU 51</stable>
        CapitalizationForUIListOrMenu,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.Capitalization"/>:
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for stand-alone usage such as an
        /// isolated name on a calendar page.
        /// </summary>
        /// <stable>ICU 51</stable>
        CapitalizationForStandalone,
        /**
         * ================================
         * Settings for DISPLAY_LENGTH (use one)
         */
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.DisplayLength"/>:
        /// use full names when generating a locale name,
        /// e.g. "United States" for US.
        /// </summary>
        /// <stable>ICU 54</stable>
        LengthFull,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.DisplayLength"/>:
        /// use short names when generating a locale name,
        /// e.g. "U.S." for US.
        /// </summary>
        /// <stable>ICU 54</stable>
        LengthShort,
        /**
         * ================================
         * Settings for SUBSTITUTE_HANDLING (choose one)
         */
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.SubstituteHandling"/>:
        /// Returns a fallback value (e.g., the input code) when no data is available.
        /// This is the default behavior.
        /// </summary>
        /// <stable>ICU 58</stable>
        Substitute,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.SubstituteHandling"/>:
        /// Returns a null value when no data is available.
        /// </summary>
        /// <stable>ICU 58</stable>
        NoSubstitute
    }

    /// <summary>
    /// Type values for <see cref="DisplayContext"/>.
    /// </summary>
    /// <stable>ICU 51</stable>
    public enum DisplayContextType
    {
        /// <summary>
        /// <see cref="DialectHandling"/> can be set to <see cref="DisplayContext.StandardNames"/> 
        /// or <see cref="DisplayContext.DialectNames"/>.
        /// </summary>
        /// <stable>ICU 51</stable>
        DialectHandling,
        /// <summary>
        /// <see cref="Capitalization"/> can be set to one of <see cref="DisplayContext.CapitalizationNone"/> through
        /// <see cref="DisplayContext.CapitalizationForStandalone"/>.
        /// </summary>
        /// <stable>ICU 51</stable>
        Capitalization,
        /// <summary>
        /// <see cref="DisplayLength"/> can be set to <see cref="DisplayContext.LengthFull"/> 
        /// or <see cref="DisplayContext.LengthShort"/>.
        /// </summary>
        /// <stable>ICU 54</stable>
        DisplayLength,
        /// <summary>
        /// <see cref="SubstituteHandling"/> can be set to <see cref="DisplayContext.Substitute"/> 
        /// or <see cref="DisplayContext.NoSubstitute"/>.
        /// </summary>
        /// <stable>ICU 58</stable>
        SubstituteHandling
    }

    /// <summary>
    /// Extension methods for <see cref="DisplayContext"/>.
    /// </summary>
    /// <draft>ICU4N 60</draft>
    public static class DisplayContextExtensions
    {
        private static IDictionary<DisplayContext, DisplayContextImpl> map = new Dictionary<DisplayContext, DisplayContextImpl>
        {
            { DisplayContext.StandardNames, new DisplayContextImpl(DisplayContextType.DialectHandling, 0) },
            { DisplayContext.DialectNames, new DisplayContextImpl(DisplayContextType.DialectHandling, 1) },

            { DisplayContext.CapitalizationNone, new DisplayContextImpl(DisplayContextType.Capitalization, 0) },
            { DisplayContext.CapitalizationForMiddleOfSentence, new DisplayContextImpl(DisplayContextType.Capitalization, 1) },
            { DisplayContext.CapitalizationForBeginningOfSentence, new DisplayContextImpl(DisplayContextType.Capitalization, 2) },
            { DisplayContext.CapitalizationForUIListOrMenu, new DisplayContextImpl(DisplayContextType.Capitalization, 3) },
            { DisplayContext.CapitalizationForStandalone, new DisplayContextImpl(DisplayContextType.Capitalization, 4) },

            { DisplayContext.LengthFull, new DisplayContextImpl(DisplayContextType.DisplayLength, 0) },
            { DisplayContext.LengthShort, new DisplayContextImpl(DisplayContextType.DisplayLength, 1) },

            { DisplayContext.Substitute, new DisplayContextImpl(DisplayContextType.SubstituteHandling, 0) },
            { DisplayContext.NoSubstitute, new DisplayContextImpl(DisplayContextType.SubstituteHandling, 1) },
        };

        /// <summary>
        /// Get the <see cref="DisplayContextType"/> part of the enum item
        /// (e.g. <see cref="DisplayContextType.Capitalization"/>)
        /// </summary>
        /// <param name="displayContext">This <see cref="DisplayContext"/>.</param>
        /// <returns>The <see cref="DisplayContextType"/> part of the enum item.</returns>
        /// <stable>ICU 51</stable>
        public static DisplayContextType Type(this DisplayContext displayContext)
        {
            return displayContext.GetImpl().Type;
        }

        /// <summary>
        /// Get the value part of the enum item
        /// (e.g. <see cref="DisplayContext.CapitalizationForStandalone"/>)
        /// </summary>
        /// <param name="displayContext">This <see cref="DisplayContext"/>.</param>
        /// <returns>The value part of the enum item.</returns>
        /// <stable>ICU 51</stable>
        public static int Value(this DisplayContext displayContext)
        {
            return displayContext.GetImpl().Value;
        }

        private static DisplayContextImpl GetImpl(this DisplayContext displayContext)
        {
            if (!map.TryGetValue(displayContext, out DisplayContextImpl impl) || impl == null)
                throw new ArgumentOutOfRangeException(string.Format("Argument {0} is not a valid DisplayContext value.", (int)displayContext));
            return impl;
        }

        private class DisplayContextImpl
        {
            private readonly DisplayContextType type;
            private readonly int value;

            public DisplayContextImpl(DisplayContextType type, int value)
            {
                this.type = type;
                this.value = value;
            }

            public DisplayContextType Type => type;
            public int Value => value;
        }
    }
}
