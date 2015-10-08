using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;
using Case42.Base.ValueObjects;

namespace Case42.Base.Events
{
    public class ChallengeRespondedToEvent : IEvent
    {
        public ChallengeResponse Response { get; private set; }

        public ChallengeRespondedToEvent(ChallengeResponse response)
        {
            Response = response;
        }
    }
}
