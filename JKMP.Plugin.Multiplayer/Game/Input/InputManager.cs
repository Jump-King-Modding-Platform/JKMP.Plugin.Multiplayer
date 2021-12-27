using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JKMP.Plugin.Multiplayer.Game.Input
{
    internal static class InputManager
    {
        /// <summary>
        /// Returns true if game input is enabled. If not the character will not be able to move.
        /// </summary>
        public static bool GameInputEnabled => gameInputDisabledCount == 0;

        private static KeyboardState keyboardState;
        private static KeyboardState lastKeyboardState;
        private static int gameInputDisabledCount;

        private static readonly HashSet<Keys> PressedKeysSet = new();
        private static readonly HashSet<Keys> ReleasedKeysSet = new();

        public static void Update(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();

            var pressedKeys = keyboardState.GetPressedKeys();
            var lastPressedKeys = lastKeyboardState.GetPressedKeys();

            PressedKeysSet.Clear();
            ReleasedKeysSet.Clear();
            
            foreach (var key in pressedKeys)
            {
                if (!lastPressedKeys.Contains(key))
                    PressedKeysSet.Add(key);
            }
            
            foreach (var key in lastPressedKeys)
            {
                if (!pressedKeys.Contains(key))
                    ReleasedKeysSet.Add(key);
            }

            lastKeyboardState = keyboardState;
        }
        
        public static bool KeyJustPressed(Keys key)
        {
            return PressedKeysSet.Contains(key);
        }
        
        public static bool KeyJustReleased(Keys key)
        {
            return ReleasedKeysSet.Contains(key);
        }
        
        public static bool KeyDown(Keys key)
        {
            return keyboardState.IsKeyDown(key);
        }
        
        public static bool KeyUp(Keys key)
        {
            return keyboardState.IsKeyUp(key);
        }

        public static void DisableGameInput()
        {
            ++gameInputDisabledCount;
        }

        public static void EnableGameInput()
        {
            if (gameInputDisabledCount <= 0)
                throw new InvalidOperationException("Game input is already enabled.");
            
            --gameInputDisabledCount;
        }
    }
}