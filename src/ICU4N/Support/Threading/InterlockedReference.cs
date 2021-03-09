using ICU4N.Globalization;
using System;
using System.Globalization;

namespace ICU4N.Support.Threading
{
    internal sealed class CultureInfoTracker
    {
        private UCultureInfo converted;
        private CultureInfo tracked;
        private int trackedHashCode;
        private readonly object syncLock = new object();

        public UCultureInfo ConvertToUCulture(CultureInfo culture)
        {
            if (culture is null)
                throw new ArgumentNullException(nameof(culture));

            lock (syncLock)
            {
                if (tracked is null)
                {
                    Convert(culture);
                    return converted;
                }

                int hashCode = culture.GetHashCode();
                if (trackedHashCode != hashCode)
                {
                    Convert(culture, hashCode);
                }
                else
                {
                    // Hash code passed, but isn't necessarily unique - check Equals to be sure
                    if (!tracked.Equals(culture))
                        Convert(culture, hashCode);

                    // Else don't touch converted
                }

                return converted;
            }
        }

        private void Convert(CultureInfo culture)
        {
            tracked = culture;
            trackedHashCode = culture.GetHashCode();
            converted = culture.ToUCultureInfo();
        }

        private void Convert(CultureInfo culture, int hashCode)
        {
            tracked = culture;
            trackedHashCode = hashCode;
            converted = culture.ToUCultureInfo();
        }
    }
}
