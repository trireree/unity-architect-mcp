using System;
using System.Collections.Generic;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Graph;

namespace Antigravity.UnityMCP.Editor.State
{
    public static class StateDiffEngine
    {
        private static ProjectStateGraph _previousGraph;

        public static StateDiffDto ComputeDiff(ProjectStateGraph currentGraph)
        {
            var diff = new StateDiffDto
            {
                previousHash = _previousGraph?.graphHash ?? "none",
                currentHash = currentGraph.graphHash
            };

            if (_previousGraph == null)
            {
                // Everything is added initially
                diff.addedCount = currentGraph.nodes.Count;
                diff.added = currentGraph.nodes.Values.Take(20).Select(n => $"{n.type}: {n.name} ({n.path})").ToList();
                _previousGraph = currentGraph;
                return diff;
            }

            var prevNodes = _previousGraph.nodes;
            var currNodes = currentGraph.nodes;

            // Detect Added & Modified
            foreach (var kvp in currNodes)
            {
                var id = kvp.Key;
                var currNode = kvp.Value;

                if (!prevNodes.TryGetValue(id, out var prevNode))
                {
                    diff.addedCount++;
                    if (diff.added.Count < 20) diff.added.Add($"{currNode.type}: {currNode.name} ({currNode.path})");
                }
                else if (prevNode.hash != currNode.hash)
                {
                    diff.modifiedCount++;
                    if (diff.modified.Count < 20) diff.modified.Add($"{currNode.type}: {currNode.name} (hash changed)");
                }
                else
                {
                    diff.unchangedCount++;
                }
            }

            // Detect Removed
            foreach (var kvp in prevNodes)
            {
                if (!currNodes.ContainsKey(kvp.Key))
                {
                    diff.removedCount++;
                    var prevNode = kvp.Value;
                    if (diff.removed.Count < 20) diff.removed.Add($"{prevNode.type}: {prevNode.name} ({prevNode.path})");
                }
            }

            _previousGraph = currentGraph;
            return diff;
        }

        public static void SetBaseline(ProjectStateGraph baseline)
        {
            _previousGraph = baseline;
        }
    }
}
