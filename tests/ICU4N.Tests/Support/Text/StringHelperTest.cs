using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU4N.Text
{

    public class StringHelperTest
    {
        private const string a = "ayz";
        private const string b = "byz";
        private const string c = "cyz";
        private const string d = "dyz";
        private const string e = "eyz";

        private const string s = "ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss";
        private const string Empty = "";


#if FEATURE_SPAN

        [Test]
        [TestCase(a + b, a, b)]
        [TestCase(a + c, a, c)]
        [TestCase(d + e, d, e)]
        [TestCase(b + s, b, s)]
        [TestCase(Empty, Empty, Empty)]
        public void TestConcat_ReadOnlySpan_x2(string expected, string str0, string str1)
        {
            Assert.AreEqual(expected, StringHelper.Concat(str0.AsSpan(), str1.AsSpan()));
        }

        [Test]
        [TestCase(a + b + c, a, b, c)]
        [TestCase(a + c + e, a, c, e)]
        [TestCase(d + e + s, d, e, s)]
        [TestCase(b + d + s, b, d, s)]
        [TestCase(Empty, Empty, Empty, Empty)]
        public void TestConcat_ReadOnlySpan_x3(string expected, string str0, string str1, string str2)
        {
            Assert.AreEqual(expected, StringHelper.Concat(str0.AsSpan(), str1.AsSpan(), str2.AsSpan()));
        }

        [Test]
        [TestCase(a + b + c + d, a, b, c, d)]
        [TestCase(a + c + e + b, a, c, e, b)]
        [TestCase(d + e + s + b, d, e, s, b)]
        [TestCase(b + d + s + c, b, d, s, c)]
        [TestCase(Empty, Empty, Empty, Empty, Empty)]
        public void TestConcat_ReadOnlySpan_x4(string expected, string str0, string str1, string str2, string str3)
        {
            Assert.AreEqual(expected, StringHelper.Concat(str0.AsSpan(), str1.AsSpan(), str2.AsSpan(), str3.AsSpan()));
        }

        [Test]
        [TestCase(a + b + c + d + e, a, b, c, d, e)]
        [TestCase(a + c + e + b + c, a, c, e, b, c)]
        [TestCase(d + e + s + b + d, d, e, s, b, d)]
        [TestCase(b + d + s + c + a, b, d, s, c, a)]
        [TestCase(Empty, Empty, Empty, Empty, Empty, Empty)]
        public void TestConcat_ReadOnlySpan_x5(string expected, string str0, string str1, string str2, string str3, string str4)
        {
            Assert.AreEqual(expected, StringHelper.Concat(str0.AsSpan(), str1.AsSpan(), str2.AsSpan(), str3.AsSpan(), str4.AsSpan()));
        }

        [Test]
        [TestCase(a + b + c + d + e + a, a, b, c, d, e, a)]
        [TestCase(a + c + e + b + c + b, a, c, e, b, c, b)]
        [TestCase(d + e + s + b + d + c, d, e, s, b, d, c)]
        [TestCase(b + d + s + c + a + d, b, d, s, c, a, d)]
        [TestCase(Empty, Empty, Empty, Empty, Empty, Empty, Empty)]
        public void TestConcat_ReadOnlySpan_x6(string expected, string str0, string str1, string str2, string str3, string str4, string str5)
        {
            Assert.AreEqual(expected, StringHelper.Concat(str0.AsSpan(), str1.AsSpan(), str2.AsSpan(), str3.AsSpan(), str4.AsSpan(), str5.AsSpan()));
        }
#endif

    }
}
