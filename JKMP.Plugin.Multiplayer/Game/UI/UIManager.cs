using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JKMP.Core.Logging;
using JumpKing;
using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D.UI;

namespace JKMP.Plugin.Multiplayer.Game.UI
{
    internal static class UIManager
    {
        public static ReadOnlyObservableCollection<Widget> Widgets { get; private set; } = null!;
        private static Desktop desktop = null!;
        
        private static int cursorShowCount;

        internal static void Initialize()
        {
            // Initialize myra
            MyraEnvironment.Game = Game1.instance;
            
            desktop = new Desktop();
            Widgets = new ReadOnlyObservableCollection<Widget>(desktop.Widgets);
        }

        public static void AddWidget(Widget widget)
        {
            desktop.Widgets.Add(widget);
        }

        public static bool RemoveWidget(Widget widget)
        {
            return desktop.Widgets.Remove(widget);
        }

        public static void PushShowCursor()
        {
            cursorShowCount += 1;
            UpdateCursorVisibility();
        }

        public static void PopShowCursor()
        {
            if (cursorShowCount == 0)
                throw new InvalidOperationException("Cursor show count is zero");
            
            cursorShowCount -= 1;
            UpdateCursorVisibility();
        }

        private static void UpdateCursorVisibility()
        {
            try
            {
                Game1.instance.IsMouseVisible = cursorShowCount > 0;
            }
            catch (NullReferenceException)
            {
                // Happens if Push or Pop is called when the game is shutting down
            }
        }
        
        public static void Draw(GameTime gameTime)
        {
            desktop.Render();
        }
    }
}