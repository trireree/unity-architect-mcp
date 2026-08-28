using System;
using System.Collections.Generic;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [InitializeOnLoad]
    public static class PlayModeHandler
    {
        private static readonly List<LogEntry> LogHistory = new List<LogEntry>();
        private const int MaxLogCount = 200;

        static PlayModeHandler()
        {
            Application.logMessageReceivedThreaded += OnLogReceived;
        }

        private static void OnLogReceived(string condition, string stackTrace, LogType type)
        {
            lock (LogHistory)
            {
                if (LogHistory.Count >= MaxLogCount)
                {
                    LogHistory.RemoveAt(0);
                }

                LogHistory.Add(new LogEntry
                {
                    type = type.ToString(),
                    condition = condition,
                    stackTrace = stackTrace,
                    timestamp = DateTime.UtcNow.ToString("HH:mm:ss.fff")
                });
            }
        }

        public static McpResponse StartPlayMode()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
                return McpResponse.Success("Entered Play Mode.");
            }
            return McpResponse.Success("Already in Play Mode.");
        }

        public static McpResponse StopPlayMode()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return McpResponse.Success("Exited Play Mode.");
            }
            return McpResponse.Success("Already in Edit Mode.");
        }

        public static McpResponse PausePlayMode(bool pause)
        {
            EditorApplication.isPaused = pause;
            return McpResponse.Success(pause ? "Paused Play Mode." : "Unpaused Play Mode.");
        }

        public static McpResponse GetConsoleLogs(int count = 50, string filterType = null)
        {
            lock (LogHistory)
            {
                var query = LogHistory.AsEnumerable();
                if (!string.IsNullOrEmpty(filterType))
                {
                    query = query.Where(l => l.type.Equals(filterType, StringComparison.OrdinalIgnoreCase));
                }

                var list = query.TakeLast(count).ToList();
                var logsJson = JsonUtility.ToJson(new LogListWrapper { logs = list }, true);
                return McpResponse.Success($"Retrieved {list.Count} console logs.", logsJson);
            }
        }

        public static McpResponse ClearConsoleLogs()
        {
            lock (LogHistory)
            {
                LogHistory.Clear();
            }

            // Also clear editor console window
            var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor");
            if (logEntries != null)
            {
                var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                clearMethod?.Invoke(null, null);
            }

            return McpResponse.Success("Console logs cleared.");
        }
    }

    [Serializable]
    public class LogListWrapper
    {
        public List<LogEntry> logs;
    }
}
