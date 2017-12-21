using System;

namespace ICU4N.Text
{
    // ICU4N TODO: Make a T4 template to parse the file rbbirpt.txt and generate this class
    /// <summary>
    /// Generated .NET File.  Do not edit by hand.
    /// This file contains the state table for the ICU Rule Based Break Iterator
    /// rule parser.
    /// It is generated from the "RBBIRuleParseTable.tt" file using the 
    /// rule parser state definitions file "rbbirpt.txt".
    /// </summary>
    internal class RBBIRuleParseTable
    {
        internal const short doCheckVarDef = 1;
        internal const short doDotAny = 2;
        internal const short doEndAssign = 3;
        internal const short doEndOfRule = 4;
        internal const short doEndVariableName = 5;
        internal const short doExit = 6;
        internal const short doExprCatOperator = 7;
        internal const short doExprFinished = 8;
        internal const short doExprOrOperator = 9;
        internal const short doExprRParen = 10;
        internal const short doExprStart = 11;
        internal const short doLParen = 12;
        internal const short doNOP = 13;
        internal const short doNoChain = 14;
        internal const short doOptionEnd = 15;
        internal const short doOptionStart = 16;
        internal const short doReverseDir = 17;
        internal const short doRuleChar = 18;
        internal const short doRuleError = 19;
        internal const short doRuleErrorAssignExpr = 20;
        internal const short doScanUnicodeSet = 21;
        internal const short doSlash = 22;
        internal const short doStartAssign = 23;
        internal const short doStartTagValue = 24;
        internal const short doStartVariableName = 25;
        internal const short doTagDigit = 26;
        internal const short doTagExpectedError = 27;
        internal const short doTagValue = 28;
        internal const short doUnaryOpPlus = 29;
        internal const short doUnaryOpQuestion = 30;
        internal const short doUnaryOpStar = 31;
        internal const short doVariableNameExpectedErr = 32;

        internal const short kRuleSet_default = 255;
        internal const short kRuleSet_digit_char = 128;
        internal const short kRuleSet_eof = 252;
        internal const short kRuleSet_escaped = 254;
        internal const short kRuleSet_name_char = 129;
        internal const short kRuleSet_name_start_char = 130;
        internal const short kRuleSet_rule_char = 131;
        internal const short kRuleSet_white_space = 132;


        internal class RBBIRuleTableElement
        {
            internal short fAction;
            internal short fCharClass;
            internal short fNextState;
            internal short fPushState;
            internal bool fNextChar;
            internal string fStateName;
            internal RBBIRuleTableElement(short a, int cc, int ns, int ps, bool nc, String sn)
            {
                fAction = a;
                fCharClass = (short)cc;
                fNextState = (short)ns;
                fPushState = (short)ps;
                fNextChar = nc;
                fStateName = sn;
            }
        };

