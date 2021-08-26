namespace JKMP.Plugin.Multiplayer.Game.Events
{
    public static class PlayerEvents
    {
        #region StartJumpCharge

        public delegate void StartJumpEventHandler();

        public static event StartJumpEventHandler? StartJump;

        internal static void OnStartJump()
        {
            StartJump?.Invoke();
        }

        #endregion

        #region Walk



        #endregion

        #region SetSprite
        
        

        #endregion
    }
}