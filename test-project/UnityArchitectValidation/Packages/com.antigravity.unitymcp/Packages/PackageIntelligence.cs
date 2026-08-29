using System;
using System.Collections.Generic;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Packages
{
    [Serializable]
    public class PackageStatusDto
    {
        public string packageName;
        public string displayName;
        public bool isInstalled;
        public string version;
    }

    [Serializable]
    public class PackageManifestReportDto
    {
        public int totalInstalled;
        public List<PackageStatusDto> keyPackages = new List<PackageStatusDto>();
    }

    public static class PackageIntelligence
    {
        private static readonly Dictionary<string, string> KeyPackageMap = new Dictionary<string, string>
        {
            { "com.unity.render-pipelines.universal", "Universal Render Pipeline (URP)" },
            { "com.unity.inputsystem", "New Input System" },
            { "com.unity.cinemachine", "Cinemachine Camera System" },
            { "com.unity.ai.navigation", "AI Navigation & NavMesh Surface" },
            { "com.unity.textmeshpro", "TextMeshPro UI" },
            { "com.unity.timeline", "Timeline Sequencer" },
            { "com.unity.addressables", "Addressable Asset System" }
        };

        public static PackageManifestReportDto InspectPackages()
        {
            var report = new PackageManifestReportDto();
            string manifestPath = "Packages/manifest.json";

            string manifestJson = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : "{}";

            foreach (var kvp in KeyPackageMap)
            {
                bool installed = manifestJson.Contains(kvp.Key);
                report.keyPackages.Add(new PackageStatusDto
                {
                    packageName = kvp.Key,
                    displayName = kvp.Value,
                    isInstalled = installed,
                    version = installed ? "Installed" : "Not Installed"
                });

                if (installed) report.totalInstalled++;
            }

            return report;
        }
    }
}
