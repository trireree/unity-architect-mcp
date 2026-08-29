#pragma warning disable CS0618, CS0619
using System;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class TechArtAndAnimationHandler
    {
        public static McpResponse CreateAnimatorControllerWithStates(string savePath, string controllerName, string[] stateNames, string[] parameters)
        {
            if (string.IsNullOrEmpty(savePath)) savePath = "Assets/Animations";
            if (!savePath.StartsWith("Assets/")) savePath = "Assets/" + savePath.TrimStart('/');

            var dir = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string fullPath = Path.Combine(savePath, $"{controllerName}.controller").Replace("\\", "/");
            var controller = AnimatorController.CreateAnimatorControllerAtPath(fullPath);

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    controller.AddParameter(p, AnimatorControllerParameterType.Float);
                }
            }

            var rootSm = controller.layers[0].stateMachine;
            if (stateNames != null && stateNames.Length > 0)
            {
                AnimatorState defaultState = null;
                for (int i = 0; i < stateNames.Length; i++)
                {
                    var state = rootSm.AddState(stateNames[i]);
                    if (i == 0) defaultState = state;
                }
                if (defaultState != null) rootSm.defaultState = defaultState;
            }

            AssetDatabase.SaveAssets();
            return McpResponse.Success($"Created AnimatorController '{controllerName}' at '{fullPath}' with {stateNames?.Length ?? 0} state(s).", fullPath);
        }

        public static McpResponse ConfigureMaterialShaderKeywords(string materialPath, string[] enableKeywords, string[] disableKeywords)
        {
            if (!materialPath.StartsWith("Assets/")) materialPath = "Assets/" + materialPath.TrimStart('/');
            var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (mat == null) return McpResponse.Error($"Material not found at '{materialPath}'.");

            Undo.RecordObject(mat, "Configure Shader Keywords via MCP");
            if (enableKeywords != null)
            {
                foreach (var k in enableKeywords) mat.EnableKeyword(k);
            }
            if (disableKeywords != null)
            {
                foreach (var k in disableKeywords) mat.DisableKeyword(k);
            }

            EditorUtility.SetDirty(mat);
            return McpResponse.Success($"Updated shader keywords on material '{mat.name}'.");
        }
    }
}
