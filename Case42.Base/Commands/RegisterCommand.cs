using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;

namespace Case42.Base.Commands
{
    public class RegisterCommand : ICommand
    {
        public string Email { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public RegisterCommand(string email, string username, string password)
        {
            Email = email;
            Username = username;
            Password = password;
        }
    }

    public class RegisterResponse : ICommandResponse
    {
        public uint Id { get; private set; }

        public RegisterResponse(uint id)
        {
            Id = id;
        }
    }
}

