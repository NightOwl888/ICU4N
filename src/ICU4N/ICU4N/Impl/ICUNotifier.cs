using ICU4N.Support.Threading;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ICU4N.Impl
{
    /// <summary>
    /// Abstract implementation of a notification facility.
    /// </summary>
    // ICU4N TODO: Docs
    public abstract class ICUNotifier //: IObservable<object>
    {
        private readonly object notifyLock = new object();
        private NotifyThread notifyThread;
        private List<EventListener> listeners;

        /**
         * Add a listener to be notified when notifyChanged is called.
         * The listener must not be null. AcceptsListener must return
         * true for the listener.  Attempts to concurrently
         * register the identical listener more than once will be
         * silently ignored.
         */
        public virtual void AddListener(EventListener l)
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
                        listeners = new List<EventListener>();
                    }
                    else
                    {
                        // identity equality check
                        foreach (EventListener ll in listeners)
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

        /**
         * Stop notifying this listener.  The listener must
         * not be null.  Attempts to remove a listener that is
         * not registered will be silently ignored.
         */
        public virtual void RemoveListener(EventListener l)
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

        /**
         * Queue a notification on the notification thread for the current
         * listeners.  When the thread unqueues the notification, notifyListener
         * is called on each listener from the notification thread.
         */
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

        /**
         * The notification thread.
         */
        private class NotifyThread : ThreadWrapper
        {
            private readonly ICUNotifier notifier;
            private readonly List<EventListener[]> queue = new List<EventListener[]>();

            internal NotifyThread(ICUNotifier notifier)
            {
                this.notifier = notifier;
            }

            /**
             * Queue the notification on the thread.
             */
            public void Queue(EventListener[] list)
            {
                lock (this)
                {
                    queue.Add(list);
                    //notify();
                    Monitor.Pulse(this);
                }
            }

            /**
             * Wait for a notification to be queued, then notify all
             * listeners listed in the notification.
             */
            public override void Run()
            {
                EventListener[] list;
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
                            //wait();
                            Monitor.Wait(this);
                        }
                        //list = queue.remove(0);
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

        /**
         * Subclasses implement this to return true if the listener is
         * of the appropriate type.
         */
        protected abstract bool AcceptsListener(EventListener l);

        /**
         * Subclasses implement this to notify the listener.
         */
        protected abstract void NotifyListener(EventListener l);

        public IDisposable Subscribe(IObserver<object> observer)
        {
            var listener = (EventListener)observer;
            this.AddListener(listener);
            return new Unsubscriber(listeners, listener);
        }

        private class Unsubscriber : IDisposable
        {
            private List<EventListener> _observers;
            private EventListener _observer;

            public Unsubscriber(List<EventListener> observers, EventListener observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (!(_observer == null)) _observers.Remove(_observer);
            }
        }
    }

    public abstract class EventListener //: IObserver<object>
    {
        //public virtual void OnCompleted()
        //{
        //    // Not used
        //}

        //public virtual void OnError(Exception error)
        //{
        //    // Not used
        //}

        //public abstract void OnNext(object value);
    }
}
