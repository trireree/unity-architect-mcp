using System;
using System.Collections.Generic;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class AssetHandler
    {
        public static McpResponse CreatePrefab(string targetGameObject, string savePath)
        {
            var go = SceneHandler.FindGameObject(targetGameObject);
            if (go == null) return McpResponse.Error($"GameObject '{targetGameObject}' not found.");

            if (string.IsNullOrEmpty(savePath))
            {
                savePath = $"Assets/{go.name}.prefab";
            }
            if (!savePath.EndsWith(".prefab")) savePath += ".prefab";

            var dir = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, savePath, InteractionMode.UserAction);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return McpResponse.Success($"Saved prefab at '{savePath}'.", savePath);
        }

        public static McpResponse InstantiatePrefab(string prefabPath, float[] position, float[] rotation, string parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                var guids = AssetDatabase.FindAssets($"{prefabPath} t:Prefab");
                if (guids.Length > 0)
                {
                    var foundPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(foundPath);
                }
            }

            if (prefab == null) return McpResponse.Error($"Prefab not found at '{prefabPath}'.");

            Transform parentTransform = null;
            if (!string.IsNullOrEmpty(parent))
            {
                var parentGo = SceneHandler.FindGameObject(parent);
                if (parentGo != null) parentTransform = parentGo.transform;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parentTransform);
            Undo.RegisterCreatedObjectUndo(instance, $"Instantiate {prefab.name}");

            if (position != null && position.Length == 3)
            {
                instance.transform.position = new Vector3(position[0], position[1], position[2]);
            }
            if (rotation != null && rotation.Length == 3)
            {
                instance.transform.eulerAngles = new Vector3(rotation[0], rotation[1], rotation[2]);
            }

            Selection.activeGameObject = instance;
            string idStr = EntityIdHelper.GetIdString(instance);
            return McpResponse.Success($"Instantiated prefab '{instance.name}' (ID: {idStr})", idStr);
        }

        public static McpResponse CreateMaterial(string materialName, string shaderName, string colorHex, string saveFolder)
        {
            if (string.IsNullOrEmpty(materialName)) materialName = "NewMaterial";
            if (string.IsNullOrEmpty(saveFolder)) saveFolder = "Assets/Materials";
            if (string.IsNullOrEmpty(shaderName)) shaderName = "Universal Render Pipeline/Lit";

            if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);

            var shader = Shader.Find(shaderName) ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
            var mat = new Material(shader);

            if (!string.IsNullOrEmpty(colorHex) && ColorUtility.TryParseHtmlString(colorHex, out var color))
            {
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            }

            string assetPath = $"{saveFolder}/{materialName}.mat";
            AssetDatabase.CreateAsset(mat, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return McpResponse.Success($"Created Material at '{assetPath}'.", assetPath);
        }

        public static McpResponse FindAssets(string filter, string searchFolder)
        {
            string[] searchInFolders = string.IsNullOrEmpty(searchFolder) ? null : new string[] { searchFolder };
            var guids = AssetDatabase.FindAssets(filter ?? "", searchInFolders);
            var results = new List<string>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var type = AssetDatabase.GetMainAssetTypeAtPath(path)?.Name ?? "Asset";
                results.Add($"{path} [{type}]");
            }

            return McpResponse.Success($"Found {results.Count} assets.", string.Join("\n", results));
        }
    }
}
