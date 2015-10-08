using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Case42.Base.Commands;
using Case42.Server.Abstract;
using Case42.Server.Components;
using NHibernate;

namespace Case42.Server.CommandHandlers
{
    public class RespondToChallengeHandler : ICommandHandler<RespondToChallengeCommand>
    {
        public readonly ISession _database;
        public readonly IApplication _application;

        public RespondToChallengeHandler(ISession database, IApplication application)
        {
            _database = database;
            _application = application;
        }

        public void Handle(INetworkedSession session, CommandContext context, RespondToChallengeCommand command)
        {
            session.Registry.Get<ChallengeComponent>(challenge =>
            {
                challenge.Respond(command.Response);
            });
        }


    }
}
