using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    public static void ServerChatHandler(PacketSession session, IPacket packet)
    {
        ServerChat chatPacket = packet as ServerChat;
        ServerSession serverSession = session as ServerSession;

        //if (chatPacket.playerId == 1)
            //Console.WriteLine(chatPacket.chat);
    }

    
}
