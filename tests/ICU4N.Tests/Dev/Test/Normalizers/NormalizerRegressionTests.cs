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

        // Regression: NormalizeSecondAndAppend aliased a ValueStringBuilder's ArrayPool rental
        // between the outer caller and ReorderingBuffer via a struct-copy in a removed constructor.
        // On Grow(), the rented char[] was returned to the pool while still live, causing two
        // threads to share the same buffer and corrupt each other's output. The input below forces
        // Grow() via NFKC_CF ligature expansion (U+FB03 "ffi" → 3 chars); prior to the fix this
        // reproduced mismatches at ~20%+ rates under contention.
        [Test]
        public void TestNormalizeSecondAndAppendConcurrency()
        {
            // Load nfkc_cf data from an embedded copy in the test assembly so the test doesn't
            // depend on the ICU4N.resources satellite (which requires tooling unavailable on some
            // dev machines — e.g. `al` assembly linker on non-Windows).
            Normalizer2 nfkcCf;
            using (var data = typeof(NormalizerRegressionTests).Assembly
                .GetManifestResourceStream("ICU4N.Dev.Test.Normalizers.nfkc_cf.nrm"))
            {
                Assert.NotNull(data, "Embedded nfkc_cf.nrm resource not found.");
                nfkcCf = Normalizer2.GetInstance(data, "nfkc_cf", Normalizer2Mode.Compose);
            }

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

            // Compute expected output single-threaded.
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
