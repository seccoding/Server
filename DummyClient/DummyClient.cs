using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{

    class DummyClient
    {
        static void Main(string[] args)
        {
            //string host = Dns.GetHostName(); // Local PC 의 Host Name
            string host = "192.168.0.40"; // Local PC 의 Host Name
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0]; // 여러개 중 하나. Domain 하나에 여러개의 IP가 물린다.
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777); // 최종 주소

            Connector connector = new Connector();
            connector.Connect(endPoint, () => SessionManager.Instance.Generate(), 50);

            while (true)
            {  
                try
                {
                    SessionManager.Instance.SendForEach();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(250);
            }
        }
    }
}
