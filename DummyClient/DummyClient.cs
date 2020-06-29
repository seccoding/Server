using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            // Send Data
            for (int i = 0; i < 5; i++)
            {
                byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello Server!{i}");
                this.Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            String recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }

    }

    class DummyClient
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName(); // Local PC 의 Host Name
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0]; // 여러개 중 하나. Domain 하나에 여러개의 IP가 물린다.
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777); // 최종 주소

            Connector connector = new Connector();
            connector.Connect(endPoint, () => new GameSession());

            while (true)
            {  
                try
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(100);
            }
        }
    }
}
