using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using System.Reflection;

namespace Orchid
{
    //https://forum.unity.com/threads/creating-an-rpc-command-system.515380/
    //Client needs to look through rpc methods and assign them ids in dictionary
    //Server needs to look through rpc methods and assign them ids in dictionary
    //Client dictionary needs sent to server
    //Server dictionary needs sent to client

    //Client receives rpc call from server -> looks in dictionary to find which one server wants to call and places arguments
    //Server needs to know what ids to assign the strings the programmer provides -> This must match clients ids
    
    //TRY AND SUPPORT OVERLOADED METHODS - LOOK AT AMOUNT OF ARGUMENTS SENT MAYBE? OR LOOK FOR BRACKETS ([1]) OR SOMETHING IN STRING TO TELL WHAT METHOD TO RUN
    
    
    /// <summary>
    /// Used to fetch all RPC methods via reflection.
    /// </summary>
    public static class OrchidReflector
    {
        /// <summary>
        /// Gets all classes and children of the given class.
        /// Inspired by TinyBirdNet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> GetAllClassesAndChildsOf<T>() where T : class
        {
            List<Type> types = new List<Type>();

            foreach (Type type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                types.Add(type);
            }

            return types;
        }

        /// <summary>
        /// Collects all methods with the 
        /// </summary>
        public static void GetAllRPCMethods()
        {
            List<Type> types = GetAllClassesAndChildsOf<OrchidBehaviour>();

            foreach (Type type in types)
            {
                // Fetch rpc methods
                MethodInfo[] methods = type
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(method => Attribute.IsDefined(method, typeof(OrchidRPC)))
                    .ToArray();

                foreach (var m in methods)
                {
                    Debug.Log(m.Name);
                }

                
                //.OrderBy(info => info.Name).ToArray();
                //Array.Sort(methods,
                //    delegate(MethodInfo x, MethodInfo y)
                ///   {
                //        return String.Compare(x.Name, y.Name, StringComparison.InvariantCulture);
               //     });
            }
        }
    }
}
