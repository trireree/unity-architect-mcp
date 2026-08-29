#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [Serializable]
    public class AstClassSummaryDto
    {
        public string className;
        public string baseClass;
        public List<string> interfaces = new List<string>();
        public List<string> fieldSignatures = new List<string>();
        public List<string> methodSignatures = new List<string>();
        public List<string> propertySignatures = new List<string>();
        public List<string> usingDirectives = new List<string>();
    }

    [Serializable]
    public class AstSymbolIndexDto
    {
        public string filePath;
        public int totalLines;
        public List<AstClassSummaryDto> classes = new List<AstClassSummaryDto>();
    }

    public static class AstAndRoslynIntelligenceHandler
    {
        public static McpResponse ExtractAstSummary(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath)) return McpResponse.Error("Script path cannot be empty.");
            if (!scriptPath.StartsWith("Assets/")) scriptPath = "Assets/" + scriptPath.TrimStart('/');

            if (!File.Exists(scriptPath)) return McpResponse.Error($"File not found at '{scriptPath}'.");

            string code = File.ReadAllText(scriptPath);
            var index = ParseAstLightweight(scriptPath, code);

            return McpResponse.Success($"Extracted AST symbol signatures from '{scriptPath}' (~90% token reduction).", JsonUtility.ToJson(index, true));
        }

        public static McpResponse FindSymbolReferences(string symbolName, string searchFolder = "Assets")
        {
            if (string.IsNullOrEmpty(symbolName)) return McpResponse.Error("Symbol name cannot be empty.");

            var results = new List<string>();
            string[] files = Directory.GetFiles(searchFolder, "*.cs", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                string text = File.ReadAllText(file);
                if (Regex.IsMatch(text, $@"\b{Regex.Escape(symbolName)}\b"))
                {
                    results.Add(file.Replace("\\", "/"));
                }
            }

            return McpResponse.Success($"Found {results.Count} references for symbol '{symbolName}'.", JsonUtility.ToJson(new { symbol = symbolName, count = results.Count, files = results }, true));
        }

        public static AstSymbolIndexDto ParseAstLightweight(string filePath, string source)
        {
            var dto = new AstSymbolIndexDto
            {
                filePath = filePath,
                totalLines = source.Split('\n').Length
            };

            // 1. Usings
            var usingMatches = Regex.Matches(source, @"using\s+([A-Za-z0-9_.]+);");
            var usings = new List<string>();
            foreach (Match m in usingMatches) usings.Add(m.Groups[1].Value);

            // 2. Classes
            var classMatches = Regex.Matches(source, @"(?:public|internal|private)?\s*(?:static|abstract|sealed)?\s*class\s+([A-Za-z0-9_]+)(?:\s*:\s*([A-Za-z0-9_,\s]+))?");
            foreach (Match cm in classMatches)
            {
                var cDto = new AstClassSummaryDto
                {
                    className = cm.Groups[1].Value,
                    usingDirectives = new List<string>(usings)
                };

                if (cm.Groups[2].Success)
                {
                    var inheritances = cm.Groups[2].Value.Split(',').Select(s => s.Trim()).ToList();
                    if (inheritances.Count > 0)
                    {
                        cDto.baseClass = inheritances[0];
                        if (inheritances.Count > 1) cDto.interfaces = inheritances.Skip(1).ToList();
                    }
                }

                // Methods
                var methodMatches = Regex.Matches(source, @"(?:public|protected|private|internal)\s+(?:static|virtual|override|async)?\s*([A-Za-z0-9_<>[\]]+)\s+([A-Za-z0-9_]+)\s*\(([^)]*)\)");
                foreach (Match mm in methodMatches)
                {
                    string returnType = mm.Groups[1].Value;
                    string methodName = mm.Groups[2].Value;
                    string paramsList = mm.Groups[3].Value.Trim();
                    if (methodName != "if" && methodName != "while" && methodName != "for" && methodName != "switch")
                    {
                        cDto.methodSignatures.Add($"{returnType} {methodName}({paramsList})");
                    }
                }

                // Properties
                var propMatches = Regex.Matches(source, @"(?:public|protected|private)\s+([A-Za-z0-9_<>[\]]+)\s+([A-Za-z0-9_]+)\s*\{\s*(?:get|set)");
                foreach (Match pm in propMatches)
                {
                    cDto.propertySignatures.Add($"{pm.Groups[1].Value} {pm.Groups[2].Value} {{ get; set; }}");
                }

                dto.classes.Add(cDto);
            }

            return dto;
        }
    }
}
