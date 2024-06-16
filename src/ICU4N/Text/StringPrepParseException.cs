using J2N.Text;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
#nullable enable

namespace ICU4N.Text
{
    /// <summary>
    /// Error types for <see cref="StringPrepFormatException"/>.
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

    internal static partial class ThrowHelper
    {
        private partial class SR // ICU4N: Naming this SR gives us the ability to move these to a resx file later so they can be localized
        {
            // These correspond with StringPrepErrorType enum
            public const string InvalidCharFound = "Invalid char found";
            public const string IllegalCharFound = "Illegal char found";
            public const string ProhibitedError = "A prohibited code point was found in the input";
            public const string UnassignedError = "An unassigned code point was found in the input";
            public const string CheckBiDiError = "The input does not conform to the rules for BiDi code points.";
            public const string STD3ASCIIRulesError = "The input does not conform to the STD 3 ASCII rules";
            public const string AcePrefixError = "The input does not start with the ACE Prefix.";
            public const string VerificationError = "Verification error";
            public const string LabelTooLongError = "The labels in the input are too long. Length > 63.";
            public const string BufferOverflowError = "The buffer length is insufficient";
            public const string ZeroLengthLabel = "Found zero length label after NamePrep.";
            public const string DomainNameTooLongError = "The output exceed the max allowed length.";
        }


        public static string GetErrorMessage(this StringPrepErrorType type) => type switch
        {
            StringPrepErrorType.InvalidCharFound => SR.InvalidCharFound,
            StringPrepErrorType.IllegalCharFound => SR.IllegalCharFound,
            StringPrepErrorType.ProhibitedError => SR.ProhibitedError,
            StringPrepErrorType.UnassignedError => SR.UnassignedError,
            StringPrepErrorType.CheckBiDiError => SR.CheckBiDiError,
            StringPrepErrorType.STD3ASCIIRulesError => SR.STD3ASCIIRulesError,
            StringPrepErrorType.AcePrefixError => SR.AcePrefixError,
            StringPrepErrorType.VerificationError => SR.VerificationError,
            StringPrepErrorType.LabelTooLongError => SR.LabelTooLongError,
            StringPrepErrorType.BufferOverflowError => SR.BufferOverflowError,
            StringPrepErrorType.ZeroLengthLabel => SR.ZeroLengthLabel,
            StringPrepErrorType.DomainNameTooLongError => SR.DomainNameTooLongError,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unexpected {nameof(type)} value: {type}")
        };

        [DoesNotReturn]
        public static void ThrowStringPrepFormatException(StringPrepErrorType errorType)
        {
            throw new StringPrepFormatException(errorType.GetErrorMessage(), errorType);
        }

        [DoesNotReturn]
        public static void ThrowStringPrepFormatException(StringPrepErrorType errorType, string rules, int errorPosition)
        {
            throw new StringPrepFormatException(errorType.GetErrorMessage(), errorType, rules, errorPosition);
        }
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
    public class StringPrepFormatException : FormatException // ICU4N specific - renamed from StringPrepParseException
    {
        /// <summary>
        /// Construct a ParseException object with the given message
        /// and error code
        /// </summary>
        /// <param name="message">A string describing the type of error that occurred.</param>
        /// <param name="error">The error that has occurred.</param>
        /// <stable>ICU 2.8</stable>
        public StringPrepFormatException(string? message, StringPrepErrorType error)
            : base(message)
        {
            this.error = error;
            this.rules = string.Empty;
            this.pos = 0;
            this.line = -1;
        }

        /// <summary>
        /// Construct a ParseException object with the given message and
        /// error code.
        /// </summary>
        /// <param name="message">A string describing the type of error that occurred.</param>
        /// <param name="error">The error that has occurred.</param>
        /// <param name="rules">The input rules string.</param>
        /// <param name="pos">The position of error in the rules string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rules"/> is <c>null</c>.</exception>
        /// <stable>ICU 2.8</stable>
        public StringPrepFormatException(string? message, StringPrepErrorType error, string rules, int pos)
            : base(message)
        {
            if (rules is null)
                throw new ArgumentNullException(nameof(rules));
            if (pos < 0)
                throw new ArgumentOutOfRangeException(nameof(pos));

            this.error = error;
            this.rules = rules;
            this.pos = pos;
            this.line = -1;
        }

