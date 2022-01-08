using System;
using System.Collections.ObjectModel;
using System.Reflection;
using JumpKing;
using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.TypeResolvers;

namespace JKMP.Plugin.Multiplayer.Game.UI
{
    internal static class UIManager
    {
        /// <summary>
        /// The type resolver to use when loading MML. It contains custom widget that this project implements.
        /// </summary>
        public static readonly AssemblyTypeResolver TypeResolver;

        static UIManager()
        {
            TypeResolver = new AssemblyTypeResolver();
            TypeResolver.AddAssembly(Assembly.GetExecutingAssembly(), "JKMP.Plugin.Multiplayer.Game.UI.Widgets");
        }
        
        public static ReadOnlyObservableCollection<Widget> Widgets { get; private set; } = null!;
        private static Desktop desktop = null!;
        
        private static int cursorShowCount;

        internal static void Initialize()
        {
            // Initialize myra
            MyraEnvironment.Game = Game1.instance;

            desktop = new Desktop
            {
                HasExternalTextInput = true
            };
            Game1.instance.Window.TextInput += (_, args) =>
            {
                desktop.OnChar(args.Character);
            };
            
            Widgets = new ReadOnlyObservableCollection<Widget>(desktop.Widgets);
        }

        public static Widget AddWidget(Widget widget)
        {
            desktop.Widgets.Add(widget);
            return widget;
        }

        public static T AddWidget<T>(T widget) where T : Widget
        {
            desktop.Widgets.Add(widget);
            return widget;
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