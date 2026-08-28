using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Antigravity.UnityMCP.Editor.State
{
    public static class StateHasher
    {
        public static string ComputeSha256(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "empty";
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                var sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string ComputeHash(string raw)
        {
            return ComputeSha256(raw);
        }

        public static string ComputeFileHash(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return "missing_file";
            try
            {
                using (var sha = SHA256.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    var bytes = sha.ComputeHash(stream);
                    var sb = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        sb.Append(bytes[i].ToString("x2"));
                    }
                    return sb.ToString().Substring(0, 16);
                }
            }
            catch
            {
                return "read_error";
            }
        }

        public static string ComputeGameObjectHash(GameObject go)
        {
            if (go == null) return "";

            var sb = new StringBuilder();
            sb.Append(go.name).Append("|");
            sb.Append(go.tag).Append("|");
            sb.Append(go.layer).Append("|");
            sb.Append(go.activeSelf).Append("|");
            sb.Append(go.transform.localPosition.ToString("F3")).Append("|");
            sb.Append(go.transform.localEulerAngles.ToString("F3")).Append("|");
            sb.Append(go.transform.localScale.ToString("F3")).Append("|");

            var comps = go.GetComponents<Component>();
            foreach (var comp in comps)
            {
                if (comp != null)
                {
                    sb.Append(comp.GetType().Name).Append(";");
                }
                else
                {
                    sb.Append("MissingScript;");
                }
            }

            return ComputeSha256(sb.ToString()).Substring(0, 16);
        }

        public static string ComputeSceneHash(Scene scene)
        {
            if (!scene.IsValid()) return "invalid_scene";

            var sb = new StringBuilder();
            sb.Append(scene.name).Append("|").Append(scene.path).Append("|");

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                AppendGameObjectRecursive(sb, root);
            }

            return ComputeSha256(sb.ToString()).Substring(0, 24);
        }

        private static void AppendGameObjectRecursive(StringBuilder sb, GameObject go)
        {
            if (go == null) return;
            sb.Append(ComputeGameObjectHash(go)).Append(";");
            for (int i = 0; i < go.transform.childCount; i++)
            {
                AppendGameObjectRecursive(sb, go.transform.GetChild(i).gameObject);
            }
        }
    }
}
