using ICU4N.Text;
using J2N;
using J2N.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Callback handler for processing prefix matches used by
    /// find method.
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public interface IResultHandler<in V>
    {
        /// <summary>
        /// Handles a prefix key match.
        /// </summary>
        /// <param name="matchLength">Matched key's length.</param>
        /// <param name="values">An enumerator of the objects associated with the matched key.</param>
        /// <returns>Return <c>true</c> to continue the search in the trie, <c>false</c> to quit.</returns>
        public bool HandlePrefixMatch(int matchLength, IEnumerator<V> values);
    }

    /// <summary>
    /// <see cref="TextTrieMap{TValue}"/> is a trie implementation for supporting
    /// fast prefix match for the key.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public partial class TextTrieMap<TValue>
    {
        private readonly Node root = new Node();
        private readonly bool ignoreCase;
        private readonly object syncLock = new object(); // ICU4N specific

        /// <summary>
        /// Constructs a <see cref="TextTrieMap{TValue}"/> object.
        /// </summary>
        /// <param name="ignoreCase"><c>true</c> to use simple (ordinal) case insensitive match.</param>
        public TextTrieMap(bool ignoreCase)
        {
            this.ignoreCase = ignoreCase;
        }

        // ICU4N specific: Put(ICharSequence text, TValue val) moved to TextTrieMapExtension.tt

        /// <summary>
        /// Gets an enumerator of the objects associated with the
        /// longest prefix matching string key.
        /// </summary>
        /// <param name="text">The text to be matched with prefixes.</param>
        /// <returns>
        /// An enumerator of the objects associated with
        /// the longest prefix matching matching key, or <c>null</c>
        /// if no matching entry is found.
        /// </returns>
        public virtual IEnumerator<TValue> Get(string text)
        {
            return Get(text, 0);
        }

        // ICU4N specific: Get(ICharSequence text, int start) moved to TextTrieMapExtension.tt

        // ICU4N specific: Get(ICharSequence text, int start, int[] matchLen) moved to TextTrieMapExtension.tt

        // ICU4N specific: Find(ICharSequence text, IResultHandler<TValue> handler) moved to TextTrieMapExtension.tt

        // ICU4N specific: Find(ICharSequence text, int offset, IResultHandler<TValue> handler) moved to TextTrieMapExtension.tt

        private void Find(Node node, CharEnumerator chitr, IResultHandler<TValue> handler)
        {
            lock (syncLock)
            {
                IEnumerator<TValue> values = node.GetValues();
                if (values != null)
                {
                    if (!handler.HandlePrefixMatch(chitr.ProcessedLength, values))
                    {
                        return;
                    }
                }

                Node nextMatch = node.FindMatch(chitr);
                if (nextMatch != null)
                {
                    Find(nextMatch, chitr, handler);
                }
            }
        }

        /// <summary>
        /// Creates an object that consumes code points one at a time and returns intermediate prefix
        /// matches.  Returns null if no match exists.
        /// </summary>
        /// <param name="startingCp">The starting code point.</param>
        /// <returns>An instance of <see cref="ParseState"/>, or <c>null</c> if the starting code point is not a
        /// prefix for any entry in the trie.</returns>
        public virtual ParseState OpenParseState(int startingCp)
        {
            // Check to see whether this is a valid starting character.  If not, return null.
            if (ignoreCase)
            {
                startingCp = UChar.FoldCase(startingCp, true);
            }
            int count = Character.CharCount(startingCp);
            char ch1 = (count == 1) ? (char)startingCp : UTF16.GetLeadSurrogate(startingCp);
            if (!root.HasChildFor(ch1))
            {
                return null;
            }

            return new ParseState(this, root);
        }

        /// <summary>
        /// <see cref="ParseState"/> is mutable, not thread-safe, and intended to be used internally by parsers for
        /// consuming values from this trie.
        /// </summary>
        public class ParseState
        {
            private readonly TextTrieMap<TValue> map;
            private Node node;
            private int offset;
            private Node.StepResult result;

            internal ParseState(TextTrieMap<TValue> map, Node start)
            {
                this.map = map ?? throw new ArgumentNullException(nameof(map));
                node = start ?? throw new ArgumentNullException(nameof(start));
                offset = 0;
                result = start.CreateStepResult();
            }

            /// <summary>
            /// Consumes a code point and walk to the next node in the trie.
            /// </summary>
            /// <param name="cp">The code point to consume.</param>
            public virtual void Accept(int cp)
            {
                Debug.Assert(node != null);
                if (map.ignoreCase)
                {
                    cp = UChar.FoldCase(cp, true);
                }
                int count = Character.CharCount(cp);
                char ch1 = (count == 1) ? (char)cp : UTF16.GetLeadSurrogate(cp);
                node.TakeStep(ch1, offset, result);
                if (count == 2 && result.node != null)
                {
                    char ch2 = UTF16.GetTrailSurrogate(cp);
                    result.node.TakeStep(ch2, result.offset, result);
                }
                node = result.node;
                offset = result.offset;
            }

            /// <summary>
            /// Gets the exact prefix matches for all code points that have been consumed so far.
            /// </summary>
            /// <returns>The matches.</returns>
            public virtual IEnumerator<TValue> GetCurrentMatches()
            {
                if (node != null && offset == node.CharCount)
                {
                    return node.GetValues();
                }
                return null;
            }

            /// <summary>
            /// Gets whether any more code points can be consumed.
            /// <para/>
            /// <c>true</c> if no more code points can be consumed; <c>false</c> otherwise.
            /// </summary>
            public bool AtEnd => node == null || (node.CharCount == offset && node.children == null);
        }

        public class CharEnumerator : IEnumerator<char>
        {
            private readonly bool ignoreCase;
            private readonly ICharSequence text;
            private int nextIdx;
            private int startIdx;

            private char? remainingChar;
            private char current;

            internal CharEnumerator(ICharSequence text, int offset, bool ignoreCase)
            {
                this.text = text ?? throw new ArgumentNullException(nameof(text)); // ICU4N: Added guard clause
                nextIdx = startIdx = offset;
                this.ignoreCase = ignoreCase;
            }

            /* (non-Javadoc)
             * @see java.util.Iterator#hasNext()
             */
            private bool HasNext
            {
                get
                {
                    if (nextIdx == text.Length && remainingChar == null)
                    {
                        return false;
                    }
                    return true;
                }
            }

            /* (non-Javadoc)
             * @see java.util.Iterator#next()
             */
            private char? Next()
            {
                if (nextIdx == text.Length && remainingChar == null)
                {
                    return null;
                }
                char? next;
                if (remainingChar != null)
                {
                    next = remainingChar;
                    remainingChar = null;
                }
                else
                {
                    if (ignoreCase)
                    {
                        int cp = UChar.FoldCase(Character.CodePointAt(text, nextIdx), true);
                        nextIdx = nextIdx + Character.CharCount(cp);

                        char[] chars = Character.ToChars(cp);
                        next = chars[0];
                        if (chars.Length == 2)
                        {
                            remainingChar = chars[1];
                        }
                    }
                    else
                    {
                        next = text[nextIdx];
                        nextIdx++;
                    }
                }
                return next;
            }

            /* (non-Javadoc)
             * @see java.util.Iterator#remove()
             */
            void IEnumerator.Reset()
            {
                throw new NotSupportedException("Reset() not supported");
            }

            public bool MoveNext()
            {
                if (!HasNext)
                    return false;
                current = Next().Value;
                return true;
            }

            public void Dispose()
            {
                // Intentionally empty
            }

            public int NextIndex => nextIdx;

            public int ProcessedLength
            {
                get
                {
                    if (remainingChar != null)
                    {
                        throw new InvalidOperationException("In the middle of surrogate pair");
                    }
                    return nextIdx - startIdx;
                }
            }

            public char Current => current;

            object IEnumerator.Current => current;
        }

        // ICU4N specific - de-nested IResultHandler<V>

        private sealed class LongestMatchHandler<V> : IResultHandler<V>
        {
            private IEnumerator<V> matches = null;
            private int length = 0;

            public bool HandlePrefixMatch(int matchLength, IEnumerator<V> values)
            {
                if (matchLength > length)
                {
                    length = matchLength;
                    matches = values;
                }
                return true;
            }

            public IEnumerator<V> Matches => matches;

            public int MatchLength => length;
        }

        /// <summary>
        /// Inner class representing a text node in the trie.
        /// </summary>
        public class Node
        {
            private char[] text;
            private IList<TValue> values;
            internal IList<Node> children;

            internal Node()
            {
            }

            private Node(char[] text, IList<TValue> values, IList<Node> children) // ICU4N NOTE: all params are nullable
            {
                this.text = text;
                this.values = values;
                this.children = children;
            }

            public int CharCount => text == null ? 0 : text.Length;

            public bool HasChildFor(char ch)
            {
                for (int i = 0; children != null && i < children.Count; i++)
                {
                    Node child = children[i];
                    if (ch < child.text[0]) break;
                    if (ch == child.text[0])
                    {
                        return true;
                    }
                }
                return false;
            }

            public IEnumerator<TValue> GetValues()
            {
                if (values == null)
                {
                    return null;
                }
                return values.GetEnumerator();
            }

            public void Add(CharEnumerator chitr, TValue value)
            {
                StringBuilder buf = new StringBuilder();
                while (chitr.MoveNext())
                {
                    buf.Append(chitr.Current);
                }
                Add(ToCharArray(buf), 0, value);
            }

            public Node FindMatch(CharEnumerator chitr)
            {
                if (children == null)
                {
                    return null;
                }
                if (!chitr.MoveNext())
                {
                    return null;
                }
                Node match = null;
                char ch = chitr.Current;
                foreach (Node child in children)
                {
                    if (ch < child.text[0])
                    {
                        break;
                    }
                    if (ch == child.text[0])
                    {
                        if (child.MatchFollowing(chitr))
                        {
                            match = child;
                        }
                        break;
                    }
                }
                return match;
            }

            public class StepResult
            {
                public Node node;
                public int offset;
            }

            public virtual StepResult CreateStepResult()
            {
                return new StepResult();
            }

            public void TakeStep(char ch, int offset, StepResult result)
            {
                Debug.Assert(offset <= CharCount);
                if (offset == CharCount)
                {
                    // Go to a child node
                    for (int i = 0; children != null && i < children.Count; i++)
                    {
                        Node child = children[i];
                        if (ch < child.text[0]) break;
                        if (ch == child.text[0])
                        {
                            // Found a matching child node
                            result.node = child;
                            result.offset = 1;
                            return;
                        }
                    }
                    // No matching children; fall through
                }
                else if (text[offset] == ch)
                {
                    // Return to this node; increase offset
                    result.node = this;
                    result.offset = offset + 1;
                    return;
                }
                // No matches
                result.node = null;
                result.offset = -1;
            }

            private void Add(char[] text, int offset, TValue value)
            {
                if (text.Length == offset)
                {
                    values = AddValue(values, value);
                    return;
                }

                if (children == null)
                {
                    children = new List<Node>();
                    Node child = new Node(SubArray(text, offset), AddValue(null, value), null);
                    children.Add(child);
                    return;
                }

                // ICU4N TODO: Check this logic

                // walk through children
                int index = 0;
                bool isPrevious = false;
                for (; index < children.Count; index++)
                {
                    Node next = children[index];
                    if (text[offset] < next.text[0])
                    {
                        isPrevious = true;
                        break;
                    }
                    if (text[offset] == next.text[0])
                    {
                        int matchLen = next.LenMatches(text, offset);
                        if (matchLen == next.text.Length)
                        {
                            // full match
                            next.Add(text, offset + matchLen, value);
                        }
                        else
                        {
                            // partial match, create a branch
                            next.Split(matchLen);
                            next.Add(text, offset + matchLen, value);
                        }
                        return;
                    }
                }

                // ICU4N: According to the javadoc for ListIterator:
                // void add(E e)

                // Inserts the specified element into the list(optional operation).The element is inserted
                // immediately before the element that would be returned by next(), if any, and after the
                // element that would be returned by previous(), if any. (If the list contains no elements,
                // the new element becomes the sole element on the list.) The new element is inserted before
                // the implicit cursor: a subsequent call to next would be unaffected, and a subsequent call to
                // previous would return the new element. (This call increases by one the value that would be
                // returned by a call to nextIndex or previousIndex.)

                if (!isPrevious)
                    index = Math.Max(index - 1, 0);

                var newNode = new Node(SubArray(text, offset), AddValue(null, value), null);
                if (index != children.Count)
                {
                    children.Insert(index, newNode);
                }
                else
                {
                    children.Add(newNode);
                }
                

                //// walk through children
                //using var litr = children.GetEnumerator();
                //while (litr.MoveNext())
                //{
                //    Node next = litr.Current;
                //    if (text[offset] < next.text[0])
                //    {
                //        litr.previous();
                //        break;
                //    }
                //    if (text[offset] == next.text[0])
                //    {
                //        int matchLen = next.LenMatches(text, offset);
                //        if (matchLen == next.text.Length)
                //        {
                //            // full match
                //            next.Add(text, offset + matchLen, value);
                //        }
                //        else
                //        {
                //            // partial match, create a branch
                //            next.Split(matchLen);
                //            next.Add(text, offset + matchLen, value);
                //        }
                //        return;
                //    }
                //}
                //// add a new child to this node
                //litr.Add(new Node(SubArray(text, offset), AddValue(null, value), null));
            }

            private bool MatchFollowing(CharEnumerator chitr)
            {
                bool matched = true;
                int idx = 1;
                while (idx < text.Length)
                {
                    if (!chitr.MoveNext())
                    {
                        matched = false;
                        break;
                    }
                    char ch = chitr.Current;
                    if (ch != text[idx])
                    {
                        matched = false;
                        break;
                    }
                    idx++;
                }
                return matched;
            }

            private int LenMatches(char[] text, int offset)
            {
                int textLen = text.Length - offset;
                int limit = this.text.Length < textLen ? this.text.Length : textLen;
                int len = 0;
                while (len < limit)
                {
                    if (this.text[len] != text[offset + len])
                    {
                        break;
                    }
                    len++;
                }
                return len;
            }

            private void Split(int offset)
            {
                // split the current node at the offset
                char[] childText = SubArray(text, offset);
                text = SubArray(text, 0, offset);

                // add the Node representing after the offset as a child
                Node child = new Node(childText, values, children);
                values = null;

                children = new List<Node> { child };
            }

            private IList<TValue> AddValue(IList<TValue> list, TValue value)
            {
                if (list == null)
                {
                    list = new List<TValue>();
                }
                list.Add(value);
                return list;
            }
        }

        private static char[] ToCharArray(StringBuilder text)
        {
            char[] array = new char[text.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = text[i];
            }
            return array;
        }

        private static char[] SubArray(char[] array, int start)
        {
            if (start == 0)
            {
                return array;
            }
            char[] sub = new char[array.Length - start];
            Array.Copy(array, start, sub, 0, sub.Length);
            return sub;
        }

        private static char[] SubArray(char[] array, int start, int limit) // ICU4N TODO: Change limit to length
        {
            if (start == 0 && limit == array.Length)
            {
                return array;
            }
            char[] sub = new char[limit - start];
            Array.Copy(array, start, sub, 0, limit - start);
            return sub;
        }
    }
}
