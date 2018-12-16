using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Utilities
{
    public static class Log
    {
        private const int MAX_LOGS = 50;

        private static readonly Dictionary<Type, Color> colorDict = new Dictionary<Type, Color>();

        private static readonly List<string> logHistory = new List<string>();

        private static Color GetColor(Type type)
        {
            Color color;
            if (colorDict.TryGetValue(type, out color))
            {
                return color;
            }

            // TODO: Generate a seed value based on given type
            // TODO: Create a few predefined color codes to pick randomly from
            color = new Color(Random.value, Random.value, Random.value);

            colorDict.Add(type, color);

            return color;
        }

        private static string GetString(string s)
        {
            Type type = GetCallerType();

            string methodName = GetMethodName();

            Color color = GetColor(type);
            string colorString = ColorUtility.ToHtmlStringRGB(color);

            return $"<color=#{colorString}>[{type.Name}:{methodName}]</color> - {s}";
        }

        private static Type GetCallerType()
        {
            StackTrace stackTrace = new StackTrace();
            return stackTrace.GetFrame(4).GetMethod().DeclaringType;
        }

        private static string GetMethodName()
        {
            StackTrace stackTrace = new StackTrace();
            return stackTrace.GetFrame(4).GetMethod().Name;
        }

        private static void DoLog(string methodName, string message)
        {
            string fullMessage = GetString(message);

            Type debugType = typeof(Debug);

            MethodInfo methodInfo = debugType.GetMethod(methodName, new[] { typeof(string) });

            if (methodInfo == null)
            {
                return;
            }

            methodInfo.Invoke(null, new object[] { fullMessage });

            logHistory.Add(fullMessage);

            if (logHistory.Count > MAX_LOGS)
            {
                logHistory.RemoveAt(0);
            }
        }

        public static void Write(string message)
        {
            DoLog("Log", message);
        }

        public static void Warn(string message)
        {
            DoLog("LogWarning", message);
        }

        public static void Error(string message)
        {
            DoLog("LogError", message);
        }

        public static Exception Exception(string message)
        {
            throw new Exception(GetString(message));
        }
    }
}
