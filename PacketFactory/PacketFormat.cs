using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PacketFactory
{
    class BufferFormat
    {
        protected const string BaseDir = "PacketFormats";

        public static readonly string PacketBase = $"{BaseDir}{Path.DirectorySeparatorChar}PacketBase.txt";
        public static readonly string Packet = $"{BaseDir}{Path.DirectorySeparatorChar}Packet.txt";
        public static readonly string PacketItem = $"{BaseDir}{Path.DirectorySeparatorChar}PacketItem.txt";
        public static readonly string PacketItemClass = $"{BaseDir}{Path.DirectorySeparatorChar}PacketItemClass.txt";
        public static readonly string PacketMember = $"{BaseDir}{Path.DirectorySeparatorChar}PacketMember.txt";
        public static readonly string PacketMemberList = $"{BaseDir}{Path.DirectorySeparatorChar}PacketMemberList.txt";
        public static readonly string PacketName = $"{BaseDir}{Path.DirectorySeparatorChar}PacketName.txt";
        public static readonly string PacketManager = $"{BaseDir}{Path.DirectorySeparatorChar}PacketManager.txt";
        public static readonly string PacketManagerMapping = $"{BaseDir}{Path.DirectorySeparatorChar}PacketManagerMapping.txt";
        public static readonly string PacketHandler = $"{BaseDir}{Path.DirectorySeparatorChar}PacketHandler.txt";
        public static readonly string PacketHandlerItem = $"{BaseDir}{Path.DirectorySeparatorChar}PacketHandlerItem.txt";

        protected static readonly string MemberSerialize = $"MemberSerialize.txt";
        protected static readonly string MemberSerializeClass = $"MemberSerializeClass.txt";
        protected static readonly string MemberSerializeList = $"MemberSerializeList.txt";
        protected static readonly string MemberSerializeString = $"MemberSerializeString.txt";

        protected const string MemberDeserialize = "MemberDeserialize.txt";
        protected const string MemberDeserializeAdd = "MemberDeserializeAdd.txt";
        protected const string MemberDeserializeAddClass = "MemberDeserializeAddClass.txt";
        protected const string MemberDeserializeAddString = "MemberDeserializeAddString.txt";
        protected const string MemberDeserializeClass = "MemberDeserializeClass.txt";
        protected const string MemberDeserializeList = "MemberDeserializeList.txt";
        protected const string MemberDeserializeString = "MemberDeserializeString.txt";

        public static Dictionary<string, string> ReadAllFiles<T>() where T : BufferFormat
        {
            Dictionary<string, string> dict = new();
            foreach (string file in GetAllFileNames<T>())
            {
                dict.Add(file, File.ReadAllText(file, System.Text.Encoding.UTF8));
            }
            return dict;
        }

        public static string[] GetAllFileNames<T>() where T : BufferFormat
        {
            FieldInfo[] infos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);
            var stringFields = infos.Where(field => field.FieldType == typeof(string));
            return stringFields.Select(field => (field.GetValue(null) as string)).ToArray();
        }
    }

    class ArraySegmentFormat : BufferFormat
    {
        private const string ArraySegmentDirPath = "ArraySegment";

        public static new readonly string MemberSerialize = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberSerialize}";
        public static new readonly string MemberSerializeClass = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberSerializeClass}";
        public static new readonly string MemberSerializeList = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberSerializeList}";
        public static new readonly string MemberSerializeString = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberSerializeString}";

        public static new readonly string MemberDeserialize = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserialize}";
        public static new readonly string MemberDeserializeAdd = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeAdd}";
        public static new readonly string MemberDeserializeAddClass = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeAddClass}";
        public static new readonly string MemberDeserializeAddString = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeAddString}";
        public static new readonly string MemberDeserializeClass = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeClass}";
        public static new readonly string MemberDeserializeList = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeList}";
        public static new readonly string MemberDeserializeString = $"{BaseDir}{Path.DirectorySeparatorChar}{ArraySegmentDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeString}";
    }

    class MemoryFormat : BufferFormat
    {
        private const string MemoryDirPath = "Memory";

        public static new readonly string MemberSerialize = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberSerialize}";
        public static new readonly string MemberSerializeClass = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberSerializeClass}";
        public static new readonly string MemberSerializeList = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberSerializeList}";
        public static new readonly string MemberSerializeString = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberSerializeString}";

        public static new readonly string MemberDeserialize = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserialize}";
        public static new readonly string MemberDeserializeAdd = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeAdd}";
        public static new readonly string MemberDeserializeAddClass = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeAddClass}";
        public static new readonly string MemberDeserializeAddString = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeAddString}";
        public static new readonly string MemberDeserializeClass = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeClass}";
        public static new readonly string MemberDeserializeList = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeList}";
        public static new readonly string MemberDeserializeString = $"{BaseDir}{Path.DirectorySeparatorChar}{MemoryDirPath}{Path.DirectorySeparatorChar}{BufferFormat.MemberDeserializeString}";
    }

    /// <summary>
    /// This class presents the text codes of the PacketFormats directory.
    /// It is written as UTF-16 (Unicode) and the text file is written as UTF-8.
    /// NOTE: ["] in UTF-8 is [""] in UTF-16.
    /// </summary>
    class PacketFormat
    {
        /// <summary>
        /// [packetNameFormatList]
        /// </summary>
        public static readonly string basePacketFileFormat =
@"using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace ServerCoreTCP.CustomBuffer
{{
    public enum Packets : ushort
    {{
        {0}
    }}

    // Note: The first data is always the whole size of the data. (ushort)
    public interface IPacket
    {{
        public ushort PacketType {{ get; }}
        public Memory<byte> MSerialize();
        public ArraySegment<byte> Serialize();
        public void MDeserialize(Memory<byte> data);
        public void Deserialize(ReadOnlySpan<byte> span);

        public static string ToString<T>(T pkt) where T : IPacket
        {{
            StringBuilder sb = new();
            sb.AppendLine($""Packet: {{(Packets) pkt.PacketType}}"");
            foreach (FieldInfo field in typeof(T).GetFields())
            {{
                var value = field.GetValue(pkt);
                
                sb.Append($""{{field.Name}}: "");
                
                if (value is null)
                {{
                    sb.AppendLine(""null"");
                    continue;
                }}
                
                if (field.FieldType.IsPrimitive)
                {{
                    sb.AppendLine(value.ToString());
                }}
                else if (value is IList list)
                {{
                    sb.AppendLine(""(List)"");
                    sb.Append('[');
                    foreach (var o in list)
                    {{
                        sb.Append($""{{o}}, "");
                    }}
                    sb.AppendLine(""]"");
                }}
                else if (value is PacketItem itemPacket)
                {{
                    sb.AppendLine(itemPacket.ToString());
                }}
                else
                {{
                    sb.AppendLine(""Unknown Type"");
                }}
            }}

            return sb.ToString().TrimEnd();
        }}
    }}

    public abstract class PacketItem
    {{
        public abstract bool MSerialize(Memory<byte> buffer, ref int offset);
        public abstract bool Serialize(Span<byte> span, ref int offset);
        public abstract void MDeserialize(Memory<byte> buffer, ref int offset);
        public abstract void Deserialize(ReadOnlySpan<byte> span, ref int offset);

        public override string ToString()
        {{
            var sb = new StringBuilder();
            sb.Append($""{{GetType().Name}}["");
            foreach (var field in GetType().GetFields())
            {{
                var value = field.GetValue(this);

                sb.Append($""{{field.Name}}: "");

                if (value is null)
                {{
                    sb.Append(""null, "");
                    continue;
                }}


                if (value is IList list)
                {{
                    sb.Append('[');
                    foreach (var i in list)
                    {{
                        sb.Append($""{{i}}, "");
                    }}
                    sb.Append(""], "");
                }}
                else
                {{
                    sb.Append($""{{value}}, "");
                }}
            }}

            sb.Append(']');
            return sb.ToString();
        }}
    }}
}}
";

        /// <summary>
        /// [name, value(ushort)]
        /// </summary>
        public static readonly string packetNameFormat =
