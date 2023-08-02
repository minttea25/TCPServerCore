using System;
using System.IO;

namespace ProtoMessageFactory
{
    class GenFormat
    {
        const string BaseDir = "ProtoMessageFactory";

        public static readonly string PacketManagerBase = $"{BaseDir}{Path.DirectorySeparatorChar}ProtoMessagePacketManager.txt";
        public static readonly string PacketHandlerBase = $"{BaseDir}{Path.DirectorySeparatorChar}ProtoMessagePacketHandler.txt";

        public static readonly string PacketManagerItemBase = $"{BaseDir}{Path.DirectorySeparatorChar}ProtoMessagePacketManagerItem.txt";
        public static readonly string PacketHandlerItemBase = $"{BaseDir}{Path.DirectorySeparatorChar}ProtoMessagePacketHandlerItem.txt";

    }

    class Program
    {
        const string HandlerBaseFormatFile = "{0}PacketHandler.cs";
        const string ManagerBaseFormatFile = "{0}PacketManager.cs";

        static void Main(string[] args)
        {
        }
    }
}
