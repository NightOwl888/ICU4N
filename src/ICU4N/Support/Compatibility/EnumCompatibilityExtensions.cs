using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N
{
    public static class EnumCompatibilityExtensions
    {
        extension(System.Enum)
        {
            public static TEnum[] GetValues<TEnum>()
                where TEnum : struct, Enum
                => (TEnum[])Enum.GetValues(typeof(TEnum));

            public static string[] GetNames<TEnum>()
                where TEnum : struct, Enum
                => Enum.GetNames(typeof(TEnum));
        }
    }
}
