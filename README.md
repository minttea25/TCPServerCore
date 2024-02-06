##### The below readme was written with some AI translations.

# TCPServerCore
This is a library that can be used in both Client and Server based on **TCP**.

## Built Framework Version
`.NET Standard 2.1`


## Conditional Compile Symbols
`PACKET_TYPE_INT`: Determines the data type of the packet type. By default, ushort (2bytes) is used, and when `PACKET_TYPE_INT` is declared, uint (4bytes) is used. This may specify the range of values of packet types.

## XML API Documentation
XML documentation for the library API is provided in build files and external files.

## Features
### Buffers
- `SendBuffer`: Each **thread** has its own SendBuffer.
- `RecvBuffer`: Each **session** has its own RecvBuffer.
### Services
- Provides `ServerService` and `ClientService` classes.
- `ServerService`: A service for the **server**, internally has a `Listener`.
- `ClientService`: A service for the **client**, internally has a `Connector`.
- When `Start()` is called on each service, the Server starts listening and the client attempts to connect.
- `ServerServiceConfig`: Structure for server service settings with the following properties:
    - `int SessionPoolCount`: The maximum number of session pools, which represents the maximum number of clients accepted by the server.
    - `int ListenerBacklogCount`: The backlog value of the Listener.
    - `int RegisterListenCount`: The value indicating how many connections the Listener will listen to at once.
    - `bool NoDelay`: Value for the **Nagle algorithm**. (Sets the NoDelay option of the socket.)
    - `bool ReuseAddress`: Sets the **ReuseAddress** option of the socket.
    - `int Linger`: Sets the **Linger** option of the socket. **0 indicates false**, and any other value sets the linger time. (seconds)
    - `int SocketAsyncEventArgsPoolCount`: **[ReadOnly]** Determines the number of asynchronous processing event arguments (`SocketAsyncEventArgs`) of the socket based on the values specified by the user for SessionPoolCount and RegisterListenCount.
- `ClientServiceConfig`: Structure for client service settings with the following properties:
    - `int ClientServiceSessionCount`: Specifies the number of sessions connecting to the server. The default value is 1.
    - `bool ReuseAddress`: Sets the **ReuseAddress** option of the socket.
    - `int SocketAsyncEventArgsPoolCount`: **[ReadOnly]** Determines the number of asynchronous processing event arguments (`SocketAsyncEventArgs`) of the socket based on the value specified by the user for `ClientServiceSessionCount`.

### Sessions
- Users can create custom sessions by inheriting from the `Session` class and the `PacketSession` class.
- The `SessionPool` pre-creates sessions. **Therefore, the `SessionFactory` parameter should create an empty session as the session's constructor is called before the service starts. Session members should be initialized in the abstract methods below.**
- The `Session` has the following abstract methods:
    - `void InitSession()`: Executed when the socket connection is successful and the session starts. **Initialize user members here.**
    - `void ClearSession()`: Called when the socket is no longer valid and returns back to the pool.
    - `void OnConnected(EndPoint endPoint)`: Called when the socket is connected to the server. endPoint represents the endpoint of the connected server.
    - `void OnRecv(ReadOnlySpan<byte> buffer)`: Called when data is received through the socket and sliced into a single message. Takes the sliced buffer as an argument.
    - `void OnSend(int numOfBytes)`: Called when data is sent through the socket. Takes the number of bytes sent as an argument.
    - `void OnDisconnected(EndPoint endPoint, object error)`: Called when the connection is disconnected for the connected socket. Also called when Disconnect is called. `endPoint` represents the endpoint of the connected socket, and `error` contains the reason for disconnection.
    - `void OnRecvProcess(ArraySegment<byte> buffer)`: Called when data is received through the socket. Multiple messages may be included. If you need to override this function, you should slice multiple messages into units and then call `OnRecv(ReadOnlySpan<byte> buffer)` for each.
    - `PacketSession`: Provides functionality to collect and transmit messages. Inherits from `Session` and internally implements `OnRecvProcess()`. **Even if `Send()` is called, the message is not immediately sent but stored in a queue. Later, the messages stored in the queue are actually sent through `FlushSend()`. Transmission criteria include the size and time of reserved messages.** Please check the values of `SessionSendFlushMinReservedByteLength` and `SessionSendFlushMinIntervalMilliseconds` in the `Defines` class. Typically, `FlushSend()` is executed infinitely inside a loop.

