using Orchid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Orchid.Util.Logger;

/// <summary>
/// Holds references to prefabs for client/server.
/// Serverside references are primarily for collisions.
/// Clientside references are for models to be spawned.
/// Clienthost uses clientside models for collisions.
/// </summary>
/// 
public class OrchidPrefabManager : MonoBehaviour
{
    //id
    //name
    //prefab
    [Header("Serverside Prefab List")] [SerializeField] List<PrefabDetails> serversidePrefabs = new List<PrefabDetails>();
    [Header("Clientside Prefab List")] [SerializeField] List<PrefabDetails> clientsidePrefabs = new List<PrefabDetails>();

    private Dictionary<int, GameObject> idToPrefab = new Dictionary<int, GameObject>();
    private Dictionary<string, GameObject> nameToPrefab = new Dictionary<string, GameObject>();
    private Dictionary<string, int> nameToId = new Dictionary<string, int>();
    private Dictionary<int, string> idToName = new Dictionary<int, string>();
    private Dictionary<long, int> netIdToPrefabId = new Dictionary<long, int>();
    
    private Dictionary<long, GameObject> aliveNetworkedObjects = new Dictionary<long, GameObject>();
    private List<GameObject> aliveNetworkObjectsList = new List<GameObject>();

    private TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
    
    private static OrchidPrefabManager _instance;
    public static OrchidPrefabManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    private void Start()
    {
        if (serversidePrefabs.Count > 0 && clientsidePrefabs.Count > 0)
        {
            Logger.LogError("Only one list should be populated in PrefabManager. Clienthost - clientside prefabs. Server - serverside prefabs. Client - clientside prefabs, ");
            return;
        }

        PopulateDictionaries();
    }

    /// <summary>
    /// Wait until the prefab manager has initialised.
    /// </summary>
    public async Task WaitUntilInit()
    {
        await tcs.Task;
    }

    /// <summary>
    /// Populate dictionaries from the prefabs lists to ensure quick access to prefab data.
    /// </summary>
    private void PopulateDictionaries()
    {
        List<PrefabDetails> localTable;

        if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Server)
            localTable = serversidePrefabs;
        else
            localTable = clientsidePrefabs;

        foreach (PrefabDetails details in localTable)
        {
            idToPrefab.Add(details.objectID, details.objectPrefab);
            nameToPrefab.Add(details.objectName, details.objectPrefab);
            nameToId.Add(details.objectName, details.objectID);
            idToName.Add(details.objectID, details.objectName);
        }
        
        tcs.SetResult(1);
    }
    
    /// <summary>
    /// Add a reference of the alive network object.
    /// </summary>
    /// <param name="obj"></param>
    public void AddAliveNetworkedObject(GameObject obj)
    {
        if (obj == null)
        {
            Logger.LogError("Instantiated network object is null - cant add a reference to the prefab manager.");
            return;
        }

        OrchidIdentity identity = obj.GetComponent<OrchidIdentity>();

        if (identity is null)
        {
            Logger.LogError("Could not find OrchidIdentity on GameObject to add to alive list. Please make sure it is a network object.");
        }
        
        
        aliveNetworkedObjects.Add(identity.GetNetworkID(), obj);
        netIdToPrefabId.Add(identity.GetNetworkID(), identity.GetPrefabID());
    }
    
    /// <summary>
    /// Add a reference of the alive network object.
    /// </summary>
    /// <param name="obj"></param>
    public void AddAliveNetworkedObject(long networkID, int prefabID, GameObject obj)
    {
        if (obj == null)
        {
            Logger.LogError("Instantiated network object is null - cant add a reference to the prefab manager.");
            return;
        }


        aliveNetworkedObjects.Add(networkID, obj);
        netIdToPrefabId.Add(networkID, prefabID);
    }

    /// <summary>
    /// Remove a reference to the alive network object.
    /// </summary>
    /// <param name="obj"></param>
    public void RemoveAliveNetworkObject(long networkID)
    {
       // if (networkID == null)
       // {
       //     Logger.LogError("Provided object is null - cant remove object when provided object is null.");
       //     return;
      //  }

        if (!aliveNetworkedObjects.ContainsKey(networkID))
        {
            Logger.LogError("Object you wish to remove is not in the alive list.");
            return;
        }

        aliveNetworkedObjects.Remove(networkID);
        netIdToPrefabId.Remove(networkID);
    }

    /// <summary>
    /// Get the alive network objects GameObject via its id.
    /// </summary>
    /// <param name="networkID"></param>
    /// <returns></returns>
    public GameObject FindAliveNetworkObject(long networkID)
    {
        if (aliveNetworkedObjects.ContainsKey(networkID))
            return aliveNetworkedObjects[networkID];
        
        return null;
    }
    
    /// <summary>
    /// Get the alive network objects that are of a specific prefab.
    /// </summary>
    /// <param name="networkID"></param>
    /// <returns></returns>
    public List<GameObject> FindAliveNetworkObjectOfPrefab(int prefabID)
    {
        List<GameObject> alive = new List<GameObject>();
        foreach (KeyValuePair<long, GameObject> kv in aliveNetworkedObjects)
        {
            if(gameObject.GetComponent<OrchidIdentity>().GetPrefabID() == prefabID)
                alive.Add(kv.Value);
        }

        return alive;
    }

    /// <summary>
    /// Get all currently alive networked game objects.
    /// </summary>
    /// <returns></returns>
    public Dictionary<long, GameObject> GetAliveNetworkedGameObjects()
    {
        return aliveNetworkedObjects;
    }

    /// <summary>
    /// Get an prefabs prefab via its prefab ID.
    /// </summary>
    public GameObject GetPrefab(int prefabId)
    {
        if (!idToPrefab.ContainsKey(prefabId))
        {
            Logger.LogError($"{prefabId} is not in the local dictionary.");
            return null;
        }
        
        return idToPrefab[prefabId];
    }
    
    /// <summary>
    /// Get an prefabs prefab via its name.
    /// </summary>
    public GameObject GetPrefab(string name)
    {
        if (!nameToPrefab.ContainsKey(name))
        {
            Logger.LogError($"{name} is not in the local dictionary.");
            return null;
        }
        
        return nameToPrefab[name];
    }

    /// <summary>
    /// Get an prefabs ID via its name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int GetPrefabID(string name)
    {
        if (!nameToId.ContainsKey(name))
        {
            Logger.LogError($"{name} is not in the local dictionary.");
            return -1;
        }
        
        return nameToId[name];
    }
    
    /// <summary>
    /// Get an prefabs ID via its net id.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int GetPrefabID(long networkID)
    {
        if (!netIdToPrefabId.ContainsKey(networkID))
        {
            Logger.LogError($"{networkID} is not in the local dictionary.");
            return -1;
        }
        
        return netIdToPrefabId[networkID];
    }
    
    /// <summary>
    /// Get an prefabs name via its prefab ID.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetName(int id)
    {
        if (!idToName.ContainsKey(id))
        {
            Logger.LogError($"{id} is not in the local dictionary.");
            return "";
        }
        
        return idToName[id];
    }
    
}
