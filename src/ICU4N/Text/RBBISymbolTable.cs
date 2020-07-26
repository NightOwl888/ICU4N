using ICU4N.Support.Collections;
using J2N.Collections.Generic.Extensions;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICU4N.Text
{
    internal class RBBISymbolTable : ISymbolTable
    {
        internal Dictionary<string, RBBISymbolTableEntry> fHashTable;
        internal RBBIRuleScanner fRuleScanner;

        // These next two fields are part of the mechanism for passing references to
        //   already-constructed UnicodeSets back to the UnicodeSet constructor
        //   when the pattern includes $variable references.
        internal string ffffString;
        internal UnicodeSet fCachedSetLookup;



        internal class RBBISymbolTableEntry
        {
            public string Key { get; set; }
            public RBBINode Val { get; set; }
        }


        internal RBBISymbolTable(RBBIRuleScanner rs)
        {
            fRuleScanner = rs;
            fHashTable = new Dictionary<string, RBBISymbolTableEntry>();
            ffffString = "\uffff";
        }

        //
        //  RBBISymbolTable::lookup       This function from the abstract symbol table inteface
        //                                looks up a variable name and returns a UnicodeString
        //                                containing the substitution text.
        //
        //                                The variable name does NOT include the leading $.
        //
        public virtual char[] Lookup(string s)
        {
            RBBISymbolTableEntry el;
            RBBINode varRefNode;
            RBBINode exprNode;

            RBBINode usetNode;
            String retString;

            el = fHashTable.Get(s);
            if (el == null)
            {
                return null;
            }

            // Walk through any chain of variable assignments that ultimately resolve to a Set Ref.
            varRefNode = el.Val;
            while (varRefNode.fLeftChild.fType == RBBINode.varRef)
            {
                varRefNode = varRefNode.fLeftChild;
            }

            exprNode = varRefNode.fLeftChild; // Root node of expression for variable
            if (exprNode.fType == RBBINode.setRef)
            {
                // The $variable refers to a single UnicodeSet
                //   return the ffffString, which will subsequently be interpreted as a
                //   stand-in character for the set by RBBISymbolTable::lookupMatcher()
                usetNode = exprNode.fLeftChild;
                fCachedSetLookup = usetNode.fInputSet;
                retString = ffffString;
            }
            else
            {
                // The variable refers to something other than just a set.
                // This is an error in the rules being compiled.  $Variables inside of UnicodeSets
                //   must refer only to another set, not to some random non-set expression.
                //   Note:  single characters are represented as sets, so they are ok.
                fRuleScanner.Error(RBBIRuleBuilder.U_BRK_MALFORMED_SET);
                retString = exprNode.fText;
                fCachedSetLookup = null;
            }
            return retString.ToCharArray();
        }

        //
        //  RBBISymbolTable::lookupMatcher   This function from the abstract symbol table
        //                                   interface maps a single stand-in character to a
        //                                   pointer to a Unicode Set.   The Unicode Set code uses this
        //                                   mechanism to get all references to the same $variable
        //                                   name to refer to a single common Unicode Set instance.
        //
        //    This implementation cheats a little, and does not maintain a map of stand-in chars
        //    to sets.  Instead, it takes advantage of the fact that  the UnicodeSet
        //    constructor will always call this function right after calling lookup(),
        //    and we just need to remember what set to return between these two calls.
        public virtual IUnicodeMatcher LookupMatcher(int ch)
        {
            UnicodeSet retVal = null;
            if (ch == 0xffff)
            {
                retVal = fCachedSetLookup;
                fCachedSetLookup = null;
            }
            return retVal;
        }

        //
        // RBBISymbolTable::parseReference   This function from the abstract symbol table interface
        //                                   looks for a $variable name in the source text.
        //                                   It does not look it up, only scans for it.
        //                                   It is used by the UnicodeSet parser.
        //
        public virtual string ParseReference(string text, ParsePosition pos, int limit)
        {
            int start = pos.Index;
            int i = start;
            string result = "";
            while (i < limit)
            {
                int c = UTF16.CharAt(text, i);
                if ((i == start && !UChar.IsUnicodeIdentifierStart(c))
                        || !UChar.IsUnicodeIdentifierPart(c))
                {
                    break;
                }
                i += UTF16.GetCharCount(c);
            }
            if (i == start)
            { // No valid name chars
                return result; // Indicate failure with empty string
            }
            pos.Index=i;
            result = text.Substring(start, i - start); // ICU4N: Corrected 2nd parameter
            return result;
        }

        //
        // RBBISymbolTable::lookupNode      Given a key (a variable name), return the
        //                                  corresponding RBBI Node.  If there is no entry
        //                                  in the table for this name, return NULL.
        //
        internal virtual RBBINode LookupNode(String key)
        {

            RBBINode retNode = null;
            RBBISymbolTableEntry el;

            el = fHashTable.Get(key);
            if (el != null)
            {
                retNode = el.Val;
            }
            return retNode;
        }

        //
        //    RBBISymbolTable::addEntry     Add a new entry to the symbol table.
        //                                  Indicate an error if the name already exists -
        //                                    this will only occur in the case of duplicate
        //                                    variable assignments.
        //
        internal virtual void AddEntry(string key, RBBINode val)
        {
            RBBISymbolTableEntry e;
            e = fHashTable.Get(key);
            if (e != null)
            {
                fRuleScanner.Error(RBBIRuleBuilder.U_BRK_VARIABLE_REDFINITION);
                return;
            }

            e = new RBBISymbolTableEntry();
            e.Key = key;
            e.Val = val;
            fHashTable[e.Key]= e;
        }

        //
        //  RBBISymbolTable::print    Debugging function, dump out the symbol table contents.
        //
        ///CLOVER:OFF
        internal virtual void RbbiSymtablePrint()
        {
            Console.Out.Write("Variable Definitions\n"
                        + "Name               Node Val     String Val\n"
                        + "----------------------------------------------------------------------\n");

            RBBISymbolTableEntry[] syms = fHashTable.Values.ToArray();

            for (int i = 0; i < syms.Length; i++)
            {
                RBBISymbolTableEntry s = syms[i];

                Console.Out.Write("  " + s.Key + "  "); // TODO:  format output into columns.
                Console.Out.Write("  " + s.Val + "  ");
                Console.Out.Write(s.Val.fLeftChild.fText);
                Console.Out.Write("\n");
            }

            Console.Out.WriteLine("\nParsed Variable Definitions\n");
            for (int i = 0; i < syms.Length; i++)
            {
                RBBISymbolTableEntry s = syms[i];
                Console.Out.Write(s.Key);
                s.Val.fLeftChild.PrintTree(true);
                Console.Out.Write("\n");
            }
        }
        //CLOVER:ON
    }
}
