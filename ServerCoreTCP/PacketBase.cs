using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace ServerCoreTCP
{
    public enum Packets : ushort
    {
        TestPacket = 1,
        TestPacket2 = 2,
        
    }

    // Note: The first data is always the whole size of the data. (ushort)
    public interface IPacket
    {
        public ushort PacketType { get; }
        public Memory<byte> MSerialize();
        public ArraySegment<byte> Serialize();
        public void MDeserialize(Memory<byte> data);
        public void Deserialize(ReadOnlySpan<byte> span);

        public static string ToString<T>(T pkt) where T : IPacket
        {
            StringBuilder sb = new();
            sb.AppendLine($"Packet: {(Packets) pkt.PacketType}");
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                var value = field.GetValue(pkt);
                
                sb.Append($"{field.Name}: ");
                
                if (value is null)
                {
                    sb.AppendLine("null");
                    continue;
                }
                
                if (field.FieldType.IsPrimitive)
                {
                    sb.AppendLine(value.ToString());
                }
                else if (value is IList list)
                {
                    sb.AppendLine("(List)");
                    sb.Append('[');
                    foreach (var o in list)
                    {
                        sb.Append($"{o}, ");
                    }
                    sb.AppendLine("]");
                }
                else if (value is PacketItem itemPacket)
                {
                    sb.AppendLine(itemPacket.ToString());
                }
                else
                {
                    sb.AppendLine("Unknown Type");
                }
            }

            return sb.ToString().TrimEnd();
        }
    }

    public abstract class PacketItem
    {
        public abstract bool MSerialize(Memory<byte> buffer, ref int offset);
        public abstract bool Serialize(Span<byte> span, ref int offset);
        public abstract void MDeserialize(Memory<byte> buffer, ref int offset);
        public abstract void Deserialize(ReadOnlySpan<byte> span, ref int offset);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{GetType().Name}[");
            foreach (var field in GetType().GetFields())
            {
                var value = field.GetValue(this);

                sb.Append($"{field.Name}: ");
                
                if (value is null)
                {
                    sb.Append("null, ");
                    continue;
                }

                if (value is IList list)
                {
                    sb.Append('[');
                    foreach (var i in list)
                    {
                        sb.Append($"{i}, ");
                    }
                    sb.Append("], ");
                }
                else
                {
                    sb.Append($"{value}, ");
                }
            }

            sb.Append(']');
            return sb.ToString();
        }
    }
}