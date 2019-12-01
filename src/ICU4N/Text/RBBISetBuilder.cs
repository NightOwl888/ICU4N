using ICU4N.Impl;
using ICU4N.Support.Collections;
using J2N.Collections;
using System;
using System.Collections.Generic;
using System.IO;

namespace ICU4N.Text
{
    //
    //  RBBISetBuilder   Handles processing of Unicode Sets from RBBI rules
    //                   (part of the rule building process.)
    //
    //      Starting with the rules parse tree from the scanner,
    //
    //                   -  Enumerate the set of UnicodeSets that are referenced
    //                      by the RBBI rules.
    //                   -  compute a set of non-overlapping character ranges
    //                      with all characters within a range belonging to the same
    //                      set of input uniocde sets.
    //                   -  Derive a set of non-overlapping UnicodeSet (like things)
    //                      that will correspond to columns in the state table for
    //                      the RBBI execution engine.  All characters within one
    //                      of these sets belong to the same set of the original
    //                      UnicodeSets from the user's rules.
    //                   -  construct the trie table that maps input characters
    //                      to the index of the matching non-overlapping set of set from
    //                      the previous step.
    //
    internal class RBBISetBuilder
    {
        internal class RangeDescriptor
        {
            internal int fStartChar;      // Start of range, unicode 32 bit value.
            internal int fEndChar;        // End of range, unicode 32 bit value.
            internal int fNum;            // runtime-mapped input value for this range.
            internal List<RBBINode> fIncludesSets;    // vector of the the original
                                                      //   Unicode sets that include this range.
                                                      //    (Contains ptrs to uset nodes)
            internal RangeDescriptor fNext;           // Next RangeDescriptor in the linked list.

            internal RangeDescriptor()
            {
                fIncludesSets = new List<RBBINode>();
            }

            internal RangeDescriptor(RangeDescriptor other)
            {
                fStartChar = other.fStartChar;
                fEndChar = other.fEndChar;
                fNum = other.fNum;
                fIncludesSets = new List<RBBINode>(other.fIncludesSets);
            }

            //-------------------------------------------------------------------------------------
            //
            //          RangeDesriptor::split()
            //
            //-------------------------------------------------------------------------------------
            internal virtual void Split(int where)
            {
                Assert.Assrt(where > fStartChar && where <= fEndChar);
                RangeDescriptor nr = new RangeDescriptor(this);

                //  RangeDescriptor copy constructor copies all fields.
                //  Only need to update those that are different after the split.
                nr.fStartChar = where;
                this.fEndChar = where - 1;
                nr.fNext = this.fNext;
                this.fNext = nr;

                // TODO:  fIncludesSets is not updated.  Check it out.
                //         Probably because they haven't been populated yet,
                //         but still sloppy.
            }


            //-------------------------------------------------------------------------------------
            //
            //          RangeDescriptor::setDictionaryFlag
            //
            //          Character Category Numbers that include characters from
            //          the original Unicode Set named "dictionary" have bit 14
            //          set to 1.  The RBBI runtime engine uses this to trigger
            //          use of the word dictionary.
            //
            //          This function looks through the Unicode Sets that it
            //          (the range) includes, and sets the bit in fNum when
            //          "dictionary" is among them.
            //
            //          TODO:  a faster way would be to find the set node for
            //          "dictionary" just once, rather than looking it
            //          up by name every time.
            //
            // -------------------------------------------------------------------------------------
            internal virtual void SetDictionaryFlag()
            {
                int i;

                for (i = 0; i < this.fIncludesSets.Count; i++)
                {
                    RBBINode usetNode = fIncludesSets[i];
                    string setName = "";
                    RBBINode setRef = usetNode.fParent;
                    if (setRef != null)
                    {
                        RBBINode varRef = setRef.fParent;
                        if (varRef != null && varRef.fType == RBBINode.varRef)
                        {
                            setName = varRef.fText;
                        }
                    }
                    if (setName.Equals("dictionary"))
                    {
                        this.fNum |= 0x4000;
                        break;
                    }
                }

            }
        }


        internal RBBIRuleBuilder fRB;             // The RBBI Rule Compiler that owns us.
        internal RangeDescriptor fRangeList;      // Head of the linked list of RangeDescriptors

        internal Trie2Writable fTrie;           // The mapping TRIE that is the end result of processing
                                                //  the Unicode Sets.
        internal Trie2_16 fFrozenTrie;

        // Groups correspond to character categories -
        //       groups of ranges that are in the same original UnicodeSets.
        //       fGroupCount is the index of the last used group.
        //       fGroupCount+1 is also the number of columns in the RBBI state table being compiled.
        //       State table column 0 is not used.  Column 1 is for end-of-input.
        //       column 2 is for group 0.  Funny counting.
        internal int fGroupCount;