@"{0} = {1},";

        /// <summary>
        /// [namespace, packetName, members, reserveBufferSize, m_memberSerialize, m_memberDeserialize, memberSerialize, memberDeserialize]
        /// </summary>
        public static readonly string packetFormat =
@"using System;
using System.Collections.Generic;
using System.Text;

using ServerCoreTCP;

namespace {0}
{{
    public class {1} : IPacket
    {{
        public ushort PacketType {{ get; private set; }} = (ushort)Packets.{1}; // 1st
        {2}
        public Memory<byte> MSerialize()
        {{
            Memory<byte> buffer = MSendBufferTLS.Reserve({3});
            bool suc = true;
            int offset = 0;

            offset += sizeof(ushort); // the header size

            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), PacketType);
            offset += sizeof(ushort);

            {4}
            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(0, sizeof(ushort)), (ushort)offset);

            return suc == true ? MSendBufferTLS.Return(offset) : null;
        }}
        
        public void MDeserialize(Memory<byte> buffer)
        {{
            int offset = 0;

            offset += sizeof(ushort);

            PacketType = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            {5}
        }}

        public ArraySegment<byte> Serialize()
        {{
            ArraySegment<byte> buffer = SendBufferTLS.Reserve({3});
            bool suc = true;
            int offset = 0;

            Span<byte> span = new(buffer.Array, buffer.Offset, buffer.Count);

            offset += sizeof(ushort);

            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), PacketType);
            offset += sizeof(ushort);

