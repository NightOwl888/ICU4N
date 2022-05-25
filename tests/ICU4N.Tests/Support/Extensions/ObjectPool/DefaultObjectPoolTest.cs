// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using NUnit.Framework;

namespace ICU4N.Extensions.ObjectPool
{
    public class DefaultObjectPoolTest
    {
        [Test]
        public void DefaultObjectPoolWithDefaultPolicy_GetAnd_ReturnObject_SameInstance()
        {
            // Arrange
            var pool = new DefaultObjectPool<object>(new DefaultPooledObjectPolicy<object>());

            var obj1 = pool.Get();
            pool.Return(obj1);

            // Act
            var obj2 = pool.Get();

            // Assert
            Assert.AreSame(obj1, obj2);
        }

        [Test]
        public void DefaultObjectPool_GetAndReturnObject_SameInstance()
        {
            // Arrange
            var pool = new DefaultObjectPool<List<int>>(new ListPolicy());

            var list1 = pool.Get();
            pool.Return(list1);

            // Act
            var list2 = pool.Get();

            // Assert
            Assert.AreSame(list1, list2);
        }

        [Test]
        public void DefaultObjectPool_CreatedByPolicy()
        {
            // Arrange
            var pool = new DefaultObjectPool<List<int>>(new ListPolicy());

            // Act
            var list = pool.Get();

            // Assert
            Assert.AreEqual(17, list.Capacity);
        }

        [Test]
        public void DefaultObjectPool_Return_RejectedByPolicy()
        {
            // Arrange
            var pool = new DefaultObjectPool<List<int>>(new ListPolicy());
            var list1 = pool.Get();
            list1.Capacity = 20;

            // Act
            pool.Return(list1);
            var list2 = pool.Get();

            // Assert
            Assert.AreNotSame(list1, list2);
        }

        private class ListPolicy : IPooledObjectPolicy<List<int>>
        {
            public List<int> Create()
            {
                return new List<int>(17);
            }

            public bool Return(List<int> obj)
            {
                return obj.Capacity == 17;
            }
        }
    }
}