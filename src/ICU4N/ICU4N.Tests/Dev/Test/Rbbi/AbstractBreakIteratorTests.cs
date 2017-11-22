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
                    return DONE;
                }
                if (position < 0)
                {
                    position = 0;
                    return DONE;
                }
                return position;
            }

            public override int First()
            {
                return Set(0);
            }

            public override int Last()
            {
                return Set(LIMIT);
            }

            public override int Next(int n)
            {
                return Set(position + n);
            }

            public override int Next()
            {
                return Next(1);
            }

            public override int Previous()
            {
                return Next(-1);
            }

            public override int Following(int offset)
            {
                return Set(offset + 1);
            }

            public override int Current
            {
                get { return position; }
            }

            public override CharacterIterator GetText()
            {
                return null;
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
            int pos = bi.Preceding(0);
            TestFmwk.assertEquals("BreakIterator preceding position not correct", BreakIterator.DONE, pos);

            pos = bi.Preceding(5);
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
