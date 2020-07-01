using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class SessionManager
    {
        public static SessionManager Instance { get; } = new SessionManager();

        int _sessionId = 0;
        Dictionary<int, Session> _sessions = new Dictionary<int, Session>();

        object _lock = new object();

        public T Generate<T>() where T : Session, new()
        {
            lock (_lock)
            {
                T session = new T();
                int sessionId = ++_sessionId;
                session.SessionId = sessionId;
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Connected : {sessionId}");

                return session;
            }
        }

        public Session Find(int id)
        {
            lock (_lock)
            {
                Session session = null;
                _sessions.TryGetValue(id, out session);
                return session;
            }
        }

        public void Remove(Session session)
        {
            lock (_lock)
            {
                _sessions.Remove(session.SessionId);
            }
        }
    }
}
