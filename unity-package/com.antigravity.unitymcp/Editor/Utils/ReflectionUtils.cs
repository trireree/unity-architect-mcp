using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Utils
{
    public static class ReflectionUtils
    {
        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public static Type FindType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

            if (TypeCache.TryGetValue(typeName, out var cachedType))
            {
                return cachedType;
            }

            // Direct search
            var directType = Type.GetType(typeName);
            if (directType != null)
            {
                TypeCache[typeName] = directType;
                return directType;
            }

            // Search in loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        TypeCache[typeName] = type;
                        return type;
                    }

                    // Try matching just class name
                    var byName = assembly.GetTypes().FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                    if (byName != null)
                    {
                        TypeCache[typeName] = byName;
                        return byName;
                    }
                }
                catch
                {
                    // Ignore reflection type load errors on dynamic assemblies
                }
            }

            return null;
        }

        public static Dictionary<string, object> GetComponentProperties(Component component)
        {
            var result = new Dictionary<string, object>();
            if (component == null) return result;

            var serializedObj = new SerializedObject(component);
            var iterator = serializedObj.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false; // Don't drill deep into subproperties by default to keep clean
                if (iterator.name == "m_Script") continue;

                switch (iterator.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        result[iterator.name] = iterator.intValue;
                        break;
                    case SerializedPropertyType.Boolean:
                        result[iterator.name] = iterator.boolValue;
                        break;
                    case SerializedPropertyType.Float:
                        result[iterator.name] = iterator.floatValue;
                        break;
                    case SerializedPropertyType.String:
                        result[iterator.name] = iterator.stringValue;
                        break;
                    case SerializedPropertyType.Color:
                        result[iterator.name] = new float[] { iterator.colorValue.r, iterator.colorValue.g, iterator.colorValue.b, iterator.colorValue.a };
                        break;
                    case SerializedPropertyType.Vector2:
                        result[iterator.name] = new float[] { iterator.vector2Value.x, iterator.vector2Value.y };
                        break;
                    case SerializedPropertyType.Vector3:
                        result[iterator.name] = new float[] { iterator.vector3Value.x, iterator.vector3Value.y, iterator.vector3Value.z };
                        break;
                    case SerializedPropertyType.Vector4:
                        result[iterator.name] = new float[] { iterator.vector4Value.x, iterator.vector4Value.y, iterator.vector4Value.z, iterator.vector4Value.w };
                        break;
                    case SerializedPropertyType.Enum:
                        result[iterator.name] = iterator.enumDisplayNames.Length > iterator.enumValueIndex && iterator.enumValueIndex >= 0
                            ? iterator.enumDisplayNames[iterator.enumValueIndex]
                            : iterator.intValue.ToString();
                        break;
                    case SerializedPropertyType.ObjectReference:
                        result[iterator.name] = iterator.objectReferenceValue != null
                            ? $"{iterator.objectReferenceValue.name} ({iterator.objectReferenceValue.GetType().Name})"
                            : null;
                        break;
                    default:
                        result[iterator.name] = iterator.propertyType.ToString();
                        break;
                }
            }

            return result;
        }

        public static bool SetSerializedPropertyValue(SerializedProperty prop, string valueStr)
        {
            if (prop == null) return false;

            try
            {
                switch (prop.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        if (int.TryParse(valueStr, out int intVal)) { prop.intValue = intVal; return true; }
                        break;
                    case SerializedPropertyType.Boolean:
                        if (bool.TryParse(valueStr, out bool boolVal)) { prop.boolValue = boolVal; return true; }
                        break;
                    case SerializedPropertyType.Float:
                        if (float.TryParse(valueStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float floatVal))
                        {
                            prop.floatValue = floatVal;
                            return true;
                        }
                        break;
                    case SerializedPropertyType.String:
                        prop.stringValue = valueStr;
                        return true;
                    case SerializedPropertyType.Enum:
                        var names = prop.enumNames;
                        int idx = Array.IndexOf(names, valueStr);
                        if (idx >= 0) { prop.enumValueIndex = idx; return true; }
                        if (int.TryParse(valueStr, out int enumInt)) { prop.enumValueIndex = enumInt; return true; }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UnityMCP] Error setting property {prop.name}: {ex.Message}");
            }
            return false;
        }
    }
}
