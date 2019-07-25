using ICU4N.Dev.Test;
using NUnit.Framework;
using System;

namespace ICU4N.Support
{
    public class IntegerExtensionsTest : TestFmwk
    {
        [Test]
        public void TestAsFlagsToEnum()
        {
            int options;
            EnumNoFlags noFlags = EnumNoFlags.Default;
            EnumWithFlags withFlags = EnumWithFlags.Default;

            options = 1;
            noFlags = options.AsFlagsToEnum<EnumNoFlags>();
            withFlags = options.AsFlagsToEnum<EnumWithFlags>();
            assertEquals($"{nameof(EnumNoFlags)} value doesn't match options", EnumNoFlags.Symbol1, noFlags);
            assertEquals($"{nameof(EnumWithFlags)} value doesn't match options", EnumWithFlags.Symbol1, withFlags);

            options = 2;
            noFlags = options.AsFlagsToEnum<EnumNoFlags>();
            withFlags = options.AsFlagsToEnum<EnumWithFlags>();
            assertEquals($"{nameof(EnumNoFlags)} value doesn't match options", EnumNoFlags.Symbol2, noFlags);
            assertEquals($"{nameof(EnumWithFlags)} value doesn't match options", EnumWithFlags.Symbol2, withFlags);

            // Test combined symbols
            options = 6;
            AssertThrows<ArgumentOutOfRangeException>(() => noFlags = options.AsFlagsToEnum<EnumNoFlags>(), $"Expected {nameof(ArgumentOutOfRangeException)} not thrown");
            withFlags = options.AsFlagsToEnum<EnumWithFlags>();
            assertEquals($"{nameof(EnumWithFlags)} value doesn't match options", EnumWithFlags.Symbol2 | EnumWithFlags.Symbol3, withFlags);

            options = 3;
            AssertThrows<ArgumentOutOfRangeException>(() => noFlags = options.AsFlagsToEnum<EnumNoFlags>(), $"Expected {nameof(ArgumentOutOfRangeException)} not thrown");
            withFlags = options.AsFlagsToEnum<EnumWithFlags>();
            assertEquals($"{nameof(EnumWithFlags)} value doesn't match options", EnumWithFlags.Symbol1 | EnumWithFlags.Symbol2, withFlags);

            // Test default value
            options = 0;
            noFlags = options.AsFlagsToEnum<EnumNoFlags>(EnumNoFlags.Symbol3);
            withFlags = options.AsFlagsToEnum<EnumWithFlags>(EnumWithFlags.Symbol3);
            assertEquals($"{nameof(EnumNoFlags)} value doesn't match options", EnumNoFlags.Symbol3, noFlags);
            assertEquals($"{nameof(EnumWithFlags)} value doesn't match options", EnumWithFlags.Symbol3, withFlags);
        }

        public enum EnumNoFlags
        {
            Default = 0,
            Symbol1 = 1,
            Symbol2 = 2,
            Symbol3 = 4
        }

        [Flags]
        public enum EnumWithFlags
        {
            Default = 0,
            Symbol1 = 1,
            Symbol2 = 2,
            Symbol3 = 4
        }
    }
}
