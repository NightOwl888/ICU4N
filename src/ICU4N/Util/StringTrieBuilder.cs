using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Util
{
    /// <summary>
    /// Base class for string trie builder classes.
    /// <para/>
    /// This class is not intended for public subclassing.
    /// </summary>
    /// <author>Markus W. Scherer</author>
    /// <stable>ICU 4.8</stable>
    public abstract class StringTrieBuilder
    {
        /// <summary>
        /// Build options for <see cref="BytesTrieBuilder"/> and <see cref="CharsTrieBuilder"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public enum Option // ICU4N TODO: API - de-nest
        {
            /// <summary>
            /// Builds a trie quickly.
            /// </summary>
            /// <stable>ICU 4.8</stable>
            Fast,
            /// <summary>
            /// Builds a trie more slowly, attempting to generate
            /// a shorter but equivalent serialization.
            /// This build option also uses more memory.
            /// <para/>
            /// This option can be effective when many integer values are the same
            /// and string/byte sequence suffixes can be shared.
            /// Runtime speed is not expected to improve.
            /// </summary>
            /// <stable>ICU 4.8</stable>
            Small
        }


        [Obsolete("This API is ICU internal only.")]
        protected StringTrieBuilder() { }


        [Obsolete("This API is ICU internal only.")]
        protected virtual void AddImpl(ICharSequence s, int value)
        {
            if (state != State.Adding)
            {
                // Cannot add elements after building.
                throw new InvalidOperationException("Cannot add (string, value) pairs after build().");
            }
            if (s.Length > 0xffff)
            {
                // Too long: Limited by iterator internals, and by builder recursion depth.
                throw new IndexOutOfRangeException("The maximum string length is 0xffff.");
            }
            if (root == null)
            {
                root = CreateSuffixNode(s, 0, value);
            }
            else
            {
                root = root.Add(this, s, 0, value);
            }
        }

        [Obsolete("This API is ICU internal only.")]
        protected void BuildImpl(Option buildOption)
        {
            switch (state)
            {
                case State.Adding:
                    if (root == null)
                    {
                        throw new IndexOutOfRangeException("No (string, value) pairs were added.");
                    }
                    if (buildOption == Option.Fast)
                    {
                        state = State.BuildingFast;
                        // Building "fast" is somewhat faster (25..50% in some test)
                        // because it makes registerNode() return the input node
                        // rather than checking for duplicates.
                        // As a result, we sometimes write larger trie serializations.
                        //
                        // In either case we need to fix-up linear-match nodes (for their maximum length)
                        // and branch nodes (turning dynamic branch nodes into trees of
                        // runtime-equivalent nodes), but the HashMap/hashCode()/equals() are omitted for
                        // nodes other than final values.
                    }
                    else
                    {
                        state = State.BuildingSmall;
                    }
                    break;
                case State.BuildingFast:
                case State.BuildingSmall:
                    // Building must have failed.
                    throw new InvalidOperationException("Builder failed and must be clear()ed.");
                case State.Built:
                    return;  // Nothing more to do.
            }
            // Implementation note:
            // We really build three versions of the trie.
            // The first is a fully dynamic trie, built successively by addImpl().
            // Then we call root.register() to turn it into a tree of nodes
            // which is 1:1 equivalent to the runtime data structure.
            // Finally, root.markRightEdgesFirst() and root.write() write that serialized form.
            root = root.Register(this);
            root.MarkRightEdgesFirst(-1);
            root.Write(this);
            state = State.Built;
        }

        [Obsolete("This API is ICU internal only.")]
        protected virtual void ClearImpl()
        {
            strings.Length = 0;
            nodes.Clear();
            root = null;
            state = State.Adding;
        }

        /// <summary>
        /// Makes sure that there is only one unique node registered that is
        /// equivalent to <paramref name="newNode"/>, unless <see cref="State.BuildingFast"/>.
        /// </summary>
        /// <param name="newNode">Input node. The builder takes ownership.</param>
        /// <returns><paramref name="newNode"/> if it is the first of its kind, or
        /// an equivalent node if <paramref name="newNode"/> is a duplicate.</returns>
        private Node RegisterNode(Node newNode)
        {
            if (state == State.BuildingFast)
            {
                return newNode;
            }
            // BUILDING_SMALL
            Node oldNode = nodes.Get(newNode);
            if (oldNode != null)
            {
                return oldNode;
            }
            // If put() returns a non-null value from an equivalent, previously
            // registered node, then get() failed to find that and we will leak newNode.
            nodes.TryGetValue(newNode, out oldNode);
            nodes[newNode] = newNode;
            Debug.Assert(oldNode == null);
            return newNode;
        }

        /// <summary>
        /// Makes sure that there is only one unique <see cref="ValueNode"/> registered
        /// with this value.
        /// Avoids creating a node if the value is a duplicate.
        /// </summary>
        /// <param name="value">A final value.</param>
        /// <returns>A <see cref="ValueNode"/> with the given value.</returns>
        private ValueNode RegisterFinalValue(int value)
        {
            // We always register final values because while ADDING
            // we do not know yet whether we will build fast or small.
            lookupFinalValueNode.SetFinalValue(value);
            Node oldNode = nodes.Get(lookupFinalValueNode);
            if (oldNode != null)
            {
                return (ValueNode)oldNode;
            }
            ValueNode newNode = new ValueNode(value);
            // If put() returns a non-null value from an equivalent, previously
            // registered node, then get() failed to find that and we will leak newNode.
            nodes.TryGetValue(newNode, out oldNode);
            nodes[newNode] = newNode;
            Debug.Assert(oldNode == null);
            return newNode;
        }

        private abstract class Node
        {
            public Node()
            {
                offset = 0;
            }
            // hashCode() and equals() for use with registerNode() and the nodes hash.
            public override abstract int GetHashCode() /*const*/;
            // Base class equals() compares the actual class types.

            public override bool Equals(object other)
            {
                return this == other || this.GetType() == other.GetType();
            }

            /// <summary>
            /// Recursive method for adding a new (string, value) pair.
            /// Matches the remaining part of <paramref name="s"/> from <paramref name="start"/>,
            /// and adds a new node where there is a mismatch.
            /// </summary>
            /// <returns>This or a replacement <see cref="Node"/>.</returns>
            public virtual Node Add(StringTrieBuilder builder, ICharSequence s, int start, int sValue)
            {
                return this;
            }

            /// <summary>
            /// Recursive method for registering unique nodes,
            /// after all (string, value) pairs have been added.
            /// Final-value nodes are pre-registered while 
            /// <see cref="Add(StringTrieBuilder, ICharSequence, int, int)"/>ing 
            /// (string, value) pairs. Other nodes created while 
            /// <see cref="Add(StringTrieBuilder, ICharSequence, int, int)"/>ing 
            /// <see cref="RegisterNode(Node)"/> themselves later and might replace 
            /// themselves with new types of nodes for <see cref="Write(StringTrieBuilder)"/>ing.
            /// </summary>
            /// <returns>The registered version of this node which implements <see cref="Write(StringTrieBuilder)"/>.</returns>
            public virtual Node Register(StringTrieBuilder builder) { return this; }
            /// <summary>
            /// Traverses the <see cref="Node"/> graph and numbers branch edges, with rightmost edges first.
            /// This is to avoid writing a duplicate node twice.
            /// </summary>
            /// <remarks>
            /// Branch nodes in this trie data structure are not symmetric.
            /// Most branch edges "jump" to other nodes but the rightmost branch edges
            /// just continue without a jump.
            /// Therefore, <see cref="Write(StringTrieBuilder)"/> must write the rightmost branch edge last
            /// (trie units are written backwards), and must write it at that point even if
            /// it is a duplicate of a node previously written elsewhere.
            /// <para/>
            /// This function visits and marks right branch edges first.
            /// Edges are numbered with increasingly negative values because we share the
            /// offset field which gets positive values when nodes are written.
            /// A branch edge also remembers the first number for any of its edges.
            /// <para/>
            /// When a further-left branch edge has a number in the range of the rightmost
            /// edge's numbers, then it will be written as part of the required right edge
            /// and we can avoid writing it first.
            /// <para/>
            /// After root.MarkRightEdgesFirst(-1) the offsets of all nodes are negative
            /// edge numbers.
            /// </remarks>
            /// <param name="edgeNumber">The first edge number for this node and its sub-nodes.</param>
            /// <returns>An edge number that is at least the maximum-negative
            /// of the input edge number and the numbers of this node and all of its sub-nodes.</returns>
            public virtual int MarkRightEdgesFirst(int edgeNumber)
            {
                if (offset == 0)
                {
                    offset = edgeNumber;
                }
                return edgeNumber;
            }
            // Write() must set the offset to a positive value.
            public abstract void Write(StringTrieBuilder builder);
            // See MarkRightEdgesFirst.
            public void WriteUnlessInsideRightEdge(int firstRight, int lastRight,
                                                   StringTrieBuilder builder)
            {
                // Note: Edge numbers are negative, lastRight<=firstRight.
                // If offset>0 then this node and its sub-nodes have been written already
                // and we need not write them again.
                // If this node is part of the unwritten right branch edge,
                // then we wait until that is written.
                if (offset < 0 && (offset < lastRight || firstRight < offset))
                {
                    Write(builder);
                }
            }
            public int Offset /*const*/ { get { return offset; } }

            protected int offset;
        }

        // Used directly for final values, and as as a superclass for
        // match nodes with intermediate values.
        private class ValueNode : Node
        {
            public ValueNode() { }
            public ValueNode(int v)
            {
                hasValue = true;
                value = v;
            }
            public void SetValue(int v)
            {
                Debug.Assert(!hasValue);
                hasValue = true;
                value = v;
            }
            internal void SetFinalValue(int v)
            {
                hasValue = true;
                value = v;
            }

            public override int GetHashCode() /*const*/
            {
                int hash = 0x111111;
                if (hasValue)
                {
                    hash = hash * 37 + value;
                }
                return hash;
            }

            public override bool Equals(object other)
            {
                if (this == other)
                {
                    return true;
                }
                if (!base.Equals(other))
                {
                    return false;
                }
                ValueNode o = (ValueNode)other;
                return hasValue == o.hasValue && (!hasValue || value == o.value);
            }

            public override Node Add(StringTrieBuilder builder, ICharSequence s, int start, int sValue)
            {
                if (start == s.Length)
                {
                    throw new ArgumentException("Duplicate string.");
                }
                // Replace self with a node for the remaining string suffix and value.
                ValueNode node = builder.CreateSuffixNode(s, start, sValue);
                node.SetValue(value);
                return node;
            }

            public override void Write(StringTrieBuilder builder)
            {
#pragma warning disable 612, 618
                offset = builder.WriteValueAndFinal(value, true);
#pragma warning restore 612, 618
            }

            protected bool hasValue;
            protected int value;

            internal int Value { get { return value; } }
        }

        private sealed class IntermediateValueNode : ValueNode
        {
            public IntermediateValueNode(int v, Node nextNode)
            {
                next = nextNode;
                SetValue(v);
            }

            public override int GetHashCode() /*const*/
            {
                return (0x222222 * 37 + value) * 37 + next.GetHashCode();
            }

            public override bool Equals(object other)
            {
                if (this == other)
                {
                    return true;
                }
                if (!base.Equals(other))
                {
                    return false;
                }
                IntermediateValueNode o = (IntermediateValueNode)other;
                return next == o.next;
            }

            public override int MarkRightEdgesFirst(int edgeNumber)
            {
                if (offset == 0)
                {
                    offset = edgeNumber = next.MarkRightEdgesFirst(edgeNumber);
                }
                return edgeNumber;
            }

            public override void Write(StringTrieBuilder builder)
            {
                next.Write(builder);
#pragma warning disable 612, 618
                offset = builder.WriteValueAndFinal(value, false);
#pragma warning restore 612, 618
            }

            private Node next;
        }

        private sealed class LinearMatchNode : ValueNode
        {
            public LinearMatchNode(ICharSequence builderStrings, int sOffset, int len, Node nextNode)
            {
                strings = builderStrings;
                stringOffset = sOffset;
                length = len;
                next = nextNode;
            }

            public override int GetHashCode() /*const*/ { return hash; }

            public override bool Equals(object other)
            {
                if (this == other)
                {
                    return true;
                }
                if (!base.Equals(other))
                {
                    return false;
                }
                LinearMatchNode o = (LinearMatchNode)other;
                if (length != o.length || next != o.next)
                {
                    return false;
                }
                for (int i = stringOffset, j = o.stringOffset, limit = stringOffset + length; i < limit; ++i, ++j)
                {
                    if (strings[i] != strings[j])
                    {
                        return false;
                    }
                }
                return true;
            }

            public override Node Add(StringTrieBuilder builder, ICharSequence s, int start, int sValue)
            {
                if (start == s.Length)
                {
                    if (hasValue)
                    {
                        throw new ArgumentException("Duplicate string.");
                    }
                    else
                    {
                        SetValue(sValue);
                        return this;
                    }
                }
                int limit = stringOffset + length;
                for (int i = stringOffset; i < limit; ++i, ++start)
                {
                    if (start == s.Length)
                    {
                        // s is a prefix with a new value. Split self into two linear-match nodes.
                        int prefixLength = i - stringOffset;
                        LinearMatchNode suffixNode = new LinearMatchNode(strings, i, length - prefixLength, next);
                        suffixNode.SetValue(sValue);
                        length = prefixLength;
                        next = suffixNode;
                        return this;
                    }
                    char thisChar = strings[i];
                    char newChar = s[start];
                    if (thisChar != newChar)
                    {
                        // Mismatch, insert a branch node.
                        DynamicBranchNode branchNode = new DynamicBranchNode();
                        // Reuse this node for one of the remaining substrings, if any.
                        Node result, thisSuffixNode;
                        if (i == stringOffset)
                        {
                            // Mismatch on first character, turn this node into a suffix.
                            if (hasValue)
                            {
                                // Move the value for prefix length "start" to the new node.
                                branchNode.SetValue(value);
                                value = 0;
                                hasValue = false;
                            }
                            ++stringOffset;
                            --length;
                            thisSuffixNode = length > 0 ? this : next;
                            // C++: if(length==0) { delete this; }
                            result = branchNode;
                        }
                        else if (i == limit - 1)
                        {
                            // Mismatch on last character, keep this node for the prefix.
                            --length;
                            thisSuffixNode = next;
                            next = branchNode;
                            result = this;
                        }
                        else
                        {
                            // Mismatch on intermediate character, keep this node for the prefix.
                            int prefixLength = i - stringOffset;
                            ++i;  // Suffix start offset (after thisChar).
                            thisSuffixNode = new LinearMatchNode(
                                    strings, i, length - (prefixLength + 1), next);
                            length = prefixLength;
                            next = branchNode;
                            result = this;
                        }
                        ValueNode newSuffixNode = builder.CreateSuffixNode(s, start + 1, sValue);
                        branchNode.Add(thisChar, thisSuffixNode);
                        branchNode.Add(newChar, newSuffixNode);
                        return result;
                    }
                }
                // s matches all of this node's characters.
                next = next.Add(builder, s, start, sValue);
                return this;
            }

            public override Node Register(StringTrieBuilder builder)
            {
                next = next.Register(builder);
                // Break the linear-match sequence into chunks of at most kMaxLinearMatchLength.
#pragma warning disable 612, 618
                int maxLinearMatchLength = builder.MaxLinearMatchLength;
#pragma warning restore 612, 618
                while (length > maxLinearMatchLength)
                {
                    int nextOffset = stringOffset + length - maxLinearMatchLength;
                    length -= maxLinearMatchLength;
                    LinearMatchNode suffixNode =
                        new LinearMatchNode(strings, nextOffset, maxLinearMatchLength, next);
                    suffixNode.SetHashCode();
                    next = builder.RegisterNode(suffixNode);
                }
                Node result;
#pragma warning disable 612, 618
                if (hasValue && !builder.MatchNodesCanHaveValues)
#pragma warning restore 612, 618
                {
                    int intermediateValue = value;
                    value = 0;
                    hasValue = false;
                    SetHashCode();
                    result = new IntermediateValueNode(intermediateValue, builder.RegisterNode(this));
                }
                else
                {
                    SetHashCode();
                    result = this;
                }
                return builder.RegisterNode(result);
            }

            public override int MarkRightEdgesFirst(int edgeNumber)
            {
                if (offset == 0)
                {
                    offset = edgeNumber = next.MarkRightEdgesFirst(edgeNumber);
                }
                return edgeNumber;
            }

            public override void Write(StringTrieBuilder builder)
            {
                next.Write(builder);
#pragma warning disable 612, 618
                builder.Write(stringOffset, length);
                offset = builder.WriteValueAndType(hasValue, value, builder.MinLinearMatch + length - 1);
#pragma warning restore 612, 618
            }

            // Must be called just before registerNode(this).
            private void SetHashCode() /*const*/
            {
                hash = (0x333333 * 37 + length) * 37 + next.GetHashCode();
                if (hasValue)
                {
                    hash = hash * 37 + value;
                }
                for (int i = stringOffset, limit = stringOffset + length; i < limit; ++i)
                {
                    hash = hash * 37 + strings[i];
                }
            }

            private ICharSequence strings;
            private int stringOffset;
            private int length;
            private Node next;
            private int hash;
        }

        private sealed class DynamicBranchNode : ValueNode
        {
            public DynamicBranchNode() { }
            // c must not be in chars yet.
            public void Add(char c, Node node)
            {
                int i = Find(c);
                chars.Insert(i, c);
                equal.Insert(i, node);
            }

            public override Node Add(StringTrieBuilder builder, ICharSequence s, int start, int sValue)
            {
                if (start == s.Length)
                {
                    if (hasValue)
                    {
                        throw new ArgumentException("Duplicate string.");
                    }
                    else
                    {
                        SetValue(sValue);
                        return this;
                    }
                }
                char c = s[start++];
                int i = Find(c);
                if (i < chars.Length && c == chars[i])
                {
                    equal[i] = equal[i].Add(builder, s, start, sValue);
                }
                else
                {
                    chars.Insert(i, c);
                    equal.Insert(i, builder.CreateSuffixNode(s, start, sValue));
                }
                return this;
            }

            public override Node Register(StringTrieBuilder builder)
            {
                Node subNode = Register(builder, 0, chars.Length);
                BranchHeadNode head = new BranchHeadNode(chars.Length, subNode);
                Node result = head;
                if (hasValue)
                {
#pragma warning disable 612, 618
                    if (builder.MatchNodesCanHaveValues)
#pragma warning restore 612, 618
                    {
                        head.SetValue(value);
                    }
                    else
                    {
                        result = new IntermediateValueNode(value, builder.RegisterNode(head));
                    }
                }
                return builder.RegisterNode(result);
            }
            private Node Register(StringTrieBuilder builder, int start, int limit)
            {
                int length = limit - start;
#pragma warning disable 612, 618
                if (length > builder.MaxBranchLinearSubNodeLength)
#pragma warning restore 612, 618
                {
                    // Branch on the middle unit.
                    int middle = start + length / 2;
                    return builder.RegisterNode(
                            new SplitBranchNode(
                                    chars[middle],
                                    Register(builder, start, middle),
                                    Register(builder, middle, limit)));
                }
                ListBranchNode listNode = new ListBranchNode(length);
                do
                {
                    char c = chars[start];
                    Node node = equal[start];
                    if (node.GetType() == typeof(ValueNode))
                    {
                        // Final value.
                        listNode.Add(c, ((ValueNode)node).Value);
                    }
                    else
                    {
                        listNode.Add(c, node.Register(builder));
                    }
                } while (++start < limit);
                return builder.RegisterNode(listNode);
            }

            private int Find(char c)
            {
                int start = 0;
                int limit = chars.Length;
                while (start < limit)
                {
                    int i = (start + limit) / 2;
                    char middleChar = chars[i];
                    if (c < middleChar)
                    {
                        limit = i;
                    }
                    else if (c == middleChar)
                    {
                        return i;
                    }
                    else
                    {
                        start = i + 1;
                    }
                }
                return start;
            }

            private StringBuilder chars = new StringBuilder();
            private List<Node> equal = new List<Node>();
        }

        private abstract class BranchNode : Node
        {
            public BranchNode() { }

            public override int GetHashCode() /*const*/ { return hash; }

            protected int hash;
            protected int firstEdgeNumber;
        }

        private sealed class ListBranchNode : BranchNode
        {
            public ListBranchNode(int capacity)
            {
                hash = 0x444444 * 37 + capacity;
                equal = new Node[capacity];
                values = new int[capacity];
                units = new char[capacity];
            }

            public override bool Equals(object other)
            {
                if (this == other)
                {
                    return true;
                }
                if (!base.Equals(other))
                {
                    return false;
                }
                ListBranchNode o = (ListBranchNode)other;
                for (int i = 0; i < length; ++i)
                {
                    if (units[i] != o.units[i] || values[i] != o.values[i] || equal[i] != o.equal[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override int MarkRightEdgesFirst(int edgeNumber)
            {
                if (offset == 0)
                {
                    firstEdgeNumber = edgeNumber;
                    int step = 0;
                    int i = length;
                    do
                    {
                        Node edge = equal[--i];
                        if (edge != null)
                        {
                            edgeNumber = edge.MarkRightEdgesFirst(edgeNumber - step);
                        }
                        // For all but the rightmost edge, decrement the edge number.
                        step = 1;
                    } while (i > 0);
                    offset = edgeNumber;
                }
                return edgeNumber;
            }

            public override void Write(StringTrieBuilder builder)
            {
                // Write the sub-nodes in reverse order: The jump lengths are deltas from
                // after their own positions, so if we wrote the minUnit sub-node first,
                // then its jump delta would be larger.
                // Instead we write the minUnit sub-node last, for a shorter delta.
                int unitNumber = length - 1;
                Node rightEdge = equal[unitNumber];
                int rightEdgeNumber = rightEdge == null ? firstEdgeNumber : rightEdge.Offset;
                do
                {
                    --unitNumber;
                    if (equal[unitNumber] != null)
                    {
                        equal[unitNumber].WriteUnlessInsideRightEdge(firstEdgeNumber, rightEdgeNumber, builder);
                    }
                } while (unitNumber > 0);
                // The maxUnit sub-node is written as the very last one because we do
                // not jump for it at all.
                unitNumber = length - 1;
                if (rightEdge == null)
                {
#pragma warning disable 612, 618
                    builder.WriteValueAndFinal(values[unitNumber], true);
#pragma warning restore 612, 618
                }
                else
                {
                    rightEdge.Write(builder);
                }
#pragma warning disable 612, 618
                offset = builder.Write(units[unitNumber]);
#pragma warning restore 612, 618
                // Write the rest of this node's unit-value pairs.
                while (--unitNumber >= 0)
                {
                    int value;
                    bool isFinal;
                    if (equal[unitNumber] == null)
                    {
                        // Write the final value for the one string ending with this unit.
                        value = values[unitNumber];
                        isFinal = true;
                    }
                    else
                    {
                        // Write the delta to the start position of the sub-node.
                        Debug.Assert(equal[unitNumber].Offset > 0);
                        value = offset - equal[unitNumber].Offset;
                        isFinal = false;
                    }
#pragma warning disable 612, 618
                    builder.WriteValueAndFinal(value, isFinal);
                    offset = builder.Write(units[unitNumber]);
#pragma warning restore 612, 618
                }
            }
            // Adds a unit with a final value.
            public void Add(int c, int value)
            {
                units[length] = (char)c;
                equal[length] = null;
                values[length] = value;
                ++length;
                hash = (hash * 37 + c) * 37 + value;
            }
            // Adds a unit which leads to another match node.
            public void Add(int c, Node node)
            {
                units[length] = (char)c;
                equal[length] = node;
                values[length] = 0;
                ++length;
                hash = (hash * 37 + c) * 37 + node.GetHashCode();
            }

            // Note: We could try to reduce memory allocations
            // by replacing these per-node arrays with per-builder ArrayLists and
            // (for units) a StringBuilder (or even use its strings for the units too).
            // It remains to be seen whether that would improve performance.
            private Node[] equal;  // null means "has final value".
            private int length;
            private int[] values;
            private char[] units;
        }

        private sealed class SplitBranchNode : BranchNode
        {
            public SplitBranchNode(char middleUnit, Node lessThanNode, Node greaterOrEqualNode)
            {
                hash = ((0x555555 * 37 + middleUnit) * 37 +
                        lessThanNode.GetHashCode()) * 37 + greaterOrEqualNode.GetHashCode();
                unit = middleUnit;
                lessThan = lessThanNode;
                greaterOrEqual = greaterOrEqualNode;
            }

            public override bool Equals(object other)
            {
                if (this == other)
                {
                    return true;
                }
                if (!base.Equals(other))
                {
                    return false;
                }
                SplitBranchNode o = (SplitBranchNode)other;
                return unit == o.unit && lessThan == o.lessThan && greaterOrEqual == o.greaterOrEqual;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override int MarkRightEdgesFirst(int edgeNumber)
            {
                if (offset == 0)
                {
                    firstEdgeNumber = edgeNumber;
                    edgeNumber = greaterOrEqual.MarkRightEdgesFirst(edgeNumber);
                    offset = edgeNumber = lessThan.MarkRightEdgesFirst(edgeNumber - 1);
                }
                return edgeNumber;
            }

            public override void Write(StringTrieBuilder builder)
            {
                // Encode the less-than branch first.
                lessThan.WriteUnlessInsideRightEdge(firstEdgeNumber, greaterOrEqual.Offset, builder);
                // Encode the greater-or-equal branch last because we do not jump for it at all.
                greaterOrEqual.Write(builder);
                // Write this node.
                Debug.Assert(lessThan.Offset > 0);
#pragma warning disable 612, 618
                builder.WriteDeltaTo(lessThan.Offset);  // less-than
                offset = builder.Write(unit);
#pragma warning restore 612, 618
            }

            private char unit;
            private Node lessThan;
            private Node greaterOrEqual;
        }

        // Branch head node, for writing the actual node lead unit.
        private sealed class BranchHeadNode : ValueNode
        {
            public BranchHeadNode(int len, Node subNode)
            {
                length = len;
                next = subNode;
            }

            public override int GetHashCode() /*const*/
            {
                return (0x666666 * 37 + length) * 37 + next.GetHashCode();
            }

            public override bool Equals(Object other)
            {
                if (this == other)
                {
                    return true;
                }
                if (!base.Equals(other))
                {
                    return false;
                }
                BranchHeadNode o = (BranchHeadNode)other;
                return length == o.length && next == o.next;
            }

            public override int MarkRightEdgesFirst(int edgeNumber)
            {
                if (offset == 0)
                {
                    offset = edgeNumber = next.MarkRightEdgesFirst(edgeNumber);
                }
                return edgeNumber;
            }

            public override void Write(StringTrieBuilder builder)
            {
                next.Write(builder);
#pragma warning disable 612, 618
                if (length <= builder.MinLinearMatch)
                {
                    offset = builder.WriteValueAndType(hasValue, value, length - 1);
                }
                else
                {
                    builder.Write(length - 1);
                    offset = builder.WriteValueAndType(hasValue, value, 0);
#pragma warning restore 612, 618
                }
            }

            private int length;
            private Node next;  // A branch sub-node.
        }

        private ValueNode CreateSuffixNode(ICharSequence s, int start, int sValue)
        {
            ValueNode node = RegisterFinalValue(sValue);
            if (start < s.Length)
            {
#pragma warning disable 612, 618
                int offset = strings.Length;
                strings.Append(s, start, s.Length - start); // ICU4N: Corrected 3rd parameter
                node = new LinearMatchNode(strings.ToCharSequence(), offset, s.Length - start, node);
#pragma warning restore 612, 618
            }
            return node;
        }

        [Obsolete("This API is ICU internal only.")]
        protected abstract bool MatchNodesCanHaveValues { get; } /*const*/

        [Obsolete("This API is ICU internal only.")]
        protected abstract int MaxBranchLinearSubNodeLength { get; } /*const*/
        [Obsolete("This API is ICU internal only.")]
        protected abstract int MinLinearMatch { get; } /*const*/
        [Obsolete("This API is ICU internal only.")]
        protected abstract int MaxLinearMatchLength { get; } /*const*/

        [Obsolete("This API is ICU internal only.")]
        protected abstract int Write(int unit);
        [Obsolete("This API is ICU internal only.")]
        protected abstract int Write(int offset, int length);
        [Obsolete("This API is ICU internal only.")]
        protected abstract int WriteValueAndFinal(int i, bool isFinal);
        [Obsolete("This API is ICU internal only.")]
        protected abstract int WriteValueAndType(bool hasValue, int value, int node);
        [Obsolete("This API is ICU internal only.")]
        protected abstract int WriteDeltaTo(int jumpTarget);

        private enum State
        {
            Adding, BuildingFast, BuildingSmall, Built
        }
        private State state = State.Adding;

        // Strings and sub-strings for linear-match nodes.
        [Obsolete("This API is ICU internal only.")]
        protected StringBuilder strings = new StringBuilder();
        private Node root;

        // Hash set of nodes, maps from nodes to integer 1.
        private Dictionary<Node, Node> nodes = new Dictionary<Node, Node>();
        private ValueNode lookupFinalValueNode = new ValueNode();
    }
}
