using System;
using System.Collections.Generic;
using Matchmaking.Client.Chat;
using Matchmaking.Client.EventData;
using Matchmaking.Client.Messages;

namespace Matchmaking.Client
{
    public class Events
    {
        public delegate void NearbyClientsReceivedHandler(ICollection<ulong> steamIds);

        public event NearbyClientsReceivedHandler? NearbyClientsReceived;

        internal void OnNearbyClientsReceived(ICollection<ulong> steamIds)
        {
            if (steamIds == null) throw new ArgumentNullException(nameof(steamIds));
            NearbyClientsReceived?.Invoke(steamIds);
        }

        public delegate void ChatMessageReceivedHandler(ChatMessage message);

        public event ChatMessageReceivedHandler? ChatMessageReceived;

        internal void OnChatMessageReceived(ChatMessage message)
        {
            ChatMessageReceived?.Invoke(message);
        }

        public delegate void ServerStatusUpdateReceivedHandler(ServerStatus status);

        public event ServerStatusUpdateReceivedHandler? ServerStatusUpdateReceived;
        
        internal void OnServerStatusUpdateReceived(ServerStatus status)
        {
            ServerStatusUpdateReceived?.Invoke(status);
        }
    }
}