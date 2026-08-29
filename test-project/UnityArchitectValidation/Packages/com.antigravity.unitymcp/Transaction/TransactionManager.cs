using System;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Transaction
{
    public static class TransactionManager
    {
        private static string _activeTransactionId;
        private static int _undoGroupId;
        private static readonly List<string> TransactionHistory = new List<string>();

        public static string ActiveTransactionId => _activeTransactionId;
        public static string LastTransactionId => TransactionHistory.Count > 0 ? TransactionHistory[TransactionHistory.Count - 1] : null;

        public static string BeginTransaction(string name = null)
        {
            string txId = $"tx_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{UnityEngine.Random.Range(100, 999)}";
            if (!string.IsNullOrEmpty(name)) txId += $"_{name}";

            _activeTransactionId = txId;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName($"[UnityArchitect] {txId}");
            _undoGroupId = Undo.GetCurrentGroup();

            SnapshotManager.CreateSnapshot(txId, out _, out _);
            TransactionHistory.Add(txId);

            Debug.Log($"<color=#00ffcc>[UnityArchitect]</color> Transaction <b>{txId}</b> started (Undo Group: {_undoGroupId}).");
            return txId;
        }

        public static McpResponse CommitTransaction(string txId = null)
        {
            string targetTx = txId ?? _activeTransactionId;
            if (string.IsNullOrEmpty(targetTx))
            {
                return McpResponse.Error("No active transaction to commit.");
            }

            Undo.CollapseUndoOperations(_undoGroupId);
            _activeTransactionId = null;

            Debug.Log($"<color=#00ff88>[UnityArchitect]</color> Transaction <b>{targetTx}</b> committed successfully.");
            return McpResponse.Success($"Transaction '{targetTx}' committed.", null, targetTx);
        }

        public static McpResponse RollbackTransaction(string txId = null)
        {
            string targetTx = txId ?? _activeTransactionId ?? LastTransactionId;
            if (string.IsNullOrEmpty(targetTx))
            {
                return McpResponse.Error("No transaction ID specified or found in history for rollback.");
            }

            // 1. Undo in-memory objects
            try
            {
                Undo.RevertAllInCurrentGroup();
            }
            catch { }

            // 2. Restore file and scene snapshot
            if (SnapshotManager.RestoreSnapshot(targetTx, out var message, out var error))
            {
                _activeTransactionId = null;
                Debug.LogWarning($"<color=#ffaa00>[UnityArchitect]</color> Transaction <b>{targetTx}</b> rolled back successfully.");
                return McpResponse.Success(message, null, targetTx);
            }

            return McpResponse.Error($"Rollback failed: {error}", null, null, targetTx);
        }
    }
}
