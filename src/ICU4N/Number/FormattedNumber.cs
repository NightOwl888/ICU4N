using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N.Collections;
using J2N.Text;
using System;
using System.IO;
using System.Text;
using static ICU4N.Text.PluralRules;

namespace ICU4N.Numerics
{
    /// <summary>
    /// The result of a number formatting operation. This class allows the result to be exported in several data types,
    /// including a <see cref="string"/>, an <see cref="AttributedCharacterIterator"/>, and a <see cref="BigDecimal"/>.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    /// <seealso cref="NumberFormatter"/>
    internal class FormattedNumber// ICU4N TODO: API - this was public in ICU4J
    {
        internal NumberStringBuilder nsb;
        internal IDecimalQuantity fq;
        internal MicroProps micros;

        internal FormattedNumber(NumberStringBuilder nsb, IDecimalQuantity fq, MicroProps micros)
        {
            this.nsb = nsb;
            this.fq = fq;
            this.micros = micros;
        }

        /**
         * Creates a String representation of the the formatted number.
         *
         * @return a String containing the localized number.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public override string ToString()
        {
            return nsb.ToString();
        }

        /**
         * Append the formatted number to an Appendable, such as a StringBuilder. This may be slightly more efficient than
         * creating a String.
         *
         * <p>
         * If an IOException occurs when appending to the Appendable, an unchecked {@link ICUUncheckedIOException} is thrown
         * instead.
         *
         * @param appendable
         *            The Appendable to which to append the formatted number string.
         * @return The same Appendable, for chaining.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see Appendable
         * @see NumberFormatter
         */
        //public <A extends Appendable> A appendTo(A appendable)
        public virtual TAppendable AppendTo<TAppendable>(TAppendable appendable) where TAppendable : IAppendable
        {
            try
            {
                appendable.Append(nsb);
            }
            catch (IOException e)
            {
                // Throw as an unchecked exception to avoid users needing try/catch
                throw new ICUUncheckedIOException(e);
            }
            return appendable;
        }

        /**
         * Append the formatted number to an Appendable, such as a StringBuilder. This may be slightly more efficient than
         * creating a String.
         *
         * <p>
         * If an IOException occurs when appending to the Appendable, an unchecked {@link ICUUncheckedIOException} is thrown
         * instead.
         *
         * @param appendable
         *            The Appendable to which to append the formatted number string.
         * @return The same Appendable, for chaining.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see Appendable
         * @see NumberFormatter
         */
        //public <A extends Appendable> A appendTo(A appendable)
        public virtual StringBuilder AppendTo(StringBuilder appendable)
        {
            try
            {
                appendable.Append(nsb);
            }
            catch (IOException e)
            {
                // Throw as an unchecked exception to avoid users needing try/catch
                throw new ICUUncheckedIOException(e);
            }
            return appendable;
        }

        /**
         * Determine the start and end indices of the first occurrence of the given <em>field</em> in the output string.
         * This allows you to determine the locations of the integer part, fraction part, and sign.
         *
         * <p>
         * If multiple different field attributes are needed, this method can be called repeatedly, or if <em>all</em> field
         * attributes are needed, consider using getFieldIterator().
         *
         * <p>
         * If a field occurs multiple times in an output string, such as a grouping separator, this method will only ever
         * return the first occurrence. Use getFieldIterator() to access all occurrences of an attribute.
         *
         * @param fieldPosition
         *            The FieldPosition to populate with the start and end indices of the desired field.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see com.ibm.icu.text.NumberFormat.Field
         * @see NumberFormatter
         */
        public virtual void PopulateFieldPosition(FieldPosition fieldPosition)
        {
            PopulateFieldPosition(fieldPosition, 0);
        }

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public virtual void PopulateFieldPosition(FieldPosition fieldPosition, int offset)
        {
            nsb.PopulateFieldPosition(fieldPosition, offset);
            fq.PopulateUFieldPosition(fieldPosition);
        }

