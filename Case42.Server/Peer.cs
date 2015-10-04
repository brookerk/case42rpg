﻿//using System;
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

namespace Case42.Server
{
    public class Case42Peer : PeerBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Case42Peer));
        private readonly Application _application;
        private readonly JsonSerializer _jsonSerializer;

        public Case42Peer(Application application, InitRequest initRequest)
            : base(initRequest.Protocol, initRequest.PhotonPeer)
        {
            _application = application;
            _jsonSerializer = new JsonSerializer();
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

                        ICommand command;
                        using (var ms = new MemoryStream(commandBytes))
                        {
                            command = (ICommand)_jsonSerializer.Deserialize(new BsonReader(ms), Type.GetType(commandType));
                        }

                        var loginCommand = command as LoginCommand;
                        var registerCommand = command as RegisterCommand;

                        if (loginCommand != null)
                        {
                            (new LoginHandler(session)).Handle(commandContext, loginCommand);
                        }
                        else if (registerCommand != null)
                        {
                            (new RegisterHandler(session)).Handle(commandContext, registerCommand);
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
