#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class BuildAndProjectHandler
    {
        public static McpResponse BuildStandalonePlayer(string targetPlatform, string outputPath)
        {
            if (string.IsNullOrEmpty(outputPath)) outputPath = "Builds/GameBuild.exe";
            var target = BuildTarget.StandaloneWindows64;

            switch (targetPlatform?.ToLowerInvariant())
            {
                case "windows":
                case "win64":
                    target = BuildTarget.StandaloneWindows64;
                    break;
                case "webgl":
                    target = BuildTarget.WebGL;
                    break;
                case "android":
                    target = BuildTarget.Android;
                    break;
                case "osx":
                case "mac":
                    target = BuildTarget.StandaloneOSX;
                    break;
                case "linux":
                    target = BuildTarget.StandaloneLinux64;
                    break;
            }

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (!string.IsNullOrEmpty(activeScene.path))
                {
                    scenes = new string[] { activeScene.path };
                }
            }

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                return McpResponse.Success($"Build succeeded in {report.summary.totalTime.TotalSeconds:F1}s! Output: '{outputPath}' ({report.summary.totalSize / (1024 * 1024)} MB)", outputPath);
            }
            else
            {
                return McpResponse.Error($"Build failed with {report.summary.totalErrors} error(s).");
            }
        }

        public static McpResponse AddTagOrLayer(string name, bool isLayer = false)
        {
            if (string.IsNullOrEmpty(name)) return McpResponse.Error("Name cannot be empty.");

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var prop = tagManager.FindProperty(isLayer ? "layers" : "tags");

            for (int i = 0; i < prop.arraySize; i++)
            {
                var p = prop.GetArrayElementAtIndex(i);
                if (p.stringValue.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return McpResponse.Success($"{(isLayer ? "Layer" : "Tag")} '{name}' already exists.");
                }
            }

            if (!isLayer)
            {
                prop.InsertArrayElementAtIndex(prop.arraySize);
                prop.GetArrayElementAtIndex(prop.arraySize - 1).stringValue = name;
            }
            else
            {
                // Find first empty user layer slot (8 to 31)
                for (int i = 8; i < prop.arraySize; i++)
                {
                    var p = prop.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(p.stringValue))
                    {
                        p.stringValue = name;
                        break;
                    }
                }
            }

            tagManager.ApplyModifiedProperties();
            return McpResponse.Success($"Added {(isLayer ? "Layer" : "Tag")} '{name}' to ProjectSettings.");
        }
    }
}
