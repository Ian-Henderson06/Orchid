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
            /// <summary>
            /// Handle RPC receiving on client.
            /// </summary>
            /// <param name="message"></param>
            /// <exception cref="Exception"></exception>
            [MessageHandler((ushort)MessageTypes.RPC)]
            private static void HandleRPCClient(Message message)
            {
                DeserializeRPC(message);
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
