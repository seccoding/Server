using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServerCore;

namespace Server
{

    class Server
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 250);
        }

        static void Main(string[] args)
        {
            //string host = Dns.GetHostName(); // Local PC 의 Host Name
            string host = "192.168.0.40"; // Local PC 의 Host Name
            Console.WriteLine($"Host: {host}");
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0]; // 여러개 중 하나. Domain 하나에 여러개의 IP가 물린다.

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 17654); // 최종 주소

            // 연결 Pipe 생성
            _listener.Init(endPoint, () => SessionManager.Instance.Generate<ClientSession>());
            Console.WriteLine("Listening...");

            FlushRoom();

            while (true) 
            {
                JobTimer.Instance.Flush();
            }

        }

    }
}
