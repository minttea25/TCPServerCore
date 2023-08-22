# Usage Guide for Packet Factory

## Build Target Framework
.NET 5.0

## Member Types and Formats

### Primitives
- char (2 bytes)
- bool (2 bytes)
- short (2 bytes)
- ushort (2 bytes)
- int (4 bytes)
- uint (4 bytes)
- long (8 bytes)
- ulong (8 bytes)
- float (4 bytes)
- double (8 bytes)
- decimal (8 bytes)

### Classes
- string
- User-defined classes

### Generic Types
- List<>

_Note: Does not support byte, sbyte, nint, and nuint._
_Note: Only supports List in Generics._

## String Formats

- UTF8
- ASCII
- Unicode

(More possible types are available in the Encoding class)

## Packet Definitions

Packets defined in the 'Packets' section will each generate separate files named '[Name].cs'. Items defined in 'Items' will be created in a single file named 'PacketItems.cs'.

This format is optimized for Visual Studio on Windows.

- Namespace, Type, and Name values must not be null or empty.
- If a member's type is not within the specified types, the factory treats it as a custom type of a user-defined class.
- Errors may occur if the total data length is shorter than ReserveBufferSize.
- If a member's type is not a string, the StringFormat will be ignored (it can be set to null).
- Custom types must be defined in 'Items'.
- The items are not PACKETS: they cannot be serialized or deserialized directly.

## Output Files

- `PacketBase.cs`: Contains interfaces and enumerations for packets.
- `PacketItems.cs`: Includes serializable and deserializable classes for IItemPacket.
- `[PacketName].cs`: Contains the class defined in PacketDef.json or as the 1st argument.
- `PacketHandler.cs`: Holds callback methods that are called based on each packet type.
- `PacketManager.cs`: Includes mapping code that maps received packets to callback methods based on packet types.
- `Copy.bat`: A batch file to copy the created files above to the designated directory. (Not created if the 2nd argument is null)

## Execution Arguments

0 to 2 execution arguments are required.

- [0]: The target file defining the packets. [default: PacketDef.json]
- [1]: The target directory for Copy.bat, where the created files will be copied.

## Format Files

- Text files are encoded in UTF-8.
- Note that [""] in UTF-8 format becomes ["] in UTF-16 format.

---

## String Format Parameters of txt Files

The following list outlines the string format for each file. The placeholders [{0}, {1}, ...] will be replaced in order. (You don't need to care these formats.)

- Packet: [NamespaceName, PacketType, MembersFormat, ReserveBufferSize, M_MemberSerializeFormat, M_MemberDeserializeFormat, MemberSerializeFormat, MemberDeserializeFormat]
- PacketBase: [NamespaceName, PacketNameFormat]
- PacketHandler: [NamespaceName, PacketHandlerItemFormat]
- PacketHandlerItem: [PacketType]
- PacketItem: [NamespaceName, PacketItemClassFormat]
- PacketItemClass: [ClassName, MembersFormat, M_MemberSerializeFormat, M_MemberDeserializeFormat, MemberSerializeFormat, MemberDeserializeFormat]
- PacketManager: [NamespaceName, PacketManagerMappingFormat]
- PacketManagerMapping: [PacketType]
- PacketMember: [PacketType, PacketName]
- PacketMemberList: [PacketType, PacketListName]
- PacketName: [PacketName, EnumValue(ushort)]
- MemberDeserialize: [MemberName, BitConverterFuncName, MemberPrimitiveType]
- MemberDeserializeAdd: [MemberName, BitConverterFuncName, MemberPrimitiveType]
- MemberDeserializeAddClass: [MemberType, MemberName]
- MemberDeserializeAddString: [MemberName, EncodingType]
- MemberDeserializeClass: [MemberName, MemberType]
- MemberDeserializeList: [MemberListName, MemberDeserializeAddFormat]
- MemberDeserializeString: [MemberName, EncodingType]
- MemberSerialize: [MemberPritimiveType, MemberName]
- MemberSerializeClass: [MemberName]
- MemberSerializeList: [MemberListName, MemberType, MemberSerializeAddFormat]
- MemberSerializeString: [MemberName, EncodingType]