using System;
using System.Collections.Generic;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Graph;
using Antigravity.UnityMCP.Editor.State;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Intelligence
{
    [Serializable]
    public class ContextQueryResponse
    {
        public string query;
        public string directAnswer;
        public List<string> matchingNodes = new List<string>();
        public List<string> relatedComponents = new List<string>();
        public List<string> dependencies = new List<string>();
    }

    public static class ContextIntelligence
    {
        public static ContextQueryResponse QueryGraph(string naturalQuery)
        {
            var graph = ProjectGraphBuilder.GetOrBuildGraph(false);
            var response = new ContextQueryResponse { query = naturalQuery };
            string lowerQuery = naturalQuery.ToLowerInvariant();

            // 1. Locate specific objects (e.g. "Where is Player?", "Find Main Camera")
            var matchedObjects = graph.nodes.Values.Where(n =>
                n.type == GraphNodeType.GAMEOBJECT.ToString() &&
                lowerQuery.Contains(n.name.ToLowerInvariant())
            ).ToList();

            if (matchedObjects.Count > 0)
            {
                var primary = matchedObjects[0];
                response.directAnswer = $"Found '{primary.name}' at hierarchy path '{primary.path}' (ID: {primary.id}).";
                response.matchingNodes = matchedObjects.Select(m => $"{m.name} [{m.path}]").ToList();

                // Find attached components
                foreach (var edge in graph.edges.Where(e => e.sourceId == primary.id && e.relation == GraphRelationType.HAS_COMPONENT.ToString()))
                {
                    if (graph.nodes.TryGetValue(edge.targetId, out var compNode))
                    {
                        response.relatedComponents.Add(compNode.name);
                    }
                }

                // Find dependencies (materials, scripts)
                foreach (var edge in graph.edges.Where(e => e.sourceId == primary.id || response.relatedComponents.Contains(e.sourceId)))
                {
                    if (graph.nodes.TryGetValue(edge.targetId, out var depNode))
                    {
                        response.dependencies.Add($"{depNode.type}: {depNode.name} ({depNode.path})");
                    }
                }
                return response;
            }

            // 2. Query Scripts / Usages (e.g. "What objects use PlayerController?")
            var matchedScripts = graph.nodes.Values.Where(n =>
                n.type == GraphNodeType.SCRIPT.ToString() &&
                lowerQuery.Contains(n.name.ToLowerInvariant())
            ).ToList();

            if (matchedScripts.Count > 0)
            {
                var script = matchedScripts[0];
                var usages = graph.nodes.Values.Where(n =>
                    n.type == GraphNodeType.COMPONENT.ToString() && n.name == script.name
                ).Select(n => n.path).Distinct().ToList();

                response.directAnswer = $"Script '{script.name}' is attached to {usages.Count} GameObject(s).";
                response.matchingNodes = usages;
                return response;
            }

            // 3. Fallback: Search general nodes
            var generalMatches = graph.FindNodesByNameOrType(naturalQuery);
            response.directAnswer = $"Found {generalMatches.Count} matching nodes in Project State Graph.";
            response.matchingNodes = generalMatches.Take(15).Select(n => $"{n.type}: {n.name} ({n.path})").ToList();

            return response;
        }
    }
}
