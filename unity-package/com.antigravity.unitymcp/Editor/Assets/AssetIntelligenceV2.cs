#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Assets
{
    [Serializable]
    public class AssetMetadataDto
    {
        public string guid;
        public string path;
        public string type;
        public long fileSizeBytes;
        public int dependencyCount;
        public List<string> dependencies = new List<string>();
    }

    [Serializable]
    public class AssetInventoryReportDto
    {
        public int totalAssets;
        public int modelCount;
        public int textureCount;
        public int materialCount;
        public int audioCount;
        public int scriptCount;
        public int prefabCount;
        public List<AssetMetadataDto> assets = new List<AssetMetadataDto>();
    }

    public static class AssetIntelligenceV2
    {
        public static AssetInventoryReportDto InspectProjectAssets()
        {
            var report = new AssetInventoryReportDto();
            var allGuids = AssetDatabase.FindAssets("", new[] { "Assets" });

            foreach (var guid in allGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Directory.Exists(path)) continue;

                var type = AssetDatabase.GetMainAssetTypeAtPath(path)?.Name ?? "Asset";
                long size = 0;
                try { size = new FileInfo(path).Length; } catch { }

                var deps = AssetDatabase.GetDependencies(path, false).ToList();

                var item = new AssetMetadataDto
                {
                    guid = guid,
                    path = path,
                    type = type,
                    fileSizeBytes = size,
                    dependencyCount = deps.Count,
                    dependencies = deps
                };

                report.assets.Add(item);
                report.totalAssets++;

                if (type.Contains("Texture") || path.EndsWith(".png") || path.EndsWith(".jpg")) report.textureCount++;
                else if (type.Contains("Material") || path.EndsWith(".mat")) report.materialCount++;
                else if (type.Contains("GameObject") || path.EndsWith(".prefab")) report.prefabCount++;
                else if (type.Contains("MonoScript") || path.EndsWith(".cs")) report.scriptCount++;
                else if (type.Contains("Audio") || path.EndsWith(".wav") || path.EndsWith(".mp3")) report.audioCount++;
                else if (path.EndsWith(".fbx") || path.EndsWith(".obj") || path.EndsWith(".gltf")) report.modelCount++;
            }

            return report;
        }
    }
}
