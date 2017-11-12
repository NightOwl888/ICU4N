using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl
{
    // 1.3 compatibility layer
    public class Assert
    {
        public static void Fail(Exception e)
        {
            Fail(e.ToString()); // can't wrap exceptions in jdk 1.3
        }
        public static void Fail(String msg)
        {
            throw new InvalidOperationException("failure '" + msg + "'");
        }
        public static void Assrt(bool val)
        {
            if (!val) throw new InvalidOperationException("assert failed");
        }
        public static void Assrt(String msg, bool val)
        {
            if (!val) throw new InvalidOperationException("assert '" + msg + "' failed");
        }
    }
}
