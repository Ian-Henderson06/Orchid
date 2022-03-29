using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orchid
{
    /// <summary>
    /// Events system containing events relevant to the client and server.
    /// </summary>
    public class EventSystem
    {
        /// <summary>
        /// All Events relevant to the server. These will not be called on the client.
        /// </summary>
        public static class ServerEvents
        {
            public static event Action OnStarted;
            public static event Action<ushort> OnClientConnected;
            public static event Action<ushort> OnClientDisconnected;
            public static event Action OnStopped;
            
            public static void CallOnServerStarted()
            {
                OnStarted?.Invoke();
            }
            
            public static void CallOnClientConnected(ushort clientId)
            {
                OnClientConnected?.Invoke(clientId);
            }
            
            public static void CallOnClientDisconnected(ushort clientId)
            {
                OnClientDisconnected?.Invoke(clientId);
            }
            
            public static void CallOnServerStopped()
            {
                OnStopped?.Invoke();
            }
        }
        
        /// <summary>
        /// All Events relevant to the client. These will not be called on the server.
        /// </summary>
        public static class ClientEvents
        {
            public static event Action OnStarted;
            public static event Action OnFailedToConnect;
            public static event Action OnConnectedToServer;
            public static event Action OnDisconnect;
            public static event Action OnStopped;
            
 
            public static event Action<ushort> OnOtherClientDisconnected;

            public static void CallOnStarted()
            {
                OnStarted?.Invoke();
            }

            public static void CallOnFailedToConnect()
            {
                OnFailedToConnect?.Invoke();
            }
            
            public static void CallOnConnectedToServer()
            {
                OnConnectedToServer?.Invoke();
            }
            
            public static void CallOnDisconnect()
            {
                OnDisconnect?.Invoke();
            }
            
            public static void CallOnOtherClientDisconnected(ushort clientId)
            {
                OnOtherClientDisconnected?.Invoke(clientId);
            }
            
            public static void CallOnClientStopped()
            {
                OnStopped?.Invoke();
            }
        }
        
        //Common events
    }
}
