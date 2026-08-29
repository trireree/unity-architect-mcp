#pragma warning disable CS0618, CS0619
using System;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class CustomEditorToolingHandler
    {
        public static McpResponse ScaffoldCustomEditorWindow(string windowClassName, string menuPath, string saveDirectory = "Assets/Editor")
        {
            if (string.IsNullOrEmpty(windowClassName)) windowClassName = "CustomToolWindow";
            if (string.IsNullOrEmpty(menuPath)) menuPath = $"Tools/{windowClassName}";
            if (!saveDirectory.StartsWith("Assets/")) saveDirectory = "Assets/" + saveDirectory.TrimStart('/');

            if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
            string scriptPath = Path.Combine(saveDirectory, $"{windowClassName}.cs").Replace("\\", "/");

            string code = $@"#pragma warning disable CS0618, CS0619
using UnityEditor;
using UnityEngine;

public class {windowClassName} : EditorWindow
{{
    [MenuItem(""{menuPath}"")]
    public static void ShowWindow()
    {{
        var window = GetWindow<{windowClassName}>(""{windowClassName}"");
        window.minSize = new Vector2(350f, 400f);
        window.Show();
    }}

    private Vector2 scrollPos;
    private string searchFilter = """";

    private void OnGUI()
    {{
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(""{windowClassName} Control Panel"", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(""Built automatically via Antigravity Unity Architect MCP."", MessageType.Info);
        
        EditorGUILayout.Space(5);
        searchFilter = EditorGUILayout.TextField(""Search"", searchFilter);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.Space(10);

        if (GUILayout.Button(""Execute Action"", GUILayout.Height(35)))
        {{
            Debug.Log(""[{windowClassName}] Action executed!"");
        }}

        EditorGUILayout.EndScrollView();
    }}
}}";

            File.WriteAllText(scriptPath, code);
            AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            return McpResponse.Success($"Scaffolded Custom EditorWindow at '{scriptPath}' (Menu: '{menuPath}').", scriptPath);
        }

        public static McpResponse ScaffoldCustomInspector(string targetComponentClass, string saveDirectory = "Assets/Editor")
        {
            if (string.IsNullOrEmpty(targetComponentClass)) return McpResponse.Error("Target component class name cannot be empty.");
            if (!saveDirectory.StartsWith("Assets/")) saveDirectory = "Assets/" + saveDirectory.TrimStart('/');

            if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
            string scriptPath = Path.Combine(saveDirectory, $"{targetComponentClass}Editor.cs").Replace("\\", "/");

            string code = $@"#pragma warning disable CS0618, CS0619
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof({targetComponentClass}))]
public class {targetComponentClass}Editor : Editor
{{
    public override void OnInspectorGUI()
    {{
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(""Custom Actions"", EditorStyles.boldLabel);

        var targetScript = ({targetComponentClass})target;
        if (GUILayout.Button(""Trigger Custom Action"", GUILayout.Height(30)))
        {{
            Debug.Log(""[{targetComponentClass}] Custom action triggered on "" + targetScript.name);
        }}
    }}
}}";

            File.WriteAllText(scriptPath, code);
            AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            return McpResponse.Success($"Scaffolded Custom Inspector Editor at '{scriptPath}' for class '{targetComponentClass}'.", scriptPath);
        }
    }
}
