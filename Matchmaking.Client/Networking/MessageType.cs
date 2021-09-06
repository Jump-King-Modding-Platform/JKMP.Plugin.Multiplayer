namespace Matchmaking.Client.Networking
{
    internal enum MessageType
    {
        HandshakeRequest,
        HandshakeResponse,
        PositionUpdate,
        SetMatchmakingPassword,
        InformNearbyClients,
    }
}