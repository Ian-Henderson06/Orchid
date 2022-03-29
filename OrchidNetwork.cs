using Orchid;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Orchid.Logger;

public class OrchidNetwork : MonoBehaviour
{
    [SerializeField] private bool isClient = false;
    [SerializeField] private bool isServer = false;
    
    [SerializeField] private bool allowRiptideLogging = true;
    [SerializeField] private bool allowRiptideTimestamps = false;

    [Header("Server Settings")] 
    [SerializeField] private ushort runningPort;
    [SerializeField] private ushort maxClients;

    private Server riptideServer;
    private Client riptideClient;

    // Singleton pattern
    private static OrchidNetwork _instance;
    public static OrchidNetwork Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null)
                _instance = value;
            else if(_instance != value)
            { 
                //Silently handle this.
            }
        }
    }

    private void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        if(allowRiptideLogging)
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, allowRiptideTimestamps);

        if (isServer)
            SetupServer();
        
        if (isClient)
            SetupClient();
    }

    private void SetupServer()
    {
        riptideServer = new Server();
        riptideServer.Start(runningPort, maxClients);

        riptideServer.ClientConnected += ServerOnClientConnected;
        riptideServer.ClientDisconnected += ServerOnClientDisconnected;
        
        EventSystem.ServerEvents.CallOnServerStarted();
    }

    private void SetupClient()
    {
        riptideClient = new Client();

        riptideClient.Connected += ClientDidConnect;
        riptideClient.ConnectionFailed += ClientFailedToConnect;
        riptideClient.ClientDisconnected += ClientOtherClientDisconnected;
        riptideClient.Disconnected += ClientDidDisconnect;
        
        EventSystem.ClientEvents.CallOnStarted();
    }
    
    /// <summary>
    /// Tick the client and server.
    /// </summary>
    void FixedUpdate()
    {
        if(isServer)
            riptideServer?.Tick();
        
        if(isClient)
            riptideClient?.Tick();
    }

    private void OnApplicationQuit()
    {
        if (isServer)
        {
            EventSystem.ServerEvents.CallOnServerStopped();
            riptideServer.Stop();
        }

        if (isClient)
        {
            EventSystem.ClientEvents.CallOnClientStopped();
            riptideClient.Disconnect();
        }
    }
    
    /// <summary>
    /// Connect to a server.
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void ClientConnect(string ip, ushort port)
    {
        if(isClient)
            riptideClient.Connect($"{ip}:{port}");
        else
            Logger.LogError("This is not a client. Please check OrchidNetwork settings.");
    }
    
    
    #region Events
    private void ClientDidConnect(object sender, EventArgs e)
    {
        EventSystem.ClientEvents.CallOnConnectedToServer();
    }
    
    private void ClientDidDisconnect(object sender, EventArgs e)
    {
        EventSystem.ClientEvents.CallOnDisconnect();
    }
    
    private void ClientOtherClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        EventSystem.ClientEvents.CallOnOtherClientDisconnected(e.Id);
    }
    
    private void ClientFailedToConnect(object sender, EventArgs e)
    {
        EventSystem.ClientEvents.CallOnFailedToConnect();
    }

    private void ServerOnClientConnected(object sender, ServerClientConnectedEventArgs e)
    {
        EventSystem.ServerEvents.CallOnClientConnected(e.Client.Id);
    }
    
    private void ServerOnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        EventSystem.ServerEvents.CallOnClientDisconnected(e.Id);
    }
    #endregion
    #region Getters

    public Server GetRiptideServer => riptideServer;
    public Client GetRiptideClient => riptideClient;
    
    #endregion
}
