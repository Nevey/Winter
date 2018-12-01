using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Game.Utilities
{
    public static class ReflectionUtility
    {
        public static Type[] GetTypes<T>()
        {
            Type type = typeof(T);

            List<Type> types = new List<Type>();

            foreach (Assembly appDomain in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    List<Type> typesForDomain = appDomain.GetTypes()
                        .Where(p => type.IsAssignableFrom(p)).ToList();

                    types.AddRange(typesForDomain);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogErrorFormat(
                        $"Error while loading types for domain {appDomain.FullName}: {ex.Message}");
                }
            }

            return types.ToArray();
        }
    }
}
