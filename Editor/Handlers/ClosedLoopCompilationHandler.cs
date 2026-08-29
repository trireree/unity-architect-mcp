#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Healing;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [Serializable]
    public class CompilationFeedbackResultDto
    {
        public bool success;
        public bool isCompiledClean;
        public int errorCount;
        public int warningCount;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public string filePath;
        public string autoFixSummary;
    }

    public static class ClosedLoopCompilationHandler
    {
        public static McpResponse WriteAndVerifyScript(string filePath, string sourceCode, bool autoFix = true)
        {
            if (string.IsNullOrEmpty(filePath)) return McpResponse.Error("File path cannot be empty.");
            if (!filePath.StartsWith("Assets/")) filePath = "Assets/" + filePath.TrimStart('/');
            if (!filePath.EndsWith(".cs")) filePath += ".cs";

            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // 1. Write file to disk
            File.WriteAllText(filePath, sourceCode);
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            // 2. Collect initial diagnostics
            var initialErrors = ConsoleAndCompilationStreamHandler.GetCompilationDiagnostics();
            var result = new CompilationFeedbackResultDto
            {
                filePath = filePath,
                isCompiledClean = !EditorUtility.scriptCompilationFailed,
                errorCount = 0
            };

            // If compile errors present and autoFix requested, trigger SelfHealingEngine
            if (EditorUtility.scriptCompilationFailed && autoFix)
            {
                var healReport = SelfHealingEngine.RunSelfHealingLoop();
                result.autoFixSummary = healReport.isHealed ? "Auto-healed all compilation errors!" : $"Heal loop finished with {healReport.remainingErrorCount} remaining issue(s).";
                result.isCompiledClean = healReport.isHealed;
            }

            return McpResponse.Success(result.isCompiledClean ? $"Script '{filePath}' compiled cleanly with zero errors!" : $"Script '{filePath}' saved, but compilation requires review.", JsonUtility.ToJson(result, true));
        }
    }
}
