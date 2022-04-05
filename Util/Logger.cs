using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Orchid.Util
{
    /// <summary>
    /// Small logger class to handle logging information.
    /// Build on the premise of minimal messaging where possible.
    /// </summary>
    public class Logger 
    {
        /// <summary>
        /// Logs an Orchid Message to the console.
        /// </summary>
        /// <param name="message"></param>
        public static void LogMessage(string message)
        {
            Debug.Log(AddLogStarter(message, ""));
        }
        
        /// <summary>
        /// Logs an Orchid Warning Message to the console.
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning(string message)
        {
            Debug.LogWarning(AddLogStarter(message, "Warning: "));
        }
        
        /// <summary>
        /// Logs an Orchid Error Message to the console.
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(string message)
        {
            Debug.LogError(AddLogStarter(message, "Error: "));
        }
        
        /// <summary>
        /// Logs an Orchid Exception Message to the console.
        /// </summary>
        /// <param name="message"></param>
        public static void LogException(Exception e)
        {
            Debug.LogException(e);
        }
        
        private static string AddLogStarter(string message, string additional)
        {
            return "[Orchid] " + additional + message;
        }
    }
}
