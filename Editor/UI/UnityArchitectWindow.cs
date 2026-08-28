using System;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Graph;
using Antigravity.UnityMCP.Editor.Performance;
using Antigravity.UnityMCP.Editor.Transaction;
using Antigravity.UnityMCP.Editor.Validation;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.UI
{
    public class UnityArchitectWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        private int _selectedTab = 0;
        private readonly string[] _tabs = new[] { "📡 Connection", "📊 Project State", "🔄 Transactions", "🛡️ Validation", "⚡ Performance" };

        private ProjectSummaryDto _summary;
        private ValidationReportDto _valReport;
        private PerformanceMetricsDto _perfMetrics;

        [MenuItem("Window/Antigravity/Unity Architect MCP")]
        [MenuItem("Tools/Antigravity/Unity Architect MCP")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnityArchitectWindow>("Unity Architect MCP");
            window.minSize = new Vector2(480, 420);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshData();
        }

        private void RefreshData()
        {
            try
            {
                _summary = ProjectGraphBuilder.BuildSummary();
                _valReport = ValidationManager.ValidateScene();
                _perfMetrics = PerformanceProvider.HarvestMetrics();
            }
            catch { }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            GUILayout.Label("🏛️ Antigravity Unity Architect MCP", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Enterprise-grade Model Context Protocol bridge for deep AI orchestration, State Graphs, and Atomic Transactions.", MessageType.Info);

            EditorGUILayout.Space(6);
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs);
            EditorGUILayout.Space(8);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_selectedTab)
            {
                case 0:
                    DrawConnectionTab();
                    break;
                case 1:
                    DrawProjectStateTab();
                    break;
                case 2:
                    DrawTransactionsTab();
                    break;
                case 3:
                    DrawValidationTab();
                    break;
                case 4:
                    DrawPerformanceTab();
                    break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            if (GUILayout.Button("🔄 Refresh All Metrics & Graph", GUILayout.Height(28)))
            {
                RefreshData();
            }
        }

        private void DrawConnectionTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Server Configuration", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Status:", GUILayout.Width(80));
            var prev = GUI.color;
            GUI.color = UnityMcpBridge.IsRunning ? Color.green : Color.red;
            GUILayout.Label(UnityMcpBridge.IsRunning ? "● ONLINE" : "○ OFFLINE", EditorStyles.boldLabel);
            GUI.color = prev;
            EditorGUILayout.EndHorizontal();

            int port = EditorGUILayout.IntField("Bridge Port:", UnityMcpBridge.Port);
            if (port != UnityMcpBridge.Port) UnityMcpBridge.Port = port;

            EditorGUILayout.LabelField("Bridge URL:", $"http://127.0.0.1:{UnityMcpBridge.Port}/", EditorStyles.textField);

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (UnityMcpBridge.IsRunning)
            {
                if (GUILayout.Button("Restart Server", GUILayout.Height(24)))
                {
                    UnityMcpBridge.StopServer();
                    UnityMcpBridge.StartServer();
                }
                if (GUILayout.Button("Stop Server", GUILayout.Height(24)))
                {
                    UnityMcpBridge.StopServer();
                }
            }
            else
            {
                if (GUILayout.Button("Start Server", GUILayout.Height(24)))
                {
                    UnityMcpBridge.StartServer();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            if (GUILayout.Button("📋 Copy MCP Server Configuration JSON", GUILayout.Height(26)))
            {
                string snippet = $"{{\n  \"mcpServers\": {{\n    \"unity\": {{\n      \"command\": \"node\",\n      \"args\": [\"path/to/mcp-server/dist/index.js\"],\n      \"env\": {{\n        \"UNITY_BRIDGE_URL\": \"http://127.0.0.1:{UnityMcpBridge.Port}\"\n      }}\n    }}\n  }}\n}}";
                EditorGUIUtility.systemCopyBuffer = snippet;
                EditorUtility.DisplayDialog("Copied", "MCP configuration copied to clipboard!", "OK");
            }
        }

        private void DrawProjectStateTab()
        {
            if (_summary == null) RefreshData();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Incremental Project State", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Active Scene:", _summary.activeScene);
            EditorGUILayout.LabelField("Current State Hash:", _summary.currentHash);
            EditorGUILayout.LabelField("GameObjects Count:", _summary.gameObjectCount.ToString());
            EditorGUILayout.LabelField("C# Scripts Count:", _summary.scriptCount.ToString());
            EditorGUILayout.LabelField("Prefabs Count:", _summary.prefabCount.ToString());
            EditorGUILayout.LabelField("Materials Count:", _summary.materialCount.ToString());
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);
            GUILayout.Label("Key Scene GameObjects:", EditorStyles.boldLabel);
            foreach (var item in _summary.keyObjects)
            {
                EditorGUILayout.LabelField("• " + item);
            }
        }

        private void DrawTransactionsTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Atomic Transactions & Snapshots", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Active Transaction:", TransactionManager.ActiveTransactionId ?? "None (Idle)");
            EditorGUILayout.LabelField("Last Transaction:", TransactionManager.LastTransactionId ?? "None");

            EditorGUILayout.Space(6);
            if (!string.IsNullOrEmpty(TransactionManager.LastTransactionId))
            {
                if (GUILayout.Button("⏪ Rollback Last AI Change", GUILayout.Height(28)))
                {
                    if (EditorUtility.DisplayDialog("Confirm Rollback", $"Revert changes made by transaction '{TransactionManager.LastTransactionId}'?", "Rollback", "Cancel"))
                    {
                        var res = TransactionManager.RollbackTransaction(TransactionManager.LastTransactionId);
                        EditorUtility.DisplayDialog("Rollback Result", res.message ?? res.error, "OK");
                        RefreshData();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawValidationTab()
        {
            if (_valReport == null) RefreshData();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Scene Integrity & Validation", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Status:", _valReport.isValid ? "✅ Clean / Valid" : "⚠️ Issues Detected");
            EditorGUILayout.LabelField("Errors:", _valReport.errorCount.ToString());
            EditorGUILayout.LabelField("Warnings:", _valReport.warningCount.ToString());

            if (_valReport.issues.Count > 0)
            {
                EditorGUILayout.Space(6);
                GUILayout.Label("Detected Issues:", EditorStyles.boldLabel);
                foreach (var issue in _valReport.issues)
                {
                    EditorGUILayout.HelpBox($"[{issue.severity}] {issue.type} on '{issue.target}': {issue.message}", issue.severity == "Error" ? MessageType.Error : MessageType.Warning);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPerformanceTab()
        {
            if (_perfMetrics == null) RefreshData();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Engine Metrics (Extension Point)", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Draw Calls:", _perfMetrics.drawCalls.ToString());
            EditorGUILayout.LabelField("Batches:", _perfMetrics.batches.ToString());
            EditorGUILayout.LabelField("Triangles:", _perfMetrics.triangles.ToString());
            EditorGUILayout.LabelField("Vertices:", _perfMetrics.vertices.ToString());
            EditorGUILayout.LabelField("Allocated Memory:", $"{_perfMetrics.totalAllocatedMemoryMb} MB");
            EditorGUILayout.LabelField("Mono/GC Memory:", $"{_perfMetrics.gcAllocatedMemoryMb} MB");
            EditorGUILayout.LabelField("Active Objects:", _perfMetrics.activeGameObjectCount.ToString());
            EditorGUILayout.EndVertical();
        }
    }
}
