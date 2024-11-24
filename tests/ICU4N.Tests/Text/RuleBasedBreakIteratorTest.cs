using System;
using System.Text;
using System.Threading.Tasks;
using ICU4N.Globalization;
using NUnit.Framework;

namespace ICU4N.Text;

public class RuleBasedBreakIteratorTest
{
    [Test]
    public void HeavyParallelLoad_WithRandomStrings_ShouldNotThrow_Issue95()
    {
        // NOTE: These failing strings are just some sampled from the Lucene.NET project's
        // TestUtil.RandomAnalysisString method that have been known to cause this to fail.
        // Hex-encoding them here to avoid any encoding/display issues.
        var failingStrings = new[]
        {
            Encoding.UTF8.GetString(HexStringToByteArray("D2BCDFAAED96B3E18F86E28BAFE29298EFA1BBE9B2AEE7BEAD76")),
            Encoding.UTF8.GetString(HexStringToByteArray("F28DB9BC2CEB8C96F1B18880CAB9E59BB5E889ADDC8017")),
            Encoding.UTF8.GetString(HexStringToByteArray("D1AD13F0A5A29DE8B794CA80")),
            Encoding.UTF8.GetString(HexStringToByteArray("D0931DEFA897D687EE9D8FE68890E3B591F28A8888E7AFADD7B3C88E05")),
            Encoding.UTF8.GetString(HexStringToByteArray("EE8F87D78CEE8187F09EA3BC6DD896EFB98FE7B298E3A5A7EFB7AAEF9CBE")),
        };

        var cjkBreakIterator = BreakIterator.GetWordInstance(UCultureInfo.InvariantCulture);
        var random = new Random();

        Parallel.For(0, 100000, _ =>
        {
            var text = failingStrings[random.Next(failingStrings.Length)];
            var rbbi = (RuleBasedBreakIterator)cjkBreakIterator.Clone();
            rbbi.SetText(text);
            rbbi.First();
            int end = rbbi.Next();
            while (end != BreakIterator.Done)
            {
                end = rbbi.Next();
            }
        });
    }

    private static byte[] HexStringToByteArray(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Input string cannot be null or empty.");

        if (hex.Length % 2 != 0)
            throw new ArgumentException("Hex string must have an even length.");

        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }
}
