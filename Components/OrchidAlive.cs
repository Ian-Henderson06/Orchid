using Orchid.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Analytics;
using Logger = Orchid.Util.Logger;

/// <summary>
/// Automatically adds GameObject to the alive list of the prefab manager.
/// This makes sure the GameObject is sent in world updates without having to spawn it officially.
/// </summary>
[RequireComponent(typeof(OrchidIdentity))]
public class OrchidAlive : MonoBehaviour
{
    [SerializeField] private int prefabID = -1;
    private OrchidIdentity identity;
    
    private void Awake()
    {
        identity = GetComponent<OrchidIdentity>();
    }
    
    void Start()
    {
        if (prefabID == -1)
        {
            Logger.LogError($"OrchidAlive component does not have prefab id set. Destroying {gameObject.name}.");
            Destroy(gameObject);
        }
        
        if (!identity.HasNetworkIDAssigned())
        {
            identity.SetNetworkID(IDIssuer.GetUniqueNetworkID());
        }
        
        identity.SetPrefabID(prefabID);
        OrchidPrefabManager.Instance.AddAliveNetworkedObject(identity.GetNetworkID(), identity.GetPrefabID(), gameObject);
    }
}
