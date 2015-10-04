using UnityEngine;
using ExitGames.Client.Photon;
using Case42.Base.Abstract;
using Assets.Code;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Case42.Base;
using Newtonsoft.Json.Bson;

public class NetworkManager : MonoBehaviour, IPhotonPeerListener
{
    //singleton 
    public static NetworkManager Instance { get; private set; }
    private PhotonPeer _photonPeer;
    private JsonSerializer _jsonSerializer;
    private Action<Dictionary<byte,object>> _commandCallback;

    // Use this for initialization
    public void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("You cannot create more than one!");
            return;
        }
        Instance = this;
        _photonPeer = new PhotonPeer(this, ConnectionProtocol.Udp);
        if (!_photonPeer.Connect("127.0.0.1:5055", "RuneSlinger"))
            Debug.LogError("Could not connect to photon!!");

        _jsonSerializer = new JsonSerializer();
    }

    // Update is called once per frame
    public void Update()
    {
        _photonPeer.Service();
    }

    public void OnApplicationQuit()
    {
        _photonPeer.Disconnect();
    }

    public void Dispatch<TCommand>(TCommand command,Action<CommandContext> action)
    {
        _commandCallback = parameters =>
            {
                action(new CommandContext(
                                    DeserializeBSON<IDictionary<string, IEnumerable<string>>>((byte[])parameters[(byte)Case42OpCodeResponseParameter.PropertyErrors]),
                                    DeserializeBSON<IEnumerable<string>>((byte[])parameters[(byte)Case42OpCodeResponseParameter.OperationErrors],true)
                                    )
                    );
            };
        DispatchInternal(command);
    }
        
    public void Dispatch<TResponse>(ICommand<TResponse> command, Action<CommandContext<TResponse>> action) where TResponse : ICommandResponse
    {
        _commandCallback = parameters =>
        {
            var response = default(TResponse);
            if (parameters.ContainsKey((byte)Case42OpCodeResponseParameter.CommandResponse))
                response = DeserializeBSON<TResponse>((byte[])parameters[(byte)Case42OpCodeResponseParameter.CommandResponse]);

            action(new CommandContext<TResponse>(
                                response,
                                DeserializeBSON<IDictionary<string, IEnumerable<string>>>((byte[])parameters[(byte)Case42OpCodeResponseParameter.PropertyErrors]),
                                DeserializeBSON<IEnumerable<string>>((byte[])parameters[(byte)Case42OpCodeResponseParameter.OperationErrors],true)
                                )
                );
        };
        DispatchInternal(command);
    }

    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        var responseCode = (Case42OpCodeResponse)operationResponse.OperationCode;
        if (responseCode == Case42OpCodeResponse.FatalError)
            Debug.LogError("You broke the serve");
        else if (responseCode == Case42OpCodeResponse.Invalid)
            Debug.LogError("Invalid command!");
        else if (responseCode == Case42OpCodeResponse.CommandDispatched)
        {
            _commandCallback(operationResponse.Parameters);

        }
    }
    
    public void OnEvent(EventData eventData)
    {
        if (eventData.Code != (byte)Case42EventCode.SendEvent)
            throw new InvalidOperationException("Unknown event received from server");

        var type = (string)eventData.Parameters[(byte)Case42EventCodeParameter.EventType];
        var bytes = (byte[])eventData.Parameters[(byte)Case42EventCodeParameter.EventBytes];

        var @event = (IEvent)DeserializeBSON(bytes, Type.GetType(type));

        GameManager.Instance.Publish(@event);

    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        
    }

    private void DispatchInternal<TCommand>(TCommand command)
    {
        var parameters = new Dictionary<byte, object>();
        parameters[(byte)Case42OpCodeParameter.CommandType] = command.GetType().AssemblyQualifiedName;
        parameters[(byte)Case42OpCodeParameter.CommandBytes] = SerializeBSON(command);

        _photonPeer.OpCustom((byte)Case42OpCode.DispatchCommand, parameters, true);
    }

    private object DeserializeBSON(byte[] bytes,Type type, bool isArray = false)
    {
        using (var ms = new MemoryStream(bytes))
        {
            return _jsonSerializer.Deserialize(new BsonReader(ms,isArray,DateTimeKind.Local), type);
        }
    }

    //generic
    private TObject DeserializeBSON<TObject>(byte[] bytes, bool isArray = false)
    {
        using (var ms = new MemoryStream(bytes))
        {
            return _jsonSerializer.Deserialize<TObject>(new BsonReader(ms,isArray,DateTimeKind.Local));
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

}

