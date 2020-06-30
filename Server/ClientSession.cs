using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
    class ClientSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            Knight knight = new Knight() { hp = 100, attack = 10 };

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] hpBuffer = BitConverter.GetBytes(knight.hp);
            Array.Copy(hpBuffer, 0, openSegment.Array, openSegment.Offset, hpBuffer.Length);
            byte[] attackBuffer = BitConverter.GetBytes(knight.attack);
            Array.Copy(attackBuffer, 0, openSegment.Array, openSegment.Offset, attackBuffer.Length);

            ArraySegment<byte> sendBuff = SendBufferHelper.Close(hpBuffer.Length + attackBuffer.Length);
            this.Send(sendBuff);

            // User가 같은 Zone 안에 100명이 있다.
            // User들이 자유롭게 움직인다. -> 99명의 유저에게 이동패킷을 전달해야 한다.
            // 100명의 유저가 모두 자유롭게 움직인다. - 전송되어야 할 패킷의 수 = 100 X 100
            // 10000번의 복사가 이루어져야 한다.
            // 따라서 1번의 계산으로 모든 유저에게 동일한 버퍼를 전송하는 것이 성능상 유리하다.


            Thread.Sleep(1000);
            this.Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }


        // 클라이언트와 서버가 미리 정한 규약대로 패킷을 송/수신해야 함.
        // 예> 이동패킷 ((3, 2) 좌표로 이동하고 싶다!) 라는 패킷을 전송
        //      15 3 2 => 15번 유닛이 3,2로 이동.  
        public override int OnRecv(ArraySegment<byte> buffer)
        {
            String recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }

    }

}
