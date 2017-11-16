using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Util
{
    public class UResourceBundleEnumerator : IEnumerator<UResourceBundle>
    {
        private UResourceBundle bundle;
        private int index = 0;
        private int size = 0;
        /**
         * Construct a resource bundle iterator for the
         * given resource bundle
         * 
         * @param bndl The resource bundle to iterate over
         * @stable ICU 3.8
         */
        public UResourceBundleEnumerator(UResourceBundle bndl)
        {
            bundle = bndl;
            size = bundle.Length;
        }

        /**
         * Returns the next element of this iterator if this iterator object has at least one more element to provide
         * @return the UResourceBundle object
         * @throws NoSuchElementException If there does not exist such an element.
         * @stable ICU 3.8
         */
        private UResourceBundle Next()
        {
            if (index < size)
            {
                return bundle.Get(index++);
            }
            return null;
            //throw new NoSuchElementException();
        }
        ///**
        // * Returns the next String of this iterator if this iterator object has at least one more element to provide
        // * @return the UResourceBundle object
        // * @throws NoSuchElementException If there does not exist such an element.
        // * @throws UResourceTypeMismatchException If resource has a type mismatch.
        // * @stable ICU 3.8
        // */
        //private string NextString()
        //        {
        //        if(index<size){
        //            return bundle.GetString(index++);
        //        }
        //            return null;
        //        //throw new NoSuchElementException();
        //    }

        /**
         * Resets the internal context of a resource so that iteration starts from the first element.
         * @stable ICU 3.8
         */
        public virtual void Reset()
        {
            //reset the internal context   
            index = 0;
        }

        /**
         * Checks whether the given resource has another element to iterate over.
         * @return TRUE if there are more elements, FALSE if there is no more elements
         * @stable ICU 3.8
         */
        public virtual bool HasNext
        {
            get { return index < size; }
        }

        #region .NET Compatibility

        private UResourceBundle current;


        public bool MoveNext()
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
