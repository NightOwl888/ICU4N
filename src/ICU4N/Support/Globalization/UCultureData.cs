using ICU4N.Text;
#nullable enable

namespace ICU4N.Globalization
{
    // ICU4N TODO: Model this after System.Globalization.CultureData.
    internal class UCultureData
    {
        internal string cultureName; // Copied from UCultureInfo upon creation. This should always be in sync.
        internal string? numbersKeyword; // Copied from UCultureInfo upon creation. This should always be in sync because the collection is readonly.

        public string CultureName => cultureName;
        public string? NumbersKeyword => numbersKeyword;

        public NumberingSystem NumberingSystem => NumberingSystem.GetInstance(cultureName, numbersKeyword); // NOTE: This is a cached value.
    }
}
