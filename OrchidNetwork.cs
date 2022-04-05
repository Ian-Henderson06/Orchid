using Orchid;
using Orchid.Events;
using Orchid.Util;

using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Orchid.Util.Logger;


/// <summary>
/// Main Unity class for Orchid which handles connection and setup.
/// </summary>
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

    private NetworkType localNetworkType;

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

        localNetworkType = FindLocalNetworkType();
    }
    
     async void Start()
    {
        if(allowRiptideLogging)
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, allowRiptideTimestamps);
        
        await OrchidReflector.GetAllRPCMethods();

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
        
        OrchidEvents.ServerEvents.CallOnServerStarted();
        
    }

    private void SetupClient()
    {
       // //No point in running the reflector twice if a client server
       riptideClient = new Client();
        
        Debug.Log("Setting up client");

        riptideClient.Connected += ClientDidConnect;
        riptideClient.ConnectionFailed += ClientFailedToConnect;
        riptideClient.ClientDisconnected += ClientOtherClientDisconnected;
        riptideClient.Disconnected += ClientDidDisconnect;
        
        OrchidEvents.ClientEvents.CallOnStarted();
    }
    
    /// <summary>
    /// Tick the client and server.
    /// </summary>
    void FixedUpdate()
    {
        if(isServer)
            if(riptideServer.IsRunning)
                riptideServer?.Tick();
        
        if(isClient)
            riptideClient?.Tick();
    }

    private void OnApplicationQuit()
    {
        if (isServer)
        {
            OrchidEvents.ServerEvents.CallOnServerStopped();
            riptideServer.Stop();
        }

        if (isClient)
        {
            OrchidEvents.ClientEvents.CallOnClientStopped();
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

    /// <summary>
    /// If client - send from client to server.
    /// If server - send from server to all.
    /// </summary>
    /// <param name="message"></param>
    public void SendMessageDefaultMethod(ref Message message)
    {
        switch (localNetworkType)
        {
            case NetworkType.ClientHost:
                riptideServer.SendToAll(message);
                break;
            case NetworkType.Server:
                riptideServer.SendToAll(message);
                break;
            case NetworkType.Client:
                riptideClient.Send(message);
                break;
            
        }
    }

    /// <summary>
    /// Send message to all as server.
    /// </summary>
    /// <param name="message"></param>
    public void ServerSendMessageToAll(ref Message message)
    {
        if (!isServer)
        {
            Logger.LogError("You are a client. Cannot send server message from a client.");
            return;
        }
        
        riptideServer.SendToAll(message);
    }
    
    /// <summary>
    /// Send message to a specific client.
    /// </summary>
    /// <param name="message"></param>
    public void ServerSendMessageToSpecific(ushort clientId, ref Message message)
    {
        if (!isServer)
        {
            Logger.LogError("You are a client. Cannot send server message from a client.");
            return;
        }
        
        riptideServer.Send(message, clientId);
    }
    
    /// <summary>
    /// Send message to all clients except specified.
    /// </summary>
    /// <param name="message"></param>
    public void ServerSendMessageExcluding(ushort clientId, ref Message message)
    {
        if (!isServer)
        {
            Logger.LogError("You are a client. Cannot send server message from a client.");
            return;
            
        }
        riptideServer.SendToAll(message, clientId);
    }

    /// <summary>
    /// Send a message to the server.
    /// </summary>
    /// <param name="message"></param>
    public void ClientSendMessage(ref Message message)
    {
        if (!isClient)
        {
            Logger.LogError("You are a server. Cannot send client message from a server.");
            return;
        }
        
        riptideClient.Send(message);
    }

    
    
    #region Events
    private void ClientDidConnect(object sender, EventArgs e)
    {
        OrchidEvents.ClientEvents.CallOnConnectedToServer();
    }
    
    private void ClientDidDisconnect(object sender, EventArgs e)
    {
        OrchidEvents.ClientEvents.CallOnDisconnect();
    }
    
    private void ClientOtherClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        OrchidEvents.ClientEvents.CallOnOtherClientDisconnected(e.Id);
    }
    
    private void ClientFailedToConnect(object sender, EventArgs e)
    {
        OrchidEvents.ClientEvents.CallOnFailedToConnect();
    }

    private void ServerOnClientConnected(object sender, ServerClientConnectedEventArgs e)
    {
        OrchidEvents.ServerEvents.CallOnClientConnected(e.Client.Id);
    }
    
    private void ServerOnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        OrchidEvents.ServerEvents.CallOnClientDisconnected(e.Id);
    }
    #endregion
    #region Getters

  //  public Server GetRiptideServer => riptideServer;
   // public Client GetRiptideClient => riptideClient;
   
   /// <summary>
   /// Get the local network type.
   /// </summary>
    public NetworkType GetLocalNetworkType => localNetworkType;

    /// <summary>
    /// Get the local type of network.
    /// </summary>
    /// <returns></returns>
    private NetworkType FindLocalNetworkType()
    {
        NetworkType type;
        if (isServer && isClient)
        {
            type = NetworkType.ClientHost;
            return type;
        }

        if (isClient)
        {
            type = NetworkType.Client;
            return type;
        }

        if (isServer)
        {
            type = NetworkType.Server;
            return type;
        }

        return 0;
    }
    
    #endregion
}
