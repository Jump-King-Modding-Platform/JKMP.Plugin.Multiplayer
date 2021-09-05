namespace Matchmaking.Client.Networking
{
    public enum MessageType
    {
        HandshakeRequest,
        HandshakeResponse,
        PositionUpdate,
        SetMatchmakingPassword,
        InformNearbyClients,
    }
}