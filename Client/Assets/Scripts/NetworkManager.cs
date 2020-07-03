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
        IPAddress ipAddress = null;
        foreach (IPAddress address in ipHost.AddressList)
        {
            if(address.ToString().Contains("."))
                ipAddress = address;
            Debug.Log($"Address : {ipAddress}");
        }
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 17654); // 최종 주소
        
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
