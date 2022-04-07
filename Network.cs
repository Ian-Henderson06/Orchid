using Orchid.Util;
using UnityEngine;
using Logger = Orchid.Util.Logger;

namespace Orchid
{
    /// <summary>
    /// Main API - Allows most functionality to take place
    /// </summary>
    public class Network
    {
        /// <summary>
        /// Spawn a networked server. Must be called by the client.
        /// </summary>
        public static void SpawnNetworkedObject(int prefabID, Vector3 position, Quaternion rotation)
        {
            if (OrchidNetwork.Instance.GetLocalNetworkType == NetworkType.Client)
            {
                Logger.LogError(
                    "Objects cannot be spawned by the client. Please check if local is server before calling SpawnNetworkedObject.");
                return;
            }

            //Spawn collider object on server then invoke clients to spawn models.
            if (OrchidNetwork.Instance.GetLocalNetworkType == NetworkType.Server)
            {
                GameObject obj = GameObject.Instantiate(
                    OrchidPrefabManager.Instance.GetPrefab(prefabID), position, rotation);

                if (obj.GetComponent<OrchidIdentity>() is null)
                    obj.AddComponent<OrchidIdentity>();

                long networkID = IDIssuer.GetUniqueNetworkID();
                
                obj.GetComponent<OrchidIdentity>().SetNetworkID(networkID);
                obj.GetComponent<OrchidIdentity>().SetPrefabID(prefabID);
                
                //Add collider to alive networked objects
                OrchidPrefabManager.Instance.AddAliveNetworkedObject(obj);
                
                OrchidSender.ServerSendObjectSpawnToClients(networkID, prefabID, position, rotation);
            }
            
            //Spawn client object on host client and then invoke other clients to spawn models.
            //Important to not spawn the object twice.
            if (OrchidNetwork.Instance.GetLocalNetworkType == NetworkType.ClientHost)
            {
                GameObject obj = GameObject.Instantiate(
                    OrchidPrefabManager.Instance.GetPrefab(prefabID), position, rotation);

                if (obj.GetComponent<OrchidIdentity>() is null)
                    obj.AddComponent<OrchidIdentity>();

                long networkID = IDIssuer.GetUniqueNetworkID();
                
                obj.GetComponent<OrchidIdentity>().SetNetworkID(networkID);
                obj.GetComponent<OrchidIdentity>().SetPrefabID(prefabID);
                
                //Add collider to alive networked objects
                OrchidPrefabManager.Instance.AddAliveNetworkedObject(obj);

                ushort? localClientID = OrchidNetwork.Instance.GetLocalClientID;
                if (localClientID == null)
                {
                    Logger.LogError("Trying to spawn object on clienthost - localid is null?");
                }
                
                //Spawn on all except current client
                OrchidSender.ServerSendObjectSpawnExcludingClient((ushort)localClientID, networkID, prefabID, position, rotation);
            }
        }
    }
}
