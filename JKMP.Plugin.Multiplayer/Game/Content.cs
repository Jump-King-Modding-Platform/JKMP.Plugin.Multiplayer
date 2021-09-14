using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace JKMP.Plugin.Multiplayer.Game
{
    internal static class Content
    {
        public enum SurfaceType
        {
            Default,
            Snow,
            Iron,
            Water,
            Sand,
            Ice
        }

        public class PlayerSoundEffects
        {
            public readonly SoundEffect Jump;
            public readonly SoundEffect Land;
            public readonly SoundEffect Bump;
            public readonly SoundEffect Splat;

            public PlayerSoundEffects(SoundEffect jump, SoundEffect land, SoundEffect bump, SoundEffect splat)
            {
                Jump = jump ?? throw new ArgumentNullException(nameof(jump));
                Land = land ?? throw new ArgumentNullException(nameof(land));
                Bump = bump ?? throw new ArgumentNullException(nameof(bump));
                Splat = splat ?? throw new ArgumentNullException(nameof(splat));
            }
        }

        public static Dictionary<SurfaceType, PlayerSoundEffects> PlayerSounds { get; } = new();

        internal static void LoadContent(ContentManager content)
        {
            LoadSounds(content);
        }

        private static void LoadSounds(ContentManager content)
        {
            var defaultJump = LoadPlayerSound(content, "king_jump");
            var defaultLand = LoadPlayerSound(content, "king_land");
            var defaultBump = LoadPlayerSound(content, "king_bump");
            var defaultSplat = LoadPlayerSound(content, "king_splat");

            var iceJump = LoadPlayerSound(content, "king_jump", "ice");
            var iceLand = LoadPlayerSound(content, "king_land", "ice");

            var sandLand = LoadPlayerSound(content, "sand_land", "sand");

            var ironLand = LoadPlayerSound(content, "iron_land", "shoes_iron");
            var ironSplat = LoadPlayerSound(content, "iron_splat", "shoes_iron");

            var snowJump = LoadPlayerSound(content, "king_jump", "snow");
            var snowLand = LoadPlayerSound(content, "king_land", "snow");
            var snowSplat = LoadPlayerSound(content, "king_splat", "snow");

            var waterJump = LoadPlayerSound(content, "water_jump", "water");
            var waterLand = LoadPlayerSound(content, "water_land", "water");
            var waterBump = LoadPlayerSound(content, "water_bump", "water");
            var waterSplat = LoadPlayerSound(content, "water_splat", "water");

            PlayerSounds.Add(SurfaceType.Default, new PlayerSoundEffects(defaultJump, defaultLand, defaultBump, defaultSplat));
            PlayerSounds.Add(SurfaceType.Ice, new PlayerSoundEffects(iceJump, iceLand, defaultBump, defaultSplat));
            PlayerSounds.Add(SurfaceType.Iron, new PlayerSoundEffects(defaultJump, ironLand, defaultBump, ironSplat));
            PlayerSounds.Add(SurfaceType.Sand, new PlayerSoundEffects(defaultJump, sandLand, defaultBump, defaultSplat));
            PlayerSounds.Add(SurfaceType.Snow, new PlayerSoundEffects(snowJump, snowLand, defaultBump, snowSplat));
            PlayerSounds.Add(SurfaceType.Water, new PlayerSoundEffects(waterJump, waterLand, waterBump, waterSplat));
        }

        private static SoundEffect LoadPlayerSound(ContentManager content, string soundName, string? subFolder = null)
            => content.Load<SoundEffect>(GetPlayerSoundPath(soundName, subFolder));

        private static string GetPlayerSoundPath(string soundName, string? subFolder = null)
        {
            string rootDirectory = "audio/jump_king/";

            if (subFolder != null)
                rootDirectory = Path.Combine(rootDirectory, subFolder);
            
            return Path.Combine(rootDirectory, soundName);
        }
    }
}