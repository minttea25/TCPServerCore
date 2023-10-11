##### The below readme was written with some AI translations.

# TCPServerCore
This is a library that can be used in both Client and Server based on TCP.

# Built Framework Version
`.NET Standard 2.1`

## Conditional Compile Symbols
- Optional: `MEMORY_BUFFER` (Build using Memory<byte>, otherwise use ArraySegment)
- Choose one out of three: `CUSTOM_PACKET`, `MESSAGE_PACKET`, `MESSAGE_WRAPPER_PACKET` (Choose the type of packet to use)

## Features
### Buffers
- Implement SendBuffer and RecvBuffer using ArraySegment and Memory respectively (functionality remains the same)
- RecvBuffer is shared, SendBuffer is per-thread (exists in TLS)
### Networks
- Provides Listener (including Accept) and Connector classes
- Provides a Session class for managing each connection (like Buffer, can choose between ArraySegment and Memory)
- Send raw bytes without additional serialization
- Minimizes the number of copy operations and copy costs for buffers
- Apply flatter-gatter when sending
### Packets
- CustomPacket: Packet using manual serialization/deserialization without an external library (Header: Packet number, Total packet size)
  - `[[packetType, 2][size, 2]]` `[data, ~]`
- Message: A packet using Google.Protobuf with no additional header; the packet number and total packet size are serialized entirely using protobuf. The size is implemented as fixed32 for direct reading and deserialization (refering to the Base128Encoding class).
  - `[[tag, 1][size, 4][tag, 1][pacektType, 2][data, ~]]`
  - Implement a MessageParser separately to create a message into a packet directly
- MessageWrapper: Only uses Google.Protobuf for the data part and manually serializes the header part (refering to the Base128Encoding class). The packet types defined in contents are be mapped with dictionary in `MessageWrapper`. 
  - `[size, 2][packeType, 2]` `[data, ~]`
  - You can wrap messages with just a single call to MessageWrapper.Serialize() by defining the packetType in the content code.
