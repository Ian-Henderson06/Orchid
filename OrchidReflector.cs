using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Orchid.Util.Logger;

using System.Reflection;
using System.Threading.Tasks;


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
    
    

    // <summary>
    /// Used to fetch all RPC methods via reflection.
    /// </summary>
    internal static class OrchidReflector
    {
        public delegate void RPCDelegate(params object[] arguments); //Delegate for a void function with any number of parameters.
            
        private static Dictionary<string, int> methodNameToId = new Dictionary<string, int>();
        private static Dictionary<int, Type> methodIdToType = new Dictionary<int, Type>();

        private static Dictionary<int, MethodInfo> methodIdToInfo =
            new Dictionary<int, MethodInfo>();
        
        private static Dictionary<int, ParameterInfo[]> methodIdToParameters =
            new Dictionary<int, ParameterInfo[]>();
        
      //  private static List<OrchidStructs.RPCInfo> rpcMethods = new List<OrchidStructs.RPCInfo>();
        private static HashSet<Type> allowedTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(bool), typeof(string),
            typeof(Vector3), typeof(Quaternion), typeof(Vector2), typeof(double[]), typeof(float[]), typeof(bool[]), typeof(string[]), typeof(byte[])
        };

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
        /// Gets all classes and children of the given class.
        /// Inspired by TinyBirdNet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> GetAllClassesThatImplement<T>()
        {
            List<Type> types = new List<Type>();
            var desiredType = typeof(T);

          //  var type = typeof(T);
            foreach(Type type in  AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => desiredType.IsAssignableFrom(p)))
            {
                types.Add(type);
            }

            return types;
        }

        /// <summary>
        /// Collects all methods with the OrchidRPC attribute.
        /// </summary>
        public static Task GetAllRPCMethods()
        {
            //List<Type> types = GetAllClassesAndChildsOf<OrchidBehaviour>();
            List<Type> types = GetAllClassesThatImplement<IOrchidRPC>();

            foreach (Type type in types)
            {
                // Fetch local rpc methods for class
                MethodInfo[] methods = type
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(method => Attribute.IsDefined(method, typeof(OrchidRPCAttribute)))
                    .ToArray();

                foreach (MethodInfo m in methods)
                {
                    ParameterInfo[] parameters = m.GetParameters();
                    OrchidRPCAttribute rpc = (OrchidRPCAttribute)m.GetCustomAttributes(typeof(OrchidRPCAttribute), true)[0]; //Only ever one OrchidRPC attribute per method so we can just get the first one.
                    
                    
                    //Check Parameters
                    foreach (ParameterInfo par in parameters)
                    {
                        //Not an allowed type
                        if (!allowedTypes.Contains(par.ParameterType))
                        {
                            Logger.LogError("Incompatible parameter used: " + par.Name + " typeof: " + par.ParameterType);
                            return Task.CompletedTask;
                        }
                    }
                    
                    Logger.LogMessage(m.Name + " Registered");

                    //If everything is good then assign it to the list
                    //RPCInfo info = new RPCInfo(methodIdToInfo.Count, parameters);

                    int methodId = methodNameToId.Count;
                    
                    methodIdToInfo.Add(methodId, m);
                    
                    //Assign its index in the list
                    methodNameToId.Add(m.Name, methodId);
                    
                    //Assign its type in the list
                    methodIdToType.Add(methodId, type);
                    
                    //Assign its parameters in the list
                    methodIdToParameters.Add(methodId, parameters);
                }
            }

            return Task.CompletedTask;
        }
        
        public static void InvokeLocalRPC(string name, params object[] parameters)
        {
            int id = -1;
            try
            {
                 id = methodNameToId[name];
            }
            catch (Exception e)
            {
                Logger.LogError("Cannot find RPC method requested.");
                return;
            }

            methodIdToInfo[id].Invoke(null, parameters);
        }
        
        #region Getters
        /// <summary>
        /// Get parameters for specified RPC method.
        /// </summary>
        /// <param name="methodId"></param>
        /// <returns></returns>
        public static ParameterInfo[] GetParameters(int methodId)
        {
            return methodIdToParameters[methodId];
        }
        
        /// <summary>
        /// Get Id for specified RPC name.
        /// </summary>
        /// <param name="methodId"></param>
        /// <returns></returns>
        public static int GetId(string methodName)
        {
            if (methodNameToId.ContainsKey(methodName))
                return methodNameToId[methodName];
            else
            {
                Logger.LogError($"RPC called {methodName} was not found");
                throw new Exception();
            }
            // throw new Exception(String.Format("RPC called {0} was not found", methodName));
        }
        
        #endregion
        
    }
}
