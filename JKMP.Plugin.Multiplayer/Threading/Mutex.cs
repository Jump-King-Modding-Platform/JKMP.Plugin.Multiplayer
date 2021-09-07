using System;
using System.Threading;
using System.Threading.Tasks;

namespace JKMP.Plugin.Multiplayer.Threading
{
    /// <summary>
    /// A mutex that holds a value. Exclusive access to value can be guaranteed by calling Lock/LockAsync.
    /// Make sure to dispose of the guard when you're done using it or the next call to Lock/LockAsync will be locked indefinitely.
    /// </summary>
    public class Mutex<T>
    {
        private T value;
        private readonly SemaphoreSlim semaphore;
        
        public Mutex(T value)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value));
            semaphore = new(initialCount: 1, maxCount: 1);
        }

        public async Task<MutexGuard<T>> LockAsync()
        {
            await semaphore.WaitAsync();
            return new MutexGuard<T>(semaphore, value);
        }

        public MutexGuard<T> Lock()
        {
            semaphore.Wait();
            return new MutexGuard<T>(semaphore, value);
        }

        public async Task SetValueAsync(T newValue)
        {
            if (newValue == null) throw new ArgumentNullException(nameof(newValue));
            await semaphore.WaitAsync().ConfigureAwait(false);
            
            try
            {
                value = newValue;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void SetValue(T newValue)
        {
            if (newValue == null) throw new ArgumentNullException(nameof(newValue));
            semaphore.Wait();

            try
            {
                value = newValue;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}