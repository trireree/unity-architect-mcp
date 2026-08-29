#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class ComponentReflectionHandler
    {
        public static McpResponse SetSerializedField(string targetObject, string componentType, string fieldName, string valueJson)
        {
            var go = SceneHandler.FindGameObject(targetObject);
            if (go == null) return McpResponse.Error($"GameObject '{targetObject}' not found.");

            var comp = go.GetComponent(componentType);
            if (comp == null) return McpResponse.Error($"Component '{componentType}' not found on '{go.name}'.");

            var so = new SerializedObject(comp);
            var prop = so.FindProperty(fieldName);

            if (prop == null)
            {
                // Fallback to C# reflection if property not exposed to serializer
                var field = comp.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    try
                    {
                        object converted = ConvertValue(valueJson, field.FieldType);
                        Undo.RecordObject(comp, $"Set Field {fieldName} via MCP");
                        field.SetValue(comp, converted);
                        EditorUtility.SetDirty(comp);
                        return McpResponse.Success($"Set field '{fieldName}' to '{valueJson}' via Reflection.");
                    }
                    catch (Exception ex)
                    {
                        return McpResponse.Error($"Failed to set field '{fieldName}': {ex.Message}");
                    }
                }
                return McpResponse.Error($"Property or Field '{fieldName}' not found on '{componentType}'.");
            }

            Undo.RecordObject(comp, $"Set SerializedProperty {fieldName} via MCP");
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    if (int.TryParse(valueJson, out int iVal)) prop.intValue = iVal;
                    break;
                case SerializedPropertyType.Boolean:
                    if (bool.TryParse(valueJson, out bool bVal)) prop.boolValue = bVal;
                    break;
                case SerializedPropertyType.Float:
                    if (float.TryParse(valueJson, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fVal)) prop.floatValue = fVal;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = valueJson.Trim('"');
                    break;
                case SerializedPropertyType.Color:
                    if (ColorUtility.TryParseHtmlString(valueJson.Trim('"'), out Color cVal)) prop.colorValue = cVal;
                    break;
                case SerializedPropertyType.Vector2:
                    var v2 = JsonUtility.FromJson<Vector2>(valueJson);
                    prop.vector2Value = v2;
                    break;
                case SerializedPropertyType.Vector3:
                    var v3 = JsonUtility.FromJson<Vector3>(valueJson);
                    prop.vector3Value = v3;
                    break;
                case SerializedPropertyType.ObjectReference:
                    var refGo = SceneHandler.FindGameObject(valueJson.Trim('"'));
                    if (refGo != null)
                    {
                        if (prop.type.Contains("GameObject")) prop.objectReferenceValue = refGo;
                        else prop.objectReferenceValue = refGo.GetComponent(prop.type.Replace("PPtr<$", "").Replace(">", ""));
                    }
                    break;
                default:
                    return McpResponse.Error($"Unsupported SerializedProperty type: {prop.propertyType}");
            }

            so.ApplyModifiedProperties();
            return McpResponse.Success($"Set SerializedProperty '{fieldName}' on '{go.name}' ({componentType}).");
        }

        public static McpResponse InvokeMethod(string targetObject, string componentType, string methodName, string[] args = null)
        {
            var go = SceneHandler.FindGameObject(targetObject);
            if (go == null) return McpResponse.Error($"GameObject '{targetObject}' not found.");

            var comp = go.GetComponent(componentType);
            if (comp == null) return McpResponse.Error($"Component '{componentType}' not found on '{go.name}'.");

            var method = comp.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) return McpResponse.Error($"Method '{methodName}' not found on '{componentType}'.");

            var paramInfos = method.GetParameters();
            object[] convertedArgs = new object[paramInfos.Length];

            for (int i = 0; i < paramInfos.Length; i++)
            {
                if (args != null && i < args.Length)
                {
                    convertedArgs[i] = ConvertValue(args[i], paramInfos[i].ParameterType);
                }
                else
                {
                    convertedArgs[i] = paramInfos[i].DefaultValue != DBNull.Value ? paramInfos[i].DefaultValue : null;
                }
            }

            object result = method.Invoke(comp, convertedArgs);
            return McpResponse.Success($"Invoked '{methodName}' on '{go.name}'. Result: {result ?? "void"}", result?.ToString());
        }

        private static object ConvertValue(string raw, Type targetType)
        {
            if (raw == null) return null;
            raw = raw.Trim('"');

            if (targetType == typeof(int)) return int.Parse(raw);
            if (targetType == typeof(float)) return float.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
            if (targetType == typeof(bool)) return bool.Parse(raw);
            if (targetType == typeof(string)) return raw;
            if (targetType.IsEnum) return Enum.Parse(targetType, raw, true);
            if (targetType == typeof(Vector3)) return JsonUtility.FromJson<Vector3>(raw);
            if (targetType == typeof(Color) && ColorUtility.TryParseHtmlString(raw, out Color c)) return c;

            return Convert.ChangeType(raw, targetType);
        }
    }
}
