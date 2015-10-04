using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Case42.Base.Commands;
using Case42.Server.Abstract;
using Case42.Server.Components;

namespace Case42.Server.CommandHandlers
{
    public class SendLobbyMessageHandler : ICommandHandler<SendLobbyMessageCommand>
    {
        private readonly IApplication _application;

        public SendLobbyMessageHandler(IApplication application)
        {
            _application = application;
        }

        public void Handle(INetworkedSession session, CommandContext context, SendLobbyMessageCommand command)
        {
            _application.Registry.Get<LobbyComponent>(lobby => lobby.SendMessage(session, command.Message));
        }
    }
}

