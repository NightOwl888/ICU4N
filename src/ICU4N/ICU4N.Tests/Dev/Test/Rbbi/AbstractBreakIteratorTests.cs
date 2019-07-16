using ICU4N.Support.Text;
using ICU4N.Text;
using NUnit.Framework;

namespace ICU4N.Dev.Test.Rbbi
{
    /// <author>sgill</author>
    public class AbstractBreakIteratorTests : TestFmwk
    {
        private class AbstractBreakIterator : BreakIterator
        {
            private int position = 0;
            private static readonly int LIMIT = 100;

            private int Set(int n)
            {
                position = n;
                if (position > LIMIT)
                {
                    position = LIMIT;
                    return Done;
                }
                if (position < 0)
                {
                    position = 0;
                    return Done;
                }
                return position;
            }

            public override int MoveFirst()
            {
                return Set(0);
            }

            public override int MoveLast()
            {
                return Set(LIMIT);
            }

            public override int Move(int n)
            {
                return Set(position + n);
            }

            public override int MoveNext()
            {
                return Move(1);
            }

            public override int MovePrevious()
            {
                return Move(-1);
            }

            public override int MoveFollowing(int offset)
            {
                return Set(offset + 1);
            }

            public override int Current
            {
                get { return position; }
            }

            public override CharacterIterator Text
            {
                get { return null; }
            }

            public override void SetText(CharacterIterator newText)
            {
            }

        }

        private BreakIterator bi;

        [SetUp]
        public void CreateBreakIterator()
        {
            bi = new AbstractBreakIterator();
        }

        [Test]
        public void TestPreceding()
        {
            int pos = bi.MovePreceding(0);
            TestFmwk.assertEquals("BreakIterator preceding position not correct", BreakIterator.Done, pos);

            pos = bi.MovePreceding(5);
            TestFmwk.assertEquals("BreakIterator preceding position not correct", 4, pos);
        }

        [Test]
        public void TestIsBoundary()
        {
            bool b = bi.IsBoundary(0);
            TestFmwk.assertTrue("BreakIterator is boundary not correct", b);

            b = bi.IsBoundary(5);
            TestFmwk.assertTrue("BreakIterator is boundary not correct", b);
        }
    }
}
