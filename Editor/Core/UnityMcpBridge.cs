#pragma warning disable CS0618, CS0619
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Architect;
using Antigravity.UnityMCP.Editor.Assets;
using Antigravity.UnityMCP.Editor.Capabilities;
using Antigravity.UnityMCP.Editor.City;
using Antigravity.UnityMCP.Editor.Graph;
using Antigravity.UnityMCP.Editor.Handlers;
using Antigravity.UnityMCP.Editor.Healing;
using Antigravity.UnityMCP.Editor.Impact;
using Antigravity.UnityMCP.Editor.Intelligence;
using Antigravity.UnityMCP.Editor.Knowledge;
using Antigravity.UnityMCP.Editor.Memory;
using Antigravity.UnityMCP.Editor.Optimization;
using Antigravity.UnityMCP.Editor.Packages;
using Antigravity.UnityMCP.Editor.Performance;
using Antigravity.UnityMCP.Editor.Planning;
using Antigravity.UnityMCP.Editor.Playtest;
using Antigravity.UnityMCP.Editor.QualityGate;
using Antigravity.UnityMCP.Editor.Safety;
using Antigravity.UnityMCP.Editor.State;
using Antigravity.UnityMCP.Editor.Templates;
using Antigravity.UnityMCP.Editor.Transaction;
using Antigravity.UnityMCP.Editor.Validation;
using Antigravity.UnityMCP.Editor.World;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Core
{
    [InitializeOnLoad]
    public static class UnityMcpBridge
    {
        private const string PrefsPortKey = "UnityMCP_Port";
        private const int DefaultPort = 8080;

        private static HttpListener _listener;
        private static Thread _listenerThread;
        private static bool _isRunning;
        private static int _activePort = DefaultPort;

        public static int Port
        {
            get => _activePort;
            set => _activePort = value;
        }

        public static bool IsRunning => _isRunning;

        static UnityMcpBridge()
        {
            EditorApplication.quitting += StopServer;
            AssemblyReloadEvents.beforeAssemblyReload += StopServer;
            AssemblyReloadEvents.afterAssemblyReload += () => EditorApplication.delayCall += StartServer;
            EditorApplication.delayCall += StartServer;
        }

        public static void StartServer()
        {
            if (_isRunning) return;

            int preferredPort = EditorPrefs.GetInt(PrefsPortKey, DefaultPort);
            for (int port = preferredPort; port < preferredPort + 15; port++)
            {
                try
                {
                    _listener = new HttpListener();
                    _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
                    _listener.Start();
                    _activePort = port;
                    _isRunning = true;

                    _listenerThread = new Thread(ListenLoop) { IsBackground = true };
                    _listenerThread.Start();

                    // Register active instance for multi-project discovery
                    RegisterInstanceDiscovery(port);

                    Debug.Log($"<color=#00ff88>[Unity Architect MCP Enterprise]</color> Bridge running on <b>http://127.0.0.1:{port}/</b> (Unity {Application.unityVersion}, Project: {Application.productName})");
                    return;
                }
                catch
                {
                    _listener?.Close();
                    _listener = null;
                }
            }

            Debug.LogWarning("[Unity Architect MCP] All ports 8080-8095 are in use. Failed to start bridge.");
        }

        public static void StopServer()
        {
            if (!_isRunning) return;

            _isRunning = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch { }

            _listener = null;
            _listenerThread = null;
            UnregisterInstanceDiscovery();
        }

        private static void RegisterInstanceDiscovery(int port)
        {
            try
            {
                var discovery = new
                {
                    projectName = Application.productName,
                    projectPath = Application.dataPath.Replace("/Assets", ""),
                    port = port,
                    unityVersion = Application.unityVersion,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                string json = JsonUtility.ToJson(discovery, true);
                if (!Directory.Exists("Library")) Directory.CreateDirectory("Library");
                File.WriteAllText("Library/unity_mcp_instance.json", json);
            }
            catch { }
        }

        private static void UnregisterInstanceDiscovery()
        {
            try
            {
                if (File.Exists("Library/unity_mcp_instance.json")) File.Delete("Library/unity_mcp_instance.json");
            }
            catch { }
        }

        private static void ListenLoop()
        {
            while (_isRunning && _listener != null && _listener.IsListening)
            {
                try
                {
                    var context = _listener.GetContext();
                    if (!_isRunning) break;
                    ThreadPool.QueueUserWorkItem(async _ => await HandleRequestAsync(context));
                }
                catch (HttpListenerException) { break; }
                catch (ThreadAbortException) { break; }
                catch (Exception) { if (!_isRunning) break; }
            }
        }

        private static async Task HandleRequestAsync(HttpListenerContext context)
        {
            var req = context.Request;
            var res = context.Response;

            res.Headers.Add("Access-Control-Allow-Origin", "*");
            res.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            res.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 200;
                res.Close();
                return;
            }

            if (req.Url.AbsolutePath == "/health")
            {
                SendJson(res, McpResponse.Success($"Unity Architect MCP is active (Port {_activePort}).", Application.unityVersion));
                return;
            }

            if (req.HttpMethod != "POST")
            {
                SendJson(res, McpResponse.Error("Only POST method is accepted for execution."));
                return;
            }

            string body;
            using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
            {
                body = await reader.ReadToEndAsync();
            }

            try
            {
                var actionReq = JsonUtility.FromJson<BridgeRequest>(body);
                if (actionReq == null || string.IsNullOrEmpty(actionReq.action))
                {
                    SendJson(res, McpResponse.Error("Invalid payload. 'action' field is required."));
                    return;
                }

                var response = await MainThreadDispatcher.EnqueueAsync(() => ExecuteAction(actionReq, body));
                SendJson(res, response);
            }
            catch (Exception ex)
            {
                SendJson(res, McpResponse.Error($"Bridge Execution Exception: {ex.Message}"));
            }
        }

        public static McpResponse ExecuteAction(BridgeRequest req, string rawBody = null)
        {
            try
            {
                switch (req.action)
                {
                    // 1. GAME ARCHITECT & FULL GAME BUILDS
                    case "architect_full_game":
                        return GameArchitectEngine.ArchitectFullGame(req.name ?? req.text ?? "OpenWorldCrime");

                    // 2. ULTRA UI SUITE
                    case "ui_create_hud":
                        return UIHandler.CreateModernGameHUD(req.text ?? "Cyberpunk");
                    case "ui_create_pause_menu":
                        return UIHandler.CreatePauseMenu();
                    case "ui_create_dashboard":
                        return UIHandler.CreateVehicleDashboard();
                    case "ui_create_inventory":
                        return UIHandler.CreateInventoryGrid(req.count > 0 ? req.count : 4, 5);
                    case "ui_create_dialogue":
                        return UIHandler.CreateDialogueBox(req.name ?? "NPC Agent", req.text ?? "Mission objective updated.");
                    case "ui_create_skill_tree":
                        return UIHandler.CreateSkillTreeUI();
                    case "ui_create_canvas":
                        return McpResponse.Success("Canvas ready.", UIHandler.GetOrCreateRootCanvas().name);
                    case "ui_create_element":
                        return UIHandler.CreateUIElement(req.elementType, req.parent, req.name, req.text, req.posX, req.posY, req.width, req.height);

                    // 3. SCENE, GAMEOBJECT & TRANSFORM CONTROLS
                    case "gameobject_create":
                    case "scene_create_object":
                        return SceneAndTransformHandler.CreateGameObject(req.name, req.primitiveType, req.position, req.rotation, req.scale, req.parent);
                    case "transform_modify":
                        return SceneAndTransformHandler.ModifyTransform(req.target, req.position, req.rotation, req.scale, null, null, null, req.parent, null);
                    case "prefab_instantiate":
                        return SceneAndTransformHandler.InstantiatePrefab(req.path, req.position, req.rotation, req.parent, req.name);
                    case "prefab_save":
                        return SceneAndTransformHandler.SaveAsPrefab(req.target, req.path);
                    case "material_modify":
                        return SceneAndTransformHandler.ModifyMaterial(req.target, req.name, req.text, null, null, req.path);
                    case "scene_get_hierarchy":
                        return McpResponse.Success("Retrieved scene hierarchy.", SceneHandler.GetHierarchy());
                    case "gameobject_delete":
                        return SceneHandler.DeleteGameObject(req.target);

                    // 4. COMPONENT REFLECTION & PROPERTIES
                    case "component_set_property":
                        return ComponentReflectionHandler.SetSerializedField(req.target, req.componentType, req.propertyName, req.propertyValue);
                    case "component_invoke_method":
                        return ComponentReflectionHandler.InvokeMethod(req.target, req.componentType, req.name);
                    case "component_add":
                        return ComponentHandler.AddComponent(req.target, req.componentType);
                    case "component_remove":
                        return ComponentHandler.RemoveComponent(req.target, req.componentType);

                    // 5. PLAYMODE, TEST & PROFILER
                    case "playmode_set":
                    case "playmode_start":
                        return PlayModeAndTestHandler.SetPlayMode(req.name ?? "play");
                    case "playmode_stop":
                        return PlayModeAndTestHandler.SetPlayMode("stop");
                    case "playmode_pause":
                        return PlayModeAndTestHandler.SetPlayMode("pause");
                    case "physics_simulate":
                        return PlayModeAndTestHandler.SimulatePhysics(req.mass > 0 ? req.mass : 0.02f, req.count > 0 ? req.count : 1);
                    case "profile_metrics":
                        return PlayModeAndTestHandler.HarvestPerformanceMetrics();

                    // 6. CONSOLE & COMPILATION STREAMING
                    case "compilation_diagnostics":
                        return ConsoleAndCompilationStreamHandler.GetCompilationDiagnostics();
                    case "console_get_logs":
                        return ConsoleAndCompilationStreamHandler.GetDetailedConsoleLogs(req.count > 0 ? req.count : 50, req.filterType ?? "All");
                    case "console_clear":
                        return PlayModeHandler.ClearConsoleLogs();

                    // 7. ANIMATION, VFX & LIGHTING
                    case "vfx_create":
                        return AnimationAndVfxHandler.CreateParticleSystem(req.target, req.text ?? "Fire");
                    case "lighting_setup_volume":
                        return AnimationAndVfxHandler.SetupLightingVolume(req.text ?? "PostProcessing");

                    // 8. BUILD & PROJECT SETTINGS
                    case "build_player":
                        return BuildAndProjectHandler.BuildStandalonePlayer(req.name ?? "Windows", req.path ?? "Builds/GameBuild.exe");
                    case "project_add_tag":
                        return BuildAndProjectHandler.AddTagOrLayer(req.name, false);
                    case "project_add_layer":
                        return BuildAndProjectHandler.AddTagOrLayer(req.name, true);

                    // 9. C# LIVE REPL
                    case "csharp_eval":
                    case "execute_script":
                        return ScriptAndCompilationHandler.ExecuteCSharpCode(req.code ?? req.text);
                    case "create_script":
                        return ScriptAndCompilationHandler.CreateOrUpdateScript(req.path, req.code ?? req.text);
                    case "compilation_status":
                        return ScriptAndCompilationHandler.GetCompilationStatus();

                    // 10. OPTIMIZATION & QUALITY GATE
                    case "optimize_project":
                    {
                        var report = OptimizationEngine.OptimizeProject(true);
                        return McpResponse.Success($"Project optimization completed ({report.totalOptimizationsApplied} actions applied, ~{report.estimatedDrawCallSavings} draw calls saved).", JsonUtility.ToJson(report, true));
                    }
                    case "optimize_combine_meshes":
                        return OptimizationEngine.CombineMeshesInGameObject(req.target);
                    case "quality_gate":
                    {
                        var report = QualityGateEngine.EvaluateProjectQuality();
                        return McpResponse.Success($"Quality Gate Score: {report.overallScore}/100 (Grade: {report.grade})", JsonUtility.ToJson(report, true));
                    }
                    case "self_heal_loop":
                    {
                        var healReport = SelfHealingEngine.RunSelfHealingLoop(req.target);
                        return McpResponse.Success(healReport.isHealed ? "Self-healing completed." : "Self-healing loop finished with remaining issues.", JsonUtility.ToJson(healReport, true));
                    }
                    case "vision_capture":
                        return VisionHandler.CaptureSceneScreenshot();

                    default:
                        return McpResponse.Error($"Unknown action: '{req.action}'");
                }
            }
            catch (Exception ex)
            {
                return McpResponse.Error($"Action '{req.action}' failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void SendJson(HttpListenerResponse res, McpResponse data)
        {
            string json = JsonUtility.ToJson(data);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            res.ContentType = "application/json; charset=utf-8";
            res.ContentLength64 = bytes.Length;
            res.StatusCode = data.success ? 200 : 400;

            using (var stream = res.OutputStream)
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
