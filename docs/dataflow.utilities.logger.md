# Logger

Namespace: DataFlow.Utilities

Provides functionality for logging messages.

```csharp
public static class Logger
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Logger](./dataflow.utilities.logger.md)

## Methods

### **Initialize(LogMethod)**

Initializes the [Logger](./dataflow.utilities.logger.md) using the provided logging method.

```csharp
public static void Initialize(LogMethod logMethod)
```

#### Parameters

`logMethod` [LogMethod](./dataflow.utilities.logger.logmethod.md)<br>
 The method used to log messages.

### **Log(LogLevel, String, String)**

Logs a message using the provided [LogLevel](./dataflow.utilities.loglevel.md).

```csharp
public static void Log(LogLevel logLevel, string message, string callerMemberName)
```

#### Parameters

`logLevel` [LogLevel](./dataflow.utilities.loglevel.md)<br>
 The message log level.

`message` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
 The message to log.

`callerMemberName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
