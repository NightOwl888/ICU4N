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

        [Test]
        public void TestCurrentCulture()
        {
            var newCulture = new UCultureInfo(UCultureInfo.CurrentCulture.Name.Equals("ja_JP", StringComparison.OrdinalIgnoreCase) ? "ar_SA" : "ja_JP");
            using (new ThreadCultureChange(newCulture))
            {
                Assert.AreEqual(UCultureInfo.CurrentCulture, newCulture);
            }

            newCulture = new UCultureInfo("de_DE@collation=phonebook");
            using (new ThreadCultureChange(newCulture))
            {
                Assert.AreEqual(UCultureInfo.CurrentCulture, newCulture);
                Assert.AreEqual("de_DE@collation=phonebook", newCulture.ToString());
            }
        }
        
        [Test] // ICU4N TODO: We need to allow this scenario to reset back to tracking the current culture
        public void TestCurrentCulture_Set_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => UCultureInfo.CurrentCulture = null);
        }

        [Test]
        public void TestCurrentUICulture()
        {
            var newUICulture = new UCultureInfo(UCultureInfo.CurrentUICulture.Name.Equals("ja_JP", StringComparison.OrdinalIgnoreCase) ? "ar_SA" : "ja_JP");
            using (new ThreadCultureChange(UCultureInfo.CurrentCulture, newUICulture))
            {
                Assert.AreEqual(UCultureInfo.CurrentUICulture, newUICulture);
            }

            newUICulture = new UCultureInfo("de_DE@collation=phonebook");
            using (new ThreadCultureChange(UCultureInfo.CurrentCulture, newUICulture))
            {
                Assert.AreEqual(UCultureInfo.CurrentUICulture, newUICulture);
                Assert.AreEqual("de_DE@collation=phonebook", newUICulture.ToString());
            }
        }
    }
}
