using System;
using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

namespace Orchid
{
    /// <summary>
    /// Caller = Who the RPC can be called by.
    /// </summary>
   // public enum RPCCaller
    //{
   //     Server,
    //    Client
   // }

    /// <summary>
    /// Allows a method to be executed remotely by another machine.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class OrchidRPC : Attribute
    {
        private NetworkType caller;
        private MessageSendMode sendMode;

        public NetworkType GetCaller()
        {
            return caller;
        }

        public MessageSendMode GetSendMode()
        {
            return sendMode;
        }

        public OrchidRPC(NetworkType caller, MessageSendMode sendMode = MessageSendMode.reliable)
        {
            this.caller = caller;
            this.sendMode = sendMode;
        }

    }
}
