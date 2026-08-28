using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Core
{
    public static class EntityIdHelper
    {
        private static readonly MethodInfo GetEntityIdMethod = typeof(UnityEngine.Object).GetMethod("GetEntityId", BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo GetInstanceIdMethod = typeof(UnityEngine.Object).GetMethod("GetInstanceID", BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo EntityIdToObjectMethod = typeof(EditorUtility).GetMethod("EntityIdToObject", BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo InstanceIdToObjectMethod = typeof(EditorUtility).GetMethod("InstanceIDToObject", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(int) }, null);

        public static string GetIdString(UnityEngine.Object obj)
        {
            if (obj == null) return "0";

            if (GetEntityIdMethod != null)
            {
                try
                {
                    var res = GetEntityIdMethod.Invoke(obj, null);
                    if (res != null) return res.ToString();
                }
                catch { }
            }

            if (GetInstanceIdMethod != null)
            {
                try
                {
                    var res = GetInstanceIdMethod.Invoke(obj, null);
                    if (res != null) return res.ToString();
                }
                catch { }
            }

            return obj.GetHashCode().ToString();
        }

        public static UnityEngine.Object FindObjectById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (ulong.TryParse(id, out ulong uId) && EntityIdToObjectMethod != null)
            {
                try
                {
                    var res = EntityIdToObjectMethod.Invoke(null, new object[] { uId }) as UnityEngine.Object;
                    if (res != null) return res;
                }
                catch { }
            }

            if (int.TryParse(id, out int intId) && InstanceIdToObjectMethod != null)
            {
                try
                {
                    var res = InstanceIdToObjectMethod.Invoke(null, new object[] { intId }) as UnityEngine.Object;
                    if (res != null) return res;
                }
                catch { }
            }

            return null;
        }
    }
}
