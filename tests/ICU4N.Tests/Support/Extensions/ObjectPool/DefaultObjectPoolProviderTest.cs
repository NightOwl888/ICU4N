// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using NUnit.Framework;

namespace ICU4N.Extensions.ObjectPool
{
    public class DefaultObjectPoolProviderTest
    {
        [Test]
        public void DefaultObjectPoolProvider_CreateForObject_DefaultObjectPoolReturned()
        {
            // Arrange
            var provider = new DefaultObjectPoolProvider();

            // Act
            var pool = provider.Create<object>();

            // Assert
            Assert.IsTrue(pool is DefaultObjectPool<object>);
        }

        [Test]
        public void DefaultObjectPoolProvider_CreateForIDisposable_DisposableObjectPoolReturned()
        {
            // Arrange
            var provider = new DefaultObjectPoolProvider();

            // Act
            var pool = provider.Create<DisposableObject>();

            // Assert
            Assert.IsTrue(pool is DefaultObjectPool<DisposableObject>);
        }

        private class DisposableObject : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }
    }
}