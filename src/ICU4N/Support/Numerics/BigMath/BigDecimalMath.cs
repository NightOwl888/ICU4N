using ICU4N.Support.Numerics.BigMath;
using System;

namespace ICU4N.Numerics.BigMath
{
    static class BigDecimalMath
    {


        

        








        

        




       





        //public static BigDecimal Add(BigDecimal value, BigDecimal augend)
        //{
        //    int diffScale = value.Scale - augend.Scale;
        //    // Fast return when some operand is zero
        //    if (value.IsZero)
        //    {
        //        if (diffScale <= 0)
        //            return augend;
        //        if (augend.IsZero)
        //            return value;
        //    }
        //    else if (augend.IsZero)
        //    {
        //        if (diffScale >= 0)
        //            return value;
        //    }
        //    // Let be:  this = [u1,s1]  and  augend = [u2,s2]
        //    if (diffScale == 0)
        //    {
        //        // case s1 == s2: [u1 + u2 , s1]
        //        if (System.Math.Max(value.BitLength, augend.BitLength) + 1 < 64)
        //        {
        //            return BigDecimal.Create(value.SmallValue + augend.SmallValue, value.Scale);
        //        }
        //        return new BigDecimal(value.UnscaledValue + augend.UnscaledValue, value.Scale);
        //    }
        //    if (diffScale > 0)
        //        // case s1 > s2 : [(u1 + u2) * 10 ^ (s1 - s2) , s1]
        //        return AddAndMult10(value, augend, diffScale);

        //    // case s2 > s1 : [(u2 + u1) * 10 ^ (s2 - s1) , s2]
        //    return AddAndMult10(augend, value, -diffScale);
        //}

        //private static BigDecimal AddAndMult10(BigDecimal thisValue, BigDecimal augend, int diffScale)
        //{
        //    if (diffScale < BigDecimal.LongTenPow.Length &&
        //        System.Math.Max(thisValue.BitLength, augend.BitLength + BigDecimal.LongTenPowBitLength[diffScale]) + 1 < 64)
        //    {
        //        return BigDecimal.Create(thisValue.SmallValue + augend.SmallValue * BigDecimal.LongTenPow[diffScale], thisValue.Scale);
        //    }
        //    return new BigDecimal(
        //        thisValue.UnscaledValue + Multiplication.MultiplyByTenPow(augend.UnscaledValue, diffScale),
        //        thisValue.Scale);
        //}

        //public static BigDecimal Add(BigDecimal value, BigDecimal augend, MathContext mc)
        //{
        //    BigDecimal larger; // operand with the largest unscaled value
        //    BigDecimal smaller; // operand with the smallest unscaled value
        //    BigInteger tempBi;
        //    long diffScale = (long)value.Scale - augend.Scale;

        //    // Some operand is zero or the precision is infinity  
        //    if ((augend.IsZero) || (value.IsZero) || (mc.Precision == 0))
        //    {
        //        return BigMath.Round(Add(value, augend), mc);
        //    }
        //    // Cases where there is room for optimizations
        //    if (value.AproxPrecision() < diffScale - 1)
        //    {
        //        larger = augend;
        //        smaller = value;
        //    }
        //    else if (augend.AproxPrecision() < -diffScale - 1)
        //    {
        //        larger = value;
        //        smaller = augend;
        //    }
        //    else
        //    {
        //        // No optimization is done 
        //        return BigMath.Round(Add(value, augend), mc);
        //    }
        //    if (mc.Precision >= larger.AproxPrecision())
        //    {
        //        // No optimization is done
        //        return BigMath.Round(Add(value, augend), mc);
        //    }

        //    // Cases where it's unnecessary to add two numbers with very different scales 
        //    var largerSignum = larger.Sign;
        //    if (largerSignum == smaller.Sign)
        //    {
        //        tempBi = Multiplication.MultiplyByPositiveInt(larger.UnscaledValue, 10) +
        //                 BigInteger.FromInt64(largerSignum);
        //    }
        //    else
        //    {
        //        tempBi = larger.UnscaledValue - BigInteger.FromInt64(largerSignum);
        //        tempBi = Multiplication.MultiplyByPositiveInt(tempBi, 10) +
        //                 BigInteger.FromInt64(largerSignum * 9);
        //    }
        //    // Rounding the improved adding 
        //    larger = new BigDecimal(tempBi, larger.Scale + 1);
        //    return BigMath.Round(larger, mc);
        //}



    }
}
