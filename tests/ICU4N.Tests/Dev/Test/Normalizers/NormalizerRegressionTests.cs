using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICU4N.Dev.Test.Normalizers
{
    public class NormalizerRegressionTests : TestFmwk
    {
        [Test]
        public void TestJB4472()
        {
            // submitter's test case
            String tamil = "\u0b87\u0ba8\u0bcd\u0ba4\u0bbf\u0baf\u0bbe";
            Logln("Normalized: " + Normalizer.IsNormalized(tamil, NormalizerMode.NFC, 0));

            // markus's test case
            // the combining cedilla can't be applied to 'b', so this is in normalized form.
            // but the isNormalized test identifies the cedilla as a 'maybe' and so tries
            // to normalize the relevant substring ("b\u0327")and compare the result to the
            // original.  the original code was passing in the start and length of the
            // substring (3, 5-3 = 2) but the called code was expecting start and limit.
            // it subtracted the start again to get what it thought was the length, but
            // ended up with -1.  the loop was incrementing an index from 0 and testing
            // against length, but 0 was never == -1 before it walked off the array end.

            // a workaround in lieu of this patch is to catch the exception and always
            // normalize.

            // this should return true, since the string is normalized (and it should
            // not throw an exception!)
            string sample = "aaab\u0327";
            Logln("Normalized: " + Normalizer.IsNormalized(sample, NormalizerMode.NFC, 0));

            // this should return false, since the string is _not_ normalized (and it should
            // not throw an exception!)
            string sample2 = "aaac\u0327";
            Logln("Normalized: " + Normalizer.IsNormalized(sample2, NormalizerMode.NFC, 0));
        }

        // Regression: prior to the fix, ReorderingBuffer's inner ValueStringBuilder could be
        // struct-copied from a caller's ref parameter, duplicating ownership of the same
        // ArrayPool<char> rental across two instances. Under concurrency, Grow()/Dispose() on
        // one side would return a buffer still live on the other, letting two threads rent the
        // same array and corrupt each other's output. Loads the `testnorm` custom normalizer
        // (already checked into the test data) and drives NormalizeSecondAndAppend with input
        // that exceeds the stack-buffer threshold AND forces Grow() via a composition that
        // expands one char into more than one.
        [Test]
        public void TestNormalizeSecondAndAppendConcurrency()
        {
            // Use the built-in NFKC_CF normalizer — it has aggressive 1→multiple expansions
            // (e.g. U+FB03 "ffi" → 3 chars) that reliably outgrow the ArrayPool rental the
            // inner ValueStringBuilder starts with, which is what exercises the aliasing bug.
            // Normalizers built from testnorm.nrm only expand ~17%, below pool-bucket rounding
            // (≥2x), so they cannot reliably force Grow().
            Normalizer2 nfkcCf = Normalizer2.GetInstance(null, "nfkc_cf", Normalizer2Mode.Compose);

            var rng = new Random(42);
            var sb = new StringBuilder(2000);
            for (int i = 0; i < 2000; i++)
            {
                switch (rng.Next(4))
                {
                    case 0: sb.Append('\uFB03'); break; // ffi ligature (1 -> 3), forces grow
                    case 1: sb.Append('\uFB01'); break; // fi ligature (1 -> 2)
                    case 2: sb.Append((char)('A' + rng.Next(26))); break;
                    case 3: sb.Append((char)(0x0300 + rng.Next(0x20))); break; // combining mark
                }
            }
            string input = sb.ToString();

            // Single-threaded expected.
            var expectedSb = new StringBuilder();
            nfkcCf.NormalizeSecondAndAppend(expectedSb, input.AsSpan());
            string expected = expectedSb.ToString();

            const int threads = 8;
            const int iterationsPerThread = 2000;
            int mismatches = 0;
            var badLengths = new ConcurrentDictionary<int, int>();

            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = threads }, t =>
            {
                for (int i = 0; i < iterationsPerThread; i++)
                {
                    var dst = new StringBuilder();
                    nfkcCf.NormalizeSecondAndAppend(dst, input.AsSpan());
                    if (!string.Equals(dst.ToString(), expected, StringComparison.Ordinal))
                    {
                        Interlocked.Increment(ref mismatches);
                        badLengths.AddOrUpdate(dst.Length, 1, (_, v) => v + 1);
                    }
                }
            });

            if (mismatches != 0)
            {
                var sample = string.Join(", ", badLengths.OrderByDescending(kv => kv.Value)
                    .Take(5).Select(kv => $"len {kv.Key}×{kv.Value}"));
                Assert.Fail($"NormalizeSecondAndAppend produced {mismatches} mismatches " +
                    $"across {threads * iterationsPerThread} concurrent calls (expected len {expected.Length}; {sample}).");
            }
        }
    }
}
