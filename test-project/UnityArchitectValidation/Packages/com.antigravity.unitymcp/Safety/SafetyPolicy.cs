#pragma warning disable CS0618, CS0619
using System;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Safety
{
    public static class SafetyPolicy
    {
        public const int MaxFileChangesPerBatch = 50;
        public const int MaxRepairAttempts = 3;

        public static bool ValidateActionSafety(BridgeRequest req, out string warning)
        {
            warning = string.Empty;
            if (req == null) return false;

            // 1. Path Traversal & Escape Check
            if (!string.IsNullOrEmpty(req.path))
            {
                if (req.path.Contains("..") || req.path.StartsWith("/") || req.path.StartsWith("\\") || (req.path.Length > 1 && req.path[1] == ':'))
                {
                    // Allow absolute paths ONLY if inside Application.dataPath or project root
                    string fullPath = Path.GetFullPath(req.path);
                    string projectRoot = Path.GetFullPath(Application.dataPath + "/..");

                    if (!fullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        warning = $"SECURITY VIOLATION: Path '{req.path}' escapes the Unity project directory. Blocked by SafetyPolicy.";
                        return false;
                    }
                }
            }

            // 2. Destructive Scene / Asset Deletions Check
            if (req.action == "gameobject_delete" && req.target == "World_Root")
            {
                warning = "CRITICAL OPERATION: Mass deletion of 'World_Root'. Snapshot must be active.";
            }

            return true;
        }

        public static bool IsSafePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (path.Contains("..")) return false;
            return path.StartsWith("Assets/") || path.StartsWith("Packages/") || path.StartsWith("Library/");
        }
    }
}
