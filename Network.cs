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
                long networkID = IDIssuer.GetUniqueNetworkID();
                GameObject obj = SpawnLocalNetworkObject(networkID, prefabID, position, rotation);
                
                OrchidSender.ServerSendObjectSpawnToClients(networkID, prefabID, position, rotation);
                return obj;
            }
            
            //Spawn client object on host client and then invoke other clients to spawn models.
            //Important to not spawn the object twice.
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.ClientHost)
            {
                long networkID = IDIssuer.GetUniqueNetworkID();
                GameObject obj = SpawnLocalNetworkObject(networkID, prefabID, position, rotation);
                
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
        /// Update a networked objects position across the network. Must be called on the server. Or on client with authority.
        /// </summary>
        /// <param name="obj"></param>
        public static void SetPositionOnNetwork(GameObject obj, Vector3 position)
        {
            OrchidIdentity identity = obj.GetComponent<OrchidIdentity>();
            if (identity is null)
            {
                Logger.LogError(
                    "Objects must be a Networked Object in order to change its position. Please make sure it has been spawned correctly.");
                return;
            }
            
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Client)
            {
                if (OrchidAuthority.GetAuthority(identity.GetNetworkID()) != ClientAuthorityType.Full)
                {
                    Logger.LogError(
                        "You do not have authority over this object. Please get the server to assign you authority.");
                    return;
                }
                
                OrchidSender.ClientSendObjectPositionToServer(identity.GetNetworkID(), position);
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
        /// Update a networked objects rotation across the network. Must be called on the server. Or on client with authority.
        /// </summary>
        /// <param name="obj"></param>
        public static void SetRotationOnNetwork(GameObject obj, Quaternion rotation)
        {
            OrchidIdentity identity = obj.GetComponent<OrchidIdentity>();
            if (identity is null)
            {
                Logger.LogError(
                    "Objects must be a Networked Object in order to change its rotation. Please make sure it has been spawned correctly.");
                return;
            }
            
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Client)
            {
                if (OrchidAuthority.GetAuthority(identity.GetNetworkID()) != ClientAuthorityType.Full)
                {
                    Logger.LogError(
                        "You do not have authority over this object. Please get the server to assign you authority.");
                    return;
                }
                
                OrchidSender.ClientSendObjectRotationToServer(identity.GetNetworkID(), rotation);
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

        /// <summary>
        /// Spawn an object locally on the server or clients without spawning on network.
        /// </summary>
        /// <returns></returns>
        public static GameObject SpawnLocalNetworkObject(long networkID, int prefabID, Vector3 position, Quaternion rotation)
        {
            GameObject obj = GameObject.Instantiate(
                OrchidPrefabManager.Instance.GetPrefab(prefabID), position, rotation);

            if (obj.GetComponent<OrchidIdentity>() is null)
                obj.AddComponent<OrchidIdentity>();

            obj.GetComponent<OrchidIdentity>().SetNetworkID(networkID);
            obj.GetComponent<OrchidIdentity>().SetPrefabID(prefabID);
                
            //Add collider to alive networked objects
            OrchidPrefabManager.Instance.AddAliveNetworkedObject(networkID, prefabID, obj);

            ushort? localClientID = OrchidNetwork.Instance.GetLocalClientID();
            if (localClientID == null)
            {
                Logger.LogError("Trying to spawn object on clienthost - local client id is null?");
            }

            return obj;
        }

        /// <summary>
        /// Registers a client authority on the network. Can only be called by server.
        /// </summary>
        public static void RegisterClientAuthority(GameObject obj, ushort clientID, ClientAuthorityType type)
        {
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Client)
            {
                Logger.LogError(
                    "Objects cannot be have authority set by the client. Please check if local is server before calling RegisterClientAuthority.");
                return;
            }
            
            OrchidIdentity identity = obj.GetComponent<OrchidIdentity>();
            if (identity is null)
            {
                Logger.LogError(
                    "Objects must be a Networked Object in order to change its authority level. Please make sure it has been spawned correctly.");
                return;
            }

            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.ClientHost)
            {
                OrchidAuthority.RegisterClientAuthority(identity.GetNetworkID(), (ushort)OrchidNetwork.Instance.GetLocalClientID(), type);
                
                //If we are trying to set the local clients id
                if (clientID == (ushort)OrchidNetwork.Instance.GetLocalClientID())
                    return;
                
                //Else send out to the specific client
                OrchidSender.ServerSendObjectAuthorityRegisterToClient(clientID, identity.GetNetworkID(), type);
            }
            
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Server)
            {
                OrchidAuthority.RegisterClientAuthority(identity.GetNetworkID(), (ushort)OrchidNetwork.Instance.GetLocalClientID(), type);
                OrchidSender.ServerSendObjectAuthorityRegisterToClient(clientID, identity.GetNetworkID(), type);
            }
        }
        
        /// <summary>
        /// Registers a client authority on the network. Can only be called by server.
        /// </summary>
        public static void UnRegisterClientAuthority(GameObject obj, ushort clientID, ClientAuthorityType type)
        {
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Client)
            {
                Logger.LogError(
                    "Objects cannot be have authority unregistered by the client. Please check if local is server before calling UnRegisterClientAuthority.");
                return;
            }
            
            OrchidIdentity identity = obj.GetComponent<OrchidIdentity>();
            if (identity is null)
            {
                Logger.LogError(
                    "Objects must be a Networked Object in order to change its authority level. Please make sure it has been spawned correctly.");
                return;
            }

            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.ClientHost)
            {
                OrchidAuthority.UnregisterClientAuthority(identity.GetNetworkID());
                
                //If we are trying to set the local clients id
                if (clientID == (ushort)OrchidNetwork.Instance.GetLocalClientID())
                    return;
                
                //Else send out to the specific client
                OrchidSender.ServerSendObjectAuthorityUnregisterToClient(clientID, identity.GetNetworkID());
            }
            
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Server)
            {
                OrchidAuthority.UnregisterClientAuthority(identity.GetNetworkID());
                OrchidSender.ServerSendObjectAuthorityUnregisterToClient(clientID, identity.GetNetworkID());
            }
        }

        /// <summary>
        /// Send inputs from client to server.
        /// </summary>
        public static void SendInputs(long networkID, bool[] inputs)
        {
            //If we have no local authority for this object then dont do anything
            if (OrchidAuthority.GetAuthority(networkID) == ClientAuthorityType.None)
            {
                Logger.LogError(
                    "Objects must be a Networked Object in order to send over inputs. Please make sure it has been spawned correctly.");
                return;
            }
            
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Client)
            {
               //Send inputs to server
            }
            
            //Set objects position on server, and echos to clients 
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.Server)
            {
               //Not allowed to send inputs
            }
            
            if (OrchidNetwork.Instance.GetLocalNetworkType() == NetworkType.ClientHost)
            {
             //Just apply inputs locally
            }
        }
    }
}
