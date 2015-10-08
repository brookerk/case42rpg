using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Case42.Base.Events;
using Case42.Base.ValueObjects;
using Case42.Server.Abstract;
using Case42.Server.ValueObjects;

namespace Case42.Server.Components
{
    public class ChallengeComponent
    {
        private readonly IApplication _application;

        public INetworkedSession CurSession { get; private set; }
        public INetworkedSession OtherSession { get; private set; }
        public ChallengeDirection Direction { get; private set; }

        public ChallengeComponent(IApplication application, INetworkedSession curSession, INetworkedSession otherSession, ChallengeDirection direction)
        {
            _application = application;
            CurSession = curSession;
            OtherSession = otherSession;
            Direction = direction;
        }

        public void Respond(ChallengeResponse response)
        {
            if (Direction == ChallengeDirection.Challenged)
                throw new InvalidOperationException("You cannot respond to a challenge that you created");

            OtherSession.Publish(new ChallengeRespondedToEvent(response));

            if (response == ChallengeResponse.Rejected)
            {
                Destroy();
            }
            else if (response == ChallengeResponse.Accepted)
            {
                _application.Registry.Get<LobbyComponent>(lobby =>
                {
                    lobby.AcceptChallenge(this);
                });
            }

        }

        public void Abort()
        {
            Destroy();

            OtherSession.Publish(new ChallengeRespondedToEvent(ChallengeResponse.Aborted));
        }

        public void Destroy()
        {
            CurSession.Registry.Remove<ChallengeComponent>();
            OtherSession.Registry.Remove<ChallengeComponent>();
        }
    }
}