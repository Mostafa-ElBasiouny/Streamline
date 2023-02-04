# PacketHandlerAttribute

Namespace: DataFlow

Provides functionality for handling a [Packet](./dataflow.packet.md) by its identifier.

```csharp
public sealed class PacketHandlerAttribute : System.Attribute
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Attribute](https://docs.microsoft.com/en-us/dotnet/api/system.attribute) → [PacketHandlerAttribute](./dataflow.packethandlerattribute.md)

## Properties

### **TypeId**

```csharp
public object TypeId { get; }
```

#### Property Value

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)<br>

## Constructors

### **PacketHandlerAttribute(UInt16)**

Initializes a new packet handler using the provided identifier.

```csharp
public PacketHandlerAttribute(ushort identifier)
```

#### Parameters

`identifier` [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16)<br>
 The packet handler identifier.

## Methods

### **FindPacketHandlers()**

Retrieves methods that has the [PacketHandlerAttribute](./dataflow.packethandlerattribute.md) applied.

```csharp
internal static MethodInfo[] FindPacketHandlers()
```

#### Returns

[MethodInfo[]](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodinfo)<br>
 An array containing packet handler methods.
