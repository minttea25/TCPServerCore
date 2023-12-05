using ServerCoreTCP.Job;
using ServerCoreTCP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    public class Server
    {
        static Server _instance = new();
        public static Server Instance => _instance;


        public NetworkManager _networkManager = null;
        readonly Thread updateThread = new(Update);
        public readonly static JobSerializerWithTimer jobs = new JobSerializerWithTimer();
        public static bool IsOn = false;
        Server()
        {
            _networkManager = new(() => { return SessionManager.Instance.CreateNewSession(); });

        }

        static void Update()
        {
            jobs.AddAfter(RoomManager.Instance.FlushRoom, 0);
            while (IsOn)
            {
                jobs.Flush();
            }
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Program.OnGoing = false;
        }

        public void StartServer()
        {
            _networkManager.StartServer();

            Program.OnGoing = true;
            updateThread.Start();
            IsOn = true;
        }

        public void StopServer()
        {
            _networkManager.StopServer();

            IsOn = false;
            updateThread.Join();
        }
    }
}