using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;

namespace Case42.Base.Events
{
    public class LobbyMessageSendEvent : IEvent
    {
        public uint UserId { get; private set; }
        public string Message { get; private set; }

        public LobbyMessageSendEvent(uint userId, string message)
        {
            UserId = userId;
            Message = message;
        }

    }
}
