using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Case42.Server.Components
{
    public class AuthComponent
    {
        public uint Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }

        public AuthComponent(uint id, string username, string email)
        {
            Id = id;
            Username = username;
            Email = email;
        }

    }
}
