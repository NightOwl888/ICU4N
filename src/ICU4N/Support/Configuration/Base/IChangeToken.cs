// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !FEATURE_MICROSOFT_EXTENSIONS_CONFIGURATION
using System;

namespace Microsoft.Extensions.Primitives
{
    interface IChangeToken
    {
        //
        // Summary:
        //     Gets a value that indicates if a change has occured.
        bool HasChanged { get; }
        //
        // Summary:
        //     Indicates if this token will pro-actively raise callbacks. Callbacks are still
        //     guaranteed to fire, eventually.
        bool ActiveChangeCallbacks { get; }

        //
        // Summary:
        //     Registers for a callback that will be invoked when the entry has changed. Microsoft.Extensions.Primitives.IChangeToken.HasChanged
        //     MUST be set before the callback is invoked.
        //
        // Parameters:
        //   callback:
        //     The System.Action`1 to invoke.
        //
        //   state:
        //     State to be passed into the callback.
        //
        // Returns:
        //     An System.IDisposable that is used to unregister the callback.
        IDisposable RegisterChangeCallback(Action<object> callback, object state);
    }
}
#endif
