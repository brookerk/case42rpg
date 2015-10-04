using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Case42.Base.Events;
using Case42.Base.ValueObjects;
using Case42.Server.Abstract;

namespace Case42.Server.Components
{
    public class LobbyComponent
    {
        private readonly HashSet<INetworkedSession> _sessions;

        public LobbyComponent()
        {
            _sessions = new HashSet<INetworkedSession>();
        }

        public bool Contains(INetworkedSession session)
        {
            return _sessions.Contains(session);
        }

        public void Join(INetworkedSession session)
        {
            if (_sessions.Contains(session))
                throw new OperationException("You may only join the lobby once");

            //inform other existing sessions that a new session has joined
            session.Publish(new JoinLobbyEvent(_sessions.Select(
                t => t.Registry.Get<AuthComponent, LobbySession>(
                    auth => new LobbySession(auth.Id, auth.Username)
                    )
                    )
                    )
                );

            //inform the rest in the lobby a new entry
            var lobbySession = session.Registry.Get<AuthComponent, LobbySession>(
                auth => new LobbySession(auth.Id, auth.Username)
                );
            foreach (var existingSession in _sessions)
                existingSession.Publish(new SessionJoinedLobbyEvent(lobbySession));

            _sessions.Add(session);
        }

        public void Leave(INetworkedSession session)
        {
            if (!_sessions.Remove(session))
                throw new OperationException("You were not in the lobby to begin with");

            //inform the rest in the lobby an entry had left
            var userId = session.Registry.Get<AuthComponent, uint>(auth => auth.Id);
            foreach (var existingSession in _sessions)
            {
                existingSession.Publish(new SessionLeftLobbyEvent(userId));
            }
        }

        public void SendMessage(INetworkedSession session, string message)
        {
            if (!_sessions.Contains(session))
                throw new InvalidOperationException("Cannot send a lobby message if you are not in the lobby");

            var userId = session.Registry.Get<AuthComponent, uint>(auth => auth.Id);

            foreach (var existingSession in _sessions.Where(t => t != session))
                existingSession.Publish(new LobbyMessageSendEvent(userId, message));
        }


    }
}
