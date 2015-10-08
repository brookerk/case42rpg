using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;
namespace Case42.Base.Commands
{
    public class ChallengePlayerCommand : ICommand
    {
        public uint UserId { get; private set; }
        public ChallengePlayerCommand(uint userid)
        {
            UserId = userid;
        }
    }
}
