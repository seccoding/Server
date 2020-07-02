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

            if (_pendingList.Count > 0)
                Console.WriteLine($"Flushed {_pendingList.Count} Items");

            _pendingList.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session)
        {
            // 플레이어 추가하고
            _sessions.Add(session);
            session.Room = this;

            // 신입 플레이어에게 모든 플레이어 목록 전송
            PlayerList players = new PlayerList();
            foreach (ClientSession s in _sessions)
            {
                players.players.Add(new PlayerList.Player() {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    position = new PlayerList.Player.Position()
                    {
                        posX = s.PosX,
                        posY = s.PosY,
                        posZ = s.PosZ
                    }
                });
            }
            session.Send(players.Write());

            // 신입 플레이어의 입장을 모두에게 알린다.
            BroadcastEnterGame enter = new BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.position.posX = 0;
            enter.position.posY = 0;
            enter.position.posZ = 0;

            Broadcast(enter.Write());
        }

        public void Leave(ClientSession session)
        {
            // 플레이어를 제거하고
            _sessions.Remove(session);

            // 모두에게 알린다.
            BroadcastLeaveGame leave = new BroadcastLeaveGame();
            leave.playerId = session.SessionId;

            Broadcast(leave.Write());
        }

        public void Move(ClientSession session, Move packet)
        {
            // 좌표를 바꾸어주고
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            // 모두에게 알린다.
            BroadcastMove move = new BroadcastMove();
            move.playerId = session.SessionId;
            move.position.posX = session.PosX;
            move.position.posY = session.PosY;
            move.position.posZ = session.PosZ;
            Broadcast(move.Write());
        }

    }
}
