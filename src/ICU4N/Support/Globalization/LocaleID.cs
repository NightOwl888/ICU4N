using System;
using System.Globalization;

namespace ICU4N.Globalization
{
    /// <summary>
    /// A struct used for passing around parts of a locale identifier.
    /// </summary>
    public struct LocaleID : IEquatable<LocaleID>
    {
        private readonly string language;
        private readonly string script;
        private readonly string country;
        private readonly string variant;

        public LocaleID(string language, string script, string country, string variant)
        {
            this.language = language ?? throw new ArgumentNullException(nameof(language));
            this.script = script ?? throw new ArgumentNullException(nameof(script));
            this.country = country ?? throw new ArgumentNullException(nameof(country));
            this.variant = variant ?? throw new ArgumentNullException(nameof(variant));
        }

        public string Language => language ?? string.Empty;

        public string Script => script ?? string.Empty;

        public string Country => country ?? string.Empty;

        public string Variant => variant ?? string.Empty;

        public bool HasLanguage => !string.IsNullOrEmpty(language);

        public bool HasScript => !string.IsNullOrEmpty(script);

        public bool HasCountry => !string.IsNullOrEmpty(country);

        public bool HasVariant => !string.IsNullOrEmpty(variant);

        public bool IsInvariantCulture => 
            string.IsNullOrEmpty(language) || language.Equals("root") || language.Equals("any") &&
            string.IsNullOrEmpty(script) &&
            string.IsNullOrEmpty(country) &&
            string.IsNullOrEmpty(variant);

        public bool IsNeutralCulture => HasLanguage && !HasCountry && !HasVariant;

        public bool Equals(LocaleID other)
        {
            return Language.Equals(other.Language) &&
               Script.Equals(other.Script) &&
               Country.Equals(other.Country) &&
               Variant.Equals(other.Variant);
        }

        public override bool Equals(object obj)
        {
            if (obj is LocaleID other)
                return Equals(other);

            return false;
        }

        public override int GetHashCode()
        {
            return Language.GetHashCode() ^
                Script.GetHashCode() ^
                Country.GetHashCode() ^
                Variant.GetHashCode();
        }

        public static bool operator ==(LocaleID left, LocaleID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LocaleID left, LocaleID right)
        {
            return !(left == right);
        }
    }
}
