using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Orchid
{
    /// <summary>
    /// Keeps a record of any set object authority levels.
    /// </summary>
    public class OrchidAuthority
    {
        //Every player has net objects at levels of authority
        private static Dictionary<long, ushort> objectsWithClientInputAuthority = new Dictionary<long, ushort>();
        private static Dictionary<long, ushort> objectsWithClientFullAuthority = new Dictionary<long, ushort>();

        /// <summary>
        /// Register or update a clients authority with the authority system.
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="networkID"></param>
        /// <param name="authorityLevel"></param>
        public static void RegisterClientAuthority(long networkID, ushort clientID, ClientAuthorityType authorityLevel)
        {
            if(authorityLevel == ClientAuthorityType.Full)
                objectsWithClientFullAuthority.Add(networkID, clientID);
            
            if(authorityLevel == ClientAuthorityType.Input)
                objectsWithClientInputAuthority.Add(networkID, clientID);
            
            if(authorityLevel == ClientAuthorityType.None)
                UnregisterClientAuthority(networkID);
        }

        /// <summary>
        /// Unregister a clients authority.
        /// </summary>
        /// <param name="networkID"></param>
        public static void UnregisterClientAuthority(long networkID)
        {
            if (objectsWithClientFullAuthority.ContainsKey(networkID))
                objectsWithClientFullAuthority.Remove(networkID);
            
            if (objectsWithClientInputAuthority.ContainsKey(networkID))
                objectsWithClientInputAuthority.Remove(networkID);
        }

        /// <summary>
        /// Get the authority level for an object.
        /// </summary>
        /// <param name="networkID"></param>
        /// <returns></returns>
        public static ClientAuthorityType GetAuthority(long networkID)
        {
            ClientAuthorityType type = ClientAuthorityType.None;

            if (objectsWithClientInputAuthority.ContainsKey(networkID))
                type = ClientAuthorityType.Input;
            
            if (objectsWithClientFullAuthority.ContainsKey(networkID))
                type = ClientAuthorityType.Full;

            return type;
        }
    }
}
