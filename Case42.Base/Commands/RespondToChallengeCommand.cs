using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;
using Case42.Base.ValueObjects;

namespace Case42.Base.Commands
{
    public class RespondToChallengeCommand : ICommand
    {
        public ChallengeResponse Response { get; private set; }
        public RespondToChallengeCommand(ChallengeResponse response)
        {
            Response = response;
        }

    }
}
