using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ICU4N.Text.MessagePatternUtil;

namespace ICU4N.Dev.Test.Format
{
    /// <summary>
    /// Test MessagePatternUtil (MessagePattern-as-tree-of-nodes API)
    /// by building parallel trees of nodes and verifying that they match.
    /// </summary>
    public sealed class MessagePatternUtilTest : TestFmwk
    {
        // The following nested "Expect..." classes are used to build
        // a tree structure parallel to what the MessagePatternUtil class builds.
        // These nested test classes are not static so that they have access to TestFmwk methods.

        internal class ExpectMessageNode
        {
            internal ExpectMessageNode ExpectTextThatContains(String s)
            {
                contents.Add(new ExpectTextNode(s));
                return this;
            }
            internal ExpectMessageNode ExpectReplaceNumber()
            {
                contents.Add(new ExpectMessageContentsNode());
                return this;
            }
            internal ExpectMessageNode ExpectNoneArg(Object name)
            {
                contents.Add(new ExpectArgNode(name));
                return this;
            }
            internal ExpectMessageNode ExpectSimpleArg(Object name, String type)
            {
                contents.Add(new ExpectArgNode(name, type));
                return this;
            }
            internal ExpectMessageNode ExpectSimpleArg(Object name, String type, String style)
            {
                contents.Add(new ExpectArgNode(name, type, style));
                return this;
            }
            internal ExpectComplexArgNode ExpectChoiceArg(Object name)
            {
                return ExpectComplexArg(name, MessagePatternArgType.Choice);
            }
            internal ExpectComplexArgNode ExpectPluralArg(Object name)
            {
                return ExpectComplexArg(name, MessagePatternArgType.Plural);
            }
            internal ExpectComplexArgNode ExpectSelectArg(Object name)
            {
                return ExpectComplexArg(name, MessagePatternArgType.Select);
            }
            internal ExpectComplexArgNode ExpectSelectOrdinalArg(Object name)
            {
                return ExpectComplexArg(name, MessagePatternArgType.SelectOrdinal);
            }
            internal ExpectComplexArgNode ExpectComplexArg(Object name, MessagePatternArgType argType)
            {
                ExpectComplexArgNode complexArg = new ExpectComplexArgNode(this, name, argType);
                contents.Add(complexArg);
                return complexArg;
            }
            internal ExpectComplexArgNode FinishVariant()
            {
                return parent;
            }
            internal void CheckMatches(MessageNode msg)
            {
                // matches() prints all errors.
                Matches(msg);
            }
            internal bool Matches(MessageNode msg)
            {
                IList<MessageContentsNode> msgContents = msg.GetContents();
                bool ok = assertEquals("different numbers of MessageContentsNode",
                                          contents.Count, msgContents.Count);
                if (ok)
                {
                    //Iterator<MessageContentsNode> msgIter = msgContents.iterator();
                    using (var msgIter = msgContents.GetEnumerator())
                        foreach (ExpectMessageContentsNode ec in contents)
                        {
                            msgIter.MoveNext();
                            ok &= ec.Matches(msgIter.Current);
                        }
                }
                if (!ok)
                {
                    Errln("error in message: " + msg.ToString());
                }
                return ok;
            }
            internal ExpectComplexArgNode parent;  // for finishVariant()
            private List<ExpectMessageContentsNode> contents =
                new List<ExpectMessageContentsNode>();
        }

        /**
         * Base class for message contents nodes.
         * Used directly for REPLACE_NUMBER nodes, subclassed for others.
         */
        internal class ExpectMessageContentsNode
        {
            internal virtual bool Matches(MessageContentsNode c)
            {
                return assertEquals("not a REPLACE_NUMBER node",
                                    MessageContentsNode.NodeType.ReplaceNumber, c.Type);
            }
        }

