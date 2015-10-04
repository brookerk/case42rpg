using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Case42.Base.Abstract;

namespace Case42.Server.Abstract
{
    public interface INetworkedSession
    {
        Registry Registry { get; }

        void Publish(IEvent @event);
    }
}
