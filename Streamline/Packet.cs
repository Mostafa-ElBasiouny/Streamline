/// Copyright (c) 2023 Mostafa Elbasiouny
///
/// This software may be modified and distributed under the terms of the MIT license.
/// See the LICENSE file for details.

using System.IO.Compression;

namespace Streamline;

/// <summary>
/// Provides functionality for converting packets to bytes and vice versa.
/// </summary>
public sealed class Packet
{
    /// <summary>
    /// The size of a packet fragment.
    /// </summary>
    public static int Size { get; } = 8192;

    /// <summary>
    /// The packet internal memory stream.
    /// </summary>
    private readonly MemoryStream _memoryStream;

    /// <summary>
    /// The packet header size.
    /// </summary>
    private const int HeaderSize = sizeof(int) * 2 + sizeof(bool);

    /// <summary>
    /// The packet header.
    /// </summary>
    private (int fragments, int identifier, bool compressed) _header;

    /// <summary>
    /// Initializes a new packet using the provided buffer.
    /// </summary>
    /// <param name="buffer"> The packet raw data. </param>
    public Packet(byte[] buffer) => _memoryStream = new MemoryStream(buffer);

    /// <summary>
    /// Initializes a new empty packet using the provided identifier.
    /// </summary>
    /// <param name="identifier"> The packet identifier. </param>
    /// <param name="binaryWriter"> The packet binary writer. </param>
    public Packet(int identifier, out BinaryWriter binaryWriter)
    {
        _header.fragments = 0;
        _header.identifier = identifier;
        _header.compressed = false;

        _memoryStream = new MemoryStream();
        binaryWriter = new BinaryWriter(_memoryStream);
    }

    /// <summary>
    /// Serializes the packet into an array of bytes suitable for transmission.
    /// </summary>
    /// <returns> The packet as a byte array. </returns>
    public List<byte[]> Serialize()
    {
        using var memoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(memoryStream);

        var buffer = Compress(out _header.compressed);

        _header.fragments = CalculateFragments(buffer.Length + HeaderSize);

        binaryWriter.Write(_header.fragments);
        binaryWriter.Write(_header.identifier);
        binaryWriter.Write(_header.compressed);

        binaryWriter.Write(buffer);

        return Fragment(memoryStream.ToArray(), _header.fragments);
    }

    /// <summary>
    /// Deserializes the packet from the raw state.
    /// </summary>
    /// <returns> The packet binary reader. </returns>
    public BinaryReader Deserialize()
    {
        var binaryReader = new BinaryReader(_memoryStream);

        _header.fragments = binaryReader.ReadInt32();
        _header.identifier = binaryReader.ReadInt32();
        _header.compressed = binaryReader.ReadBoolean();

        return _header.compressed
            ? new BinaryReader(Decompress(binaryReader.ReadBytes(_memoryStream.ToArray().Length)))
            : binaryReader;
    }

    /// <summary>
    /// Extracts the packet header.
    /// </summary>
    /// <param name="buffer"> The packet raw data. </param>
    /// <param name="fragments"> The packet fragments. </param>
    /// <param name="identifier"> The packet identifier. </param>
    public static void GetHeader(byte[] buffer, out int fragments, out int identifier)
    {
        using var binaryReader = new BinaryReader(new MemoryStream(buffer));

        fragments = binaryReader.ReadInt32();
        identifier = binaryReader.ReadInt32();
    }

    /// <summary>
    /// Breaks down the packet into smaller fragments suitable for transmission.
    /// </summary>
    /// <param name="buffer"> The packet raw data. </param>
    /// <param name="fragments"> The packet fragments. </param>
    /// <returns> An array of fragments. </returns>
    private static List<byte[]> Fragment(byte[] buffer, int fragments)
    {
        var fragmentsBuffer = new List<byte[]>(fragments);

        for (var i = 0; i < fragments; i++) fragmentsBuffer.Add(buffer.Skip(i * Size).Take(Size).ToArray());

        return fragmentsBuffer;
    }

    /// <summary>
    /// Calculates the number of packet fragments.
    /// </summary>
    /// <param name="bufferLength"> The packet raw data length. </param>
    /// <returns> The number of packet fragments. </returns>
    private static int CalculateFragments(int bufferLength)
    {
        return (int)Math.Round(--bufferLength / Size + 0.5, 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Attempts compressing the packet into a byte array.
    /// </summary>
    /// <param name="compressible"> Determines whether the packet is compressible or not. </param>
    /// <returns> The compressed packet if compressible. </returns>
    private byte[] Compress(out bool compressible)
    {
        using var outputMemoryStream = new MemoryStream();
        using var inputMemoryStream = new MemoryStream(_memoryStream.ToArray());

        using (var gZipStream = new GZipStream(outputMemoryStream, CompressionMode.Compress))
        {
            inputMemoryStream.CopyTo(gZipStream);
        }

        compressible = outputMemoryStream.ToArray().Length <= _memoryStream.Length;

        return compressible ? outputMemoryStream.ToArray() : _memoryStream.ToArray();
    }

    /// <summary>
    /// Decompresses the provided packet raw data.
    /// </summary>
    /// <param name="buffer"> The packet raw data. </param>
    /// <returns> The decompressed packet memory stream. </returns>
    private static MemoryStream Decompress(byte[] buffer)
    {
        var outputMemoryStream = new MemoryStream();
        var inputMemoryStream = new MemoryStream(buffer);

        using (var gZipStream = new GZipStream(inputMemoryStream, CompressionMode.Decompress))
        {
            gZipStream.CopyTo(outputMemoryStream);
        }

        outputMemoryStream.Position = 0;

        return outputMemoryStream;
    }
}