using Orchid;
using Logger = Orchid.Util.Logger;


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using System;
using System.Reflection;


namespace Orchid
{ 
    /// <summary>
    /// Handles all riptide receiving calls for client and server.
    /// </summary>
    internal class OrchidReceiver 
    {
        #region Client Receiving Methods
        [MessageHandler((ushort)MessageTypes.RPC)]
            private static void HandleRPCClient(Message message)
            {
                DeserializeRPC(message);
            }
            
            [MessageHandler((ushort)MessageTypes.Sync)]
            private static void HandleSync(Message message)
            {
                uint lastServerTick = message.GetUInt();
                OrchidNetwork.Instance.SetServerLastTick(lastServerTick);
            }

            [MessageHandler((ushort)MessageTypes.WorldState)]
            private static void HandleWorldState(Message message)
            {
                long networkID = message.GetLong();
                int prefabID = message.GetInt();
                uint tick = message.GetUInt();
                Vector3 position = message.GetVector3();
                
                GameObject reference = OrchidPrefabManager.Instance.FindAliveNetworkObject(networkID);
                
                //Not been locally spawned
                if (reference == null)
                {
                    _ = Network.SpawnLocalNetworkObject(networkID, prefabID, position, Quaternion.identity);
                }
                else   //Otherwise if locally spawned then make sure to set its positional info
                {
                    OrchidInterpolator interpolator = reference.GetComponent<OrchidInterpolator>();
                    
                    if(interpolator == null)
                        reference.transform.position = position;
                    else
                        interpolator.AddPosition(tick, position);
                }
            }

            [MessageHandler((ushort)MessageTypes.ObjectSpawn)]
            private static void HandleObjectSpawn(Message message)
            {
                long networkID = message.GetLong();
                int prefabID = message.GetInt();
                Vector3 position = message.GetVector3();
                Quaternion rotation = message.GetQuaternion();

                GameObject obj = GameObject.Instantiate(
                    OrchidPrefabManager.Instance.GetPrefab(prefabID), position, rotation);
                
                if (obj.GetComponent<OrchidIdentity>() is null)
                    obj.AddComponent<OrchidIdentity>();
                
                obj.GetComponent<OrchidIdentity>().SetNetworkID(networkID);
                obj.GetComponent<OrchidIdentity>().SetPrefabID(prefabID);
                
                OrchidPrefabManager.Instance.AddAliveNetworkedObject(obj);
            }

            [MessageHandler((ushort)MessageTypes.ObjectDestroy)]
            private static void HandleObjectDestroy(Message message)
            {
                long networkID = message.GetLong();
                GameObject obj = OrchidPrefabManager.Instance.FindAliveNetworkObject(networkID);
                
                if (obj != null)
                {
                    OrchidPrefabManager.Instance.RemoveAliveNetworkObject(networkID);
                    GameObject.Destroy(obj);
                }
            }

            [MessageHandler((ushort)MessageTypes.ObjectPosition)]
            private static void HandleObjectPositionUpdate(Message message)
            {
                long networkID = message.GetLong();
                uint tick = message.GetUInt();
                Vector3 position = message.GetVector3();

                GameObject aliveObject = OrchidPrefabManager.Instance.FindAliveNetworkObject(networkID);
                OrchidInterpolator interpolator = aliveObject.GetComponent<OrchidInterpolator>();
                
                if(interpolator == null)
                    aliveObject.transform.position = position;
                else
                    interpolator.AddPosition(tick, position);
            }
            
            [MessageHandler((ushort)MessageTypes.ObjectRotation)]
            private static void HandleObjectRotationUpdate(Message message)
            {
                long networkID = message.GetLong();
                Quaternion rotation = message.GetQuaternion();
                OrchidPrefabManager.Instance.FindAliveNetworkObject(networkID).transform.rotation = rotation;
            }

            [MessageHandler((ushort)MessageTypes.RegisterAuthority)]
            private static void HandleClientAuthorityRegister(Message message)
            {
                long networkID = message.GetLong();
                ClientAuthorityType authorityLevel = (ClientAuthorityType)message.GetUShort();
                OrchidAuthority.RegisterClientAuthority(networkID, (ushort)OrchidNetwork.Instance.GetLocalClientID(), authorityLevel);
            }
            
            [MessageHandler((ushort)MessageTypes.UnregisterAuthority)]
            private static void HandleClientAuthorityUnregister(Message message)
            {
                long networkID = message.GetLong();
                OrchidAuthority.UnregisterClientAuthority(networkID);
            }
            
        #endregion


        #region Server Receiving Methods
            /// <summary>
            /// Handle RPC receiving on the server.
            /// </summary>
            /// <param name="message"></param>
            [MessageHandler((ushort)MessageTypes.RPC)]
            private static void HandleRPCServer(ushort fromClientId, Message message)
            {
                DeserializeRPC(message);
            }
            
