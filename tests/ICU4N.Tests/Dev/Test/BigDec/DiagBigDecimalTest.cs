﻿using ICU4N.Numerics;
using J2N;
using J2N.Globalization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


/* ------------------------------------------------------------------ */
/* Decimal diagnostic tests mfc */
/* Copyright (c) IBM Corporation 1996-2010. All Rights Reserved. */
/* ------------------------------------------------------------------ */
/* DiagBigDecimal */
/*                                                                    */
/* A class that tests the BigDecimal and MathContext classes. */
/*                                                                    */
/* The tests here are derived from or cover the same paths as: */
/* -- ANSI X3-274 testcases */
/* -- Java JCK testcases */
/* -- NetRexx testcases */
/* -- VM/CMS S/370 REXX implementation testcases [1981+] */
/* -- IBM Vienna Laboratory Rexx compiler testcases [1988+] */
/* -- New testcases */
/*                                                                    */
/* The authoritative sources for how the underlying technology */
/* (arithmetic) should work are: */
/* -- for digits=0 (fixed point): java.math.BigDecimal */
/* -- for digits>0 (floating point): ANSI X3.274-1996 + errata */
/*                                                                    */
/* ------------------------------------------------------------------ */
/* Change list */
/* 1997.09.05 Initial implementation, from DiagRexx [NetRexx tests] */
/* 1998.05.02 0.07 changes (e.g., compareTo) */
/* 1998.06.06 Rounding modes and format additions */
/* 1998.06.25 Rename from DiagDecimal; make stand-alone [add */
/* DiagException as a Minor class] */
/* 1998.06.27 Start adding testcases for DIGITS=0/FORM=PLAIN cases */
/* Reorganize for faster trace compilation */
/* 1998.06.28 new: valueof, scale, movePointX, unscaledValue, etc. */
/* 1998.07.07 Scaled divide */
/* 1998.07.08 setScale */
/* 1998.07.15 new scaffolding (Minor Test class) -- see diagabs */
/* 1998.12.14 add toBigDecimal and BigDecimal(java.math.BigDecimal) */
/* 1999.02.04 number preparation rounds instead of digits+1 trunc */
/* 1999.02.09 format method now only has two signatures */
/* 1999.02.27 no longer use Rexx class or RexxIO class */
/* 1999.03.05 add MathContext tests */
/* 1999.03.05 update for 0.96 [no null settings, etc.] */
/* drop sundry constructors; no blanks; char[] gets ints */
/* drop sundry converters, add Exact converters */
/* 1999.05.27 additional tests for scaled arithmetic */
/* 1999.06.29 additional tests for exponent overflows */
/* 1999.07.03 add 'continue' option */
/* 1999.07.10 additional tests for scaled arithmetic */
/* 1999.07.18 randomly-generated tests added for base operators */
/* 1999.10.28 weird intValueExact bad cases */
/* 1999.12.21 multiplication fast path failure and edge cases */
/* 2000.01.01 copyright update */
/* 2000.03.26 cosmetic updates; add extra format() testcases */
/* 2000.03.27 1.00 move to com.ibm.icu.math package; open source release; */
/* change to javadoc comments */
/* ------------------------------------------------------------------ */

// note BINARY for conversions checking


namespace ICU4N.Dev.Test.BigDec
{
    /// <summary>
    /// The <code>DiagBigDecimal</code> class forms a standalone test suite for the
    /// <code>BigDecimal</code> and
    /// <code>MathContext</code> classes (or, by changing the
    /// <code>package</code> statement, other classes of the same names and
    /// definition in other packages). It may also be used as a constructed object to
    /// embed the tests in an external test harness.
    /// <para/>
    /// The tests are collected into <i>groups</i>, each corresponding to a tested
    /// method or a more general grouping. By default, when run from the static
    /// {@link #main(string[])} method, the run will end if any test fails
    /// in a group. The <code>continue</code> argument may be specified to force
    /// the tests to run to completion.
    /// </summary>
    /// <seealso cref="BigDecimal"/>
    /// <seealso cref="MathContext"/>
    /// <version>1.00 2000.03.27</version>
    /// <author>Mike Cowlishaw</author>
    public class DiagBigDecimalTest : TestFmwk
    {
        private static readonly BigDecimal zero = BigDecimal.Zero;
        private static readonly BigDecimal one = BigDecimal.One;
        private static readonly BigDecimal two = new BigDecimal(2);
        private static readonly BigDecimal ten = BigDecimal.Ten;
        private static readonly BigDecimal tenlong = new BigDecimal((long)1234554321); // 10-digiter

        /* Some context objects -- [some of these are checked later] */
        private static readonly MathContext mcdef = MathContext.Default;
        private static readonly MathContext mc3 = new MathContext(3);
        private static readonly MathContext mc6 = new MathContext(6);
        private static readonly MathContext mc9 = new MathContext(9);
        private static readonly MathContext mc50 = new MathContext(50);
        private static readonly MathContext mcs = new MathContext(9, MathContext.Scientific);
        private static readonly MathContext mce = new MathContext(9, MathContext.Engineering);
        private static readonly MathContext mcld = new MathContext(9, MathContext.Scientific, true); // lost digits
        private static readonly MathContext mcld0 = new MathContext(0, MathContext.Scientific, true); // lost digits, digits=0
        private static readonly MathContext mcfd = new MathContext(0, MathContext.Plain); // fixed decimal style

        /* boundary primitive values */
        private const sbyte bmin = -128;
        private const sbyte bmax = 127;
        private const sbyte bzer = 0;
        private const sbyte bneg = -1;
        private const sbyte bpos = 1;
        private const int imin = -2147483648;
        private const int imax = 2147483647;
        private const int izer = 0;
        private const int ineg = -1;
        private const int ipos = 1;
        private const long lmin = -9223372036854775808L;
        private const long lmax = 9223372036854775807L;
        private const long lzer = 0;
        private const long lneg = -1;
        private const long lpos = 1;
        private const short smin = -32768;
        private const short smax = 32767;
        private const short szer = (short)0;
        private const short sneg = (short)(-1);
        private const short spos = (short)1;

        /**
         * Constructs a <code>DiagBigDecimal</code> test suite.
         * <para/>
         * Invoke its {@link #diagrun} method to run the tests.
         */

        //public DiagBigDecimalTest()
        //{
        //    super();
        //}

        //const bool isJDK15OrLater =
        //    TestUtil.getJavaVendor() == JavaVendor.Android ||
        //    TestUtil.getJavaVersion() >= 5;


        /*--------------------------------------------------------------------*/
        /* Diagnostic group methods */
        /*--------------------------------------------------------------------*/

        /** Test constructors (and {@link #toString()} for equalities). */
        [Test]
        public void diagconstructors()
        {
            bool flag = false;
            string num;
            BigInteger bip;
            BigInteger biz;
            BigInteger bin;
            BigDecimal bda;
            BigDecimal bdb;
            BigDecimal bmc;
            BigDecimal bmd;
            BigDecimal bme;
            Exception e = null;
            char[] ca;
            double dzer;
            double dpos;
            double dneg;
            double dpos5;
            double dneg5;
            //double dmin;
            double dmax;
            double d;
            string[] badstrings;
            //int i = 0;

            // constants [statically-called constructors]
            TestFmwk.assertTrue("con001", (BigDecimal.Zero.ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("con002", (BigDecimal.One.ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("con003", (BigDecimal.Ten.ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("con004", (BigDecimal.Zero.ToInt32Exact()) == 0);
            TestFmwk.assertTrue("con005", (BigDecimal.One.ToInt32Exact()) == 1);
            TestFmwk.assertTrue("con006", (BigDecimal.Ten.ToInt32Exact()) == 10);

            // ICU4N: Made BigDecimal(string) constructor into BigDecimal.Parse(string)
//#if FEATURE_IKVM
//            // [java.math.] BigDecimal
//            TestFmwk.assertTrue("cbd001", ((new BigDecimal(new java.math.BigDecimal("0").ToString())).ToString(CultureInfo.InvariantCulture)).Equals("0"));
//            TestFmwk.assertTrue("cbd002", ((new BigDecimal(new java.math.BigDecimal("1").ToString())).ToString(CultureInfo.InvariantCulture)).Equals("1"));
//            TestFmwk.assertTrue("cbd003", ((new BigDecimal(new java.math.BigDecimal("10").ToString())).ToString(CultureInfo.InvariantCulture)).Equals("10"));
//            TestFmwk.assertTrue("cbd004", ((new BigDecimal(new java.math.BigDecimal("1000").ToString())).ToString(CultureInfo.InvariantCulture)).Equals("1000"));
//            TestFmwk.assertTrue("cbd005", ((new BigDecimal(new java.math.BigDecimal("10.0").ToString())).ToString(CultureInfo.InvariantCulture)).Equals("10.0"));
//            TestFmwk.assertTrue("cbd006", ((new BigDecimal(new java.math.BigDecimal("10.1").ToString())).ToString(CultureInfo.InvariantCulture)).Equals("10.1"));
//            TestFmwk.assertTrue("cbd007", ((new BigDecimal(new java.math.BigDecimal("-1.1").ToString())).ToString(CultureInfo.InvariantCulture)).Equals("-1.1"));
//            TestFmwk.assertTrue("cbd008", ((new BigDecimal(new java.math.BigDecimal("-9.0").ToString())).ToString(CultureInfo.InvariantCulture)).Equals("-9.0"));
//            TestFmwk.assertTrue("cbd009", ((new BigDecimal(new java.math.BigDecimal("0.9").ToString())).ToString(CultureInfo.InvariantCulture)).Equals("0.9"));

//            num = "123456789.123456789";
//            TestFmwk.assertTrue("cbd010", ((new BigDecimal(new java.math.BigDecimal(num).ToString())).ToString(CultureInfo.InvariantCulture)).Equals(num));
//            num = "123456789.000000000";
//            TestFmwk.assertTrue("cbd011", ((new BigDecimal(new java.math.BigDecimal(num).ToString())).ToString(CultureInfo.InvariantCulture)).Equals(num));
//            num = "123456789000000000";
//            TestFmwk.assertTrue("cbd012", ((new BigDecimal(new java.math.BigDecimal(num).ToString())).ToString(CultureInfo.InvariantCulture)).Equals(num));
//            num = "0.00000123456789";
//            TestFmwk.assertTrue("cbd013", ((new BigDecimal(new java.math.BigDecimal(num).ToString())).ToString(CultureInfo.InvariantCulture)).Equals(num));
//            num = "0.000000123456789";

//            // ignore format change issues with 1.5
//            //if (!isJDK15OrLater)
//            //    TestFmwk.assertTrue("cbd014", ((new BigDecimal(new java.math.BigDecimal(num).ToString())).ToString(CultureInfo.InvariantCulture)).Equals(num));

//            // ICU4N TODO: java.math.BigDecimal constructor overload
//            //try
//            //{
//            //    new BigDecimal((Deveel.Math.BigDecimal)null);
//            //    flag = false;
//            //}
//            //catch (ArgumentNullException e3) {
//            //    flag = true;
//            //}/* checknull */
//            //TestFmwk.assertTrue("cbi015", flag);
//#endif
            // BigInteger
            bip = BigInteger.Parse("987654321987654321987654321", CultureInfo.InvariantCulture); // biggie +ve
            biz = BigInteger.Parse("0", CultureInfo.InvariantCulture); // biggie 0
            bin = BigInteger.Parse("-12345678998765432112345678", CultureInfo.InvariantCulture); // biggie -ve
            TestFmwk.assertTrue("cbi001", ((new BigDecimal(bip)).ToString(CultureInfo.InvariantCulture)).Equals(bip.ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("cbi002", ((new BigDecimal(biz)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("cbi003", ((new BigDecimal(bin)).ToString(CultureInfo.InvariantCulture)).Equals(bin.ToString(CultureInfo.InvariantCulture)));

            // ICU4N: Struct cannot be null
            //try
            //{
            //    new BigDecimal((BigInteger)null);
            //    flag = false;
            //}
            //catch (ArgumentNullException $4) {
            //    flag = true;
            //}/* checknull */
            //TestFmwk.assertTrue("cbi004", flag);

            // BigInteger with scale
            bip = BigInteger.Parse("123456789", CultureInfo.InvariantCulture); // bigish
            bda = new BigDecimal(bip);
            bdb = new BigDecimal(bip, 5);
            bmc = new BigDecimal(bip, 15);
            TestFmwk.assertTrue("cbs001", (bda.ToString(CultureInfo.InvariantCulture)).Equals("123456789"));
            TestFmwk.assertTrue("cbs002", (bdb.ToString(CultureInfo.InvariantCulture)).Equals("1234.56789"));
            TestFmwk.assertTrue("cbs003", (bmc.ToString(CultureInfo.InvariantCulture)).Equals("0.000000123456789"));
            bip = BigInteger.Parse("123456789123456789123456789", CultureInfo.InvariantCulture); // biggie
            bda = new BigDecimal(bip);
            bdb = new BigDecimal(bip, 7);
            bmc = new BigDecimal(bip, 13);
            bmd = new BigDecimal(bip, 19);
            bme = new BigDecimal(bip, 29);
            TestFmwk.assertTrue("cbs011", (bda.ToString(CultureInfo.InvariantCulture)).Equals("123456789123456789123456789"));
            TestFmwk.assertTrue("cbs012", (bdb.ToString(CultureInfo.InvariantCulture)).Equals("12345678912345678912.3456789"));
            TestFmwk.assertTrue("cbs013", (bmc.ToString(CultureInfo.InvariantCulture)).Equals("12345678912345.6789123456789"));
            TestFmwk.assertTrue("cbs014", (bmd.ToString(CultureInfo.InvariantCulture)).Equals("12345678.9123456789123456789"));
            TestFmwk.assertTrue("cbs015", (bme.ToString(CultureInfo.InvariantCulture)).Equals("0.00123456789123456789123456789"));
            // ICU4N: Struct cannot be null
            //try
            //{
            //    new BigDecimal((BigInteger)null, 1);
            //    flag = false;
            //}
            //catch (ArgumentNullException $5) {
            //    flag = true;
            //}/* checknull */
            //TestFmwk.assertTrue("cbs004", flag);
            try
            {
                new BigDecimal(bip, -8);
                flag = false;
            }
            catch (ArgumentOutOfRangeException e6)
            {
                e = e6;
                flag = (e.Message).StartsWith("Negative scale: -8", StringComparison.Ordinal);
            }/* checkscale */
            TestFmwk.assertTrue("cbs005", flag);

            // ICU4N: Made BigDecimal(string) constructor into BigDecimal.Parse(char[])

            //// char[]
            //// We just test it's there
            //// Functionality is tested by BigDecimal(String).
            //ca = ("123.45").ToCharArray();
            //TestFmwk.assertTrue("cca001", ((new BigDecimal(ca)).ToString(CultureInfo.InvariantCulture)).Equals("123.45"));
            //try
            //{
            //    new BigDecimal((char[])null);
            //    flag = false;
            //}
            //catch (ArgumentNullException e7)
            //{
            //    flag = true;
            //}/* checknull */
            //TestFmwk.assertTrue("cca010", flag);

            // ICU4N: Constructor made into BigDecimal.Parse(char[], int, int, NumberStyle, NumberFormatInfo)

            ////// char[],int,int
            ////// We just test it's there, and that offsets work.
            ////// Functionality is tested by BigDecimal(String).
            ////ca = ("123.45").ToCharArray();
            ////TestFmwk.assertTrue("cca101", ((new BigDecimal(ca, 0, 6)).ToString(CultureInfo.InvariantCulture)).Equals("123.45"));
            ////TestFmwk.assertTrue("cca102", ((new BigDecimal(ca, 1, 5)).ToString(CultureInfo.InvariantCulture)).Equals("23.45"));
            ////TestFmwk.assertTrue("cca103", ((new BigDecimal(ca, 2, 4)).ToString(CultureInfo.InvariantCulture)).Equals("3.45"));
            ////TestFmwk.assertTrue("cca104", ((new BigDecimal(ca, 3, 3)).ToString(CultureInfo.InvariantCulture)).Equals("0.45"));
            ////TestFmwk.assertTrue("cca105", ((new BigDecimal(ca, 4, 2)).ToString(CultureInfo.InvariantCulture)).Equals("45"));
            ////TestFmwk.assertTrue("cca106", ((new BigDecimal(ca, 5, 1)).ToString(CultureInfo.InvariantCulture)).Equals("5"));

            ////TestFmwk.assertTrue("cca110", ((new BigDecimal(ca, 0, 1)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            ////TestFmwk.assertTrue("cca111", ((new BigDecimal(ca, 1, 1)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            ////TestFmwk.assertTrue("cca112", ((new BigDecimal(ca, 2, 1)).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            ////TestFmwk.assertTrue("cca113", ((new BigDecimal(ca, 4, 1)).ToString(CultureInfo.InvariantCulture)).Equals("4"));

            ////TestFmwk.assertTrue("cca120", ((new BigDecimal(ca, 0, 2)).ToString(CultureInfo.InvariantCulture)).Equals("12"));
            ////TestFmwk.assertTrue("cca121", ((new BigDecimal(ca, 1, 2)).ToString(CultureInfo.InvariantCulture)).Equals("23"));
            ////TestFmwk.assertTrue("cca122", ((new BigDecimal(ca, 2, 2)).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            ////TestFmwk.assertTrue("cca123", ((new BigDecimal(ca, 3, 2)).ToString(CultureInfo.InvariantCulture)).Equals("0.4"));

            ////TestFmwk.assertTrue("cca130", ((new BigDecimal(ca, 0, 3)).ToString(CultureInfo.InvariantCulture)).Equals("123"));
            ////TestFmwk.assertTrue("cca131", ((new BigDecimal(ca, 1, 3)).ToString(CultureInfo.InvariantCulture)).Equals("23"));
            ////TestFmwk.assertTrue("cca132", ((new BigDecimal(ca, 2, 3)).ToString(CultureInfo.InvariantCulture)).Equals("3.4"));

            ////TestFmwk.assertTrue("cca140", ((new BigDecimal(ca, 0, 4)).ToString(CultureInfo.InvariantCulture)).Equals("123"));
            ////TestFmwk.assertTrue("cca141", ((new BigDecimal(ca, 1, 4)).ToString(CultureInfo.InvariantCulture)).Equals("23.4"));

            ////TestFmwk.assertTrue("cca150", ((new BigDecimal(ca, 0, 5)).ToString(CultureInfo.InvariantCulture)).Equals("123.4"));

            ////// a couple of oddies
            ////ca = ("x23.4x").ToCharArray();
            ////TestFmwk.assertTrue("cca160", ((new BigDecimal(ca, 1, 4)).ToString(CultureInfo.InvariantCulture)).Equals("23.4"));
            ////TestFmwk.assertTrue("cca161", ((new BigDecimal(ca, 1, 1)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            ////TestFmwk.assertTrue("cca162", ((new BigDecimal(ca, 4, 1)).ToString(CultureInfo.InvariantCulture)).Equals("4"));

            ////ca = ("0123456789.9876543210").ToCharArray();
            ////TestFmwk.assertTrue("cca163", ((new BigDecimal(ca, 0, 21)).ToString(CultureInfo.InvariantCulture)).Equals("123456789.9876543210"));
            ////TestFmwk.assertTrue("cca164", ((new BigDecimal(ca, 1, 20)).ToString(CultureInfo.InvariantCulture)).Equals("123456789.9876543210"));
            ////TestFmwk.assertTrue("cca165", ((new BigDecimal(ca, 2, 19)).ToString(CultureInfo.InvariantCulture)).Equals("23456789.9876543210"));
            ////TestFmwk.assertTrue("cca166", ((new BigDecimal(ca, 2, 18)).ToString(CultureInfo.InvariantCulture)).Equals("23456789.987654321"));
            ////TestFmwk.assertTrue("cca167", ((new BigDecimal(ca, 2, 17)).ToString(CultureInfo.InvariantCulture)).Equals("23456789.98765432"));
            ////TestFmwk.assertTrue("cca168", ((new BigDecimal(ca, 2, 16)).ToString(CultureInfo.InvariantCulture)).Equals("23456789.9876543"));

            ////try
            ////{
            ////    new BigDecimal((char[])null, 0, 1);
            ////    flag = false;
            ////}
            ////catch (ArgumentNullException e8)
            ////{
            ////    flag = true;
            ////}/* checknull */
            ////TestFmwk.assertTrue("cca200", flag);

            ////try
            ////{
            ////    new BigDecimal("123".ToCharArray(), 0, 0);
            ////    flag = false;
            ////}
            ////catch (FormatException e9) // ICU4N TODO: Handle parse with 0 length
            ////{
            ////    flag = true;
            ////}/* checklen */
            ////TestFmwk.assertTrue("cca201", flag);

            ////try
            ////{
            ////    new BigDecimal("123".ToCharArray(), 2, 4);
            ////    flag = false;
            ////}
            ////catch (ArgumentOutOfRangeException e10)
            ////{ // anything OK
            ////    flag = true;
            ////}/* checkbound */
            ////TestFmwk.assertTrue("cca202", flag);
            ////try
            ////{
            ////    new BigDecimal("123".ToCharArray(), -1, 2);
            ////    flag = false;
            ////}
            ////catch (ArgumentOutOfRangeException e11)
            ////{ // anything OK
            ////    flag = true;
            ////}/* checkbound2 */
            ////TestFmwk.assertTrue("cca203", flag);
            ////try
            ////{
            ////    new BigDecimal("123".ToCharArray(), 1, -2);
            ////    flag = false;
            ////}
            ////catch (ArgumentOutOfRangeException e12)
            ////{ // anything OK
            ////    flag = true;
            ////}/* checkbound3 */
            ////TestFmwk.assertTrue("cca204", flag);

            // double [deprecated]
            // Note that many of these differ from the valueOf(double) results.
            dzer = 0;
            dpos = 1;
            dpos = dpos / (10);
            dneg = -dpos;
            TestFmwk.assertTrue("cdo001", ((new BigDecimal(dneg)).ToString(CultureInfo.InvariantCulture)).Equals("-0.1000000000000000055511151231257827021181583404541015625"));

            TestFmwk.assertTrue("cdo002", ((new BigDecimal(dzer)).ToString(CultureInfo.InvariantCulture)).Equals("0")); // NB, not '0.0'
            TestFmwk.assertTrue("cdo003", ((new BigDecimal(dpos)).ToString(CultureInfo.InvariantCulture)).Equals("0.1000000000000000055511151231257827021181583404541015625"));

            dpos5 = 0.5D;
            dneg5 = -dpos5;
            TestFmwk.assertTrue("cdo004", ((new BigDecimal(dneg5)).ToString(CultureInfo.InvariantCulture)).Equals("-0.5"));
            TestFmwk.assertTrue("cdo005", ((new BigDecimal(dpos5)).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            //dmin = double.Epsilon; // ICU4N: Corrected MIN_VALUE to Epsilon (smallest postive value)
            dmax = double.MaxValue;
            //if (!isJDK15OrLater) // for some reason we format using scientific
            //                     // notation on 1.5 after 30 decimals or so
            //    TestFmwk.assertTrue("cdo006", ((new BigDecimal(dmin)).ToString(CultureInfo.InvariantCulture)).Equals("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000004940656458412465441765687928682213723650598026143247644255856825006755072702087518652998363616359923797965646954457177309266567103559397963987747960107818781263007131903114045278458171678489821036887186360569987307230500063874091535649843873124733972731696151400317153853980741262385655911710266585566867681870395603106249319452715914924553293054565444011274801297099995419319894090804165633245247571478690147267801593552386115501348035264934720193790268107107491703332226844753335720832431936092382893458368060106011506169809753078342277318329247904982524730776375927247874656084778203734469699533647017972677717585125660551199131504891101451037862738167250955837389733598993664809941164205702637090279242767544565229087538682506419718265533447265625"));

            TestFmwk.assertTrue("cdo007", ((new BigDecimal(dmax)).ToString(CultureInfo.InvariantCulture)).Equals("179769313486231570814527423731704356798070567525844996598917476803157260780028538760589558632766878171540458953514382464234321326889464182768467546703537516986049910576551282076245490090389328944075868508455133942304583236903222948165808559332123348274797826204144723168738177180919299881250404026184124858368"));

            // nasties
            d = 9;
            d = d / (10);
            TestFmwk.assertTrue("cdo010", ((new BigDecimal(d)).ToString(CultureInfo.InvariantCulture)).Equals("0.90000000000000002220446049250313080847263336181640625"));

            d = d / (10);
            TestFmwk.assertTrue("cdo011", ((new BigDecimal(d)).ToString(CultureInfo.InvariantCulture)).Equals("0.0899999999999999966693309261245303787291049957275390625"));

            d = d / (10);
            TestFmwk.assertTrue("cdo012", ((new BigDecimal(d)).ToString(CultureInfo.InvariantCulture)).Equals("0.00899999999999999931998839741709161899052560329437255859375"));

            d = d / (10);
            TestFmwk.assertTrue("cdo013", ((new BigDecimal(d)).ToString(CultureInfo.InvariantCulture)).Equals("0.00089999999999999997536692664112933925935067236423492431640625"));

            d = d / (10);
            TestFmwk.assertTrue("cdo014", ((new BigDecimal(d)).ToString(CultureInfo.InvariantCulture)).Equals("0.00008999999999999999211568180168541175589780323207378387451171875"));

            d = d / (10);
            TestFmwk.assertTrue("cdo015", ((new BigDecimal(d)).ToString(CultureInfo.InvariantCulture)).Equals("0.00000899999999999999853394182236510090433512232266366481781005859375"));

            d = d / (10);
            //if (!isJDK15OrLater)
            //    TestFmwk.assertTrue("cdo016", ((new BigDecimal(d)).ToString(CultureInfo.InvariantCulture)).Equals("0.000000899999999999999853394182236510090433512232266366481781005859375"));

            d = d / (10);
            //if (!isJDK15OrLater)
            //    TestFmwk.assertTrue("cdo017", ((new BigDecimal(d)).ToString(CultureInfo.InvariantCulture)).Equals("0.0000000899999999999999853394182236510090433512232266366481781005859375"));

            d = d / (10);
            //if (!isJDK15OrLater)
            //    TestFmwk.assertTrue("cdo018", ((new BigDecimal(d)).ToString(CultureInfo.InvariantCulture)).Equals("0.000000008999999999999997872197332322678764437995369007694534957408905029296875"));

            try
            {
                new BigDecimal(
                        double.PositiveInfinity);
                flag = false;
            }
            catch (OverflowException e13)
            {
                flag = true;
            }/* checkpin */
            TestFmwk.assertTrue("cdo101", flag);
            try
            {
                new BigDecimal(
                        double.NegativeInfinity);
                flag = false;
            }
            catch (OverflowException e14)
            {
                flag = true;
            }/* checknin */
            TestFmwk.assertTrue("cdo102", flag);
            try
            {
                new BigDecimal(double.NaN);
                flag = false;
            }
            catch (OverflowException e15)
            {
                flag = true;
            }/* checknan */
            TestFmwk.assertTrue("cdo103", flag);

            // int
            TestFmwk.assertTrue("cin001", ((new BigDecimal(imin)).ToString(CultureInfo.InvariantCulture)).Equals("-2147483648"));
            TestFmwk.assertTrue("cin002", ((new BigDecimal(imax)).ToString(CultureInfo.InvariantCulture)).Equals("2147483647"));
            TestFmwk.assertTrue("cin003", ((new BigDecimal(ineg)).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("cin004", ((new BigDecimal(izer)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("cin005", ((new BigDecimal(ipos)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("cin006", ((new BigDecimal(10)).ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("cin007", ((new BigDecimal(9)).ToString(CultureInfo.InvariantCulture)).Equals("9"));
            TestFmwk.assertTrue("cin008", ((new BigDecimal(5)).ToString(CultureInfo.InvariantCulture)).Equals("5"));
            TestFmwk.assertTrue("cin009", ((new BigDecimal(2)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("cin010", ((new BigDecimal(-2)).ToString(CultureInfo.InvariantCulture)).Equals("-2"));
            TestFmwk.assertTrue("cin011", ((new BigDecimal(-5)).ToString(CultureInfo.InvariantCulture)).Equals("-5"));
            TestFmwk.assertTrue("cin012", ((new BigDecimal(-9)).ToString(CultureInfo.InvariantCulture)).Equals("-9"));
            TestFmwk.assertTrue("cin013", ((new BigDecimal(-10)).ToString(CultureInfo.InvariantCulture)).Equals("-10"));
            TestFmwk.assertTrue("cin014", ((new BigDecimal(-11)).ToString(CultureInfo.InvariantCulture)).Equals("-11"));
            TestFmwk.assertTrue("cin015", ((new BigDecimal(-99)).ToString(CultureInfo.InvariantCulture)).Equals("-99"));
            TestFmwk.assertTrue("cin016", ((new BigDecimal(-100)).ToString(CultureInfo.InvariantCulture)).Equals("-100"));
            TestFmwk.assertTrue("cin017", ((new BigDecimal(-999)).ToString(CultureInfo.InvariantCulture)).Equals("-999"));
            TestFmwk.assertTrue("cin018", ((new BigDecimal(-1000)).ToString(CultureInfo.InvariantCulture)).Equals("-1000"));

            TestFmwk.assertTrue("cin019", ((new BigDecimal(11)).ToString(CultureInfo.InvariantCulture)).Equals("11"));
            TestFmwk.assertTrue("cin020", ((new BigDecimal(99)).ToString(CultureInfo.InvariantCulture)).Equals("99"));
            TestFmwk.assertTrue("cin021", ((new BigDecimal(100)).ToString(CultureInfo.InvariantCulture)).Equals("100"));
            TestFmwk.assertTrue("cin022", ((new BigDecimal(999)).ToString(CultureInfo.InvariantCulture)).Equals("999"));
            TestFmwk.assertTrue("cin023", ((new BigDecimal(1000)).ToString(CultureInfo.InvariantCulture)).Equals("1000"));

            // long
            TestFmwk.assertTrue("clo001", ((new BigDecimal(lmin)).ToString(CultureInfo.InvariantCulture)).Equals("-9223372036854775808"));
            TestFmwk.assertTrue("clo002", ((new BigDecimal(lmax)).ToString(CultureInfo.InvariantCulture)).Equals("9223372036854775807"));
            TestFmwk.assertTrue("clo003", ((new BigDecimal(lneg)).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("clo004", ((new BigDecimal(lzer)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("clo005", ((new BigDecimal(lpos)).ToString(CultureInfo.InvariantCulture)).Equals("1"));

            // ICU4N: Constructor made into BigDecimal.Parse(string, NumberStyle, NumberFormatInfo)

            ////// String [many more examples are elsewhere]
            ////// strings without E cannot generate E in result
            ////TestFmwk.assertTrue("cst001", ((new BigDecimal("12")).ToString(CultureInfo.InvariantCulture)).Equals("12"));
            ////TestFmwk.assertTrue("cst002", ((new BigDecimal("-76")).ToString(CultureInfo.InvariantCulture)).Equals("-76"));
            ////TestFmwk.assertTrue("cst003", ((new BigDecimal("12.76")).ToString(CultureInfo.InvariantCulture)).Equals("12.76"));
            ////TestFmwk.assertTrue("cst004", ((new BigDecimal("+12.76")).ToString(CultureInfo.InvariantCulture)).Equals("12.76"));
            ////TestFmwk.assertTrue("cst005", ((new BigDecimal("012.76")).ToString(CultureInfo.InvariantCulture)).Equals("12.76"));
            ////TestFmwk.assertTrue("cst006", ((new BigDecimal("+0.003")).ToString(CultureInfo.InvariantCulture)).Equals("0.003"));
            ////TestFmwk.assertTrue("cst007", ((new BigDecimal("17.")).ToString(CultureInfo.InvariantCulture)).Equals("17"));
            ////TestFmwk.assertTrue("cst008", ((new BigDecimal(".5")).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            ////TestFmwk.assertTrue("cst009", ((new BigDecimal("044")).ToString(CultureInfo.InvariantCulture)).Equals("44"));
            ////TestFmwk.assertTrue("cst010", ((new BigDecimal("0044")).ToString(CultureInfo.InvariantCulture)).Equals("44"));
            ////TestFmwk.assertTrue("cst011", ((new BigDecimal("0.0005")).ToString(CultureInfo.InvariantCulture)).Equals("0.0005"));
            ////TestFmwk.assertTrue("cst012", ((new BigDecimal("00.00005")).ToString(CultureInfo.InvariantCulture)).Equals("0.00005"));
            ////TestFmwk.assertTrue("cst013", ((new BigDecimal("0.000005")).ToString(CultureInfo.InvariantCulture)).Equals("0.000005"));
            ////TestFmwk.assertTrue("cst014", ((new BigDecimal("0.0000005")).ToString(CultureInfo.InvariantCulture)).Equals("0.0000005")); // \NR
            ////TestFmwk.assertTrue("cst015", ((new BigDecimal("0.00000005")).ToString(CultureInfo.InvariantCulture)).Equals("0.00000005")); // \NR
            ////TestFmwk.assertTrue("cst016", ((new BigDecimal("12345678.876543210")).ToString(CultureInfo.InvariantCulture)).Equals("12345678.876543210"));
            ////TestFmwk.assertTrue("cst017", ((new BigDecimal("2345678.876543210")).ToString(CultureInfo.InvariantCulture)).Equals("2345678.876543210"));
            ////TestFmwk.assertTrue("cst018", ((new BigDecimal("345678.876543210")).ToString(CultureInfo.InvariantCulture)).Equals("345678.876543210"));
            ////TestFmwk.assertTrue("cst019", ((new BigDecimal("0345678.87654321")).ToString(CultureInfo.InvariantCulture)).Equals("345678.87654321"));
            ////TestFmwk.assertTrue("cst020", ((new BigDecimal("345678.8765432")).ToString(CultureInfo.InvariantCulture)).Equals("345678.8765432"));
            ////TestFmwk.assertTrue("cst021", ((new BigDecimal("+345678.8765432")).ToString(CultureInfo.InvariantCulture)).Equals("345678.8765432"));
            ////TestFmwk.assertTrue("cst022", ((new BigDecimal("+0345678.8765432")).ToString(CultureInfo.InvariantCulture)).Equals("345678.8765432"));
            ////TestFmwk.assertTrue("cst023", ((new BigDecimal("+00345678.8765432")).ToString(CultureInfo.InvariantCulture)).Equals("345678.8765432"));
            ////TestFmwk.assertTrue("cst024", ((new BigDecimal("-345678.8765432")).ToString(CultureInfo.InvariantCulture)).Equals("-345678.8765432"));
            ////TestFmwk.assertTrue("cst025", ((new BigDecimal("-0345678.8765432")).ToString(CultureInfo.InvariantCulture)).Equals("-345678.8765432"));
            ////TestFmwk.assertTrue("cst026", ((new BigDecimal("-00345678.8765432")).ToString(CultureInfo.InvariantCulture)).Equals("-345678.8765432"));

            ////// exotics --
            ////TestFmwk.assertTrue("cst035", ((new BigDecimal("\u0e57.\u0e50")).ToString(CultureInfo.InvariantCulture)).Equals("7.0"));
            ////TestFmwk.assertTrue("cst036", ((new BigDecimal("\u0b66.\u0b67")).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            ////TestFmwk.assertTrue("cst037", ((new BigDecimal("\u0b66\u0b66")).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            ////TestFmwk.assertTrue("cst038", ((new BigDecimal("\u0b6a\u0b66")).ToString(CultureInfo.InvariantCulture)).Equals("40"));

            ////// strings with E
            ////TestFmwk.assertTrue("cst040", ((new BigDecimal("1E+9")).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            ////TestFmwk.assertTrue("cst041", ((new BigDecimal("1e+09")).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            ////TestFmwk.assertTrue("cst042", ((new BigDecimal("1E+90")).ToString(CultureInfo.InvariantCulture)).Equals("1E+90"));
            ////TestFmwk.assertTrue("cst043", ((new BigDecimal("+1E+009")).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            ////TestFmwk.assertTrue("cst044", ((new BigDecimal("0E+9")).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            ////TestFmwk.assertTrue("cst045", ((new BigDecimal("1E+9")).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            ////TestFmwk.assertTrue("cst046", ((new BigDecimal("1E+09")).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            ////TestFmwk.assertTrue("cst047", ((new BigDecimal("1e+90")).ToString(CultureInfo.InvariantCulture)).Equals("1E+90"));
            ////TestFmwk.assertTrue("cst048", ((new BigDecimal("1E+009")).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            ////TestFmwk.assertTrue("cst049", ((new BigDecimal("0E+9")).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            ////TestFmwk.assertTrue("cst050", ((new BigDecimal("1E9")).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            ////TestFmwk.assertTrue("cst051", ((new BigDecimal("1e09")).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            ////TestFmwk.assertTrue("cst052", ((new BigDecimal("1E90")).ToString(CultureInfo.InvariantCulture)).Equals("1E+90"));
            ////TestFmwk.assertTrue("cst053", ((new BigDecimal("1E009")).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            ////TestFmwk.assertTrue("cst054", ((new BigDecimal("0E9")).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            ////TestFmwk.assertTrue("cst055", ((new BigDecimal("0.000e+0")).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            ////TestFmwk.assertTrue("cst056", ((new BigDecimal("0.000E-1")).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            ////TestFmwk.assertTrue("cst057", ((new BigDecimal("4E+9")).ToString(CultureInfo.InvariantCulture)).Equals("4E+9"));
            ////TestFmwk.assertTrue("cst058", ((new BigDecimal("44E+9")).ToString(CultureInfo.InvariantCulture)).Equals("4.4E+10"));
            ////TestFmwk.assertTrue("cst059", ((new BigDecimal("0.73e-7")).ToString(CultureInfo.InvariantCulture)).Equals("7.3E-8"));
            ////TestFmwk.assertTrue("cst060", ((new BigDecimal("00E+9")).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            ////TestFmwk.assertTrue("cst061", ((new BigDecimal("00E-9")).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            ////TestFmwk.assertTrue("cst062", ((new BigDecimal("10E+9")).ToString(CultureInfo.InvariantCulture)).Equals("1.0E+10"));
            ////TestFmwk.assertTrue("cst063", ((new BigDecimal("10E+09")).ToString(CultureInfo.InvariantCulture)).Equals("1.0E+10"));
            ////TestFmwk.assertTrue("cst064", ((new BigDecimal("10e+90")).ToString(CultureInfo.InvariantCulture)).Equals("1.0E+91"));
            ////TestFmwk.assertTrue("cst065", ((new BigDecimal("10E+009")).ToString(CultureInfo.InvariantCulture)).Equals("1.0E+10"));
            ////TestFmwk.assertTrue("cst066", ((new BigDecimal("100e+9")).ToString(CultureInfo.InvariantCulture)).Equals("1.00E+11"));
            ////TestFmwk.assertTrue("cst067", ((new BigDecimal("100e+09")).ToString(CultureInfo.InvariantCulture)).Equals("1.00E+11"));
            ////TestFmwk.assertTrue("cst068", ((new BigDecimal("100E+90")).ToString(CultureInfo.InvariantCulture)).Equals("1.00E+92"));
            ////TestFmwk.assertTrue("cst069", ((new BigDecimal("100e+009")).ToString(CultureInfo.InvariantCulture)).Equals("1.00E+11"));

            ////TestFmwk.assertTrue("cst070", ((new BigDecimal("1.265")).ToString(CultureInfo.InvariantCulture)).Equals("1.265"));
            ////TestFmwk.assertTrue("cst071", ((new BigDecimal("1.265E-20")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-20"));
            ////TestFmwk.assertTrue("cst072", ((new BigDecimal("1.265E-8")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-8"));
            ////TestFmwk.assertTrue("cst073", ((new BigDecimal("1.265E-4")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-4"));
            ////TestFmwk.assertTrue("cst074", ((new BigDecimal("1.265E-3")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-3"));
            ////TestFmwk.assertTrue("cst075", ((new BigDecimal("1.265E-2")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-2"));
            ////TestFmwk.assertTrue("cst076", ((new BigDecimal("1.265E-1")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-1"));
            ////TestFmwk.assertTrue("cst077", ((new BigDecimal("1.265E-0")).ToString(CultureInfo.InvariantCulture)).Equals("1.265"));
            ////TestFmwk.assertTrue("cst078", ((new BigDecimal("1.265E+1")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+1"));
            ////TestFmwk.assertTrue("cst079", ((new BigDecimal("1.265E+2")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+2"));
            ////TestFmwk.assertTrue("cst080", ((new BigDecimal("1.265E+3")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+3"));
            ////TestFmwk.assertTrue("cst081", ((new BigDecimal("1.265E+4")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+4"));
            ////TestFmwk.assertTrue("cst082", ((new BigDecimal("1.265E+8")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+8"));
            ////TestFmwk.assertTrue("cst083", ((new BigDecimal("1.265E+20")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+20"));

            ////TestFmwk.assertTrue("cst090", ((new BigDecimal("12.65")).ToString(CultureInfo.InvariantCulture)).Equals("12.65"));
            ////TestFmwk.assertTrue("cst091", ((new BigDecimal("12.65E-20")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-19"));
            ////TestFmwk.assertTrue("cst092", ((new BigDecimal("12.65E-8")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-7"));
            ////TestFmwk.assertTrue("cst093", ((new BigDecimal("12.65E-4")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-3"));
            ////TestFmwk.assertTrue("cst094", ((new BigDecimal("12.65E-3")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-2"));
            ////TestFmwk.assertTrue("cst095", ((new BigDecimal("12.65E-2")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-1"));
            ////TestFmwk.assertTrue("cst096", ((new BigDecimal("12.65E-1")).ToString(CultureInfo.InvariantCulture)).Equals("1.265"));
            ////TestFmwk.assertTrue("cst097", ((new BigDecimal("12.65E-0")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+1"));
            ////TestFmwk.assertTrue("cst098", ((new BigDecimal("12.65E+1")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+2"));
            ////TestFmwk.assertTrue("cst099", ((new BigDecimal("12.65E+2")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+3"));
            ////TestFmwk.assertTrue("cst100", ((new BigDecimal("12.65E+3")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+4"));
            ////TestFmwk.assertTrue("cst101", ((new BigDecimal("12.65E+4")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+5"));
            ////TestFmwk.assertTrue("cst102", ((new BigDecimal("12.65E+8")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+9"));
            ////TestFmwk.assertTrue("cst103", ((new BigDecimal("12.65E+20")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+21"));

            ////TestFmwk.assertTrue("cst110", ((new BigDecimal("126.5")).ToString(CultureInfo.InvariantCulture)).Equals("126.5"));
            ////TestFmwk.assertTrue("cst111", ((new BigDecimal("126.5E-20")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-18"));
            ////TestFmwk.assertTrue("cst112", ((new BigDecimal("126.5E-8")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-6"));
            ////TestFmwk.assertTrue("cst113", ((new BigDecimal("126.5E-4")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-2"));
            ////TestFmwk.assertTrue("cst114", ((new BigDecimal("126.5E-3")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-1"));
            ////TestFmwk.assertTrue("cst115", ((new BigDecimal("126.5E-2")).ToString(CultureInfo.InvariantCulture)).Equals("1.265"));
            ////TestFmwk.assertTrue("cst116", ((new BigDecimal("126.5E-1")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+1"));
            ////TestFmwk.assertTrue("cst117", ((new BigDecimal("126.5E-0")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+2"));
            ////TestFmwk.assertTrue("cst118", ((new BigDecimal("126.5E+1")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+3"));
            ////TestFmwk.assertTrue("cst119", ((new BigDecimal("126.5E+2")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+4"));
            ////TestFmwk.assertTrue("cst120", ((new BigDecimal("126.5E+3")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+5"));
            ////TestFmwk.assertTrue("cst121", ((new BigDecimal("126.5E+4")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+6"));
            ////TestFmwk.assertTrue("cst122", ((new BigDecimal("126.5E+8")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+10"));
            ////TestFmwk.assertTrue("cst123", ((new BigDecimal("126.5E+20")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+22"));

            ////TestFmwk.assertTrue("cst130", ((new BigDecimal("1265")).ToString(CultureInfo.InvariantCulture)).Equals("1265"));
            ////TestFmwk.assertTrue("cst131", ((new BigDecimal("1265E-20")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-17"));
            ////TestFmwk.assertTrue("cst132", ((new BigDecimal("1265E-8")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-5"));
            ////TestFmwk.assertTrue("cst133", ((new BigDecimal("1265E-4")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-1"));
            ////TestFmwk.assertTrue("cst134", ((new BigDecimal("1265E-3")).ToString(CultureInfo.InvariantCulture)).Equals("1.265"));
            ////TestFmwk.assertTrue("cst135", ((new BigDecimal("1265E-2")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+1"));
            ////TestFmwk.assertTrue("cst136", ((new BigDecimal("1265E-1")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+2"));
            ////TestFmwk.assertTrue("cst137", ((new BigDecimal("1265E-0")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+3"));
            ////TestFmwk.assertTrue("cst138", ((new BigDecimal("1265E+1")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+4"));
            ////TestFmwk.assertTrue("cst139", ((new BigDecimal("1265E+2")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+5"));
            ////TestFmwk.assertTrue("cst140", ((new BigDecimal("1265E+3")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+6"));
            ////TestFmwk.assertTrue("cst141", ((new BigDecimal("1265E+4")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+7"));
            ////TestFmwk.assertTrue("cst142", ((new BigDecimal("1265E+8")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+11"));
            ////TestFmwk.assertTrue("cst143", ((new BigDecimal("1265E+20")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+23"));

            ////TestFmwk.assertTrue("cst150", ((new BigDecimal("0.1265")).ToString(CultureInfo.InvariantCulture)).Equals("0.1265"));
            ////TestFmwk.assertTrue("cst151", ((new BigDecimal("0.1265E-20")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-21"));
            ////TestFmwk.assertTrue("cst152", ((new BigDecimal("0.1265E-8")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-9"));
            ////TestFmwk.assertTrue("cst153", ((new BigDecimal("0.1265E-4")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-5"));
            ////TestFmwk.assertTrue("cst154", ((new BigDecimal("0.1265E-3")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-4"));
            ////TestFmwk.assertTrue("cst155", ((new BigDecimal("0.1265E-2")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-3"));
            ////TestFmwk.assertTrue("cst156", ((new BigDecimal("0.1265E-1")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-2"));
            ////TestFmwk.assertTrue("cst157", ((new BigDecimal("0.1265E-0")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E-1"));
            ////TestFmwk.assertTrue("cst158", ((new BigDecimal("0.1265E+1")).ToString(CultureInfo.InvariantCulture)).Equals("1.265"));
            ////TestFmwk.assertTrue("cst159", ((new BigDecimal("0.1265E+2")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+1"));
            ////TestFmwk.assertTrue("cst160", ((new BigDecimal("0.1265E+3")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+2"));
            ////TestFmwk.assertTrue("cst161", ((new BigDecimal("0.1265E+4")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+3"));
            ////TestFmwk.assertTrue("cst162", ((new BigDecimal("0.1265E+8")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+7"));
            ////TestFmwk.assertTrue("cst163", ((new BigDecimal("0.1265E+20")).ToString(CultureInfo.InvariantCulture)).Equals("1.265E+19"));

            ////TestFmwk.assertTrue("cst170", ((new BigDecimal("0.09e999999999")).ToString(CultureInfo.InvariantCulture)).Equals("9E+999999997"));
            ////TestFmwk.assertTrue("cst171", ((new BigDecimal("0.9e999999999")).ToString(CultureInfo.InvariantCulture)).Equals("9E+999999998"));
            ////TestFmwk.assertTrue("cst172", ((new BigDecimal("9e999999999")).ToString(CultureInfo.InvariantCulture)).Equals("9E+999999999"));
            ////TestFmwk.assertTrue("cst173", ((new BigDecimal("9.9e999999999")).ToString(CultureInfo.InvariantCulture)).Equals("9.9E+999999999"));
            ////TestFmwk.assertTrue("cst174", ((new BigDecimal("9.99e999999999")).ToString(CultureInfo.InvariantCulture)).Equals("9.99E+999999999"));
            ////TestFmwk.assertTrue("cst175", ((new BigDecimal("9.99e-999999999")).ToString(CultureInfo.InvariantCulture)).Equals("9.99E-999999999"));
            ////TestFmwk.assertTrue("cst176", ((new BigDecimal("9.9e-999999999")).ToString(CultureInfo.InvariantCulture)).Equals("9.9E-999999999"));
            ////TestFmwk.assertTrue("cst177", ((new BigDecimal("9e-999999999")).ToString(CultureInfo.InvariantCulture)).Equals("9E-999999999"));
            ////TestFmwk.assertTrue("cst179", ((new BigDecimal("99e-999999999")).ToString(CultureInfo.InvariantCulture)).Equals("9.9E-999999998"));
            ////TestFmwk.assertTrue("cst180", ((new BigDecimal("999e-999999999")).ToString(CultureInfo.InvariantCulture)).Equals("9.99E-999999997"));

            // ICU4N TODO: Convert error cases

            // baddies --
            badstrings = new string[] { "1..2", ".", "..", "++1", "--1",
                "-+1", "+-1", "12e", "12e++", "12f4", " +1", "+ 1", "12 ",
                " + 1", " - 1 ", "x", "-1-", "12-", "3+", "", "1e-",
                "7e1000000000", "", "e100", "\u0e5a", "\u0b65", "99e999999999",
                "999e999999999", "0.9e-999999999", "0.09e-999999999",
                "0.1e1000000000", "10e-1000000000", "0.9e9999999999",
                "99e-9999999999", "111e9999999999",
                "1111e-9999999999" + " " + "111e*123", "111e123-", "111e+12+",
                "111e1-3-", "111e1*23", "111e1e+3", "1e1.0", "1e123e", "ten",
                "ONE", "1e.1", "1e1.", "1ee", "e+1" }; // 200-203
                                                       // 204-207
                                                       // 208-211
                                                       // 211-214
                                                       // 215-219
                                                       // 220-222
                                                       // 223-224
                                                       // 225-226
                                                       // 227-228
                                                       // 229-230
                                                       // 231-232
                                                       // 233-234
                                                       // 235-237
                                                       // 238-240
                                                       // 241-244
                                                       // 245-248

            //// watch out for commas on continuation lines

            //{
            //    int len16 = badstrings.Length;
            //    i = 0;
            //    for (; len16 > 0; len16--, i++)
            //    {
            //        try
            //        {
            //            new BigDecimal(badstrings[i]);
            //            say(">>> cst" + (200 + i) + ":" + " " + badstrings[i] + " " + (new BigDecimal(badstrings[i])).ToString(CultureInfo.InvariantCulture));
            //            flag = false;
            //        }
            //        catch (FormatException e17)
            //        {
            //            flag = true;
            //        }
            //        TestFmwk.assertTrue("cst" + (200 + i), flag);
            //    }
            //}/* i */

            //try
            //{
            //    new BigDecimal((string)null);
            //    flag = false;
            //}
            //catch (ArgumentNullException e18)
            //{
            //    flag = true;
            //}/* checknull */
            //TestFmwk.assertTrue("cst301", flag);

        }

        /** Mutation tests (checks that contents of constant objects are unchanged). */

        [Test]
        public void diagmutation()
        {
            /* ---------------------------------------------------------------- */
            /* Final tests -- check constants haven't mutated */
            /* -- also that MC objects haven't mutated */
            /* ---------------------------------------------------------------- */
            TestFmwk.assertTrue("cuc001", (BigDecimal.Zero.ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("cuc002", (BigDecimal.One.ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("cuc003", (BigDecimal.Ten.ToString(CultureInfo.InvariantCulture)).Equals("10"));

            TestFmwk.assertTrue("cuc010", BigDecimal.RoundCeiling == RoundingMode.Ceiling);
            TestFmwk.assertTrue("cuc011", BigDecimal.RoundDown == RoundingMode.Down);
            TestFmwk.assertTrue("cuc012", BigDecimal.RoundFloor == RoundingMode.Floor);
            TestFmwk.assertTrue("cuc013", BigDecimal.RoundHalfDown == RoundingMode.HalfDown);
            TestFmwk.assertTrue("cuc014", BigDecimal.RoundHalfEven == RoundingMode.HalfEven);
            TestFmwk.assertTrue("cuc015", BigDecimal.RoundHalfUp == RoundingMode.HalfUp);
            TestFmwk.assertTrue("cuc016", BigDecimal.RoundUnnecessary == RoundingMode.Unnecessary);
            TestFmwk.assertTrue("cuc017", BigDecimal.RoundUp == RoundingMode.Up);

            TestFmwk.assertTrue("cuc020", (MathContext.Default.Digits) == 9);
            TestFmwk.assertTrue("cuc021", (MathContext.Default.Form) == ExponentForm.Scientific);
            TestFmwk.assertTrue("cuc022", (MathContext.Default.LostDigits ? 1 : 0) == 0);
            TestFmwk.assertTrue("cuc023", (MathContext.Default.RoundingMode) == RoundingMode.HalfUp);

            // mc9 =MathContext(9)
            // mcld =MathContext(9, SCIENTIFIC, 1)
            // mcfd =MathContext(0, PLAIN)
            TestFmwk.assertTrue("cuc030", (mc9.Digits) == 9);
            TestFmwk.assertTrue("cuc031", (mc9.Form) == ExponentForm.Scientific);
            TestFmwk.assertTrue("cuc032", (mc9.LostDigits ? 1 : 0) == 0);
            TestFmwk.assertTrue("cuc033", (mc9.RoundingMode) == RoundingMode.HalfUp);
            TestFmwk.assertTrue("cuc034", (mcld.Digits) == 9);
            TestFmwk.assertTrue("cuc035", (mcld.Form) == ExponentForm.Scientific);
            TestFmwk.assertTrue("cuc036", (mcld.LostDigits ? 1 : 0) == 1);
            TestFmwk.assertTrue("cuc037", (mcld.RoundingMode) == RoundingMode.HalfUp);
            TestFmwk.assertTrue("cuc038", (mcfd.Digits) == 0);
            TestFmwk.assertTrue("cuc039", (mcfd.Form) == ExponentForm.Plain);
            TestFmwk.assertTrue("cuc040", (mcfd.LostDigits ? 1 : 0) == 0);
            TestFmwk.assertTrue("cuc041", (mcfd.RoundingMode) == RoundingMode.HalfUp);

        }


        /* ----------------------------------------------------------------- */
        /* Operator test methods */
        /* ----------------------------------------------------------------- */
        // The use of context in these tests are primarily to show that they
        // are correctly passed to the methods, except that we check that
        // each method checks for lostDigits.

        /** Test the {@link BigDecimal#abs} method. */

        [Test]
        public void diagabs()
        {
            bool flag = false;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            // most of the function of this is tested by add
            TestFmwk.assertTrue("abs001", ((BigDecimal.Parse("2", numberStyle, provider)).Abs().ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("abs002", ((BigDecimal.Parse("-2", numberStyle, provider)).Abs().ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("abs003", ((BigDecimal.Parse("+0.000", numberStyle, provider)).Abs().ToString(CultureInfo.InvariantCulture)).Equals("0.000"));
            TestFmwk.assertTrue("abs004", ((BigDecimal.Parse("00.000", numberStyle, provider)).Abs().ToString(CultureInfo.InvariantCulture)).Equals("0.000"));
            TestFmwk.assertTrue("abs005", ((BigDecimal.Parse("-0.000", numberStyle, provider)).Abs().ToString(CultureInfo.InvariantCulture)).Equals("0.000"));
            TestFmwk.assertTrue("abs006", ((BigDecimal.Parse("+0.000", numberStyle, provider)).Abs(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("abs007", ((BigDecimal.Parse("00.000", numberStyle, provider)).Abs(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("abs008", ((BigDecimal.Parse("-0.000", numberStyle, provider)).Abs(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("abs009", ((BigDecimal.Parse("-2000000", numberStyle, provider)).Abs().ToString(CultureInfo.InvariantCulture)).Equals("2000000"));
            TestFmwk.assertTrue("abs010", ((BigDecimal.Parse("-2000000", numberStyle, provider)).Abs(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2000000"));
            TestFmwk.assertTrue("abs011", ((BigDecimal.Parse("-2000000", numberStyle, provider)).Abs(mc6).ToString(CultureInfo.InvariantCulture)).Equals("2.00000E+6"));
            TestFmwk.assertTrue("abs012", ((BigDecimal.Parse("2000000", numberStyle, provider)).Abs(mc6).ToString(CultureInfo.InvariantCulture)).Equals("2.00000E+6"));
            TestFmwk.assertTrue("abs013", ((BigDecimal.Parse("0.2", numberStyle, provider)).Abs().ToString(CultureInfo.InvariantCulture)).Equals("0.2"));
            TestFmwk.assertTrue("abs014", ((BigDecimal.Parse("-0.2", numberStyle, provider)).Abs().ToString(CultureInfo.InvariantCulture)).Equals("0.2"));
            TestFmwk.assertTrue("abs015", ((BigDecimal.Parse("0.01", numberStyle, provider)).Abs().ToString(CultureInfo.InvariantCulture)).Equals("0.01"));
            TestFmwk.assertTrue("abs016", ((BigDecimal.Parse("-0.01", numberStyle, provider)).Abs().ToString(CultureInfo.InvariantCulture)).Equals("0.01"));
            try
            {
                tenlong.Abs(mcld);
                flag = false;
            }
            catch (ArithmeticException e19)
            {
                ae = e19;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("abs020", flag);
            // check lostdigits not raised if digits=0 [monadic method]
            try
            {
                tenlong.Abs(mcld0);
                flag = true;
            }
            catch (ArithmeticException e20)
            {
                ae = e20;
                flag = false;
            }/* checkdigits */
            TestFmwk.assertTrue("abs021", flag);
            try
            {
                BigDecimal.Ten
                        .Abs((MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e21)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("abs022", flag);

        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#add} method. */

        [Test]
        public void diagadd()
        {
            bool flag = false;
            BigDecimal alhs;
            BigDecimal arhs;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            // [first group are 'quick confidence check']
            TestFmwk.assertTrue("add001", ((new BigDecimal(2)).Add(new BigDecimal(3), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("5"));
            TestFmwk.assertTrue("add003", ((BigDecimal.Parse("5.75", numberStyle, provider)).Add(BigDecimal.Parse("3.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("9.05"));
            TestFmwk.assertTrue("add004", ((BigDecimal.Parse("5", numberStyle, provider)).Add(BigDecimal.Parse("-3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("add005", ((BigDecimal.Parse("-5", numberStyle, provider)).Add(BigDecimal.Parse("-3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-8"));
            TestFmwk.assertTrue("add006", ((BigDecimal.Parse("-7", numberStyle, provider)).Add(BigDecimal.Parse("2.5", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-4.5"));
            TestFmwk.assertTrue("add007", ((BigDecimal.Parse("0.7", numberStyle, provider)).Add(BigDecimal.Parse("0.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("add008", ((BigDecimal.Parse("1.25", numberStyle, provider)).Add(BigDecimal.Parse("1.25", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.50"));
            TestFmwk.assertTrue("add009", ((BigDecimal.Parse("1.23456789", numberStyle, provider)).Add(BigDecimal.Parse("1.00000000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.23456789"));

            TestFmwk.assertTrue("add010", ((BigDecimal.Parse("1.23456789", numberStyle, provider)).Add(BigDecimal.Parse("1.00000011", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.23456800"));


            TestFmwk.assertTrue("add011", ((BigDecimal.Parse("0.4444444444", numberStyle, provider)).Add(BigDecimal.Parse("0.5555555555", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000"));

            TestFmwk.assertTrue("add012", ((BigDecimal.Parse("0.4444444440", numberStyle, provider)).Add(BigDecimal.Parse("0.5555555555", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000"));

            TestFmwk.assertTrue("add013", ((BigDecimal.Parse("0.4444444444", numberStyle, provider)).Add(BigDecimal.Parse("0.5555555550", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.999999999"));

            TestFmwk.assertTrue("add014", ((BigDecimal.Parse("0.4444444444999", numberStyle, provider)).Add(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.444444444"));

            TestFmwk.assertTrue("add015", ((BigDecimal.Parse("0.4444444445000", numberStyle, provider)).Add(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.444444445"));


            TestFmwk.assertTrue("add016", ((BigDecimal.Parse("70", numberStyle, provider)).Add(BigDecimal.Parse("10000e+9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000E+13"));

            TestFmwk.assertTrue("add017", ((BigDecimal.Parse("700", numberStyle, provider)).Add(BigDecimal.Parse("10000e+9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000E+13"));

            TestFmwk.assertTrue("add018", ((BigDecimal.Parse("7000", numberStyle, provider)).Add(BigDecimal.Parse("10000e+9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000E+13"));

            TestFmwk.assertTrue("add019", ((BigDecimal.Parse("70000", numberStyle, provider)).Add(BigDecimal.Parse("10000e+9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000001E+13"));

            TestFmwk.assertTrue("add020", ((BigDecimal.Parse("700000", numberStyle, provider)).Add(BigDecimal.Parse("10000e+9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000007E+13"));


            // [Now the same group with fixed arithmetic]
            TestFmwk.assertTrue("add030", ((new BigDecimal(2)).Add(new BigDecimal(3)).ToString(CultureInfo.InvariantCulture)).Equals("5"));
            TestFmwk.assertTrue("add031", ((BigDecimal.Parse("5.75", numberStyle, provider)).Add(BigDecimal.Parse("3.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9.05"));
            TestFmwk.assertTrue("add032", ((BigDecimal.Parse("5", numberStyle, provider)).Add(BigDecimal.Parse("-3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("add033", ((BigDecimal.Parse("-5", numberStyle, provider)).Add(BigDecimal.Parse("-3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-8"));
            TestFmwk.assertTrue("add034", ((BigDecimal.Parse("-7", numberStyle, provider)).Add(BigDecimal.Parse("2.5", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-4.5"));
            TestFmwk.assertTrue("add035", ((BigDecimal.Parse("0.7", numberStyle, provider)).Add(BigDecimal.Parse("0.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("add036", ((BigDecimal.Parse("1.25", numberStyle, provider)).Add(BigDecimal.Parse("1.25", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.50"));
            TestFmwk.assertTrue("add037", ((BigDecimal.Parse("1.23456789", numberStyle, provider)).Add(BigDecimal.Parse("1.00000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.23456789"));

            TestFmwk.assertTrue("add038", ((BigDecimal.Parse("1.23456789", numberStyle, provider)).Add(BigDecimal.Parse("1.00000011", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.23456800"));


            TestFmwk.assertTrue("add039", ((BigDecimal.Parse("0.4444444444", numberStyle, provider)).Add(BigDecimal.Parse("0.5555555555", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.9999999999"));

            TestFmwk.assertTrue("add040", ((BigDecimal.Parse("0.4444444440", numberStyle, provider)).Add(BigDecimal.Parse("0.5555555555", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.9999999995"));

            TestFmwk.assertTrue("add041", ((BigDecimal.Parse("0.4444444444", numberStyle, provider)).Add(BigDecimal.Parse("0.5555555550", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.9999999994"));

            TestFmwk.assertTrue("add042", ((BigDecimal.Parse("0.4444444444999", numberStyle, provider)).Add(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.4444444444999"));

            TestFmwk.assertTrue("add043", ((BigDecimal.Parse("0.4444444445000", numberStyle, provider)).Add(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.4444444445000"));


            TestFmwk.assertTrue("add044", ((BigDecimal.Parse("70", numberStyle, provider)).Add(BigDecimal.Parse("10000e+9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000000070"));

            TestFmwk.assertTrue("add045", ((BigDecimal.Parse("700", numberStyle, provider)).Add(BigDecimal.Parse("10000e+9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000000700"));

            TestFmwk.assertTrue("add046", ((BigDecimal.Parse("7000", numberStyle, provider)).Add(BigDecimal.Parse("10000e+9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000007000"));

            TestFmwk.assertTrue("add047", ((BigDecimal.Parse("70000", numberStyle, provider)).Add(BigDecimal.Parse("10000e+9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000070000"));

            TestFmwk.assertTrue("add048", ((BigDecimal.Parse("700000", numberStyle, provider)).Add(BigDecimal.Parse("10000e+9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000700000"));


            // symmetry:
            TestFmwk.assertTrue("add049", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("70", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000E+13"));

            TestFmwk.assertTrue("add050", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("700", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000E+13"));

            TestFmwk.assertTrue("add051", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("7000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000E+13"));

            TestFmwk.assertTrue("add052", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("70000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000001E+13"));

            TestFmwk.assertTrue("add053", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("700000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000007E+13"));


            TestFmwk.assertTrue("add054", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("70", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000000070"));

            TestFmwk.assertTrue("add055", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("700", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000000700"));

            TestFmwk.assertTrue("add056", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("7000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000007000"));

            TestFmwk.assertTrue("add057", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("70000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000070000"));

            TestFmwk.assertTrue("add058", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("700000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000700000"));

            // some rounding effects
            TestFmwk.assertTrue("add059", ((BigDecimal.Parse("0.9998", numberStyle, provider)).Add(BigDecimal.Parse("0.0000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.9998"));

            TestFmwk.assertTrue("add060", ((BigDecimal.Parse("0.9998", numberStyle, provider)).Add(BigDecimal.Parse("0.0001", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.9999"));

            TestFmwk.assertTrue("add061", ((BigDecimal.Parse("0.9998", numberStyle, provider)).Add(BigDecimal.Parse("0.0002", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1.0000"));

            TestFmwk.assertTrue("add062", ((BigDecimal.Parse("0.9998", numberStyle, provider)).Add(BigDecimal.Parse("0.0003", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1.0001"));


            // MC
            TestFmwk.assertTrue("add070", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("70000", numberStyle, provider), mcfd).ToString(CultureInfo.InvariantCulture)).Equals("10000000070000"));

            TestFmwk.assertTrue("add071", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("70000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000001E+13"));

            TestFmwk.assertTrue("add072", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Add(BigDecimal.Parse("70000", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("1.00000E+13"));


            // zero preservation
            TestFmwk.assertTrue("add080", (BigDecimal.One.Add(BigDecimal.Parse("0.0001", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("1.0001"));

            TestFmwk.assertTrue("add081", (BigDecimal.One.Add(BigDecimal.Parse("0.00001", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("1.00001"));

            TestFmwk.assertTrue("add082", (BigDecimal.One.Add(BigDecimal.Parse("0.000001", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("1.00000"));

            TestFmwk.assertTrue("add083", (BigDecimal.One.Add(BigDecimal.Parse("0.0000001", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("1.00000"));

            TestFmwk.assertTrue("add084", (BigDecimal.One.Add(BigDecimal.Parse("0.00000001", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("1.00000"));


            // more fixed, LHS swaps
            TestFmwk.assertTrue("add090", ((BigDecimal.Parse("-56267E-10", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.0000056267"));
            TestFmwk.assertTrue("add091", ((BigDecimal.Parse("-56267E-6", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.056267"));
            TestFmwk.assertTrue("add092", ((BigDecimal.Parse("-56267E-5", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.56267"));
            TestFmwk.assertTrue("add093", ((BigDecimal.Parse("-56267E-4", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-5.6267"));
            TestFmwk.assertTrue("add094", ((BigDecimal.Parse("-56267E-3", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-56.267"));
            TestFmwk.assertTrue("add095", ((BigDecimal.Parse("-56267E-2", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-562.67"));
            TestFmwk.assertTrue("add096", ((BigDecimal.Parse("-56267E-1", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-5626.7"));
            TestFmwk.assertTrue("add097", ((BigDecimal.Parse("-56267E-0", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-56267"));
            TestFmwk.assertTrue("add098", ((BigDecimal.Parse("-5E-10", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.0000000005"));
            TestFmwk.assertTrue("add099", ((BigDecimal.Parse("-5E-5", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.00005"));
            TestFmwk.assertTrue("add100", ((BigDecimal.Parse("-5E-1", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.5"));
            TestFmwk.assertTrue("add101", ((BigDecimal.Parse("-5E-10", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.0000000005"));
            TestFmwk.assertTrue("add102", ((BigDecimal.Parse("-5E-5", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.00005"));
            TestFmwk.assertTrue("add103", ((BigDecimal.Parse("-5E-1", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.5"));
            TestFmwk.assertTrue("add104", ((BigDecimal.Parse("-5E10", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-50000000000"));
            TestFmwk.assertTrue("add105", ((BigDecimal.Parse("-5E5", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-500000"));
            TestFmwk.assertTrue("add106", ((BigDecimal.Parse("-5E1", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-50"));
            TestFmwk.assertTrue("add107", ((BigDecimal.Parse("-5E0", numberStyle, provider)).Add(zero).ToString(CultureInfo.InvariantCulture)).Equals("-5"));

            // more fixed, RHS swaps
            TestFmwk.assertTrue("add108", (zero.Add(BigDecimal.Parse("-56267E-10", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.0000056267"));
            TestFmwk.assertTrue("add109", (zero.Add(BigDecimal.Parse("-56267E-6", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.056267"));
            TestFmwk.assertTrue("add110", (zero.Add(BigDecimal.Parse("-56267E-5", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.56267"));
            TestFmwk.assertTrue("add111", (zero.Add(BigDecimal.Parse("-56267E-4", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-5.6267"));
            TestFmwk.assertTrue("add112", (zero.Add(BigDecimal.Parse("-56267E-3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-56.267"));
            TestFmwk.assertTrue("add113", (zero.Add(BigDecimal.Parse("-56267E-2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-562.67"));
            TestFmwk.assertTrue("add114", (zero.Add(BigDecimal.Parse("-56267E-1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-5626.7"));
            TestFmwk.assertTrue("add115", (zero.Add(BigDecimal.Parse("-56267E-0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-56267"));
            TestFmwk.assertTrue("add116", (zero.Add(BigDecimal.Parse("-5E-10", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.0000000005"));
            TestFmwk.assertTrue("add117", (zero.Add(BigDecimal.Parse("-5E-5", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.00005"));
            TestFmwk.assertTrue("add118", (zero.Add(BigDecimal.Parse("-5E-1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.5"));
            TestFmwk.assertTrue("add129", (zero.Add(BigDecimal.Parse("-5E-10", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.0000000005"));
            TestFmwk.assertTrue("add130", (zero.Add(BigDecimal.Parse("-5E-5", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.00005"));
            TestFmwk.assertTrue("add131", (zero.Add(BigDecimal.Parse("-5E-1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.5"));
            TestFmwk.assertTrue("add132", (zero.Add(BigDecimal.Parse("-5E10", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-50000000000"));
            TestFmwk.assertTrue("add133", (zero.Add(BigDecimal.Parse("-5E5", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-500000"));
            TestFmwk.assertTrue("add134", (zero.Add(BigDecimal.Parse("-5E1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-50"));
            TestFmwk.assertTrue("add135", (zero.Add(BigDecimal.Parse("-5E0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-5"));

            // [some of the next group are really constructor tests]
            TestFmwk.assertTrue("add140", ((BigDecimal.Parse("00.0", numberStyle, provider)).Add(BigDecimal.Parse("0.00", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("add141", ((BigDecimal.Parse("0.00", numberStyle, provider)).Add(BigDecimal.Parse("00.0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("add142", ((BigDecimal.Parse("3", numberStyle, provider)).Add(BigDecimal.Parse(".3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("3.3"));
            TestFmwk.assertTrue("add143", ((BigDecimal.Parse("3.", numberStyle, provider)).Add(BigDecimal.Parse(".3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("3.3"));
            TestFmwk.assertTrue("add144", ((BigDecimal.Parse("3.0", numberStyle, provider)).Add(BigDecimal.Parse(".3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("3.3"));
            TestFmwk.assertTrue("add145", ((BigDecimal.Parse("3.00", numberStyle, provider)).Add(BigDecimal.Parse(".3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("3.30"));
            TestFmwk.assertTrue("add146", ((BigDecimal.Parse("3", numberStyle, provider)).Add(BigDecimal.Parse("3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("6"));
            TestFmwk.assertTrue("add147", ((BigDecimal.Parse("3", numberStyle, provider)).Add(BigDecimal.Parse("+3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("6"));
            TestFmwk.assertTrue("add148", ((BigDecimal.Parse("3", numberStyle, provider)).Add(BigDecimal.Parse("-3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("add149", ((BigDecimal.Parse("0.03", numberStyle, provider)).Add(BigDecimal.Parse("-0.03", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("add150", ((BigDecimal.Parse("00.0", numberStyle, provider)).Add(BigDecimal.Parse("0.00", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("add151", ((BigDecimal.Parse("0.00", numberStyle, provider)).Add(BigDecimal.Parse("00.0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("add152", ((BigDecimal.Parse("3", numberStyle, provider)).Add(BigDecimal.Parse(".3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("3.3"));
            TestFmwk.assertTrue("add153", ((BigDecimal.Parse("3.", numberStyle, provider)).Add(BigDecimal.Parse(".3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("3.3"));
            TestFmwk.assertTrue("add154", ((BigDecimal.Parse("3.0", numberStyle, provider)).Add(BigDecimal.Parse(".3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("3.3"));
            TestFmwk.assertTrue("add155", ((BigDecimal.Parse("3.00", numberStyle, provider)).Add(BigDecimal.Parse(".3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("3.30"));
            TestFmwk.assertTrue("add156", ((BigDecimal.Parse("3", numberStyle, provider)).Add(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("6"));
            TestFmwk.assertTrue("add157", ((BigDecimal.Parse("3", numberStyle, provider)).Add(BigDecimal.Parse("+3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("6"));
            TestFmwk.assertTrue("add158", ((BigDecimal.Parse("3", numberStyle, provider)).Add(BigDecimal.Parse("-3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("add159", ((BigDecimal.Parse("0.3", numberStyle, provider)).Add(BigDecimal.Parse("-0.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("add160", ((BigDecimal.Parse("0.03", numberStyle, provider)).Add(BigDecimal.Parse("-0.03", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("add161", ((BigDecimal.Parse("7E+12", numberStyle, provider)).Add(BigDecimal.Parse("-1", numberStyle, provider), mcfd).ToString(CultureInfo.InvariantCulture)).Equals("6999999999999"));

            TestFmwk.assertTrue("add162", ((BigDecimal.Parse("7E+12", numberStyle, provider)).Add(BigDecimal.Parse("1.11", numberStyle, provider), mcfd).ToString(CultureInfo.InvariantCulture)).Equals("7000000000001.11"));

            TestFmwk.assertTrue("add163", ((BigDecimal.Parse("1.11", numberStyle, provider)).Add(BigDecimal.Parse("7E+12", numberStyle, provider), mcfd).ToString(CultureInfo.InvariantCulture)).Equals("7000000000001.11"));


            // input preparation tests
            alhs = BigDecimal.Parse("12345678900000", numberStyle, provider);
            arhs = BigDecimal.Parse("9999999999999", numberStyle, provider);
            TestFmwk.assertTrue("add170", (alhs.Add(arhs, mc3).ToString(CultureInfo.InvariantCulture)).Equals("2.23E+13"));
            TestFmwk.assertTrue("add171", (arhs.Add(alhs, mc3).ToString(CultureInfo.InvariantCulture)).Equals("2.23E+13"));
            TestFmwk.assertTrue("add172", ((BigDecimal.Parse("12E+3", numberStyle, provider)).Add(BigDecimal.Parse("3456", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("1.55E+4"));
            // next was 1.54E+4 under old [truncate to digits+1] rules
            TestFmwk.assertTrue("add173", ((BigDecimal.Parse("12E+3", numberStyle, provider)).Add(BigDecimal.Parse("3446", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("1.55E+4"));
            TestFmwk.assertTrue("add174", ((BigDecimal.Parse("12E+3", numberStyle, provider)).Add(BigDecimal.Parse("3454", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("1.55E+4"));
            TestFmwk.assertTrue("add175", ((BigDecimal.Parse("12E+3", numberStyle, provider)).Add(BigDecimal.Parse("3444", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("1.54E+4"));

            TestFmwk.assertTrue("add176", ((BigDecimal.Parse("3456", numberStyle, provider)).Add(BigDecimal.Parse("12E+3", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("1.55E+4"));
            // next was 1.54E+4 under old [truncate to digits+1] rules
            TestFmwk.assertTrue("add177", ((BigDecimal.Parse("3446", numberStyle, provider)).Add(BigDecimal.Parse("12E+3", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("1.55E+4"));
            TestFmwk.assertTrue("add178", ((BigDecimal.Parse("3454", numberStyle, provider)).Add(BigDecimal.Parse("12E+3", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("1.55E+4"));
            TestFmwk.assertTrue("add179", ((BigDecimal.Parse("3444", numberStyle, provider)).Add(BigDecimal.Parse("12E+3", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("1.54E+4"));

            try
            {
                ten.Add((BigDecimal)null);
                flag = false;
            }
            catch (ArgumentNullException e22)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("add200", flag);
            try
            {
                ten.Add(ten, (MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e23)
            {
                flag = true;
            }/* checknull2 */
            TestFmwk.assertTrue("add201", flag);

            try
            {
                tenlong.Add(BigDecimal.Zero, mcld);
                flag = false;
            }
            catch (ArithmeticException e24)
            {
                ae = e24;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("add202", flag);
            try
            {
                BigDecimal.Zero.Add(tenlong, mcld);
                flag = false;
            }
            catch (ArithmeticException e25)
            {
                ae = e25;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("add203", flag);

            // check lostdigits not raised if digits=0 [dyadic method]
            try
            {
                tenlong.Add(BigDecimal.Zero, mcld0);
                flag = true;
            }
            catch (ArithmeticException e26)
            {
                ae = e26;
                flag = false;
            }/* checkdigits */
            TestFmwk.assertTrue("add204", flag);
            try
            {
                BigDecimal.Zero.Add(tenlong, mcld0);
                flag = true;
            }
            catch (ArithmeticException e27)
            {
                ae = e27;
                flag = false;
            }/* checkdigits */
            TestFmwk.assertTrue("add205", flag);

        }

        /* ----------------------------------------------------------------- */
        /**
         * Test the {@link BigDecimal#compareTo(BigDecimal)}
         * method.
         */

        [Test]
        public void diagcompareto()
        {
            bool flag = false;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;
            // we assume add/subtract test function; this just
            // tests existence, exceptions, and possible results

            TestFmwk.assertTrue("cpt001", ((BigDecimal.Parse("5", numberStyle, provider)).CompareTo(BigDecimal.Parse("2", numberStyle, provider))) == 1);
            TestFmwk.assertTrue("cpt002", ((BigDecimal.Parse("5", numberStyle, provider)).CompareTo(BigDecimal.Parse("5", numberStyle, provider))) == 0);
            TestFmwk.assertTrue("cpt003", ((BigDecimal.Parse("5", numberStyle, provider)).CompareTo(BigDecimal.Parse("5.00", numberStyle, provider))) == 0);
            TestFmwk.assertTrue("cpt004", ((BigDecimal.Parse("0.5", numberStyle, provider)).CompareTo(BigDecimal.Parse("0.5", numberStyle, provider))) == 0);
            TestFmwk.assertTrue("cpt005", ((BigDecimal.Parse("2", numberStyle, provider)).CompareTo(BigDecimal.Parse("5", numberStyle, provider))) == (-1));
            TestFmwk.assertTrue("cpt006", ((BigDecimal.Parse("2", numberStyle, provider)).CompareTo(BigDecimal.Parse("5", numberStyle, provider), mcdef)) == (-1));
            TestFmwk.assertTrue("cpt007", ((BigDecimal.Parse("2", numberStyle, provider)).CompareTo(BigDecimal.Parse("5", numberStyle, provider), mc6)) == (-1));
            TestFmwk.assertTrue("cpt008", ((BigDecimal.Parse("2", numberStyle, provider)).CompareTo(BigDecimal.Parse("5", numberStyle, provider), mcfd)) == (-1));
            //    try
            //    {
            //        ten.CompareTo((BigDecimal)null);
            //        flag = false;
            //    }
            //    catch (ArgumentNullException e28) {
            //    flag = true;
            //}/* checknull */
            //TestFmwk.assertTrue("cpt100", flag);

            // ICU4N: Return 1 when comparing to null to match other .NET classes
            TestFmwk.assertTrue("cpt100", ten.CompareTo((BigDecimal)null) == 1);

            try
            {
                ten.CompareTo(ten, (MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e29)
            {
                flag = true;
            }/* checknull2 */
            TestFmwk.assertTrue("cpt101", flag);

            try
            {
                tenlong.CompareTo(BigDecimal.One, mcld);
                flag = false;
            }
            catch (ArithmeticException e30)
            {
                ae = e30;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("cpt102", flag);
            try
            {
                BigDecimal.One.CompareTo(tenlong, mcld);
                flag = false;
            }
            catch (ArithmeticException e31)
            {
                ae = e31;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("cpt103", flag);

        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#divide} method. */

        [Test]
        public void diagdivide()
        {
            bool flag = false;
            MathContext rmcd;
            RoundingMode rhu;
            RoundingMode rd;
            RoundingMode ru;
            Exception e = null;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("div301", ((BigDecimal.Parse("1", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.333333333"));
            TestFmwk.assertTrue("div302", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.666666667"));
            TestFmwk.assertTrue("div303", ((BigDecimal.Parse("2.4", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.4"));
            TestFmwk.assertTrue("div304", ((BigDecimal.Parse("2.4", numberStyle, provider)).Divide(BigDecimal.Parse("-1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-2.4"));
            TestFmwk.assertTrue("div305", ((BigDecimal.Parse("-2.4", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-2.4"));
            TestFmwk.assertTrue("div306", ((BigDecimal.Parse("-2.4", numberStyle, provider)).Divide(BigDecimal.Parse("-1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.4"));
            TestFmwk.assertTrue("div307", ((BigDecimal.Parse("2.40", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.4"));
            TestFmwk.assertTrue("div308", ((BigDecimal.Parse("2.400", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.4"));
            TestFmwk.assertTrue("div309", ((BigDecimal.Parse("2.4", numberStyle, provider)).Divide(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.2"));
            TestFmwk.assertTrue("div310", ((BigDecimal.Parse("2.400", numberStyle, provider)).Divide(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.2"));
            TestFmwk.assertTrue("div311", ((BigDecimal.Parse("2.", numberStyle, provider)).Divide(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div312", ((BigDecimal.Parse("20", numberStyle, provider)).Divide(BigDecimal.Parse("20", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div313", ((BigDecimal.Parse("187", numberStyle, provider)).Divide(BigDecimal.Parse("187", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div314", ((BigDecimal.Parse("5", numberStyle, provider)).Divide(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.5"));
            TestFmwk.assertTrue("div315", ((BigDecimal.Parse("5", numberStyle, provider)).Divide(BigDecimal.Parse("2.0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.5"));
            TestFmwk.assertTrue("div316", ((BigDecimal.Parse("5", numberStyle, provider)).Divide(BigDecimal.Parse("2.000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.5"));
            TestFmwk.assertTrue("div317", ((BigDecimal.Parse("5", numberStyle, provider)).Divide(BigDecimal.Parse("0.200", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("25"));
            TestFmwk.assertTrue("div318", ((BigDecimal.Parse("999999999", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("999999999"));
            TestFmwk.assertTrue("div319", ((BigDecimal.Parse("999999999.4", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("999999999"));
            TestFmwk.assertTrue("div320", ((BigDecimal.Parse("999999999.5", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            TestFmwk.assertTrue("div321", ((BigDecimal.Parse("999999999.9", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            TestFmwk.assertTrue("div322", ((BigDecimal.Parse("999999999.999", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            TestFmwk.assertTrue("div323", ((BigDecimal.Parse("0.0000E-50", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            // MC
            TestFmwk.assertTrue("div325", ((BigDecimal.Parse("999999999", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("999999999"));
            TestFmwk.assertTrue("div326", ((BigDecimal.Parse("999999999", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("1E+9"));
            TestFmwk.assertTrue("div327", ((BigDecimal.Parse("9999999", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("1E+7"));
            TestFmwk.assertTrue("div328", ((BigDecimal.Parse("999999", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("999999"));

            // check rounding explicitly [note: digits+1 truncation]
            rmcd = new MathContext(2, MathContext.Scientific, false, MathContext.RoundCeiling);
            TestFmwk.assertTrue("div330", ((BigDecimal.Parse("1.50", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            TestFmwk.assertTrue("div331", ((BigDecimal.Parse("1.51", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.6"));
            TestFmwk.assertTrue("div332", ((BigDecimal.Parse("1.55", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.6"));
            rmcd = new MathContext(2, MathContext.Scientific, false, MathContext.RoundDown);
            TestFmwk.assertTrue("div333", ((BigDecimal.Parse("1.55", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            TestFmwk.assertTrue("div334", ((BigDecimal.Parse("1.59", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            rmcd = new MathContext(2, MathContext.Scientific, false, MathContext.RoundFloor);
            TestFmwk.assertTrue("div335", ((BigDecimal.Parse("1.55", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            TestFmwk.assertTrue("div336", ((BigDecimal.Parse("1.59", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            rmcd = new MathContext(2, MathContext.Scientific, false, MathContext.RoundHalfDown);
            TestFmwk.assertTrue("div337", ((BigDecimal.Parse("1.45", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.4"));
            TestFmwk.assertTrue("div338", ((BigDecimal.Parse("1.50", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            TestFmwk.assertTrue("div339", ((BigDecimal.Parse("1.55", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            rmcd = new MathContext(2, MathContext.Scientific, false, MathContext.RoundHalfEven);
            TestFmwk.assertTrue("div340", ((BigDecimal.Parse("1.45", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.4"));
            TestFmwk.assertTrue("div341", ((BigDecimal.Parse("1.50", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            TestFmwk.assertTrue("div342", ((BigDecimal.Parse("1.55", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.6"));
            rmcd = new MathContext(2, MathContext.Scientific, false, MathContext.RoundHalfUp);
            TestFmwk.assertTrue("div343", ((BigDecimal.Parse("1.45", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            TestFmwk.assertTrue("div344", ((BigDecimal.Parse("1.50", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            TestFmwk.assertTrue("div345", ((BigDecimal.Parse("1.55", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.6"));
            rmcd = new MathContext(2, MathContext.Scientific, false, MathContext.RoundUp);
            TestFmwk.assertTrue("div346", ((BigDecimal.Parse("1.50", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.5"));
            TestFmwk.assertTrue("div347", ((BigDecimal.Parse("1.51", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.6"));
            TestFmwk.assertTrue("div348", ((BigDecimal.Parse("1.55", numberStyle, provider)).Divide(one, rmcd).ToString(CultureInfo.InvariantCulture)).Equals("1.6"));

            // fixed point...
            TestFmwk.assertTrue("div350", ((BigDecimal.Parse("1", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div351", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div352", ((BigDecimal.Parse("2.4", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.4"));
            TestFmwk.assertTrue("div353", ((BigDecimal.Parse("2.4", numberStyle, provider)).Divide(BigDecimal.Parse("-1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-2.4"));
            TestFmwk.assertTrue("div354", ((BigDecimal.Parse("-2.4", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-2.4"));
            TestFmwk.assertTrue("div355", ((BigDecimal.Parse("-2.4", numberStyle, provider)).Divide(BigDecimal.Parse("-1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.4"));
            TestFmwk.assertTrue("div356", ((BigDecimal.Parse("2.40", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.40"));
            TestFmwk.assertTrue("div357", ((BigDecimal.Parse("2.400", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.400"));
            TestFmwk.assertTrue("div358", ((BigDecimal.Parse("2.4", numberStyle, provider)).Divide(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1.2"));
            TestFmwk.assertTrue("div359", ((BigDecimal.Parse("2.400", numberStyle, provider)).Divide(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1.200"));
            TestFmwk.assertTrue("div360", ((BigDecimal.Parse("2.", numberStyle, provider)).Divide(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div361", ((BigDecimal.Parse("20", numberStyle, provider)).Divide(BigDecimal.Parse("20", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div362", ((BigDecimal.Parse("187", numberStyle, provider)).Divide(BigDecimal.Parse("187", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div363", ((BigDecimal.Parse("5", numberStyle, provider)).Divide(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            TestFmwk.assertTrue("div364", ((BigDecimal.Parse("5", numberStyle, provider)).Divide(BigDecimal.Parse("2.0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            TestFmwk.assertTrue("div365", ((BigDecimal.Parse("5", numberStyle, provider)).Divide(BigDecimal.Parse("2.000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            TestFmwk.assertTrue("div366", ((BigDecimal.Parse("5", numberStyle, provider)).Divide(BigDecimal.Parse("0.200", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("25"));
            TestFmwk.assertTrue("div367", ((BigDecimal.Parse("5.0", numberStyle, provider)).Divide(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.5"));
            TestFmwk.assertTrue("div368", ((BigDecimal.Parse("5.0", numberStyle, provider)).Divide(BigDecimal.Parse("2.0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.5"));
            TestFmwk.assertTrue("div369", ((BigDecimal.Parse("5.0", numberStyle, provider)).Divide(BigDecimal.Parse("2.000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.5"));
            TestFmwk.assertTrue("div370", ((BigDecimal.Parse("5.0", numberStyle, provider)).Divide(BigDecimal.Parse("0.200", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("25.0"));
            TestFmwk.assertTrue("div371", ((BigDecimal.Parse("999999999", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("999999999"));
            TestFmwk.assertTrue("div372", ((BigDecimal.Parse("999999999.4", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("999999999.4"));
            TestFmwk.assertTrue("div373", ((BigDecimal.Parse("999999999.5", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("999999999.5"));
            TestFmwk.assertTrue("div374", ((BigDecimal.Parse("999999999.9", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("999999999.9"));
            TestFmwk.assertTrue("div375", ((BigDecimal.Parse("999999999.999", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("999999999.999"));
            TestFmwk.assertTrue("div376", ((BigDecimal.Parse("0.0000E-5", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div377", ((BigDecimal.Parse("0.000000000", numberStyle, provider)).Divide(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.000000000"));

            // - Fixed point; explicit scales & rounds [old BigDecimal divides]
            rhu = MathContext.RoundHalfUp;
            rd = MathContext.RoundDown;
            TestFmwk.assertTrue("div001", ((BigDecimal.Parse("0", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div002", ((BigDecimal.Parse("0", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div003", ((BigDecimal.Parse("0", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 0, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div004", ((BigDecimal.Parse("0", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 1, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("div005", ((BigDecimal.Parse("0", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 2, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("div006", ((BigDecimal.Parse("0", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 3, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.000"));
            TestFmwk.assertTrue("div007", ((BigDecimal.Parse("0", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.0000"));
            TestFmwk.assertTrue("div008", ((BigDecimal.Parse("1", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div009", ((BigDecimal.Parse("1", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div010", ((BigDecimal.Parse("1", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 0, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div011", ((BigDecimal.Parse("1", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 1, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.3"));
            TestFmwk.assertTrue("div012", ((BigDecimal.Parse("1", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 2, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.33"));
            TestFmwk.assertTrue("div013", ((BigDecimal.Parse("1", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 3, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.333"));
            TestFmwk.assertTrue("div014", ((BigDecimal.Parse("1", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.3333"));
            TestFmwk.assertTrue("div015", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div016", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div017", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 0, rhu).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div018", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 1, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.7"));
            TestFmwk.assertTrue("div019", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 2, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.67"));
            TestFmwk.assertTrue("div020", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 3, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.667"));
            TestFmwk.assertTrue("div021", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.6667"));

            TestFmwk.assertTrue("div030", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("2000", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.5000"));
            TestFmwk.assertTrue("div031", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("2000", numberStyle, provider), 3, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.500"));
            TestFmwk.assertTrue("div032", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("2000", numberStyle, provider), 2, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.50"));
            TestFmwk.assertTrue("div033", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("2000", numberStyle, provider), 1, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            TestFmwk.assertTrue("div034", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("2000", numberStyle, provider), 0, rhu).ToString(CultureInfo.InvariantCulture)).Equals("1"));

            TestFmwk.assertTrue("div035", ((BigDecimal.Parse("100", numberStyle, provider)).Divide(BigDecimal.Parse("5000", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.0200"));
            TestFmwk.assertTrue("div036", ((BigDecimal.Parse("100", numberStyle, provider)).Divide(BigDecimal.Parse("5000", numberStyle, provider), 3, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.020"));
            TestFmwk.assertTrue("div037", ((BigDecimal.Parse("100", numberStyle, provider)).Divide(BigDecimal.Parse("5000", numberStyle, provider), 2, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.02"));
            TestFmwk.assertTrue("div038", ((BigDecimal.Parse("100", numberStyle, provider)).Divide(BigDecimal.Parse("5000", numberStyle, provider), 1, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("div039", ((BigDecimal.Parse("100", numberStyle, provider)).Divide(BigDecimal.Parse("5000", numberStyle, provider), 0, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("div040", ((BigDecimal.Parse("9.99999999", numberStyle, provider)).Divide(BigDecimal.Parse("9.77777777", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("1.0227"));
            TestFmwk.assertTrue("div041", ((BigDecimal.Parse("9.9999999", numberStyle, provider)).Divide(BigDecimal.Parse("9.7777777", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("1.0227"));
            TestFmwk.assertTrue("div042", ((BigDecimal.Parse("9.999999", numberStyle, provider)).Divide(BigDecimal.Parse("9.777777", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("1.0227"));
            TestFmwk.assertTrue("div043", ((BigDecimal.Parse("9.77777777", numberStyle, provider)).Divide(BigDecimal.Parse("9.99999999", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.9778"));
            TestFmwk.assertTrue("div044", ((BigDecimal.Parse("9.7777777", numberStyle, provider)).Divide(BigDecimal.Parse("9.9999999", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.9778"));
            TestFmwk.assertTrue("div045", ((BigDecimal.Parse("9.777777", numberStyle, provider)).Divide(BigDecimal.Parse("9.999999", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.9778"));
            TestFmwk.assertTrue("div046", ((BigDecimal.Parse("9.77777", numberStyle, provider)).Divide(BigDecimal.Parse("9.99999", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.9778"));
            TestFmwk.assertTrue("div047", ((BigDecimal.Parse("9.7777", numberStyle, provider)).Divide(BigDecimal.Parse("9.9999", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.9778"));
            TestFmwk.assertTrue("div048", ((BigDecimal.Parse("9.777", numberStyle, provider)).Divide(BigDecimal.Parse("9.999", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.9778"));
            TestFmwk.assertTrue("div049", ((BigDecimal.Parse("9.77", numberStyle, provider)).Divide(BigDecimal.Parse("9.99", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.9780"));
            TestFmwk.assertTrue("div050", ((BigDecimal.Parse("9.7", numberStyle, provider)).Divide(BigDecimal.Parse("9.9", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.9798"));
            TestFmwk.assertTrue("div051", ((BigDecimal.Parse("9.", numberStyle, provider)).Divide(BigDecimal.Parse("9.", numberStyle, provider), 4, rhu).ToString(CultureInfo.InvariantCulture)).Equals("1.0000"));

            TestFmwk.assertTrue("div060", ((BigDecimal.Parse("9.99999999", numberStyle, provider)).Divide(BigDecimal.Parse("9.77777777", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("1.02272727"));
            TestFmwk.assertTrue("div061", ((BigDecimal.Parse("9.9999999", numberStyle, provider)).Divide(BigDecimal.Parse("9.7777777", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("1.0227273"));
            TestFmwk.assertTrue("div062", ((BigDecimal.Parse("9.999999", numberStyle, provider)).Divide(BigDecimal.Parse("9.777777", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("1.022727"));
            TestFmwk.assertTrue("div063", ((BigDecimal.Parse("9.77777777", numberStyle, provider)).Divide(BigDecimal.Parse("9.99999999", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.97777778"));
            TestFmwk.assertTrue("div064", ((BigDecimal.Parse("9.7777777", numberStyle, provider)).Divide(BigDecimal.Parse("9.9999999", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.9777778"));
            TestFmwk.assertTrue("div065", ((BigDecimal.Parse("9.777777", numberStyle, provider)).Divide(BigDecimal.Parse("9.999999", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.977778"));
            TestFmwk.assertTrue("div066", ((BigDecimal.Parse("9.77777", numberStyle, provider)).Divide(BigDecimal.Parse("9.99999", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.97778"));
            TestFmwk.assertTrue("div067", ((BigDecimal.Parse("9.7777", numberStyle, provider)).Divide(BigDecimal.Parse("9.9999", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.9778"));
            TestFmwk.assertTrue("div068", ((BigDecimal.Parse("9.777", numberStyle, provider)).Divide(BigDecimal.Parse("9.999", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.978"));
            TestFmwk.assertTrue("div069", ((BigDecimal.Parse("9.77", numberStyle, provider)).Divide(BigDecimal.Parse("9.99", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("0.98"));
            TestFmwk.assertTrue("div070", ((BigDecimal.Parse("9.7", numberStyle, provider)).Divide(BigDecimal.Parse("9.9", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("div071", ((BigDecimal.Parse("9.", numberStyle, provider)).Divide(BigDecimal.Parse("9.", numberStyle, provider), rhu).ToString(CultureInfo.InvariantCulture)).Equals("1"));

            rd = MathContext.RoundDown; // test this is actually being used
            TestFmwk.assertTrue("div080", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 0, rd).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div081", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 1, rd).ToString(CultureInfo.InvariantCulture)).Equals("0.6"));
            TestFmwk.assertTrue("div082", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 2, rd).ToString(CultureInfo.InvariantCulture)).Equals("0.66"));
            TestFmwk.assertTrue("div083", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 3, rd).ToString(CultureInfo.InvariantCulture)).Equals("0.666"));
            TestFmwk.assertTrue("div084", ((BigDecimal.Parse("2", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), 4, rd).ToString(CultureInfo.InvariantCulture)).Equals("0.6666"));

            ru = MathContext.RoundUnnecessary; // check for some 0 residues
            TestFmwk.assertTrue("div090", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("5", numberStyle, provider), 4, ru).ToString(CultureInfo.InvariantCulture)).Equals("200.0000"));
            TestFmwk.assertTrue("div091", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("50", numberStyle, provider), 4, ru).ToString(CultureInfo.InvariantCulture)).Equals("20.0000"));
            TestFmwk.assertTrue("div092", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("500", numberStyle, provider), 4, ru).ToString(CultureInfo.InvariantCulture)).Equals("2.0000"));
            TestFmwk.assertTrue("div093", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("5000", numberStyle, provider), 4, ru).ToString(CultureInfo.InvariantCulture)).Equals("0.2000"));
            TestFmwk.assertTrue("div094", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("5000", numberStyle, provider), 3, ru).ToString(CultureInfo.InvariantCulture)).Equals("0.200"));
            TestFmwk.assertTrue("div095", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("5000", numberStyle, provider), 2, ru).ToString(CultureInfo.InvariantCulture)).Equals("0.20"));
            TestFmwk.assertTrue("div096", ((BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("5000", numberStyle, provider), 1, ru).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));

            // check rounding explicitly
            TestFmwk.assertTrue("div101", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 2, MathContext.RoundCeiling).ToString(CultureInfo.InvariantCulture)).Equals("0.06"));
            TestFmwk.assertTrue("div102", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 1, MathContext.RoundCeiling).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("div103", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 0, MathContext.RoundCeiling).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("div104", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 2, MathContext.RoundDown).ToString(CultureInfo.InvariantCulture)).Equals("0.05"));
            TestFmwk.assertTrue("div105", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 1, MathContext.RoundDown).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("div106", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 0, MathContext.RoundDown).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div107", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 2, MathContext.RoundFloor).ToString(CultureInfo.InvariantCulture)).Equals("0.05"));
            TestFmwk.assertTrue("div108", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 1, MathContext.RoundFloor).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("div109", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 0, MathContext.RoundFloor).ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("div110", ((BigDecimal.Parse("0.045", numberStyle, provider)).Divide(one, 2, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0.04"));
            TestFmwk.assertTrue("div111", ((BigDecimal.Parse("0.045", numberStyle, provider)).Divide(one, 1, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("div112", ((BigDecimal.Parse("0.045", numberStyle, provider)).Divide(one, 0, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div113", ((BigDecimal.Parse("0.050", numberStyle, provider)).Divide(one, 2, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0.05"));
            TestFmwk.assertTrue("div114", ((BigDecimal.Parse("0.050", numberStyle, provider)).Divide(one, 1, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("div115", ((BigDecimal.Parse("0.050", numberStyle, provider)).Divide(one, 0, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div116", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 2, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0.05"));
            TestFmwk.assertTrue("div117", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 1, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("div118", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 0, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("div120", ((BigDecimal.Parse("0.045", numberStyle, provider)).Divide(one, 2, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.04"));
            TestFmwk.assertTrue("div121", ((BigDecimal.Parse("0.045", numberStyle, provider)).Divide(one, 1, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("div122", ((BigDecimal.Parse("0.045", numberStyle, provider)).Divide(one, 0, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div123", ((BigDecimal.Parse("0.050", numberStyle, provider)).Divide(one, 2, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.05"));
            TestFmwk.assertTrue("div124", ((BigDecimal.Parse("0.050", numberStyle, provider)).Divide(one, 1, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("div125", ((BigDecimal.Parse("0.050", numberStyle, provider)).Divide(one, 0, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div126", ((BigDecimal.Parse("0.150", numberStyle, provider)).Divide(one, 2, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.15"));
            TestFmwk.assertTrue("div127", ((BigDecimal.Parse("0.150", numberStyle, provider)).Divide(one, 1, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));
            TestFmwk.assertTrue("div128", ((BigDecimal.Parse("0.150", numberStyle, provider)).Divide(one, 0, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div129", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 2, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.06"));
            TestFmwk.assertTrue("div130", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 1, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("div131", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 0, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("div140", ((BigDecimal.Parse("0.045", numberStyle, provider)).Divide(one, 2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.05"));
            TestFmwk.assertTrue("div141", ((BigDecimal.Parse("0.045", numberStyle, provider)).Divide(one, 1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("div142", ((BigDecimal.Parse("0.045", numberStyle, provider)).Divide(one, 0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div143", ((BigDecimal.Parse("0.050", numberStyle, provider)).Divide(one, 2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.05"));
            TestFmwk.assertTrue("div144", ((BigDecimal.Parse("0.050", numberStyle, provider)).Divide(one, 1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("div145", ((BigDecimal.Parse("0.050", numberStyle, provider)).Divide(one, 0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("div146", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.06"));
            TestFmwk.assertTrue("div147", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("div148", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("div150", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 2, MathContext.RoundUp).ToString(CultureInfo.InvariantCulture)).Equals("0.06"));
            TestFmwk.assertTrue("div151", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 1, MathContext.RoundUp).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("div52.", ((BigDecimal.Parse("0.055", numberStyle, provider)).Divide(one, 0, MathContext.RoundUp).ToString(CultureInfo.InvariantCulture)).Equals("1"));

            // - error conditions ---
            try
            {
                ten.Divide((BigDecimal)null);
                flag = false;
            }
            catch (ArgumentNullException e32)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("div201", flag);
            try
            {
                ten.Divide(ten, (MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e33)
            {
                flag = true;
            }/* checknull2 */
            TestFmwk.assertTrue("div202", flag);

            try
            {
                (BigDecimal.Parse("1", numberStyle, provider)).Divide(BigDecimal.Parse("3", numberStyle, provider), -8, 0);
                flag = false;
            }
            catch (ArgumentOutOfRangeException e34)
            {
                e = e34;
                flag = flag & (e.Message).StartsWith("Negative scale: -8", StringComparison.Ordinal);
            }/* checkscale */
            TestFmwk.assertTrue("div203", flag);

            try
            {
                (BigDecimal.Parse("1000", numberStyle, provider)).Divide(BigDecimal.Parse("5000", numberStyle, provider), 0, MathContext.RoundUnnecessary);
                flag = false;
            }
            catch (ArithmeticException e35)
            {
                ae = e35;
                flag = (ae.Message).Equals("Rounding necessary");
            }/* rounn */
            TestFmwk.assertTrue("div204", flag);
            try
            {
                (BigDecimal.Parse("1001", numberStyle, provider)).Divide(BigDecimal.Parse("10", numberStyle, provider), 0, MathContext.RoundUnnecessary);
                flag = false;
            }
            catch (ArithmeticException e36)
            {
                ae = e36;
                flag = (ae.Message).Equals("Rounding necessary");
            }/* rounn */
            TestFmwk.assertTrue("div205", flag);
            try
            {
                (BigDecimal.Parse("1001", numberStyle, provider)).Divide(BigDecimal.Parse("100", numberStyle, provider), 1, MathContext.RoundUnnecessary);
                flag = false;
            }
            catch (ArithmeticException e37)
            {
                ae = e37;
                flag = (ae.Message).Equals("Rounding necessary");
            }/* rounn */
            TestFmwk.assertTrue("div206", flag);
            try
            {
                (BigDecimal.Parse("10001", numberStyle, provider)).Divide(
                        BigDecimal.Parse("10000", numberStyle, provider), 1,
                        MathContext.RoundUnnecessary);
                flag = false;
            }
            catch (ArithmeticException e38)
            {
                ae = e38;
                flag = (ae.Message).Equals("Rounding necessary");
            }/* rounn */
            TestFmwk.assertTrue("div207", flag);
            try
            {
                (BigDecimal.Parse("1.0001", numberStyle, provider)).Divide(
                        BigDecimal.Parse("1", numberStyle, provider), 1,
                        MathContext.RoundUnnecessary);
                flag = false;
            }
            catch (ArithmeticException e39)
            {
                ae = e39;
                flag = (ae.Message).Equals("Rounding necessary");
            }/* rounn */
            TestFmwk.assertTrue("div208", flag);

            try
            {
                (BigDecimal.Parse("5", numberStyle, provider))
                        .Divide(BigDecimal.Parse("0.00", numberStyle, provider));
                flag = false;
            }
            catch (ArithmeticException e40)
            {
                ae = e40;
                flag = (ae.Message).Equals("Divide by 0");
            }/* div0 */
            TestFmwk.assertTrue("div209", flag);

            try
            {
                tenlong.Divide(BigDecimal.One, mcld);
                flag = false;
            }
            catch (ArithmeticException e41)
            {
                ae = e41;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("div210", flag);
            try
            {
                BigDecimal.One.Divide(tenlong, mcld);
                flag = false;
            }
            catch (ArithmeticException e42)
            {
                ae = e42;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("div211", flag);

        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#divideInteger} method. */

        [Test]
        public void diagdivideInteger()
        {
            bool flag = false;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("dvI001", ((BigDecimal.Parse("101.3", numberStyle, provider)).DivideInteger(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("101"));
            TestFmwk.assertTrue("dvI002", ((BigDecimal.Parse("101.0", numberStyle, provider)).DivideInteger(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("101"));
            TestFmwk.assertTrue("dvI003", ((BigDecimal.Parse("101.3", numberStyle, provider)).DivideInteger(BigDecimal.Parse("3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("33"));
            TestFmwk.assertTrue("dvI004", ((BigDecimal.Parse("101.0", numberStyle, provider)).DivideInteger(BigDecimal.Parse("3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("33"));
            TestFmwk.assertTrue("dvI005", ((BigDecimal.Parse("2.4", numberStyle, provider)).DivideInteger(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("dvI006", ((BigDecimal.Parse("2.400", numberStyle, provider)).DivideInteger(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("dvI007", ((BigDecimal.Parse("18", numberStyle, provider)).DivideInteger(BigDecimal.Parse("18", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("dvI008", ((BigDecimal.Parse("1120", numberStyle, provider)).DivideInteger(BigDecimal.Parse("1000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("dvI009", ((BigDecimal.Parse("2.4", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("dvI010", ((BigDecimal.Parse("2.400", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("dvI011", ((BigDecimal.Parse("0.5", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2.000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("dvI012", ((BigDecimal.Parse("8.005", numberStyle, provider)).DivideInteger(BigDecimal.Parse("7", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("dvI013", ((BigDecimal.Parse("5", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("dvI014", ((BigDecimal.Parse("0", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("dvI015", ((BigDecimal.Parse("0.00", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            // MC
            TestFmwk.assertTrue("dvI016", ((BigDecimal.Parse("5", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("dvI017", ((BigDecimal.Parse("5", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("2"));

            // Fixed --
            TestFmwk.assertTrue("dvI021", ((BigDecimal.Parse("101.3", numberStyle, provider)).DivideInteger(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("101"));
            TestFmwk.assertTrue("dvI022", ((BigDecimal.Parse("101.0", numberStyle, provider)).DivideInteger(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("101"));
            TestFmwk.assertTrue("dvI023", ((BigDecimal.Parse("101.3", numberStyle, provider)).DivideInteger(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("33"));
            TestFmwk.assertTrue("dvI024", ((BigDecimal.Parse("101.0", numberStyle, provider)).DivideInteger(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("33"));
            TestFmwk.assertTrue("dvI025", ((BigDecimal.Parse("2.4", numberStyle, provider)).DivideInteger(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("dvI026", ((BigDecimal.Parse("2.400", numberStyle, provider)).DivideInteger(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("dvI027", ((BigDecimal.Parse("18", numberStyle, provider)).DivideInteger(BigDecimal.Parse("18", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("dvI028", ((BigDecimal.Parse("1120", numberStyle, provider)).DivideInteger(BigDecimal.Parse("1000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("dvI029", ((BigDecimal.Parse("2.4", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("dvI030", ((BigDecimal.Parse("2.400", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("dvI031", ((BigDecimal.Parse("0.5", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2.000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("dvI032", ((BigDecimal.Parse("8.005", numberStyle, provider)).DivideInteger(BigDecimal.Parse("7", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("dvI033", ((BigDecimal.Parse("5", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("dvI034", ((BigDecimal.Parse("0", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("dvI035", ((BigDecimal.Parse("0.00", numberStyle, provider)).DivideInteger(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));

            try
            {
                ten.DivideInteger((BigDecimal)null);
                flag = false;
            }
            catch (ArgumentNullException e43)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("dvI101", flag);
            try
            {
                ten.DivideInteger(ten, (MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e44)
            {
                flag = true;
            }/* checknull2 */
            TestFmwk.assertTrue("dvI102", flag);

            try
            {
                BigDecimal.One.DivideInteger(tenlong, mcld);
                flag = false;
            }
            catch (ArithmeticException e45)
            {
                ae = e45;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("dvI103", flag);

            try
            {
                tenlong.DivideInteger(BigDecimal.One, mcld);
                flag = false;
            }
            catch (ArithmeticException e46)
            {
                ae = e46;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("dvI104", flag);

        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#max} method. */

        [Test]
        public void diagmax()
        {
            bool flag = false;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            // we assume add/subtract test function; this and min just
            // test existence and test the truth table
            TestFmwk.assertTrue("max001", ((BigDecimal.Parse("5", numberStyle, provider)).Max(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("5"));
            TestFmwk.assertTrue("max002", ((BigDecimal.Parse("5", numberStyle, provider)).Max(BigDecimal.Parse("5", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("5"));
            TestFmwk.assertTrue("max003", ((BigDecimal.Parse("2", numberStyle, provider)).Max(BigDecimal.Parse("7", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("7"));
            TestFmwk.assertTrue("max004", ((BigDecimal.Parse("2", numberStyle, provider)).Max(BigDecimal.Parse("7", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("7"));
            TestFmwk.assertTrue("max005", ((BigDecimal.Parse("2", numberStyle, provider)).Max(BigDecimal.Parse("7", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("7"));
            TestFmwk.assertTrue("max006", ((BigDecimal.Parse("2E+3", numberStyle, provider)).Max(BigDecimal.Parse("7", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2000"));
            TestFmwk.assertTrue("max007", ((BigDecimal.Parse("2E+3", numberStyle, provider)).Max(BigDecimal.Parse("7", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("2E+3"));
            TestFmwk.assertTrue("max008", ((BigDecimal.Parse("7", numberStyle, provider)).Max(BigDecimal.Parse("2E+3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2000"));
            TestFmwk.assertTrue("max009", ((BigDecimal.Parse("7", numberStyle, provider)).Max(BigDecimal.Parse("2E+3", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("2E+3"));
            try
            {
                ten.Max((BigDecimal)null);
                flag = false;
            }
            catch (ArgumentNullException e47)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("max010", flag);
            try
            {
                ten.Max(ten, (MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e48)
            {
                flag = true;
            }/* checknull2 */
            TestFmwk.assertTrue("max011", flag);
            try
            {
                tenlong.Max(BigDecimal.One, mcld);
                flag = false;
            }
            catch (ArithmeticException e49)
            {
                ae = e49;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("max012", flag);
            try
            {
                BigDecimal.One.Max(tenlong, mcld);
                flag = false;
            }
            catch (ArithmeticException e50)
            {
                ae = e50;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("max013", flag);
        }

        /** Test the {@link BigDecimal#min} method. */

        [Test]
        public void diagmin()
        {
            bool flag = false;
            BigDecimal minx = null;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;
            // we assume add/subtract test function; this and max just
            // test existence and test the truth table

            TestFmwk.assertTrue("min001", ((BigDecimal.Parse("5", numberStyle, provider)).Min(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("min002", ((BigDecimal.Parse("5", numberStyle, provider)).Min(BigDecimal.Parse("5", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("5"));
            TestFmwk.assertTrue("min003", ((BigDecimal.Parse("2", numberStyle, provider)).Min(BigDecimal.Parse("7", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("min004", ((BigDecimal.Parse("2", numberStyle, provider)).Min(BigDecimal.Parse("7", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("min005", ((BigDecimal.Parse("1", numberStyle, provider)).Min(BigDecimal.Parse("7", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("min006", ((BigDecimal.Parse("-2E+3", numberStyle, provider)).Min(BigDecimal.Parse("7", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-2000"));
            TestFmwk.assertTrue("min007", ((BigDecimal.Parse("-2E+3", numberStyle, provider)).Min(BigDecimal.Parse("7", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("-2E+3"));
            TestFmwk.assertTrue("min008", ((BigDecimal.Parse("7", numberStyle, provider)).Min(BigDecimal.Parse("-2E+3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-2000"));
            TestFmwk.assertTrue("min009", ((BigDecimal.Parse("7", numberStyle, provider)).Min(BigDecimal.Parse("-2E+3", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("-2E+3"));
            try
            {
                minx = ten;
                minx.Min((BigDecimal)null);
                flag = false;
            }
            catch (ArgumentNullException e51)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("min010", flag);
            try
            {
                minx = ten;
                minx.Min(ten, (MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e52)
            {
                flag = true;
            }/* checknull2 */
            TestFmwk.assertTrue("min011", flag);

            try
            {
                tenlong.Min(BigDecimal.One, mcld);
                flag = false;
            }
            catch (ArithmeticException e53)
            {
                ae = e53;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("min012", flag);
            try
            {
                (new BigDecimal(9)).Min(tenlong, mcld);
                flag = false;
            }
            catch (ArithmeticException e54)
            {
                ae = e54;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("min013", flag);
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#multiply} method. */

        [Test]
        public void diagmultiply()
        {
            bool flag = false;
            BigDecimal l9;
            BigDecimal l77e;
            BigDecimal l12345;
            BigDecimal edge;
            BigDecimal tenedge;
            BigDecimal hunedge;
            BigDecimal opo;
            BigDecimal d1 = null;
            BigDecimal d2 = null;
            ArithmeticException oe = null;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("mul001", ((BigDecimal.Parse("2", numberStyle, provider)).Multiply(BigDecimal.Parse("3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("6"));
            TestFmwk.assertTrue("mul002", ((BigDecimal.Parse("5", numberStyle, provider)).Multiply(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("5"));
            TestFmwk.assertTrue("mul003", ((BigDecimal.Parse("5", numberStyle, provider)).Multiply(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("mul004", ((BigDecimal.Parse("1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.40"));
            TestFmwk.assertTrue("mul005", ((BigDecimal.Parse("1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("mul006", ((BigDecimal.Parse("1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("-2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-2.40"));
            TestFmwk.assertTrue("mul007", ((BigDecimal.Parse("-1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-2.40"));
            TestFmwk.assertTrue("mul008", ((BigDecimal.Parse("-1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("mul009", ((BigDecimal.Parse("-1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("-2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.40"));
            TestFmwk.assertTrue("mul010", ((BigDecimal.Parse("5.09", numberStyle, provider)).Multiply(BigDecimal.Parse("7.1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("36.139"));
            TestFmwk.assertTrue("mul011", ((BigDecimal.Parse("2.5", numberStyle, provider)).Multiply(BigDecimal.Parse("4", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("10.0"));
            TestFmwk.assertTrue("mul012", ((BigDecimal.Parse("2.50", numberStyle, provider)).Multiply(BigDecimal.Parse("4", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("10.00"));
            TestFmwk.assertTrue("mul013", ((BigDecimal.Parse("1.23456789", numberStyle, provider)).Multiply(BigDecimal.Parse("1.00000000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.23456789"));

            TestFmwk.assertTrue("mul014", ((BigDecimal.Parse("9.999999999", numberStyle, provider)).Multiply(BigDecimal.Parse("9.999999999", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("100.000000"));

            TestFmwk.assertTrue("mul015", ((BigDecimal.Parse("2.50", numberStyle, provider)).Multiply(BigDecimal.Parse("4", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("10.00"));
            TestFmwk.assertTrue("mul016", ((BigDecimal.Parse("2.50", numberStyle, provider)).Multiply(BigDecimal.Parse("4", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("10.00"));
            TestFmwk.assertTrue("mul017", ((BigDecimal.Parse("9.999999999", numberStyle, provider)).Multiply(BigDecimal.Parse("9.999999999", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("100.000"));


            TestFmwk.assertTrue("mul020", ((BigDecimal.Parse("2", numberStyle, provider)).Multiply(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("6"));
            TestFmwk.assertTrue("mul021", ((BigDecimal.Parse("5", numberStyle, provider)).Multiply(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("5"));
            TestFmwk.assertTrue("mul022", ((BigDecimal.Parse("5", numberStyle, provider)).Multiply(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("mul023", ((BigDecimal.Parse("1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.40"));
            TestFmwk.assertTrue("mul024", ((BigDecimal.Parse("1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("mul025", ((BigDecimal.Parse("1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("-2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-2.40"));
            TestFmwk.assertTrue("mul026", ((BigDecimal.Parse("-1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-2.40"));
            TestFmwk.assertTrue("mul027", ((BigDecimal.Parse("-1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("mul028", ((BigDecimal.Parse("-1.20", numberStyle, provider)).Multiply(BigDecimal.Parse("-2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.40"));
            TestFmwk.assertTrue("mul029", ((BigDecimal.Parse("5.09", numberStyle, provider)).Multiply(BigDecimal.Parse("7.1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("36.139"));
            TestFmwk.assertTrue("mul030", ((BigDecimal.Parse("2.5", numberStyle, provider)).Multiply(BigDecimal.Parse("4", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10.0"));
            TestFmwk.assertTrue("mul031", ((BigDecimal.Parse("2.50", numberStyle, provider)).Multiply(BigDecimal.Parse("4", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10.00"));
            TestFmwk.assertTrue("mul032", ((BigDecimal.Parse("1.23456789", numberStyle, provider)).Multiply(BigDecimal.Parse("1.00000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1.2345678900000000"));

            TestFmwk.assertTrue("mul033", ((BigDecimal.Parse("1234.56789", numberStyle, provider)).Multiply(BigDecimal.Parse("-1000.00000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-1234567.8900000000"));

            TestFmwk.assertTrue("mul034", ((BigDecimal.Parse("-1234.56789", numberStyle, provider)).Multiply(BigDecimal.Parse("1000.00000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-1234567.8900000000"));

            TestFmwk.assertTrue("mul035", ((BigDecimal.Parse("9.999999999", numberStyle, provider)).Multiply(BigDecimal.Parse("9.999999999", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("99.999999980000000001"));

            TestFmwk.assertTrue("mul036", ((BigDecimal.Parse("5.00", numberStyle, provider)).Multiply(BigDecimal.Parse("1E-3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00500"));
            TestFmwk.assertTrue("mul037", ((BigDecimal.Parse("00.00", numberStyle, provider)).Multiply(BigDecimal.Parse("0.000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00000"));
            TestFmwk.assertTrue("mul038", ((BigDecimal.Parse("00.00", numberStyle, provider)).Multiply(BigDecimal.Parse("0E-3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00")); // rhs is '0'
                                                                                                                                                               // 1999.12.21: next one is a edge case if intermediate longs are used
            TestFmwk.assertTrue("mul039", ((BigDecimal.Parse("999999999999", numberStyle, provider)).Multiply(BigDecimal.Parse("9765625", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9765624999990234375"));

            l9 = BigDecimal.Parse("123456789E+10", numberStyle, provider);
            l77e = BigDecimal.Parse("77E-20", numberStyle, provider);
            TestFmwk.assertTrue("mul040", (l9.Multiply(BigDecimal.Parse("3456757", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("4267601195732730000000000"));
            TestFmwk.assertTrue("mul041", (l9.Multiply(BigDecimal.Parse("3456757", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("4.26E+24"));
            TestFmwk.assertTrue("mul042", (l9.Multiply(l77e).ToString(CultureInfo.InvariantCulture)).Equals("0.95061727530000000000"));
            TestFmwk.assertTrue("mul043", (l9.Multiply(l77e, mc3).ToString(CultureInfo.InvariantCulture)).Equals("0.947"));
            TestFmwk.assertTrue("mul044", (l77e.Multiply(l9, mc3).ToString(CultureInfo.InvariantCulture)).Equals("0.947"));

            l12345 = BigDecimal.Parse("123.45", numberStyle, provider);
            TestFmwk.assertTrue("mul050", (l12345.Multiply(BigDecimal.Parse("1e11", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.2345E+13"));
            TestFmwk.assertTrue("mul051", (l12345.Multiply(BigDecimal.Parse("1e11", numberStyle, provider), mcs).ToString(CultureInfo.InvariantCulture)).Equals("1.2345E+13"));
            TestFmwk.assertTrue("mul052", (l12345.Multiply(BigDecimal.Parse("1e+9", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("123.45E+9"));
            TestFmwk.assertTrue("mul053", (l12345.Multiply(BigDecimal.Parse("1e10", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("1.2345E+12"));
            TestFmwk.assertTrue("mul054", (l12345.Multiply(BigDecimal.Parse("1e11", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("12.345E+12"));
            TestFmwk.assertTrue("mul055", (l12345.Multiply(BigDecimal.Parse("1e12", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("123.45E+12"));
            TestFmwk.assertTrue("mul056", (l12345.Multiply(BigDecimal.Parse("1e13", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("1.2345E+15"));

            // test some cases that are close to exponent overflow
            TestFmwk.assertTrue("mul060", (one.Multiply(BigDecimal.Parse("9e999999999", numberStyle, provider), mcs).ToString(CultureInfo.InvariantCulture)).Equals("9E+999999999"));
            TestFmwk.assertTrue("mul061", (one.Multiply(BigDecimal.Parse("9.9e999999999", numberStyle, provider), mcs).ToString(CultureInfo.InvariantCulture)).Equals("9.9E+999999999"));
            TestFmwk.assertTrue("mul062", (one.Multiply(BigDecimal.Parse("9.99e999999999", numberStyle, provider), mcs).ToString(CultureInfo.InvariantCulture)).Equals("9.99E+999999999"));
            TestFmwk.assertTrue("mul063", (ten.Multiply(BigDecimal.Parse("9e999999999", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("90E+999999999"));
            TestFmwk.assertTrue("mul064", (ten.Multiply(BigDecimal.Parse("9.9e999999999", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("99.0E+999999999"));
            edge = BigDecimal.Parse("9.999e999999999", numberStyle, provider);
            tenedge = ten.Multiply(edge, mce);
            TestFmwk.assertTrue("mul065", (tenedge.ToString(CultureInfo.InvariantCulture)).Equals("99.990E+999999999"));
            hunedge = ten.Multiply(tenedge, mce);
            TestFmwk.assertTrue("mul066", (hunedge.ToString(CultureInfo.InvariantCulture)).Equals("999.900E+999999999"));
            opo = BigDecimal.Parse("0.1", numberStyle, provider); // one tenth
            TestFmwk.assertTrue("mul067", (opo.Multiply(BigDecimal.Parse("9e-999999998", numberStyle, provider), mcs).ToString(CultureInfo.InvariantCulture)).Equals("9E-999999999"));
            TestFmwk.assertTrue("mul068", (opo.Multiply(BigDecimal.Parse("99e-999999998", numberStyle, provider), mcs).ToString(CultureInfo.InvariantCulture)).Equals("9.9E-999999998"));
            TestFmwk.assertTrue("mul069", (opo.Multiply(BigDecimal.Parse("999e-999999998", numberStyle, provider), mcs).ToString(CultureInfo.InvariantCulture)).Equals("9.99E-999999997"));

            TestFmwk.assertTrue("mul070", (opo.Multiply(BigDecimal.Parse("9e-999999998", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("9E-999999999"));
            TestFmwk.assertTrue("mul071", (opo.Multiply(BigDecimal.Parse("99e-999999998", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("99E-999999999"));
            TestFmwk.assertTrue("mul072", (opo.Multiply(BigDecimal.Parse("999e-999999998", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("999E-999999999"));
            TestFmwk.assertTrue("mul073", (opo.Multiply(BigDecimal.Parse("999e-999999997", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("9.99E-999999996"));
            TestFmwk.assertTrue("mul074", (opo.Multiply(BigDecimal.Parse("9999e-999999997", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("99.99E-999999996"));
            TestFmwk.assertTrue("mul074", (opo.Multiply(BigDecimal.Parse("99999e-999999997", numberStyle, provider), mce).ToString(CultureInfo.InvariantCulture)).Equals("999.99E-999999996"));

            // test some intermediate lengths
            TestFmwk.assertTrue("mul080", (opo.Multiply(BigDecimal.Parse("123456789", numberStyle, provider), mcs).ToString(CultureInfo.InvariantCulture)).Equals("12345678.9"));
            TestFmwk.assertTrue("mul081", (opo.Multiply(BigDecimal.Parse("12345678901234", numberStyle, provider), mcs).ToString(CultureInfo.InvariantCulture)).Equals("1.23456789E+12"));
            TestFmwk.assertTrue("mul082", (opo.Multiply(BigDecimal.Parse("123456789123456789", numberStyle, provider), mcs).ToString(CultureInfo.InvariantCulture)).Equals("1.23456789E+16"));
            TestFmwk.assertTrue("mul083", (opo.Multiply(BigDecimal.Parse("123456789", numberStyle, provider), mcfd).ToString(CultureInfo.InvariantCulture)).Equals("12345678.9"));
            TestFmwk.assertTrue("mul084", (opo.Multiply(BigDecimal.Parse("12345678901234", numberStyle, provider), mcfd).ToString(CultureInfo.InvariantCulture)).Equals("1234567890123.4"));
            TestFmwk.assertTrue("mul085", (opo.Multiply(BigDecimal.Parse("123456789123456789", numberStyle, provider), mcfd).ToString(CultureInfo.InvariantCulture)).Equals("12345678912345678.9"));

            TestFmwk.assertTrue("mul090", ((BigDecimal.Parse("123456789", numberStyle, provider)).Multiply(opo, mcs).ToString(CultureInfo.InvariantCulture)).Equals("12345678.9"));
            TestFmwk.assertTrue("mul091", ((BigDecimal.Parse("12345678901234", numberStyle, provider)).Multiply(opo, mcs).ToString(CultureInfo.InvariantCulture)).Equals("1.23456789E+12"));
            TestFmwk.assertTrue("mul092", ((BigDecimal.Parse("123456789123456789", numberStyle, provider)).Multiply(opo, mcs).ToString(CultureInfo.InvariantCulture)).Equals("1.23456789E+16"));
            TestFmwk.assertTrue("mul093", ((BigDecimal.Parse("123456789", numberStyle, provider)).Multiply(opo, mcfd).ToString(CultureInfo.InvariantCulture)).Equals("12345678.9"));
            TestFmwk.assertTrue("mul094", ((BigDecimal.Parse("12345678901234", numberStyle, provider)).Multiply(opo, mcfd).ToString(CultureInfo.InvariantCulture)).Equals("1234567890123.4"));
            TestFmwk.assertTrue("mul095", ((BigDecimal.Parse("123456789123456789", numberStyle, provider)).Multiply(opo, mcfd).ToString(CultureInfo.InvariantCulture)).Equals("12345678912345678.9"));

            // test some more edge cases and carries
            TestFmwk.assertTrue("mul101", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("81"));
            TestFmwk.assertTrue("mul102", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("90", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("810"));
            TestFmwk.assertTrue("mul103", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("900", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("8100"));
            TestFmwk.assertTrue("mul104", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("9000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("81000"));
            TestFmwk.assertTrue("mul105", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("90000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("810000"));
            TestFmwk.assertTrue("mul106", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("900000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("8100000"));
            TestFmwk.assertTrue("mul107", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("9000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("81000000"));
            TestFmwk.assertTrue("mul108", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("90000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("810000000"));
            TestFmwk.assertTrue("mul109", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("900000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("8100000000"));
            TestFmwk.assertTrue("mul110", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("9000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("81000000000"));
            TestFmwk.assertTrue("mul111", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("90000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("810000000000"));
            TestFmwk.assertTrue("mul112", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("900000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("8100000000000"));
            TestFmwk.assertTrue("mul113", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("9000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("81000000000000"));
            TestFmwk.assertTrue("mul114", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("90000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("810000000000000"));
            TestFmwk.assertTrue("mul115", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("900000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("8100000000000000"));
            TestFmwk.assertTrue("mul116", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("9000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("81000000000000000"));
            TestFmwk.assertTrue("mul117", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("90000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("810000000000000000"));
            TestFmwk.assertTrue("mul118", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("900000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("8100000000000000000"));
            TestFmwk.assertTrue("mul119", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("9000000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("81000000000000000000"));
            TestFmwk.assertTrue("mul120", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("90000000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("810000000000000000000"));
            TestFmwk.assertTrue("mul121", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("900000000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("8100000000000000000000"));
            TestFmwk.assertTrue("mul122", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("9000000000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("81000000000000000000000"));
            TestFmwk.assertTrue("mul123", ((BigDecimal.Parse("9", numberStyle, provider)).Multiply(BigDecimal.Parse("90000000000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("810000000000000000000000"));
            // test some more edge cases without carries
            TestFmwk.assertTrue("mul131", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9"));
            TestFmwk.assertTrue("mul132", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("30", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("90"));
            TestFmwk.assertTrue("mul133", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("300", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("900"));
            TestFmwk.assertTrue("mul134", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("3000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9000"));
            TestFmwk.assertTrue("mul135", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("30000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("90000"));
            TestFmwk.assertTrue("mul136", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("300000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("900000"));
            TestFmwk.assertTrue("mul137", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("3000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9000000"));
            TestFmwk.assertTrue("mul138", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("30000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("90000000"));
            TestFmwk.assertTrue("mul139", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("300000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("900000000"));
            TestFmwk.assertTrue("mul140", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("3000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9000000000"));
            TestFmwk.assertTrue("mul141", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("30000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("90000000000"));
            TestFmwk.assertTrue("mul142", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("300000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("900000000000"));
            TestFmwk.assertTrue("mul143", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("3000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9000000000000"));
            TestFmwk.assertTrue("mul144", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("30000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("90000000000000"));
            TestFmwk.assertTrue("mul145", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("300000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("900000000000000"));
            TestFmwk.assertTrue("mul146", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("3000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9000000000000000"));
            TestFmwk.assertTrue("mul147", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("30000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("90000000000000000"));
            TestFmwk.assertTrue("mul148", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("300000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("900000000000000000"));
            TestFmwk.assertTrue("mul149", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("3000000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9000000000000000000"));
            TestFmwk.assertTrue("mul150", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("30000000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("90000000000000000000"));
            TestFmwk.assertTrue("mul151", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("300000000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("900000000000000000000"));
            TestFmwk.assertTrue("mul152", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("3000000000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9000000000000000000000"));
            TestFmwk.assertTrue("mul153", ((BigDecimal.Parse("3", numberStyle, provider)).Multiply(BigDecimal.Parse("30000000000000000000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("90000000000000000000000"));

            try
            {
                ten.Multiply((BigDecimal)null);
                flag = false;
            }
            catch (ArgumentNullException e55)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("mul200", flag);
            try
            {
                ten.Multiply(ten, (MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e56)
            {
                flag = true;
            }/* checknull2 */
            TestFmwk.assertTrue("mul201", flag);

            try
            {
                d1 = BigDecimal.Parse("-1.23456789012345E-0", numberStyle, provider);
                d2 = BigDecimal.Parse("9E+999999999", numberStyle, provider);
                d1.Multiply(d2, mcdef); // marginal overflow
                flag = false;
            }
            catch (ArithmeticException e57)
            {
                oe = e57;
                flag = (oe.Message).Equals("Exponent Overflow: 1000000000");
            }/* checkover */
            TestFmwk.assertTrue("mul202", flag);
            try
            {
                d1 = BigDecimal.Parse("112", numberStyle, provider);
                d2 = BigDecimal.Parse("9E+999999999", numberStyle, provider);
                d1.Multiply(d2, mce); // marginal overflow, engineering
                flag = false;
            }
            catch (ArithmeticException e58)
            {
                oe = e58;
                flag = (oe.Message).Equals("Exponent Overflow: 1000000002");
            }/* checkover */
            TestFmwk.assertTrue("mul203", flag);

            try
            {
                d1 = BigDecimal.Parse("0.9", numberStyle, provider);
                d2 = BigDecimal.Parse("1E-999999999", numberStyle, provider);
                d1.Multiply(d2, mcdef); // marginal negative overflow
                flag = false;
            }
            catch (ArithmeticException e59)
            {
                oe = e59;
                flag = (oe.Message).Equals("Exponent Overflow: -1000000000");
            }/* checkover */
            TestFmwk.assertTrue("mul204", flag);
            try
            {
                d1 = BigDecimal.Parse("0.9", numberStyle, provider);
                d2 = BigDecimal.Parse("1E-999999999", numberStyle, provider);
                d1.Multiply(d2, mce); // marginal negative overflow,
                                      // engineering
                flag = false;
            }
            catch (ArithmeticException e60)
            {
                oe = e60;
                flag = (oe.Message).Equals("Exponent Overflow: -1000000002");
            }/* checkover */
            TestFmwk.assertTrue("mul205", flag);

            try
            {
                tenlong.Multiply(BigDecimal.One, mcld);
                flag = false;
            }
            catch (ArithmeticException e61)
            {
                ae = e61;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("mul206", flag);
            try
            {
                BigDecimal.Ten.Multiply(tenlong, mcld);
                flag = false;
            }
            catch (ArithmeticException e62)
            {
                ae = e62;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("mul207", flag);

        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#negate} method. */

        [Test]
        public void diagnegate()
        {
            bool flag = false;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("neg001", ((BigDecimal.Parse("2", numberStyle, provider)).Negate().ToString(CultureInfo.InvariantCulture)).Equals("-2"));
            TestFmwk.assertTrue("neg002", ((BigDecimal.Parse("-2", numberStyle, provider)).Negate().ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("neg003", ((BigDecimal.Parse("2.00", numberStyle, provider)).Negate(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-2.00"));
            TestFmwk.assertTrue("neg004", ((BigDecimal.Parse("-2.00", numberStyle, provider)).Negate(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.00"));
            TestFmwk.assertTrue("neg005", ((BigDecimal.Parse("0", numberStyle, provider)).Negate(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("neg006", ((BigDecimal.Parse("0.00", numberStyle, provider)).Negate(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("neg007", ((BigDecimal.Parse("00.0", numberStyle, provider)).Negate(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("neg008", ((BigDecimal.Parse("00", numberStyle, provider)).Negate(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("neg010", ((BigDecimal.Parse("2.00", numberStyle, provider)).Negate().ToString(CultureInfo.InvariantCulture)).Equals("-2.00"));
            TestFmwk.assertTrue("neg011", ((BigDecimal.Parse("-2.00", numberStyle, provider)).Negate().ToString(CultureInfo.InvariantCulture)).Equals("2.00"));
            TestFmwk.assertTrue("neg012", ((BigDecimal.Parse("0", numberStyle, provider)).Negate().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("neg013", ((BigDecimal.Parse("0.00", numberStyle, provider)).Negate().ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("neg014", ((BigDecimal.Parse("00.0", numberStyle, provider)).Negate().ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("neg015", ((BigDecimal.Parse("00.00", numberStyle, provider)).Negate().ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("neg016", ((BigDecimal.Parse("00", numberStyle, provider)).Negate().ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("neg020", ((BigDecimal.Parse("-2000000", numberStyle, provider)).Negate().ToString(CultureInfo.InvariantCulture)).Equals("2000000"));
            TestFmwk.assertTrue("neg021", ((BigDecimal.Parse("-2000000", numberStyle, provider)).Negate(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2000000"));
            TestFmwk.assertTrue("neg022", ((BigDecimal.Parse("-2000000", numberStyle, provider)).Negate(mc6).ToString(CultureInfo.InvariantCulture)).Equals("2.00000E+6"));
            TestFmwk.assertTrue("neg023", ((BigDecimal.Parse("2000000", numberStyle, provider)).Negate(mc6).ToString(CultureInfo.InvariantCulture)).Equals("-2.00000E+6"));

            try
            {
                ten.Negate((MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e63)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("neg100", flag);

            try
            {
                tenlong.Negate(mcld);
                flag = false;
            }
            catch (ArithmeticException e64)
            {
                ae = e64;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("neg101", flag);
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#plus} method. */

        [Test]
        public void diagplus()
        {
            bool flag = false;
            MathContext mche1;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("plu001", ((BigDecimal.Parse("2", numberStyle, provider)).Plus(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("plu002", ((BigDecimal.Parse("-2", numberStyle, provider)).Plus(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-2"));
            TestFmwk.assertTrue("plu003", ((BigDecimal.Parse("2.00", numberStyle, provider)).Plus(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.00"));
            TestFmwk.assertTrue("plu004", ((BigDecimal.Parse("-2.00", numberStyle, provider)).Plus(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-2.00"));
            TestFmwk.assertTrue("plu005", ((BigDecimal.Parse("0", numberStyle, provider)).Plus(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("plu006", ((BigDecimal.Parse("0.00", numberStyle, provider)).Plus(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("plu007", ((BigDecimal.Parse("00.0", numberStyle, provider)).Plus(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("plu008", ((BigDecimal.Parse("00", numberStyle, provider)).Plus(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("plu010", ((BigDecimal.Parse("2", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("plu011", ((BigDecimal.Parse("-2", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("-2"));
            TestFmwk.assertTrue("plu012", ((BigDecimal.Parse("2.00", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("2.00"));
            TestFmwk.assertTrue("plu013", ((BigDecimal.Parse("-2.00", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("-2.00"));
            TestFmwk.assertTrue("plu014", ((BigDecimal.Parse("0", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("plu015", ((BigDecimal.Parse("0.00", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("plu016", ((BigDecimal.Parse("00.0", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("plu017", ((BigDecimal.Parse("00.00", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("plu018", ((BigDecimal.Parse("00", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("plu020", ((BigDecimal.Parse("-2000000", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("-2000000"));
            TestFmwk.assertTrue("plu021", ((BigDecimal.Parse("-2000000", numberStyle, provider)).Plus(mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-2000000"));
            TestFmwk.assertTrue("plu022", ((BigDecimal.Parse("-2000000", numberStyle, provider)).Plus(mc6).ToString(CultureInfo.InvariantCulture)).Equals("-2.00000E+6"));
            TestFmwk.assertTrue("plu023", ((BigDecimal.Parse("2000000", numberStyle, provider)).Plus(mc6).ToString(CultureInfo.InvariantCulture)).Equals("2.00000E+6"));

            // try some exotic but silly rounding [format checks more varieties]
            // [this mostly ensures we can set up and pass the setting]
            mche1 = new MathContext(1, MathContext.Scientific, false, MathContext.RoundHalfEven);
            TestFmwk.assertTrue("plu030", ((BigDecimal.Parse("0.24", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));
            TestFmwk.assertTrue("plu031", ((BigDecimal.Parse("0.25", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));
            TestFmwk.assertTrue("plu032", ((BigDecimal.Parse("0.26", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("0.3"));
            TestFmwk.assertTrue("plu033", ((BigDecimal.Parse("0.14", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("plu034", ((BigDecimal.Parse("0.15", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));
            TestFmwk.assertTrue("plu035", ((BigDecimal.Parse("0.16", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));

            TestFmwk.assertTrue("plu040", ((BigDecimal.Parse("0.251", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("0.3"));
            TestFmwk.assertTrue("plu041", ((BigDecimal.Parse("0.151", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));

            TestFmwk.assertTrue("plu050", ((BigDecimal.Parse("-0.24", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("-0.2"));
            TestFmwk.assertTrue("plu051", ((BigDecimal.Parse("-0.25", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("-0.2"));
            TestFmwk.assertTrue("plu052", ((BigDecimal.Parse("-0.26", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("-0.3"));
            TestFmwk.assertTrue("plu053", ((BigDecimal.Parse("-0.14", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("-0.1"));
            TestFmwk.assertTrue("plu054", ((BigDecimal.Parse("-0.15", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("-0.2"));
            TestFmwk.assertTrue("plu055", ((BigDecimal.Parse("-0.16", numberStyle, provider)).Plus(mche1).ToString(CultureInfo.InvariantCulture)).Equals("-0.2"));

            // more fixed, potential LHS swaps if done by add 0
            TestFmwk.assertTrue("plu060", ((BigDecimal.Parse("-56267E-10", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("-0.0000056267"));
            TestFmwk.assertTrue("plu061", ((BigDecimal.Parse("-56267E-5", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("-0.56267"));
            TestFmwk.assertTrue("plu062", ((BigDecimal.Parse("-56267E-2", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("-562.67"));
            TestFmwk.assertTrue("plu063", ((BigDecimal.Parse("-56267E-1", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("-5626.7"));
            TestFmwk.assertTrue("plu065", ((BigDecimal.Parse("-56267E-0", numberStyle, provider)).Plus().ToString(CultureInfo.InvariantCulture)).Equals("-56267"));

            try
            {
                ten.Plus((MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e65)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("plu100", flag);

            try
            {
                tenlong.Plus(mcld);
                flag = false;
            }
            catch (ArithmeticException e66)
            {
                ae = e66;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("plu101", flag);
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#pow} method. */

        [Test]
        public void diagpow()
        {
            bool flag;
            BigDecimal x;
            BigDecimal temp;
            int n = 0;
            BigDecimal vx;
            BigDecimal vn;
            ArithmeticException ae = null;
            flag = true;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("pow001", "1".Equals((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow002", "0.3".Equals((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow003", "0.3".Equals((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("1.00", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow004", "0.09".Equals((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("2.00", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow005", "0.09".Equals((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("2.000000000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow006", ("1E-8").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-8", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow007", ("1E-7").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-7", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow008", "0.000001".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-6", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow009", "0.00001".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-5", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow010", "0.0001".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-4", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow011", "0.001".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow012", "0.01".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow013", "0.1".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow014", "1".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow015", "10".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow016", "100000000".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("8", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow017", ("1E+9").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow018", ("1E+99").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("99", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow019", ("1E+999999999").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("999999999", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow020", ("1E+999999998").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("999999998", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow021", ("1E+999999997").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("999999997", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow022", ("1E+333333333").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("333333333", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow023", ("1E-333333333").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-333333333", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow024", ("1E-999999998").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-999999998", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow025", ("1E-999999999").Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-999999999", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow026", "0.5".Equals((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("-1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow027", "0.25".Equals((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("-2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow028", "0.0625".Equals((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("-4", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));

            TestFmwk.assertTrue("pow050", ((BigDecimal.Parse("0", numberStyle, provider)).Pow(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("pow051", ((BigDecimal.Parse("0", numberStyle, provider)).Pow(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("pow052", ((BigDecimal.Parse("0", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("pow053", ((BigDecimal.Parse("1", numberStyle, provider)).Pow(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("pow054", ((BigDecimal.Parse("1", numberStyle, provider)).Pow(BigDecimal.Parse("1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("pow055", ((BigDecimal.Parse("1", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("pow056", ((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("pow057", ((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("999999999", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E+999999999"));
            TestFmwk.assertTrue("pow058", ((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("999999998", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E+999999998"));
            TestFmwk.assertTrue("pow059", ((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("999999997", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E+999999997"));
            TestFmwk.assertTrue("pow060", ((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("333333333", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E+333333333"));
            TestFmwk.assertTrue("pow061", ((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("77", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E+77"));
            TestFmwk.assertTrue("pow062", ((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("22", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E+22"));
            TestFmwk.assertTrue("pow063", ((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-77", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E-77"));
            TestFmwk.assertTrue("pow064", ((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("-22", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E-22"));
            TestFmwk.assertTrue("pow065", ((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("-1", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            TestFmwk.assertTrue("pow066", ((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("-2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.25"));
            TestFmwk.assertTrue("pow067", ((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("-4", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.0625"));
            TestFmwk.assertTrue("pow068", ((BigDecimal.Parse("6.0", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("36"));
            TestFmwk.assertTrue("pow069", ((BigDecimal.Parse("-3", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("9"));/* from book */
            TestFmwk.assertTrue("pow070", ((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider), mcdef).Pow(BigDecimal.Parse("3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("64"));/* from book */

            // 1998.12.14 Next test removed as pow() no longer rounds RHS [as per ANSI]
            // Test('pow071').ok=BigDecimal('2').Pow(BigDecimal('2.000000001'),mcdef).toString == '4'/* check input rounding */

            /* General tests from original Rexx diagnostics */
            x = BigDecimal.Parse("0.5", numberStyle, provider);
            temp = BigDecimal.One;
            flag = true;
            {
                n = 1;
                for (; n <= 10; n++)
                {
                    temp = temp.Multiply(x).Divide(BigDecimal.One);
                    flag = flag
                            & (x.Pow(new BigDecimal(n), mcdef)
                                    .ToString(CultureInfo.InvariantCulture)).Equals(temp.ToString(CultureInfo.InvariantCulture));
                }
            }/* n */
            TestFmwk.assertTrue("pow080", flag);

            x = BigDecimal.Parse("2", numberStyle, provider);
            temp = BigDecimal.One;
            flag = true;
            {
                n = 1;
                for (; n <= 29; n++)
                {
                    temp = temp.Multiply(x).Divide(BigDecimal.One);
                    flag = flag & (x.Pow(new BigDecimal(n), mcdef).ToString(CultureInfo.InvariantCulture)).Equals(temp.ToString(CultureInfo.InvariantCulture));
                    flag = flag & (x.Pow(new BigDecimal(-n), mcdef).ToString(CultureInfo.InvariantCulture)).Equals(BigDecimal.One.Divide(temp, mcdef).ToString(CultureInfo.InvariantCulture));
                    /* Note that rounding errors are possible for larger "n" */
                    /* due to the information content of the exponent */
                }
            }/* n */
            TestFmwk.assertTrue("pow081", flag);

            /* The Vienna case. Checks both setup and 1/acc working precision */
            // Modified 1998.12.14 as RHS no longer rounded before use (must fit)
            // Modified 1990.02.04 as LHS is now rounded (instead of truncated to guard)
            vx = BigDecimal.Parse("123456789E+10", numberStyle, provider); // lhs .. rounded to 1.23E+18
            vn = BigDecimal.Parse("-1.23000e+2", numberStyle, provider); // rhs .. [was: -1.23455e+2, rounds to -123]
            TestFmwk.assertTrue("pow090", (vx.Pow(vn, mc3).ToString(CultureInfo.InvariantCulture)).Equals("8.74E-2226"));

            // - fixed point versions ---
            TestFmwk.assertTrue("pow101", "1".Equals((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow102", "0.3".Equals((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow103", "0.3".Equals((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("1.00", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow104", "0.09".Equals((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow105", "0.09".Equals((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("2.00", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow106", "10".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow107", "100000000".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("8", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow108", "1000000000".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow109", "10000000000".Equals((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("10", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow110", "1".Equals((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow111", "16".Equals((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("4", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow112", "256".Equals((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("8", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow113", "1024".Equals((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("10", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("pow114", "1.0510100501".Equals((BigDecimal.Parse("1.01", numberStyle, provider)).Pow(BigDecimal.Parse("5", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)));

            TestFmwk.assertTrue("pow120", ((BigDecimal.Parse("0", numberStyle, provider)).Pow(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("pow121", ((BigDecimal.Parse("0", numberStyle, provider)).Pow(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("pow122", ((BigDecimal.Parse("0", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("pow123", ((BigDecimal.Parse("1", numberStyle, provider)).Pow(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("pow144", ((BigDecimal.Parse("1", numberStyle, provider)).Pow(BigDecimal.Parse("1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("pow125", ((BigDecimal.Parse("1", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("pow126", ((BigDecimal.Parse("0.3", numberStyle, provider)).Pow(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("pow127", ((BigDecimal.Parse("10", numberStyle, provider)).Pow(BigDecimal.Parse("7", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("10000000"));
            TestFmwk.assertTrue("pow128", ((BigDecimal.Parse("6.0", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("36.00"));
            TestFmwk.assertTrue("pow129", ((BigDecimal.Parse("6.00", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("36.0000"));
            TestFmwk.assertTrue("pow130", ((BigDecimal.Parse("6.000", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("36.000000"));
            TestFmwk.assertTrue("pow131", ((BigDecimal.Parse("-3", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9"));
            TestFmwk.assertTrue("pow132", ((BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("2", numberStyle, provider)).Pow(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("64"));

            /* errors */
            try
            {
                ten.Pow((BigDecimal)null);
                flag = false;
            }
            catch (ArgumentNullException e67)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("pow150", flag);
            try
            {
                ten.Pow(ten, (MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e68)
            {
                flag = true;
            }/* checknull2 */
            TestFmwk.assertTrue("pow151", flag);

            flag = true;
            try
            {
                tenlong.Pow(BigDecimal.One, mcld);
                flag = false;
            }
            catch (ArithmeticException e69)
            {
                ae = e69;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("pow152", flag);

            try
            {
                BigDecimal.One.Pow(tenlong, mcld);
                flag = false;
            }
            catch (ArithmeticException e70)
            {
                ae = e70;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("pow153", flag);

            try
            {
                BigDecimal.One
                        .Pow(BigDecimal.Parse("-71", numberStyle, provider));
                flag = false;
            }
            catch (ArithmeticException e71)
            {
                ae = e71;
                flag = (ae.Message).Equals("Negative power: -71");
            }/* checkpos */
            TestFmwk.assertTrue("pow154", flag);

            try
            {
                BigDecimal.One.Pow(
                        BigDecimal.Parse("1234", numberStyle, provider), mc3);
                flag = false;
            }
            catch (ArithmeticException e72)
            {
                ae = e72;
                flag = (ae.Message).Equals("Too many digits: 1234");
            }/* checkwhole */
            TestFmwk.assertTrue("pow155", flag);

            try
            {
                BigDecimal.One.Pow(
                        BigDecimal.Parse("12.34e+2", numberStyle, provider), mc3);
                flag = false;
            }
            catch (ArithmeticException e73)
            {
                ae = e73;
                flag = (ae.Message).Equals("Too many digits: 1.234E+3");
            }/* checkwhole1 */
            TestFmwk.assertTrue("pow156", flag);

            try
            {
                BigDecimal.One.Pow(
                        BigDecimal.Parse("12.4", numberStyle, provider), mcdef);
                flag = false;
            }
            catch (ArithmeticException e74)
            {
                ae = e74;
                flag = (ae.Message).Equals("Decimal part non-zero: 12.4");
            }/* checkwhole2 */
            TestFmwk.assertTrue("pow157", flag);

            try
            {
                BigDecimal.One.Pow(
                        BigDecimal.Parse("1.01", numberStyle, provider), mcdef);
                flag = false;
            }
            catch (ArithmeticException e75)
            {
                ae = e75;
                flag = (ae.Message).Equals("Decimal part non-zero: 1.01");
            }/* checkwhole3 */
            TestFmwk.assertTrue("pow158", flag);

            try
            {
                BigDecimal.One.Pow(
                        BigDecimal.Parse("1.000000001", numberStyle, provider), mcdef);
                flag = false;
            }
            catch (ArithmeticException e76)
            {
                ae = e76;
                flag = (ae.Message)
                        .Equals("Decimal part non-zero: 1.000000001");
            }/* checkwhole4 */
            TestFmwk.assertTrue("pow159", flag);

            try
            {
                BigDecimal.One.Pow(
                        BigDecimal.Parse("1.000000001", numberStyle, provider), mc3);
                flag = false;
            }
            catch (ArithmeticException e77)
            {
                ae = e77;
                flag = (ae.Message)
                        .Equals("Decimal part non-zero: 1.000000001");
            }/* checkwhole5 */
            TestFmwk.assertTrue("pow160", flag);

            try
            {
                BigDecimal.One
                        .Pow(
                                BigDecimal.Parse("5.67E-987654321", numberStyle, provider), mc3);
                flag = false;
            }
            catch (ArithmeticException e78)
            {
                ae = e78;
                flag = (ae.Message)
                        .Equals("Decimal part non-zero: 5.67E-987654321");
            }/* checkwhole6 */
            TestFmwk.assertTrue("pow161", flag);
        }

        /*--------------------------------------------------------------------*/

        /** Test the {@link BigDecimal#remainder} method. */

        [Test]
        public void diagremainder()
        {
            bool flag = false;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("rem001", ((BigDecimal.Parse("1", numberStyle, provider)).Remainder(BigDecimal.Parse("3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("rem002", ((BigDecimal.Parse("5", numberStyle, provider)).Remainder(BigDecimal.Parse("5", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("rem003", ((BigDecimal.Parse("13", numberStyle, provider)).Remainder(BigDecimal.Parse("10", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            TestFmwk.assertTrue("rem004", ((BigDecimal.Parse("13", numberStyle, provider)).Remainder(BigDecimal.Parse("50", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("13"));
            TestFmwk.assertTrue("rem005", ((BigDecimal.Parse("13", numberStyle, provider)).Remainder(BigDecimal.Parse("100", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("13"));
            TestFmwk.assertTrue("rem006", ((BigDecimal.Parse("13", numberStyle, provider)).Remainder(BigDecimal.Parse("1000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("13"));
            TestFmwk.assertTrue("rem007", ((BigDecimal.Parse(".13", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.13"));
            TestFmwk.assertTrue("rem008", ((BigDecimal.Parse("0.133", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.133"));
            TestFmwk.assertTrue("rem009", ((BigDecimal.Parse("0.1033", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.1033"));
            TestFmwk.assertTrue("rem010", ((BigDecimal.Parse("1.033", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.033"));
            TestFmwk.assertTrue("rem011", ((BigDecimal.Parse("10.33", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.33"));
            TestFmwk.assertTrue("rem012", ((BigDecimal.Parse("10.33", numberStyle, provider)).Remainder(BigDecimal.Ten).ToString(CultureInfo.InvariantCulture)).Equals("0.33"));
            TestFmwk.assertTrue("rem013", ((BigDecimal.Parse("103.3", numberStyle, provider)).Remainder(BigDecimal.One).ToString(CultureInfo.InvariantCulture)).Equals("0.3"));
            TestFmwk.assertTrue("rem014", ((BigDecimal.Parse("133", numberStyle, provider)).Remainder(BigDecimal.Ten).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            TestFmwk.assertTrue("rem015", ((BigDecimal.Parse("1033", numberStyle, provider)).Remainder(BigDecimal.Ten).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            TestFmwk.assertTrue("rem016", ((BigDecimal.Parse("1033", numberStyle, provider)).Remainder(new BigDecimal(50), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("33"));
            TestFmwk.assertTrue("rem017", ((BigDecimal.Parse("101.0", numberStyle, provider)).Remainder(new BigDecimal(3), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.0"));
            TestFmwk.assertTrue("rem018", ((BigDecimal.Parse("102.0", numberStyle, provider)).Remainder(new BigDecimal(3), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("rem019", ((BigDecimal.Parse("103.0", numberStyle, provider)).Remainder(new BigDecimal(3), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("rem020", ((BigDecimal.Parse("2.40", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.40"));
            TestFmwk.assertTrue("rem021", ((BigDecimal.Parse("2.400", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.400"));
            TestFmwk.assertTrue("rem022", ((BigDecimal.Parse("2.4", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.4"));
            TestFmwk.assertTrue("rem023", ((BigDecimal.Parse("2.4", numberStyle, provider)).Remainder(new BigDecimal(2), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.4"));
            TestFmwk.assertTrue("rem024", ((BigDecimal.Parse("2.400", numberStyle, provider)).Remainder(new BigDecimal(2), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.400"));
            TestFmwk.assertTrue("rem025", ((BigDecimal.Parse("1", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("rem026", ((BigDecimal.Parse("1", numberStyle, provider)).Remainder(BigDecimal.Parse("0.30", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.10"));
            TestFmwk.assertTrue("rem027", ((BigDecimal.Parse("1", numberStyle, provider)).Remainder(BigDecimal.Parse("0.300", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.100"));
            TestFmwk.assertTrue("rem028", ((BigDecimal.Parse("1", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.1000"));
            TestFmwk.assertTrue("rem029", ((BigDecimal.Parse("1.0", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("rem030", ((BigDecimal.Parse("1.00", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.10"));
            TestFmwk.assertTrue("rem031", ((BigDecimal.Parse("1.000", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.100"));
            TestFmwk.assertTrue("rem032", ((BigDecimal.Parse("1.0000", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.1000"));
            TestFmwk.assertTrue("rem033", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("2.001", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));

            TestFmwk.assertTrue("rem040", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.5000001", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            TestFmwk.assertTrue("rem041", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.50000001", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            TestFmwk.assertTrue("rem042", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.500000001", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            TestFmwk.assertTrue("rem043", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.5000000001", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("rem044", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.50000000001", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("rem045", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.4999999", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E-7"));
            TestFmwk.assertTrue("rem046", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.49999999", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E-8"));
            TestFmwk.assertTrue("rem047", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.499999999", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E-9"));
            TestFmwk.assertTrue("rem048", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.4999999999", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("rem049", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.49999999999", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));

            TestFmwk.assertTrue("rem050", ((BigDecimal.Parse("0.03", numberStyle, provider)).Remainder(BigDecimal.Parse("7", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.03"));
            TestFmwk.assertTrue("rem051", ((BigDecimal.Parse("5", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("rem052", ((BigDecimal.Parse("4.1", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("rem053", ((BigDecimal.Parse("4.01", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.01"));
            TestFmwk.assertTrue("rem054", ((BigDecimal.Parse("4.001", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.001"));
            TestFmwk.assertTrue("rem055", ((BigDecimal.Parse("4.0001", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.0001"));
            TestFmwk.assertTrue("rem056", ((BigDecimal.Parse("4.00001", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.00001"));
            TestFmwk.assertTrue("rem057", ((BigDecimal.Parse("4.000001", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.000001"));
            TestFmwk.assertTrue("rem058", ((BigDecimal.Parse("4.0000001", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1E-7"));

            TestFmwk.assertTrue("rem060", ((BigDecimal.Parse("1.2", numberStyle, provider)).Remainder(BigDecimal.Parse("0.7345", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.4655"));
            TestFmwk.assertTrue("rem061", ((BigDecimal.Parse("0.8", numberStyle, provider)).Remainder(BigDecimal.Parse("12", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.8"));
            TestFmwk.assertTrue("rem062", ((BigDecimal.Parse("0.8", numberStyle, provider)).Remainder(BigDecimal.Parse("0.2", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("rem063", ((BigDecimal.Parse("0.8", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));
            TestFmwk.assertTrue("rem064", ((BigDecimal.Parse("0.800", numberStyle, provider)).Remainder(BigDecimal.Parse("12", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.800"));
            TestFmwk.assertTrue("rem065", ((BigDecimal.Parse("0.800", numberStyle, provider)).Remainder(BigDecimal.Parse("1.7", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.800"));
            TestFmwk.assertTrue("rem066", ((BigDecimal.Parse("2.400", numberStyle, provider)).Remainder(new BigDecimal(2), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.400"));

            // MC --
            TestFmwk.assertTrue("rem071", ((BigDecimal.Parse("2.400", numberStyle, provider)).Remainder(new BigDecimal(2), mc6).ToString(CultureInfo.InvariantCulture)).Equals("0.400"));
            TestFmwk.assertTrue("rem072", ((BigDecimal.Parse("12345678900000", numberStyle, provider)).Remainder(BigDecimal.Parse("12e+12", numberStyle, provider), mc3).ToString(CultureInfo.InvariantCulture)).Equals("3E+11"));

            // Fixed --
            TestFmwk.assertTrue("rem101", ((BigDecimal.Parse("1", numberStyle, provider)).Remainder(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("rem102", ((BigDecimal.Parse("5", numberStyle, provider)).Remainder(BigDecimal.Parse("5", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("rem103", ((BigDecimal.Parse("13", numberStyle, provider)).Remainder(BigDecimal.Parse("10", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            TestFmwk.assertTrue("rem104", ((BigDecimal.Parse("13", numberStyle, provider)).Remainder(BigDecimal.Parse("50", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("13"));
            TestFmwk.assertTrue("rem105", ((BigDecimal.Parse("13", numberStyle, provider)).Remainder(BigDecimal.Parse("100", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("13"));
            TestFmwk.assertTrue("rem106", ((BigDecimal.Parse("13", numberStyle, provider)).Remainder(BigDecimal.Parse("1000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("13"));
            TestFmwk.assertTrue("rem107", ((BigDecimal.Parse(".13", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.13"));
            TestFmwk.assertTrue("rem108", ((BigDecimal.Parse("0.133", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.133"));
            TestFmwk.assertTrue("rem109", ((BigDecimal.Parse("0.1033", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.1033"));
            TestFmwk.assertTrue("rem110", ((BigDecimal.Parse("1.033", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.033"));
            TestFmwk.assertTrue("rem111", ((BigDecimal.Parse("10.33", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.33"));
            TestFmwk.assertTrue("rem112", ((BigDecimal.Parse("10.33", numberStyle, provider)).Remainder(BigDecimal.Ten).ToString(CultureInfo.InvariantCulture)).Equals("0.33"));
            TestFmwk.assertTrue("rem113", ((BigDecimal.Parse("103.3", numberStyle, provider)).Remainder(BigDecimal.One).ToString(CultureInfo.InvariantCulture)).Equals("0.3"));
            TestFmwk.assertTrue("rem114", ((BigDecimal.Parse("133", numberStyle, provider)).Remainder(BigDecimal.Ten).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            TestFmwk.assertTrue("rem115", ((BigDecimal.Parse("1033", numberStyle, provider)).Remainder(BigDecimal.Ten).ToString(CultureInfo.InvariantCulture)).Equals("3"));
            TestFmwk.assertTrue("rem116", ((BigDecimal.Parse("1033", numberStyle, provider)).Remainder(new BigDecimal(50)).ToString(CultureInfo.InvariantCulture)).Equals("33"));
            TestFmwk.assertTrue("rem117", ((BigDecimal.Parse("101.0", numberStyle, provider)).Remainder(new BigDecimal(3)).ToString(CultureInfo.InvariantCulture)).Equals("2.0"));
            TestFmwk.assertTrue("rem118", ((BigDecimal.Parse("102.0", numberStyle, provider)).Remainder(new BigDecimal(3)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("rem119", ((BigDecimal.Parse("103.0", numberStyle, provider)).Remainder(new BigDecimal(3)).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("rem120", ((BigDecimal.Parse("2.40", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.40"));
            TestFmwk.assertTrue("rem121", ((BigDecimal.Parse("2.400", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.400"));
            TestFmwk.assertTrue("rem122", ((BigDecimal.Parse("2.4", numberStyle, provider)).Remainder(one).ToString(CultureInfo.InvariantCulture)).Equals("0.4"));
            TestFmwk.assertTrue("rem123", ((BigDecimal.Parse("2.4", numberStyle, provider)).Remainder(new BigDecimal(2)).ToString(CultureInfo.InvariantCulture)).Equals("0.4"));
            TestFmwk.assertTrue("rem124", ((BigDecimal.Parse("2.400", numberStyle, provider)).Remainder(new BigDecimal(2)).ToString(CultureInfo.InvariantCulture)).Equals("0.400"));
            TestFmwk.assertTrue("rem125", ((BigDecimal.Parse("1", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("rem126", ((BigDecimal.Parse("1", numberStyle, provider)).Remainder(BigDecimal.Parse("0.30", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.10"));
            TestFmwk.assertTrue("rem127", ((BigDecimal.Parse("1", numberStyle, provider)).Remainder(BigDecimal.Parse("0.300", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.100"));
            TestFmwk.assertTrue("rem128", ((BigDecimal.Parse("1", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.1000"));
            TestFmwk.assertTrue("rem129", ((BigDecimal.Parse("1.0", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("rem130", ((BigDecimal.Parse("1.00", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.10"));
            TestFmwk.assertTrue("rem131", ((BigDecimal.Parse("1.000", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.100"));
            TestFmwk.assertTrue("rem132", ((BigDecimal.Parse("1.0000", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.1000"));
            TestFmwk.assertTrue("rem133", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("2.001", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            TestFmwk.assertTrue("rem134", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.500000001", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            TestFmwk.assertTrue("rem135", ((BigDecimal.Parse("0.5", numberStyle, provider)).Remainder(BigDecimal.Parse("0.5000000001", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            TestFmwk.assertTrue("rem136", ((BigDecimal.Parse("0.03", numberStyle, provider)).Remainder(BigDecimal.Parse("7", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.03"));
            TestFmwk.assertTrue("rem137", ((BigDecimal.Parse("5", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("rem138", ((BigDecimal.Parse("4.1", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("rem139", ((BigDecimal.Parse("4.01", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.01"));
            TestFmwk.assertTrue("rem140", ((BigDecimal.Parse("4.001", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.001"));
            TestFmwk.assertTrue("rem141", ((BigDecimal.Parse("4.0001", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.0001"));
            TestFmwk.assertTrue("rem142", ((BigDecimal.Parse("4.00001", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00001"));
            TestFmwk.assertTrue("rem143", ((BigDecimal.Parse("4.000001", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.000001"));
            TestFmwk.assertTrue("rem144", ((BigDecimal.Parse("4.0000001", numberStyle, provider)).Remainder(BigDecimal.Parse("2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.0000001")); // 1E-7, plain
            TestFmwk.assertTrue("rem145", ((BigDecimal.Parse("1.2", numberStyle, provider)).Remainder(BigDecimal.Parse("0.7345", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.4655"));
            TestFmwk.assertTrue("rem146", ((BigDecimal.Parse("0.8", numberStyle, provider)).Remainder(BigDecimal.Parse("12", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.8"));
            TestFmwk.assertTrue("rem147", ((BigDecimal.Parse("0.8", numberStyle, provider)).Remainder(BigDecimal.Parse("0.2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("rem148", ((BigDecimal.Parse("0.8", numberStyle, provider)).Remainder(BigDecimal.Parse("0.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));
            TestFmwk.assertTrue("rem149", ((BigDecimal.Parse("0.800", numberStyle, provider)).Remainder(BigDecimal.Parse("12", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.800"));
            TestFmwk.assertTrue("rem150", ((BigDecimal.Parse("0.800", numberStyle, provider)).Remainder(BigDecimal.Parse("1.7", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.800"));
            TestFmwk.assertTrue("rem151", ((BigDecimal.Parse("2.400", numberStyle, provider)).Remainder(new BigDecimal(2), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.400"));


            try
            {
                ten.Remainder((BigDecimal)null);
                flag = false;
            }
            catch (ArgumentNullException e79)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("rem200", flag);
            try
            {
                ten.Remainder(ten, (MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e80)
            {
                flag = true;
            }/* checknull2 */
            TestFmwk.assertTrue("rem201", flag);

            try
            {
                BigDecimal.One.Remainder(tenlong, mcld);
                flag = false;
            }
            catch (ArithmeticException e81)
            {
                ae = e81;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("rem202", flag);

            try
            {
                tenlong.Remainder(one, mcld);
                flag = false;
            }
            catch (ArithmeticException e82)
            {
                ae = e82;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("rem203", flag);
        }

        /*--------------------------------------------------------------------*/

        /** Test the {@link BigDecimal#subtract} method. */

        [Test]
        public void diagsubtract()
        {
            bool flag = false;
            BigDecimal alhs;
            BigDecimal arhs;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            // [first group are 'quick confidence check']
            TestFmwk.assertTrue("sub301", ((new BigDecimal(2)).Subtract(new BigDecimal(3), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("sub302", ((BigDecimal.Parse("5.75", numberStyle, provider)).Subtract(BigDecimal.Parse("3.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.45"));
            TestFmwk.assertTrue("sub303", ((BigDecimal.Parse("5", numberStyle, provider)).Subtract(BigDecimal.Parse("-3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("8"));
            TestFmwk.assertTrue("sub304", ((BigDecimal.Parse("-5", numberStyle, provider)).Subtract(BigDecimal.Parse("-3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-2"));
            TestFmwk.assertTrue("sub305", ((BigDecimal.Parse("-7", numberStyle, provider)).Subtract(BigDecimal.Parse("2.5", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-9.5"));
            TestFmwk.assertTrue("sub306", ((BigDecimal.Parse("0.7", numberStyle, provider)).Subtract(BigDecimal.Parse("0.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.4"));
            TestFmwk.assertTrue("sub307", ((BigDecimal.Parse("1.3", numberStyle, provider)).Subtract(BigDecimal.Parse("0.3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("sub308", ((BigDecimal.Parse("1.25", numberStyle, provider)).Subtract(BigDecimal.Parse("1.25", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("sub309", ((BigDecimal.Parse("1.23456789", numberStyle, provider)).Subtract(BigDecimal.Parse("1.00000000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.23456789"));

            TestFmwk.assertTrue("sub310", ((BigDecimal.Parse("1.23456789", numberStyle, provider)).Subtract(BigDecimal.Parse("1.00000089", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.23456700"));

            TestFmwk.assertTrue("sub311", ((BigDecimal.Parse("0.5555555559", numberStyle, provider)).Subtract(BigDecimal.Parse("0.0000000001", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.555555556"));

            TestFmwk.assertTrue("sub312", ((BigDecimal.Parse("0.5555555559", numberStyle, provider)).Subtract(BigDecimal.Parse("0.0000000005", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.555555556"));

            TestFmwk.assertTrue("sub313", ((BigDecimal.Parse("0.4444444444", numberStyle, provider)).Subtract(BigDecimal.Parse("0.1111111111", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.333333333"));

            TestFmwk.assertTrue("sub314", ((BigDecimal.Parse("1.0000000000", numberStyle, provider)).Subtract(BigDecimal.Parse("0.00000001", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.99999999"));

            TestFmwk.assertTrue("sub315", ((BigDecimal.Parse("0.4444444444999", numberStyle, provider)).Subtract(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.444444444"));

            TestFmwk.assertTrue("sub316", ((BigDecimal.Parse("0.4444444445000", numberStyle, provider)).Subtract(BigDecimal.Parse("0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0.444444445"));


            TestFmwk.assertTrue("sub317", ((BigDecimal.Parse("70", numberStyle, provider)).Subtract(BigDecimal.Parse("10000e+9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-1.00000000E+13"));

            TestFmwk.assertTrue("sub318", ((BigDecimal.Parse("700", numberStyle, provider)).Subtract(BigDecimal.Parse("10000e+9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-1.00000000E+13"));

            TestFmwk.assertTrue("sub319", ((BigDecimal.Parse("7000", numberStyle, provider)).Subtract(BigDecimal.Parse("10000e+9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-1.00000000E+13"));

            TestFmwk.assertTrue("sub320", ((BigDecimal.Parse("70000", numberStyle, provider)).Subtract(BigDecimal.Parse("10000e+9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-9.9999999E+12"));

            TestFmwk.assertTrue("sub321", ((BigDecimal.Parse("700000", numberStyle, provider)).Subtract(BigDecimal.Parse("10000e+9", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("-9.9999993E+12"));

            // symmetry:
            TestFmwk.assertTrue("sub322", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("70", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000E+13"));

            TestFmwk.assertTrue("sub323", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("700", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000E+13"));

            TestFmwk.assertTrue("sub324", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("7000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("1.00000000E+13"));

            TestFmwk.assertTrue("sub325", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("70000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("9.9999999E+12"));

            TestFmwk.assertTrue("sub326", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("700000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("9.9999993E+12"));


            // [same with fixed point arithmetic]
            TestFmwk.assertTrue("sub001", ((new BigDecimal(2)).Subtract(new BigDecimal(3)).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("sub002", ((BigDecimal.Parse("5.75", numberStyle, provider)).Subtract(BigDecimal.Parse("3.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.45"));
            TestFmwk.assertTrue("sub003", ((BigDecimal.Parse("5", numberStyle, provider)).Subtract(BigDecimal.Parse("-3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("8"));
            TestFmwk.assertTrue("sub004", ((BigDecimal.Parse("-5", numberStyle, provider)).Subtract(BigDecimal.Parse("-3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-2"));
            TestFmwk.assertTrue("sub005", ((BigDecimal.Parse("-7", numberStyle, provider)).Subtract(BigDecimal.Parse("2.5", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-9.5"));
            TestFmwk.assertTrue("sub006", ((BigDecimal.Parse("0.7", numberStyle, provider)).Subtract(BigDecimal.Parse("0.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.4"));
            TestFmwk.assertTrue("sub007", ((BigDecimal.Parse("1.3", numberStyle, provider)).Subtract(BigDecimal.Parse("0.3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("sub008", ((BigDecimal.Parse("1.25", numberStyle, provider)).Subtract(BigDecimal.Parse("1.25", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("sub009", ((BigDecimal.Parse("0.02", numberStyle, provider)).Subtract(BigDecimal.Parse("0.02", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));

            TestFmwk.assertTrue("sub010", ((BigDecimal.Parse("1.23456789", numberStyle, provider)).Subtract(BigDecimal.Parse("1.00000000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.23456789"));

            TestFmwk.assertTrue("sub011", ((BigDecimal.Parse("1.23456789", numberStyle, provider)).Subtract(BigDecimal.Parse("1.00000089", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.23456700"));

            TestFmwk.assertTrue("sub012", ((BigDecimal.Parse("0.5555555559", numberStyle, provider)).Subtract(BigDecimal.Parse("0.0000000001", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.5555555558"));

            TestFmwk.assertTrue("sub013", ((BigDecimal.Parse("0.5555555559", numberStyle, provider)).Subtract(BigDecimal.Parse("0.0000000005", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.5555555554"));

            TestFmwk.assertTrue("sub014", ((BigDecimal.Parse("0.4444444444", numberStyle, provider)).Subtract(BigDecimal.Parse("0.1111111111", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.3333333333"));

            TestFmwk.assertTrue("sub015", ((BigDecimal.Parse("1.0000000000", numberStyle, provider)).Subtract(BigDecimal.Parse("0.00000001", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.9999999900"));

            TestFmwk.assertTrue("sub016", ((BigDecimal.Parse("0.4444444444999", numberStyle, provider)).Subtract(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.4444444444999"));

            TestFmwk.assertTrue("sub017", ((BigDecimal.Parse("0.4444444445000", numberStyle, provider)).Subtract(BigDecimal.Parse("0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.4444444445000"));


            TestFmwk.assertTrue("sub018", ((BigDecimal.Parse("70", numberStyle, provider)).Subtract(BigDecimal.Parse("10000e+9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-9999999999930"));

            TestFmwk.assertTrue("sub019", ((BigDecimal.Parse("700", numberStyle, provider)).Subtract(BigDecimal.Parse("10000e+9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-9999999999300"));

            TestFmwk.assertTrue("sub020", ((BigDecimal.Parse("7000", numberStyle, provider)).Subtract(BigDecimal.Parse("10000e+9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-9999999993000"));

            TestFmwk.assertTrue("sub021", ((BigDecimal.Parse("70000", numberStyle, provider)).Subtract(BigDecimal.Parse("10000e+9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-9999999930000"));

            TestFmwk.assertTrue("sub022", ((BigDecimal.Parse("700000", numberStyle, provider)).Subtract(BigDecimal.Parse("10000e+9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-9999999300000"));

            // symmetry:
            TestFmwk.assertTrue("sub023", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("70", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9999999999930"));

            TestFmwk.assertTrue("sub024", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("700", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9999999999300"));

            TestFmwk.assertTrue("sub025", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("7000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9999999993000"));

            TestFmwk.assertTrue("sub026", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("70000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9999999930000"));

            TestFmwk.assertTrue("sub027", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("700000", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("9999999300000"));

            // MC
            TestFmwk.assertTrue("sub030", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("70000", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("9.9999999E+12"));

            TestFmwk.assertTrue("sub031", ((BigDecimal.Parse("10000e+9", numberStyle, provider)).Subtract(BigDecimal.Parse("70000", numberStyle, provider), mc6).ToString(CultureInfo.InvariantCulture)).Equals("1.00000E+13"));


            // some of the next group are really constructor tests
            TestFmwk.assertTrue("sub040", ((BigDecimal.Parse("00.0", numberStyle, provider)).Subtract(BigDecimal.Parse("0.0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("sub041", ((BigDecimal.Parse("00.0", numberStyle, provider)).Subtract(BigDecimal.Parse("0.00", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("sub042", ((BigDecimal.Parse("0.00", numberStyle, provider)).Subtract(BigDecimal.Parse("00.0", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("sub043", ((BigDecimal.Parse("00.0", numberStyle, provider)).Subtract(BigDecimal.Parse("0.00", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("sub044", ((BigDecimal.Parse("0.00", numberStyle, provider)).Subtract(BigDecimal.Parse("00.0", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("sub045", ((BigDecimal.Parse("3", numberStyle, provider)).Subtract(BigDecimal.Parse(".3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.7"));
            TestFmwk.assertTrue("sub046", ((BigDecimal.Parse("3.", numberStyle, provider)).Subtract(BigDecimal.Parse(".3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.7"));
            TestFmwk.assertTrue("sub047", ((BigDecimal.Parse("3.0", numberStyle, provider)).Subtract(BigDecimal.Parse(".3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.7"));
            TestFmwk.assertTrue("sub048", ((BigDecimal.Parse("3.00", numberStyle, provider)).Subtract(BigDecimal.Parse(".3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("2.70"));
            TestFmwk.assertTrue("sub049", ((BigDecimal.Parse("3", numberStyle, provider)).Subtract(BigDecimal.Parse("3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("sub050", ((BigDecimal.Parse("3", numberStyle, provider)).Subtract(BigDecimal.Parse("+3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("sub051", ((BigDecimal.Parse("3", numberStyle, provider)).Subtract(BigDecimal.Parse("-3", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)).Equals("6"));
            TestFmwk.assertTrue("sub052", ((BigDecimal.Parse("3", numberStyle, provider)).Subtract(BigDecimal.Parse(".3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.7"));
            TestFmwk.assertTrue("sub053", ((BigDecimal.Parse("3.", numberStyle, provider)).Subtract(BigDecimal.Parse(".3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.7"));
            TestFmwk.assertTrue("sub054", ((BigDecimal.Parse("3.0", numberStyle, provider)).Subtract(BigDecimal.Parse(".3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.7"));
            TestFmwk.assertTrue("sub055", ((BigDecimal.Parse("3.00", numberStyle, provider)).Subtract(BigDecimal.Parse(".3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("2.70"));
            TestFmwk.assertTrue("sub056", ((BigDecimal.Parse("3", numberStyle, provider)).Subtract(BigDecimal.Parse("3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("sub057", ((BigDecimal.Parse("3", numberStyle, provider)).Subtract(BigDecimal.Parse("+3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("sub058", ((BigDecimal.Parse("3", numberStyle, provider)).Subtract(BigDecimal.Parse("-3", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("6"));

            // the above all from add; massaged and extended. Now some new ones...
            // [particularly important for comparisons]
            // NB: -1E-7 below were non-exponents pre-ANSI
            TestFmwk.assertTrue("sub080", ("-1E-7").Equals((BigDecimal.Parse("10.23456784", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub081", "0".Equals((BigDecimal.Parse("10.23456785", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub082", "0".Equals((BigDecimal.Parse("10.23456786", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub083", "0".Equals((BigDecimal.Parse("10.23456787", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub084", "0".Equals((BigDecimal.Parse("10.23456788", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub085", "0".Equals((BigDecimal.Parse("10.23456789", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub086", "0".Equals((BigDecimal.Parse("10.23456790", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub087", "0".Equals((BigDecimal.Parse("10.23456791", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub088", "0".Equals((BigDecimal.Parse("10.23456792", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub089", "0".Equals((BigDecimal.Parse("10.23456793", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub090", "0".Equals((BigDecimal.Parse("10.23456794", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456789", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub091", ("-1E-7").Equals((BigDecimal.Parse("10.23456781", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub092", ("-1E-7").Equals((BigDecimal.Parse("10.23456782", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub093", ("-1E-7").Equals((BigDecimal.Parse("10.23456783", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub094", ("-1E-7").Equals((BigDecimal.Parse("10.23456784", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub095", "0".Equals((BigDecimal.Parse("10.23456785", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub096", "0".Equals((BigDecimal.Parse("10.23456786", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub097", "0".Equals((BigDecimal.Parse("10.23456787", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub098", "0".Equals((BigDecimal.Parse("10.23456788", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub099", "0".Equals((BigDecimal.Parse("10.23456789", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub100", "0".Equals((BigDecimal.Parse("10.23456790", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub101", "0".Equals((BigDecimal.Parse("10.23456791", numberStyle, provider)).Subtract(BigDecimal.Parse("10.23456786", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub102", "0".Equals(BigDecimal.One.Subtract(BigDecimal.Parse("0.999999999", numberStyle, provider), mcdef).ToString(CultureInfo.InvariantCulture)));
            TestFmwk.assertTrue("sub103", "0".Equals((BigDecimal.Parse("0.999999999", numberStyle, provider)).Subtract(BigDecimal.One, mcdef).ToString(CultureInfo.InvariantCulture)));

            alhs = BigDecimal.Parse("12345678900000", numberStyle, provider);
            arhs = BigDecimal.Parse("9999999999999", numberStyle, provider);
            TestFmwk.assertTrue("sub110", (alhs.Subtract(arhs, mc3).ToString(CultureInfo.InvariantCulture)).Equals("2.3E+12"));
            TestFmwk.assertTrue("sub111", (arhs.Subtract(alhs, mc3).ToString(CultureInfo.InvariantCulture)).Equals("-2.3E+12"));
            TestFmwk.assertTrue("sub112", (alhs.Subtract(arhs).ToString(CultureInfo.InvariantCulture)).Equals("2345678900001"));
            TestFmwk.assertTrue("sub113", (arhs.Subtract(alhs).ToString(CultureInfo.InvariantCulture)).Equals("-2345678900001"));

            // additional scaled arithmetic tests [0.97 problem]
            TestFmwk.assertTrue("sub120", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse(".1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.1"));
            TestFmwk.assertTrue("sub121", ((BigDecimal.Parse("00", numberStyle, provider)).Subtract(BigDecimal.Parse(".97983", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.97983"));
            TestFmwk.assertTrue("sub122", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse(".9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.9"));
            TestFmwk.assertTrue("sub123", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("0.102", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.102"));
            TestFmwk.assertTrue("sub124", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse(".4", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.4"));
            TestFmwk.assertTrue("sub125", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse(".307", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.307"));
            TestFmwk.assertTrue("sub126", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse(".43822", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.43822"));
            TestFmwk.assertTrue("sub127", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse(".911", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.911"));
            TestFmwk.assertTrue("sub128", ((BigDecimal.Parse(".0", numberStyle, provider)).Subtract(BigDecimal.Parse(".02", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.02"));
            TestFmwk.assertTrue("sub129", ((BigDecimal.Parse("00", numberStyle, provider)).Subtract(BigDecimal.Parse(".392", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.392"));
            TestFmwk.assertTrue("sub130", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse(".26", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.26"));
            TestFmwk.assertTrue("sub131", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("0.51", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.51"));
            TestFmwk.assertTrue("sub132", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse(".2234", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.2234"));
            TestFmwk.assertTrue("sub133", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse(".2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.2"));
            TestFmwk.assertTrue("sub134", ((BigDecimal.Parse(".0", numberStyle, provider)).Subtract(BigDecimal.Parse(".0008", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("-0.0008"));
            // 0. on left
            TestFmwk.assertTrue("sub140", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("sub141", ((BigDecimal.Parse("0.00", numberStyle, provider)).Subtract(BigDecimal.Parse("-.97983", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.97983"));
            TestFmwk.assertTrue("sub142", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.9"));
            TestFmwk.assertTrue("sub143", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-0.102", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.102"));
            TestFmwk.assertTrue("sub144", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.4", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.4"));
            TestFmwk.assertTrue("sub145", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.307", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.307"));
            TestFmwk.assertTrue("sub146", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.43822", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.43822"));
            TestFmwk.assertTrue("sub147", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.911", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.911"));
            TestFmwk.assertTrue("sub148", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.02", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.02"));
            TestFmwk.assertTrue("sub149", ((BigDecimal.Parse("0.00", numberStyle, provider)).Subtract(BigDecimal.Parse("-.392", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.392"));
            TestFmwk.assertTrue("sub150", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.26", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.26"));
            TestFmwk.assertTrue("sub151", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-0.51", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.51"));
            TestFmwk.assertTrue("sub152", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.2234", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.2234"));
            TestFmwk.assertTrue("sub153", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));
            TestFmwk.assertTrue("sub154", ((BigDecimal.Parse("0.0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.0008", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.0008"));
            // negatives of same
            TestFmwk.assertTrue("sub160", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.1", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("sub161", ((BigDecimal.Parse("00", numberStyle, provider)).Subtract(BigDecimal.Parse("-.97983", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.97983"));
            TestFmwk.assertTrue("sub162", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.9", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.9"));
            TestFmwk.assertTrue("sub163", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-0.102", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.102"));
            TestFmwk.assertTrue("sub164", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.4", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.4"));
            TestFmwk.assertTrue("sub165", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.307", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.307"));
            TestFmwk.assertTrue("sub166", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.43822", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.43822"));
            TestFmwk.assertTrue("sub167", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.911", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.911"));
            TestFmwk.assertTrue("sub168", ((BigDecimal.Parse(".0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.02", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.02"));
            TestFmwk.assertTrue("sub169", ((BigDecimal.Parse("00", numberStyle, provider)).Subtract(BigDecimal.Parse("-.392", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.392"));
            TestFmwk.assertTrue("sub170", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.26", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.26"));
            TestFmwk.assertTrue("sub171", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-0.51", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.51"));
            TestFmwk.assertTrue("sub172", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.2234", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.2234"));
            TestFmwk.assertTrue("sub173", ((BigDecimal.Parse("0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.2", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.2"));
            TestFmwk.assertTrue("sub174", ((BigDecimal.Parse(".0", numberStyle, provider)).Subtract(BigDecimal.Parse("-.0008", numberStyle, provider)).ToString(CultureInfo.InvariantCulture)).Equals("0.0008"));

            // more fixed, LHS swaps [really same as testcases under add]
            TestFmwk.assertTrue("sub180", ((BigDecimal.Parse("-56267E-10", numberStyle, provider)).Subtract(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.0000056267"));
            TestFmwk.assertTrue("sub181", ((BigDecimal.Parse("-56267E-5", numberStyle, provider)).Subtract(zero).ToString(CultureInfo.InvariantCulture)).Equals("-0.56267"));
            TestFmwk.assertTrue("sub182", ((BigDecimal.Parse("-56267E-2", numberStyle, provider)).Subtract(zero).ToString(CultureInfo.InvariantCulture)).Equals("-562.67"));
            TestFmwk.assertTrue("sub183", ((BigDecimal.Parse("-56267E-1", numberStyle, provider)).Subtract(zero).ToString(CultureInfo.InvariantCulture)).Equals("-5626.7"));
            TestFmwk.assertTrue("sub185", ((BigDecimal.Parse("-56267E-0", numberStyle, provider)).Subtract(zero).ToString(CultureInfo.InvariantCulture)).Equals("-56267"));

            try
            {
                ten.Subtract((BigDecimal)null);
                flag = false;
            }
            catch (ArgumentNullException e83)
            {
                flag = true;
            }/* checknull */
            TestFmwk.assertTrue("sub200", flag);
            try
            {
                ten.Subtract(ten, (MathContext)null);
                flag = false;
            }
            catch (ArgumentNullException e84)
            {
                flag = true;
            }/* checknull2 */
            TestFmwk.assertTrue("sub201", flag);

            try
            {
                BigDecimal.One.Subtract(tenlong, mcld);
                flag = false;
            }
            catch (ArithmeticException e85)
            {
                ae = e85;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("sub202", flag);
            try
            {
                tenlong.Subtract(BigDecimal.One, mcld);
                flag = false;
            }
            catch (ArithmeticException e86)
            {
                ae = e86;
                flag = (ae.Message).Equals("Too many digits:" + " "
                        + tenlong.ToString(CultureInfo.InvariantCulture));
            }/* checkdigits */
            TestFmwk.assertTrue("sub203", flag);
        }

        /* ----------------------------------------------------------------- */

        /* ----------------------------------------------------------------- */
        /* Other methods */
        /* ----------------------------------------------------------------- */

        /** Test the <code>BigDecimal.byteValue()</code> method. */

        [Test]
        public void diagbyteValue()
        {
            bool flag = false;
            string v = null;
            ArithmeticException ae = null;
            string[] badstrings;
            int i = 0;
            string norm = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("byv001", ((((sbyte)-128))) == ((BigDecimal.Parse("-128", numberStyle, provider)).ToSByte()));
            TestFmwk.assertTrue("byv002", ((0)) == ((BigDecimal.Parse("0", numberStyle, provider)).ToSByte()));
            TestFmwk.assertTrue("byv003", ((1)) == ((BigDecimal.Parse("1", numberStyle, provider)).ToSByte()));
            TestFmwk.assertTrue("byv004", ((99)) == ((BigDecimal.Parse("99", numberStyle, provider)).ToSByte()));
            TestFmwk.assertTrue("byv005", ((127)) == ((BigDecimal.Parse("127", numberStyle, provider)).ToSByte()));
            TestFmwk.assertTrue("byv006", ((-128)) == ((BigDecimal.Parse("128", numberStyle, provider)).ToSByte()));
            TestFmwk.assertTrue("byv007", ((-127)) == ((BigDecimal.Parse("129", numberStyle, provider)).ToSByte()));
            TestFmwk.assertTrue("byv008", ((127)) == ((BigDecimal.Parse("-129", numberStyle, provider)).ToSByte()));
            TestFmwk.assertTrue("byv009", ((126)) == ((BigDecimal.Parse("-130", numberStyle, provider)).ToSByte()));
            TestFmwk.assertTrue("byv010", ((bmax)) == ((new BigDecimal(bmax)).ToSByte()));
            TestFmwk.assertTrue("byv011", ((bmin)) == ((new BigDecimal(bmin)).ToSByte()));
            TestFmwk.assertTrue("byv012", ((bneg)) == ((new BigDecimal(bneg)).ToSByte()));
            TestFmwk.assertTrue("byv013", ((bzer)) == ((new BigDecimal(bzer)).ToSByte()));
            TestFmwk.assertTrue("byv014", ((bpos)) == ((new BigDecimal(bpos)).ToSByte()));
            TestFmwk.assertTrue("byv015", ((bmin)) == ((new BigDecimal(bmax + 1)).ToSByte()));
            TestFmwk.assertTrue("byv016", ((bmax)) == ((new BigDecimal(bmin - 1)).ToSByte()));

            TestFmwk.assertTrue("byv021", (((unchecked((sbyte)-128)))) == ((BigDecimal.Parse("-128", numberStyle, provider)).ToSByteExact()));
            TestFmwk.assertTrue("byv022", ((0)) == ((BigDecimal.Parse("0", numberStyle, provider)).ToSByteExact()));
            TestFmwk.assertTrue("byv023", ((1)) == ((BigDecimal.Parse("1", numberStyle, provider)).ToSByteExact()));
            TestFmwk.assertTrue("byv024", ((99)) == ((BigDecimal.Parse("99", numberStyle, provider)).ToSByteExact()));
            TestFmwk.assertTrue("byv025", ((127)) == ((BigDecimal.Parse("127", numberStyle, provider)).ToSByteExact()));
            TestFmwk.assertTrue("byv026", ((bmax)) == ((new BigDecimal(bmax)).ToSByteExact()));
            TestFmwk.assertTrue("byv027", ((bmin)) == ((new BigDecimal(bmin)).ToSByteExact()));
            TestFmwk.assertTrue("byv028", ((bneg)) == ((new BigDecimal(bneg)).ToSByteExact()));
            TestFmwk.assertTrue("byv029", ((bzer)) == ((new BigDecimal(bzer)).ToSByteExact()));
            TestFmwk.assertTrue("byv030", ((bpos)) == ((new BigDecimal(bpos)).ToSByteExact()));

            try
            {
                v = "-129";
                (BigDecimal.Parse(v, numberStyle, provider)).ToByteExact();
                flag = false;
            }
            catch (ArithmeticException e87)
            {
                ae = e87;
                flag = (ae.Message).Equals("Conversion overflow:" + " " + v);
            }
            TestFmwk.assertTrue("byv100", flag);
            try
            {
                v = "128";
                (BigDecimal.Parse(v, numberStyle, provider)).ToByteExact();
                flag = false;
            }
            catch (ArithmeticException e88)
            {
                ae = e88;
                flag = (ae.Message).Equals("Conversion overflow:" + " " + v);
            }
            TestFmwk.assertTrue("byv101", flag);
            try
            {
                v = "1.5";
                (BigDecimal.Parse(v, numberStyle, provider)).ToByteExact();
                flag = false;
            }
            catch (ArithmeticException e89)
            {
                ae = e89;
                flag = (ae.Message).Equals("Decimal part non-zero:" + " " + v);
            }
            TestFmwk.assertTrue("byv102", flag);

            badstrings = new string[] {
                "1234",
                (new BigDecimal(bmax)).Add(one).ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(bmin)).Subtract(one)
                        .ToString(CultureInfo.InvariantCulture),
                "170",
                "270",
                "370",
                "470",
                "570",
                "670",
                "770",
                "870",
                "970",
                "-170",
                "-270",
                "-370",
                "-470",
                "-570",
                "-670",
                "-770",
                "-870",
                "-970",
                (new BigDecimal(bmin)).Multiply(two)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(bmax)).Multiply(two)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(bmin)).Multiply(ten)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(bmax)).Multiply(ten)
                        .ToString(CultureInfo.InvariantCulture), "-1234" }; // 220
                                                                            // 221
                                                                            // 222
                                                                            // 223
                                                                            // 224
                                                                            // 225
                                                                            // 226
                                                                            // 227
                                                                            // 228
                                                                            // 229
                                                                            // 230
                                                                            // 231
                                                                            // 232
                                                                            // 233
                                                                            // 234
                                                                            // 235
                                                                            // 236
                                                                            // 237
                                                                            // 238
                                                                            // 239
                                                                            // 240
                                                                            // 241
                                                                            // 242
                                                                            // 243
                                                                            // 244
                                                                            // 245
            {
                int e90 = badstrings.Length;
                i = 0;
                for (; e90 > 0; e90--, i++)
                {
                    try
                    {
                        v = badstrings[i];
                        (BigDecimal.Parse(v, numberStyle, provider)).ToByteExact();
                        flag = false;
                    }
                    catch (ArithmeticException e91)
                    {
                        ae = e91;
                        norm = (BigDecimal.Parse(v, numberStyle, provider)).ToString(CultureInfo.InvariantCulture);
                        flag = (ae.Message).Equals("Conversion overflow:"
                                + " " + norm);
                    }
                    TestFmwk.assertTrue("byv" + (220 + i), flag);
                }
            }/* i */
        }

        /* ----------------------------------------------------------------- */

        /**
         * Test the {@link BigDecimal#compareTo(java.lang.Object)}
         * method.
         */

        [Test]
        public void diagcomparetoObj()
        {
            //        bool flag = false;
            //        BigDecimal d;
            //        BigDecimal long1;
            //        BigDecimal long2;
            //
            //        d = new BigDecimal(17);
            //        (new Test("cto001")).ok = (d
            //                .CompareTo((java.lang.Object) (new BigDecimal(
            //                        66)))) == (-1);
            //        (new Test("cto002")).ok = (d
            //                .CompareTo((java.lang.Object) ((new BigDecimal(
            //                        10)).Add(new BigDecimal(7))))) == 0;
            //        (new Test("cto003")).ok = (d
            //                .CompareTo((java.lang.Object) (new BigDecimal(
            //                        10)))) == 1;
            //        long1 = new BigDecimal("12345678903");
            //        long2 = new BigDecimal("12345678900");
            //        TestFmwk.assertTrue("cto004", (long1.CompareTo((java.lang.Object) long2)) == 1);
            //        TestFmwk.assertTrue("cto005", (long2.CompareTo((java.lang.Object) long1)) == (-1));
            //        TestFmwk.assertTrue("cto006", (long2.CompareTo((java.lang.Object) long2)) == 0);
            //        try {
            //            d.CompareTo((java.lang.Object) null);
            //            flag = false;
            //        } catch (ArgumentNullException $92) {
            //            flag = true; // should get here
            //        }
            //        TestFmwk.assertTrue("cto101", flag);
            //        try {
            //            d.CompareTo((java.lang.Object) "foo");
            //            flag = false;
            //        } catch (java.lang.ClassCastException $93) {
            //            flag = true; // should get here
            //        }
            //        TestFmwk.assertTrue("cto102", flag);
            //        summary("compareTo(Obj)");
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#doubleValue} method. */

        [Test]
        public void diagdoublevalue()
        {
#if FEATURE_IKVM
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            string val;
            // 1999.03.07 Infinities no longer errors
            val = "-1";
            TestFmwk.assertTrue("dov001", ((BigDecimal.Parse(val, numberStyle, provider)).ToDouble()) == ((new java.lang.Double(val)).doubleValue()));
            val = "-0.1";
            TestFmwk.assertTrue("dov002", ((BigDecimal.Parse(val, numberStyle, provider)).ToDouble()) == ((new java.lang.Double(val)).doubleValue()));
            val = "0";
            TestFmwk.assertTrue("dov003", ((BigDecimal.Parse(val, numberStyle, provider)).ToDouble()) == ((new java.lang.Double(val)).doubleValue()));
            val = "0.1";
            TestFmwk.assertTrue("dov004", ((BigDecimal.Parse(val, numberStyle, provider)).ToDouble()) == ((new java.lang.Double(val)).doubleValue()));
            val = "1";
            TestFmwk.assertTrue("dov005", ((BigDecimal.Parse(val, numberStyle, provider)).ToDouble()) == ((new java.lang.Double(val)).doubleValue()));
            val = "1e1000";
            TestFmwk.assertTrue("dov006", ((BigDecimal.Parse(val, numberStyle, provider)).ToDouble()) == java.lang.Double.POSITIVE_INFINITY);
            val = "-1e1000";
            TestFmwk.assertTrue("dov007", ((BigDecimal.Parse(val, numberStyle, provider)).ToDouble()) == java.lang.Double.NEGATIVE_INFINITY);
#endif
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#equals} method. */

        [Test]
        public void diagequals()
        {
            BigDecimal d;
            d = new BigDecimal(17);
            TestFmwk.assertTrue("equ001", (!(d.Equals((object)null))));
            TestFmwk.assertTrue("equ002", (!(d.Equals("foo"))));
            TestFmwk.assertTrue("equ003", (!(d.Equals((new BigDecimal(66))))));
            TestFmwk.assertTrue("equ004", d.Equals(d));
            TestFmwk.assertTrue("equ005", d.Equals(((new BigDecimal(10)).Add(new BigDecimal(7)))));
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#floatValue} method. */

        [Test]
        public void diagfloatvalue()
        {
#if FEATURE_IKVM
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            string val;
            // 1999.03.07 Infinities no longer errors
            val = "-1";
            TestFmwk.assertTrue("flv001", ((BigDecimal.Parse(val, numberStyle, provider)).ToSingle()) == ((new java.lang.Float(val)).floatValue()));
            val = "-0.1";
            TestFmwk.assertTrue("flv002", ((BigDecimal.Parse(val, numberStyle, provider)).ToSingle()) == ((new java.lang.Float(val)).floatValue()));
            val = "0";
            TestFmwk.assertTrue("flv003", ((BigDecimal.Parse(val, numberStyle, provider)).ToSingle()) == ((new java.lang.Float(val)).floatValue()));
            val = "0.1";
            TestFmwk.assertTrue("flv004", ((BigDecimal.Parse(val, numberStyle, provider)).ToSingle()) == ((new java.lang.Float(val)).floatValue()));
            val = "1";
            TestFmwk.assertTrue("flv005", ((BigDecimal.Parse(val, numberStyle, provider)).ToSingle()) == ((new java.lang.Float(val)).floatValue()));
            val = "1e200";
            TestFmwk.assertTrue("flv006", ((BigDecimal.Parse(val, numberStyle, provider)).ToSingle()) == java.lang.Float.POSITIVE_INFINITY);
            val = "-1e200";
            TestFmwk.assertTrue("flv007", ((BigDecimal.Parse(val, numberStyle, provider)).ToSingle()) == java.lang.Float.NEGATIVE_INFINITY);
            val = "1e1000";
            TestFmwk.assertTrue("flv008", ((BigDecimal.Parse(val, numberStyle, provider)).ToSingle()) == java.lang.Float.POSITIVE_INFINITY);
            val = "-1e1000";
            TestFmwk.assertTrue("flv009", ((BigDecimal.Parse(val, numberStyle, provider)).ToSingle()) == java.lang.Float.NEGATIVE_INFINITY);
#endif
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#format} method. */

        [Test]
        public void diagformat()
        {
            bool flag = false;
            ExponentForm eng;
            ExponentForm sci;
            BigDecimal d04;
            BigDecimal d05;
            BigDecimal d06;
            BigDecimal d15;
            ArgumentException iae = null;
            BigDecimal d050;
            BigDecimal d150;
            BigDecimal m050;
            BigDecimal m150;
            BigDecimal d051;
            BigDecimal d151;
            BigDecimal d000;
            BigDecimal d500;
            ArithmeticException ae = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;
            // 1999.02.09 now only two signatures for format(), so some tests below
            // may now be redundant

            TestFmwk.assertTrue("for001", ((BigDecimal.Parse("12.3", numberStyle, provider)).Format(-1, -1)).Equals("12.3"));
            TestFmwk.assertTrue("for002", ((BigDecimal.Parse("-12.73", numberStyle, provider)).Format(-1, -1)).Equals("-12.73"));
            TestFmwk.assertTrue("for003", ((BigDecimal.Parse("0.000", numberStyle, provider)).Format(-1, -1)).Equals("0.000"));
            TestFmwk.assertTrue("for004", ((BigDecimal.Parse("3E+3", numberStyle, provider)).Format(-1, -1)).Equals("3000"));
            TestFmwk.assertTrue("for005", ((BigDecimal.Parse("3", numberStyle, provider)).Format(4, -1)).Equals("   3"));
            TestFmwk.assertTrue("for006", ((BigDecimal.Parse("1.73", numberStyle, provider)).Format(4, 0)).Equals("   2"));
            TestFmwk.assertTrue("for007", ((BigDecimal.Parse("1.73", numberStyle, provider)).Format(4, 1)).Equals("   1.7"));
            TestFmwk.assertTrue("for008", ((BigDecimal.Parse("1.75", numberStyle, provider)).Format(4, 1)).Equals("   1.8"));
            TestFmwk.assertTrue("for009", ((BigDecimal.Parse("0.5", numberStyle, provider)).Format(4, 1)).Equals("   0.5"));
            TestFmwk.assertTrue("for010", ((BigDecimal.Parse("0.05", numberStyle, provider)).Format(4, 1)).Equals("   0.1"));
            TestFmwk.assertTrue("for011", ((BigDecimal.Parse("0.04", numberStyle, provider)).Format(4, 1)).Equals("   0.0"));
            TestFmwk.assertTrue("for012", ((BigDecimal.Parse("0", numberStyle, provider)).Format(4, 0)).Equals("   0"));
            TestFmwk.assertTrue("for013", ((BigDecimal.Parse("0", numberStyle, provider)).Format(4, 1)).Equals("   0.0"));
            TestFmwk.assertTrue("for014", ((BigDecimal.Parse("0", numberStyle, provider)).Format(4, 2)).Equals("   0.00"));
            TestFmwk.assertTrue("for015", ((BigDecimal.Parse("0", numberStyle, provider)).Format(4, 3)).Equals("   0.000"));
            TestFmwk.assertTrue("for016", ((BigDecimal.Parse("0", numberStyle, provider)).Format(4, 4)).Equals("   0.0000"));
            TestFmwk.assertTrue("for017", ((BigDecimal.Parse("0.005", numberStyle, provider)).Format(4, 0)).Equals("   0"));
            TestFmwk.assertTrue("for018", ((BigDecimal.Parse("0.005", numberStyle, provider)).Format(4, 1)).Equals("   0.0"));
            TestFmwk.assertTrue("for019", ((BigDecimal.Parse("0.005", numberStyle, provider)).Format(4, 2)).Equals("   0.01"));
            TestFmwk.assertTrue("for020", ((BigDecimal.Parse("0.004", numberStyle, provider)).Format(4, 2)).Equals("   0.00"));
            TestFmwk.assertTrue("for021", ((BigDecimal.Parse("0.005", numberStyle, provider)).Format(4, 3)).Equals("   0.005"));
            TestFmwk.assertTrue("for022", ((BigDecimal.Parse("0.005", numberStyle, provider)).Format(4, 4)).Equals("   0.0050"));

            TestFmwk.assertTrue("for023", ((BigDecimal.Parse("1.73", numberStyle, provider)).Format(4, 2)).Equals("   1.73"));
            TestFmwk.assertTrue("for024", ((BigDecimal.Parse("1.73", numberStyle, provider)).Format(4, 3)).Equals("   1.730"));
            TestFmwk.assertTrue("for025", ((BigDecimal.Parse("-.76", numberStyle, provider)).Format(4, 1)).Equals("  -0.8"));
            TestFmwk.assertTrue("for026", ((BigDecimal.Parse("-12.73", numberStyle, provider)).Format(-1, 4)).Equals("-12.7300"));

            TestFmwk.assertTrue("for027", ((BigDecimal.Parse("3.03", numberStyle, provider)).Format(4, -1)).Equals("   3.03"));
            TestFmwk.assertTrue("for028", ((BigDecimal.Parse("3.03", numberStyle, provider)).Format(4, 1)).Equals("   3.0"));
            TestFmwk.assertTrue("for029", ((BigDecimal.Parse("3.03", numberStyle, provider)).Format(4, -1, 3, -1, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("   3.03     "));
            TestFmwk.assertTrue("for030", ((BigDecimal.Parse("3.03", numberStyle, provider)).Format(-1, -1, 3, -1, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("3.03     "));
            TestFmwk.assertTrue("for031", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, -1, -1, 4, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.234573E+4"));
            TestFmwk.assertTrue("for032", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, -1, -1, 5, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("12345.73"));
            TestFmwk.assertTrue("for033", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, -1, -1, 6, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("12345.73"));

            TestFmwk.assertTrue("for034", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, 8, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.23457300E+4"));
            TestFmwk.assertTrue("for035", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, 7, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.2345730E+4"));
            TestFmwk.assertTrue("for036", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, 6, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.234573E+4"));
            TestFmwk.assertTrue("for037", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, 5, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.23457E+4"));
            TestFmwk.assertTrue("for038", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, 4, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.2346E+4"));
            TestFmwk.assertTrue("for039", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, 3, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.235E+4"));
            TestFmwk.assertTrue("for040", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, 2, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.23E+4"));
            TestFmwk.assertTrue("for041", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, 1, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.2E+4"));
            TestFmwk.assertTrue("for042", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, 0, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1E+4"));

            TestFmwk.assertTrue("for043", ((BigDecimal.Parse("99999.99", numberStyle, provider)).Format(-1, 6, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("9.999999E+4"));
            TestFmwk.assertTrue("for044", ((BigDecimal.Parse("99999.99", numberStyle, provider)).Format(-1, 5, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.00000E+5"));
            TestFmwk.assertTrue("for045", ((BigDecimal.Parse("99999.99", numberStyle, provider)).Format(-1, 2, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.00E+5"));
            TestFmwk.assertTrue("for046", ((BigDecimal.Parse("99999.99", numberStyle, provider)).Format(-1, 0, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1E+5"));
            TestFmwk.assertTrue("for047", ((BigDecimal.Parse("99999.99", numberStyle, provider)).Format(3, 0, -1, 3, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("  1E+5"));

            TestFmwk.assertTrue("for048", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, -1, 2, 2, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.234573E+04"));
            TestFmwk.assertTrue("for049", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, 3, -1, 0, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.235E+4"));
            TestFmwk.assertTrue("for050", ((BigDecimal.Parse("1.234573", numberStyle, provider)).Format(-1, 3, -1, 0, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.235"));
            TestFmwk.assertTrue("for051", ((BigDecimal.Parse("123.45", numberStyle, provider)).Format(-1, 3, 2, 0, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.235E+02"));

            TestFmwk.assertTrue("for052", ((BigDecimal.Parse("0.444", numberStyle, provider)).Format(-1, 0)).Equals("0"));
            TestFmwk.assertTrue("for053", ((BigDecimal.Parse("-0.444", numberStyle, provider)).Format(-1, 0)).Equals("0"));
            TestFmwk.assertTrue("for054", ((BigDecimal.Parse("0.4", numberStyle, provider)).Format(-1, 0)).Equals("0"));
            TestFmwk.assertTrue("for055", ((BigDecimal.Parse("-0.4", numberStyle, provider)).Format(-1, 0)).Equals("0"));

            eng = MathContext.Engineering;
            sci = MathContext.Scientific;
            TestFmwk.assertTrue("for060", ((BigDecimal.Parse("1234.5", numberStyle, provider)).Format(-1, 3, 2, 0, eng, (RoundingMode)(-1))).Equals("1.235E+03"));
            TestFmwk.assertTrue("for061", ((BigDecimal.Parse("12345", numberStyle, provider)).Format(-1, 3, 3, 0, eng, (RoundingMode)(-1))).Equals("12.345E+003"));
            TestFmwk.assertTrue("for062", ((BigDecimal.Parse("12345", numberStyle, provider)).Format(-1, 3, 3, 0, sci, (RoundingMode)(-1))).Equals("1.235E+004"));
            TestFmwk.assertTrue("for063", ((BigDecimal.Parse("1234.5", numberStyle, provider)).Format(4, 3, 2, 0, eng, (RoundingMode)(-1))).Equals("   1.235E+03"));
            TestFmwk.assertTrue("for064", ((BigDecimal.Parse("12345", numberStyle, provider)).Format(5, 3, 3, 0, eng, (RoundingMode)(-1))).Equals("   12.345E+003"));
            TestFmwk.assertTrue("for065", ((BigDecimal.Parse("12345", numberStyle, provider)).Format(6, 3, 3, 0, sci, (RoundingMode)(-1))).Equals("     1.235E+004"));

            TestFmwk.assertTrue("for066", ((BigDecimal.Parse("1.2345", numberStyle, provider)).Format(-1, 3, 2, 0, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.235    "));
            TestFmwk.assertTrue("for067", ((BigDecimal.Parse("12345.73", numberStyle, provider)).Format(-1, -1, 3, 6, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("12345.73     "));
            TestFmwk.assertTrue("for068", ((BigDecimal.Parse("12345e+5", numberStyle, provider)).Format(-1, 0)).Equals("1234500000"));
            TestFmwk.assertTrue("for069", ((BigDecimal.Parse("12345e+5", numberStyle, provider)).Format(-1, 1)).Equals("1234500000.0"));
            TestFmwk.assertTrue("for070", ((BigDecimal.Parse("12345e+5", numberStyle, provider)).Format(-1, 2)).Equals("1234500000.00"));
            TestFmwk.assertTrue("for071", ((BigDecimal.Parse("12345e+5", numberStyle, provider)).Format(-1, 3)).Equals("1234500000.000"));
            TestFmwk.assertTrue("for072", ((BigDecimal.Parse("12345e+5", numberStyle, provider)).Format(-1, 4)).Equals("1234500000.0000"));

            // some from ANSI Dallas [Nov 1998]
            TestFmwk.assertTrue("for073", ((BigDecimal.Parse("99.999", numberStyle, provider)).Format(-1, 2, -1, 2, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("100.00"));
            TestFmwk.assertTrue("for074", ((BigDecimal.Parse("0.99999", numberStyle, provider)).Format(-1, 4, 2, 2, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("1.0000    "));

            // try some rounding modes [default ROUND_HALF_UP widely tested above]
            // the first few also tests that defaults are accepted for the others
            d04 = BigDecimal.Parse("0.04", numberStyle, provider);
            d05 = BigDecimal.Parse("0.05", numberStyle, provider);
            d06 = BigDecimal.Parse("0.06", numberStyle, provider);
            d15 = BigDecimal.Parse("0.15", numberStyle, provider);
            TestFmwk.assertTrue("for080", (d05.Format(-1, 1)).Equals("0.1"));
            TestFmwk.assertTrue("for081", (d05.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfUp)).Equals("0.1"));
            TestFmwk.assertTrue("for082", (d05.Format(-1, 1, -1, -1, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("0.1"));
            TestFmwk.assertTrue("for083", (d05.Format(-1, -1, -1, -1, (ExponentForm)(-1), (RoundingMode)(-1))).Equals("0.05"));
            TestFmwk.assertTrue("for084", (d05.Format(-1, -1)).Equals("0.05"));
            try
            {
                d05.Format(-1, -1, -1, -1, (ExponentForm)(-1), (RoundingMode)30); // bad mode
                flag = false; // shouldn't get here
            }
            catch (ArgumentOutOfRangeException e94)
            {
                iae = e94;
                flag = (iae.Message).Contains("Bad argument 6 to format: 30");
            }
            TestFmwk.assertTrue("for085", flag);

            TestFmwk.assertTrue("for090", (d04.Format(-1, 1)).Equals("0.0"));
            TestFmwk.assertTrue("for091", (d06.Format(-1, 1)).Equals("0.1"));
            TestFmwk.assertTrue("for092", (d04.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfDown)).Equals("0.0"));
            TestFmwk.assertTrue("for093", (d05.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfDown)).Equals("0.0"));
            TestFmwk.assertTrue("for094", (d06.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfDown)).Equals("0.1"));

            TestFmwk.assertTrue("for095", (d04.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfEven)).Equals("0.0"));
            TestFmwk.assertTrue("for096", (d05.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfEven)).Equals("0.0"));
            TestFmwk.assertTrue("for097", (d06.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfEven)).Equals("0.1"));
            TestFmwk.assertTrue("for098", (d15.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfEven)).Equals("0.2"));
            d050 = BigDecimal.Parse("0.050", numberStyle, provider);
            d150 = BigDecimal.Parse("0.150", numberStyle, provider);
            TestFmwk.assertTrue("for099", (d050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfEven)).Equals("0.0"));
            TestFmwk.assertTrue("for100", (d150.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfEven)).Equals("0.2"));
            m050 = BigDecimal.Parse("-0.050", numberStyle, provider);
            m150 = BigDecimal.Parse("-0.150", numberStyle, provider);
            TestFmwk.assertTrue("for101", (m050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfEven)).Equals("0.0"));
            TestFmwk.assertTrue("for102", (m150.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfEven)).Equals("-0.2"));
            d051 = BigDecimal.Parse("0.051", numberStyle, provider);
            d151 = BigDecimal.Parse("0.151", numberStyle, provider);
            TestFmwk.assertTrue("for103", (d051.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfEven)).Equals("0.1"));
            TestFmwk.assertTrue("for104", (d151.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundHalfEven)).Equals("0.2"));

            TestFmwk.assertTrue("for105", (m050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundCeiling)).Equals("0.0"));
            TestFmwk.assertTrue("for106", (m150.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundCeiling)).Equals("-0.1"));
            TestFmwk.assertTrue("for107", (d050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundCeiling)).Equals("0.1"));
            TestFmwk.assertTrue("for108", (d150.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundCeiling)).Equals("0.2"));

            TestFmwk.assertTrue("for109", (m050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundFloor)).Equals("-0.1"));
            TestFmwk.assertTrue("for110", (m150.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundFloor)).Equals("-0.2"));
            TestFmwk.assertTrue("for111", (d050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundFloor)).Equals("0.0"));
            TestFmwk.assertTrue("for112", (d150.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundFloor)).Equals("0.1"));

            TestFmwk.assertTrue("for113", (m050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundUp)).Equals("-0.1"));
            TestFmwk.assertTrue("for114", (m150.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundUp)).Equals("-0.2"));
            TestFmwk.assertTrue("for115", (d050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundUp)).Equals("0.1"));
            TestFmwk.assertTrue("for116", (d150.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundUp)).Equals("0.2"));

            TestFmwk.assertTrue("for117", (m050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundDown)).Equals("0.0"));
            TestFmwk.assertTrue("for118", (m150.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundDown)).Equals("-0.1"));
            TestFmwk.assertTrue("for119", (d050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundDown)).Equals("0.0"));
            TestFmwk.assertTrue("for120", (d150.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundDown)).Equals("0.1"));

            d000 = BigDecimal.Parse("0.000", numberStyle, provider);
            d500 = BigDecimal.Parse("0.500", numberStyle, provider);
            TestFmwk.assertTrue("for121", (d000.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundUnnecessary)).Equals("0.0"));
            TestFmwk.assertTrue("for122", (d000.Format(-1, 2, -1, -1, (ExponentForm)(-1), MathContext.RoundUnnecessary)).Equals("0.00"));
            TestFmwk.assertTrue("for123", (d000.Format(-1, 3, -1, -1, (ExponentForm)(-1), MathContext.RoundUnnecessary)).Equals("0.000"));
            try
            { // this should trap..
                d050.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundUnnecessary);
                flag = false;
            }
            catch (ArithmeticException e95)
            {
                ae = e95;
                flag = (ae.Message).Equals("Rounding necessary");
            }
            TestFmwk.assertTrue("for124", flag);
            TestFmwk.assertTrue("for125", (d050.Format(-1, 2, -1, -1, (ExponentForm)(-1), MathContext.RoundUnnecessary)).Equals("0.05"));
            TestFmwk.assertTrue("for126", (d050.Format(-1, 3, -1, -1, (ExponentForm)(-1), MathContext.RoundUnnecessary)).Equals("0.050"));
            TestFmwk.assertTrue("for127", (d500.Format(-1, 1, -1, -1, (ExponentForm)(-1), MathContext.RoundUnnecessary)).Equals("0.5"));
            TestFmwk.assertTrue("for128", (d500.Format(-1, 2, -1, -1, (ExponentForm)(-1), MathContext.RoundUnnecessary)).Equals("0.50"));
            TestFmwk.assertTrue("for129", (d500.Format(-1, 3, -1, -1, (ExponentForm)(-1), MathContext.RoundUnnecessary)).Equals("0.500"));

            // bad negs --
            try
            {
                d050.Format(-2, -1, -1, -1, (ExponentForm)(-1), (RoundingMode)(-1));
                flag = false;
            }
            catch (ArgumentOutOfRangeException e96)
            {
                flag = true;
            }
            TestFmwk.assertTrue("for131", flag);
            try
            {
                d050.Format(-1, -2, -1, -1, (ExponentForm)(-1), (RoundingMode)(-1));
                flag = false;
            }
            catch (ArgumentOutOfRangeException e97)
            {
                flag = true;
            }
            TestFmwk.assertTrue("for132", flag);
            try
            {
                d050.Format(-1, -1, -2, -1, (ExponentForm)(-1), (RoundingMode)(-1));
                flag = false;
            }
            catch (ArgumentOutOfRangeException e98)
            {
                flag = true;
            }
            TestFmwk.assertTrue("for133", flag);
            try
            {
                d050.Format(-1, -1, -1, -2, (ExponentForm)(-1), (RoundingMode)(-1));
                flag = false;
            }
            catch (ArgumentOutOfRangeException e99)
            {
                flag = true;
            }
            TestFmwk.assertTrue("for134", flag);
            try
            {
                d050.Format(-1, -1, -1, -1, (ExponentForm)(-2), (RoundingMode)(-1));
                flag = false;
            }
            catch (ArgumentOutOfRangeException e100)
            {
                flag = true;
            }
            TestFmwk.assertTrue("for135", flag);
            try
            {
                d050.Format(-1, -1, -1, -1, (ExponentForm)(-1), (RoundingMode)(-2));
                flag = false;
            }
            catch (ArgumentOutOfRangeException e101)
            {
                flag = true;
            }
            TestFmwk.assertTrue("for136", flag);
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#hashCode} method. */

        [Test]
        public void diaghashcode()
        {
            string hs;
            BigDecimal d;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            hs = "27827817";
            d = BigDecimal.Parse(hs, numberStyle, provider);
            TestFmwk.assertTrue("has001", (d.GetHashCode()) == (hs.GetHashCode()));
            hs = "1.265E+200";
            d = BigDecimal.Parse(hs, numberStyle, provider);
            TestFmwk.assertTrue("has002", (d.GetHashCode()) == (hs.GetHashCode()));
            hs = "126.5E+200";
            d = BigDecimal.Parse(hs, numberStyle, provider);
            TestFmwk.assertTrue("has003", (d.GetHashCode()) != (hs.GetHashCode()));
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#intValue} method. */

        [Test]
        public void diagintvalue()
        {
            bool flag = false;
            string v = null;
            ArithmeticException ae = null;
            string[] badstrings;
            int i = 0;
            string norm = null;
            BigDecimal dimax;
            BigDecimal num = null;
            BigDecimal dv = null;
            BigDecimal dimin;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            // intValue --

            TestFmwk.assertTrue("inv001", imin == ((new BigDecimal(imin)).ToInt32()));
            TestFmwk.assertTrue("inv002", ((99)) == ((BigDecimal.Parse("99", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv003", ((1)) == ((BigDecimal.Parse("1", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv004", ((0)) == ((BigDecimal.Parse("0", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv005", ((-1)) == ((BigDecimal.Parse("-1", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv006", ((-99)) == ((BigDecimal.Parse("-99", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv007", imax == ((new BigDecimal(imax)).ToInt32()));
            TestFmwk.assertTrue("inv008", ((5)) == ((BigDecimal.Parse("5.0", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv009", ((5)) == ((BigDecimal.Parse("5.3", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv010", ((5)) == ((BigDecimal.Parse("5.5", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv011", ((5)) == ((BigDecimal.Parse("5.7", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv012", ((5)) == ((BigDecimal.Parse("5.9", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv013", ((-5)) == ((BigDecimal.Parse("-5.0", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv014", ((-5)) == ((BigDecimal.Parse("-5.3", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv015", ((-5)) == ((BigDecimal.Parse("-5.5", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv016", ((-5)) == ((BigDecimal.Parse("-5.7", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv017", ((-5)) == ((BigDecimal.Parse("-5.9", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv018", ((BigDecimal.Parse("88888888888", numberStyle, provider)).ToInt32()) == (-1305424328)); // ugh
            TestFmwk.assertTrue("inv019", ((BigDecimal.Parse("-88888888888", numberStyle, provider)).ToInt32()) == 1305424328); // ugh
            TestFmwk.assertTrue("inv020", ((imin)) == ((new BigDecimal((((long)imax)) + 1)).ToInt32()));
            TestFmwk.assertTrue("inv021", ((imax)) == ((new BigDecimal((((long)imin)) - 1)).ToInt32()));

            // intValueExact --

            TestFmwk.assertTrue("inv101", imin == ((new BigDecimal(imin)).ToInt32Exact()));
            TestFmwk.assertTrue("inv102", ((99)) == ((BigDecimal.Parse("99", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv103", ((1)) == ((BigDecimal.Parse("1", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv104", ((0)) == ((BigDecimal.Parse("0", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv105", ((-1)) == ((BigDecimal.Parse("-1", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv106", ((-99)) == ((BigDecimal.Parse("-99", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv107", imax == ((new BigDecimal(imax)).ToInt32()));
            TestFmwk.assertTrue("inv108", ((5)) == ((BigDecimal.Parse("5.0", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv109", ((-5)) == ((BigDecimal.Parse("-5.0", numberStyle, provider)).ToInt32()));
            TestFmwk.assertTrue("inv110", imax == ((new BigDecimal(imax)).ToInt32Exact()));

            try
            {
                v = "-88588688888";
                (BigDecimal.Parse(v, numberStyle, provider)).ToInt32Exact();
                flag = false;
            }
            catch (ArithmeticException e102)
            {
                ae = e102;
                flag = (ae.Message).Equals("Conversion overflow:" + " " + v);
            }
            TestFmwk.assertTrue("inv200", flag);

            // this one could raise either overflow or bad decimal part
            try
            {
                v = "88088818888.00001";
                (BigDecimal.Parse(v, numberStyle, provider)).ToInt32Exact();
                flag = false;
            }
            catch (ArithmeticException e103)
            {
                flag = true;
            }
            TestFmwk.assertTrue("inv201", flag);

            // 1999.10.28: the testcases marked '*' failed
            badstrings = new string[] {
                "12345678901",
                (new BigDecimal(imax)).Add(one).ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(imin)).Subtract(one)
                        .ToString(CultureInfo.InvariantCulture),
                "3731367293",
                "4731367293",
                "5731367293",
                "6731367293",
                "7731367293",
                "8731367293",
                "9731367293",
                "-3731367293",
                "-4731367293",
                "-5731367293",
                "-6731367293",
                "-7731367293",
                "-8731367293",
                "-9731367293",
                (new BigDecimal(imin)).Multiply(two)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(imax)).Multiply(two)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(imin)).Multiply(ten)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(imax)).Multiply(ten)
                        .ToString(CultureInfo.InvariantCulture), "4731367293", "4831367293", "4931367293",
                "5031367293", "5131367293", "5231367293", "5331367293",
                "5431367293", "5531367293", "5631367293", "5731367293",
                "5831367293", "5931367293", "6031367293", "6131367293",
                "6231367293", "6331367293", "6431367293", "6531367293",
                "6631367293", "6731367293", "2200000000", "2300000000",
                "2400000000", "2500000000", "2600000000", "2700000000",
                "2800000000", "2900000000", "-2200000000", "-2300000000",
                "-2400000000", "-2500000000", "-2600000000", "-2700000000",
                "-2800000000", "-2900000000", "25E+8", "-25E+8", "-12345678901" }; // 220
                                                                                   // 221
                                                                                   // 222
                                                                                   // 223
                                                                                   // 224
                                                                                   // 225 *
                                                                                   // 226
                                                                                   // 227
                                                                                   // 228
                                                                                   // 229 *
                                                                                   // 230
                                                                                   // 231
                                                                                   // 232 *
                                                                                   // 233
                                                                                   // 234
                                                                                   // 235
                                                                                   // 236 *
                                                                                   // 237
                                                                                   // 238
                                                                                   // 239
                                                                                   // 240
                                                                                   // 241
                                                                                   // 242 *
                                                                                   // 243 *
                                                                                   // 244 *
                                                                                   // 245 *
                                                                                   // 246 *
                                                                                   // 247 *
                                                                                   // 248 *
                                                                                   // 249 *
                                                                                   // 250 *
                                                                                   // 251 *
                                                                                   // 252 *
                                                                                   // 253 *
                                                                                   // 254 *
                                                                                   // 255 *
                                                                                   // 256 *
                                                                                   // 257 *
                                                                                   // 258 *
                                                                                   // 259
                                                                                   // 260
                                                                                   // 261
                                                                                   // 262
                                                                                   // 263
                                                                                   // 264
                                                                                   // 265
                                                                                   // 266
                                                                                   // 267
                                                                                   // 268
                                                                                   // 269
                                                                                   // 270
                                                                                   // 271
                                                                                   // 272
                                                                                   // 273
                                                                                   // 274
                                                                                   // 275
                                                                                   // 276
                                                                                   // 277
                                                                                   // 278
                                                                                   // 279
                                                                                   // 280
            {
                int e104 = badstrings.Length;
                i = 0;
                for (; e104 > 0; e104--, i++)
                {
                    try
                    {
                        v = badstrings[i];
                        (BigDecimal.Parse(v, numberStyle, provider)).ToInt32Exact();
                        flag = false;
                    }
                    catch (ArithmeticException e105)
                    {
                        ae = e105;
                        norm = (BigDecimal.Parse(v, numberStyle, provider)).ToString(CultureInfo.InvariantCulture);
                        flag = (ae.Message).Equals("Conversion overflow:"
                                + " " + norm);
                    }
                    TestFmwk.assertTrue("inv" + (220 + i), flag);
                }
            }/* i */

            // now slip in some single bits...
            dimax = new BigDecimal(imax);
            {
                i = 0;
                for (; i <= 49; i++)
                {
                    try
                    {
                        num = two.Pow(new BigDecimal(i), mc50);
                        dv = dimax.Add(num, mc50);
                        dv.ToInt32Exact();
                        flag = false;
                    }
                    catch (ArithmeticException e106)
                    {
                        ae = e106;
                        norm = dv.ToString(CultureInfo.InvariantCulture);
                        flag = (ae.Message).Equals("Conversion overflow:"
                                + " " + norm);
                    }
                    TestFmwk.assertTrue("inv" + (300 + i), flag);
                }
            }/* i */
            dimin = new BigDecimal(imin);
            {
                i = 50;
                for (; i <= 99; i++)
                {
                    try
                    {
                        num = two.Pow(new BigDecimal(i), mc50);
                        dv = dimin.Subtract(num, mc50);
                        dv.ToInt32Exact();
                        flag = false;
                    }
                    catch (ArithmeticException e107)
                    {
                        ae = e107;
                        norm = dv.ToString(CultureInfo.InvariantCulture);
                        flag = (ae.Message).Equals("Conversion overflow:"
                                + " " + norm);
                    }
                    TestFmwk.assertTrue("inv" + (300 + i), flag);
                }
            }/* i */

            // the following should all raise bad-decimal-part exceptions
            badstrings = new string[] { "0.09", "0.9", "0.01", "0.1",
                "-0.01", "-0.1", "1.01", "-1.01", "-1.1", "-111.111",
                "+111.111", "1.09", "1.05", "1.04", "1.99", "1.9", "1.5",
                "1.4", "-1.09", "-1.05", "-1.04", "-1.99", "-1.9", "-1.5",
                "-1.4", "1E-1000", "-1E-1000", "11E-1", "1.5" }; // 400-403
                                                                 // 404-407
                                                                 // 408-411
                                                                 // 412-416
                                                                 // 417-420
                                                                 // 421-424
                                                                 // 425-428

            {
                int len108 = badstrings.Length;
                i = 0;
                for (; len108 > 0; len108--, i++)
                {
                    try
                    {
                        v = badstrings[i];
                        (BigDecimal.Parse(v, numberStyle, provider)).ToInt32Exact();
                        flag = false;
                    }
                    catch (ArithmeticException e109)
                    {
                        ae = e109;
                        norm = (BigDecimal.Parse(v, numberStyle, provider)).ToString(CultureInfo.InvariantCulture);
                        flag = (ae.Message).Equals("Decimal part non-zero:"
                                + " " + norm);
                    }
                    TestFmwk.assertTrue("inv" + (400 + i), flag);
                }
            }/* i */
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#longValue} method. */

        [Test]
        public void diaglongvalue()
        {
            bool flag = false;
            string v = null;
            ArithmeticException ae = null;
            string[] badstrings;
            int i = 0;
            string norm = null;
            BigDecimal dlmax;
            BigDecimal num = null;
            BigDecimal dv = null;
            BigDecimal dlmin;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            // longValue --

            TestFmwk.assertTrue("lov001", lmin == ((new BigDecimal(lmin)).ToInt64()));
            TestFmwk.assertTrue("lov002", ((99)) == ((BigDecimal.Parse("99", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov003", ((1)) == ((BigDecimal.Parse("1", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov004", ((0)) == ((BigDecimal.Parse("0", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov005", ((-1)) == ((BigDecimal.Parse("-1", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov006", ((-99)) == ((BigDecimal.Parse("-99", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov007", lmax == ((new BigDecimal(lmax)).ToInt64()));
            TestFmwk.assertTrue("lov008", ((5)) == ((BigDecimal.Parse("5.0", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov009", ((5)) == ((BigDecimal.Parse("5.3", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov010", ((5)) == ((BigDecimal.Parse("5.5", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov011", ((5)) == ((BigDecimal.Parse("5.7", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov012", ((5)) == ((BigDecimal.Parse("5.9", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov013", ((-5)) == ((BigDecimal.Parse("-5.0", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov014", ((-5)) == ((BigDecimal.Parse("-5.3", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov015", ((-5)) == ((BigDecimal.Parse("-5.5", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov016", ((-5)) == ((BigDecimal.Parse("-5.7", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov017", ((-5)) == ((BigDecimal.Parse("-5.9", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov018", ((BigDecimal.Parse("888888888899999999998", numberStyle, provider)).ToInt64()) == 3445173361941522430L); // ugh
            TestFmwk.assertTrue("lov019", ((BigDecimal.Parse("-888888888899999999998", numberStyle, provider)).ToInt64()) == (-3445173361941522430L)); // ugh

            // longValueExact --

            TestFmwk.assertTrue("lov101", lmin == ((new BigDecimal(lmin)).ToInt64()));
            TestFmwk.assertTrue("lov102", ((99)) == ((BigDecimal.Parse("99", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov103", ((1)) == ((BigDecimal.Parse("1", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov104", ((0)) == ((BigDecimal.Parse("0", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov105", ((-1)) == ((BigDecimal.Parse("-1", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov106", ((-99)) == ((BigDecimal.Parse("-99", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov107", lmax == ((new BigDecimal(lmax)).ToInt64()));
            TestFmwk.assertTrue("lov108", ((5)) == ((BigDecimal.Parse("5.0", numberStyle, provider)).ToInt64()));
            TestFmwk.assertTrue("lov109", ((-5)) == ((BigDecimal.Parse("-5.0", numberStyle, provider)).ToInt64()));

            try
            {
                v = "-888888888899999999998";
                (BigDecimal.Parse(v, numberStyle, provider)).ToInt64Exact();
                flag = false;
            }
            catch (ArithmeticException e110)
            {
                ae = e110;
                flag = (ae.Message).Equals("Conversion overflow:" + " " + v);
            }
            TestFmwk.assertTrue("lov200", flag);
            try
            {
                v = "88888887487487479488888";
                (BigDecimal.Parse(v, numberStyle, provider)).ToInt64Exact();
                flag = false;
            }
            catch (ArithmeticException e111)
            {
                ae = e111;
                flag = (ae.Message).Equals("Conversion overflow:" + " " + v);
            }
            TestFmwk.assertTrue("lov201", flag);
            try
            {
                v = "1.5";
                (BigDecimal.Parse(v, numberStyle, provider)).ToInt64Exact();
                flag = false;
            }
            catch (ArithmeticException e112)
            {
                ae = e112;
                flag = (ae.Message).Equals("Decimal part non-zero:" + " " + v);
            }
            TestFmwk.assertTrue("lov202", flag);

            badstrings = new string[] {
                "1234567890110987654321",
                "-1234567890110987654321",
                (new BigDecimal(lmax)).Add(one).ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(lmin)).Subtract(one)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(lmin)).Multiply(two)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(lmax)).Multiply(two)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(lmin)).Multiply(ten)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(lmax)).Multiply(ten)
                        .ToString(CultureInfo.InvariantCulture), "9223372036854775818",
                "9323372036854775818", "9423372036854775818",
                "9523372036854775818", "9623372036854775818",
                "9723372036854775818", "9823372036854775818",
                "9923372036854775818", "-9223372036854775818",
                "-9323372036854775818", "-9423372036854775818",
                "-9523372036854775818", "-9623372036854775818",
                "-9723372036854775818", "-9823372036854775818",
                "-9923372036854775818", "12345678901234567890" }; // 220
                                                                  // 221
                                                                  // 222
                                                                  // 223
                                                                  // 224
                                                                  // 225
                                                                  // 226
                                                                  // 227
                                                                  // 228
                                                                  // 229
                                                                  // 230
                                                                  // 231
                                                                  // 232
                                                                  // 233
                                                                  // 234
                                                                  // 235
                                                                  // 236
                                                                  // 237
                                                                  // 238
                                                                  // 239
                                                                  // 240
                                                                  // 241
                                                                  // 242
                                                                  // 243
                                                                  // 244
            {
                int e113 = badstrings.Length;
                i = 0;
                for (; e113 > 0; e113--, i++)
                {
                    try
                    {
                        v = badstrings[i];
                        (BigDecimal.Parse(v, numberStyle, provider)).ToInt64Exact();
                        flag = false;
                    }
                    catch (ArithmeticException e114)
                    {
                        ae = e114;
                        norm = (BigDecimal.Parse(v, numberStyle, provider)).ToString(CultureInfo.InvariantCulture);
                        flag = (ae.Message).Equals("Conversion overflow:"
                                + " " + norm);
                    }
                    TestFmwk.assertTrue("lov" + (220 + i), flag);
                }
            }/* i */

            // now slip in some single bits...
            dlmax = new BigDecimal(lmax);
            {
                i = 0;
                for (; i <= 99; i++)
                {
                    try
                    {
                        num = two.Pow(new BigDecimal(i), mc50);
                        dv = dlmax.Add(num, mc50);
                        dv.ToInt64Exact();
                        flag = false;
                    }
                    catch (ArithmeticException e115)
                    {
                        ae = e115;
                        norm = dv.ToString(CultureInfo.InvariantCulture);
                        flag = (ae.Message).Equals("Conversion overflow:"
                                + " " + norm);
                    }
                    TestFmwk.assertTrue("lov" + (300 + i), flag);
                }
            }/* i */
            dlmin = new BigDecimal(lmin);
            {
                i = 0;
                for (; i <= 99; i++)
                {
                    try
                    {
                        num = two.Pow(new BigDecimal(i), mc50);
                        dv = dlmin.Subtract(num, mc50);
                        dv.ToInt64Exact();
                        flag = false;
                    }
                    catch (ArithmeticException e116)
                    {
                        ae = e116;
                        norm = dv.ToString(CultureInfo.InvariantCulture);
                        flag = (ae.Message).Equals("Conversion overflow:"
                                + " " + norm);
                    }
                    TestFmwk.assertTrue("lov" + (400 + i), flag);
                }
            }/* i */
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#movePointLeft} method. */

        [Test]
        public void diagmovepointleft()
        {
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("mpl001", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointLeft(-10).ToString(CultureInfo.InvariantCulture)).Equals("-10000000000"));
            TestFmwk.assertTrue("mpl002", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointLeft(-5).ToString(CultureInfo.InvariantCulture)).Equals("-100000"));
            TestFmwk.assertTrue("mpl003", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointLeft(-1).ToString(CultureInfo.InvariantCulture)).Equals("-10"));
            TestFmwk.assertTrue("mpl004", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointLeft(0).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("mpl005", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointLeft(+1).ToString(CultureInfo.InvariantCulture)).Equals("-0.1"));
            TestFmwk.assertTrue("mpl006", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointLeft(+5).ToString(CultureInfo.InvariantCulture)).Equals("-0.00001"));
            TestFmwk.assertTrue("mpl007", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointLeft(+10).ToString(CultureInfo.InvariantCulture)).Equals("-0.0000000001"));

            TestFmwk.assertTrue("mpl010", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointLeft(-10).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("mpl010", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointLeft(-5).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("mpl010", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointLeft(-1).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("mpl010", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointLeft(0).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("mpl010", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointLeft(+1).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("mpl010", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointLeft(+5).ToString(CultureInfo.InvariantCulture)).Equals("0.00000"));
            TestFmwk.assertTrue("mpl010", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointLeft(+10).ToString(CultureInfo.InvariantCulture)).Equals("0.0000000000"));

            TestFmwk.assertTrue("mpl020", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointLeft(-10).ToString(CultureInfo.InvariantCulture)).Equals("10000000000"));
            TestFmwk.assertTrue("mpl021", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointLeft(-5).ToString(CultureInfo.InvariantCulture)).Equals("100000"));
            TestFmwk.assertTrue("mpl022", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointLeft(-1).ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("mpl023", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointLeft(0).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("mpl024", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointLeft(+1).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("mpl025", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointLeft(+5).ToString(CultureInfo.InvariantCulture)).Equals("0.00001"));
            TestFmwk.assertTrue("mpl026", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointLeft(+10).ToString(CultureInfo.InvariantCulture)).Equals("0.0000000001"));

            TestFmwk.assertTrue("mpl030", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointLeft(-10).ToString(CultureInfo.InvariantCulture)).Equals("50000000000"));
            TestFmwk.assertTrue("mpl031", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointLeft(-5).ToString(CultureInfo.InvariantCulture)).Equals("500000"));
            TestFmwk.assertTrue("mpl032", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointLeft(-1).ToString(CultureInfo.InvariantCulture)).Equals("50"));
            TestFmwk.assertTrue("mpl033", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointLeft(0).ToString(CultureInfo.InvariantCulture)).Equals("5"));
            TestFmwk.assertTrue("mpl034", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointLeft(+1).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            TestFmwk.assertTrue("mpl035", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointLeft(+5).ToString(CultureInfo.InvariantCulture)).Equals("0.00005"));
            TestFmwk.assertTrue("mpl036", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointLeft(+10).ToString(CultureInfo.InvariantCulture)).Equals("0.0000000005"));
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#movePointRight} method. */

        [Test]
        public void diagmovepointright()
        {
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("mpr001", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointRight(+10).ToString(CultureInfo.InvariantCulture)).Equals("-10000000000"));
            TestFmwk.assertTrue("mpr002", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointRight(+5).ToString(CultureInfo.InvariantCulture)).Equals("-100000"));
            TestFmwk.assertTrue("mpr003", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointRight(+1).ToString(CultureInfo.InvariantCulture)).Equals("-10"));
            TestFmwk.assertTrue("mpr004", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointRight(0).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("mpr005", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointRight(-1).ToString(CultureInfo.InvariantCulture)).Equals("-0.1"));
            TestFmwk.assertTrue("mpr006", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointRight(-5).ToString(CultureInfo.InvariantCulture)).Equals("-0.00001"));
            TestFmwk.assertTrue("mpr007", ((BigDecimal.Parse("-1", numberStyle, provider)).MovePointRight(-10).ToString(CultureInfo.InvariantCulture)).Equals("-0.0000000001"));

            TestFmwk.assertTrue("mpr010", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointRight(+10).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("mpr011", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointRight(+5).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("mpr012", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointRight(+1).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("mpr013", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointRight(0).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("mpr014", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointRight(-1).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("mpr015", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointRight(-5).ToString(CultureInfo.InvariantCulture)).Equals("0.00000"));
            TestFmwk.assertTrue("mpr016", ((BigDecimal.Parse("0", numberStyle, provider)).MovePointRight(-10).ToString(CultureInfo.InvariantCulture)).Equals("0.0000000000"));

            TestFmwk.assertTrue("mpr020", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointRight(+10).ToString(CultureInfo.InvariantCulture)).Equals("10000000000"));
            TestFmwk.assertTrue("mpr021", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointRight(+5).ToString(CultureInfo.InvariantCulture)).Equals("100000"));
            TestFmwk.assertTrue("mpr022", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointRight(+1).ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("mpr023", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointRight(0).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("mpr024", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointRight(-1).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("mpr025", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointRight(-5).ToString(CultureInfo.InvariantCulture)).Equals("0.00001"));
            TestFmwk.assertTrue("mpr026", ((BigDecimal.Parse("+1", numberStyle, provider)).MovePointRight(-10).ToString(CultureInfo.InvariantCulture)).Equals("0.0000000001"));

            TestFmwk.assertTrue("mpr030", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointRight(+10).ToString(CultureInfo.InvariantCulture)).Equals("50000000000"));
            TestFmwk.assertTrue("mpr031", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointRight(+5).ToString(CultureInfo.InvariantCulture)).Equals("500000"));
            TestFmwk.assertTrue("mpr032", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointRight(+1).ToString(CultureInfo.InvariantCulture)).Equals("50"));
            TestFmwk.assertTrue("mpr033", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointRight(0).ToString(CultureInfo.InvariantCulture)).Equals("5"));
            TestFmwk.assertTrue("mpr034", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointRight(-1).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            TestFmwk.assertTrue("mpr035", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointRight(-5).ToString(CultureInfo.InvariantCulture)).Equals("0.00005"));
            TestFmwk.assertTrue("mpr036", ((BigDecimal.Parse("0.5E+1", numberStyle, provider)).MovePointRight(-10).ToString(CultureInfo.InvariantCulture)).Equals("0.0000000005"));
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#scale} method. */

        [Test]
        public void diagscale()
        {
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("sca001", ((BigDecimal.Parse("-1", numberStyle, provider)).Scale) == 0);
            TestFmwk.assertTrue("sca002", ((BigDecimal.Parse("-10", numberStyle, provider)).Scale) == 0);
            TestFmwk.assertTrue("sca003", ((BigDecimal.Parse("+1", numberStyle, provider)).Scale) == 0);
            TestFmwk.assertTrue("sca004", ((BigDecimal.Parse("+10", numberStyle, provider)).Scale) == 0);
            TestFmwk.assertTrue("sca005", ((BigDecimal.Parse("1E+10", numberStyle, provider)).Scale) == 0);
            TestFmwk.assertTrue("sca006", ((BigDecimal.Parse("1E-10", numberStyle, provider)).Scale) == 10);
            TestFmwk.assertTrue("sca007", ((BigDecimal.Parse("0E-10", numberStyle, provider)).Scale) == 0);
            TestFmwk.assertTrue("sca008", ((BigDecimal.Parse("0.000", numberStyle, provider)).Scale) == 3);
            TestFmwk.assertTrue("sca009", ((BigDecimal.Parse("0.00", numberStyle, provider)).Scale) == 2);
            TestFmwk.assertTrue("sca010", ((BigDecimal.Parse("0.0", numberStyle, provider)).Scale) == 1);
            TestFmwk.assertTrue("sca011", ((BigDecimal.Parse("0.1", numberStyle, provider)).Scale) == 1);
            TestFmwk.assertTrue("sca012", ((BigDecimal.Parse("0.12", numberStyle, provider)).Scale) == 2);
            TestFmwk.assertTrue("sca013", ((BigDecimal.Parse("0.123", numberStyle, provider)).Scale) == 3);
            TestFmwk.assertTrue("sca014", ((BigDecimal.Parse("-0.0", numberStyle, provider)).Scale) == 1);
            TestFmwk.assertTrue("sca015", ((BigDecimal.Parse("-0.1", numberStyle, provider)).Scale) == 1);
            TestFmwk.assertTrue("sca016", ((BigDecimal.Parse("-0.12", numberStyle, provider)).Scale) == 2);
            TestFmwk.assertTrue("sca017", ((BigDecimal.Parse("-0.123", numberStyle, provider)).Scale) == 3);
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#setScale} method. */

        [Test]
        public void diagsetscale()
        {
            bool flag = false;
            Exception e = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("ssc001", ((BigDecimal.Parse("-1", numberStyle, provider)).SetScale(0).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("ssc002", ((BigDecimal.Parse("-1", numberStyle, provider)).SetScale(1).ToString(CultureInfo.InvariantCulture)).Equals("-1.0"));
            TestFmwk.assertTrue("ssc003", ((BigDecimal.Parse("-1", numberStyle, provider)).SetScale(2).ToString(CultureInfo.InvariantCulture)).Equals("-1.00"));
            TestFmwk.assertTrue("ssc004", ((BigDecimal.Parse("0", numberStyle, provider)).SetScale(0).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("ssc005", ((BigDecimal.Parse("0", numberStyle, provider)).SetScale(1).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("ssc006", ((BigDecimal.Parse("0", numberStyle, provider)).SetScale(2).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("ssc007", ((BigDecimal.Parse("+1", numberStyle, provider)).SetScale(0).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("ssc008", ((BigDecimal.Parse("+1", numberStyle, provider)).SetScale(1).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("ssc009", ((BigDecimal.Parse("+1", numberStyle, provider)).SetScale(2).ToString(CultureInfo.InvariantCulture)).Equals("1.00"));
            TestFmwk.assertTrue("ssc010", ((BigDecimal.Parse("-1", numberStyle, provider)).SetScale(0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("ssc011", ((BigDecimal.Parse("-1", numberStyle, provider)).SetScale(1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("-1.0"));
            TestFmwk.assertTrue("ssc012", ((BigDecimal.Parse("-1", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("-1.00"));
            TestFmwk.assertTrue("ssc013", ((BigDecimal.Parse("0", numberStyle, provider)).SetScale(0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("ssc014", ((BigDecimal.Parse("0", numberStyle, provider)).SetScale(1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("ssc015", ((BigDecimal.Parse("0", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("ssc016", ((BigDecimal.Parse("+1", numberStyle, provider)).SetScale(0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("ssc017", ((BigDecimal.Parse("+1", numberStyle, provider)).SetScale(1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("ssc018", ((BigDecimal.Parse("+1", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.00"));

            TestFmwk.assertTrue("ssc020", ((BigDecimal.Parse("1.04", numberStyle, provider)).SetScale(3, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.040"));
            TestFmwk.assertTrue("ssc021", ((BigDecimal.Parse("1.04", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.04"));
            TestFmwk.assertTrue("ssc022", ((BigDecimal.Parse("1.04", numberStyle, provider)).SetScale(1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("ssc023", ((BigDecimal.Parse("1.04", numberStyle, provider)).SetScale(0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("ssc024", ((BigDecimal.Parse("1.05", numberStyle, provider)).SetScale(3, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.050"));
            TestFmwk.assertTrue("ssc025", ((BigDecimal.Parse("1.05", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.05"));
            TestFmwk.assertTrue("ssc026", ((BigDecimal.Parse("1.05", numberStyle, provider)).SetScale(1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.1"));
            TestFmwk.assertTrue("ssc027", ((BigDecimal.Parse("1.05", numberStyle, provider)).SetScale(0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("ssc028", ((BigDecimal.Parse("1.05", numberStyle, provider)).SetScale(3, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("1.050"));
            TestFmwk.assertTrue("ssc029", ((BigDecimal.Parse("1.05", numberStyle, provider)).SetScale(2, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("1.05"));
            TestFmwk.assertTrue("ssc030", ((BigDecimal.Parse("1.05", numberStyle, provider)).SetScale(1, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("1.0"));
            TestFmwk.assertTrue("ssc031", ((BigDecimal.Parse("1.05", numberStyle, provider)).SetScale(0, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("ssc032", ((BigDecimal.Parse("1.06", numberStyle, provider)).SetScale(3, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.060"));
            TestFmwk.assertTrue("ssc033", ((BigDecimal.Parse("1.06", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.06"));
            TestFmwk.assertTrue("ssc034", ((BigDecimal.Parse("1.06", numberStyle, provider)).SetScale(1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.1"));
            TestFmwk.assertTrue("ssc035", ((BigDecimal.Parse("1.06", numberStyle, provider)).SetScale(0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1"));

            TestFmwk.assertTrue("ssc040", ((BigDecimal.Parse("-10", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("-10.00"));
            TestFmwk.assertTrue("ssc041", ((BigDecimal.Parse("+1", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("1.00"));
            TestFmwk.assertTrue("ssc042", ((BigDecimal.Parse("+10", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("10.00"));
            TestFmwk.assertTrue("ssc043", ((BigDecimal.Parse("1E+10", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("10000000000.00"));
            TestFmwk.assertTrue("ssc044", ((BigDecimal.Parse("1E-10", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("ssc045", ((BigDecimal.Parse("1E-2", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.01"));
            TestFmwk.assertTrue("ssc046", ((BigDecimal.Parse("0E-10", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));

            // check rounding
            TestFmwk.assertTrue("ssc050", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(2, MathContext.RoundCeiling).ToString(CultureInfo.InvariantCulture)).Equals("0.01"));
            TestFmwk.assertTrue("ssc051", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(1, MathContext.RoundCeiling).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("ssc052", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(0, MathContext.RoundCeiling).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("ssc053", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(2, MathContext.RoundDown).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("ssc054", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(1, MathContext.RoundDown).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("ssc055", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(0, MathContext.RoundDown).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("ssc056", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(2, MathContext.RoundFloor).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("ssc057", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(1, MathContext.RoundFloor).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("ssc058", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(0, MathContext.RoundFloor).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("ssc059", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(2, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("ssc060", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(1, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("ssc061", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(0, MathContext.RoundHalfDown).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("ssc062", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(2, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("ssc063", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(1, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("ssc064", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(0, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("ssc065", ((BigDecimal.Parse("0.015", numberStyle, provider)).SetScale(2, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.02"));
            TestFmwk.assertTrue("ssc066", ((BigDecimal.Parse("0.015", numberStyle, provider)).SetScale(1, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("ssc067", ((BigDecimal.Parse("0.015", numberStyle, provider)).SetScale(0, MathContext.RoundHalfEven).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("ssc068", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.01"));
            TestFmwk.assertTrue("ssc069", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("ssc070", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("ssc071", ((BigDecimal.Parse("0.095", numberStyle, provider)).SetScale(2, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.10"));
            TestFmwk.assertTrue("ssc072", ((BigDecimal.Parse("0.095", numberStyle, provider)).SetScale(1, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("ssc073", ((BigDecimal.Parse("0.095", numberStyle, provider)).SetScale(0, MathContext.RoundHalfUp).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("ssc074", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(2, MathContext.RoundUp).ToString(CultureInfo.InvariantCulture)).Equals("0.01"));
            TestFmwk.assertTrue("ssc075", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(1, MathContext.RoundUp).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            TestFmwk.assertTrue("ssc076", ((BigDecimal.Parse("0.005", numberStyle, provider)).SetScale(0, MathContext.RoundUp).ToString(CultureInfo.InvariantCulture)).Equals("1"));

            try
            {
                (new BigDecimal(1)).SetScale(-8);
                flag = false;
            }
            catch (Exception e117)
            {
                e = e117;
                flag = (e.Message).Equals("Negative scale: -8");
            }/* checkscale */
            TestFmwk.assertTrue("ssc100", flag);
            try
            {
                (new BigDecimal(1.0001D)).SetScale(3);
                flag = false;
            }
            catch (Exception e118)
            {
                e = e118;
                flag = (e.Message).Equals("Rounding necessary");
            }/* checkrunn */
            TestFmwk.assertTrue("ssc101", flag);
            try
            {
                (new BigDecimal(1E-8D)).SetScale(3);
                flag = false;
            }
            catch (Exception e119)
            {
                e = e119;
                flag = (e.Message).Equals("Rounding necessary");
            }/* checkrunn */
            TestFmwk.assertTrue("ssc102", flag);
        }

        /* ----------------------------------------------------------------- */

        /** Test the <code>BigDecimal.ToInt16()</code> method. */

        [Test]
        public void diagshortvalue()
        {
            bool flag = false;
            string v = null;
            ArithmeticException ae = null;
            string[] badstrings;
            int i = 0;
            string norm = null;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("shv002", (((short)0)) == ((BigDecimal.Parse("0", numberStyle, provider)).ToInt16()));
            TestFmwk.assertTrue("shv003", (((short)1)) == ((BigDecimal.Parse("1", numberStyle, provider)).ToInt16()));
            TestFmwk.assertTrue("shv004", (((short)99)) == ((BigDecimal.Parse("99", numberStyle, provider)).ToInt16()));
            TestFmwk.assertTrue("shv006", ((smax)) == ((new BigDecimal(smax)).ToInt16()));
            TestFmwk.assertTrue("shv007", ((smin)) == ((new BigDecimal(smin)).ToInt16()));
            TestFmwk.assertTrue("shv008", ((sneg)) == ((new BigDecimal(sneg)).ToInt16()));
            TestFmwk.assertTrue("shv009", ((szer)) == ((new BigDecimal(szer)).ToInt16()));
            TestFmwk.assertTrue("shv010", ((spos)) == ((new BigDecimal(spos)).ToInt16()));
            TestFmwk.assertTrue("shv011", ((smin)) == ((new BigDecimal(smax + 1)).ToInt16()));
            TestFmwk.assertTrue("shv012", ((smax)) == ((new BigDecimal(smin - 1)).ToInt16()));

            TestFmwk.assertTrue("shv022", (((short)0)) == ((BigDecimal.Parse("0", numberStyle, provider)).ToInt16Exact()));
            TestFmwk.assertTrue("shv023", (((short)1)) == ((BigDecimal.Parse("1", numberStyle, provider)).ToInt16Exact()));
            TestFmwk.assertTrue("shv024", (((short)99)) == ((BigDecimal.Parse("99", numberStyle, provider)).ToInt16Exact()));
            TestFmwk.assertTrue("shv026", ((smax)) == ((new BigDecimal(smax)).ToInt16Exact()));
            TestFmwk.assertTrue("shv027", ((smin)) == ((new BigDecimal(smin)).ToInt16Exact()));
            TestFmwk.assertTrue("shv028", ((sneg)) == ((new BigDecimal(sneg)).ToInt16Exact()));
            TestFmwk.assertTrue("shv029", ((szer)) == ((new BigDecimal(szer)).ToInt16Exact()));
            TestFmwk.assertTrue("shv030", ((spos)) == ((new BigDecimal(spos)).ToInt16Exact()));
            try
            {
                v = "-88888888888";
                (BigDecimal.Parse(v, numberStyle, provider)).ToInt16Exact();
                flag = false;
            }
            catch (ArithmeticException e120)
            {
                ae = e120;
                flag = (ae.Message).Equals("Conversion overflow:" + " " + v);
            }
            TestFmwk.assertTrue("shv100", flag);
            try
            {
                v = "88888888888";
                (BigDecimal.Parse(v, numberStyle, provider)).ToInt16Exact();
                flag = false;
            }
            catch (ArithmeticException e121)
            {
                ae = e121;
                flag = (ae.Message).Equals("Conversion overflow:" + " " + v);
            }
            TestFmwk.assertTrue("shv101", flag);
            try
            {
                v = "1.5";
                (BigDecimal.Parse(v, numberStyle, provider)).ToInt16Exact();
                flag = false;
            }
            catch (ArithmeticException e122)
            {
                ae = e122;
                flag = (ae.Message).Equals("Decimal part non-zero:" + " " + v);
            }
            TestFmwk.assertTrue("shv102", flag);

            badstrings = new string[] {
                "123456",
                (new BigDecimal(smax)).Add(one).ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(smin)).Subtract(one)
                        .ToString(CultureInfo.InvariantCulture),
                "71111",
                "81111",
                "91111",
                "-71111",
                "-81111",
                "-91111",
                (new BigDecimal(smin)).Multiply(two)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(smax)).Multiply(two)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(smin)).Multiply(ten)
                        .ToString(CultureInfo.InvariantCulture),
                (new BigDecimal(smax)).Multiply(ten)
                        .ToString(CultureInfo.InvariantCulture), "-123456" }; // 220
                                                                              // 221
                                                                              // 222
                                                                              // 223
                                                                              // 224
                                                                              // 225
                                                                              // 226
                                                                              // 227
                                                                              // 228
                                                                              // 229
                                                                              // 230
                                                                              // 231
                                                                              // 232
                                                                              // 233
            {
                int len123 = badstrings.Length;
                i = 0;
                for (; len123 > 0; len123--, i++)
                {
                    try
                    {
                        v = badstrings[i];
                        (BigDecimal.Parse(v, numberStyle, provider)).ToInt16Exact();
                        flag = false;
                    }
                    catch (ArithmeticException e124)
                    {
                        ae = e124;
                        norm = (BigDecimal.Parse(v, numberStyle, provider)).ToString(CultureInfo.InvariantCulture);
                        flag = (ae.Message).Equals("Conversion overflow:"
                                + " " + norm);
                    }
                    TestFmwk.assertTrue("shv" + (220 + i), flag);
                }
            }/* i */
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#signum} method. */

        [Test]
        public void diagsignum()
        {
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            // necessarily checks some obscure constructions, too
            TestFmwk.assertTrue("sig001", (-1) == ((BigDecimal.Parse("-1", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig002", (-1) == ((BigDecimal.Parse("-0.0010", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig003", (-1) == ((BigDecimal.Parse("-0.001", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig004", 0 == ((BigDecimal.Parse("-0.00", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig005", 0 == ((BigDecimal.Parse("-0", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig006", 0 == ((BigDecimal.Parse("0", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig007", 0 == ((BigDecimal.Parse("00", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig008", 0 == ((BigDecimal.Parse("00.0", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig009", 1 == ((BigDecimal.Parse("00.01", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig010", 1 == ((BigDecimal.Parse("00.01", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig011", 1 == ((BigDecimal.Parse("00.010", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig012", 1 == ((BigDecimal.Parse("01.01", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig013", 1 == ((BigDecimal.Parse("+0.01", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig014", 1 == ((BigDecimal.Parse("+0.001", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig015", 1 == ((BigDecimal.Parse("1", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig016", 1 == ((BigDecimal.Parse("1e+12", numberStyle, provider)).Sign));
            TestFmwk.assertTrue("sig017", 0 == ((BigDecimal.Parse("00e+12", numberStyle, provider)).Sign));
        }

        /* ----------------------------------------------------------------- */

        // ICU4N TODO: Complete implementation

        ///** Test the {@link BigDecimal#toBigDecimal} method. */
        //[Test]
        //public void diagtobigdecimal()
        //{
        //    NumberStyle numberStyle = NumberStyle.Float;
        //    CultureInfo provider = CultureInfo.InvariantCulture;

        //    TestFmwk.assertTrue("tbd001", ((BigDecimal.Parse("0", numberStyle, provider)).ToBigDecimal().ToString(CultureInfo.InvariantCulture)).Equals("0"));
        //    TestFmwk.assertTrue("tbd002", ((BigDecimal.Parse("-1", numberStyle, provider)).ToBigDecimal().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
        //    TestFmwk.assertTrue("tbd003", ((BigDecimal.Parse("+1", numberStyle, provider)).ToBigDecimal().ToString(CultureInfo.InvariantCulture)).Equals("1"));
        //    TestFmwk.assertTrue("tbd004", ((BigDecimal.Parse("1", numberStyle, provider)).ToBigDecimal().ToString(CultureInfo.InvariantCulture)).Equals("1"));
        //    TestFmwk.assertTrue("tbd005", ((BigDecimal.Parse("1E+2", numberStyle, provider)).ToBigDecimal().ToString(CultureInfo.InvariantCulture)).Equals("100"));
        //    TestFmwk.assertTrue("tbd006", ((BigDecimal.Parse("1E-2", numberStyle, provider)).ToBigDecimal().ToString(CultureInfo.InvariantCulture)).Equals("0.01"));
        //    //if (!isJDK15OrLater) {
        //    //    TestFmwk.assertTrue("tbd007", ((BigDecimal.Parse("1E-8", numberStyle, provider)).ToBigDecimal().ToString(CultureInfo.InvariantCulture)).Equals("0.00000001"));
        //    //}
        //    //if (!isJDK15OrLater) {
        //    //    TestFmwk.assertTrue("tbd008", ((BigDecimal.Parse("1E-9", numberStyle, provider)).ToBigDecimal().ToString(CultureInfo.InvariantCulture)).Equals("0.000000001"));
        //    //}
        //    TestFmwk.assertTrue("tbd009", ((BigDecimal.Parse("1E10", numberStyle, provider)).ToBigDecimal().ToString(CultureInfo.InvariantCulture)).Equals("10000000000"));
        //    TestFmwk.assertTrue("tbd010", ((BigDecimal.Parse("1E12", numberStyle, provider)).ToBigDecimal().ToString(CultureInfo.InvariantCulture)).Equals("1000000000000"));
        //}

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#toBigInteger} method. */

        [Test]
        public void diagtobiginteger()
        {
            bool flag = false;
            string[] badstrings;
            int i = 0;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            TestFmwk.assertTrue("tbi001", ((BigDecimal.Parse("-1", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi002", ((BigDecimal.Parse("0", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi003", ((BigDecimal.Parse("+1", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi004", ((BigDecimal.Parse("10", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("tbi005", ((BigDecimal.Parse("1000", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1000"));
            TestFmwk.assertTrue("tbi006", ((BigDecimal.Parse("-1E+0", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi007", ((BigDecimal.Parse("0E+0", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi008", ((BigDecimal.Parse("+1E+0", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi009", ((BigDecimal.Parse("10E+0", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("tbi010", ((BigDecimal.Parse("1E+3", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1000"));
            TestFmwk.assertTrue("tbi011", ((BigDecimal.Parse("0.00", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi012", ((BigDecimal.Parse("0.01", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi013", ((BigDecimal.Parse("0.0", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi014", ((BigDecimal.Parse("0.1", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi015", ((BigDecimal.Parse("-0.00", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi016", ((BigDecimal.Parse("-0.01", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi017", ((BigDecimal.Parse("-0.0", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi018", ((BigDecimal.Parse("-0.1", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi019", ((BigDecimal.Parse("1.00", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi020", ((BigDecimal.Parse("1.01", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi021", ((BigDecimal.Parse("1.0", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi022", ((BigDecimal.Parse("1.1", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi023", ((BigDecimal.Parse("-1.00", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi024", ((BigDecimal.Parse("-1.01", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi025", ((BigDecimal.Parse("-1.0", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi026", ((BigDecimal.Parse("-1.1", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi027", ((BigDecimal.Parse("-111.111", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-111"));
            TestFmwk.assertTrue("tbi028", ((BigDecimal.Parse("+111.111", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("111"));
            TestFmwk.assertTrue("tbi029", ((BigDecimal.Parse("0.09", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi030", ((BigDecimal.Parse("0.9", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi031", ((BigDecimal.Parse("1.09", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi032", ((BigDecimal.Parse("1.05", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi033", ((BigDecimal.Parse("1.04", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi034", ((BigDecimal.Parse("1.99", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi034", ((BigDecimal.Parse("1.9", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi035", ((BigDecimal.Parse("1.5", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi036", ((BigDecimal.Parse("1.4", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi037", ((BigDecimal.Parse("-1.09", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi038", ((BigDecimal.Parse("-1.05", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi039", ((BigDecimal.Parse("-1.04", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi040", ((BigDecimal.Parse("-1.99", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi041", ((BigDecimal.Parse("-1.9", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi042", ((BigDecimal.Parse("-1.5", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi043", ((BigDecimal.Parse("-1.4", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi044", ((BigDecimal.Parse("1E-1000", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi045", ((BigDecimal.Parse("-1E-1000", numberStyle, provider)).ToBigInteger().ToString(CultureInfo.InvariantCulture)).Equals("0"));

            // Exact variety --
            TestFmwk.assertTrue("tbi101", ((BigDecimal.Parse("-1", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi102", ((BigDecimal.Parse("0", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi103", ((BigDecimal.Parse("+1", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi104", ((BigDecimal.Parse("10", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("tbi105", ((BigDecimal.Parse("1000", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("1000"));
            TestFmwk.assertTrue("tbi106", ((BigDecimal.Parse("-1E+0", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi107", ((BigDecimal.Parse("0E+0", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi108", ((BigDecimal.Parse("+1E+0", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi109", ((BigDecimal.Parse("10E+0", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("tbi110", ((BigDecimal.Parse("1E+3", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("1000"));
            TestFmwk.assertTrue("tbi111", ((BigDecimal.Parse("0.00", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi112", ((BigDecimal.Parse("0.0", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi113", ((BigDecimal.Parse("-0.00", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi114", ((BigDecimal.Parse("-0.0", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("tbi115", ((BigDecimal.Parse("1.00", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi116", ((BigDecimal.Parse("1.0", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("tbi117", ((BigDecimal.Parse("-1.00", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi118", ((BigDecimal.Parse("-1.0", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("tbi119", ((BigDecimal.Parse("1.00000000000000000000000000000", numberStyle, provider)).ToBigIntegerExact().ToString(CultureInfo.InvariantCulture)).Equals("1"));


            // the following should all raise exceptions

            badstrings = new string[] { "0.09", "0.9", "0.01", "0.1",
                "-0.01", "-0.1", "1.01", "-1.01", "-1.1", "-111.111",
                "+111.111", "1.09", "1.05", "1.04", "1.99", "1.9", "1.5",
                "1.4", "-1.09", "-1.05", "-1.04", "-1.99", "-1.9", "-1.5",
                "-1.4", "1E-1000", "-1E-1000", "11E-1", "1.1",
                "127623156123656561356123512315631231551312356.000001",
                "0.000000000000000000000000000000000000000000000001" }; // 300-303
                                                                        // 304-307
                                                                        // 308-311
                                                                        // 312-316
                                                                        // 317-320
                                                                        // 321-324
                                                                        // 325-328
                                                                        // 329
                                                                        // 330

            {
                int len125 = badstrings.Length;
                i = 0;
                for (; len125 > 0; len125--, i++)
                {
                    try
                    {
                        (BigDecimal.Parse(badstrings[i], numberStyle, provider))
                                .ToBigIntegerExact();
                        flag = false;
                    }
                    catch (ArithmeticException e126)
                    {
                        flag = true;
                    }
                    TestFmwk.assertTrue("tbi" + (300 + i), flag);
                }
            }/* i */
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#toCharArray} method. */

        [Test]
        public void diagtochararray()
        {
            string str;
            char[] car;
            BigDecimal d;
            char[] ca;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            // the function of this has been tested above, this is simply an
            // existence proof and type-check
            str = "-123.45";
            car = (str).ToCharArray();
            d = BigDecimal.Parse(str, numberStyle, provider);
            ca = d.ToCharArray();
            TestFmwk.assertTrue("tca001", ca.Length == car.Length);
            TestFmwk.assertTrue("tca002", (new string(ca))
                    .Equals((new string(car))));
            TestFmwk.assertTrue("tca003", (d.ToCharArray() is char[]));
            TestFmwk.assertTrue("tca004", (ca is char[]));
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#toString} method. */

        [Test]
        public void diagtostring()
        {
            string str;
            char[] car;
            BigDecimal d;
            char[] ca;
            string cs;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            // the function of this has been tested above, this is simply an
            // existence proof and type-check
            str = "123.45";
            car = (str).ToCharArray();
            d = BigDecimal.Parse(car, 0, car.Length, numberStyle, provider);
            ca = d.ToCharArray();
            cs = d.ToString(CultureInfo.InvariantCulture);
            TestFmwk.assertTrue("tos001", (str.ToCharArray().Length) == ca.Length);
            TestFmwk.assertTrue("tos002", (str.Length) == (cs.Length));
            TestFmwk.assertTrue("tos003", str.Equals((new string(ca))));
            TestFmwk.assertTrue("tos004", str.Equals(cs));
            TestFmwk.assertTrue("tos005", (cs is string));
            TestFmwk.assertTrue("tos006", (d.ToString(CultureInfo.InvariantCulture) is string));
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link BigDecimal#unscaledValue} method. */

        [Test]
        public void diagunscaledvalue()
        {
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            // just like toBigInteger, but scaly bits are preserved [without dots]
            TestFmwk.assertTrue("uns001", ((BigDecimal.Parse("-1", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("uns002", ((BigDecimal.Parse("0", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("uns003", ((BigDecimal.Parse("+1", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("uns004", ((BigDecimal.Parse("10", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("uns005", ((BigDecimal.Parse("1000", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("1000"));
            TestFmwk.assertTrue("uns006", ((BigDecimal.Parse("-1E+0", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("uns007", ((BigDecimal.Parse("0E+0", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("uns008", ((BigDecimal.Parse("+1E+0", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("uns009", ((BigDecimal.Parse("10E+0", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("uns010", ((BigDecimal.Parse("1E+3", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("1000"));
            TestFmwk.assertTrue("uns011", ((BigDecimal.Parse("0.00", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("uns012", ((BigDecimal.Parse("0.01", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("uns013", ((BigDecimal.Parse("0.0", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("uns014", ((BigDecimal.Parse("0.1", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("uns015", ((BigDecimal.Parse("-0.00", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("uns016", ((BigDecimal.Parse("-0.01", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("uns017", ((BigDecimal.Parse("-0.0", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("uns018", ((BigDecimal.Parse("-0.1", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("uns019", ((BigDecimal.Parse("1.00", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("100"));
            TestFmwk.assertTrue("uns020", ((BigDecimal.Parse("1.01", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("101"));
            TestFmwk.assertTrue("uns021", ((BigDecimal.Parse("1.0", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("uns022", ((BigDecimal.Parse("1.1", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("11"));
            TestFmwk.assertTrue("uns023", ((BigDecimal.Parse("-1.00", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("-100"));
            TestFmwk.assertTrue("uns024", ((BigDecimal.Parse("-1.01", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("-101"));
            TestFmwk.assertTrue("uns025", ((BigDecimal.Parse("-1.0", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("-10"));
            TestFmwk.assertTrue("uns026", ((BigDecimal.Parse("-1.1", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("-11"));
            TestFmwk.assertTrue("uns027", ((BigDecimal.Parse("-111.111", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("-111111"));
            TestFmwk.assertTrue("uns028", ((BigDecimal.Parse("+111.111", numberStyle, provider)).ToUnscaledValue().ToString(CultureInfo.InvariantCulture)).Equals("111111"));
        }

        /* ----------------------------------------------------------------- */

        /**
         * Test the {@link BigDecimal#valueOf} method [long and
         * double].
         */

        [Test]
        public void diagvalueof()
        {
            bool flag = false;
            ArgumentOutOfRangeException e = null;
            double dzer;
            double dpos;
            double dneg;
            double dpos5;
            double dneg5;
            double dmin;
            double dmax;
            double d;

            // valueOf(long [,scale]) --

            // ICU4N TODO: This should be an implicit conversion (when BigDecimal is a struct)
            TestFmwk.assertTrue("val001", (BigDecimal.GetInstance(((sbyte)-2)).ToString(CultureInfo.InvariantCulture)).Equals("-2"));
            TestFmwk.assertTrue("val002", (BigDecimal.GetInstance(((sbyte)-1)).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("val003", (BigDecimal.GetInstance(((sbyte)-0)).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("val004", (BigDecimal.GetInstance(((sbyte)+1)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("val005", (BigDecimal.GetInstance(((sbyte)+2)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
            TestFmwk.assertTrue("val006", (BigDecimal.GetInstance(((sbyte)10)).ToString(CultureInfo.InvariantCulture)).Equals("10"));
            TestFmwk.assertTrue("val007", (BigDecimal.GetInstance(((sbyte)11)).ToString(CultureInfo.InvariantCulture)).Equals("11"));
            TestFmwk.assertTrue("val008", (BigDecimal.GetInstance(lmin).ToString(CultureInfo.InvariantCulture)).Equals("-9223372036854775808"));
            TestFmwk.assertTrue("val009", (BigDecimal.GetInstance(lmax).ToString(CultureInfo.InvariantCulture)).Equals("9223372036854775807"));
            TestFmwk.assertTrue("val010", (BigDecimal.GetInstance(lneg).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("val011", (BigDecimal.GetInstance(lzer).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("val012", (BigDecimal.GetInstance(lpos).ToString(CultureInfo.InvariantCulture)).Equals("1"));
            TestFmwk.assertTrue("val013", (BigDecimal.GetInstance(lmin, 0).ToString(CultureInfo.InvariantCulture)).Equals("-9223372036854775808"));
            TestFmwk.assertTrue("val014", (BigDecimal.GetInstance(lmax, 0).ToString(CultureInfo.InvariantCulture)).Equals("9223372036854775807"));
            TestFmwk.assertTrue("val015", (BigDecimal.GetInstance(lneg, 0).ToString(CultureInfo.InvariantCulture)).Equals("-1"));
            TestFmwk.assertTrue("val016", (BigDecimal.GetInstance(lpos, 0).ToString(CultureInfo.InvariantCulture)).Equals("1"));

            TestFmwk.assertTrue("val017", (BigDecimal.GetInstance(lzer, 0).ToString(CultureInfo.InvariantCulture)).Equals("0"));
            TestFmwk.assertTrue("val018", (BigDecimal.GetInstance(lzer, 1).ToString(CultureInfo.InvariantCulture)).Equals("0.0"));
            TestFmwk.assertTrue("val019", (BigDecimal.GetInstance(lzer, 2).ToString(CultureInfo.InvariantCulture)).Equals("0.00"));
            TestFmwk.assertTrue("val020", (BigDecimal.GetInstance(lzer, 3).ToString(CultureInfo.InvariantCulture)).Equals("0.000"));
            TestFmwk.assertTrue("val021", (BigDecimal.GetInstance(lzer, 10).ToString(CultureInfo.InvariantCulture)).Equals("0.0000000000"));

            TestFmwk.assertTrue("val022", (BigDecimal.GetInstance(lmin, 7).ToString(CultureInfo.InvariantCulture)).Equals("-922337203685.4775808"));
            TestFmwk.assertTrue("val023", (BigDecimal.GetInstance(lmax, 11).ToString(CultureInfo.InvariantCulture)).Equals("92233720.36854775807"));

            try
            {
                BigDecimal.GetInstance(23, -8);
                flag = false;
            }
            catch (ArgumentOutOfRangeException e127)  //java.lang.NumberFormatException
            {
                e = e127;
                flag = (e.Message).StartsWith("Negative scale: -8", StringComparison.Ordinal);
            }/* checkscale */
            TestFmwk.assertTrue("val100", flag);

            // valueOf(double) --

            dzer = 0;
            dpos = 1;
            dpos = dpos / (10);
            dneg = -dpos;
            TestFmwk.assertTrue("val201", (BigDecimal.GetInstance(dneg).ToString(CultureInfo.InvariantCulture)).Equals("-0.1"));
            TestFmwk.assertTrue("val202", (BigDecimal.GetInstance(dzer).ToString(CultureInfo.InvariantCulture)).Equals("0.0")); // cf. constructor
            TestFmwk.assertTrue("val203", (BigDecimal.GetInstance(dpos).ToString(CultureInfo.InvariantCulture)).Equals("0.1"));
            dpos5 = 0.5D;
            dneg5 = -dpos5;
            TestFmwk.assertTrue("val204", (BigDecimal.GetInstance(dneg5).ToString(CultureInfo.InvariantCulture)).Equals("-0.5"));
            TestFmwk.assertTrue("val205", (BigDecimal.GetInstance(dpos5).ToString(CultureInfo.InvariantCulture)).Equals("0.5"));
            dmin = double.Epsilon; // ICU4N: Corrected MIN_VALUE to Epsilon (smallest postive value)
            dmax = double.MaxValue;
            TestFmwk.assertTrue("val206", (BigDecimal.GetInstance(dmin).ToString(CultureInfo.InvariantCulture)).Equals("4.9E-324"));
            TestFmwk.assertTrue("val207", (BigDecimal.GetInstance(dmax).ToString(CultureInfo.InvariantCulture)).Equals("1.7976931348623157E+308"));

            // nasties
            d = 9;
            d = d / (10);
            TestFmwk.assertTrue("val210", (BigDecimal.GetInstance(d).ToString(CultureInfo.InvariantCulture)).Equals("0.9"));
            d = d / (10);
            TestFmwk.assertTrue("val211", (BigDecimal.GetInstance(d).ToString(CultureInfo.InvariantCulture)).Equals("0.09"));
            d = d / (10);
            // The primitive double 0.009 is different in OpenJDK. In Oracle/IBM java <= 6, there is a trailing 0 (e.g 0.0090).
            String s = BigDecimal.GetInstance(d).ToString(CultureInfo.InvariantCulture);
            TestFmwk.assertTrue("val212", s.Equals("0.0090") || s.Equals("0.009"));
            d = d / (10);
            TestFmwk.assertTrue("val213", (BigDecimal.GetInstance(d).ToString(CultureInfo.InvariantCulture)).Equals("9.0E-4"));
            d = d / (10);
            TestFmwk.assertTrue("val214", (BigDecimal.GetInstance(d).ToString(CultureInfo.InvariantCulture)).Equals("8.999999999999999E-5"));
            d = d / (10);
            TestFmwk.assertTrue("val215", (BigDecimal.GetInstance(d).ToString(CultureInfo.InvariantCulture)).Equals("8.999999999999999E-6"));
            d = d / (10);
            TestFmwk.assertTrue("val216", (BigDecimal.GetInstance(d).ToString(CultureInfo.InvariantCulture)).Equals("8.999999999999999E-7"));
            d = d / (10);
            TestFmwk.assertTrue("val217", (BigDecimal.GetInstance(d).ToString(CultureInfo.InvariantCulture)).Equals("8.999999999999999E-8"));
            d = d / (10);
            TestFmwk.assertTrue("val218", (BigDecimal.GetInstance(d).ToString(CultureInfo.InvariantCulture)).Equals("8.999999999999998E-9"));

            try
            {
                BigDecimal
                        .GetInstance(double.PositiveInfinity);
                flag = false;
            }
            catch (OverflowException e128) //java.lang.NumberFormatException
            {
                flag = true;
            }/* checkpin */
            TestFmwk.assertTrue("val301", flag);
            try
            {
                BigDecimal
                        .GetInstance(double.NegativeInfinity);
                flag = false;
            }
            catch (OverflowException e129) //java.lang.NumberFormatException
            {
                flag = true;
            }/* checknin */
            TestFmwk.assertTrue("val302", flag);
            try
            {
                BigDecimal.GetInstance(double.NaN);
                flag = false;
            }
            catch (OverflowException e130) //java.lang.NumberFormatException
            {
                flag = true;
            }/* checknan */
            TestFmwk.assertTrue("val303", flag);
        }

        /* ----------------------------------------------------------------- */

        /** Test the {@link MathContext} class. */

        [Test]
        public void diagmathcontext()
        {
            MathContext mccon1;
            MathContext mccon2;
            MathContext mccon3;
            MathContext mccon4;
            MathContext mcrmc;
            MathContext mcrmd;
            MathContext mcrmf;
            MathContext mcrmhd;
            MathContext mcrmhe;
            MathContext mcrmhu;
            MathContext mcrmun;
            MathContext mcrmu;
            bool flag = false;
            ArgumentOutOfRangeException e = null;
            // these tests are mostly existence checks
            TestFmwk.assertTrue("mcn001", (MathContext.Default.Digits) == 9);
            TestFmwk.assertTrue("mcn002", (MathContext.Default.Form) == MathContext.Scientific);
            TestFmwk.assertTrue("mcn003", (MathContext.Default.Form) != MathContext.Engineering);
            TestFmwk.assertTrue("mcn004", (MathContext.Default.Form) != MathContext.Plain);
            TestFmwk.assertTrue("mcn005", (MathContext.Default.LostDigits ? 1 : 0) == 0);
            TestFmwk.assertTrue("mcn006", (MathContext.Default.RoundingMode) == MathContext.RoundHalfUp);

            TestFmwk.assertTrue("mcn010", MathContext.RoundCeiling >= 0);
            TestFmwk.assertTrue("mcn011", MathContext.RoundDown >= 0);
            TestFmwk.assertTrue("mcn012", MathContext.RoundFloor >= 0);
            TestFmwk.assertTrue("mcn013", MathContext.RoundHalfDown >= 0);
            TestFmwk.assertTrue("mcn014", MathContext.RoundHalfEven >= 0);
            TestFmwk.assertTrue("mcn015", MathContext.RoundHalfUp >= 0);
            TestFmwk.assertTrue("mcn016", MathContext.RoundUnnecessary >= 0);
            TestFmwk.assertTrue("mcn017", MathContext.RoundUp >= 0);

            mccon1 = new MathContext(111);
            TestFmwk.assertTrue("mcn021", (mccon1.Digits) == 111);
            TestFmwk.assertTrue("mcn022", (mccon1.Form) == MathContext.Scientific);
            TestFmwk.assertTrue("mcn023", (mccon1.LostDigits ? 1 : 0) == 0);
            TestFmwk.assertTrue("mcn024", (mccon1.RoundingMode) == MathContext.RoundHalfUp);

            mccon2 = new MathContext(78, MathContext.Engineering);
            TestFmwk.assertTrue("mcn031", (mccon2.Digits) == 78);
            TestFmwk.assertTrue("mcn032", (mccon2.Form) == MathContext.Engineering);
            TestFmwk.assertTrue("mcn033", (mccon2.LostDigits ? 1 : 0) == 0);
            TestFmwk.assertTrue("mcn034", (mccon2.RoundingMode) == MathContext.RoundHalfUp);

            mccon3 = new MathContext(5, MathContext.Plain, true);
            TestFmwk.assertTrue("mcn041", (mccon3.Digits) == 5);
            TestFmwk.assertTrue("mcn042", (mccon3.Form) == MathContext.Plain);
            TestFmwk.assertTrue("mcn043", (mccon3.LostDigits ? 1 : 0) == 1);
            TestFmwk.assertTrue("mcn044", (mccon3.RoundingMode) == MathContext.RoundHalfUp);

            mccon4 = new MathContext(0, MathContext.Scientific, false, MathContext.RoundFloor);
            TestFmwk.assertTrue("mcn051", (mccon4.Digits) == 0);
            TestFmwk.assertTrue("mcn052", (mccon4.Form) == MathContext.Scientific);
            TestFmwk.assertTrue("mcn053", (mccon4.LostDigits ? 1 : 0) == 0);
            TestFmwk.assertTrue("mcn054", (mccon4.RoundingMode) == MathContext.RoundFloor);

            // ICU4N TODO: Change the form/rounding mode to return same case
            TestFmwk.assertTrue("mcn061", (mccon1.ToString()).Equals("digits=111 form=SCIENTIFIC lostDigits=0 roundingMode=ROUND_HALF_UP"));

            TestFmwk.assertTrue("mcn062", (mccon2.ToString()).Equals("digits=78 form=ENGINEERING lostDigits=0 roundingMode=ROUND_HALF_UP"));

            TestFmwk.assertTrue("mcn063", (mccon3.ToString()).Equals("digits=5 form=PLAIN lostDigits=1 roundingMode=ROUND_HALF_UP"));

            TestFmwk.assertTrue("mcn064", (mccon4.ToString()).Equals("digits=0 form=SCIENTIFIC lostDigits=0 roundingMode=ROUND_FLOOR"));

            // complete testing rounding modes round trips
            mcrmc = new MathContext(0, MathContext.Plain, false, MathContext.RoundCeiling);
            mcrmd = new MathContext(0, MathContext.Plain, false, MathContext.RoundDown);
            mcrmf = new MathContext(0, MathContext.Plain, false, MathContext.RoundFloor);
            mcrmhd = new MathContext(0, MathContext.Plain, false, MathContext.RoundHalfDown);
            mcrmhe = new MathContext(0, MathContext.Plain, false, MathContext.RoundHalfEven);
            mcrmhu = new MathContext(0, MathContext.Plain, false, MathContext.RoundHalfUp);
            mcrmun = new MathContext(0, MathContext.Plain, false, MathContext.RoundUnnecessary);
            mcrmu = new MathContext(0, MathContext.Plain, false, MathContext.RoundUp);

            // ICU4N TODO: Change the form/rounding mode to return same case
            TestFmwk.assertTrue("mcn071", (mcrmc.ToString()).Equals("digits=0 form=PLAIN lostDigits=0 roundingMode=ROUND_CEILING"));

            TestFmwk.assertTrue("mcn072", (mcrmd.ToString()).Equals("digits=0 form=PLAIN lostDigits=0 roundingMode=ROUND_DOWN"));

            TestFmwk.assertTrue("mcn073", (mcrmf.ToString()).Equals("digits=0 form=PLAIN lostDigits=0 roundingMode=ROUND_FLOOR"));

            TestFmwk.assertTrue("mcn074", (mcrmhd.ToString()).Equals("digits=0 form=PLAIN lostDigits=0 roundingMode=ROUND_HALF_DOWN"));

            TestFmwk.assertTrue("mcn075", (mcrmhe.ToString()).Equals("digits=0 form=PLAIN lostDigits=0 roundingMode=ROUND_HALF_EVEN"));

            TestFmwk.assertTrue("mcn076", (mcrmhu.ToString()).Equals("digits=0 form=PLAIN lostDigits=0 roundingMode=ROUND_HALF_UP"));

            TestFmwk.assertTrue("mcn077", (mcrmun.ToString()).Equals("digits=0 form=PLAIN lostDigits=0 roundingMode=ROUND_UNNECESSARY"));

            TestFmwk.assertTrue("mcn078", (mcrmu.ToString()).Equals("digits=0 form=PLAIN lostDigits=0 roundingMode=ROUND_UP"));

            // [get methods tested already]

            // errors...

            try
            {
                new MathContext(-1);
                flag = false;
            }
            catch (ArgumentOutOfRangeException e131)
            {
                e = e131;
                flag = (e.Message).StartsWith("Digits too small: -1", StringComparison.Ordinal);
            }/* checkdig */
            TestFmwk.assertTrue("mcn101", flag);
            try
            {
                new MathContext(1000000000);
                flag = false;
            }
            catch (ArgumentOutOfRangeException e132)
            {
                e = e132;
                flag = (e.Message).StartsWith("Digits too large: 1000000000", StringComparison.Ordinal);
            }/* checkdigbig */
            TestFmwk.assertTrue("mcn102", flag);

            try
            {
                new MathContext(0, (ExponentForm)5);
                flag = false;
            }
            catch (ArgumentOutOfRangeException e133)
            {
                e = e133;
                flag = (e.Message).StartsWith("Bad form value: 5", StringComparison.Ordinal);
            }/* checkform */
            TestFmwk.assertTrue("mcn111", flag);
            try
            {
                new MathContext(0, (ExponentForm)(-1));
                flag = false;
            }
            catch (ArgumentOutOfRangeException e134)
            {
                e = e134;
                flag = (e.Message).StartsWith("Bad form value: -1", StringComparison.Ordinal);
            }/* checkformneg */
            TestFmwk.assertTrue("mcn112", flag);

            // [lostDigits cannot be invalid]

            try
            {
                new MathContext(0,
                        MathContext.Plain, false, (RoundingMode)12);
                flag = false;
            }
            catch (ArgumentOutOfRangeException e135)
            {
                e = e135;
                flag = (e.Message).StartsWith("Bad roundingMode value: 12", StringComparison.Ordinal);
            }/* checkround */
            TestFmwk.assertTrue("mcn121", flag);
            try
            {
                new MathContext(0,
                        MathContext.Plain, false, (RoundingMode)(-1));
                flag = false;
            }
            catch (ArgumentOutOfRangeException e136)
            {
                e = e136;
                flag = (e.Message).StartsWith("Bad roundingMode value: -1", StringComparison.Ordinal);
            }/* checkroundneg */
            TestFmwk.assertTrue("mcn122", flag);
        }

        /* ----------------------------------------------------------------- */

        /**
         * Test general arithmetic (base operators).
         * <para/>
         * Unlike the specific method tests, these tests were randomly generated by
         * an IBM Object Rexx procedure, then manually corrected for known
         * differences from ANSI X3-274. These differences are:
         * <ol>
         * <li>the trigger point in exponential notation is fixed in ANSI X3-274
         * but varies with DIGITS in Classic and Object Rexx
         * <li>some trailing zeros were missing (e.g., 1.3 + 1E-60 should show
         * seven trailing zeros)
         * <li>the power operator is less accurate in Object Rexx
         * <li>ANSI X3-274 [errata 1999] rounds input numbers to DIGITS (rather
         * than truncating to DIGITS+1).
         * </ol>
         */

        [Test]
        public void diagmath()
        {
            MathContext def;
            def = MathContext.Default;
            mathtest(1, def, "-9375284.42", "5516.99832E+27276984", "5.51699832E+27276987", "-5.51699832E+27276987", "-5.17234284E+27276994", "-1.69934516E-27276981", "0", "-9375284.42", "6.79057752E+41");
            mathtest(2, def, "-410.832710", "99.3588243E-502740862", "-410.832710", "-410.832710", "-4.08198550E-502740858", "-4.13483868E+502740862", "", "", "1.36977786E+26");
            mathtest(3, def, "80025.2340", "-8.03097581", "80017.2030", "80033.2650", "-642680.718", "-9964.57167", "-9964", "4.59102916", "5.94544517E-40");
            mathtest(4, def, "81052020.2", "-464525495", "-383473475", "545577515", "-3.76507298E+16", "-0.17448347", "0", "81052020.2", "");
            mathtest(5, def, "715.069294E-26923151", "85.4404128E+796388557", "8.54404128E+796388558", "-8.54404128E+796388558", "6.10958157E+769465410", "8.36921628E-823311708", "0", "7.15069294E-26923149", "4.88802213E-242308334");
            mathtest(6, def, "-21971575.0E+31454441", "-70944960.3E+111238221", "-7.09449603E+111238228", "7.09449603E+111238228", "1.55877252E+142692677", "3.09698884E-79783781", "0", "-2.19715750E+31454448", "-4.04549502E-220181139");
            mathtest(7, def, "682.25316", "54470185.6", "54470867.9", "-54469503.4", "3.71624563E+10", "0.0000125252586", "0", "682.25316", "3.48578699E+154365541");
            mathtest(8, def, "-257586757.", "2082888.71", "-255503868", "-259669646", "-5.36524548E+14", "-123.668036", "-123", "-1391445.67", "-1.26879515E+17519020");
            mathtest(9, def, "319577540.E+242599761", "60.7124561", "3.19577540E+242599769", "3.19577540E+242599769", "1.94023374E+242599771", "5.26378869E+242599767", "", "", "");
            mathtest(10, def, "-13769977.0", "24371.3381", "-13745605.7", "-13794348.3", "-3.35592765E+11", "-565.007015", "-565", "-170.9735", "-8.73734001E+173982");
            mathtest(11, def, "-475.434972E-725464311", "-3.22214066E-865476836", "-4.75434972E-725464309", "-4.75434972E-725464309", "", "1.47552519E+140012527", "", "", "");
            mathtest(12, def, "842.01250", "197199893", "197200735", "-197199051", "1.66044775E+11", "0.00000426984258", "0", "842.01250", "7.00674164E+576872502");
            mathtest(13, def, "572.173103E+280128428", "-7140.19428", "5.72173103E+280128430", "5.72173103E+280128430", "-4.08542712E+280128434", "-8.01341085E+280128426", "", "", "");
            mathtest(14, def, "674235.954E+476135291", "9684.82245", "6.74235954E+476135296", "6.74235954E+476135296", "6.52985550E+476135300", "6.96177919E+476135292", "", "", "");
            mathtest(15, def, "-360557.921E+437116514", "930428850", "-3.60557921E+437116519", "-3.60557921E+437116519", "-3.35473492E+437116528", "-3.87517993E+437116510", "", "", "");
            mathtest(16, def, "957165918E-394595705", "1676.59073E-829618944", "9.57165918E-394595697", "9.57165918E-394595697", "", "5.70900161E+435023244", "", "", "9.16166595E-789191393");
            mathtest(17, def, "-2610864.40", "31245912.7", "28635048.3", "-33856777.1", "-8.15788411E+13", "-0.0835585897", "0", "-2610864.40", "-3.12008905E+200498284");
            mathtest(18, def, "959.548461", "98.994577E+776775426", "9.89945770E+776775427", "-9.89945770E+776775427", "9.49900940E+776775430", "9.69293965E-776775426", "0", "959.548461", "6.61712185E+29");
            mathtest(19, def, "-41085.0268", "3115477.61", "3074392.58", "-3156562.64", "-1.27999481E+11", "-0.0131873927", "0", "-41085.0268", "4.73844173E+14373829");
            mathtest(20, def, "-723420285.", "2681660.35", "-720738625", "-726101945", "-1.93996749E+15", "-269.765813", "-269", "-2053650.85", "4.14324113E+23757873");
            mathtest(21, def, "63542018.0E-817732230", "-8836243.22", "-8836243.22", "8836243.22", "-5.61472726E-817732216", "-7.19106711E-817732230", "0", "6.35420180E-817732223", "");
            mathtest(22, def, "-96051.7108", "-291201.955", "-387253.666", "195150.244", "2.79704460E+10", "0.329845694", "0", "-96051.7108", "3.53617153E-1450916");
            mathtest(23, def, "108490.853", "91685996.5", "91794487.4", "-91577505.7", "9.94709197E+12", "0.00118328706", "0", "108490.853", "6.98124265E+461675038");
            mathtest(24, def, "-27489.1735", "-9835835.4E-506411649", "-27489.1735", "-27489.1735", "2.70378986E-506411638", "2.79479804E+506411646", "", "", "4.05866472E-45");
            mathtest(25, def, "-89220406.6", "993391.008E-611041175", "-89220406.6", "-89220406.6", "-8.86307496E-611041162", "-8.98139865E+611041176", "", "", "3.19625913E+79");
            mathtest(26, def, "4.75502020", "-17089144.9", "-17089140.2", "17089149.7", "-81259229.2", "-2.78247989E-7", "0", "4.75502020", "1.0630191E-11571955");
            mathtest(27, def, "68027916.2", "-796883.839", "67231032.4", "68824800.0", "-5.42103470E+13", "-85.3674185", "-85", "292789.885", "8.29415374E-6241744");
            mathtest(28, def, "-8.01969439E+788605478", "92154156.0", "-8.01969439E+788605478", "-8.01969439E+788605478", "-7.39048168E+788605486", "-8.70247717E+788605470", "", "", "");
            mathtest(29, def, "-8012.98341", "96188.8651", "88175.8817", "-104201.849", "-770759780", "-0.0833046881", "0", "-8012.98341", "-1.16010156E+375502");
            mathtest(30, def, "21761476E+592330677", "-9.70744506", "2.17614760E+592330684", "2.17614760E+592330684", "-2.11248333E+592330685", "-2.24173053E+592330683", "", "", "");
            mathtest(31, def, "-9840778.51", "-17907.219", "-9858685.73", "-9822871.29", "1.76220976E+11", "549.542534", "549", "-9715.279", "-6.62997437E-125225");
            mathtest(32, def, "-4.1097614", "-819.225776E-145214751", "-4.10976140", "-4.10976140", "3.36682247E-145214748", "5.01664074E+145214748", "", "", "0.0000122876018");
            mathtest(33, def, "-448.880985", "-394.087374E-442511435", "-448.880985", "-448.880985", "1.76898329E-442511430", "1.13903925E+442511435", "", "", "2.46306099E-11");
            mathtest(34, def, "779.445304E+882688544", "-797868519", "7.79445304E+882688546", "7.79445304E+882688546", "-6.21894870E+882688555", "-9.7690946E+882688537", "", "", "");
            mathtest(35, def, "799995477", "-6.23675208E+156309440", "-6.23675208E+156309440", "6.23675208E+156309440", "-4.98937346E+156309449", "-1.28271169E-156309432", "0", "799995477", "3.81482667E-54");
            mathtest(36, def, "-51932.8170", "591840275E-278480289", "-51932.8170", "-51932.8170", "-3.07359327E-278480276", "-8.7748028E+278480284", "", "", "1.96178443E+28");
            mathtest(37, def, "70.3552392", "-4228656.73", "-4228586.38", "4228727.09", "-297508156", "-0.0000166377277", "0", "70.3552392", "9.14742382E-7811584");
            mathtest(38, def, "1588359.34", "-12232799.2", "-10644439.9", "13821158.5", "-1.94300809E+13", "-0.129844307", "0", "1588359.34", "1.56910086E-75854960");
            mathtest(39, def, "2842.16206", "-3.23234345", "2838.92972", "2845.39440", "-9186.84392", "-879.288388", "-879", "0.93216745", "4.35565514E-11");
            mathtest(40, def, "29960.2305", "45.2735747E-95205475", "29960.2305", "29960.2305", "1.35640673E-95205469", "6.61759773E+95205477", "", "", "2.413936E+22");
            mathtest(41, def, "2916565.77", "1151935.43E-787118724", "2916565.77", "2916565.77", "3.35969544E-787118712", "2.53188303E+787118724", "", "", "2916565.77");
            mathtest(42, def, "-52723012.9E-967143787", "79.4088237", "79.4088237", "-79.4088237", "-4.18667244E-967143778", "-6.63944011E-967143782", "0", "-5.27230129E-967143780", "");
            mathtest(43, def, "-167473465", "793646.597", "-166679819", "-168267112", "-1.32914746E+14", "-211.017682", "-211", "-14033.033", "-1.19053789E+6526910");
            mathtest(44, def, "-31769071.0", "133.4360", "-31768937.6", "-31769204.4", "-4.23913776E+9", "-238084.707", "-238084", "-94.3760", "-5.84252432E+997");
            mathtest(45, def, "45960.6383", "-93352.7468", "-47392.1085", "139313.385", "-4.29055183E+9", "-0.492333004", "0", "45960.6383", "1.88335323E-435248");
            mathtest(46, def, "606.175648", "5.28528458E-981983620", "606.175648", "606.175648", "3.20381081E-981983617", "1.14691203E+981983622", "", "", "8.18450516E+13");
            mathtest(47, def, "171578.617E+643006110", "-407774.293", "1.71578617E+643006115", "1.71578617E+643006115", "-6.99653492E+643006120", "-4.20768597E+643006109", "", "", "");
            mathtest(48, def, "-682286332.", "-464.871699", "-682286797", "-682285867", "3.17175606E+11", "1467687.39", "1467687", "-182.709787", "-1.6050843E-4108");
            mathtest(49, def, "492088.428", "653.72170", "492742.150", "491434.706", "321688884", "752.74911", "752", "489.70960", "3.94658596E+3722");
            mathtest(50, def, "74303782.5", "1141.68058", "74304924.2", "74302640.8", "8.48311855E+10", "65082.812", "65082", "926.99244", "4.94849869E+8988");
            mathtest(51, def, "74.7794084E+119375329", "-34799355.6", "7.47794084E+119375330", "7.47794084E+119375330", "-2.60227522E+119375338", "-2.14887337E+119375323", "", "", "");
            mathtest(52, def, "-9432.08369", "33735.5058", "24303.4221", "-43167.5895", "-318196114", "-0.279589218", "0", "-9432.08369", "2.309567E+134087");
            mathtest(53, def, "4249198.78E-112433155", "418673051.", "418673051", "-418673051", "1.77902502E-112433140", "1.01492054E-112433157", "0", "4.24919878E-112433149", "");
            mathtest(54, def, "-2960933.02", "-207933.38", "-3168866.40", "-2752999.64", "6.15676811E+11", "14.2398158", "14", "-49865.70", "-2.75680397E-1345624");
            mathtest(55, def, "29317.7519E+945600035", "1.43555750", "2.93177519E+945600039", "2.93177519E+945600039", "4.20873186E+945600039", "2.04225549E+945600039", "", "", "2.93177519E+945600039");
            mathtest(56, def, "-51.1693770", "-638055.414", "-638106.583", "638004.245", "32648898.0", "0.0000801958198", "0", "-51.1693770", "-3.48266075E-1090443");
            mathtest(57, def, "-756343055.", "-68.9248344E+217100975", "-6.89248344E+217100976", "6.89248344E+217100976", "5.21308198E+217100985", "1.09734475E-217100968", "0", "-756343055", "-7.06265897E-63");
            mathtest(58, def, "2538.80406E+694185197", "-3386499.65", "2.53880406E+694185200", "2.53880406E+694185200", "-8.59765906E+694185206", "-7.49683839E+694185193", "", "", "");
            mathtest(59, def, "-54344.0672", "-8086.45235", "-62430.5196", "-46257.6149", "439450710", "6.72038427", "6", "-5825.35310", "3.62916861E-38289");
            mathtest(60, def, "3.31600054", "217481648", "217481651", "-217481645", "721169262", "1.5247266E-8", "0", "3.31600054", "3.73134969E+113224119");
            mathtest(61, def, "681832.671", "320341.161E+629467560", "3.20341161E+629467565", "-3.20341161E+629467565", "2.18419069E+629467571", "2.12845789E-629467560", "0", "681832.671", "3.16981139E+17");
            mathtest(62, def, "832689481", "348040024E-882122501", "832689481", "832689481", "2.89809267E-882122484", "2.3925107E+882122501", "", "", "5.77363381E+26");
            mathtest(63, def, "14.5512326E+257500811", "60.9979577E-647314724", "1.45512326E+257500812", "1.45512326E+257500812", "8.87595471E-389813911", "2.38552784E+904815534", "", "", "");
            mathtest(64, def, "-901.278844", "449461667.", "449460766", "-449462568", "-4.05090292E+11", "-0.00000200524074", "0", "-901.278844", "");
            mathtest(65, def, "-5.32627675", "-738860216E-238273224", "-5.32627675", "-5.32627675", "3.93537399E-238273215", "7.20877459E+238273215", "", "", "-0.00000822306838");
            mathtest(66, def, "-505383463.", "3.18756328", "-505383460", "-505383466", "-1.61094177E+9", "-158548527", "-158548527", "-0.23671144", "-1.29081226E+26");
            mathtest(67, def, "769241.44E-720927320", "-145382631.", "-145382631", "145382631", "-1.11834344E-720927306", "-5.29115091E-720927323", "0", "7.6924144E-720927315", "");
            mathtest(68, def, "-6.45038910", "56736.4411E+440937167", "5.67364411E+440937171", "-5.67364411E+440937171", "-3.65972121E+440937172", "-1.13690407E-440937171", "0", "-6.45038910", "72030.3421");
            mathtest(69, def, "58.4721075", "-712186829", "-712186771", "712186887", "-4.16430648E+10", "-8.21022028E-8", "0", "58.4721075", "");
            mathtest(70, def, "8244.08357", "245.302828E+652007959", "2.45302828E+652007961", "-2.45302828E+652007961", "2.02229701E+652007965", "3.36077804E-652007958", "0", "8244.08357", "67964913.9");
            mathtest(71, def, "45.5361397", "-76579063.9", "-76579018.4", "76579109.4", "-3.48711495E+9", "-5.94629098E-7", "0", "45.5361397", "3.98335374E-126995367");
            mathtest(72, def, "594420.54E+685263039", "-952420.179", "5.94420540E+685263044", "5.94420540E+685263044", "-5.66138117E+685263050", "-6.24115861E+685263038", "", "", "");
            mathtest(73, def, "-841310701.", "9398110.4", "-831912591", "-850708811", "-7.90673085E+15", "-89.5191337", "-89", "-4878875.4", "1.30001466E+83877722");
            mathtest(74, def, "904392146E-140100276", "168116093.", "168116093", "-168116093", "1.52042874E-140100259", "5.37956914E-140100276", "0", "9.04392146E-140100268", "");
            mathtest(75, def, "-907324792E+685539670", "-15.6902171", "-9.07324792E+685539678", "-9.07324792E+685539678", "1.42361230E+685539680", "5.78274211E+685539677", "", "", "");
            mathtest(76, def, "987013606.", "-26818.3572E+560907442", "-2.68183572E+560907446", "2.68183572E+560907446", "-2.64700834E+560907455", "-3.68036565E-560907438", "0", "987013606", "1.0399934E-27");
            mathtest(77, def, "-741317564", "630.241530E-212782946", "-741317564", "-741317564", "-4.67209116E-212782935", "-1.1762436E+212782952", "", "", "1.65968527E+53");
            mathtest(78, def, "61867907.2", "-139204670", "-77336763", "201072577", "-8.61230161E+15", "-0.444438446", "0", "61867907.2", "");
            mathtest(79, def, "-273.622743E+531282717", "-4543.68684", "-2.73622743E+531282719", "-2.73622743E+531282719", "1.24325606E+531282723", "6.02204229E+531282715", "", "", "");
            mathtest(80, def, "-383588949.", "-428640583.", "-812229532", "45051634", "1.64421791E+17", "0.89489648", "0", "-383588949", "");
            mathtest(81, def, "-56182.2686", "32.7741649", "-56149.4944", "-56215.0428", "-1841326.94", "-1714.22426", "-1714", "-7.3499614", "-5.45476402E+156");
            mathtest(82, def, "-6366384.30", "332014.980", "-6034369.32", "-6698399.28", "-2.11373496E+12", "-19.1749911", "-19", "-58099.680", "-3.05392399E+2258994");
            mathtest(83, def, "-1.27897702", "-8213776.03E-686519123", "-1.27897702", "-1.27897702", "1.05052308E-686519116", "1.55711212E+686519116", "", "", "0.139668371");
            mathtest(84, def, "65.4059036", "401162145E+884155506", "4.01162145E+884155514", "-4.01162145E+884155514", "2.62383726E+884155516", "1.63041066E-884155513", "0", "65.4059036", "18300704.1");
            mathtest(85, def, "-20630916.8", "158987411.E-480500612", "-20630916.8", "-20630916.8", "-3.28005605E-480500597", "-1.29764468E+480500611", "", "", "4.25634728E+14");
            mathtest(86, def, "-4.72705853", "-97626742.4", "-97626747.1", "97626737.7", "461487325", "4.84197097E-8", "0", "-4.72705853", "2.92654449E-65858120");
            mathtest(87, def, "8.43528169", "-4573.45752", "-4565.02224", "4581.89280", "-38578.4025", "-0.00184439927", "0", "8.43528169", "8.84248688E-4236");
            mathtest(88, def, "1.91075189", "-704247089.", "-704247087", "704247091", "-1.34564146E+9", "-2.71318394E-9", "0", "1.91075189", "6.84547494E-198037309");
            mathtest(89, def, "31997198E-551746308", "326.892584", "326.892584", "-326.892584", "1.04596467E-551746298", "9.78829119E-551746304", "0", "3.1997198E-551746301", "");
            mathtest(90, def, "127589.213", "84184304.", "84311893.2", "-84056714.8", "1.07410091E+13", "0.00151559385", "0", "127589.213", "2.87917042E+429829394");
            mathtest(91, def, "714494248", "-7025063.59", "707469185", "721519312", "-5.01936753E+15", "-101.706446", "-101", "4962825.41", "1.65018516E-62199908");
            mathtest(92, def, "-52987680.2E+279533503", "-42014114.8", "-5.29876802E+279533510", "-5.29876802E+279533510", "2.22623048E+279533518", "1.26118759E+279533503", "", "", "");
            mathtest(93, def, "-8795.0513", "-225294.394E-884414238", "-8795.05130", "-8795.05130", "1.98147575E-884414229", "3.90380388E+884414236", "", "", "1.2927759E-8");
            mathtest(94, def, "83280.1394", "161566354.", "161649634", "-161483074", "1.34552685E+13", "0.000515454718", "0", "83280.1394", "5.30774809E+794993940");
            mathtest(95, def, "112.877897", "-9.96481666", "102.913080", "122.842714", "-1124.80755", "-11.3276441", "-11", "3.26491374", "2.97790545E-21");
            mathtest(96, def, "-572542.121E+847487397", "433.843420", "-5.72542121E+847487402", "-5.72542121E+847487402", "-2.48393632E+847487405", "-1.3196976E+847487400", "", "", "");
            mathtest(97, def, "4709649.89", "20949266.4", "25658916.3", "-16239616.5", "9.86637102E+13", "0.224812163", "0", "4709649.89", "4.85293644E+139794213");
            mathtest(98, def, "-9475.19322", "-30885.2475E+584487341", "-3.08852475E+584487345", "3.08852475E+584487345", "2.92643688E+584487349", "3.06787026E-584487342", "0", "-9475.19322", "-1.17553557E-12");
            mathtest(99, def, "-213230447.", "864.815822E+127783046", "8.64815822E+127783048", "-8.64815822E+127783048", "-1.84405064E+127783057", "-2.46561686E-127783041", "0", "-213230447", "-9.11261361E+74");
            mathtest(100, def, "-89.1168786E+403375873", "6464.05744", "-8.91168786E+403375874", "-8.91168786E+403375874", "-5.76056622E+403375878", "-1.37865233E+403375871", "", "", "");
            mathtest(101, def, "61774.4958", "-14000.7706", "47773.7252", "75775.2664", "-864890545", "-4.41222112", "-4", "5771.4134", "7.59030407E-67077");
            mathtest(102, def, "1.60731414", "7.04330293E-427033419", "1.60731414", "1.60731414", "1.13208004E-427033418", "2.28204602E+427033418", "", "", "27.7143921");
            mathtest(103, def, "7955012.51", "-230117662.", "-222162650", "238072675", "-1.83058888E+15", "-0.0345693261", "0", "7955012.51", "");
            mathtest(104, def, "4086661.08", "1.77621994", "4086662.86", "4086659.30", "7258808.90", "2300762.98", "2300762", "1.73840572", "1.67007988E+13");
            mathtest(105, def, "-610.076931", "-207.658306", "-817.735237", "-402.418625", "126687.542", "2.93788841", "2", "-194.760319", "4.36518377E-580");
            mathtest(106, def, "-98.6353697", "-99253.3899E-716309653", "-98.6353697", "-98.6353697", "9.78989481E-716309647", "9.93773309E+716309649", "", "", "1.14729007E-20");
            mathtest(107, def, "-959923730", "409.125542E-900295528", "-959923730", "-959923730", "-3.92729316E-900295517", "-2.3462816E+900295534", "", "", "8.49076677E+35");
            mathtest(108, def, "379965133", "-8.15869657", "379965125", "379965141", "-3.10002023E+9", "-46571793.6", "-46571793", "5.19214999", "2.30170697E-69");
            mathtest(109, def, "833.646797", "1389499.46E-443407251", "833.646797", "833.646797", "1.15835177E-443407242", "5.99961944E+443407247", "", "", "833.646797");
            mathtest(110, def, "2314933.4E-646489194", "-7401538.17", "-7401538.17", "7401538.17", "-1.71340679E-646489181", "-3.12763826E-646489195", "0", "2.3149334E-646489188", "");
            mathtest(111, def, "808525347", "-5959.74667E+58232168", "-5.95974667E+58232171", "5.95974667E+58232171", "-4.81860624E+58232180", "-1.35664382E-58232163", "0", "808525347", "3.5796302E-54");
            mathtest(112, def, "-17220490.6E+726428704", "19.9855688", "-1.72204906E+726428711", "-1.72204906E+726428711", "-3.44161300E+726428712", "-8.61646259E+726428709", "", "", "");
            mathtest(113, def, "59015.9705", "-72070405.4E+322957279", "-7.20704054E+322957286", "7.20704054E+322957286", "-4.25330492E+322957291", "-8.18865527E-322957283", "0", "59015.9705", "4.01063488E-34");
            mathtest(114, def, "16411470E+578192008", "497470.005E-377473621", "1.64114700E+578192015", "1.64114700E+578192015", "8.16421406E+200718399", "3.29898684E+955665630", "", "", "");
            mathtest(115, def, "-107.353544E+609689808", "-659.50136E-456711743", "-1.07353544E+609689810", "-1.07353544E+609689810", "7.07998083E+152978069", "", "", "", "");
            mathtest(116, def, "786.134163", "-53.0292275E-664419768", "786.134163", "786.134163", "-4.16880874E-664419764", "-1.48245449E+664419769", "", "", "3.33055532E-15");
            mathtest(117, def, "23.5414714", "5000786.91", "5000810.45", "-5000763.37", "117725882", "0.0000047075534", "0", "23.5414714", "4.4895618E+6860247");
            mathtest(118, def, "-69775.6113", "561292120.", "561222344", "-561361896", "-3.91645008E+13", "-0.000124312473", "0", "-69775.6113", "");
            mathtest(119, def, "919043.871", "-71606613.7", "-70687569.8", "72525657.6", "-6.58096194E+13", "-0.0128346227", "0", "919043.871", "3.05862429E-427014317");
            mathtest(120, def, "-27667.1915", "-293455.107E-789181924", "-27667.1915", "-27667.1915", "8.11907864E-789181915", "9.42808315E+789181922", "", "", "-4.72176938E-14");
            mathtest(121, def, "-908603625.", "-982.409273E+449441134", "-9.82409273E+449441136", "9.82409273E+449441136", "8.92620627E+449441145", "9.2487281E-449441129", "0", "-908603625", "2.60768632E-90");
            mathtest(122, def, "847.113351", "5.71511268", "852.828464", "841.398238", "4841.34825", "148.223386", "148", "1.27667436", "3.69529538E+17");
            mathtest(123, def, "-992140475", "3.82918218", "-992140471", "-992140479", "-3.79908663E+9", "-259099836", "-259099836", "-0.14787752", "9.68930595E+35");
            mathtest(124, def, "-12606437.5", "268123145E+362798858", "2.68123145E+362798866", "-2.68123145E+362798866", "-3.38007767E+362798873", "-4.70173416E-362798860", "0", "-12606437.5", "-2.00344362E+21");
            mathtest(125, def, "3799470.64", "-264.703992", "3799205.94", "3799735.34", "-1.00573505E+9", "-14353.6583", "-14353", "174.242824", "2.3625466E-1744");
            mathtest(126, def, "-8.11070247", "-931284056.E-654288974", "-8.11070247", "-8.11070247", "7.55336789E-654288965", "8.70916067E+654288965", "", "", "-6.58375662E-9");
            mathtest(127, def, "-242660177.", "-6.09832715E-943742415", "-242660177", "-242660177", "1.47982115E-943742406", "3.97912692E+943742422", "", "", "4.89788901E-51");
            mathtest(128, def, "76.1463803", "-45.6758006E-636907996", "76.1463803", "76.1463803", "-3.47804688E-636907993", "-1.66710554E+636907996", "", "", "3.90619287E-10");
            mathtest(129, def, "761185.862", "-70878470.9E+221214712", "-7.08784709E+221214719", "7.08784709E+221214719", "-5.39516900E+221214725", "-1.07393099E-221214714", "0", "761185.862", "6.75406144E-42");
            mathtest(130, def, "6203606.54", "-195.92748E-833512061", "6203606.54", "6203606.54", "-1.21545700E-833512052", "-3.1662769E+833512065", "", "", "2.59843292E-14");
            mathtest(131, def, "-163274837.", "95.0448550E+887876533", "9.50448550E+887876534", "-9.50448550E+887876534", "-1.55184332E+887876543", "-1.71787139E-887876527", "0", "-163274837", "1.34645731E+82");
            mathtest(132, def, "2.38638190", "-807986179.", "-807986177", "807986181", "-1.92816359E+9", "-2.95349347E-9", "0", "2.38638190", "1.19029305E-305208656");
            mathtest(133, def, "-109022296E-811981158", "7.19685680", "7.19685680", "-7.19685680", "-7.84617852E-811981150", "-1.51485988E-811981151", "0", "-1.09022296E-811981150", "");
            mathtest(134, def, "-559250.780E-273710421", "-393780811.", "-393780811", "393780811", "2.20222226E-273710407", "1.42020831E-273710424", "0", "-5.59250780E-273710416", "");
            mathtest(135, def, "-88021.9966E+555334642", "7599686.64E+818884053", "7.59968664E+818884059", "-7.59968664E+818884059", "", "-1.15823192E-263549413", "0", "-8.80219966E+555334646", "");
            mathtest(136, def, "194.317648E-197450009", "-930.979064", "-930.979064", "930.979064", "-1.80905662E-197450004", "-2.08723972E-197450010", "0", "1.94317648E-197450007", "");
            mathtest(137, def, "9495479.65", "7405697.96", "16901177.6", "2089781.69", "7.03206543E+13", "1.28218565", "1", "2089781.69", "1.0135446E+51673383");
            mathtest(138, def, "-1656.28925", "-163050511E-682882380", "-1656.28925", "-1656.28925", "2.70058809E-682882369", "1.01581359E+682882375", "", "", "3.64525265E-7");
            mathtest(139, def, "95581.3784E+64262149", "-99.2879365", "9.55813784E+64262153", "9.55813784E+64262153", "-9.49007783E+64262155", "-9.62668596E+64262151", "", "", "");
            mathtest(140, def, "643761.452", "3.73446939", "643765.186", "643757.718", "2404107.44", "172383.647", "172383", "2.41514363", "1.71751236E+23");
            mathtest(141, def, "7960.49866E-129827423", "3220.22850", "3220.22850", "-3220.22850", "2.56346247E-129827416", "2.47202913E-129827423", "0", "7.96049866E-129827420", "");
            mathtest(142, def, "-6356.64112E-707203818", "1805054.98", "1805054.98", "-1805054.98", "-1.14740867E-707203808", "-3.52157756E-707203821", "0", "-6.35664112E-707203815", "");
            mathtest(143, def, "2.3904042", "8476.52006", "8478.91046", "-8474.12966", "20262.3092", "0.000282003013", "0", "2.3904042", "2.00251752E+3208");
            mathtest(144, def, "-713298.658", "-957.782729", "-714256.441", "-712340.875", "683185135", "744.739528", "744", "-708.307624", "3.68122321E-5608");
            mathtest(145, def, "607779233.E-820497365", "-20.1188742E-857318323", "6.07779233E-820497357", "6.07779233E-820497357", "", "-3.02094057E+36820965", "", "", "");
            mathtest(146, def, "-205888251", "-908.792922E+250680613", "-9.08792922E+250680615", "9.08792922E+250680615", "1.87109785E+250680624", "2.26551336E-250680608", "0", "-205888251", "-1.5042358E-75");
            mathtest(147, def, "51542399.1", "-23212.2414", "51519186.9", "51565611.3", "-1.19641461E+12", "-2220.4835", "-2220", "11223.1920", "1.71641348E-179015");
            mathtest(148, def, "4.44287230", "158923023", "158923027", "-158923019", "706074697", "2.79561275E-8", "0", "4.44287230", "7.12573416E+102928693");
            mathtest(149, def, "-79123682.6", "-3.8571770", "-79123686.5", "-79123678.8", "305194049", "20513365.8", "20513365", "-2.9293950", "2.55137345E-32");
            mathtest(150, def, "-80.3324347E-569715030", "883142.351", "883142.351", "-883142.351", "-7.09449752E-569715023", "-9.09620455E-569715035", "0", "-8.03324347E-569715029", "");
            mathtest(151, def, "13637.483", "-52798.5631", "-39161.0801", "66436.0461", "-720039507", "-0.258292692", "0", "13637.483", "1.47163791E-218310");
            mathtest(152, def, "6.42934843E-276476458", "84057440.0E-388039782", "6.42934843E-276476458", "6.42934843E-276476458", "5.40434570E-664516232", "7.64875593E+111563316", "", "", "");
            mathtest(153, def, "-5.64133087", "-17401297.", "-17401302.6", "17401291.4", "98166473.9", "3.24190253E-7", "0", "-5.64133087", "-1.25908916E-13075014");
            mathtest(154, def, "95469.7057E+865733824", "198.829749", "9.54697057E+865733828", "9.54697057E+865733828", "1.89822176E+865733831", "4.80158056E+865733826", "", "", "");
            mathtest(155, def, "-416466.209", "-930153427", "-930569893", "929736961", "3.87377472E+14", "0.000447739262", "0", "-416466.209", "");
            mathtest(156, def, "-1541733.85", "-1.99208708", "-1541735.84", "-1541731.86", "3071268.08", "773928.944", "773928", "-1.88034976", "4.20708401E-13");
            mathtest(157, def, "-39152691.8", "-645131748.", "-684284440", "605979056", "2.52586445E+16", "0.0606894513", "0", "-39152691.8", "");
            mathtest(158, def, "113.939979", "-58282550.4", "-58282436.5", "58282664.3", "-6.64071257E+9", "-0.0000019549587", "0", "113.939979", "2.106557E-119868330");
            mathtest(159, def, "-324971.736", "-9517.15154", "-334488.888", "-315454.585", "3.09280526E+9", "34.1459033", "34", "-1388.58364", "-5.82795263E-52457");
            mathtest(160, def, "-76.9436744", "-9548122.75E-273599728", "-76.9436744", "-76.9436744", "7.34667648E-273599720", "8.05851332E+273599722", "", "", "1.37489895E-19");
            mathtest(161, def, "-430393.282", "-70.2551505", "-430463.537", "-430323.027", "30237344.8", "6126.14561", "6126", "-10.2300370", "4.26006409E-395");
            mathtest(162, def, "-3308051.90", "-349433799.E+397813188", "-3.49433799E+397813196", "3.49433799E+397813196", "1.15594514E+397813203", "9.46689161E-397813191", "0", "-3308051.90", "-2.76237768E-20");
            mathtest(163, def, "23.1543212E-655822712", "5848.20853", "5848.20853", "-5848.20853", "1.35411299E-655822707", "3.95921607E-655822715", "0", "2.31543212E-655822711", "");
            mathtest(164, def, "-174.261308E-82902077", "-200096204.", "-200096204", "200096204", "3.48690262E-82902067", "8.70887626E-82902084", "0", "-1.74261308E-82902075", "");
            mathtest(165, def, "-50669105.2", "9105789.01E+609889700", "9.10578901E+609889706", "-9.10578901E+609889706", "-4.61382181E+609889714", "-5.56449366E-609889700", "0", "-50669105.2", "-2.20135008E+69");
            mathtest(166, def, "424768856.", "-971.71757", "424767884", "424769828", "-4.12755361E+11", "-437132.012", "-437132", "11.19076", "2.72651473E-8387");
            mathtest(167, def, "7181.2767", "999117.918", "1006299.19", "-991936.641", "7.17494223E+9", "0.00718761677", "0", "7181.2767", "3.09655124E+3852800");
            mathtest(168, def, "8096417.07E-433694528", "-68.4863363", "-68.4863363", "68.4863363", "-5.54493942E-433694520", "-1.18219451E-433694523", "0", "8.09641707E-433694522", "");
            mathtest(169, def, "1236287.5", "-7119.97299E-176200498", "1236287.50", "1236287.50", "-8.80233361E-176200489", "-1.73636544E+176200500", "", "", "2.26549784E-43");
            mathtest(170, def, "-752995833E-654401067", "-15.2736930E+803939983", "-1.52736930E+803939984", "1.52736930E+803939984", "1.15010272E+149538926", "", "0", "-7.52995833E-654401059", "");
            mathtest(171, def, "702992.459", "-312.689474", "702679.770", "703305.148", "-219818342", "-2248.21274", "-2248", "66.521448", "8.02493322E-1831");
            mathtest(172, def, "-4414.38805", "-17680.4630E-584364536", "-4414.38805", "-4414.38805", "7.80484246E-584364529", "2.49676044E+584364535", "", "", "5.13167312E-8");
            mathtest(173, def, "9.46350807", "7826.65424", "7836.11775", "-7817.19073", "74067.6056", "0.00120913839", "0", "9.46350807", "3.63271495E+7639");
            mathtest(174, def, "2078153.7", "-16934607.3E+233594439", "-1.69346073E+233594446", "1.69346073E+233594446", "-3.51927168E+233594452", "-1.2271638E-233594440", "0", "2078153.7", "2.31549939E-13");
            mathtest(175, def, "-9359.74629", "7.07761788E+252457696", "7.07761788E+252457696", "-7.07761788E+252457696", "-6.62447077E+252457700", "-1.32244301E-252457693", "0", "-9359.74629", "-6.29286677E+27");
            mathtest(176, def, "66.2319284E+730468479", "25.9391685E+221147044", "6.62319284E+730468480", "6.62319284E+730468480", "1.71800115E+951615526", "2.55335588E+509321435", "", "", "");
            mathtest(177, def, "317997088.E-90968742", "-977426.461", "-977426.461", "977426.461", "-3.10818768E-90968728", "-3.2534119E-90968740", "0", "3.17997088E-90968734", "");
            mathtest(178, def, "227473386", "-6759.61390", "227466626", "227480146", "-1.53763226E+12", "-33651.8312", "-33651", "5618.65110", "1.40992627E-56493");
            mathtest(179, def, "-392019.462", "-245456.503", "-637475.965", "-146562.959", "9.62237263E+10", "1.59710359", "1", "-146562.959", "-3.08656533E-1372917");
            mathtest(180, def, "-3619556.28E+587673583", "-3.45236972", "-3.61955628E+587673589", "-3.61955628E+587673589", "1.24960465E+587673590", "1.04842661E+587673589", "", "", "");
            mathtest(181, def, "-249.400704E-923930848", "831102.919", "831102.919", "-831102.919", "-2.07277653E-923930840", "-3.00084019E-923930852", "0", "-2.49400704E-923930846", "");
            mathtest(182, def, "65234.2739E+154949914", "-694581895", "6.52342739E+154949918", "6.52342739E+154949918", "-4.53105456E+154949927", "-9.39187652E+154949909", "", "", "");
            mathtest(183, def, "45.2316213", "-88775083.4", "-88775038.2", "88775128.6", "-4.01544095E+9", "-5.09508069E-7", "0", "45.2316213", "1.92314254E-146962015");
            mathtest(184, def, "331100375.", "442.343378", "331100817", "331099933", "1.46460058E+11", "748514.37", "748514", "163.759708", "6.64011043E+3765");
            mathtest(185, def, "81.8162765", "5.61239515E+467372163", "5.61239515E+467372163", "-5.61239515E+467372163", "4.59185273E+467372165", "1.45777826E-467372162", "0", "81.8162765", "2.99942677E+11");
            mathtest(186, def, "-5738.13069E+789464078", "33969715.0", "-5.73813069E+789464081", "-5.73813069E+789464081", "-1.94922664E+789464089", "-1.68919012E+789464074", "", "", "");
            mathtest(187, def, "-7413.03911", "2.70630320E-254858264", "-7413.03911", "-7413.03911", "-2.00619315E-254858260", "-2.73917539E+254858267", "", "", "-4.07369842E+11");
            mathtest(188, def, "-417696.182", "27400.6002", "-390295.582", "-445096.782", "-1.14451261E+10", "-15.2440523", "-15", "-6687.1790", "-1.58020334E+154017");
            mathtest(189, def, "68.8538735E+655647287", "3198.17933E-132454826", "6.88538735E+655647288", "6.88538735E+655647288", "2.20207035E+523192466", "2.15290846E+788102111", "", "", "");
            mathtest(190, def, "-6817.04246", "434420.439", "427603.397", "-441237.481", "-2.96146258E+9", "-0.0156922692", "0", "-6817.04246", "5.94143518E+1665390");
            mathtest(191, def, "8578.27511", "647042.341E-490924334", "8578.27511", "8578.27511", "5.55050721E-490924325", "1.3257672E+490924332", "", "", "3.98473846E+23");
            mathtest(192, def, "4124.11615E+733109424", "597385828E+375928745", "4.12411615E+733109427", "4.12411615E+733109427", "", "6.9036056E+357180673", "", "", "");
            mathtest(193, def, "102.714400", "-919017.468", "-918914.754", "919120.182", "-94396327.8", "-0.000111765449", "0", "102.714400", "4.04295689E-1848724");
            mathtest(194, def, "-4614.33015E+996778733", "-433.560812E+22860599", "-4.61433015E+996778736", "-4.61433015E+996778736", "", "1.06428672E+973918135", "", "", "");
            mathtest(195, def, "457455170.", "3709230.48E+677010879", "3.70923048E+677010885", "-3.70923048E+677010885", "1.69680666E+677010894", "1.23328861E-677010877", "0", "457455170", "4.37919376E+34");
            mathtest(196, def, "-2522468.15", "-48482043.5", "-51004511.7", "45959575.4", "1.22294411E+14", "0.0520289156", "0", "-2522468.15", "1.42348178E-310373595");
            mathtest(197, def, "-659811384", "62777.6118", "-659748606", "-659874162", "-4.14213829E+13", "-10510.2976", "-10510", "-18683.9820", "3.4393524E+553665");
            mathtest(198, def, "4424.94176", "-825848.20", "-821423.258", "830273.142", "-3.65433019E+9", "-0.00535805704", "0", "4424.94176", "3.42152775E-3010966");
            mathtest(199, def, "43.6441884", "-6509.89663E-614169377", "43.6441884", "43.6441884", "-2.84119155E-614169372", "-6.70428286E+614169374", "", "", "3.31524056E-12");
            mathtest(200, def, "897.388381E-843864876", "84195.1369", "84195.1369", "-84195.1369", "7.55557376E-843864869", "1.06584348E-843864878", "0", "8.97388381E-843864874", "");
            mathtest(201, def, "796199825", "496.76834", "796200322", "796199328", "3.95526865E+11", "1602758.79", "1602758", "393.91828", "6.42647264E+4423");
            mathtest(202, def, "573583582", "1598.69521", "573585181", "573581983", "9.16985325E+11", "358782.323", "358782", "517.16578", "9.91156302E+14004");
            mathtest(203, def, "-783144270.", "6347.71496", "-783137922", "-783150618", "-4.97117660E+12", "-123374.202", "-123374", "-1284.52496", "1.28110803E+56458");
            mathtest(204, def, "26909234.7", "52411.5081", "26961646.2", "26856823.2", "1.41035357E+12", "513.422255", "513", "22131.0447", "9.75836528E+389415");
            mathtest(205, def, "8.21915282", "24859.7841E-843282959", "8.21915282", "8.21915282", "2.04326365E-843282954", "3.30620443E+843282955", "", "", "67.5544731");
            mathtest(206, def, "-688.387710", "82783.5207E-831870858", "-688.387710", "-688.387710", "-5.69871582E-831870851", "-8.31551623E+831870855", "", "", "5.04272012E+22");
            mathtest(207, def, "-9792232.", "-1749.01166", "-9793981.01", "-9790482.99", "1.71267279E+10", "5598.72311", "5598", "-1264.72732", "-8.86985674E-12228");
            mathtest(208, def, "-130.765600", "8.67437427", "-122.091226", "-139.439974", "-1134.30976", "-15.0749317", "-15", "-0.64998595", "-1.11799947E+19");
            mathtest(209, def, "917.259102", "-368640.426", "-367723.167", "369557.685", "-338138786", "-0.00248822169", "0", "917.259102", "8.67104255E-1092094");
            mathtest(210, def, "-4.9725631", "-294563717.", "-294563722", "294563712", "1.46473667E+9", "1.6881112E-8", "0", "-4.9725631", "-6.27962584E-205187284");
            mathtest(211, def, "-60962887.2E-514249661", "-243021.407", "-243021.407", "243021.407", "1.48152866E-514249648", "2.5085398E-514249659", "0", "-6.09628872E-514249654", "");
            mathtest(212, def, "-55389219.8", "-3772200E+981866393", "-3.77220000E+981866399", "3.77220000E+981866399", "2.08939215E+981866407", "1.46835321E-981866392", "0", "-55389219.8", "1.06242678E-31");
            mathtest(213, def, "681.666010", "626886700", "626887382", "-626886018", "4.27327356E+11", "0.00000108738311", "0", "681.666010", "");
            mathtest(214, def, "6.42652138", "53465894.5", "53465900.9", "-53465888.1", "343599714", "1.2019852E-7", "0", "6.42652138", "4.61155532E+43199157");
            mathtest(215, def, "561546656", "651408.476", "562198064", "560895248", "3.65796251E+14", "862.049968", "862", "32549.688", "8.6052377E+5699419");
            mathtest(216, def, "7845778.36E-79951139", "9.45859047", "9.45859047", "-9.45859047", "7.42100044E-79951132", "8.29487056E-79951134", "0", "7.84577836E-79951133", "1.12648216E-719560189");
            mathtest(217, def, "54486.2112", "10.7565078", "54496.9677", "54475.4547", "586081.356", "5065.41828", "5065", "4.4991930", "1.25647168E+52");
            mathtest(218, def, "16576482.5", "-2217720.83", "14358761.7", "18794203.3", "-3.67620105E+13", "-7.47455779", "-7", "1052436.69", "1.38259374E-16010820");
            mathtest(219, def, "61.2793787E-392070111", "6.22575651", "6.22575651", "-6.22575651", "3.81510491E-392070109", "9.84288072E-392070111", "0", "6.12793787E-392070110", "");
            mathtest(220, def, "5115136.39", "-653674372.", "-648559236", "658789508", "-3.34363357E+15", "-0.00782520565", "0", "5115136.39", "");
            mathtest(221, def, "-7.84238366E-416477339", "-37432758.9E+97369393", "-3.74327589E+97369400", "3.74327589E+97369400", "2.93562057E-319107938", "2.09505895E-513846739", "0", "-7.84238366E-416477339", "");
            mathtest(222, def, "-387781.3E+284108380", "-218085.592", "-3.87781300E+284108385", "-3.87781300E+284108385", "8.45695144E+284108390", "1.77811517E+284108380", "", "", "");
            mathtest(223, def, "-5353.17736", "3.39332346E+546685359", "3.39332346E+546685359", "-3.39332346E+546685359", "-1.81650623E+546685363", "-1.57756177E-546685356", "0", "-5353.17736", "-1.53403369E+11");
            mathtest(224, def, "-20837.2900E-168652772", "-8236.78305E-712819173", "-2.08372900E-168652768", "-2.08372900E-168652768", "1.71632237E-881471937", "2.52978497E+544166401", "", "", "");
            mathtest(225, def, "-98573.8722E+829022366", "309011.007", "-9.85738722E+829022370", "-9.85738722E+829022370", "-3.04604115E+829022376", "-3.18997932E+829022365", "", "", "");
            mathtest(226, def, "49730750.7", "-5315.10636E-299586991", "49730750.7", "49730750.7", "-2.64324229E-299586980", "-9.35649211E+299586994", "", "", "3.28756936E-39");
            mathtest(227, def, "1539523.40", "-962388.581", "577134.82", "2501911.98", "-1.48161974E+12", "-1.59969001", "-1", "577134.819", "3.10144834E-5954673");
            mathtest(228, def, "81596.2121", "-37600.9653", "43995.2468", "119197.177", "-3.06809634E+9", "-2.17005631", "-2", "6394.2815", "1.97878299E-184684");
            mathtest(229, def, "590146199", "-1425404.61", "588720794", "591571604", "-8.41197113E+14", "-414.020128", "-414", "28690.46", "2.04650994E-12502170");
            mathtest(230, def, "196.05543", "505.936305", "701.991735", "-309.880875", "99191.5598", "0.387510104", "0", "196.05543", "8.78437397E+1159");
            mathtest(231, def, "77.8058449", "-642.275274", "-564.469429", "720.081119", "-49972.7704", "-0.121140963", "0", "77.8058449", "9.33582626E-1215");
            mathtest(232, def, "1468.60684", "10068.138", "11536.7448", "-8599.5312", "14786136.3", "0.145866777", "0", "1468.60684", "2.54122484E+31884");
            mathtest(233, def, "4.98774767E-387968632", "4.41731439E-578812376", "4.98774767E-387968632", "4.98774767E-387968632", "2.20324496E-966781007", "1.12913577E+190843744", "", "", "");
            mathtest(234, def, "981.091059", "-92238.9930", "-91257.9020", "93220.0841", "-90494851.3", "-0.0106364025", "0", "981.091059", "5.29943342E-275953");
            mathtest(235, def, "-3606.24992", "8290224.70", "8286618.45", "-8293830.95", "-2.98966222E+10", "-0.000435000262", "0", "-3606.24992", "-1.23747107E+29488793");
            mathtest(236, def, "-8978571.35", "92243.4796", "-8886327.87", "-9070814.83", "-8.28214663E+11", "-97.3355666", "-97", "-30953.8288", "-4.95762813E+641384");
            mathtest(237, def, "-61968.1992E+810060478", "474294671.E+179263414", "-6.19681992E+810060482", "-6.19681992E+810060482", "-2.93911867E+989323905", "-1.30653374E+630797060", "", "", "");
            mathtest(238, def, "61298431.6E-754429041", "-2584862.79", "-2584862.79", "2584862.79", "-1.58448035E-754429027", "-2.37143851E-754429040", "0", "6.12984316E-754429034", "");
            mathtest(239, def, "621039.064", "-5351539.62", "-4730500.56", "5972578.68", "-3.32351516E+12", "-0.116048672", "0", "621039.064", "2.41163312E-31002108");
            mathtest(240, def, "-19.6007605", "-57905696.", "-57905715.6", "57905676.4", "1.13499568E+9", "3.38494515E-7", "0", "-19.6007605", "1.05663646E-74829963");
            mathtest(241, def, "3626.13109E+687030346", "189.896004", "3.62613109E+687030349", "3.62613109E+687030349", "6.88587804E+687030351", "1.90953523E+687030347", "", "", "");
            mathtest(242, def, "-249334.026", "-7.54735834E-14137188", "-249334.026", "-249334.026", "1.88181324E-14137182", "3.30359332E+14137192", "", "", "6.69495408E-44");
            mathtest(243, def, "417613928.", "-925213.216", "416688715", "418539141", "-3.86381925E+14", "-451.370474", "-451", "342767.584", "8.38430085E-7976054");
            mathtest(244, def, "23.8320309", "-50074996.1", "-50074972.3", "50075019.9", "-1.19338885E+9", "-4.75926765E-7", "0", "23.8320309", "5.81466387E-68961335");
            mathtest(245, def, "49789677.7", "-131827812E+156412534", "-1.31827812E+156412542", "1.31827812E+156412542", "-6.56366427E+156412549", "-3.77687204E-156412535", "0", "49789677.7", "2.00844843E-8");
            mathtest(246, def, "-8907163.61E-741867246", "773651.288E-472033282", "7.73651288E-472033277", "-7.73651288E-472033277", "", "-1.15131504E-269833963", "0", "-8.90716361E-741867240", "");
            mathtest(247, def, "514021711.E+463536646", "617441659.", "5.14021711E+463536654", "5.14021711E+463536654", "3.17378418E+463536663", "8.32502478E+463536645", "", "", "");
            mathtest(248, def, "998175750", "2.39285478", "998175752", "998175748", "2.38848961E+9", "417148487", "417148486", "1.30513692", "9.96354828E+17");
            mathtest(249, def, "873575426.", "647853.152E+497450781", "6.47853152E+497450786", "-6.47853152E+497450786", "5.65948593E+497450795", "1.3484158E-497450778", "0", "873575426", "4.44429064E+53");
            mathtest(250, def, "4352626.8", "-130338048.E-744560911", "4352626.80", "4352626.80", "-5.67312881E-744560897", "-3.33949055E+744560909", "", "", "2.29746322E-7");
            mathtest(251, def, "437.286960", "7.37560835", "444.662568", "429.911352", "3225.25735", "59.2882565", "59", "2.12606735", "3.05749452E+18");
            mathtest(252, def, "8498280.45E+220511522", "588617612", "8.49828045E+220511528", "8.49828045E+220511528", "5.00223754E+220511537", "1.44376931E+220511520", "", "", "");
            mathtest(253, def, "-5320387.77", "-7673237.46", "-12993625.2", "2352849.69", "4.08245987E+13", "0.693369363", "0", "-5320387.77", "-1.30113745E-51609757");
            mathtest(254, def, "587655375", "-4.9748366", "587655370", "587655380", "-2.92348947E+9", "-118125563", "-118125563", "0.7919942", "1.42687667E-44");
            mathtest(255, def, "1266098.44", "-2661.64904E-642601142", "1266098.44", "1266098.44", "-3.36990970E-642601133", "-4.75681963E+642601144", "", "", "4.92717036E-19");
            mathtest(256, def, "3.92737463E+482873483", "-685.522747", "3.92737463E+482873483", "3.92737463E+482873483", "-2.69230464E+482873486", "-5.72902161E+482873480", "", "", "");
            mathtest(257, def, "22826494.1", "986189474.", "1.00901597E+9", "-963362980", "2.25112482E+16", "0.0231461547", "0", "22826494.1", "");
            mathtest(258, def, "-647342.380", "-498816386", "-499463728", "498169044", "3.22904986E+14", "0.00129775685", "0", "-647342.380", "");
            mathtest(259, def, "393092373.", "-25.7226822", "393092347", "393092399", "-1.01113902E+10", "-15281935.6", "-15281935", "15.5939430", "3.49252839E-224");
            mathtest(260, def, "2.96253492", "20.7444888", "23.7070237", "-17.7819539", "61.4562725", "0.142810698", "0", "2.96253492", "8.03402246E+9");
            mathtest(261, def, "53553.3750E+386955423", "-732470876", "5.35533750E+386955427", "5.35533750E+386955427", "-3.92262875E+386955436", "-7.31133165E+386955418", "", "", "");
            mathtest(262, def, "-696451.406E-286535917", "-73086090.8", "-73086090.8", "73086090.8", "5.09009107E-286535904", "9.52919219E-286535920", "0", "-6.96451406E-286535912", "");
            mathtest(263, def, "1551.29957", "-580358622.E+117017265", "-5.80358622E+117017273", "5.80358622E+117017273", "-9.00310081E+117017276", "-2.67300168E-117017271", "0", "1551.29957", "7.17506711E-20");
            mathtest(264, def, "-205123006.E-213752799", "-78638468.6", "-78638468.6", "78638468.6", "1.61305591E-213752783", "2.60843083E-213752799", "0", "-2.05123006E-213752791", "");
            mathtest(265, def, "77632.8073", "-3378542.88E+677441319", "-3.37854288E+677441325", "3.37854288E+677441325", "-2.62285768E+677441330", "-2.29781921E-677441321", "0", "77632.8073", "2.13729331E-15");
            mathtest(266, def, "3068999.37", "2.21006212", "3069001.58", "3068997.16", "6782679.25", "1388648.46", "1388648", "1.02718624", "9.41875713E+12");
            mathtest(267, def, "625524274.", "55.2468624", "625524329", "625524219", "3.45582535E+10", "11322349.3", "11322349", "16.7522224", "6.21482943E+483");
            mathtest(268, def, "61269134.9", "-845761303.", "-784492168", "907030438", "-5.18190634E+16", "-0.0724425848", "0", "61269134.9", "");
            mathtest(269, def, "-2840.12099", "-2856.76731E-82743650", "-2840.12099", "-2840.12099", "8.11356480E-82743644", "9.94173022E+82743649", "", "", "-4.36505254E-11");
            mathtest(270, def, "8.9538781", "-7.56603391", "1.38784419", "16.5199120", "-67.7453453", "-1.18343087", "-1", "1.38784419", "2.42053061E-8");
            mathtest(271, def, "-56233547.2", "509752530", "453518983", "-565986077", "-2.86651930E+16", "-0.110315386", "0", "-56233547.2", "");
            mathtest(272, def, "-3167.47853E-854859497", "-110852115", "-110852115", "110852115", "3.51121694E-854859486", "2.85739116E-854859502", "0", "-3.16747853E-854859494", "");
            mathtest(273, def, "-5652.52092", "-632243244.", "-632248897", "632237592", "3.57376816E+12", "0.00000894042123", "0", "-5652.52092", "");
            mathtest(274, def, "-946.009928", "820090.66E-589278015", "-946.009928", "-946.009928", "-7.75813906E-589278007", "-1.15354311E+589278012", "", "", "6.41454053E+23");
            mathtest(275, def, "-367.757758", "-959.626016", "-1327.38377", "591.868258", "352909.912", "0.383230292", "0", "-367.757758", "1.14982199E-2463");
            mathtest(276, def, "809926721.E-744611554", "-67.6560549", "-67.6560549", "67.6560549", "-5.47964467E-744611544", "-1.19712378E-744611547", "0", "8.09926721E-744611546", "");
            mathtest(277, def, "-1725.08555", "75586.3031", "73861.2176", "-77311.3887", "-130392839", "-0.0228227269", "0", "-1725.08555", "3.70540587E+244657");
            mathtest(278, def, "2659.84191E+29314492", "-74372.4551E+518196680", "-7.43724551E+518196684", "7.43724551E+518196684", "-1.97818973E+547511180", "-3.5763804E-488882190", "0", "2.65984191E+29314495", "1.06171811E-205201468");
            mathtest(279, def, "-91.1431113", "12147507.0", "12147415.9", "-12147598.1", "-1.10716158E+9", "-0.00000750303015", "0", "-91.1431113", "-1.52417006E+23805759");
            mathtest(280, def, "-1136778.91E+697783878", "-801552569.", "-1.13677891E+697783884", "-1.13677891E+697783884", "9.11188056E+697783892", "1.41822128E+697783875", "", "", "");
            mathtest(281, def, "73123773.0E+433334149", "63.3548930", "7.31237730E+433334156", "7.31237730E+433334156", "4.63274881E+433334158", "1.15419298E+433334155", "", "", "");
            mathtest(282, def, "-9765484.8", "7979.90802E-234029715", "-9765484.80", "-9765484.80", "-7.79276705E-234029705", "-1.22375907E+234029718", "", "", "8.27085614E+55");
            mathtest(283, def, "-695010288", "-8.26582820", "-695010296", "-695010280", "5.74483564E+9", "84082353.4", "84082353", "-3.45024540", "1.83683495E-71");
            mathtest(284, def, "23975643.3E-155955264", "-505547.692E+137258948", "-5.05547692E+137258953", "5.05547692E+137258953", "-1.21208311E-18696303", "-4.7425087E-293214211", "0", "2.39756433E-155955257", "1.26225952E+779776283");
            mathtest(285, def, "2862.95921", "-32601248.6E-605861333", "2862.95921", "2862.95921", "-9.33360449E-605861323", "-8.78174712E+605861328", "", "", "4.26142175E-11");
            mathtest(286, def, "-13.133518E+246090516", "-8.71269925E-945092108", "-1.31335180E+246090517", "-1.31335180E+246090517", "1.14428392E-699001590", "", "", "", "");
            mathtest(287, def, "-34671.2232", "817710.762", "783039.539", "-852381.985", "-2.83510323E+10", "-0.0424003508", "0", "-34671.2232", "-5.30788828E+3712382");
            mathtest(288, def, "-22464769", "62.4366060", "-22464706.6", "-22464831.4", "-1.40262393E+9", "-359801.252", "-359801", "-15.7245940", "6.21042536E+455");
            mathtest(289, def, "-9458.60887E-563051963", "5676056.01", "5676056.01", "-5676056.01", "-5.36875937E-563051953", "-1.66640513E-563051966", "0", "-9.45860887E-563051960", "");
            mathtest(290, def, "-591.924123E-95331874", "-134.596188", "-134.596188", "134.596188", "7.96707305E-95331870", "4.39777777E-95331874", "0", "-5.91924123E-95331872", "");
            mathtest(291, def, "-182566085.E+68870646", "-960345993.", "-1.82566085E+68870654", "-1.82566085E+68870654", "1.75326608E+68870663", "1.9010449E+68870645", "", "", "");
            mathtest(292, def, "8232.54893", "-99822004E+891979845", "-9.98220040E+891979852", "9.98220040E+891979852", "-8.21789532E+891979856", "-8.24722867E-891979850", "0", "8232.54893", "6.99289156E-40");
            mathtest(293, def, "-4336.94317", "-819373.601E+563233430", "-8.19373601E+563233435", "8.19373601E+563233435", "3.55357674E+563233439", "5.29299841E-563233433", "0", "-4336.94317", "7.98969405E-30");
            mathtest(294, def, "-2.09044362E-876527908", "-6515463.33", "-6515463.33", "6515463.33", "1.36202087E-876527901", "3.20843433E-876527915", "0", "-2.09044362E-876527908", "");
            mathtest(295, def, "-194343.344", "1.95929977", "-194341.385", "-194345.303", "-380776.869", "-99190.2041", "-99190", "-0.39981370", "3.77693354E+10");
            mathtest(296, def, "-326002.927", "4215.99030", "-321786.937", "-330218.917", "-1.37442518E+9", "-77.3253503", "-77", "-1371.67390", "5.51875821E+23243");
            mathtest(297, def, "-12037.8590E+876429044", "314.81827", "-1.20378590E+876429048", "-1.20378590E+876429048", "-3.78973794E+876429050", "-3.82374854E+876429045", "", "", "");
            mathtest(298, def, "21036045.4E-162804809", "-91.7149219", "-91.7149219", "91.7149219", "-1.92931926E-162804800", "-2.2936339E-162804804", "0", "2.10360454E-162804802", "");
            mathtest(299, def, "-947019.534", "9916.29280", "-937103.241", "-956935.827", "-9.39092299E+9", "-95.5013686", "-95", "-4971.71800", "3.76029022E+59261");
            mathtest(300, def, "-5985.84136", "-12.4090184E-12364204", "-5985.84136", "-5985.84136", "7.42784156E-12364200", "4.82378313E+12364206", "", "", "-0.000167060893");
            mathtest(301, def, "-85344379.4", "-6783.08669E+218840215", "-6.78308669E+218840218", "6.78308669E+218840218", "5.78898324E+218840226", "1.25819385E-218840211", "0", "-85344379.4", "-3.03232347E-56");
            mathtest(302, def, "-94.1947070E-938257103", "15003.240", "15003.2400", "-15003.2400", "-1.41322580E-938257097", "-6.27829102E-938257106", "0", "-9.41947070E-938257102", "");
            mathtest(303, def, "-4846233.6", "-8289769.76", "-13136003.4", "3443536.16", "4.01741607E+13", "0.584604125", "0", "-4846233.6", "4.25077524E-55420465");
            mathtest(304, def, "67.9147198", "-108373645.E+291715415", "-1.08373645E+291715423", "1.08373645E+291715423", "-7.36016573E+291715424", "-6.26671916E-291715422", "0", "67.9147198", "0.0147243485");
            mathtest(305, def, "1958.77994", "5.57285137E+690137826", "5.57285137E+690137826", "-5.57285137E+690137826", "1.09159895E+690137830", "3.51486126E-690137824", "0", "1958.77994", "5.64824968E+19");
            mathtest(306, def, "22780314.3", "8805279.83", "31585594.1", "13975034.5", "2.00587042E+14", "2.58711986", "2", "5169754.64", "2.39132169E+64785373");
            mathtest(307, def, "596745.184", "197602423.", "198199168", "-197005678", "1.17918294E+14", "0.00301992848", "0", "596745.184", "");
            mathtest(308, def, "171.340497", "-480349.924", "-480178.584", "480521.264", "-82303394.7", "-0.000356699332", "0", "171.340497", "2.17914102E-1073035");
            mathtest(309, def, "824.65555", "-379287.530", "-378462.875", "380112.186", "-312781567", "-0.00217422268", "0", "824.65555", "6.35829256E-1106108");
            mathtest(310, def, "19.3164031", "-9207644.24E+988115069", "-9.20764424E+988115075", "9.20764424E+988115075", "-1.77858568E+988115077", "-2.09786592E-988115075", "0", "19.3164031", "2.67093711E-12");
            mathtest(311, def, "-3123.77646E+177814265", "973284435.E+383256112", "9.73284435E+383256120", "-9.73284435E+383256120", "-3.04032301E+561070389", "-3.20952062E-205441853", "0", "-3.12377646E+177814268", "");
            mathtest(312, def, "-850.123915E+662955309", "6774849.81E-846576865", "-8.50123915E+662955311", "-8.50123915E+662955311", "-5.75946184E-183621547", "", "", "", "");
            mathtest(313, def, "-23349.7724", "2921.35355", "-20428.4189", "-26271.1260", "-68212940.5", "-7.99279238", "-7", "-2900.29755", "-5.6705546E+12759");
            mathtest(314, def, "18886653.3", "568707476.", "587594129", "-549820823", "1.07409809E+16", "0.0332097855", "0", "18886653.3", "");
            mathtest(315, def, "-90552818.0", "-542.03563E-986606878", "-90552818.0", "-90552818.0", "4.90828538E-986606868", "1.67060638E+986606883", "", "", "-1.64244241E-40");
            mathtest(316, def, "41501126.1E+791838765", "-69.6651675E+204268348", "4.15011261E+791838772", "4.15011261E+791838772", "-2.89118290E+996107122", "-5.95722763E+587570422", "", "", "");
            mathtest(317, def, "76783193.3E-271488154", "3765.01829E-520346003", "7.67831933E-271488147", "7.67831933E-271488147", "2.89090127E-791834146", "2.03938434E+248857853", "", "", "");
            mathtest(318, def, "4192.9928", "987822007E-146560989", "4192.99280", "4192.99280", "4.14193056E-146560977", "4.24468454E+146560983", "", "", "1.67973653E+36");
            mathtest(319, def, "-891845.629", "48277955.", "47386109.4", "-49169800.6", "-4.30564831E+13", "-0.0184731443", "0", "-891845.629", "-6.32964147E+287267817");
            mathtest(320, def, "334.901176", "-7609296.55E+447340228", "-7.60929655E+447340234", "7.60929655E+447340234", "-2.54836236E+447340237", "-4.40121073E-447340233", "0", "334.901176", "6.31926575E-21");
            mathtest(321, def, "4.49868636", "-341880896E-447251873", "4.49868636", "4.49868636", "-1.53801492E-447251864", "-1.31586363E+447251865", "", "", "0.010983553");
            mathtest(322, def, "807615.58", "-314286480", "-313478865", "315094096", "-2.53822658E+14", "-0.00256967968", "0", "807615.58", "");
            mathtest(323, def, "-37.7457954", "53277.8129E-859225538", "-37.7457954", "-37.7457954", "-2.01101343E-859225532", "-7.08471188E+859225534", "", "", "-76620134.1");
            mathtest(324, def, "-28671081.", "98.8819623", "-28670982.1", "-28671179.9", "-2.83505275E+9", "-289952.589", "-289952", "-58.2671904", "-1.93625566E+738");
            mathtest(325, def, "-89752.2106E-469496896", "99.9879961", "99.9879961", "-99.9879961", "-8.97414368E-469496890", "-8.97629857E-469496894", "0", "-8.97522106E-469496892", "");
            mathtest(326, def, "-497983567E-13538052", "39.4578742", "39.4578742", "-39.4578742", "-1.96493729E-13538042", "-1.26206385E-13538045", "0", "-4.97983567E-13538044", "-1.55376543E-527983689");
            mathtest(327, def, "845739221E-654202565", "-33313.1551", "-33313.1551", "33313.1551", "-2.81742418E-654202552", "-2.53875449E-654202561", "0", "8.45739221E-654202557", "");
            mathtest(328, def, "742.332067E+537827843", "-4532.70023E-855387414", "7.42332067E+537827845", "7.42332067E+537827845", "-3.36476873E-317559565", "", "", "", "");
            mathtest(329, def, "-893.48654", "670389960", "670389067", "-670390853", "-5.98984406E+11", "-0.00000133278628", "0", "-893.48654", "");
            mathtest(330, def, "1.37697162", "-915.737474E-351578724", "1.37697162", "1.37697162", "-1.26094451E-351578721", "-1.50367508E+351578721", "", "", "0.0561920784");
            mathtest(331, def, "-65.2839808E+550288403", "-121389.306", "-6.52839808E+550288404", "-6.52839808E+550288404", "7.92477712E+550288409", "5.37806689E+550288399", "", "", "");
            mathtest(332, def, "-30346603.E+346067390", "792661.544", "-3.03466030E+346067397", "-3.03466030E+346067397", "-2.40545852E+346067403", "-3.82844396E+346067391", "", "", "");
            mathtest(333, def, "-61170.7065", "-453731131.", "-453792302", "453669960", "2.77550538E+13", "0.000134817081", "0", "-61170.7065", "");
            mathtest(334, def, "6569.51133", "13.8706351E+399434914", "1.38706351E+399434915", "-1.38706351E+399434915", "9.11232944E+399434918", "4.73627291E-399434912", "0", "6569.51133", "6569.51133");
            mathtest(335, def, "300703925.", "-3156736.8", "297547188", "303860662", "-9.49243146E+14", "-95.2578387", "-95", "813929.0", "4.18609114E-26763256");
            mathtest(336, def, "192138216E+353011592", "-473.080633", "1.92138216E+353011600", "1.92138216E+353011600", "-9.08968688E+353011602", "-4.06142637E+353011597", "", "", "");
            mathtest(337, def, "8607.64794", "-34740.3367", "-26132.6888", "43347.9846", "-299032588", "-0.247770999", "0", "8607.64794", "1.29604519E-136698");
            mathtest(338, def, "-67913.8241", "-93815.4229", "-161729.247", "25901.5988", "6.37136413E+9", "0.723908948", "0", "-67913.8241", "-6.96355203E-453311");
            mathtest(339, def, "34.5559455", "-998799398.", "-998799364", "998799433", "-3.45144576E+10", "-3.45974833E-8", "0", "34.5559455", "");
            mathtest(340, def, "387995.328", "990199543.E-124623607", "387995.328", "387995.328", "3.84192796E-124623593", "3.91835495E+124623603", "", "", "7.73152138E+55");
            mathtest(341, def, "-471.09166E-83521919", "-441222368", "-441222368", "441222368", "2.07856178E-83521908", "1.06769669E-83521925", "0", "-4.7109166E-83521917", "");
            mathtest(342, def, "-97834.3858", "70779789.8E+502166065", "7.07797898E+502166072", "-7.07797898E+502166072", "-6.92469726E+502166077", "-1.38223617E-502166068", "0", "-97834.3858", "-8.57907886E+34");
            mathtest(343, def, "7732331.06", "-952719.482E+115325505", "-9.52719482E+115325510", "9.52719482E+115325510", "-7.36674244E+115325517", "-8.11606271E-115325505", "0", "7732331.06", "1.30886724E-69");
            mathtest(344, def, "23.2745547", "2.23194245E-221062592", "23.2745547", "23.2745547", "5.19474666E-221062591", "1.04279368E+221062593", "", "", "541.704896");
            mathtest(345, def, "671.083363E-218324205", "-787150031", "-787150031", "787150031", "-5.28243290E-218324194", "-8.52548227E-218324212", "0", "6.71083363E-218324203", "");
            mathtest(346, def, "365167.80", "-80263.6516", "284904.148", "445431.452", "-2.93097011E+10", "-4.54960362", "-4", "44113.1936", "1.27052227E-446468");
            mathtest(347, def, "-1.43297604E-65129780", "56.598733E-135581942", "-1.43297604E-65129780", "-1.43297604E-65129780", "-8.11046283E-200711721", "-2.53181646E+70452160", "", "", "8.65831881E-390778680");
            mathtest(348, def, "416998859.", "260.220323E-349285593", "416998859", "416998859", "1.08511578E-349285582", "1.60248383E+349285599", "", "", "7.25111178E+25");
            mathtest(349, def, "7267.17611E+862630607", "4021.56861", "7.26717611E+862630610", "7.26717611E+862630610", "2.92254473E+862630614", "1.80705014E+862630607", "", "", "");
            mathtest(350, def, "12.2142434E+593908740", "5.27236571E-396050748", "1.22142434E+593908741", "1.22142434E+593908741", "6.43979581E+197857993", "2.3166533E+989959488", "", "", "");
            mathtest(351, def, "-28.591932", "-1.79153238E-817064576", "-28.5919320", "-28.5919320", "5.12233720E-817064575", "1.59594838E+817064577", "", "", "0.00122324372");
            mathtest(352, def, "590.849666", "753424.306E+277232744", "7.53424306E+277232749", "-7.53424306E+277232749", "4.45160500E+277232752", "7.84219014E-277232748", "0", "590.849666", "1.48530607E+22");
            mathtest(353, def, "1.7270628", "-1325026.67", "-1325024.94", "1325028.40", "-2288404.27", "-0.00000130341739", "0", "1.7270628", "2.09260036E-314440");
            mathtest(354, def, "33402118.", "-5534.83745", "33396583.2", "33407652.8", "-1.84875294E+11", "-6034.8869", "-6034", "4908.82670", "8.14473913E-41645");
            mathtest(355, def, "-439842.506", "-775110.807", "-1214953.31", "335268.301", "3.40926680E+11", "0.567457584", "0", "-439842.506", "-1.84678472E-4374182");
            mathtest(356, def, "-248664.779", "-440890.44E+666433944", "-4.40890440E+666433949", "4.40890440E+666433949", "1.09633924E+666433955", "5.64005831E-666433945", "0", "-248664.779", "2.61542877E-22");
            mathtest(357, def, "-14161.9142", "8306.49493", "-5855.4193", "-22468.4091", "-117635869", "-1.70492059", "-1", "-5855.41927", "1.65573372E+34479");
            mathtest(358, def, "-6417227.13", "16679.8842", "-6400547.25", "-6433907.01", "-1.07038605E+11", "-384.728518", "-384", "-12151.5972", "3.58767978E+113546");
            mathtest(359, def, "514825024.", "-25.0446345E-103809457", "514825024", "514825024", "-1.28936046E-103809447", "-2.05563002E+103809464", "", "", "7.32860062E-27");
            mathtest(360, def, "525948196", "219450390", "745398586", "306497806", "1.15419537E+17", "2.39666102", "2", "87047416", "");
            mathtest(361, def, "-638509.181", "45580189.0E+269212559", "4.55801890E+269212566", "-4.55801890E+269212566", "-2.91033691E+269212572", "-1.40084803E-269212561", "0", "-638509.181", "-1.06129405E+29");
            mathtest(362, def, "330590422", "74.359928E+535377965", "7.43599280E+535377966", "-7.43599280E+535377966", "2.45826800E+535377975", "4.44581418E-535377959", "0", "330590422", "4.31550742E+59");
            mathtest(363, def, "-3.48593871E-940579904", "-20265.9640E-322988987", "-2.02659640E-322988983", "2.02659640E-322988983", "", "1.72009519E-617590921", "0", "-3.48593871E-940579904", "");
            mathtest(364, def, "-328103480.", "-721.949371E-923938665", "-328103480", "-328103480", "2.36874101E-923938654", "4.54468822E+923938670", "", "", "-2.4430038E-60");
            mathtest(365, def, "-1857.01448", "19081578.1", "19079721.1", "-19083435.1", "-3.54347668E+10", "-0.0000973197537", "0", "-1857.01448", "8.44397087E+62374153");
            mathtest(366, def, "347.28720E+145930771", "-62821.9906E-676564106", "3.47287200E+145930773", "3.47287200E+145930773", "-2.18172732E-530633328", "-5.52811518E+822494874", "", "", "5.69990135E-875584642");
            mathtest(367, def, "-643.211399E+441807003", "-50733419.2", "-6.43211399E+441807005", "-6.43211399E+441807005", "3.26323135E+441807013", "1.26782584E+441806998", "", "", "");
            mathtest(368, def, "-53991661.4E-843339554", "20718.7346", "20718.7346", "-20718.7346", "-1.11863890E-843339542", "-2.60593431E-843339551", "0", "-5.39916614E-843339547", "");
            mathtest(369, def, "-900181424", "-105763982.", "-1.00594541E+9", "-794417442", "9.52067719E+16", "8.51122856", "8", "-54069568", "1.32627061E-947045602");
            mathtest(370, def, "94218.7462E+563233951", "19262.6382E+765263890", "1.92626382E+765263894", "-1.92626382E+765263894", "", "4.89126906E-202029939", "0", "9.42187462E+563233955", "");
            mathtest(371, def, "28549.271E+921331828", "-2150590.40", "2.85492710E+921331832", "2.85492710E+921331832", "-6.13977881E+921331838", "-1.32750853E+921331826", "", "", "");
            mathtest(372, def, "810.7080E+779625763", "5957.94044", "8.10708000E+779625765", "8.10708000E+779625765", "4.83014998E+779625769", "1.36071854E+779625762", "", "", "");
            mathtest(373, def, "-23.7357549E+77116908", "351.100649E+864348022", "3.51100649E+864348024", "-3.51100649E+864348024", "-8.33363895E+941464933", "-6.7603848E-787231116", "0", "-2.37357549E+77116909", "3.17403853E+308467637");
            mathtest(374, def, "40216102.2E+292724544", "661.025962", "4.02161022E+292724551", "4.02161022E+292724551", "2.65838876E+292724554", "6.08389148E+292724548", "", "", "");
            mathtest(375, def, "22785024.3E+783719168", "399.505989E+137478666", "2.27850243E+783719175", "2.27850243E+783719175", "9.10275367E+921197843", "5.70329981E+646240506", "", "", "");
            mathtest(376, def, "515.591819E+821371364", "-692137914.E-149498690", "5.15591819E+821371366", "5.15591819E+821371366", "-3.56860646E+671872685", "-7.44926421E+970870047", "", "", "");
            mathtest(377, def, "-536883072E+477911251", "624996.301", "-5.36883072E+477911259", "-5.36883072E+477911259", "-3.35549934E+477911265", "-8.59017999E+477911253", "", "", "");
            mathtest(378, def, "-399492.914E-334369192", "5202119.87E+442442258", "5.20211987E+442442264", "-5.20211987E+442442264", "-2.07821003E+108073078", "-7.67942539E-776811452", "0", "-3.99492914E-334369187", "");
            mathtest(379, def, "762.071184", "9851631.37", "9852393.44", "-9850869.30", "7.50764438E+9", "0.0000773548213", "0", "762.071184", "4.02198436E+28392356");
            mathtest(380, def, "5626.12471", "72989818.3", "72995444.4", "-72984192.2", "4.10649820E+11", "0.0000770809524", "0", "5626.12471", "1.79814757E+273727098");
            mathtest(381, def, "-47207260.1", "-2073.3152", "-47209333.4", "-47205186.8", "9.78755299E+10", "22768.9741", "22768", "-2019.6264", "-6.02238319E-15909");
            mathtest(382, def, "207.740860", "-51.0390090", "156.701851", "258.779869", "-10602.8876", "-4.07023694", "-4", "3.5848240", "6.40297515E-119");
            mathtest(383, def, "-572.812464E-745934021", "-182805872.E+604508681", "-1.82805872E+604508689", "1.82805872E+604508689", "1.04713482E-141425329", "", "0", "-5.72812464E-745934019", "");
            mathtest(384, def, "-6418504E+3531407", "8459416.1", "-6.41850400E+3531413", "-6.41850400E+3531413", "-5.42967961E+3531420", "-7.58740784E+3531406", "", "", "");
            mathtest(385, def, "280689.531", "-128212543", "-127931854", "128493233", "-3.59879186E+13", "-0.00218925173", "0", "280689.531", "1.42173809E-698530938");
            mathtest(386, def, "15.803551E-783422793", "239108038E-489186308", "2.39108038E-489186300", "-2.39108038E-489186300", "", "6.60937672E-294236493", "0", "1.5803551E-783422792", "");
            mathtest(387, def, "26.515922", "-9418242.96E-105481628", "26.5159220", "26.5159220", "-2.49733396E-105481620", "-2.81537885E+105481622", "", "", "1.54326108E-13");
            mathtest(388, def, "-88.1094557", "-54029934.1", "-54030022.2", "54029846.0", "4.76054809E+9", "0.0000016307526", "0", "-88.1094557", "5.05289826E-105089439");
            mathtest(389, def, "6770.68602E-498420397", "-6.11248908E-729616908", "6.77068602E-498420394", "6.77068602E-498420394", "", "-1.10768067E+231196514", "", "", "");
            mathtest(390, def, "-892973818.E-781904441", "555201299.", "555201299", "-555201299", "-4.95780224E-781904424", "-1.60837847E-781904441", "0", "-8.92973818E-781904433", "");
            mathtest(391, def, "670175802E+135430680", "27355195.4", "6.70175802E+135430688", "6.70175802E+135430688", "1.83327900E+135430696", "2.44990318E+135430681", "", "", "");
            mathtest(392, def, "-440950.26", "205.477469E-677345561", "-440950.260", "-440950.260", "-9.06053434E-677345554", "-2.14597864E+677345564", "", "", "1.94437132E+11");
            mathtest(393, def, "-8.2335779", "573665010E+742722075", "5.73665010E+742722083", "-5.73665010E+742722083", "-4.72331555E+742722084", "-1.43525886E-742722083", "0", "-8.2335779", "311552.753");
            mathtest(394, def, "452943.863", "7022.23629", "459966.099", "445921.627", "3.18067883E+9", "64.5013703", "64", "3520.74044", "5.54158976E+39716");
            mathtest(395, def, "62874.1079", "-52719654.1", "-52656780.0", "52782528.2", "-3.31470122E+12", "-0.0011926123", "0", "62874.1079", "1.18819936E-252973775");
            mathtest(396, def, "-7428.41741E+609772037", "-46024819.3", "-7.42841741E+609772040", "-7.42841741E+609772040", "3.41891569E+609772048", "1.61400251E+609772033", "", "", "");
            mathtest(397, def, "2.27959297", "41937.019", "41939.2986", "-41934.7394", "95599.3337", "0.0000543575348", "0", "2.27959297", "2.89712423E+15007");
            mathtest(398, def, "508692408E-671967782", "8491989.20", "8491989.20", "-8491989.20", "4.31981043E-671967767", "5.99026207E-671967781", "0", "5.08692408E-671967774", "");
            mathtest(399, def, "940.533705E-379310421", "-4.01176961E+464620037", "-4.01176961E+464620037", "4.01176961E+464620037", "-3.77320453E+85309619", "-2.34443599E-843930456", "0", "9.40533705E-379310419", "");
            mathtest(400, def, "97.0649652", "-92.4485649E-151989098", "97.0649652", "97.0649652", "-8.97351673E-151989095", "-1.0499348E+151989098", "", "", "1.30748728E-18");
            mathtest(401, def, "297544.536E+360279473", "8.80275007", "2.97544536E+360279478", "2.97544536E+360279478", "2.61921019E+360279479", "3.38013159E+360279477", "", "", "");
            mathtest(402, def, "-28861028.", "82818.820E+138368758", "8.28188200E+138368762", "-8.28188200E+138368762", "-2.39023628E+138368770", "-3.48483932E-138368756", "0", "-28861028", "4.81387013E+59");
            mathtest(403, def, "36.2496238E+68828039", "49243.00", "3.62496238E+68828040", "3.62496238E+68828040", "1.78504022E+68828045", "7.36137599E+68828035", "", "", "");
            mathtest(404, def, "22.447828E-476014683", "-56067.5520", "-56067.5520", "56067.5520", "-1.25859476E-476014677", "-4.00371109E-476014687", "0", "2.2447828E-476014682", "");
            mathtest(405, def, "282688.791E+75011952", "5.99789051", "2.82688791E+75011957", "2.82688791E+75011957", "1.69553642E+75011958", "4.7131369E+75011956", "", "", "5.10330507E+450071744");
            mathtest(406, def, "-981.860310E-737387002", "-994046289", "-994046289", "994046289", "9.76014597E-737386991", "9.87741035E-737387009", "0", "-9.81860310E-737387000", "");
            mathtest(407, def, "-702.91210", "-6444903.55", "-6445606.46", "6444200.64", "4.53020069E+9", "0.000109064797", "0", "-702.91210", "1.70866703E-18348004");
            mathtest(408, def, "972456720E-17536823", "16371.2590", "16371.2590", "-16371.2590", "1.59203408E-17536810", "5.94002404E-17536819", "0", "9.72456720E-17536815", "");
            mathtest(409, def, "71471.2045", "-74303278.4", "-74231807.2", "74374749.6", "-5.31054481E+12", "-0.00096188494", "0", "71471.2045", "2.14535374E-360677853");
            mathtest(410, def, "643.103951E+439708441", "788251925.", "6.43103951E+439708443", "6.43103951E+439708443", "5.06927927E+439708452", "8.15860933E+439708434", "", "", "");
            mathtest(411, def, "4.30838663", "-7.43110827", "-3.12272164", "11.7394949", "-32.0160875", "-0.579777131", "0", "4.30838663", "0.0000362908645");
            mathtest(412, def, "823.678025", "-513.581840E-324453141", "823.678025", "823.678025", "-4.23026076E-324453136", "-1.60379118E+324453141", "", "", "2.63762228E-15");
            mathtest(413, def, "4461.81162", "3.22081680", "4465.03244", "4458.59080", "14370.6778", "1385.30438", "1385", "0.98035200", "8.8824688E+10");
            mathtest(414, def, "-4458527.10", "-99072605", "-103531132", "94614077.9", "4.41717894E+14", "0.0450026231", "0", "-4458527.10", "-6.23928099E-658752715");
            mathtest(415, def, "-577964618", "487424368.", "-90540250", "-1.06538899E+9", "-2.81714039E+17", "-1.18575241", "-1", "-90540250", "");
            mathtest(416, def, "-867.036184", "-57.1768608", "-924.213045", "-809.859323", "49574.4072", "15.1641096", "15", "-9.3832720", "-3.40312837E-168");
            mathtest(417, def, "771871921E-330504770", "5.34285236", "5.34285236", "-5.34285236", "4.12399771E-330504761", "1.44468136E-330504762", "0", "7.71871921E-330504762", "");
            mathtest(418, def, "-338683.062E-728777518", "166441931", "166441931", "-166441931", "-5.63710628E-728777505", "-2.03484218E-728777521", "0", "-3.38683062E-728777513", "");
            mathtest(419, def, "-512568743", "-416376887.E-965945295", "-512568743", "-512568743", "2.13421778E-965945278", "1.23102112E+965945295", "", "", "1.44874358E-35");
            mathtest(420, def, "7447181.99", "5318438.52", "12765620.5", "2128743.47", "3.96073796E+13", "1.40025723", "1", "2128743.47", "1.21634782E+36548270");
            mathtest(421, def, "54789.8207", "93165435.2", "93220225.0", "-93110645.4", "5.10451749E+12", "0.000588091716", "0", "54789.8207", "3.80769825E+441483035");
            mathtest(422, def, "41488.5960", "146.797094", "41635.3931", "41341.7989", "6090405.33", "282.625459", "282", "91.815492", "6.84738153E+678");
            mathtest(423, def, "785741.663E+56754529", "-461.531732", "7.85741663E+56754534", "7.85741663E+56754534", "-3.62644711E+56754537", "-1.70246509E+56754532", "", "", "");
            mathtest(424, def, "-4.95436786", "-3132.4233", "-3137.37767", "3127.46893", "15519.1773", "0.0015816406", "0", "-4.95436786", "1.98062422E-2177");
            mathtest(425, def, "77321.8478E+404626874", "82.4797688", "7.73218478E+404626878", "7.73218478E+404626878", "6.37748813E+404626880", "9.3746441E+404626876", "", "", "");
            mathtest(426, def, "-7.99307725", "-29153.7273", "-29161.7204", "29145.7342", "233027.994", "0.000274169994", "0", "-7.99307725", "1.88688028E-26318");
            mathtest(427, def, "-61.6337401E+474999517", "5254.87092", "-6.16337401E+474999518", "-6.16337401E+474999518", "-3.23877349E+474999522", "-1.1728878E+474999515", "", "", "");
            mathtest(428, def, "-16.4043088", "35.0064812", "18.6021724", "-51.4107900", "-574.257128", "-0.468607762", "0", "-16.4043088", "-3.33831843E+42");
            mathtest(429, def, "-8.41156520", "-56508958.9", "-56508967.3", "56508950.5", "475328792", "1.48853657E-7", "0", "-8.41156520", "-8.86365458E-52263827");
            mathtest(430, def, "-360165.79E+503559835", "-196688.515", "-3.60165790E+503559840", "-3.60165790E+503559840", "7.08404744E+503559845", "1.83114805E+503559835", "", "", "");
            mathtest(431, def, "-653236480.E+565648495", "-930.445274", "-6.53236480E+565648503", "-6.53236480E+565648503", "6.07800796E+565648506", "7.02068674E+565648500", "", "", "");
            mathtest(432, def, "-3.73342903", "855.029289", "851.295860", "-858.762718", "-3192.19117", "-0.00436643408", "0", "-3.73342903", "-1.41988961E+489");
            mathtest(433, def, "-5.14890532E+562048011", "10847127.8E-390918910", "-5.14890532E+562048011", "-5.14890532E+562048011", "-5.58508340E+171129108", "-4.74679142E+952966914", "", "", "-5.14890532E+562048011");
            mathtest(434, def, "653311907", "-810.036965E+744537823", "-8.10036965E+744537825", "8.10036965E+744537825", "-5.29206794E+744537834", "-8.06521104E-744537818", "0", "653311907", "3.01325171E-71");
            mathtest(435, def, "-1.31557907", "98.9139300E-579281802", "-1.31557907", "-1.31557907", "-1.30129096E-579281800", "-1.33002406E+579281800", "", "", "15.529932");
            mathtest(436, def, "-875192389", "-72071565.6", "-947263955", "-803120823", "6.30764857E+16", "12.1433797", "12", "-10333601.8", "1.25564408E-644471405");
            mathtest(437, def, "-72838078.8", "-391.398423", "-72838470.2", "-72837687.4", "2.85087092E+10", "186097.017", "186097", "-6.474969", "-6.574057E-3075");
            mathtest(438, def, "29186560.9", "-79.7419988", "29186481.2", "29186640.6", "-2.32739470E+9", "-366012.407", "-366012", "32.4352144", "6.10050869E-598");
            mathtest(439, def, "-329801660E-730249465", "-6489.9256", "-6489.92560", "6489.92560", "2.14038824E-730249453", "5.08174793E-730249461", "0", "-3.29801660E-730249457", "");
            mathtest(440, def, "91.8429117E+103164883", "7131455.16", "9.18429117E+103164884", "9.18429117E+103164884", "6.54973607E+103164891", "1.28785654E+103164878", "", "", "");
            mathtest(441, def, "3943866.38E+150855113", "-31927007.3", "3.94386638E+150855119", "3.94386638E+150855119", "-1.25915851E+150855127", "-1.23527594E+150855112", "", "", "");
            mathtest(442, def, "-7002.0468E-795962156", "-5937891.05", "-5937891.05", "5937891.05", "4.15773910E-795962146", "1.17921443E-795962159", "0", "-7.0020468E-795962153", "");
            mathtest(443, def, "696504605.", "54506.4617", "696559111", "696450099", "3.79640016E+13", "12778.386", "12778", "21037.3974", "2.6008532E+481992");
            mathtest(444, def, "-5115.76467", "690.960979E+815126701", "6.90960979E+815126703", "-6.90960979E+815126703", "-3.53479376E+815126707", "-7.4038402E-815126701", "0", "-5115.76467", "-9.17009655E+25");
            mathtest(445, def, "-261.279392", "-613.079357", "-874.358749", "351.799965", "160185.002", "0.426175484", "0", "-261.279392", "-2.06318841E-1482");
            mathtest(446, def, "-591407763", "-80145822.8", "-671553586", "-511261940", "4.73988618E+16", "7.37914644", "7", "-30387003.4", "-2.79334522E-703030105");
            mathtest(447, def, "615630407", "-69.4661869", "615630338", "615630476", "-4.27654969E+10", "-8862303.15", "-8862303", "10.4375693", "3.44283102E-607");
            mathtest(448, def, "1078757.50", "27402569.0E-713742082", "1078757.50", "1078757.50", "2.95607268E-713742069", "3.93670207E+713742080", "", "", "1.25536924E+18");
            mathtest(449, def, "-4865.60358E-401116515", "66952.5315", "66952.5315", "-66952.5315", "-3.25764477E-401116507", "-7.26724363E-401116517", "0", "-4.86560358E-401116512", "");
            mathtest(450, def, "-87805.3921E-934896690", "-1875.14745", "-1875.14745", "1875.14745", "1.64648057E-934896682", "4.68258601E-934896689", "0", "-8.78053921E-934896686", "");
            mathtest(451, def, "-232540609.E+602702520", "68.0834223", "-2.32540609E+602702528", "-2.32540609E+602702528", "-1.58321605E+602702530", "-3.41552468E+602702526", "", "", "");
            mathtest(452, def, "-320610803.", "-863871235.", "-1.18448204E+9", "543260432", "2.76966450E+17", "0.37113263", "0", "-320610803", "");
            mathtest(453, def, "-303956364E+278139979", "229537.920E+479603725", "2.29537920E+479603730", "-2.29537920E+479603730", "-6.97695116E+757743717", "-1.3242098E-201463743", "0", "-3.03956364E+278139987", "9.23894712E+556279974");
            mathtest(454, def, "-439.747348", "74.9494457E-353117582", "-439.747348", "-439.747348", "-3.29588200E-353117578", "-5.86725284E+353117582", "", "", "-3.17996693E+18");
            mathtest(455, def, "-89702231.9", "1.28993993", "-89702230.6", "-89702233.2", "-115710491", "-69539852.1", "-69539852", "-0.07890964", "-89702231.9");
            mathtest(456, def, "-5856939.14", "-6743375.34", "-12600314.5", "886436.20", "3.94955390E+13", "0.868547107", "0", "-5856939.14", "-3.29213248E-45636942");
            mathtest(457, def, "733317.669E+100381349", "-13832.6792E+174055607", "-1.38326792E+174055611", "1.38326792E+174055611", "-1.01437481E+274436966", "-5.30134227E-73674257", "0", "7.33317669E+100381354", "1.36366549E-100381355");
            mathtest(458, def, "87.4798787E-80124704", "108497.32", "108497.320", "-108497.320", "9.49133239E-80124698", "8.06286079E-80124708", "0", "8.74798787E-80124703", "");
            mathtest(459, def, "-694562052", "310681.319E+549445264", "3.10681319E+549445269", "-3.10681319E+549445269", "-2.15787454E+549445278", "-2.23560932E-549445261", "0", "-694562052", "-3.35068155E+26");
            mathtest(460, def, "-9744135.85", "1797016.04", "-7947119.81", "-11541151.9", "-1.75103684E+13", "-5.42239782", "-5", "-759055.65", "3.83848006E+12558883");
            mathtest(461, def, "3625.87308", "-50.2208536E+658627487", "-5.02208536E+658627488", "5.02208536E+658627488", "-1.82094441E+658627492", "-7.21985554E-658627486", "0", "3625.87308", "1.5956477E-18");
            mathtest(462, def, "365347.52", "-3655414.47", "-3290066.95", "4020761.99", "-1.33549661E+12", "-0.099946948", "0", "365347.52", "1.02663257E-20333994");
            mathtest(463, def, "-19706333.6E-816923050", "-383858032.", "-383858032", "383858032", "7.56443443E-816923035", "5.1337557E-816923052", "0", "-1.97063336E-816923043", "");
            mathtest(464, def, "-86346.2616", "-98.8063785", "-86445.0680", "-86247.4552", "8531561.41", "873.893598", "873", "-88.2931695", "-2.05064086E-489");
            mathtest(465, def, "-445588.160E-496592215", "328.822976", "328.822976", "-328.822976", "-1.46519625E-496592207", "-1.35510044E-496592212", "0", "-4.45588160E-496592210", "");
            mathtest(466, def, "-9709213.71", "-34.6690137", "-9709248.38", "-9709179.04", "336608863", "280054.512", "280054", "-17.7472602", "-2.80903974E-245");
            mathtest(467, def, "742395536.", "-43533.6889", "742352002", "742439070", "-3.23192163E+13", "-17053.3569", "-17053", "15539.1883", "5.7622734E-386175");
            mathtest(468, def, "-878849193.", "-5842982.47E-972537342", "-878849193", "-878849193", "5.13510043E-972537327", "1.50411061E+972537344", "", "", "2.17027042E-54");
            mathtest(469, def, "-78014142.1", "-624658.522", "-78638800.6", "-77389483.6", "4.87321987E+13", "124.890863", "124", "-556485.372", "-7.86063865E-4929918");
            mathtest(470, def, "857039.371", "454.379672", "857493.751", "856584.991", "389421268", "1886.17454", "1886", "79.309608", "3.82253101E+2693");
            mathtest(471, def, "166534010.", "-173.012236", "166533837", "166534183", "-2.88124214E+10", "-962556.255", "-962556", "44.164784", "4.78620664E-1423");
            mathtest(472, def, "-810.879063", "43776.610", "42965.7309", "-44587.4891", "-35497536.5", "-0.0185231123", "0", "-810.879063", "-2.34758691E+127345");
            mathtest(473, def, "-327.127935", "93458944", "93458616.9", "-93459271.1", "-3.05730314E+10", "-0.00000350023145", "0", "-327.127935", "2.29323021E+235022854");
            mathtest(474, def, "539295218.", "-9587941.10E-309643098", "539295218", "539295218", "-5.17073079E-309643083", "-5.62472394E+309643099", "", "", "4.80545269E-88");
            mathtest(475, def, "-3862702.65", "879616.733", "-2983085.92", "-4742319.38", "-3.39769789E+12", "-4.3913474", "-4", "-344235.718", "-3.50650167E+5793941");
            mathtest(476, def, "-8.25290500", "992.091584E+256070257", "9.92091584E+256070259", "-9.92091584E+256070259", "-8.18763759E+256070260", "-8.31869268E-256070260", "0", "-8.25290500", "1.46577888E+9");
            mathtest(477, def, "546875205.", "447.52857E+557357101", "4.47528570E+557357103", "-4.47528570E+557357103", "2.44742278E+557357112", "1.22198948E-557357095", "0", "546875205", "8.94443542E+34");
            mathtest(478, def, "177623437", "-7779116.14", "169844321", "185402553", "-1.38175335E+15", "-22.83337", "-22", "6482881.92", "2.90085309E-64173820");
            mathtest(479, def, "377204735.", "13768.1401", "377218503", "377190967", "5.19340764E+12", "27396.9274", "27396", "12768.8204", "2.06065297E+118082");
            mathtest(480, def, "-2435.49239", "-11732.0640E-23331504", "-2435.49239", "-2435.49239", "2.85733526E-23331497", "2.07592832E+23331503", "", "", "-0.00041059459");
            mathtest(481, def, "-6128465.14E-137123294", "-5742264.27", "-5742264.27", "5742264.27", "3.51912664E-137123281", "1.06725585E-137123294", "0", "-6.12846514E-137123288", "");
            mathtest(482, def, "-2898065.44", "-5.11638105", "-2898070.56", "-2898060.32", "14827607.1", "566428.773", "566428", "-3.95461060", "-4.89169151E-33");
            mathtest(483, def, "1851395.31E+594383160", "-550301.475", "1.85139531E+594383166", "1.85139531E+594383166", "-1.01882557E+594383172", "-3.36432918E+594383160", "", "", "");
            mathtest(484, def, "536412589.E+379583977", "899.601161", "5.36412589E+379583985", "5.36412589E+379583985", "4.82557388E+379583988", "5.96278231E+379583982", "", "", "");
            mathtest(485, def, "185.85297", "867419480.", "867419666", "-867419294", "1.61212487E+11", "2.14259622E-7", "0", "185.85297", "");
            mathtest(486, def, "-5.26631053", "-3815941.35E+183291763", "-3.81594135E+183291769", "3.81594135E+183291769", "2.00959321E+183291770", "1.38008162E-183291769", "0", "-5.26631053", "0.00130009218");
            mathtest(487, def, "-8.11587021E-245942806", "4553.06753E+943412048", "4.55306753E+943412051", "-4.55306753E+943412051", "-3.69521051E+697469246", "", "0", "-8.11587021E-245942806", "");
            mathtest(488, def, "-405765.352", "854963231", "854557466", "-855368996", "-3.46914456E+14", "-0.000474599769", "0", "-405765.352", "");
            mathtest(489, def, "-159.609757", "-43356.7567", "-43516.3665", "43197.1470", "6920161.40", "0.00368131219", "0", "-159.609757", "-8.95397849E-95519");
            mathtest(490, def, "-564240.241E-501316672", "-557.781977", "-557.781977", "557.781977", "3.14723037E-501316664", "1.01157847E-501316669", "0", "-5.64240241E-501316667", "");
            mathtest(491, def, "318847.270", "582107878.E+399633412", "5.82107878E+399633420", "-5.82107878E+399633420", "1.85603508E+399633426", "5.47746014E-399633416", "0", "318847.270", "1.0507423E+33");
            mathtest(492, def, "-4426.59663", "95.1096765", "-4331.48695", "-4521.70631", "-421012.173", "-46.5420217", "-46", "-51.5515110", "-2.38037379E+346");
            mathtest(493, def, "6037.28310", "578264.105", "584301.388", "-572226.822", "3.49114411E+9", "0.010440356", "0", "6037.28310", "3.57279483E+2186324");
            mathtest(494, def, "-66.9556692", "-53.8519404", "-120.807610", "-13.1037288", "3605.69271", "1.24332881", "1", "-13.1037288", "2.55554086E-99");
            mathtest(495, def, "-92486.0222", "-59935.8544", "-152421.877", "-32550.1678", "5.54322876E+9", "1.5430834", "1", "-32550.1678", "1.83152656E-297647");
            mathtest(496, def, "852136219.E+917787351", "9246221.91", "8.52136219E+917787359", "8.52136219E+917787359", "7.87904058E+917787366", "9.21604767E+917787352", "", "", "");
            mathtest(497, def, "-2120096.16E-269253718", "9437.00514", "9437.00514", "-9437.00514", "-2.00073584E-269253708", "-2.24657731E-269253716", "0", "-2.12009616E-269253712", "");
            mathtest(498, def, "-524653.169E-865784226", "228054.698", "228054.698", "-228054.698", "-1.19649620E-865784215", "-2.30055848E-865784226", "0", "-5.24653169E-865784221", "");
            mathtest(499, def, "-288193133", "-312268737.", "-600461870", "24075604", "8.99937057E+16", "0.922901011", "0", "-288193133", "");
            mathtest(500, def, "-373484759E-113589964", "844101958E-852538240", "-3.73484759E-113589956", "-3.73484759E-113589956", "-3.15259216E-966128187", "-4.42464036E+738948275", "", "", "3.78602147E-908719644");
        }

        /* mathtest -- general arithmetic test routine
         Arg1  is test number
         Arg2  is MathContext
         Arg3  is left hand side (LHS)
         Arg4  is right hand side (RHS)
         Arg5  is the expected result for add
         Arg6  is the expected result for subtract
         Arg7  is the expected result for multiply
         Arg8  is the expected result for divide
         Arg9  is the expected result for integerDivide
         Arg10 is the expected result for remainder
         Arg11 is the expected result for power

         For power RHS, 0 is added to the number, any exponent is removed and
         the number is then rounded to an integer, using format(rhs+0,,0)

         If an error should result for an operation, the 'expected result' is
         an empty string.
         */

        private void mathtest(int test, MathContext mc,
                string slhs, string srhs, string add,
                string sub, string mul, string div,
                string idv, string rem, string pow)
        {
            BigDecimal lhs;
            BigDecimal rhs;
            string res = null;
            string sn = null;
            int e = 0;
            NumberStyle numberStyle = NumberStyle.Float;
            CultureInfo provider = CultureInfo.InvariantCulture;

            lhs = BigDecimal.Parse(slhs, numberStyle, provider);
            rhs = BigDecimal.Parse(srhs, numberStyle, provider);

            try
            {
                res = lhs.Add(rhs, mc).ToString(CultureInfo.InvariantCulture);
            }
            catch (ArithmeticException e137)
            {
                res = "";
            }
            mathtestcheck(test, lhs, rhs, "add", res, add);

            try
            {
                res = lhs.Subtract(rhs, mc).ToString(CultureInfo.InvariantCulture);
            }
            catch (ArithmeticException e138)
            {
                res = "";
            }
            mathtestcheck(test, lhs, rhs, "sub", res, sub);

            try
            {
                res = lhs.Multiply(rhs, mc).ToString(CultureInfo.InvariantCulture);
            }
            catch (ArithmeticException e139)
            {
                res = "";
            }
            mathtestcheck(test, lhs, rhs, "mul", res, mul);

            try
            {
                res = lhs.Divide(rhs, mc).ToString(CultureInfo.InvariantCulture);
            }
            catch (ArithmeticException e140)
            {
                res = "";
            }
            mathtestcheck(test, lhs, rhs, "div", res, div);

            try
            {
                res = lhs.DivideInteger(rhs, mc).ToString(CultureInfo.InvariantCulture);
            }
            catch (ArithmeticException e141)
            {
                res = "";
            }
            mathtestcheck(test, lhs, rhs, "idv", res, idv);

            try
            {
                res = lhs.Remainder(rhs, mc).ToString(CultureInfo.InvariantCulture);
            }
            catch (ArithmeticException e142)
            {
                res = "";
            }
            mathtestcheck(test, lhs, rhs, "rem", res, rem);

            try
            {
                // prepare an integer from the rhs
                // in Rexx:
                //   n=rhs+0
                //   e=pos('E', n)
                //   if e>0 then n=left(n,e-1)
                //   n=format(n,,0)

                sn = rhs.Plus(mc).ToString(CultureInfo.InvariantCulture);
                e = sn.IndexOf("E", 0);
                if (e > 0)
                    sn = sn.Substring(0, e); // ICU4N: Checked 2nd parameter
                sn = (BigDecimal.Parse(sn, numberStyle, provider)).Format(-1, 0);

                res = lhs.Pow(BigDecimal.Parse(sn, numberStyle, provider), mc).ToString(CultureInfo.InvariantCulture);
            }
            catch (ArithmeticException e143)
            {
                res = "";
            }
            mathtestcheck(test, lhs, rhs, "pow", res, pow);
        }

        /* mathtestcheck -- check for general mathtest error
         Arg1  is test number
         Arg2  is left hand side (LHS)
         Arg3  is right hand side (RHS)
         Arg4  is the operation
         Arg5  is the actual result
         Arg6  is the expected result
         Show error message if a problem, otherwise return quietly
         */

        private void mathtestcheck(int test, BigDecimal lhs,
                BigDecimal rhs, string op,
                string got, string want)
        {
            bool flag;
            string testnum;

            flag = want.Equals(got);

            if ((!flag))
                say(">" + test + ">" + " " + lhs.ToString(CultureInfo.InvariantCulture) + " " + op + " "
                        + rhs.ToString(CultureInfo.InvariantCulture) + " " + "=" + " " + want + " " + "[got"
                        + " " + got + "]");

            testnum = "gen"
                    + right((new BigDecimal(test + 1000))
                            .ToString(CultureInfo.InvariantCulture), 3);

            TestFmwk.assertTrue(testnum, flag);
        }

        /* ------------------------------------------------------------------ */
        /* Support routines and minor classes follow                          */
        /* ------------------------------------------------------------------ */

        /* ----------------------------------------------------------------- */
        /* Method called to summarise pending tests                          */
        /* ----------------------------------------------------------------- */
        /* Arg1 is section name */

        //    private void summary(string section) {
        //        int bad;
        //        int count;
        //        int i = 0;
        //        Test item = null;
        //        bad = 0;
        //        count = Tests.size();
        //        {
        //            int $144 = count;
        //            i = 0;
        //            for (; $144 > 0; $144--, i++) {
        //                item = (Test) (Tests.get(i));
        //                if ((!item.ok))
        //                {
        //                    bad++;
        //                    errln("Failed:" + " " + item.name);
        //                }
        //            }
        //        }/*i*/
        //        totalcount = totalcount + count;
        //        Tests = new java.util.ArrayList(100); // reinitialize
        //        if (bad == 0)
        //            say("OK" + " " + left(section, 14) + " "
        //                    + right("[" + count + " " + "tests]", 12));
        //        else
        //            throw new DiagException(section + " " + "[failed" + " " + bad + " "
        //                    + "of" + " " + count + " " + "tests]", bad);
        //    }

        /* ----------------------------------------------------------------- */
        /* right - Utility to do a 'right' on a Java String                  */
        /* ----------------------------------------------------------------- */
        /* Arg1 is string to right-justify */
        /* Arg2 is desired length */

        private static string right(string s, int len)
        {
            int slen;
            slen = s.Length;
            if (slen == len)
                return s; // length just right
            if (slen > len)
                return s.Substring(slen - len); // truncate on left
                                                // too short
            return string.Concat(new string(new char[len - slen])
                    .Replace('\0', ' '), s);
        }

        /* ----------------------------------------------------------------- */
        /* say - Utility to do a display                                     */
        /* ----------------------------------------------------------------- */
        /* Arg1 is string to display, omitted if none */
        /*         [null or omitted gives blank line] */
        // this version doesn't heed continuation final character
        private void say(string s)
        {
            if (s == null)
                s = "  ";
            Logln(s);
        }


        public class CharSequences : TestFmwk
        {

            #region ParseTestCase

            public abstract class ParseTestCase
            {
                #region TestParse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_ForFloatStyle_Data

                public static IEnumerable<TestCaseData> TestParse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_ForFloatStyle_Data
                {
                    get
                    {
                        var style = NumberStyle.Float;
                        var provider = CultureInfo.InvariantCulture;
                        string charSequence = "123.45";
                        //                            id  | char sequence | startIndex | length | expected
                        yield return new TestCaseData("cca101", charSequence, 0, 6, style, provider, "123.45");
                        yield return new TestCaseData("cca102", charSequence, 1, 5, style, provider, "23.45");
                        yield return new TestCaseData("cca103", charSequence, 2, 4, style, provider, "3.45");
                        yield return new TestCaseData("cca104", charSequence, 3, 3, style, provider, "0.45");
                        yield return new TestCaseData("cca105", charSequence, 4, 2, style, provider, "45");
                        yield return new TestCaseData("cca106", charSequence, 5, 1, style, provider, "5");
  

                        yield return new TestCaseData("cca110", charSequence, 0, 1, style, provider, "1");
                        yield return new TestCaseData("cca111", charSequence, 1, 1, style, provider, "2");
                        yield return new TestCaseData("cca112", charSequence, 2, 1, style, provider, "3");
                        yield return new TestCaseData("cca113", charSequence, 4, 1, style, provider, "4");

                        yield return new TestCaseData("cca120", charSequence, 0, 2, style, provider, "12");
                        yield return new TestCaseData("cca121", charSequence, 1, 2, style, provider, "23");
                        yield return new TestCaseData("cca122", charSequence, 2, 2, style, provider, "3");
                        yield return new TestCaseData("cca123", charSequence, 3, 2, style, provider, "0.4");

                        yield return new TestCaseData("cca130", charSequence, 0, 3, style, provider, "123");
                        yield return new TestCaseData("cca131", charSequence, 1, 3, style, provider, "23");
                        yield return new TestCaseData("cca132", charSequence, 2, 3, style, provider, "3.4");

                        yield return new TestCaseData("cca140", charSequence, 0, 4, style, provider, "123");
                        yield return new TestCaseData("cca141", charSequence, 1, 4, style, provider, "23.4");

                        yield return new TestCaseData("cca150", charSequence, 0, 5, style, provider, "123.4");

                        // a couple of oddies
                        charSequence = "x23.4x";
                        yield return new TestCaseData("cca160", charSequence, 1, 4, style, provider, "23.4");
                        yield return new TestCaseData("cca161", charSequence, 1, 1, style, provider, "2");
                        yield return new TestCaseData("cca162", charSequence, 4, 1, style, provider, "4");

                        charSequence = "0123456789.9876543210";
                        yield return new TestCaseData("cca163", charSequence, 0, 21, style, provider, "123456789.9876543210");
                        yield return new TestCaseData("cca164", charSequence, 1, 20, style, provider, "123456789.9876543210");
                        yield return new TestCaseData("cca165", charSequence, 2, 19, style, provider, "23456789.9876543210");
                        yield return new TestCaseData("cca166", charSequence, 2, 18, style, provider, "23456789.987654321");
                        yield return new TestCaseData("cca167", charSequence, 2, 17, style, provider, "23456789.98765432");
                        yield return new TestCaseData("cca168", charSequence, 02, 16, style, provider, "23456789.9876543");
                        
                    }
                }

                #endregion TestParse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_ForFloatStyle_Data

                #region TestParse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_ForFloatStyleException_Data

                public static IEnumerable<TestCaseData> TestParse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_ForFloatStyleException_Data
                {
                    get
                    {
                        var style = NumberStyle.Float;
                        var provider = CultureInfo.InvariantCulture;

                        //                            id,  expected                       value, startIndex, length,  style, provider, expected message (or null to skip)
                        yield return new TestCaseData("cca200", typeof(ArgumentNullException), null, 0, 1, style, provider, null);
                        yield return new TestCaseData("cca201", typeof(FormatException), "123", 0, 0, style, provider, null);
                        yield return new TestCaseData("cca202", typeof(ArgumentOutOfRangeException), "123", 2, 4, style, provider, null);
                        yield return new TestCaseData("cca203", typeof(ArgumentOutOfRangeException), "123", -1, 2, style, provider, null);
                        yield return new TestCaseData("cca204", typeof(ArgumentOutOfRangeException), "123", 1, -2, style, provider, null);

                    }
                }

                #endregion TestParse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_ForFloatStyleException_Data

                #region TestParse_CharSequence_NumberStyle_IFormatProvider_ForFloatStyle_Data

                public static IEnumerable<TestCaseData> TestParse_CharSequence_NumberStyle_IFormatProvider_ForFloatStyle_Data
                {
                    get
                    {
                        var style = NumberStyle.Float;
                        var provider = CultureInfo.InvariantCulture;
                        string num;
                        //                            id  | char sequence | style | provider | expected
#if FEATURE_IKVM

                        yield return new TestCaseData("cbd001", new java.math.BigDecimal("0").ToString(), style, provider, "0");
                        yield return new TestCaseData("cbd002", new java.math.BigDecimal("1").ToString(), style, provider, "1");
                        yield return new TestCaseData("cbd003", new java.math.BigDecimal("10").ToString(), style, provider, "10");
                        yield return new TestCaseData("cbd004", new java.math.BigDecimal("1000").ToString(), style, provider, "1000");
                        yield return new TestCaseData("cbd005", new java.math.BigDecimal("10.0").ToString(), style, provider, "10.0");
                        yield return new TestCaseData("cbd006", new java.math.BigDecimal("10.1").ToString(), style, provider, "10.1");
                        yield return new TestCaseData("cbd007", new java.math.BigDecimal("-1.1").ToString(), style, provider, "-1.1");
                        yield return new TestCaseData("cbd008", new java.math.BigDecimal("-9.0").ToString(), style, provider, "-9.0");
                        yield return new TestCaseData("cbd009", new java.math.BigDecimal("0.9").ToString(), style, provider, "0.9");

                        num = "123456789.123456789";
                        yield return new TestCaseData("cbd010", new java.math.BigDecimal(num).ToString(), style, provider, num);
                        num = "123456789.000000000";
                        yield return new TestCaseData("cbd011", new java.math.BigDecimal(num).ToString(), style, provider, num);
                        num = "123456789000000000";
                        yield return new TestCaseData("cbd012", new java.math.BigDecimal(num).ToString(), style, provider, num);
                        num = "0.00000123456789";
                        yield return new TestCaseData("cbd013", new java.math.BigDecimal(num).ToString(), style, provider, num);

#endif

                        // strings without E cannot generate E in result
                        yield return new TestCaseData("cst001", "12", style, provider, "12");
                        yield return new TestCaseData("cst002", "-76", style, provider, "-76");
                        yield return new TestCaseData("cst003", "12.76", style, provider, "12.76");
                        yield return new TestCaseData("cst004", "+12.76", style, provider, "12.76");
                        yield return new TestCaseData("cst005", "012.76", style, provider, "12.76");
                        yield return new TestCaseData("cst006", "+0.003", style, provider, "0.003");
                        yield return new TestCaseData("cst007", "17.", style, provider, "17");
                        yield return new TestCaseData("cst008", ".5", style, provider, "0.5");
                        yield return new TestCaseData("cst009", "044", style, provider, "44");
                        yield return new TestCaseData("cst010", "0044", style, provider, "44");
                        yield return new TestCaseData("cst011", "0.0005", style, provider, "0.0005");
                        yield return new TestCaseData("cst012", "00.00005", style, provider, "0.00005");
                        yield return new TestCaseData("cst013", "0.000005", style, provider, "0.000005");
                        yield return new TestCaseData("cst014", "0.0000005", style, provider, "0.0000005"); // \NR
                        yield return new TestCaseData("cst015", "0.00000005", style, provider, "0.00000005"); // \NR
                        yield return new TestCaseData("cst016", "12345678.876543210", style, provider, "12345678.876543210");
                        yield return new TestCaseData("cst017", "2345678.876543210", style, provider, "2345678.876543210");
                        yield return new TestCaseData("cst018", "345678.876543210", style, provider, "345678.876543210");
                        yield return new TestCaseData("cst019", "0345678.87654321", style, provider, "345678.87654321");
                        yield return new TestCaseData("cst020", "345678.8765432", style, provider, "345678.8765432");
                        yield return new TestCaseData("cst021", "+345678.8765432", style, provider, "345678.8765432");
                        yield return new TestCaseData("cst022", "+0345678.8765432", style, provider, "345678.8765432");
                        yield return new TestCaseData("cst023", "+00345678.8765432", style, provider, "345678.8765432");
                        yield return new TestCaseData("cst024", "-345678.8765432", style, provider, "-345678.8765432");
                        yield return new TestCaseData("cst025", "-0345678.8765432", style, provider, "-345678.8765432");
                        yield return new TestCaseData("cst026", "-00345678.8765432", style, provider, "-345678.8765432");

                        // exotics --
                        yield return new TestCaseData("cst035", "\u0e57.\u0e50", style, provider, "7.0");
                        yield return new TestCaseData("cst036", "\u0b66.\u0b67", style, provider, "0.1");
                        yield return new TestCaseData("cst037", "\u0b66\u0b66", style, provider, "0");
                        yield return new TestCaseData("cst038", "\u0b6a\u0b66", style, provider, "40");

                        // strings with E
                        yield return new TestCaseData("cst040", "1E+9", style, provider, "1E+9");
                        yield return new TestCaseData("cst041", "1e+09", style, provider, "1E+9");
                        yield return new TestCaseData("cst042", "1E+90", style, provider, "1E+90");
                        yield return new TestCaseData("cst043", "+1E+009", style, provider, "1E+9");
                        yield return new TestCaseData("cst044", "0E+9", style, provider, "0");
                        yield return new TestCaseData("cst045", "1E+9", style, provider, "1E+9");
                        yield return new TestCaseData("cst046", "1E+09", style, provider, "1E+9");
                        yield return new TestCaseData("cst047", "1e+90", style, provider, "1E+90");
                        yield return new TestCaseData("cst048", "1E+009", style, provider, "1E+9");
                        yield return new TestCaseData("cst049", "0E+9", style, provider, "0");
                        yield return new TestCaseData("cst050", "1E9", style, provider, "1E+9");
                        yield return new TestCaseData("cst051", "1e09", style, provider, "1E+9");
                        yield return new TestCaseData("cst052", "1E90", style, provider, "1E+90");
                        yield return new TestCaseData("cst053", "1E009", style, provider, "1E+9");
                        yield return new TestCaseData("cst054", "0E9", style, provider, "0");
                        yield return new TestCaseData("cst055", "0.000e+0", style, provider, "0");
                        yield return new TestCaseData("cst056", "0.000E-1", style, provider, "0");
                        yield return new TestCaseData("cst057", "4E+9", style, provider, "4E+9");
                        yield return new TestCaseData("cst058", "44E+9", style, provider, "4.4E+10");
                        yield return new TestCaseData("cst059", "0.73e-7", style, provider, "7.3E-8");
                        yield return new TestCaseData("cst060", "00E+9", style, provider, "0");
                        yield return new TestCaseData("cst061", "00E-9", style, provider, "0");
                        yield return new TestCaseData("cst062", "10E+9", style, provider, "1.0E+10");
                        yield return new TestCaseData("cst063", "10E+09", style, provider, "1.0E+10");
                        yield return new TestCaseData("cst064", "10e+90", style, provider, "1.0E+91");
                        yield return new TestCaseData("cst065", "10E+009", style, provider, "1.0E+10");
                        yield return new TestCaseData("cst066", "100e+9", style, provider, "1.00E+11");
                        yield return new TestCaseData("cst067", "100e+09", style, provider, "1.00E+11");
                        yield return new TestCaseData("cst068", "100E+90", style, provider, "1.00E+92");
                        yield return new TestCaseData("cst069", "100e+009", style, provider, "1.00E+11");

                        yield return new TestCaseData("cst070", "1.265", style, provider, "1.265");
                        yield return new TestCaseData("cst071", "1.265E-20", style, provider, "1.265E-20");
                        yield return new TestCaseData("cst072", "1.265E-8", style, provider, "1.265E-8");
                        yield return new TestCaseData("cst073", "1.265E-4", style, provider, "1.265E-4");
                        yield return new TestCaseData("cst074", "1.265E-3", style, provider, "1.265E-3");
                        yield return new TestCaseData("cst075", "1.265E-2", style, provider, "1.265E-2");
                        yield return new TestCaseData("cst076", "1.265E-1", style, provider, "1.265E-1");
                        yield return new TestCaseData("cst077", "1.265E-0", style, provider, "1.265");
                        yield return new TestCaseData("cst078", "1.265E+1", style, provider, "1.265E+1");
                        yield return new TestCaseData("cst079", "1.265E+2", style, provider, "1.265E+2");
                        yield return new TestCaseData("cst080", "1.265E+3", style, provider, "1.265E+3");
                        yield return new TestCaseData("cst081", "1.265E+4", style, provider, "1.265E+4");
                        yield return new TestCaseData("cst082", "1.265E+8", style, provider, "1.265E+8");
                        yield return new TestCaseData("cst083", "1.265E+20", style, provider, "1.265E+20");

                        yield return new TestCaseData("cst090", "12.65", style, provider, "12.65");
                        yield return new TestCaseData("cst091", "12.65E-20", style, provider, "1.265E-19");
                        yield return new TestCaseData("cst092", "12.65E-8", style, provider, "1.265E-7");
                        yield return new TestCaseData("cst093", "12.65E-4", style, provider, "1.265E-3");
                        yield return new TestCaseData("cst094", "12.65E-3", style, provider, "1.265E-2");
                        yield return new TestCaseData("cst095", "12.65E-2", style, provider, "1.265E-1");
                        yield return new TestCaseData("cst096", "12.65E-1", style, provider, "1.265");
                        yield return new TestCaseData("cst097", "12.65E-0", style, provider, "1.265E+1");
                        yield return new TestCaseData("cst098", "12.65E+1", style, provider, "1.265E+2");
                        yield return new TestCaseData("cst099", "12.65E+2", style, provider, "1.265E+3");
                        yield return new TestCaseData("cst100", "12.65E+3", style, provider, "1.265E+4");
                        yield return new TestCaseData("cst101", "12.65E+4", style, provider, "1.265E+5");
                        yield return new TestCaseData("cst102", "12.65E+8", style, provider, "1.265E+9");
                        yield return new TestCaseData("cst103", "12.65E+20", style, provider, "1.265E+21");

                        yield return new TestCaseData("cst110", "126.5", style, provider, "126.5");
                        yield return new TestCaseData("cst111", "126.5E-20", style, provider, "1.265E-18");
                        yield return new TestCaseData("cst112", "126.5E-8", style, provider, "1.265E-6");
                        yield return new TestCaseData("cst113", "126.5E-4", style, provider, "1.265E-2");
                        yield return new TestCaseData("cst114", "126.5E-3", style, provider, "1.265E-1");
                        yield return new TestCaseData("cst115", "126.5E-2", style, provider, "1.265");
                        yield return new TestCaseData("cst116", "126.5E-1", style, provider, "1.265E+1");
                        yield return new TestCaseData("cst117", "126.5E-0", style, provider, "1.265E+2");
                        yield return new TestCaseData("cst118", "126.5E+1", style, provider, "1.265E+3");
                        yield return new TestCaseData("cst119", "126.5E+2", style, provider, "1.265E+4");
                        yield return new TestCaseData("cst120", "126.5E+3", style, provider, "1.265E+5");
                        yield return new TestCaseData("cst121", "126.5E+4", style, provider, "1.265E+6");
                        yield return new TestCaseData("cst122", "126.5E+8", style, provider, "1.265E+10");
                        yield return new TestCaseData("cst123", "126.5E+20", style, provider, "1.265E+22");

                        yield return new TestCaseData("cst130", "1265", style, provider, "1265");
                        yield return new TestCaseData("cst131", "1265E-20", style, provider, "1.265E-17");
                        yield return new TestCaseData("cst132", "1265E-8", style, provider, "1.265E-5");
                        yield return new TestCaseData("cst133", "1265E-4", style, provider, "1.265E-1");
                        yield return new TestCaseData("cst134", "1265E-3", style, provider, "1.265");
                        yield return new TestCaseData("cst135", "1265E-2", style, provider, "1.265E+1");
                        yield return new TestCaseData("cst136", "1265E-1", style, provider, "1.265E+2");
                        yield return new TestCaseData("cst137", "1265E-0", style, provider, "1.265E+3");
                        yield return new TestCaseData("cst138", "1265E+1", style, provider, "1.265E+4");
                        yield return new TestCaseData("cst139", "1265E+2", style, provider, "1.265E+5");
                        yield return new TestCaseData("cst140", "1265E+3", style, provider, "1.265E+6");
                        yield return new TestCaseData("cst141", "1265E+4", style, provider, "1.265E+7");
                        yield return new TestCaseData("cst142", "1265E+8", style, provider, "1.265E+11");
                        yield return new TestCaseData("cst143", "1265E+20", style, provider, "1.265E+23");

                        yield return new TestCaseData("cst150", "0.1265", style, provider, "0.1265");
                        yield return new TestCaseData("cst151", "0.1265E-20", style, provider, "1.265E-21");
                        yield return new TestCaseData("cst152", "0.1265E-8", style, provider, "1.265E-9");
                        yield return new TestCaseData("cst153", "0.1265E-4", style, provider, "1.265E-5");
                        yield return new TestCaseData("cst154", "0.1265E-3", style, provider, "1.265E-4");
                        yield return new TestCaseData("cst155", "0.1265E-2", style, provider, "1.265E-3");
                        yield return new TestCaseData("cst156", "0.1265E-1", style, provider, "1.265E-2");
                        yield return new TestCaseData("cst157", "0.1265E-0", style, provider, "1.265E-1");
                        yield return new TestCaseData("cst158", "0.1265E+1", style, provider, "1.265");
                        yield return new TestCaseData("cst159", "0.1265E+2", style, provider, "1.265E+1");
                        yield return new TestCaseData("cst160", "0.1265E+3", style, provider, "1.265E+2");
                        yield return new TestCaseData("cst161", "0.1265E+4", style, provider, "1.265E+3");
                        yield return new TestCaseData("cst162", "0.1265E+8", style, provider, "1.265E+7");
                        yield return new TestCaseData("cst163", "0.1265E+20", style, provider, "1.265E+19");

                        yield return new TestCaseData("cst170", "0.09e999999999", style, provider, "9E+999999997");
                        yield return new TestCaseData("cst171", "0.9e999999999", style, provider, "9E+999999998");
                        yield return new TestCaseData("cst172", "9e999999999", style, provider, "9E+999999999");
                        yield return new TestCaseData("cst173", "9.9e999999999", style, provider, "9.9E+999999999");
                        yield return new TestCaseData("cst174", "9.99e999999999", style, provider, "9.99E+999999999");
                        yield return new TestCaseData("cst175", "9.99e-999999999", style, provider, "9.99E-999999999");
                        yield return new TestCaseData("cst176", "9.9e-999999999", style, provider, "9.9E-999999999");
                        yield return new TestCaseData("cst177", "9e-999999999", style, provider, "9E-999999999");
                        yield return new TestCaseData("cst179", "99e-999999999", style, provider, "9.9E-999999998");
                        yield return new TestCaseData("cst180", "999e-999999999", style, provider, "9.99E-999999997");
                    }
                }

                #endregion TestParse_CharSequence_NumberStyle_IFormatProvider_ForFloatStyle_Data

                #region TestParse_CharSequence_NumberStyle_IFormatProvider_ForFloatStyleException_Data

                public static IEnumerable<TestCaseData> TestParse_CharSequence_NumberStyle_IFormatProvider_ForFloatStyleException_Data
                {
                    get
                    {
                        var style = NumberStyle.Float;
                        var provider = CultureInfo.InvariantCulture;

                        // ICU4N specific tests

                        //                            id,  expected                       value, style, provider, expected message (or null to skip)
                        yield return new TestCaseData("cst300", typeof(ArgumentNullException), null, style, provider, null);
                        yield return new TestCaseData("cst301", typeof(FormatException), "", style, provider, null);
                        yield return new TestCaseData("cst302", typeof(FormatException), "<Garbage>", style, provider, null);

                        // ICU4J tests

                        // baddies --
                        string[] badstrings = new string[] { "1..2", ".", "..", "++1", "--1",
                            "-+1", "+-1", "12e", "12e++", "12f4", " +1", "+ 1", "12 ",
                            " + 1", " - 1 ", "x", "-1-", "12-", "3+", "", "1e-",
                            "7e1000000000", "", "e100", "\u0e5a", "\u0b65", "99e999999999",
                            "999e999999999", "0.9e-999999999", "0.09e-999999999",
                            "0.1e1000000000", "10e-1000000000", "0.9e9999999999",
                            "99e-9999999999", "111e9999999999",
                            "1111e-9999999999" + " " + "111e*123", "111e123-", "111e+12+",
                            "111e1-3-", "111e1*23", "111e1e+3", "1e1.0", "1e123e", "ten",
                            "ONE", "1e.1", "1e1.", "1ee", "e+1" }; // 200-203
                                                                   // 204-207
                                                                   // 208-211
                                                                   // 211-214
                                                                   // 215-219
                                                                   // 220-222
                                                                   // 223-224
                                                                   // 225-226
                                                                   // 227-228
                                                                   // 229-230
                                                                   // 231-232
                                                                   // 233-234
                                                                   // 235-237
                                                                   // 238-240
                                                                   // 241-244
                                                                   // 245-248

                        // watch out for commas on continuation lines

                        int len16 = badstrings.Length;
                        int i = 0;
                        for (; len16 > 0; len16--, i++)
                        {
                            yield return new TestCaseData("cst" + (200 + i).ToString(CultureInfo.InvariantCulture), typeof(FormatException), badstrings[i], style, provider, null);
                        }
                    }
                }

                #endregion TestParse_CharSequence_NumberStyle_IFormatProvider_ForFloatStyleException_Data
            }

            #endregion

            #region Parse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider

            public abstract class Parse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_TestCase : ParseTestCase
            {
                protected virtual bool IsNullableType => true;

                private protected abstract BigDecimal GetResult(string value, int startIndex, int length, NumberStyle style, IFormatProvider provider);

                [TestCaseSource(typeof(ParseTestCase), "TestParse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_ForFloatStyle_Data")]
                public void TestParse_CharSequence_IFormatProvider_ForFloatStyle(string id, string value, int startIndex, int length, NumberStyle style, IFormatProvider provider, string expectedString)
                {
                    Assume.That(IsNullableType || (!IsNullableType && value != null), "null is not supported by this character sequence type.");
                    Assume.That((style & ~(NumberStyle.Float | NumberStyle.AllowThousands)) == 0, "Custom NumberStyles are not supported on this overload.");

                    BigDecimal actual = GetResult(value, startIndex, length, style, provider);

                    string actualString = actual.ToString(CultureInfo.InvariantCulture);
                    string errorMsg = $"input string is:<{value}>. "
                        + $"The expected result should be:<{expectedString}>, "
                        + $"but was: <{actualString}>. ";

                    assertEquals(errorMsg, expectedString, actualString);
                }

                [TestCaseSource(typeof(ParseTestCase), "TestParse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_ForFloatStyleException_Data")]
                public void TestParse_CharSequence_IFormatProvider_ForFloatStyleException(string id, Type expectedExceptionType, string value, int startIndex, int length, NumberStyle style, IFormatProvider provider, string message)
                {
                    Assume.That(IsNullableType || (!IsNullableType && value != null), "null is not supported by this character sequence type.");
                    Assume.That((style & ~(NumberStyle.Float | NumberStyle.AllowThousands)) == 0, "Custom NumberStyles are not supported on this overload.");

                    Assert.Throws(expectedExceptionType, () => GetResult(value, startIndex, length, style, provider), message);
                }
            }

            public class Parse_String_Int32_Int32_NumberStyle_IFormatProvider : Parse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_TestCase
            {
                private protected override BigDecimal GetResult(string value, int startIndex, int length, NumberStyle style, IFormatProvider provider)
                {
                    return BigDecimal.Parse(value, startIndex, length, style, provider);
                }
            }

            public class Parse_CharArray_Int32_Int32_NumberStyle_IFormatProvider : Parse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_TestCase
            {
                private protected override BigDecimal GetResult(string value, int startIndex, int length, NumberStyle style, IFormatProvider provider)
                {
                    return BigDecimal.Parse(value?.ToCharArray(), startIndex, length, style, provider);
                }
            }

            #endregion Parse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider

            #region Parse_CharSequence_NumberStyle_IFormatProvider

            public abstract class Parse_CharSequence_NumberStyle_IFormatProvider_TestCase : ParseTestCase
            {
                protected virtual bool IsNullableType => true;

                private protected abstract BigDecimal GetResult(string value, NumberStyle style, IFormatProvider provider);

                [TestCaseSource(typeof(ParseTestCase), "TestParse_CharSequence_NumberStyle_IFormatProvider_ForFloatStyle_Data")]
                public void TestParse_CharSequence_IFormatProvider_ForFloatStyle(string id, string value, NumberStyle style, IFormatProvider provider, string expectedString)
                {
                    Assume.That(IsNullableType || (!IsNullableType && value != null), "null is not supported by this character sequence type.");
                    Assume.That((style & ~(NumberStyle.Float | NumberStyle.AllowThousands)) == 0, "Custom NumberStyles are not supported on this overload.");

                    BigDecimal actual = GetResult(value, style, provider);

                    string actualString = actual.ToString(CultureInfo.InvariantCulture);
                    string errorMsg = $"input string is:<{value}>. "
                        + $"The expected result should be:<{expectedString}>, "
                        + $"but was: <{actualString}>. ";

                    assertEquals(errorMsg, expectedString, actualString);
                }

                [TestCaseSource(typeof(ParseTestCase), "TestParse_CharSequence_NumberStyle_IFormatProvider_ForFloatStyleException_Data")]
                public void TestParse_CharSequence_IFormatProvider_ForFloatStyleException(string id, Type expectedExceptionType, string value, NumberStyle style, IFormatProvider provider, string message)
                {
                    Assume.That(IsNullableType || (!IsNullableType && value != null), "null is not supported by this character sequence type.");
                    Assume.That((style & ~(NumberStyle.Float | NumberStyle.AllowThousands)) == 0, "Custom NumberStyles are not supported on this overload.");

                    Assert.Throws(expectedExceptionType, () => GetResult(value, style, provider), message);
                }
            }

            public class Parse_String_NumberStyle_IFormatProvider : Parse_CharSequence_NumberStyle_IFormatProvider_TestCase
            {
                private protected override BigDecimal GetResult(string value, NumberStyle style, IFormatProvider provider)
                {
                    return BigDecimal.Parse(value, style, provider);
                }
            }

            public class Parse_CharArray_NumberStyle_IFormatProvider : Parse_CharSequence_NumberStyle_IFormatProvider_TestCase
            {
                private protected override BigDecimal GetResult(string value, NumberStyle style, IFormatProvider provider)
                {
                    return BigDecimal.Parse(value?.ToCharArray(), style, provider);
                }
            }

            #endregion Parse_CharSequence_NumberStyle_IFormatProvider
        }




        //[Test]
        //public void TestParse_CharSequence_Int32_Int32_NumberStyle_IFormatProvider_ForFloatStyle()
        //{
        //    // char[],int,int
        //    // We just test it's there, and that offsets work.
        //    // Functionality is tested by BigDecimal(String).
        //    char[] ca = ("123.45").ToCharArray();
        //    TestFmwk.assertTrue("cca101", ((new BigDecimal(ca, 0, 6)).ToString(CultureInfo.InvariantCulture)).Equals("123.45"));
        //    TestFmwk.assertTrue("cca102", ((new BigDecimal(ca, 1, 5)).ToString(CultureInfo.InvariantCulture)).Equals("23.45"));
        //    TestFmwk.assertTrue("cca103", ((new BigDecimal(ca, 2, 4)).ToString(CultureInfo.InvariantCulture)).Equals("3.45"));
        //    TestFmwk.assertTrue("cca104", ((new BigDecimal(ca, 3, 3)).ToString(CultureInfo.InvariantCulture)).Equals("0.45"));
        //    TestFmwk.assertTrue("cca105", ((new BigDecimal(ca, 4, 2)).ToString(CultureInfo.InvariantCulture)).Equals("45"));
        //    TestFmwk.assertTrue("cca106", ((new BigDecimal(ca, 5, 1)).ToString(CultureInfo.InvariantCulture)).Equals("5"));

        //    TestFmwk.assertTrue("cca110", ((new BigDecimal(ca, 0, 1)).ToString(CultureInfo.InvariantCulture)).Equals("1"));
        //    TestFmwk.assertTrue("cca111", ((new BigDecimal(ca, 1, 1)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
        //    TestFmwk.assertTrue("cca112", ((new BigDecimal(ca, 2, 1)).ToString(CultureInfo.InvariantCulture)).Equals("3"));
        //    TestFmwk.assertTrue("cca113", ((new BigDecimal(ca, 4, 1)).ToString(CultureInfo.InvariantCulture)).Equals("4"));

        //    TestFmwk.assertTrue("cca120", ((new BigDecimal(ca, 0, 2)).ToString(CultureInfo.InvariantCulture)).Equals("12"));
        //    TestFmwk.assertTrue("cca121", ((new BigDecimal(ca, 1, 2)).ToString(CultureInfo.InvariantCulture)).Equals("23"));
        //    TestFmwk.assertTrue("cca122", ((new BigDecimal(ca, 2, 2)).ToString(CultureInfo.InvariantCulture)).Equals("3"));
        //    TestFmwk.assertTrue("cca123", ((new BigDecimal(ca, 3, 2)).ToString(CultureInfo.InvariantCulture)).Equals("0.4"));

        //    TestFmwk.assertTrue("cca130", ((new BigDecimal(ca, 0, 3)).ToString(CultureInfo.InvariantCulture)).Equals("123"));
        //    TestFmwk.assertTrue("cca131", ((new BigDecimal(ca, 1, 3)).ToString(CultureInfo.InvariantCulture)).Equals("23"));
        //    TestFmwk.assertTrue("cca132", ((new BigDecimal(ca, 2, 3)).ToString(CultureInfo.InvariantCulture)).Equals("3.4"));

        //    TestFmwk.assertTrue("cca140", ((new BigDecimal(ca, 0, 4)).ToString(CultureInfo.InvariantCulture)).Equals("123"));
        //    TestFmwk.assertTrue("cca141", ((new BigDecimal(ca, 1, 4)).ToString(CultureInfo.InvariantCulture)).Equals("23.4"));

        //    TestFmwk.assertTrue("cca150", ((new BigDecimal(ca, 0, 5)).ToString(CultureInfo.InvariantCulture)).Equals("123.4"));

        //    // a couple of oddies
        //    ca = ("x23.4x").ToCharArray();
        //    TestFmwk.assertTrue("cca160", ((new BigDecimal(ca, 1, 4)).ToString(CultureInfo.InvariantCulture)).Equals("23.4"));
        //    TestFmwk.assertTrue("cca161", ((new BigDecimal(ca, 1, 1)).ToString(CultureInfo.InvariantCulture)).Equals("2"));
        //    TestFmwk.assertTrue("cca162", ((new BigDecimal(ca, 4, 1)).ToString(CultureInfo.InvariantCulture)).Equals("4"));

        //    ca = ("0123456789.9876543210").ToCharArray();
        //    TestFmwk.assertTrue("cca163", ((new BigDecimal(ca, 0, 21)).ToString(CultureInfo.InvariantCulture)).Equals("123456789.9876543210"));
        //    TestFmwk.assertTrue("cca164", ((new BigDecimal(ca, 1, 20)).ToString(CultureInfo.InvariantCulture)).Equals("123456789.9876543210"));
        //    TestFmwk.assertTrue("cca165", ((new BigDecimal(ca, 2, 19)).ToString(CultureInfo.InvariantCulture)).Equals("23456789.9876543210"));
        //    TestFmwk.assertTrue("cca166", ((new BigDecimal(ca, 2, 18)).ToString(CultureInfo.InvariantCulture)).Equals("23456789.987654321"));
        //    TestFmwk.assertTrue("cca167", ((new BigDecimal(ca, 2, 17)).ToString(CultureInfo.InvariantCulture)).Equals("23456789.98765432"));
        //    TestFmwk.assertTrue("cca168", ((new BigDecimal(ca, 2, 16)).ToString(CultureInfo.InvariantCulture)).Equals("23456789.9876543"));

        //    try
        //    {
        //        new BigDecimal((char[])null, 0, 1);
        //        flag = false;
        //    }
        //    catch (ArgumentNullException e8)
        //    {
        //        flag = true;
        //    }/* checknull */
        //    TestFmwk.assertTrue("cca200", flag);

        //    try
        //    {
        //        new BigDecimal("123".ToCharArray(), 0, 0);
        //        flag = false;
        //    }
        //    catch (FormatException e9) // ICU4N TODO: Handle parse with 0 length
        //    {
        //        flag = true;
        //    }/* checklen */
        //    TestFmwk.assertTrue("cca201", flag);

        //    try
        //    {
        //        new BigDecimal("123".ToCharArray(), 2, 4);
        //        flag = false;
        //    }
        //    catch (ArgumentOutOfRangeException e10)
        //    { // anything OK
        //        flag = true;
        //    }/* checkbound */
        //    TestFmwk.assertTrue("cca202", flag);
        //    try
        //    {
        //        new BigDecimal("123".ToCharArray(), -1, 2);
        //        flag = false;
        //    }
        //    catch (ArgumentOutOfRangeException e11)
        //    { // anything OK
        //        flag = true;
        //    }/* checkbound2 */
        //    TestFmwk.assertTrue("cca203", flag);
        //    try
        //    {
        //        new BigDecimal("123".ToCharArray(), 1, -2);
        //        flag = false;
        //    }
        //    catch (ArgumentOutOfRangeException e12)
        //    { // anything OK
        //        flag = true;
        //    }/* checkbound3 */
        //    TestFmwk.assertTrue("cca204", flag);
        //}
    }
}
