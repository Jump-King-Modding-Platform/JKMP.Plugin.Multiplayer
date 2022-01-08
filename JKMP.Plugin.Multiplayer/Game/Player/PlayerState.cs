namespace JKMP.Plugin.Multiplayer.Game.Player
{
    public enum PlayerState : byte
    {
        StartJump,
        Jump,
        Falling,
        Land,
        Knocked,
        Splat,
        Walk,
        Idle,
    }
}