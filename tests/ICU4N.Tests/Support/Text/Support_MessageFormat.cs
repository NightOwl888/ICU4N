//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ICU4N.Support.Text
//{
//    public class Support_MessageFormat : Support_Format
//    {
//        public Support_MessageFormat(String p1)
//            : base(p1)
//        {
//        }

//    public override void RunTest()
//        {
//            t_formatToCharacterIterator();
//            t_format_with_FieldPosition();
//        }

//        //public static void main(String[] args)
//        //{
//        //    new Support_MessageFormat("").runTest();
//        //}

//        public void t_format_with_FieldPosition()
//        {

//            String pattern = "On {4,date} at {3,time}, he ate {2,number, integer} hamburger{2,choice,1#|1<s} and drank {1, number} litres of coke. That was {0,choice,1#just enough|1<more than enough} food!";
//            MessageFormat format = new MessageFormat(pattern, new CultureInfo("en-US") /*Locale.US*/);

//            //Date date = new GregorianCalendar(2005, 1, 28, 14, 20, 16).getTime();
//            DateTime date = new DateTime(2005, 1, 28, 14, 20, 16, new GregorianCalendar());
//            Integer hamburgers = new Integer(8);
//            Object[] objects = new Object[] { hamburgers, J2N.Numerics.Double.GetInstance(3.5),
//                hamburgers, date, date };

//            base.text = "On Feb 28, 2005 at 2:20:16 PM, he ate 8 hamburgers and drank 3.5 litres of coke. That was more than enough food!";

//            // test with MessageFormat.Field.ARGUMENT
//            t_FormatWithField(1, format, objects, null, Field.ARGUMENT, 3, 15);

//            // test other format fields that are included in the formatted text
//            t_FormatWithField(2, format, objects, null, DateFormat.Field.AM_PM, 0,
//                    0);
//            t_FormatWithField(3, format, objects, null,
//                    NumberFormat.Field.FRACTION, 0, 0);

//            // test fields that are not included in the formatted text
//            t_FormatWithField(4, format, objects, null, DateFormat.Field.ERA, 0, 0);
//            t_FormatWithField(5, format, objects, null,
//                    NumberFormat.Field.EXPONENT_SIGN, 0, 0);
//        }

//        public void t_formatToCharacterIterator()
//        {

//            String pattern = "On {4,date} at {3,time}, he ate {2,number, integer} hamburger{2,choice,1#|1<s} and drank {1, number} litres of coke. That was {0,choice,1#just enough|1<more than enough} food!";
//            MessageFormat format = new MessageFormat(pattern, new CultureInfo("en-US") /*Locale.US*/);

//            //Date date = new GregorianCalendar(2005, 1, 28, 14, 20, 16).getTime();
//            DateTime date = new DateTime(2005, 1, 28, 14, 20, 16, new GregorianCalendar());
//            Integer hamburgers = new Integer(8);
//            Object[] objects = new Object[] { hamburgers, J2N.Numerics.Double.GetInstance(3.5),
//                hamburgers, date, date };

//            t_Format(1, objects, format, getMessageVector1());
//        }

//        private IList<FieldContainer> getMessageVector1()
//        {
//            IList<FieldContainer> v = new List<FieldContainer>();
//            v.Add(new FieldContainer(3, 6, Field.ARGUMENT, 4));
//            v.Add(new FieldContainer(3, 6, DateFormat.Field.MONTH));
//            v.Add(new FieldContainer(6, 7, Field.ARGUMENT, 4));
//            v.Add(new FieldContainer(7, 9, Field.ARGUMENT, 4));
//            v.Add(new FieldContainer(7, 9, DateFormat.Field.DAY_OF_MONTH));
//            v.Add(new FieldContainer(9, 11, Field.ARGUMENT, 4));
//            v.Add(new FieldContainer(11, 15, Field.ARGUMENT, 4));
//            v.Add(new FieldContainer(11, 15, DateFormat.Field.YEAR));
//            v.Add(new FieldContainer(19, 20, Field.ARGUMENT, 3));
//            v.Add(new FieldContainer(19, 20, DateFormat.Field.HOUR1));
//            v.Add(new FieldContainer(20, 21, Field.ARGUMENT, 3));
//            v.Add(new FieldContainer(21, 23, Field.ARGUMENT, 3));
//            v.Add(new FieldContainer(21, 23, DateFormat.Field.MINUTE));
//            v.Add(new FieldContainer(23, 24, Field.ARGUMENT, 3));
//            v.Add(new FieldContainer(24, 26, Field.ARGUMENT, 3));
//            v.Add(new FieldContainer(24, 26, DateFormat.Field.SECOND));
//            v.Add(new FieldContainer(26, 27, Field.ARGUMENT, 3));
//            v.Add(new FieldContainer(27, 29, Field.ARGUMENT, 3));
//            v.Add(new FieldContainer(27, 29, DateFormat.Field.AM_PM));
//            v.Add(new FieldContainer(38, 39, Field.ARGUMENT, 2));
//            v.Add(new FieldContainer(38, 39, NumberFormat.Field.INTEGER));
//            v.Add(new FieldContainer(49, 50, Field.ARGUMENT, 2));
//            v.Add(new FieldContainer(61, 62, Field.ARGUMENT, 1));
//            v.Add(new FieldContainer(61, 62, NumberFormat.Field.INTEGER));
//            v.Add(new FieldContainer(62, 63, Field.ARGUMENT, 1));
//            v.Add(new FieldContainer(62, 63, NumberFormat.Field.DECIMAL_SEPARATOR));
//            v.Add(new FieldContainer(63, 64, Field.ARGUMENT, 1));
//            v.Add(new FieldContainer(63, 64, NumberFormat.Field.FRACTION));
//            v.Add(new FieldContainer(90, 106, Field.ARGUMENT, 0));
//            return v;
//        }
//    }
//}
