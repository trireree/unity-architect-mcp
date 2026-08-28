#pragma warning disable CS0618, CS0619
using System;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class VisionHandler
    {
        public static McpResponse CaptureGameView(int width, int height)
        {
            var camera = Camera.main ?? UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                return McpResponse.Error("No active Camera found in scene to render Game View.");
            }

            if (width <= 0) width = 1280;
            if (height <= 0) height = 720;

            var renderTexture = new RenderTexture(width, height, 24);
            var prevTarget = camera.targetTexture;
            var prevActive = RenderTexture.active;

            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;

                var texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
                texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture2D.Apply();

                byte[] pngBytes = texture2D.EncodeToPNG();
                string base64 = Convert.ToBase64String(pngBytes);

                UnityEngine.Object.DestroyImmediate(texture2D);

                return McpResponse.Success($"Captured Game View ({width}x{height})", base64);
            }
            finally
            {
                camera.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        public static McpResponse CaptureSceneView(int width, int height)
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                return McpResponse.Error("No active Scene View found.");
            }

            var camera = sceneView.camera;
            if (camera == null)
            {
                return McpResponse.Error("Scene View camera is unavailable.");
            }

            if (width <= 0) width = (int)sceneView.position.width;
            if (height <= 0) height = (int)sceneView.position.height;

            if (width <= 0) width = 1280;
            if (height <= 0) height = 720;

            var renderTexture = new RenderTexture(width, height, 24);
            var prevTarget = camera.targetTexture;
            var prevActive = RenderTexture.active;

            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;

                var texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
                texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture2D.Apply();

                byte[] pngBytes = texture2D.EncodeToPNG();
                string base64 = Convert.ToBase64String(pngBytes);

                UnityEngine.Object.DestroyImmediate(texture2D);

                return McpResponse.Success($"Captured Scene View ({width}x{height})", base64);
            }
            finally
            {
                camera.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        public static McpResponse InspectGameObjectVisual(string target)
        {
            var go = SceneHandler.FindGameObject(target);
            if (go == null)
            {
                return McpResponse.Error($"GameObject '{target}' not found.");
            }

            Selection.activeGameObject = go;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }

            return CaptureSceneView(800, 600);
        }
    }
}
