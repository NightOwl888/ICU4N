using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Defines the types of culture lists that can be retrieved using the <see cref="UCultureInfo.GetUCultures(UCultureTypes)"/> method.
    /// <para/>
    /// This enumeration has a <see cref="FlagsAttribute"/> attribute that allows a bitwise combination of its member values.
    /// </summary>
    [Flags]
    public enum UCultureTypes
    {
        NeutralCultures = 0x0001, // Neutral cultures are cultures like "en", "de", "zh", etc, for enumeration this includes ALL neutrals regardless of other flags
        SpecificCultures = 0x0002, // Non-netural cultuers.  Examples are "en-us", "zh-tw", etc., for enumeration this includes ALL specifics regardless of other flags
        //InstalledWin32Cultures = 0x0004, // Win32 installed cultures in the system and exists in the framework too., this is effectively all cultures

        AllCultures = NeutralCultures | SpecificCultures,
    }
}
