﻿using ICU4N.Text;
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

        /// <summary>
        /// Adds the text key and its associated value in this object.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value associated with the text.</param>
        /// <returns></returns>
        public virtual TextTrieMap<TValue> Put(string text, TValue value)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text)); // ICU4N: Added guard clause.
            return Put(text.AsSpan(), value);
        }

        /// <summary>
        /// Adds the text key and its associated value in this object.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value associated with the text.</param>
        /// <returns></returns>
        public virtual TextTrieMap<TValue> Put(ReadOnlySpan<char> text, TValue value)
        {
            CharEnumerator chitr = new CharEnumerator(text, ignoreCase);
            root.Add(chitr, value);
            return this;
        }

        /// <summary>
        /// Gets an enumerator of the values associated with the
        /// longest prefix matching string key.
        /// </summary>
        /// <param name="text">The text to be matched with prefixes.</param>
        /// <returns>
        /// An enumerator of the values associated with
        /// the longest prefix matching matching key, or <c>null</c>
        /// if no matching entry is found.
        /// </returns>
        public virtual IEnumerator<TValue> Get(string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text)); // ICU4N: Added guard clause.

            return Get(text.AsSpan());
        }

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
        public virtual IEnumerator<TValue> Get(ReadOnlySpan<char> text)
        {
            return Get(text, out _);
        }

        // ICU4N specific: Eliminatd Get(ICharSequence text, int start, int[] matchLen) because we can slice a ReadOnlySpan<char>

        public virtual IEnumerator<TValue> Get(string text, out int matchLength)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text)); // ICU4N: Added guard clause.

            return Get(text.AsSpan(), out matchLength);
        }

        public virtual IEnumerator<TValue> Get(ReadOnlySpan<char> text, out int matchLength)
        {
            LongestMatchHandler<TValue> handler = new LongestMatchHandler<TValue>();
            Find(text, handler);
            matchLength = handler.MatchLength;
            return handler.Matches;
        }

        public virtual void Find(string text, IResultHandler<TValue> handler)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text)); // ICU4N: Added guard clause.

            Find(text.AsSpan(), handler);
        }

        public virtual void Find(ReadOnlySpan<char> text, IResultHandler<TValue> handler)
        {
            CharEnumerator chitr = new CharEnumerator(text, ignoreCase);
            Find(root, chitr, handler);
        }

        // ICU4N specific: Eliminated Find(ICharSequence text, int offset, IResultHandler<TValue> handler) because we can slice a ReadOnlySpan<char>

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

            return new ParseState(ignoreCase, root);
        }

        /// <summary>
        /// <see cref="ParseState"/> is mutable, not thread-safe, and intended to be used internally by parsers for
        /// consuming values from this trie.
        /// </summary>
        public class ParseState
        {
            private readonly bool ignoreCase;
            private Node node;
            private int offset;

            internal ParseState(bool ignoreCase, Node start)
            {
                this.ignoreCase = ignoreCase;
                node = start ?? throw new ArgumentNullException(nameof(start));
                offset = 0;
            }

            /// <summary>
            /// Consumes a code point and walk to the next node in the trie.
            /// </summary>
            /// <param name="cp">The code point to consume.</param>
            public virtual void Accept(int cp)
            {
                Debug.Assert(node != null);
                if (ignoreCase)
                {
                    cp = UChar.FoldCase(cp, true);
                }
                int count = Character.CharCount(cp);
                char ch1 = (count == 1) ? (char)cp : UTF16.GetLeadSurrogate(cp);
                node = node.TakeStep(ch1, ref offset);
                if (count == 2 && node != null)
                {
                    char ch2 = UTF16.GetTrailSurrogate(cp);
                    node = node.TakeStep(ch2, ref offset);
                }
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

        internal ref struct CharEnumerator // ICU4N: Cannot implement IEnumerator<char> here because we are a ref struct. Eliminate like in C++? Not sure why this is public in Java, since it cannot be instantiated or subclassed with a package private constructor.
        {
            private readonly bool ignoreCase;
            private readonly ReadOnlySpan<char> text;
            private int nextIdx;

            private char? remainingChar;
            private char current;

            internal CharEnumerator(ReadOnlySpan<char> text, bool ignoreCase)
            {
                this.text = text;
                nextIdx = 0;
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

            public bool MoveNext()
            {
                if (!HasNext)
                    return false;
                current = Next().Value;
                return true;
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
                    return nextIdx;
                }
            }

            public char Current => current;
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

#nullable enable

        /// <summary>
        /// Inner class representing a text node in the trie.
        /// </summary>
        public class Node
        {
            private ReadOnlyMemory<char> text;
            private object? textReference; // ICU4N: Keeps the string or char[] behind text alive for the lifetime of this class
            private IList<TValue>? values;
            internal IList<Node>? children;

            internal Node()
            {
            }

            private Node(ReadOnlyMemory<char> text, IList<TValue>? values, IList<Node>? children)
            {
                this.text = text;
                text.TryGetReference(ref this.textReference);
                this.values = values;
                this.children = children;
            }

            public int CharCount => text.Length;

            public bool HasChildFor(char ch)
            {
                for (int i = 0; children != null && i < children.Count; i++)
                {
                    ReadOnlySpan<char> childText = children[i].text.Span;
                    if (ch < childText[0]) break;
                    if (ch == childText[0])
                    {
                        return true;
                    }
                }
                return false;
            }

            public IEnumerator<TValue>? GetValues()
            {
                if (values == null)
                {
                    return null;
                }
                return values.GetEnumerator();
            }

            internal void Add(CharEnumerator chitr, TValue value) // ICU4N: Marked internal instead of public so we don't expose CharEnumerator (which has an internal constructor, anyway).
            {
                using ValueStringBuilder buf = new ValueStringBuilder(stackalloc char[32]);
                while (chitr.MoveNext())
                {
                    buf.Append(chitr.Current);
                }

                // ICU4N: Now that we know the length, allocate the memory for the node.
                Add(buf.ToString().AsMemory(), value);
            }

            internal Node? FindMatch(CharEnumerator chitr) // ICU4N: Marked internal instead of public so we don't expose CharEnumerator (which has an internal constructor, anyway).
            {
                if (children == null)
                {
                    return null;
                }
                if (!chitr.MoveNext())
                {
                    return null;
                }
                Node? match = null;
                char ch = chitr.Current;
                foreach (Node child in children)
                {
                    char childText0 = child.text.Span[0];
                    if (ch < childText0)
                    {
                        break;
                    }
                    if (ch == childText0)
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

            // ICU4N: Factored out StepResult and CreateStepResult() because we can
            // use a ref int for offset and simply return the node instead of maintaining
            // this state in a separate type.

            public Node? TakeStep(char ch, ref int offset)
            {
                Debug.Assert(offset <= CharCount);
                if (offset == CharCount)
                {
                    // Go to a child node
                    for (int i = 0; children != null && i < children.Count; i++)
                    {
                        Node child = children[i];
                        char childText0 = child.text.Span[0];
                        if (ch < childText0) break;
                        if (ch == childText0)
                        {
                            // Found a matching child node
                            offset = 1;
                            return child;
                        }
                    }
                    // No matching children; fall through
                }
                else if (text.Span[offset] == ch)
                {
                    // Return to this node; increase offset
                    offset += 1;
                    return this;
                }
                // No matches
                offset = -1;
                return null;
            }

            private void Add(ReadOnlyMemory<char> text, TValue value) // ICU4N: Removed offset, since we can slice a ReadOnlyMemory
            {
                if (text.Length == 0)
                {
                    values = AddValue(values, value);
                    return;
                }

                if (children == null)
                {
                    children = new List<Node>();
                    Node child = new Node(text, AddValue(null, value), null);
                    children.Add(child);
                    return;
                }

                // walk through children
                int index = 0;
                //bool isPrevious = false; // ICU4N: Not used
                for (; index < children.Count; index++)
                {
                    Node next = children[index];
                    char nextText0 = next.text.Span[0];
                    ReadOnlySpan<char> textSpan = text.Span;
                    if (textSpan[0] < nextText0)
                    {
                        //isPrevious = true;
                        break;
                    }
                    if (textSpan[0] == nextText0)
                    {
                        int matchLen = next.LenMatches(textSpan);
                        if (matchLen == next.text.Length)
                        {
                            // full match
                            next.Add(text.Slice(matchLen), value);
                        }
                        else
                        {
                            // partial match, create a branch
                            next.Split(matchLen);
                            next.Add(text.Slice(matchLen), value);
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

                var newNode = new Node(text, AddValue(null, value), null);
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
                ReadOnlySpan<char> textSpan = text.Span;
                while (idx < text.Length)
                {
                    if (!chitr.MoveNext())
                    {
                        matched = false;
                        break;
                    }
                    char ch = chitr.Current;
                    if (ch != textSpan[idx])
                    {
                        matched = false;
                        break;
                    }
                    idx++;
                }
                return matched;
            }

            private int LenMatches(ReadOnlySpan<char> text)
            {
                int textLen = text.Length;
                int limit = this.text.Length < textLen ? this.text.Length : textLen;
                int len = 0;
                ReadOnlySpan<char> thisText = this.text.Span;
                while (len < limit)
                {
                    if (thisText[len] != text[len])
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

                // ICU4N: Note that we don't allocate memory for the new
                // node, we simply slice the existing memory. Each node
                // holds a reference to the original underlying string
                // to keep it in scope.
                ReadOnlyMemory<char> childText = text.Slice(offset);
                text = text.Slice(0, offset);

                // add the Node representing after the offset as a child
                Node child = new Node(childText, values, children);
                values = null;

                children = new List<Node> { child };
            }

            private IList<TValue> AddValue(IList<TValue>? list, TValue value)
            {
                if (list == null)
                {
                    list = new List<TValue>();
                }
                list.Add(value);
                return list;
            }
        }

        // ICU4N: Removed ToCharArray() and replaced with StringBuilder.ToString().AsMemory()

        // ICU4N: Removed SubArray() and replaced with ReadOnlyMemory<char>.Slice()
    }
}
