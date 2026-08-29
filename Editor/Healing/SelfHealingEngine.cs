#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
        public const int MaxRepairAttempts = 2; // Strict limit to prevent infinite loops

        public static HealingReportDto RunSelfHealingLoop(string transactionId = null)
        {
            var report = new HealingReportDto();
            var tx = transactionId ?? TransactionManager.ActiveTransactionId;
            int lastErrorCount = int.MaxValue;

            for (int attempt = 1; attempt <= MaxRepairAttempts; attempt++)
            {
                report.attemptsUsed = attempt;

                var errors = CollectAllErrors();
                if (attempt == 1) report.initialErrorCount = errors.Count;

                if (errors.Count == 0)
                {
                    report.isHealed = true;
                    report.remainingErrorCount = 0;
                    return report;
                }

                // If errors did not decrease after an attempt, break out immediately
                if (errors.Count >= lastErrorCount && attempt > 1)
                {
                    report.remainingErrors = errors;
                    report.remainingErrorCount = errors.Count;
                    report.isHealed = false;
                    report.appliedPatches.Add("Self-Healing loop broken: Errors require AI / developer manual code edit.");
                    return report;
                }
                lastErrorCount = errors.Count;

                bool anyPatchApplied = false;
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
                    report.remainingErrors = errors;
                    report.remainingErrorCount = errors.Count;
                    report.isHealed = false;
                    return report;
                }

                AssetDatabase.Refresh();
            }

            var finalErrors = CollectAllErrors();
            report.remainingErrors = finalErrors;
            report.remainingErrorCount = finalErrors.Count;
            report.isHealed = (finalErrors.Count == 0);

            return report;
        }

        public static List<ClassifiedError> CollectAllErrors()
        {
            var results = new List<ClassifiedError>();

            // 1. Scene Integrity Check
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

            // 2. Console Logs Check
            var logsRes = PlayModeHandler.GetConsoleLogs(30, "Error");
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

            // 1. Missing Namespaces (CS0246 / CS0234)
            if (err.category == "CompileError" && (err.code == "CS0246" || err.code == "CS0234") && !string.IsNullOrEmpty(err.filePath) && File.Exists(err.filePath))
            {
                string content = File.ReadAllText(err.filePath);
                string missingUsing = null;

                if (err.message.Contains("NavMesh") && !content.Contains("using UnityEngine.AI;")) missingUsing = "using UnityEngine.AI;";
                else if (err.message.Contains("TMP_") && !content.Contains("using TMPro;")) missingUsing = "using TMPro;";
                else if ((err.message.Contains("Image") || err.message.Contains("Button") || err.message.Contains("Canvas")) && !content.Contains("using UnityEngine.UI;")) missingUsing = "using UnityEngine.UI;";
                else if (err.message.Contains("EditorSceneManager") && !content.Contains("using UnityEditor.SceneManagement;")) missingUsing = "using UnityEditor.SceneManagement;";

                if (!string.IsNullOrEmpty(missingUsing))
                {
                    File.WriteAllText(err.filePath, $"{missingUsing}\n{content}");
                    AssetDatabase.ImportAsset(err.filePath, ImportAssetOptions.ForceUpdate);
                    patchDesc = $"Injected '{missingUsing}' into '{err.filePath}'";
                    return true;
                }
            }

            // 2. Obsolete FindObjectsOfType Call Replacement
            if (err.category == "CompileError" && !string.IsNullOrEmpty(err.filePath) && File.Exists(err.filePath))
            {
                string content = File.ReadAllText(err.filePath);
                if (content.Contains("FindObjectsOfType<"))
                {
                    string patched = Regex.Replace(content, @"FindObjectsOfType<([^>]+)>\(\)", "FindObjectsByType<$1>(FindObjectsSortMode.None)");
                    if (patched != content)
                    {
                        File.WriteAllText(err.filePath, patched);
                        AssetDatabase.ImportAsset(err.filePath, ImportAssetOptions.ForceUpdate);
                        patchDesc = $"Updated deprecated FindObjectsOfType to FindObjectsByType in '{err.filePath}'";
                        return true;
                    }
                }
            }

            // 3. Clean Missing Scripts from Scene GameObjects
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

            // 4. Missing Meta File Auto-Generator
            if (err.message.Contains("has no meta file"))
            {
                AssetDatabase.ForceReserializeAssets();
                patchDesc = "Triggered AssetDatabase.ForceReserializeAssets() for missing meta files.";
                return true;
            }

            return false;
        }

        private static string GetSceneFixSuggestion(string issueType)
        {
            switch (issueType)
            {
                case "MissingScript": return "Remove missing MonoBehaviour component or reattach script asset.";
                case "UnassignedReference": return "Assign required serialized field reference.";
                case "DuplicateCamera": return "Disable secondary AudioListener or redundant Camera.";
                default: return "Inspect GameObject hierarchy and component state.";
            }
        }
    }
}
