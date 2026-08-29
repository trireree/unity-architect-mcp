using System;
using System.Collections.Generic;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class SceneHandler
    {
        public static string GetHierarchy()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            var list = new List<HierarchyNode>();

            foreach (var root in roots)
            {
                list.Add(BuildNode(root));
            }

            return JsonUtility.ToJson(new HierarchyWrapper { nodes = list }, true);
        }

        private static HierarchyNode BuildNode(GameObject go)
        {
            var node = new HierarchyNode
            {
                stableId = EntityIdHelper.GetIdString(go),
                name = go.name,
                tag = go.tag,
                layer = LayerMask.LayerToName(go.layer),
                activeSelf = go.activeSelf,
                activeInHierarchy = go.activeInHierarchy,
                position = new float[] { go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z },
                rotation = new float[] { go.transform.localEulerAngles.x, go.transform.localEulerAngles.y, go.transform.localEulerAngles.z },
                scale = new float[] { go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z }
            };

            foreach (var comp in go.GetComponents<Component>())
            {
                if (comp != null)
                {
                    node.components.Add(comp.GetType().Name);
                }
            }

            for (int i = 0; i < go.transform.childCount; i++)
            {
                node.children.Add(BuildNode(go.transform.GetChild(i).gameObject));
            }

            return node;
        }

        public static GameObject FindGameObject(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) return null;

            var obj = EntityIdHelper.FindObjectById(identifier) as GameObject;
            if (obj != null) return obj;

            var byName = GameObject.Find(identifier);
            if (byName != null) return byName;

            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            return all.FirstOrDefault(g => g.name.Equals(identifier, StringComparison.OrdinalIgnoreCase) && !EditorUtility.IsPersistent(g));
        }

        public static McpResponse CreateGameObject(string name, string primitiveType, float[] position, float[] rotation, float[] scale, string parent)
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

            Undo.RegisterCreatedObjectUndo(go, $"Create {go.name}");

            if (!string.IsNullOrEmpty(parent))
            {
                var parentGo = FindGameObject(parent);
                if (parentGo != null)
                {
                    go.transform.SetParent(parentGo.transform, false);
                }
            }

            if (position != null && position.Length == 3)
            {
                go.transform.localPosition = new Vector3(position[0], position[1], position[2]);
            }
            if (rotation != null && rotation.Length == 3)
            {
                go.transform.localEulerAngles = new Vector3(rotation[0], rotation[1], rotation[2]);
            }
            if (scale != null && scale.Length == 3)
            {
                go.transform.localScale = new Vector3(scale[0], scale[1], scale[2]);
            }

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            string idStr = EntityIdHelper.GetIdString(go);
            return McpResponse.Success($"Created GameObject '{go.name}' (ID: {idStr})", idStr);
        }

        public static McpResponse ModifyGameObject(string identifier, string newName, float[] position, float[] rotation, float[] scale, string tag, string layer, bool? active)
        {
            var go = FindGameObject(identifier);
            if (go == null) return McpResponse.Error($"GameObject '{identifier}' not found.");

            Undo.RecordObject(go, "Modify GameObject");
            Undo.RecordObject(go.transform, "Modify Transform");

            if (!string.IsNullOrEmpty(newName)) go.name = newName;
            if (position != null && position.Length == 3) go.transform.localPosition = new Vector3(position[0], position[1], position[2]);
            if (rotation != null && rotation.Length == 3) go.transform.localEulerAngles = new Vector3(rotation[0], rotation[1], rotation[2]);
            if (scale != null && scale.Length == 3) go.transform.localScale = new Vector3(scale[0], scale[1], scale[2]);
            if (!string.IsNullOrEmpty(tag)) go.tag = tag;
            if (!string.IsNullOrEmpty(layer))
            {
                int l = LayerMask.NameToLayer(layer);
                if (l >= 0) go.layer = l;
            }
            if (active.HasValue) go.SetActive(active.Value);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return McpResponse.Success($"Modified GameObject '{go.name}'");
        }

        public static McpResponse DeleteGameObject(string identifier)
        {
            var go = FindGameObject(identifier);
            if (go == null) return McpResponse.Error($"GameObject '{identifier}' not found.");

            string name = go.name;
            Undo.DestroyObjectImmediate(go);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return McpResponse.Success($"Deleted GameObject '{name}'");
        }

        public static McpResponse DuplicateGameObject(string identifier)
        {
            var go = FindGameObject(identifier);
            if (go == null) return McpResponse.Error($"GameObject '{identifier}' not found.");

            var clone = GameObject.Instantiate(go, go.transform.parent);
            clone.name = go.name + "_Copy";
            Undo.RegisterCreatedObjectUndo(clone, "Duplicate GameObject");
            Selection.activeGameObject = clone;

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            string idStr = EntityIdHelper.GetIdString(clone);
            return McpResponse.Success($"Duplicated '{go.name}' as '{clone.name}' (ID: {idStr})", idStr);
        }
    }

    [Serializable]
    public class HierarchyWrapper
    {
        public List<HierarchyNode> nodes;
    }
}
