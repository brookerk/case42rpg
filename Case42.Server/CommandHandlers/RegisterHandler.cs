
using Case42.Server.Abstract;
using Case42.Base.Commands;
using NHibernate;
using NHibernate.Linq;
using System.Linq;
using Case42.Server.Entities;
using System;
using Case42.Server.Components;
using Case42.Server.ValueObjects;

namespace Case42.Server.CommandHandlers
{
    public class RegisterHandler : ICommandHandler<RegisterCommand>
    {
        private readonly ISession _database;
        private readonly IApplication _application;

        public RegisterHandler(ISession database, IApplication application)
        {
            _database = database;
            _application = application;
        }

        public void Handle(INetworkedSession session, CommandContext context, RegisterCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password) || string.IsNullOrWhiteSpace(command.Email))
            {
                context.RaiseOperationError("All fields are required");
                return;
            }

            if (command.Username.Length > 128)
            {
                context.RaisePropertyError("Username", "Must be les than 128 characters long");
                return;
            }

            if (command.Email.Length > 200)
            {
                context.RaisePropertyError("Email", "Must be less than 200 characters long");
                return;
            }

            if (_database.Query<User>().Any(t => t.Username == command.Username || t.Email == command.Email))
            {
                context.RaiseOperationError("Username and email must be unique");
                return;
            }

            var user = new User
            {
                Username = command.Username,
                Email = command.Email,
                CreatedAt = DateTime.UtcNow,
                Password = HashedPassword.fromPlainText(command.Password)
            };

            _database.Save(user);
            session.Registry.Set(new AuthComponent(user.Id, user.Username, user.Email));

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

            context.SetResponse(new RegisterResponse(user.Id));

        }
    }
}

