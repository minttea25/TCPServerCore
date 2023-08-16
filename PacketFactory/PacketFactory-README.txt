--------------------------------------------------------------------------------------------------------------------------------------------------------
Possible Data Types for Members (in bytes)
 
char(2), bool(2), 
short(2), ushort(2), int(4), uint(4), long(8), ulong(8),
float(4), double(8), decimal(8)
string
 
Generic Type: List<>
 
Custom Type (User-defined Class)
 
Note: Does not support byte, sbyte, nint, and nuint.
Note: Only supports List in Generics.
--------------------------------------------------------------------------------------------------------------------------------------------------------
Possible String Formats (related to Encoding class)
 
UTF8, ASCII, Unicode (More possible types are available in Encoding class)
--------------------------------------------------------------------------------------------------------------------------------------------------------
Packet Definitions
 
The packets defined in 'Packets' will each be created as a separate file named '[Name].cs'.
The items defined in 'Items' will be created in one file named 'PacketItems.cs'.
 
The format is for Visual Studio on Windows.
The values of Namespace, Type, and Name must not be null or empty.
If the type of a member is not one of the above types, the factory considers it a custom type of user-defined class.
Errors may occur if the length of the total data is shorter than ReserveBufferSize.
If the type of a member is not a string, the StringFormat will be ignored (You can set it to null).
Custom types must be defined in 'Items'.
The items are not PACKETS: they cannot be serialized or deserialized directly.
-------------------------------------------------------------------------------------------------------------------------------------------------------- 
OUTPUT
 
PacketBase.cs: Contains interfaces and enumerations of packets.
PacketItems.cs: Includes serializable and deserializable classes of IItemPacket.
[PacketName].cs: Includes a class that you defined in PacketDef.json or as the 1st argument.
PacketHandler.cs: Contains callback methods that are called depending on the type of each packet.
PacketManager.cs: Contains mapping code that maps received packets to callback methods depending on the type of each packet.
Copy.bat: A batch file to copy the created files above to the designated directory.
(If the 2nd argument is null, it will not be created)
-------------------------------------------------------------------------------------------------------------------------------------------------------- 
Execution Arguments
 
It requires 0 to 2 execution arguments.
 
[0]: The target file that defines the packets. [default: PacketDef.json]
[1]: The target directory of Copy.bat where the created files will be copied.
-------------------------------------------------------------------------------------------------------------------------------------------------------- 
In the program.cs file, the format string is written as UTF-16.
In text files, it is written as UTF-8.
Note that [""] in UTF-8 format becomes ["] in UTF-16 format.
--------------------------------------------------------------------------------------------------------------------------------------------------------
String Format Parameter of txt files

The following list represents the string format for each file.
In order, [{0}, {1}, ...] will be inserted. Please pay attention to the order.

Packet: [NamespaceName, PacketType, MembersFormat, ReserveBufferSize, M_MemberSerializeFormat, M_MemberDeserializeFormat, MemberSerializeFormat, MemberDeserializeFormat]
PacketBase: [NamespaceName, PacketNameFormat]
PacketHandler: [NamespaceName, PacketHandlerItemFormat]
PacketHandlerItem: [PacketType]
PacketItem: [NamespaceName, PacketItemClassFormat]
PacketItemClass: [ClassName, MembersFormat, M_MemberSerializeFormat, M_MemberDeserializeFormat, MemberSerializeFormat, MemberDeserializeFormat]
PacketManager: [NamespaceName, PacketManagerMappingFormat]
PacketManagerMapping: [PacketType]
PacketMember: [PacketType, PacketName]
PacketMemberList: [PacketType, PacketListName]
PacketName: [PacketName, EnumValue(ushort)]

MemberDeserialize: [MemberName, BitConverterFuncName, MemberPrimitiveType]
MemberDeserializeAdd: [MemberName, BitConverterFuncName, MemberPrimitiveType]
MemberDeserializeAddClass: [MemberType, MemberName]
MemberDeserializeAddString: [MemberName, EncodingType]
MemberDeserializeClass: [MemberName, MemberType]
MemberDeserializeList: [MemberListName, MemberDeserializeAddFormat]
MemberDeserializeString: [MemberName, EncodingType]
MemberSerialize: [MemberPritimiveType, MemberName]
MemberSerializeClass: [MemberName]
MemberSerializeList: [MemberListName, MemberType, MemberSerializeAddFormat]
MemberSerializeString: [MemberName, EncodingType]
