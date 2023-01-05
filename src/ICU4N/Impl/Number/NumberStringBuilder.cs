using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using static ICU4N.Numerics.BigDecimal;
using Field = ICU4N.Text.NumberFormatField;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A StringBuilder optimized for number formatting. It implements the following key features beyond a normal .NET
    /// StringBuilder:
    /// 
    /// <list type="number">
    ///     <item><description>Efficient prepend as well as append.</description></item>
    ///     <item><description>Keeps tracks of Fields in an efficient manner.</description></item>
    ///     <item><description>String operations are fast-pathed to code point operations when possible.</description></item>
    /// </list>
    /// </summary>
    internal partial class NumberStringBuilder : ICharSequence // ICU4N TODO: API - this was public in ICU4J
    {
        // ICU4N specific
        bool ICharSequence.HasValue => true;


        /// <summary>
        /// A constant, empty NumberStringBuilder. Do NOT call mutative operations on this.
        /// </summary>
        public static readonly NumberStringBuilder EMPTY = new NumberStringBuilder();

        private char[] chars;
        internal Field[] fields;
        internal int zero;
        internal int length;
        private Fields fieldsInstance;

        public NumberStringBuilder()
            : this(40)
        {
        }
        public NumberStringBuilder(int capacity)
        {
            chars = new char[capacity];
            fields = new Field[capacity];
            zero = capacity / 2;
            length = 0;
            fieldsInstance = new Fields(this);
        }

        public NumberStringBuilder(NumberStringBuilder source)
        {
            CopyFrom(source);
        }

        public void CopyFrom(NumberStringBuilder source)
        {
            chars = new char[source.chars.Length];
            fields = new Field[source.fields.Length];
            Array.Copy(source.chars, chars, chars.Length);
            Array.Copy(source.fields, fields, fields.Length);
            //chars = Arrays.copyOf(source.chars, source.chars.length);
            //fields = Arrays.copyOf(source.fields, source.fields.length);
            zero = source.zero;
            length = source.length;
            fieldsInstance = new Fields(this);
        }

        public virtual int Length => length;



        public virtual int CodePointCount => Character.CodePointCount(this, 0, Length);

        public virtual char this[int index]
        {
            get
            {
                if (index < 0 || index > length - 1)
                    throw new IndexOutOfRangeException(nameof(index));
                return chars[zero + index];
            }
        }

        public Fields Fields => fieldsInstance; // ICU4N TODO: This was extensible in ICU
        

        public virtual int GetFirstCodePoint()
        {
            if (length == 0)
            {
                return -1;
            }
            return Character.CodePointAt(chars, zero, zero + length);
        }

        public virtual int GetLastCodePoint()
        {
            if (length == 0)
            {
                return -1;
            }
            return Character.CodePointBefore(chars, zero + length, zero);
        }

        public virtual int CodePointAt(int index)
        {
            return Character.CodePointAt(chars, zero + index, zero + length);
        }

        public virtual int CodePointBefore(int index)
        {
            return Character.CodePointBefore(chars, zero + index, zero);
        }

        public virtual NumberStringBuilder Clear()
        {
            zero = Capacity / 2;
            length = 0;
            return this;
        }

        /// <summary>
        /// Appends the specified <paramref name="codePoint"/> to the end of the string.
        /// </summary>
        /// <returns>The number of chars added: 1 if the code point is in the BMP, or 2 otherwise.</returns>
        public virtual int AppendCodePoint(int codePoint, Field field)
        {
            return InsertCodePoint(length, codePoint, field);
        }

        /// <summary>
        /// Inserts the specified <paramref name="codePoint"/> at the specified index in the string.
        /// </summary>
        /// <returns>The number of chars added: 1 if the code point is in the BMP, or 2 otherwise.</returns>
        public virtual int InsertCodePoint(int index, int codePoint, Field field)
        {
            int count = Character.CharCount(codePoint);
            int position = PrepareForInsert(index, count);
            Character.ToChars(codePoint, chars, position);
            fields[position] = field;
            if (count == 2)
                fields[position + 1] = field;
            return count;
        }

        // ICU4N specific - moved Append(ICharSequence sequence, Field field) to NumberStringBuilderExtension.tt

        // ICU4N specific - moved Insert(int index, ICharSequence sequence, Field field) to NumberStringBuilderExtension.tt

        // ICU4N specific - moved Append(ICharSequence sequence, Field field) to NumberStringBuilderExtension.tt

        /// <summary>
        /// Appends the chars in the specified char array to the end of the string, and associates them with the fields in
        /// the specified field array, which must have the same length as chars.
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="fields"></param>
        /// <returns>The number of chars added, which is the length of the char array.</returns>
        public virtual int Append(char[] chars, Field[] fields)
        {
            return Insert(length, chars, fields);
        }

        /// <summary>
        /// Inserts the chars in the specified char array at the specified index in the string, and associates them with the
        /// fields in the specified field array, which must have the same length as chars.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="chars"></param>
        /// <param name="fields"></param>
        /// <returns>The number of chars added, which is the length of the char array.</returns>
        public virtual int Insert(int index, char[] chars, Field[] fields)
        {
            Debug.Assert(fields == null || chars.Length == fields.Length);
            int count = chars.Length;
            if (count == 0)
                return 0; // nothing to insert
            int position = PrepareForInsert(index, count);
            for (int i = 0; i < count; i++)
            {
                this.chars[position + i] = chars[i];
                this.fields[position + i] = fields == null ? null : fields[i];
            }
            return count;
        }

        /// <summary>
        /// Appends the contents of another <see cref="NumberStringBuilder"/> to the end of this instance.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The number of chars added, which is the length of the other <see cref="NumberStringBuilder"/>.</returns>
        public virtual int Append(NumberStringBuilder other)
        {
            return Insert(length, other);
        }

        /// <summary>
        /// Inserts the contents of another <see cref="NumberStringBuilder"/> into this instance at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="other"></param>
        /// <returns>The number of chars added, which is the length of the other <see cref="NumberStringBuilder"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public virtual int Insert(int index, NumberStringBuilder other)
        {
            if (this == other)
            {
                throw new ArgumentException("Cannot call insert/append on myself");
            }
            int count = other.length;
            if (count == 0)
            {
                // Nothing to insert.
                return 0;
            }
            int position = PrepareForInsert(index, count);
            for (int i = 0; i < count; i++)
            {
                this.chars[position + i] = other[i];
                this.fields[position + i] = other.Fields[i];
            }
            return count;
        }

        /// <summary>
        /// Shifts around existing data if necessary to make room for new characters.
        /// </summary>
        /// <param name="index">The location in the string where the operation is to take place.</param>
        /// <param name="count">The number of chars (UTF-16 code units) to be inserted at that location.</param>
        /// <returns>The position in the char array to insert the chars.</returns>
        private int PrepareForInsert(int index, int count)
        {
            if (index == 0 && zero - count >= 0)
            {
                // Append to start
                zero -= count;
                length += count;
                return zero;
            }
            else if (index == length && zero + length + count < Capacity)
            {
                // Append to end
                length += count;
                return zero + length - count;
            }
            else
            {
                // Move chars around and/or allocate more space
                return PrepareForInsertHelper(index, count);
            }
        }

#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private int PrepareForInsertHelper(int index, int count)
        {
            // Java note: Keeping this code out of prepareForInsert() increases the speed of append operations.
            int oldCapacity = Capacity;
            int oldZero = zero;
            char[] oldChars = chars;
            Field[] oldFields = fields;
            if (length + count > oldCapacity)
            {
                int newCapacity = (length + count) * 2;
                int newZero = newCapacity / 2 - (length + count) / 2;

                char[] newChars = new char[newCapacity];
                Field[] newFields = new Field[newCapacity];

                // First copy the prefix and then the suffix, leaving room for the new chars that the
                // caller wants to insert.
                Array.Copy(oldChars, oldZero, newChars, newZero, index);
                Array.Copy(oldChars, oldZero + index, newChars, newZero + index + count, length - index);
                Array.Copy(oldFields, oldZero, newFields, newZero, index);
                Array.Copy(oldFields, oldZero + index, newFields, newZero + index + count, length - index);

                chars = newChars;
                fields = newFields;
                zero = newZero;
                length += count;
            }
            else
            {
                int newZero = oldCapacity / 2 - (length + count) / 2;

                // First copy the entire string to the location of the prefix, and then move the suffix
                // to make room for the new chars that the caller wants to insert.
                Array.Copy(oldChars, oldZero, oldChars, newZero, length);
                Array.Copy(oldChars, newZero + index, oldChars, newZero + index + count, length - index);
                Array.Copy(oldFields, oldZero, oldFields, newZero, length);
                Array.Copy(oldFields, newZero + index, oldFields, newZero + index + count, length - index);

                zero = newZero;
                length += count;
            }
            return zero + index;
        }

        private int Capacity => chars.Length;

        public virtual ICharSequence Subsequence(int startIndex, int length)
        {
            // From Apache Harmony String class
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (startIndex > this.length - length) // Checks for int overflow
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_IndexLength);

            int end = startIndex + length;

            NumberStringBuilder other = new NumberStringBuilder(this);
            other.zero = zero + startIndex;
            other.length = end - startIndex;
            return other;
        }

        /// <summary>
        /// Returns the string represented by the characters in this string builder.
        /// <para/>
        /// For a string intended be used for debugging, use <see cref="ToDebugString()"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new string(chars, zero, length);
        }

        /// <summary>
        /// Returns a slice of the string represented by the characters in this string builder.
        /// <para/>
        /// For a string intended be used for debugging, use <see cref="ToDebugString()"/>.
        /// </summary>
        /// <returns></returns>
        public virtual string ToString(int startIndex, int length) // ICU4N specific
        {
            return new string(chars, startIndex, length);
        }

        private static readonly IDictionary<Field, char> fieldToDebugChar = new Dictionary<Field, char>
        {
            [Field.Sign] = '-',
            [Field.Integer] = 'i',
            [Field.Fraction] = 'f',
            [Field.Exponent] = 'e',
            [Field.ExponentSign] = '+',
            [Field.ExponentSymbol] = 'E',
            [Field.DecimalSeparator] = '.',
            [Field.GroupingSeparator] = ',',
            [Field.Percent] = '%',
            [Field.PerMille] = '‰',
            [Field.Currency] = '$',
        };

        /// <summary>
        /// Returns a string that includes field information, for debugging purposes.
        /// <para/>
        /// For example, if the string is "-12.345", the debug string will be something like "&lt;NumberStringBuilder
        /// [-123.45] [-iii.ff]&gt;"
        /// </summary>
        /// <returns>A string for debugging purposes.</returns>
        public virtual string ToDebugString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<NumberStringBuilder [");
            sb.Append(this.ToString());
            sb.Append("] [");
            for (int i = zero; i < zero + length; i++)
            {
                if (fields[i] == null)
                {
                    sb.Append('n');
                }
                else
                {
                    sb.Append(fieldToDebugChar[fields[i]]);
                }
            }
            sb.Append("]>");
            return sb.ToString();
        }

        /// <summary>
        /// Returns a new array containing the contents of this string builder.
        /// </summary>
        /// <returns>A new array containing the contents of this string builder.</returns>
        public virtual char[] ToCharArray()
        {
            var result = new char[length];
            Array.Copy(chars, zero, result, destinationIndex: 0, length);
            return result;

            //return Arrays.copyOfRange(chars, zero, zero + length);
        }

        /// <summary>
        /// Returns a new array containing the field values of this string builder.
        /// </summary>
        /// <returns>A new array containing the field values of this string builder.</returns>
        public virtual Field[] ToFieldArray()
        {
            var result = new Field[length];
            Array.Copy(fields, zero, result, destinationIndex: 0, length);
            return result;

            //return Arrays.copyOfRange(fields, zero, zero + length);
        }

        /// <summary>
        /// Whether the contents and field values of this string builder are equal to the given chars and fields.
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        /// <seealso cref="ToCharArray()"/>
        /// <seealso cref="ToFieldArray()"/>
        public virtual bool ContentEquals(char[] chars, Field[] fields)
        {
            if (chars.Length != length)
                return false;
            if (fields.Length != length)
                return false;
            for (int i = 0; i < length; i++)
            {
                if (this.chars[zero + i] != chars[i])
                    return false;
                if (this.fields[zero + i] != fields[i])
                    return false;
            }
            return true;
        }

        /// <param name="other">The instance to compare.</param>
        /// <returns>Whether the contents of this instance is currently equal to the given instance.</returns>
        public virtual bool ContentEquals(NumberStringBuilder other)
        {
            if (length != other.Length)
                return false;
            for (int i = 0; i < length; i++)
            {
                if (this[i] != other[i] || Fields[i] != other.Fields[i])
                {
                    return false;
                }
            }
            return true;
        }

        // ICU4N TODO: Figure out how to deal with this
        //public override int GetHashCode()
        //{
        //    throw new NotSupportedException("Don't call #hashCode() or #equals() on a mutable.");
        //}

        //public override bool Equals(object other)
        //{
        //    throw new NotSupportedException("Don't call #hashCode() or #equals() on a mutable.");
        //}

        /// <summary>
        /// Populates the given <see cref="FieldPosition"/> based on this string builder.
        /// </summary>
        /// <param name="fp">The <see cref="FieldPosition"/> to populate.</param>
        /// <param name="offset">An offset to add to the field position index; can be zero.</param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void PopulateFieldPosition(FieldPosition fp, int offset)
        {
            FormatField rawField = fp.FieldAttribute;

            if (rawField == null)
            {
                // Backwards compatibility: read from fp.getField()
                if (fp.Field == NumberFormat.IntegerField)
                {
                    rawField = NumberFormatField.Integer;
                }
                else if (fp.Field == NumberFormat.FractionField)
                {
                    rawField = NumberFormatField.Fraction;
                }
                else
                {
                    // No field is set
                    return;
                }
            }

            /* com.ibm.icu.text.NumberFormat. */
            if (!(rawField is Field field))
            {
                throw new ArgumentException(
                        "You must pass an instance of ICU4N.Text.NumberFormatField as your FieldPosition attribute.  You passed: "
                                + rawField.GetType().ToString());
            }

            bool seenStart = false;
            int fractionStart = -1;
            for (int i = zero; i <= zero + length; i++)
            {
                Field _field = (i < zero + length) ? fields[i] : null;
                if (seenStart && field != _field)
                {
                    // Special case: GROUPING_SEPARATOR counts as an INTEGER.
                    if (field == Field.Integer && _field == Field.GroupingSeparator)
                    {
                        continue;
                    }
                    fp.EndIndex = i - zero + offset;
                    break;
                }
                else if (!seenStart && field == _field)
                {
                    fp.BeginIndex = i - zero + offset;
                    seenStart = true;
                }
                if (_field == Field.Integer || _field == Field.DecimalSeparator)
                {
                    fractionStart = i - zero + 1;
                }
            }

            // Backwards compatibility: FRACTION needs to start after INTEGER if empty
            if (field == Field.Fraction && !seenStart)
            {
                fp.BeginIndex = (fractionStart + offset);
                fp.EndIndex = (fractionStart + offset);
            }
        }

        public virtual AttributedCharacterIterator GetIterator()
        {
            AttributedString @as = new AttributedString(ToString());
            Field current = null;
            int currentStart = -1;
            for (int i = 0; i < length; i++)
            {
                Field field = fields[i + zero];
                if (current == Field.Integer && field == Field.GroupingSeparator)
                {
                    // Special case: GROUPING_SEPARATOR counts as an INTEGER.
                    @as.AddAttribute(Field.GroupingSeparator, Field.GroupingSeparator, i, i + 1);
                }
                else if (current != field)
                {
                    if (current != null)
                    {
                        @as.AddAttribute(current, current, currentStart, i);
                    }
                    current = field;
                    currentStart = i;
                }
            }
            if (current != null)
            {
                @as.AddAttribute(current, current, currentStart, length);
            }

            return @as.GetIterator();
        }

    }

    internal struct Fields
    {
        private readonly NumberStringBuilder numberStringBuilder;

        internal Fields(NumberStringBuilder numberStringBuilder)
        {
            this.numberStringBuilder = numberStringBuilder ?? throw new ArgumentNullException(nameof(numberStringBuilder));
        }

        public Field this[int index]
        {
            get
            {
                if (index < 0 || index > numberStringBuilder.length - 1)
                    throw new IndexOutOfRangeException(nameof(index));
                return numberStringBuilder.fields[numberStringBuilder.zero + index];
            }
        }
    }
}
