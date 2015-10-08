using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Case42.Server.Abstract;

namespace Case42.Server.Components
{
    public class Case42Game
    {
        private readonly IEnumerable<INetworkedSession> _sessions;

        public Case42Game(IEnumerable<INetworkedSession> sessions)
        {
            _sessions = sessions;
        }


    }
}
