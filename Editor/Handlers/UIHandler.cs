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
        private static Sprite _cachedRoundedSprite;
        private static Sprite _cachedGlowBorderSprite;

        public static Sprite GetOrCreateRoundedSprite(int radius = 16, int size = 64)
        {
            if (_cachedRoundedSprite != null) return _cachedRoundedSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color transparent = new Color(0, 0, 0, 0);
            Color solid = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = 0f;
                    int cx = x < radius ? radius : (x >= size - radius ? size - radius - 1 : x);
                    int cy = y < radius ? radius : (y >= size - radius ? size - radius - 1 : y);

                    if (cx != x || cy != y)
                    {
                        dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        if (dist > radius)
                        {
                            tex.SetPixel(x, y, transparent);
                            continue;
                        }
                        else if (dist > radius - 1.5f)
                        {
                            float alpha = 1f - (dist - (radius - 1.5f)) / 1.5f;
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                            continue;
                        }
                    }
                    tex.SetPixel(x, y, solid);
                }
            }

            tex.Apply();
            _cachedRoundedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            return _cachedRoundedSprite;
        }

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

        public static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color, bool useRounded = true)
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
            if (useRounded)
            {
                img.sprite = GetOrCreateRoundedSprite(14, 64);
                img.type = Image.Type.Sliced;
            }
            return go;
        }

        public static GameObject CreateStyledText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, FontStyle fontStyle = FontStyle.Bold)
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
            txt.fontStyle = fontStyle;
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
            img.sprite = GetOrCreateRoundedSprite(10, 64);
            img.type = Image.Type.Sliced;
            img.color = normalColor;

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = normalColor * 0.75f;
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            CreateStyledText(btnGo.transform, "Label", label, 16, TextAnchor.MiddleCenter, textColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
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
            bgImg.sprite = GetOrCreateRoundedSprite(8, 64);
            bgImg.type = Image.Type.Sliced;
            bgImg.color = bgColor;

            // Fill
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(barRoot.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(initialFill, 1f);
            fillRect.sizeDelta = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.sprite = GetOrCreateRoundedSprite(8, 64);
            fillImg.type = Image.Type.Sliced;
            fillImg.color = fillColor;

            // Label
            if (!string.IsNullOrEmpty(labelText))
            {
                CreateStyledText(barRoot.transform, "Label", labelText, 13, TextAnchor.MiddleLeft, Color.white, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-20f, 0f));
            }

            return barRoot;
        }

        // ==========================================
        // PROFESSIONAL THEMED UI SYSTEMS
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
            Color healthColor = HexToColor("#ff2a5f");
            Color armorColor = HexToColor("#00b4d8");
            Color staminaColor = HexToColor("#ffd166");
            Color darkBg = HexToColor("#0b131e", 0.88f);
            Color glowBorder = HexToColor("#1e293b", 0.7f);

            if (theme.Equals("Military", StringComparison.OrdinalIgnoreCase))
            {
                primaryColor = HexToColor("#e6a100");
                healthColor = HexToColor("#d9381e");
                armorColor = HexToColor("#4d88ff");
                darkBg = HexToColor("#14181c", 0.90f);
            }

            // 1. Bottom-Left: Status Group (Health, Armor, Stamina)
            var statusGroup = new GameObject("Status_Group");
            statusGroup.transform.SetParent(hudRoot.transform, false);
            var statusRect = statusGroup.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 0f);
            statusRect.anchorMax = new Vector2(0f, 0f);
            statusRect.anchoredPosition = new Vector2(35f, 35f);
            statusRect.pivot = new Vector2(0f, 0f);
            statusRect.sizeDelta = new Vector2(340f, 120f);

            CreatePanel(statusGroup.transform, "Status_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, darkBg);
            CreateProgressBar(statusGroup.transform, "HealthBar", "✚ HEALTH 100", healthColor, HexToColor("#2a0812", 0.9f), new Vector2(12f, 78f), new Vector2(316f, 26f));
            CreateProgressBar(statusGroup.transform, "ArmorBar", "🛡 ARMOR 100", armorColor, HexToColor("#031b2e", 0.9f), new Vector2(12f, 46f), new Vector2(316f, 22f));
            CreateProgressBar(statusGroup.transform, "StaminaBar", "⚡ STAMINA", staminaColor, HexToColor("#281d02", 0.9f), new Vector2(12f, 18f), new Vector2(316f, 16f));

            // 2. Bottom-Right: Tactical Weapon & Ammo Card
            var ammoGroup = new GameObject("Weapon_Ammo_Group");
            ammoGroup.transform.SetParent(hudRoot.transform, false);
            var ammoRect = ammoGroup.AddComponent<RectTransform>();
            ammoRect.anchorMin = new Vector2(1f, 0f);
            ammoRect.anchorMax = new Vector2(1f, 0f);
            ammoRect.anchoredPosition = new Vector2(-35f, 35f);
            ammoRect.pivot = new Vector2(1f, 0f);
            ammoRect.sizeDelta = new Vector2(260f, 95f);

            CreatePanel(ammoGroup.transform, "Ammo_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, darkBg);
            CreateStyledText(ammoGroup.transform, "WeaponName", "ASSAULT RIFLE MK-IV", 14, TextAnchor.UpperLeft, primaryColor, Vector2.zero, Vector2.one, new Vector2(18f, -14f), new Vector2(-36f, -30f));
            CreateStyledText(ammoGroup.transform, "AmmoCount", "30 / 120", 34, TextAnchor.LowerRight, Color.white, Vector2.zero, Vector2.one, new Vector2(-18f, 12f), new Vector2(-36f, -30f));

            // 3. Top-Right: Minimap Radar Frame
            var miniGroup = new GameObject("Minimap_Frame");
            miniGroup.transform.SetParent(hudRoot.transform, false);
            var miniRect = miniGroup.AddComponent<RectTransform>();
            miniRect.anchorMin = new Vector2(1f, 1f);
            miniRect.anchorMax = new Vector2(1f, 1f);
            miniRect.anchoredPosition = new Vector2(-35f, -35f);
            miniRect.pivot = new Vector2(1f, 1f);
            miniRect.sizeDelta = new Vector2(200f, 200f);

            CreatePanel(miniGroup.transform, "Radar_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, darkBg);
            CreateStyledText(miniGroup.transform, "Compass_N", "▲ N", 14, TextAnchor.UpperCenter, primaryColor, Vector2.zero, Vector2.one, new Vector2(0f, -8f), Vector2.zero);
            CreateStyledText(miniGroup.transform, "LocationLabel", "METROPOLIS DOWNTOWN", 11, TextAnchor.LowerCenter, HexToColor("#94a3b8"), Vector2.zero, Vector2.one, new Vector2(0f, 8f), Vector2.zero);

            // 4. Center: Sleek Dynamic Crosshair
            var crossGroup = new GameObject("Crosshair_Center");
            crossGroup.transform.SetParent(hudRoot.transform, false);
            var crossRect = crossGroup.AddComponent<RectTransform>();
            crossRect.anchorMin = new Vector2(0.5f, 0.5f);
            crossRect.anchorMax = new Vector2(0.5f, 0.5f);
            crossRect.anchoredPosition = Vector2.zero;
            crossRect.sizeDelta = new Vector2(20f, 20f);

            CreatePanel(crossGroup.transform, "Dot", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4f, 4f), primaryColor, false);
            CreatePanel(crossGroup.transform, "Line_T", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 9f), new Vector2(2f, 8f), primaryColor, false);
            CreatePanel(crossGroup.transform, "Line_B", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -9f), new Vector2(2f, 8f), primaryColor, false);
            CreatePanel(crossGroup.transform, "Line_L", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-9f, 0f), new Vector2(8f, 2f), primaryColor, false);
            CreatePanel(crossGroup.transform, "Line_R", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(9f, 0f), new Vector2(8f, 2f), primaryColor, false);

            Undo.RegisterCreatedObjectUndo(hudRoot, "Create Modern Game HUD");
            return McpResponse.Success($"Created Professional Handcrafted HUD ({theme}) with procedural rounded graphics!");
        }

        public static McpResponse CreatePauseMenu()
        {
            var canvas = GetOrCreateRootCanvas("Pause_Menu_Canvas");
            var menuRoot = new GameObject("Pause_Menu_Root");
            menuRoot.transform.SetParent(canvas.transform, false);
            var menuRect = menuRoot.AddComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.sizeDelta = Vector2.zero;

            CreatePanel(menuRoot.transform, "Backdrop_Blur", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#030712", 0.90f), false);
            var card = CreatePanel(menuRoot.transform, "Card_Dialog", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 540f), HexToColor("#0f172a", 0.98f));
            CreateStyledText(card.transform, "Title", "PAUSE MENU", 32, TextAnchor.UpperCenter, HexToColor("#38bdf8"), Vector2.zero, Vector2.one, new Vector2(0f, -28f), Vector2.zero);

            string[] btnLabels = new string[] { "RESUME GAME", "OPTIONS & SETTINGS", "AUDIO & GRAPHICS", "RESTART LEVEL", "EXIT TO DESKTOP" };
            for (int i = 0; i < btnLabels.Length; i++)
            {
                float yPos = 85f - (i * 68f);
                CreateStyledButton(card.transform, "Btn_" + btnLabels[i].Replace(" ", ""), btnLabels[i], new Vector2(0f, yPos), new Vector2(320f, 50f), HexToColor("#1e293b"), HexToColor("#38bdf8"), Color.white);
            }

            Undo.RegisterCreatedObjectUndo(menuRoot, "Create Pause Menu");
            return McpResponse.Success("Created Professional Glassmorphism Pause Menu!");
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
            rect.sizeDelta = new Vector2(400f, 130f);

            CreatePanel(dashRoot.transform, "Dash_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#0a0f18", 0.90f));
            CreateStyledText(dashRoot.transform, "SpeedNumber", "0", 52, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, new Vector2(-55f, 0f), new Vector2(150f, 70f));
            CreateStyledText(dashRoot.transform, "SpeedUnit", "KM/H", 16, TextAnchor.MiddleLeft, HexToColor("#00ffcc"), Vector2.zero, Vector2.one, new Vector2(35f, -8f), new Vector2(80f, 30f));
            CreateStyledText(dashRoot.transform, "GearIndicator", "D", 30, TextAnchor.MiddleRight, HexToColor("#f59e0b"), Vector2.zero, Vector2.one, new Vector2(-25f, 0f), Vector2.zero);
            CreateProgressBar(dashRoot.transform, "RPMBar", "RPM", HexToColor("#ef4444"), HexToColor("#1e293b"), new Vector2(0f, -42f), new Vector2(360f, 14f), 0.3f);

            Undo.RegisterCreatedObjectUndo(dashRoot, "Create Vehicle Dashboard");
            return McpResponse.Success("Created Professional Vehicle Dashboard!");
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
            rect.sizeDelta = new Vector2(600f, 500f);

            CreatePanel(invRoot.transform, "Inv_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#0b121d", 0.96f));
            CreateStyledText(invRoot.transform, "Header", "TACTICAL BACKPACK", 22, TextAnchor.UpperLeft, HexToColor("#38bdf8"), Vector2.zero, Vector2.one, new Vector2(28f, -20f), Vector2.zero);

            var gridGo = new GameObject("Slot_Grid");
            gridGo.transform.SetParent(invRoot.transform, false);
            var gridRect = gridGo.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridRect.anchoredPosition = new Vector2(0f, -22f);
            gridRect.sizeDelta = new Vector2(540f, 400f);

            var glg = gridGo.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(90f, 90f);
            glg.spacing = new Vector2(14f, 14f);
            glg.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < (rows * cols); i++)
            {
                var slot = CreatePanel(gridGo.transform, $"Slot_{i + 1}", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#172231"));
                CreatePanel(slot.transform, "Border", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#334155", 0.6f));
                CreateStyledText(slot.transform, "SlotNum", (i + 1).ToString(), 12, TextAnchor.LowerRight, HexToColor("#94a3b8"), Vector2.zero, Vector2.one, new Vector2(-6f, 6f), Vector2.zero);
            }

            Undo.RegisterCreatedObjectUndo(invRoot, "Create Inventory Grid");
            return McpResponse.Success($"Created Professional Inventory Grid ({cols}x{rows})!");
        }

        public static McpResponse CreateDialogueBox(string speaker = "COMMANDER", string dialogue = "Mission objective: Infiltrate the downtown sector and extract the encrypted payload.")
        {
            var canvas = GetOrCreateRootCanvas("Dialogue_Canvas");
            var diagRoot = new GameObject("Dialogue_Panel");
            diagRoot.transform.SetParent(canvas.transform, false);
            var rect = diagRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 120f);
            rect.sizeDelta = new Vector2(880f, 190f);

            CreatePanel(diagRoot.transform, "Diag_BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#090e17", 0.95f));
            CreatePanel(diagRoot.transform, "Portrait_Box", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(125f, 125f), HexToColor("#1e293b"));
            CreateStyledText(diagRoot.transform, "SpeakerName", speaker.ToUpper(), 18, TextAnchor.UpperLeft, HexToColor("#38bdf8"), Vector2.zero, Vector2.one, new Vector2(170f, -22f), Vector2.zero);
            CreateStyledText(diagRoot.transform, "DialogueText", dialogue, 16, TextAnchor.UpperLeft, Color.white, Vector2.zero, Vector2.one, new Vector2(170f, -58f), new Vector2(-200f, -85f), FontStyle.Normal);
            CreateStyledButton(diagRoot.transform, "Btn_Continue", "[ SPACE ] CONTINUE ▶", new Vector2(355f, -55f), new Vector2(150f, 36f), HexToColor("#1e293b"), HexToColor("#38bdf8"), Color.white);

            Undo.RegisterCreatedObjectUndo(diagRoot, "Create Dialogue Box");
            return McpResponse.Success("Created Professional Dialogue Box UI!");
        }

        public static McpResponse CreateSkillTreeUI()
        {
            var canvas = GetOrCreateRootCanvas("SkillTree_Canvas");
            var treeRoot = new GameObject("Skill_Tree_Root");
            treeRoot.transform.SetParent(canvas.transform, false);
            var rect = treeRoot.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            CreatePanel(treeRoot.transform, "Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, HexToColor("#030712", 0.96f), false);
            CreateStyledText(treeRoot.transform, "Title", "ABILITY & PERK MATRIX", 34, TextAnchor.UpperCenter, HexToColor("#38bdf8"), Vector2.zero, Vector2.one, new Vector2(0f, -32f), Vector2.zero);

            string[] skills = new string[] { "SPEED I", "SPEED II", "DASH", "HEALTH I", "ARMOR I", "REGEN", "DAMAGE I", "CRITICAL", "OVERDRIVE" };
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    int idx = r * 3 + c;
                    float x = (c - 1) * 230f;
                    float y = (1 - r) * 170f - 20f;
                    CreateStyledButton(treeRoot.transform, $"Node_{skills[idx]}", $"★ {skills[idx]}", new Vector2(x, y), new Vector2(170f, 70f), HexToColor("#172231"), HexToColor("#38bdf8"), Color.white);
                }
            }

            Undo.RegisterCreatedObjectUndo(treeRoot, "Create Skill Tree UI");
            return McpResponse.Success("Created Professional Skill & Perk Tree UI!");
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
                case "dialogue":
                case "dialog":
                    return CreateDialogueBox();
                case "skill_tree":
                case "perk_tree":
                    return CreateSkillTreeUI();
                case "button":
                    var btn = CreateStyledButton(parentTransform, name ?? "Button", text ?? "Button", new Vector2(posX, posY), new Vector2(width > 0 ? width : 160f, height > 0 ? height : 45f), HexToColor("#1e293b"), HexToColor("#38bdf8"), Color.white);
                    Selection.activeGameObject = btn;
                    return McpResponse.Success($"Created Button '{btn.name}'", EntityIdHelper.GetIdString(btn));
                case "text":
                    var txt = CreateStyledText(parentTransform, name ?? "Text", text ?? "Sample Text", 18, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(posX, posY), new Vector2(width > 0 ? width : 200f, height > 0 ? height : 40f));
                    Selection.activeGameObject = txt;
                    return McpResponse.Success($"Created Text '{txt.name}'", EntityIdHelper.GetIdString(txt));
                default:
                    var pnl = CreatePanel(parentTransform, name ?? "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(posX, posY), new Vector2(width > 0 ? width : 200f, height > 0 ? height : 200f), HexToColor("#0f172a", 0.9f));
                    Selection.activeGameObject = pnl;
                    return McpResponse.Success($"Created Panel '{pnl.name}'", EntityIdHelper.GetIdString(pnl));
            }
        }
    }
}
