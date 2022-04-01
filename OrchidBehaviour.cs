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
    // As Orchid is replacing Unitys start and update methods - Orchid provides a task method to allow async/await to be used.
    async void Start()
    {
        OrchidStart();
        
        await OrchidTask();
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
    /// Async method - Replacement for async void Start(). Takes place following OrchidStart method.
    /// </summary>
    /// <returns></returns>
    protected virtual Task OrchidTask() { return null; }
    
    /// <summary>
    /// Standard update method - Works the same as Unity's update method.
    /// </summary>
    protected abstract void OrchidUpdate();
}