        /**
         * Export the formatted number as an AttributedCharacterIterator. This allows you to determine which characters in
         * the output string correspond to which <em>fields</em>, such as the integer part, fraction part, and sign.
         *
         * <p>
         * If information on only one field is needed, consider using populateFieldPosition() instead.
         *
         * @return An AttributedCharacterIterator, containing information on the field attributes of the number string.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see com.ibm.icu.text.NumberFormat.Field
         * @see AttributedCharacterIterator
         * @see NumberFormatter
         */
        public virtual AttributedCharacterIterator GetFieldIterator()
        {
            return nsb.GetIterator();
        }

        /**
         * Export the formatted number as a BigDecimal. This endpoint is useful for obtaining the exact number being printed
         * after scaling and rounding have been applied by the number formatting pipeline.
         *
         * @return A BigDecimal representation of the formatted number.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public virtual BigMath.BigDecimal ToBigDecimal()
        {
            return fq.ToBigDecimal();
        }

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public virtual string GetPrefix()
        {
            NumberStringBuilder temp = new NumberStringBuilder();
            int length = micros.modOuter.Apply(temp, 0, 0);
            length += micros.modMiddle.Apply(temp, 0, length);
            /* length += */
            micros.modInner.Apply(temp, 0, length);
            int prefixLength = micros.modOuter.PrefixLength + micros.modMiddle.PrefixLength
                    + micros.modInner.PrefixLength;
            //return temp.subSequence(0, prefixLength).toString();
            return temp.ToString(0, prefixLength); // ICU4N: Checked 2nd arg
        }

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public virtual string GetSuffix()
        {
            NumberStringBuilder temp = new NumberStringBuilder();
            int length = micros.modOuter.Apply(temp, 0, 0);
            length += micros.modMiddle.Apply(temp, 0, length);
            length += micros.modInner.Apply(temp, 0, length);
            int prefixLength = micros.modOuter.PrefixLength + micros.modMiddle.PrefixLength
                    + micros.modInner.PrefixLength;
            //return temp.subSequence(prefixLength, length).toString();
            return temp.ToString(prefixLength, length - prefixLength); // ICU4N: Corrected 2nd arg
        }

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public virtual IFixedDecimal FixedDecimal => fq;

        /**
         * {@inheritDoc}
         *
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         */
        public override int GetHashCode()
        {
            // NumberStringBuilder and BigDecimal are mutable, so we can't call
            // #equals() or #hashCode() on them directly.
            //return Arrays.hashCode(nsb.ToCharArray()) ^ Arrays.hashCode(nsb.toFieldArray()) ^ fq.toBigDecimal().hashCode();
            return ArrayEqualityComparer<char>.OneDimensional.GetHashCode(nsb.ToCharArray()) ^
                ArrayEqualityComparer<NumberFormatField>.OneDimensional.GetHashCode(nsb.ToFieldArray()) ^
                fq.ToBigDecimal().GetHashCode();
        }

        /**
         * {@inheritDoc}
         *
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         */
        public override bool Equals(object other)
        {
            if (this == other)
                return true;
            if (other == null)
                return false;
            if (!(other is FormattedNumber _other))
                return false;
            // NumberStringBuilder and BigDecimal are mutable, so we can't call
            // #equals() or #hashCode() on them directly.
            //return Arrays.equals(nsb.toCharArray(), _other.nsb.toCharArray())
            //        ^ Arrays.equals(nsb.toFieldArray(), _other.nsb.toFieldArray())
            //        ^ fq.toBigDecimal().equals(_other.fq.toBigDecimal());
            return ArrayEqualityComparer<char>.OneDimensional.Equals(nsb.ToCharArray(), _other.nsb.ToCharArray()) &&
                ArrayEqualityComparer<NumberFormatField>.OneDimensional.Equals(nsb.ToFieldArray(), _other.nsb.ToFieldArray()) &&
                fq.ToBigDecimal().Equals(_other.fq.ToBigDecimal());

        }
    }
}
