using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;
using Case42.Base.ValueObjects;

namespace Case42.Base.Events
{
    public class SessionJoinedLobbyEvent : IEvent
    {
        public LobbySession Session { get; private set; }
        public SessionJoinedLobbyEvent(LobbySession session)
        {
            Session = session;
        }
    }
}
