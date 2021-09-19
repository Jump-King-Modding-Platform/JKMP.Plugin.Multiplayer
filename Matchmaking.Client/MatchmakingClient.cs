using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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
        public string? Password { get; private set; }
        
        public bool IsConnected => client?.Connected ?? false;

        public Events Events { get; } = new();

        private TcpClient? client;
        private Framed<NetworkStream, MessagesCodec, Message>? messages;
        private readonly MatchmakingMessageProcessor processor = new();

        private static readonly ILogger Logger = Log.ForContext(typeof(MatchmakingClient));
        
        /// <summary>
        /// Connects to the endpoint and waits until we're disconnected.
        /// </summary>
        /// <exception cref="HostnameNotFoundException">Thrown when the hostname was not able to be resolved to an ip address.</exception>
        public async Task Connect(string hostname, int port, byte[] sessionTicket, string name, string levelName, string? password, Vector2 position, CancellationToken cancellationToken = default)
        {
            if (!IPAddress.TryParse(hostname, out IPAddress? ipAddress))
            {
                ipAddress = (await Dns.GetHostAddressesAsync(hostname)).FirstOrDefault();

                if (ipAddress == null)
                    throw new HostnameNotFoundException(hostname);
            }

            await Connect(ipAddress, port, sessionTicket, name, levelName, password, position, cancellationToken);
        }

        /// <summary>
        /// Connects to the endpoint and waits until we're disconnected.
        /// </summary>
        public async Task Connect(IPAddress ipAddress, int port, byte[] sessionTicket, string name, string levelName, string? password, Vector2 position, CancellationToken cancellationToken = default)
        {
            if (client?.Connected == true)
                throw new InvalidOperationException("Client is already connected");
            
            client = new TcpClient(ipAddress.AddressFamily);
            Password = password;

            try
            {
                await client.ConnectAsync(ipAddress, port);
                await HandleConnection(sessionTicket, name, levelName, position, cancellationToken);
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
        }

        private async Task HandleConnection(byte[] sessionTicket, string name, string levelName, Vector2 position, CancellationToken cancellationToken)
        {
            messages = new Framed<NetworkStream, MessagesCodec, Message>(client!.GetStream(), new MessagesCodec());

            Logger.Debug("Connected! Sending handshake...");
            
            if (!await messages.Send(new HandshakeRequest
            {
                AuthSessionTicket = sessionTicket,
                Name = name,
                MatchmakingPassword = Password,
                LevelName = levelName,
                Position = position
            }))
            {
                return;
            }

            if (await messages.Next(cancellationToken) is HandshakeResponse response)
            {
                Logger.Write(response.Success ? LogEventLevel.Verbose : LogEventLevel.Warning, "Got handshake response, success = {successful}", response.Success);

                if (!response.Success)
                {
                    Logger.Warning("Error message: {message}", response.ErrorMessage);
                    return;
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

            Logger.Verbose("Sending position update");
            messages.Send(new PositionUpdate
            {
                Position = position
            });
        }
    }
}