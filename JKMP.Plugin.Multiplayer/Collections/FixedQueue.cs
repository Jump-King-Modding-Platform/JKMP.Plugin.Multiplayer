using System;
using System.Collections;
using System.Collections.Generic;

namespace JKMP.Plugin.Multiplayer.Collections
{
    /// <summary>
    /// A fixed sized queue that discards the oldest items when full.
    /// </summary>
    public class FixedQueue<T> : IReadOnlyCollection<T>
    {
        public readonly int MaxCount;
        
        public int Count => queue.Count;
        
        private readonly Queue<T> queue = new();
        private readonly object lockObject = new();

        public FixedQueue(int maxCount)
        {
            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCount), "MaxCount must be greater than 0.");
            
            MaxCount = maxCount;
        }

        /// <summary>
        /// Returns true if the item could be added to the queue without discarding the oldest item.
        /// </summary>
        public bool Enqueue(T value)
        {
            bool discarded = false;
            
            queue.Enqueue(value);
            lock (lockObject)
            {
                while (queue.Count > MaxCount)
                {
                    queue.Dequeue();
                    discarded = true;
                }
            }

            return !discarded;
        }

        public T? Dequeue()
        {
            return queue.Dequeue();
        }

        public T? Peek() => queue.Peek();

        public void Clear() => queue.Clear();

        public IEnumerator<T> GetEnumerator() => queue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}