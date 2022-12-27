// 
//  Copyright 2009-2017  Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using ICU4N.Support.Numerics.BigMath;
using System;

namespace ICU4N.Numerics.BigMath
{
    internal static class BigMath // ICU4N TODO: Clean up and make public
    {


        

        

        
        ////public static BigDecimal Pow(BigDecimal number, int exp)
        ////{
        ////    return BigDecimalMath.Pow(number, exp);
        ////}

        
        ////public static BigDecimal Pow(BigDecimal number, int exp, MathContext context)
        ////{
        ////    return BigDecimalMath.Pow(number, exp, context);
        ////}

        

        

        
        /////**
        //// * Returns a new {@code BigDecimal} whose value is {@code this}, rounded
        //// * according to the passed context {@code mc}.
        //// * <p>
        //// * If {@code mc.precision = 0}, then no rounding is performed.
        //// * <p>
        //// * If {@code mc.precision > 0} and {@code mc.roundingMode == UNNECESSARY},
        //// * then an {@code ArithmeticException} is thrown if the result cannot be
        //// * represented exactly within the given precision.
        //// *
        //// * @param mc
        //// *            rounding mode and precision for the result of this operation.
        //// * @return {@code this} rounded according to the passed context.
        //// * @throws ArithmeticException
        //// *             if {@code mc.precision > 0} and {@code mc.roundingMode ==
        //// *             UNNECESSARY} and this cannot be represented within the given
        //// *             precision.
        //// */
        ////public static BigDecimal Round(BigDecimal number, MathContext mc)
        ////{
        ////    var thisBD = new BigDecimal(number.UnscaledValue, number.Scale);

        ////    thisBD.InplaceRound(mc);
        ////    return thisBD;
        ////}






        



        


        /////**
        ////* Returns a new {@code BigDecimal} instance with the specified scale.
        ////* <p>
        ////* If the new scale is greater than the old scale, then additional zeros are
        ////* added to the unscaled value. In this case no rounding is necessary.
        ////* <p>
        ////* If the new scale is smaller than the old scale, then trailing digits are
        ////* removed. If these trailing digits are not zero, then the remaining
        ////* unscaled value has to be rounded. For this rounding operation the
        ////* specified rounding mode is used.
        ////*
        ////* @param newScale
        ////*            scale of the result returned.
        ////* @param roundingMode
        ////*            rounding mode to be used to round the result.
        ////* @return a new {@code BigDecimal} instance with the specified scale.
        ////* @throws NullPointerException
        ////*             if {@code roundingMode == null}.
        ////* @throws ArithmeticException
        ////*             if {@code roundingMode == ROUND_UNNECESSARY} and rounding is
        ////*             necessary according to the given scale.
        ////*/
        ////public static BigDecimal Scale(BigDecimal number, int newScale, RoundingMode roundingMode)
        ////{
        ////    if (!Enum.IsDefined(typeof(RoundingMode), roundingMode))
        ////        throw new ArgumentException();

        ////    return BigDecimalMath.Scale(number, newScale, roundingMode);
        ////}














    }
}