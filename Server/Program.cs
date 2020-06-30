﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServerCore;

namespace Server
{

    class Knight
    {
        public int hp;
        public int attack;
    }

    class Server
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS (Domain Name Server)
            string host = Dns.GetHostName(); // Local PC 의 Host Name
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0]; // 여러개 중 하나. Domain 하나에 여러개의 IP가 물린다.
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777); // 최종 주소

            // 연결 Pipe 생성
            _listener.Init(endPoint, () => new ClientSession());
            Console.WriteLine("Listening...");

            while (true) { }

        }

    }
}
