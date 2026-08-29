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
    public class SemanticSearchResultDto
    {
        public string filePath;
        public float score;
        public string snippet;
        public int line;
    }

    public static class CodebaseRagAndVectorHandler
    {
        public static McpResponse SemanticSearchCodebase(string query, string searchFolder = "Assets", int topK = 5)
        {
            if (string.IsNullOrEmpty(query)) return McpResponse.Error("Query cannot be empty.");

            var results = new List<SemanticSearchResultDto>();
            string[] files = Directory.GetFiles(searchFolder, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".cs") || f.EndsWith(".shader") || f.EndsWith(".hlsl") || f.EndsWith(".md"))
                .ToArray();

            var queryTokens = Regex.Matches(query.ToLowerInvariant(), @"\b[a-z0-9_]+\b")
                .Cast<Match>().Select(m => m.Value).Distinct().ToArray();

            foreach (var file in files)
            {
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string lineLower = lines[i].ToLowerInvariant();
                    int matchCount = queryTokens.Count(t => lineLower.Contains(t));
                    if (matchCount > 0)
                    {
                        float score = (float)matchCount / queryTokens.Length;
                        int start = Mathf.Max(0, i - 2);
                        int end = Mathf.Min(lines.Length - 1, i + 3);
                        string snippet = string.Join("\n", lines.Skip(start).Take(end - start + 1));

                        results.Add(new SemanticSearchResultDto
                        {
                            filePath = file.Replace("\\", "/"),
                            line = i + 1,
                            score = score,
                            snippet = snippet
                        });
                    }
                }
            }

            var topResults = results.OrderByDescending(r => r.score).Take(topK).ToList();
            return McpResponse.Success($"Retrieved {topResults.Count} semantically relevant code snippets.", JsonUtility.ToJson(new { query = query, matches = topResults }, true));
        }
    }
}
