#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class UIHandler
    {
        public static Canvas GetOrCreateRootCanvas(string name = "Main_Canvas")
        {
            var canvasGo = GameObject.Find(name);
            Canvas canvas = null;
            if (canvasGo != null) canvas = canvasGo.GetComponent<Canvas>();

            if (canvas == null)
            {
                canvasGo = new GameObject(name);
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGo.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasGo, "Create Main Canvas");
            }

            var eventSystem = UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
            }

            return canvas;
        }

        public static Color HexToColor(string hex, float alpha = 1f)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
            {
                c.a = alpha;
                return c;
            }
            return Color.white;
        }

        public static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        public static GameObject CreateStyledText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = color;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf") ?? Font.CreateDynamicFontFromOSFont("Segoe UI", fontSize);
            return go;
        }

        public static GameObject CreateStyledButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Color normalColor, Color hoverColor, Color textColor)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var img = btnGo.AddComponent<Image>();
            img.color = normalColor;

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = normalColor * 0.8f;
            btn.colors = colors;

            CreateStyledText(btnGo.transform, "Label", label, 18, TextAnchor.MiddleCenter, textColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return btnGo;
        }

        public static GameObject CreateProgressBar(Transform parent, string name, string labelText, Color fillColor, Color bgColor, Vector2 anchoredPos, Vector2 size, float initialFill = 1.0f)
        {
            var barRoot = new GameObject(name);
            barRoot.transform.SetParent(parent, false);
            var rootRect = barRoot.AddComponent<RectTransform>();
            rootRect.anchoredPosition = anchoredPos;
            rootRect.sizeDelta = size;

            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(barRoot.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = bgColor;

            // Fill
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(barRoot.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(initialFill, 1f);
            fillRect.sizeDelta = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = fillColor;

            // Label
            if (!string.IsNullOrEmpty(labelText))
            {
                CreateStyledText(barRoot.transform, "Label", labelText, 14, TextAnchor.MiddleLeft, Color.white, Vector2.zero, Vector2.one, new Vector2(8f, 0f), new Vector2(-16f, 0f));
            }

            return barRoot;
        }

        // ==========================================
        // 1000X ULTRA UI PRESETS
        // ==========================================

        public static McpResponse CreateModernGameHUD(string theme = "Cyberpunk")
        {
            var canvas = GetOrCreateRootCanvas("Modern_Game_HUD_Canvas");
            var hudRoot = new GameObject("HUD_Layout");
            hudRoot.transform.SetParent(canvas.transform, false);
            var hudRect = hudRoot.AddComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.sizeDelta = Vector2.zero;

            Color primaryColor = HexToColor("#00ffcc");
            Color healthColor = HexToColor("#ff3366");
            Color armorColor = HexToColor("#00aaff");
            Color staminaColor = HexToColor("#ffcc00");
            Color darkBg = HexToColor("#0a0e14", 0.75f);

            if (theme.Equals("Military", StringComparison.OrdinalIgnoreCase))
            {
                primaryColor = HexToColor("#e6a100");
                healthColor = HexToColor("#d9381e");
                armorColor = HexToColor("#4d88ff");
                darkBg = HexToColor("#14181c", 0.85f);
            }

            // 1. Bottom-Left: Health & Armor & Stamina Bars
            var statusGroup = new GameObject("Status_Group");
            statusGroup.transform.SetParent(hudRoot.transform, false);
            var statusRect = statusGroup.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 0f);
            statusRect.anchorMax = new Vector2(0f, 0f);
            statusRect.anchoredPosition = new Vector2(30f, 30f);
            statusRect.pivot = new Vector2(0f, 0f);
            statusRect.sizeDelta = new Vector2(320f, 110f);

            CreatePanel(statusGroup.transform, "Status_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, darkBg);
            CreateProgressBar(statusGroup.transform, "HealthBar", "✚ HEALTH 100", healthColor, HexToColor("#330011", 0.8f), new Vector2(10f, 70f), new Vector2(300f, 26f));
            CreateProgressBar(statusGroup.transform, "ArmorBar", "🛡 ARMOR 100", armorColor, HexToColor("#002244", 0.8f), new Vector2(10f, 40f), new Vector2(300f, 20f));
            CreateProgressBar(statusGroup.transform, "StaminaBar", "⚡ STAMINA", staminaColor, HexToColor("#332200", 0.8f), new Vector2(10f, 15f), new Vector2(300f, 14f));

            // 2. Bottom-Right: Weapon & Ammo Card
            var ammoGroup = new GameObject("Weapon_Ammo_Group");
            ammoGroup.transform.SetParent(hudRoot.transform, false);
            var ammoRect = ammoGroup.AddComponent<RectTransform>();
            ammoRect.anchorMin = new Vector2(1f, 0f);
            ammoRect.anchorMax = new Vector2(1f, 0f);
            ammoRect.anchoredPosition = new Vector2(-30f, 30f);
            ammoRect.pivot = new Vector2(1f, 0f);
            ammoRect.sizeDelta = new Vector2(240f, 90f);

            CreatePanel(ammoGroup.transform, "Ammo_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, darkBg);
            CreateStyledText(ammoGroup.transform, "WeaponName", "ASSAULT RIFLE", 16, TextAnchor.UpperLeft, primaryColor, Vector2.zero, Vector2.one, new Vector2(16f, -14f), new Vector2(-32f, -30f));
            CreateStyledText(ammoGroup.transform, "AmmoCount", "30 / 120", 32, TextAnchor.LowerRight, Color.white, Vector2.zero, Vector2.one, new Vector2(-16f, 12f), new Vector2(-32f, -30f));

            // 3. Top-Right: Minimap Radar Frame
            var miniGroup = new GameObject("Minimap_Frame");
            miniGroup.transform.SetParent(hudRoot.transform, false);
            var miniRect = miniGroup.AddComponent<RectTransform>();
            miniRect.anchorMin = new Vector2(1f, 1f);
            miniRect.anchorMax = new Vector2(1f, 1f);
            miniRect.anchoredPosition = new Vector2(-30f, -30f);
            miniRect.pivot = new Vector2(1f, 1f);
            miniRect.sizeDelta = new Vector2(180f, 180f);

            CreatePanel(miniGroup.transform, "Radar_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, darkBg);
            CreateStyledText(miniGroup.transform, "Compass_N", "N", 14, TextAnchor.UpperCenter, primaryColor, Vector2.zero, Vector2.one, new Vector2(0f, -6f), Vector2.zero);
            CreateStyledText(miniGroup.transform, "LocationLabel", "DOWNTOWN DISTRICT", 11, TextAnchor.LowerCenter, HexToColor("#aaaaaa"), Vector2.zero, Vector2.one, new Vector2(0f, 6f), Vector2.zero);

            // 4. Center: Dynamic Crosshair
            var crossGroup = new GameObject("Crosshair_Center");
            crossGroup.transform.SetParent(hudRoot.transform, false);
            var crossRect = crossGroup.AddComponent<RectTransform>();
            crossRect.anchorMin = new Vector2(0.5f, 0.5f);
            crossRect.anchorMax = new Vector2(0.5f, 0.5f);
            crossRect.anchoredPosition = Vector2.zero;
            crossRect.sizeDelta = new Vector2(16f, 16f);

            CreatePanel(crossGroup.transform, "Dot", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4f, 4f), primaryColor);
            CreatePanel(crossGroup.transform, "Line_T", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(2f, 8f), primaryColor);
            CreatePanel(crossGroup.transform, "Line_B", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(2f, 8f), primaryColor);
            CreatePanel(crossGroup.transform, "Line_L", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-8f, 0f), new Vector2(8f, 2f), primaryColor);
            CreatePanel(crossGroup.transform, "Line_R", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(8f, 0f), new Vector2(8f, 2f), primaryColor);

            Undo.RegisterCreatedObjectUndo(hudRoot, "Create Modern Game HUD");
            return McpResponse.Success($"Created Ultra Modern Game HUD with {theme} styling!");
        }

        public static McpResponse CreatePauseMenu(string theme = "Glassmorphism")
        {
            var canvas = GetOrCreateRootCanvas("Pause_Menu_Canvas");
            var menuRoot = new GameObject("Pause_Menu_Root");
            menuRoot.transform.SetParent(canvas.transform, false);
            var menuRect = menuRoot.AddComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.sizeDelta = Vector2.zero;

            // Frosted Backdrop
            CreatePanel(menuRoot.transform, "Backdrop_Blur", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#05080c", 0.85f));

            // Dialog Card
            var card = CreatePanel(menuRoot.transform, "Card_Dialog", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 520f), HexToColor("#101720", 0.95f));
            CreateStyledText(card.transform, "Title", "PAUSED", 36, TextAnchor.UpperCenter, HexToColor("#00ffcc"), Vector2.zero, Vector2.one, new Vector2(0f, -30f), Vector2.zero);

            string[] btnLabels = new string[] { "RESUME GAME", "OPTIONS", "AUDIO & GRAPHICS", "RESTART LEVEL", "MAIN MENU" };
            for (int i = 0; i < btnLabels.Length; i++)
            {
                float yPos = 80f - (i * 65f);
                CreateStyledButton(card.transform, "Btn_" + btnLabels[i].Replace(" ", ""), btnLabels[i], new Vector2(0f, yPos), new Vector2(300f, 48f), HexToColor("#1a2430"), HexToColor("#00ffcc"), Color.white);
            }

            Undo.RegisterCreatedObjectUndo(menuRoot, "Create Pause Menu");
            return McpResponse.Success("Created Ultra Pause Menu successfully!");
        }

        public static McpResponse CreateVehicleDashboard()
        {
            var canvas = GetOrCreateRootCanvas("Vehicle_Dashboard_Canvas");
            var dashRoot = new GameObject("Vehicle_Dashboard");
            dashRoot.transform.SetParent(canvas.transform, false);
            var rect = dashRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 35f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(380f, 120f);

            CreatePanel(dashRoot.transform, "Dash_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#0b0f14", 0.85f));
            CreateStyledText(dashRoot.transform, "SpeedNumber", "0", 48, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(-50f, 0f), new Vector2(140f, 60f));
            CreateStyledText(dashRoot.transform, "SpeedUnit", "KM/H", 16, TextAnchor.MiddleLeft, HexToColor("#00ffcc"), Vector2.zero, Vector2.one, new Vector2(30f, -8f), new Vector2(80f, 30f));
            CreateStyledText(dashRoot.transform, "GearIndicator", "D", 28, TextAnchor.MiddleRight, HexToColor("#ffcc00"), Vector2.zero, Vector2.one, new Vector2(-20f, 0f), Vector2.zero);
            CreateProgressBar(dashRoot.transform, "RPMBar", "RPM", HexToColor("#ff3366"), HexToColor("#222222"), new Vector2(0f, -40f), new Vector2(340f, 12f), 0.3f);

            Undo.RegisterCreatedObjectUndo(dashRoot, "Create Vehicle Dashboard");
            return McpResponse.Success("Created Ultra Vehicle Dashboard successfully!");
        }

        public static McpResponse CreateInventoryGrid(int rows = 4, int cols = 5)
        {
            var canvas = GetOrCreateRootCanvas("Inventory_Canvas");
            var invRoot = new GameObject("Inventory_Panel");
            invRoot.transform.SetParent(canvas.transform, false);
            var rect = invRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(580f, 480f);

            CreatePanel(invRoot.transform, "Inv_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#0c1117", 0.95f));
            CreateStyledText(invRoot.transform, "Header", "INVENTORY / BACKPACK", 22, TextAnchor.UpperLeft, HexToColor("#00ffcc"), Vector2.zero, Vector2.one, new Vector2(24f, -16f), Vector2.zero);

            var gridGo = new GameObject("Slot_Grid");
            gridGo.transform.SetParent(invRoot.transform, false);
            var gridRect = gridGo.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridRect.anchoredPosition = new Vector2(0f, -20f);
            gridRect.sizeDelta = new Vector2(520f, 380f);

            var glg = gridGo.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(85f, 85f);
            glg.spacing = new Vector2(14f, 14f);
            glg.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < (rows * cols); i++)
            {
                var slot = CreatePanel(gridGo.transform, $"Slot_{i + 1}", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#161f28"));
                CreatePanel(slot.transform, "Border", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#283645", 0.5f));
                CreateStyledText(slot.transform, "SlotNum", (i + 1).ToString(), 11, TextAnchor.LowerRight, HexToColor("#667788"), Vector2.zero, Vector2.one, new Vector2(-4f, 4f), Vector2.zero);
            }

            Undo.RegisterCreatedObjectUndo(invRoot, "Create Inventory Grid");
            return McpResponse.Success($"Created Ultra Inventory Grid ({cols}x{rows}) successfully!");
        }

        public static McpResponse CreateUIElement(string elementType, string parent, string name, string text, float posX, float posY, float width, float height)
        {
            var canvas = GetOrCreateRootCanvas();
            Transform parentTransform = canvas.transform;
            if (!string.IsNullOrEmpty(parent))
            {
                var p = SceneHandler.FindGameObject(parent);
                if (p != null) parentTransform = p.transform;
            }

            GameObject elemGo;
            switch (elementType?.ToLowerInvariant())
            {
                case "hud":
                case "modern_hud":
                    return CreateModernGameHUD();
                case "pause_menu":
                case "pausemenu":
                    return CreatePauseMenu();
                case "dashboard":
                case "vehicle_dashboard":
                    return CreateVehicleDashboard();
                case "inventory":
                    return CreateInventoryGrid(4, 5);
                case "button":
                    elemGo = CreateStyledButton(parentTransform, name ?? "Button", text ?? "Button", new Vector2(posX, posY), new Vector2(width > 0 ? width : 160f, height > 0 ? height : 45f), HexToColor("#1a2430"), HexToColor("#00ffcc"), Color.white);
                    break;
                case "text":
                    elemGo = CreateStyledText(parentTransform, name ?? "Text", text ?? "Sample Text", 18, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(posX, posY), new Vector2(width > 0 ? width : 200f, height > 0 ? height : 40f));
                    break;
                case "progressbar":
                case "healthbar":
                    elemGo = CreateProgressBar(parentTransform, name ?? "ProgressBar", text ?? "STATUS", HexToColor("#00ffcc"), HexToColor("#222222"), new Vector2(posX, posY), new Vector2(width > 0 ? width : 240f, height > 0 ? height : 24f));
                    break;
                default:
                    elemGo = CreatePanel(parentTransform, name ?? "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(posX, posY), new Vector2(width > 0 ? width : 200f, height > 0 ? height : 200f), HexToColor("#111822", 0.8f));
                    break;
            }

            Undo.RegisterCreatedObjectUndo(elemGo, $"Create UI {elementType}");
            Selection.activeGameObject = elemGo;
            string idStr = EntityIdHelper.GetIdString(elemGo);
            return McpResponse.Success($"Created UI Element '{elemGo.name}' (ID: {idStr})", idStr);
        }
    }
}
