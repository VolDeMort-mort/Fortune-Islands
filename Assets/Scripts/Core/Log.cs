using System.Diagnostics;

namespace FortuneIslands.Core
{
    public static class Log
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Info(string message) => UnityEngine.Debug.Log(message);
        public static void LogWarning(string message) => UnityEngine.Debug.Log(message);
        public static void LogError(string message) => UnityEngine.Debug.Log(message);

    }
}
