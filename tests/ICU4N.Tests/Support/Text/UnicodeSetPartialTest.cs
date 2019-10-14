using ICU4N.Dev.Test;
using NUnit.Framework;

namespace ICU4N.Text
{
    public partial class UnicodeSetPartialTest : TestFmwk
    {
        private const int Mai_Han_Akat = 0x0E31;
        private const int Letter_A = 0x0041;
        private const int Letter_D = 0x0044;
        private const int Letter_E = 0x0045;
        private const int Letter_F = 0x0046;
        private const int Letter_G = 0x0047;
        private const int Letter_M = 0x004d;

        private UnicodeSet thaiWordSet;
        private UnicodeSet thaiWordSuperset;
        private UnicodeSet thaiWordSubset;
        private UnicodeSet thaiWordSet2;
        private UnicodeSet burmeseWordSet;
        private UnicodeSet emptySet;

        private UnicodeSet aThruFSet;
        private UnicodeSet aThruFSubset;
        private UnicodeSet aThruFSuperset;
        private UnicodeSet dThruMSet;

        public override void TestInitialize()
        {
            base.TestInitialize();

            thaiWordSet = new UnicodeSet("[[:Thai:]&[:LineBreak=SA:]]").Compact();
            thaiWordSet2 = new UnicodeSet("[[:Thai:]&[:LineBreak=SA:]]").Compact();
            thaiWordSubset = new UnicodeSet("[[:Thai:]&[:LineBreak=SA:]]").Remove(Mai_Han_Akat).Compact();
            thaiWordSuperset = new UnicodeSet("[[:Thai:]&[:LineBreak=SA:]]").Add(Letter_A).Compact();
            burmeseWordSet = new UnicodeSet("[[:Mymr:]&[:LineBreak=SA:]]").Compact();
            emptySet = new UnicodeSet();

            aThruFSet = new UnicodeSet(Letter_A, Letter_F).Compact();
            aThruFSubset = new UnicodeSet(Letter_A, Letter_E).Compact();
            aThruFSuperset = new UnicodeSet(Letter_A, Letter_G).Compact();
            dThruMSet = new UnicodeSet(Letter_D, Letter_M).Compact();
        }


        [Test]
        public void TestSetEquals_UnicodeSet()
        {
            string methodName = nameof(UnicodeSet.SetEquals);

            // Test empty set
            assertFalse($"{methodName}: The word sets are equal", thaiWordSet.SetEquals(emptySet));
            assertFalse($"{methodName}: The word sets are equal", emptySet.SetEquals(thaiWordSet));

            assertTrue($"{methodName}: The word sets are not equal", thaiWordSet.SetEquals(thaiWordSet2));
            assertTrue($"{methodName}: The word sets are not equal", thaiWordSet2.SetEquals(thaiWordSet));
            assertFalse($"{methodName}: The word sets are equal", thaiWordSet.SetEquals(burmeseWordSet));
        }

        [Test]
        public void TestIsSupersetOf_UnicodeSet()
        {
            string setOperation = "superset", methodName = nameof(UnicodeSet.IsSupersetOf);

            // Test empty set
            assertTrue($"{methodName}: {nameof(thaiWordSet)} is not a {setOperation} of {nameof(emptySet)}", thaiWordSet.IsSupersetOf(emptySet));
            assertTrue($"{methodName}: {nameof(emptySet)} is not a {setOperation} of {nameof(emptySet)}", emptySet.IsSupersetOf(emptySet));
            assertFalse($"{methodName}: {nameof(emptySet)} is a {setOperation} of {nameof(thaiWordSet)}", emptySet.IsSupersetOf(thaiWordSet));

            assertTrue($"{methodName}: {nameof(thaiWordSet)} is not a {setOperation} of {nameof(thaiWordSubset)}", thaiWordSet.IsSupersetOf(thaiWordSubset));
            assertTrue($"{methodName}: {nameof(thaiWordSet)} is not a {setOperation} of {nameof(thaiWordSet2)}", thaiWordSet.IsSupersetOf(thaiWordSet2));
            assertFalse($"{methodName}: {nameof(thaiWordSet)} is a {setOperation} of {nameof(burmeseWordSet)}", thaiWordSet.IsSupersetOf(burmeseWordSet));

        }

        [Test]
        public void TestIsProperSupersetOf_UnicodeSet()
        {
            string setOperation = "proper superset", methodName = nameof(UnicodeSet.IsProperSupersetOf);
            

            // Test empty set
            assertTrue($"{methodName}: {nameof(thaiWordSet)} is not a {setOperation} of {nameof(emptySet)}", thaiWordSet.IsProperSupersetOf(emptySet));
            assertFalse($"{methodName}: {nameof(emptySet)} is a {setOperation} of {nameof(emptySet)}", emptySet.IsProperSupersetOf(emptySet));
            assertFalse($"{methodName}: {nameof(emptySet)} is a {setOperation} of {nameof(thaiWordSet)}", emptySet.IsProperSupersetOf(thaiWordSet));

            assertTrue($"{methodName}: {nameof(thaiWordSet)} is not a {setOperation} of {nameof(thaiWordSubset)}", thaiWordSet.IsProperSupersetOf(thaiWordSubset));
            assertFalse($"{methodName}: {nameof(thaiWordSet)} is a {setOperation} of {nameof(thaiWordSet2)}", thaiWordSet.IsProperSupersetOf(thaiWordSet2));
            assertFalse($"{methodName}: {nameof(thaiWordSet)} is a {setOperation} of {nameof(burmeseWordSet)}", thaiWordSet.IsProperSupersetOf(burmeseWordSet));
        }

