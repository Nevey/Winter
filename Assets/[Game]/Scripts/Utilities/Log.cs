using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Utilities
{
    public static class Log
    {
        private static readonly Dictionary<Type, Color> colorDict = new Dictionary<Type, Color>();

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

        private static string GetString(Type type, string s)
        {
            Color color = GetColor(type);
            string colorString = ColorUtility.ToHtmlStringRGB(color);

            return $"<color=#{colorString}>[{type.Name}]</color> - {s}";
        }

        private static Type GetCallerType()
        {
            StackTrace stackTrace = new StackTrace();
            return stackTrace.GetFrame(2).GetMethod().DeclaringType;
        }

        public static void Write(string s)
        {
            Type callerType = GetCallerType();
            Debug.LogFormat(GetString(callerType, s));
        }

        public static void Write(string s, params object[] args)
        {
            Type callerType = GetCallerType();
            string formattedS = string.Format(s, args);
            Debug.LogFormat(GetString(callerType, formattedS));
        }

        public static void Error(string s)
        {
            Type callerType = GetCallerType();
            Debug.LogErrorFormat(GetString(callerType, s));
        }

        public static void Error(Object target, string s)
        {
            Type callerType = GetCallerType();
            Debug.LogErrorFormat(target, GetString(callerType, s));
        }

        public static void Error(string s, params object[] args)
        {
            Type callerType = GetCallerType();

            string formattedS = string.Format(s, args);
            Debug.LogErrorFormat(GetString(callerType, formattedS));
        }

        public static void Warn(string s)
        {
            Type callerType = GetCallerType();
            Debug.LogWarningFormat(GetString(callerType, s));
        }

        public static void Warn(string s, params object[] args)
        {
            Type callerType = GetCallerType();

            string formattedS = string.Format(s, args);
            Debug.LogWarningFormat(GetString(callerType, formattedS));
        }

        public static Exception Exception(string s)
        {
            throw new Exception(GetString(GetCallerType(), s));
        }
    }
}
