﻿using System;
using System.Globalization;
#if !NETSTANDARD
using System.Threading;
#endif

namespace ICU4N.Support.Globalization
{
    /// <summary>
    /// Allows switching the current thread to a new culture in a using block that will automatically 
    /// return the culture to its previous state upon completion.
    /// </summary>
    public sealed class CultureContext : IDisposable
    {
#if !NETSTANDARD
        public CultureContext(int culture)
            : this(new CultureInfo(culture), CultureInfo.CurrentUICulture)
        {
        }

        public CultureContext(int culture, int uiCulture)
            : this(new CultureInfo(culture), new CultureInfo(uiCulture))
        {
        }
#endif

        public CultureContext(string cultureName)
            : this(new CultureInfo(cultureName), CultureInfo.CurrentUICulture)
        {
        }

        public CultureContext(string cultureName, string uiCultureName)
            : this(new CultureInfo(cultureName), new CultureInfo(uiCultureName))
        {
        }

        public CultureContext(CultureInfo culture)
            : this(culture, CultureInfo.CurrentUICulture)
        {
        }

        public CultureContext(CultureInfo culture, CultureInfo uiCulture)
        {
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));
            if (uiCulture == null)
                throw new ArgumentNullException(nameof(uiCulture));
#if !NETSTANDARD
            this.currentThread = Thread.CurrentThread;
#endif

            // Record the current culture settings so they can be restored later.
            this.originalCulture = CultureInfo.CurrentCulture;
            this.originalUICulture = CultureInfo.CurrentUICulture;

            // Set both the culture and UI culture for this context.
#if !NETSTANDARD
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = uiCulture;
#else
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = uiCulture;
#endif
        }

#if !NETSTANDARD
        private readonly Thread currentThread;
#endif
        private readonly CultureInfo originalCulture;
        private readonly CultureInfo originalUICulture;

        public CultureInfo OriginalCulture
        {
            get { return this.originalCulture; }
        }

        public CultureInfo OriginalUICulture
        {
            get { return this.originalUICulture; }
        }

        public void RestoreOriginalCulture()
        {
            // Restore the culture to the way it was before the constructor was called.
#if !NETSTANDARD
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUICulture;
#else
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUICulture;
#endif
        }
        public void Dispose()
        {
            RestoreOriginalCulture();
        }
    }
}