            {6}
            suc &= BitConverter.TryWriteBytes(span.Slice(0, sizeof(ushort)), (ushort)offset);

            return suc == true ? SendBufferTLS.Return(offset) : null;
        }}

        public void Deserialize(ReadOnlySpan<byte> span)
        {{
            int offset = 0;

            offset += sizeof(ushort);

            PacketType = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            {7}
        }}
    }}
}}
";

        /// <summary>
        /// [memberType, memberName]
        /// </summary>
        public static readonly string packetMemberFormat =
@"public {0} {1};";

        /// <summary>
        /// [classType, listName]
        /// </summary>
        public static readonly string packetMemberListFormat =
@"public List<{0}> {1} = new();";

        /// <summary>
        /// [memberType, memberName]
        /// </summary>
        public static readonly string m_memberSerializeFormat =
@"suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof({0})), {1});
offset += sizeof({0});";

        /// <summary>
        /// [memberName, stringFormat]
        /// </summary>
        public static readonly string m_memberSerializeStringFormat =
@"ushort {0}Len = (ushort)Encoding.{1}.GetBytes({0}, buffer.Span.Slice(offset + sizeof(ushort)));
suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), {0}Len);
offset += sizeof(ushort);
offset += {0}Len;";

        /// <summary>
        /// [memberName]
        /// </summary>
        public static readonly string m_memberSerializeClassFormat =
@"suc &= {0}.MSerialize(buffer, ref offset);";

        /// <summary>
        /// [memberName, memberType, m_serializeFormat]
        /// </summary>
        public static readonly string m_listSerializeFormat =
@"suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), (ushort){0}.Count);
offset += sizeof(ushort);
foreach ({1} _t in {0})
{{
    {2}
}}";

        /// <summary>
        /// [memberName, BitConverterFuncName, memberType]
        /// </summary>
        public static readonly string m_memberDeserializeFormat =
@"{0} = BitConverter.{1}(buffer.Span.Slice(offset, sizeof({2})));
offset += sizeof({2});";

        /// <summary>
        /// [memberName, stringFormat]
        /// </summary>
        public static readonly string m_memberDeserializeStringFormat =
@"int {0}Len = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
offset += sizeof(ushort);
{0} = Encoding.{1}.GetString(buffer.Span.Slice(offset, {0}Len));
offset += {0}Len;";

        /// <summary>
        /// [memberName]
        /// </summary>
        public static readonly string m_memberDeserializeClassFormat =
@"{0} = new();
{0}.MDeserialize(buffer, ref offset);";

        /// <summary>
        /// [memberName, m_deserializeFormat]
        /// </summary>
        public static readonly string m_listDeserializeFormat =
@"{0}.Clear();
ushort {0}Cnt = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
offset += sizeof(ushort);
for (ushort i = 0; i < {0}Cnt; ++i)
{{
    {1}
}}";

        /// <summary>
        /// [memberName, BitConverterFuncName, memberType]
        /// </summary>
        public static readonly string m_listDeserializeAddFormat =
