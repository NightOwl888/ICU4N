using System.Threading;

namespace ICU4N.Impl
{
    /// <summary>
    /// Internal class used to gather statistics on the <see cref="ICUReaderWriterLock"/>.
    /// </summary>
    public sealed class ICUReaderWriterLockStats
    {
        /// <summary>
        /// Number of times read access granted (read count).
        /// </summary>
        public int ReadCount { get; internal set; }

        /// <summary>
        /// Number of times concurrent read access granted (multiple read count).
        /// </summary>
        public int MultipleReadCount { get; internal set; }

        /// <summary>
        /// Number of times blocked for read (waiting reader count).
        /// </summary>
        public int WaitingReadCount { get; internal set; } // wait for read

        /// <summary>
        /// Number of times write access granted (writer count).
        /// </summary>
        public int WriterCount { get; internal set; }

        /// <summary>
        /// Number of times blocked for write (waiting writer count).
        /// </summary>
        public int WaitingWriterCount { get; internal set; }

        internal ICUReaderWriterLockStats()
        {
        }

        internal ICUReaderWriterLockStats(int rc, int mrc, int wrc, int wc, int wwc)
        {
            this.ReadCount = rc;
            this.MultipleReadCount = mrc;
            this.WaitingReadCount = wrc;
            this.WriterCount = wc;
            this.WaitingWriterCount = wwc;
        }

        internal ICUReaderWriterLockStats(ICUReaderWriterLockStats rhs)
            : this(rhs.ReadCount, rhs.MultipleReadCount, rhs.WaitingReadCount, rhs.WriterCount, rhs.WaitingWriterCount)
        {
        }

        /// <summary>
        /// Return a string listing all the stats.
        /// </summary>
        public override string ToString()
        {
            return " rc: " + ReadCount +
                " mrc: " + MultipleReadCount +
                " wrc: " + WaitingReadCount +
                " wc: " + WriterCount +
                " wwc: " + WaitingWriterCount;
        }
    }

    /// <summary>
    /// A Reader/Writer lock originally written for ICU service
    /// implementation. The internal implementation was replaced
    /// with .NET's stock read write lock <see cref="ReaderWriterLockSlim"/>
    /// for ICU 52.
    /// </summary>
    /// <remarks>
    /// This assumes that there will be little writing contention.
    /// It also doesn't allow active readers to acquire and release
    /// a write lock, or deal with priority inversion issues.
    /// <para/>
    /// Access to the lock should be enclosed in a try/finally block
    /// in order to ensure that the lock is always released in case of
    /// exceptions:
    /// <code>
    /// try
    /// {
    ///     lock.AcquireRead();
    ///     // use service protected by the lock
    /// }
    /// finally
    /// {
    ///     lock.ReleaseRead();
    /// }
    /// </code>
    /// <para/>
    /// The lock provides utility methods <see cref="GetStats()"/> and <see cref="ClearStats()"/>
    /// to return statistics on the use of the lock.
    /// </remarks>
    public class ICUReaderWriterLock
    {
        private readonly ReaderWriterLockSlim rwl = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly object syncLock = new object();
        private ICUReaderWriterLockStats stats = null;

        // ICU4N specific - de-nested Stats and renamed ICUReaderWriterLockStats

        ~ICUReaderWriterLock() // ICU4N specific - Added finalizer
        {
            rwl?.Dispose();
        }

        /// <summary>
        /// Reset the stats.  Returns existing stats, if any.
        /// </summary>
        public virtual ICUReaderWriterLockStats ResetStats()
        {
            lock (syncLock)
            {
                ICUReaderWriterLockStats result = stats;
                stats = new ICUReaderWriterLockStats();
                return result;
            }
        }

        /// <summary>
        /// Clear the stats (stop collecting stats).  Returns existing stats, if any.
        /// </summary>
        public virtual ICUReaderWriterLockStats ClearStats()
        {
            lock (syncLock)
            {
                ICUReaderWriterLockStats result = stats;
                stats = null;
                return result;
            }
        }

        /// <summary>
        /// Return a snapshot of the current stats.  This does not reset the stats.
        /// </summary>
        public virtual ICUReaderWriterLockStats GetStats()
        {
            lock (syncLock)
            {
                return stats == null ? null : new ICUReaderWriterLockStats(stats);
            }
        }

        /// <summary>
        /// Acquire a read lock, blocking until a read lock is
        /// available.  Multiple readers can concurrently hold the read
        /// lock.
        /// </summary>
        /// <remarks>
        /// If there's a writer, or a waiting writer, increment the
        /// waiting reader count and block on this.  Otherwise
        /// increment the active reader count and return.  Caller must call
        /// <see cref="ReleaseRead()"/> when done (for example, in a finally block).
        /// </remarks>
        public virtual void AcquireRead()
        {
            if (stats != null)
            {    // stats is null by default
                lock (syncLock)
                {
                    stats.ReadCount++;
                    if (rwl.CurrentReadCount > 0)
                    {
                        stats.MultipleReadCount++;
                    }
                    if (rwl.IsWriteLockHeld)
                    {
                        stats.WaitingReadCount++;
                    }
                }
            }
            rwl.EnterReadLock();
        }

        /// <summary>
        /// Release a read lock and return.  An error will be thrown
        /// if a read lock is not currently held.
        /// </summary>
        /// <remarks>
        /// If this is the last active reader, notify the oldest
        /// waiting writer.  Call when finished with work
        /// controlled by <see cref="AcquireRead()"/>.
        /// </remarks>
        public virtual void ReleaseRead()
        {
            rwl.ExitReadLock();
        }

        /// <summary>
        /// Acquire the write lock, blocking until the write lock is
        /// available.  Only one writer can acquire the write lock, and
        /// when held, no readers can acquire the read lock.
        /// </summary>
        /// <remarks>
        /// If there are no readers and no waiting writers, mark as
        /// having an active writer and return.  Otherwise, add a lock to the
        /// end of the waiting writer list, and block on it.  Caller
        /// must call <see cref="ReleaseWrite()"/> when done (for example, in a finally
        /// block).
        /// </remarks>
        public virtual void AcquireWrite()
        {
            if (stats != null)
            {    // stats is null by default
                lock (syncLock)
                {
                    stats.WriterCount++;
                    if (rwl.CurrentReadCount > 0 || rwl.IsWriteLockHeld)
                    {
                        stats.WaitingWriterCount++;
                    }
                }
            }
            rwl.EnterWriteLock();
        }

        /// <summary>
        /// Release the write lock and return.  An error will be thrown
        /// if the write lock is not currently held.
        /// </summary>
        /// <remarks>
        /// If there are waiting readers, make them all active and
        /// notify all of them.  Otherwise, notify the oldest waiting
        /// writer, if any.  Call when finished with work controlled by
        /// <see cref="AcquireWrite()"/>.
        /// </remarks>
        public virtual void ReleaseWrite()
        {
            rwl.ExitWriteLock();
        }
    }
}
