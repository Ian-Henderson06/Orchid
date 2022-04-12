using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orchid
{
    public enum NetworkType 
    {
        Client, 
        Server,
        ClientHost
    }

    public enum MessageTypes
    {
        RPC, //Send client to server and server to client
        Sync, //Sent server to client
        ObjectSpawn, //Sent server to client
        ObjectDestroy, //Sent server to client
        ObjectPosition, //Sent server to client and server to client if allowed
        ObjectRotation //Sent server to client and server to client if allowed
    }

    public enum ClientAuthorityType
    {
        None,
        Input,
        Full
    }
}
