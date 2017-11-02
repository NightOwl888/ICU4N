using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    public enum DisplayContext
    {
        /**
         * ================================
         * Settings for DIALECT_HANDLING (use one)
         */
        /**
         * A possible setting for DIALECT_HANDLING:
         * use standard names when generating a locale name,
         * e.g. en_GB displays as 'English (United Kingdom)'.
         * @stable ICU 51
         */
        STANDARD_NAMES,
        /**
         * A possible setting for DIALECT_HANDLING:
         * use dialect names, when generating a locale name,
         * e.g. en_GB displays as 'British English'.
         * @stable ICU 51
         */
        DIALECT_NAMES,
        /**
         * ================================
         * Settings for CAPITALIZATION (use one)
         */
        /**
         * A possible setting for CAPITALIZATION:
         * The capitalization context to be used is unknown (this is the default value).
         * @stable ICU 51
         */
        CAPITALIZATION_NONE,
        /**
         * A possible setting for CAPITALIZATION:
         * The capitalization context if a date, date symbol or display name is to be
         * formatted with capitalization appropriate for the middle of a sentence.
         * @stable ICU 51
         */
        CAPITALIZATION_FOR_MIDDLE_OF_SENTENCE,
        /**
         * A possible setting for CAPITALIZATION:
         * The capitalization context if a date, date symbol or display name is to be
         * formatted with capitalization appropriate for the beginning of a sentence.
         * @stable ICU 51
         */
        CAPITALIZATION_FOR_BEGINNING_OF_SENTENCE,
        /**
         * A possible setting for CAPITALIZATION:
         * The capitalization context if a date, date symbol or display name is to be
         * formatted with capitalization appropriate for a user-interface list or menu item.
         * @stable ICU 51
         */
        CAPITALIZATION_FOR_UI_LIST_OR_MENU,
        /**
         * A possible setting for CAPITALIZATION:
         * The capitalization context if a date, date symbol or display name is to be
         * formatted with capitalization appropriate for stand-alone usage such as an
         * isolated name on a calendar page.
         * @stable ICU 51
         */
        CAPITALIZATION_FOR_STANDALONE,
        /**
         * ================================
         * Settings for DISPLAY_LENGTH (use one)
         */
        /**
         * A possible setting for DISPLAY_LENGTH:
         * use full names when generating a locale name,
         * e.g. "United States" for US.
         * @stable ICU 54
         */
        LENGTH_FULL,
        /**
         * A possible setting for DISPLAY_LENGTH:
         * use short names when generating a locale name,
         * e.g. "U.S." for US.
         * @stable ICU 54
         */
        LENGTH_SHORT,
        /**
         * ================================
         * Settings for SUBSTITUTE_HANDLING (choose one)
         */
        /**
         * A possible setting for SUBSTITUTE_HANDLING:
         * Returns a fallback value (e.g., the input code) when no data is available.
         * This is the default behavior.
         * @stable ICU 58
         */
        SUBSTITUTE,
        /**
         * A possible setting for SUBSTITUTE_HANDLING:
         * Returns a null value when no data is available.
         * @stable ICU 58
         */
        NO_SUBSTITUTE
    }

    /**
     * Type values for DisplayContext
     * @stable ICU 51
     */
    public enum DisplayContextType
    {
        /**
         * DIALECT_HANDLING can be set to STANDARD_NAMES or DIALECT_NAMES.
         * @stable ICU 51
         */
        DIALECT_HANDLING,
        /**
         * CAPITALIZATION can be set to one of CAPITALIZATION_NONE through
         * CAPITALIZATION_FOR_STANDALONE.
         * @stable ICU 51
         */
        CAPITALIZATION,
        /**
         * DISPLAY_LENGTH can be set to LENGTH_FULL or LENGTH_SHORT.
         * @stable ICU 54
         */
        DISPLAY_LENGTH,
        /**
         * SUBSTITUTE_HANDLING can be set to SUBSTITUTE or NO_SUBSTITUTE.
         * @stable ICU 58
         */
        SUBSTITUTE_HANDLING
    }

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
        /// Get the DisplayContextType part of the enum item
        /// (e.g. CAPITALIZATION)
        /// </summary>
        /// <param name="displayContext">This <see cref="DisplayContext"/>.</param>
        /// <returns>The DisplayContextType part of the enum item.</returns>
        /// <stable>ICU 51</stable>
        public static DisplayContextType Type(this DisplayContext displayContext)
        {
            return displayContext.GetImpl().Type;
        }

        /// <summary>
        /// Get the value part of the enum item
        /// (e.g. CAPITALIZATION_FOR_STANDALONE)
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
