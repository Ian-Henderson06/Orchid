using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Deprecated Class used to ensure proper RPC serialisation.
/// </summary>
public abstract class OrchidBehaviour : MonoBehaviour
{
    // As Orchid is replacing Unity's start and update methods it needs to provide some overridable ones.
    void Start()
    {
        OrchidStart();
    }

    // Update is called once per frame
    void Update()
    {
        OrchidUpdate();
    }

    void FixedUpdate()
    {
    }

    /// <summary>
    /// Standard start method - Works the same as Unity's start method.
    /// </summary>
    protected abstract void OrchidStart();
    
    
    /// <summary>
    /// Standard update method - Works the same as Unity's update method.
    /// </summary>
    protected abstract void OrchidUpdate();
}
