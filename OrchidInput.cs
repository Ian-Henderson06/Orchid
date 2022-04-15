using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Orchid.Util.Logger;


namespace Orchid
{
    /// <summary>
    /// Keeps track of the assigned input handler for each client.
    /// </summary>
    public class OrchidInput
    {
        private static Dictionary<ushort, IOrchidInputHandler> inputHandler = new Dictionary<ushort, IOrchidInputHandler>();

        /// <summary>
        /// Assign an input handler. This is in charge of dealing with input to a player controlled entity.
        /// </summary>
        /// <param name="handler"></param>
        public static void AddInputHandler(ushort clientID, IOrchidInputHandler handler)
        {
            if (inputHandler.ContainsKey(clientID))
            {
                Logger.LogWarning(
                    "Only one orchid input handler can be assigned per client. This will override the previous input handler.");

                inputHandler.Remove(clientID);
            }

            inputHandler.Add(clientID, handler);
        }

        /// <summary>
        /// Unassign the currently set input handler.
        /// </summary>
        public static void UnassignInputHandler(ushort clientID)
        {
            if (!inputHandler.ContainsKey(clientID))
            {
                Logger.LogWarning("Trying to unassign an input handler when none has been assigned.");
                return;
            }

            inputHandler.Remove(clientID);
        }

        /// <summary>
        /// Get the currently assigned input handler.
        /// </summary>
        /// <returns></returns>
        public static IOrchidInputHandler GetInputHandler(ushort clientID)
        {
            if (!inputHandler.ContainsKey(clientID))
            {
                Logger.LogWarning("There is no input handler set. Please assign one.");
                return null;
            }

            return inputHandler[clientID];
        }
    }
}
