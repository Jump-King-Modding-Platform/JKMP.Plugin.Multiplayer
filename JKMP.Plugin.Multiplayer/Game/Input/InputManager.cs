using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JKMP.Plugin.Multiplayer.Game.Input
{
    internal static class InputManager
    {
        private static KeyboardState keyboardState;
        private static KeyboardState lastKeyboardState;

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
    }
}