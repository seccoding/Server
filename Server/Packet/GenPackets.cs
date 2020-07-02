using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public enum PacketID
{
    ClientChat = 1,
	ServerChat = 2,
	
}

interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}

class ClientChat : IPacket
{
    public string chat;
	

    public ushort Protocol { get { return (ushort) PacketID.ClientChat; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);
        ushort chatLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(segment.Array, segment.Offset + count, chatLen);
		count += chatLen;
		
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort) PacketID.ClientChat), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        ushort chatLen = (ushort) Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(chatLen), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		count += chatLen;
		
        
        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}
class ServerChat : IPacket
{
    public int playerId;
	public string chat;

    public ushort Protocol { get { return (ushort) PacketID.ServerChat; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt32(segment.Array, segment.Offset + count);
		count += sizeof(int);
		ushort chatLen = BitConverter.ToUInt16(segment.Array, segment.Offset + count);
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(segment.Array, segment.Offset + count, chatLen);
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes((ushort) PacketID.ServerChat), 0, segment.Array, segment.Offset + count, sizeof(ushort));
        count += sizeof(ushort);
        Array.Copy(BitConverter.GetBytes(playerId), 0, segment.Array, segment.Offset + count, sizeof(int));
		count += sizeof(int);
		ushort chatLen = (ushort) Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(chatLen), 0, segment.Array, segment.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		count += chatLen;
        
        Array.Copy(BitConverter.GetBytes(count), 0, segment.Array, segment.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

