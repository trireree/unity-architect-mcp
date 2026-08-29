#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [Serializable]
    public class CompilationDiagnosticDto
    {
        public bool hasErrors;
        public int errorCount;
        public int warningCount;
        public List<CompilationIssueDto> issues = new List<CompilationIssueDto>();
    }

    [Serializable]
    public class CompilationIssueDto
    {
        public string severity; // Error or Warning
        public string file;
        public int line;
        public int column;
        public string code;
        public string message;
    }

    public static class ConsoleAndCompilationStreamHandler
    {
        private static readonly List<CompilationIssueDto> _lastCompilationIssues = new List<CompilationIssueDto>();

        [InitializeOnLoadMethod]
        private static void InitCompilationHook()
        {
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            _lastCompilationIssues.Clear();
            foreach (var msg in messages)
            {
                _lastCompilationIssues.Add(new CompilationIssueDto
                {
                    severity = msg.type == CompilerMessageType.Error ? "Error" : "Warning",
                    file = msg.file,
                    line = msg.line,
                    column = msg.column,
                    message = msg.message
                });
            }
        }

        public static McpResponse GetCompilationDiagnostics()
        {
            var dto = new CompilationDiagnosticDto
            {
                hasErrors = _lastCompilationIssues.Any(i => i.severity == "Error"),
                errorCount = _lastCompilationIssues.Count(i => i.severity == "Error"),
                warningCount = _lastCompilationIssues.Count(i => i.severity == "Warning"),
                issues = new List<CompilationIssueDto>(_lastCompilationIssues)
            };

            return McpResponse.Success($"Retrieved compilation diagnostics ({dto.errorCount} error(s), {dto.warningCount} warning(s)).", JsonUtility.ToJson(dto, true));
        }

        public static McpResponse GetDetailedConsoleLogs(int maxCount = 50, string filterType = "All")
        {
            var logs = new List<LogEntryDto>();
            try
            {
                var logEntriesType = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
                if (logEntriesType != null)
                {
                    var clearMethod = logEntriesType.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
                    var getCountMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
                    var getEntryMethod = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);

                    int total = (int)(getCountMethod?.Invoke(null, null) ?? 0);
                    int start = Mathf.Max(0, total - maxCount);

                    var entryType = Type.GetType("UnityEditor.LogEntry, UnityEditor.dll");
                    if (entryType != null && getEntryMethod != null)
                    {
                        var entryInstance = Activator.CreateInstance(entryType);
                        var conditionField = entryType.GetField("message") ?? entryType.GetField("condition");
                        var modeField = entryType.GetField("mode");
                        var fileField = entryType.GetField("file");
                        var lineField = entryType.GetField("line");

                        for (int i = start; i < total; i++)
                        {
                            getEntryMethod.Invoke(null, new object[] { i, entryInstance });
                            string msg = conditionField?.GetValue(entryInstance)?.ToString() ?? "";
                            int mode = (int)(modeField?.GetValue(entryInstance) ?? 0);
                            string file = fileField?.GetValue(entryInstance)?.ToString() ?? "";
                            int line = (int)(lineField?.GetValue(entryInstance) ?? 0);

                            string type = "Log";
                            if ((mode & 1) != 0 || (mode & 2) != 0) type = "Error";
                            else if ((mode & 4) != 0) type = "Warning";

                            if (filterType != "All" && !type.Equals(filterType, StringComparison.OrdinalIgnoreCase))
                                continue;

                            logs.Add(new LogEntryDto
                            {
                                type = type,
                                condition = msg,
                                stackTrace = !string.IsNullOrEmpty(file) ? $"{file}:{line}" : ""
                            });
                        }
                    }
                }
            }
            catch { }

            return McpResponse.Success($"Retrieved {logs.Count} console log entries.", JsonUtility.ToJson(new { logs = logs }, true));
        }
    }
}
