using System;
using System.Net;

using ServerCoreTCP;
using ServerCoreTCP.Protobuf;

namespace TestClient
{
    public class ServerSession : Session
    {
        readonly Random random = new();
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected: {0}", endPoint);

            for(int i=0; i<5; i++)
            {
                Vector3 v = new()
                {
                    X = (float)random.NextDouble(),
                    Y = (float)random.NextDouble(),
                    Z = (float)random.NextDouble()
                };
                Send(v);
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
