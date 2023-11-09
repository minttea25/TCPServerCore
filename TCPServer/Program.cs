using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Serilog;
using Serilog.Core;
using ServerCoreTCP;
using ServerCoreTCP.LoggerDebug;
using ServerCoreTCP.Utils;

namespace TCPServer
{
    class Program
    {
        public readonly static Logger Logger = new LoggerConfiguration().WriteTo.File(
            path: LoggerHelper.GetFileName("ServerLogs"),
            encoding: Encoding.Unicode,
            flushToDiskInterval: TimeSpan.FromSeconds(1)
            ).CreateLogger();

        static void FlushRoom()
        {
            foreach (var room in Rooms.Values)
            {
                room.AddJob(() => room.Flush());
            }
            JobTimer.Instance.Push(FlushRoom, 500);
        }
        readonly static object _lock = new object();
        public static IReadOnlyDictionary<uint, Room> Rooms => _rooms;
        readonly static Dictionary<uint, Room> _rooms = new Dictionary<uint, Room>();

        public static void AddRoom(uint roomNo, Room room)
        {
            lock (_lock)
            {
                _rooms.Add(roomNo, room);
            }
        }

        static void Main(string[] args)
        {
#if DEBUG
            CoreLogger.Logging = true;
#else
            CoreLogger.Logger = new LoggerConfiguration().WriteTo.File(
                path: LoggerHelper.GetFileName("CoreLogs"),
                encoding: Encoding.Unicode,
                flushToDiskInterval: TimeSpan.FromSeconds(1)
            ).CreateLogger();
#endif

            string host = Dns.GetHostName(); // local host name of my pc
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address: ipAddr, port: 8888);

            Listener listener = new Listener(endPoint, () => { return SessionManager.Instance.CreateNewSession(); });
            listener.Listen(register: 10, backLog: 100);

            // exec right now
            JobTimer.Instance.Push(FlushRoom, 0);

            bool onGoing = true;

            Thread t = new Thread(() =>
            {
                while (onGoing)
                {
                    JobTimer.Instance.Flush();
                }
                Console.WriteLine("Server stopped.");
            });
            t.Start();

            while (true)
            {
                string s = Console.ReadLine();
                if (s.Equals("stop")) break;
            }

            onGoing = false;
            t.Join();

            Logger.Dispose();
            Console.WriteLine("Server is closed.");
        }
    }
}
