#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [Serializable]
    public class LiveConsoleLogEntryDto
    {
        public string type; // Log, Warning, Error, Exception, Assert
        public string message;
        public string stackTrace;
        public string timestamp;
    }

    [InitializeOnLoad]
    public static class RealtimeConsoleStreamBridge
    {
        private static readonly ConcurrentQueue<LiveConsoleLogEntryDto> _logQueue = new ConcurrentQueue<LiveConsoleLogEntryDto>();
        private const int MaxLogHistory = 500;

        static RealtimeConsoleStreamBridge()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            var entry = new LiveConsoleLogEntryDto
            {
                type = type.ToString(),
                message = condition,
                stackTrace = stackTrace,
                timestamp = DateTime.UtcNow.ToString("HH:mm:ss.fff")
            };

            _logQueue.Enqueue(entry);

            while (_logQueue.Count > MaxLogHistory)
            {
                _logQueue.TryDequeue(out _);
            }
        }

        public static McpResponse GetLiveConsoleLogs(int count = 50, string filter = "All")
        {
            var logs = _logQueue.ToArray();

            IEnumerable<LiveConsoleLogEntryDto> query = logs;
            if (!string.IsNullOrEmpty(filter) && !filter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => l.type.Equals(filter, StringComparison.OrdinalIgnoreCase) ||
                                         (filter.Equals("ErrorsOnly", StringComparison.OrdinalIgnoreCase) && (l.type == "Error" || l.type == "Exception" || l.type == "Assert")));
            }

            var resultLogs = query.Reverse().Take(count).Reverse().ToList();

            return McpResponse.Success($"Retrieved {resultLogs.Count} live console logs.", JsonUtility.ToJson(new { totalInQueue = _logQueue.Count, returned = resultLogs.Count, logs = resultLogs }, true));
        }

        public static McpResponse ClearLiveConsoleLogs()
        {
            while (_logQueue.TryDequeue(out _)) { }
            return McpResponse.Success("Cleared live console logs ring buffer.");
        }
    }
}
