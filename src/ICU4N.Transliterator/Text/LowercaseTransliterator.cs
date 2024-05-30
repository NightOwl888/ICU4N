using ICU4N.Globalization;
using ICU4N.Impl;
using System;
using System.Text;
using System.Threading;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that performs locale-sensitive ToLower()
    /// case mapping.
    /// </summary>
    internal class LowercaseTransliterator : CaseMapTransliterator
    {
        /// <summary>
        /// Package accessible ID.
        /// </summary>
        internal const string _ID = "Any-Lower";

        // TODO: Add variants for tr/az, lt, default = default locale: ICU ticket #12720

        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            LowercaseTransliteratorFactory.Register();
            Transliterator.RegisterSpecialInverse("Lower", "Upper", true);
        }

        private sealed class LowercaseTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory(_ID, new LowercaseTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new LowercaseTransliterator(new UCultureInfo("en_US"));
        }

        private readonly UCultureInfo locale;

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        public LowercaseTransliterator(UCultureInfo loc)
            : base(_ID, UCaseProperties.Instance.ToFullLower, UCaseProperties.GetCaseLocale(loc))
        {
            locale = loc;
        }

        // NOTE: normally this would be static, but because the results vary by locale....
        SourceTargetUtility sourceTargetUtility = null;

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>.
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
            if (sourceTargetUtility == null)
            {
                LazyInitializer.EnsureInitialized(ref sourceTargetUtility, () =>
                {
                    return new SourceTargetUtility(new ToLowerTransform(locale));
                });
            }

            sourceTargetUtility.AddSourceTargetSet(this, inputFilter, sourceSet, targetSet);
        }

        private sealed class ToLowerTransform : IStringTransform
        {
            private readonly UCultureInfo locale;
            public ToLowerTransform(UCultureInfo locale)
            {
                this.locale = locale ?? throw new ArgumentNullException(nameof(locale));
            }

            public string Transform(string source)
                => UChar.ToLower(locale, source);
        }
    }
}
