using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;
using Case42.Base.ValueObjects;

namespace Case42.Base.Events
{
    public class JoinLobbyEvent : IEvent
    {

        public IEnumerable<LobbySession> Sessions { get; set; }

        public JoinLobbyEvent(IEnumerable<LobbySession> session)
        {
            Sessions = session;
        }

    }
}
