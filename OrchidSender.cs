using RiptideNetworking;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Logger = Orchid.Util.Logger;

namespace Orchid
{
    /// <summary>
    /// Handles all riptide sending calls for client and server.
    /// </summary>
    internal class OrchidSender
    {
        
        #region Server Sending Methods

            /// <summary>
            /// Sends the current server tick to all clients.
            /// </summary>
            /// <param name="currentTick"></param>
            public static void ServerSendTickToClients(uint currentTick)
            {
                Message message = Message.Create(MessageSendMode.unreliable, (ushort)MessageTypes.Sync);
                message.AddUInt(currentTick);
                OrchidNetwork.Instance.ServerSendMessageToAll(ref message);
            }
        
            /// <summary>
            /// Send an RPC call to all clients.
            /// </summary>
            /// <param name="methodID"></param>
            /// <param name="parameters"></param>
            public static void ServerSendRPCToClients(MessageSendMode sendMode, string rpcName, params object[] parameters)
            {
                Message message = Message.Create(sendMode, (ushort)MessageTypes.RPC);
                SerializeRPC(ref message, rpcName, parameters);
                OrchidNetwork.Instance.ServerSendMessageToAll(ref message);
            }
            
            /// <summary>
            /// Sends the current world state to all clients.
            /// </summary>
            /// <param name="currentTick"></param>
            public static void ServerSendWorldStateToClients()
            {
                foreach (KeyValuePair<long, GameObject> kv in
                    OrchidPrefabManager.Instance.GetAliveNetworkedGameObjects())
                {
                    Message message = Message.Create(MessageSendMode.unreliable, (ushort)MessageTypes.WorldState);
                    message.Add(kv.Key);
                    message.AddInt(OrchidPrefabManager.Instance.GetPrefabID(OrchidPrefabManager.Instance.GetPrefabID(kv.Key)));
                    message.AddVector3(kv.Value.transform.position);
                    
                    //Exclude local client if clienthost
                    if(OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.ClientHost)
                        OrchidNetwork.Instance.ServerSendMessageExcluding((ushort)OrchidNetwork.Instance.GetLocalClientID(), ref message);
                    else if(OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Server)
                        OrchidNetwork.Instance.ServerSendMessageToAll(ref message);
                    
                }
               
            }
            
            /// <summary>
            /// Send an RPC call to a specific client.
            /// </summary>
            /// <param name="methodID"></param>
            /// <param name="parameters"></param>
            public static void ServerSendRPCToClient(MessageSendMode sendMode, ushort clientID, string rpcName, params object[] parameters)
            {
                Message message = Message.Create(sendMode, (ushort)MessageTypes.RPC);
                SerializeRPC(ref message, rpcName, parameters);
                OrchidNetwork.Instance.ServerSendMessageToSpecific(clientID, ref message);
            }

