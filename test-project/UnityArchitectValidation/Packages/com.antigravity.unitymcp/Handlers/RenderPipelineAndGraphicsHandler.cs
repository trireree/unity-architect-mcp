#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [Serializable]
    public class GraphicsQualityOverviewDto
    {
        public string activeQualityLevel;
        public string renderPipelineAsset;
        public float shadowDistance;
        public int antiAliasing;
        public bool vsyncEnabled;
        public int targetFrameRate;
    }

    public static class RenderPipelineAndGraphicsHandler
    {
        public static McpResponse GetGraphicsSettings()
        {
            var dto = new GraphicsQualityOverviewDto
            {
                activeQualityLevel = QualitySettings.names.Length > QualitySettings.GetQualityLevel() ? QualitySettings.names[QualitySettings.GetQualityLevel()] : "Default",
                renderPipelineAsset = GraphicsSettings.currentRenderPipeline != null ? GraphicsSettings.currentRenderPipeline.name : "Built-in Standard",
                shadowDistance = QualitySettings.shadowDistance,
                antiAliasing = QualitySettings.antiAliasing,
                vsyncEnabled = QualitySettings.vSyncCount > 0,
                targetFrameRate = Application.targetFrameRate
            };

            return McpResponse.Success("Retrieved Graphics and Render Pipeline settings.", JsonUtility.ToJson(dto, true));
        }

        public static McpResponse SetQualityLevel(string qualityName)
        {
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                if (QualitySettings.names[i].Equals(qualityName, StringComparison.OrdinalIgnoreCase))
                {
                    QualitySettings.SetQualityLevel(i, true);
                    return McpResponse.Success($"Set QualityLevel to '{qualityName}'.");
                }
            }
            return McpResponse.Error($"Quality level '{qualityName}' not found (available: {string.Join(", ", QualitySettings.names)}).");
        }
    }
}
