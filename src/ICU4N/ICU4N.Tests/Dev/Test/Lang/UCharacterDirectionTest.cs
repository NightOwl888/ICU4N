using ICU4N.Lang;
using NUnit.Framework;
using System;

namespace ICU4N.Dev.Test.Lang
{
    /// <summary>
    /// Testing UCharacterDirection
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <since>July 22 2002</since>
    public class UCharacterDirectionTest : TestFmwk
    {
        // constructor -----------------------------------------------------------

        /**
        * Private constructor to prevent initialization
        */
        public UCharacterDirectionTest()
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
            String[] name = {"Left-to-Right",
                         "Right-to-Left",
                         "European Number",
                         "European Number Separator",
                         "European Number Terminator",
                         "Arabic Number",
                         "Common Number Separator",
                         "Paragraph Separator",
                         "Segment Separator",
                         "Whitespace",
                         "Other Neutrals",
                         "Left-to-Right Embedding",
                         "Left-to-Right Override",
                         "Right-to-Left Arabic",
                         "Right-to-Left Embedding",
                         "Right-to-Left Override",
                         "Pop Directional Format",
                         "Non-Spacing Mark",
                         "Boundary Neutral",
                         "First Strong Isolate",
                         "Left-to-Right Isolate",
                         "Right-to-Left Isolate",
                         "Pop Directional Isolate",
                         "Unassigned"};

            for (UCharacterDirection i = UCharacterDirection.LeftToRight;
                // Placed <= because we need to consider 'Unassigned'
                // when it goes out of bounds of UCharacterDirection
                i <= UCharacterDirection.CharDirectionCount; i++)
            {
                if (!i.AsString().Equals(name[(int)i]))
                {
                    Errln("Error toString for direction " + i + " expected " +
                          name[(int)i]);
                }
            }
        }
    }
}
