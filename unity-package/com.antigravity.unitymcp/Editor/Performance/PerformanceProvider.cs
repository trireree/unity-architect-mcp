#pragma warning disable CS0618, CS0619
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Antigravity.UnityMCP.Editor.Performance
{
    [Serializable]
    public class PerformanceMetricsDto
    {
        public int fpsEstimate;
        public int drawCalls;
        public int batches;
        public int triangles;
        public int vertices;
        public long totalAllocatedMemoryMb;
        public long totalReservedMemoryMb;
        public long gcAllocatedMemoryMb;
        public int activeGameObjectCount;
        public string renderPipeline;
    }

    public static class PerformanceProvider
    {
        public static PerformanceMetricsDto HarvestMetrics()
        {
            var metrics = new PerformanceMetricsDto();

            // Total active objects in scene
            var allGos = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            metrics.activeGameObjectCount = allGos.Length;

            // Memory metrics
            metrics.totalAllocatedMemoryMb = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
            metrics.totalReservedMemoryMb = Profiler.GetTotalReservedMemoryLong() / (1024 * 1024);
            metrics.gcAllocatedMemoryMb = Profiler.GetMonoUsedSizeLong() / (1024 * 1024);

            // Render Pipeline
            metrics.renderPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
                ? UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().Name
                : "Built-in Render Pipeline";

            // UnityStats Reflection (Batches, DrawCalls, Tris, Verts)
            try
            {
                Type unityStatsType = typeof(EditorWindow).Assembly.GetType("UnityEditor.UnityStats");
                if (unityStatsType != null)
                {
                    var batchesProp = unityStatsType.GetProperty("batches", BindingFlags.Static | BindingFlags.Public);
                    if (batchesProp != null) metrics.batches = (int)batchesProp.GetValue(null, null);

                    var drawCallsProp = unityStatsType.GetProperty("drawCalls", BindingFlags.Static | BindingFlags.Public);
                    if (drawCallsProp != null) metrics.drawCalls = (int)drawCallsProp.GetValue(null, null);

                    var trisProp = unityStatsType.GetProperty("triangles", BindingFlags.Static | BindingFlags.Public);
                    if (trisProp != null) metrics.triangles = (int)trisProp.GetValue(null, null);

                    var vertsProp = unityStatsType.GetProperty("vertices", BindingFlags.Static | BindingFlags.Public);
                    if (vertsProp != null) metrics.vertices = (int)vertsProp.GetValue(null, null);
                }
            }
            catch { }

            return metrics;
        }
    }
}