### Serializations
- Serialization and deserialization for messages are primarily handled using `Google.Protobuf`.
- The `MessageWrapper` class provides serialization capabilities for `Google.Protobuf.IMessage`.
- By default, the following packet structure is used:
    - `[size, 2][message_type, 2(or4)[message, N~]`
- `MessageWrapper` has a member called `PacketMap`, which is a **Dictionary** type, and it is referenced during serialization/deserialization. This value stores the message type and its value in the `MessageManager` class.

### Loggings
- Logging is used with `Serilog`.
- `CoreLogger` is provided as a global variable, and logging sinks can be specified using `CreateLoggerWithFlag()`. Supports `CONSOLE`, `FILE`, and `DEBUG` logging.
- `CoreLogger.CreateLoggerWithFlag()`: Allows specifying logging sinks (`CONSOLE`, `FILE`, `DEBUG`) in bit flag format and options using the `LoggerConfig` structure. Default Serilog options can be set with `LoggerConfig.GetDefault()` or specified directly by the user. 
    - Example
    ```csharp 
    var config = LoggerConfig.GetDefault();
    config.RestrictedMinimumLevel = Serilog.Events.LogEventLevel.Error;
    CoreLogger.CreateLoggerWithFlag((uint)(CoreLogger.LoggerSinks.CONSOLE | CoreLogger.LoggerSinks.FILE), config);
    ```
- `CoreLogger` can also be used in user code and provides the following functions. The `header` argument is the text appearing at the beginning of the log, and other arguments are used as is for `Serilog` logging functions.
    - `void LogInfo(string header, string messageTemplate, params object?[]? propertyValues)`: Logger.Information()
    - `void LogError(string header, Exception ex, string messageTemplate, params object?[]? propertyValues)`: Logger.Error()
    - `void LogError(string header, string messageTemplate, params object?[]? propertyValues)`: Logger.Error()
    - `void LogDebug(string header, string messageTemplate, params object?[]? propertyValues)`: Logger.Debug()
### Jobs
- **To improve processing efficiency on the server, `JobSerializer` and `JobSerializerWithTimer` classes are provided to handle a series of tasks in a single thread, with the latter allowing specifying time.**
- `Job`: A class that holds an action and its parameters, supporting **up to 5** parameters.
- `CancelableJob`: Represents a cancelable job. After adding a job, it can be canceled as follows:
    ```csharp
    JobSerializerWithTimer jobs = new();
    ...
    
    // Add job
    var job1 = jobs.AddAfter(DoSomthing, 1000); // Execute 'DoSomething' after 1000 ms.
    
    // Cancel the job
    job1.Canceled = true;
    
    // => The serializer will not execute job1 after 1000 ms.
    ```
- `JobSerializer`: Allows adding tasks to be executed via `Add()`. **The `Flush()` function needs to be explicitly called by the user for the added tasks to be executed sequentially.** While tasks can be added concurrently, it should be noted that the added tasks are not executed concurrently.
- `JobSerializerWithTimer`: A class derived from `JobSerializer` that includes all the functionality of `JobSerializer` and additionally includes timer functionality. Tasks can be scheduled using `AddAfter(IJob job, long millisecondsAfter = 0)`. **Similarly, `Flush()` needs to be explicitly called by the user codes.**

### Etc
- The `Serialization` and `Deserialization` classes provide serialization and deserialization using bit-shift operations for types **int, uint, short, and ushort**.
- The `Global` class provides stopwatch functionality globally using the `Stopwatch` class. `JobSerializerWithTimer` calculates time using this stopwatch. It can also be used in user code.

