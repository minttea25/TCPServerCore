using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketFactory
{
    class PacketFormat
    {
        /// <summary>
        /// [namespace, packetNameFormatList]
        /// </summary>
        public static readonly string basePacketFileFormat =
@"using System;

using ServerCoreTCP;

namespace {0}
{{
    public enum Packets : ushort
    {{
        {1}
    }}

    // Note: The first data is always the whole size of the data. (ushort)
    public interface IPacket
    {{
        public ushort PacketType {{ get; }}
        public Memory<byte> MSerialize();
        public ArraySegment<byte> Serialize();
        public void MDeserialize(Memory<byte> data);
        public void Deserialize(ReadOnlySpan<byte> span);
    }}

    public interface IItemPacket
    {{
        public bool MSerialize(Memory<byte> buffer, ref int offset);
        public bool Serialize(Span<byte> span, ref int offset);
        public void MDeserialize(Memory<byte> buffer, ref int offset);
        public void Deserialize(ReadOnlySpan<byte> span, ref int offset);
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
@"public class {0} : IItemPacket
{{
    {1}
    public bool MSerialize(Memory<byte> buffer, ref int offset)
    {{
        bool suc = true;

        {2}
        return suc;
    }}

    public void MDeserialize(Memory<byte> buffer, ref int offset)
    {{
        {3}
    }}

    public bool Serialize(Span<byte> span, ref int offset)
    {{
        bool suc = true;

        {4}
        return suc;
    }}

    public void Deserialize(ReadOnlySpan<byte> span, ref int offset)
    {{
        {5}
    }}
}}

";
    }
}
