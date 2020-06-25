using System;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Allows switching the current thread to a new culture in a using block that will automatically 
    /// return the culture to its previous state upon completion.
    /// <para/>
    /// <see cref="ThreadCultureChange"/> can be used to run arbitrary code within a specific culture without
    /// having to change APIs to pass a culture parameter.
    /// <para/>
    /// <code>
    /// using (var context = new ThreadCultureChange("fr-FR"))
    /// {
    ///     // Execute code in the french culture
    /// }
    /// </code>
    /// <para/>
    /// Unlike <see cref="J2N.Globalization.CultureContext"/>, this class utilizes <see cref="UCultureInfo"/>
    /// instead of <see cref="System.Globalization.CultureInfo"/>.
    /// </summary>
    internal class ThreadCultureChange : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UCultureInfo"/>
        /// based on the culture specified by the <paramref name="cultureName"/> identifier.
        /// </summary>
        /// <param name="cultureName">A predefined <see cref="UCultureInfo"/> name, <see cref="UCultureInfo.Name"/> of an
        /// existing <see cref="UCultureInfo"/>, or Windows-only culture name. name is not case-sensitive. This value will be applied
        /// to the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </param>
        public ThreadCultureChange(string cultureName)
            : this(new UCultureInfo(cultureName), UCultureInfo.CurrentUICulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UCultureInfo"/>
        /// based on the culture specified by the <paramref name="cultureName"/> identifier.
        /// </summary>
        /// <param name="cultureName">A predefined <see cref="UCultureInfo"/> name, <see cref="UCultureInfo.Name"/> of an
        /// existing <see cref="UCultureInfo"/>, or Windows-only culture name. name is not case-sensitive. This value will be applied
        /// to the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </param>
        /// <param name="uiCultureName">A predefined <see cref="UCultureInfo"/> name, <see cref="UCultureInfo.Name"/> of an
        /// existing <see cref="UCultureInfo"/>, or Windows-only culture name. name is not case-sensitive. This value will be applied
        /// to the <see cref="UCultureInfo.CurrentUICulture"/>.</param>
        public ThreadCultureChange(string cultureName, string uiCultureName)
            : this(new UCultureInfo(cultureName), new UCultureInfo(uiCultureName))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UCultureInfo"/>
        /// based on the <see cref="UCultureInfo"/> specified by the <paramref name="culture"/> identifier.
        /// </summary>
        /// <param name="culture">A <see cref="UCultureInfo"/> object. This value will be applied
        /// to the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </param>
        public ThreadCultureChange(UCultureInfo culture)
            : this(culture, UCultureInfo.CurrentUICulture)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UCultureInfo"/>
        /// based on the <see cref="UCultureInfo"/> specified by the <paramref name="culture"/> identifier.
        /// </summary>
        /// <param name="culture">A <see cref="UCultureInfo"/> object. This value will be applied
        /// to the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </param>
        /// <param name="uiCulture">A <see cref="UCultureInfo"/> object. This value will be applied
        /// to the <see cref="UCultureInfo.CurrentUICulture"/>.</param>
        public ThreadCultureChange(UCultureInfo culture, UCultureInfo uiCulture)
        {
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));
            if (uiCulture == null)
                throw new ArgumentNullException(nameof(uiCulture));

            // Record the current culture settings so they can be restored later.
            this.originalCulture = UCultureInfo.CurrentCulture;
            this.originalUICulture = UCultureInfo.CurrentUICulture;

            // Set both the culture and UI culture for this context.
            UCultureInfo.CurrentCulture = culture;
            UCultureInfo.CurrentUICulture = uiCulture;
        }

        private readonly UCultureInfo originalCulture;
        private readonly UCultureInfo originalUICulture;

        /// <summary>
        /// Gets the original <see cref="UCultureInfo.CurrentCulture"/> value that existed on the current
        /// thread when this instance was initialized.
        /// </summary>
        public UCultureInfo OriginalCulture => originalCulture;

        /// <summary>
        /// Gets the original <see cref="UCultureInfo.CurrentUICulture"/> value that existed on the current
        /// thread when this instance was initialized.
        /// </summary>
        public UCultureInfo OriginalUICulture => originalUICulture;

        /// <summary>
        /// Restores the <see cref="UCultureInfo.CurrentCulture"/> and <see cref="UCultureInfo.CurrentUICulture"/> to their
        /// original values, <see cref="OriginalCulture"/> and <see cref="OriginalUICulture"/>, respectively.
        /// </summary>
        public void RestoreOriginalCulture()
        {
            // Restore the culture to the way it was before the constructor was called.
            UCultureInfo.CurrentCulture = originalCulture;
            UCultureInfo.CurrentUICulture = originalUICulture;
        }

        /// <summary>
        /// Restores the <see cref="UCultureInfo.CurrentCulture"/> and <see cref="UCultureInfo.CurrentUICulture"/> to their
        /// original values, <see cref="OriginalCulture"/> and <see cref="OriginalUICulture"/>, respectively.
        /// <para/>
        /// This can be called automatically with a using block to ensure the culture is reset even in the event of an exception.
        /// <code>
        /// using (var context = new ThreadCultureChange("fr-FR"))
        /// {
        ///     // Execute code in the french culture
        /// }
        /// </code>
        /// </summary>
        public void Dispose()
        {
            RestoreOriginalCulture();
        }
    }
}