        private class ExpectTextNode : ExpectMessageContentsNode
        {
            internal ExpectTextNode(String subString)
            {
                this.subString = subString;
            }
            internal override bool Matches(MessageContentsNode c)
            {
                return
                    assertEquals("not a TextNode",
                                 MessageContentsNode.NodeType.Text, c.Type) &&
                    assertTrue("TextNode does not contain \"" + subString + "\"",
                               ((TextNode)c).Text.Contains(subString));
            }
            private String subString;
        }

        internal class ExpectArgNode : ExpectMessageContentsNode
        {
            internal ExpectArgNode(Object name)
                    : this(name, null, null)
            {

            }
            internal ExpectArgNode(Object name, String type)
                        : this(name, type, null)
            {

            }
            internal ExpectArgNode(Object name, String type, String style)
            {
                if (name is String)
                {
                    this.name = (String)name;
                    this.number = -1;
                }
                else
                {
                    this.number = (int)name;
                    this.name = this.number.ToString(CultureInfo.InvariantCulture); //Integer.toString(this.number);
                }
                if (type == null)
                {
                    argType = MessagePatternArgType.None;
                }
                else
                {
                    argType = MessagePatternArgType.Simple;
                }
                this.type = type;
                this.style = style;
            }
            internal override bool Matches(MessageContentsNode c)
            {
                bool ok =
                    assertEquals("not an ArgNode",
                                 MessageContentsNode.NodeType.Arg, c.Type);
                if (!ok)
                {
                    return ok;
                }
                ArgNode arg = (ArgNode)c;
                ok &= assertEquals("unexpected ArgNode argType",
                                   argType, arg.ArgType);
                ok &= assertEquals("unexpected ArgNode arg name",
                                   name, arg.Name);
                ok &= assertEquals("unexpected ArgNode arg number",
                                   number, arg.Number);
                ok &= assertEquals("unexpected ArgNode arg type name",
                                   type, arg.TypeName);
                ok &= assertEquals("unexpected ArgNode arg style",
                                   style, arg.SimpleStyle);
                if (argType == MessagePatternArgType.None || argType == MessagePatternArgType.Simple)
                {
                    ok &= assertNull("unexpected non-null complex style", arg.ComplexStyle);
                }
                return ok;
            }
            private String name;
            private int number;
            protected MessagePatternArgType argType;
            private String type;
            private String style;
        }

        internal class ExpectComplexArgNode : ExpectArgNode
        {
            internal ExpectComplexArgNode(ExpectMessageNode parent,
                                         Object name, MessagePatternArgType argType)
            : base(name, argType.ToString().ToLowerInvariant())
            {
                this.argType = argType;
                this.parent = parent;
            }
            internal ExpectComplexArgNode ExpectOffset(double offset)
            {
                this.offset = offset;
                explicitOffset = true;
                return this;
            }
            internal ExpectMessageNode ExpectVariant(String selector)
            {
                ExpectVariantNode variant = new ExpectVariantNode(this, selector);
                variants.Add(variant);
                return variant.msg;
            }
            internal ExpectMessageNode ExpectVariant(String selector, double value)
            {
                ExpectVariantNode variant = new ExpectVariantNode(this, selector, value);
                variants.Add(variant);
                return variant.msg;
            }
            internal ExpectMessageNode finishComplexArg()
            {
                return parent;
            }

            internal override bool Matches(MessageContentsNode c)
            {
                bool ok = base.Matches(c);
                if (!ok)
                {
                    return ok;
                }
                ArgNode arg = (ArgNode)c;
                ComplexArgStyleNode complexStyle = arg.ComplexStyle;
                ok &= assertNotNull("unexpected null complex style", complexStyle);
                if (!ok)
                {
                    return ok;
                }
                ok &= assertEquals("unexpected complex-style argType",
                                   argType, complexStyle.ArgType);
                ok &= assertEquals("unexpected complex-style hasExplicitOffset()",
                                   explicitOffset, complexStyle.HasExplicitOffset);
                ok &= assertEquals("unexpected complex-style offset",
                                   offset, complexStyle.Offset);
                IList<VariantNode> complexVariants = complexStyle.Variants;
                ok &= assertEquals("different number of variants",
                                   variants.Count, complexVariants.Count);
                if (!ok)
                {
                    return ok;
                }
                //Iterator<VariantNode> complexIter = complexVariants.iterator();
                using (var complexIter = complexVariants.GetEnumerator())
                    foreach (ExpectVariantNode variant in variants)
                    {
                        complexIter.MoveNext();
                        ok &= variant.Matches(complexIter.Current);
                    }
                return ok;
            }
            private ExpectMessageNode parent;  // for finishComplexArg()
            private bool explicitOffset;
            private double offset;
            private List<ExpectVariantNode> variants = new List<ExpectVariantNode>();
        }

