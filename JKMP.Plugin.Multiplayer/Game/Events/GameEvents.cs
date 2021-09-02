using System;

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
    }
}