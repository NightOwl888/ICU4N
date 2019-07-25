﻿using System;
using System.Threading;

namespace ICU4N.Support.Threading
{
    /// <summary>
    /// Support class used to handle threads
    /// </summary>
    internal class ThreadWrapper //: IThreadRunnable
    {
        /// <summary>
        /// The instance of System.Threading.Thread
        /// </summary>
        private Thread _threadField;

        /// <summary>
        /// Initializes a new instance of the ThreadClass class
        /// </summary>
        public ThreadWrapper()
        {
            _threadField = new Thread(Run);
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="name">The name of the thread</param>
        public ThreadWrapper(string name)
        {
            _threadField = new Thread(Run);
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
        public ThreadWrapper(ThreadStart start)
        {
            _threadField = new Thread(start);
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
        /// <param name="name">The name of the thread</param>
        public ThreadWrapper(ThreadStart start, string name)
        {
            _threadField = new Thread(start);
            this.Name = name;
        }

        /// <summary>
        /// This method has no functionality unless the method is overridden
        /// </summary>
        public virtual void Run()
        {
        }

        /// <summary>
        /// Causes the operating system to change the state of the current thread instance to ThreadState.Running
        /// </summary>
        public virtual void Start()
        {
            _threadField.Start();
        }

        /// <summary>
        /// Interrupts a thread that is in the WaitSleepJoin thread state
        /// </summary>
        public virtual void Interrupt()
        {
#if !NETSTANDARD1_3
            _threadField.Interrupt();
#endif
        }

        /// <summary>
        /// Gets the current thread instance
        /// </summary>
        public System.Threading.Thread Instance
        {
            get
            {
                return _threadField;
            }
            set
            {
                _threadField = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the thread
        /// </summary>
        public String Name
        {
            get
            {
                return _threadField.Name;
            }
            set
            {
                if (_threadField.Name == null)
                    _threadField.Name = value;
            }
        }

        public void SetDaemon(bool isDaemon)
        {
            _threadField.IsBackground = isDaemon;
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Gets or sets a value indicating the scheduling priority of a thread
        /// </summary>
        public ThreadPriority Priority
        {
            get
            {
                try
                {
                    return _threadField.Priority;
                }
                catch
                {
                    return ThreadPriority.Normal;
                }
            }
            set
            {
                try
                {
                    _threadField.Priority = value;
                }
                catch { }
            }
        }
#endif

        /// <summary>
        /// Gets a value indicating the execution status of the current thread
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return _threadField.IsAlive;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not a thread is a background thread.
        /// </summary>
        public bool IsBackground
        {
            get
            {
                return _threadField.IsBackground;
            }
            set
            {
                _threadField.IsBackground = value;
            }
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates
        /// </summary>
        public void Join()
        {
            _threadField.Join();
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="milliSeconds">Time of wait in milliseconds</param>
        public void Join(long milliSeconds)
        {
            _threadField.Join(Convert.ToInt32(milliSeconds));
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="milliSeconds">Time of wait in milliseconds</param>
        /// <param name="nanoSeconds">Time of wait in nanoseconds</param>
        public void Join(long milliSeconds, int nanoSeconds)
        {
            int totalTime = Convert.ToInt32(milliSeconds + (nanoSeconds * 0.000001));

            _threadField.Join(totalTime);
        }

        /// <summary>
        /// Resumes a thread that has been suspended
        /// </summary>
        public void Resume()
        {
            Monitor.PulseAll(_threadField);
        }

#if !NETSTANDARD1_3

        /// <summary>
        /// Raises a ThreadAbortException in the thread on which it is invoked,
        /// to begin the process of terminating the thread. Calling this method
        /// usually terminates the thread
        /// </summary>
        public void Abort()
        {
            _threadField.Abort();
        }

        /// <summary>
        /// Raises a ThreadAbortException in the thread on which it is invoked,
        /// to begin the process of terminating the thread while also providing
        /// exception information about the thread termination.
        /// Calling this method usually terminates the thread.
        /// </summary>
        /// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted</param>
        public void Abort(object stateInfo)
        {
            _threadField.Abort(stateInfo);
        }
#endif

        /// <summary>
        /// Suspends the thread, if the thread is already suspended it has no effect
        /// </summary>
        public void Suspend()
        {
            Monitor.Wait(_threadField);
        }

        /// <summary>
        /// Obtain a String that represents the current object
        /// </summary>
        /// <returns>A String that represents the current object</returns>
        public override System.String ToString()
        {
#if !NETSTANDARD1_3
            return "Thread[" + Name + "," + Priority.ToString() + "]";
#else
            return "Thread[" + Name + "]";
#endif
        }

        [ThreadStatic]
        private static ThreadWrapper This = null;

        // named as the Java version
        public static ThreadWrapper CurrentThread()
        {
            return Current();
        }

        public static void Sleep(long ms)
        {
            // casting long ms to int ms could lose resolution, however unlikely
            // that someone would want to sleep for that long...
            Thread.Sleep((int)ms);
        }

        /// <summary>
        /// Gets the currently running thread
        /// </summary>
        /// <returns>The currently running thread</returns>
        public static ThreadWrapper Current()
        {
            if (This == null)
            {
                This = new ThreadWrapper();
                This.Instance = Thread.CurrentThread;
            }
            return This;
        }

        public static bool operator ==(ThreadWrapper t1, object t2)
        {
            if (((object)t1) == null) return t2 == null;
            return t1.Equals(t2);
        }

        public static bool operator !=(ThreadWrapper t1, object t2)
        {
            return !(t1 == t2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ThreadWrapper) return this._threadField.Equals(((ThreadWrapper)obj)._threadField);
            return false;
        }

        public override int GetHashCode()
        {
            return this._threadField.GetHashCode();
        }

        public ThreadState State
        {
            get { return _threadField.ThreadState; }
        }
    }
}
