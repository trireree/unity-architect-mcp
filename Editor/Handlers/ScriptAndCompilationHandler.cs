using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using Antigravity.UnityMCP.Editor.Core;
using Microsoft.CSharp;
using UnityEditor;
using UnityEditor.Compilation;
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

                // Add Unity and .NET references
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                        {
                            parameters.ReferencedAssemblies.Add(assembly.Location);
                        }
                    }
                    catch { }
                }

                string fullCode = $@"
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Antigravity.DynamicExec
{{
    public class RuntimeExecutor
    {{
        public static object Run()
        {{
            {code}
        }}
    }}
}}";

                var results = csc.CompileAssemblyFromSource(parameters, fullCode);

                if (results.Errors.HasErrors)
                {
                    var errors = string.Join("\n", results.Errors.Cast<CompilerError>().Select(e => $"Line {e.Line}: {e.ErrorText}"));
                    return McpResponse.Error($"C# Compilation Error:\n{errors}");
                }

                var type = results.CompiledAssembly.GetType("Antigravity.DynamicExec.RuntimeExecutor");
                var method = type.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);
                var result = method.Invoke(null, null);

                return McpResponse.Success("Execution successful.", result?.ToString() ?? "void");
            }
            catch (Exception ex)
            {
                return McpResponse.Error($"Runtime Execution Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
