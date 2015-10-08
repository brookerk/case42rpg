//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System;
using Case42.Base;
using NHibernate.Linq;
using Case42.Server.Entities;
using Case42.Server.ValueObjects;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Bson;
using Case42.Base.Abstract;
using Case42.Server.CommandHandlers;
using Case42.Base.Commands;
using Case42.Server.Abstract;

namespace Case42.Server
{
    public class Case42Peer : PeerBase, INetworkedSession
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Case42Peer));
        private readonly Application _application;
        private readonly JsonSerializer _jsonSerializer;

        public Registry Registry { get; private set; }

        public Case42Peer(Application application, InitRequest initRequest)
            : base(initRequest.Protocol, initRequest.PhotonPeer)
        {
            _application = application;
            _jsonSerializer = new JsonSerializer();
            Registry = new Registry();

            log.InfoFormat("Peer created at {0}:{1}", initRequest.RemoteIP, initRequest.RemotePort);

            //SendEvent(new EventData(
            //    0, 
            //    new Dictionary<byte, object> 
            //    {
            //        {0,"Peer created"} 
            //    }), 
            //    new SendParameters 
            //    { 
            //        Unreliable = false 
            //    });
        }

        public void Publish(IEvent @event)
        {
            SendEvent(
                new EventData(
                    (byte)Case42EventCode.SendEvent,
                    new Dictionary<byte, object>
                    {
                        {(byte) Case42EventCodeParameter.EventType, @event.GetType().AssemblyQualifiedName},
                        {(byte) Case42EventCodeParameter.EventBytes,SerializeBSON(@event)}
                    })
                    , new SendParameters { Unreliable = false });
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //invoke when client tell server to do something
            //if (operationRequest.OperationCode != 0)
            //{
            //    log.WarnFormat("Peer sent unknown opcode: {0}", operationRequest.OperationCode);
            //    return;
            //}
            //var message = (string)operationRequest.Parameters[0];
            //log.DebugFormat("Get message from client: {0}", message);

            //var eventData = new EventData(0, new Dictionary<byte, object> { { 0, message } });
            //var parameters = new SendParameters { Unreliable = false };
            //foreach (var peer in _application.Peers.Where(t => t != this))
            //{
            //    peer.SendEvent(eventData,parameters);
            //}


            //user registration
            // user authentication
            // sending a message
            if (operationRequest.OperationCode != (byte)Case42OpCode.DispatchCommand)
            {
                SendOperationResponse(new OperationResponse((byte)Case42OpCodeResponse.Invalid), sendParameters);
                log.WarnFormat("Peer sent unknown operation code: {0}", operationRequest.OperationCode);
                return;
            }

            using (var session = _application.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        //implementing command factory design
                        var commandContext = new CommandContext();

                        var commandType = (string)operationRequest.Parameters[(byte)Case42OpCodeParameter.CommandType];
                        var commandBytes = (byte[])operationRequest.Parameters[(byte)Case42OpCodeParameter.CommandBytes];
                        var commandId = operationRequest.Parameters[(byte)Case42OpCodeParameter.CommandId];

                        ICommand command;
                        using (var ms = new MemoryStream(commandBytes))
                        {
                            command = (ICommand)_jsonSerializer.Deserialize(new BsonReader(ms), Type.GetType(commandType));
                        }

                        var loginCommand = command as LoginCommand;
                        var registerCommand = command as RegisterCommand;
                        var sendlobbyMessageCommand = command as SendLobbyMessageCommand;
                        var challengeCommand = command as ChallengePlayerCommand;
                        var respondToChallengeCommand = command as RespondToChallengeCommand;

                        if (loginCommand != null)
                        {
                            (new LoginHandler(session, _application)).Handle(this, commandContext, loginCommand);
                        }
                        else if (registerCommand != null)
                        {
                            (new RegisterHandler(session, _application)).Handle(this, commandContext, registerCommand);
                        }
                        else if (sendlobbyMessageCommand != null)
                        {
                            (new SendLobbyMessageHandler(_application)).Handle(this, commandContext, sendlobbyMessageCommand);
                        }
                        else if (challengeCommand != null)
                        {
                            (new ChallengePlayerHandler(session, _application)).Handle(this, commandContext, challengeCommand);
                        }
                        else if (respondToChallengeCommand != null)
                        {
                            (new RespondToChallengeHandler(session, _application)).Handle(this, commandContext, respondToChallengeCommand);
                        }
                        else
                        {
                            SendOperationResponse(new OperationResponse((byte)Case42OpCodeResponse.Invalid), sendParameters);
                            log.WarnFormat("Peer sent unknown command: {0}", commandType);
                            trans.Rollback();
                            return;
                        }

                        var parameters = new Dictionary<byte, object>();
                        if (commandContext.Response != null)
                        {
                            parameters[(byte)Case42OpCodeResponseParameter.CommandResponse] = SerializeBSON(commandContext.Response);
                        }

                        parameters[(byte)Case42OpCodeResponseParameter.OperationErrors] = SerializeBSON(commandContext.OperationErrors);
                        parameters[(byte)Case42OpCodeResponseParameter.PropertyErrors] = SerializeBSON(commandContext.PropertyErrors);
                        parameters[(byte)Case42OpCodeResponseParameter.CommandId] = commandId;

                        SendOperationResponse(new OperationResponse((byte)Case42OpCodeResponse.CommandDispatched, parameters), sendParameters);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        SendOperationResponse(new OperationResponse((byte)Case42OpCodeResponse.FatalError), sendParameters);
                        trans.Rollback();
                        log.ErrorFormat("Error processing operation {0} : {1}", operationRequest.OperationCode, ex);
                    }
                }
            }



        }

        private byte[] SerializeBSON(object obj)
        {
            using (var ms = new MemoryStream())
            {

                _jsonSerializer.Serialize(new BsonWriter(ms), obj);
                return ms.ToArray();
            }
        }
        private void SendMessage(NHibernate.ISession session, string message)
        {

        }


        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            //photon telling client has disconnected
            log.InfoFormat("Peer disconnected {0}:{1}", reasonCode, reasonDetail);
            _application.DestroyPeer(this);
        }

        //private void SendSuccess()
        //{
        //    SendOperationResponse(new OperationResponse((byte)Case42OpCodeResponse.Success), new SendParameters { Unreliable = false });
        //}

        //private void SendError(string message)
        //{
        //    SendOperationResponse(
        //        new OperationResponse(
        //            (byte)Case42OpCodeResponse.Error, 
        //            new Dictionary<byte,object>
        //            {
        //                {(byte)Case42OpCodeResponseParameter.ErrorMessage, message}
        //            }
        //            )
        //       , new SendParameters
        //        {
        //            Unreliable = false
        //        });
        //}

    }
}
