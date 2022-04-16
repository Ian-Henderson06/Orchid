using Orchid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Orchid.Util.Logger;

/// <summary>
/// Component placed on all networked objects.
/// Stores their IDs.
/// </summary>
public class OrchidIdentity : MonoBehaviour
{
    [SerializeField] private long networkID = -1;
    [SerializeField] private int prefabID = -1;
    [SerializeField] private ClientAuthorityType clientAuthority = ClientAuthorityType.None;

    /// <summary>
    /// Set the network ID for the networked object.
    /// </summary>
    /// <param name="networkID"></param>
    public void SetNetworkID(long networkID)
    {
        if (this.networkID != -1)
        {
            Logger.LogError($"Network ID for {gameObject.name} has already been set.");
            return;
        }
        
        this.networkID = networkID;
    }
    
    /// <summary>
    /// Set the prefab ID for the networked object.
    /// </summary>
    /// <param name="networkID"></param>
    public void SetPrefabID(int prefabID)
    {
        if (this.prefabID != -1)
        {
            Logger.LogError($"Prefab ID for {gameObject.name} has already been set.");
            return;
        }
        
        this.prefabID = prefabID;
    }
    
    /// <summary>
    /// Get the network ID for the networked object.
    /// </summary>
    /// <returns></returns>
    public long GetNetworkID()
    {
        if (networkID == -1)
        {
            Logger.LogError($"You are trying to get a null Network ID for {gameObject.name}");
        }

        return networkID;
    }
    
    /// <summary>
    /// Get the prefab ID for the networked object.
    /// </summary>
    /// <returns></returns>
    public int GetPrefabID()
    {
        if (prefabID == -1)
        {
            Logger.LogError($"You are trying to get a null Prefab ID for {gameObject.name}");
        }

        return prefabID;
    }

    /// <summary>
    /// Check if a network ID has been assigned to this identity.
    /// </summary>
    /// <returns></returns>
    public bool HasNetworkIDAssigned()
    {
        if (networkID == -1)
            return false;

        return true;
    }
    
    /// <summary>
    /// Check if a prefab ID has been assigned to this identity.
    /// </summary>
    /// <returns></returns>
    public bool HasPrefabIDAssigned()
    {
        if (prefabID == -1)
            return false;

        return true;
    }

    /// <summary>
    /// Get the current client authority level.
    /// </summary>
    /// <returns></returns>
    public ClientAuthorityType GetClientAuthorityType()
    {
        return clientAuthority;
    }

    /// <summary>
    /// Sets the client authority.
    /// Note - Changing this on the client wont do anything.
    /// This is serversided.
    /// </summary>
    /// <param name="type"></param>
    public void SetClientAuthority(ClientAuthorityType authorityType)
    {
        clientAuthority = authorityType;
    }
    
}
