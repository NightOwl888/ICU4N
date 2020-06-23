using ICU4N.Impl;
using System;
using System.Globalization;
#if !FEATURE_ASYNCLOCAL
using System.Runtime.Remoting.Messaging;
#endif
using System.Threading;

#nullable enable

namespace ICU4N.Globalization
{
    public partial class UCultureInfo
    {
        //// Get the current user default culture. This one is almost always used, so we create it by default.
        //private static volatile UCultureInfo/*?*/ s_userDefaultCulture;

        //// The culture used in the user interface. This is mostly used to load correct localized resources.
        //private static volatile UCultureInfo/*?*/ s_userDefaultUICulture;

        // These are defaults that we use if a thread has not opted into having an explicit culture
        private static volatile UCultureInfo? s_DefaultThreadCurrentUICulture;
        private static volatile UCultureInfo? s_DefaultThreadCurrentCulture;

#if FEATURE_ASYNCLOCAL
        [ThreadStatic]
        private static UCultureInfo? s_currentThreadCulture;
        [ThreadStatic]
        private static UCultureInfo? s_currentThreadUICulture;

        private static AsyncLocal<UCultureInfo>? s_asyncLocalCurrentCulture;
        private static AsyncLocal<UCultureInfo>? s_asyncLocalCurrentUICulture;

        private static void AsyncLocalSetCurrentCulture(AsyncLocalValueChangedArgs<UCultureInfo> args)
        {
            s_currentThreadCulture = args.CurrentValue;
        }

        private static void AsyncLocalSetCurrentUICulture(AsyncLocalValueChangedArgs<UCultureInfo> args)
        {
            s_currentThreadUICulture = args.CurrentValue;
        }
#else
        private const string CurrentCultureLogicalCallContextName = "_icu_CurrentThreadCulture";
        private const string CurrentUICultureLogicalCallContextName = "_icu_CurrentThreadUICulture";
#endif

        //private static UCultureInfo InitializeUserDefaultCulture()
        //{
        //    Interlocked.CompareExchange(ref s_userDefaultCulture, GetUserDefaultCulture(), null);
        //    return s_userDefaultCulture!;
        //}

        //private static UCultureInfo InitializeUserDefaultUICulture()
        //{
        //    Interlocked.CompareExchange(ref s_userDefaultUICulture, GetUserDefaultUICulture(), null);
        //    return s_userDefaultUICulture!;
        //}

        //internal static UCultureInfo GetUserDefaultCulture()
        //{
        //    // Native call
        //}

        //private static UCultureInfo GetUserDefaultUICulture()
        //{
        //    return CultureInfo.InstalledUICulture.ToUCultureInfo();
        //}

        internal static UCultureInfo GetCurrentCulture()
        {
            return CultureInfo.CurrentCulture.ToUCultureInfo();
        }

        private static UCultureInfo GetCurrentUICulture()
        {
            return CultureInfo.CurrentUICulture.ToUCultureInfo();
        }

        // This is used to get the base name without doing a call to ToUCultureInfo().
        // Doing so would lead to infinite recursion.
        internal static string CurrentCultureBaseName
        {
            get
            {
                UCultureInfo? current =
#if FEATURE_ASYNCLOCAL
                    s_currentThreadCulture ??
#else
                    CallContext.LogicalGetData(CurrentCultureLogicalCallContextName) as UCultureInfo ??
#endif
                        s_DefaultThreadCurrentCulture;

                if (current != null)
                    return current.Name;
                else
                    return new LocaleIDParser(CultureInfo.CurrentCulture.Name).GetBaseName();
            }
        }

        internal static string CurrentCultureFullName
        {
            get
            {
                UCultureInfo? current =
#if FEATURE_ASYNCLOCAL
                    s_currentThreadCulture ??
#else
                    CallContext.LogicalGetData(CurrentCultureLogicalCallContextName) as UCultureInfo ??
#endif
                        s_DefaultThreadCurrentCulture;

                if (current != null)
                    return current.FullName;
                else
                    return new LocaleIDParser(CultureInfo.CurrentCulture.Name).GetFullName();
            }
        }

        // For now, we are simply tracking the current culture of the .NET platform and
        // converting it to UCultureInfo, unless CurrentCulture or DefaultThreadCurrentCulture is explicitly
        // set, which will override the default tracking behavior.
        public new static UCultureInfo CurrentCulture
        {
            get
            {
#if FEATURE_ASYNCLOCAL
                return s_currentThreadCulture ??
#else
                return CallContext.LogicalGetData(CurrentCultureLogicalCallContextName) as UCultureInfo ??
#endif
                    s_DefaultThreadCurrentCulture ??
                    //s_userDefaultCulture ??
                    //InitializeUserDefaultCulture();
                    GetCurrentCulture();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

#if FEATURE_ASYNCLOCAL
                if (s_asyncLocalCurrentCulture == null)
                {
                    Interlocked.CompareExchange(ref s_asyncLocalCurrentCulture, new AsyncLocal<UCultureInfo>(AsyncLocalSetCurrentCulture), null);
                }
                s_asyncLocalCurrentCulture!.Value = value;
#else
                CallContext.LogicalSetData(CurrentCultureLogicalCallContextName, value);
#endif
            }
        }

        // For now, we are simply tracking the current UI culture of the .NET platform and
        // converting it to UCultureInfo, unless CurrentCulture or DefaultThreadCurrentUICulture is explicitly
        // set, which will override the default tracking behavior.
        public new static UCultureInfo CurrentUICulture
        {
            get
            {
#if FEATURE_ASYNCLOCAL
                return s_currentThreadUICulture ??
#else
                return CallContext.LogicalGetData(CurrentUICultureLogicalCallContextName) as UCultureInfo ??
#endif
                    s_DefaultThreadCurrentUICulture ??
                    //s_userDefaultUICulture ??
                    //InitializeUserDefaultUICulture();
                    GetCurrentUICulture();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                //CultureInfo.VerifyCultureName(value, true);

#if FEATURE_ASYNCLOCAL
                if (s_asyncLocalCurrentUICulture == null)
                {
                    Interlocked.CompareExchange(ref s_asyncLocalCurrentUICulture, new AsyncLocal<UCultureInfo>(AsyncLocalSetCurrentUICulture), null);
                }

                // this one will set s_currentThreadUICulture too
                s_asyncLocalCurrentUICulture!.Value = value;
#else
                CallContext.LogicalSetData(CurrentUICultureLogicalCallContextName, value);
#endif
            }
        }

        public new static UCultureInfo? DefaultThreadCurrentCulture
        {
            get => s_DefaultThreadCurrentCulture;
            set =>
                // If you add pre-conditions to this method, check to see if you also need to
                // add them to Thread.CurrentCulture.set.
                s_DefaultThreadCurrentCulture = value;
        }

        public new static UCultureInfo? DefaultThreadCurrentUICulture
        {
            get => s_DefaultThreadCurrentUICulture;
            set
            {
                // If they're trying to use a Culture with a name that we can't use in resource lookup,
                // don't even let them set it on the thread.

                // If you add more pre-conditions to this method, check to see if you also need to
                // add them to Thread.CurrentUICulture.set.

                //if (value != null)
                //{
                //    CultureInfo.VerifyCultureName(value, true);
                //}

                s_DefaultThreadCurrentUICulture = value;
            }
        }
    }
}
