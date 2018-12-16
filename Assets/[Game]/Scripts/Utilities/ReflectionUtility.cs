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

            List<Type> typeList = new List<Type>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly appDomain in assemblies)
            {
                try
                {
                    Type[] types = appDomain.GetTypes().Where(p => type.IsAssignableFrom(p))
                        .ToArray();

                    typeList.AddRange(types);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Log.Error(
                        $"Error while loading types for domain {appDomain.FullName}: {ex.Message}");
                }
            }

            return typeList.ToArray();
        }

        public static Type GetType(string typeString)
        {
            Type type = null;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly appDomain in assemblies)
            {
                try
                {
                    Type[] types = appDomain.GetTypes();

                    for (int i = 0; i < types.Length; i++)
                    {
                        Type t = types[i];

                        // This can easily cause ambiguous cases...
                        if (t.Name == typeString)
                        {
                            type = t;
                            break;
                        }
                    }

                    if (type != null)
                    {
                        break;
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Log.Warn(
                        $"Error while loading types for domain {appDomain.FullName}: {ex.Message}");
                }
            }

            return type;
        }

        public static MethodInfo[] GetMethodsWithCustomAttribute<T>() where T : Attribute
        {
            List<MethodInfo> methodList = new List<MethodInfo>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly appDomain in assemblies)
            {
                try
                {
                    Type[] types = appDomain.GetTypes();

                    foreach (Type type in types)
                    {
                        MethodInfo[] methods = type.GetMethods();

                        foreach (MethodInfo methodInfo in methods)
                        {
                            T attribute = methodInfo.GetCustomAttribute<T>();

                            if (attribute == null)
                            {
                                continue;
                            }

                            methodList.Add(methodInfo);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Log.Error(
                        $"Error while loading types for domain {appDomain.FullName}: {ex.Message}");
                }
            }

            return methodList.ToArray();
        }
    }
}
