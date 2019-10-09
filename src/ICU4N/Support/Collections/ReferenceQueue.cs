using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ICU4N.Support.Collections
{
    /// <summary>
    /// The <see cref="ReferenceQueue{T}"/> is the container on which reference objects are
    /// enqueued when the garbage collector detects the reachability type specified
    /// for the referent.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ReferenceQueue<T> where T : class
    {
        private const int DEFAULT_QUEUE_SIZE = 128;

        private Reference<T>[] references;

        private int head;

        private int tail;

        private bool empty;

        /**
         * Constructs a new instance of this class.
         */
        public ReferenceQueue()
            : base()
        {
            references = NewArray(DEFAULT_QUEUE_SIZE);
            head = 0;
            tail = 0;
            empty = true;
        }


        private Reference<T>[] NewArray(int size)
        {
            return new Reference<T>[size];
        }

        /**
         * Returns the next available reference from the queue, removing it in the
         * process. Does not wait for a reference to become available.
         *
         * @return the next available reference, or {@code null} if no reference is
         *         immediately available
         */
        public Reference<T> Poll()
        {
            Reference<T> reference;
            lock (this)
            {
                if (empty)
                {
                    return null;
                }
                reference = references[head++];
                reference.Dequeue(); 
                if (head == references.Length)
                {
                    head = 0;
                }
                if (head == tail)
                {
                    empty = true;
                }
            }
            return reference;
        }

        /**
         * Returns the next available reference from the queue, removing it in the
         * process. Waits indefinitely for a reference to become available.
         *
         * @return the next available reference
         *
         * @throws InterruptedException
         *             if the blocking call was interrupted for some reason
         */
        public Reference<T> Remove()
        {
            return Remove(0);
        }

        /**
         * Returns the next available reference from the queue, removing it in the
         * process. Waits for a reference to become available or the given timeout
         * period to elapse, whichever happens first.
         *
         * @param timeout
         *            maximum time (in ms) to spend waiting for a reference object
         *            to become available. A value of zero results in the method
         *            waiting indefinitely.
         * @return the next available reference, or {@code null} if no reference
         *         becomes available within the timeout period
         * @throws IllegalArgumentException
         *             if the wait period is negative.
         * @throws InterruptedException
         *             if the blocking call was interrupted for some reason
         */
        public Reference<T> Remove(int timeout)
        {
            if (timeout < 0)
            {
                throw new ArgumentException();
            }

            Reference<T> reference;
            lock (this)
            {
                if (empty)
                {
                    Monitor.Wait(this, timeout);
                    if (empty)
                    {
                        return null;
                    }
                }
                reference = references[head++];
                reference.Dequeue();
                if (head == references.Length)
                {
                    head = 0;
                }
                if (head == tail)
                {
                    empty = true;
                }
                else
                {
                    Monitor.PulseAll(this);
                }
            }
            return reference;
        }

        /**
         * Enqueue the reference object on the receiver.
         *
         * @param reference
         *            reference object to be enqueued.
         * @return boolean true if reference is enqueued. false if reference failed
         *         to enqueue.
         */
        internal bool Enqueue(Reference<T> reference)
        {
            lock (this)
            {
                if (!empty && head == tail)
                {
                    /* Queue is full - grow */
                    int newQueueSize = (int)(references.Length * 1.10);
                    Reference<T>[] newQueue = NewArray(newQueueSize);
                    System.Array.Copy(references, head, newQueue, 0, references.Length - head);
                    if (tail > 0)
                    {
                        System.Array.Copy(references, 0, newQueue, references.Length - head, tail);
                    }
                    head = 0;
                    tail = references.Length;
                    references = newQueue;
                }
                references[tail++] = reference;
                if (tail == references.Length)
                {
                    tail = 0;
                }
                empty = false;
                Monitor.PulseAll(this);
            }
            return true;
        }
    }
}
