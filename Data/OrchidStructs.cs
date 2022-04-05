using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Orchid;
using RiptideNetworking;
using System;

namespace Orchid
{
    /// <summary>
    /// Client and Server in riptide dont share any send methods in common
    /// This is so they can be dealt with as one parameter
    /// </summary>

    /// <summary>
        /// Store information about an RPC call.
        /// It's method, parameters and send type.
        /// </summary>
        public struct RPCInfo
        {
            public int methodID { get; private set; }
            public ParameterInfo[] parameters { get; private set; }

            public RPCInfo(int methodID, ParameterInfo[] parameters)
            {
                this.methodID = methodID;
                this.parameters = parameters;
            }

        }
        
}
