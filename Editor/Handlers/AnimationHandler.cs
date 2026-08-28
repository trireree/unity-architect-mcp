using System;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class AnimationHandler
    {
        public static McpResponse CreateAnimatorController(string savePath, string name)
        {
            if (string.IsNullOrEmpty(savePath)) savePath = "Assets/Animations";
            if (string.IsNullOrEmpty(name)) name = "NewAnimator";
            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

            string fullPath = $"{savePath}/{name}.controller";
            var controller = AnimatorController.CreateAnimatorControllerAtPath(fullPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return McpResponse.Success($"Created AnimatorController at '{fullPath}'.", fullPath);
        }

        public static McpResponse AddState(string controllerPath, string stateName, string motionClipPath)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) return McpResponse.Error($"AnimatorController not found at '{controllerPath}'.");

            var rootStateMachine = controller.layers[0].stateMachine;
            var state = rootStateMachine.AddState(stateName);

            if (!string.IsNullOrEmpty(motionClipPath))
            {
                var clip = AssetDatabase.LoadAssetAtPath<Motion>(motionClipPath);
                if (clip != null) state.motion = clip;
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            return McpResponse.Success($"Added state '{stateName}' to AnimatorController.");
        }

        public static McpResponse AddParameter(string controllerPath, string paramName, string paramType)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) return McpResponse.Error($"AnimatorController not found at '{controllerPath}'.");

            var type = AnimatorControllerParameterType.Float;
            switch (paramType?.ToLowerInvariant())
            {
                case "int": type = AnimatorControllerParameterType.Int; break;
                case "bool": type = AnimatorControllerParameterType.Bool; break;
                case "trigger": type = AnimatorControllerParameterType.Trigger; break;
                default: type = AnimatorControllerParameterType.Float; break;
            }

            controller.AddParameter(paramName, type);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            return McpResponse.Success($"Added parameter '{paramName}' ({type}) to AnimatorController.");
        }
    }
}
