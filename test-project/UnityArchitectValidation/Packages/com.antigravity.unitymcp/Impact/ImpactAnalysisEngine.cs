using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Graph;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Impact
{
    [Serializable]
    public class ImpactReportDto
    {
        public string target;
        public string operation; // "DELETE", "MODIFY", "RENAME"
        public string riskLevel; // "LOW", "MEDIUM", "HIGH", "CRITICAL"
        public int affectedObjectCount;
        public int affectedSceneCount;
        public List<string> directUsers = new List<string>();
        public List<string> affectedPrefabs = new List<string>();
        public List<string> affectedScenes = new List<string>();
        public string recommendation;
    }

    public static class ImpactAnalysisEngine
    {
        public static ImpactReportDto AnalyzeImpact(string targetIdentifier, string operation = "DELETE")
        {
            var report = new ImpactReportDto
            {
                target = targetIdentifier,
                operation = operation.ToUpperInvariant()
            };

            var graph = ProjectGraphBuilder.GetOrBuildGraph(false);
            string targetName = Path.GetFileNameWithoutExtension(targetIdentifier);

            // Find matching nodes in graph
            var matchedNodes = graph.nodes.Values.Where(n =>
                n.name.Equals(targetName, StringComparison.OrdinalIgnoreCase) ||
                n.id == targetIdentifier ||
                (n.path != null && n.path.IndexOf(targetIdentifier, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            if (matchedNodes.Count == 0)
            {
                report.riskLevel = "LOW";
                report.recommendation = $"'{targetIdentifier}' not found in active graph. Operation is low risk.";
                return report;
            }

            var targetNode = matchedNodes[0];

            // 1. Find all incoming edges (who depends on or uses this target)
            var incomingEdges = graph.edges.Where(e => e.targetId == targetNode.id).ToList();
            foreach (var edge in incomingEdges)
            {
                if (graph.nodes.TryGetValue(edge.sourceId, out var sourceNode))
                {
                    string entry = $"{sourceNode.type}: {sourceNode.name} ({edge.relation})";
                    if (!report.directUsers.Contains(entry)) report.directUsers.Add(entry);

                    if (sourceNode.type == GraphNodeType.PREFAB.ToString() && !report.affectedPrefabs.Contains(sourceNode.name))
                    {
                        report.affectedPrefabs.Add(sourceNode.name);
                    }
                    if (sourceNode.type == GraphNodeType.SCENE.ToString() && !report.affectedScenes.Contains(sourceNode.name))
                    {
                        report.affectedScenes.Add(sourceNode.name);
                    }
                }
            }

            // 2. Also search if target is a C# script attached to GameObjects in scene
            if (targetNode.type == GraphNodeType.SCRIPT.ToString())
            {
                var sceneUsages = graph.nodes.Values.Where(n =>
                    n.type == GraphNodeType.COMPONENT.ToString() && n.name == targetNode.name
                ).ToList();

                foreach (var usage in sceneUsages)
                {
                    report.directUsers.Add($"Scene GameObject: {usage.path}");
                }
            }

            report.affectedObjectCount = report.directUsers.Count;
            report.affectedSceneCount = Math.Max(1, report.affectedScenes.Count);

            // 3. Determine Risk Level
            if (report.affectedObjectCount >= 10 || report.affectedPrefabs.Count > 2)
            {
                report.riskLevel = "CRITICAL";
                report.recommendation = "CRITICAL RISK: Deletion or breaking modification affects 10+ objects or key prefabs. An atomic snapshot MUST be taken.";
            }
            else if (report.affectedObjectCount >= 3)
            {
                report.riskLevel = "HIGH";
                report.recommendation = "HIGH RISK: Multiple scene objects and components depend on this asset. Review references before proceeding.";
            }
            else if (report.affectedObjectCount > 0)
            {
                report.riskLevel = "MEDIUM";
                report.recommendation = "MEDIUM RISK: 1-2 objects depend on this item.";
            }
            else
            {
                report.riskLevel = "LOW";
                report.recommendation = "LOW RISK: No active dependents found in project graph.";
            }

            return report;
        }
    }
}