## External Libraries
- [Google.Protobuf](https://protobuf.dev/) (3.21.12)
- [Serilog](https://serilog.net/) (3.1.1)
    - [Serilog.Sinks.Console] (5.0.1)
    - [Serilog.Sinks.Debug] (2.0.0)
    - [Serilog.Sinks.File] (5.0.0)

---
# MessageWrapperFactory
This tool automatically generates `MessageManager.cs`, `MessageHandler.cs`, and `MessageTypes.cs` files based on the Proto files provided. These files can be used by both clients and servers.

## Built Framework Version
`.NET 6.0`

## Execution Arguments
- None

## Output Files
- `MessageManager.cs`: Contains code to put messages and their values into `ServerCoreTCP.MessageWrapper.PacketMap`, code for deserializing messages, and code to link handlers to message types.
- `MessageHandler.cs`: Contains callback functions for deserialized messages in MessageManager.
- `MessageTypes.cs`: Contains definitions for messages and their enum values.

## How to use
1. Check the `message_wrapper_factory_confi`.json` file and specify values.
2. Place proto files in the directory specified in `TargetDirectoryPath`.
3. Run the `.exe` file to generate the code.

## Features
- If executed for the first time, a `message_wrapper_factory_config.json` file will be created in the execution path.
- Values in `message_wrapper_factory_config.json` include:
    - `CommonNamespace`: Represents the namespace of the generated files.
    - `TargetDirectoryPath`: Represents the directory path where proto files are located.
    - `OutputServerCodeDirectoryPath`: Specifies the path for output files related to the server side.
    - `OutputClientCodeDirectoryPath`: Specifies the path for output files related to the client side.
    - `ServerPacketExcludePrefix`: Specifies the prefix to **exclude** messages intended for processing only on the server from the messages specified in the proto files.
    - `ClientPacketExcludePrefix`: Specifies the prefix to **exclude** messages intended for processing only on the client from the messages specified in the proto files.
    - `ProtoFileExtension`:  Specifies the extension of proto files in the target directory.
- `MessageWrapperFormats` directory and its formats must be included in the execution path.
- Upon successful execution, log files will be created, and `MessageManager.cs`, `MessageHandler.cs`, and `MessageTypes.cs` files will be generated in the directories specified by `OutputServerCodeDirectoryPath` and `OutputClientCodeDirectoryPath`.
- The enum name for message types is determined as follows:
    - `P_[Message Name]`
- The enum value for message types is automatically determined by the application.
- After successfully generating the code, a log file with the following name will be created, which includes information about the proto files in the target directory, configuration values, and message information read from those files:
    - `MessageWrapperFactory_{DateTime.Now:yyyy-MM-dd-HH-mm}.log`
- Messages contained within other messages (nested classes) are not included in the output.

## Example
- `message_wrapper_factory_config.json`: Configured with the namespace `Chat`, and it will be executed for all files with the `.proto` extension in the directory named `Chat`.
- Code related to the server will be generated in the directory named `server`, and code related to the client will be generated in the directory named `client`.
- Messages prefixed with `C` will be excluded from the server code, and messages prefixed with `S` will be excluded from the client code (meaning they will not be included in the message handler).
    ```json
    {
        "CommonNamespace": "Chat",
        "TargetDirectoryPath": "Chat",
        "OutputServerCodeDirectoryPath": "server",
        "OutputClientCodeDirectoryPath": "client",
        "ServerPacketExcludePrefix": "C",
        "ClientPacketExcludePrefix": "S",
        "ProtoFileExtension": "proto"
    }
    ```
- Prefixes: If the value of `ServerPacketExcludePrefix` is `C`
    |Message Name|Exclude?|
    |------------|--------|
    |CChatMessage|excluded|
    |ChatBase|not excluded|
    |CSign|excluded|


---


# TCPServerCore
**TCP**를 기반으로 하는 Client와 Server에서 모두 사용가능한 라이브러리입니다.


## Built Framework Version
`.NET Standard 2.1`


## Conditional Compile Symbols
- `PACKET_TYPE_INT`: 패킷 타입의 데이터 유형을 정합니다. default로 ushort(2bytes)를 사용하며, `PACKET_TYPE_INT` 선언시, uint(4bytes)를 사용합니다. 패킷의 타입의 종류나 값에 대한 범위를 지정할 수 있을 것입니다.

## XML API Documentation
- 빌드 파일과 외부 파일에 해당 라이브러리 API에 대한 xml documentation이 제공됩니다.

## Features
### Buffers
- `SendBuffer`: **스레드**는 각각의 SendBuffer를 가지고 있습니다.
- `RecvBuffer`: **세션**은 각각의 RecvBuffer를 가지고 있습니다.
### Services
- `ServerService`와 `ClientService` 클래스를 제공합니다.
- `ServerService`: **서버**를 위한 서비스로, 내부적으로 `Listener`를 가지고 있습니다.
- `ClientService`: **클라이언트**를 위한 서비스로, 내부적으로 `Connector`를 가지고 있습니다.
- 각 서비스에서 `Start()` 호출 시, Server에서는 Listen을 시작하고 클라이언트에서는 Connect를 시도합니다.
- `ServerServiceConfig`: 서버 서비스의 설정 구조체로 다음과 같은 속성을 가집니다.
    - int SessionPoolCount: 세션 풀의 최대 개수를 나타내는 값으로, 이 값은 서버에서 Accept하는 최대 클라이언트 수를 의미합니다.
    - int ListenerBacklogCount: Listener의 backlog 값입니다.
    - int RegisterListenCount: Listener가 한번에 몇개의 연결을 listen할 지 나타내는 값 입니다.
    - bool NoDelay: **Nagle 알고리즘**에 대한 값입니다. (소켓의 NoDelay 옵션을 설정합니다.)
    - bool ReuseAddress: 소켓의 **ReuseAddress** 옵션을 설정합니다.
    - int Linger: 소켓의 **Linger** 옵션을 설정합니다. **`0`일 시에는 false이고**, 그 외의 값은 linger 타임을 설정합니다. (seconds)
    - int SocketAsyncEventArgsPoolCount: **[ReadOnly]** 사용자가 지정한 SessionPoolCount와 RegisterListenCount의 값에 따라 결정되는 값으로, 소켓의 비동기 처리 이벤트 인자(SocketAsyncEventArgs)의 수를 나타냅니다.
- `ClientServiceConfig`: 클라이언트 서비스의 설정 구조체로 다음과 같은 속성을 가집니다.
    - int ClientServiceSessionCount: 서버에 접속하는 세션의 수를 정할 수 있습니다. 기본값은 1입니다.
    - bool ReuseAddress: 소켓의 **ReuseAddress** 옵션을 설정합니다.
    - int SocketAsyncEventArgsPoolCount: **[ReadOnly]** 사용자가 지정한 `ClientServiceSessionCount의` 값에 따라 결정되는 값으로, 소켓의 비동기 처리 이벤트 인자(`SocketAsyncEventArgs`)의 수를 나타냅니다.

### Sessions
- 사용자는 `Session` 클래스와 `PacketSession` 클래스를 상속하여 사용자 정의 세션을 만들 수 있습니다.
- `SessionPool`에서는 미리 Session을 생성해 놓습니다. **따라서 `SessionFactory` 파라미터는 비어있는 세션(Empty Session)을 생성해야 합니다. Session의 생성자는 서비스 시작전에 호출되기 때문입니다. 세션의 멤버들은 아래 추상 메서드에서 초기화 해야합니다.**
- `Session`은 다음과 같은 추상 메서드들을 가집니다.
    - `void InitSession()`: 소켓이 연결 성공이 되어 세션을 시작할 때 실행됩니다. **여기서 사용자 정의 멤버들을 초기화하면 됩니다.**
    - `void ClearSession()`: 소켓이 더 이상 유효하지 않아 정리 될 때, Pool로 되돌아 가기전 호출 되는 함수 입니다.
    - `void OnConnected(EndPoint endPoint)`: 소켓이 서버와 연결이 되었을 때 호출 되는 함수입니다. `endPoint`는 연결된 서버의 엔드 포인트를 나타냅니다.
    - `void OnRecv(ReadOnlySpan<byte> buffer)`: 소켓으로 데이터를 수신하고 그 데이터를 하나의 메시지로 슬라이스 했을 때 호출되는 함수입니다. 메시지 단위로 슬라이스된 `buffer`를 인자로 갖습니다.
    - `void OnSend(int numOfBytes)`: 소켓으로 데이터를 송신했을 때 호출 되는 함수입니다. 보낸 바이트 수를 인자로 갖습니다.
    - `void OnDisconnected(EndPoint endPoint, object error)`: 연결된 소켓에 대해 연결이 끊어졌을 때 호출되는 함수 입니다. Disconnect가 호출되었을 때도 호출 됩니다. `endPoint`는 연결되어 있던 소켓의 엔드 포인트를 나타내고, `error` disconnected 사유가 담겨 있습니다.
    - `void OnRecvProcess(ArraySegment<byte> buffer)`: 소켓으로 데이터를 수신했을 때 호출 되는 함수입니다. 여러개의 메시지가 포함되어 있을 수 있습니다. 이 함수를 오버라이드할 필요가 있을 경우, 다수의 메시지를 슬라이스하여 유닛으로 만든 다음 각각에 대해 `OnRecv(ReadOnlySpan<byte> buffer)`를 호출해야 합니다.
- `PacketSession`: 메시지를 모아서 전송하는 기능을 제공합니다. `Session`을 상속하고 있고, 내부적으로 `OnRecvPrcoess()`가 구현되어 있습니다. **Send를 호출해도 실제로 바로 메시지가 전송되지 않고 큐에 저장이 됩니다. 이후 `FlushSend()`를 통해 큐에 저장되어 있는 메세지를 실제로 전송하게 됩니다.** 전송을 하는 기준에는 예약된 메시지의 크기와 시간이 있습니다. `Defines`클래스의 `SessionSendFlushMinReservedByteLength`값과 `SessionSendFlushMinIntervalMilliseconds` 확인해주세요. 보통 `FlushSend()`는 루프 안에서 무한히 실행시킵니다.

### Serializations
- 기본적으로 메시지에 대한 직렬화 및 역직렬화는 `Google.Protobuf`를 사용하고 있습니다.
- `MessageWrapper`클래스는 `Google.Protobuf.IMessage`에 대한 직렬화 기능을 제공합니다.
- 기본적으로 다음의 패킷 구조를 사용합니다.
    - `[size, 2][message_type, 2(or4)[message, N~]`
- `MessageWrapper`는 `PacketMap`이라는 **Dictionary** 타입의 멤버를 가지고 있고, 직렬화/역직렬화 시에 이 값의 Key-Value 쌍을 참고하게 됩니다. 이 값은 `MessageManager` 클래스에서 메시지 타입과 그 값을 저장합니다.

### Loggings
- `Serilog`를 이용하여 로깅을 합니다.
- 전역 변수로 `CoreLogger`를 제공하고 `CreateLoggerWithFlag()`를 통해 로깅 Sink를 지정할 수 있습니다. `CONSOLE`, `FILE`, `DEBUG`로깅을 지원합니다. 
- `CoreLogger.CreateLoggerWithFlag()`: 로깅 Sink(`CONSOLE`, `FILE`, `DEBUG`)를 비트 플래그 형식으로 지정할 수 있고, LoggerConfig 구조체로 옵션을 지정할 수 있습니다. `LoggerConfig.GetDefault()`로 `Serilog`의 기본 옵션을 설정할 수 있고, 사용자가 직접 지정할 수도 있습니다.  
    - Example
    ```csharp 
    var config = LoggerConfig.GetDefault();
    config.RestrictedMinimumLevel = Serilog.Events.LogEventLevel.Error;
    CoreLogger.CreateLoggerWithFlag((uint)(CoreLogger.LoggerSinks.CONSOLE | CoreLogger.LoggerSinks.FILE), config);
    ```
- `CoreLogger`는 사용자 코드에서도 사용할 수 있고 다음의 함수를 제공합니다. `header`인자는 로그의 앞부분에 나타나는 텍스트 이고 그외의 인자는 `Serilog`의 로깅 함수 인자를 그대로 사용합니다.
    - `void LogInfo(string header, string messageTemplate, params object?[]? propertyValues)`: Logger.Information()
    - `void LogError(string header, Exception ex, string messageTemplate, params object?[]? propertyValues)`: Logger.Error()
    - `void LogError(string header, string messageTemplate, params object?[]? propertyValues)`: Logger.Error()
    - `void LogDebug(string header, string messageTemplate, params object?[]? propertyValues)`: Logger.Debug()
### Jobs
- **서버에서 처리 효율을 높히기 위해 하나의 쓰레드에서 일련된 작업을 처리할 수 있도록 `JobSerializer`와 시간까지 지정 가능한 `JobSerializerWithTimer` 클래스를 제공합니다. (추상)**
- `Job`: 액션과 액션에 대한 파라미터를 가지고 있는 클래스입니다. 5개의 파라미터까지 지원합니다.
- `CancelableJob`: 취소가능한 Job을 의미합니다. 다음과 같이 작업에 추가 후, 이 작업을 취소 할 수 있습니다.
    ```csharp
    JobSerializerWithTimer jobs = new();
    ...
    
    // Add job
    var job1 = jobs.AddAfter(DoSomthing, 1000); // Execute 'DoSomething' after 1000 ms.
    
    // Cancel the job
    job1.Canceled = true;
    
    // => The serializer will not execute job1 after 1000 ms.
    ```
- `JobSerializer`: `Add()`를 통해 실행할 작업을 추가할 수 있습니다. **`Flush()`함수를 명시적으로 사용자가 직접 호출을 해야 추가된 작업이 순차적으로 실행이 될 것입니다.** 동시에 작업을 추가할 수는 있지만 추가된 작업들은 동시에 실행되지 않음에 유의해야 합니다.
- `JobSerializerWithTimer`: `JobSerializer`로부터 파생된 클래스로 `JobSerializer`의 기능을 모두 포함하고 있으며, 추가적으로 Timer의 기능을 포함하고 있습니다. `AddAfter(IJob job, long millisecondsAfter = 0)`을 통해 실행할 작업을 예약할 수 있습니다. **마찬가지로 `Flush()`를 명시적으로 사용자가 호출해야 합니다.**

### Etc
- `Serialization`와 `Deserialization` 클래스에서는 **`int`, `uint`, `short`, `ushort`** 타입에 대한 비트연산을 사용한 직렬화 및 역직렬화를 제공합니다.
- `Global` 클래스에서는 `Stopwatch` 클래스를 이용한 스톱워치 기능을 전역으로 제공합니다. `JobSerializerWithTimer`는 이 스톱워치로 시간을 계산합니다. 사용자 코드에서도 사용할 수 있습니다.

## External Libraries
- [Google.Protobuf](https://protobuf.dev/) (3.21.12)
- [Serilog](https://serilog.net/) (3.1.1)
    - [Serilog.Sinks.Console] (5.0.1)
    - [Serilog.Sinks.Debug] (2.0.0)
    - [Serilog.Sinks.File] (5.0.0)

---

# MessageWrapperFactory
Proto 파일을 읽고 포함되어 있는 Message에 대해 `MessageManager.cs`, `MessageHandler.cs`, `MessageTypes.cs` 파일을 자동으로 생성합니다. 클라이언트와 서버에서는 이 파일을 이용할 수 있습니다.

## Built Framework Version
`.NET 6.0`

## Execution Arguments
- None

## Output Files
- `MessageManager.cs`: `ServerCoreTCP.MessageWrapper.PacketMap`에 메시지와 그 값을 넣는 코드와 메시지를 역직렬화(deserialization)하는 코드, 그리고 메시지 타입에 따라 실행 시킬 핸들러를 연결하는 코드를 포함합니다.
- `MessageHandler.cs`: `MessageManager`에서 역직렬화된 메시지에 대한 callback 함수를 포함합니다.
- `MessageTypes.cs`: 메시지와 그 enum 값에 대한 정의를 포함합니다.

## How to use
1. `message_wrapper_factory_config.json` 파일을 확인하고 값을 지정합니다.
2. `TargetDirectoryPath`에 코드를 생성할 proto 파일들을 위치시킵니다.
3. `.exe`파일을 실행시켜 코드를 생성합니다.

## Features
- 만약 처음으로 실행시킨다면, 실행경로에 `message_wrapper_factory_config.json`파일이 생성될 것입니다. 
- `message_wrapper_factory_config.json`에 있는 값은 다음과 같습니다.
    - `CommonNamespace`: 생성할 파일의 네임스페이스를 나타냅니다.
    - `TargetDirectoryPath`: proto 파일들이 있는 디렉터리 경로를 나타냅니다.
    - `OutputServerCodeDirectoryPath`: 서버 사이드에 해당되는 출력 파일들의 경로를 지정합니다.
    - `OutputClientCodeDirectoryPath`: 클라이언트 사이드에 해당되는 출력 파일들의 경로를 지정합니다.
    - `ServerPacketExcludePrefix`: proto 파일에 명시되어 있는 메시지들에서 서버에서만 처리할 메시지를 필터링 하기위한 접두사를 지정합니다.
    - `ClientPacketExcludePrefix`: proto 파일에 명시되어 있는 메시지들에서 클라이언트에서만 처리할 메시지를 필터링하기 위한 접두사를 지정합니다.
    - `ProtoFileExtension`: 타겟 디렉터리에서 프로토 파일들의 확장자를 지정합니다. 
- 실행 경로에 `MessageWrapperFormats` 디렉터리와 그 포맷들이 포함되어 있어야 합니다.
- 실행에 성공한다면, 로그 파일이 생성되고 `OutputServerCodeDirectoryPath`와 `OutputClientCodeDirectoryPath`에 지정한 디렉터리 경로에 각각에 해당하는 `MessageManager.cs`, `MessageHandler.cs`, `MessageTypes.cs` 파일들이 생성될 것입니다.
- 메시지 타입에 대한 enum 이름은 다음과 같이 결정 됩니다.
    - `P_[Message Name]`
- 메시지 타입에 대한 enum 값은 애플리케이션이 자동으로 정합니다.
- 코드를 성공적으로 생성했다면, 다음 이름의 로그 파일이 생성될 것입니다. 이 파일에는 타겟 디렉터리에 있는 프로토 파일들의 정보와 설정 값, 그 파일로 부터 읽어들인 메시지 정보를 포함합니다.
    - `MessageWrapperFactory_{DateTime.Now:yyyy-MM-dd-HH-mm}.log`
- `Nested Class`와 같이 메시지 안에 포함된 메시지는 출력 대상에 포함시키지 않습니다.

## Example
- `message_wrapper_factory_config.json`: 네임스페이스를 `Chat`으로 설정하고, `Chat`이라는 디렉터리에 있는 확장자가 `proto`인 모든 파일들에 대해 실행 합니다.
- 서버에 해당하는 코드는 `server`라는 디렉터리에 파일이 생성이 되고, 클라이언트에 해당하는 코드는 `client`라는 디렉터리에 파일이 생성이 될 것입니다. 
- 이름 앞에 `C`가 접두사로 붙은 메시지는 서버 코드에서 제외될 것입니다. 마찬가지로 이름 앞에 `S`가 접두사로 붙은 메시지는 클라이언트 코드에서 제외 될 것입니다. (MessageHandler에서 처리하는 코드를 포함하지 않음을 의미합니다.)
    ```json
    {
        "CommonNamespace": "Chat",
        "TargetDirectoryPath": "Chat",
        "OutputServerCodeDirectoryPath": "server",
        "OutputClientCodeDirectoryPath": "client",
        "ServerPacketExcludePrefix": "C",
        "ClientPacketExcludePrefix": "S",
        "ProtoFileExtension": "proto"
    }
    ```
- 접두사: 만약 `ServerPacketExcludePrefix`의 값이 `C`일 경우
    |Message Name|Exclude?|
    |------------|--------|
    |CChatMessage|excluded|
    |ChatBase|not excluded|
    |CSign|excluded|







