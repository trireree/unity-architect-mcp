#pragma warning disable CS0618, CS0619
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Antigravity.UnityMCP.Editor.Core;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [Serializable]
    public class DryRunCompilationResultDto
    {
        public bool isClean;
        public int errorCount;
        public int warningCount;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<string> lintViolations = new List<string>();
    }

    [Serializable]
    public class AtomicStepDto
    {
        public string action;
        public string target;
        public string name;
        public string path;
        public string componentType;
        public string text;
    }

    [Serializable]
    public class AtomicTransactionPayloadDto
    {
        public string transactionName;
        public List<AtomicStepDto> steps = new List<AtomicStepDto>();
    }

    public static class SafetyAndPreFlightValidationHandler
    {
        // 1. ROSLYN / IN-MEMORY DRY-RUN COMPILATION
        public static McpResponse DryRunCompileCSharp(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode)) return McpResponse.Error("Source code cannot be empty.");

            var dto = new DryRunCompilationResultDto();

            // Run Custom Static Linting first
            var lintIssues = LintCSharpSafety(sourceCode);
            if (lintIssues.Count > 0)
            {
                dto.lintViolations.AddRange(lintIssues);
                dto.isClean = false;
                dto.errorCount += lintIssues.Count;
                foreach (var issue in lintIssues) dto.errors.Add($"[Lint Violation] {issue}");
            }

            // In-Memory Compilation with loaded Unity assemblies
            try
            {
                var provider = new CSharpCodeProvider();
                var parameters = new CompilerParameters
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false,
                    TreatWarningsAsErrors = false
                };

                // Deduplicate assemblies (prevent CoreLib vs mscorlib clash)
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .Select(a => a.Location)
                    .Distinct()
                    .Where(loc => !loc.EndsWith("System.Private.CoreLib.dll", StringComparison.OrdinalIgnoreCase));

                foreach (var loc in assemblies)
                {
                    try { parameters.ReferencedAssemblies.Add(loc); } catch { }
                }

                var results = provider.CompileAssemblyFromSource(parameters, sourceCode);
                foreach (CompilerError err in results.Errors)
                {
                    if (err.IsWarning)
                    {
                        dto.warningCount++;
                        dto.warnings.Add($"Line {err.Line}: {err.ErrorText}");
                    }
                    else
                    {
                        dto.errorCount++;
                        dto.errors.Add($"Line {err.Line}, Col {err.Column} ({err.ErrorNumber}): {err.ErrorText}");
                    }
                }

                dto.isClean = dto.errorCount == 0;
            }
            catch (Exception ex)
            {
                dto.isClean = false;
                dto.errorCount++;
                dto.errors.Add($"Dry-Run Compilation Exception: {ex.Message}");
            }

            if (dto.isClean)
            {
                return McpResponse.Success("Dry-Run compilation passed with 0 errors! Code is safe to write to disk.", JsonUtility.ToJson(dto, true));
            }
            else
            {
                return McpResponse.Error($"Dry-Run rejected code with {dto.errorCount} error(s). File was NOT written to disk.", JsonUtility.ToJson(dto, true));
            }
        }

        // 2. CUSTOM STATIC LINTING RULES
        public static List<string> LintCSharpSafety(string code)
        {
            var violations = new List<string>();
            string[] lines = code.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Check for obsolete FindObjectsOfType
                if (line.Contains("FindObjectsOfType<") && !line.Contains("FindObjectsByType<"))
                {
                    violations.Add($"Line {i + 1}: Obsolete API 'FindObjectsOfType' detected. Use 'FindObjectsByType<T>(FindObjectsSortMode.None)'.");
                }

                // Check for new List/GameObject inside Update
                if (Regex.IsMatch(line, @"new\s+(?:List|Dictionary|GameObject)\s*\(") && code.Contains("void Update"))
                {
                    violations.Add($"Line {i + 1}: Instantiating collections/GameObjects inside Update loop causes GC spikes. Cache in Awake/Start.");
                }

                // Unchecked GetComponent chaining (e.g. GetComponent<Rigidbody>().velocity without null check)
                if (Regex.IsMatch(line, @"GetComponent<[A-Za-z0-9_]+>\(\)\.[A-Za-z0-9_]+") && !line.Contains("TryGetComponent"))
                {
                    violations.Add($"Line {i + 1}: Direct member access on GetComponent result without null check. Use TryGetComponent or cache in Awake.");
                }
            }

            return violations;
        }

        // 3. PRE-FLIGHT ASSET & ENTITY EXISTENCE VERIFICATION
        public static McpResponse PreFlightCheckEntity(string targetPathOrName, bool isAsset = true)
        {
            if (string.IsNullOrEmpty(targetPathOrName)) return McpResponse.Error("Target cannot be empty.");

            if (isAsset)
            {
                string path = targetPathOrName;
                if (!path.StartsWith("Assets/")) path = "Assets/" + path.TrimStart('/');
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null)
                {
                    // Search by name
                    string[] guids = AssetDatabase.FindAssets(targetPathOrName);
                    if (guids.Length == 0)
                    {
                        return McpResponse.Error($"[Pre-Flight Error] Target asset '{targetPathOrName}' does NOT exist in Project. Create the asset first to prevent MissingReferenceException.");
                    }
                    string resolvedPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return McpResponse.Success($"Asset found at '{resolvedPath}'.", resolvedPath);
                }
                return McpResponse.Success($"Asset verified at '{path}'.", path);
            }
            else
            {
                var go = SceneHandler.FindGameObject(targetPathOrName);
                if (go == null)
                {
                    return McpResponse.Error($"[Pre-Flight Error] Target GameObject '{targetPathOrName}' does NOT exist in active scene. Instantiate or create it first.");
                }
                return McpResponse.Success($"GameObject '{go.name}' verified in scene.", EntityIdHelper.GetIdString(go));
            }
        }

        // 4. ATOMIC TRANSACTION & AUTOMATIC ROLLBACK
        public static McpResponse ExecuteAtomicTransaction(string payloadJson)
        {
            AtomicTransactionPayloadDto payload;
            try
            {
                payload = JsonUtility.FromJson<AtomicTransactionPayloadDto>(payloadJson);
            }
            catch (Exception ex)
            {
                return McpResponse.Error($"Failed to parse atomic transaction payload: {ex.Message}");
            }

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(payload.transactionName ?? "Atomic MCP Transaction");

            var executedActions = new List<string>();

            try
            {
                foreach (var step in payload.steps)
                {
                    var bridgeReq = new BridgeRequest
                    {
                        action = step.action,
                        target = step.target,
                        name = step.name,
                        path = step.path,
                        componentType = step.componentType,
                        text = step.text
                    };

                    var res = UnityMcpBridge.ExecuteAction(bridgeReq);
                    if (!res.success)
                    {
                        // Rollback entire group on step failure!
                        Undo.RevertAllDownToGroup(undoGroup);
                        return McpResponse.Error($"Transaction '{payload.transactionName}' FAILED at step '{step.action}' on '{step.target ?? step.name}'. Entire transaction rolled back to initial state! Reason: {res.error}");
                    }
                    executedActions.Add(step.action);
                }

                Undo.CollapseUndoOperations(undoGroup);
                return McpResponse.Success($"Transaction '{payload.transactionName}' completed atomically with {executedActions.Count} successful step(s)!", JsonUtility.ToJson(executedActions));
            }
            catch (Exception ex)
            {
                Undo.RevertAllDownToGroup(undoGroup);
                return McpResponse.Error($"Transaction '{payload.transactionName}' aborted due to unhandled exception: {ex.Message}. Rolled back cleanly.");
            }
        }
    }
}
