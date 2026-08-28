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

        public static int Port
        {
            get => EditorPrefs.GetInt(PrefsPortKey, DefaultPort);
            set => EditorPrefs.SetInt(PrefsPortKey, value);
        }

        public static bool IsRunning => _isRunning;

        static UnityMcpBridge()
        {
            EditorApplication.quitting += StopServer;
            AssemblyReloadEvents.beforeAssemblyReload += StopServer;
            AssemblyReloadEvents.afterAssemblyReload += StartServer;

            StartServer();
        }

        public static void StartServer()
        {
            if (_isRunning) return;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
                _listener.Start();
                _isRunning = true;

                _listenerThread = new Thread(ListenLoop) { IsBackground = true };
                _listenerThread.Start();

                Debug.Log($"<color=#00ff88>[Unity Architect MCP v4.0.0]</color> Enterprise Server running on <b>http://127.0.0.1:{Port}/</b> (Unity {Application.unityVersion})");
            }
            catch (Exception ex)
            {
                _isRunning = false;
                Debug.LogWarning($"[Unity Architect MCP] Failed to start HTTP server on port {Port}: {ex.Message}");
            }
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

            try
            {
                if (_listenerThread != null && _listenerThread.IsAlive)
                {
                    _listenerThread.Join(50);
                }
            }
            catch { }

            _listener = null;
            _listenerThread = null;
            Debug.Log("<color=#ffaa00>[Unity Architect MCP]</color> Server stopped.");
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
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception)
                {
                    if (!_isRunning) break;
                }
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
                SendJson(res, McpResponse.Success("Unity Architect MCP Bridge v4.0.0 is active and healthy.", Application.unityVersion));
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
                    SendJson(res, McpResponse.Error("Invalid request payload. 'action' field is required."));
                    return;
                }

                if (!SafetyPolicy.ValidateActionSafety(actionReq, out string safetyWarning))
                {
                    SendJson(res, McpResponse.Error(safetyWarning));
                    return;
                }

                var response = await MainThreadDispatcher.EnqueueAsync(() => ExecuteAction(actionReq, body));

                if (response.success)
                {
                    DevelopmentMemory.RecordAction(actionReq.action, response.message, response.transactionId);
                }

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
                    // --- PHASE 60+: AUTONOMOUS AAA GAME DEVELOPMENT ---
                    case "generate_game_architecture":
                    {
                        var plan = GameArchitecturePlanner.GenerateArchitecture(req.text ?? req.query ?? "open-world crime game");
                        return McpResponse.Success($"Generated {plan.layers.Count}-layer game architecture.", JsonUtility.ToJson(plan, true));
                    }

                    case "decompose_game":
                    {
                        var dec = GameArchitecturePlanner.DecomposeGame(req.text ?? req.query ?? "open-world crime game");
                        return McpResponse.Success($"Decomposed game into {dec.totalTasks} dependency-ordered tasks.", JsonUtility.ToJson(dec, true));
                    }

                    case "compile_intent":
                    {
                        var intent = GameArchitecturePlanner.CompileIntent(req.text ?? req.query ?? "open-world crime game");
                        return McpResponse.Success("Compiled natural language intent.", JsonUtility.ToJson(intent, true));
                    }

                    case "generate_world":
                    {
                        var cfg = new WorldGenerationConfig();
                        if (req.count > 0) cfg.districtSize = req.count;
                        return WorldEngineV2.GenerateFullWorld(cfg);
                    }

                    case "inspect_assets_v2":
                    {
                        var inv = AssetIntelligenceV2.InspectProjectAssets();
                        return McpResponse.Success($"Inspected {inv.totalAssets} project assets.", JsonUtility.ToJson(inv, true));
                    }

                    case "optimize_project":
                    {
                        var opt = OptimizationEngine.OptimizeProject(true);
                        return McpResponse.Success($"Project optimization completed ({opt.totalOptimizationsApplied} actions applied).", JsonUtility.ToJson(opt, true));
                    }

                    case "build_kenney_city":
                    {
                        return KenneyCityGenerator.BuildCity();
                    }

                    case "engine_capabilities":
                    {
                        var caps = CapabilityEngine.GetCapabilities();
                        return McpResponse.Success("Engine capabilities retrieved.", JsonUtility.ToJson(caps, true));
                    }

                    case "build_game_full":
                    {
                        // 1. World Generation
                        var wRes = WorldEngineV2.GenerateFullWorld(new WorldGenerationConfig { seed = 48392, districtSize = 3 });
                        // 2. Player & Camera
                        var pRes = ScaffoldingHandler.ScaffoldThirdPersonPlayer("PlayerCharacter");
                        var cRes = ScaffoldingHandler.ScaffoldThirdPersonCamera("PlayerCharacter");
                        // 3. Vehicle & Police
                        var vRes = GameSystemTemplates.ScaffoldSystem("vehicle", "Assets/Scripts");
                        var polRes = GameSystemTemplates.ScaffoldSystem("police", "Assets/Scripts");
                        // 4. Day/Night & HUD
                        var dnRes = GameSystemTemplates.ScaffoldSystem("daynight", "Assets/Scripts");
                        var uiRes = UIHandler.CreateCanvas("ScreenSpaceOverlay");
                        // 5. Quality Gate
                        var qg = QualityGateEngine.EvaluateProjectQuality();

                        return McpResponse.Success($"Autonomous Game Build Finished! Quality Score: {qg.overallScore}/100 (Grade: {qg.grade})", JsonUtility.ToJson(qg, true));
                    }

                    // --- REPAIR CONTEXT & KNOWLEDGE ---
                    case "repair_context":
                    {
                        var errors = SelfHealingEngine.CollectAllErrors();
                        if (errors.Count == 0) return McpResponse.Success("No active errors found to repair.");
                        var ctx = RepairContextGenerator.GenerateMinimalRepairContext(errors[0]);
                        return McpResponse.Success("Generated minimal LLM repair context.", JsonUtility.ToJson(ctx, true));
                    }

                    case "query_knowledge_v2":
                    case "search_knowledge":
                    {
                        var chunks = KnowledgeIngestionPipeline.QueryKnowledge(req.query ?? req.text ?? "", req.filter);
                        return McpResponse.Success($"Retrieved {chunks.Count} version-aware knowledge chunk(s).", JsonUtility.ToJson(new { chunks = chunks }, true));
                    }

                    case "analyze_impact":
                    {
                        var report = ImpactAnalysisEngine.AnalyzeImpact(req.target ?? req.name ?? req.path, req.text ?? "DELETE");
                        return McpResponse.Success("Impact analysis completed.", JsonUtility.ToJson(report, true));
                    }

                    case "run_playtest":
                    {
                        var report = PlayModeTestEngine.RunPlaytest(req.target ?? "PlayerCharacter");
                        return McpResponse.Success(report.overallPassed ? "Playtest passed successfully." : "Playtest detected issues.", JsonUtility.ToJson(report, true));
                    }

                    case "generate_city":
                    {
                        var cfg = new CityConfig();
                        if (req.count > 0) cfg.gridWidth = req.count;
                        return ProceduralCityGenerator.GenerateProceduralCity(cfg);
                    }

                    case "quality_gate":
                    {
                        var report = QualityGateEngine.EvaluateProjectQuality();
                        return McpResponse.Success($"Quality Gate Score: {report.overallScore}/100 (Grade: {report.grade})", JsonUtility.ToJson(report, true));
                    }

                    case "inspect_packages":
                    {
                        var report = PackageIntelligence.InspectPackages();
                        return McpResponse.Success($"Package inspection completed ({report.totalInstalled} key packages installed).", JsonUtility.ToJson(report, true));
                    }

                    case "self_heal_loop":
                    {
                        var healReport = SelfHealingEngine.RunSelfHealingLoop(req.target);
                        return McpResponse.Success(healReport.isHealed ? "Self-healing completed successfully." : "Self-healing loop finished with remaining issues.", JsonUtility.ToJson(healReport, true));
                    }
                    case "diagnose_errors":
                    {
                        var errors = SelfHealingEngine.CollectAllErrors();
                        return McpResponse.Success($"Diagnosed {errors.Count} error(s).", JsonUtility.ToJson(new { errors = errors }, true));
                    }
                    case "query_context":
                    {
                        var ctx = ContextIntelligence.QueryGraph(req.query ?? req.text);
                        return McpResponse.Success("Context query processed.", JsonUtility.ToJson(ctx, true));
                    }
                    case "asset_dependencies":
                    {
                        var deps = AssetIntelligence.GetAssetDependencies(req.path);
                        return McpResponse.Success("Retrieved asset dependencies.", JsonUtility.ToJson(deps, true));
                    }
                    case "find_duplicates":
                    {
                        var dupes = AssetIntelligence.FindDuplicateAssets(req.path ?? "Assets");
                        return McpResponse.Success($"Found {dupes.Count} duplicate asset(s).", JsonUtility.ToJson(new { duplicates = dupes }, true));
                    }
                    case "plan_system":
                    {
                        var plan = PlanningEngine.GeneratePlan(req.text ?? req.name);
                        return McpResponse.Success($"Generated execution plan ({plan.totalSteps} steps).", JsonUtility.ToJson(plan, true));
                    }
                    case "execute_plan":
                    {
                        var plan = PlanningEngine.GeneratePlan(req.text ?? req.name);
                        return PlanningEngine.ExecutePlan(plan);
                    }
                    case "scaffold_system":
                    {
                        return GameSystemTemplates.ScaffoldSystem(req.name, req.path ?? "Assets/Scripts");
                    }
                    case "architect_game":
                    {
                        return GameArchitectEngine.ArchitectFullGamePrototype(req.name ?? "OpenWorldCrime");
                    }
                    case "memory_history":
                    {
                        var history = DevelopmentMemory.QueryRecentHistory(req.count > 0 ? req.count : 20, req.query);
                        return McpResponse.Success($"Retrieved {history.Count} journal entries.", JsonUtility.ToJson(new { history = history }, true));
                    }
                    case "inspect_project":
                    {
                        var summary = ProjectGraphBuilder.BuildSummary();
                        return McpResponse.Success("Retrieved project state summary.", JsonUtility.ToJson(summary, true));
                    }
                    case "inspect_scene":
                    {
                        var graph = ProjectGraphBuilder.GetOrBuildGraph(false);
                        var sceneSummary = new
                        {
                            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                            sceneHash = graph.graphHash,
                            objectCount = graph.nodes.Values.Count(n => n.type == GraphNodeType.GAMEOBJECT.ToString()),
                            rootObjects = graph.nodes.Values.Where(n => n.type == GraphNodeType.GAMEOBJECT.ToString() && !n.path.Contains("/")).Select(n => n.name).ToList()
                        };
                        return McpResponse.Success("Retrieved active scene overview.", JsonUtility.ToJson(sceneSummary, true));
                    }
                    case "inspect_object":
                    {
                        var graph = ProjectGraphBuilder.GetOrBuildGraph(false);
                        var targetNode = graph.nodes.Values.FirstOrDefault(n => n.name.Equals(req.target, StringComparison.OrdinalIgnoreCase) || n.id == req.target);
                        if (targetNode == null)
                        {
                            return McpResponse.Error($"Object '{req.target}' not found in Project State Graph.");
                        }
                        var subtree = graph.GetSubtree(targetNode.id);
                        return McpResponse.Success($"Retrieved subtree for '{req.target}' ({subtree.Count} nodes).", JsonUtility.ToJson(new { root = targetNode, subtree = subtree }, true));
                    }
                    case "state_diff":
                    {
                        var graph = ProjectGraphBuilder.GetOrBuildGraph(true);
                        var diff = StateDiffEngine.ComputeDiff(graph);
                        return McpResponse.Success("Computed project state diff.", JsonUtility.ToJson(diff, true));
                    }
                    case "snapshot_create":
                    {
                        string txId = TransactionManager.BeginTransaction(req.name);
                        return McpResponse.Success($"Created snapshot for transaction '{txId}'.", txId, txId);
                    }
                    case "snapshot_rollback":
                    {
                        return TransactionManager.RollbackTransaction(req.target);
                    }
                    case "validate_scene":
                    {
                        var report = ValidationManager.ValidateScene();
                        return McpResponse.Success($"Scene validation completed (Errors: {report.errorCount}, Warnings: {report.warningCount}).", JsonUtility.ToJson(report, true));
                    }
                    case "execute_batch":
                    {
                        if (string.IsNullOrEmpty(rawBody)) return McpResponse.Error("Raw JSON payload required for execute_batch.");
                        var batchReq = JsonUtility.FromJson<BatchRequestDto>(rawBody);
                        return BatchExecutor.ExecuteBatch(batchReq, r => ExecuteAction(r));
                    }
                    case "profile_metrics":
                    {
                        var metrics = PerformanceProvider.HarvestMetrics();
                        return McpResponse.Success("Harvested engine performance metrics.", JsonUtility.ToJson(metrics, true));
                    }

                    // --- GRANULAR ENGINE TOOLS ---
                    case "scene_get_hierarchy":
                        return McpResponse.Success("Retrieved scene hierarchy.", SceneHandler.GetHierarchy());
                    case "gameobject_create":
                        return SceneHandler.CreateGameObject(req.name, req.primitiveType, req.position, req.rotation, req.scale, req.parent);
                    case "gameobject_modify":
                        return SceneHandler.ModifyGameObject(req.target, req.name, req.position, req.rotation, req.scale, req.tag, req.layer, req.active);
                    case "gameobject_delete":
                        return SceneHandler.DeleteGameObject(req.target);
                    case "gameobject_duplicate":
                        return SceneHandler.DuplicateGameObject(req.target);
                    case "component_add":
                        return ComponentHandler.AddComponent(req.target, req.componentType);
                    case "component_remove":
                        return ComponentHandler.RemoveComponent(req.target, req.componentType);
                    case "component_get_properties":
                        return ComponentHandler.GetComponentProperties(req.target, req.componentType);
                    case "component_set_property":
                        return ComponentHandler.SetComponentProperty(req.target, req.componentType, req.propertyName, req.propertyValue);
                    case "asset_create_prefab":
                        return AssetHandler.CreatePrefab(req.target, req.path);
                    case "asset_instantiate_prefab":
                        return AssetHandler.InstantiatePrefab(req.path, req.position, req.rotation, req.parent);
                    case "asset_create_material":
                        return AssetHandler.CreateMaterial(req.name, req.shaderName, req.colorHex, req.path);
                    case "asset_find":
                        return AssetHandler.FindAssets(req.filter, req.path);
                    case "script_create":
                        return ScriptAndCompilationHandler.CreateOrUpdateScript(req.path, req.content);
                    case "script_status":
                        return ScriptAndCompilationHandler.GetCompilationStatus();
                    case "csharp_eval":
                        return ScriptAndCompilationHandler.ExecuteCSharpCode(req.code);
                    case "vision_capture_scene":
                        return VisionHandler.CaptureSceneView(req.width, req.height);
                    case "vision_capture_game":
                        return VisionHandler.CaptureGameView(req.width, req.height);
                    case "vision_inspect_object":
                        return VisionHandler.InspectGameObjectVisual(req.target);
                    case "physics_setup_rigidbody":
                        return PhysicsAndNavHandler.SetupRigidbody(req.target, req.mass, req.drag, req.angularDrag, req.useGravity, req.isKinematic);
                    case "physics_setup_collider":
                        return PhysicsAndNavHandler.SetupCollider(req.target, req.colliderType, req.isTrigger, req.center, req.size);
                    case "physics_bake_navmesh":
                        return PhysicsAndNavHandler.BakeNavMesh();
                    case "animator_create":
                        return AnimationHandler.CreateAnimatorController(req.path, req.name);
                    case "animator_add_state":
                        return AnimationHandler.AddState(req.path, req.name, req.motionPath);
                    case "animator_add_param":
                        return AnimationHandler.AddParameter(req.path, req.name, req.paramType);
                    case "ui_create_canvas":
                        return UIHandler.CreateCanvas(req.renderMode);
                    case "ui_create_element":
                        return UIHandler.CreateUIElement(req.elementType, req.parent, req.name, req.text, req.posX, req.posY, req.width, req.height);
                    case "playmode_start":
                        return PlayModeHandler.StartPlayMode();
                    case "playmode_stop":
                        return PlayModeHandler.StopPlayMode();
                    case "playmode_pause":
                        return PlayModeHandler.PausePlayMode(req.pause);
                    case "console_get_logs":
                        return PlayModeHandler.GetConsoleLogs(req.count, req.filterType);
                    case "console_clear":
                        return PlayModeHandler.ClearConsoleLogs();
                    case "scaffold_player":
                        return ScaffoldingHandler.ScaffoldThirdPersonPlayer(req.name);
                    case "scaffold_enemy":
                        return ScaffoldingHandler.ScaffoldEnemyAI(req.name);
                    case "scaffold_camera":
                        return ScaffoldingHandler.ScaffoldThirdPersonCamera(req.target);

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
            res.Close();
        }
    }

    [Serializable]
    public class BridgeRequest
    {
        public string action;
        public string target;
        public string name;
        public string path;
        public string content;
        public string code;
        public string query;
        public string primitiveType;
        public string componentType;
        public string propertyName;
        public string propertyValue;
        public string shaderName;
        public string colorHex;
        public string filter;
        public string colliderType;
        public string renderMode;
        public string elementType;
        public string text;
        public string motionPath;
        public string paramType;
        public string filterType;
        public int width;
        public int height;
        public int count = 50;
        public float mass = 1f;
        public float drag = 0f;
        public float angularDrag = 0.05f;
        public bool useGravity = true;
        public bool isKinematic = false;
        public bool isTrigger = false;
        public bool pause = true;
        public float posX;
        public float posY;
        public bool? active;
        public float[] position;
        public float[] rotation;
        public float[] scale;
        public float[] center;
        public float[] size;
        public string parent;
        public string tag;
        public string layer;
    }
}
