using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession session in _sessions)
                session.Send(_pendingList);

            Console.WriteLine($"Flushed {_pendingList.Count} Items");
            _pendingList.Clear();
        }

        public void Broadcast(ClientSession clientSession, string chat)
        {
            ServerChat packet = new ServerChat();
            packet.playerId = clientSession.SessionId;
            packet.chat = $"{chat} => I am {packet.playerId}";

            _pendingList.Add(packet.Write());

        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
             _sessions.Remove(session);
        }

    }
}
