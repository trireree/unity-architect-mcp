#pragma warning disable CS0618, CS0619
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Antigravity.UnityMCP.Editor.Core;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class ScriptAndCompilationHandler
    {
        public static McpResponse CreateOrUpdateScript(string filePath, string content)
        {
            if (string.IsNullOrEmpty(filePath)) return McpResponse.Error("File path cannot be empty.");
            if (!filePath.StartsWith("Assets/")) filePath = "Assets/" + filePath.TrimStart('/');
            if (!filePath.EndsWith(".cs")) filePath += ".cs";

            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(filePath, content);
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            return McpResponse.Success($"Script saved at '{filePath}'. Compilation triggered.", filePath);
        }

        public static McpResponse GetCompilationStatus()
        {
            bool isCompiling = EditorApplication.isCompiling;
            bool isUpdating = EditorApplication.isUpdating;
            string status = $"Compiling: {isCompiling}, Updating: {isUpdating}";
            return McpResponse.Success(status, isCompiling ? "COMPILING" : "READY");
        }

        public static McpResponse ExecuteCSharpCode(string code)
        {
            try
            {
                var csc = new CSharpCodeProvider();
                var parameters = new CompilerParameters
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false
                };

                // Add sanitized references without duplicate BCL conflicts
                var addedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location) || !File.Exists(assembly.Location))
                            continue;

                        string asmName = assembly.GetName().Name;
                        if (string.Equals(asmName, "System.Private.CoreLib", StringComparison.OrdinalIgnoreCase) && addedNames.Contains("mscorlib"))
                            continue;

                        if (addedNames.Add(asmName))
                        {
                            parameters.ReferencedAssemblies.Add(assembly.Location);
                        }
                    }
                    catch { }
                }

                string wrappedCode = code.Trim();
                if (!wrappedCode.Contains("return ") && !wrappedCode.EndsWith(";"))
                {
                    wrappedCode = $"return ({wrappedCode});";
                }
                else if (!wrappedCode.Contains("return "))
                {
                    wrappedCode += "\nreturn \"Execution completed successfully.\";";
                }

                string fullSource = $@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Antigravity.DynamicExec
{{
    public static class DynamicScriptRunner
    {{
        public static object Execute()
        {{
            {wrappedCode}
        }}
    }}
}}";

                var results = csc.CompileAssemblyFromSource(parameters, fullSource);

                if (results.Errors.HasErrors)
                {
                    var errors = new List<string>();
                    foreach (CompilerError err in results.Errors)
                    {
                        if (!err.IsWarning) errors.Add($"Line {err.Line}: {err.ErrorText}");
                    }
                    if (errors.Count > 0)
                    {
                        return McpResponse.Error($"C# Dynamic Compilation Error:\n{string.Join("\n", errors)}");
                    }
                }

                var type = results.CompiledAssembly.GetType("Antigravity.DynamicExec.DynamicScriptRunner");
                var method = type.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);
                var result = method.Invoke(null, null);

                return McpResponse.Success("Execution successful.", result?.ToString() ?? "null");
            }
            catch (TargetInvocationException tex)
            {
                var inner = tex.InnerException ?? tex;
                return McpResponse.Error($"Runtime Exception: {inner.Message}\n{inner.StackTrace}");
            }
            catch (Exception ex)
            {
                return McpResponse.Error($"Execution Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
