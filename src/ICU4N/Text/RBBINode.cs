using ICU4N.Impl;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ICU4N.Text
{
    /// <summary>
    /// This class represents a node in the parse tree created by the RBBI Rule compiler.
    /// </summary>
    internal class RBBINode
    {
        //   enum NodeType {
        internal static readonly int setRef = 0;
        internal static readonly int uset = 1;
        internal static readonly int varRef = 2;
        internal static readonly int leafChar = 3;
        internal static readonly int lookAhead = 4;
        internal static readonly int tag = 5;
        internal static readonly int endMark = 6;
        internal static readonly int opStart = 7;
        internal static readonly int opCat = 8;
        internal static readonly int opOr = 9;
        internal static readonly int opStar = 10;
        internal static readonly int opPlus = 11;
        internal static readonly int opQuestion = 12;
        internal static readonly int opBreak = 13;
        internal static readonly int opReverse = 14;
        internal static readonly int opLParen = 15;
        internal static readonly int nodeTypeLimit = 16;    //  For Assertion checking only.

        internal static readonly string[] nodeTypeNames = {
            "setRef",
            "uset",
            "varRef",
            "leafChar",
            "lookAhead",
            "tag",
            "endMark",
            "opStart",
            "opCat",
            "opOr",
            "opStar",
            "opPlus",
            "opQuestion",
            "opBreak",
            "opReverse",
            "opLParen"
        };

        //    enum OpPrecedence {
        internal static readonly int precZero = 0;
        internal static readonly int precStart = 1;
        internal static readonly int precLParen = 2;
        internal static readonly int precOpOr = 3;
        internal static readonly int precOpCat = 4;

        internal int fType;   // enum NodeType
        internal RBBINode fParent;
        internal RBBINode fLeftChild;
        internal RBBINode fRightChild;
        internal UnicodeSet fInputSet;           // For uset nodes only.
        internal int fPrecedence = precZero;   // enum OpPrecedence, For binary ops only.

        internal string fText;                 // Text corresponding to this node.
                                               //   May be lazily evaluated when (if) needed
                                               //   for some node types.
        internal int fFirstPos;            // Position in the rule source string of the
                                           //   first text associated with the node.
                                           //   If there's a left child, this will be the same
                                           //   as that child's left pos.
        internal int fLastPos;             //  Last position in the rule source string
                                           //    of any text associated with this node.
                                           //    If there's a right child, this will be the same
                                           //    as that child's last postion.

        internal bool fNullable;            //  See Aho DFA table generation algorithm
        internal int fVal;                 // For leafChar nodes, the value.
                                           //   Values are the character category,
                                           //   corresponds to columns in the final
                                           //   state transition table.

        internal bool fLookAheadEnd;        // For endMark nodes, set TRUE if
                                            //   marking the end of a look-ahead rule.

        internal bool fRuleRoot;             // True if this node is the root of a rule.
        internal bool fChainIn;              // True if chaining into this rule is allowed
                                             //     (no '^' present).


        internal ISet<RBBINode> fFirstPosSet;         // See Aho DFA table generation algorithm
        internal ISet<RBBINode> fLastPosSet;          // See Aho.
        internal ISet<RBBINode> fFollowPos;           // See Aho.

        internal int fSerialNum;           //  Debugging aids.  Each node gets a unique serial number.
        internal static int gLastSerial;

        internal RBBINode(int t)
        {
            Assert.Assrt(t < nodeTypeLimit);
            fSerialNum = ++gLastSerial;
            fType = t;

            fFirstPosSet = new HashSet<RBBINode>();
            fLastPosSet = new HashSet<RBBINode>();
            fFollowPos = new HashSet<RBBINode>();
            if (t == opCat)
            {
                fPrecedence = precOpCat;
            }
            else if (t == opOr)
            {
                fPrecedence = precOpOr;
            }
            else if (t == opStart)
            {
                fPrecedence = precStart;
            }
            else if (t == opLParen)
            {
                fPrecedence = precLParen;
            }
            else
            {
                fPrecedence = precZero;
            }
        }

        internal RBBINode(RBBINode other)
        {
            fSerialNum = ++gLastSerial;
            fType = other.fType;
            fInputSet = other.fInputSet;
            fPrecedence = other.fPrecedence;
            fText = other.fText;
            fFirstPos = other.fFirstPos;
            fLastPos = other.fLastPos;
            fNullable = other.fNullable;
            fVal = other.fVal;
            fRuleRoot = false;
            fChainIn = other.fChainIn;
            fFirstPosSet = new HashSet<RBBINode>(other.fFirstPosSet);
            fLastPosSet = new HashSet<RBBINode>(other.fLastPosSet);
            fFollowPos = new HashSet<RBBINode>(other.fFollowPos);
        }

        //-------------------------------------------------------------------------
        //
        //        cloneTree Make a copy of the subtree rooted at this node.
        //                      Discard any variable references encountered along the way,
        //                      and replace with copies of the variable's definitions.
        //                      Used to replicate the expression underneath variable
        //                      references in preparation for generating the DFA tables.
        //
        //-------------------------------------------------------------------------
        internal virtual RBBINode CloneTree()
        {
            RBBINode n;

            if (fType == RBBINode.varRef)
            {
                // If the current node is a variable reference, skip over it
                //   and clone the definition of the variable instead.
                n = fLeftChild.CloneTree();
            }
            else if (fType == RBBINode.uset)
            {
                n = this;
            }
            else
            {
                n = new RBBINode(this);
                if (fLeftChild != null)
                {
                    n.fLeftChild = fLeftChild.CloneTree();
                    n.fLeftChild.fParent = n;
                }
                if (fRightChild != null)
                {
                    n.fRightChild = fRightChild.CloneTree();
                    n.fRightChild.fParent = n;
                }
            }
            return n;
        }



        //-------------------------------------------------------------------------
        //
        //       flattenVariables Walk a parse tree, replacing any variable
        //                          references with a copy of the variable's definition.
        //                          Aside from variables, the tree is not changed.
        //
        //                          Return the root of the tree. If the root was not a variable
        //                          reference, it remains unchanged - the root we started with
        //                          is the root we return. If, however, the root was a variable
        //                          reference, the root of the newly cloned replacement tree will
        //                          be returned, and the original tree deleted.
        //
        //                          This function works by recursively walking the tree
        //                          without doing anything until a variable reference is
        //                          found, then calling cloneTree() at that point. Any
        //                          nested references are handled by cloneTree(), not here.
        //
        //-------------------------------------------------------------------------
        internal virtual RBBINode FlattenVariables()
        {
            if (fType == varRef)
            {
                RBBINode retNode = fLeftChild.CloneTree();
                retNode.fRuleRoot = this.fRuleRoot;
                retNode.fChainIn = this.fChainIn;
                return retNode;
            }

            if (fLeftChild != null)
            {
                fLeftChild = fLeftChild.FlattenVariables();
                fLeftChild.fParent = this;
            }
            if (fRightChild != null)
            {
                fRightChild = fRightChild.FlattenVariables();
                fRightChild.fParent = this;
            }
            return this;
        }

        //-------------------------------------------------------------------------
        //
        //      flattenSets Walk the parse tree, replacing any nodes of type setRef
        //                     with a copy of the expression tree for the set. A set's
        //                     equivalent expression tree is precomputed and saved as
        //                     the left child of the uset node.
        //
        //-------------------------------------------------------------------------
        internal virtual void FlattenSets()
        {
            Assert.Assrt(fType != setRef);

            if (fLeftChild != null)
            {
                if (fLeftChild.fType == setRef)
                {
                    RBBINode setRefNode = fLeftChild;
                    RBBINode usetNode = setRefNode.fLeftChild;
                    RBBINode replTree = usetNode.fLeftChild;
                    fLeftChild = replTree.CloneTree();
                    fLeftChild.fParent = this;
                }
                else
                {
                    fLeftChild.FlattenSets();
                }
            }

            if (fRightChild != null)
            {
                if (fRightChild.fType == setRef)
                {
                    RBBINode setRefNode = fRightChild;
                    RBBINode usetNode = setRefNode.fLeftChild;
                    RBBINode replTree = usetNode.fLeftChild;
                    fRightChild = replTree.CloneTree();
                    fRightChild.fParent = this;
                    // delete setRefNode;
                }
                else
                {
                    fRightChild.FlattenSets();
                }
            }
        }

        //-------------------------------------------------------------------------
        //
        //       findNodes() Locate all the nodes of the specified type, starting
        //                       at the specified root.
        //
        //-------------------------------------------------------------------------
        internal virtual void FindNodes(List<RBBINode> dest, int kind)
        {
            if (fType == kind)
            {
                dest.Add(this);
            }
            if (fLeftChild != null)
            {
                fLeftChild.FindNodes(dest, kind);
            }
            if (fRightChild != null)
            {
                fRightChild.FindNodes(dest, kind);
            }
        }



        //-------------------------------------------------------------------------
        //
        //        print. Print out a single node, for debugging.
        //
        //-------------------------------------------------------------------------
        ///CLOVER:OFF
        internal static void PrintNode(RBBINode n)
        {

            if (n == null)
            {
                Console.Out.Write(" -- null --\n");
            }
            else
            {
                RBBINode.PrintInt32(n.fSerialNum, 10);
                RBBINode.PrintString(nodeTypeNames[n.fType], 11);
                RBBINode.PrintInt32(n.fParent == null ? 0 : n.fParent.fSerialNum, 11);
                RBBINode.PrintInt32(n.fLeftChild == null ? 0 : n.fLeftChild.fSerialNum, 11);
                RBBINode.PrintInt32(n.fRightChild == null ? 0 : n.fRightChild.fSerialNum, 12);
                RBBINode.PrintInt32(n.fFirstPos, 12);
                RBBINode.PrintInt32(n.fVal, 7);

                if (n.fType == varRef)
                {
                    Console.Out.Write(" " + n.fText);
                }
            }
            Console.Out.WriteLine("");
        }
        //CLOVER:ON


        // Print a String in a fixed field size.
        // Debugging function.
        //CLOVER:OFF
        internal static void PrintString(String s, int minWidth)
        {
            for (int i = minWidth; i < 0; i++)
            {
                // negative width means pad leading spaces, not fixed width.
                Console.Out.Write(' ');
            }
            for (int i = s.Length; i < minWidth; i++)
            {
                Console.Out.Write(' ');
            }
            Console.Out.Write(s);
        }
        //CLOVER:ON

        //
        //  Print an int in a fixed size field.
        //  Debugging function.
        //
        //CLOVER:OFF
        internal static void PrintInt32(int i, int minWidth)
        {
            string s = i.ToString(CultureInfo.InvariantCulture);
            PrintString(s, Math.Max(minWidth, s.Length + 1));
        }
        //CLOVER:ON

        //CLOVER:OFF
        internal static void PrintHex(int i, int minWidth)
        {
            string s = Convert.ToString(i, 16); // ICU4N TODO: Check this conversion
            string leadingZeroes = "00000"
                    .Substring(0, Math.Max(0, 5 - s.Length) - 0); // ICU4N: Checked 2nd parameter
            s = leadingZeroes + s;
            PrintString(s, minWidth);
        }
        //CLOVER:ON


        // -------------------------------------------------------------------------
        //
        //        print. Print out the tree of nodes rooted at "this"
        //
        // -------------------------------------------------------------------------
        //CLOVER:OFF
        internal virtual void PrintTree(bool printHeading)
        {
            if (printHeading)
            {
                Console.Out.WriteLine("-------------------------------------------------------------------");
                Console.Out.WriteLine("    Serial       type     Parent  LeftChild  RightChild    position  value");
            }
            PrintNode(this);
            // Only dump the definition under a variable reference if asked to.
            // Unconditinally dump children of all other node types.
            if (fType != varRef)
            {
                if (fLeftChild != null)
                {
                    fLeftChild.PrintTree(false);
                }

                if (fRightChild != null)
                {
                    fRightChild.PrintTree(false);
                }
            }
        }
        //CLOVER:ON
    }
}
