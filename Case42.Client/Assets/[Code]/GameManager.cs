using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using Case42.Base;
using Case42.Base.Commands;
using System;
using Case42.Base.Abstract;
using Case42.Base.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    enum GameManagerState
    {
        Form,
        Sending,
        Error,
        LoggedIn
    }

    private string _message;
    private string _displayUsername;
    private string _registerEmail;
    private string _registerPassword;
    private string _loginEmail;
    private string _loginPassword;
    private string _username;
    private string _error;
    private GameManagerState _state;

    private List<string> _messages;
    public void Start()
    {
        _message = "";   
        _registerEmail = "";
        _registerPassword = "";
        _username = "";
        _loginEmail = "";
        _loginPassword = "";
        _state = GameManagerState.Form;
        _messages = new List<string>();

        if (Instance != null)
            throw new InvalidOperationException("Cannot create more than one game manager");

        Instance = this;
    }

    public void Update()
    {
        
    }

    public void Publish(IEvent @event)
    {

        //can reimplement below into class as similar to how commands are handle
        var joinLobbyEvent = @event as JoinLobbyEvent;
        var lobbyJoinedEvent = @event as SessionJoinedLobbyEvent;
        var lobbyLeftEvent = @event as SessionLeftLobbyEvent;
        var messageSendEvent = @event as LobbyMessageSendEvent;

        if (joinLobbyEvent != null)
        {
            foreach (var session in joinLobbyEvent.Sessions)
                _messages.Add(string.Format("{0} - {1} is in lobby", session.Username, session.Id));
        }
        else if (lobbyJoinedEvent != null)
        {
            _messages.Add(string.Format("{0} - {1} entered the lobby", lobbyJoinedEvent.Session.Username, lobbyJoinedEvent.Session.Id));
        }
        else if (lobbyLeftEvent != null)
        {
            _messages.Add(string.Format("{0} left the lobby", lobbyLeftEvent.UserId));
        }
        else if (messageSendEvent != null)
        {
            _messages.Add(string.Format("{0} said {1}", messageSendEvent.UserId, messageSendEvent.Message));
        }
    }

    //public void OnApplicationQuit()
    //{
    //    _photonPeer.Disconnect();
    //}

    public void OnGUI()
    {
        GUILayout.BeginVertical(GUILayout.Width(800), GUILayout.Height(600));

        if (_state == GameManagerState.Form || _state == GameManagerState.Error)
        {
            GUILayout.Label("Case42 Registration");

            if (_state == GameManagerState.Error)
                GUILayout.Label(string.Format("error: {0}", _error));

            GUILayout.Label("Username");
            _username = GUILayout.TextField(_username);

            GUILayout.Label("Email");
            _registerEmail = GUILayout.TextField(_registerEmail);

            GUILayout.Label("Password");
            _registerPassword = GUILayout.TextField(_registerPassword);

            if (GUILayout.Button("Register"))
            {
                Register(_username, _registerPassword, _registerEmail);
            }

            GUILayout.Label("Case42 Login");

            GUILayout.Label("Email");
            _loginEmail = GUILayout.TextField(_loginEmail);

            GUILayout.Label("Password");
            _loginPassword = GUILayout.TextField(_loginPassword);

            if (GUILayout.Button("Login"))
                Login(_loginEmail, _loginPassword);
        }
        else if (_state == GameManagerState.Sending)
        {
            GUILayout.Label("Sending ...");
        }
        else if (_state == GameManagerState.LoggedIn)
        {
            //GUILayout.Label("Success: " + _displayUsername) ;
            GUILayout.BeginVertical();
            _message = GUILayout.TextField(_message);
            if (GUILayout.Button("Send"))
                SendLobbyMessage(_message);
            GUILayout.EndVertical();

            foreach (var message in _messages)
                GUILayout.Label(message);
        }

        GUILayout.EndHorizontal();

        //_message = GUI.TextField(new Rect(0, 0, 200, 40), _message);
        //if (GUI.Button(new Rect(0, 45, 100, 40), "Send Message"))
        //{
        //    SendServer(_message);
        //    _message = "";
        //}

        //GUI.Label(new Rect(0, 90, 300, 500), string.Join("\n", _messages.ToArray()));
    }

    private void SendLobbyMessage(string message)
    {
        NetworkManager.Instance.Dispatch(new SendLobbyMessageCommand(message),
            response => { }
            );
    }

    private void Login(string _email, string _password)
    {
        _state = GameManagerState.Sending;
        NetworkManager.Instance.Dispatch(new LoginCommand(_email, _password), response =>
            {
                if (response.IsValid)
                {
                    _state = GameManagerState.LoggedIn;
                    _displayUsername = response.Response.Username;
                }
                else
                {
                    _state = GameManagerState.Error;
                    _error = response.ToErrorString();
                }
            }
        );

        //_photonPeer.OpCustom(
        //    (byte)Case42OpCode.Login,
        //    new Dictionary<byte, object>
        //        {
        //            { (byte)Case42OpCodeParameter.Email,_email} ,
        //            { (byte)Case42OpCodeParameter.Password,_password}

        //        },
        //        true
        //        );
    }

    private void Register(string _username, string _password, string _email)
    {
        //_state = GameManagerState.Sending;
        //_photonPeer.OpCustom(
        //    (byte)Case42OpCode.Register,
        //    new Dictionary<byte, object>
        //        {
        //            { (byte)Case42OpCodeParameter.Username,_username} ,
        //            { (byte)Case42OpCodeParameter.Email,_email} ,
        //            { (byte)Case42OpCodeParameter.Password,_password}

        //        },
        //        true
        //        );

        _state = GameManagerState.Sending;
        NetworkManager.Instance.Dispatch(new RegisterCommand(_email, _username, _password), response =>
            {
                if (response.IsValid)
                {
                    _state = GameManagerState.LoggedIn;
                    _displayUsername = _username;
                }
                else
                {
                    _state = GameManagerState.Error;
                    _error = response.ToErrorString();
                }
            });
    }

    //private void SendServer(string message)
    //{
    //    //opcustom will cause photon to send message to server
    //    _photonPeer.OpCustom(
    //        0,
    //        new Dictionary<byte, object>
    //        {
    //            {0,message}
    //        },
    //        true);

    //}

    

    
}

