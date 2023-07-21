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
     * custom-type (user-defined-class)
     * 
     * Note: not support byte, sbyte, nint and nuint.
     * ------------------------------------------------------------------------------------------------------------
     * Possible type of StringFormat (regarding to Encoding class)
     * 
     * UTF8, ASCII, Unicode (There are more possible types in Encdoing class)
     * ------------------------------------------------------------------------------------------------------------
     * Packet Defines
     * 
     * The packets you defined at 'Packets' would be created as each file named '[Name].cs'.
     * The items you defined at 'Items' would be created in one file named 'PacketItems.cs'.
     * 
     * The format is for Visual-Studio in Window.
     * The value of Namespace, Type and Name must not be null or empty.
     * If the type of a member is not one of the above types, the factory consider it custom-type of user-defined-class.
     * It would cause error when the length of the total data is shorter than ReserveBufferSize. 
     * If the type of a member is not string, the StringFormat would be ignored. (You can set it null)
     * The custom-type must be defined at 'Items'.
     * The items are not PACKETS: it can not be serialized or deserialized directly.
     * ------------------------------------------------------------------------------------------------------------
     * OUTPUT
     * 
     * PacketBase.cs: It contains interfaces and enumerations of packets.
     * PacketItems.cs: It contains serializable and deserializable classes of IItemPacket.
     * [PacketName].cs: It contains a class that you defines in the PacketDef.json or 1st-argument.
     * Copy.bat: The bat file to copy created files above to designated directory. 
     * (If the 2nd argument is null, it would not created)
     * ------------------------------------------------------------------------------------------------------------
     * Execution Arguments
     * It needs 0 ~ 2 execution arguments.
     * 
     * [0]: The target file that defines the packets. [default: PacketDef.json]
     * [1]: The target directory of Copy.bat where the created files would be copied.
     * ------------------------------------------------------------------------------------------------------------
     */

    public class Packet
    {
        public string Name { get; set; }
        public int ReserveBufferSize { get; set; }
        public List<Member> Members { get; set; } = new();
    }

    public class PacketItem
    {
        public string Name { get; set; }
        public List<Member> Members { get; set; } = new();
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
        public List<Packet> Packets { get; set; } = new();
        public List<PacketItem> Items { get; set; } = new();
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

            List<string> createdFiles = new();

            #region Packet Files
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
                        members += ParseMember(m);
                        s_m_members += ParseMSerialize(m);
                        d_m_members += ParseMDeserialize(m);
                        s_members += ParseSerialize(m);
                        d_members += ParseDeserialize(m);

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
                        PacketFormat.packetFormat,
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
            #endregion

            #region Packet Base
            {
                List<string> packetTypes = new();
                foreach (var pkt in format.Packets)
                {
                    packetTypes.Add(pkt.Name);
                }
                string types = "";
                for (int i = 1; i <= packetTypes.Count; ++i)
                {
                    types += string.Format(PacketFormat.packetNameFormat, packetTypes[i - 1], i);
                    types += Environment.NewLine;
                }
                types = types.Replace(Environment.NewLine, $"{Environment.NewLine}\t\t");
                types = types.Replace("\t", "    ");
                File.WriteAllText(packetBaseFile, string.Format(
                        PacketFormat.basePacketFileFormat,
                        format.Namespace, types));
                createdFiles.Add(packetBaseFile);
            }
            #endregion

            #region Packet Items
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
                        members += ParseMember(m);
                        s_m_members += ParseMSerialize(m);
                        d_m_members += ParseMDeserialize(m);
                        s_members += ParseSerialize(m);
                        d_members += ParseDeserialize(m);

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
                            PacketFormat.packetItemClassFormat,
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
                    PacketFormat.itemsFormat,
                    format.Namespace, packetItems));
                createdFiles.Add(itemsFile);
            }
            #endregion

            #region Packet Manager
            {
                string packetMapping = "";
                foreach (Packet pkt in format.Packets)
                {
                    packetMapping += string.Format(
                        PacketFormat.packetManagerMappingFormat,
                        pkt.Name);
                    packetMapping += Environment.NewLine;
                }
                File.WriteAllText(packetManagerFile, string.Format(
                    PacketFormat.packetManagerFormat,
                    format.Namespace,
                    packetMapping.Replace(Environment.NewLine, $"{Environment.NewLine}            ")));
                createdFiles.Add(packetManagerFile);
            }
            #endregion

            #region Packet Handler
            {
                string packetHandlerItem = "";
                foreach (Packet pkt in format.Packets)
                {
                    packetHandlerItem += string.Format(
                        PacketFormat.packetHandlerItemFormat,
                        pkt.Name);
                    packetHandlerItem += Environment.NewLine;
                }
                File.WriteAllText(packetHandlerFile, string.Format(
                    PacketFormat.packetHandlerFormat,
                    format.Namespace,
                    packetHandlerItem.Replace(Environment.NewLine, $"{Environment.NewLine}        ")));
                createdFiles.Add(packetHandlerFile);
            }
            #endregion

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

        public static string CopyBatFormat() => @"XCOPY /Y {0} {1}";

        public static string ParseMember(Member member)
        {
            if (member.List == false)
            {
                return string.Format(
                            PacketFormat.packetMemberFormat,
                            member.Type,
                            member.Name);
            }
            else
            {
                return string.Format(
                            PacketFormat.packetMemberListFormat,
                            member.Type,
                            member.Name);
            }
        }

        public static string ParseMSerialize(Member member)
        {
            if (member.List == false)
            {
                return member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(PacketFormat.m_memberSerializeFormat, member.Type, member.Name),
                    "string" => string.Format(PacketFormat.m_memberSerializeStringFormat, member.Name, member.StringFormat),
                    _ => string.Format(PacketFormat.m_memberSerializeClassFormat, member.Name),
                };
            }
            else
            {
                string serialize = member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(
                                               PacketFormat.m_memberSerializeFormat,
                                               member.Type, temp),
                    "string" => string.Format(
                        PacketFormat.m_memberSerializeStringFormat,
                        temp, member.StringFormat),
                    _ => string.Format(
                        PacketFormat.m_memberSerializeClassFormat,
                        temp),
                };
                return string.Format(PacketFormat.m_listSerializeFormat,
                    member.Name,
                    member.Type,
                    serialize.Replace(Environment.NewLine, $"{Environment.NewLine}\t"));
            }
            
        }

        public static string ParseMDeserialize(Member member)
        {
            if (member.List == false)
            {
                return member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(PacketFormat.m_memberDeserializeFormat, member.Name, BitConverterTo(member.Type), member.Type),
                    "string" => string.Format(PacketFormat.m_memberDeserializeStringFormat, member.Name, member.StringFormat),
                    _ => string.Format(PacketFormat.m_memberDeserializeClassFormat, member.Name),
                };
            }
            else
            {
                string deserialize = member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(
                                               PacketFormat.m_listDeserializeAddFormat,
                                               member.Name, BitConverterTo(member.Type), member.Type),
                    "string" => string.Format(
                        PacketFormat.m_listDeserializeStringAddFormat,
                        member.Name, member.StringFormat),
                    _ => string.Format(
                        PacketFormat.m_listDeserializeClassAddFormat,
                        member.Type, member.Name),
                };

                return string.Format(
                    PacketFormat.m_listDeserializeFormat,
                    member.Name, deserialize.Replace(Environment.NewLine, $"{Environment.NewLine}\t"));
            }
        }

        public static string ParseSerialize(Member member)
        {
            if (member.List == false)
            {
                return member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(PacketFormat.memberSerializeFormat, member.Type, member.Name),
                    "string" => string.Format(PacketFormat.memberSerializeStringFormat, member.Name, member.StringFormat),
                    _ => string.Format(PacketFormat.memberSerializeClassFormat, member.Name),
                };
            }
            else
            {
                string serialize = member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(
                                               PacketFormat.memberSerializeFormat,
                                               member.Type, temp),
                    "string" => string.Format(
                        PacketFormat.memberSerializeStringFormat,
                        temp, member.StringFormat),
                    _ => string.Format(
                        PacketFormat.memberSerializeClassFormat,
                        temp),
                };
                return string.Format(PacketFormat.listSerializeFormat,
                    member.Name,
                    member.Type,
                    serialize.Replace(Environment.NewLine, $"{Environment.NewLine}\t"));
            }

        }

        public static string ParseDeserialize(Member member)
        {
            if (member.List == false)
            {
                return member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(PacketFormat.memberDeserializeFormat, member.Name, BitConverterTo(member.Type), member.Type),
                    "string" => string.Format(PacketFormat.memberDeserializeStringFormat, member.Name, member.StringFormat),
                    _ => string.Format(PacketFormat.memberDeserializeClassFormat, member.Name),
                };
            }
            else
            {
                string deserialize = member.Type switch
                {
                    "char" or "bool" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" => string.Format(
                                               PacketFormat.listDeserializeAddFormat,
                                               member.Name, BitConverterTo(member.Type), member.Type),
                    "string" => string.Format(
                        PacketFormat.listDeserializeStringAddFormat,
                        member.Name, member.StringFormat),
                    _ => string.Format(
                        PacketFormat.listDeserializeClassAddFormat,
                        member.Type, member.Name),
                };

                return string.Format(
                    PacketFormat.listDeserializeFormat,
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
