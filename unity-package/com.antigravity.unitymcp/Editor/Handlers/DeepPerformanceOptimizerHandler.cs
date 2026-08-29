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
    public class GcIssueDto
    {
        public string file;
        public int line;
        public string issueType;
        public string snippet;
        public string recommendation;
    }

    public static class DeepPerformanceOptimizerHandler
    {
        public static McpResponse AuditAndTagStaticBatching()
        {
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            int taggedCount = 0;

            foreach (var r in renderers)
            {
                var go = r.gameObject;
                if (go.GetComponent<Rigidbody>() != null) continue; // Skip dynamic objects

                if (!go.isStatic)
                {
                    Undo.RecordObject(go, "Auto Tag Static Batching via MCP");
                    GameObjectUtility.SetStaticEditorFlags(go,
                        StaticEditorFlags.BatchingStatic |
                        StaticEditorFlags.OccludeeStatic |
                        StaticEditorFlags.OccluderStatic |
                        StaticEditorFlags.ReflectionProbeStatic);
                    taggedCount++;
                }
            }

            return McpResponse.Success($"Audited {renderers.Length} renderers. Automatically tagged {taggedCount} static environment objects for Static Batching & Occlusion Culling!", taggedCount.ToString());
        }

        public static McpResponse DetectGcAllocationsInCode(string searchFolder = "Assets")
        {
            var issues = new List<GcIssueDto>();
            string[] files = Directory.GetFiles(searchFolder, "*.cs", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                string[] lines = File.ReadAllLines(file);
                bool insideUpdate = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (Regex.IsMatch(line, @"void\s+(?:Update|FixedUpdate|LateUpdate)\s*\(\)"))
                    {
                        insideUpdate = true;
                    }
                    else if (insideUpdate && line.Trim().StartsWith("}"))
                    {
                        insideUpdate = false;
                    }

                    if (insideUpdate)
                    {
                        // 1. new allocation inside Update
                        if (Regex.IsMatch(line, @"new\s+(?!Vector2|Vector3|Quaternion|Color)[A-Za-z0-9_]+\s*\("))
                        {
                            issues.Add(new GcIssueDto
                            {
                                file = file.Replace("\\", "/"),
                                line = i + 1,
                                issueType = "GC Allocation (new Object in Update)",
                                snippet = line.Trim(),
                                recommendation = "Move instantiation to Start/Awake or use an ObjectPool<T>."
                            });
                        }

                        // 2. GetComponent inside Update
                        if (line.Contains("GetComponent<") || line.Contains("FindObject"))
                        {
                            issues.Add(new GcIssueDto
                            {
                                file = file.Replace("\\", "/"),
                                line = i + 1,
                                issueType = "GetComponent in Update loop",
                                snippet = line.Trim(),
                                recommendation = "Cache component reference in Awake/Start."
                            });
                        }

                        // 3. String concatenation in Update
                        if (line.Contains("\"") && line.Contains("+") && !line.Contains("Debug.Log"))
                        {
                            issues.Add(new GcIssueDto
                            {
                                file = file.Replace("\\", "/"),
                                line = i + 1,
                                issueType = "String concatenation in Update",
                                snippet = line.Trim(),
                                recommendation = "Use StringBuilder or cached string format."
                            });
                        }
                    }
                }
            }

            return McpResponse.Success($"GC Detective scanned C# codebase. Found {issues.Count} potential GC allocation hotspots.", JsonUtility.ToJson(new { count = issues.Count, issues = issues }, true));
        }

        public static McpResponse OptimizeTextureAndMeshImports()
        {
            int texturesUpdated = 0;
            int meshesUpdated = 0;

            string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new string[] { "Assets" });
            foreach (var guid in texGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool modified = false;
                    if (path.Contains("UI") && importer.maxTextureSize > 2048)
                    {
                        importer.maxTextureSize = 2048;
                        modified = true;
                    }
                    if (!importer.mipmapEnabled && !path.Contains("UI") && !path.Contains("Fonts"))
                    {
                        importer.mipmapEnabled = true;
                        modified = true;
                    }
                    if (modified)
                    {
                        importer.SaveAndReimport();
                        texturesUpdated++;
                    }
                }
            }

            string[] modelGuids = AssetDatabase.FindAssets("t:Model", new string[] { "Assets" });
            foreach (var guid in modelGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null && importer.isReadable)
                {
                    importer.isReadable = false; // Free CPU memory duplicate
                    importer.SaveAndReimport();
                    meshesUpdated++;
                }
            }

            return McpResponse.Success($"Optimized imports: {texturesUpdated} textures clamped/mipmapped, {meshesUpdated} meshes set to non-readable to free CPU RAM.");
        }

        public static McpResponse GenerateLodGroup(string targetObject, float lod1Distance = 0.5f, float lod2Distance = 0.2f, float cullDistance = 0.05f)
        {
            var go = SceneHandler.FindGameObject(targetObject);
            if (go == null) return McpResponse.Error($"Target GameObject '{targetObject}' not found.");

            var lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup == null) lodGroup = go.AddComponent<LODGroup>();

            var renderers = go.GetComponentsInChildren<Renderer>();
            var lods = new LOD[3];
            lods[0] = new LOD(lod1Distance, renderers);
            lods[1] = new LOD(lod2Distance, renderers);
            lods[2] = new LOD(cullDistance, new Renderer[0]);

            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();
            EditorUtility.SetDirty(go);

            return McpResponse.Success($"Configured LODGroup on '{go.name}' with distance cull thresholds ({lod1Distance:F2}, {lod2Distance:F2}, {cullDistance:F2}).");
        }
    }
}
