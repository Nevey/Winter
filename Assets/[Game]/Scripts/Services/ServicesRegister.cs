using System;
using System.Collections.Generic;
using Game.Utilities;
using UnityEngine;

namespace Game.Services
{
    public static class ServicesRegister
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnAfterSceneLoadRuntimeMethod()
        {
            InitializeAllServices();
        }

        private static void InitializeAllServices()
        {
            Log.Write("===== Start Instantiating Services =====");

            Type[] types = ReflectionUtility.GetTypes<Service>();

            List<Service> services = new List<Service>();

            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].IsAbstract)
                {
                    continue;
                }

                Service service = (Service)Activator.CreateInstance(types[i]);
                service.SetInstance(service);

                Log.Write($"{service.GetType().Name}");

                services.Add(service);
            }

            Log.Write("===== Finish Instantiating Services =====");
            Log.Write("===== Start Initializing Services =====");

            for (int i = 0; i < services.Count; i++)
            {
                services[i].Initialize();

                Log.Write($"{services[i].GetType().Name}");
            }

            Log.Write("===== Finish Initializing Services =====");
        }
    }
}
