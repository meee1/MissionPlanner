using System;
using CircularBuffer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MissionPlanner.Tests.Unit.Collections
{
    /// <summary>
    /// Tests for the generic ring buffer (ExtLibs/Utilities/CircularBuffer.cs)
    /// used to buffer serial/telemetry byte streams: FIFO order, wrap-around,
    /// capacity growth, and overflow semantics.
    /// </summary>
    [TestClass]
    public class CircularBufferTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void NewBuffer_IsEmpty()
        {
            var b = new CircularBuffer<int>(4);
            Assert.AreEqual(4, b.Capacity);
            Assert.AreEqual(0, b.Size);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Put_Then_Get_PreservesFifoOrder()
        {
            var b = new CircularBuffer<int>(4);
            b.Put(1); b.Put(2); b.Put(3);
            Assert.AreEqual(3, b.Size);
            Assert.AreEqual(1, b.Get());
            Assert.AreEqual(2, b.Get());
            Assert.AreEqual(3, b.Get());
            Assert.AreEqual(0, b.Size);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void PutArray_GetArray_RoundTrips()
        {
            var b = new CircularBuffer<int>(8);
            b.Put(new[] { 1, 2, 3, 4 });
            Assert.AreEqual(4, b.Size);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, b.Get(4));
            Assert.AreEqual(0, b.Size);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void WrapAround_MaintainsOrder()
        {
            // Drain part of the buffer then refill past the end so tail wraps.
            var b = new CircularBuffer<int>(4);
            b.Put(new[] { 1, 2, 3 });
            b.Get(2);                  // consume 1,2 ; head advances
            b.Put(new[] { 4, 5, 6 });  // tail wraps around the end
            CollectionAssert.AreEqual(new[] { 3, 4, 5, 6 }, b.ToArray());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Put_WhenFull_ThrowsIfOverflowDisallowed()
        {
            var b = new CircularBuffer<int>(2); // AllowOverflow defaults to false
            b.Put(1);
            b.Put(2);
            Assert.ThrowsException<InvalidOperationException>(() => b.Put(3));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Put_WhenFull_DoesNotThrowIfOverflowAllowed()
        {
            var b = new CircularBuffer<int>(2, allowOverflow: true);
            b.Put(new[] { 1, 2, 3 }); // overwrites, no exception
            Assert.AreEqual(2, b.Size);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Get_OnEmpty_Throws()
        {
            var b = new CircularBuffer<int>(2);
            Assert.ThrowsException<InvalidOperationException>(() => b.Get());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Contains_FindsBufferedItems()
        {
            var b = new CircularBuffer<int>(4);
            b.Put(new[] { 1, 2, 3 });
            Assert.IsTrue(b.Contains(2));
            Assert.IsFalse(b.Contains(99));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Clear_ResetsToEmpty()
        {
            var b = new CircularBuffer<int>(4);
            b.Put(new[] { 1, 2 });
            b.Clear();
            Assert.AreEqual(0, b.Size);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void IncreasingCapacity_PreservesContents()
        {
            var b = new CircularBuffer<int>(2);
            b.Put(new[] { 1, 2 });
            b.Capacity = 4;
            Assert.AreEqual(4, b.Capacity);
            CollectionAssert.AreEqual(new[] { 1, 2 }, b.ToArray());
        }
    }
}
