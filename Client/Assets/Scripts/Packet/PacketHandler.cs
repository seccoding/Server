using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

class PacketHandler
{
    public static void BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {
        BroadcastEnterGame pkt = packet as BroadcastEnterGame;
        ServerSession serverSession = session as ServerSession;
        PlayerManager.Instance.EnterGame(pkt);
    }

    public static void BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        BroadcastLeaveGame pkt = packet as BroadcastLeaveGame;
        ServerSession serverSession = session as ServerSession;
        PlayerManager.Instance.LeaveGame(pkt);
    }

    public static void PlayerListHandler(PacketSession session, IPacket packet)
    {
        PlayerList pkt = packet as PlayerList;
        ServerSession serverSession = session as ServerSession;
        PlayerManager.Instance.Add(pkt);
    }

    public static void BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        BroadcastMove pkt = packet as BroadcastMove;
        ServerSession serverSession = session as ServerSession;
        PlayerManager.Instance.Move(pkt);
    }

}
