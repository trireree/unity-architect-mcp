using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antigravity.UnityMCP.Editor.Core;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Healing
{
    [Serializable]
    public class ClassifiedError
    {
        public string category; // "CompileError", "RuntimeException", "SceneIntegrity", "ShaderError"
        public string code;     // e.g. "CS0246", "NullReferenceException", "MissingScript"
        public string message;
        public string filePath;
        public int lineNumber;
        public string targetObject;
        public string suggestedFix;
    }

    public static class ErrorClassifier
    {
        private static readonly Regex CsErrorRegex = new Regex(@"(?<file>[a-zA-Z0-9_\-/\\]+\.cs)\((?<line>\d+),\d+\):\s+error\s+(?<code>CS\d+):\s+(?<msg>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<ClassifiedError> ClassifyLogs(List<LogEntry> logs)
        {
            var results = new List<ClassifiedError>();
            if (logs == null) return results;

            foreach (var log in logs)
            {
                if (log.type == "Error" || log.type == "Exception")
                {
                    results.Add(ClassifySingleError(log.condition, log.stackTrace));
                }
            }
            return results;
        }

        public static ClassifiedError ClassifySingleError(string condition, string stackTrace)
        {
            var err = new ClassifiedError
            {
                category = "Unknown",
                code = "ERR_GENERIC",
                message = condition
            };

            if (string.IsNullOrEmpty(condition)) return err;

            // 1. Check C# Compiler Error format: Assets/Scripts/Foo.cs(12,5): error CS0246: The type or namespace name...
            var match = CsErrorRegex.Match(condition);
            if (match.Success)
            {
                err.category = "CompileError";
                err.filePath = match.Groups["file"].Value.Replace("\\", "/");
                int.TryParse(match.Groups["line"].Value, out err.lineNumber);
                err.code = match.Groups["code"].Value;
                err.message = match.Groups["msg"].Value;
                err.suggestedFix = GetCsErrorSuggestion(err.code, err.message);
                return err;
            }

            // 2. Check Runtime NullReferenceException
            if (condition.Contains("NullReferenceException"))
            {
                err.category = "RuntimeException";
                err.code = "NullReferenceException";
                err.suggestedFix = "Check if public references/serialized fields are assigned or use FindFirstObjectByType / GetComponent before accessing.";
                ExtractStackLocation(stackTrace, err);
                return err;
            }

            // 3. Check MissingReferenceException
            if (condition.Contains("MissingReferenceException"))
            {
                err.category = "RuntimeException";
                err.code = "MissingReferenceException";
                err.suggestedFix = "The GameObject/Component being referenced has been destroyed. Re-instantiate or check null before access.";
                ExtractStackLocation(stackTrace, err);
                return err;
            }

            // 4. MissingComponentException
            if (condition.Contains("MissingComponentException") || condition.Contains("There is no '"))
            {
                err.category = "RuntimeException";
                err.code = "MissingComponentException";
                err.suggestedFix = "Add the required component (e.g. Rigidbody, Collider, AudioSource) using [RequireComponent] or gameObject.AddComponent<T>().";
                ExtractStackLocation(stackTrace, err);
                return err;
            }

            // 5. Shader Compilation / Pink Material Error
            if (condition.Contains("Shader error") || condition.Contains("Hidden/InternalErrorShader"))
            {
                err.category = "ShaderError";
                err.code = "ShaderError";
                err.suggestedFix = "Assign a valid Universal Render Pipeline/Lit or Standard shader to the affected material.";
                return err;
            }

            return err;
        }

        private static string GetCsErrorSuggestion(string csCode, string message)
        {
            switch (csCode.ToUpperInvariant())
            {
                case "CS0246": // Type or namespace not found
                    if (message.Contains("NavMeshAgent") || message.Contains("NavMesh")) return "Add 'using UnityEngine.AI;' at the top of the file.";
                    if (message.Contains("TMP_Text") || message.Contains("TextMeshProUGUI")) return "Add 'using TMPro;' at the top of the file.";
                    if (message.Contains("Image") || message.Contains("Button") || message.Contains("Canvas")) return "Add 'using UnityEngine.UI;' at the top of the file.";
                    if (message.Contains("Cinemachine")) return "Add 'using Cinemachine;' or install the Cinemachine package.";
                    return "Add the missing namespace with 'using <Namespace>;' or define the missing class/interface.";

                case "CS0103": // The name does not exist in current context
                    return "Check variable spelling or declare the variable before using it.";

                case "CS1002": // ; expected
                    return "Add missing semicolon ';' at the end of the statement.";

                case "CS1513": // } expected
                    return "Add missing closing brace '}' to balance class or method blocks.";

                case "CS0117": // Type does not contain definition
                    return "Check if API method/property name is correct (e.g. FindFirstObjectByType vs FindObjectOfType, localPosition vs position).";

                default:
                    return $"Review and fix C# compilation error: {message}";
            }
        }

        private static void ExtractStackLocation(string stackTrace, ClassifiedError err)
        {
            if (string.IsNullOrEmpty(stackTrace)) return;

            var match = Regex.Match(stackTrace, @"(?<file>[a-zA-Z0-9_\-/\\]+\.cs):line\s+(?<line>\d+)");
            if (match.Success)
            {
                err.filePath = match.Groups["file"].Value.Replace("\\", "/");
                int.TryParse(match.Groups["line"].Value, out err.lineNumber);
            }
        }
    }
}
