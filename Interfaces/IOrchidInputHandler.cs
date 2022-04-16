using System.Collections;
using System.Collections.Generic;


public interface IOrchidInputHandler
{
    /// <summary>
    /// Process inputs for this networked entity.
    /// </summary>
    /// <param name="inputs"></param>
    public void ProcessInputs(bool[] inputs);
}
