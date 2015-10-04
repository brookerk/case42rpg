using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Case42.Base.ValueObjects
{
    public class LobbySession
    {
        public uint Id { get; private set; }
        public string Username { get; private set; }

        public LobbySession(uint id, string username)
        {
            Id = id;
            Username = username;
        }
    }
}
