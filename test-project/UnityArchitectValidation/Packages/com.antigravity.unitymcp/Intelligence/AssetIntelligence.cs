using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Intelligence
{
    [Serializable]
    public class AssetDependencyReportDto
    {
        public string assetPath;
        public string assetType;
        public List<string> dependencies = new List<string>();
        public List<string> referencedBy = new List<string>();
    }

    [Serializable]
    public class DuplicateAssetDto
    {
        public string originalPath;
        public string duplicatePath;
        public long fileSize;
        public string hash;
    }

    public static class AssetIntelligence
    {
        public static AssetDependencyReportDto GetAssetDependencies(string assetPath)
        {
            var report = new AssetDependencyReportDto
            {
                assetPath = assetPath,
                assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath)?.Name ?? "Unknown"
            };

            // Direct and indirect dependencies
            string[] deps = AssetDatabase.GetDependencies(assetPath, true);
            foreach (var dep in deps)
            {
                if (!dep.Equals(assetPath, StringComparison.OrdinalIgnoreCase))
                {
                    report.dependencies.Add(dep);
                }
            }

            return report;
        }

        public static List<DuplicateAssetDto> FindDuplicateAssets(string folder = "Assets")
        {
            var duplicates = new List<DuplicateAssetDto>();
            var hashMap = new Dictionary<string, string>(); // hash -> first path

            var guids = AssetDatabase.FindAssets("", new[] { folder });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Directory.Exists(path)) continue;

                try
                {
                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Length == 0) continue;

                    string fileHash = $"{fileInfo.Length}_{Path.GetFileName(path)}";
                    if (hashMap.TryGetValue(fileHash, out var existingPath))
                    {
                        duplicates.Add(new DuplicateAssetDto
                        {
                            originalPath = existingPath,
                            duplicatePath = path,
                            fileSize = fileInfo.Length,
                            hash = fileHash
                        });
                    }
                    else
                    {
                        hashMap[fileHash] = path;
                    }
                }
                catch { }
            }

            return duplicates;
        }
    }
}
