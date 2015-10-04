
using Case42.Server.Abstract;
using Case42.Base.Commands;
using NHibernate;
using NHibernate.Linq;
using System.Linq;
using Case42.Server.Entities;

namespace Case42.Server.CommandHandlers
{
    public class LoginHandler : ICommandHandler<LoginCommand>
    {
        private readonly ISession _database;

        public LoginHandler(ISession database)
        {
            _database = database;
        }

        public void Handle(CommandContext context, LoginCommand command)
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

            context.SetResponse(new LoginResponse(user.Id, user.Email, user.Username));
        }

    }
}
