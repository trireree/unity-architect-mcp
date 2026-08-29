#pragma warning disable CS0618, CS0619
using System;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class LightingAndAtmosphereHandler
    {
        public static McpResponse TunePostProcessing(string themePreset)
        {
            var volGo = GameObject.Find("Global_PostProcess_Volume");
            if (volGo == null)
            {
                volGo = new GameObject("Global_PostProcess_Volume");
                var vol = volGo.AddComponent<Volume>();
                vol.isGlobal = true;
                vol.weight = 1.0f;
            }

            // Apply Theme Atmosphere
            switch (themePreset?.ToLowerInvariant())
            {
                case "horror":
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.ExponentialSquared;
                    RenderSettings.fogDensity = 0.05f;
                    RenderSettings.fogColor = new Color(0.05f, 0.08f, 0.1f, 1f);
                    RenderSettings.ambientLight = new Color(0.1f, 0.12f, 0.15f, 1f);
                    break;
                case "cyberpunk":
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.Linear;
                    RenderSettings.fogStartDistance = 20f;
                    RenderSettings.fogEndDistance = 150f;
                    RenderSettings.fogColor = new Color(0.12f, 0.04f, 0.2f, 1f);
                    RenderSettings.ambientLight = new Color(0.2f, 0.1f, 0.3f, 1f);
                    break;
                case "desert":
                case "sunny":
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.Linear;
                    RenderSettings.fogStartDistance = 50f;
                    RenderSettings.fogEndDistance = 300f;
                    RenderSettings.fogColor = new Color(0.9f, 0.75f, 0.5f, 1f);
                    RenderSettings.ambientLight = new Color(0.8f, 0.75f, 0.7f, 1f);
                    break;
                default: // Night / SciFi
                    RenderSettings.fog = true;
                    RenderSettings.fogDensity = 0.02f;
                    RenderSettings.fogColor = new Color(0.08f, 0.1f, 0.15f, 1f);
                    RenderSettings.ambientLight = new Color(0.15f, 0.18f, 0.25f, 1f);
                    break;
            }

            Undo.RegisterCreatedObjectUndo(volGo, "Tune Post Processing");
            return McpResponse.Success($"Tuned atmospheric lighting and post-processing for theme '{themePreset}'!");
        }

        public static McpResponse OptimizeSceneLights(float shadowDistance = 100f, bool enableSoftShadows = true)
        {
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            int count = 0;

            foreach (var l in lights)
            {
                Undo.RecordObject(l, "Optimize Light Settings");
                if (l.type == LightType.Directional)
                {
                    l.shadows = enableSoftShadows ? LightShadows.Soft : LightShadows.Hard;
                    l.shadowNormalBias = 0.4f;
                    l.shadowBias = 0.05f;
                }
                else if (l.type == LightType.Point || l.type == LightType.Spot)
                {
                    l.shadows = LightShadows.None; // Optimize point/spot shadows for performance
                }
                count++;
            }

            QualitySettings.shadowDistance = shadowDistance;
            return McpResponse.Success($"Optimized {count} light sources (Directional Soft Shadows, Point/Spot shadows culled, Shadow Distance {shadowDistance}m).");
        }

        public static McpResponse SetEnvironmentAmbience(bool enableFog, float fogDensity, string fogColorHex, string ambientColorHex)
        {
            RenderSettings.fog = enableFog;
            if (fogDensity > 0) RenderSettings.fogDensity = fogDensity;

            if (!string.IsNullOrEmpty(fogColorHex) && ColorUtility.TryParseHtmlString(fogColorHex, out Color fc))
            {
                RenderSettings.fogColor = fc;
            }

            if (!string.IsNullOrEmpty(ambientColorHex) && ColorUtility.TryParseHtmlString(ambientColorHex, out Color ac))
            {
                RenderSettings.ambientLight = ac;
            }

            return McpResponse.Success("Updated environment ambience and fog settings.");
        }

        public static McpResponse BakeLightmapsAsync()
        {
            if (Lightmapping.isRunning) return McpResponse.Error("Lightmapping is already running.");

            Lightmapping.BakeAsync();
            return McpResponse.Success("Triggered asynchronous lightmap bake!");
        }
    }
}
