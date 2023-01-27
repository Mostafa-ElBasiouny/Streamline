/// Copyright (c) 2023 Mostafa Elbasiouny
///
/// This software may be modified and distributed under the terms of the MIT license.
/// See the LICENSE file for details.

using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Streamline.Server;

/// <summary>
/// Provides functionality for managing a server.
/// </summary>
public class Server
{
    /// <summary>
    /// Connected clients accessible by their (GUID).
    /// </summary>
    public readonly Dictionary<Guid, Client> Clients;

    /// <summary>
    /// Invoked when a client is connected.
    /// </summary>
    public event EventHandler<Guid> Connected;

    /// <summary>
    /// Invoked when a client is disconnected.
    /// </summary>
    public event EventHandler<Guid> Disconnected;

    /// <summary>
    /// The server TCP network listener.
    /// </summary>
    private readonly TcpListener _tcpListener;

    /// <summary>
    /// Initializes a new server using the provided port.
    /// </summary>
    /// <param name="port"> The port number. </param>
    public Server(int port)
    {
        Clients = new Dictionary<Guid, Client>();
        Disconnected += (_, guid) => Clients.Remove(guid);

        _tcpListener = new TcpListener(IPAddress.Any, port);
        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(ConnectCallback, null);
    }

    /// <summary>
    /// Invoked when a connection is established.
    /// </summary>
    /// <param name="asyncResult"> The status of the operation. </param>
    private void ConnectCallback(IAsyncResult asyncResult)
    {
        var client = _tcpListener.EndAcceptTcpClient(asyncResult);
        var guid = Guid.NewGuid();

        Clients.Add(guid, new Client(guid, client, Disconnected));
        Connected.Invoke(this, guid);

        _tcpListener.BeginAcceptTcpClient(ConnectCallback, null);
    }

    /// <summary>
    /// Provides functionality for managing a client connected to the server.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// The client (GUID).
        /// </summary>
        private readonly Guid _guid;

        /// <summary>
        /// Stores the data read from the network stream.
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// The client TCP network service.
        /// </summary>
        private readonly TcpClient _tcpClient;

        /// <summary>
        /// The packet fragments.
        /// </summary>
        private (int current, int total) _fragments;

        /// <summary>
        /// The client network stream.
        /// </summary>
        private readonly NetworkStream _networkStream;

        /// <summary>
        /// Invoked when the client is disconnected.
        /// </summary>
        private event EventHandler<Guid> Disconnected;

        /// <summary>
        /// The received packet.
        /// </summary>
        private (List<byte> buffer, int identifier) _packet;

        /// <summary>
        /// Packet handler methods accessible by their identifier.
        /// </summary>
        private Dictionary<int, PacketHandler> _packetHandlers;

        /// <summary>
        /// Encapsulates a packet handler method.
        /// </summary>
        /// <param name="guid"> The sender (GUID). </param>
        /// <param name="packet"> The packet received. </param>
        private delegate void PacketHandler(Guid guid, Packet packet);

        /// <summary>
        /// Initializes a new client using the provided (GUID), TCP network service and event handler.
        /// </summary>
        /// <param name="guid"> The client (GUID). </param>
        /// <param name="tcpClient"> The client TCP network service. </param>
        /// <param name="disconnected"> The client disconnection event. </param>
        public Client(Guid guid, TcpClient tcpClient, EventHandler<Guid> disconnected)
        {
            PopulatePacketHandlers();

            Disconnected += disconnected;

            _guid = guid;

            _buffer = new byte[Packet.Size];
            _packet.buffer = new List<byte>();

            _tcpClient = tcpClient;
            _tcpClient.ReceiveBufferSize = _tcpClient.SendBufferSize = Packet.Size;

            _networkStream = _tcpClient.GetStream();
            _networkStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);
        }

        /// <summary>
        /// Sends a packet to the connected peer.
        /// </summary>
        /// <param name="packet"> The packet to be sent. </param>
        public void Send(Packet packet)
        {
            var fragments = packet.Serialize();

            foreach (var fragment in fragments)
            {
                _networkStream.BeginWrite(fragment, 0, fragment.Length, null, null);
            }
        }

        /// <summary>
        /// Disconnects the client.
        /// </summary>
        public void Disconnect()
        {
            _tcpClient.Close();

            Disconnected.Invoke(this, _guid);
        }

        /// <summary>
        /// Invoked when a packet is received.
        /// </summary>
        /// <param name="asyncResult"> The status of the operation. </param>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                if (_networkStream.EndRead(asyncResult) <= 0)
                {
                    Disconnect();
                    return;
                }

                _fragments.current++;
                _packet.buffer.AddRange(_buffer);

                if (_fragments.current == 1)
                {
                    Packet.GetHeader(_packet.buffer.ToArray(), out _fragments.total, out _packet.identifier);
                }

                if (_fragments.current == _fragments.total)
                {
                    var packet = new Packet(_packet.buffer.ToArray());

                    if (_packetHandlers.TryGetValue(_packet.identifier, out var packetHandler))
                    {
                        packetHandler(_guid, packet);
                    }

                    _fragments.current = 0;
                    _packet.buffer.Clear();
                }

                _networkStream.BeginRead(_buffer, 0, Packet.Size, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Populates the packet handler methods.
        /// </summary>
        private void PopulatePacketHandlers()
        {
            var methodInfos = PacketHandlerAttribute.GetPacketHandlers();

            _packetHandlers = new Dictionary<int, PacketHandler>(methodInfos.Length);

            foreach (var methodInfo in methodInfos)
            {
                var packetHandlerAttribute = methodInfo.GetCustomAttribute<PacketHandlerAttribute>();
                var packetHandler = Delegate.CreateDelegate(typeof(PacketHandler), methodInfo, false);

                if (packetHandlerAttribute == null || packetHandler == null) continue;
                if (_packetHandlers.ContainsKey(packetHandlerAttribute.Identifier)) continue;

                _packetHandlers.Add(packetHandlerAttribute.Identifier, (PacketHandler)packetHandler);
            }
        }
    }
}