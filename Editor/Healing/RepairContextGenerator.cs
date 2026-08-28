using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Graph;
using Antigravity.UnityMCP.Editor.Knowledge;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Healing
{
    [Serializable]
    public class RepairContextDto
    {
        public string errorCode;
        public string errorMessage;
        public string filePath;
        public int errorLine;
        public string codeSnippet;
        public List<string> relevantDependencies = new List<string>();
        public string relevantKnowledge;
        public string expectedBehavior;
    }

    public static class RepairContextGenerator
    {
        public static RepairContextDto GenerateMinimalRepairContext(ClassifiedError err)
        {
            var ctx = new RepairContextDto
            {
                errorCode = err.code ?? "UNKNOWN_ERROR",
                errorMessage = err.message ?? "An error occurred",
                filePath = err.filePath,
                errorLine = err.lineNumber,
                expectedBehavior = "C# script must compile cleanly without errors and reference valid Unity APIs."
            };

            // 1. Extract Code Snippet around the line (+- 6 lines)
            if (!string.IsNullOrEmpty(err.filePath) && File.Exists(err.filePath))
            {
                try
                {
                    var lines = File.ReadAllLines(err.filePath);
                    int start = Math.Max(0, err.lineNumber - 6);
                    int end = Math.Min(lines.Length - 1, err.lineNumber + 5);

                    var snippetLines = new List<string>();
                    for (int i = start; i <= end; i++)
                    {
                        string prefix = (i + 1 == err.lineNumber) ? ">> " : "   ";
                        snippetLines.Add($"{prefix}{i + 1}: {lines[i]}");
                    }
                    ctx.codeSnippet = string.Join("\n", snippetLines);
                }
                catch { }
            }

            // 2. Fetch Relevant Dependencies from State Graph
            try
            {
                var graph = ProjectGraphBuilder.GetOrBuildGraph(false);
                string fileName = Path.GetFileNameWithoutExtension(err.filePath ?? "");
                var scriptNode = graph.nodes.Values.FirstOrDefault(n => n.name == fileName && n.type == GraphNodeType.SCRIPT.ToString());

                if (scriptNode != null)
                {
                    foreach (var edge in graph.edges.Where(e => e.sourceId == scriptNode.id || e.targetId == scriptNode.id))
                    {
                        string otherId = (edge.sourceId == scriptNode.id) ? edge.targetId : edge.sourceId;
                        if (graph.nodes.TryGetValue(otherId, out var otherNode))
                        {
                            ctx.relevantDependencies.Add($"{otherNode.type}: {otherNode.name}");
                        }
                    }
                }
            }
            catch { }

            // 3. Fetch Targeted Knowledge Snippet
            var knowledge = UnityKnowledgeIndex.SearchKnowledge(err.message ?? err.code);
            if (knowledge.Count > 0)
            {
                var top = knowledge[0];
                ctx.relevantKnowledge = $"[{top.topic}] {top.summary}\nBest Practice: {top.bestPractice}";
            }

            return ctx;
        }
    }
}
