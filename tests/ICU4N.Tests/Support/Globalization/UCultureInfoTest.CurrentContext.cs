using ICU4N.Dev.Test;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ICU4N.Globalization
{
    public class UCultureInfoTest_CurrentContext : TestFmwk
    {
        [Test]
        public void TestCurrentCulturesAsync()
        {
            var newCurrentCulture = new UCultureInfo(UCultureInfo.CurrentCulture.Name.Equals("ja-JP", StringComparison.OrdinalIgnoreCase) ? "en-US" : "ja-JP");
            var newCurrentUICulture = new UCultureInfo(UCultureInfo.CurrentUICulture.Name.Equals("ja-JP", StringComparison.OrdinalIgnoreCase) ? "en-US" : "ja-JP");
            using (new ThreadCultureChange(newCurrentCulture, newCurrentUICulture))
            {
#if FEATURE_TASK_RUN
                Task t = Task.Run(() =>
#else
                Task t = Task.Factory.StartNew(() =>
#endif
                {
                    Assert.AreEqual(UCultureInfo.CurrentCulture, newCurrentCulture);
                    Assert.AreEqual(UCultureInfo.CurrentUICulture, newCurrentUICulture);
#if FEATURE_TASK_RUN
                });
#else
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
#endif

                ((IAsyncResult)t).AsyncWaitHandle.WaitOne();
                t.Wait();
            }
        }

#if FEATURE_TASK_ASYNC_AWAIT
        [Test]
        public void TestCurrentCulturesWithAwait()
        {
            var newCurrentCulture = new UCultureInfo(UCultureInfo.CurrentCulture.Name.Equals("ja-JP", StringComparison.OrdinalIgnoreCase) ? "en-US" : "ja-JP");
            var newCurrentUICulture = new UCultureInfo(UCultureInfo.CurrentUICulture.Name.Equals("ja-JP", StringComparison.OrdinalIgnoreCase) ? "en-US" : "ja-JP");
            using (new ThreadCultureChange(newCurrentCulture, newCurrentUICulture))
            {
                MainAsync().Wait();

                async Task MainAsync()
                {
                    await Task.Delay(1).ConfigureAwait(false);

                    Assert.AreEqual(UCultureInfo.CurrentCulture, newCurrentCulture);
                    Assert.AreEqual(UCultureInfo.CurrentUICulture, newCurrentUICulture);
                }
            }
        }
#endif

        private class ThreadCultureChange : IDisposable
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
            /// using (var context = new CultureContext("fr-FR"))
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
}
