using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;

namespace Case42.Base.Events
{
    public class SessionJoinedGameEvent : IEvent
    {
        public IEnumerable<uint> UserIds { get; private set; }

        public SessionJoinedGameEvent(IEnumerable<uint> userIds)
        {
            UserIds = userIds;
        }

    }
}

