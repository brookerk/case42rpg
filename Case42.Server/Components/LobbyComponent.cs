using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Case42.Base.Events;
using Case42.Base.ValueObjects;
using Case42.Server.Abstract;
using Case42.Server.ValueObjects;

namespace Case42.Server.Components
{
    public class LobbyComponent
    {
        private readonly HashSet<INetworkedSession> _sessions;
        private readonly IApplication _application;
        private readonly List<Case42Game> _games;
        private readonly HashSet<INetworkedSession> _sessionInGame;

        public LobbyComponent(IApplication application)
        {
            _application = application;
            _sessions = new HashSet<INetworkedSession>();
            _games = new List<Case42Game>();
            _sessionInGame = new HashSet<INetworkedSession>();
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

            session.Registry.TryGet<ChallengeComponent>(challenge =>
            {
                challenge.Abort();
            });
        }

        public void SendMessage(INetworkedSession session, string message)
        {
            if (!_sessions.Contains(session))
                throw new InvalidOperationException("Cannot send a lobby message if you are not in the lobby");

            var userId = session.Registry.Get<AuthComponent, uint>(auth => auth.Id);

            foreach (var existingSession in _sessions.Where(t => t != session))
                existingSession.Publish(new LobbyMessageSendEvent(userId, message));
        }

        public bool CreateChallenge(INetworkedSession challenger, INetworkedSession challenged)
        {
            if (_sessionInGame.Contains(challenger) || _sessionInGame.Contains(challenged) ||
                challenged.Registry.Has<ChallengeComponent>() || challenger.Registry.Has<ChallengeComponent>())
                return false;

            var challengerUserId = challenger.Registry.Get<AuthComponent, uint>(t => t.Id);
            challenger.Registry.Set(new ChallengeComponent(_application, challenger, challenged, ChallengeDirection.Challenged));
            challenged.Registry.Set(new ChallengeComponent(_application, challenged, challenger, ChallengeDirection.Challenger));

            challenged.Publish(new ChallengeCreatedEvent(challengerUserId));

            return true;
        }

        public void AcceptChallenge(ChallengeComponent challenge)
        {
            _sessionInGame.Add(challenge.CurSession);
            _sessionInGame.Add(challenge.OtherSession);

            var game = new Case42Game(new[] { challenge.CurSession, challenge.OtherSession });
            var sessionInGame = new[] { challenge.CurSession, challenge.OtherSession };

            var sessionsJoinedEvent = new SessionJoinedGameEvent(sessionInGame.Select(s => s.Registry.Get<AuthComponent, uint>(t => t.Id)).ToList());
            foreach (var session in _sessions)
                session.Publish(sessionsJoinedEvent);

            _games.Add(game);

            challenge.Destroy();
        }

    }
}
