namespace Matchmaking.Client.Networking
{
    internal enum MessageType
    {
        HandshakeRequest,
        HandshakeResponse,
        PositionUpdate,
        SetMatchmakingPassword,
        InformNearbyClients,
        /// <summary>Incoming chat message from a client (to server)</summary>
        IncomingChatMessage,
        /// <summary>Outgoing chat message from server (to client)</summary>
        OutgoingChatMessage,
        ServerStatusUpdate,
    }
}