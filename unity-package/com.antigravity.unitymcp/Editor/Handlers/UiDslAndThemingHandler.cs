#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [Serializable]
    public class UiLayoutNodeDto
    {
        public string type; // Panel, Text, Button, Image, Slider
        public string name;
        public string text;
        public string colorHex;
        public string anchorPreset; // Center, StretchAll, TopLeft, TopRight, BottomLeft, BottomRight, TopStretch, BottomStretch
        public Vector2 position;
        public Vector2 size;
        public List<UiLayoutNodeDto> children = new List<UiLayoutNodeDto>();
    }

    public static class UiDslAndThemingHandler
    {
        public static McpResponse BuildUiLayout(string jsonSchema, string parentCanvas = "Main_Canvas")
        {
            if (string.IsNullOrEmpty(jsonSchema)) return McpResponse.Error("JSON schema cannot be empty.");

            var canvas = UIHandler.GetOrCreateRootCanvas(parentCanvas);
            UiLayoutNodeDto rootNode;
            try
            {
                rootNode = JsonUtility.FromJson<UiLayoutNodeDto>(jsonSchema);
            }
            catch (Exception ex)
            {
                return McpResponse.Error($"Failed to parse UI JSON layout: {ex.Message}");
            }

            var rootObj = InstantiateUiNode(rootNode, canvas.transform);
            Undo.RegisterCreatedObjectUndo(rootObj, "Build UI Layout via MCP DSL");

            return McpResponse.Success($"Built UI Layout '{rootObj.name}' from DSL schema successfully!", rootObj.name);
        }

        public static McpResponse ApplyUiTheme(string themeJson, string rootCanvasName = "Main_Canvas")
        {
            var canvas = UIHandler.GetOrCreateRootCanvas(rootCanvasName);
            var texts = canvas.GetComponentsInChildren<Text>(true);
            var images = canvas.GetComponentsInChildren<Image>(true);

            // Apply theme colors
            Color primary = UIHandler.HexToColor("#38bdf8");
            Color bg = UIHandler.HexToColor("#0f172a", 0.95f);

            foreach (var img in images)
            {
                if (img.gameObject.name.Contains("BG") || img.gameObject.name.Contains("Panel")) img.color = bg;
            }

            foreach (var txt in texts)
            {
                if (txt.gameObject.name.Contains("Title") || txt.gameObject.name.Contains("Header")) txt.color = primary;
            }

            return McpResponse.Success($"Applied UI Design Theme across {texts.Length} Text and {images.Length} Image components.");
        }

        public static McpResponse BindUiEvent(string buttonName, string targetObjectName, string componentType, string methodName)
        {
            var btnGo = SceneHandler.FindGameObject(buttonName);
            if (btnGo == null) return McpResponse.Error($"Button '{buttonName}' not found.");

            var btn = btnGo.GetComponent<Button>();
            if (btn == null) return McpResponse.Error($"GameObject '{buttonName}' does not have a Button component.");

            var targetGo = SceneHandler.FindGameObject(targetObjectName);
            if (targetGo == null) return McpResponse.Error($"Target GameObject '{targetObjectName}' not found.");

            var comp = targetGo.GetComponent(componentType);
            if (comp == null) return McpResponse.Error($"Component '{componentType}' not found on '{targetObjectName}'.");

            var method = comp.GetType().GetMethod(methodName);
            if (method == null) return McpResponse.Error($"Method '{methodName}' not found on '{componentType}'.");

            var action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), comp, method);
            UnityEventTools.AddPersistentListener(btn.onClick, action);
            EditorUtility.SetDirty(btn);

            return McpResponse.Success($"Bound Button '{buttonName}' onClick -> '{targetObjectName}.{componentType}.{methodName}()'.");
        }

        private static GameObject InstantiateUiNode(UiLayoutNodeDto node, Transform parent)
        {
            var go = new GameObject(string.IsNullOrEmpty(node.name) ? node.type : node.name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();

            ApplyAnchorPreset(rect, node.anchorPreset);
            if (node.position != Vector2.zero) rect.anchoredPosition = node.position;
            if (node.size != Vector2.zero) rect.sizeDelta = node.size;

            Color color = !string.IsNullOrEmpty(node.colorHex) ? UIHandler.HexToColor(node.colorHex) : Color.white;

            switch (node.type?.ToLowerInvariant())
            {
                case "panel":
                    var img = go.AddComponent<Image>();
                    img.sprite = UIHandler.GetOrCreateRoundedSprite(12, 64);
                    img.type = Image.Type.Sliced;
                    img.color = color;
                    break;
                case "text":
                    var txt = go.AddComponent<Text>();
                    txt.text = node.text ?? "Text Label";
                    txt.fontSize = 16;
                    txt.color = color;
                    txt.alignment = TextAnchor.MiddleCenter;
                    break;
                case "button":
                    var bImg = go.AddComponent<Image>();
                    bImg.sprite = UIHandler.GetOrCreateRoundedSprite(10, 64);
                    bImg.type = Image.Type.Sliced;
                    bImg.color = color;
                    var btn = go.AddComponent<Button>();
                    UIHandler.CreateStyledText(go.transform, "Label", node.text ?? "Button", 15, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    break;
            }

            if (node.children != null)
            {
                foreach (var child in node.children)
                {
                    InstantiateUiNode(child, go.transform);
                }
            }

            return go;
        }

        private static void ApplyAnchorPreset(RectTransform rect, string preset)
        {
            switch (preset?.ToLowerInvariant())
            {
                case "stretchall":
                case "stretch":
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.sizeDelta = Vector2.zero;
                    break;
                case "topstretch":
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    break;
                case "bottomstretch":
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    break;
                case "topleft":
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    break;
                case "topright":
                    rect.anchorMin = new Vector2(1f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 1f);
                    break;
                case "bottomleft":
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(0f, 0f);
                    rect.pivot = new Vector2(0f, 0f);
                    break;
                case "bottomright":
                    rect.anchorMin = new Vector2(1f, 0f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(1f, 0f);
                    break;
                default: // Center
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }
        }
    }
}
