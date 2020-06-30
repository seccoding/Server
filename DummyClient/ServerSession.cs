using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DummyClient
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

            for ( int i = 0; i < skillLen; i++ )
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
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort) PacketID.PlayerInfoReq);
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
            ushort nameLen = (ushort) Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            // 08abcd 사이즈를 나중에 넣는다.
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            //skill list
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort) skills.Count);
            count += sizeof(ushort);
            foreach(SkillInfo skill in skills)
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

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name="ABCD" };
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 101, duration = 3.0f, level = 1});
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 102, duration = 4.0f, level = 2});
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 103, duration = 5.0f, level = 3});
            packet.skills.Add(new PlayerInfoReq.SkillInfo() { id = 104, duration = 6.0f, level = 4});

            // Send Data
            for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = packet.Write();
                if ( s != null )
                    this.Send(s);
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
