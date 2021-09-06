using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public class P2PManager : IDisposable
    {
        private Dictionary<ulong, RemotePlayer> ConnectedPlayers { get; } = new();

        private static readonly ILogger Logger = LogManager.CreateLogger<P2PManager>();

        private readonly Framed<GameMessagesCodec, GameMessage> messages;

        public P2PManager()
        {
            SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
            SteamNetworking.OnP2PConnectionFailed += OnP2PConnectionFailed;

            messages = new(new GameMessagesCodec());
            Task.Run(ProcessMessages);
        }

        public void Dispose()
        {
            SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequest;
            SteamNetworking.OnP2PConnectionFailed -= OnP2PConnectionFailed;
            messages.Dispose();

            foreach (var player in ConnectedPlayers)
            {
                player.Value.Destroy();
                SteamNetworking.CloseP2PSessionWithUser(player.Key);
            }

            ConnectedPlayers.Clear();
        }

        private void OnP2PConnectionFailed(SteamId steamId, P2PSessionError error)
        {
            Logger.Warning("Connection to {steamId} failed: {sessionError}", steamId, error);
            Disconnect(steamId);
        }

        private void OnP2PSessionRequest(SteamId steamId)
        {
            Logger.Verbose("P2P session request incoming from {steamId}", steamId);
            SteamNetworking.AcceptP2PSessionWithUser(steamId);
            ConnectTo(steamId);
        }

        public void ConnectTo(ICollection<SteamId> steamIds)
        {
            foreach (var steamId in steamIds)
            {
                ConnectTo(steamId);
            }
        }

        public void ConnectTo(SteamId steamId)
        {
            if (ConnectedPlayers.ContainsKey(steamId))
                return;

            Logger.Verbose("Connecting to {steamId}...", steamId);
            ConnectedPlayers.Add(steamId, new RemotePlayer(steamId));
            
            messages.Send(steamId, new HandshakeRequest
            {
                AuthSessionTicket = new byte[] { 1, 2, 3, 4 }
            });
        }
        
        public void Disconnect(RemotePlayer player) => Disconnect(player.SteamId);
        public void Disconnect(SteamId steamId)
        {
            if (ConnectedPlayers.TryGetValue(steamId, out var player))
            {
                Logger.Verbose("Destroying RemotePlayer associated with {steamId}", player.SteamId);
                player.Destroy();
                ConnectedPlayers.Remove(steamId);
            }
            
            Logger.Verbose("Disconnecting from {steamId}", player.SteamId);
            SteamNetworking.CloseP2PSessionWithUser(steamId);
        }

        private async Task ProcessMessages()
        {
            try
            {
                while (await messages.Next() is {} message)
                {
                    Logger.Verbose("Received message from {steamId}: {messageType}", message.Sender, message.GetType().Name);

                    if (ConnectedPlayers.TryGetValue(message.Sender, out var player))
                    {
                        player.HandleMessage(message);
                    }
                }

                Logger.Verbose("Finished message processing");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unhandled exception was raised in the message handler thread");
                throw;
            }
        }
    }
}