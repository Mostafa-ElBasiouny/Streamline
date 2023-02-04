# Packet

Namespace: DataFlow

Provides functionality for converting packets to bytes and vice versa.

```csharp
public sealed class Packet
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Packet](./dataflow.packet.md)

## Fields

### **Size**

The maximum size of a packet.

```csharp
public static uint Size;
```

## Constructors

### **Packet(Byte[])**

Initializes a new packet using the provided buffer.

```csharp
public Packet(Byte[] buffer)
```

#### Parameters

`buffer` [Byte[]](https://docs.microsoft.com/en-us/dotnet/api/system.byte)<br>
 The packet raw buffer.

### **Packet(UInt16, BinaryWriter&)**

Initializes a new empty packet using the provided identifier.

```csharp
public Packet(ushort identifier, BinaryWriter& binaryWriter)
```

#### Parameters

`identifier` [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16)<br>
 The packet identifier.

`binaryWriter` [BinaryWriter&](https://docs.microsoft.com/en-us/dotnet/api/system.io.binarywriter&)<br>
 The packet binary writer.

## Methods

### **Serialize()**

Serializes the packet into an array of bytes suitable for transmission.

```csharp
public Byte[] Serialize()
```

#### Returns

[Byte[]](https://docs.microsoft.com/en-us/dotnet/api/system.byte)<br>
 The packet as a byte array.

### **Deserialize()**

Deserializes the packet from the raw state.

```csharp
public BinaryReader Deserialize()
```

#### Returns

[BinaryReader](https://docs.microsoft.com/en-us/dotnet/api/system.io.binaryreader)<br>
 The packet binary reader.

### **ReadIdentifier()**

Reads the packet identifier.

```csharp
public ushort ReadIdentifier()
```

#### Returns

[UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16)<br>
 The packet identifier.
