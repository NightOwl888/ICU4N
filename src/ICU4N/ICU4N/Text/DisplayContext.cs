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
    public enum DisplayContext // ICU4N TODO: API - Change case to follow .NET Conventions
    {
        // ICU4N TODO: update docs below from en_GB to en-GB ?
        /**
         * ================================
         * Settings for DIALECT_HANDLING (use one)
         */
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.DIALECT_HANDLING"/>:
        /// use standard names when generating a locale name,
        /// e.g. en_GB displays as 'English (United Kingdom)'.
        /// </summary>
        /// <stable>ICU 51</stable>
        STANDARD_NAMES,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.DIALECT_HANDLING"/>:
        /// use dialect names, when generating a locale name,
        /// e.g. en_GB displays as 'British English'.
        /// </summary>
        /// <stable>ICU 51</stable>
        DIALECT_NAMES,
        /**
         * ================================
         * Settings for CAPITALIZATION (use one)
         */
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.CAPITALIZATION"/>:
        /// The capitalization context to be used is unknown (this is the default value).
        /// </summary>
        /// <stable>ICU 51</stable>
        CAPITALIZATION_NONE,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.CAPITALIZATION"/>:
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for the middle of a sentence.
        /// </summary>
        /// <stable>ICU 51</stable>
        CAPITALIZATION_FOR_MIDDLE_OF_SENTENCE,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.CAPITALIZATION"/>:
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for the beginning of a sentence.
        /// </summary>
        /// <stable>ICU 51</stable>
        CAPITALIZATION_FOR_BEGINNING_OF_SENTENCE,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.CAPITALIZATION"/>:
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for a user-interface list or menu item.
        /// </summary>
        /// <stable>ICU 51</stable>
        CAPITALIZATION_FOR_UI_LIST_OR_MENU,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.CAPITALIZATION"/>:
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for stand-alone usage such as an
        /// isolated name on a calendar page.
        /// </summary>
        /// <stable>ICU 51</stable>
        CAPITALIZATION_FOR_STANDALONE,
        /**
         * ================================
         * Settings for DISPLAY_LENGTH (use one)
         */
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.DISPLAY_LENGTH"/>:
        /// use full names when generating a locale name,
        /// e.g. "United States" for US.
        /// </summary>
        /// <stable>ICU 54</stable>
        LENGTH_FULL,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.DISPLAY_LENGTH"/>:
        /// use short names when generating a locale name,
        /// e.g. "U.S." for US.
        /// </summary>
        /// <stable>ICU 54</stable>
        LENGTH_SHORT,
        /**
         * ================================
         * Settings for SUBSTITUTE_HANDLING (choose one)
         */
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.SUBSTITUTE_HANDLING"/>:
        /// Returns a fallback value (e.g., the input code) when no data is available.
        /// This is the default behavior.
        /// </summary>
        /// <stable>ICU 58</stable>
        SUBSTITUTE,
        /// <summary>
        /// A possible setting for <see cref="DisplayContextType.SUBSTITUTE_HANDLING"/>:
        /// Returns a null value when no data is available.
        /// </summary>
        /// <stable>ICU 58</stable>
        NO_SUBSTITUTE
    }

    /// <summary>
    /// Type values for <see cref="DisplayContext"/>.
    /// </summary>
    /// <stable>ICU 51</stable>
    public enum DisplayContextType // ICU4N TODO: API - change case to follow .NET Conventions
    {
        /// <summary>
        /// <see cref="DIALECT_HANDLING"/> can be set to <see cref="DisplayContext.STANDARD_NAMES"/> 
        /// or <see cref="DisplayContext.DIALECT_NAMES"/>.
        /// </summary>
        /// <stable>ICU 51</stable>
        DIALECT_HANDLING,
        /// <summary>
        /// <see cref="CAPITALIZATION"/> can be set to one of <see cref="DisplayContext.CAPITALIZATION_NONE"/> through
        /// <see cref="DisplayContext.CAPITALIZATION_FOR_STANDALONE"/>.
        /// </summary>
        /// <stable>ICU 51</stable>
        CAPITALIZATION,
        /// <summary>
        /// <see cref="DISPLAY_LENGTH"/> can be set to <see cref="DisplayContext.LENGTH_FULL"/> 
        /// or <see cref="DisplayContext.LENGTH_SHORT"/>.
        /// </summary>
        /// <stable>ICU 54</stable>
        DISPLAY_LENGTH,
        /// <summary>
        /// <see cref="SUBSTITUTE_HANDLING"/> can be set to <see cref="DisplayContext.SUBSTITUTE"/> 
        /// or <see cref="DisplayContext.NO_SUBSTITUTE"/>.
        /// </summary>
        /// <stable>ICU 58</stable>
        SUBSTITUTE_HANDLING
    }

    /// <summary>
    /// Extension methods for <see cref="DisplayContext"/>.
    /// </summary>
    /// <draft>ICU4N 60</draft>
    public static class DisplayContextExtensions
    {
        private static IDictionary<DisplayContext, DisplayContextImpl> map = new Dictionary<DisplayContext, DisplayContextImpl>
        {
            { DisplayContext.STANDARD_NAMES, new DisplayContextImpl(DisplayContextType.DIALECT_HANDLING, 0) },
            { DisplayContext.DIALECT_NAMES, new DisplayContextImpl(DisplayContextType.DIALECT_HANDLING, 1) },

            { DisplayContext.CAPITALIZATION_NONE, new DisplayContextImpl(DisplayContextType.CAPITALIZATION, 0) },
            { DisplayContext.CAPITALIZATION_FOR_MIDDLE_OF_SENTENCE, new DisplayContextImpl(DisplayContextType.CAPITALIZATION, 1) },
            { DisplayContext.CAPITALIZATION_FOR_BEGINNING_OF_SENTENCE, new DisplayContextImpl(DisplayContextType.CAPITALIZATION, 2) },
            { DisplayContext.CAPITALIZATION_FOR_UI_LIST_OR_MENU, new DisplayContextImpl(DisplayContextType.CAPITALIZATION, 3) },
            { DisplayContext.CAPITALIZATION_FOR_STANDALONE, new DisplayContextImpl(DisplayContextType.CAPITALIZATION, 4) },

            { DisplayContext.LENGTH_FULL, new DisplayContextImpl(DisplayContextType.DISPLAY_LENGTH, 0) },
            { DisplayContext.LENGTH_SHORT, new DisplayContextImpl(DisplayContextType.DISPLAY_LENGTH, 1) },

            { DisplayContext.SUBSTITUTE, new DisplayContextImpl(DisplayContextType.SUBSTITUTE_HANDLING, 0) },
            { DisplayContext.NO_SUBSTITUTE, new DisplayContextImpl(DisplayContextType.SUBSTITUTE_HANDLING, 1) },
        };

        /// <summary>
        /// Get the <see cref="DisplayContextType"/> part of the enum item
        /// (e.g. <see cref="DisplayContextType.CAPITALIZATION"/>)
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
        /// (e.g. <see cref="DisplayContext.CAPITALIZATION_FOR_STANDALONE"/>)
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
            var impl = map.Get(displayContext);
            if (impl == null)
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

            public DisplayContextType Type { get { return type; } }
            public int Value { get { return value; } }
        }
    }
}
