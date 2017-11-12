using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// This class is part of the Rule Based Break Iterator rule compiler.
    /// It scans the rules and builds the parse tree.
    /// There is no public API here.
    /// </summary>
    internal class RBBIRuleScanner
    {
        private readonly static int kStackSize = 100;               // The size of the state stack for
                                                                 //   rules parsing.  Corresponds roughly
                                                                 //   to the depth of parentheses nesting
                                                                 //   that is allowed in the rules.

        internal class RBBIRuleChar
        {
            internal int fChar;
            internal bool fEscaped;
        }



        internal RBBIRuleBuilder fRB;              // The rule builder that we are part of.

        internal int fScanIndex;        // Index of current character being processed
                                        //   in the rule input string.
        internal int fNextIndex;        // Index of the next character, which
                                        //   is the first character not yet scanned.
        internal bool fQuoteMode;        // Scan is in a 'quoted region'
        internal int fLineNum;          // Line number in input file.
        internal int fCharNum;          // Char position within the line.
        internal int fLastChar;         // Previous char, needed to count CR-LF
                                        //   as a single line, not two.

        internal RBBIRuleChar fC = new RBBIRuleChar();    // Current char for parse state machine
                                                          //   processing.


        internal short[] fStack = new short[kStackSize];  // State stack, holds state pushes
        internal int fStackPtr;           //  and pops as specified in the state
                                          //  transition rules.

        internal RBBINode[] fNodeStack = new RBBINode[kStackSize]; // Node stack, holds nodes created
                                                                   //  during the parse of a rule
        internal int fNodeStackPtr;


        internal bool fReverseRule;         // True if the rule currently being scanned
                                            //  is a reverse direction rule (if it
                                            //  starts with a '!')

        internal bool fLookAheadRule;       // True if the rule includes a '/'
                                            //   somewhere within it.

        internal bool fNoChainInRule;       // True if the current rule starts with a '^'.


        internal RBBISymbolTable fSymbolTable;         // symbol table, holds definitions of
                                                       //   $variable symbols.

        internal Dictionary<string, RBBISetTableEl> fSetTable = new Dictionary<string, RBBISetTableEl>(); // UnicocodeSet hash table, holds indexes to
                                                                                                          //   the sets created while parsing rules.
                                                                                                          //   The key is the string used for creating
                                                                                                          //   the set.

        internal UnicodeSet[] fRuleSets = new UnicodeSet[10];    // Unicode Sets that are needed during
                                                                 //  the scanning of RBBI rules.  The
                                                                 //  Indices for these are assigned by the
                                                                 //  perl script that builds the state tables.
                                                                 //  See rbbirpt.h.

        internal int fRuleNum;         // Counts each rule as it is scanned.

        internal int fOptionStart;     // Input index of start of a !!option
                              //   keyword, while being scanned.


        // gRuleSet_rule_char_pattern is characters that may appear as literals in patterns without escaping or quoting.
        static private string gRuleSet_rule_char_pattern = "[^[\\p{Z}\\u0020-\\u007f]-[\\p{L}]-[\\p{N}]]";
        static private string gRuleSet_name_char_pattern = "[_\\p{L}\\p{N}]";
        static private string gRuleSet_digit_char_pattern = "[0-9]";
        static private string gRuleSet_name_start_char_pattern = "[_\\p{L}]";
        static private string gRuleSet_white_space_pattern = "[\\p{Pattern_White_Space}]";
        static private string kAny = "any";




        //----------------------------------------------------------------------------------------
        //
        //  Constructor.
        //
        //----------------------------------------------------------------------------------------
        internal RBBIRuleScanner(RBBIRuleBuilder rb)
        {
            fRB = rb;
            fLineNum = 1;

            //
            //  Set up the constant Unicode Sets.
            //     Note: These could be made static and shared among
            //            all instances of RBBIRuleScanners.
            fRuleSets[RBBIRuleParseTable.kRuleSet_rule_char - 128] = new UnicodeSet(gRuleSet_rule_char_pattern);
            fRuleSets[RBBIRuleParseTable.kRuleSet_white_space - 128] = new UnicodeSet(gRuleSet_white_space_pattern);
            fRuleSets[RBBIRuleParseTable.kRuleSet_name_char - 128] = new UnicodeSet(gRuleSet_name_char_pattern);
            fRuleSets[RBBIRuleParseTable.kRuleSet_name_start_char - 128] = new UnicodeSet(gRuleSet_name_start_char_pattern);
            fRuleSets[RBBIRuleParseTable.kRuleSet_digit_char - 128] = new UnicodeSet(gRuleSet_digit_char_pattern);

            fSymbolTable = new RBBISymbolTable(this);
        }

        //----------------------------------------------------------------------------------------
        //
        //  doParseAction Do some action during rule parsing.
        //                       Called by the parse state machine.
        //                       Actions build the parse tree and Unicode Sets,
        //                       and maintain the parse stack for nested expressions.
        //
        //----------------------------------------------------------------------------------------
        internal virtual bool DoParseActions(int action)
        {
            RBBINode n = null;

            bool returnVal = true;

            switch (action)
            {

                case RBBIRuleParseTable.doExprStart:
                    PushNewNode(RBBINode.opStart);
                    fRuleNum++;
                    break;

                case RBBIRuleParseTable.doNoChain:
                    // Scanned a '^' while on the rule start state.
                    fNoChainInRule = true;
                    break;


                case RBBIRuleParseTable.doExprOrOperator:
                    {
                        FixOpStack(RBBINode.precOpCat);
                        RBBINode operandNode = fNodeStack[fNodeStackPtr--];
                        RBBINode orNode = PushNewNode(RBBINode.opOr);
                        orNode.fLeftChild = operandNode;
                        operandNode.fParent = orNode;
                    }
                    break;

                case RBBIRuleParseTable.doExprCatOperator:
                    // concatenation operator.
                    // For the implicit concatenation of adjacent terms in an expression
                    // that are
                    //   not separated by any other operator. Action is invoked between the
                    //   actions for the two terms.
                    {
                        FixOpStack(RBBINode.precOpCat);
                        RBBINode operandNode = fNodeStack[fNodeStackPtr--];
                        RBBINode catNode = PushNewNode(RBBINode.opCat);
                        catNode.fLeftChild = operandNode;
                        operandNode.fParent = catNode;
                    }
                    break;

                case RBBIRuleParseTable.doLParen:
                    // Open Paren.
                    //   The openParen node is a dummy operation type with a low
                    // precedence,
                    //     which has the affect of ensuring that any real binary op that
                    //     follows within the parens binds more tightly to the operands than
                    //     stuff outside of the parens.
                    PushNewNode(RBBINode.opLParen);
                    break;

                case RBBIRuleParseTable.doExprRParen:
                    FixOpStack(RBBINode.precLParen);
                    break;

                case RBBIRuleParseTable.doNOP:
                    break;

                case RBBIRuleParseTable.doStartAssign:
                    // We've just scanned "$variable = "
                    // The top of the node stack has the $variable ref node.

                    // Save the start position of the RHS text in the StartExpression
                    // node
                    //   that precedes the $variableReference node on the stack.
                    //   This will eventually be used when saving the full $variable
                    // replacement
                    //   text as a string.
                    n = fNodeStack[fNodeStackPtr - 1];
                    n.fFirstPos = fNextIndex; // move past the '='

                    // Push a new start-of-expression node; needed to keep parse of the
                    //   RHS expression happy.
                    PushNewNode(RBBINode.opStart);
                    break;

                case RBBIRuleParseTable.doEndAssign:
                    {
                        // We have reached the end of an assignement statement.
                        //   Current scan char is the ';' that terminates the assignment.

                        // Terminate expression, leaves expression parse tree rooted in TOS
                        // node.
                        FixOpStack(RBBINode.precStart);

                        RBBINode startExprNode = fNodeStack[fNodeStackPtr - 2];
                        RBBINode varRefNode = fNodeStack[fNodeStackPtr - 1];
                        RBBINode RHSExprNode = fNodeStack[fNodeStackPtr];

                        // Save original text of right side of assignment, excluding the
                        // terminating ';'
                        //  in the root of the node for the right-hand-side expression.
                        RHSExprNode.fFirstPos = startExprNode.fFirstPos;
                        RHSExprNode.fLastPos = fScanIndex;
                        // fRB.fRules.extractBetween(RHSExprNode.fFirstPos,
                        // RHSExprNode.fLastPos, RHSExprNode.fText);
                        RHSExprNode.fText = fRB.fRules.Substring(RHSExprNode.fFirstPos,
                                RHSExprNode.fLastPos - RHSExprNode.fFirstPos); // ICU4N: Corrected 2nd parameter

                        // Expression parse tree becomes l. child of the $variable reference
                        // node.
                        varRefNode.fLeftChild = RHSExprNode;
                        RHSExprNode.fParent = varRefNode;

                        // Make a symbol table entry for the $variableRef node.
                        fSymbolTable.AddEntry(varRefNode.fText, varRefNode);

                        // Clean up the stack.
                        fNodeStackPtr -= 3;
                        break;
                    }

                case RBBIRuleParseTable.doEndOfRule:
                    {
                        FixOpStack(RBBINode.precStart); // Terminate expression, leaves
                                                        // expression

                        if (fRB.fDebugEnv != null && fRB.fDebugEnv.IndexOf("rtree") >= 0)
                        {
                            PrintNodeStack("end of rule");
                        }
                        Assert.Assrt(fNodeStackPtr == 1);
                        RBBINode thisRule = fNodeStack[fNodeStackPtr];

                        // If this rule includes a look-ahead '/', add a endMark node to the
                        //   expression tree.
                        if (fLookAheadRule)
                        {
                            RBBINode endNode = PushNewNode(RBBINode.endMark);
                            RBBINode catNode = PushNewNode(RBBINode.opCat);
                            fNodeStackPtr -= 2;
                            catNode.fLeftChild = thisRule;
                            catNode.fRightChild = endNode;
                            fNodeStack[fNodeStackPtr] = catNode;
                            endNode.fVal = fRuleNum;
                            endNode.fLookAheadEnd = true;
                            thisRule = catNode;

                            // TODO: Disable chaining out of look-ahead (hard break) rules.
                            //   The break on rule match is forced, so there is no point in building up
                            //   the state table to chain into another rule for a longer match.
                        }

                        // Mark this node as being the root of a rule.
                        thisRule.fRuleRoot = true;

                        // Flag if chaining into this rule is wanted.
                        //
                        if (fRB.fChainRules &&          // If rule chaining is enabled globally via !!chain
                                !fNoChainInRule)
                        {      //     and no '^' chain-in inhibit was on this rule
                            thisRule.fChainIn = true;
                        }


                        // All rule expressions are ORed together.
                        // The ';' that terminates an expression really just functions as a
                        // '|' with
                        //   a low operator prededence.
                        //
                        // Each of the four sets of rules are collected separately.
                        //  (forward, reverse, safe_forward, safe_reverse)
                        //  OR this rule into the appropriate group of them.
                        //

                        int destRules = (fReverseRule ? RBBIRuleBuilder.fReverseTree : fRB.fDefaultTree);

                        if (fRB.fTreeRoots[destRules] != null)
                        {
                            // This is not the first rule encountered.
                            // OR previous stuff (from *destRules)
                            // with the current rule expression (on the Node Stack)
                            //  with the resulting OR expression going to *destRules
                            //
                            thisRule = fNodeStack[fNodeStackPtr];
                            RBBINode prevRules = fRB.fTreeRoots[destRules];
                            RBBINode orNode = PushNewNode(RBBINode.opOr);
                            orNode.fLeftChild = prevRules;
                            prevRules.fParent = orNode;
                            orNode.fRightChild = thisRule;
                            thisRule.fParent = orNode;
                            fRB.fTreeRoots[destRules] = orNode;
                        }
                        else
                        {
                            // This is the first rule encountered (for this direction).
                            // Just move its parse tree from the stack to *destRules.
                            fRB.fTreeRoots[destRules] = fNodeStack[fNodeStackPtr];
                        }
                        fReverseRule = false; // in preparation for the next rule.
                        fLookAheadRule = false;
                        fNoChainInRule = false;
                        fNodeStackPtr = 0;
                    }
                    break;

                case RBBIRuleParseTable.doRuleError:
                    Error(RBBIRuleBuilder.U_BRK_RULE_SYNTAX);
                    returnVal = false;
                    break;

                case RBBIRuleParseTable.doVariableNameExpectedErr:
                    Error(RBBIRuleBuilder.U_BRK_RULE_SYNTAX);
                    break;

                //
                //  Unary operands + ? *
                //    These all appear after the operand to which they apply.
                //    When we hit one, the operand (may be a whole sub expression)
                //    will be on the top of the stack.
                //    Unary Operator becomes TOS, with the old TOS as its one child.
                case RBBIRuleParseTable.doUnaryOpPlus:
                    {
                        RBBINode operandNode = fNodeStack[fNodeStackPtr--];
                        RBBINode plusNode = PushNewNode(RBBINode.opPlus);
                        plusNode.fLeftChild = operandNode;
                        operandNode.fParent = plusNode;
                    }
                    break;

                case RBBIRuleParseTable.doUnaryOpQuestion:
                    {
                        RBBINode operandNode = fNodeStack[fNodeStackPtr--];
                        RBBINode qNode = PushNewNode(RBBINode.opQuestion);
                        qNode.fLeftChild = operandNode;
                        operandNode.fParent = qNode;
                    }
                    break;

                case RBBIRuleParseTable.doUnaryOpStar:
                    {
                        RBBINode operandNode = fNodeStack[fNodeStackPtr--];
                        RBBINode starNode = PushNewNode(RBBINode.opStar);
                        starNode.fLeftChild = operandNode;
                        operandNode.fParent = starNode;
                    }
                    break;

                case RBBIRuleParseTable.doRuleChar:
                    // A "Rule Character" is any single character that is a literal part
                    // of the regular expression. Like a, b and c in the expression "(abc*)
                    // | [:L:]"
                    // These are pretty uncommon in break rules; the terms are more commonly
                    //  sets. To keep things uniform, treat these characters like as
                    // sets that just happen to contain only one character.
                    {
                        n = PushNewNode(RBBINode.setRef);
                        String s = new string(new char[] { (char)fC.fChar });
                        FindSetFor(s, n, null);
                        n.fFirstPos = fScanIndex;
                        n.fLastPos = fNextIndex;
                        n.fText = fRB.fRules.Substring(n.fFirstPos, n.fLastPos - n.fFirstPos); // ICU4N: Corrected 2nd parameter
                        break;
                    }

                case RBBIRuleParseTable.doDotAny:
                    // scanned a ".", meaning match any single character.
                    {
                        n = PushNewNode(RBBINode.setRef);
                        FindSetFor(kAny, n, null);
                        n.fFirstPos = fScanIndex;
                        n.fLastPos = fNextIndex;
                        n.fText = fRB.fRules.Substring(n.fFirstPos, n.fLastPos - n.fFirstPos); // ICU4N: Corrected 2nd parameter
                        break;
                    }

                case RBBIRuleParseTable.doSlash:
                    // Scanned a '/', which identifies a look-ahead break position in a
                    // rule.
                    n = PushNewNode(RBBINode.lookAhead);
                    n.fVal = fRuleNum;
                    n.fFirstPos = fScanIndex;
                    n.fLastPos = fNextIndex;
                    n.fText = fRB.fRules.Substring(n.fFirstPos, n.fLastPos - n.fFirstPos); // ICU4N: Corrected 2nd parameter
                    fLookAheadRule = true;
                    break;

                case RBBIRuleParseTable.doStartTagValue:
                    // Scanned a '{', the opening delimiter for a tag value within a
                    // rule.
                    n = PushNewNode(RBBINode.tag);
                    n.fVal = 0;
                    n.fFirstPos = fScanIndex;
                    n.fLastPos = fNextIndex;
                    break;

                case RBBIRuleParseTable.doTagDigit:
                    // Just scanned a decimal digit that's part of a tag value
                    {
                        n = fNodeStack[fNodeStackPtr];
                        int v = UCharacter.Digit((char)fC.fChar, 10);
                        n.fVal = n.fVal * 10 + v;
                        break;
                    }

                case RBBIRuleParseTable.doTagValue:
                    n = fNodeStack[fNodeStackPtr];
                    n.fLastPos = fNextIndex;
                    n.fText = fRB.fRules.Substring(n.fFirstPos, n.fLastPos - n.fFirstPos); // ICU4N: Corrected 2nd parameter
                    break;

                case RBBIRuleParseTable.doTagExpectedError:
                    Error(RBBIRuleBuilder.U_BRK_MALFORMED_RULE_TAG);
                    returnVal = false;
                    break;

                case RBBIRuleParseTable.doOptionStart:
                    // Scanning a !!option. At the start of string.
                    fOptionStart = fScanIndex;
                    break;

                case RBBIRuleParseTable.doOptionEnd:
                    {
                        string opt = fRB.fRules.Substring(fOptionStart, fScanIndex - fOptionStart); // ICU4N: Corrected 2nd parameter
                        if (opt.Equals("chain"))
                        {
                            fRB.fChainRules = true;
                        }
                        else if (opt.Equals("LBCMNoChain"))
                        {
                            fRB.fLBCMNoChain = true;
                        }
                        else if (opt.Equals("forward"))
                        {
                            fRB.fDefaultTree = RBBIRuleBuilder.fForwardTree;
                        }
                        else if (opt.Equals("reverse"))
                        {
                            fRB.fDefaultTree = RBBIRuleBuilder.fReverseTree;
                        }
                        else if (opt.Equals("safe_forward"))
                        {
                            fRB.fDefaultTree = RBBIRuleBuilder.fSafeFwdTree;
                        }
                        else if (opt.Equals("safe_reverse"))
                        {
                            fRB.fDefaultTree = RBBIRuleBuilder.fSafeRevTree;
                        }
                        else if (opt.Equals("lookAheadHardBreak"))
                        {
                            fRB.fLookAheadHardBreak = true;
                        }
                        else if (opt.Equals("quoted_literals_only"))
                        {
                            fRuleSets[RBBIRuleParseTable.kRuleSet_rule_char - 128].Clear();
                        }
                        else if (opt.Equals("unquoted_literals"))
                        {
                            fRuleSets[RBBIRuleParseTable.kRuleSet_rule_char - 128].ApplyPattern(gRuleSet_rule_char_pattern);
                        }
                        else
                        {
                            Error(RBBIRuleBuilder.U_BRK_UNRECOGNIZED_OPTION);
                        }
                        break;
                    }

                case RBBIRuleParseTable.doReverseDir:
                    fReverseRule = true;
                    break;

                case RBBIRuleParseTable.doStartVariableName:
                    n = PushNewNode(RBBINode.varRef);
                    n.fFirstPos = fScanIndex;
                    break;

                case RBBIRuleParseTable.doEndVariableName:
                    n = fNodeStack[fNodeStackPtr];
                    if (n == null || n.fType != RBBINode.varRef)
                    {
                        Error(RBBIRuleBuilder.U_BRK_INTERNAL_ERROR);
                        break;
                    }
                    n.fLastPos = fScanIndex;
                    n.fText = fRB.fRules.Substring(n.fFirstPos + 1, n.fLastPos - (n.fFirstPos + 1)); // ICU4N: Corrected 2nd parameter
                    // Look the newly scanned name up in the symbol table
                    //   If there's an entry, set the l. child of the var ref to the
                    // replacement expression.
                    //   (We also pass through here when scanning assignments, but no harm
                    // is done, other
                    //    than a slight wasted effort that seems hard to avoid. Lookup will
                    // be null)
                    n.fLeftChild = fSymbolTable.LookupNode(n.fText);
                    break;

                case RBBIRuleParseTable.doCheckVarDef:
                    n = fNodeStack[fNodeStackPtr];
                    if (n.fLeftChild == null)
                    {
                        Error(RBBIRuleBuilder.U_BRK_UNDEFINED_VARIABLE);
                        returnVal = false;
                    }
                    break;

                case RBBIRuleParseTable.doExprFinished:
                    break;

                case RBBIRuleParseTable.doRuleErrorAssignExpr:
                    Error(RBBIRuleBuilder.U_BRK_ASSIGN_ERROR);
                    returnVal = false;
                    break;

                case RBBIRuleParseTable.doExit:
                    returnVal = false;
                    break;

                case RBBIRuleParseTable.doScanUnicodeSet:
                    ScanSet();
                    break;

                default:
                    Error(RBBIRuleBuilder.U_BRK_INTERNAL_ERROR);
                    returnVal = false;
                    break;
            }
            return returnVal;
        }

        //----------------------------------------------------------------------------------------
        //
        //  Error Throw and IllegalArgumentException in response to a rule parse
        // error.
        //
        //----------------------------------------------------------------------------------------
        internal virtual void Error(int e)
        {
            string s = "Error " + e + " at line " + fLineNum + " column "
                    + fCharNum;
            ArgumentException ex = new ArgumentException(s);
            throw ex;

        }

        //----------------------------------------------------------------------------------------
        //
        //  fixOpStack The parse stack holds partially assembled chunks of the parse
        // tree.
        //               An entry on the stack may be as small as a single setRef node,
        //               or as large as the parse tree
        //               for an entire expression (this will be the one item left on the stack
        //               when the parsing of an RBBI rule completes.
        //
        //               This function is called when a binary operator is encountered.
        //               It looks back up the stack for operators that are not yet associated
        //               with a right operand, and if the precedence of the stacked operator >=
        //               the precedence of the current operator, binds the operand left,
        //               to the previously encountered operator.
        //
        //----------------------------------------------------------------------------------------
        internal virtual void FixOpStack(int p)
        {
            RBBINode n;
            // printNodeStack("entering fixOpStack()");
            for (; ; )
            {
                n = fNodeStack[fNodeStackPtr - 1]; // an operator node
                if (n.fPrecedence == 0)
                {
                    Console.Out.Write("RBBIRuleScanner.fixOpStack, bad operator node");
                    Error(RBBIRuleBuilder.U_BRK_INTERNAL_ERROR);
                    return;
                }

                if (n.fPrecedence < p || n.fPrecedence <= RBBINode.precLParen)
                {
                    // The most recent operand goes with the current operator,
                    //   not with the previously stacked one.
                    break;
                }
                // Stack operator is a binary op ( '|' or concatenation)
                //   TOS operand becomes right child of this operator.
                //   Resulting subexpression becomes the TOS operand.
                n.fRightChild = fNodeStack[fNodeStackPtr];
                fNodeStack[fNodeStackPtr].fParent = n;
                fNodeStackPtr--;
                // printNodeStack("looping in fixOpStack() ");
            }

            if (p <= RBBINode.precLParen)
            {
                // Scan is at a right paren or end of expression.
                //  The scanned item must match the stack, or else there was an
                // error.
                //  Discard the left paren (or start expr) node from the stack,
                //  leaving the completed (sub)expression as TOS.
                if (n.fPrecedence != p)
                {
                    // Right paren encountered matched start of expression node, or
                    // end of expression matched with a left paren node.
                    Error(RBBIRuleBuilder.U_BRK_MISMATCHED_PAREN);
                }
                fNodeStack[fNodeStackPtr - 1] = fNodeStack[fNodeStackPtr];
                fNodeStackPtr--;
                // Delete the now-discarded LParen or Start node.
                // delete n;
            }
            // printNodeStack("leaving fixOpStack()");
        }

        //----------------------------------------------------------------------------
        //
        //       RBBISetTableEl is an entry in the hash table of UnicodeSets that have
        //                        been encountered. The val Node will be of nodetype uset
        //                        and contain pointers to the actual UnicodeSets.
        //                        The Key is the source string for initializing the set.
        //
        //                        The hash table is used to avoid creating duplicate
        //                        unnamed (not $var references) UnicodeSets.
        //
        //----------------------------------------------------------------------------
        internal class RBBISetTableEl
        {
            public string Key { get; set; }

            public RBBINode Val { get; set; }
        }


        //----------------------------------------------------------------------------------------
        //
        //   findSetFor given a String,
        //                  - find the corresponding Unicode Set (uset node)
        //                         (create one if necessary)
        //                  - Set fLeftChild of the caller's node (should be a setRef node)
        //                         to the uset node
        //                 Maintain a hash table of uset nodes, so the same one is always used
        //                    for the same string.
        //                 If a "to adopt" set is provided and we haven't seen this key before,
        //                    add the provided set to the hash table.
        //                 If the string is one (32 bit) char in length, the set contains
        //                    just one element which is the char in question.
        //                 If the string is "any", return a set containing all chars.
        //
        //----------------------------------------------------------------------------------------
        internal virtual void FindSetFor(String s, RBBINode node, UnicodeSet setToAdopt)
        {

            RBBISetTableEl el;

            // First check whether we've already cached a set for this string.
            // If so, just use the cached set in the new node.
            //   delete any set provided by the caller, since we own it.
            el = fSetTable.Get(s);
            if (el != null)
            {
                node.fLeftChild = el.Val;
                Assert.Assrt(node.fLeftChild.fType == RBBINode.uset);
                return;
            }

            // Haven't seen this set before.
            // If the caller didn't provide us with a prebuilt set,
            //   create a new UnicodeSet now.
            if (setToAdopt == null)
            {
                if (s.Equals(kAny))
                {
                    setToAdopt = new UnicodeSet(0x000000, 0x10ffff);
                }
                else
                {
                    int c;
                    c = UTF16.CharAt(s, 0);
                    setToAdopt = new UnicodeSet(c, c);
                }
            }

            //
            // Make a new uset node to refer to this UnicodeSet
            // This new uset node becomes the child of the caller's setReference
            // node.
            //
            RBBINode usetNode = new RBBINode(RBBINode.uset);
            usetNode.fInputSet = setToAdopt;
            usetNode.fParent = node;
            node.fLeftChild = usetNode;
            usetNode.fText = s;

            //
            // Add the new uset node to the list of all uset nodes.
            //
            fRB.fUSetNodes.Add(usetNode);

            //
            // Add the new set to the set hash table.
            //
            el = new RBBISetTableEl();
            el.Key = s;
            el.Val = usetNode;
            fSetTable[el.Key]= el;

            return;
        }

        //
        //  Assorted Unicode character constants.
        //     Numeric because there is no portable way to enter them as literals.
        //     (Think EBCDIC).
        //
        internal static readonly int chNEL = 0x85; //    NEL newline variant

        internal static readonly int chLS = 0x2028; //    Unicode Line Separator

        //----------------------------------------------------------------------------------------
        //
        //  stripRules    Return a rules string without unnecessary
        //                characters.
        //
        //----------------------------------------------------------------------------------------
        internal static string StripRules(String rules)
        {
            StringBuilder strippedRules = new StringBuilder();
            int rulesLength = rules.Length;
            for (int idx = 0; idx < rulesLength;)
            {
                char ch = rules[idx++];
                if (ch == '#')
                {
                    while (idx < rulesLength
                            && ch != '\r' && ch != '\n' && ch != chNEL)
                    {
                        ch = rules[idx++];
                    }
                }
                if (!UCharacter.IsISOControl(ch))
                {
                    strippedRules.Append(ch);
                }
            }
            return strippedRules.ToString();
        }

        //----------------------------------------------------------------------------------------
        //
        //  nextCharLL    Low Level Next Char from rule input source.
        //                Get a char from the input character iterator,
        //                keep track of input position for error reporting.
        //
        //----------------------------------------------------------------------------------------
        internal virtual int NextCharLL()
        {
            int ch;

            if (fNextIndex >= fRB.fRules.Length)
            {
                return -1;
            }
            ch = UTF16.CharAt(fRB.fRules, fNextIndex);
            fNextIndex = UTF16.MoveCodePointOffset(fRB.fRules, fNextIndex, 1);

            if (ch == '\r' ||
                ch == chNEL ||
                ch == chLS ||
                ch == '\n' && fLastChar != '\r')
            {
                // Character is starting a new line.  Bump up the line number, and
                //  reset the column to 0.
                fLineNum++;
                fCharNum = 0;
                if (fQuoteMode)
                {
                    Error(RBBIRuleBuilder.U_BRK_NEW_LINE_IN_QUOTED_STRING);
                    fQuoteMode = false;
                }
            }
            else
            {
                // Character is not starting a new line.  Except in the case of a
                //   LF following a CR, increment the column position.
                if (ch != '\n')
                {
                    fCharNum++;
                }
            }
            fLastChar = ch;
            return ch;
        }

        //---------------------------------------------------------------------------------
        //
        //   nextChar     for rules scanning.  At this level, we handle stripping
        //                out comments and processing backslash character escapes.
        //                The rest of the rules grammar is handled at the next level up.
        //
        //---------------------------------------------------------------------------------
        internal virtual void NextChar(RBBIRuleChar c)
        {

            // Unicode Character constants needed for the processing done by nextChar(),
            //   in hex because literals wont work on EBCDIC machines.

            fScanIndex = fNextIndex;
            c.fChar = NextCharLL();
            c.fEscaped = false;

            //
            //  check for '' sequence.
            //  These are recognized in all contexts, whether in quoted text or not.
            //
            if (c.fChar == '\'')
            {
                if (UTF16.CharAt(fRB.fRules, fNextIndex) == '\'')
                {
                    c.fChar = NextCharLL(); // get nextChar officially so character counts
                    c.fEscaped = true; //   stay correct.
                }
                else
                {
                    // Single quote, by itself.
                    //   Toggle quoting mode.
                    //   Return either '('  or ')', because quotes cause a grouping of the quoted text.
                    fQuoteMode = !fQuoteMode;
                    if (fQuoteMode == true)
                    {
                        c.fChar = '(';
                    }
                    else
                    {
                        c.fChar = ')';
                    }
                    c.fEscaped = false; // The paren that we return is not escaped.
                    return;
                }
            }

            if (fQuoteMode)
            {
                c.fEscaped = true;
            }
            else
            {
                // We are not in a 'quoted region' of the source.
                //
                if (c.fChar == '#')
                {
                    // Start of a comment.  Consume the rest of it.
                    //  The new-line char that terminates the comment is always returned.
                    //  It will be treated as white-space, and serves to break up anything
                    //    that might otherwise incorrectly clump together with a comment in
                    //    the middle (a variable name, for example.)
                    for (; ; )
                    {
                        c.fChar = NextCharLL();
                        if (c.fChar == -1 || // EOF
                            c.fChar == '\r' ||
                            c.fChar == '\n' ||
                            c.fChar == chNEL ||
                            c.fChar == chLS)
                        {
                            break;
                        }
                    }
                }
                if (c.fChar == -1)
                {
                    return;
                }

                //
                //  check for backslash escaped characters.
                //  Use String.unescapeAt() to handle them.
                //
                if (c.fChar == '\\')
                {
                    c.fEscaped = true;
                    int[] unescapeIndex = new int[1];
                    unescapeIndex[0] = fNextIndex;
                    c.fChar = Utility.UnescapeAt(fRB.fRules, unescapeIndex);
                    if (unescapeIndex[0] == fNextIndex)
                    {
                        Error(RBBIRuleBuilder.U_BRK_HEX_DIGITS_EXPECTED);
                    }

                    fCharNum += unescapeIndex[0] - fNextIndex;
                    fNextIndex = unescapeIndex[0];
                }
            }
            // putc(c.fChar, stdout);
        }

        //---------------------------------------------------------------------------------
        //
        //  Parse RBBI rules.   The state machine for rules parsing is here.
        //                      The state tables are hand-written in the file rbbirpt.txt,
        //                      and converted to the form used here by a perl
        //                      script rbbicst.pl
        //
        //---------------------------------------------------------------------------------
        internal virtual void Parse()
        {
            int state;
            RBBIRuleParseTable.RBBIRuleTableElement tableEl;

            state = 1;
            NextChar(fC);
            //
            // Main loop for the rule parsing state machine.
            //   Runs once per state transition.
            //   Each time through optionally performs, depending on the state table,
            //      - an advance to the the next input char
            //      - an action to be performed.
            //      - pushing or popping a state to/from the local state return stack.
            //
            for (; ; )
            {
                // Quit if state == 0.  This is the normal way to exit the state machine.
                //
                if (state == 0)
                {
                    break;
                }

                // Find the state table element that matches the input char from the rule, or the
                //    class of the input character.  Start with the first table row for this
                //    state, then linearly scan forward until we find a row that matches the
                //    character.  The last row for each state always matches all characters, so
                //    the search will stop there, if not before.
                //
                tableEl = RBBIRuleParseTable.gRuleParseStateTable[state];
                if (fRB.fDebugEnv != null && fRB.fDebugEnv.IndexOf("scan") >= 0)
                {
                    Console.Out.WriteLine("char, line, col = (\'" + (char)fC.fChar
                            + "\', " + fLineNum + ", " + fCharNum + "    state = "
                            + tableEl.fStateName);
                }

                for (int tableRow = state; ; tableRow++)
                { // loop over the state table rows associated with this state.
                    tableEl = RBBIRuleParseTable.gRuleParseStateTable[tableRow];
                    if (fRB.fDebugEnv != null && fRB.fDebugEnv.IndexOf("scan") >= 0)
                    {
                        Console.Out.Write(".");
                    }
                    if (tableEl.fCharClass < 127 && fC.fEscaped == false
                            && tableEl.fCharClass == fC.fChar)
                    {
                        // Table row specified an individual character, not a set, and
                        //   the input character is not escaped, and
                        //   the input character matched it.
                        break;
                    }
                    if (tableEl.fCharClass == 255)
                    {
                        // Table row specified default, match anything character class.
                        break;
                    }
                    if (tableEl.fCharClass == 254 && fC.fEscaped)
                    {
                        // Table row specified "escaped" and the char was escaped.
                        break;
                    }
                    if (tableEl.fCharClass == 253 && fC.fEscaped
                            && (fC.fChar == 0x50 || fC.fChar == 0x70))
                    {
                        // Table row specified "escaped P" and the char is either 'p' or 'P'.
                        break;
                    }
                    if (tableEl.fCharClass == 252 && fC.fChar == -1)
                    {
                        // Table row specified eof and we hit eof on the input.
                        break;
                    }

                    if (tableEl.fCharClass >= 128 && tableEl.fCharClass < 240 && // Table specs a char class &&
                            fC.fEscaped == false && //   char is not escaped &&
                            fC.fChar != -1)
                    { //   char is not EOF
                        UnicodeSet uniset = fRuleSets[tableEl.fCharClass - 128];
                        if (uniset.Contains(fC.fChar))
                        {
                            // Table row specified a character class, or set of characters,
                            //   and the current char matches it.
                            break;
                        }
                    }
                }

                if (fRB.fDebugEnv != null && fRB.fDebugEnv.IndexOf("scan") >= 0)
                {
                    Console.Out.WriteLine("");
                }
                //
                // We've found the row of the state table that matches the current input
                //   character from the rules string.
                // Perform any action specified  by this row in the state table.
                if (DoParseActions(tableEl.fAction) == false)
                {
                    // Break out of the state machine loop if the
                    //   the action signalled some kind of error, or
                    //   the action was to exit, occurs on normal end-of-rules-input.
                    break;
                }

                if (tableEl.fPushState != 0)
                {
                    fStackPtr++;
                    if (fStackPtr >= kStackSize)
                    {
                        Console.Out.WriteLine("RBBIRuleScanner.parse() - state stack overflow.");
                        Error(RBBIRuleBuilder.U_BRK_INTERNAL_ERROR);
                    }
                    fStack[fStackPtr] = tableEl.fPushState;
                }

                if (tableEl.fNextChar)
                {
                    NextChar(fC);
                }

                // Get the next state from the table entry, or from the
                //   state stack if the next state was specified as "pop".
                if (tableEl.fNextState != 255)
                {
                    state = tableEl.fNextState;
                }
                else
                {
                    state = fStack[fStackPtr];
                    fStackPtr--;
                    if (fStackPtr < 0)
                    {
                        Console.Out.WriteLine("RBBIRuleScanner.parse() - state stack underflow.");
                        Error(RBBIRuleBuilder.U_BRK_INTERNAL_ERROR);
                    }
                }

            }

            // If there are no forward rules throw an error.
            //
            if (fRB.fTreeRoots[RBBIRuleBuilder.fForwardTree] == null)
            {
                Error(RBBIRuleBuilder.U_BRK_RULE_SYNTAX);
            }

            //
            // If there were NO user specified reverse rules, set up the equivalent of ".*;"
            //
            if (fRB.fTreeRoots[RBBIRuleBuilder.fReverseTree] == null)
            {
                fRB.fTreeRoots[RBBIRuleBuilder.fReverseTree] = PushNewNode(RBBINode.opStar);
                RBBINode operand = PushNewNode(RBBINode.setRef);
                FindSetFor(kAny, operand, null);
                fRB.fTreeRoots[RBBIRuleBuilder.fReverseTree].fLeftChild = operand;
                operand.fParent = fRB.fTreeRoots[RBBIRuleBuilder.fReverseTree];
                fNodeStackPtr -= 2;
            }

            //
            // Parsing of the input RBBI rules is complete.
            // We now have a parse tree for the rule expressions
            // and a list of all UnicodeSets that are referenced.
            //
            if (fRB.fDebugEnv != null && fRB.fDebugEnv.IndexOf("symbols") >= 0)
            {
                fSymbolTable.RbbiSymtablePrint();
            }
            if (fRB.fDebugEnv != null && fRB.fDebugEnv.IndexOf("ptree") >= 0)
            {
                Console.Out.WriteLine("Completed Forward Rules Parse Tree...");
                fRB.fTreeRoots[RBBIRuleBuilder.fForwardTree].PrintTree(true);
                Console.Out.WriteLine("\nCompleted Reverse Rules Parse Tree...");
                fRB.fTreeRoots[RBBIRuleBuilder.fReverseTree].PrintTree(true);
                Console.Out.WriteLine("\nCompleted Safe Point Forward Rules Parse Tree...");
                if (fRB.fTreeRoots[RBBIRuleBuilder.fSafeFwdTree] == null)
                {
                    Console.Out.WriteLine("  -- null -- ");
                }
                else
                {
                    fRB.fTreeRoots[RBBIRuleBuilder.fSafeFwdTree].PrintTree(true);
                }
                Console.Out.WriteLine("\nCompleted Safe Point Reverse Rules Parse Tree...");
                if (fRB.fTreeRoots[RBBIRuleBuilder.fSafeRevTree] == null)
                {
                    Console.Out.WriteLine("  -- null -- ");
                }
                else
                {
                    fRB.fTreeRoots[RBBIRuleBuilder.fSafeRevTree].PrintTree(true);
                }
            }
        }

        //---------------------------------------------------------------------------------
        //
        //  printNodeStack     for debugging...
        //
        //---------------------------------------------------------------------------------
        ///CLOVER:OFF
        internal virtual void PrintNodeStack(string title)
        {
            int i;
            Console.Out.WriteLine(title + ".  Dumping node stack...\n");
            for (i = fNodeStackPtr; i > 0; i--)
            {
                fNodeStack[i].PrintTree(true);
            }
        }
        ///CLOVER:ON

        //---------------------------------------------------------------------------------
        //
        //  pushNewNode   create a new RBBINode of the specified type and push it
        //                onto the stack of nodes.
        //
        //---------------------------------------------------------------------------------
        internal virtual RBBINode PushNewNode(int nodeType)
        {
            fNodeStackPtr++;
            if (fNodeStackPtr >= kStackSize)
            {
                Console.Out.WriteLine("RBBIRuleScanner.pushNewNode - stack overflow.");
                Error(RBBIRuleBuilder.U_BRK_INTERNAL_ERROR);
            }
            fNodeStack[fNodeStackPtr] = new RBBINode(nodeType);
            return fNodeStack[fNodeStackPtr];
        }

        //---------------------------------------------------------------------------------
        //
        //  scanSet    Construct a UnicodeSet from the text at the current scan
        //             position.  Advance the scan position to the first character
        //             after the set.
        //
        //             A new RBBI setref node referring to the set is pushed onto the node
        //             stack.
        //
        //             The scan position is normally under the control of the state machine
        //             that controls rule parsing.  UnicodeSets, however, are parsed by
        //             the UnicodeSet constructor, not by the RBBI rule parser.
        //
        //---------------------------------------------------------------------------------
        internal virtual void ScanSet()
        {
            UnicodeSet uset = null;
            int startPos;
            ParsePosition pos = new ParsePosition(fScanIndex);
            int i;

            startPos = fScanIndex;
            try
            {
                uset = new UnicodeSet(fRB.fRules, pos, fSymbolTable, UnicodeSet.IGNORE_SPACE);
            }
            catch (Exception e)
            { // TODO:  catch fewer exception types.
              // Repackage UnicodeSet errors as RBBI rule builder errors, with location info.
                Error(RBBIRuleBuilder.U_BRK_MALFORMED_SET);
            }

            // Verify that the set contains at least one code point.
            //
            if (uset.IsEmpty)
            {
                // This set is empty.
                //  Make it an error, because it almost certainly is not what the user wanted.
                //  Also, avoids having to think about corner cases in the tree manipulation code
                //   that occurs later on.
                //  TODO:  this shouldn't be an error; it does happen.
                Error(RBBIRuleBuilder.U_BRK_RULE_EMPTY_SET);
            }

            // Advance the RBBI parse postion over the UnicodeSet pattern.
            //   Don't just set fScanIndex because the line/char positions maintained
            //   for error reporting would be thrown off.
            i = pos.Index;
            for (; ; )
            {
                if (fNextIndex >= i)
                {
                    break;
                }
                NextCharLL();
            }

            RBBINode n;

            n = PushNewNode(RBBINode.setRef);
            n.fFirstPos = startPos;
            n.fLastPos = fNextIndex;
            n.fText = fRB.fRules.Substring(n.fFirstPos, n.fLastPos - n.fFirstPos); // ICU4N: Corrected 2nd parameter
            //  findSetFor() serves several purposes here:
            //     - Adopts storage for the UnicodeSet, will be responsible for deleting.
            //     - Mantains collection of all sets in use, needed later for establishing
            //          character categories for run time engine.
            //     - Eliminates mulitiple instances of the same set.
            //     - Creates a new uset node if necessary (if this isn't a duplicate.)
            FindSetFor(n.fText, n, uset);
        }

    }
}