        internal class ExpectVariantNode
        {
            internal ExpectVariantNode(ExpectComplexArgNode parent, String selector)
                      : this(parent, selector, MessagePattern.NoNumericValue)
            {

            }
            internal ExpectVariantNode(ExpectComplexArgNode parent, String selector, double value)
            {
                this.selector = selector;
                numericValue = value;
                msg = new ExpectMessageNode();
                msg.parent = parent;
            }
            internal bool Matches(VariantNode v)
            {
                bool ok = assertEquals("different selector strings",
                                          selector, v.Selector);
                ok &= assertEquals("different selector strings",
                                   IsSelectorNumeric(), v.IsSelectorNumeric);
                ok &= assertEquals("different selector strings",
                                   numericValue, v.SelectorValue);
                return ok & msg.Matches(v.Message);
            }
            internal bool IsSelectorNumeric()
            {
                return numericValue != MessagePattern.NoNumericValue;
            }
            private String selector;
            private double numericValue;
            internal ExpectMessageNode msg;
        }

        // The actual tests start here. ---------------------------------------- ***
        // Sample message strings are mostly from the MessagePatternUtilDemo.

        [Test]
        public void TestHello()
        {
            // No syntax.
            MessageNode msg = MessagePatternUtil.BuildMessageNode("Hello!");
            ExpectMessageNode expect = new ExpectMessageNode().ExpectTextThatContains("Hello");
            expect.CheckMatches(msg);
        }

        [Test]
        public void TestHelloWithApos()
        {
            // Literal ASCII apostrophe.
            MessageNode msg = MessagePatternUtil.BuildMessageNode("Hel'lo!");
            ExpectMessageNode expect = new ExpectMessageNode().ExpectTextThatContains("Hel'lo");
            expect.CheckMatches(msg);
        }

        [Test]
        public void TestHelloWithQuote()
        {
            // Apostrophe starts quoted literal text.
            MessageNode msg = MessagePatternUtil.BuildMessageNode("Hel'{o!");
            ExpectMessageNode expect = new ExpectMessageNode().ExpectTextThatContains("Hel{o");
            expect.CheckMatches(msg);
            // Terminating the quote should yield the same result.
            msg = MessagePatternUtil.BuildMessageNode("Hel'{'o!");
            expect.CheckMatches(msg);
            // Double apostrophe inside quoted literal text still encodes a single apostrophe.
            msg = MessagePatternUtil.BuildMessageNode("a'{bc''de'f");
            expect = new ExpectMessageNode().ExpectTextThatContains("a{bc'def");
            expect.CheckMatches(msg);
        }

        [Test]
        public void TestNoneArg()
        {
            // Numbered argument.
            MessageNode msg = MessagePatternUtil.BuildMessageNode("abc{0}def");
            ExpectMessageNode expect = new ExpectMessageNode().
                ExpectTextThatContains("abc").ExpectNoneArg(0).ExpectTextThatContains("def");
            expect.CheckMatches(msg);
            // Named argument.
            msg = MessagePatternUtil.BuildMessageNode("abc{ arg }def");
            expect = new ExpectMessageNode().
                ExpectTextThatContains("abc").ExpectNoneArg("arg").ExpectTextThatContains("def");
            expect.CheckMatches(msg);
            // Numbered and named arguments.
            msg = MessagePatternUtil.BuildMessageNode("abc{1}def{arg}ghi");
            expect = new ExpectMessageNode().
                ExpectTextThatContains("abc").ExpectNoneArg(1).ExpectTextThatContains("def").
                ExpectNoneArg("arg").ExpectTextThatContains("ghi");
            expect.CheckMatches(msg);
        }

