using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using Case42.Base;

public class GameManager : MonoBehaviour , IPhotonPeerListener
{

    private PhotonPeer _photonPeer;

    private string _registeredemail;
    private string _registeredpassword;
    private string _loginemail;
    private string _loginpassword;
    private string _username;
    private string _error;

    enum GameManagerState
    {
        Form,
        Sending,
        Error,
        Success
    }
    private GameManagerState _state;

    //private string _message;
    //private List<string> _messages;

	// Use this for initialization
	public void Start () 
    {

        _photonPeer = new PhotonPeer(this, ConnectionProtocol.Udp);
        if (!_photonPeer.Connect("127.0.0.1:5055", "Case42"))
            Debug.LogError("Could not connect to photon!!");

        //_message = "";
        //_messages = new List<string>();

        _registeredemail = "";
        _registeredpassword = "";
        _loginemail = "";
        _loginpassword = "";
        _username = "";
        _error = "";
	}
	
	// Update is called once per frame
	public void Update () 
    {

        _photonPeer.Service();
	}

    public void OnGUI()
    {
        GUILayout.BeginVertical(GUILayout.Width(800), GUILayout.Height(600));

        if (_state == GameManagerState.Form || _state == GameManagerState.Error)
        {
            GUILayout.Label("Case42 RPG Registration");

            if (_state == GameManagerState.Error)
                GUILayout.Label(string.Format("error: {0}", _error));

            GUILayout.Label("Username");
            _username = GUILayout.TextField(_username);

            GUILayout.Label("Email");
            _registeredemail = GUILayout.TextField(_registeredemail);

            GUILayout.Label("Password");
            _registeredpassword = GUILayout.TextField(_registeredpassword);

            if (GUILayout.Button("Register"))
            {
                Register(_username, _registeredpassword, _registeredemail);
            }

            GUILayout.Label("Case42 RPG Login");

            GUILayout.Label("Email");
            _loginemail = GUILayout.TextField(_loginemail);

            GUILayout.Label("Password");
            _loginpassword = GUILayout.TextField(_loginpassword);

            if (GUILayout.Button("Login"))
                Login(_loginemail, _loginpassword);
        }
        else if (_state == GameManagerState.Sending)
        {
            GUILayout.Label("Sending ...");
        }
        else if (_state == GameManagerState.Success)
        {
            GUILayout.Label("Success");
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

    private void Login(string _email, string _password)
    {
        _state = GameManagerState.Sending;
        _photonPeer.OpCustom(
            (byte)Case42OpCode.Login,
            new Dictionary<byte, object>
                {
                    { (byte)Case42OpCodeParameter.Email,_email} ,
                    { (byte)Case42OpCodeParameter.Password,_password}

                },
                true
                );
    }

    private void Register(string _username, string _password, string _email)
    {
        _state = GameManagerState.Sending;
        _photonPeer.OpCustom(
            (byte)Case42OpCode.Register,
            new Dictionary<byte, object>
                {
                    { (byte)Case42OpCodeParameter.Username,_username} ,
                    { (byte)Case42OpCodeParameter.Email,_email} ,
                    { (byte)Case42OpCodeParameter.Password,_password}

                },
                true
                );
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

    public void OnApplicationQuit()
    {
        _photonPeer.Disconnect();
    }

    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        var response = (Case42OpCodeResponse)operationResponse.OperationCode;
        if (response == Case42OpCodeResponse.Error)
        {
            _state = GameManagerState.Error;
            _error = (string)operationResponse.Parameters[(byte)Case42OpCodeResponseParameter.ErrorMessage];
        }
        else if (response == Case42OpCodeResponse.FatalError || response == Case42OpCodeResponse.Invalid)
        {
            _state = GameManagerState.Error;
            _error = "You broke the server";
        }
        else if (response == Case42OpCodeResponse.Success)
        {
            _state = GameManagerState.Success;
        }
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
    }

    public void OnEvent(EventData eventData)
    {
        ////Debug.Log("Event: " + eventData.Parameters[0].ToString());
        //_messages.Add(eventData.Parameters[0].ToString());
    }
}