### Logging
- Uses Serilog (Serilog.File, Serilog.Console)
- Logs only for ServerCore can be recorded separately, and the same logger can be used in the content code
- `CoreLogger.Logger` (Applied in Release build only)
### JobQueue and JobTimer Provided
- Push messages into the queue and flush for broadcasting
- Use the corresponding JobQueue in the content code (event signal or busy-waiting)
- JobTimer is implemented as a priority queue
### PacketFactories
- Content code generators for the three types of packets
- Create `PacketManager` to interpret received packets from the recv buffer and `PacketHandler` to process each packet type based on the input to create the packet as defined
- More details can be found here
  - CustomPacket: [PacketFactory-README.md](https://github.com/minttea25/TCPServerCore/blob/master/PacketFactory/PacketFactory-README.md)
  - Message: [MessageFactory-README.md](https://github.com/minttea25/TCPServerCore/blob/master/MessageFactory/MessageFactory-README.md)
  - MessageWrapper: [MessageWrapperFactory-README.md](https://github.com/minttea25/TCPServerCore/blob/master/MessageWrapperFactory/MessageWrapperFactory-README.md)
### TestServer
- Test server for testing, corresponding to the TestClient
- Creates and manages rooms according to client requests
- Inside the Protobuf folder, you can find the used proto files and generated C# files
### TestClient
- Test client for testing, corresponding to the TestServer (Console)
- Allows sending and receiving messages to clients in the same room
- Inside the Protobuf folder, you can find the used proto files and generated C# files

## Used External Libraries
- [Google.Protobuf](https://protobuf.dev/)
- [Serilog](https://serilog.net/) (File, Console, Async)

## Other Notes
- Situations where the transmission data gets larger and exceeds the maximum size of SendBuffer are not considered (if needed, you can add code to create a new SendBuffer when reserving in SenderBufferTLS).

## Test
- Memory stability test OK (No memory increases in a scenario with many connections and data transmission)


---


# TCPServerCore
TCP를 기반으로 하는 Client와 Server에서 모두 공용으로 사용가능한 라이브러리입니다.

# Built Framework Version
`.NET Standard 2.1`

## Conditional Compile Symbols
- optional: `MEMORY_BUFFER` (Memory<byte>를 이용하도록 빌드, 그렇지 않으면 ArraySegment 사용)
- 3개 중 하나 사용: `CUSTOM_PACKET`, `MESSAGE_PACKET`, `MESSAGE_WRAPPER_PACKET` (사용할 패킷 유형 선택)

## Features
### Buffers
- SendBuffer와 RecvBuffer를 ArraySegment와 Memory로 각각 구현 (동작 내용은 동일)
- RecvBuffer는 공용 사용, SendBuffer는 스레드 별 사용 (TLS에 존재)
### Networks
- Listener(Accept 포함), Connector 클래스 제공
- 각 연결 관리를 위한 Session 클래스 제공(Buffer와 마찬가지로 ArraySegment, Memory 선택 사용)
- 추가적인 직렬화 없이 SendRaw를 통해 바이트 값 전송 가능
- 버퍼에 대한 복사 횟수 및 복사 비용 최소화
- send시 flatter-gatter 적용 (패킷 모아 보내기)
### Packets
- CustomPacket: 별도 라이브러리 없이 수동 직렬화 이용한 패킷 (헤더: 패킷 번호, 패킷 전체 사이즈) 
  - `[[packetType, 2][size, 2]]` `[data, ~]`
- Message: Google.Protobuf를 이용한 패킷으로 별도의 헤더 없이 패킷 번호와 패킷 전체 사이즈를 전체를 protobuf로 직렬화함, size를 fixed32로 구현하여 직접 읽어서 역직렬화에 사용
  - `[[tag, 1][size, 4][tag, 1][pacektType, 2][data, ~]]`
  - `MessageParser`를 따로 구현하여 메시지를 바로 packet으로 생성 가능
- MessageWrapper: data 부분만 Google.Protobuf를 이용하고 헤더 부분은 수동 직렬화(Base128Encoding 클래스 참조) 이용, 어떤 메시지에 대해 protobuf를 통한 직렬화를 하고 직렬화된 값에 대한 사이즈와 패킷번호를 정하여 헤더를 추가해 패킷을 완성시킵니다.
  - `[size, 2][packeType, 2]` `[data, ~]`
  - packetType를 컨텐츠 코드에서 정의 하여 dictionary를 형성하고 해당 메시지를 `MessageWrapper.Serialize()`를 호출하는 것만으로 메시지 랩핑 가능
### Logging
- `Serilog` 사용 (Serilog.File, Serilog.Console)
- ServerCore에 대한 로그만 따로 기록될 수 있도록 하였고, 콘텐츠 코드에서도 해당 로거 사용 가능
- `CoreLogger.Logger` (Release용에서만 적용)
### JobQueue, JobTimer 제공
- Queue에 메시지를 push후, flush하여 broadcasting
- 컨텐츠 코드에서 해당 JobQueue 사용 (event signal or busy-waiting)
- JobTimer는 우선순위큐로 되어 있음
### PacketFactories
- 3가지 Packet 종류에 대한 컨텐츠 코드 생성기
- recv 버퍼를 통해 받은 패킷을 해석하는 `PacketManager`와 각 패킷을 종류별로 처리하는 `PacketHandler`를 입력으로 넣은 패킷 정의에 맞게 생성
- 자세한 내용은 아래에서 확인
  - CustomPacket: https://github.com/minttea25/TCPServerCore/blob/master/PacketFactory/PacketFactory-README.md
  - Message: https://github.com/minttea25/TCPServerCore/blob/master/MessageFactory/MessageFactory-README.md
  - MessageWrapper: https://github.com/minttea25/TCPServerCore/blob/master/MessageWrapperFactory/MessageWrapperFactory-README.md
### TestServer
- TestClient에 대응하는 테스트용 서버
- 클라이언트 요청에 따라 방을 생성 및 관리
- Protobuf 폴더 안에 사용한 proto 파일과 C# 생성 파일이 있음
### TestClient
- TestServer에 대응하는 테스트용 클라이언트 (콘솔)
- 같은 방에 있는 클라이언트에 메시지를 송수신 할 수 있음
- Protobuf 폴더 안에 사용한 proto 파일과 C# 생성 파일이 있음

## 사용 외부 라이브러리
- [Google.Protobuf](https://protobuf.dev/)
- [Serilog](https://serilog.net/) (File, Console, Async)

## 그 외 특이사항
- 전송 데이터가 큰 Send 수행이 점점 밀려서 SendBuffer의 최대값을 넘어가는 상황은 고려하지 않았음 (만약 필요하다면 SenderBufferTLS에서 Reserve시에 새로운 SendBuffer를 추가해주는 코드를 넣으면 됨)

## Test
- 메모리 안정성 테스트 OK (많은 연결과 데이터 송/수신에 대해 메모리가 우상향으로 계속 증가하는 현상 없음)