@"{0}.Add(BitConverter.{1}(buffer.Span.Slice(offset, sizeof({2}))));
offset += sizeof({2});";

        /// <summary>
        /// [memberType, memberName]
        /// </summary>
        public static readonly string m_listDeserializeClassAddFormat =
@"{0} _t = new();
_t.MDeserialize(buffer, ref offset);
{1}.Add(_t);";

        /// <summary>
        /// [memberName, stringFormat]
        /// </summary>
        public static readonly string m_listDeserializeStringAddFormat =
@"int _len = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
offset += sizeof(ushort);
{0}.Add(Encoding.{1}.GetString(buffer.Span.Slice(offset, _len)));
offset += _len;";



        /// <summary>
        /// [memberType, memberName]
        /// </summary>
        public static readonly string memberSerializeFormat =
@"suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof({0})), {1});
offset += sizeof({0});";

        /// <summary>
        /// [memberName,stringFormat]
        /// </summary>
        public static readonly string memberSerializeStringFormat =
@"ushort {0}Len = (ushort)Encoding.{1}.GetBytes({0}, span.Slice(offset + sizeof(ushort)));
suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), {0}Len);
offset += sizeof(ushort);
offset += {0}Len;";

        /// <summary>
        /// [memberName]
        /// </summary>
        public static readonly string memberSerializeClassFormat =
@"suc &= {0}.Serialize(buffer, ref offset);";

        /// <summary>
        /// [memberName, memberType, serializeFormat]
        /// </summary>
        public static readonly string listSerializeFormat =
@"suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), (ushort){0}.Count);
offset += sizeof(ushort);
foreach ({1} _t in {0})
{{
    {2}
}}";

        /// <summary>
        /// [memberName, BitConverterFuncName, memberType]
        /// </summary>
        public static readonly string memberDeserializeFormat =
@"{0} = BitConverter.{1}(span.Slice(offset, sizeof({2})));
offset += sizeof({2});";

        /// <summary>
        /// [memberName, stringFormat]
        /// </summary>
        public static readonly string memberDeserializeStringFormat =
@"int {0}Len = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
offset += sizeof(ushort);
{0} = Encoding.{1}.GetString(span.Slice(offset, {0}Len));
offset += {0}Len;";

        /// <summary>
        /// [memberName, stringFormat]
        /// </summary>
        public static readonly string memberDeserializeClassFormat =
@"{0} = new();
{0}.Deserialize(span, ref offset);";

        /// <summary>
        /// [memberName, deserializeFormat]
        /// </summary>
        public static readonly string listDeserializeFormat =
@"{0}.Clear();
ushort {0}Cnt = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
offset += sizeof(ushort);
for (ushort i = 0; i < {0}Cnt; ++i)
{{
    {1}
}}";

        /// <summary>
        /// [memberName, BitConverterFuncName, memberType]
        /// </summary>
        public static readonly string listDeserializeAddFormat =
@"{0}.Add(BitConverter.{1}(span.Slice(offset, sizeof({2}))));
offset += sizeof({2});";

        /// <summary>
        /// [memberName, memberType, deserializeFormat]
        /// </summary>
        public static readonly string listDeserializeClassAddFormat =
@"{0} _t = new();
_t.Deserialize(span, ref offset);
{1}.Add(_t);
";

        /// <summary>
        /// [memberName, stringFormat]
        /// </summary>
        public static readonly string listDeserializeStringAddFormat =
@"int _len = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
offset += sizeof(ushort);
{0}.Add(Encoding.{1}.GetString(span.Slice(offset, _len)));
offset += _len;";

        /// <summary>
        /// [namespace, items]
        /// </summary>
        public static readonly string itemsFormat =
@"using System;
using System.Collections.Generic;
using System.Text;

using ServerCoreTCP;

namespace {0} 
{{
    {1}
}}
";

        /// <summary>
        /// [className, members, m_memberSerialize, m_memberDeserialize, memberSerialize, memberDeserialize]
        /// </summary>
        public static readonly string packetItemClassFormat =