        [Test]
        public void TestSimpleArg()
        {
            MessageNode msg = MessagePatternUtil.BuildMessageNode("a'{bc''de'f{0,number,g'hi''jk'l#}");
            ExpectMessageNode expect = new ExpectMessageNode().
                ExpectTextThatContains("a{bc'def").ExpectSimpleArg(0, "number", "g'hi''jk'l#");
            expect.CheckMatches(msg);
        }

        [Test]
        public void TestSelectArg()
        {
            MessageNode msg = MessagePatternUtil.BuildMessageNode(
                    "abc{2, number}ghi{3, select, xx {xxx} other {ooo}} xyz");
            ExpectMessageNode expect = new ExpectMessageNode().
                ExpectTextThatContains("abc").ExpectSimpleArg(2, "number").
                ExpectTextThatContains("ghi").
                ExpectSelectArg(3).
                    ExpectVariant("xx").ExpectTextThatContains("xxx").FinishVariant().
                    ExpectVariant("other").ExpectTextThatContains("ooo").FinishVariant().
                    finishComplexArg().
                ExpectTextThatContains(" xyz");
            expect.CheckMatches(msg);
        }

        [Test]
        public void TestPluralArg()
        {
            // Plural with only keywords.
            MessageNode msg = MessagePatternUtil.BuildMessageNode(
                    "abc{num_people, plural, offset:17 few{fff} other {oooo}}xyz");
            ExpectMessageNode expect = new ExpectMessageNode().
                ExpectTextThatContains("abc").
                ExpectPluralArg("num_people").
                    ExpectOffset(17).
                    ExpectVariant("few").ExpectTextThatContains("fff").FinishVariant().
                    ExpectVariant("other").ExpectTextThatContains("oooo").FinishVariant().
                    finishComplexArg().
                ExpectTextThatContains("xyz");
            expect.CheckMatches(msg);
            // Plural with explicit-value selectors.
            msg = MessagePatternUtil.BuildMessageNode(
                    "abc{ num , plural , offset: 2 =1 {1} =-1 {-1} =3.14 {3.14} other {oo} }xyz");
            expect = new ExpectMessageNode().
                ExpectTextThatContains("abc").
                ExpectPluralArg("num").
                    ExpectOffset(2).
                    ExpectVariant("=1", 1).ExpectTextThatContains("1").FinishVariant().
                    ExpectVariant("=-1", -1).ExpectTextThatContains("-1").FinishVariant().
                    ExpectVariant("=3.14", 3.14).ExpectTextThatContains("3.14").FinishVariant().
                    ExpectVariant("other").ExpectTextThatContains("oo").FinishVariant().
                    finishComplexArg().
                ExpectTextThatContains("xyz");
            expect.CheckMatches(msg);
            // Plural with number replacement.
            msg = MessagePatternUtil.BuildMessageNode(
                    "a_{0,plural,other{num=#'#'=#'#'={1,number,##}!}}_z");
            expect = new ExpectMessageNode().
                ExpectTextThatContains("a_").
                ExpectPluralArg(0).
                    ExpectVariant("other").
                        ExpectTextThatContains("num=").ExpectReplaceNumber().
                        ExpectTextThatContains("#=").ExpectReplaceNumber().
                        ExpectTextThatContains("#=").ExpectSimpleArg(1, "number", "##").
                        ExpectTextThatContains("!").FinishVariant().
                    finishComplexArg().
                ExpectTextThatContains("_z");
            expect.CheckMatches(msg);
            // Plural with explicit offset:0.
            msg = MessagePatternUtil.BuildMessageNode(
                    "a_{0,plural,offset:0 other{num=#!}}_z");
            expect = new ExpectMessageNode().
                ExpectTextThatContains("a_").
                ExpectPluralArg(0).
                    ExpectOffset(0).
                    ExpectVariant("other").
                        ExpectTextThatContains("num=").ExpectReplaceNumber().
                        ExpectTextThatContains("!").FinishVariant().
                    finishComplexArg().
                ExpectTextThatContains("_z");
            expect.CheckMatches(msg);
        }


