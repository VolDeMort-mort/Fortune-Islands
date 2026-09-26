using System.Diagnostics;

namespace FortuneIslands.Core
{
    public static class Log
    {
        // Stripped from release builds together with argument evaluation (string interpolation included).
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Info(string message) => UnityEngine.Debug.Log(message);

        // Warnings and errors are kept in release builds on purpose.
        public static void Warning(string message) => UnityEngine.Debug.LogWarning(message);
        public static void Error(string message) => UnityEngine.Debug.LogError(message);
    }
}
