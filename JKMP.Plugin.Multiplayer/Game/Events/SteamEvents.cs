using System;

namespace JKMP.Plugin.Multiplayer.Game.Events
{
    public static class SteamEvents
    {
        #region SteamInitialized

        public delegate void SteamInitializedEventHandler(SteamInitializedEventArgs args);

        public static event SteamInitializedEventHandler? SteamInitialized;

        public class SteamInitializedEventArgs : EventArgs
        {
            /// <summary>
            /// Gets or sets the success value. If this is set to false the original method will also return false (effectively shutting the game down).
            /// </summary>
            public bool Success { get; set; }

            public SteamInitializedEventArgs(bool success)
            {
                Success = success;
            }
        }

        internal static void OnSteamInitialized(SteamInitializedEventArgs args)
        {
            SteamInitialized?.Invoke(args);
        }

        #endregion
    }
}