        [Test]
        public void TestSelectOrdinalArg()
        {
            MessageNode msg = MessagePatternUtil.BuildMessageNode(
                    "abc{num, selectordinal, offset:17 =0{null} few{fff} other {oooo}}xyz");
            ExpectMessageNode expect = new ExpectMessageNode().
                ExpectTextThatContains("abc").
                ExpectSelectOrdinalArg("num").
                    ExpectOffset(17).
                    ExpectVariant("=0", 0).ExpectTextThatContains("null").FinishVariant().
                    ExpectVariant("few").ExpectTextThatContains("fff").FinishVariant().
                    ExpectVariant("other").ExpectTextThatContains("oooo").FinishVariant().
                    finishComplexArg().
                ExpectTextThatContains("xyz");
            expect.CheckMatches(msg);
        }

        [Test]
        public void TestChoiceArg()
        {
            MessageNode msg = MessagePatternUtil.BuildMessageNode(
                    "a_{0,choice,-∞ #-inf|  5≤ five | 99 # ninety'|'nine  }_z");
            ExpectMessageNode expect = new ExpectMessageNode().
                ExpectTextThatContains("a_").
                ExpectChoiceArg(0).
                    ExpectVariant("#", double.NegativeInfinity).
                        ExpectTextThatContains("-inf").FinishVariant().
                    ExpectVariant("≤", 5).ExpectTextThatContains(" five ").FinishVariant().
                    ExpectVariant("#", 99).ExpectTextThatContains(" ninety|nine  ").FinishVariant().
                    finishComplexArg().
                ExpectTextThatContains("_z");
            expect.CheckMatches(msg);
        }

        [Test]
        public void TestComplexArgs()
        {
            MessageNode msg = MessagePatternUtil.BuildMessageNode(
                    "I don't {a,plural,other{w'{'on't #'#'}} and " +
                    "{b,select,other{shan't'}'}} '{'''know'''}' and " +
                    "{c,choice,0#can't'|'}" +
                    "{z,number,#'#'###.00'}'}.");
            ExpectMessageNode expect = new ExpectMessageNode().
                ExpectTextThatContains("I don't ").
                ExpectPluralArg("a").
                    ExpectVariant("other").
                        ExpectTextThatContains("w{on't ").ExpectReplaceNumber().
                        ExpectTextThatContains("#").FinishVariant().
                    finishComplexArg().
                ExpectTextThatContains(" and ").
                ExpectSelectArg("b").
                    ExpectVariant("other").ExpectTextThatContains("shan't}").FinishVariant().
                    finishComplexArg().
                ExpectTextThatContains(" {'know'} and ").
                ExpectChoiceArg("c").
                    ExpectVariant("#", 0).ExpectTextThatContains("can't|").FinishVariant().
                    finishComplexArg().
                ExpectSimpleArg("z", "number", "#'#'###.00'}'").
                ExpectTextThatContains(".");
            expect.CheckMatches(msg);
        }

        /**
         * @return the text string of the VariantNode's message;
         *         assumes that its message consists of only text
         */
        private String VariantText(VariantNode v)
        {
            return ((TextNode)v.Message.GetContents()[0]).Text;
        }

