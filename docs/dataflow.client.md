# Client

Namespace: DataFlow

Provides functionality for managing a client.

```csharp
public class Client
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Client](./dataflow.client.md)

## Constructors

### **Client(String, UInt16)**

Initializes a new client using the provided IP address and port.

```csharp
public Client(string ipAddress, ushort port)
```

#### Parameters

`ipAddress` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
 The server IP address.

`port` [UInt16](https://docs.microsoft.com/en-us/dotnet/api/system.uint16)<br>
 The port number.

## Methods

### **Connect()**

Connects the client.

```csharp
public void Connect()
```

### **Disconnect()**

Disconnects the client.

```csharp
public void Disconnect()
```

### **Send(Packet)**

Sends a packet to the server.

```csharp
public Task Send(Packet packet)
```

#### Parameters

`packet` [Packet](./dataflow.packet.md)<br>
 The packet to be sent.

#### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)<br>

## Events

### **Connected**

Invoked when the client gets connected.

```csharp
public event EventHandler Connected;
```

### **Disconnected**

Invoked when the client gets disconnected.

```csharp
public event EventHandler Disconnected;
```

### **PacketReceived**

Invoked when the client receives a packet.

```csharp
public event EventHandler PacketReceived;
```
