using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Case42.Base.Abstract;

namespace Case42.Base.Commands
{
    public class LoginCommand : ICommand<LoginResponse>
    {
        public string Email { get; private set; }
        public string Password { get; private set; }

        //the parameter name should be the same as the above parameter if using Json
        //if parameter below email is renamed as email123, it will not work for serialization
        public LoginCommand(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }

    public class LoginResponse : ICommandResponse
    {
        public uint Id { get; private set; }
        public string Email { get; private set; }
        public string Username { get; private set; }

        //the parameter name should be the same as the above parameter if using Json
        public LoginResponse(uint id, string email, string username)
        {
            Id = id;
            Email = email;
            Username = username;
        }
    }

}
