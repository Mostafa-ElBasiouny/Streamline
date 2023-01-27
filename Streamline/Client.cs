/// Copyright (c) 2023 Mostafa Elbasiouny
///
/// This software may be modified and distributed under the terms of the MIT license.
/// See the LICENSE file for details.

using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Streamline.Client;

/// <summary>
/// Provides functionality for managing a client.
/// </summary>
public class Client
{
    /// <summary>
    /// Invoked when the client gets connected.
    /// </summary>
    public event EventHandler Connected;

    /// <summary>
    /// Invoked when the client gets disconnected.
    /// </summary>
    public event EventHandler Disconnected;

    /// <summary>
    /// Stores the data read from the network stream.
    /// </summary>
    private readonly byte[] _buffer;

    /// <summary>
    /// The client network stream.
    /// </summary>
    private NetworkStream _networkStream;

    /// <summary>
    /// The client TCP network service.
    /// </summary>
    private readonly TcpClient _tcpClient;

    /// <summary>
    /// The packet fragments.
    /// </summary>
    private (int current, int total) _fragments;

    /// <summary>
    /// Encapsulates a packet handler method.
    /// </summary>
    /// <param name="packet"> The packet received. </param>
    private delegate void PacketHandler(Packet packet);

    /// <summary>
    /// The received packet.
    /// </summary>
    private (List<byte> buffer, int identifier) _packet;

    /// <summary>
    /// Packet handler methods accessible by their identifier.
    /// </summary>
    private Dictionary<int, PacketHandler> _packetHandlers;

    /// <summary>
    /// Initializes a new client using the provided IP address and port.
    /// </summary>
    /// <param name="ipAddress"> The server IP address. </param>
    /// <param name="port"> The port number. </param>
    public Client(IPAddress ipAddress, int port)
    {
        PopulatePacketHandlers();

        _buffer = new byte[Packet.Size];
        _packet.buffer = new List<byte>();

        _tcpClient = new TcpClient();
        _tcpClient.ReceiveBufferSize = _tcpClient.SendBufferSize = Packet.Size;
        _tcpClient.BeginConnect(ipAddress, port, ConnectCallback, _tcpClient);
    }

    /// <summary>
    /// Sends a packet to the server.
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

        Disconnected.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invoked when a connection is established.
    /// </summary>
    /// <param name="asyncResult"> The status of the operation. </param>
    private void ConnectCallback(IAsyncResult asyncResult)
    {
        _tcpClient.EndConnect(asyncResult);

        _networkStream = _tcpClient.GetStream();
        _networkStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);

        Connected.Invoke(this, EventArgs.Empty);
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

                if (_packetHandlers.TryGetValue(_packet.identifier, out var packetHandler)) packetHandler(packet);

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