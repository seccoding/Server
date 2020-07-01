﻿using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Server
{

    class ClientSession : PacketSession
    {
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");
            Server.Room.Push(() => Server.Room.Enter(this));
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if (Room != null)
            {
                GameRoom room = Room;
                room.Push(() => room.Leave(this));
                Room = null;
            }
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        // 클라이언트와 서버가 미리 정한 규약대로 패킷을 송/수신해야 함.
        // 예> 이동패킷 ((3, 2) 좌표로 이동하고 싶다!) 라는 패킷을 전송
        //      15 3 2 => 15번 유닛이 3,2로 이동.  
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }

    }

}