        [Test]
        public void TestPluralVariantsByType()
        {
            MessageNode msg = MessagePatternUtil.BuildMessageNode(
                    "{p,plural,a{A}other{O}=4{iv}b{B}other{U}=2{ii}}");
            ExpectMessageNode expect = new ExpectMessageNode().
            ExpectPluralArg("p").
                ExpectVariant("a").ExpectTextThatContains("A").FinishVariant().
                ExpectVariant("other").ExpectTextThatContains("O").FinishVariant().
                ExpectVariant("=4", 4).ExpectTextThatContains("iv").FinishVariant().
                ExpectVariant("b").ExpectTextThatContains("B").FinishVariant().
                ExpectVariant("other").ExpectTextThatContains("U").FinishVariant().
                ExpectVariant("=2", 2).ExpectTextThatContains("ii").FinishVariant().
                finishComplexArg();
            if (!expect.Matches(msg))
            {
                return;
            }
            List<VariantNode> numericVariants = new List<VariantNode>();
            List<VariantNode> keywordVariants = new List<VariantNode>();
            VariantNode other =
                ((ArgNode)msg.GetContents()[0]).ComplexStyle.
                GetVariantsByType(numericVariants, keywordVariants);
            assertEquals("'other' selector", "other", other.Selector);
            assertEquals("message string of first 'other'", "O", VariantText(other));

            assertEquals("numericVariants.size()", 2, numericVariants.Count);
            VariantNode v = numericVariants[0];
            assertEquals("numericVariants[0] selector", "=4", v.Selector);
            assertEquals("numericVariants[0] selector value", 4.0, v.SelectorValue);
            assertEquals("numericVariants[0] text", "iv", VariantText(v));
            v = numericVariants[1];
            assertEquals("numericVariants[1] selector", "=2", v.Selector);
            assertEquals("numericVariants[1] selector value", 2.0, v.SelectorValue);
            assertEquals("numericVariants[1] text", "ii", VariantText(v));

            assertEquals("keywordVariants.size()", 2, keywordVariants.Count);
            v = keywordVariants[0];
            assertEquals("keywordVariants[0] selector", "a", v.Selector);
            assertFalse("keywordVariants[0].isSelectorNumeric()", v.IsSelectorNumeric);
            assertEquals("keywordVariants[0] text", "A", VariantText(v));
            v = keywordVariants[1];
            assertEquals("keywordVariants[1] selector", "b", v.Selector);
            assertFalse("keywordVariants[1].isSelectorNumeric()", v.IsSelectorNumeric);
            assertEquals("keywordVariants[1] text", "B", VariantText(v));
        }

        [Test]
        public void TestSelectVariantsByType()
        {
            MessageNode msg = MessagePatternUtil.BuildMessageNode(
                    "{s,select,a{A}other{O}b{B}other{U}}");
            ExpectMessageNode expect = new ExpectMessageNode().
            ExpectSelectArg("s").
                ExpectVariant("a").ExpectTextThatContains("A").FinishVariant().
                ExpectVariant("other").ExpectTextThatContains("O").FinishVariant().
                ExpectVariant("b").ExpectTextThatContains("B").FinishVariant().
                ExpectVariant("other").ExpectTextThatContains("U").FinishVariant().
                finishComplexArg();
            if (!expect.Matches(msg))
            {
                return;
            }
            // Check that we can use numericVariants = null.
            IList<VariantNode> keywordVariants = new List<VariantNode>();
            VariantNode other =
                ((ArgNode)msg.GetContents()[0]).ComplexStyle.
                GetVariantsByType(null, keywordVariants);
            assertEquals("'other' selector", "other", other.Selector);
            assertEquals("message string of first 'other'", "O", VariantText(other));

            assertEquals("keywordVariants.size()", 2, keywordVariants.Count);
            VariantNode v = keywordVariants[0];
            assertEquals("keywordVariants[0] selector", "a", v.Selector);
            assertFalse("keywordVariants[0].isSelectorNumeric()", v.IsSelectorNumeric);
            assertEquals("keywordVariants[0] text", "A", VariantText(v));
            v = keywordVariants[1];
            assertEquals("keywordVariants[1] selector", "b", v.Selector);
            assertFalse("keywordVariants[1].isSelectorNumeric()", v.IsSelectorNumeric);
            assertEquals("keywordVariants[1] text", "B", VariantText(v));
        }
    }
}
