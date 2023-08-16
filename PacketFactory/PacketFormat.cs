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
}


