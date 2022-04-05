using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Class used to ensure proper RPC serialisation.
/// </summary>
public abstract class OrchidBehaviour : MonoBehaviour
{
    // Made start async to allow await 
    // As Orchid is replacing Unitys start and update methods.
    void Start()
    {
        OrchidStart();
    }

    // Update is called once per frame
    void Update()
    {
        OrchidUpdate();
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
