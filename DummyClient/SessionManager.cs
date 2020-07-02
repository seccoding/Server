using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    class SessionManager
    {
        public static SessionManager Instance { get; } = new SessionManager();

        List<ServerSession> _sessions = new List<ServerSession>();
        object _lock = new object();
        Random _rand = new Random();
        public ServerSession Generate()
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                _sessions.Add(session);
                return session;
            }
        }

        public void SendForEach()
        {
            lock (_lock)
            {
                foreach(ServerSession session in _sessions)
                {
                    Move movePacket = new Move();
                    movePacket.posX = _rand.Next(-50, 50);
                    movePacket.posY = _rand.Next(-50, 50); ;
                    movePacket.posZ = _rand.Next(-50, 50); ;

                    session.Send(movePacket.Write());
                }
            }
        }
    }
}