        internal bool fSawBOF;


        //------------------------------------------------------------------------
        //
        //       RBBISetBuilder Constructor
        //
        //------------------------------------------------------------------------
        internal RBBISetBuilder(RBBIRuleBuilder rb)
        {
            fRB = rb;
        }


        //------------------------------------------------------------------------
        //
        //           build          Build the list of non-overlapping character ranges
        //                          from the Unicode Sets.
        //
        //------------------------------------------------------------------------
        internal virtual void Build()
        {
            RangeDescriptor rlRange;

            if (fRB.fDebugEnv != null && fRB.fDebugEnv.IndexOf("usets", StringComparison.Ordinal) >= 0) { PrintSets(); }

            //  Initialize the process by creating a single range encompassing all characters
            //  that is in no sets.
            //
            fRangeList = new RangeDescriptor();
            fRangeList.fStartChar = 0;
            fRangeList.fEndChar = 0x10ffff;

            //
            //  Find the set of non-overlapping ranges of characters
            //
            foreach (RBBINode usetNode in fRB.fUSetNodes)
            {
                UnicodeSet inputSet = usetNode.fInputSet;
                int inputSetRangeCount = inputSet.RangeCount;
                int inputSetRangeIndex = 0;
                rlRange = fRangeList;

                for (; ; )
                {
                    if (inputSetRangeIndex >= inputSetRangeCount)
                    {
                        break;
                    }
                    int inputSetRangeBegin = inputSet.GetRangeStart(inputSetRangeIndex);
                    int inputSetRangeEnd = inputSet.GetRangeEnd(inputSetRangeIndex);

                    // skip over ranges from the range list that are completely
                    //   below the current range from the input unicode set.
                    while (rlRange.fEndChar < inputSetRangeBegin)
                    {
                        rlRange = rlRange.fNext;
                    }

                    // If the start of the range from the range list is before with
                    //   the start of the range from the unicode set, split the range list range
                    //   in two, with one part being before (wholly outside of) the unicode set
                    //   and the other containing the rest.
                    //   Then continue the loop; the post-split current range will then be skipped
                    //     over
                    if (rlRange.fStartChar < inputSetRangeBegin)
                    {
                        rlRange.Split(inputSetRangeBegin);
                        continue;
                    }

                    // Same thing at the end of the ranges...
                    // If the end of the range from the range list doesn't coincide with
                    //   the end of the range from the unicode set, split the range list
                    //   range in two.  The first part of the split range will be
                    //   wholly inside the Unicode set.
                    if (rlRange.fEndChar > inputSetRangeEnd)
                    {
                        rlRange.Split(inputSetRangeEnd + 1);
                    }

                    // The current rlRange is now entirely within the UnicodeSet range.
                    // Add this unicode set to the list of sets for this rlRange
                    if (rlRange.fIncludesSets.IndexOf(usetNode) == -1)
                    {
                        rlRange.fIncludesSets.Add(usetNode);
                    }

                    // Advance over ranges that we are finished with.
                    if (inputSetRangeEnd == rlRange.fEndChar)
                    {
                        inputSetRangeIndex++;
                    }
                    rlRange = rlRange.fNext;
                }
            }

            if (fRB.fDebugEnv != null && fRB.fDebugEnv.IndexOf("range", StringComparison.Ordinal) >= 0) { PrintRanges(); }

            //
            //  Group the above ranges, with each group consisting of one or more
            //    ranges that are in exactly the same set of original UnicodeSets.
            //    The groups are numbered, and these group numbers are the set of
            //    input symbols recognized by the run-time state machine.
            //
            //    Numbering: # 0  (state table column 0) is unused.
            //               # 1  is reserved - table column 1 is for end-of-input
            //               # 2  is reserved - table column 2 is for beginning-in-input
            //               # 3  is the first range list.
            //
            RangeDescriptor rlSearchRange;
            for (rlRange = fRangeList; rlRange != null; rlRange = rlRange.fNext)
            {
                for (rlSearchRange = fRangeList; rlSearchRange != rlRange; rlSearchRange = rlSearchRange.fNext)
                {
                    if (CollectionUtil.Equals(rlRange.fIncludesSets, rlSearchRange.fIncludesSets))
                    {
                        rlRange.fNum = rlSearchRange.fNum;
                        break;
                    }
                }
                if (rlRange.fNum == 0)
                {
                    fGroupCount++;
                    rlRange.fNum = fGroupCount + 2;
                    rlRange.SetDictionaryFlag();
                    AddValToSets(rlRange.fIncludesSets, fGroupCount + 2);
                }
            }

            // Handle input sets that contain the special string {eof}.
            //   Column 1 of the state table is reserved for EOF on input.
            //   Column 2 is reserved for before-the-start-input.
            //            (This column can be optimized away later if there are no rule
            //             references to {bof}.)
            //   Add this column value (1 or 2) to the equivalent expression
            //     subtree for each UnicodeSet that contains the string {eof}
            //   Because {bof} and {eof} are not a characters in the normal sense,
            //   they doesn't affect the computation of ranges or TRIE.

            string eofString = "eof";
            string bofString = "bof";

            foreach (RBBINode usetNode in fRB.fUSetNodes)
            {
                UnicodeSet inputSet = usetNode.fInputSet;
                if (inputSet.Contains(eofString))
                {
                    AddValToSet(usetNode, 1);
                }
                if (inputSet.Contains(bofString))
                {
                    AddValToSet(usetNode, 2);
                    fSawBOF = true;
                }
            }


            if (fRB.fDebugEnv != null && fRB.fDebugEnv.IndexOf("rgroup", StringComparison.Ordinal) >= 0) { PrintRangeGroups(); }
            if (fRB.fDebugEnv != null && fRB.fDebugEnv.IndexOf("esets", StringComparison.Ordinal) >= 0) { PrintSets(); }

            fTrie = new Trie2Writable(0,       //   Initial value for all code points.
                                      0);      //   Error value for out-of-range input.

            for (rlRange = fRangeList; rlRange != null; rlRange = rlRange.fNext)
            {
                fTrie.SetRange(
                        rlRange.fStartChar,     // Range start
                        rlRange.fEndChar,       // Range end (inclusive)
                        rlRange.fNum,           // value for range
                        true                    // Overwrite previously written values
                        );
            }
        }


