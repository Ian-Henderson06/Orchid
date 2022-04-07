using RiptideNetworking;
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
            /// Send an RPC call to all clients.
            /// </summary>
            /// <param name="methodID"></param>
            /// <param name="parameters"></param>
            public static void SendRPCToClients(string rpcName, params object[] parameters)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.OrchidRPC);
                SerializeRPC(ref message, rpcName, parameters);
                OrchidNetwork.Instance.ServerSendMessageToAll(ref message);
            }
            
            /// <summary>
            /// Send an RPC call to a specific client.
            /// </summary>
            /// <param name="methodID"></param>
            /// <param name="parameters"></param>
            public static void SendRPCToClient(ushort clientID, string rpcName, params object[] parameters)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.OrchidRPC);
                SerializeRPC(ref message, rpcName, parameters);
                OrchidNetwork.Instance.ServerSendMessageToSpecific(clientID, ref message);
            }
        #endregion
        
        #region Client Sending Mthods
            /// <summary>
            /// Send an RPC call to the server.
            /// </summary>
            /// <param name="methodID"></param>
            /// <param name="parameters"></param>
            public static void SendRPCToServer(string rpcName, params object[] parameters)
            {
                Message message = Message.Create(MessageSendMode.reliable, (ushort)MessageTypes.OrchidRPC);
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
