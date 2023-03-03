using System;
using System.Collections.Generic;
using System.Globalization;

// Port of text.LocaleDisplayNames.UiListItem from ICU4J

namespace ICU4N.Globalization
{
    /// <summary>
    /// Struct used to return information for constructing a UI list, each corresponding to a locale.
    /// </summary>
    /// <stable>ICU 55</stable>
    public struct UiListItem : IEquatable<UiListItem>
    {
        /// <summary>
        /// Returns the minimized locale for an input locale, such as sr-Cyrl → sr
        /// </summary>
        /// <stable>ICU 55</stable>
        public UCultureInfo Minimized { get; private set; }
        /// <summary>
        /// Returns the modified locale for an input locale, such as sr → sr-Cyrl, where there is also an sr-Latn in the list
        /// </summary>
        /// <stable>ICU 55</stable>
        public UCultureInfo Modified { get; private set; }
        /// <summary>
        /// Returns the name of the modified locale in the display locale, such as "Englisch (VS)" (for 'en-US', where the display locale is 'de').
        /// </summary>
        /// <stable>ICU 55</stable>
        public string NameInDisplayLocale { get; private set; }
        /// <summary>
        /// Returns the name of the modified locale in itself, such as "English (US)" (for 'en-US').
        /// </summary>
        /// <stable>ICU 55</stable>
        public string NameInSelf { get; private set; }

        /// <summary>
        /// Constructor, normally only called internally.
        /// </summary>
        /// <param name="minimized">Locale for an input locale.</param>
        /// <param name="modified">Modified for an input locale.</param>
        /// <param name="nameInDisplayLocale">Name of the modified locale in the display locale.</param>
        /// <param name="nameInSelf">Name of the modified locale in itself.</param>
        /// <stable>ICU 55</stable>
        public UiListItem(UCultureInfo minimized, UCultureInfo modified, string nameInDisplayLocale, string nameInSelf)
        {
            this.Minimized = minimized;
            this.Modified = modified;
            this.NameInDisplayLocale = nameInDisplayLocale;
            this.NameInSelf = nameInSelf;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 55</stable>
        public override bool Equals(object obj)
        {
            if (obj is UiListItem other)
                return Equals(other);

            return false;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The <see cref="UiListItem"/> to compare with the current <see cref="UiListItem"/>.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 55</stable>
        public bool Equals(UiListItem other)
        {
            return NameInDisplayLocale.Equals(other.NameInDisplayLocale)
                    && NameInSelf.Equals(other.NameInSelf)
                    && Minimized.Equals(other.Minimized)
                    && Modified.Equals(other.Modified);
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <stable>ICU 55</stable>
        public override int GetHashCode()
        {
            return Modified.GetHashCode() ^ NameInDisplayLocale.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        /// <stable>ICU 55</stable>
        public override string ToString()
        {
            return "{" + Minimized + ", " + Modified + ", " + NameInDisplayLocale + ", " + NameInSelf + "}";
        }

        /// <summary>
        /// Return a comparer that compares the locale names for the display locale or the in-self names,
        /// depending on an input parameter.
        /// </summary>
        /// <param name="comparer">The string <see cref="IComparer{T}"/> to order the <see cref="UiListItem"/>s.</param>
        /// <param name="inSelf">If <c>true</c>, compares the <see cref="NameInSelf"/>, otherwise the <see cref="NameInDisplayLocale"/>.</param>
        /// <returns><see cref="UiListItem"/> comparer.</returns>
        /// <stable>ICU 55</stable>
        public static IComparer<UiListItem> GetComparer(IComparer<string> comparer, bool inSelf)
        {
            return new UiListItemComparer(comparer, inSelf);
        }

        /// <summary>
        /// Return a comparer that compares the locale names for the display locale or the in-self names,
        /// depending on an input parameter.
        /// </summary>
        /// <param name="comparer">The string <see cref="IComparer{T}"/> to order the <see cref="UiListItem"/>s.</param>
        /// <param name="inSelf">If <c>true</c>, compares the <see cref="NameInSelf"/>, otherwise the <see cref="NameInDisplayLocale"/>.</param>
        /// <returns><see cref="UiListItem"/> comparer.</returns>
        /// <stable>ICU4N 60</stable>
        public static IComparer<UiListItem> GetComparer(CompareInfo comparer, bool inSelf) // ICU4N specific overload, since CompareInfo doesn't implement IComparer<string>
        {
            return new UiListItemComparer(comparer, inSelf);
        }

        private sealed class UiListItemComparer : IComparer<UiListItem>
        {
            private readonly IComparer<string> collator;
            private readonly bool useSelf;

            internal UiListItemComparer(CompareInfo collator, bool useSelf) // ICU4N specific overload, since CompareInfo doesn't implement IComparer<string>
                : this(collator.AsComparer(), useSelf)
            {
            }

            internal UiListItemComparer(IComparer<string> collator, bool useSelf)
            {
                this.collator = collator;
                this.useSelf = useSelf;
            }

            public int Compare(UiListItem o1, UiListItem o2)
            {
                int result = useSelf ? collator.Compare(o1.NameInSelf, o2.NameInSelf)
                        : collator.Compare(o1.NameInDisplayLocale, o2.NameInDisplayLocale);
                return result != 0 ? result : o1.Modified.CompareTo(o2.Modified); // just in case
            }
        }

        /// <summary>
        /// Equality comparison operator.
        /// </summary>
        /// <param name="left">The left <see cref="UiListItem"/> to compare.</param>
        /// <param name="right">The right <see cref="UiListItem"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is equal to <paramref name="right"/>, otherwise <c>false</c>.</returns>
        /// <draft>ICU4N 60</draft>
        public static bool operator ==(UiListItem left, UiListItem right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality comparison operator.
        /// </summary>
        /// <param name="left">The left <see cref="UiListItem"/> to compare.</param>
        /// <param name="right">The right <see cref="UiListItem"/> to compare.</param>
        /// <returns><c>false</c> if <paramref name="left"/> is equal to <paramref name="right"/>, otherwise <c>true</c>.</returns>
        /// <draft>ICU4N 60</draft>
        public static bool operator !=(UiListItem left, UiListItem right)
        {
            return !(left == right);
        }
    }
}
