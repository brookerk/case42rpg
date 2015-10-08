using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;
namespace Case42.Base.Commands
{
    public class SendLobbyMessageCommand : ICommand
    {
        public string Message { get; private set; }
        public SendLobbyMessageCommand(string message)
        {
            Message = message;
        }
    }
}