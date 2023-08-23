# MessageFactory

Generates MessageHandler.cs, MessageManager.cs and PacketBase.proto of the input files (proto).

## Build Target Framework
.NET 5.0

## Usage
```
MessageFactory.exe [-s ExcludePrefixForServer] [-c ExcludePrefixForClient] NAMESPACE INPUT_DIRECTORY
```
## Notes
- It generates codes for only top-level messages (not nested messsages).
- If proto file has error(s), it may not generate files.
- The directory `MessageFormats` should be located with `MessageFactory.exe`.
- Even if the name of a message type starts with the specified exclude-prefix value, the name will not be excluded from generation if removing this value from the name results in the remaining part starting with a lowercase letter.
- PacketBase.proto contain all found message types. (It does not exclude anything.)
- To use 'P' as a prefix in the PacketType enumeration (at csharp-script), change the message type names as follows: 

Replace consecutive uppercase alphabets from the left with '[char]_'.
The last alphabet among consecutive uppercase alphabets is not considered.

###### e.g. :
 - SendChat -> SendChat (becomes `PSendChat` in PacketType.cs)
 - SSendChat ->S_SendChat (becomes `PSSendChat` in PacketType.cs)
 - SVSendChat -> S_V_SendChat (becomes `PSVSendChat` in PacketType.cs)

## Arguments

- **ExcludePrefixForServer**: Specifies that top-level message types with names starting with the provided value (string) will be excluded from server-side file generation. *[optional]*

- **ExcludePrefixForClient**: Specifies that top-level message types with names starting with the provided value (string) will be excluded from client-side file generation. *[optional]*

- **NAMESPACE**: Sets the namespace for the created files.

- **INPUT_DIRECTORY**: This directory will be scanned by the program to find all proto files (.proto).


### Example

Using the argument `-s C`:

- `CSendChat`: Excluded (becomes `SendChat`)
- `ChatBase`: Not excluded (remains `ChatBase`)

## Output Files

### MessageManager.cs
- Description: This singleton class reads the original packet type from the received data buffer (`ReadOnlySpan<byte>`) and executes specific callback functions based on the type. It accomplishes the following tasks:
    - Creates the `PacketType` enumeration.
    - Completes a `Dictionary` for callback functions and `Google.Protobuf.IMessage` parsers based on `PacketType`.

### MessageHandler.cs
- Description: This static class contains static callback functions for each Packet Type. You can write code within these functions to perform actions with the parsed message data.

### PacketBase.proto
- Description: It contains only one enumeration: `PacketType`. The first value(value = 0) is always `PInvalid`. The program will write the members for each message type. Namespace information is commented out, so users need to fill it in after the file is generated.
- The member name will be:
    - P[message_type]

## Format Files
- In text files, it is written as UTF-8.
- Note that `""` in UTF-8 format becomes `"` in UTF-16 format.

## String Format Parameter of txt files
The following list represents the string format for each file. In order, [{0}, {1}, ...] will be inserted. (You don't need to care these formats.)

- MessageHandler: [Namespace, MessagehandlerItems]
- MessageHandlerItem: [MessageType]
- MessageManager: [Namespace, MessageManagerInitFormat]
- MessageManagerInit: [MessageType]
- MessageManagerMapping: [MessageType]
- PacketBase: [PacketBaseEnumItems]
- PacketBaseEnumItem: [Formatted MessageType]