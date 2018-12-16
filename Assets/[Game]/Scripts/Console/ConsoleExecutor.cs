using System;
using System.Linq;
using System.Reflection;
using Game.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scripts.Console
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleExecutableAttribute : Attribute
    {

    }

    public class ConsoleExecutor
    {
        private const string COMMAND_FIND_COMMANDS = "find_commands";

        private MethodInfo[] methods;

        public ConsoleExecutor()
        {
            FindConsoleExecutables();
        }

        private void FindConsoleExecutables()
        {
            methods = ReflectionUtility.GetMethodsWithCustomAttribute<ConsoleExecutableAttribute>();
        }

        private MethodInfo GetMethodInfo(string methodName)
        {
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name == methodName)
                {
                    return methods[i];
                }
            }

            return null;
        }

        private void ExecuteCommand(string command)
        {
            string[] s = command.Split(' ');

            string methodName = s[0];
            methodName = methodName.First().ToString().ToUpper()
                         + String.Join("", methodName.Skip(1));

            MethodInfo methodInfo = GetMethodInfo(methodName);

            if (methodInfo == null)
            {
                Log.Write($"Unknown command <{methodName}>");
                return;
            }

            ParameterInfo[] parameters = methodInfo.GetParameters();

            object[] @params = new object[parameters.Length];

            for (int i = 0; i < @params.Length; i++)
            {
                if (i >= s.Length - 1)
                {
                    @params[i] = Type.Missing;
                }
                else
                {
                    @params[i] = s[i + 1];
                }
            }

            try
            {
                Type type = methodInfo.DeclaringType;

                if (type.BaseType == typeof(MonoBehaviour))
                {
                    Object[] objects = MonoBehaviour.FindObjectsOfType(type);

                    for (int i = 0; i < objects.Length; i++)
                    {
                        methodInfo.Invoke(objects[i], @params);
                    }
                }
                else
                {
                    Log.Warn("No support for executing commands on non-MonoBehaviours yet...");
                }

            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }

        public void HandleCommand(string command)
        {
            if (command == COMMAND_FIND_COMMANDS)
            {
                methods = ReflectionUtility.GetMethodsWithCustomAttribute<ConsoleExecutableAttribute>();
            }
            else
            {
                ExecuteCommand(command);
            }
        }
    }
}
