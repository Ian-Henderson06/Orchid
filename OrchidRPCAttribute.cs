using System;
using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;


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
    public class OrchidRPCAttribute : Attribute
    {
        private NetworkType target;

        public NetworkType GetCaller()
        {
            return target;
        }
        
        public OrchidRPCAttribute(NetworkType target, MessageSendMode sendMode = MessageSendMode.reliable)
        {
            this.target = target;
        }

    }
}
