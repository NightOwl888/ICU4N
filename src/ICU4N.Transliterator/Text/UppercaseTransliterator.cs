using ICU4N.Globalization;
using ICU4N.Impl;
using System;
using System.Text;
using System.Threading;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that performs locale-sensitive ToUpper()
    /// case mapping.
    /// </summary>
    internal class UppercaseTransliterator : CaseMapTransliterator
    {
        /// <summary>
        /// Package accessible ID.
        /// </summary>
        internal const string _ID = "Any-Upper";
        // TODO: Add variants for tr/az, el, lt, default = default locale: ICU ticket #12720

        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            UppercaseTransliteratorFactory.Register();
        }

        private sealed class UppercaseTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory(_ID, new UppercaseTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new UppercaseTransliterator(new UCultureInfo("en_US"));
        }

        private readonly UCultureInfo locale;

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        public UppercaseTransliterator(UCultureInfo loc)
            : base(_ID, UCaseProperties.Instance.ToFullUpper, UCaseProperties.GetCaseLocale(loc))
        {
            locale = loc;
        }

        // NOTE: normally this would be static, but because the results vary by locale....
        SourceTargetUtility sourceTargetUtility = null;

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
            if (sourceTargetUtility == null)
            {
                LazyInitializer.EnsureInitialized(ref sourceTargetUtility, () =>
                {
                    return new SourceTargetUtility(new ToUpperTransform(locale));
                });
            }
            sourceTargetUtility.AddSourceTargetSet(this, inputFilter, sourceSet, targetSet);
        }

        private sealed class ToUpperTransform : IStringTransform
        {
            private readonly UCultureInfo locale;
            public ToUpperTransform(UCultureInfo locale)
            {
                this.locale = locale ?? throw new ArgumentNullException(nameof(locale));
            }

            public string Transform(string source)
                => UChar.ToUpper(locale, source);
        }
    }
}
