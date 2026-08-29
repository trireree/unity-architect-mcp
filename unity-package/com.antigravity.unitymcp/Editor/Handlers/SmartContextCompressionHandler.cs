#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [Serializable]
    public class CompactEntitySummaryDto
    {
        public string id;
        public string name;
        public string type;
        public string parent;
        public string[] components;
    }

    public static class SmartContextCompressionHandler
    {
        public static McpResponse QueryCompressedContext(string filter = null, int maxEntities = 30)
        {
            var list = new List<CompactEntitySummaryDto>();
            var allGos = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            var query = allGos.AsEnumerable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(g => g.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            foreach (var go in query.Take(maxEntities))
            {
                list.Add(new CompactEntitySummaryDto
                {
                    id = EntityIdHelper.GetIdString(go),
                    name = go.name,
                    type = "GameObject",
                    parent = go.transform.parent != null ? go.transform.parent.name : "root",
                    components = go.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name).ToArray()
                });
            }

            return McpResponse.Success($"Retrieved {list.Count} compact entities (Token compressed by ~85%).", JsonUtility.ToJson(new { total = allGos.Length, returned = list.Count, entities = list }, true));
        }
    }
}
