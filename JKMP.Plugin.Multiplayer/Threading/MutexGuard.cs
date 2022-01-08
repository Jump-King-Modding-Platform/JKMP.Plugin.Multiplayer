using System;
using System.Threading;

namespace JKMP.Plugin.Multiplayer.Threading
{
    public struct MutexGuard<T> : IDisposable
    {
        public T Value
        {
            get
            {
                if (!valid)
                    throw new InvalidOperationException("Tried to access value from a disposed MutexGuard");

                return value;
            }
        }

        public bool IsValid => valid;

        private readonly T value;
        private readonly SemaphoreSlim semaphore;
        private bool valid;

        public MutexGuard(SemaphoreSlim semaphore, T value)
        {
            this.semaphore = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
            this.value = value ?? throw new ArgumentNullException(nameof(value));
            valid = true;
        }

        public void Dispose()
        {
            if (!valid)
                return;

            try
            {
                semaphore.Release();
                valid = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to release semaphore: {ex}");
                throw;
            }
        }
    }
}