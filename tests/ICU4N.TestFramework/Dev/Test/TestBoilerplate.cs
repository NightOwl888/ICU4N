using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Text;
using J2N.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ICU4N.Dev.Test
{
    /// <summary>
    /// To use, override the abstract and the protected methods as necessary.
    /// Tests boilerplate invariants:
    /// <para/>a.Equals(a)
    /// <para/>!a.Equals(null)
    /// <para/>if a.Equals(b) then 
    /// <para/>(1) a.GetHashCode() == b.hashCode  // note: the reverse is not necessarily true.
    /// <para/>(2) a functions in all aspects as equivalent to b
    /// <para/>(3) b.Equals(a)
    /// <para/>if b = Clone(a)
    /// <para/>(1) b.Equals(a), and the above checks
    /// <para/>(2) if mutable(a), then a.Clone() != a // note: the reverse is not necessarily true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <author>Davis</author>
    public abstract class TestBoilerplate<T> : TestFmwk
    {
        protected static Random random = new Random(12345);

        public virtual void Test()
        {
            IList<T> list = new List<T>();
            while (AddTestObject(list))
            {
            }
            T[] testArray = (T[])list.ToArray();
            for (int i = 0; i < testArray.Length; ++i)
            {
                //logln("Testing " + i);
                T a = testArray[i];
                int aHash = a.GetHashCode();
                if (a.Equals(null))
                {
                    Errln("Equality/Null invariant fails: " + i);
                }
                if (!a.Equals(a))
                {
                    Errln("Self-Equality invariant fails: " + i);
                }
                T b;
                if (CanClone(a))
                {
                    b = Clone(a);
                    if (ReferenceEquals(b, a))
                    {
                        if (IsMutable(a))
                        {
                            Errln("Clone/Mutability invariant fails: " + i);
                        }
                    }
                    else
                    {
                        if (!a.Equals(b))
                        {
                            Errln("Clone/Equality invariant fails: " + i);
                        }
                    }
                    CheckEquals(i, -1, a, aHash, b);
                }
                for (int j = i; j < testArray.Length; ++j)
                {
                    b = testArray[j];
                    if (a.Equals(b)) CheckEquals(i, j, a, aHash, b);
                }
            }
        }

        private void CheckEquals(int i, int j, T a, int aHash, T b)
        {
            int bHash = b.GetHashCode();
            if (!b.Equals(a)) Errln("Equality/Symmetry", i, j);
            if (aHash != bHash) Errln("Equality/Hash", i, j);
            if (!ReferenceEquals(a, b) && !HasSameBehavior(a, b))
            {
                Errln("Equality/Equivalence", i, j);
            }
        }

        private void Errln(string title, int i, int j)
        {
            if (j < 0) Errln("Clone/" + title + "invariant fails: " + i);
            else Errln(title + "invariant fails: " + i + "," + j);
        }

        /**
         * Must be overridden to check whether a and be behave the same
         */
        protected abstract bool HasSameBehavior(T a, T b);

        /**
         * This method will be called multiple times until false is returned.
         * The results should be a mixture of different objects of the same
         * type: some equal and most not equal.
         * The subclasser controls how many are produced (recommend about 
         * 100, based on the size of the objects and how costly they are
         * to run this test on. The running time grows with the square of the
         * count.
         * NOTE: this method will only be called if the objects test as equal.
         */
        protected abstract bool AddTestObject(IList<T> c);
        /**
         * Override if the tested objects are mutable.
         * <br>Since Java doesn't tell us, we need a function to tell if so.
         * The default is true, so must be overridden if not.
         */
        protected virtual bool IsMutable(T a)
        {
            return true;
        }
        /**
         * Override if the tested objects can be cloned.
         */
        protected virtual bool CanClone(T a)
        {
            return true;
        }
        /**
         * Produce a clone of the object. Tries two methods
         * (a) clone
         * (b) constructor
         * Must be overridden if _canClone returns true and
         * the above methods don't work.
         * @param a
         * @return clone
         */
        protected virtual T Clone(T a)
        {
            Type aClass = a.GetType();

            // String is a special case, since in .NET Standard 1.x 
            // there is no Clone method and Clone only returns the
            // same instance anyway
            if (typeof(string).Equals(aClass))
            {
                return a;
            }

            try
            {
                MethodInfo cloner = aClass.GetMethod("Clone");
                if (cloner != null)
                {
                    return (T)cloner.Invoke(a, new object[0]);
                }
            }
            catch (MissingMethodException)
            {
                // Ignore
            }

            ConstructorInfo constructor = aClass.GetConstructor(new Type[] { aClass });
            return (T)constructor.Invoke(new object[] { a });
        }

        /* Utilities */
        public static bool VerifySetsIdentical(AbstractTestLog here, UnicodeSet set1, UnicodeSet set2)
        {
            if (set1.Equals(set2)) return true;
            TestFmwk.Errln("Sets differ:");
            TestFmwk.Errln("UnicodeMap - HashMap");
            TestFmwk.Errln(new UnicodeSet(set1).RemoveAll(set2).ToPattern(true));
            TestFmwk.Errln("HashMap - UnicodeMap");
            TestFmwk.Errln(new UnicodeSet(set2).RemoveAll(set1).ToPattern(true));
            return false;
        }

        public static bool VerifySetsIdentical(AbstractTestLog here, ISet<T> values1, ISet<T> values2)
        {
            if (CollectionUtil.Equals(values1, values2)) return true;
            ISet<T> temp;
            TestFmwk.Errln("Values differ:");
            TestFmwk.Errln("UnicodeMap - HashMap");
            temp = new SortedSet<T>(values1, GenericComparer.NaturalComparer<T>());
            temp.ExceptWith(values2);
            TestFmwk.Errln(Show(temp));
            TestFmwk.Errln("HashMap - UnicodeMap");
            temp = new SortedSet<T>(values2, GenericComparer.NaturalComparer<T>());
            temp.ExceptWith(values1);
            TestFmwk.Errln(Show(temp));
            return false;
        }

        public static string Show<TKey, TValue>(IDictionary<TKey, TValue> m)
        {
            StringBuilder buffer = new StringBuilder();
            foreach (var key in m.Keys)
            {
                buffer.Append(key + "=>" + m.Get(key) + "\r\n");
            }
            return buffer.ToString();
        }

        public static UnicodeSet GetSet(IDictionary<Integer, T> m, T value)
        {
            UnicodeSet result = new UnicodeSet();
            foreach (var key in m.Keys)
            {
                T val = m.Get(key);
                if (!val.Equals(value)) continue;
                result.Add(key.Value);
            }
            return result;
        }

        public static string Show(ICollection<T> c)
        {
            StringBuilder buffer = new StringBuilder();
            foreach (var item in c)
            {
                buffer.Append(item + "\r\n");
            }
            return buffer.ToString();
        }
    }
}
