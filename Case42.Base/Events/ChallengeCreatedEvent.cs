using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;

namespace Case42.Base.Events
{
    public class ChallengeCreatedEvent : IEvent
    {
        public uint ChallengerUserId { get; private set; }

        public ChallengeCreatedEvent(uint challengerUserId)
        {
            ChallengerUserId = challengerUserId;
        }



    }
}
