
using Case42.Server.Abstract;
using Case42.Base.Commands;
using NHibernate;
using NHibernate.Linq;
using System.Linq;
using Case42.Server.Components;
using Case42.Server.Entities;

namespace Case42.Server.CommandHandlers
{
    public class LoginHandler : ICommandHandler<LoginCommand>
    {
        private readonly ISession _database;
        private readonly IApplication _application;
        private static readonly object AuthLock = new object();

        public LoginHandler(ISession database, IApplication application)
        {
            _database = database;
            _application = application;
        }

        public void Handle(INetworkedSession session, CommandContext context, LoginCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
            {
                context.RaisePropertyError("Email", "Required");
                return;
            }

            if (string.IsNullOrWhiteSpace(command.Password))
            {
                context.RaisePropertyError("Password", "Required");
                return;
            }

            var user = _database.Query<User>().SingleOrDefault(s => s.Email == command.Email);
            if (user == null || !user.Password.EqualsPlainText(command.Password))
            {
                context.RaiseOperationError("Invalid email or password");
                return;
            }

            //this part of the code need to be thread safe in the event where two logged in happen almost at the same time
            lock (AuthLock)
            {
                //check if any one is logged in using the same id
                if (_application.Sessions.Any(s => s.Registry.TryGet<AuthComponent, bool>(auth => auth.Id == user.Id)))
                {
                    context.RaiseOperationError("You cannot log in more than once");
                    return;
                }

                session.Registry.Set(new AuthComponent(user.Id, user.Username, user.Email));
            }

            //try to join a session
            try
            {
                //lobby.Join will throw operationException if fail
                _application.Registry.Get<LobbyComponent>(lobby => lobby.Join(session));
            }
            catch (OperationException ex)
            {
                context.RaiseOperationError(ex.Message);
                return;
            }

            context.SetResponse(new LoginResponse(user.Id, user.Email, user.Username));
        }

    }
}
