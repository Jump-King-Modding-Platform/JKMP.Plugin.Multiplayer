using System;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Steam
{
    public static class SteamManager
    {
        public static bool InitializeSteam()
        {
            try
            {
                SteamClient.Init(1061090);
            }
            catch (Exception e)
            {
                Console.WriteLine($"SteamClient.Init failed: {e}");
                return false;
            }

            SteamNetworkingUtils.InitRelayNetworkAccess();

            return true;
        }
    }
}