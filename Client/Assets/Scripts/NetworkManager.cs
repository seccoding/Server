using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();

    public void Send(ArraySegment<byte> sendBuffer)
    {
        _session.Send(sendBuffer);
    }

    void Start()
    {

        //string host = Dns.GetHostName(); // Local PC 의 Host Name
        string host = "192.168.0.40"; // Local PC 의 Host Name

        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddress = ipHost.AddressList[0]; // 여러개 중 하나. Domain 하나에 여러개의 IP가 물린다.
        int ipPort = Convert.ToInt16(7777);

        IPEndPoint endPoint = new IPEndPoint(ipAddress, ipPort); // 최종 주소
        
        Connector connector = new Connector();
        connector.Connect(endPoint, () => _session, 1);

    }

    void Update()
    {
        List<IPacket> list = PacketQueue.Instance.PopAll();
        foreach(IPacket packet in list)
            PacketManager.Instance.HandlePacket(_session, packet);
    }

}
