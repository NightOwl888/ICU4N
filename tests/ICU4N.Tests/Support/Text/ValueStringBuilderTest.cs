using J2N.Globalization;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Text;

namespace ICU4N.Text
{
    public class ValueStringBuilderTest
    {
        // String larger than 16384 bytes
        private const string LargeUnicodeString = "⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛⽄\u2fda⼃⾮⾵\u2fde⼒⼱⾠⽚⽕⽆⾭⾕⼇⼂⽖⽋⽲\u2fd8⿄⽁⼄⼽⾸⼉⽤⾲⼡\u2fdb⼱⼈⽥⾰⽬⼤⿃⽞⽪⽗⼟⾃⼪⾔⾏⼼\u2fdb⼩⼘⼷⾪⼲⾛⾫⾊⼃⿕⾥⿕⾫⽹⽀⼐⾤⼩⽍⿀\u2fdd⼩⿂⼞\u2fd7⿁⼚⾹⽁⼖⽐⾎⽻⼍⼻⾚⿊⼰\u2fdf⽌⾚⼥⽨⼯⼞⽩⾞⾽⾿⽳⽥⽫⽁⽛";


        [Test]
        public void Ctor_Default_CanAppend()
        {
            var vsb = default(ValueStringBuilder);
            Assert.AreEqual(0, vsb.Length);

            vsb.Append('a');
            Assert.AreEqual(1, vsb.Length);
            Assert.AreEqual("a", vsb.ToString());
        }

        [Test]
        public void Ctor_Span_CanAppend()
        {
            var vsb = new ValueStringBuilder(new char[1]);
            Assert.AreEqual(0, vsb.Length);

            vsb.Append('a');
            Assert.AreEqual(1, vsb.Length);
            Assert.AreEqual("a", vsb.ToString());
        }

        [Test]
        public void Ctor_InitialCapacity_CanAppend()
        {
            var vsb = new ValueStringBuilder(1);
            Assert.AreEqual(0, vsb.Length);

            vsb.Append('a');
            Assert.AreEqual(1, vsb.Length);
            Assert.AreEqual("a", vsb.ToString());
        }

        [Test]
        public void Append_Char_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (int i = 1; i <= 100; i++)
            {
                sb.Append((char)i);
                vsb.Append((char)i);
            }

            Assert.AreEqual(sb.Length, vsb.Length);
            Assert.AreEqual(sb.ToString(), vsb.ToString());
        }

        [Test]
        public void Append_String_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (int i = 1; i <= 100; i++)
            {
                string s = i.ToString();
                sb.Append(s);
                vsb.Append(s);
            }

            Assert.AreEqual(sb.Length, vsb.Length);
            Assert.AreEqual(sb.ToString(), vsb.ToString());
        }

        [Test]
        [TestCase(0, 4 * 1024 * 1024)]
        [TestCase(1025, 4 * 1024 * 1024)]
        [TestCase(3 * 1024 * 1024, 6 * 1024 * 1024)]
        public void Append_String_Large_MatchesStringBuilder(int initialLength, int stringLength)
        {
            var sb = new StringBuilder(initialLength);
            var vsb = new ValueStringBuilder(new char[initialLength]);

            string s = new string('a', stringLength);
            sb.Append(s);
            vsb.Append(s);

            Assert.AreEqual(sb.Length, vsb.Length);
            Assert.AreEqual(sb.ToString(), vsb.ToString());
        }

        [Test]
        public void Append_CharInt_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (int i = 1; i <= 100; i++)
            {
                sb.Append((char)i, i);
                vsb.Append((char)i, i);
            }

            Assert.AreEqual(sb.Length, vsb.Length);
            Assert.AreEqual(sb.ToString(), vsb.ToString());
        }

#if FEATURE_STRINGBUILDER_APPEND_CHARPTR
        [Test]
        public unsafe void Append_PtrInt_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (int i = 1; i <= 100; i++)
            {
                string s = i.ToString();
                fixed (char* p = s)
                {
                    sb.Append(p, s.Length);
                    vsb.Append(p, s.Length);
                }
            }

            Assert.AreEqual(sb.Length, vsb.Length);
            Assert.AreEqual(sb.ToString(), vsb.ToString());
        }
