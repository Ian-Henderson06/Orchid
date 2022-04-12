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
    [Header("General Settings")]
    [SerializeField] private bool isClient = false;
    [SerializeField] private bool isServer = false;

    [SerializeField] private bool allowRiptideLogging = true;
    [SerializeField] private bool allowRiptideTimestamps = false;
    [Space(10)]
    [SerializeField] private float tickRate = 40;
    [SerializeField] private int tickDivergenceTolerance = 1;
    [SerializeField] private bool useWorldUpdates = false;
    [SerializeField] private uint ticksBetweenWorldUpdates = 2;

    [Header("Server Settings")] [SerializeField]
    private ushort runningPort;

    [SerializeField] private ushort maxClients;

    private Server riptideServer;
    private Client riptideClient;

    private NetworkType? localNetworkType = null;
    private ushort? localClientID = null;

    private uint currentTick; //This could overflow if the game is ran for a crazy amount of time - Will still need address this at some point most likely.
    private uint lastServerTick = 0; //Last tick client received from the server
    
    
    
    // Singleton pattern
    private static OrchidNetwork _instance;

    public static OrchidNetwork Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null)
                _instance = value;
            else if (_instance != value)
            {
                //Silently handle this.
            }
        }
    }

    private void Awake()
    {
        Instance = this;
        Time.fixedDeltaTime = 1/tickRate; //Set time step - This should match on client and server
        DontDestroyOnLoad(this);
        localNetworkType = FindLocalNetworkType();
    }

    async void Start()
    {
        if (allowRiptideLogging)
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
        if (isServer)
        {
            if (riptideServer.IsRunning)
            {
                if (currentTick % (5 * tickRate) == 0) //Every 5 seconds send update tick
                {
                    OrchidSender.ServerSendTickToClients(currentTick);
                }

                if (useWorldUpdates)
                {
                    if (currentTick % ticksBetweenWorldUpdates == 0)
                    {
                        OrchidSender.ServerSendWorldStateToClients();
                    }
                }

                riptideServer?.Tick();
            }
        }

        if (isClient)
            riptideClient?.Tick();

        currentTick++;
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

    private void OnDestroy()
    {
    }

    /// <summary>
    /// Connect to a server.
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void ClientConnect(string ip, ushort port)
    {
        if (isClient)
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

    /// <summary>
    /// Set the last server tick received - This is used for interpolation and reconcillation
    /// Only called on client or clienthost
    /// </summary>
    /// <param name="serverTick"></param>
    public void SetServerLastTick(uint serverTick)
    {
        lastServerTick = serverTick - ticksBetweenWorldUpdates; //Set a late tick - used as interpolation tick
        if (Mathf.Abs(currentTick - serverTick) > tickDivergenceTolerance)
        {
            Logger.LogMessage($"Client tick update: {currentTick} -> {serverTick}");
            currentTick = serverTick; //May cause occasional jittering
        }
           
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
    public NetworkType GetLocalNetworkType()
    {
        if (localNetworkType == null)
            localNetworkType = FindLocalNetworkType();

        return (NetworkType)localNetworkType;
    }

    /// <summary>
    /// Returns the local client ID - This is only valid for client or clienthost.
    /// If server then this is null.
    /// </summary>
    public ushort? GetLocalClientID()
    {
        if (localClientID == null)
            localClientID = FindLocalClientID();

        return localClientID;
    }



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

    private ushort? FindLocalClientID()
    {
        NetworkType local;
        local = GetLocalNetworkType();
        
        if (local == NetworkType.Client)
           return riptideClient.Id;

        if (local == NetworkType.Server)
            return null;

        if (local == NetworkType.ClientHost)
            return riptideClient.Id;

        return null;
    }

    /// <summary>
    /// Get the current server/client tick.
    /// </summary>
    /// <returns></returns>
    public uint GetCurrentTick() => currentTick;

    /// <summary>
    /// Get the last server tick that the server sent over.
    /// </summary>
    /// <returns></returns>
    public uint GetLastServerTick() => lastServerTick;

    #endregion
}
