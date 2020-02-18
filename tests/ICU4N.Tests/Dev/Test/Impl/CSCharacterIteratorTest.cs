using ICU4N.Impl;
using ICU4N.Support.Text;
using J2N.Text;
using NUnit.Framework;
using System;

namespace ICU4N.Dev.Test.Impl
{
    public class CSCharacterIteratorTest : TestFmwk
    {
        public CSCharacterIteratorTest() { }

        [Test]
        public void TestAPI()
        {
            String text = "Hello, World";

            ICharSequence cs = text.AsCharSequence();
            ICharacterEnumerator csci = new CharSequenceCharacterEnumerator(cs);
            ICharacterEnumerator sci = new StringCharacterEnumerator(text);

            assertEquals("", sci.TrySetIndex(6), csci.TrySetIndex(6));
            assertEquals("", sci.Index, csci.Index);
            assertEquals("", sci.Current, csci.Current);
            assertEquals("", sci.MovePrevious(), csci.MovePrevious());
            assertEquals("", sci.Current, csci.Current);
            assertEquals("", sci.MoveNext(), csci.MoveNext());
            assertEquals("", sci.Current, csci.Current);
            assertEquals("", sci.StartIndex, csci.StartIndex);
            assertEquals("", sci.EndIndex, csci.EndIndex);
            assertEquals("", sci.MoveFirst(), csci.MoveFirst());
            assertEquals("", sci.Current, csci.Current);
            assertEquals("", sci.MoveLast(), csci.MoveLast());
            assertEquals("", sci.Current, csci.Current);

            csci.Index = 4;
            sci.Index = 4;
            ICharacterEnumerator clci = (ICharacterEnumerator)csci.Clone();
            for (int i = 0; i < 50; ++i)
            {
                assertEquals("", sci.MoveNext(), clci.MoveNext());
                assertEquals("", sci.Current, clci.Current);
            }
            for (int i = 0; i < 50; ++i)
            {
                assertEquals("", sci.MovePrevious(), clci.MovePrevious());
                assertEquals("", sci.Current, clci.Current);
            }
        }
    }
}
