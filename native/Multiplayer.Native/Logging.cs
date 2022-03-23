using System;
using System.Text;
using Serilog;
using Serilog.Events;

namespace JKMP.Plugin.Multiplayer.Native
{
    public static class Logging
    {
        private static readonly OnLogCallback OnLogCallback;

        static Logging()
        {
            // We need a reference to the callback to be able to call it from native code
            // If passed directly to the native function, it will be garbage collected
            OnLogCallback = OnLog;
        }

        public static void Initialize()
        {
            Bindings.initialize_logging(OnLogCallback);
        }

        private static void OnLog(LogLevel logLevel, Sliceu8 utf8Message)
        {
            string message;
            
            unsafe
            {
                fixed (byte* messagePtr = &utf8Message.ReadOnlySpan.GetPinnableReference())
                {
                    message = Encoding.UTF8.GetString(messagePtr, utf8Message.Count);
                }
            }

            var logEventLevel = (LogEventLevel)logLevel;
            NativeLog.Logger.Write(logEventLevel, (Exception?)null, "{nativeMessage}", message);
        }
    }

    internal static class NativeLog
    {
        public static readonly ILogger Logger = Log.Logger.ForContext(typeof(NativeLog));
    }
}