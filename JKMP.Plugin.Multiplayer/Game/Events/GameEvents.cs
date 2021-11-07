using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace JKMP.Plugin.Multiplayer.Game.Events
{
    public static class GameEvents
    {
        #region SceneChanged

        public delegate void SceneChangedEventHandler(SceneChangedEventArgs args);

        /// <summary>
        /// Called when the game changes scenes. For example, going from main menu to game, or game back to main menu.
        /// It's also called when the game starts.
        /// </summary>
        public static event SceneChangedEventHandler? SceneChanged;

        public class SceneChangedEventArgs : EventArgs
        {
            public SceneType SceneType { get; }

            public SceneChangedEventArgs(SceneType sceneType)
            {
                SceneType = sceneType;
            }
        }

        internal static void OnSceneChanged(SceneChangedEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            SceneChanged?.Invoke(args);
        }

        #endregion
        
        #region GameStarted

        public delegate void RunStartedEventHandler(RunStartedEventArgs args);

        /// <summary>
        /// Called when the game is started and the player has gained control of the character and is able to move/jump.
        /// It's called for both new and continued games.
        /// </summary>
        public static event RunStartedEventHandler? RunStarted;

        public class RunStartedEventArgs : EventArgs
        {
            internal RunStartedEventArgs()
            {
            }
        }

        internal static void OnRunStarted(RunStartedEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            RunStarted?.Invoke(args);
        }
        
        #endregion
        
        #region GameInitialize

        public delegate void GameInitializeEventHandler();

        public static event GameInitializeEventHandler? GameInitialize;

        internal static void OnGameInitialize()
        {
            GameInitialize?.Invoke();
        }
        
        #endregion
        
        #region GameUpdate

        public delegate void GameUpdateEventHandler(GameTime gameTime);

        public static event GameUpdateEventHandler? GameUpdate;

        internal static void OnGameUpdate(GameTime gameTime)
        {
            GameUpdate?.Invoke(gameTime);
        }
        
        #endregion
        
        #region GameDraw

        public delegate void GameDrawEventHandler(GameTime gameTime);

        public static event GameDrawEventHandler? GameDraw;

        internal static void OnGameDraw(GameTime gameTime)
        {
            GameDraw?.Invoke(gameTime);
        }
        
        #endregion
        
        #region LoadContent

        public delegate void LoadContentEventHandler(ContentManager game);

        public static event LoadContentEventHandler? LoadContent;

        internal static void OnLoadContent(ContentManager content)
        {
            LoadContent?.Invoke(content);
        }

        #endregion
    }
}