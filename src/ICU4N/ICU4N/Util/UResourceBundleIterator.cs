using System;
using System.Collections;
using System.Collections.Generic;

namespace ICU4N.Util
{
    /// <summary>
    /// Class for enabling iteration over <see cref="UResourceBundle"/> objects.
    /// </summary>
    /// <remarks>
    /// Example of use:
    /// <code>
    /// ICUResourceBundleEnumerator iterator = resB.GetEnumerator();
    /// ICUResourceBundle temp;
    /// while (iterator.MoveNext())
    /// {
    ///     temp = iterator.Current;
    ///     int type = temp.Type;
    ///     switch (type)
    ///     {
    ///         case UResourceBundle.STRING:
    ///             str = temp.GetString();
    ///             break;
    ///         case UResourceBundle.INT32:
    ///             integer = temp.GetInt32();
    ///             break;
    ///         .....
    ///     }
    ///     // do something interesting with data collected
    /// }
    /// </code>
    /// </remarks>
    /// <author>ram</author>
    /// <stable>ICU 3.8</stable>
    public class UResourceBundleEnumerator : IEnumerator<UResourceBundle> // ICU4N TODO: API Rename UResourceManagerEnumerator, Update code sample
    {
        private UResourceBundle bundle;
        private int index = 0;
        private int size = 0;

        /// <summary>
        /// Construct a resource bundle iterator for the
        /// given resource bundle.
        /// </summary>
        /// <param name="bndl">The resource bundle to iterate over.</param>
        /// <stable>ICU 3.8</stable>
        public UResourceBundleEnumerator(UResourceBundle bndl)
        {
            bundle = bndl;
            size = bundle.Length;
        }

        /// <summary>
        /// Returns the next element of this iterator if this iterator object has at least one more element to provide.
        /// </summary>
        /// <returns>The UResourceBundle object.</returns>
        /// <stable>ICU 3.8</stable>
        private UResourceBundle Next()
        {
            if (index < size)
            {
                return bundle.Get(index++);
            }
            return null;
        }

        // ICU4N specific - removed NextString() method.
        // Alternative: iter.MoveNext(); string str = iter.Current.GetString();

        /// <summary>
        /// Resets the internal context of a resource so that iteration starts from the first element.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public virtual void Reset()
        {
            //reset the internal context   
            index = 0;
        }

        /// <summary>
        /// Checks whether the given resource has another element to iterate over.
        /// TRUE if there are more elements, FALSE if there is no more elements.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public virtual bool HasNext
        {
            get { return index < size; }
        }

        #region .NET Compatibility

        private UResourceBundle current;

        /// <summary>
        /// Advances to the next element of this enumerator if this enumerator object has at least one more element to provide.
        /// </summary>
        /// <returns>true if another bundle is available; otherwise false.</returns>
        /// <stable>ICU 3.8</stable>
        public virtual bool MoveNext()
        {
            if (!HasNext)
                return false;
            current = Next();
            return current != null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // nothing to do
        }


        public UResourceBundle Current => current;

        object IEnumerator.Current => current;

        #endregion
    }
}
