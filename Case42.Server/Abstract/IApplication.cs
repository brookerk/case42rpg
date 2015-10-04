using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Case42.Server.Abstract
{
    public interface IApplication
    {
        Registry Registry { get; }
        IEnumerable<INetworkedSession> Sessions { get; }
    }
}
