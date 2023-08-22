# MessageWrapperFactory

Generates MessageHandler.cs and MessageManager.cs of the input files (proto).

## Build Target Framework
.NET 5.0

## Usage
```
MessageWrapperFactory.exe [-s ExcludePrefixForServer] [-c ExcludePrefixForClient] NAMESPACE INPUT_DIRECTORY
```
## Notes
- It generates codes for only top-level messages (not nested messsages).
- If proto file has error(s), it may not generate files.
- The directory `MessageWrapperFormats` should be located with `MessageWrapperFactory.exe`.

## Arguments

- **ExcludePrefixForServer**: Specifies that top-level message types with names starting with the provided value (string) will be excluded from server-side file generation. *[optional]*

- **ExcludePrefixForClient**: Specifies that top-level message types with names starting with the provided value (string) will be excluded from client-side file generation. *[optional]*

- **NAMESPACE**: Sets the namespace for the created files.

- **INPUT_DIRECTORY**: This directory will be scanned by the program to find all proto files (.proto).

### Note

Even if the name of a message type starts with the specified exclude-prefix value, the name will not be excluded from generation if removing this value from the name results in the remaining part starting with a lowercase letter.

### Example

Using the argument `-s C`:

- `CSendChat`: Excluded (becomes `SendChat`)
- `ChatBase`: Not excluded (remains `ChatBase`)

## Output Files

### MessageManager.cs
- Description: This singleton class reads the original packet type from the received data buffer (`ReadOnlySpan<byte>`) and executes specific callback functions based on the type. It accomplishes the following tasks:
    - Creates the `PacketType` enumeration.
    - Adds items to `ServerCoreTCP.MessageWrapper.PacketMap` with data for each message type.
    - Completes a `Dictionary` for callback functions and `Google.Protobuf.IMessage` parsers based on `PacketType`.

### MessageHandler.cs
- Description: This singleton class contains static callback functions for each Packet Type. You can write code within these functions to perform actions with the parsed message data.

This version should be more organized and easier to read.

## Format Files
- In text files, it is written as UTF-8.
- Note that `""` in UTF-8 format becomes `"` in UTF-16 format.

## String Format Parameter of txt files
The following list represents the string format for each file. In order, [{0}, {1}, ...] will be inserted. (You don't need to care these formats.)

- MessageHandler: [Namespace, MessagehandlerItems]
- MessageHandlerItem: [MessageType]
- MessageManager: [Namespace, PacketTypeEnumItemFormat, MessageManagerMappingFormat, MessageManagerInitFormat]
- MessageManagerInit: [MessageType]
- MessageManagerMapping: [MessageType]
- PacketTypeEnumItemFormat: [MessageType, EnumValue]