            [MessageHandler((ushort)MessageTypes.ObjectPosition)]
            private static void HandleObjectPositionUpdateServer(ushort fromClientId, Message message)
            {
                //Server should check authority first
                long networkID = message.GetLong();

                ushort? authorityOwner = OrchidAuthority.GetAuthorityOwner(networkID);
                if (authorityOwner == null)
                    return;

                if ((ushort)authorityOwner != fromClientId)
                    return;
                
                Vector3 position = message.GetVector3();
                OrchidPrefabManager.Instance.FindAliveNetworkObject(networkID).transform.position = position;
                OrchidSender.ServerSendObjectPositionToClients(networkID, position);
            }
            
            [MessageHandler((ushort)MessageTypes.ObjectRotation)]
            private static void HandleObjectRotationUpdateServer(ushort fromClientId, Message message)
            {
                //Server should check authority first
                long networkID = message.GetLong();
                
                ushort? authorityOwner = OrchidAuthority.GetAuthorityOwner(networkID);
                if (authorityOwner == null)
                    return;

                if ((ushort)authorityOwner != fromClientId)
                    return;
                
                Quaternion rotation = message.GetQuaternion();
                OrchidPrefabManager.Instance.FindAliveNetworkObject(networkID).transform.rotation = rotation;
                OrchidSender.ServerSendObjectRotationToClients(networkID, rotation);
            }
            
            [MessageHandler((ushort)MessageTypes.Input)]
            private static void HandleInputsServer(ushort fromClientId, Message message)
            {
               //If from local clienthost do nothing because that's already handled in network
               if(OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.ClientHost)
                if (fromClientId == OrchidNetwork.Instance.GetLocalClientID())
                   return;

               bool[] inputs = message.GetBools();
               
               //If not from client host then handle the input.
               IOrchidInputHandler handler = OrchidInput.GetInputHandler(fromClientId);
               if (handler == null)
               {
                   Logger.LogError($"No input handler setup for client: {fromClientId}");
                   return;
               }
               
               handler.ProcessInputs(inputs);
            }
        #endregion

        #region Deserialize Methods
        private static void DeserializeRPC(Message message)
        { 
           string rpcName = message.GetString();
           int rpcMethodId = OrchidReflector.GetId(rpcName);
           ParameterInfo[] parameterInfos = OrchidReflector.GetParameters(rpcMethodId);
           object[] parameters = new object[parameterInfos.Length];
           
           for (int i = 0; i < parameterInfos.Length; i++)
           {
               ParameterInfo info = parameterInfos[i];
               
               if (info.ParameterType == typeof(byte))
                   parameters[i] = message.GetByte();
               else if(info.ParameterType == typeof(short))
                   parameters[i] = message.GetShort();
               else if(info.ParameterType == typeof(ushort))
                   parameters[i] = message.GetUShort();
               else if(info.ParameterType == typeof(int))
                   parameters[i] = message.GetInt();
               else if(info.ParameterType == typeof(uint))
                   parameters[i] = message.GetUInt();
               else if(info.ParameterType == typeof(long))
                   parameters[i] = message.GetLong();
               else if(info.ParameterType == typeof(ulong))
                   parameters[i] = message.GetULong();
               else if(info.ParameterType == typeof(float))
                   parameters[i] = message.GetFloat();
               else if(info.ParameterType == typeof(double))
                   parameters[i] = message.GetDouble();
               else if(info.ParameterType == typeof(bool))
                   parameters[i] = message.GetBool();
               else if(info.ParameterType == typeof(string))
                   parameters[i] = message.GetString();
               else if(info.ParameterType == typeof(Vector3))
                   parameters[i] = message.GetVector3();
               else if(info.ParameterType == typeof(Vector2))
                   parameters[i] = message.GetVector2();
               else if(info.ParameterType == typeof(Quaternion))
                   parameters[i] = message.GetQuaternion();
               else if(info.ParameterType == typeof(double[]))
                   parameters[i] = message.GetDoubles();
               else if(info.ParameterType == typeof(float[]))
                   parameters[i] = message.GetFloats();
               else if(info.ParameterType == typeof(bool[]))
                   parameters[i] = message.GetBools();
               else if(info.ParameterType == typeof(string[]))
                   parameters[i] = message.GetStrings();
               else if(info.ParameterType == typeof(byte[]))
                   parameters[i] = message.GetBytes();
               else
                   throw new System.Exception("DeSerialization for RPC is impossible - Incompatible parameter type: " + info.ParameterType.GetType());
           }
           
           OrchidReflector.InvokeLocalRPC(rpcName, parameters);
        }
        
        #endregion
    }
}
