namespace JKMP.Plugin.Multiplayer.Networking
{
    public enum MessageType
    {
        HandshakeRequest,
        HandshakeResponse,
        PlayerStateChanged,
        LocalChatMessage,
        Disconnected,
    }
}