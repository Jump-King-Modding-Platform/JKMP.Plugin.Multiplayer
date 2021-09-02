using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Matchmaking.Client.Messages;
using Matchmaking.Client.Networking;

namespace Matchmaking.Client
{
    public class MatchmakingClient
    {
        private TcpClient? client;
        private Framed<NetworkStream, MessagesCodec, Message>? messages;
        
        /// <summary>
        /// Connects to the endpoint and waits until we're disconnected.
        /// </summary>
        /// <exception cref="HostnameNotFoundException">Thrown when the hostname was not able to be resolved to an ip address.</exception>
        public async Task Connect(string hostname, int port, byte[] sessionTicket, string name, CancellationToken cancellationToken = default)
        {
            if (!IPAddress.TryParse(hostname, out IPAddress? ipAddress))
            {
                ipAddress = (await Dns.GetHostAddressesAsync(hostname)).FirstOrDefault();

                if (ipAddress == null)
                    throw new HostnameNotFoundException(hostname);
            }

            await Connect(ipAddress, port, sessionTicket, name, cancellationToken);
        }

        /// <summary>
        /// Connects to the endpoint and waits until we're disconnected.
        /// </summary>
        public async Task Connect(IPAddress ipAddress, int port, byte[] sessionTicket, string name, CancellationToken cancellationToken = default)
        {
            if (client?.Connected == true)
                throw new InvalidOperationException("Client is already connected");
            
            client = new TcpClient(ipAddress.AddressFamily);

            try
            {
                await client.ConnectAsync(ipAddress, port);
                await HandleConnection(sessionTicket, name, cancellationToken);
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

        private async Task HandleConnection(byte[] sessionTicket, string name, CancellationToken cancellationToken)
        {
            messages = new Framed<NetworkStream, MessagesCodec, Message>(client!.GetStream(), new MessagesCodec());

            Console.WriteLine($"Connected! Sending handshake...");
            
            if (!await messages.Send(new HandshakeRequest
            {
                AuthSessionTicket = sessionTicket,
                Name = name
            }))
            {
                return;
            }

            if (await messages.Next(cancellationToken) is HandshakeResponse response)
            {
                Console.WriteLine($"Got handshake response! Success = {response.Success}");

                if (!response.Success)
                {
                    Console.WriteLine($"Error message: {response.ErrorMessage}");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Failed to get handshake response");
                return;
            }
            
            // {} basically means not null
            while (await messages.Next(cancellationToken) is {} message)
            {
                Console.WriteLine($"New message received: {message.GetType().Name}");
            }
        }
    }
}