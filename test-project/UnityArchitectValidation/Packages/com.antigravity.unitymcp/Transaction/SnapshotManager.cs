using System;
using System.Collections.Generic;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Antigravity.UnityMCP.Editor.Transaction
{
    public static class SnapshotManager
    {
        private const string SnapshotsBasePath = "Library/UnityArchitect/Snapshots";

        public static bool CreateSnapshot(string transactionId, out string snapshotPath, out string error)
        {
            snapshotPath = null;
            error = null;

            try
            {
                var dir = Path.Combine(SnapshotsBasePath, transactionId);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // 1. Backup active scene
                var activeScene = SceneManager.GetActiveScene();
                if (!string.IsNullOrEmpty(activeScene.path) && File.Exists(activeScene.path))
                {
                    string sceneBackupPath = Path.Combine(dir, "SceneBackup.unity");
                    File.Copy(activeScene.path, sceneBackupPath, true);
                }

                // 2. Save metadata
                string metaPath = Path.Combine(dir, "metadata.json");
                var meta = new SnapshotMetadata
                {
                    transactionId = transactionId,
                    timestamp = DateTime.UtcNow.ToString("o"),
                    activeScenePath = activeScene.path,
                    activeSceneName = activeScene.name
                };
                File.WriteAllText(metaPath, JsonUtility.ToJson(meta, true));

                snapshotPath = dir;
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to create snapshot for '{transactionId}': {ex.Message}";
                return false;
            }
        }

        public static bool RestoreSnapshot(string transactionId, out string message, out string error)
        {
            message = null;
            error = null;

            try
            {
                var dir = Path.Combine(SnapshotsBasePath, transactionId);
                if (!Directory.Exists(dir))
                {
                    error = $"Snapshot for transaction '{transactionId}' not found at '{dir}'.";
                    return false;
                }

                string metaPath = Path.Combine(dir, "metadata.json");
                if (File.Exists(metaPath))
                {
                    var meta = JsonUtility.FromJson<SnapshotMetadata>(File.ReadAllText(metaPath));
                    string sceneBackupPath = Path.Combine(dir, "SceneBackup.unity");

                    if (File.Exists(sceneBackupPath) && !string.IsNullOrEmpty(meta.activeScenePath))
                    {
                        File.Copy(sceneBackupPath, meta.activeScenePath, true);
                        EditorSceneManager.OpenScene(meta.activeScenePath, OpenSceneMode.Single);
                    }
                }

                AssetDatabase.Refresh();
                message = $"Successfully restored snapshot for transaction '{transactionId}'.";
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to restore snapshot for '{transactionId}': {ex.Message}";
                return false;
            }
        }
    }

    [Serializable]
    public class SnapshotMetadata
    {
        public string transactionId;
        public string timestamp;
        public string activeScenePath;
        public string activeSceneName;
        public List<string> trackedAssetPaths = new List<string>();
    }
}