        internal static RBBIRuleTableElement[] gRuleParseStateTable = {
               new RBBIRuleTableElement(doNOP, 0, 0,0,  true,   null )     //  0 
             , new RBBIRuleTableElement(doExprStart, 254, 29, 9, false,   "start")     //  1 
             , new RBBIRuleTableElement(doNOP, 132, 1,0,  true,   null )     //  2 
             , new RBBIRuleTableElement(doNoChain,'^',  12, 9, true,   null )     //  3 
             , new RBBIRuleTableElement(doExprStart,'$',  88, 98, false,   null )     //  4 
             , new RBBIRuleTableElement(doNOP,'!',  19,0,  true,   null )     //  5 
             , new RBBIRuleTableElement(doNOP,';',  1,0,  true,   null )     //  6 
             , new RBBIRuleTableElement(doNOP, 252, 0,0,  false,   null )     //  7 
             , new RBBIRuleTableElement(doExprStart, 255, 29, 9, false,   null )     //  8 
             , new RBBIRuleTableElement(doEndOfRule,';',  1,0,  true,   "break-rule-end")     //  9 
             , new RBBIRuleTableElement(doNOP, 132, 9,0,  true,   null )     //  10 
             , new RBBIRuleTableElement(doRuleError, 255, 103,0,  false,   null )     //  11 
             , new RBBIRuleTableElement(doExprStart, 254, 29,0,  false,   "start-after-caret")     //  12 
             , new RBBIRuleTableElement(doNOP, 132, 12,0,  true,   null )     //  13 
             , new RBBIRuleTableElement(doRuleError,'^',  103,0,  false,   null )     //  14 
             , new RBBIRuleTableElement(doExprStart,'$',  88, 37, false,   null )     //  15 
             , new RBBIRuleTableElement(doRuleError,';',  103,0,  false,   null )     //  16 
             , new RBBIRuleTableElement(doRuleError, 252, 103,0,  false,   null )     //  17 
             , new RBBIRuleTableElement(doExprStart, 255, 29,0,  false,   null )     //  18 
             , new RBBIRuleTableElement(doNOP,'!',  21,0,  true,   "rev-option")     //  19 
             , new RBBIRuleTableElement(doReverseDir, 255, 28, 9, false,   null )     //  20 
             , new RBBIRuleTableElement(doOptionStart, 130, 23,0,  true,   "option-scan1")     //  21 
             , new RBBIRuleTableElement(doRuleError, 255, 103,0,  false,   null )     //  22 
             , new RBBIRuleTableElement(doNOP, 129, 23,0,  true,   "option-scan2")     //  23 
             , new RBBIRuleTableElement(doOptionEnd, 255, 25,0,  false,   null )     //  24 
             , new RBBIRuleTableElement(doNOP,';',  1,0,  true,   "option-scan3")     //  25 
             , new RBBIRuleTableElement(doNOP, 132, 25,0,  true,   null )     //  26 
             , new RBBIRuleTableElement(doRuleError, 255, 103,0,  false,   null )     //  27 
             , new RBBIRuleTableElement(doExprStart, 255, 29, 9, false,   "reverse-rule")     //  28 
             , new RBBIRuleTableElement(doRuleChar, 254, 38,0,  true,   "term")     //  29 
             , new RBBIRuleTableElement(doNOP, 132, 29,0,  true,   null )     //  30 
             , new RBBIRuleTableElement(doRuleChar, 131, 38,0,  true,   null )     //  31 
             , new RBBIRuleTableElement(doNOP,'[',  94, 38, false,   null )     //  32 
             , new RBBIRuleTableElement(doLParen,'(',  29, 38, true,   null )     //  33 
             , new RBBIRuleTableElement(doNOP,'$',  88, 37, false,   null )     //  34 
             , new RBBIRuleTableElement(doDotAny,'.',  38,0,  true,   null )     //  35 
             , new RBBIRuleTableElement(doRuleError, 255, 103,0,  false,   null )     //  36 
             , new RBBIRuleTableElement(doCheckVarDef, 255, 38,0,  false,   "term-var-ref")     //  37 
             , new RBBIRuleTableElement(doNOP, 132, 38,0,  true,   "expr-mod")     //  38 
             , new RBBIRuleTableElement(doUnaryOpStar,'*',  43,0,  true,   null )     //  39 
             , new RBBIRuleTableElement(doUnaryOpPlus,'+',  43,0,  true,   null )     //  40 
             , new RBBIRuleTableElement(doUnaryOpQuestion,'?',  43,0,  true,   null )     //  41 
             , new RBBIRuleTableElement(doNOP, 255, 43,0,  false,   null )     //  42 
             , new RBBIRuleTableElement(doExprCatOperator, 254, 29,0,  false,   "expr-cont")     //  43 
             , new RBBIRuleTableElement(doNOP, 132, 43,0,  true,   null )     //  44 
             , new RBBIRuleTableElement(doExprCatOperator, 131, 29,0,  false,   null )     //  45 
             , new RBBIRuleTableElement(doExprCatOperator,'[',  29,0,  false,   null )     //  46 
             , new RBBIRuleTableElement(doExprCatOperator,'(',  29,0,  false,   null )     //  47 
             , new RBBIRuleTableElement(doExprCatOperator,'$',  29,0,  false,   null )     //  48 
             , new RBBIRuleTableElement(doExprCatOperator,'.',  29,0,  false,   null )     //  49 
             , new RBBIRuleTableElement(doExprCatOperator,'/',  55,0,  false,   null )     //  50 
             , new RBBIRuleTableElement(doExprCatOperator,'{',  67,0,  true,   null )     //  51 
             , new RBBIRuleTableElement(doExprOrOperator,'|',  29,0,  true,   null )     //  52 
             , new RBBIRuleTableElement(doExprRParen,')',  255,0,  true,   null )     //  53 
             , new RBBIRuleTableElement(doExprFinished, 255, 255,0,  false,   null )     //  54 
             , new RBBIRuleTableElement(doSlash,'/',  57,0,  true,   "look-ahead")     //  55 
             , new RBBIRuleTableElement(doNOP, 255, 103,0,  false,   null )     //  56 
             , new RBBIRuleTableElement(doExprCatOperator, 254, 29,0,  false,   "expr-cont-no-slash")     //  57 
             , new RBBIRuleTableElement(doNOP, 132, 43,0,  true,   null )     //  58 
             , new RBBIRuleTableElement(doExprCatOperator, 131, 29,0,  false,   null )     //  59 
             , new RBBIRuleTableElement(doExprCatOperator,'[',  29,0,  false,   null )     //  60 
             , new RBBIRuleTableElement(doExprCatOperator,'(',  29,0,  false,   null )     //  61 
             , new RBBIRuleTableElement(doExprCatOperator,'$',  29,0,  false,   null )     //  62 
             , new RBBIRuleTableElement(doExprCatOperator,'.',  29,0,  false,   null )     //  63 
             , new RBBIRuleTableElement(doExprOrOperator,'|',  29,0,  true,   null )     //  64 
             , new RBBIRuleTableElement(doExprRParen,')',  255,0,  true,   null )     //  65 
             , new RBBIRuleTableElement(doExprFinished, 255, 255,0,  false,   null )     //  66 
             , new RBBIRuleTableElement(doNOP, 132, 67,0,  true,   "tag-open")     //  67 
             , new RBBIRuleTableElement(doStartTagValue, 128, 70,0,  false,   null )     //  68 
             , new RBBIRuleTableElement(doTagExpectedError, 255, 103,0,  false,   null )     //  69 
             , new RBBIRuleTableElement(doNOP, 132, 74,0,  true,   "tag-value")     //  70 
             , new RBBIRuleTableElement(doNOP,'}',  74,0,  false,   null )     //  71 
             , new RBBIRuleTableElement(doTagDigit, 128, 70,0,  true,   null )     //  72 
             , new RBBIRuleTableElement(doTagExpectedError, 255, 103,0,  false,   null )     //  73 
             , new RBBIRuleTableElement(doNOP, 132, 74,0,  true,   "tag-close")     //  74 
             , new RBBIRuleTableElement(doTagValue,'}',  77,0,  true,   null )     //  75 
             , new RBBIRuleTableElement(doTagExpectedError, 255, 103,0,  false,   null )     //  76 
             , new RBBIRuleTableElement(doExprCatOperator, 254, 29,0,  false,   "expr-cont-no-tag")     //  77 
             , new RBBIRuleTableElement(doNOP, 132, 77,0,  true,   null )     //  78 
             , new RBBIRuleTableElement(doExprCatOperator, 131, 29,0,  false,   null )     //  79 
             , new RBBIRuleTableElement(doExprCatOperator,'[',  29,0,  false,   null )     //  80 
             , new RBBIRuleTableElement(doExprCatOperator,'(',  29,0,  false,   null )     //  81 
             , new RBBIRuleTableElement(doExprCatOperator,'$',  29,0,  false,   null )     //  82 
             , new RBBIRuleTableElement(doExprCatOperator,'.',  29,0,  false,   null )     //  83 
             , new RBBIRuleTableElement(doExprCatOperator,'/',  55,0,  false,   null )     //  84 
             , new RBBIRuleTableElement(doExprOrOperator,'|',  29,0,  true,   null )     //  85 
             , new RBBIRuleTableElement(doExprRParen,')',  255,0,  true,   null )     //  86 
             , new RBBIRuleTableElement(doExprFinished, 255, 255,0,  false,   null )     //  87 
             , new RBBIRuleTableElement(doStartVariableName,'$',  90,0,  true,   "scan-var-name")     //  88 
             , new RBBIRuleTableElement(doNOP, 255, 103,0,  false,   null )     //  89 
             , new RBBIRuleTableElement(doNOP, 130, 92,0,  true,   "scan-var-start")     //  90 
             , new RBBIRuleTableElement(doVariableNameExpectedErr, 255, 103,0,  false,   null )     //  91 
             , new RBBIRuleTableElement(doNOP, 129, 92,0,  true,   "scan-var-body")     //  92 
             , new RBBIRuleTableElement(doEndVariableName, 255, 255,0,  false,   null )     //  93 
             , new RBBIRuleTableElement(doScanUnicodeSet,'[',  255,0,  true,   "scan-unicode-set")     //  94 
             , new RBBIRuleTableElement(doScanUnicodeSet,'p',  255,0,  true,   null )     //  95 
             , new RBBIRuleTableElement(doScanUnicodeSet,'P',  255,0,  true,   null )     //  96 
             , new RBBIRuleTableElement(doNOP, 255, 103,0,  false,   null )     //  97 
             , new RBBIRuleTableElement(doNOP, 132, 98,0,  true,   "assign-or-rule")     //  98 
             , new RBBIRuleTableElement(doStartAssign,'=',  29, 101, true,   null )     //  99 
             , new RBBIRuleTableElement(doNOP, 255, 37, 9, false,   null )     //  100 
             , new RBBIRuleTableElement(doEndAssign,';',  1,0,  true,   "assign-end")     //  101 
             , new RBBIRuleTableElement(doRuleErrorAssignExpr, 255, 103,0,  false,   null )     //  102 
             , new RBBIRuleTableElement(doExit, 255, 103,0,  true,   "errorDeath")     //  103 
        };
    }
}
