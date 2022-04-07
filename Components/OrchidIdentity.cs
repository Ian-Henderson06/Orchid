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
    [ShowOnly] [SerializeField] private long? networkID = null;
    [ShowOnly] [SerializeField] private int? prefabID;

    /// <summary>
    /// Set the network ID for the networked object.
    /// </summary>
    /// <param name="networkID"></param>
    public void SetNetworkID(long networkID)
    {
        if (this.networkID != null)
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
        if (this.prefabID != null)
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
        if (networkID == null)
        {
            Logger.LogError($"You are trying to get a null Network ID for {gameObject.name}");
            return -1;
        }

        return (long)networkID;
    }
    
    /// <summary>
    /// Get the prefab ID for the networked object.
    /// </summary>
    /// <returns></returns>
    public long GetPrefabID()
    {
        if (prefabID == null)
        {
            Logger.LogError($"You are trying to get a null Prefab ID for {gameObject.name}");
            return -1;
        }

        return (int)prefabID;
    }
    
}
