using System;
using System.Reflection;
using HarmonyLib;

namespace JKMP.Plugin.Multiplayer
{
    // ReSharper disable once UnusedType.Global
    public class MultiplayerPlugin : Core.Plugins.Plugin
    {
        private readonly Harmony harmony = new("com.jkmp.plugin.multiplayer");
        
        public MultiplayerPlugin()
        {
        }

        public override void Initialize()
        {
            harmony.PatchAll(typeof(MultiplayerPlugin).Assembly);
        }
    }
}