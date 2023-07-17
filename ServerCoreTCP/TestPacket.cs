using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    public class TClass : IPacket
    {
        public ushort id; // 2
        public float value; // 3
        public string msg; // 4
        public List<NestedT> list = new(); // 5
        public struct NestedT : IItemPacket
        {
            public uint itemNo;
            public string info;

            public NestedT(uint itemNo, string info)
            {
                this.itemNo = itemNo;
                this.info = info;
            }

            public void MDeserialize(Memory<byte> buffer, ref int offset)
            {
                itemNo = BitConverter.ToUInt32(buffer.Span.Slice(offset, sizeof(uint)));
                offset += sizeof(uint);

                ushort infoLen = BitConverter.ToUInt16(buffer.Span.Slice(offset, sizeof(ushort)));
                offset += sizeof(ushort);
                info = Encoding.Unicode.GetString(buffer.Span.Slice(offset, infoLen));
                offset += infoLen;
            }

            public bool MSerialize(Memory<byte> buffer, ref int offset)
            {
                bool suc = true;

                suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(uint)), itemNo);
                offset += sizeof(uint);

                if (string.IsNullOrEmpty(info) == false)
                {
                    byte[] s = Encoding.Unicode.GetBytes(info);
                    suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), (ushort)s.Length);
                    offset += sizeof(ushort);

                    int b = Encoding.Unicode.GetBytes(info, buffer.Span.Slice(offset, s.Length));
                    suc &= (b > 0);
                    offset += s.Length;
                }
                else
                {
                    suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), 0);
                    offset += sizeof(ushort);
                }

                return suc;
            }

            public void Deserialize(ReadOnlySpan<byte> span, ref int offset)
            {
                itemNo = BitConverter.ToUInt32(span.Slice(offset, sizeof(uint)));
                offset += sizeof(uint);

                ushort infoLen = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
                offset += sizeof(ushort);
                info = Encoding.Unicode.GetString(span.Slice(offset, infoLen));
                offset += infoLen;
            }

            public bool Serialize(Span<byte> span, ref int offset)
            {
                bool suc = true;

                suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(uint)), itemNo);
                offset += sizeof(uint);

                if (string.IsNullOrEmpty(info) == false)
                {
                    byte[] s = Encoding.Unicode.GetBytes(info);
                    suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), (ushort)s.Length);
                    offset += sizeof(ushort);

                    int b = Encoding.Unicode.GetBytes(info, span.Slice(offset, s.Length));
                    suc &= (b > 0);
                    offset += s.Length;
                }
                else
                {
                    suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), 0);
                    offset += sizeof(ushort);
                }

                return suc;
            }

            public override string ToString()
            {
                return $"[{itemNo}, {info}]";
            }
        }
        public PacketType Packet { get; private set; } = PacketType.TClass; // 1

        public void MDeserialize(Memory<byte> data)
        {
            int offset = 0;

            offset += sizeof(ushort);

            Packet = (PacketType)BitConverter.ToUInt16(data.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            id = BitConverter.ToUInt16(data.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            value = BitConverter.ToSingle(data.Span.Slice(offset, sizeof(float)));
            offset += sizeof(float);

            int msgLen = BitConverter.ToUInt16(data.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            if (msgLen > 0)
            {
                msg = Encoding.Unicode.GetString(data.Span.Slice(offset, msgLen));
                offset += msgLen;
            }

            list.Clear();
            ushort listCnt = BitConverter.ToUInt16(data.Span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            for (ushort i = 0; i < listCnt; i++)
            {
                NestedT t = new();
                t.MDeserialize(data, ref offset);
                list.Add(t);
            }
        }

        public Memory<byte> MSerialize()
        {
            Memory<byte> buffer = MSendBufferTLS.Reserve(4096);
            bool suc = true;
            int offset = 0;

            offset += sizeof(ushort); // the header size

            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), (ushort)Packet);
            offset += sizeof(ushort);

            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), id);
            offset += sizeof(ushort);

            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(float)), value);
            offset += sizeof(float);
            if (string.IsNullOrEmpty(msg) == false)
            {
                byte[] s = Encoding.Unicode.GetBytes(msg);

                // write the size of the string(msg) first
                suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), (ushort)s.Length);
                offset += sizeof(ushort);

                int b = Encoding.Unicode.GetBytes(msg, buffer.Span.Slice(offset, s.Length));
                suc &= (b > 0);
                offset += s.Length;
            }
            else
            {
                suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), 0);
                offset += sizeof(ushort);
            }

            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(offset, sizeof(ushort)), (ushort)list.Count);
            offset += sizeof(ushort);
            foreach (NestedT t in list)
            {
                suc &= t.MSerialize(buffer, ref offset);
            }

            // Set the header with the whole size of the data
            suc &= BitConverter.TryWriteBytes(buffer.Span.Slice(0, sizeof(ushort)), (ushort)offset);

            if (suc == false)
            {
                Console.WriteLine("Error when serializing");
                return null;
            }

            return MSendBufferTLS.Return(offset);
        }

        public override string ToString()
        {
            string s = "";
            s += $"Packet: {Packet}, id: {id}, value: {value}, msg: {msg}, cnt of list: {list.Count}\n";
            foreach (var l in list)
            {
                s += $"{l}, ";
            }
            return s;
        }

        public ArraySegment<byte> Serialize()
        {
            var buffer = SendBufferTLS.Reserve(4096);
            bool suc = true;
            int offset = 0;

            Span<byte> span = new(buffer.Array, buffer.Offset, buffer.Count);

            offset += sizeof(ushort); // the header size

            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), (ushort)Packet);
            offset += sizeof(ushort);

            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), id);
            offset += sizeof(ushort);

            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(float)), value);
            offset += sizeof(float);
            if (string.IsNullOrEmpty(msg) == false)
            {
                byte[] s = Encoding.Unicode.GetBytes(msg);

                // write the size of the string(msg) first
                suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), (ushort)s.Length);
                offset += sizeof(ushort);

                int b = Encoding.Unicode.GetBytes(msg, span.Slice(offset, s.Length));
                suc &= (b > 0);
                offset += s.Length;
            }
            else
            {
                suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), 0);
                offset += sizeof(ushort);
            }

            suc &= BitConverter.TryWriteBytes(span.Slice(offset, sizeof(ushort)), (ushort)list.Count);
            offset += sizeof(ushort);
            foreach (NestedT t in list)
            {
                suc &= t.MSerialize(buffer, ref offset);
            }

            // Set the header with the whole size of the data
            suc &= BitConverter.TryWriteBytes(span.Slice(0, sizeof(ushort)), (ushort)offset);

            if (suc == false)
            {
                Console.WriteLine("Error when serializing");
                return null;
            }

            return SendBufferTLS.Return(offset);
        }

        public void Deserialize(ReadOnlySpan<byte> span)
        {
            int offset = 0;

            offset += sizeof(ushort);

            Packet = (PacketType)BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            id = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);

            value = BitConverter.ToSingle(span.Slice(offset, sizeof(float)));
            offset += sizeof(float);

            int msgLen = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            if (msgLen > 0)
            {
                msg = Encoding.Unicode.GetString(span.Slice(offset, msgLen));
                offset += msgLen;
            }

            list.Clear();
            ushort listCnt = BitConverter.ToUInt16(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            for (ushort i = 0; i < listCnt; i++)
            {
                NestedT t = new();
                t.Deserialize(span, ref offset);
                list.Add(t);
            }
        }
    }
}
