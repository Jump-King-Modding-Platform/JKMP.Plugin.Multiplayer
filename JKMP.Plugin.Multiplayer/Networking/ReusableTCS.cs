using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    /// <summary>
    /// A reusable task completion source
    /// </summary>
    internal class ReusableTCS<T> : INotifyCompletion
    {
        private bool isCompleted;
        private T? result;

        public bool IsCompleted => isCompleted;

        private Action? onCompleted;
        
        public void OnCompleted(Action continuation)
        {
            if (onCompleted != null)
                throw new NotSupportedException("Multiple awaiters is not supported");
            
            onCompleted = continuation;
        }

        public void Reset()
        {
            isCompleted = false;
            result = default;
            onCompleted = null;
        }

        public async Task SetResult(T? result)
        {
            if (isCompleted)
                throw new InvalidOperationException("Result is already set. Make sure to call Reset before calling SetResult again.");
            
            this.result = result;
            isCompleted = true;

            if (onCompleted != null)
                await Task.Run(onCompleted).ConfigureAwait(false);
        }

        public ReusableTCS<T> GetAwaiter() => this;

        public T? GetResult() => result;
    }
}