@"public class {0} : PacketItem
{{
    {1}
    public override bool MSerialize(Memory<byte> buffer, ref int offset)
    {{
        bool suc = true;

        {2}
        return suc;
    }}

    public override void MDeserialize(Memory<byte> buffer, ref int offset)
    {{
        {3}
    }}

    public override bool Serialize(Span<byte> span, ref int offset)
    {{
        bool suc = true;

        {4}
        return suc;
    }}

    public override void Deserialize(ReadOnlySpan<byte> span, ref int offset)
    {{
        {5}
    }}
}}

";

        /// <summary>
        /// [namespace, packetMapping]
        /// </summary>
        public static readonly string packetManagerFormat =
@"#define MEMORY_BUFFER

using System;
using System.Collections.Generic;

using ServerCoreTCP;

namespace {0}
{{
    public class PacketManager
    {{
        #region Singleton
        static PacketManager _instance = new();
        public static PacketManager Instance {{ get {{ return _instance; }} }}
        #endregion

#if MEMORY_BUFFER
        /// <summary>
        /// Key is Packets(ushort); Value: the func returns a created packet with the received buffer.
        /// </summary>
        Dictionary<ushort, Func<Session, Memory<byte>, IPacket>> _packetFactory = new();
#else
        /// <summary>
        /// Key is Packets(ushort); Value: the func returns a created packet with the received buffer.
        /// </summary>
        Dictionary<ushort, Func<Session, ArraySegment<byte>, IPacket>> _packetFactory = new();
#endif
        /// <summary>
        /// Value: action which handles the packet(ushort, Packets).
        /// </summary>
        Dictionary<ushort, Action<IPacket, Session>> _handlers = new();

        public PacketManager()
        {{
            {1}
        }}
#if MEMORY_BUFFER
        public void OnRecvPacket(Session session, Memory<byte> buffer, Action<Session, IPacket> callback = null)
        {{
            int offset = 0;

            // The whole size of the received packet
            ushort size = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            // The type of the received packet
            ushort pkt = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            if (_packetFactory.TryGetValue(pkt, out var factory))
            {{
                IPacket packet = factory.Invoke(session, buffer);

                callback?.Invoke(session, packet);

                HandlePacket(packet, session);
            }}
        }}

        static T MakePacket<T>(Session session, Memory<byte> buffer) where T : IPacket, new()
        {{
            T packet = new();
            packet.MDeserialize(buffer);
            return packet;
        }}
#else
        public void OnRecvPacket(Session session, ArraySegment<byte> buffer, Action<Session, IPacket> callback = null)
        {{
            int offset = 0;

            // The whole size of the received packet
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + offset);
            offset += sizeof(ushort);

            // The type of the received packet
            ushort pkt = BitConverter.ToUInt16(buffer.Array, buffer.Offset + offset);
            offset += sizeof(ushort);

            if (_packetFactory.TryGetValue(pkt, out var factory))
            {{
                IPacket packet = factory.Invoke(session, buffer);

                callback?.Invoke(session, packet);

                HandlePacket(packet, session);
            }}
        }}

        static T MakePacket<T>(Session session, ArraySegment<byte> buffer) where T : IPacket, new()
        {{
            T packet = new();
            packet.Deserialize(buffer);
            return packet;
        }}
#endif

        void HandlePacket(IPacket packet, Session session)
        {{
            if (_handlers.TryGetValue(packet.PacketType, out var action))
            {{
                action.Invoke(packet, session);
            }}
        }}
    }}
}}
";

        /// <summary>
        /// [packetType]
        /// </summary>
        public static readonly string packetManagerMappingFormat =
@"_packetFactory.Add((ushort)Packets.{0}, MakePacket<{0}>);
_handlers.Add((ushort)Packets.{0}, PacketHandler.{0}Handler);
";

        /// <summary>
        /// [namespace, handlerFormat]
        /// </summary>
        public static readonly string packetHandlerFormat =
@"using System;

using ServerCoreTCP;

namespace {0}
{{
    public class PacketHandler
    {{
        {1}
    }}
}}
";

        /// <summary>
        /// [packetType]
        /// </summary>
        public static readonly string packetHandlerItemFormat =
@"public static void {0}Handler(IPacket packet, Session session)
{{
    {0} pkt = packet as {0};

    // TODO
}}
";
    }
}