            /// <summary>
            /// Send object spawn message to all clients.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectSpawnToClients(long networkID, int prefabID, Vector3 position,
                Quaternion rotation)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectSpawn);
                message.AddLong(networkID);
                message.AddInt(prefabID);
                message.AddVector3(position);
                message.AddQuaternion(rotation);
                OrchidNetwork.Instance.ServerSendMessageToAll(ref message);
            }
            
            /// <summary>
            /// Send object spawn message to a specific client.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectSpawnToClient(ushort clientID, long networkID, int prefabID, Vector3 position,
                Quaternion rotation)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectSpawn);
                message.AddLong(networkID);
                message.AddInt(prefabID);
                message.AddVector3(position);
                message.AddQuaternion(rotation);
                OrchidNetwork.Instance.ServerSendMessageToSpecific(clientID, ref message);
            }
            
            /// <summary>
            /// Send object spawn message to all excluding a specific client.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectSpawnExcludingClient(ushort clientID, long networkID, int prefabID, Vector3 position,
                Quaternion rotation)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectSpawn);
                message.AddLong(networkID);
                message.AddInt(prefabID);
                message.AddVector3(position);
                message.AddQuaternion(rotation);
                OrchidNetwork.Instance.ServerSendMessageExcluding(clientID, ref message);
            }
            
            /// <summary>
            /// Send object destroy message to all clients.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectDestroyToClients(long networkID)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectDestroy);
                message.AddLong(networkID);
                OrchidNetwork.Instance.ServerSendMessageToAll(ref message);
            }
            
            /// <summary>
            /// Send object destroy to a specific client.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectDestroyToClient(ushort clientID, long networkID)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectDestroy);
                message.AddLong(networkID);
                OrchidNetwork.Instance.ServerSendMessageToSpecific(clientID, ref message);
            }
            
            /// <summary>
            /// Send object destroy message to all excluding a specific client.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectDestroyExcludingClient(ushort clientID, long networkID)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectDestroy);
                message.AddLong(networkID);
                OrchidNetwork.Instance.ServerSendMessageExcluding(clientID, ref message);
            }
            
             /// <summary>
            /// Send object position message to all clients.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectPositionToClients(long networkID, Vector3 position)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectPosition);
                message.AddLong(networkID);
                message.AddVector3(position);
                OrchidNetwork.Instance.ServerSendMessageToAll(ref message);
            }
            
            /// <summary>
            /// Send object position message to a specific client.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectPositionToClient(ushort clientID, long networkID, Vector3 position)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectPosition);
                message.AddLong(networkID);
                message.AddVector3(position);
                OrchidNetwork.Instance.ServerSendMessageToSpecific(clientID, ref message);
            }
            
            /// <summary>
            /// Send object position message to all excluding a specific client.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectPositionExcludingClient(ushort clientID, long networkID, Vector3 position)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectPosition);
                message.AddLong(networkID);
                message.AddVector3(position);
                OrchidNetwork.Instance.ServerSendMessageExcluding(clientID, ref message);
            }
            
            /// <summary>
            /// Send object rotation message to all clients.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectRotationToClients(long networkID, Quaternion rotation)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectRotation);
                message.AddLong(networkID);
                message.AddQuaternion(rotation);
                OrchidNetwork.Instance.ServerSendMessageToAll(ref message);
            }
            
            /// <summary>
            /// Send object rotation message to a specific client.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectRotationToClient(ushort clientID, long networkID, Quaternion rotation)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectRotation);
                message.AddLong(networkID);
                message.AddQuaternion(rotation);
                OrchidNetwork.Instance.ServerSendMessageToSpecific(clientID, ref message);
            }
            
            /// <summary>
            /// Send object rotation message to all excluding a specific client.
            /// </summary>
            /// <param name="networkID"></param>
            /// <param name="prefabID"></param>
            /// <param name="position"></param>
            /// <param name="rotation"></param>
            public static void ServerSendObjectRotationExcludingClient(ushort clientID, long networkID, Quaternion rotation)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.ObjectRotation);
                message.AddLong(networkID); 
                message.AddQuaternion(rotation);
                OrchidNetwork.Instance.ServerSendMessageExcluding(clientID, ref message);
            }

            /// <summary>
            /// Send object authority update to client.
            /// </summary>
            /// <param name="clientID"></param>
            /// <param name="networkID"></param>
            /// <param name="type"></param>
            public static void ServerSendObjectAuthorityRegisterToClient(ushort clientID, long networkID,
                ClientAuthorityType type)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.RegisterAuthority);
                message.AddLong(networkID); 
                message.AddUShort((ushort)type);
                OrchidNetwork.Instance.ServerSendMessageToSpecific(clientID, ref message);
            }
            
            /// <summary>
            /// Send object authority unregister to client.
            /// </summary>
            /// <param name="clientID"></param>
            /// <param name="networkID"></param>
            /// <param name="type"></param>
            public static void ServerSendObjectAuthorityUnregisterToClient(ushort clientID, long networkID)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.UnregisterAuthority);
                message.AddLong(networkID);
                OrchidNetwork.Instance.ServerSendMessageToSpecific(clientID, ref message);
            }
            #endregion
        
        #region Client Sending Methods
            /// <summary>
            /// Send an RPC call to the server.
            /// </summary>
            /// <param name="methodID"></param>
            /// <param name="parameters"></param>
            public static void ClientSendRPCToServer(string rpcName, params object[] parameters)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.RPC);
                SerializeRPC(ref message, rpcName, parameters);
                OrchidNetwork.Instance.ClientSendMessage(ref message);
            }
        
            #endregion
        
        
        #region Serializing Methods
        /// <summary>
        /// Add the correct data to the RPC message.
        /// </summary>
        /// <param name="rpcName"></param>
        /// <param name="parameters"></param>
        /// <param name="message"></param>
        private static void SerializeRPC(ref Message message, string rpcName, params object[] parameters)
        {
            message.AddString(rpcName);

            foreach (object par in parameters)
            {
                if (par is byte)
                    message.AddByte((byte)par);
                else if (par is short)
                    message.AddShort((short)par);
                else if (par is ushort)
                    message.AddUShort((ushort)par);
                else if (par is int)
                    message.AddInt((int)par);
                else if (par is uint)
                    message.AddUInt((uint)par);
                else if (par is long)
                    message.AddLong((long)par);
                else if (par is ulong)
                    message.AddULong((ulong)par);
                else if (par is float)
                    message.AddFloat((float)par);
                else if (par is double)
                    message.AddDouble((double)par);
                else if (par is bool)
                    message.AddBool((bool)par);
                else if (par is string)
                    message.AddString((string)par);
                else if (par is Vector3)
                    message.AddVector3((Vector3)par);
                else if (par is Vector2)
                    message.AddVector2((Vector2)par);
                else if (par is Quaternion)
                    message.AddQuaternion((Quaternion)par);
                else if (par is double[])
                    message.AddDoubles((double[])par);
                else if (par is float[])
                    message.AddFloats((float[])par);
                else if (par is bool[])
                    message.AddBools((bool[])par);
                else if (par is string[])
                    message.AddStrings((string[])par);
                else if (par is byte[])
                    message.AddBytes((byte[])par);
                else
                {
                    Logger.LogError("Serialization for RPC is impossible - Incompatible parameter type: " + par.GetType());
                }
            }
        }
        #endregion
    }
}