#endif

        [Test]
        public void AppendSpan_DataAppendedCorrectly()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();

            for (int i = 1; i <= 1000; i++)
            {
                string s = i.ToString();

                sb.Append(s);

                Span<char> span = vsb.AppendSpan(s.Length);
                Assert.AreEqual(sb.Length, vsb.Length);

                s.AsSpan().CopyTo(span);
            }

            Assert.AreEqual(sb.Length, vsb.Length);
            Assert.AreEqual(sb.ToString(), vsb.ToString());
        }

        [Test]
        public void Insert_IntCharInt_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            var rand = new Random(42);

            for (int i = 1; i <= 100; i++)
            {
                int index = rand.Next(sb.Length);
                sb.Insert(index, new string((char)i, 1), i);
                vsb.Insert(index, (char)i, i);
            }

            Assert.AreEqual(sb.Length, vsb.Length);
            Assert.AreEqual(sb.ToString(), vsb.ToString());
        }

        [Test]
        public void AsSpan_ReturnsCorrectValue_DoesntClearBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();

            for (int i = 1; i <= 100; i++)
            {
                string s = i.ToString();
                sb.Append(s);
                vsb.Append(s);
            }

            var resultString = vsb.AsSpan().ToString();
            Assert.AreEqual(sb.ToString(), resultString);

            Assert.AreNotEqual(0, sb.Length);
            Assert.AreEqual(sb.Length, vsb.Length);
        }

        [Test]
        public void ToString_ClearsBuilder_ThenReusable()
        {
            const string Text1 = "test";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);
            Assert.AreEqual(Text1.Length, vsb.Length);

            string s = vsb.ToString();
            Assert.AreEqual(Text1, s);

            Assert.AreEqual(0, vsb.Length);
            Assert.AreEqual(string.Empty, vsb.ToString());
            Assert.True(vsb.TryCopyTo(Span<char>.Empty, out _));

            const string Text2 = "another test";
            vsb.Append(Text2);
            Assert.AreEqual(Text2.Length, vsb.Length);
            Assert.AreEqual(Text2, vsb.ToString());
        }

        [Test]
        public void TryCopyTo_FailsWhenDestinationIsTooSmall_SucceedsWhenItsLargeEnough()
        {
            var vsb = new ValueStringBuilder();

            const string Text = "expected text";
            vsb.Append(Text);
            Assert.AreEqual(Text.Length, vsb.Length);

            Span<char> dst = new char[Text.Length - 1];
            Assert.False(vsb.TryCopyTo(dst, out int charsWritten));
            Assert.AreEqual(0, charsWritten);
            Assert.AreEqual(0, vsb.Length);
        }

        [Test]
        public void TryCopyTo_ClearsBuilder_ThenReusable()
        {
            const string Text1 = "test";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);
            Assert.AreEqual(Text1.Length, vsb.Length);

            Span<char> dst = new char[Text1.Length];
            Assert.True(vsb.TryCopyTo(dst, out int charsWritten));
            Assert.AreEqual(Text1.Length, charsWritten);
            Assert.AreEqual(Text1, dst.ToString());

            Assert.AreEqual(0, vsb.Length);
            Assert.AreEqual(string.Empty, vsb.ToString());
            Assert.True(vsb.TryCopyTo(Span<char>.Empty, out _));

            const string Text2 = "another test";
            vsb.Append(Text2);
            Assert.AreEqual(Text2.Length, vsb.Length);
            Assert.AreEqual(Text2, vsb.ToString());
        }

        [Test]
        public void Dispose_ClearsBuilder_ThenReusable()
        {
            const string Text1 = "test";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);
            Assert.AreEqual(Text1.Length, vsb.Length);

            vsb.Dispose();

            Assert.AreEqual(0, vsb.Length);
            Assert.AreEqual(string.Empty, vsb.ToString());
            Assert.True(vsb.TryCopyTo(Span<char>.Empty, out _));

            const string Text2 = "another test";
            vsb.Append(Text2);
            Assert.AreEqual(Text2.Length, vsb.Length);
            Assert.AreEqual(Text2, vsb.ToString());
        }

        [Test]
        public /*unsafe*/ void Indexer()
        {
            const string Text1 = "foobar";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);

            Assert.AreEqual('b', vsb[3]);
            vsb[3] = 'c';
            Assert.AreEqual('c', vsb[3]);
        }

        [Test]
        public void EnsureCapacity_IfRequestedCapacityWins()
        {
            // Note: constants used here may be dependent on minimal buffer size
            // the ArrayPool is able to return.
            var builder = new ValueStringBuilder(stackalloc char[32]);

            builder.EnsureCapacity(65);

            Assert.AreEqual(128, builder.Capacity);
        }

        [Test]
        public void EnsureCapacity_IfBufferTimesTwoWins()
        {
            var builder = new ValueStringBuilder(stackalloc char[32]);

            builder.EnsureCapacity(33);

            Assert.AreEqual(64, builder.Capacity);
        }

        [Test]
        public void EnsureCapacity_NoAllocIfNotNeeded()
        {
            // Note: constants used here may be dependent on minimal buffer size
            // the ArrayPool is able to return.
            var builder = new ValueStringBuilder(stackalloc char[64]);

            builder.EnsureCapacity(16);

            Assert.AreEqual(64, builder.Capacity);
        }


        [Test]
        [TestCase("", 0, 0, "")]
        [TestCase("Hello", 0, 5, "")]
        [TestCase("Hello", 1, 3, "Ho")]
        [TestCase("Hello", 1, 4, "H")]
        [TestCase("Hello", 1, 0, "Hello")]
        [TestCase("Hello", 5, 0, "Hello")]
        public static void Remove(string value, int startIndex, int length, string expected)
        {
            var builder = new ValueStringBuilder(stackalloc char[64]);
            builder.Append(value);
            builder.Remove(startIndex, length);
            Assert.AreEqual(expected, builder.ToString());
        }


        [Test]
        public virtual void TestAppendCodePointBmp()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = 97; // a

            sb.AppendCodePoint(codePoint);

            Assert.AreEqual("foo bara", sb.ToString());
        }

        [Test]
        public virtual void TestAppendCodePointUnicode()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = 3594; // ช

            sb.AppendCodePoint(codePoint);

            Assert.AreEqual("foo barช", sb.ToString());
        }

        [Test]
        public virtual void TestAppendCodePointUTF16Surrogates()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = 176129; // '\uD86C', '\uDC01' (𫀁)

            sb.AppendCodePoint(codePoint);

            Assert.AreEqual("foo bar𫀁", sb.ToString());
        }

        [Test]
        public virtual void TestAppendCodePointTooHigh()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = J2N.Character.MaxCodePoint + 1;

            try
            {
                sb.AppendCodePoint(codePoint);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
            }
        }

        [Test]
        public virtual void TestAppendCodePointTooLow()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = J2N.Character.MinCodePoint - 1;

            try
            {
                sb.AppendCodePoint(codePoint);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
            }
        }


        [Test]
        public virtual void TestInsertCodePointBmp()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = 97; // a

            sb.InsertCodePoint(0, codePoint);

            Assert.AreEqual("afoo bar", sb.ToString());
        }

        [Test]
        public virtual void TestInsertCodePointUnicode()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = 3594; // ช

            sb.InsertCodePoint(1, codePoint);

            Assert.AreEqual("fชoo bar", sb.ToString());
        }

        [Test]
        public virtual void TestInsertCodePointUTF16Surrogates()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = 176129; // '\uD86C', '\uDC01' (𫀁)

            sb.InsertCodePoint(2, codePoint);

            Assert.AreEqual("fo𫀁o bar", sb.ToString());
        }

        [Test]
        public virtual void TestInsertCodePointTooHigh()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = J2N.Character.MaxCodePoint + 1;

            try
            {
                sb.InsertCodePoint(0, codePoint);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
            }
        }

        [Test]
        public virtual void TestInsertCodePointTooLow()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = J2N.Character.MinCodePoint - 1;

            try
            {
                sb.InsertCodePoint(0, codePoint);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException)
            {
            }
        }

        [Test]
        public virtual void TestInsertCodePointIndexTooHigh()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = J2N.Character.MaxCodePoint;

            try
            {
                sb.InsertCodePoint(sb.Length + 1, codePoint);
                Assert.Fail("Expected ArgumentOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [Test]
        public virtual void TestInsertCodePointIndexTooLow()
        {
            var sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("foo bar");

            int codePoint = J2N.Character.MinCodePoint;

            try
            {
                sb.InsertCodePoint(-1, codePoint);
                Assert.Fail("Expected ArgumentOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        /**
         * @tests java.lang.StringBuilder.append(CharSequence)
         */
        [Test]
        public void Test_Append_String()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("ab");
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("cd");
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append((string)null);
            // assertEquals("null", sb.ToString());
            Assert.AreEqual("", sb.AsSpan().ToString()); // J2N: Changed the behavior to be a no-op rather than appending the string "null"
            sb.Dispose();
        }

        /**
         * @tests java.lang.StringBuilder.append(CharSequence, int, int)
         */
        [Test]
        public void Test_Append_String_Int32_Int32()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("ab", 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("cd", 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("abcd", 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("abcd", 2, 4 - 2); // J2N: corrected 3rd parameter
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            //try
            //{
            //    assertSame(sb, sb.Append((ICharSequence)null, 0, 2)); // J2N: Changed the behavior to throw an exception (to match .NET Core 3.0's Append(StringBuilder,int,int) overload) rather than appending the string "null"
            //    fail("no NPE");
            //}
            //catch (ArgumentNullException e)
            //{
            //    // Expected
            //}
            //assertEquals("nu", sb.ToString());
            Assert.AreEqual("", sb.AsSpan().ToString());
            sb.Dispose();
        }

        /**
         * @tests java.lang.StringBuilder.append(CharSequence)
         */
        [Test]
        public void Test_Append_CharArray()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("ab".ToCharArray());
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("cd".ToCharArray());
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append((char[])null);
            // assertEquals("null", sb.ToString());
            Assert.AreEqual("", sb.AsSpan().ToString()); // J2N: Changed the behavior to be a no-op rather than appending the string "null"
            sb.Dispose();
        }

        /**
         * @tests java.lang.StringBuilder.append(CharSequence, int, int)
         */
        [Test]
        public void Test_Append_CharArray_Int32_Int32()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("ab".ToCharArray(), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("cd".ToCharArray(), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("abcd".ToCharArray(), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("abcd".ToCharArray(), 2, 4 - 2); // J2N: corrected 3rd parameter
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            //try
            //{
            //    assertSame(sb, sb.Append((ICharSequence)null, 0, 2)); // J2N: Changed the behavior to throw an exception (to match .NET Core 3.0's Append(StringBuilder,int,int) overload) rather than appending the string "null"
            //    fail("no NPE");
            //}
            //catch (ArgumentNullException e)
            //{
            //    // Expected
            //}
            //assertEquals("nu", sb.ToString());
            Assert.AreEqual("", sb.AsSpan().ToString());
            sb.Dispose();
        }


        /**
         * @tests java.lang.StringBuilder.append(CharSequence)
         */
        [Test]
        public void Test_Append_ICharSequence()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("ab".AsCharSequence());
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("cd".AsCharSequence());
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append((ICharSequence)null);
            // assertEquals("null", sb.ToString());
            Assert.AreEqual("", sb.AsSpan().ToString()); // J2N: Changed the behavior to be a no-op rather than appending the string "null"
            sb.Dispose();
        }

        /**
         * @tests java.lang.StringBuilder.append(CharSequence, int, int)
         */
        [Test]
        public void Test_Append_ICharSequence_Int32_Int32()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append("ab".AsCharSequence(), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("cd".AsCharSequence(), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("abcd".AsCharSequence(), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append("abcd".AsCharSequence(), 2, 4 - 2); // J2N: corrected 3rd parameter
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            //try
            //{
            //    assertSame(sb, sb.Append((ICharSequence)null, 0, 2)); // J2N: Changed the behavior to throw an exception (to match .NET Core 3.0's Append(StringBuilder,int,int) overload) rather than appending the string "null"
            //    fail("no NPE");
            //}
            //catch (ArgumentNullException e)
            //{
            //    // Expected
            //}
            //assertEquals("nu", sb.ToString());
            Assert.AreEqual("", sb.AsSpan().ToString());
            sb.Dispose();
        }

        private sealed class MyCharSequence : ICharSequence
        {
            private readonly string value;
            public MyCharSequence(string value)
            {
                this.value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public char this[int index] => value[index];

            public bool HasValue => true;

            public int Length => value.Length;

            public ICharSequence Subsequence(int startIndex, int length)
                => value.Substring(startIndex, length).AsCharSequence();
        }

        /**
         * @tests java.lang.StringBuilder.append(CharSequence)
         */
        [Test]
        public void Test_Append_ICharSequence_Custom()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append(new MyCharSequence("ab"));
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append(new MyCharSequence("cd"));
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append((MyCharSequence)null);
            // assertEquals("null", sb.ToString());
            Assert.AreEqual("", sb.AsSpan().ToString()); // J2N: Changed the behavior to be a no-op rather than appending the string "null"
            sb.Dispose();
        }

        /**
         * @tests java.lang.StringBuilder.append(CharSequence, int, int)
         */
        [Test]
        public void Test_Append_ICharSequence_Int32_Int32_Custom()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append(new MyCharSequence("ab"), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append(new MyCharSequence("cd"), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append(new MyCharSequence("abcd"), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append(new MyCharSequence("abcd"), 2, 4 - 2); // J2N: corrected 3rd parameter
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            //try
            //{
            //    assertSame(sb, sb.Append((ICharSequence)null, 0, 2)); // J2N: Changed the behavior to throw an exception (to match .NET Core 3.0's Append(StringBuilder,int,int) overload) rather than appending the string "null"
            //    fail("no NPE");
            //}
            //catch (ArgumentNullException e)
            //{
            //    // Expected
            //}
            //assertEquals("nu", sb.ToString());
            Assert.AreEqual("", sb.AsSpan().ToString());
            sb.Dispose();
        }

        /**
         * @tests java.lang.StringBuilder.append(CharSequence)
         */
        [Test]
        public void Test_Append_StringBuilder()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append(new StringBuilder("ab"));
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append(new StringBuilder("cd"));
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append((StringBuilder)null);
            // assertEquals("null", sb.ToString());
            Assert.AreEqual("", sb.AsSpan().ToString()); // J2N: Changed the behavior to be a no-op rather than appending the string "null"
            sb.Dispose();
        }

        /**
         * @tests java.lang.StringBuilder.append(CharSequence, int, int)
         */
        [Test]
        public void Test_Append_StringBuilder_Int32_Int32()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[16]);
            sb.Append(new StringBuilder("ab"), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append(new StringBuilder("cd"), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append(new StringBuilder("abcd"), 0, 2 - 0); // J2N: corrected 3rd parameter
            Assert.AreEqual("ab", sb.AsSpan().ToString());
            sb.Length = (0);
            sb.Append(new StringBuilder("abcd"), 2, 4 - 2); // J2N: corrected 3rd parameter
            Assert.AreEqual("cd", sb.AsSpan().ToString());
            sb.Length = (0);
            //try
            //{
            //    assertSame(sb, sb.Append((StringBuilder)null, 0, 2)); // J2N: Changed the behavior to throw an exception (to match .NET Core 3.0) rather than appending the string "null"
            //    fail("no NPE");
            //}
            //catch (ArgumentNullException e)
            //{
            //    // Expected
            //}
            //assertEquals("nu", sb.ToString());
            Assert.AreEqual("", sb.AsSpan().ToString());
            sb.Dispose();
        }


        private void reverseTest(String org, String rev, String back)
        {
            // create non-shared StringBuilder
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[32]);
            sb.Append(org);
            sb.Reverse();
            String reversed = sb.AsSpan().ToString();
            Assert.AreEqual(rev, reversed);
            // create non-shared StringBuilder
            //sb = new ValueStringBuilder(reversed);
            sb.Length = 0;
            sb.Append(reversed);
            sb.Reverse();
            reversed = sb.AsSpan().ToString();
            Assert.AreEqual(back, reversed);

            // test algorithm when StringBuilder is shared
            //sb = new StringBuilder(org);
            sb.Length = 0;
            sb.Append(org);
            String copy = sb.AsSpan().ToString();
            Assert.AreEqual(org, copy);
            sb.Reverse();
            reversed = sb.AsSpan().ToString();
            Assert.AreEqual(rev, reversed);
            //sb = new StringBuilder(reversed);
            sb.Length = 0;
            sb.Append(reversed);
            copy = sb.AsSpan().ToString();
            Assert.AreEqual(rev, copy);
            sb.Reverse();
            reversed = sb.AsSpan().ToString();
            Assert.AreEqual(back, reversed);
            sb.Dispose();
        }

        /**
         * @tests java.lang.StringBuilder.delete(int, int)
         */
        [Test]
        public void Test_DeleteII()
        {
            string fixture = "0123456789";
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[32]);
            sb.Append(fixture);
            sb.Delete(0, 0 - 0); // J2N: Corrected 2nd parameter
            Assert.AreEqual(fixture, sb.AsSpan().ToString());
            sb.Delete(5, 5 - 5); // J2N: Corrected 2nd parameter
            Assert.AreEqual(fixture, sb.AsSpan().ToString());
            sb.Delete(0, 1 - 0); // J2N: Corrected 2nd parameter
            Assert.AreEqual("123456789", sb.AsSpan().ToString());
            Assert.AreEqual(9, sb.Length);
            sb.Delete(0, sb.Length - 0); // J2N: Corrected 2nd parameter
            Assert.AreEqual("", sb.AsSpan().ToString());
            Assert.AreEqual(0, sb.Length);

            //sb = new StringBuilder(fixture);
            sb.Length = 0;
            sb.Append(fixture);
            sb.Delete(0, 11 - 0); // J2N: Corrected 2nd parameter
            Assert.AreEqual("", sb.AsSpan().ToString());
            Assert.AreEqual(0, sb.Length);

            //try
            //{
            //    new StringBuilder(fixture).Delete(-1, 2 - -1); // J2N: Corrected 2nd parameter
            //    fail("no SIOOBE, negative start");
            //}
            //catch (ArgumentOutOfRangeException e)
            //{
            //    // Expected
            //}

            //try
            //{
            //    new StringBuilder(fixture).Delete(11, 12 - 11); // J2N: Corrected 2nd parameter
            //    fail("no SIOOBE, start too far");
            //}
            //catch (ArgumentOutOfRangeException e)
            //{
            //    // Expected
            //}

            //try
            //{
            //    new StringBuilder(fixture).Delete(13, 12 - 13); // J2N: Corrected 2nd parameter
            //    fail("no SIOOBE, start larger than end");
            //}
            //catch (ArgumentOutOfRangeException e)
            //{
            //    // Expected
            //}

            // HARMONY 6212
            //sb = new StringBuilder();
            sb.Length = 0;
            sb.Append("abcde");
            String str = sb.AsSpan().ToString();
            sb.Delete(0, sb.Length - 0); // J2N: Corrected 2nd parameter
            sb.Append("YY");
            Assert.AreEqual("abcde", str);
            Assert.AreEqual("YY", sb.AsSpan().ToString());
            sb.Dispose();
        }

        /**
         * @tests java.lang.StringBuilder.reverse()
         */
        [Test]
        public void Test_Reverse()
        {
            string fixture = "0123456789";
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[32]);
            sb.Append(fixture);
            sb.Reverse();
            Assert.AreEqual("9876543210", sb.AsSpan().ToString());

            sb.Length = 0;
            sb.Append("012345678");
            sb.Reverse();
            Assert.AreEqual("876543210", sb.AsSpan().ToString());

            sb.Length=(1);
            sb.Reverse();
            Assert.AreEqual("8", sb.AsSpan().ToString());

            sb.Length=(0);
            sb.Reverse();
            Assert.AreEqual("", sb.AsSpan().ToString());
            sb.Dispose();

            String str;
            str = "a";
            reverseTest(str, str, str);

            str = "ab";
            reverseTest(str, "ba", str);

            str = "abcdef";
            reverseTest(str, "fedcba", str);

            str = "abcdefg";
            reverseTest(str, "gfedcba", str);

            str = "\ud800\udc00";
            reverseTest(str, str, str);

            str = "\udc00\ud800";
            reverseTest(str, "\ud800\udc00", "\ud800\udc00");

            str = "a\ud800\udc00";
            reverseTest(str, "\ud800\udc00a", str);

            str = "ab\ud800\udc00";
            reverseTest(str, "\ud800\udc00ba", str);

            str = "abc\ud800\udc00";
            reverseTest(str, "\ud800\udc00cba", str);

            str = "\ud800\udc00\udc01\ud801\ud802\udc02";
            reverseTest(str, "\ud802\udc02\ud801\udc01\ud800\udc00",
                    "\ud800\udc00\ud801\udc01\ud802\udc02");

            str = "\ud800\udc00\ud801\udc01\ud802\udc02";
            reverseTest(str, "\ud802\udc02\ud801\udc01\ud800\udc00", str);

            str = "\ud800\udc00\udc01\ud801a";
            reverseTest(str, "a\ud801\udc01\ud800\udc00",
                    "\ud800\udc00\ud801\udc01a");

            str = "a\ud800\udc00\ud801\udc01";
            reverseTest(str, "\ud801\udc01\ud800\udc00a", str);

            str = "\ud800\udc00\udc01\ud801ab";
            reverseTest(str, "ba\ud801\udc01\ud800\udc00",
                    "\ud800\udc00\ud801\udc01ab");

            str = "ab\ud800\udc00\ud801\udc01";
            reverseTest(str, "\ud801\udc01\ud800\udc00ba", str);

            str = "\ud800\udc00\ud801\udc01";
            reverseTest(str, "\ud801\udc01\ud800\udc00", str);

            str = "a\ud800\udc00z\ud801\udc01";
            reverseTest(str, "\ud801\udc01z\ud800\udc00a", str);

            str = "a\ud800\udc00bz\ud801\udc01";
            reverseTest(str, "\ud801\udc01zb\ud800\udc00a", str);

            str = "abc\ud802\udc02\ud801\udc01\ud800\udc00";
            reverseTest(str, "\ud800\udc00\ud801\udc01\ud802\udc02cba", str);

            str = "abcd\ud802\udc02\ud801\udc01\ud800\udc00";
            reverseTest(str, "\ud800\udc00\ud801\udc01\ud802\udc02dcba", str);
        }

        /**
         * @tests java.lang.StringBuilder.replace(int, int, String)'
         */
        [Test]
        public void Test_Replace_String()
        {
            string fixture = "0000";
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[64]);
            sb.Append(fixture);
            //Assert.AreSame(sb, sb.Replace(1, 3 - 1, "11")); // J2N; Corrected 2nd parameter
            sb.Replace(1, 3 - 1, "11"); // J2N; Corrected 2nd parameter
            Assert.AreEqual("0110", sb.AsSpan().ToString());
            Assert.AreEqual(4, sb.Length);

            //sb = new StringBuilder(fixture);
            sb.Length = 0;
            sb.Append(fixture);
            //Assert.AreSame(sb, );
            sb.Replace(1, 2 - 1, "11"); // J2N; Corrected 2nd parameter
            Assert.AreEqual("01100", sb.AsSpan().ToString());
            Assert.AreEqual(5, sb.Length);

            //sb = new StringBuilder(fixture);
            sb.Length = 0;
            sb.Append(fixture);
            //Assert.AreSame(sb, );
            sb.Replace(4, 5 - 4, "11"); // J2N; Corrected 2nd parameter
            Assert.AreEqual("000011", sb.AsSpan().ToString());
            Assert.AreEqual(6, sb.Length);

            //sb = new StringBuilder(fixture);
            sb.Length = 0;
            sb.Append(fixture);
            //Assert.AreSame(sb, ); 
            sb.Replace(4, 6 - 4, "11"); // J2N; Corrected 2nd parameter
            Assert.AreEqual("000011", sb.AsSpan().ToString());
            Assert.AreEqual(6, sb.Length);

            //// FIXME Undocumented NPE in Sun's JRE 5.0_5
            //try
            //{
            //    sb.Replace(1, 2 - 1, null); // J2N; Corrected 2nd parameter
            //    Assert.Fail("No NPE");
            //}
            //catch (ArgumentNullException e)
            //{
            //    // Expected
            //}

            //try
            //{
            //    //sb = new StringBuilder(fixture);
            //    sb.Length = 0;
            //    sb.Append(fixture);
            //    sb.Replace(-1, 2 - -1, "11"); // J2N; Corrected 2nd parameter
            //    Assert.Fail("No SIOOBE, negative start");
            //}
            //catch (ArgumentOutOfRangeException e)
            //{
            //    // Expected
            //}

            //try
            //{
            //    //sb = new StringBuilder(fixture);
            //    sb.Length = 0;
            //    sb.Append(fixture);
            //    sb.Replace(5, 2 - 5, "11"); // J2N; Corrected 2nd parameter
            //    Assert.Fail("No SIOOBE, start > length");
            //}
            //catch (ArgumentOutOfRangeException e)
            //{
            //    // Expected
            //}

            //try
            //{
            //    //sb = new StringBuilder(fixture);
            //    sb.Length = 0;
            //    sb.Append(fixture);
            //    sb.Replace(3, 2 - 3, "11"); // J2N; Corrected 2nd parameter
            //    Assert.Fail("No SIOOBE, start > end");
            //}
            //catch (ArgumentOutOfRangeException e)
            //{
            //    // Expected
            //}

            // Regression for HARMONY-348
            using ValueStringBuilder buffer = new ValueStringBuilder(stackalloc char[8]);
            buffer.Append("1234567");
            buffer.Replace(2, 6 - 2, "XXX"); // J2N; Corrected 2nd parameter
            Assert.AreEqual("12XXX7", buffer.ToString());
        }


        /**
         * @tests java.lang.StringBuilder.replace(int, int, String)'
         */
        [Test]
        public void Test_Replace_ReadOnlySpan()
        {
            string fixture = "0000";
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[64]);
            sb.Append(fixture);
            //Assert.AreSame(sb, );
            sb.Replace(1, 3 - 1, "11".AsSpan()); // J2N; Corrected 2nd parameter
            Assert.AreEqual("0110", sb.AsSpan().ToString());
            Assert.AreEqual(4, sb.Length);

            sb.Length = 0;
            sb.Append(fixture);
            //Assert.AreSame(sb, );
            sb.Replace(1, 2 - 1, "11".AsSpan()); // J2N; Corrected 2nd parameter
            Assert.AreEqual("01100", sb.AsSpan().ToString());
            Assert.AreEqual(5, sb.Length);

            sb.Length = 0;
            sb.Append(fixture);
            //Assert.AreSame(sb, );
            sb.Replace(4, 5 - 4, "11".AsSpan()); // J2N; Corrected 2nd parameter
            Assert.AreEqual("000011", sb.AsSpan().ToString());
            Assert.AreEqual(6, sb.Length);

            sb.Length = 0;
            sb.Append(fixture);
            //Assert.AreSame(sb, );
            sb.Replace(4, 6 - 4, "11".AsSpan()); // J2N; Corrected 2nd parameter
            Assert.AreEqual("000011", sb.AsSpan().ToString());
            Assert.AreEqual(6, sb.Length);

            ////// FIXME Undocumented NPE in Sun's JRE 5.0_5
            ////try
            ////{
            ////    sb.Replace(1, 2 - 1, null); // J2N; Corrected 2nd parameter
            ////    fail("No NPE");
            ////}
            ////catch (ArgumentNullException e)
            ////{
            ////    // Expected
            ////}

            //try
            //{
            //    sb.Length = 0;
            //    sb.Append(fixture);
            //    sb.Replace(-1, 2 - -1, "11".AsSpan()); // J2N; Corrected 2nd parameter
            //    Assert.Fail("No SIOOBE, negative start");
            //}
            //catch (ArgumentOutOfRangeException e)
            //{
            //    // Expected
            //}

            //try
            //{
            //    sb.Length = 0;
            //    sb.Append(fixture);
            //    sb.Replace(5, 2 - 5, "11".AsSpan()); // J2N; Corrected 2nd parameter
            //    Assert.Fail("No SIOOBE, start > length");
            //}
            //catch (ArgumentOutOfRangeException e)
            //{
            //    // Expected
            //}

            //try
            //{
            //    sb.Length = 0;
            //    sb.Append(fixture);
            //    sb.Replace(3, 2 - 3, "11".AsSpan()); // J2N; Corrected 2nd parameter
            //    Assert.Fail("No SIOOBE, start > end");
            //}
            //catch (ArgumentOutOfRangeException e)
            //{
            //    // Expected
            //}

            // Regression for HARMONY-348
            using ValueStringBuilder buffer = new ValueStringBuilder(stackalloc char[8]);
            buffer.Append("1234567");
            buffer.Replace(2, 6 - 2, "XXX".AsSpan()); // J2N; Corrected 2nd parameter
            Assert.AreEqual("12XXX7", buffer.AsSpan().ToString());
        }



        [Test]
        public void Test_IndexOf_String_CultureSensitivity()
        {
            string fixture = "ዬ፡ዶጶቝአሄኢቌጕኬ\u124fቖኋዘዻ፡ሆገኅጬሷ\u135cቔቿ፺ዃጫቭዄ";
            string searchFor = "ሄኢቌጕኬ\u124fቖኋዘዻ";
            using ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[fixture.Length]);
            sb.Append(fixture);

            using (var context = new CultureContext("ru-MD"))
            {
                //Assert.AreEqual(fixture.IndexOf(searchFor, StringComparison.CurrentCulture), sb.IndexOf(searchFor, StringComparison.CurrentCulture));
                Assert.AreEqual(6, sb.IndexOf(searchFor, StringComparison.Ordinal));
                //Assert.AreEqual(6, sb.IndexOf(searchFor, StringComparison.OrdinalIgnoreCase));
                //Assert.AreEqual(fixture.IndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase), sb.IndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase));
                //Assert.AreEqual(fixture.IndexOf(searchFor, StringComparison.InvariantCulture), sb.IndexOf(searchFor, StringComparison.InvariantCulture));
                //Assert.AreEqual(fixture.IndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase), sb.IndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [Test]
        public void Test_IndexOf_String_CultureSensitivity_LargeString()
        {
            string fixture = LargeUnicodeString + "ዬ፡ዶጶቝአሄኢቌጕኬ\u124fቖኋዘዻ፡ሆገኅጬሷ\u135cቔቿ፺ዃጫቭዄ";
            string searchFor = "ሄኢቌጕኬ\u124fቖኋዘዻ";
            using ValueStringBuilder sb = new ValueStringBuilder(fixture.Length);
            sb.Append(fixture);

            using (var context = new CultureContext("ru-MD"))
            {
                //Assert.AreEqual(fixture.IndexOf(searchFor, StringComparison.CurrentCulture), sb.IndexOf(searchFor, StringComparison.CurrentCulture));
                Assert.AreEqual(LargeUnicodeString.Length + 6, sb.IndexOf(searchFor, StringComparison.Ordinal));
                //Assert.AreEqual(LargeUnicodeString.Length + 6, sb.IndexOf(searchFor, StringComparison.OrdinalIgnoreCase));
                //Assert.AreEqual(fixture.IndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase), sb.IndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase));
                //Assert.AreEqual(fixture.IndexOf(searchFor, StringComparison.InvariantCulture), sb.IndexOf(searchFor, StringComparison.InvariantCulture));
                //Assert.AreEqual(fixture.IndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase), sb.IndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [Test]
        public void Test_IndexOf_String_Int32_CultureSensitivity()
        {
            string fixture = "ዬ፡ዶጶቝአሄኢቌጕኬ\u124fቖኋዘዻ፡ሆገኅጬሷ\u135cቔቿ፺ዃጫቭዄ";
            string searchFor = "ሄኢቌጕኬ\u124fቖኋዘዻ";
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[fixture.Length]);
            sb.Append(fixture);

            using (var context = new CultureContext("ru-MD"))
            {
                //Assert.AreEqual(fixture.IndexOf(searchFor, 4, StringComparison.CurrentCulture), sb.IndexOf(searchFor, 4, StringComparison.CurrentCulture));
                Assert.AreEqual(6, sb.IndexOf(searchFor, 4, StringComparison.Ordinal));
                //Assert.AreEqual(6, sb.IndexOf(searchFor, 4, StringComparison.OrdinalIgnoreCase));
                //Assert.AreEqual(fixture.IndexOf(searchFor, 4, StringComparison.CurrentCultureIgnoreCase), sb.IndexOf(searchFor, 4, StringComparison.CurrentCultureIgnoreCase));
                //Assert.AreEqual(fixture.IndexOf(searchFor, 4, StringComparison.InvariantCulture), sb.IndexOf(searchFor, 4, StringComparison.InvariantCulture));
                //Assert.AreEqual(fixture.IndexOf(searchFor, 4, StringComparison.InvariantCultureIgnoreCase), sb.IndexOf(searchFor, 4, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [Test]
        public void Test_IndexOf_String_Int32_CultureSensitivity_LargeString()
        {
            string fixture = LargeUnicodeString + "ዬ፡ዶጶቝአሄኢቌጕኬ\u124fቖኋዘዻ፡ሆገኅጬሷ\u135cቔቿ፺ዃጫቭዄ";
            string searchFor = "ሄኢቌጕኬ\u124fቖኋዘዻ";
            ValueStringBuilder sb = new ValueStringBuilder(fixture.Length);
            sb.Append(fixture);

            using (var context = new CultureContext("ru-MD"))
            {
                //Assert.AreEqual(fixture.IndexOf(searchFor, 4, StringComparison.CurrentCulture), sb.IndexOf(searchFor, 4, StringComparison.CurrentCulture));
                Assert.AreEqual(LargeUnicodeString.Length + 6, sb.IndexOf(searchFor, 4, StringComparison.Ordinal));
                //Assert.AreEqual(LargeUnicodeString.Length + 6, sb.IndexOf(searchFor, 4, StringComparison.OrdinalIgnoreCase));
                //Assert.AreEqual(fixture.IndexOf(searchFor, 4, StringComparison.CurrentCultureIgnoreCase), sb.IndexOf(searchFor, 4, StringComparison.CurrentCultureIgnoreCase));
                //Assert.AreEqual(fixture.IndexOf(searchFor, 4, StringComparison.InvariantCulture), sb.IndexOf(searchFor, 4, StringComparison.InvariantCulture));
                //Assert.AreEqual(fixture.IndexOf(searchFor, 4, StringComparison.InvariantCultureIgnoreCase), sb.IndexOf(searchFor, 4, StringComparison.InvariantCultureIgnoreCase));
            }
        }


        [Test]
        public void Test_LastIndexOf_String_CultureSensitivity()
        {
            string fixture = "ዬ፡ዶጶቝአሄኢቌጕኬ\u124fቖኋዘዻ፡ሆገኅጬሷ\u135cቔቿ፺ዃጫቭዄ";
            string searchFor = "ሄኢቌጕኬ\u124fቖኋዘዻ";
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[fixture.Length]);
            sb.Append(fixture);

            using (var context = new CultureContext("ru-MD"))
            {
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, StringComparison.CurrentCulture), sb.LastIndexOf(searchFor, StringComparison.CurrentCulture));
                Assert.AreEqual(6, sb.LastIndexOf(searchFor, StringComparison.Ordinal));
                //Assert.AreEqual(6, sb.LastIndexOf(searchFor, StringComparison.OrdinalIgnoreCase));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase), sb.LastIndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, StringComparison.InvariantCulture), sb.LastIndexOf(searchFor, StringComparison.InvariantCulture));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase), sb.LastIndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [Test]
        public void Test_LastIndexOf_String_CultureSensitivity_LargeString()
        {
            string fixture = "ዬ፡ዶጶቝአሄኢቌጕኬ\u124fቖኋዘዻ፡ሆገኅጬሷ\u135cቔቿ፺ዃጫቭዄ" + LargeUnicodeString;
            string searchFor = "ሄኢቌጕኬ\u124fቖኋዘዻ";
            ValueStringBuilder sb = new ValueStringBuilder(fixture.Length);
            sb.Append(fixture);

            using (var context = new CultureContext("ru-MD"))
            {
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, StringComparison.CurrentCulture), sb.LastIndexOf(searchFor, StringComparison.CurrentCulture));
                Assert.AreEqual(6, sb.LastIndexOf(searchFor, StringComparison.Ordinal));
                //Assert.AreEqual(6, sb.LastIndexOf(searchFor, StringComparison.OrdinalIgnoreCase));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase), sb.LastIndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, StringComparison.InvariantCulture), sb.LastIndexOf(searchFor, StringComparison.InvariantCulture));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase), sb.LastIndexOf(searchFor, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [Test]
        public void Test_LastIndexOf_String_Int32_CultureSensitivity()
        {
            string fixture = "ዬ፡ዶጶቝአሄኢቌጕኬ\u124fቖኋዘዻ፡ሆገኅጬሷ\u135cቔቿ፺ዃጫቭዄ";
            string searchFor = "ሄኢቌጕኬ\u124fቖኋዘዻ";
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[fixture.Length]);
            sb.Append(fixture);

            using (var context = new CultureContext("ru-MD"))
            {
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, 20, StringComparison.CurrentCulture), sb.LastIndexOf(searchFor, 20, StringComparison.CurrentCulture));
                Assert.AreEqual(6, sb.LastIndexOf(searchFor, 20, StringComparison.Ordinal));
                //Assert.AreEqual(6, sb.LastIndexOf(searchFor, 20, StringComparison.OrdinalIgnoreCase));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, 20, StringComparison.CurrentCultureIgnoreCase), sb.LastIndexOf(searchFor, 20, StringComparison.CurrentCultureIgnoreCase));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, 20, StringComparison.InvariantCulture), sb.LastIndexOf(searchFor, 20, StringComparison.InvariantCulture));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, 20, StringComparison.InvariantCultureIgnoreCase), sb.LastIndexOf(searchFor, 20, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [Test]
        public void Test_LastIndexOf_String_Int32_CultureSensitivity_LargeString()
        {
            string fixture = "ዬ፡ዶጶቝአሄኢቌጕኬ\u124fቖኋዘዻ፡ሆገኅጬሷ\u135cቔቿ፺ዃጫቭዄ" + LargeUnicodeString;
            string searchFor = "ሄኢቌጕኬ\u124fቖኋዘዻ";
            ValueStringBuilder sb = new ValueStringBuilder(fixture.Length);
            sb.Append(fixture);

            using (var context = new CultureContext("ru-MD"))
            {
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, LargeUnicodeString.Length - 20, StringComparison.CurrentCulture), sb.LastIndexOf(searchFor, LargeUnicodeString.Length - 20, StringComparison.CurrentCulture));
                Assert.AreEqual(6, sb.LastIndexOf(searchFor, LargeUnicodeString.Length - 20, StringComparison.Ordinal));
                //Assert.AreEqual(6, sb.LastIndexOf(searchFor, LargeUnicodeString.Length - 20, StringComparison.OrdinalIgnoreCase));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, LargeUnicodeString.Length - 20, StringComparison.CurrentCultureIgnoreCase), sb.LastIndexOf(searchFor, LargeUnicodeString.Length - 20, StringComparison.CurrentCultureIgnoreCase));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, LargeUnicodeString.Length - 20, StringComparison.InvariantCulture), sb.LastIndexOf(searchFor, LargeUnicodeString.Length - 20, StringComparison.InvariantCulture));
                //Assert.AreEqual(fixture.LastIndexOf(searchFor, LargeUnicodeString.Length - 20, StringComparison.InvariantCultureIgnoreCase), sb.LastIndexOf(searchFor, LargeUnicodeString.Length - 20, StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
