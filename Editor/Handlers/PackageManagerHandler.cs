#pragma warning disable CS0618, CS0619
using System;
using System.IO;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class PackageManagerHandler
    {
        public static McpResponse AddUpmPackage(string packageIdentifier)
        {
            if (string.IsNullOrEmpty(packageIdentifier)) return McpResponse.Error("Package identifier cannot be empty.");

            var request = Client.Add(packageIdentifier);
            // Non-blocking asynchronous start
            return McpResponse.Success($"Triggered installation for UPM package '{packageIdentifier}'. Unity is resolving dependencies.", packageIdentifier);
        }

        public static McpResponse RemoveUpmPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName)) return McpResponse.Error("Package name cannot be empty.");

            var request = Client.Remove(packageName);
            return McpResponse.Success($"Triggered removal for package '{packageName}'.", packageName);
        }

        public static McpResponse GetInstalledPackages()
        {
            string manifestPath = "Packages/manifest.json";
            if (!File.Exists(manifestPath)) return McpResponse.Error("Packages/manifest.json not found.");

            string json = File.ReadAllText(manifestPath);
            return McpResponse.Success("Retrieved project packages manifest.", json);
        }
    }
}
