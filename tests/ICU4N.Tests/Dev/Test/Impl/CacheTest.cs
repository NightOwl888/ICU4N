using ICU4N.Impl;
using NUnit.Framework;
using System;

namespace ICU4N.Dev.Test.Impl
{
    public class CacheTest : TestFmwk
    {
        public CacheTest() { }

        /** Code coverage for CacheValue. */
        [Test]
        public void TestNullCacheValue()
        {
            CacheValue<object> nv = CacheValue<object>.GetInstance(null);
            assertTrue("null CacheValue isNull()", nv.IsNull);
            assertTrue("null CacheValue get()==null", nv.Get() == null);
            // ICU4N: ResetIfCleared/FutureInstancesWillBeStrong factored out, as we are always strong
            //assertTrue("null CacheValue reset==null", nv.ResetIfCleared(null) == null);
            //try
            //{
            //    object v = nv.ResetIfCleared(this);
            //    fail("null CacheValue reset(not null) should throw an Exception, returned " +
            //            v + " instead");
            //}
            //catch (Exception expected)
            //{
            //}
        }

        /** Code coverage for CacheValue. */
        [Test]
        public void TestStrongCacheValue()
        {
            // ICU4N: ResetIfCleared/FutureInstancesWillBeStrong factored out, as we are always strong
            //bool wasStrong = CacheValue<object>.FutureInstancesWillBeStrong;
            //CacheValue<object>.Strength = CacheValueStrength.Strong;
            //assertTrue("setStrength(STRONG).futureInstancesWillBeStrong()",
            //        CacheValue<object>.FutureInstancesWillBeStrong);
            CacheValue<Object> sv = CacheValue<Object>.GetInstance(this);
            assertFalse("strong CacheValue not isNull()", sv.IsNull);
            assertTrue("strong CacheValue get()==same", sv.Get() == this);
            //// A strong CacheValue never changes value.
            //// The implementation does not check that the new value is equal to the old one,
            //// or even of equal type, so it does not matter which new value we pass in.
            //assertTrue("strong CacheValue reset==same", sv.ResetIfCleared("") == this);
            //if (!wasStrong)
            //{
            //    CacheValue<object>.Strength = CacheValueStrength.Soft;
            //}
        }

        // ICU4N: ResetIfCleared/FutureInstancesWillBeStrong factored out, as we are always strong
        ///** Code coverage for CacheValue. */
        //[Test]
        //public void TestSoftCacheValue()
        //{
        //    bool wasStrong = CacheValue<object>.FutureInstancesWillBeStrong;
        //    CacheValue<object>.Strength = CacheValueStrength.Soft;
        //    assertFalse("setStrength(SOFT).futureInstancesWillBeStrong()",
        //            CacheValue<object>.FutureInstancesWillBeStrong);
        //    CacheValue<Object> sv = CacheValue<object>.GetInstance(this);
        //    assertFalse("soft CacheValue not isNull()", sv.IsNull);
        //    Object v = sv.Get();
        //    assertTrue("soft CacheValue get()==same or null", v == this || v == null);
        //    assertTrue("soft CacheValue reset==same", sv.ResetIfCleared(this) == this);
        //    if (wasStrong)
        //    {
        //        CacheValue<object>.Strength = CacheValueStrength.Strong;
        //    }
        //}
    }
}
