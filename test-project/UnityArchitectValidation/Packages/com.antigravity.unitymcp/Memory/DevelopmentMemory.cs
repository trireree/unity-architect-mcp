using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Memory
{
    [Serializable]
    public class JournalEntry
    {
        public string timestamp;
        public string action;
        public string summary;
        public string transactionId;
        public List<string> affectedObjects = new List<string>();
    }

    [Serializable]
    public class MemoryStore
    {
        public List<JournalEntry> entries = new List<JournalEntry>();
    }

    public static class DevelopmentMemory
    {
        private const string MemoryFilePath = "Library/UnityArchitect/development_memory.json";
        private static MemoryStore _store;

        public static void RecordAction(string action, string summary, string txId = null, List<string> affectedObjects = null)
        {
            EnsureLoaded();
            var entry = new JournalEntry
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                action = action,
                summary = summary,
                transactionId = txId,
                affectedObjects = affectedObjects ?? new List<string>()
            };

            _store.entries.Add(entry);
            if (_store.entries.Count > 100) _store.entries.RemoveAt(0);

            Save();
        }

        public static List<JournalEntry> QueryRecentHistory(int count = 20, string query = null)
        {
            EnsureLoaded();
            var list = _store.entries.AsEnumerable();

            if (!string.IsNullOrEmpty(query))
            {
                list = list.Where(e =>
                    (e.action != null && e.action.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (e.summary != null && e.summary.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    e.affectedObjects.Any(o => o.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                );
            }

            return list.TakeLast(count).ToList();
        }

        private static void EnsureLoaded()
        {
            if (_store != null) return;

            try
            {
                if (File.Exists(MemoryFilePath))
                {
                    string json = File.ReadAllText(MemoryFilePath);
                    _store = JsonUtility.FromJson<MemoryStore>(json) ?? new MemoryStore();
                    return;
                }
            }
            catch { }

            _store = new MemoryStore();
        }

        private static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(MemoryFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string json = JsonUtility.ToJson(_store, true);
                File.WriteAllText(MemoryFilePath, json);
            }
            catch { }
        }
    }
}
