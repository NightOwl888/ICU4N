using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// If <paramref name="obj"/> is type <see cref="string"/>, <see cref="StringBuilder"/>, or
        /// <see cref="T:char[]"/>, it is wrapped in an adapter class that implements <see cref="ICharSequence"/>
        /// and returned. If the object already is <see cref="ICharSequence"/> it is cast to <see cref="ICharSequence"/>
        /// unchanged. If <paramref name="obj"/> is another type, the result is null.
        /// </summary>
        internal static ICharSequence ConvertToCharSequence<T>(this T obj) where T : class
        {
            if (obj is ICharSequence)
            {
                return obj as ICharSequence;
            }
            else if (obj is string)
            {
                return (obj as string).ToCharSequence();
            }
            else if (obj is StringBuilder)
            {
                return (obj as StringBuilder).ToCharSequence();
            }
            else if (obj is char[])
            {
                return (obj as char[]).ToCharSequence();
            }
            else 
            {
                return null;
            }
        }
    }
}
