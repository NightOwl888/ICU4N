using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    public interface ICharSequence
    {
        int Length { get; }

        char this[int index] { get; }

        ICharSequence SubSequence(int start, int end);

        string ToString();
    }

    internal static class CharSequenceExtensions
    {
        public static ICharSequence ToCharSequence(this ICharSequence csq)
        {
            return csq;
        }
    }
}
