using ICU4N.Support.Collections;
using J2N.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Utilities for working with a <see cref="MessagePattern"/>.
    /// Intended for use in tools when convenience is more important than
    /// minimizing runtime and object creations.
    /// <para/>
    /// This class only has static methods.
    /// Each of the nested classes is immutable and thread-safe.
    /// <para/>
    /// This class and its nested classes are not intended for public subclassing.
    /// </summary>
    /// <stable>ICU 49</stable>
    /// <author>Markus Scherer</author>
    public static class MessagePatternUtil
    {
        /// <summary>
        /// Factory method, builds and returns a <see cref="MessageNode"/> from a <see cref="MessageFormat"/> pattern string.
        /// </summary>
        /// <param name="patternString">a <see cref="MessageFormat"/> pattern string</param>
        /// <returns>A <see cref="MessageNode"/> or a <see cref="ComplexArgStyleNode"/></returns>
        /// <exception cref="ArgumentException">if the <see cref="MessagePattern"/> is empty
        /// or does not represent a <see cref="MessageFormat"/> pattern</exception>
        /// <stable>ICU 49</stable>
        public static MessageNode BuildMessageNode(string patternString)
        {
            return BuildMessageNode(new MessagePattern(patternString));
        }

        /// <summary>
        /// Factory method, builds and returns a <see cref="MessageNode"/> from a <see cref="MessagePattern"/>.
        /// </summary>
        /// <param name="pattern">A parsed <see cref="MessageFormat"/> pattern string</param>
        /// <returns>A <see cref="MessageNode"/> or a <see cref="ComplexArgStyleNode"/></returns>
        /// <exception cref="ArgumentException">if the <see cref="MessagePattern"/> is empty
        /// or does not represent a <see cref="MessageFormat"/> pattern</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="pattern"/> is null.</exception>
        /// <stable>ICU 49</stable>
        public static MessageNode BuildMessageNode(this MessagePattern pattern) // ICU4N specific - turned it into an extension method
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            int limit = pattern.CountParts() - 1;
            if (limit < 0)
            {
                throw new ArgumentException("The MessagePattern is empty");
            }
            else if (pattern.GetPartType(0) != MessagePatternPartType.MsgStart)
            {
                throw new ArgumentException(
                "The MessagePattern does not represent a MessageFormat pattern");
            }
            return BuildMessageNode(pattern, 0, limit);
        }

        // ICU4N specific - de-nested Node class

        // ICU4N specific - de-nested MessageNode class

        // ICU4N specific - de-nested MessageContentsNode class

        // ICU4N specific - de-nested TextNode class

        // ICU4N specific - de-nested ArgNode class

        // ICU4N specific - de-nested ComplexArgStyleNode class

        // ICU4N specific - de-nested VariantNode class

        private static MessageNode BuildMessageNode(MessagePattern pattern, int start, int limit)
        {
            int prevPatternIndex = pattern.GetPart(start).Limit;
            MessageNode node = new MessageNode();
            for (int i = start + 1; ; ++i)
            {
                MessagePatternPart part = pattern.GetPart(i);
                int patternIndex = part.Index;
                if (prevPatternIndex < patternIndex)
                {
                    node.AddContentsNode(
                            new TextNode(pattern.PatternString.Substring(prevPatternIndex, patternIndex - prevPatternIndex))); // ICU4N: Corrected 2nd arg
                }
                if (i == limit)
                {
                    break;
                }
                MessagePatternPartType partType = part.Type;
                if (partType == MessagePatternPartType.ArgStart)
                {
                    int argLimit = pattern.GetLimitPartIndex(i);
                    node.AddContentsNode(BuildArgNode(pattern, i, argLimit));
                    i = argLimit;
                    part = pattern.GetPart(i);
                }
                else if (partType == MessagePatternPartType.ReplaceNumber)
                {
                    node.AddContentsNode(MessageContentsNode.CreateReplaceNumberNode());
                    // else: ignore SKIP_SYNTAX and INSERT_CHAR parts.
                }
                prevPatternIndex = part.Limit;
            }
            return node.Freeze();
        }

        private static ArgNode BuildArgNode(MessagePattern pattern, int start, int limit)
        {
            ArgNode node = ArgNode.CreateArgNode();
            MessagePatternPart part = pattern.GetPart(start);
            MessagePatternArgType argType = node.ArgType = part.ArgType;
            part = pattern.GetPart(++start);  // ARG_NAME or ARG_NUMBER
            node.Name = pattern.GetSubstring(part);
            if (part.Type == MessagePatternPartType.ArgNumber)
            {
                node.Number = part.Value;
            }
            ++start;
            switch (argType)
            {
                case MessagePatternArgType.Simple:
                    // ARG_TYPE
                    node.TypeName = pattern.GetSubstring(pattern.GetPart(start++));
                    if (start < limit)
                    {
                        // ARG_STYLE
                        node.SimpleStyle = pattern.GetSubstring(pattern.GetPart(start));
                    }
                    break;
                case MessagePatternArgType.Choice:
                    node.TypeName = "choice";
                    node.ComplexStyle = BuildChoiceStyleNode(pattern, start, limit);
                    break;
                case MessagePatternArgType.Plural:
                    node.TypeName = "plural";
                    node.ComplexStyle = BuildPluralStyleNode(pattern, start, limit, argType);
                    break;
                case MessagePatternArgType.Select:
                    node.TypeName = "select";
                    node.ComplexStyle = BuildSelectStyleNode(pattern, start, limit);
                    break;
                case MessagePatternArgType.SelectOrdinal:
                    node.TypeName = "selectordinal";
                    node.ComplexStyle = BuildPluralStyleNode(pattern, start, limit, argType);
                    break;
                default:
                    // NONE type, nothing else to do
                    break;
            }
            return node;
        }

        private static ComplexArgStyleNode BuildChoiceStyleNode(MessagePattern pattern,
                                                                int start, int limit)
        {
            ComplexArgStyleNode node = new ComplexArgStyleNode(MessagePatternArgType.Choice);
            while (start < limit)
            {
                int valueIndex = start;
                MessagePatternPart part = pattern.GetPart(start);
                double value = pattern.GetNumericValue(part);
                start += 2;
                int msgLimit = pattern.GetLimitPartIndex(start);
                VariantNode variant = new VariantNode();
                variant.Selector = pattern.GetSubstring(pattern.GetPart(valueIndex + 1));
                variant.SelectorValue = value;
                variant.Message = BuildMessageNode(pattern, start, msgLimit);
                node.AddVariant(variant);
                start = msgLimit + 1;
            }
            return node.Freeze();
        }

        private static ComplexArgStyleNode BuildPluralStyleNode(MessagePattern pattern,
                                                                int start, int limit,
                                                                MessagePatternArgType argType)
        {
            ComplexArgStyleNode node = new ComplexArgStyleNode(argType);
            MessagePatternPart offset = pattern.GetPart(start);
            if (offset.Type.HasNumericValue())
            {
                node.HasExplicitOffset = true;
                node.Offset = pattern.GetNumericValue(offset);
                ++start;
            }
            while (start < limit)
            {
                MessagePatternPart selector = pattern.GetPart(start++);
                double value = MessagePattern.NoNumericValue;
                MessagePatternPart part = pattern.GetPart(start);
                if (part.Type.HasNumericValue())
                {
                    value = pattern.GetNumericValue(part);
                    ++start;
                }
                int msgLimit = pattern.GetLimitPartIndex(start);
                VariantNode variant = new VariantNode();
                variant.Selector = pattern.GetSubstring(selector);
                variant.SelectorValue = value;
                variant.Message = BuildMessageNode(pattern, start, msgLimit);
                node.AddVariant(variant);
                start = msgLimit + 1;
            }
            return node.Freeze();
        }

        private static ComplexArgStyleNode BuildSelectStyleNode(MessagePattern pattern,
                                                                int start, int limit)
        {
            ComplexArgStyleNode node = new ComplexArgStyleNode(MessagePatternArgType.Select);
            while (start < limit)
            {
                MessagePatternPart selector = pattern.GetPart(start++);
                int msgLimit = pattern.GetLimitPartIndex(start);
                VariantNode variant = new VariantNode();
                variant.Selector = pattern.GetSubstring(selector);
                variant.Message = BuildMessageNode(pattern, start, msgLimit);
                node.AddVariant(variant);
                start = msgLimit + 1;
            }
            return node.Freeze();
        }
    }

    /// <summary>
    /// Common base class for all elements in a tree of nodes
    /// returned by <see cref="MessagePatternUtil.BuildMessageNode(MessagePattern)"/>.
    /// This class and all subclasses are immutable and thread-safe.
    /// </summary>
    /// <stable>ICU 49</stable>
    public class MessagePatternNode
    {
        internal MessagePatternNode() { }
    }

    /// <summary>
    /// A Node representing a parsed <see cref="MessageFormat"/> pattern string.
    /// </summary>
    /// <stable>ICU 49</stable>
    public class MessageNode : MessagePatternNode
    {
        /// <returns>The list of MessageContentsNode nodes that this message contains.</returns>
        /// <stable>ICU 49</stable>
        public virtual IList<MessageContentsNode> GetContents()
        {
            return list;
        }

        /// <stable>ICU 49</stable>
        public override string ToString()
        {
            return list.ToString();
        }

        internal MessageNode()
        {
        }
        internal void AddContentsNode(MessageContentsNode node)
        {
            if (node is TextNode && list.Any())
            {
                // Coalesce adjacent text nodes.
                MessageContentsNode lastNode = list[list.Count - 1];
                if (lastNode is TextNode)
                {
                    TextNode textNode = (TextNode)lastNode;
                    textNode.Text = textNode.Text + ((TextNode)node).Text;
                    return;
                }
            }
            list.Add(node);
        }
        internal MessageNode Freeze()
        {
            list = list.ToUnmodifiableList(); //Collections.unmodifiableList(list);
            return this;
        }

        private volatile IList<MessageContentsNode> list = new List<MessageContentsNode>();
    }

    /// <summary>
    /// A piece of <see cref="MessageNode"/> contents.
    /// Use <see cref="Type"/> to determine the type and the actual <see cref="MessagePatternNode"/> subclass.
    /// </summary>
    /// <stable>ICU 49</stable>
    public class MessageContentsNode : MessagePatternNode
    {
        /// <summary>
        /// The type of a piece of <see cref="MessageNode"/> contents.
        /// </summary>
        /// <stable>ICU 49</stable>
        public enum NodeType
        {
            /// <summary>
            /// This is a <see cref="TextNode"/> containing literal text (downcast and call <see cref="Text"/>).
            /// </summary>
            /// <stable>ICU 49</stable>
            Text,

            /// <summary>
            /// This is an <see cref="ArgNode"/> representing a message argument
            /// (downcast and use specific methods).
            /// </summary>
            /// <stable>ICU 49</stable>
            Arg,

            /// <summary>
            /// This <see cref="MessagePatternNode"/> represents a place in a plural argument's variant where
            /// the formatted (plural-offset) value is to be put.
            /// </summary>
            /// <stable>ICU 49</stable>
            ReplaceNumber
        }

        /// <summary>
        /// Returns the type of this piece of <see cref="MessageNode"/> contents.
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual NodeType Type
        {
            get { return type; }
        }

        /// <stable>ICU 49</stable>
        public override string ToString()
        {
            // Note: There is no specific subclass for REPLACE_NUMBER
            // because it would not provide any additional API.
            // Therefore we have a little bit of REPLACE_NUMBER-specific code
            // here in the contents-node base class.
            return "{REPLACE_NUMBER}";
        }

        internal MessageContentsNode(NodeType type)
        {
            this.type = type;
        }
        internal static MessageContentsNode CreateReplaceNumberNode()
        {
            return new MessageContentsNode(NodeType.ReplaceNumber);
        }

        private NodeType type;
    }

    /// <summary>
    /// Literal text, a piece of MessageNode contents.
    /// </summary>
    /// <stable>ICU 49</stable>
    public class TextNode : MessageContentsNode
    {
        /// <summary>
        /// Gets or sets the literal text at this point in the message
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual string Text
        {
            get => text;
            internal set => text = value;
        }

        /// <stable>ICU 49</stable>
        public override string ToString()
        {
            return "«" + text + "»";
        }

        internal TextNode(string text)
            : base(NodeType.Text)
        {
            this.text = text;
        }

        private string text;
    }

    /// <summary>
    /// A piece of <see cref="MessageNode"/> contents representing a message argument and its details.
    /// </summary>
    /// <stable>ICU 49</stable>
    public class ArgNode : MessageContentsNode
    {
        /// <summary>
        /// Gets or Sets the argument type.
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual MessagePatternArgType ArgType
        {
            get { return argType; }
            internal set { argType = value; }
        }

        /// <summary>
        /// Gets or sets the argument name string (the decimal-digit string if the argument has a number)
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual string Name
        {
            get { return name; }
            internal set { name = value; }
        }

        /// <summary>
        /// Gets or sets the argument number, or -1 if none (for a named argument).
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual int Number
        {
            get { return number; }
            internal set { number = value; }
        }

        /// <summary>
        /// Gets or sets the argument type string, or null if none was specified.
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual string TypeName
        {
            get { return typeName; }
            internal set { typeName = value; }
        }

        /// <summary>
        /// Gets or sets the simple-argument style string,
        /// or null if no style is specified and for other argument types.
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual string SimpleStyle
        {
            get { return style; }
            internal set { style = value; }
        }

        /// <summary>
        /// Gets or sets the complex-argument-style object,
        /// or null if the argument type is <see cref="MessagePatternArgType.None"/> 
        /// or <see cref="MessagePatternArgType.Simple"/>
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual ComplexArgStyleNode ComplexStyle
        {
            get { return complexStyle; }
            internal set { complexStyle = value; }
        }

        /// <stable>ICU 49</stable>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('{').Append(name);
            if (argType != MessagePatternArgType.None)
            {
                sb.Append(',').Append(typeName);
                if (argType == MessagePatternArgType.Simple)
                {
                    if (style != null)
                    {
                        sb.Append(',').Append(style);
                    }
                }
                else
                {
                    sb.Append(',').Append(complexStyle.ToString());
                }
            }
            return sb.Append('}').ToString();
        }

        internal ArgNode()
            : base(NodeType.Arg)
        {
        }
        internal static ArgNode CreateArgNode()
        {
            return new ArgNode();
        }

        private MessagePatternArgType argType;
        private string name;
        private int number = -1;
        private string typeName;
        private string style;
        private ComplexArgStyleNode complexStyle;
    }

    /// <summary>
    /// A <see cref="MessagePatternNode"/> representing details of the argument style of a complex argument.
    /// (Which is a choice/plural/select argument which selects among nested messages.)
    /// </summary>
    /// <stable>ICU 49</stable>
    public class ComplexArgStyleNode : MessagePatternNode
    {
        /// <summary>
        /// Gets or sets the argument type (same as <see cref="ArgType"/>getArgType() on the parent <see cref="ArgNode"/>)
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual MessagePatternArgType ArgType
        {
            get { return argType; }
            internal set { ArgType = value; }
        }

        /// <summary>
        /// Gets or sets true if this is a plural style with an explicit offset
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual bool HasExplicitOffset
        {
            get { return explicitOffset; }
            internal set { explicitOffset = value; }
        }

        /// <summary>
        /// Gets or sets the plural offset, or 0 if this is not a plural style or
        /// the offset is explicitly or implicitly 0
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual double Offset
        {
            get { return offset; }
            internal set { offset = value; }
        }

        /// <summary>
        /// Gets or sets the list of variants: the nested messages with their selection criteria
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual IList<VariantNode> Variants
        {
            get { return list; }
            internal set { list = value; }
        }

        /// <summary>
        /// Separates the variants by type.
        /// Intended for use with plural and select argument styles,
        /// not useful for choice argument styles.
        /// <para/>
        /// Both parameters are used only for output, and are first cleared.
        /// </summary>
        /// <param name="numericVariants">Variants with numeric-value selectors (if any) are added here.
        /// Can be null for a select argument style.</param>
        /// <param name="keywordVariants">Variants with keyword selectors, except "other", are added here.
        /// For a plural argument, if this list is empty after the call, then
        /// all variants except "other" have explicit values
        /// and PluralRules need not be called.</param>
        /// <returns>the "other" variant (the first one if there are several),
        /// null if none (choice style)</returns>
        /// <stable>ICU 49</stable>
        public virtual VariantNode GetVariantsByType(IList<VariantNode> numericVariants,
                                             IList<VariantNode> keywordVariants)
        {
            if (numericVariants != null)
            {
                numericVariants.Clear();
            }
            keywordVariants.Clear();
            VariantNode other = null;
            foreach (VariantNode variant in list)
            {
                if (variant.IsSelectorNumeric)
                {
                    numericVariants.Add(variant);
                }
                else if ("other".Equals(variant.Selector))
                {
                    if (other == null)
                    {
                        // Return the first "other" variant. (MessagePattern allows duplicates.)
                        other = variant;
                    }
                }
                else
                {
                    keywordVariants.Add(variant);
                }
            }
            return other;
        }

        /// <stable>ICU 49</stable>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(').Append(argType.ToString()).Append(" style) ");
            if (HasExplicitOffset)
            {
                sb.Append("offset:").Append(offset).Append(' ');
            }
            return sb.Append(list.ToString()).ToString();
        }

        internal ComplexArgStyleNode(MessagePatternArgType argType)
        {
            this.argType = argType;
        }
        internal void AddVariant(VariantNode variant)
        {
            list.Add(variant);
        }
        internal ComplexArgStyleNode Freeze()
        {
            list = list.ToUnmodifiableList(); //Collections.unmodifiableList(list);
            return this;
        }

        private MessagePatternArgType argType;
        private double offset;
        private bool explicitOffset;
        private volatile IList<VariantNode> list = new List<VariantNode>();
    }

    /// <summary>
    /// A <see cref="MessagePatternNode"/> representing a nested message (nested inside an argument)
    /// with its selection criterium.
    /// </summary>
    /// <stable>ICU 49</stable>
    public class VariantNode : MessagePatternNode
    {
        /// <summary>
        /// Gets the selector string.
        /// For example: A plural/select keyword ("few"), a plural explicit value ("=1"),
        /// a choice comparison operator ("#").
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual string Selector
        {
            get { return selector; }
            internal set { selector = value; }
        }

        /// <summary>
        /// Gets true for choice variants and for plural explicit values
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual bool IsSelectorNumeric
        {
            get { return numericValue != MessagePattern.NoNumericValue; }
        }

        /// <summary>
        /// Gets the selector's numeric value, or NO_NUMERIC_VALUE if !isSelectorNumeric()
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual double SelectorValue
        {
            get { return numericValue; }
            internal set { numericValue = value; }
        }

        /// <summary>
        /// Gets the nested message
        /// </summary>
        /// <stable>ICU 49</stable>
        public virtual MessageNode Message
        {
            get { return msgNode; }
            internal set { msgNode = value; }
        }

        /// <stable>ICU 49</stable>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (IsSelectorNumeric)
            {
                sb.Append(numericValue).Append(" (").Append(selector).Append(") {");
            }
            else
            {
                sb.Append(selector).Append(" {");
            }
            return sb.Append(msgNode.ToString()).Append('}').ToString();
        }

        internal VariantNode()
        {
        }

        private string selector;
        private double numericValue = MessagePattern.NoNumericValue;
        private MessageNode msgNode;
    }
}
