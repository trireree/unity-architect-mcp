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
    public class ScriptableObjectDataDto
    {
        public string assetPath;
        public string typeName;
        public List<SerializedFieldInfoDto> properties = new List<SerializedFieldInfoDto>();
    }

    public static class ScriptableObjectAndInspectorHandler
    {
        public static McpResponse CreateScriptableObject(string scriptableClassName, string savePath)
        {
            if (string.IsNullOrEmpty(scriptableClassName)) return McpResponse.Error("ClassName cannot be empty.");
            if (string.IsNullOrEmpty(savePath)) savePath = $"Assets/Data/{scriptableClassName}.asset";
            if (!savePath.StartsWith("Assets/")) savePath = "Assets/" + savePath.TrimStart('/');
            if (!savePath.EndsWith(".asset")) savePath += ".asset";

            var dir = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var allTypes = TypeCache.GetTypesDerivedFrom<ScriptableObject>();
            var targetType = allTypes.FirstOrDefault(t => t.Name == scriptableClassName);

            if (targetType == null)
            {
                return McpResponse.Error($"ScriptableObject class '{scriptableClassName}' not found in project assemblies.");
            }

            var soInstance = ScriptableObject.CreateInstance(targetType);
            AssetDatabase.CreateAsset(soInstance, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return McpResponse.Success($"Created ScriptableObject '{scriptableClassName}' at '{savePath}'.", savePath);
        }

        public static McpResponse ReadScriptableObjectData(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/")) assetPath = "Assets/" + assetPath.TrimStart('/');
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null) return McpResponse.Error($"ScriptableObject not found at '{assetPath}'.");

            var so = new SerializedObject(asset);
            var dto = new ScriptableObjectDataDto
            {
                assetPath = assetPath,
                typeName = asset.GetType().Name
            };

            var iterator = so.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    dto.properties.Add(new SerializedFieldInfoDto
                    {
                        name = iterator.name,
                        type = iterator.type,
                        valueString = iterator.propertyType == SerializedPropertyType.String ? iterator.stringValue : iterator.type
                    });
                }
                while (iterator.NextVisible(false));
            }

            return McpResponse.Success($"Read ScriptableObject data for '{asset.name}'.", JsonUtility.ToJson(dto, true));
        }

        public static McpResponse SetScriptableObjectProperty(string assetPath, string propertyName, string valueJson)
        {
            if (!assetPath.StartsWith("Assets/")) assetPath = "Assets/" + assetPath.TrimStart('/');
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (asset == null) return McpResponse.Error($"ScriptableObject not found at '{assetPath}'.");

            var so = new SerializedObject(asset);
            var prop = so.FindProperty(propertyName);
            if (prop == null) return McpResponse.Error($"Property '{propertyName}' not found on '{asset.name}'.");

            Undo.RecordObject(asset, $"Set ScriptableObject property {propertyName}");
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    if (int.TryParse(valueJson, out int i)) prop.intValue = i;
                    break;
                case SerializedPropertyType.Float:
                    if (float.TryParse(valueJson, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float f)) prop.floatValue = f;
                    break;
                case SerializedPropertyType.Boolean:
                    if (bool.TryParse(valueJson, out bool b)) prop.boolValue = b;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = valueJson.Trim('"');
                    break;
                default:
                    return McpResponse.Error($"Unsupported property type: {prop.propertyType}");
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            return McpResponse.Success($"Updated property '{propertyName}' on ScriptableObject '{asset.name}'.");
        }
    }
}