        //-----------------------------------------------------------------------------------
        //
        //          getTrieSize()    Return the size that will be required to serialize the Trie.
        //
        //-----------------------------------------------------------------------------------
        internal virtual int GetTrieSize()
        {
            if (fFrozenTrie == null)
            {
                fFrozenTrie = fTrie.ToTrie2_16();
                fTrie = null;
            }
            return fFrozenTrie.SerializedLength;
        }


        //-----------------------------------------------------------------------------------
        //
        //          serializeTrie()   Write the serialized trie to an output stream
        //
        //-----------------------------------------------------------------------------------
        internal virtual void SerializeTrie(Stream os)
        {
            if (fFrozenTrie == null)
            {
                fFrozenTrie = fTrie.ToTrie2_16();
                fTrie = null;
            }
            fFrozenTrie.Serialize(os);
        }

        //------------------------------------------------------------------------
        //
        //      addValToSets     Add a runtime-mapped input value to each uset from a
        //      list of uset nodes. (val corresponds to a state table column.)
        //      For each of the original Unicode sets - which correspond
        //      directly to uset nodes - a logically equivalent expression
        //      is constructed in terms of the remapped runtime input
        //      symbol set.  This function adds one runtime input symbol to
        //      a list of sets.
        //
        //      The "logically equivalent expression" is the tree for an
        //      or-ing together of all of the symbols that go into the set.
        //
        //------------------------------------------------------------------------
        internal virtual void AddValToSets(IList<RBBINode> sets, int val)
        {
            foreach (RBBINode usetNode in sets)
            {
                AddValToSet(usetNode, val);
            }
        }

        internal virtual void AddValToSet(RBBINode usetNode, int val)
        {
            RBBINode leafNode = new RBBINode(RBBINode.leafChar);
            leafNode.fVal = val;
            if (usetNode.fLeftChild == null)
            {
                usetNode.fLeftChild = leafNode;
                leafNode.fParent = usetNode;
            }
            else
            {
                // There are already input symbols present for this set.
                // Set up an OR node, with the previous stuff as the left child
                //   and the new value as the right child.
                RBBINode orNode = new RBBINode(RBBINode.opOr);
                orNode.fLeftChild = usetNode.fLeftChild;
                orNode.fRightChild = leafNode;
                orNode.fLeftChild.fParent = orNode;
                orNode.fRightChild.fParent = orNode;
                usetNode.fLeftChild = orNode;
                orNode.fParent = usetNode;
            }
        }


        //------------------------------------------------------------------------
        //
        //           getNumCharCategories
        //
        //------------------------------------------------------------------------
        internal virtual int NumCharCategories
        {
            get { return fGroupCount + 3; }
        }


        //------------------------------------------------------------------------
        //
        //           sawBOF
        //
        //------------------------------------------------------------------------
        internal virtual bool SawBOF
        {
            get { return fSawBOF; }
        }


        //------------------------------------------------------------------------
        //
        //           getFirstChar      Given a runtime RBBI character category, find
        //                             the first UChar32 that is in the set of chars
        //                             in the category.
        //------------------------------------------------------------------------
        internal virtual int GetFirstChar(int category)
        {
            RangeDescriptor rlRange;
            int retVal = -1;
            for (rlRange = fRangeList; rlRange != null; rlRange = rlRange.fNext)
            {
                if (rlRange.fNum == category)
                {
                    retVal = rlRange.fStartChar;
                    break;
                }
            }
            return retVal;
        }



