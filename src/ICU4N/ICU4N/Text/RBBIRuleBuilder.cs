using ICU4N.Impl;
using ICU4N.Support.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ICU4N.Text
{
    internal class RBBIRuleBuilder
    {
        //   This is the main class for building (compiling) break rules into the tables
        //    required by the runtime RBBI engine.
        //

        internal string fDebugEnv;              // controls debug trace output
        internal string fRules;                 // The rule string that we are compiling
        internal RBBIRuleScanner fScanner;      // The scanner.


        //
        //  There are four separate parse trees generated, one for each of the
        //    forward rules, reverse rules, safe forward rules and safe reverse rules.
        //  This array references the root of each of the trees.
        //
        internal RBBINode[] fTreeRoots = new RBBINode[4];
        internal static readonly int fForwardTree = 0;  // Indexes into the above fTreeRoots array
        internal static readonly int fReverseTree = 1;  //   for each of the trees.
        internal static readonly int fSafeFwdTree = 2;  //   (in C, these are pointer variables and
        internal static readonly int fSafeRevTree = 3;  //    there is no array.)
        internal int fDefaultTree = fForwardTree;      // For rules not qualified with a !
                                                       //   the tree to which they belong to.

        internal bool fChainRules;                  // True for chained Unicode TR style rules.
                                                    // False for traditional regexp rules.

        internal bool fLBCMNoChain;                 // True:  suppress chaining of rules on
                                                    //   chars with LineBreak property == CM.

        internal bool fLookAheadHardBreak;          // True:  Look ahead matches cause an
                                                    // immediate break, no continuing for the
                                                    // longest match.

        internal RBBISetBuilder fSetBuilder;           // Set and Character Category builder.
        internal IList<RBBINode> fUSetNodes;            // Vector of all uset nodes.
        internal RBBITableBuilder fForwardTables;      // State transition tables
        internal RBBITableBuilder fReverseTables;
        internal RBBITableBuilder fSafeFwdTables;
        internal RBBITableBuilder fSafeRevTables;

        //
        // Status {tag} values.   These structures are common to all of the rule sets (Forward, Reverse, etc.).
        //
        internal IDictionary<ISet<int>, int?> fStatusSets = new Dictionary<ISet<int>, int?>(); // Status value sets encountered so far.
                                                                                             //  Map Key is the set of values.
                                                                                             //  Map Value is the runtime array index.

        internal List<int> fRuleStatusVals;        // List of Integer objects.  Has same layout as the
                                                    //   runtime array of status (tag) values -
                                                    //     number of values in group 1
                                                    //        first status value in group 1
                                                    //        2nd status value in group 1
                                                    //        ...
                                                    //     number of values in group 2
                                                    //        first status value in group 2
                                                    //        etc.
                                                    //
                                                    // Error codes from ICU4C.
                                                    //    using these simplified the porting, and consolidated the
                                                    //    creation of Java exceptions
                                                    //
        internal static readonly int U_BRK_ERROR_START = 0x10200;
        /**< Start of codes indicating Break Iterator failures */

        internal static readonly int U_BRK_INTERNAL_ERROR = 0x10201;
        /**< An internal error (bug) was detected.             */

        internal static readonly int U_BRK_HEX_DIGITS_EXPECTED = 0x10202;
        /**< Hex digits expected as part of a escaped char in a rule. */

        internal static readonly int U_BRK_SEMICOLON_EXPECTED = 0x10203;
        /**< Missing ';' at the end of a RBBI rule.            */

        internal static readonly int U_BRK_RULE_SYNTAX = 0x10204;
        /**< Syntax error in RBBI rule.                        */

        internal static readonly int U_BRK_UNCLOSED_SET = 0x10205;
        /**< UnicodeSet witing an RBBI rule missing a closing ']'.  */

        internal static readonly int U_BRK_ASSIGN_ERROR = 0x10206;
        /**< Syntax error in RBBI rule assignment statement.   */

        internal static readonly int U_BRK_VARIABLE_REDFINITION = 0x10207;
        /**< RBBI rule $Variable redefined.                    */

        internal static readonly int U_BRK_MISMATCHED_PAREN = 0x10208;
        /**< Mis-matched parentheses in an RBBI rule.          */

        internal static readonly int U_BRK_NEW_LINE_IN_QUOTED_STRING = 0x10209;
        /**< Missing closing quote in an RBBI rule.            */

        internal static readonly int U_BRK_UNDEFINED_VARIABLE = 0x1020a;
        /**< Use of an undefined $Variable in an RBBI rule.    */

        internal static readonly int U_BRK_INIT_ERROR = 0x1020b;
        /**< Initialization failure.  Probable missing ICU Data. */

        internal static readonly int U_BRK_RULE_EMPTY_SET = 0x1020c;
        /**< Rule contains an empty Unicode Set.               */

        internal static readonly int U_BRK_UNRECOGNIZED_OPTION = 0x1020d;
        /**< !!option in RBBI rules not recognized.            */

        internal static readonly int U_BRK_MALFORMED_RULE_TAG = 0x1020e;
        /**< The {nnn} tag on a rule is mal formed             */
        internal static readonly int U_BRK_MALFORMED_SET = 0x1020f;

        internal static readonly int U_BRK_ERROR_LIMIT = 0x10210;
        /**< This must always be the last value to indicate the limit for Break Iterator failures */


        //----------------------------------------------------------------------------------------
        //
        //  Constructor.
        //
        //----------------------------------------------------------------------------------------
        internal RBBIRuleBuilder(String rules)
        {
            fDebugEnv = ICUDebug.Enabled("rbbi") ?
                                ICUDebug.Value("rbbi") : null;
            fRules = rules;
            fUSetNodes = new List<RBBINode>();
            fRuleStatusVals = new List<int>();
            fScanner = new RBBIRuleScanner(this);
            fSetBuilder = new RBBISetBuilder(this);
        }

        //----------------------------------------------------------------------------------------
        //
        //   flattenData() -  Collect up the compiled RBBI rule data and put it into
        //                    the format for saving in ICU data files,
        //
        //                    See the ICU4C file common/rbidata.h for a detailed description.
        //
        //----------------------------------------------------------------------------------------
        internal static int Align8(int i)
        {
            return (i + 7) & unchecked((int)0xfffffff8);
        }

        internal void FlattenData(Stream os)
        {
            DataOutputStream dos = new DataOutputStream(os);
            int i;

            //  Remove comments and whitespace from the rules to make it smaller.
            string strippedRules = RBBIRuleScanner.StripRules(fRules);

            // Calculate the size of each section in the data in bytes.
            //   Sizes here are padded up to a multiple of 8 for better memory alignment.
            //   Sections sizes actually stored in the header are for the actual data
            //     without the padding.
            //
            int headerSize = 24 * 4;     // align8(sizeof(RBBIDataHeader));
            int forwardTableSize = Align8(fForwardTables.GetTableSize());
            int reverseTableSize = Align8(fReverseTables.GetTableSize());
            // int safeFwdTableSize = Align8(fSafeFwdTables.getTableSize());
            int safeRevTableSize = Align8(fSafeRevTables.GetTableSize());
            int trieSize = Align8(fSetBuilder.GetTrieSize());
            int statusTableSize = Align8(fRuleStatusVals.Count * 4);
            int rulesSize = Align8((strippedRules.Length) * 2);

            int totalSize = headerSize
                    + forwardTableSize
                    + /* reverseTableSize */ 0
                    + /* safeFwdTableSize */ 0
                    + (safeRevTableSize > 0 ? safeRevTableSize : reverseTableSize)
                    + statusTableSize + trieSize + rulesSize;
            int outputPos = 0;               // Track stream position, starting from RBBIDataHeader.

            //
            // Write out an ICU Data Header
            //
            ICUBinary.WriteHeader(RBBIDataWrapper.DATA_FORMAT, RBBIDataWrapper.FORMAT_VERSION, 0, dos);

            //
            // Write out the RBBIDataHeader
            //
            int[] header = new int[RBBIDataWrapper.DH_SIZE];                 // sizeof struct RBBIDataHeader
            header[RBBIDataWrapper.DH_MAGIC] = 0xb1a0;
            header[RBBIDataWrapper.DH_FORMATVERSION] = RBBIDataWrapper.FORMAT_VERSION;
            header[RBBIDataWrapper.DH_LENGTH] = totalSize;            // fLength, the total size of all rule sections.
            header[RBBIDataWrapper.DH_CATCOUNT] = fSetBuilder.NumCharCategories; // fCatCount.

            // Only save the forward table and the safe reverse table,
            // because these are the only ones used at run-time.
            //
            // For the moment, we still build the other tables if they are present in the rule source files,
            // for backwards compatibility. Old rule files need to work, and this is the simplest approach.
            //
            // Additional backwards compatibility consideration: if no safe rules are provided, consider the
            // reverse rules to actually be the safe reverse rules.

            header[RBBIDataWrapper.DH_FTABLE] = headerSize;           // fFTable
            header[RBBIDataWrapper.DH_FTABLELEN] = forwardTableSize;     // fTableLen

            // Do not save Reverse Table.
            header[RBBIDataWrapper.DH_RTABLE] = header[RBBIDataWrapper.DH_FTABLE] + forwardTableSize; // fRTable
            header[RBBIDataWrapper.DH_RTABLELEN] = 0;                    // fRTableLen

            // Do not save the Safe Forward table.
            header[RBBIDataWrapper.DH_SFTABLE] = header[RBBIDataWrapper.DH_RTABLE]
                                                         + 0;                // fSTable
            header[RBBIDataWrapper.DH_SFTABLELEN] = 0;                    // fSTableLen

            // Safe reverse table. Use if present, otherwise save regular reverse table as the safe reverse.
            header[RBBIDataWrapper.DH_SRTABLE] = header[RBBIDataWrapper.DH_SFTABLE]
                                                         + 0;                // fSRTable
            if (safeRevTableSize > 0)
            {
                header[RBBIDataWrapper.DH_SRTABLELEN] = safeRevTableSize;
            }
            else
            {
                Debug.Assert(reverseTableSize > 0);
                header[RBBIDataWrapper.DH_SRTABLELEN] = reverseTableSize;
            }

            header[RBBIDataWrapper.DH_TRIE] = header[RBBIDataWrapper.DH_SRTABLE]
                                                                 + header[RBBIDataWrapper.DH_SRTABLELEN]; // fTrie
            header[RBBIDataWrapper.DH_TRIELEN] = fSetBuilder.GetTrieSize(); // fTrieLen
            header[RBBIDataWrapper.DH_STATUSTABLE] = header[RBBIDataWrapper.DH_TRIE]
                                                         + header[RBBIDataWrapper.DH_TRIELEN];
            header[RBBIDataWrapper.DH_STATUSTABLELEN] = statusTableSize; // fStatusTableLen
            header[RBBIDataWrapper.DH_RULESOURCE] = header[RBBIDataWrapper.DH_STATUSTABLE]
                                                         + statusTableSize;
            header[RBBIDataWrapper.DH_RULESOURCELEN] = strippedRules.Length * 2;
            for (i = 0; i < header.Length; i++)
            {
                dos.WriteInt32(header[i]);
                outputPos += 4;
            }

            // Write out the actual state tables.
            short[] tableData;
            tableData = fForwardTables.ExportTable();
            Assert.Assrt(outputPos == header[4]);
            for (i = 0; i < tableData.Length; i++)
            {
                dos.WriteInt16(tableData[i]);
                outputPos += 2;
            }

            /* do not write the reverse table
            tableData = fReverseTables.exportTable();
            Assert.Assrt(outputPos == header[6]);
            for (i = 0; i < tableData.length; i++) {
                dos.WriteInt16(tableData[i]);
                outputPos += 2;
            }
            */

            /* do not write safe forwards table
            Assert.Assrt(outputPos == header[8]);
            tableData = fSafeFwdTables.exportTable();
            for (i = 0; i < tableData.length; i++) {
                dos.WriteInt16(tableData[i]);
                outputPos += 2;
            }
            */

            // Write the safe reverse table.
            // If not present, write the plain reverse table (old style rule compatibility)
            Assert.Assrt(outputPos == header[10]);
            if (safeRevTableSize > 0)
            {
                tableData = fSafeRevTables.ExportTable();
            }
            else
            {
                tableData = fReverseTables.ExportTable();
            }
            for (i = 0; i < tableData.Length; i++)
            {
                dos.WriteInt16(tableData[i]);
                outputPos += 2;
            }

            // write out the Trie table
            Assert.Assrt(outputPos == header[12]);
            fSetBuilder.SerializeTrie(os);
            outputPos += header[13];
            while (outputPos % 8 != 0)
            { // pad to an 8 byte boundary
                dos.Write(0);
                outputPos += 1;
            }

            // Write out the status {tag} table.
            Assert.Assrt(outputPos == header[16]);
            foreach (var val in fRuleStatusVals)
            {
                dos.WriteInt32(val);
                outputPos += 4;
            }

            while (outputPos % 8 != 0)
            { // pad to an 8 byte boundary
                dos.Write(0);
                outputPos += 1;
            }

            // Write out the stripped rules (rules with extra spaces removed
            //   These go last in the data area, even though they are not last in the header.
            Assert.Assrt(outputPos == header[14]);
            dos.WriteChars(strippedRules);
            outputPos += strippedRules.Length * 2;
            while (outputPos % 8 != 0)
            { // pad to an 8 byte boundary
                dos.Write(0);
                outputPos += 1;
            }
        }

        //----------------------------------------------------------------------------------------
        //
        //  compileRules          compile source rules, placing the compiled form into a output stream
        //                        The compiled form is identical to that from ICU4C (Big Endian).
        //
        //----------------------------------------------------------------------------------------
        internal static void CompileRules(string rules, Stream os)
        {
            //
            // Read the input rules, generate a parse tree, symbol table,
            // and list of all Unicode Sets referenced by the rules.
            //
            RBBIRuleBuilder builder = new RBBIRuleBuilder(rules);
            builder.fScanner.Parse();

            //
            // UnicodeSet processing.
            //    Munge the Unicode Sets to create a set of character categories.
            //    Generate the mapping tables (TRIE) from input 32-bit characters to
            //    the character categories.
            //
            builder.fSetBuilder.Build();

            //
            //   Generate the DFA state transition table.
            //
            builder.fForwardTables = new RBBITableBuilder(builder, fForwardTree);
            builder.fReverseTables = new RBBITableBuilder(builder, fReverseTree);
            builder.fSafeFwdTables = new RBBITableBuilder(builder, fSafeFwdTree);
            builder.fSafeRevTables = new RBBITableBuilder(builder, fSafeRevTree);
            builder.fForwardTables.Build();
            builder.fReverseTables.Build();
            builder.fSafeFwdTables.Build();
            builder.fSafeRevTables.Build();
            if (builder.fDebugEnv != null
                    && builder.fDebugEnv.IndexOf("states") >= 0)
            {
                builder.fForwardTables.PrintRuleStatusTable();
            }

            //
            //   Package up the compiled data, writing it to an output stream
            //      in the serialization format.  This is the same as the ICU4C runtime format.
            //
            builder.FlattenData(os);
        }
    }
}
