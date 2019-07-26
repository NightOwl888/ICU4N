using System;
using System.Diagnostics;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// Error types for <see cref="StringPrepParseException"/>.
    /// </summary>
    public enum StringPrepErrorType
    {
        /// <stable>ICU 2.8</stable>
        InvalidCharFound = 0,
        /// <stable>ICU 2.8</stable>
        IllegalCharFound = 1,
        /// <stable>ICU 2.8</stable>
        ProhibitedError = 2,
        /// <stable>ICU 2.8</stable>
        UnassignedError = 3,
        /// <stable>ICU 2.8</stable>
        CheckBiDiError = 4,
        /// <stable>ICU 2.8</stable>
        STD3ASCIIRulesError = 5,
        /// <stable>ICU 2.8</stable>
        AcePrefixError = 6,
        /// <stable>ICU 2.8</stable>
        VerificationError = 7,
        /// <stable>ICU 2.8</stable>
        LabelTooLongError = 8,
        /// <stable>ICU 2.8</stable>
        BufferOverflowError = 9,
        /// <stable>ICU 2.8</stable>
        ZeroLengthLabel = 10,
        /// <stable>ICU 2.8</stable>
        DomainNameTooLongError = 11,
    }

    /// <summary>
    /// Exception that signals an error has occurred while parsing the
    /// input to <see cref="StringPrep"/> or <see cref="IDNA"/>.
    /// </summary>
    /// <author>Ram Viswanadha</author>
    /// <stable>ICU 2.8</stable>
#if FEATURE_SERIALIZABLE_EXCEPTIONS
    [Serializable]
#endif
    public class StringPrepParseException : FormatException
    {
        /// <summary>
        /// Construct a ParseException object with the given message
        /// and error code
        /// </summary>
        /// <param name="message">A string describing the type of error that occurred.</param>
        /// <param name="error">The error that has occurred.</param>
        /// <stable>ICU 2.8</stable>
        public StringPrepParseException(string message, StringPrepErrorType error)
            : base(message)
        {
            this.error = error;
            this.line = 0;
        }

        /// <summary>
        /// Construct a ParseException object with the given message and
        /// error code.
        /// </summary>
        /// <param name="message">A string describing the type of error that occurred.</param>
        /// <param name="error">The error that has occurred.</param>
        /// <param name="rules">The input rules string.</param>
        /// <param name="pos">The position of error in the rules string.</param>
        /// <stable>ICU 2.8</stable>
        public StringPrepParseException(string message, StringPrepErrorType error, string rules, int pos)
            : base(message)
        {
            this.error = error;
            SetContext(rules, pos);
            this.line = 0;
        }

        /// <summary>
        /// Construct  a ParseException object with the given message and error code.
        /// </summary>
        /// <param name="message">A string describing the type of error that occurred.</param>
        /// <param name="error">The error that has occurred.</param>
        /// <param name="rules">The input rules string.</param>
        /// <param name="pos">The position of error in the rules string.</param>
        /// <param name="lineNumber">
        /// The line number at which the error has occurred.
        /// If the parse engine is not using this field, it should set it to zero.  Otherwise
        /// it should be a positive integer. The default value of this field
        /// is -1. It will be set to 0 if the code populating this struct is not
        /// using line numbers.
        /// </param>
        /// <stable>ICU 2.8</stable>
        public StringPrepParseException(string message, StringPrepErrorType error, string rules, int pos, int lineNumber)
            : base(message)
        {
            this.error = error;
            SetContext(rules, pos);
            this.line = lineNumber;
        }

#if FEATURE_SERIALIZABLE_EXCEPTIONS
        /// <summary>
        /// Initializes a new instance of this class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected StringPrepParseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif

        /// <summary>
        /// Compare this ParseException to another and evaluate if they are equal.
        /// The comparison works only on the type of error and does not compare
        /// the rules strings, if any, for equality.
        /// </summary>
        /// <param name="other">The exception that this object should be compared to.</param>
        /// <returns>true if the objects are equal, false if unequal.</returns>
        /// <stable>ICU 2.8</stable>
        public override bool Equals(object other)
        {
            if (!(other is StringPrepParseException))
            {
                return false;
            }
            return ((StringPrepParseException)other).error == this.error;

        }

        /// <summary>
        /// Mock implementation of <see cref="GetHashCode()"/>. This implementation always returns a constant
        /// value. When .NET assertion is enabled, this method triggers an assertion failure.
        /// </summary>
        /// <returns>This API is ICU internal only.</returns>
        /// <internal/>
#pragma warning disable 809
        [Obsolete("This API is ICU internal only.")]
        public override int GetHashCode()
#pragma warning disable 809
        {
            Debug.Assert(false, "hashCode not designed");
            return 42;
        }

        /// <summary>
        /// Returns the position of error in the rules string.
        /// </summary>
        /// <returns>String.</returns>
        /// <stable>ICU 2.8</stable>
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(base.Message);
            buf.Append(". line:  ");
            buf.Append(line);
            buf.Append(". preContext:  ");
            buf.Append(preContext);
            buf.Append(". postContext: ");
            buf.Append(postContext);
            buf.Append("\n");
            return buf.ToString();
        }

        private StringPrepErrorType error;

        /// <summary>
        /// The line on which the error occurred.  If the parse engine
        /// is not using this field, it should set it to zero.  Otherwise
        /// it should be a positive integer. The default value of this field
        /// is -1. It will be set to 0 if the code populating this struct is not
        /// using line numbers.
        /// </summary>
        private int line;

        /// <summary>
        /// Textual context before the error.  Null-terminated.
        /// May be the empty string if not implemented by parser.
        /// </summary>
        private StringBuffer preContext = new StringBuffer();

        /// <summary>
        /// Textual context after the error.  Null-terminated.
        /// May be the empty string if not implemented by parser.
        /// </summary>
        private StringBuffer postContext = new StringBuffer();

        private static readonly int PARSE_CONTEXT_LEN = 16;

        private void SetPreContext(string str, int pos)
        {
            SetPreContext(str.ToCharArray(), pos);
        }

        private void SetPreContext(char[] str, int pos)
        {
            int start = (pos <= PARSE_CONTEXT_LEN) ? 0 : (pos - (PARSE_CONTEXT_LEN - 1));
            int len = (start <= PARSE_CONTEXT_LEN) ? start : PARSE_CONTEXT_LEN;
            preContext.Append(str, start, len);

        }

        private void SetPostContext(String str, int pos)
        {
            SetPostContext(str.ToCharArray(), pos);
        }

        private void SetPostContext(char[] str, int pos)
        {
            int start = pos;
            int len = str.Length - start;
            postContext.Append(str, start, len);

        }

        private void SetContext(String str, int pos)
        {
            SetPreContext(str, pos);
            SetPostContext(str, pos);
        }

        /// <summary>
        /// Returns the error code of this exception.
        /// This method is only used for testing to verify the error.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public virtual StringPrepErrorType Error
        {
            get { return error; }
        }

        /// <summary>
        /// Gets the line on which the error occurred.
        /// </summary>
        public virtual int Line // ICU4N specific - FormatException doesn't have a line number
        {
            get { return line; }
        }
    }
}
