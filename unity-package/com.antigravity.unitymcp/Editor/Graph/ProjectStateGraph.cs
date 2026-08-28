using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.State;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Graph
{
    [Serializable]
    public class ProjectStateGraph
    {
        public string version = "1.0";
        public string graphHash = "";
        public string timestamp = "";
        [NonSerialized]
        public Dictionary<string, GraphNodeDto> nodes = new Dictionary<string, GraphNodeDto>();
        public List<GraphEdgeDto> edges = new List<GraphEdgeDto>();

        private const string CacheFilePath = "Library/UnityArchitect/project_graph.json";

        public void AddNode(GraphNodeDto node)
        {
            if (node == null || string.IsNullOrEmpty(node.id)) return;
            nodes[node.id] = node;
        }

        public void AddEdge(string sourceId, string targetId, GraphRelationType relation)
        {
            if (string.IsNullOrEmpty(sourceId) || string.IsNullOrEmpty(targetId)) return;
            edges.Add(new GraphEdgeDto
            {
                sourceId = sourceId,
                targetId = targetId,
                relation = relation.ToString()
            });
        }

        public GraphNodeDto GetNode(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            nodes.TryGetValue(id, out var node);
            return node;
        }

        public List<GraphNodeDto> FindNodesByNameOrType(string query, GraphNodeType? type = null)
        {
            return nodes.Values.Where(n =>
                (string.IsNullOrEmpty(query) || (n.name != null && n.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)) &&
                (!type.HasValue || n.type == type.Value.ToString())
            ).ToList();
        }

        public List<GraphNodeDto> GetSubtree(string rootId)
        {
            var result = new List<GraphNodeDto>();
            var visited = new HashSet<string>();
            var queue = new Queue<string>();

            if (nodes.ContainsKey(rootId))
            {
                queue.Enqueue(rootId);
                visited.Add(rootId);
            }

            while (queue.Count > 0)
            {
                var currId = queue.Dequeue();
                if (nodes.TryGetValue(currId, out var node))
                {
                    result.Add(node);
                }

                foreach (var edge in edges.Where(e => e.sourceId == currId))
                {
                    if (!visited.Contains(edge.targetId) && nodes.ContainsKey(edge.targetId))
                    {
                        visited.Add(edge.targetId);
                        queue.Enqueue(edge.targetId);
                    }
                }
            }

            return result;
        }

        public string ComputeAndCacheGraphHash()
        {
            var raw = string.Join(";", nodes.Values.Select(n => $"{n.id}:{n.hash}")) + "|" + edges.Count;
            graphHash = StateHasher.ComputeSha256(raw).Substring(0, 16);
            timestamp = DateTime.UtcNow.ToString("o");
            return graphHash;
        }

        public void SaveToDisk()
        {
            try
            {
                var dir = Path.GetDirectoryName(CacheFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var json = EditorJsonUtility.ToJson(this, false);
                File.WriteAllText(CacheFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UnityArchitect] Failed to cache graph to disk: {ex.Message}");
            }
        }

        public static void SaveToDisk(ProjectStateGraph graph)
        {
            graph?.SaveToDisk();
        }

        public static ProjectStateGraph LoadFromDisk()
        {
            try
            {
                if (File.Exists(CacheFilePath))
                {
                    var json = File.ReadAllText(CacheFilePath);
                    var graph = new ProjectStateGraph();
                    EditorJsonUtility.FromJsonOverwrite(json, graph);
                    return graph;
                }
            }
            catch { }
            return new ProjectStateGraph();
        }
    }
}
