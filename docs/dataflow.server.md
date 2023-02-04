# Server

Namespace: DataFlow

```csharp
public class Server
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Server](./dataflow.server.md)

## Constructors

### **Server(UInt16)**

Initializes a new server using the provided port.

```csharp
public Server(ushort port)
```

#### Parameters

`port` [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16)<br>
 The port number.

## Methods

### **Start()**

Begins accepting incoming connections.

```csharp
public void Start()
```

### **Shutdown()**

Shuts down the server disconnecting all connected clients.

```csharp
public void Shutdown()
```

### **Send(Guid, Packet)**

Sends a packet to a connected client.

```csharp
public Task Send(Guid guid, Packet packet)
```

#### Parameters

`guid` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>
 The client GUID.

`packet` [Packet](./dataflow.packet.md)<br>
 The packet to be sent.

#### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)<br>

## Events

### **Connected**

Invoked when a client gets connected.

```csharp
public event EventHandler<Guid> Connected;
```

### **Disconnected**

Invoked when a client gets disconnected.

```csharp
public event EventHandler<Guid> Disconnected;
```

### **PacketReceived**

Invoked when a packet is received.

```csharp
public event EventHandler<Guid> PacketReceived;
```
