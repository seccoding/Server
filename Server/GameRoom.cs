using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(ClientSession clientSession, string chat)
        {
            ServerChat packet = new ServerChat();
            packet.playerId = clientSession.SessionId;
            packet.chat = $"{chat} | I am {packet.playerId}";

            ArraySegment<byte> segment = packet.Write();

            lock (_lock)
            {
                foreach (ClientSession session in _sessions)
                    session.Send(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }
        }
    }
}
