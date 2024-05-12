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

            ReadOnlyMemory<char> cs = text.AsMemory();
            CharacterIterator csci = new ReadOnlyMemoryCharacterIterator(cs);
            CharacterIterator sci = new StringCharacterIterator(text);

            assertEquals("", sci.SetIndex(6), csci.SetIndex(6));
            assertEquals("", sci.Index, csci.Index);
            assertEquals("", sci.Current, csci.Current);
            assertEquals("", sci.Previous(), csci.Previous());
            assertEquals("", sci.Next(), csci.Next());
            assertEquals("", sci.BeginIndex, csci.BeginIndex);
            assertEquals("", sci.EndIndex, csci.EndIndex);
            assertEquals("", sci.First(), csci.First());
            assertEquals("", sci.Last(), csci.Last());

            csci.SetIndex(4);
            sci.SetIndex(4);
            CharacterIterator clci = (CharacterIterator)csci.Clone();
            for (int i = 0; i < 50; ++i)
            {
                assertEquals("", sci.Next(), clci.Next());
            }
            for (int i = 0; i < 50; ++i)
            {
                assertEquals("", sci.Previous(), clci.Previous());
            }
        }
    }
}
