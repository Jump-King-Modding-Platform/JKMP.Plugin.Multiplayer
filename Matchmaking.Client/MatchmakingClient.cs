using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Matchmaking.Client.Chat;
using Matchmaking.Client.Messages;
using Matchmaking.Client.Messages.Handlers;
using Matchmaking.Client.Messages.Processing;
using Matchmaking.Client.Networking;
using Microsoft.Xna.Framework;
using Serilog;
using Serilog.Events;

namespace Matchmaking.Client
{
    public class MatchmakingClient
    {
        public const uint Version = 3;
        
        public string? Password { get; private set; }
        
        public bool IsConnected => client?.Connected ?? false;

        public Events Events { get; } = new();

        private TcpClient? client;
        private Framed<NetworkStream, MessagesCodec, Message>? messages;
        private readonly MatchmakingMessageProcessor processor = new();

        private readonly Queue<(Action action, TaskCompletionSource<bool> tcs)> pendingMainThreadActions = new();

        private static readonly ILogger Logger = Log.ForContext(typeof(MatchmakingClient));
        
        /// <summary>
        /// Connects to the endpoint and waits until we're disconnected.
        /// </summary>
        /// <exception cref="HostnameNotFoundException">Thrown when the hostname was not able to be resolved to an ip address.</exception>
        public async Task Connect(string hostname, int port, byte[] sessionTicket, string name, string levelHash, string? password, Vector2 position, CancellationToken cancellationToken = default)
        {
            if (!IPAddress.TryParse(hostname, out IPAddress? ipAddress))
            {
                ipAddress = (await Dns.GetHostAddressesAsync(hostname)).FirstOrDefault();

                if (ipAddress == null)
                    throw new HostnameNotFoundException(hostname);
            }

            await Connect(ipAddress, port, sessionTicket, name, levelHash, password, position, cancellationToken);
        }

        /// <summary>
        /// Connects to the endpoint and waits until we're disconnected.
        /// </summary>
        public async Task Connect(IPAddress ipAddress, int port, byte[] sessionTicket, string name, string levelHash, string? password, Vector2 position, CancellationToken cancellationToken = default)
        {
            if (client?.Connected == true)
                throw new InvalidOperationException("Client is already connected");
            
            client = new TcpClient(ipAddress.AddressFamily);
            Password = password;

            Logger.Debug("Connecting with level hash: {levelHash}", levelHash);

            try
            {
                await client.ConnectAsync(ipAddress, port);
                await HandleConnection(sessionTicket, name, levelHash, position, cancellationToken);
                Disconnect();
            }
            catch (Exception)
            {
                Disconnect();
                throw;
            }
        }

        public void Disconnect()
        {
            if (client == null || !client.Connected)
                return;

            client.Close();
            
            while (pendingMainThreadActions.Count > 0)
            {
                var (_, tcs) = pendingMainThreadActions.Dequeue();
                tcs.TrySetResult(false);
            }
        }
        
        public void RunPendingMainThreadActions()
        {
            while (pendingMainThreadActions.Count > 0)
            {
                var (action, tcs) = pendingMainThreadActions.Dequeue();
                action();
                tcs.TrySetResult(true);
            }
        }
        
        public Task<bool> ExecuteOnMainThread(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            pendingMainThreadActions.Enqueue((action, tcs));
            return tcs.Task;
        }

        private async Task HandleConnection(byte[] sessionTicket, string name, string levelName, Vector2 position, CancellationToken cancellationToken)
        {
            messages = new Framed<NetworkStream, MessagesCodec, Message>(client!.GetStream(), new MessagesCodec());

            Logger.Debug("Connected! Sending handshake...");
            
            if (!await messages.Send(new HandshakeRequest
            {
                AuthSessionTicket = sessionTicket,
                MatchmakingPassword = Password,
                LevelName = levelName,
                Position = position,
                Version = Version,
            }))
            {
                return;
            }

            if (await messages.Next(cancellationToken) is HandshakeResponse response)
            {
                Logger.Write(response.Success ? LogEventLevel.Verbose : LogEventLevel.Warning, "Got handshake response, success = {successful}", response.Success);

                if (!response.Success)
                {
                    Logger.Warning("Failed to handshake with matchmaking server: {message}", response.ErrorMessage);
                    Events.OnChatMessageReceived(new ChatMessage(ChatChannel.Global, null, null, $"Failed to connect to matchmaking server: {response.ErrorMessage}"));
                    throw new MatchmakingConnectException(response.ErrorMessage!);
                }
            }
            else
            {
                Logger.Warning("Failed to get handshake response");
                return;
            }

            await messages.Send(new PositionUpdate
            {
                Position = position
            });

            // {} basically means not null
            while (await messages.Next(cancellationToken) is {} message)
            {
                Logger.Verbose("New message received: {messageType}", message.GetType().Name);

                var context = new Context(messages, this);
                await processor.HandleMessage(message, context);
            }

            messages = null;
        }

        public async Task SetPassword(string? password)
        {
            Password = password;

            if (client?.Connected == true)
            {
                await messages!.Send(new SetMatchmakingPassword
                {
                    Password = password
                });
            }
        }

        public void SendPosition(Vector2 position)
        {
            if (client?.Connected == false || messages == null)
            {
                Logger.Warning("Tried to send position update but client is not connected");
                return;
            }

            messages.Send(new PositionUpdate
            {
                Position = position
            });
        }

        public void SendChatMessage(string message, ChatChannel channel)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (channel != ChatChannel.Global && channel != ChatChannel.Group)
                throw new ArgumentOutOfRangeException(nameof(channel), "Only Global and Group messages should be sent to the server");

            if (client?.Connected == false || messages == null)
                return;

            Logger.Verbose("Sending chat message: {message}", message);
            messages.Send(new IncomingChatMessage
            {
                Message = message,
                Channel = channel
            });
        }
    }
}