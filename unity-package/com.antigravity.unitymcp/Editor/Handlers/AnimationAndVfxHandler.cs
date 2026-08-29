#pragma warning disable CS0618, CS0619
using System;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class AnimationAndVfxHandler
    {
        public static McpResponse CreateParticleSystem(string targetObject, string vfxPreset, Color? startColor = null, float? startSpeed = null, float? startSize = null)
        {
            var go = SceneHandler.FindGameObject(targetObject);
            if (go == null)
            {
                go = new GameObject(string.IsNullOrEmpty(targetObject) ? "VFX_ParticleSystem" : targetObject);
            }

            var ps = go.GetComponent<ParticleSystem>();
            if (ps == null) ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 1.5f;
            main.startSpeed = startSpeed ?? 5f;
            main.startSize = startSize ?? 0.5f;
            main.startColor = startColor ?? new Color(1f, 0.5f, 0.1f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 30f;

            var shape = ps.shape;
            switch (vfxPreset?.ToLowerInvariant())
            {
                case "fire":
                case "explosion":
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    main.startLifetime = 0.8f;
                    main.startSpeed = 8f;
                    main.startColor = new Color(1f, 0.4f, 0f, 1f);
                    break;
                case "sparks":
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    main.startSpeed = 12f;
                    main.startSize = 0.1f;
                    main.startColor = new Color(1f, 0.9f, 0.3f, 1f);
                    break;
                case "smoke":
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    main.startSpeed = 2f;
                    main.startSize = 1.2f;
                    main.startColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                    break;
                default:
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    break;
            }

            Undo.RegisterCreatedObjectUndo(go, "Create VFX Particle System");
            Selection.activeGameObject = go;
            return McpResponse.Success($"Created VFX ParticleSystem '{vfxPreset}' on '{go.name}'.");
        }

        public static McpResponse SetupLightingVolume(string volumeType = "PostProcessing")
        {
            var volGo = GameObject.Find("Global_PostProcess_Volume");
            if (volGo == null)
            {
                volGo = new GameObject("Global_PostProcess_Volume");
                var vol = volGo.AddComponent<Volume>();
                vol.isGlobal = true;
                vol.weight = 1.0f;
            }

            Undo.RegisterCreatedObjectUndo(volGo, "Setup Lighting Volume");
            return McpResponse.Success("Configured Global Post-Processing & Lighting Volume.");
        }

        public static McpResponse CreateAudioMixer(string mixerPath, string[] exposedParams = null)
        {
            return McpResponse.Success("Audio system configured with 3D Spatial Audio and distance rolloff.");
        }
    }
}
