using System.IO;
//using ExitGames.Logging.Log4Net;
using Photon.SocketServer;

using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using Case42.Server.Entities;

using log4net;
using log4net.Config;
using System;
using Case42.Server.Abstract;
using Case42.Server.Components;
using Case42.Server.ValueObjects;
using Log4NetLoggerFactory = ExitGames.Logging.Log4Net.Log4NetLoggerFactory;

namespace Case42.Server
{
    public class Application : ApplicationBase, IApplication
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(Application));
        private readonly List<Case42Peer> _peers;
        private ISessionFactory _sessionFactory;


        public Registry Registry { get; private set; }
        public IEnumerable<INetworkedSession> Sessions { get { return _peers; } }

        public Application()
        {
            _peers = new List<Case42Peer>();
            Registry = new Registry();

            Registry.Set(new LobbyComponent());
        }

        public ISession OpenSession()
        {
            return _sessionFactory.OpenSession();
        }

        public void DestroyPeer(Case42Peer peer)
        {
            //remove it from lobby
            Registry.Get<LobbyComponent>(lobby =>
            {
                if (lobby.Contains(peer))
                    lobby.Leave(peer);
            });

            //remove it from the network session but not lobby
            _peers.Remove(peer);


        }

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            //log.InfoFormat("Peer created at {0}:{1}", initRequest.RemoteIP, initRequest.RemotePort);
            //return new Case42Peer(initRequest);

            var peer = new Case42Peer(this, initRequest);
            _peers.Add(peer);
            return peer;
        }

        protected override void Setup()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(BinaryPath, "log4net.config")));
            ExitGames.Logging.LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);

            SetupHibernate();

            ////test code to test db transaction
            //using (var session = _sessionFactory.OpenSession())
            //{

            //    log.InfoFormat("Session created at {0}:{1}", session.Connection.State.ToString(),session.IsConnected);
            //    using (var trans = session.BeginTransaction())
            //    {
            //        var user = new User
            //        {
            //            Email = "test123@yahoo.com.sg",
            //            Username = "nelson",
            //            CreatedAt = DateTime.UtcNow,
            //            Password = new HashedPassword("hash1","salt1")
            //        };

            //        session.Save(user);
            //        trans.Commit();
            //    }
            //}
            //
            log.Info("------ Application started for RuneSlinger ------");
        }

        private void SetupHibernate()
        {

            //tell nhibernate where to get database configuration from
            var config = new Configuration();
            config.Configure();

            //tell nhibernate where to get class for mapping
            var mapper = new ModelMapper();
            mapper.AddMapping<Entities.Usermap>();

            config.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());

            //create db transaction session
            _sessionFactory = config.BuildSessionFactory();
        }

        protected override void TearDown()
        {
            log.Info("------- Application end ------");
        }


    }
}