        //------------------------------------------------------------------------
        //
        //           printRanges        A debugging function.
        //                              dump out all of the range definitions.
        //
        //------------------------------------------------------------------------
        ///CLOVER:OFF
        internal virtual void PrintRanges()
        {
            RangeDescriptor rlRange;
            int i;

            Console.Out.Write("\n\n Nonoverlapping Ranges ...\n");
            for (rlRange = fRangeList; rlRange != null; rlRange = rlRange.fNext)
            {
                Console.Out.Write(" " + rlRange.fNum + "   " + rlRange.fStartChar + "-" + rlRange.fEndChar);

                for (i = 0; i < rlRange.fIncludesSets.Count; i++)
                {
                    RBBINode usetNode = rlRange.fIncludesSets[i];
                    String setName = "anon";
                    RBBINode setRef = usetNode.fParent;
                    if (setRef != null)
                    {
                        RBBINode varRef = setRef.fParent;
                        if (varRef != null && varRef.fType == RBBINode.varRef)
                        {
                            setName = varRef.fText;
                        }
                    }
                    Console.Out.Write(setName); Console.Out.Write("  ");
                }
                Console.Out.WriteLine("");
            }
        }
        //CLOVER:ON


        //------------------------------------------------------------------------
        //
        //           printRangeGroups     A debugging function.
        //                                dump out all of the range groups.
        //
        //------------------------------------------------------------------------
        ///CLOVER:OFF
        internal virtual void PrintRangeGroups()
        {
            RangeDescriptor rlRange;
            RangeDescriptor tRange;
            int i;
            int lastPrintedGroupNum = 0;

            Console.Out.Write("\nRanges grouped by Unicode Set Membership...\n");
            for (rlRange = fRangeList; rlRange != null; rlRange = rlRange.fNext)
            {
                int groupNum = rlRange.fNum & 0xbfff;
                if (groupNum > lastPrintedGroupNum)
                {
                    lastPrintedGroupNum = groupNum;
                    if (groupNum < 10) { Console.Out.Write(" "); }
                    Console.Out.Write(groupNum + " ");

                    if ((rlRange.fNum & 0x4000) != 0) { Console.Out.Write(" <DICT> "); }

                    for (i = 0; i < rlRange.fIncludesSets.Count; i++)
                    {
                        RBBINode usetNode = rlRange.fIncludesSets[i];
                        String setName = "anon";
                        RBBINode setRef = usetNode.fParent;
                        if (setRef != null)
                        {
                            RBBINode varRef = setRef.fParent;
                            if (varRef != null && varRef.fType == RBBINode.varRef)
                            {
                                setName = varRef.fText;
                            }
                        }
                        Console.Out.Write(setName); Console.Out.Write(" ");
                    }

                    i = 0;
                    for (tRange = rlRange; tRange != null; tRange = tRange.fNext)
                    {
                        if (tRange.fNum == rlRange.fNum)
                        {
                            if (i++ % 5 == 0)
                            {
                                Console.Out.Write("\n    ");
                            }
                            RBBINode.PrintHex(tRange.fStartChar, -1);
                            Console.Out.Write("-");
                            RBBINode.PrintHex(tRange.fEndChar, 0);
                        }
                    }
                    Console.Out.Write("\n");
                }
            }
            Console.Out.Write("\n");
        }
        //CLOVER:ON


        //------------------------------------------------------------------------
        //
        //           printSets          A debugging function.
        //                              dump out all of the set definitions.
        //
        //------------------------------------------------------------------------
        ///CLOVER:OFF
        internal virtual void PrintSets()
        {
            int i;
            Console.Out.Write("\n\nUnicode Sets List\n------------------\n");
            for (i = 0; i < fRB.fUSetNodes.Count; i++)
            {
                RBBINode usetNode;
                RBBINode setRef;
                RBBINode varRef;
                String setName;

                usetNode = fRB.fUSetNodes[i];

                //System.out.print(" " + i + "   ");
                RBBINode.PrintInt32(2, i);
                setName = "anonymous";
                setRef = usetNode.fParent;
                if (setRef != null)
                {
                    varRef = setRef.fParent;
                    if (varRef != null && varRef.fType == RBBINode.varRef)
                    {
                        setName = varRef.fText;
                    }
                }
                Console.Out.Write("  " + setName);
                Console.Out.Write("   ");
                Console.Out.Write(usetNode.fText);
                Console.Out.Write("\n");
                if (usetNode.fLeftChild != null)
                {
                    usetNode.fLeftChild.PrintTree(true);
                }
            }
            Console.Out.Write("\n");
        }
        //CLOVER:ON
    }
}
