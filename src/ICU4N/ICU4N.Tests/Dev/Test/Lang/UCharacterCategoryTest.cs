using ICU4N.Lang;
using NUnit.Framework;
using System;
using System.Globalization;

namespace ICU4N.Dev.Test.Lang
{
    /// <summary>
    /// Testing UCharacterCategory
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <since>April 02 2002</since>
    public class UCharacterCategoryTest : TestFmwk
    {
        // constructor -----------------------------------------------------------

        /**
        * Private constructor to prevent initialisation
        */
        public UCharacterCategoryTest()
        {
        }

        // public methods --------------------------------------------------------

        /**
        * Gets the name of the argument category
        * @returns category name
        */
        [Test]
        public void TestToString()
        {
            String[] name = {"Unassigned",
                           "Letter, Uppercase",
                           "Letter, Lowercase",
                           "Letter, Titlecase",
                           "Letter, Modifier",
                           "Letter, Other",
                           "Mark, Non-Spacing",
                           "Mark, Enclosing",
                           "Mark, Spacing Combining",
                           "Number, Decimal Digit",
                           "Number, Letter",
                           "Number, Other",
                           "Separator, Space",
                           "Separator, Line",
                           "Separator, Paragraph",
                           "Other, Control",
                           "Other, Format",
                           "Other, Private Use",
                           "Other, Surrogate",
                           "Punctuation, Dash",
                           "Punctuation, Open",
                           "Punctuation, Close",
                           "Punctuation, Connector",
                           "Punctuation, Other",
                           "Symbol, Math",
                           "Symbol, Currency",
                           "Symbol, Modifier",
                           "Symbol, Other",
                           "Punctuation, Initial quote",
                           "Punctuation, Final quote"};

            for (int i = UnicodeCategory.OtherNotAssigned.ToIcuValue();
                     i < UCharacterCategory.CHAR_CATEGORY_COUNT; i++)
            {
                if (!UCharacterCategory.ToString(UnicodeCategoryConvert.FromIcuValue(i)).Equals(name[i]))
                {
                    Errln("Error toString for category " + i + " expected " +
                          name[i]);
                }
            }

            foreach (UnicodeCategory category in Enum.GetValues(typeof(UnicodeCategory)))
            {
                if (!UCharacterCategory.ToString(category).Equals(name[category.ToIcuValue()]))
                {
                    Errln("Error toString for category " + category + " expected " +
                          name[category.ToIcuValue()]);
                }
            }
        }
    }
}
