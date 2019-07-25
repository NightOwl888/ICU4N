using ICU4N.Impl;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// A replacer that produces static text as its output.  The text may
    /// contain transliterator stand-in characters that represent nested
    /// <see cref="IUnicodeReplacer"/> objects, making it possible to encode a tree of
    /// replacers in a <see cref="StringReplacer"/>.  A <see cref="StringReplacer"/> that contains such
    /// stand-ins is called a <em>complex</em> <see cref="StringReplacer"/>.  A complex
    /// <see cref="StringReplacer"/> has a slower processing loop than a non-complex one.
    /// </summary>
    /// <author>Alan Liu</author>
    internal class StringReplacer : IUnicodeReplacer
    {
        /// <summary>
        /// Output text, possibly containing stand-in characters that
        /// represent nested <see cref="IUnicodeReplacer"/>s.
        /// </summary>
        private string output;

        /// <summary>
        /// Cursor position.  Value is ignored if hasCursor is false.
        /// </summary>
        private int cursorPos;

        /// <summary>
        /// True if this object outputs a cursor position.
        /// </summary>
        private bool hasCursor;

        /// <summary>
        /// A complex object contains nested replacers and requires more
        /// complex processing.  StringReplacers are initially assumed to
        /// be complex.  If no nested replacers are seen during processing,
        /// then isComplex is set to false, and future replacements are
        /// short circuited for better performance.
        /// </summary>
        private bool isComplex;

        /// <summary>
        /// Object that translates stand-in characters in 'output' to
        /// <see cref="IUnicodeReplacer"/> objects.
        /// </summary>
#pragma warning disable 612, 618
        private readonly RuleBasedTransliterator.Data data;
#pragma warning restore 612, 618

        /// <summary>
        /// Construct a <see cref="StringReplacer"/> that sets the emits the given output
        /// text and sets the cursor to the given position.
        /// </summary>
        /// <param name="theOutput">Text that will replace input text when the
        /// <see cref="Replace(IReplaceable, int, int, out int[])"/> method is called.  May contain stand-in characters
        /// that represent nested replacers.</param>
        /// <param name="theCursorPos">Cursor position that will be returned by 
        /// the <see cref="Replace(IReplaceable, int, int, out int[])"/> method.</param>
        /// <param name="theData">Transliterator context object that translates
        /// stand-in characters to <see cref="IUnicodeReplacer"/> objects.</param>
        public StringReplacer(string theOutput,
                              int theCursorPos,
#pragma warning disable 612, 618
                              RuleBasedTransliterator.Data theData)
#pragma warning restore 612, 618
        {
            output = theOutput;
            cursorPos = theCursorPos;
            hasCursor = true;
            data = theData;
            isComplex = true;
        }

        /// <summary>
        /// Construct a <see cref="StringReplacer"/> that sets the emits the given output
        /// text and does not modify the cursor.
        /// </summary>
        /// <param name="theOutput">Text that will replace input text when the
        /// <see cref="Replace(IReplaceable, int, int, out int[])"/> method is called.  
        /// May contain stand-in characters that represent nested replacers.</param>
        /// <param name="theData">Transliterator context object that translates
        /// stand-in characters to <see cref="IUnicodeReplacer"/> objects.</param>
        public StringReplacer(string theOutput,
#pragma warning disable 612, 618
                              RuleBasedTransliterator.Data theData)
#pragma warning restore 612, 618
        {
            output = theOutput;
            cursorPos = 0;
            hasCursor = false;
            data = theData;
            isComplex = true;
        }

        //=    public static UnicodeReplacer valueOf(String output,
        //=                                          int cursorPos,
        //=                                          RuleBasedTransliterator.Data data) {
        //=        if (output.length() == 1) {
        //=            char c = output.charAt(0);
        //=            UnicodeReplacer r = data.lookupReplacer(c);
        //=            if (r != null) {
        //=                return r;
        //=            }
        //=        }
        //=        return new StringReplacer(output, cursorPos, data);
        //=    }

        /// <summary>
        /// <see cref="IUnicodeReplacer"/> API
        /// </summary>
        public virtual int Replace(IReplaceable text,
                           int start,
                           int limit,
                           int[] cursor)
        {
            int outLen;
            int newStart = 0;

            // NOTE: It should be possible to _always_ run the complex
            // processing code; just slower.  If not, then there is a bug
            // in the complex processing code.

            // Simple (no nested replacers) Processing Code :
            if (!isComplex)
            {
                text.Replace(start, limit, output);
                outLen = output.Length;

                // Setup default cursor position (for cursorPos within output)
                newStart = cursorPos;
            }

            // Complex (nested replacers) Processing Code :
            else
            {
                /* When there are segments to be copied, use the Replaceable.copy()
                 * API in order to retain out-of-band data.  Copy everything to the
                 * end of the string, then copy them back over the key.  This preserves
                 * the integrity of indices into the key and surrounding context while
                 * generating the output text.
                 */
                StringBuffer buf = new StringBuffer();
                int oOutput; // offset into 'output'
                isComplex = false;

                // The temporary buffer starts at tempStart, and extends
                // to destLimit + tempExtra.  The start of the buffer has a single
                // character from before the key.  This provides style
                // data when addition characters are filled into the
                // temporary buffer.  If there is nothing to the left, use
                // the non-character U+FFFF, which Replaceable subclasses
                // should treat specially as a "no-style character."
                // destStart points to the point after the style context
                // character, so it is tempStart+1 or tempStart+2.
                int tempStart = text.Length; // start of temp buffer
                int destStart = tempStart; // copy new text to here
                if (start > 0)
                {
                    int len = UTF16.GetCharCount(text.Char32At(start - 1));
                    text.Copy(start - len, start, tempStart);
                    destStart += len;
                }
                else
                {
                    text.Replace(tempStart, tempStart, "\uFFFF");
                    destStart++;
                }
                int destLimit = destStart;
                int tempExtra = 0; // temp chars after destLimit

                for (oOutput = 0; oOutput < output.Length;)
                {
                    if (oOutput == cursorPos)
                    {
                        // Record the position of the cursor
                        newStart = buf.Length + destLimit - destStart; // relative to start
                                                                       // the buf.length() was inserted for bug 5789
                                                                       // the problem is that if we are accumulating into a buffer (when r == null below)
                                                                       // then the actual length of the text at that point needs to add the buf length.
                                                                       // there was an alternative suggested in #5789, but that looks like it won't work
                                                                       // if we have accumulated some stuff in the dest part AND have a non-zero buffer.
                    }
                    int c = UTF16.CharAt(output, oOutput);

                    // When we are at the last position copy the right style
                    // context character into the temporary buffer.  We don't
                    // do this before because it will provide an incorrect
                    // right context for previous replace() operations.
                    int nextIndex = oOutput + UTF16.GetCharCount(c);
                    if (nextIndex == output.Length)
                    {
                        tempExtra = UTF16.GetCharCount(text.Char32At(limit));
                        text.Copy(limit, limit + tempExtra, destLimit);
                    }

                    IUnicodeReplacer r = data.LookupReplacer(c);
                    if (r == null)
                    {
                        // Accumulate straight (non-segment) text.
                        UTF16.Append(buf, c);
                    }
                    else
                    {
                        isComplex = true;

                        // Insert any accumulated straight text.
                        if (buf.Length > 0)
                        {
                            text.Replace(destLimit, destLimit, buf.ToString());
                            destLimit += buf.Length;
                            buf.Length = 0;
                        }

                        // Delegate output generation to replacer object
                        int len = r.Replace(text, destLimit, destLimit, cursor);
                        destLimit += len;
                    }
                    oOutput = nextIndex;
                }
                // Insert any accumulated straight text.
                if (buf.Length > 0)
                {
                    text.Replace(destLimit, destLimit, buf.ToString());
                    destLimit += buf.Length;
                }
                if (oOutput == cursorPos)
                {
                    // Record the position of the cursor
                    newStart = destLimit - destStart; // relative to start
                }

                outLen = destLimit - destStart;

                // Copy new text to start, and delete it
                text.Copy(destStart, destLimit, start);
                text.Replace(tempStart + outLen, destLimit + tempExtra + outLen, "");

                // Delete the old text (the key)
                text.Replace(start + outLen, limit + outLen, "");
            }

            if (hasCursor)
            {
                // Adjust the cursor for positions outside the key.  These
                // refer to code points rather than code units.  If cursorPos
                // is within the output string, then use newStart, which has
                // already been set above.
                if (cursorPos < 0)
                {
                    newStart = start;
                    int n = cursorPos;
                    // Outside the output string, cursorPos counts code points
                    while (n < 0 && newStart > 0)
                    {
                        newStart -= UTF16.GetCharCount(text.Char32At(newStart - 1));
                        ++n;
                    }
                    newStart += n;
                }
                else if (cursorPos > output.Length)
                {
                    newStart = start + outLen;
                    int n = cursorPos - output.Length;
                    // Outside the output string, cursorPos counts code points
                    while (n > 0 && newStart < text.Length)
                    {
                        newStart += UTF16.GetCharCount(text.Char32At(newStart));
                        --n;
                    }
                    newStart += n;
                }
                else
                {
                    // Cursor is within output string.  It has been set up above
                    // to be relative to start.
                    newStart += start;
                }

                cursor[0] = newStart;
            }

            return outLen;
        }

        /// <summary>
        /// <see cref="IUnicodeReplacer"/> API
        /// </summary>
        public virtual string ToReplacerPattern(bool escapeUnprintable)
        {
            StringBuffer rule = new StringBuffer();
            StringBuffer quoteBuf = new StringBuffer();

            int cursor = cursorPos;

            // Handle a cursor preceding the output
            if (hasCursor && cursor < 0)
            {
                while (cursor++ < 0)
                {
                    Utility.AppendToRule(rule, '@', true, escapeUnprintable, quoteBuf);
                }
                // Fall through and append '|' below
            }

            for (int i = 0; i < output.Length; ++i)
            {
                if (hasCursor && i == cursor)
                {
                    Utility.AppendToRule(rule, '|', true, escapeUnprintable, quoteBuf);
                }
                char c = output[i]; // Ok to use 16-bits here

                IUnicodeReplacer r = data.LookupReplacer(c);
                if (r == null)
                {
                    Utility.AppendToRule(rule, c, false, escapeUnprintable, quoteBuf);
                }
                else
                {
                    StringBuffer buf = new StringBuffer(" ");
                    buf.Append(r.ToReplacerPattern(escapeUnprintable));
                    buf.Append(' ');
                    Utility.AppendToRule(rule, buf.ToString(),
                                         true, escapeUnprintable, quoteBuf);
                }
            }

            // Handle a cursor after the output.  Use > rather than >= because
            // if cursor == output.length() it is at the end of the output,
            // which is the default position, so we need not emit it.
            if (hasCursor && cursor > output.Length)
            {
                cursor -= output.Length;
                while (cursor-- > 0)
                {
                    Utility.AppendToRule(rule, '@', true, escapeUnprintable, quoteBuf);
                }
                Utility.AppendToRule(rule, '|', true, escapeUnprintable, quoteBuf);
            }
            // Flush quoteBuf out to result
            Utility.AppendToRule(rule, -1,
                                 true, escapeUnprintable, quoteBuf);

            return rule.ToString();
        }

        /// <summary>
        /// Union the set of all characters that may output by this object
        /// into the given set.
        /// </summary>
        /// <param name="toUnionTo">The set into which to union the output characters.</param>
        public virtual void AddReplacementSetTo(UnicodeSet toUnionTo)
        {
            int ch;
            for (int i = 0; i < output.Length; i += UTF16.GetCharCount(ch))
            {
                ch = UTF16.CharAt(output, i);
                IUnicodeReplacer r = data.LookupReplacer(ch);
                if (r == null)
                {
                    toUnionTo.Add(ch);
                }
                else
                {
                    r.AddReplacementSetTo(toUnionTo);
                }
            }
        }
    }
}
