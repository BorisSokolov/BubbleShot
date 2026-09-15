using System;
using UnityEngine;

namespace BubbleShot.Runtime.Lifecycle
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        None = 4
    }

    /// <summary>
    /// Centralized logging utility with environment-based log filtering and category tagging.
    /// </summary>
    public static class GameLogger
    {
        public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;

        public static void LogDebug(string tag, string message)
        {
            if (MinimumLogLevel <= LogLevel.Debug)
            {
                Debug.Log($"[{tag}] {message}");
            }
        }

        public static void LogInfo(string tag, string message)
        {
            if (MinimumLogLevel <= LogLevel.Info)
            {
                Debug.Log($"[{tag}] {message}");
            }
        }

        public static void LogWarning(string tag, string message)
        {
            if (MinimumLogLevel <= LogLevel.Warning)
            {
                Debug.LogWarning($"[{tag}] {message}");
            }
        }

        public static void LogError(string tag, string message, Exception? exception = null)
        {
            if (MinimumLogLevel <= LogLevel.Error)
            {
                if (exception != null)
                {
                    Debug.LogError($"[{tag}] {message}\nException: {exception.Message}\n{exception.StackTrace}");
                }
                else
                {
                    Debug.LogError($"[{tag}] {message}");
                }
            }
        }
    }
}
