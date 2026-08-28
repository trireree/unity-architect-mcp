using System;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class UIHandler
    {
        public static McpResponse CreateCanvas(string renderMode)
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();

            if (Enum.TryParse<RenderMode>(renderMode, true, out var mode))
            {
                canvas.renderMode = mode;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");

            var eventSystem = UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
            }

            Selection.activeGameObject = canvasGo;
            string idStr = EntityIdHelper.GetIdString(canvasGo);
            return McpResponse.Success($"Created Canvas (ID: {idStr})", idStr);
        }

        public static McpResponse CreateUIElement(string elementType, string parent, string name, string text, float posX, float posY, float width, float height)
        {
            Transform parentTransform = null;
            if (!string.IsNullOrEmpty(parent))
            {
                var parentGo = SceneHandler.FindGameObject(parent);
                if (parentGo != null) parentTransform = parentGo.transform;
            }

            if (parentTransform == null)
            {
                var canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
                if (canvas == null)
                {
                    CreateCanvas("ScreenSpaceOverlay");
                    canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
                }
                if (canvas != null) parentTransform = canvas.transform;
            }

            var elemGo = new GameObject(string.IsNullOrEmpty(name) ? elementType : name);
            if (parentTransform != null) elemGo.transform.SetParent(parentTransform, false);

            var rect = elemGo.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(posX, posY);
            if (width > 0 && height > 0) rect.sizeDelta = new Vector2(width, height);

            switch (elementType?.ToLowerInvariant())
            {
                case "text":
                    var txt = elemGo.AddComponent<Text>();
                    txt.text = string.IsNullOrEmpty(text) ? "New Text" : text;
                    txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                    txt.color = Color.white;
                    txt.alignment = TextAnchor.MiddleCenter;
                    break;

                case "button":
                    var btnImg = elemGo.AddComponent<Image>();
                    btnImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                    elemGo.AddComponent<Button>();

                    var btnTextGo = new GameObject("Text");
                    btnTextGo.transform.SetParent(elemGo.transform, false);
                    var btnRect = btnTextGo.AddComponent<RectTransform>();
                    btnRect.anchorMin = Vector2.zero;
                    btnRect.anchorMax = Vector2.one;
                    btnRect.sizeDelta = Vector2.zero;
                    var btnText = btnTextGo.AddComponent<Text>();
                    btnText.text = string.IsNullOrEmpty(text) ? "Button" : text;
                    btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                    btnText.color = Color.black;
                    btnText.alignment = TextAnchor.MiddleCenter;
                    break;

                case "image":
                case "panel":
                    var img = elemGo.AddComponent<Image>();
                    img.color = elementType.ToLowerInvariant() == "panel" ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
                    break;

                default:
                    elemGo.AddComponent<Image>();
                    break;
            }

            Undo.RegisterCreatedObjectUndo(elemGo, $"Create UI {elementType}");
            Selection.activeGameObject = elemGo;
            string idStr = EntityIdHelper.GetIdString(elemGo);
            return McpResponse.Success($"Created UI Element '{elemGo.name}' (ID: {idStr})", idStr);
        }
    }
}
