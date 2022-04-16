using Orchid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using UnityEngine;
using Network = Orchid.Network;

/// <summary>
/// Class to allow easier development with Orchid.
/// </summary>
public class OrchidBehaviour : MonoBehaviour
{
   /// <summary>
   /// Shorthand way of accessing the local network type.
   /// </summary>
    protected NetworkType localNetwork;
    
    // As Orchid is replacing Unity's start and update methods it needs to provide some overridable ones.
    void Start()
    {
        localNetwork = OrchidNetwork.Instance.GetLocalNetworkType();
        
        OrchidStart();
        
        if(localNetwork == NetworkType.Client || localNetwork == NetworkType.ClientHost)
            OrchidClientStart();
        
        if(localNetwork == NetworkType.Server || localNetwork == NetworkType.ClientHost)
            OrchidServerStart();
    }
    
    // Update is called once per frame
    void Update()
    {
        OrchidUpdate();
        
        if(localNetwork == NetworkType.Client || localNetwork == NetworkType.ClientHost)
            OrchidClientUpdate();
        
        if(localNetwork == NetworkType.Server || localNetwork == NetworkType.ClientHost)
            OrchidServerUpdate();
    }
    void FixedUpdate()
    {
        OrchidFixedUpdate();
    }

    /// <summary>
    /// Standard start method - Works the same as Unity's start method.
    /// </summary>
    protected virtual void OrchidStart(){ }

    /// <summary>
    /// Standard update method - Works the same as Unity's update method.
    /// </summary>
    protected virtual void OrchidUpdate(){ }
    
    /// <summary>
    /// Standard update method - Works the same as Unity's update method.
    /// </summary>
    protected virtual void OrchidFixedUpdate(){ }
    
    /// <summary>
    /// Start method that is only ran on the client or clienthost.
    /// </summary>
    protected virtual void OrchidClientStart(){}
    
    /// <summary>
    /// Start method that is only ran on the server or clienthost.
    /// </summary>
    protected virtual void OrchidServerStart(){}
    
    /// <summary>
    /// Update method that is only ran on the client or clienthost.
    /// </summary>
    protected virtual void OrchidClientUpdate(){}
    
    /// <summary>
    /// Update method that is only ran on the server or clienthost.
    /// </summary>
    protected virtual void OrchidServerUpdate(){}
}