        /// <summary>
        /// Construct a ParseException object with the given message and error code.
        /// </summary>
        /// <param name="message">A string describing the type of error that occurred.</param>
        /// <param name="error">The error that has occurred.</param>
        /// <param name="rules">The input rules string.</param>
        /// <param name="pos">The position of error in the rules string.</param>
        /// <param name="lineNumber">
        /// The line number at which the error has occurred.
        /// If the parse engine is not using this field, it should set it to zero. Otherwise
        /// it should be a positive integer. The default value of this field
        /// is -1. It will be set to 0 if the code populating this struct is not
        /// using line numbers.
        /// </param>
        /// <stable>ICU 2.8</stable>
        public StringPrepFormatException(string message, StringPrepErrorType error, string rules, int pos, int lineNumber)
            : base(message)
        {
            if (rules is null)
                throw new ArgumentNullException(nameof(rules));
            if (pos < 0)
                throw new ArgumentOutOfRangeException(nameof(pos));
            if (lineNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(lineNumber));

            this.error = error;
            this.rules = rules;
            this.pos = pos;
            this.line = lineNumber;
        }

#if FEATURE_SERIALIZABLE_EXCEPTIONS
        /// <summary>
        /// Initializes a new instance of this class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected StringPrepFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            line = info.GetInt32(LineName);
            rules = info.GetString(RulesName);
            pos = info.GetInt32(PositionName);
        }

        /// <summary>
        /// Sets the <see cref="System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(LineName, line);
            info.AddValue(RulesName, rules);
            info.AddValue(PositionName, pos);

            base.GetObjectData(info, context);
        }
#endif

        /// <summary>
        /// Compare this <see cref="StringPrepFormatException"/> to another and evaluate if they are equal.
        /// The comparison works only on the type of error and does not compare
        /// the rules strings, if any, for equality.
        /// </summary>
        /// <param name="other">The exception that this object should be compared to.</param>
        /// <returns><c>true</c> if the objects are equal, <c>false</c> if unequal.</returns>
        /// <stable>ICU 2.8</stable>
        public override bool Equals(object? other)
        {
            if (other is StringPrepFormatException otherParseException)
                return otherParseException.error == this.error;

            return false;
        }

        /// <summary>
        /// Gets a hash code for the current <see cref="StringPrepFormatException"/>.
        /// </summary>
        /// <internal/>
        // [Obsolete("This API is ICU internal only.")] // ICU4N: Not possible for GetHashCode() to be obsolete, since it is required by the framework
        public override int GetHashCode()
        {
            // ICU4N specific - implemented hash code
            return error.GetHashCode();
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
            buf.Append(PreContext);
            buf.Append(". postContext: ");
            buf.Append(PostContext);
            buf.Append("\n");
            return buf.ToString();
        }

        private readonly StringPrepErrorType error;

        /// <summary>
        /// The line on which the error occurred.  If the parse engine
        /// is not using this field, it should set it to zero.  Otherwise
        /// it should be a positive integer. The default value of this field
        /// is -1. It will be set to 0 if the code populating this class is not
        /// using line numbers.
        /// </summary>
        [NonSerialized]
        private readonly int line;

        /// <summary>
        /// The rules string that was passed into the constructor,
        /// which is used to get the <see cref="PreContext"/> and
        /// <see cref="PostContext"/> values.
        /// </summary>
        [NonSerialized]
        private readonly string rules;

        /// <summary>
        /// The position that was passed into the constructor,
        /// which is used to get the <see cref="PreContext"/> and
        /// <see cref="PostContext"/> values.
        /// </summary>
        [NonSerialized]
        private readonly int pos;

        private const int PARSE_CONTEXT_LEN = 16;

#if FEATURE_SERIALIZABLE_EXCEPTIONS

        private const string LineName = "line";
        private const string RulesName = "rules";
        private const string PositionName = "position";

#endif

        /// <summary>
        /// Textual context before the error.
        /// May be the empty string if not implemented by parser.
        /// </summary>
        private ReadOnlySpan<char> PreContext
        {
            get
            {
                int start = (pos <= PARSE_CONTEXT_LEN) ? 0 : (pos - (PARSE_CONTEXT_LEN - 1));
                int len = (start <= PARSE_CONTEXT_LEN) ? start : PARSE_CONTEXT_LEN;
                return rules.AsSpan(start, len);
            }
        }

        /// <summary>
        /// Textual context after the error.
        /// May be the empty string if not implemented by parser.
        /// </summary>
        private ReadOnlySpan<char> PostContext
        {
            get
            {
                int start = pos;
                int len = rules.Length - start;
                return rules.AsSpan(start, len);
            }
        }

        /// <summary>
        /// Returns the error code of this exception.
        /// This method is only used for testing to verify the error.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public virtual StringPrepErrorType Error => error;

        /// <summary>
        /// Gets the line on which the error occurred.
        /// </summary>
        public virtual int Line => line; // ICU4N specific - FormatException doesn't have a line number
    }
}
