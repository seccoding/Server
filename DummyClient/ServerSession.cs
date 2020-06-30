using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DummyClient
{
    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            // Send Data
            for (int i = 0; i < 5; i++)
            {

                Packet packet = new Packet() { size = 4, packetId = 7 };

                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
                byte[] sizeBuffer = BitConverter.GetBytes(packet.size);
                Array.Copy(sizeBuffer, 0, openSegment.Array, openSegment.Offset, sizeBuffer.Length);
                byte[] packetIdBuffer = BitConverter.GetBytes(packet.packetId);
                Array.Copy(packetIdBuffer, 0, openSegment.Array, openSegment.Offset + sizeBuffer.Length, packetIdBuffer.Length);
                ArraySegment<byte> sendBuff = SendBufferHelper.Close(sizeBuffer.Length + packetIdBuffer.Length);

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
}
