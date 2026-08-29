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
    public static class SceneAndTransformHandler
    {
        public static McpResponse CreateGameObject(string name, string primitiveType = null, Vector3? position = null, Vector3? rotation = null, Vector3? scale = null, string parent = null)
        {
            GameObject go;
            if (!string.IsNullOrEmpty(primitiveType) && Enum.TryParse<PrimitiveType>(primitiveType, true, out var pType))
            {
                go = GameObject.CreatePrimitive(pType);
                go.name = string.IsNullOrEmpty(name) ? pType.ToString() : name;
            }
            else
            {
                go = new GameObject(string.IsNullOrEmpty(name) ? "GameObject" : name);
            }

            if (position.HasValue) go.transform.position = position.Value;
            if (rotation.HasValue) go.transform.rotation = Quaternion.Euler(rotation.Value);
            if (scale.HasValue) go.transform.localScale = scale.Value;

            if (!string.IsNullOrEmpty(parent))
            {
                var parentGo = SceneHandler.FindGameObject(parent);
                if (parentGo != null) go.transform.SetParent(parentGo.transform, false);
            }

            Undo.RegisterCreatedObjectUndo(go, "Create GameObject via MCP");
            Selection.activeGameObject = go;
            string id = EntityIdHelper.GetIdString(go);
            return McpResponse.Success($"Created GameObject '{go.name}' (ID: {id})", id);
        }

        public static McpResponse ModifyTransform(string target, Vector3? position, Vector3? rotation, Vector3? scale, Vector3? localPosition, Vector3? localRotation, Vector3? localScale, string parent, int? siblingIndex)
        {
            var go = SceneHandler.FindGameObject(target);
            if (go == null) return McpResponse.Error($"Target GameObject '{target}' not found.");

            Undo.RecordObject(go.transform, "Modify Transform via MCP");

            if (position.HasValue) go.transform.position = position.Value;
            if (rotation.HasValue) go.transform.rotation = Quaternion.Euler(rotation.Value);
            if (scale.HasValue) go.transform.localScale = scale.Value;

            if (localPosition.HasValue) go.transform.localPosition = localPosition.Value;
            if (localRotation.HasValue) go.transform.localRotation = Quaternion.Euler(localRotation.Value);
            if (localScale.HasValue) go.transform.localScale = localScale.Value;

            if (!string.IsNullOrEmpty(parent))
            {
                if (parent.Equals("null", StringComparison.OrdinalIgnoreCase) || parent.Equals("root", StringComparison.OrdinalIgnoreCase))
                {
                    go.transform.SetParent(null, true);
                }
                else
                {
                    var parentGo = SceneHandler.FindGameObject(parent);
                    if (parentGo != null) go.transform.SetParent(parentGo.transform, true);
                }
            }

            if (siblingIndex.HasValue)
            {
                go.transform.SetSiblingIndex(siblingIndex.Value);
            }

            return McpResponse.Success($"Updated transform of '{go.name}' successfully.");
        }

        public static McpResponse InstantiatePrefab(string assetPath, Vector3? position, Vector3? rotation, string parent, string name)
        {
            if (string.IsNullOrEmpty(assetPath)) return McpResponse.Error("Prefab asset path cannot be empty.");
            if (!assetPath.StartsWith("Assets/")) assetPath = "Assets/" + assetPath.TrimStart('/');

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) return McpResponse.Error($"Prefab asset not found at '{assetPath}'.");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (!string.IsNullOrEmpty(name)) instance.name = name;

            if (position.HasValue) instance.transform.position = position.Value;
            if (rotation.HasValue) instance.transform.rotation = Quaternion.Euler(rotation.Value);

            if (!string.IsNullOrEmpty(parent))
            {
                var parentGo = SceneHandler.FindGameObject(parent);
                if (parentGo != null) instance.transform.SetParent(parentGo.transform, false);
            }

            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab via MCP");
            Selection.activeGameObject = instance;
            string id = EntityIdHelper.GetIdString(instance);
            return McpResponse.Success($"Instantiated prefab '{prefab.name}' as '{instance.name}' (ID: {id})", id);
        }

        public static McpResponse SaveAsPrefab(string targetObject, string savePath, bool asVariant = false)
        {
            var go = SceneHandler.FindGameObject(targetObject);
            if (go == null) return McpResponse.Error($"Target GameObject '{targetObject}' not found.");

            if (!savePath.StartsWith("Assets/")) savePath = "Assets/" + savePath.TrimStart('/');
            if (!savePath.EndsWith(".prefab")) savePath += ".prefab";

            var dir = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            GameObject savedPrefab;
            if (asVariant)
            {
                savedPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, savePath, InteractionMode.UserAction);
            }
            else
            {
                savedPrefab = PrefabUtility.SaveAsPrefabAsset(go, savePath);
            }

            AssetDatabase.Refresh();
            return McpResponse.Success($"Saved '{go.name}' as prefab to '{savePath}'.", savePath);
        }

        public static McpResponse ModifyMaterial(string targetObject, string materialName, string colorHex, float? smoothness, float? metallic, string shaderName)
        {
            var go = SceneHandler.FindGameObject(targetObject);
            if (go == null) return McpResponse.Error($"Target GameObject '{targetObject}' not found.");

            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return McpResponse.Error($"GameObject '{go.name}' does not have a Renderer component.");

            Undo.RecordObject(renderer, "Modify Material via MCP");

            Material mat = null;
            if (!string.IsNullOrEmpty(materialName))
            {
                string[] guids = AssetDatabase.FindAssets($"t:Material {materialName}");
                if (guids.Length > 0)
                {
                    string p = AssetDatabase.GUIDToAssetPath(guids[0]);
                    mat = AssetDatabase.LoadAssetAtPath<Material>(p);
                    renderer.sharedMaterial = mat;
                }
            }

            mat = renderer.sharedMaterial;
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit"));
                renderer.sharedMaterial = mat;
            }

            if (!string.IsNullOrEmpty(shaderName))
            {
                var s = Shader.Find(shaderName);
                if (s != null) mat.shader = s;
            }

            if (!string.IsNullOrEmpty(colorHex) && ColorUtility.TryParseHtmlString(colorHex, out Color c))
            {
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            }

            if (smoothness.HasValue && mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness.Value);
            if (metallic.HasValue && mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic.Value);

            EditorUtility.SetDirty(mat);
            return McpResponse.Success($"Updated material on '{go.name}' (Material: {mat.name}, Shader: {mat.shader.name}).");
        }
    }
}
