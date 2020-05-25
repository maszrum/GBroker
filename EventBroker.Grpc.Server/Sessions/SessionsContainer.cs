using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EventBroker.Grpc.Server.Sessions
{
    internal class SessionsContainer : IEnumerable<Session>
    {
        private readonly ConcurrentDictionary<Guid, Session> _sessions = 
            new ConcurrentDictionary<Guid, Session>();

        public void Add(Session session)
        {
            _sessions.TryAdd(session.Id, session);
        }

        public void Remove(Guid id)
        {
            _sessions.TryRemove(id, out var session);
            session.Dispose();
        }

        public bool TryGetSession(Guid id, out Session session)
        {
            return _sessions.TryGetValue(id, out session);
        }

        public bool Exists(Guid id)
        {
            return _sessions.ContainsKey(id);
        }

        public IEnumerator<Session> GetEnumerator()
        {
            return _sessions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
