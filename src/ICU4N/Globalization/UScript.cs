using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Constants for ISO 15924 script codes.
    /// </summary>
    /// <remarks>
    /// The current set of script code constants supports at least all scripts
    /// that are encoded in the version of Unicode which ICU currently supports.
    /// The names of the constants are usually derived from the
    /// Unicode script property value aliases.
    /// See UAX #24 Unicode Script Property (<a href="http://www.unicode.org/reports/tr24/">http://www.unicode.org/reports/tr24/</a>)
    /// and <a href="http://www.unicode.org/Public/UCD/latest/ucd/PropertyValueAliases.txt">ttp://www.unicode.org/Public/UCD/latest/ucd/PropertyValueAliases.txt</a>.
    /// <para/>
    /// In addition, constants for many ISO 15924 script codes
    /// are included, for use with language tags, CLDR data, and similar.
    /// Some of those codes are not used in the Unicode Character Database (UCD).
    /// For example, there are no characters that have a UCD script property value of
    /// Hans or Hant. All Han ideographs have the Hani script property value in Unicode.
    /// <para/>
    /// Private-use codes Qaaa..Qabx are not included, except as used in the UCD or in CLDR.
    /// <para/>
    /// Starting with ICU 55, script codes are only added when their scripts
    /// have been or will certainly be encoded in Unicode,
    /// and have been assigned Unicode script property value aliases,
    /// to ensure that their script names are stable and match the names of the constants.
    /// Script codes like Latf and Aran that are not subject to separate encoding
    /// may be added at any time.
    /// </remarks>
    /// <stable>ICU 2.2</stable>
    public static class UScript // ICU4N specific - made static since there are no instance members
    {
        /// <summary>
        /// Invalid code
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int InvalidCode = -1;

        /// <summary>
        /// Common
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Common = 0;  /* Zyyy */

        /// <summary>
        /// Inherited
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Inherited = 1;  /* Zinh */ /* "Code for inherited script"; for non-spacing combining marks; also Qaai */

        /// <summary>
        /// Arabic
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Arabic = 2;  /* Arab */

        /// <summary>
        /// Armenian
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Armenian = 3;  /* Armn */

        /// <summary>
        /// Bengali
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Bengali = 4;  /* Beng */

        /// <summary>
        /// Bopomofo
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Bopomofo = 5;  /* Bopo */

        /// <summary>
        /// Cherokee
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Cherokee = 6;  /* Cher */

        /// <summary>
        /// Coptic
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Coptic = 7;  /* Qaac */

        /// <summary>
        /// Cyrillic
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Cyrillic = 8;  /* Cyrl (Cyrs) */

        /// <summary>
        /// Deseret
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Deseret = 9;  /* Dsrt */

        /// <summary>
        /// Devanagari
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Devanagari = 10;  /* Deva */

        /// <summary>
        /// Ethiopic
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Ethiopic = 11;  /* Ethi */

        /// <summary>
        /// Georgian
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Georgian = 12;  /* Geor (Geon; Geoa) */

        /// <summary>
        /// Gothic
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Gothic = 13;  /* Goth */

        /// <summary>
        /// Greek
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Greek = 14;  /* Grek */

        /// <summary>
        /// Gujarati
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Gujarati = 15;  /* Gujr */

        /// <summary>
        /// Gurmukhi
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Gurmukhi = 16;  /* Guru */

        /// <summary>
        /// Han
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Han = 17;  /* Hani */

        /// <summary>
        /// Hangul
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Hangul = 18;  /* Hang */

        /// <summary>
        /// Hebrew
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Hebrew = 19;  /* Hebr */

        /// <summary>
        /// Hiragana
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Hiragana = 20;  /* Hira */

        /// <summary>
        /// Kannada
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Kannada = 21;  /* Knda */

        /// <summary>
        /// Katakana
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Katakana = 22;  /* Kana */

        /// <summary>
        /// Khmer
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Khmer = 23;  /* Khmr */

        /// <summary>
        /// Lao
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Lao = 24;  /* Laoo */

        /// <summary>
        /// Latin
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Latin = 25;  /* Latn (Latf; Latg) */

        /// <summary>
        /// Malayalam
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Malayalam = 26;  /* Mlym */

        /// <summary>
        /// Mongolian
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Mongolian = 27;  /* Mong */

        /// <summary>
        /// Myanmar
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Myanmar = 28;  /* Mymr */

        /// <summary>
        /// Ogham
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Ogham = 29;  /* Ogam */

        /// <summary>
        /// Old Italic
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int OldItalic = 30;  /* Ital */

        /// <summary>
        /// Oriya
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Oriya = 31;  /* Orya */

        /// <summary>
        /// Runic
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Runic = 32;  /* Runr */

        /// <summary>
        /// Sinhala
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Sinhala = 33;  /* Sinh */

        /// <summary>
        /// Syriac
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Syriac = 34;  /* Syrc (Syrj; Syrn; Syre) */

        /// <summary>
        /// Tamil
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Tamil = 35;  /* Taml */

        /// <summary>
        /// Telugu
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Telugu = 36;  /* Telu */

        /// <summary>
        /// Thaana
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Thaana = 37;  /* Thaa */

        /// <summary>
        /// Thai
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Thai = 38;  /* Thai */

        /// <summary>
        /// Tibetan
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Tibetan = 39;  /* Tibt */

        /// <summary>
        /// Unified Canadian Aboriginal Symbols
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int CanadianAboriginal = 40;  /* Cans */

        /// <summary>
        /// Unified Canadian Aboriginal Symbols (alias)
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int UCAS = CanadianAboriginal;  /* Cans */

        /// <summary>
        /// Yi syllables
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Yi = 41;  /* Yiii */

        /// <summary>
        /// Tagalog
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Tagalog = 42;  /* Tglg */

        /// <summary>
        /// Hanunoo
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Hanunoo = 43;  /* Hano */

        /// <summary>
        /// Buhid
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public const int Buhid = 44;  /* Buhd */

        /// <summary>
        /// Tagbanwa
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int Tagbanwa = 45;  /* Tagb */

        /// <summary>
        /// Braille
        /// Script in Unicode 4
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int Braille = 46;  /* Brai */

        /// <summary>
        /// Cypriot
        /// Script in Unicode 4
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int Cypriot = 47;  /* Cprt */

        /// <summary>
        /// Limbu
        /// Script in Unicode 4
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int Limbu = 48;  /* Limb */

        /// <summary>
        /// Linear B
        /// Script in Unicode 4
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int LinearB = 49;  /* Linb */

        /// <summary>
        /// Osmanya
        /// Script in Unicode 4
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int Osmanya = 50;  /* Osma */

        /// <summary>
        /// Shavian
        /// Script in Unicode 4
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int Shavian = 51;  /* Shaw */

        /// <summary>
        /// Tai Le
        /// Script in Unicode 4
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int TaiLe = 52;  /* Tale */

        /// <summary>
        /// Ugaritic
        /// Script in Unicode 4
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int Ugaritic = 53;  /* Ugar */

        /// <summary>
        /// Japanese syllabaries (alias for Hiragana + Katakana)
        /// Script in Unicode 4.0.1
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public const int KatakanaOrHiragana = 54;  /*Hrkt */

        /// <summary>
        /// Buginese
        /// Script in Unicode 4.1
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int Buginese = 55;           /* Bugi */

        /// <summary>
        /// Glagolitic
        /// Script in Unicode 4.1
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int Glagolitic = 56;         /* Glag */

        /// <summary>
        /// Kharoshthi
        /// Script in Unicode 4.1
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int Kharoshthi = 57;         /* Khar */

        /// <summary>
        /// Syloti Nagri
        /// Script in Unicode 4.1
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int SylotiNagri = 58;       /* Sylo */

        /// <summary>
        /// New Tai Lue
        /// Script in Unicode 4.1
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int NewTaiLue = 59;        /* Talu */

        /// <summary>
        /// Tifinagh
        /// Script in Unicode 4.1
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int Tifinagh = 60;           /* Tfng */

        /// <summary>
        /// Old Persian
        /// Script in Unicode 4.1
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public const int OldPersian = 61;        /* Xpeo */

        /// <summary>
        /// Balinese
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Balinese = 62; /* Bali */

        /// <summary>
        /// Batak
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Batak = 63; /* Batk */

        /// <summary>
        /// Blissymbols
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Blissymbols = 64; /* Blis */

        /// <summary>
        /// Brahmi
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Brahmi = 65; /* Brah */

        /// <summary>
        /// Cham
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Cham = 66; /* Cham */

        /// <summary>
        /// Cirth
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Cirth = 67; /* Cirt */

        /// <summary>
        /// Cyrillic (Old Church Slavonic variant)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int OldChurchSlavonicCyrillic = 68; /* Cyrs */

        /// <summary>
        /// Egyptian demotic
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int DemoticEgyptian = 69; /* Egyd */

        /// <summary>
        /// Egyptian hieratic
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int HieraticEgyptian = 70; /* Egyh */

        /// <summary>
        /// Egyptian hieroglyphs
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int EgyptianHieroglyphs = 71; /* Egyp */

        /// <summary>
        /// Khutsuri
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Khutsuri = 72; /* Geok */

        /// <summary>
        /// Han (Simplified variant)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int SimplifiedHan = 73; /* Hans */

        /// <summary>
        /// Han (Traditional variant)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int TraditionalHan = 74; /* Hant */

        /// <summary>
        /// Pahawh Hmong
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int PahawhHmong = 75; /* Hmng */

        /// <summary>
        /// Old Hungarian
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int OldHungarian = 76; /* Hung */

        /// <summary>
        /// Indus (Harappan)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int HarappanIndus = 77; /* Inds */

        /// <summary>
        /// Javanese
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Javanese = 78; /* Java */

        /// <summary>
        /// Kayah Li
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int KayahLi = 79; /* Kali */

        /// <summary>
        /// Latin (Fraktur variant)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int LatinFraktur = 80; /* Latf */

        /// <summary>
        /// Latin (Gaelic variant)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int LatinGaelic = 81; /* Latg */

        /// <summary>
        /// Lepcha (Róng)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Lepcha = 82; /* Lepc */

        /// <summary>
        /// Linear A
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int LinearA = 83; /* Lina */

        /// <summary>
        /// Mandaic
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int Mandaic = 84; /* Mand */

        /// <summary>
        /// Mandaean
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Mandaean = Mandaic;

        /// <summary>
        /// Mayan hieroglyphs
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int MayanHieroglyphs = 85; /* Maya */

        /// <summary>
        /// Meroitic Hieroglyphs
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int MeroiticHieroglyphs = 86; /* Mero */

        /// <summary>
        /// Meroitic
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Meroitic = MeroiticHieroglyphs;

        /// <summary>
        /// N’Ko
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int NKo = 87; /* Nkoo */

        /// <summary>
        /// Orkhon
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Orkhon = 88; /* Orkh */

        /// <summary>
        /// Old Permic
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int OldPermic = 89; /* Perm */

        /// <summary>
        /// Phags-pa
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int PhagsPa = 90; /* Phag */

        /// <summary>
        /// Phoenician
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Phoenician = 91; /* Phnx */

        /// <summary>
        /// Miao (Pollard)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 52</stable>
        public const int Miao = 92; /* Plrd */

        /// <summary>
        /// Miao (Pollard)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int PhoneticPollard = Miao;

        /// <summary>
        /// Rongorongo
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Rongorongo = 93; /* Roro */

        /// <summary>
        /// Sarati
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Sarati = 94; /* Sara */

        /// <summary>
        /// Syriac (Estrangelo variant)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int EstrangeloSyriac = 95; /* Syre */

        /// <summary>
        /// Syriac (Western variant)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int WesternSyriac = 96; /* Syrj */

        /// <summary>
        /// Syriac (Eastern variant)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int EasternSyriac = 97; /* Syrn */

        /// <summary>
        /// Tengwar
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Tengwar = 98; /* Teng */

        /// <summary>
        /// Vai
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Vai = 99; /* Vaii */

        /// <summary>
        /// Visible Speech
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int VisibleSpeech = 100;/* Visp */

        /// <summary>
        /// Cuneiform; Sumero-Akkadian
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Cuneiform = 101;/* Xsux */

        /// <summary>
        /// Code for unwritten documents
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int UnwrittenLanguages = 102;/* Zxxx */

        /// <summary>
        /// Unknown
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public const int Unknown = 103;/* Zzzz */ /* Unknown="Code for uncoded script"; for unassigned code points */

        /// <summary>
        /// Carian
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int Carian = 104;/* Cari */

        /// <summary>
        /// Japanese
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int Japanese = 105;/* Jpan */

        /// <summary>
        /// Tai Tham (Lanna)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int Lanna = 106;/* Lana */

        /// <summary>
        /// Lycian
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int Lycian = 107;/* Lyci */

        /// <summary>
        /// Lydian
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int Lydian = 108;/* Lydi */

        /// <summary>
        /// Ol Chiki (Ol Cemet’; Ol; Santali)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int OlChiki = 109;/* Olck */

        /// <summary>
        /// Rejang (Redjang; Kaganga)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int Rejang = 110;/* Rjng */

        /// <summary>
        /// Saurashtra
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int Saurashtra = 111;/* Saur */

        /// <summary>
        /// ISO 15924 script code for Sutton SignWriting
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int SignWriting = 112;/* Sgnw */

        /// <summary>
        /// Sundanese
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int Sundanese = 113;/* Sund */

        /// <summary>
        /// Moon (Moon code; Moon script; Moon type)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int Moon = 114;/* Moon */

        /// <summary>
        /// Meitei Mayek (Meithei; Meetei)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public const int MeiteiMayek = 115;/* Mtei */

        /// <summary>
        /// Imperial Aramaic
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int ImperialAramaic = 116;/* Armi */

        /// <summary>
        /// Avestan
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int Avestan = 117;/* Avst */

        /// <summary>
        /// Chakma
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int Chakma = 118;/* Cakm */

        /// <summary>
        /// Korean
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int Korean = 119;/* Kore */

        /// <summary>
        /// Kaithi
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int Kaithi = 120;/* Kthi */

        /// <summary>
        /// Manichaean
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int Manichaean = 121;/* Mani */

        /// <summary>
        /// Inscriptional Pahlavi
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int InscriptionalPahlavi = 122;/* Phli */

        /// <summary>
        /// Psalter Pahlavi
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int PsalterPahlavi = 123;/* Phlp */

        /// <summary>
        /// Book Pahlavi
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int BookPahlavi = 124;/* Phlv */

        /// <summary>
        /// Inscriptional Parthian
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int InscriptionalParthian = 125;/* Prti */

        /// <summary>
        /// Samaritan
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int Samaritan = 126;/* Samr */

        /// <summary>
        /// Tai Viet
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int TaiViet = 127;/* Tavt */

        /// <summary>
        /// Mathematical notation
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int MathematicalNotation = 128;/* Zmth */

        /// <summary>
        /// Symbols
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public const int Symbols = 129;/* Zsym */

        /// <summary>
        /// Bamum
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public const int Bamum = 130;/* Bamu */

        /// <summary>
        /// Lisu (Fraser)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public const int Lisu = 131;/* Lisu */

        /// <summary>
        /// Nakhi Geba
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public const int NakhiGeba = 132;/* Nkgb */

        /// <summary>
        /// Old South Arabian
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public const int OldSouthArabian = 133;/* Sarb */

        /// <summary>
        /// Bassa Vah
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int BassaVah = 134;/* Bass */

        /// <summary>
        /// Duployan shorthand; Duployan stenography
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 54</stable>
        public const int Duployan = 135;/* Dupl */
        /// <summary>
        /// Typo; use DUPLOYAN
        /// </summary>
        [Obsolete("ICU 54")]
        public const int DuployanShorthand = Duployan;

        /// <summary>
        /// Elbasan
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int Elbasan = 136;/* Elba */

        /// <summary>
        /// Grantha
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int Grantha = 137;/* Gran */

        /// <summary>
        /// Kpelle
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int Kpelle = 138;/* Kpel */

        /// <summary>
        /// Loma
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int Loma = 139;/* Loma */

        /// <summary>
        /// Mende Kikakui
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int Mende = 140;/* Mend */

        /// <summary>
        /// Meroitic Cursive
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int MeroiticCursive = 141;/* Merc */

        /// <summary>
        /// Old North Arabian (Ancient North Arabian)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int OldNorthArabian = 142;/* Narb */

        /// <summary>
        /// Nabataean
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int Nabataean = 143;/* Nbat */

        /// <summary>
        /// Palmyrene
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int Palmyrene = 144;/* Palm */

        /// <summary>
        /// Khudawadi; Sindhi
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 54</stable>
        public const int Khudawadi = 145;/* Sind */

        /// <summary>
        /// Khudawadi; Sindhi
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int Sindhi = Khudawadi;

        /// <summary>
        /// Warang Citi (Varang Kshiti)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public const int WarangCiti = 146;/* Wara */

        /// <summary>
        /// Afaka
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Afaka = 147;/* Afak */

        /// <summary>
        /// Jurchen
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Jurchen = 148;/* Jurc */

        /// <summary>
        /// Mro; Mru
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Mro = 149;/* Mroo */

        /// <summary>
        /// Nüshu
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Nushu = 150;/* Nshu */

        /// <summary>
        /// Sharada; Śāradā
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Sharada = 151;/* Shrd */

        /// <summary>
        /// Sora Sompeng
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int SoraSompeng = 152;/* Sora */

        /// <summary>
        /// Takri; Ṭākrī; Ṭāṅkrī
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Takri = 153;/* Takr */

        /// <summary>
        /// Tangut
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Tangut = 154;/* Tang */

        /// <summary>
        /// Woleai
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Woleai = 155;/* Wole */

        /// <summary>
        /// Anatolian Hieroglyphs (Luwian Hieroglyphs; Hittite Hieroglyphs)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 49</stable>
        public const int AnatolianHieroglyphs = 156;/* Hluw */

        /// <summary>
        /// Khojki
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 49</stable>
        public const int Khojki = 157;/* Khoj */

        /// <summary>
        /// Tirhuta
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 49</stable>
        public const int Tirhuta = 158;/* Tirh */

        /// <summary>
        /// Caucasian Albanian
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 52</stable>
        public const int CaucasianAlbanian = 159; /* Aghb */

        /// <summary>
        /// Mahajani
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 52</stable>
        public const int Mahajani = 160; /* Mahj */

        /// <summary>
        /// Ahom; Tai Ahom
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 54</stable>
        public const int Ahom = 161; /* Ahom */

        /// <summary>
        /// Hatran
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 54</stable>
        public const int Hatran = 162; /* Hatr */

        /// <summary>
        /// Modi; Moḍī
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 54</stable>
        public const int Modi = 163; /* Modi */

        /// <summary>
        /// Multani
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 54</stable>
        public const int Multani = 164; /* Mult */

        /// <summary>
        /// Pau Cin Hau
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 54</stable>
        public const int PauCinHau = 165; /* Pauc */

        /// <summary>
        /// Siddham; Siddhaṃ; Siddhamātṛkā
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 54</stable>
        public const int Siddham = 166; /* Sidd */

        /// <summary>
        /// Adlam
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 58</stable>
        public const int Adlam = 167; /* Adlm */

        /// <summary>
        /// Bhaiksuki
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 58</stable>
        public const int Bhaiksuki = 168; /* Bhks */

        /// <summary>
        /// Marchen
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 58</stable>
        public const int Marchen = 169; /* Marc */

        /// <summary>
        /// Newa; Newar; Newari; Nepāla lipi
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 58</stable>
        public const int Newa = 170; /* Newa */

        /// <summary>
        /// Osage
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 58</stable>
        public const int Osage = 171; /* Osge */

        /// <summary>
        /// Han with Bopomofo (alias for Han + Bopomofo)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 58</stable>
        public const int HanWithBopomofo = 172; /* Hanb */
        /// <summary>
        /// Jamo (alias for Jamo subset of Hangul)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 58</stable>
        public const int Jamo = 173; /* Jamo */

        /// <summary>
        /// Symbols (Emoji variant)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 58</stable>
        public const int SymbolsEmoji = 174; /* Zsye */

        /// <summary>
        /// Masaram Gondi
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 60</stable>
        public const int MasaramGondi = 175; /* Gonm */
        /// <summary>
        /// Soyombo
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 60</stable>
        public const int Soyombo = 176; /* Soyo */
        /// <summary>
        /// Zanabazar Square (Zanabazarin Dörböljin Useg; Xewtee Dörböljin Bicig; Horizontal Square Script)
        /// ISO 15924 script code
        /// </summary>
        /// <stable>ICU 60</stable>
        public const int ZanabazarSquare = 177; /* Zanb */

        /// <summary>
        /// One more than the highest normal Script code.
        /// The highest value is available via <see cref="UChar.GetIntPropertyMaxValue(UProperty)"/> (passing <see cref="UProperty.Script"/>).
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time, see ICU ticket #12420.")]
        public const int CodeLimit = 178;

        private static int[] GetCodesFromLocale(ULocale locale)
        {
            // Multi-script languages, equivalent to the LocaleScript data
            // that we used to load from locale resource bundles.
            string lang = locale.GetLanguage();
            if (lang.Equals("ja"))
            {
                return new int[] { UScript.Katakana, UScript.Hiragana, UScript.Han };
            }
            if (lang.Equals("ko"))
            {
                return new int[] { UScript.Hangul, UScript.Han };
            }
            string script = locale.GetScript();
            if (lang.Equals("zh") && script.Equals("Hant"))
            {
                return new int[] { UScript.Han, UScript.Bopomofo };
            }
            // Explicit script code.
            if (script.Length != 0)
            {
                int scriptCode = UScript.GetCodeFromName(script);
                if (scriptCode != UScript.InvalidCode)
                {
                    if (scriptCode == UScript.SimplifiedHan || scriptCode == UScript.TraditionalHan)
                    {
                        scriptCode = UScript.Han;
                    }
                    return new int[] { scriptCode };
                }
            }
            return null;
        }

        /// <summary>
        /// Helper function to find the code from locale.
        /// </summary>
        /// <param name="locale">The locale.</param>
        private static int[] FindCodeFromLocale(ULocale locale)
        {
            int[] result = GetCodesFromLocale(locale);
            if (result != null)
            {
                return result;
            }
            ULocale likely = ULocale.AddLikelySubtags(locale);
            return GetCodesFromLocale(likely);
        }

        /// <summary>
        /// Gets a script codes associated with the given locale or ISO 15924 abbreviation or name.
        /// Returns <see cref="UScript.Malayalam"/> given "Malayam" OR "Mlym".
        /// Returns <see cref="UScript.Latin"/> given "en" OR "en_US"
        /// </summary>
        /// <param name="locale"><see cref="CultureInfo"/>.</param>
        /// <returns>The script codes array. null if the the code cannot be found.</returns>
        /// <stable>ICU 2.4</stable>
        public static int[] GetCode(CultureInfo locale)
        {
            return FindCodeFromLocale(ULocale.ForLocale(locale));
        }

        /// <summary>
        /// Gets a script codes associated with the given locale or ISO 15924 abbreviation or name.
        /// Returns <see cref="UScript.Malayalam"/> given "Malayam" OR "Mlym".
        /// Returns <see cref="UScript.Latin"/> given "en" OR "en_US"
        /// </summary>
        /// <param name="locale"><see cref="ULocale"/>.</param>
        /// <returns>The script codes array. null if the the code cannot be found.</returns>
        /// <stable>ICU 3.0.</stable>
        public static int[] GetCode(ULocale locale)
        {
            return FindCodeFromLocale(locale);
        }

        /// <summary>
        /// Gets the script codes associated with the given locale or ISO 15924 abbreviation or name.
        /// Returns <see cref="UScript.Malayalam"/> given "Malayam" OR "Mlym".
        /// Returns <see cref="UScript.Latin"/> given "en" OR "en_US"
        /// <para/>
        /// Note: To search by short or long script alias only, use
        /// <see cref="GetCodeFromName(string)"/> instead.
        /// That does a fast lookup with no access of the locale data.
        /// </summary>
        /// <param name="nameOrAbbrOrLocale">Name of the script or ISO 15924 code or locale.</param>
        /// <returns>The script codes array. null if the the code cannot be found.</returns>
        /// <stable>ICU 2.4</stable>
        public static int[] GetCode(string nameOrAbbrOrLocale)
        {
            bool triedCode = false;
            if (nameOrAbbrOrLocale.IndexOf('_') < 0 && nameOrAbbrOrLocale.IndexOf('-') < 0)
            {
                // ICU4N specific - using TryGetPropertyValueEnum rather than GetPropertyValueEnumNoThrow
                if (UChar.TryGetPropertyValueEnum(UProperty.Script, nameOrAbbrOrLocale, out int propNum))
                {
                    return new int[] { propNum };
                }
                triedCode = true;
            }
            int[] scripts = FindCodeFromLocale(new ULocale(nameOrAbbrOrLocale));
            if (scripts != null)
            {
                return scripts;
            }
            if (!triedCode)
            {
                // ICU4N specific - using TryGetPropertyValueEnum rather than GetPropertyValueEnumNoThrow
                if (UChar.TryGetPropertyValueEnum(UProperty.Script, nameOrAbbrOrLocale, out int propNum))
                {
                    return new int[] { propNum };
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the script code associated with the given Unicode script property alias
        /// (name or abbreviation).
        /// Short aliases are ISO 15924 script codes.
        /// Returns <see cref="UScript.Malayalam"/> given "Malayam" OR "Mlym".
        /// </summary>
        /// <param name="nameOrAbbr">Name of the script or ISO 15924 code.</param>
        /// <returns>The script code value, or <see cref="UScript.InvalidCode"/> if the code cannot be found.</returns>
        /// <stable>ICU 54</stable>
        public static int GetCodeFromName(string nameOrAbbr)
        {
            // ICU4N specific - using TryGetPropertyValueEnum rather than GetPropertyValueEnumNoThrow
            if (UChar.TryGetPropertyValueEnum(UProperty.Script, nameOrAbbr, out int propNum))
            {
                return propNum;
            }
            return UScript.InvalidCode;
        }

        /// <summary>
        /// Gets the script code associated with the given codepoint.
        /// Returns <see cref="UScript.Malayalam"/> given 0x0D02
        /// </summary>
        /// <param name="codepoint">UChar32 codepoint.</param>
        /// <returns>The script code.</returns>
        /// <stable>ICU 2.4</stable>
        public static int GetScript(int codepoint)
        {
            if (codepoint >= UChar.MinValue & codepoint <= UChar.MaxValue)
            {
                int scriptX = UCharacterProperty.Instance.GetAdditional(codepoint, 0) & UCharacterProperty.ScriptXMask;
                if (scriptX < UCharacterProperty.ScriptXWithCommon)
                {
                    return scriptX;
                }
                else if (scriptX < UCharacterProperty.ScriptXWithInherited)
                {
                    return UScript.Common;
                }
                else if (scriptX < UCharacterProperty.ScriptXWithOther)
                {
                    return UScript.Inherited;
                }
                else
                {
                    return UCharacterProperty.Instance.m_scriptExtensions_[scriptX & UCharacterProperty.ScriptMask];
                }
            }
            else
            {
                throw new ArgumentException(codepoint.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Do the Script_Extensions of code point <paramref name="c"/> contain script <paramref name="sc"/>?
        /// If <paramref name="c"/> does not have explicit Script_Extensions, then this tests whether
        /// <paramref name="c"/> has the Script property value <paramref name="sc"/>.
        /// <para/>
        /// Some characters are commonly used in multiple scripts.
        /// For more information, see UAX #24: <a href="http://www.unicode.org/reports/tr24/">http://www.unicode.org/reports/tr24/</a>.
        /// </summary>
        /// <param name="c">Code point.</param>
        /// <param name="sc">Script code.</param>
        /// <returns>true if <paramref name="sc"/> is in Script_Extensions(<paramref name="c"/>).</returns>
        /// <stable>ICU 49</stable>
        public static bool HasScript(int c, int sc)
        {
            int scriptX = UCharacterProperty.Instance.GetAdditional(c, 0) & UCharacterProperty.ScriptXMask;
            if (scriptX < UCharacterProperty.ScriptXWithCommon)
            {
                return sc == scriptX;
            }

            char[] scriptExtensions = UCharacterProperty.Instance.m_scriptExtensions_;
            int scx = scriptX & UCharacterProperty.ScriptMask;  // index into scriptExtensions
            if (scriptX >= UCharacterProperty.ScriptXWithOther)
            {
                scx = scriptExtensions[scx + 1];
            }
            if (sc > 0x7fff)
            {
                // Guard against bogus input that would
                // make us go past the Script_Extensions terminator.
                return false;
            }
            while (sc > scriptExtensions[scx])
            {
                ++scx;
            }
            return sc == (scriptExtensions[scx] & 0x7fff);
        }

        /// <summary>
        /// Sets code point <paramref name="c"/>'s Script_Extensions as script code integers into the output <see cref="BitArray"/>.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///     <item><description>
        ///         If <paramref name="c"/> does have Script_Extensions, then the return value is
        ///         the negative number of Script_Extensions codes (= -set.Cardinality());
        ///         in this case, the Script property value
        ///         (normally Common or Inherited) is not included in the set.
        ///     </description></item>
        ///     <item><description>
        ///         If <paramref name="c"/> does not have Script_Extensions, then the one Script code is put into the set
        ///         and also returned.
        ///     </description></item>
        ///     <item><description>
        ///         If <paramref name="c"/> is not a valid code point, then the one <see cref="UScript.Unknown"/> code is put into the set
        ///         and also returned.
        ///     </description></item>
        /// </list>
        /// In other words, if the return value is non-negative, it is <paramref name="c"/>'s single Script code
        /// and the set contains exactly this Script code.
        /// If the return value is -n, then the set contains <paramref name="c"/>'s n&gt;=2 Script_Extensions script codes.
        /// <para/>
        /// Some characters are commonly used in multiple scripts.
        /// For more information, see UAX #24: <a href="http://www.unicode.org/reports/tr24/">http://www.unicode.org/reports/tr24/</a>.
        /// </remarks>
        /// <param name="c">Code point.</param>
        /// <param name="set">Set of script code integers; will be cleared, then bits are set
        /// corresponding to <paramref name="c"/>'s Script_Extensions.</param>
        /// <returns>Negative number of script codes in c's Script_Extensions,
        /// or the non-negative single Script value.</returns>
        /// <stable>ICU 49</stable>
        public static int GetScriptExtensions(int c, BitArray set)
        {
            set.SetAll(false);
            int scriptX = UCharacterProperty.Instance.GetAdditional(c, 0) & UCharacterProperty.ScriptXMask;
            if (scriptX < UCharacterProperty.ScriptXWithCommon)
            {
                set.Set(scriptX, true);
                return scriptX;
            }

            char[] scriptExtensions = UCharacterProperty.Instance.m_scriptExtensions_;
            int scx = scriptX & UCharacterProperty.ScriptMask;  // index into scriptExtensions
            if (scriptX >= UCharacterProperty.ScriptXWithOther)
            {
                scx = scriptExtensions[scx + 1];
            }
            int length = 0;
            int sx;
            do
            {
                sx = scriptExtensions[scx++];
                set.Set(sx & 0x7fff, true);
                ++length;
            } while (sx < 0x8000);
            // length==set.cardinality()
            return -length;
        }

        /// <summary>
        /// Returns the long Unicode script name, if there is one.
        /// Otherwise returns the 4-letter ISO 15924 script code.
        /// Returns "Malayam" given <see cref="UScript.Malayalam"/>.
        /// </summary>
        /// <param name="scriptCode">int script code.</param>
        /// <returns>Long script name as given in PropertyValueAliases.txt, or the 4-letter code.</returns>
        /// <exception cref="ArgumentException">If the script code is not valid.</exception>
        /// <seealso cref="TryGetName(int, out string)"/>
        /// <stable>ICU 2.4</stable>
        public static string GetName(int scriptCode)
        {
            return UChar.GetPropertyValueName(UProperty.Script,
                    scriptCode,
                    NameChoice.Long);
        }

        /// <summary>
        /// Gets the long Unicode script name, if there is one.
        /// Otherwise returns the 4-letter ISO 15924 script code.
        /// Returns "Malayam" given <see cref="UScript.Malayalam"/>.
        /// </summary>
        /// <param name="scriptCode">Script code.</param>
        /// <param name="result">Long script name as given in PropertyValueAliases.txt, or the 4-letter code.</param>
        /// <returns>true if the script code is valid, otherwise false.</returns>
        /// <seealso cref="GetName(int)"/>
        /// <stable>ICU4N 60.1.0</stable>
        public static bool TryGetName(int scriptCode, out string result) // ICU4N TODO: Tests
        {
            return UChar.TryGetPropertyValueName(UProperty.Script,
                    scriptCode,
                    NameChoice.Long, out result);
        }

        /// <summary>
        /// Returns the 4-letter ISO 15924 script code,
        /// which is the same as the short Unicode script name if Unicode has names for the script.
        /// Returns "Mlym" given <see cref="UScript.Malayalam"/>.
        /// </summary>
        /// <param name="scriptCode">int script code</param>
        /// <returns>Short script name (4-letter code).</returns>
        /// <exception cref="ArgumentException">If the script code is not valid.</exception>
        /// <seealso cref="TryGetShortName(int, out string)"/>
        /// <stable>ICU 2.4</stable>
        public static string GetShortName(int scriptCode)
        {
            return UChar.GetPropertyValueName(UProperty.Script,
                    scriptCode,
                    NameChoice.Short);
        }

        /// <summary>
        /// Gets the 4-letter ISO 15924 script code,
        /// which is the same as the short Unicode script name if Unicode has names for the script.
        /// Returns "Mlym" given <see cref="UScript.Malayalam"/>.
        /// </summary>
        /// <param name="scriptCode">int script code</param>
        /// <param name="result">Short script name (4-letter code).</param>
        /// <returns>true if the name was retrieved, otherwise false.</returns>
        /// <exception cref="ArgumentException">If the script code is not valid.</exception>
        /// <seealso cref="GetShortName(int)"/>
        /// <stable>ICU4N 60.1.0</stable>
        public static bool TryGetShortName(int scriptCode, out string result) // ICU4N TODO: Tests
        {
            return UChar.TryGetPropertyValueName(UProperty.Script,
                    scriptCode,
                    NameChoice.Short, out result);
        }

        /// <summary>
        /// Script metadata (script properties).
        /// See <a href="http://unicode.org/cldr/trac/browser/trunk/common/properties/scriptMetadata.txt">
        /// http://unicode.org/cldr/trac/browser/trunk/common/properties/scriptMetadata.txt</a>
        /// </summary>
        private sealed class ScriptMetadata
        {
            // 0 = NOT_ENCODED, no sample character, default false script properties.
            // Bits 20.. 0: sample character

            // Bits 23..21: usage
            private const int UNKNOWN = 1 << 21;
            private const int EXCLUSION = 2 << 21;
            private const int LIMITED_USE = 3 << 21;
            // private const int ASPIRATIONAL = 4 << 21; -- not used any more since Unicode 10
            private const int RECOMMENDED = 5 << 21;

            // Bits 31..24: Single-bit flags
            internal const int RTL = 1 << 24;
            internal const int LB_LETTERS = 1 << 25;
            internal const int CASED = 1 << 26;

            private static readonly int[] SCRIPT_PROPS = {
                // Begin copy-paste output from
                // tools/trunk/unicode/py/parsescriptmetadata.py
                // or from icu/trunk/source/common/uscript_props.cpp
                0x0040 | RECOMMENDED,  // Zyyy
                0x0308 | RECOMMENDED,  // Zinh
                0x0628 | RECOMMENDED | RTL,  // Arab
                0x0531 | RECOMMENDED | CASED,  // Armn
                0x0995 | RECOMMENDED,  // Beng
                0x3105 | RECOMMENDED | LB_LETTERS,  // Bopo
                0x13C4 | LIMITED_USE | CASED,  // Cher
                0x03E2 | EXCLUSION | CASED,  // Copt
                0x042F | RECOMMENDED | CASED,  // Cyrl
                0x10414 | EXCLUSION | CASED,  // Dsrt
                0x0905 | RECOMMENDED,  // Deva
                0x12A0 | RECOMMENDED,  // Ethi
                0x10D3 | RECOMMENDED,  // Geor
                0x10330 | EXCLUSION,  // Goth
                0x03A9 | RECOMMENDED | CASED,  // Grek
                0x0A95 | RECOMMENDED,  // Gujr
                0x0A15 | RECOMMENDED,  // Guru
                0x5B57 | RECOMMENDED | LB_LETTERS,  // Hani
                0xAC00 | RECOMMENDED,  // Hang
                0x05D0 | RECOMMENDED | RTL,  // Hebr
                0x304B | RECOMMENDED | LB_LETTERS,  // Hira
                0x0C95 | RECOMMENDED,  // Knda
                0x30AB | RECOMMENDED | LB_LETTERS,  // Kana
                0x1780 | RECOMMENDED | LB_LETTERS,  // Khmr
                0x0EA5 | RECOMMENDED | LB_LETTERS,  // Laoo
                0x004C | RECOMMENDED | CASED,  // Latn
                0x0D15 | RECOMMENDED,  // Mlym
                0x1826 | LIMITED_USE,  // Mong
                0x1000 | RECOMMENDED | LB_LETTERS,  // Mymr
                0x168F | EXCLUSION,  // Ogam
                0x10300 | EXCLUSION,  // Ital
                0x0B15 | RECOMMENDED,  // Orya
                0x16A0 | EXCLUSION,  // Runr
                0x0D85 | RECOMMENDED,  // Sinh
                0x0710 | LIMITED_USE | RTL,  // Syrc
                0x0B95 | RECOMMENDED,  // Taml
                0x0C15 | RECOMMENDED,  // Telu
                0x078C | RECOMMENDED | RTL,  // Thaa
                0x0E17 | RECOMMENDED | LB_LETTERS,  // Thai
                0x0F40 | RECOMMENDED,  // Tibt
                0x14C0 | LIMITED_USE,  // Cans
                0xA288 | LIMITED_USE | LB_LETTERS,  // Yiii
                0x1703 | EXCLUSION,  // Tglg
                0x1723 | EXCLUSION,  // Hano
                0x1743 | EXCLUSION,  // Buhd
                0x1763 | EXCLUSION,  // Tagb
                0x280E | UNKNOWN,  // Brai
                0x10800 | EXCLUSION | RTL,  // Cprt
                0x1900 | LIMITED_USE,  // Limb
                0x10000 | EXCLUSION,  // Linb
                0x10480 | EXCLUSION,  // Osma
                0x10450 | EXCLUSION,  // Shaw
                0x1950 | LIMITED_USE | LB_LETTERS,  // Tale
                0x10380 | EXCLUSION,  // Ugar
                0,
                0x1A00 | EXCLUSION,  // Bugi
                0x2C00 | EXCLUSION | CASED,  // Glag
                0x10A00 | EXCLUSION | RTL,  // Khar
                0xA800 | LIMITED_USE,  // Sylo
                0x1980 | LIMITED_USE | LB_LETTERS,  // Talu
                0x2D30 | LIMITED_USE,  // Tfng
                0x103A0 | EXCLUSION,  // Xpeo
                0x1B05 | LIMITED_USE,  // Bali
                0x1BC0 | LIMITED_USE,  // Batk
                0,
                0x11005 | EXCLUSION,  // Brah
                0xAA00 | LIMITED_USE,  // Cham
                0,
                0,
                0,
                0,
                0x13153 | EXCLUSION,  // Egyp
                0,
                0x5B57 | RECOMMENDED | LB_LETTERS,  // Hans
                0x5B57 | RECOMMENDED | LB_LETTERS,  // Hant
                0x16B1C | EXCLUSION,  // Hmng
                0x10CA1 | EXCLUSION | RTL | CASED,  // Hung
                0,
                0xA984 | LIMITED_USE,  // Java
                0xA90A | LIMITED_USE,  // Kali
                0,
                0,
                0x1C00 | LIMITED_USE,  // Lepc
                0x10647 | EXCLUSION,  // Lina
                0x0840 | LIMITED_USE | RTL,  // Mand
                0,
                0x10980 | EXCLUSION | RTL,  // Mero
                0x07CA | LIMITED_USE | RTL,  // Nkoo
                0x10C00 | EXCLUSION | RTL,  // Orkh
                0x1036B | EXCLUSION,  // Perm
                0xA840 | EXCLUSION,  // Phag
                0x10900 | EXCLUSION | RTL,  // Phnx
                0x16F00 | LIMITED_USE,  // Plrd
                0,
                0,
                0,
                0,
                0,
                0,
                0xA549 | LIMITED_USE,  // Vaii
                0,
                0x12000 | EXCLUSION,  // Xsux
                0,
                0xFDD0 | UNKNOWN,  // Zzzz
                0x102A0 | EXCLUSION,  // Cari
                0x304B | RECOMMENDED | LB_LETTERS,  // Jpan
                0x1A20 | LIMITED_USE | LB_LETTERS,  // Lana
                0x10280 | EXCLUSION,  // Lyci
                0x10920 | EXCLUSION | RTL,  // Lydi
                0x1C5A | LIMITED_USE,  // Olck
                0xA930 | EXCLUSION,  // Rjng
                0xA882 | LIMITED_USE,  // Saur
                0x1D850 | EXCLUSION,  // Sgnw
                0x1B83 | LIMITED_USE,  // Sund
                0,
                0xABC0 | LIMITED_USE,  // Mtei
                0x10840 | EXCLUSION | RTL,  // Armi
                0x10B00 | EXCLUSION | RTL,  // Avst
                0x11103 | LIMITED_USE,  // Cakm
                0xAC00 | RECOMMENDED,  // Kore
                0x11083 | EXCLUSION,  // Kthi
                0x10AD8 | EXCLUSION | RTL,  // Mani
                0x10B60 | EXCLUSION | RTL,  // Phli
                0x10B8F | EXCLUSION | RTL,  // Phlp
                0,
                0x10B40 | EXCLUSION | RTL,  // Prti
                0x0800 | EXCLUSION | RTL,  // Samr
                0xAA80 | LIMITED_USE | LB_LETTERS,  // Tavt
                0,
                0,
                0xA6A0 | LIMITED_USE,  // Bamu
                0xA4D0 | LIMITED_USE,  // Lisu
                0,
                0x10A60 | EXCLUSION | RTL,  // Sarb
                0x16AE6 | EXCLUSION,  // Bass
                0x1BC20 | EXCLUSION,  // Dupl
                0x10500 | EXCLUSION,  // Elba
                0x11315 | EXCLUSION,  // Gran
                0,
                0,
                0x1E802 | EXCLUSION | RTL,  // Mend
                0x109A0 | EXCLUSION | RTL,  // Merc
                0x10A95 | EXCLUSION | RTL,  // Narb
                0x10896 | EXCLUSION | RTL,  // Nbat
                0x10873 | EXCLUSION | RTL,  // Palm
                0x112BE | EXCLUSION,  // Sind
                0x118B4 | EXCLUSION | CASED,  // Wara
                0,
                0,
                0x16A4F | EXCLUSION,  // Mroo
                0x1B1C4 | EXCLUSION | LB_LETTERS,  // Nshu
                0x11183 | EXCLUSION,  // Shrd
                0x110D0 | EXCLUSION,  // Sora
                0x11680 | EXCLUSION,  // Takr
                0x18229 | EXCLUSION | LB_LETTERS,  // Tang
                0,
                0x14400 | EXCLUSION,  // Hluw
                0x11208 | EXCLUSION,  // Khoj
                0x11484 | EXCLUSION,  // Tirh
                0x10537 | EXCLUSION,  // Aghb
                0x11152 | EXCLUSION,  // Mahj
                0x11717 | EXCLUSION | LB_LETTERS,  // Ahom
                0x108F4 | EXCLUSION | RTL,  // Hatr
                0x1160E | EXCLUSION,  // Modi
                0x1128F | EXCLUSION,  // Mult
                0x11AC0 | EXCLUSION,  // Pauc
                0x1158E | EXCLUSION,  // Sidd
                0x1E909 | LIMITED_USE | RTL | CASED,  // Adlm
                0x11C0E | EXCLUSION,  // Bhks
                0x11C72 | EXCLUSION,  // Marc
                0x11412 | LIMITED_USE,  // Newa
                0x104B5 | LIMITED_USE | CASED,  // Osge
                0x5B57 | RECOMMENDED | LB_LETTERS,  // Hanb
                0x1112 | RECOMMENDED,  // Jamo
                0,
                0x11D10 | EXCLUSION,  // Gonm
                0x11A5C | EXCLUSION,  // Soyo
                0x11A0B | EXCLUSION,  // Zanb
                // End copy-paste from parsescriptmetadata.py
            };

            internal static int GetScriptProps(int script)
            {
                if (0 <= script && script < SCRIPT_PROPS.Length)
                {
                    return SCRIPT_PROPS[script];
                }
                else
                {
                    return 0;
                }
            }
        }

        // ICU4N specific - de-nested ScriptUsage enum

        private static readonly ScriptUsage[] usageValues = (ScriptUsage[])Enum.GetValues(typeof(ScriptUsage));

        /// <summary>
        /// Returns the script sample character string.
        /// This string normally consists of one code point but might be longer.
        /// The string is empty if the script is not encoded.
        /// </summary>
        /// <param name="script">Script code.</param>
        /// <returns>The sample character string.</returns>
        /// <stable>ICU 51</stable>
        public static string GetSampleString(int script)
        {
            int sampleChar = ScriptMetadata.GetScriptProps(script) & 0x1fffff;
            if (sampleChar != 0)
            {
                return new StringBuilder().AppendCodePoint(sampleChar).ToString();
            }
            return "";
        }

        /// <summary>
        /// Returns the script usage according to UAX #31 Unicode Identifier and Pattern Syntax.
        /// Returns <see cref="ScriptUsage.NotEncoded"/> if the script is not encoded in Unicode.
        /// </summary>
        /// <param name="script">Script code.</param>
        /// <returns>Script usage.</returns>
        /// <seealso cref="ScriptUsage"/>
        /// <stable>ICU 51</stable>
        public static ScriptUsage GetUsage(int script)
        {
            return usageValues[(ScriptMetadata.GetScriptProps(script) >> 21) & 7];
        }

        /// <summary>
        /// Returns true if the script is written right-to-left.
        /// For example, Arab and Hebr.
        /// </summary>
        /// <param name="script">Script code.</param>
        /// <returns>true if the script is right-to-left.</returns>
        /// <stable>ICU 51</stable>
        public static bool IsRightToLeft(int script)
        {
            return (ScriptMetadata.GetScriptProps(script) & ScriptMetadata.RTL) != 0;
        }

        /// <summary>
        /// Returns true if the script allows line breaks between letters (excluding hyphenation).
        /// Such a script typically requires dictionary-based line breaking.
        /// For example, Hani and Thai.
        /// </summary>
        /// <param name="script">Script code.</param>
        /// <returns>true if the script allows line breaks between letters.</returns>
        /// <stable>ICU 51</stable>
        public static bool BreaksBetweenLetters(int script)
        {
            return (ScriptMetadata.GetScriptProps(script) & ScriptMetadata.LB_LETTERS) != 0;
        }

        /// <summary>
        /// Returns true if in modern (or most recent) usage of the script case distinctions are customary.
        /// For example, Latn and Cyrl.
        /// </summary>
        /// <param name="script">Script code.</param>
        /// <returns>true if the script is cased.</returns>
        /// <stable>ICU 51</stable>
        public static bool IsCased(int script)
        {
            return (ScriptMetadata.GetScriptProps(script) & ScriptMetadata.CASED) != 0;
        }

        // ICU4N specific - removed private constructor and made class static
    }

    /// <summary>
    /// Script usage constants.
    /// See UAX #31 Unicode Identifier and Pattern Syntax.
    /// <a href="http://www.unicode.org/reports/tr31/#Table_Candidate_Characters_for_Exclusion_from_Identifiers">
    /// http://www.unicode.org/reports/tr31/#Table_Candidate_Characters_for_Exclusion_from_Identifiers</a>
    /// </summary>
    /// <stable>ICU 51</stable>
    public enum ScriptUsage
    {
        /// <summary>
        /// Not encoded in Unicode.
        /// </summary>
        /// <stable>ICU 51</stable>
        NotEncoded,
        /// <summary>
        /// Unknown script usage.
        /// </summary>
        /// <stable>ICU 51</stable>
        Unknown,
        /// <summary>
        /// Candidate for Exclusion from Identifiers.
        /// </summary>
        /// <stable>ICU 51</stable>
        Excluded,
        /// <summary>
        /// Limited Use script.
        /// </summary>
        /// <stable>ICU 51</stable>
        LimitedUse,
        /// <summary>
        /// Aspirational Use script.
        /// </summary>
        /// <stable>ICU 51</stable>
        Aspirational,
        /// <summary>
        /// Recommended script.
        /// </summary>
        /// <stable>ICU 51</stable>
        Recommended
    }
}
