using System;
using System.Net;
using System.Threading;
using ServerCoreTCP;

using ServerCoreTCP.ProtobufWrapper;

namespace TestClient
{
    public class ServerSession : PacketSession
    {
        readonly Random random = new();
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);

            while (true)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector3 v = new()
                    {
                        X = (float)random.NextDouble(),
                        Y = (float)random.NextDouble(),
                        Z = (float)random.NextDouble()
                    };
                    Send(v);

                    Test1 t = new()
                    {
                        PlayerId = (uint)i,
                        PlayerName = $"playerasdfasdfasdf {i}"
                    };
                    Send(t);
                }
                Thread.Sleep(500);
            }
        }

        public override void OnDisconnected(EndPoint endPoint, object error = null)
        {
            Console.WriteLine("OnDisconnected: {0}", endPoint);
        }

        public override void OnRecv(ReadOnlySpan<byte> buffer)
        {

        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine("Sent: {0} bytes", numOfBytes);
        }
    }
}
