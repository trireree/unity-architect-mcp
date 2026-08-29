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
    [Serializable]
    public class SerializedFieldInfoDto
    {
        public string name;
        public string type;
        public string valueString;
    }

    public static class SerializedDataAndMetaHandler
    {
        public static McpResponse InspectSerializedProperties(string targetObject, string componentType)
        {
            var go = SceneHandler.FindGameObject(targetObject);
            if (go == null) return McpResponse.Error($"GameObject '{targetObject}' not found.");

            var comp = go.GetComponent(componentType);
            if (comp == null) return McpResponse.Error($"Component '{componentType}' not found on '{go.name}'.");

            var so = new SerializedObject(comp);
            var list = new List<SerializedFieldInfoDto>();
            var iterator = so.GetIterator();

            if (iterator.NextVisible(true))
            {
                do
                {
                    list.Add(new SerializedFieldInfoDto
                    {
                        name = iterator.name,
                        type = iterator.type,
                        valueString = GetPropertyValueAsString(iterator)
                    });
                }
                while (iterator.NextVisible(false));
            }

            return McpResponse.Success($"Inspected {list.Count} serialized fields on '{go.name}' ({componentType}).", JsonUtility.ToJson(new { fields = list }, true));
        }

        public static McpResponse ResolveGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return McpResponse.Error("GUID cannot be empty.");
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return McpResponse.Error($"GUID '{guid}' could not be resolved to an asset path.");
            return McpResponse.Success($"Resolved GUID '{guid}' -> '{path}'", path);
        }

        public static McpResponse ResolvePathToGuid(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return McpResponse.Error("Path cannot be empty.");
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid)) return McpResponse.Error($"Path '{assetPath}' has no valid GUID.");
            return McpResponse.Success($"Resolved Path '{assetPath}' -> GUID '{guid}'", guid);
        }

        private static string GetPropertyValueAsString(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer: return prop.intValue.ToString();
                case SerializedPropertyType.Boolean: return prop.boolValue.ToString();
                case SerializedPropertyType.Float: return prop.floatValue.ToString("F2");
                case SerializedPropertyType.String: return prop.stringValue;
                case SerializedPropertyType.Color: return prop.colorValue.ToString();
                case SerializedPropertyType.Vector2: return prop.vector2Value.ToString();
                case SerializedPropertyType.Vector3: return prop.vector3Value.ToString();
                case SerializedPropertyType.ObjectReference: return prop.objectReferenceValue != null ? prop.objectReferenceValue.name : "null";
                case SerializedPropertyType.Enum: return prop.enumNames.Length > prop.enumValueIndex && prop.enumValueIndex >= 0 ? prop.enumNames[prop.enumValueIndex] : prop.enumValueIndex.ToString();
                default: return prop.type;
            }
        }
    }
}
