using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace PacketFactory
{
    /* ------------------------------------------------------------------------------------------------------------
     * The possible type of member (bytes)
     * 
     * char(2), bool(2), 
     * short(2), ushort(2), int(4), uint(4), long(8), ulong(8),
     * float(4), double(8), decimal(8)
     * string
     * 
     * Generic: List<>
     * 
     * Custom-type (User-defined Class)
     * 
     * Note: Does not supports byte, sbyte, nint and nuint.
     * Note: Only supports List in Generics.
     * ------------------------------------------------------------------------------------------------------------
     * Possible type of String Formats (related to Encoding class)
     * 
     * UTF8, ASCII, Unicode (More possible types are available in Encoding class)
     * ------------------------------------------------------------------------------------------------------------
     * Packet Definition
     * 
     * The packets defined in 'Packets' will each be created as a separate file named '[Name].cs'.
     * The items defined in 'Items' will be created in one file named 'PacketItems.cs'.
     * 
     * The format is for Visual Studio on Window.
     * The values of Namespace, Type and Name must not be null or empty.
     * If the type of a member is not one of the above types, the factory consider it a custom-type of user-defined class.
     * Error may occur when the length of the total data is shorter than ReserveBufferSize. 
     * If the type of a member is not string type, the StringFormat will be ignored. (You can set it to null)
     * Custom types must be defined in 'Items'.
     * The items are not PACKETS: they can not be serialized or deserialized directly.
     * ------------------------------------------------------------------------------------------------------------
     * OUTPUT
     * 
     * PacketBase.cs: Contains interfaces and enumerations of packets.
     * PacketItems.cs: Includes serializable and deserializable classes of IItemPacket.
     * [PacketName].cs: Includes a class that you defined in the PacketDef.json or in the 1st argument.
     * PacketHandler.cs: Contains callback methods that are called depending on the type of each packet.
     * PacketManager.cs: Includes mapping code that maps received packets to callback methods depending on the type of each packet.
     * Copy.bat: The batch file to copy created files above to designated directory. 
     * (If the 2nd argument is null, it will not created)
     * ------------------------------------------------------------------------------------------------------------
     * Execution Arguments
     * It needs 0 to 2 execution arguments.
     * 
     * [0]: The target file that defines the packets. [default: PacketDef.json]
     * [1]: The target directory of Copy.bat where the created files will be copied.
     * ------------------------------------------------------------------------------------------------------------
     * In the program.cs file, the format string is written as UTF-16.
     * In the text files, it is written as UTF-8.
     * Note that ["] in UTF-8 format becomes [""] in UTF-16 format.
     */

    public class Packet
    {
        public string Name { get; set; }
        public int ReserveBufferSize { get; set; }
        public List<Member> Members { get; set; } = new List<Member>();
    }

    public class PacketItem
    {
        public string Name { get; set; }
        public List<Member> Members { get; set; } = new List<Member>();
    }

    public struct Member
    {
        public bool List { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string StringFormat { get; set; }
    }

    public class Format
    {
        public string Namespace { get; set; }
        public List<Packet> Packets { get; set; } = new List<Packet>();
        public List<PacketItem> Items { get; set; } = new List<PacketItem>();
    }

    class Program
    {
        const string temp = "_t";

        const string packetDefFile = "PacketDef.json";
        const string packetBaseFile = "PacketBase.cs";
        const string itemsFile = "PacketItems.cs";
        const string packetManagerFile = "PacketManager.cs";
        const string packetHandlerFile = "PacketHandler.cs";

        static void Main(string[] args)
        {
            string target = args.Length > 0 ? args[0] : packetDefFile;
            if (CreateBaseJson(target) == false) return;

            string baseText = File.ReadAllText(target);
            Format format = JsonSerializer.Deserialize<Format>(baseText);

            List<string> createdFiles = new List<string>();

            // check files
            if (!(CheckFiles<BufferFormat>() && CheckFiles<MemoryFormat>() && CheckFiles<ArraySegmentFormat>()) == true) return;

            Dictionary<string, string> baseFormat = BufferFormat.ReadAllFiles<BufferFormat>();
            Dictionary<string, string> memoryFormat = BufferFormat.ReadAllFiles<MemoryFormat>();
            Dictionary<string, string> arraySegmentFormat = BufferFormat.ReadAllFiles<ArraySegmentFormat>();

            WritePacketFiles(format, createdFiles,
                baseFormat[BufferFormat.Packet], baseFormat[BufferFormat.PacketMember], baseFormat[BufferFormat.PacketMemberList],
                memoryFormat[MemoryFormat.MemberSerialize], memoryFormat[MemoryFormat.MemberSerializeString], memoryFormat[MemoryFormat.MemberSerializeClass], memoryFormat[MemoryFormat.MemberSerializeList],
                memoryFormat[MemoryFormat.MemberDeserialize], memoryFormat[MemoryFormat.MemberDeserializeString], memoryFormat[MemoryFormat.MemberDeserializeClass], memoryFormat[MemoryFormat.MemberDeserializeList],
                memoryFormat[MemoryFormat.MemberDeserializeAdd], memoryFormat[MemoryFormat.MemberDeserializeAddString], memoryFormat[MemoryFormat.MemberDeserializeAddClass],
                arraySegmentFormat[ArraySegmentFormat.MemberSerialize], arraySegmentFormat[ArraySegmentFormat.MemberSerializeString], arraySegmentFormat[ArraySegmentFormat.MemberSerializeClass], arraySegmentFormat[ArraySegmentFormat.MemberSerializeList],
                arraySegmentFormat[ArraySegmentFormat.MemberDeserialize], arraySegmentFormat[ArraySegmentFormat.MemberDeserializeString], arraySegmentFormat[ArraySegmentFormat.MemberDeserializeClass], arraySegmentFormat[ArraySegmentFormat.MemberDeserializeList],
                arraySegmentFormat[ArraySegmentFormat.MemberDeserializeAdd], arraySegmentFormat[ArraySegmentFormat.MemberDeserializeAddString], arraySegmentFormat[ArraySegmentFormat.MemberDeserializeAddClass]);

            WritePacketBaseFile(format, createdFiles,
                baseFormat[BufferFormat.PacketName], baseFormat[BufferFormat.PacketBase], format.Namespace);

            WritePacketItemFile(format, createdFiles,
                baseFormat[BufferFormat.PacketItem], baseFormat[BufferFormat.PacketItemClass], baseFormat[BufferFormat.PacketMember], baseFormat[BufferFormat.PacketMemberList],
                memoryFormat[MemoryFormat.MemberSerialize], memoryFormat[MemoryFormat.MemberSerializeString], memoryFormat[MemoryFormat.MemberSerializeClass], memoryFormat[MemoryFormat.MemberSerializeList],
                memoryFormat[MemoryFormat.MemberDeserialize], memoryFormat[MemoryFormat.MemberDeserializeString], memoryFormat[MemoryFormat.MemberDeserializeClass], memoryFormat[MemoryFormat.MemberDeserializeList],
                memoryFormat[MemoryFormat.MemberDeserializeAdd], memoryFormat[MemoryFormat.MemberDeserializeAddString], memoryFormat[MemoryFormat.MemberDeserializeAddClass],
                arraySegmentFormat[ArraySegmentFormat.MemberSerialize], arraySegmentFormat[ArraySegmentFormat.MemberSerializeString], arraySegmentFormat[ArraySegmentFormat.MemberSerializeClass], arraySegmentFormat[ArraySegmentFormat.MemberSerializeList],
                arraySegmentFormat[ArraySegmentFormat.MemberDeserialize], arraySegmentFormat[ArraySegmentFormat.MemberDeserializeString], arraySegmentFormat[ArraySegmentFormat.MemberDeserializeClass], arraySegmentFormat[ArraySegmentFormat.MemberDeserializeList],
                arraySegmentFormat[ArraySegmentFormat.MemberDeserializeAdd], arraySegmentFormat[ArraySegmentFormat.MemberDeserializeAddString], arraySegmentFormat[ArraySegmentFormat.MemberDeserializeAddClass]);

            WritePacketManagerFile(format, createdFiles,
                baseFormat[BufferFormat.PacketManager], baseFormat[BufferFormat.PacketManagerMapping]);

            WritePacketHandlerFile(format, createdFiles,
                baseFormat[BufferFormat.PacketHandler], baseFormat[BufferFormat.PacketHandlerItem]);

            Console.WriteLine("The files are created:");
            foreach (string s in createdFiles)
            {
                Console.WriteLine($@"{s}");
            }

            #region Copy Bat File
            string copyFile = "Copy.bat";
            if (args.Length < 2) return;
            string targetDir = args[1];
            string batText = "";
            foreach(string s in createdFiles)
            {
                if (s == packetDefFile) continue;

                batText += string.Format(CopyBatFormat(), s, $"\"{targetDir}\"");
                batText += Environment.NewLine;
            }
            File.WriteAllText(copyFile, batText);
            Console.WriteLine("Copy.bat is created");
            #endregion
        }

        public static bool CheckFiles<T>() where T : BufferFormat
        {
            bool flag = true;
            foreach (string file in BufferFormat.GetAllFileNames<T>())
            {
                if (File.Exists(file) == false)
                {
                    Console.WriteLine($"Can not find a format file: {file}");
                    flag = false;
                }
            }
            return flag;
        }

        public static void WritePacketFiles(Format format, List<string> createdFiles, 
            string packetFormat, 
            string packetMemberFormat, string packetMemberListFormat,
            string m_memberSerializeFormat, string m_memberSerializeStringFormat, string m_memberSerializeClassFormat, string m_memberSerializeListFormat,
            string m_memberDeserializeFormat, string m_memberDeserializeStringFormat, string m_memberDeserializeClassFormat, string m_memberDeserializeListFormat,
            string m_memberDeserializeAddFormat, string m_memberDeserializeAddStringFormat, string m_memberDeserializeAddClassFormat,
            string memberSerializeFormat, string memberSerializeStringFormat, string memberSerializeClassFormat, string memberSerializeListFormat,
            string memberDeserializeFormat, string memberDeserializeStringFormat, string memberDeserializeClassFormat, string memberDeserializeListFormat,
            string memberDeserializeAddFormat, string memberDeserializeAddStringFormat, string memberDeserializeAddClassFormat)
        {
            List<Packet> packets = format.Packets;
            foreach (Packet pkt in packets)
            {
                string filename = $"{pkt.Name}.cs";
                string content = "";

                string members = "";
                string s_m_members = "";
                string d_m_members = "";
                string s_members = "";
                string d_members = "";

                foreach (Member m in pkt.Members)
                {
                    members += ParseMember(m, packetMemberFormat, packetMemberListFormat);
                    s_m_members += ParseSerialize(m, m_memberSerializeFormat, m_memberSerializeStringFormat, m_memberSerializeClassFormat, m_memberSerializeListFormat);
                    d_m_members += ParseDeserialize(m, m_memberDeserializeFormat, m_memberDeserializeStringFormat, m_memberDeserializeClassFormat, m_memberDeserializeListFormat, m_memberDeserializeAddFormat, m_memberDeserializeAddStringFormat, m_memberDeserializeAddClassFormat);
                    s_members += ParseSerialize(m, memberSerializeFormat, memberSerializeStringFormat, memberSerializeClassFormat, memberSerializeListFormat);
                    d_members += ParseDeserialize(m, memberDeserializeFormat, memberDeserializeStringFormat, memberDeserializeClassFormat, memberDeserializeListFormat, memberDeserializeAddFormat, memberDeserializeAddStringFormat, memberDeserializeAddClassFormat);

                    members += Environment.NewLine;
                    s_m_members += Environment.NewLine;
                    d_m_members += Environment.NewLine;
                    s_members += Environment.NewLine;
                    d_members += Environment.NewLine;
                }

                members = members.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t");
                s_m_members = s_m_members.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t\t");
                d_m_members = d_m_members.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t\t");
                s_members = s_members.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t\t");
                d_members = d_members.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t\t");

                content += string.Format(
                    packetFormat,
                    format.Namespace,
                    pkt.Name,
                    members,
                    pkt.ReserveBufferSize,
                    s_m_members, d_m_members,
                    s_members, d_members);

                content = content.Replace("\t", "    ");
                File.WriteAllText(filename, content);

                createdFiles.Add(filename);
            }
        }

        public static void WritePacketBaseFile(Format format, List<string> createdFiles,
            string packetNameFormat, string basePacketFormat, string namespaceName)
        {
            List<string> packetTypes = new List<string>();
            foreach (var pkt in format.Packets)
            {
                packetTypes.Add(pkt.Name);
            }
            string types = "";
            for (int i = 1; i <= packetTypes.Count; ++i)
            {
                types += string.Format(packetNameFormat, packetTypes[i - 1], i);
                types += Environment.NewLine;
            }
            types = types.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t");
            types = types.Replace("\t", "    ");
            File.WriteAllText(packetBaseFile, string.Format(
                    basePacketFormat,
                    namespaceName,
                    types));
            createdFiles.Add(packetBaseFile);
        }

        public static void WritePacketItemFile(Format format, List<string> createdFiles,
            string packetItemFormat, string packetItemClassFormat,
            string packetMemberFormat, string packetMemberListFormat,
            string m_memberSerializeFormat, string m_memberSerializeStringFormat, string m_memberSerializeClassFormat, string m_memberSerializeListFormat,
            string m_memberDeserializeFormat, string m_memberDeserializeStringFormat, string m_memberDeserializeClassFormat, string m_memberDeserializeListFormat,
            string m_memberDeserializeAddFormat, string m_memberDeserializeAddStringFormat, string m_memberDeserializeAddClassFormat,
            string memberSerializeFormat, string memberSerializeStringFormat, string memberSerializeClassFormat, string memberSerializeListFormat,
            string memberDeserializeFormat, string memberDeserializeStringFormat, string memberDeserializeClassFormat, string memberDeserializeListFormat,
            string memberDeserializeAddFormat, string memberDeserializeAddStringFormat, string memberDeserializeAddClassFormat)
        {
            List<PacketItem> items = format.Items;
            string packetItems = "";
            foreach (PacketItem item in items)
            {
                string members = "";
                string s_m_members = "";
                string d_m_members = "";
                string s_members = "";
                string d_members = "";

                foreach (Member m in item.Members)
                {
                    members += ParseMember(m, packetMemberFormat, packetMemberListFormat);
                    s_m_members += ParseSerialize(m, m_memberSerializeFormat, m_memberSerializeStringFormat, m_memberSerializeClassFormat, m_memberSerializeListFormat);
                    d_m_members += ParseDeserialize(m, m_memberDeserializeFormat, m_memberDeserializeStringFormat, m_memberDeserializeClassFormat, m_memberDeserializeListFormat, m_memberDeserializeAddFormat, m_memberDeserializeAddStringFormat, m_memberDeserializeAddClassFormat);
                    s_members += ParseSerialize(m, memberSerializeFormat, memberSerializeStringFormat, memberSerializeClassFormat, memberSerializeListFormat);
                    d_members += ParseDeserialize(m, memberDeserializeFormat, memberDeserializeStringFormat, memberDeserializeClassFormat, memberDeserializeListFormat, memberDeserializeAddFormat, memberDeserializeAddStringFormat, memberDeserializeAddClassFormat);


                    members += Environment.NewLine;
                    s_m_members += Environment.NewLine;
                    d_m_members += Environment.NewLine;
                    s_members += Environment.NewLine;
                    d_members += Environment.NewLine;
                }

                members = members.Replace(Environment.NewLine, $"{Environment.NewLine}\t");
                s_m_members = s_m_members.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t");
                d_m_members = d_m_members.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t");
                s_members = s_members.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t");
                d_members = d_members.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t");

                packetItems += string.Format(
                        packetItemClassFormat,
                        item.Name,
                        members,
                        s_m_members,
                        d_m_members,
                        s_members,
                        d_members
                    );
            }
            packetItems = packetItems.Replace(Environment.NewLine, $"{Environment.NewLine}\t");
            packetItems = packetItems.Replace("\t", "    ");
            File.WriteAllText(itemsFile, string.Format(
                packetItemFormat,
                format.Namespace, packetItems));
            createdFiles.Add(itemsFile);
        }

        public static void WritePacketManagerFile(Format format, List<string> createdFiles, 
            string packetManagerFormat, string packetManagerMappingFormat)
        {
            string packetMapping = "";
            foreach (Packet pkt in format.Packets)
            {
                packetMapping += string.Format(
                    packetManagerMappingFormat,
                    pkt.Name);
                packetMapping += Environment.NewLine;
            }
            File.WriteAllText(packetManagerFile, string.Format(
                packetManagerFormat,
                format.Namespace,
                packetMapping.Replace(Environment.NewLine, $"{Environment.NewLine}            ")));
            createdFiles.Add(packetManagerFile);
        }

        public static void WritePacketHandlerFile(Format format, List<string> createdFiles, 
            string packetHandlerFormat, string packetHandlerItemFormat)
        {
            string packetHandlerItem = "";
            foreach (Packet pkt in format.Packets)
            {
                packetHandlerItem += string.Format(
                    packetHandlerItemFormat,
                    pkt.Name);
                packetHandlerItem += Environment.NewLine;
            }
            File.WriteAllText(packetHandlerFile, string.Format(
                packetHandlerFormat,
                format.Namespace,
                packetHandlerItem.Replace(Environment.NewLine, $"{Environment.NewLine}        ")));
            createdFiles.Add(packetHandlerFile);
        }


        public static string CopyBatFormat() => @"XCOPY /Y {0} {1}";

        public static string ParseMember(Member member, string packetMemberFormat, string packetMemberListFormat)
        {
            if (member.List == false)
            {
                return string.Format(
                            packetMemberFormat,
                            member.Type,
                            member.Name);
            }
            else
            {
                return string.Format(
                            packetMemberListFormat,
                            member.Type,
                            member.Name);
            }
        }

        public static string ParseSerialize(Member member, 
            string memberSerializeFormat, string memberSerializeStringFormat, string memberSerializeClassFormat, string memberSerializeListFormat)
        {
            if (member.List == false)
            {
                return member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(memberSerializeFormat, member.Type, member.Name),
                    "string" => string.Format(memberSerializeStringFormat, member.Name, member.StringFormat),
                    _ => string.Format(memberSerializeClassFormat, member.Name),
                };
            }
            else
            {
                string serialize = member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(
                                               memberSerializeFormat,
                                               member.Type, temp),
                    "string" => string.Format(
                        memberSerializeStringFormat,
                        temp, member.StringFormat),
                    _ => string.Format(
                        memberSerializeClassFormat,
                        temp),
                };
                return string.Format(memberSerializeListFormat,
                    member.Name,
                    member.Type,
                    serialize.Replace(Environment.NewLine, $"{Environment.NewLine}\t"));
            }
            
        }

        public static string ParseDeserialize(Member member, 
            string memberDeserializeFormat, string memberDeserializeStringFormat, string memberDeserializeClassFormat, string memberDeserializeListFormat,
            string memberDeserializeAddFormat, string memberDeserializeAddStringFormat, string memberDeserializeAddClassFormat)
        {
            if (member.List == false)
            {
                return member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(memberDeserializeFormat, member.Name, BitConverterTo(member.Type), member.Type),
                    "string" => string.Format(memberDeserializeStringFormat, member.Name, member.StringFormat),
                    _ => string.Format(memberDeserializeClassFormat, member.Name, member.Type),
                };
            }
            else
            {
                string deserialize = member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(
                                               memberDeserializeAddFormat,
                                               member.Name, BitConverterTo(member.Type), member.Type),
                    "string" => string.Format(
                        memberDeserializeAddStringFormat,
                        member.Name, member.StringFormat),
                    _ => string.Format(
                        memberDeserializeAddClassFormat,
                        member.Type, member.Name),
                };

                return string.Format(
                    memberDeserializeListFormat,
                    member.Name, deserialize.Replace(Environment.NewLine, $"{Environment.NewLine}\t"));
            }
        }


        public static string BitConverterTo(string type) => type switch
        {
            "bool" => "ToBoolean",
            "short" => "ToInt16",
            "ushort" => "ToUInt16",
            "int" => "ToInt32",
            "long" => "ToInt64",
            "float" => "ToSingle",
            "double" => "ToDouble",
            _ => "",
        };

        public static bool CreateBaseJson(string target)
        {
            if (File.Exists(target) == true) return true;

            Format format = new();

            format.Namespace = "TestNamespace";
            // add sample packet
            Packet p = new();
            p.Name = "TestPacket";
            p.ReserveBufferSize = 2048;
            p.Members.Add(new()
            {
                List = false,
                Type = "ushort",
                Name = "itemId",
            });
            p.Members.Add(new()
            {
                List = true,
                Type = "string",
                Name = "titles",
                StringFormat = "Unicode"
            });

            PacketItem Item = new();
            Item.Name = "Item";
            Item.Members.Add(new()
            {
                List = false,
                Type = "long",
                Name = "playerId",
            });
            Item.Members.Add(new()
            {
                List = false,
                Type = "string",
                Name = "playerName",
                StringFormat = "Unicode"
            });

            p.Members.Add(new()
            {
                List = false,
                Type = "Item",
                Name = "items",
                StringFormat = "Unicode"
            });

            format.Packets.Add(p);
            format.Items.Add(Item);

            string text = JsonSerializer.Serialize(format);
            File.WriteAllText(target, text);

            Console.WriteLine(@$"The format file is created: {target}");

            return false;
        }
    }
}
