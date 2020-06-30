using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Server
{
    abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
        public string name;

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                count += sizeof(ushort);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                count += sizeof(float);
                return success;
            }

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);
                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);
            }

        }

        public List<SkillInfo> skills = new List<SkillInfo>();

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            count += sizeof(ushort);

            // Client가 1024만큼의 Packet을 전달하고 PacketSize는 4로 전달되었을 때, 이상없이 동작되는 문제가 발생했으므로
            // Client가 보내온 Packet의 Size에서 정확히 추출할 수 있도록 한다.
            // 즉 PacketSize가 잘못 전달되었을 경우 Exception을 발생시키도록 한다.
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            // string
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            // skill list
            skills.Clear();

            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            for (int i = 0; i < skillLen; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(s, ref count);
                skills.Add(skill);
            }
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);

            //string
            //C# (Default UTF-16)
            //ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name); // string type의 byte 길이
            //success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            //count += sizeof(ushort);
            //Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
            //count += nameLen;

            // __abcd 데이터를 먼저 넣고 (데이터 개수가 들어갈 공간을 확보해둔다)
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            // 08abcd 사이즈를 나중에 넣는다.
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            //skill list
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);
            foreach (SkillInfo skill in skills)
                success &= skill.Write(s, ref count);

            // PacketSize 는 가장 마지막에 처리해야 한다.
            success &= BitConverter.TryWriteBytes(s, count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);
        }
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2
    }

    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");
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
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            ushort packetId = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch((PacketID) packetId)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);
                        Console.WriteLine($"PlayerInfoReq: {p.playerId}, {p.name}");

                        foreach(PlayerInfoReq.SkillInfo skill in p.skills)
                            Console.WriteLine($"Skill({skill.id})({skill.level})({skill.duration})");
                    }
                    break;
                case PacketID.PlayerInfoOk:
                    break;
            }
            Console.WriteLine($"ReceivePacketId: {packetId}, ReceiveSize: {dataSize}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }

    }

}
