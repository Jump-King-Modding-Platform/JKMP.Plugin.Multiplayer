using System;
using JKMP.Core.Logging;
using Serilog;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Steam
{
    public static class SteamManager
    {
        private static readonly ILogger Logger = LogManager.CreateLogger(typeof(SteamManager));
        
        public static bool InitializeSteam()
        {
            try
            {
                SteamClient.Init(1061090);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "SteamClient.Init failed");
                return false;
            }

            SteamNetworkingUtils.InitRelayNetworkAccess();
            SteamUtil.Initialize();

            return true;
        }
    }
}