        [Test]
        public void TestIsSubsetOf_UnicodeSet()
        {
            string setOperation = "subset", methodName = nameof(UnicodeSet.IsSubsetOf);

            // Test empty set
            assertFalse($"{methodName}: {nameof(thaiWordSet)} is a {setOperation} of {nameof(emptySet)}", thaiWordSet.IsSubsetOf(emptySet));
            assertTrue($"{methodName}: {nameof(emptySet)} is not a {setOperation} of {nameof(emptySet)}", emptySet.IsSubsetOf(emptySet));
            assertTrue($"{methodName}: {nameof(emptySet)} is not a {setOperation} of {nameof(thaiWordSet)}", emptySet.IsSubsetOf(thaiWordSet));

            assertTrue($"{methodName}: {nameof(thaiWordSet)} is not a {setOperation} of {nameof(thaiWordSuperset)}", thaiWordSet.IsSubsetOf(thaiWordSuperset));
            assertTrue($"{methodName}: {nameof(thaiWordSet)} is not a {setOperation} of {nameof(thaiWordSet2)}", thaiWordSet.IsSubsetOf(thaiWordSet2));
            assertFalse($"{methodName}: {nameof(thaiWordSet)} is a {setOperation} of {nameof(burmeseWordSet)}", thaiWordSet.IsSubsetOf(burmeseWordSet));
            assertFalse($"{methodName}: {nameof(thaiWordSet)} is a {setOperation} of {nameof(thaiWordSubset)}", thaiWordSet.IsSubsetOf(thaiWordSubset));

        }

        [Test]
        public void TestIsProperSubsetOf_UnicodeSet()
        {
            string setOperation = "proper subset", methodName = nameof(UnicodeSet.IsProperSubsetOf);


            // Test empty set
            assertFalse($"{methodName}: {nameof(thaiWordSet)} is a {setOperation} of {nameof(emptySet)}", thaiWordSet.IsProperSubsetOf(emptySet));
            assertFalse($"{methodName}: {nameof(emptySet)} is a {setOperation} of {nameof(emptySet)}", emptySet.IsProperSubsetOf(emptySet));
            assertTrue($"{methodName}: {nameof(emptySet)} is not a {setOperation} of {nameof(thaiWordSet)}", emptySet.IsProperSubsetOf(thaiWordSet));

            assertTrue($"{methodName}: {nameof(thaiWordSet)} is not a {setOperation} of {nameof(thaiWordSuperset)}", thaiWordSet.IsProperSubsetOf(thaiWordSuperset));
            assertFalse($"{methodName}: {nameof(thaiWordSet)} is a {setOperation} of {nameof(thaiWordSubset)}", thaiWordSet.IsProperSubsetOf(thaiWordSubset));
            assertFalse($"{methodName}: {nameof(thaiWordSet)} is a {setOperation} of {nameof(thaiWordSet2)}", thaiWordSet.IsProperSubsetOf(thaiWordSet2));
            assertFalse($"{methodName}: {nameof(thaiWordSet)} is a {setOperation} of {nameof(burmeseWordSet)}", thaiWordSet.IsProperSubsetOf(burmeseWordSet));
        }

        [Test]
        public void TestOverlaps_UnicodeSet()
        {
            string setOperation = "overlap", methodName = nameof(UnicodeSet.Overlaps);

            // Test empty set
            assertFalse($"{methodName}: {nameof(thaiWordSet)} does {setOperation} with {nameof(emptySet)}", thaiWordSet.Overlaps(emptySet));
            assertFalse($"{methodName}: {nameof(emptySet)} does {setOperation} with {nameof(emptySet)}", emptySet.Overlaps(emptySet));
            assertFalse($"{methodName}: {nameof(emptySet)} does {setOperation} with {nameof(thaiWordSet)}", emptySet.Overlaps(thaiWordSet));

            assertTrue($"{methodName}: {nameof(thaiWordSet)} does not {setOperation} with {nameof(thaiWordSuperset)}", thaiWordSet.Overlaps(thaiWordSuperset));
            assertTrue($"{methodName}: {nameof(thaiWordSet)} does not {setOperation} with {nameof(thaiWordSubset)}", thaiWordSet.Overlaps(thaiWordSubset));
            assertTrue($"{methodName}: {nameof(thaiWordSet)} does not {setOperation} with {nameof(thaiWordSet2)}", thaiWordSet.Overlaps(thaiWordSet2));
            assertFalse($"{methodName}: {nameof(thaiWordSet)} does {setOperation} with {nameof(burmeseWordSet)}", thaiWordSet.Overlaps(burmeseWordSet));
        }

        [Test]
        public void TestSymmetricExceptWith_UnicodeSet()
        {
            string setOperation = "symmetric except with (xOr)", methodName = nameof(UnicodeSet.SymmetricExceptWith);

            // Test empty set
            assertEquals($"{methodName}: {nameof(aThruFSet)} {setOperation} {nameof(emptySet)} is wrong", aThruFSet, aThruFSet.SymmetricExceptWith(emptySet));
            assertEquals($"{methodName}: {nameof(emptySet)} {setOperation} {nameof(aThruFSet)} is wrong", aThruFSet, emptySet.SymmetricExceptWith(aThruFSet));

            assertEquals($"{methodName}: {nameof(aThruFSet)} {setOperation} {nameof(dThruMSet)} is wrong", new UnicodeSet("[A-CG-M]"), aThruFSet.SymmetricExceptWith(dThruMSet));
            assertEquals($"{methodName}: {nameof(thaiWordSet)} {setOperation} {nameof(thaiWordSuperset)} is wrong", new UnicodeSet("[A]"), thaiWordSet.SymmetricExceptWith(thaiWordSuperset));
        }
    }
}
