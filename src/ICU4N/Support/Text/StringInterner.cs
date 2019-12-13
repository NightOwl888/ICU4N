//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ICU4N.Support.Text
//{
//    /// <summary> Subclasses of StringInterner are required to
//    /// return the same single String object for all equal strings.
//    /// Depending on the implementation, this may not be
//    /// the same object returned as String.intern().
//    /// 
//    /// This StringInterner base class simply delegates to String.Intern().
//    /// </summary>
//    public class StringInterner
//    {
//        /// <summary>Returns a single object instance for each equal string. </summary>
//        public virtual string Intern(string s)
//        {
//#if !NETSTANDARD1_3
//            return string.Intern(s);
//#else
//            throw new PlatformNotSupportedException("string.Intern not supported.  Use SimpleStringInterner.");
//#endif

//        }

//        /// <summary>Returns a single object instance for each equal string. </summary>
//        public virtual string Intern(char[] arr, int offset, int len)
//        {
//            return Intern(new string(arr, offset, len));
//        }
//    }
//}
