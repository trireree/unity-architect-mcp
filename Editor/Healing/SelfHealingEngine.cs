using System;
using System.Collections.Generic;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Handlers;
using Antigravity.UnityMCP.Editor.Transaction;
using Antigravity.UnityMCP.Editor.Validation;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Healing
{
    [Serializable]
    public class HealingReportDto
    {
        public bool isHealed;
        public int initialErrorCount;
        public int remainingErrorCount;
        public int attemptsUsed;
        public List<string> appliedPatches = new List<string>();
        public List<ClassifiedError> remainingErrors = new List<ClassifiedError>();
    }

    public static class SelfHealingEngine
    {
        public const int MaxRepairAttempts = 3;

        public static HealingReportDto RunSelfHealingLoop(string transactionId = null)
        {
            var report = new HealingReportDto();
            var tx = transactionId ?? TransactionManager.ActiveTransactionId;

            for (int attempt = 1; attempt <= MaxRepairAttempts; attempt++)
            {
                report.attemptsUsed = attempt;

                // 1. Collect all errors (Compile + Console + Scene Validation)
                var errors = CollectAllErrors();
                if (attempt == 1) report.initialErrorCount = errors.Count;

                if (errors.Count == 0)
                {
                    report.isHealed = true;
                    report.remainingErrorCount = 0;
                    return report;
                }

                bool anyPatchApplied = false;

                // 2. Attempt automated patches
                foreach (var err in errors)
                {
                    if (ApplyAutomatedPatch(err, out string patchDesc))
                    {
                        report.appliedPatches.Add($"Attempt {attempt}: {patchDesc}");
                        anyPatchApplied = true;
                    }
                }

                if (!anyPatchApplied)
                {
                    // No automated rule matched; return remaining errors for AI intervention
                    report.remainingErrors = errors;
                    report.remainingErrorCount = errors.Count;
                    report.isHealed = false;
                    return report;
                }

                // 3. Trigger compilation/asset refresh and verify
                AssetDatabase.Refresh();
            }

            var finalErrors = CollectAllErrors();
            report.remainingErrors = finalErrors;
            report.remainingErrorCount = finalErrors.Count;
            report.isHealed = (finalErrors.Count == 0);

            if (!report.isHealed && !string.IsNullOrEmpty(tx))
            {
                // Auto-rollback if repairs failed
                TransactionManager.RollbackTransaction(tx);
                report.appliedPatches.Add($"Auto-rolled back transaction '{tx}' due to persistent errors.");
            }

            return report;
        }

        public static List<ClassifiedError> CollectAllErrors()
        {
            var results = new List<ClassifiedError>();

            // Check Scene Validation
            var valReport = ValidationManager.ValidateScene();
            foreach (var issue in valReport.issues)
            {
                if (issue.severity == "Error")
                {
                    results.Add(new ClassifiedError
                    {
                        category = "SceneIntegrity",
                        code = issue.type,
                        message = issue.message,
                        targetObject = issue.target,
                        suggestedFix = GetSceneFixSuggestion(issue.type)
                    });
                }
            }

            // Check Console Logs
            var logsRes = PlayModeHandler.GetConsoleLogs(50, "Error");
            if (logsRes.success && !string.IsNullOrEmpty(logsRes.data))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<LogListWrapper>(logsRes.data);
                    if (wrapper?.logs != null)
                    {
                        results.AddRange(ErrorClassifier.ClassifyLogs(wrapper.logs));
                    }
                }
                catch { }
            }

            return results;
        }

        private static bool ApplyAutomatedPatch(ClassifiedError err, out string patchDesc)
        {
            patchDesc = null;

            // Automated Missing Namespace Patch for C#
            if (err.category == "CompileError" && err.code == "CS0246" && !string.IsNullOrEmpty(err.filePath) && File.Exists(err.filePath))
            {
                string content = File.ReadAllText(err.filePath);
                string missingNamespace = null;

                if (err.message.Contains("NavMesh") && !content.Contains("using UnityEngine.AI;")) missingNamespace = "using UnityEngine.AI;";
                else if (err.message.Contains("TMP_") && !content.Contains("using TMPro;")) missingNamespace = "using TMPro;";
                else if ((err.message.Contains("Image") || err.message.Contains("Button")) && !content.Contains("using UnityEngine.UI;")) missingNamespace = "using UnityEngine.UI;";

                if (!string.IsNullOrEmpty(missingNamespace))
                {
                    File.WriteAllText(err.filePath, $"{missingNamespace}\n{content}");
                    AssetDatabase.ImportAsset(err.filePath, ImportAssetOptions.ForceUpdate);
                    patchDesc = $"Injected '{missingNamespace}' into '{err.filePath}'";
                    return true;
                }
            }

            // Automated Missing Component Patch for GameObjects
            if (err.category == "SceneIntegrity" && err.code == "MissingScript" && !string.IsNullOrEmpty(err.targetObject))
            {
                var go = SceneHandler.FindGameObject(err.targetObject);
                if (go != null)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    patchDesc = $"Cleaned missing script components from GameObject '{go.name}'";
                    return true;
                }
            }

            return false;
        }

        private static string GetSceneFixSuggestion(string issueType)
        {
            switch (issueType)
            {
                case "MissingScript": return "Remove missing script component with GameObjectUtility.RemoveMonoBehavioursWithMissingScript or reassign class.";
                case "BrokenShader": return "Reassign valid Universal Render Pipeline/Lit shader to material.";
                case "MissingCamera": return "Create a Main Camera with Camera and AudioListener components.";
                default: return "Review scene hierarchy and inspector settings.";
            }
        }
    }
}
