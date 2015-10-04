using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;

namespace Case42.Base.Events
{
    public class SessionLeftLobbyEvent : IEvent
    {
        public uint UserId { get; private set; }

        public SessionLeftLobbyEvent(uint userId)
        {
            UserId = userId;
        }
    }
}
