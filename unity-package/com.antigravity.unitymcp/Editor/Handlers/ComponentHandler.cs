using System;
using System.Collections.Generic;
using System.Reflection;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class ComponentHandler
    {
        public static McpResponse AddComponent(string target, string componentType)
        {
            var go = SceneHandler.FindGameObject(target);
            if (go == null) return McpResponse.Error($"GameObject '{target}' not found.");

            var type = ReflectionUtils.FindType(componentType);
            if (type == null) return McpResponse.Error($"Component type '{componentType}' not found in loaded assemblies.");

            if (!typeof(Component).IsAssignableFrom(type))
            {
                return McpResponse.Error($"Type '{componentType}' is not a UnityEngine.Component.");
            }

            var comp = Undo.AddComponent(go, type);
            EditorUtility.SetDirty(go);

            return McpResponse.Success($"Added component '{type.Name}' to '{go.name}'.");
        }

        public static McpResponse RemoveComponent(string target, string componentType)
        {
            var go = SceneHandler.FindGameObject(target);
            if (go == null) return McpResponse.Error($"GameObject '{target}' not found.");

            var type = ReflectionUtils.FindType(componentType);
            if (type == null) return McpResponse.Error($"Component type '{componentType}' not found.");

            var comp = go.GetComponent(type);
            if (comp == null) return McpResponse.Error($"GameObject '{go.name}' does not have component '{type.Name}'.");

            Undo.DestroyObjectImmediate(comp);
            EditorUtility.SetDirty(go);

            return McpResponse.Success($"Removed component '{type.Name}' from '{go.name}'.");
        }

        public static McpResponse GetComponentProperties(string target, string componentType)
        {
            var go = SceneHandler.FindGameObject(target);
            if (go == null) return McpResponse.Error($"GameObject '{target}' not found.");

            var type = ReflectionUtils.FindType(componentType);
            if (type == null) return McpResponse.Error($"Component type '{componentType}' not found.");

            var comp = go.GetComponent(type);
            if (comp == null) return McpResponse.Error($"GameObject '{go.name}' does not have component '{type.Name}'.");

            var props = ReflectionUtils.GetComponentProperties(comp);
            var serializedObj = new SerializedObject(comp);

            var list = new List<string>();
            foreach (var kvp in props)
            {
                list.Add($"\"{kvp.Key}\": {JsonUtility.ToJson(kvp.Value?.ToString() ?? "null")}");
            }

            string json = "{" + string.Join(",", list) + "}";
            return McpResponse.Success($"Retrieved properties for '{type.Name}' on '{go.name}'.", json);
        }

        public static McpResponse SetComponentProperty(string target, string componentType, string propertyName, string value)
        {
            var go = SceneHandler.FindGameObject(target);
            if (go == null) return McpResponse.Error($"GameObject '{target}' not found.");

            var type = ReflectionUtils.FindType(componentType);
            if (type == null) return McpResponse.Error($"Component type '{componentType}' not found.");

            var comp = go.GetComponent(type);
            if (comp == null) return McpResponse.Error($"GameObject '{go.name}' does not have component '{type.Name}'.");

            var so = new SerializedObject(comp);
            var prop = so.FindProperty(propertyName);

            if (prop != null)
            {
                Undo.RecordObject(comp, $"Set {propertyName} on {type.Name}");
                if (ReflectionUtils.SetSerializedPropertyValue(prop, value))
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(comp);
                    return McpResponse.Success($"Successfully set SerializedProperty '{propertyName}' to '{value}'.");
                }
            }

            // Fallback to Reflection Field/Property
            var field = type.GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                Undo.RecordObject(comp, $"Set Field {propertyName}");
                try
                {
                    var converted = Convert.ChangeType(value, field.FieldType);
                    field.SetValue(comp, converted);
                    EditorUtility.SetDirty(comp);
                    return McpResponse.Success($"Successfully set field '{propertyName}' to '{value}'.");
                }
                catch (Exception ex)
                {
                    return McpResponse.Error($"Failed to set field '{propertyName}': {ex.Message}");
                }
            }

            return McpResponse.Error($"Property or field '{propertyName}' not found on '{type.Name}'.");
        }
    }
}
