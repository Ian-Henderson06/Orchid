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
        /// Spawn a networked object. Must be called by the server.
        /// </summary>
        public static GameObject SpawnNetworkedObject(int prefabID, Vector3 position, Quaternion rotation)
        {
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Client)
            {
                Logger.LogError(
                    "Objects cannot be spawned by the client. Please check if local is server before calling SpawnNetworkedObject.");
                return null;
            }

            //Spawn collider object on server then invoke clients to spawn models.
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Server)
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

                return obj;
            }
            
            //Spawn client object on host client and then invoke other clients to spawn models.
            //Important to not spawn the object twice.
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.ClientHost)
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

                ushort? localClientID = OrchidNetwork.Instance.GetLocalClientID();
                if (localClientID == null)
                {
                    Logger.LogError("Trying to spawn object on clienthost - local client id is null?");
                }
                
                //Spawn on all except current client
                OrchidSender.ServerSendObjectSpawnExcludingClient((ushort)localClientID, networkID, prefabID, position, rotation);
                
                return obj;
            }

            return null;
        }

        /// <summary>
        /// Update a networked objects position across the network. Must be called on the server.
        /// </summary>
        /// <param name="obj"></param>
        public static void SetPositionOnNetwork(GameObject obj, Vector3 position)
        {
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Client)
            {
                Logger.LogError(
                    "Objects position cannot be set on the client. Please check if local is server before calling SetPositionOnNetwork.");
                return;
            }

            OrchidIdentity identity = obj.GetComponent<OrchidIdentity>();
            if (identity is null)
            {
                Logger.LogError(
                    "Objects must be a Networked Object in order to change its position. Please make sure it has been spawned correctly.");
                return;
            }


            //Set objects position on server, and echos to clients 
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Server)
            {
                long networkID = identity.GetNetworkID();
                obj.transform.position = position;
                OrchidSender.ServerSendObjectPositionToClients(networkID, position);
            }

            //Spawn client object on host client and then invoke other clients to spawn models.
            //Important to not spawn the object twice.
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.ClientHost)
            {
                long networkID = identity.GetNetworkID();
                obj.transform.position = position;
                
                ushort? localClientID = OrchidNetwork.Instance.GetLocalClientID();
                if (localClientID == null)
                {
                    Logger.LogError("Trying to spawn object on clienthost - localid is null?");
                }

                //Spawn on all except current client
                OrchidSender.ServerSendObjectPositionExcludingClient((ushort)localClientID, networkID, position);
            }
        }
        
         /// <summary>
        /// Update a networked objects rotation across the network. Must be called on the server.
        /// </summary>
        /// <param name="obj"></param>
        public static void SetRotationOnNetwork(GameObject obj, Quaternion rotation)
        {
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Client)
            {
                Logger.LogError(
                    "Objects rotation cannot be set on the client. Please check if local is server before calling SetRotationOnNetwork.");
                return;
            }

            OrchidIdentity identity = obj.GetComponent<OrchidIdentity>();
            if (identity is null)
            {
                Logger.LogError(
                    "Objects must be a Networked Object in order to change its rotation. Please make sure it has been spawned correctly.");
                return;
            }


            //Set objects position on server, and echos to clients 
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Server)
            {
                long networkID = identity.GetNetworkID();
                obj.transform.rotation = rotation;
                OrchidSender.ServerSendObjectRotationToClients(networkID, rotation);
            }

            //Spawn client object on host client and then invoke other clients to spawn models.
            //Important to not spawn the object twice.
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.ClientHost)
            {
                long networkID = identity.GetNetworkID();
                obj.transform.rotation = rotation;
                
                ushort? localClientID = OrchidNetwork.Instance.GetLocalClientID();
                if (localClientID == null)
                {
                    Logger.LogError("Trying to spawn object on clienthost - localid is null?");
                }

                //Spawn on all except current client
                OrchidSender.ServerSendObjectRotationExcludingClient((ushort)localClientID, networkID, rotation);
            }
        }

        /// <summary>
        /// Destroy a networked object on the network.
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyNetworkedObject(GameObject obj)
        {
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Client)
            {
                Logger.LogError(
                    "Objects cannot be destroyed by the client. Please check if local is server before calling DestroyNetworkedObject.");
                return;
            }

            OrchidIdentity identity = obj.GetComponent<OrchidIdentity>();
            if (identity is null)
            {
                Logger.LogError(
                    "Objects must be a Networked Object in order to delete it. Please make sure it has been spawned correctly.");
                return;
            }


            //Spawn collider object on server then invoke clients to spawn models.
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Server)
            {
                long networkID = identity.GetNetworkID();
                
                OrchidPrefabManager.Instance.RemoveAliveNetworkObject(networkID);
                GameObject.Destroy(obj);
                
                OrchidSender.ServerSendObjectDestroyToClients(networkID);
            }
            
            //Spawn client object on host client and then invoke other clients to spawn models.
            //Important to not spawn the object twice.
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.ClientHost)
            {
                long networkID = identity.GetNetworkID();
                
                OrchidPrefabManager.Instance.RemoveAliveNetworkObject(networkID);
                GameObject.Destroy(obj);

                ushort? localClientID = OrchidNetwork.Instance.GetLocalClientID();
                if (localClientID == null)
                {
                    Logger.LogError("Trying to spawn object on clienthost - localid is null?");
                }
                
                //Spawn on all except current client
                OrchidSender.ServerSendObjectDestroyExcludingClient((ushort)localClientID, networkID);
            }
        }
    }
}
