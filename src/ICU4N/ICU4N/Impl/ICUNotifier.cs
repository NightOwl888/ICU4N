using ICU4N.Support.Threading;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ICU4N.Impl
{
    /// <summary>
    /// Abstract implementation of a notification facility.
    /// </summary>
    /// <remarks>
    /// Clients add <see cref="IEventListener"/>s with <see cref="AddListener(IEventListener)"/> 
    /// and remove them with <see cref="RemoveListener(IEventListener)"/>.
    /// Notifiers call <see cref="NotifyChanged()"/> when they wish to notify listeners.
    /// This queues the listener list on the notification thread, which
    /// eventually dequeues the list and calls <see cref="NotifyListener(IEventListener)"/> on each
    /// listener in the list.
    /// <para/>
    /// Subclasses override <see cref="AcceptsListener(IEventListener)"/> and <see cref="NotifyListener(IEventListener)"/>
    /// to add type-safe notification.  <see cref="AcceptsListener(IEventListener)"/> should return
    /// true if the listener is of the appropriate type; <see cref="ICUNotifier"/>
    /// itself will ensure the listener is non-null and that the
    /// identical listener is not already registered with the Notifier.
    /// <see cref="NotifyListener(IEventListener)"/> should cast the listener to the appropriate
    /// type and call the appropriate method on the listener.
    /// </remarks>
    public abstract class ICUNotifier 
    {
        private readonly object notifyLock = new object();
        private NotifyThread notifyThread;
        private List<IEventListener> listeners;

        /// <summary>
        /// Add a listener to be notified when <see cref="NotifyChanged()"/> is called.
        /// The listener must not be null. <see cref="AcceptsListener(IEventListener)"/> must return
        /// true for the listener.  Attempts to concurrently
        /// register the identical listener more than once will be
        /// silently ignored.
        /// </summary>
        public virtual void AddListener(IEventListener l)
        {
            if (l == null)
            {
                throw new ArgumentNullException(nameof(l));
            }

            if (AcceptsListener(l))
            {
                lock (notifyLock)
                {
                    if (listeners == null)
                    {
                        listeners = new List<IEventListener>();
                    }
                    else
                    {
                        // identity equality check
                        foreach (IEventListener ll in listeners)
                        {
                            if (ll == l)
                            {
                                return;
                            }
                        }
                    }

                    listeners.Add(l);
                }
            }
            else
            {
                throw new InvalidOperationException("Listener invalid for this notifier.");
            }
        }

        /// <summary>
        /// Stop notifying this listener.  The listener must
        /// not be null.  Attempts to remove a listener that is
        /// not registered will be silently ignored.
        /// </summary>
        public virtual void RemoveListener(IEventListener l)
        {
            if (l == null)
            {
                throw new ArgumentNullException(nameof(l));
            }
            lock (notifyLock)
            {
                if (listeners != null)
                {
                    // identity equality check
                    for (int i = 0; i < listeners.Count; i++)
                    {
                        if (listeners[i] == l)
                        {
                            listeners.RemoveAt(i);
                            if (listeners.Count == 0)
                            {
                                listeners = null;
                            }
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Queue a notification on the notification thread for the current
        /// listeners.  When the thread unqueues the notification, <see cref="NotifyListener(IEventListener)"/>
        /// is called on each listener from the notification thread.
        /// </summary>
        public virtual void NotifyChanged()
        {
            lock (notifyLock)
            {
                if (listeners != null)
                {
                    if (notifyThread == null)
                    {
                        notifyThread = new NotifyThread(this);
                        notifyThread.SetDaemon(true);
                        notifyThread.Start();
                    }
                    notifyThread.Queue(listeners.ToArray());
                }
            }
        }

        /// <summary>
        /// The notification thread.
        /// </summary>
        private class NotifyThread : ThreadWrapper
        {
            private readonly ICUNotifier notifier;
            private readonly List<IEventListener[]> queue = new List<IEventListener[]>();

            internal NotifyThread(ICUNotifier notifier)
            {
                this.notifier = notifier;
            }

            /// <summary>
            /// Queue the notification on the thread.
            /// </summary>
            public virtual void Queue(IEventListener[] list)
            {
                lock (this)
                {
                    queue.Add(list);
                    Monitor.Pulse(this);
                }
            }

            /// <summary>
            /// Wait for a notification to be queued, then notify all
            /// listeners listed in the notification.
            /// </summary>
            public override void Run()
            {
                IEventListener[] list;
                while (true)
                {
#if !NETSTANDARD1_3
                    try
                    {
#endif
                    lock (this)
                    {
                        while (queue.Count == 0)
                        {
                            Monitor.Wait(this);
                        }
                        list = queue[0];
                        queue.RemoveAt(0);
                    }

                    for (int i = 0; i < list.Length; ++i)
                    {
                        notifier.NotifyListener(list[i]);
                    }
#if !NETSTANDARD1_3
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
#endif
                }
            }
        }

        /// <summary>
        /// Subclasses implement this to return true if the listener is
        /// of the appropriate type.
        /// </summary>
        protected abstract bool AcceptsListener(IEventListener l);

        /// <summary>
        /// Subclasses implement this to notify the listener.
        /// </summary>
        protected abstract void NotifyListener(IEventListener l);
    }

    // ICU4N TODO: API Move to support ?
    public interface IEventListener
    {
    }
}
