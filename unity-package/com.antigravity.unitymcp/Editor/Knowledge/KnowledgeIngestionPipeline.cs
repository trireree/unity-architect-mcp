using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Knowledge
{
    [Serializable]
    public class KnowledgeChunk
    {
        public string id;
        public string topic;
        public string category; // CORE, SCRIPTING, INPUT, PHYSICS, AI, RENDERING, ANIMATION, UI, AUDIO, ASSETS, WORLD, PERFORMANCE, BUILD
        public string unityVersionMin; // e.g. "2021.3", "2023.1", "6000.0"
        public string api;
        public string[] tags;
        public string summary;
        public string bestPractice;
        public string codeExample;
        public string deprecatedApi;
        public string modernReplacement;
        public string source;
    }

    public static class KnowledgeIngestionPipeline
    {
        private static readonly List<KnowledgeChunk> KnowledgeStore = new List<KnowledgeChunk>();

        static KnowledgeIngestionPipeline()
        {
            IngestCoreKnowledge();
        }

        private static void IngestCoreKnowledge()
        {
            // 1. CORE & SCRIPTING (Modern API vs Deprecated)
            AddChunk(new KnowledgeChunk
            {
                id = "core_find_objects",
                topic = "Find Objects in Scene (Modern vs Deprecated)",
                category = "CORE",
                unityVersionMin = "2023.1",
                api = "Object.FindFirstObjectByType<T>",
                tags = new[] { "find", "findobjectoftype", "findfirstobjectbytype", "performance" },
                deprecatedApi = "Object.FindObjectOfType<T>()",
                modernReplacement = "Object.FindFirstObjectByType<T>() or Object.FindAnyObjectByType<T>(FindObjectsInactive.Exclude)",
                summary = "Unity 2023+ and Unity 6 deprecated FindObjectOfType due to undefined ordering overhead. Use FindFirstObjectByType or FindAnyObjectByType.",
                bestPractice = "Always cache component lookups in Awake/Start. Never execute in Update().",
                codeExample = "var player = Object.FindFirstObjectByType<PlayerController>();",
                source = "Unity Scripting API Reference"
            });

            // 2. INPUT SYSTEM (New Input System vs Legacy Input)
            AddChunk(new KnowledgeChunk
            {
                id = "input_actions",
                topic = "Unity Input System (com.unity.inputsystem)",
                category = "INPUT",
                unityVersionMin = "2021.3",
                api = "UnityEngine.InputSystem",
                tags = new[] { "input", "keyboard", "gamepad", "wasd", "touch", "playerinput" },
                deprecatedApi = "Input.GetAxis(\"Horizontal\"), Input.GetKeyDown",
                modernReplacement = "PlayerInput component or InputActionAsset / Keyboard.current",
                summary = "The modern Unity Input System is cross-platform, handles re-binding, multi-player splitscreen, and input buffers.",
                bestPractice = "Use PlayerInput component with Unity Events or C# generated class from .inputactions asset.",
                codeExample = "Vector2 move = Keyboard.current.wKey.isPressed ? Vector2.up : Vector2.zero;",
                source = "Unity Input System Package Manual"
            });

            // 3. PHYSICS & CHARACTER CONTROLLER
            AddChunk(new KnowledgeChunk
            {
                id = "physics_character_controller",
                topic = "CharacterController Collision and Slope Limits",
                category = "PHYSICS",
                unityVersionMin = "2021.3",
                api = "UnityEngine.CharacterController",
                tags = new[] { "charactercontroller", "slope", "stepoffset", "isgrounded", "move" },
                summary = "CharacterController does not react to forces naturally. Use controller.Move(). Handle custom slope sliding if slopeLimit is exceeded.",
                bestPractice = "Keep downward velocity at -2f when grounded to maintain grounding contact over uneven terrain.",
                codeExample = "controller.Move(velocity * Time.deltaTime);",
                source = "Unity Physics Manual"
            });

            // 4. RENDERING & URP LIT SHADER
            AddChunk(new KnowledgeChunk
            {
                id = "rendering_urp_lit",
                topic = "Universal Render Pipeline (URP) Lit Shader Properties",
                category = "RENDERING",
                unityVersionMin = "2021.3",
                api = "Universal Render Pipeline",
                tags = new[] { "urp", "lit", "_basecolor", "_basemap", "_smoothness", "shader" },
                deprecatedApi = "Standard Shader (_Color, _MainTex, _Glossiness)",
                modernReplacement = "Universal Render Pipeline/Lit (_BaseColor, _BaseMap, _Smoothness)",
                summary = "URP uses SRP Batcher compatible property names. Setting _Color instead of _BaseColor fails silently.",
                bestPractice = "Always check material.HasProperty('_BaseColor') before assigning runtime colors.",
                codeExample = "mat.SetColor(\"_BaseColor\", Color.blue); mat.SetFloat(\"_Smoothness\", 0.8f);",
                source = "Unity URP Manual"
            });

            // 5. AI & NAVMESH
            AddChunk(new KnowledgeChunk
            {
                id = "ai_navmesh_surface",
                topic = "NavMeshAgent Navigation & Dynamic Carving",
                category = "AI",
                unityVersionMin = "2021.3",
                api = "UnityEngine.AI.NavMeshAgent",
                tags = new[] { "navmesh", "agent", "obstacle", "carve", "destination" },
                summary = "NavMeshAgent finds optimal paths over baked NavMesh. For dynamic moving obstacles, use NavMeshObstacle with carve enabled.",
                bestPractice = "Set agent.stoppingDistance > 0 to prevent oscillation when reaching destination.",
                codeExample = "agent.SetDestination(target.position);",
                source = "Unity Navigation Manual"
            });

            // 6. UI & UI TOOLKIT
            AddChunk(new KnowledgeChunk
            {
                id = "ui_toolkit_vs_ugui",
                topic = "UI Toolkit (UXML / USS) vs uGUI Canvas",
                category = "UI",
                unityVersionMin = "2021.3",
                api = "UnityEngine.UIElements",
                tags = new[] { "ui", "canvas", "uitoolkit", "uxml", "uss", "textmeshpro" },
                summary = "UI Toolkit is the modern standard for runtime and editor UI, offering zero draw call batches and CSS-like styling.",
                bestPractice = "For runtime HUD, use UI Document component with UXML layouts, or TextMeshProUGUI for Canvas.",
                codeExample = "var root = GetComponent<UIDocument>().rootVisualElement;\nvar btn = root.Q<Button>(\"start-btn\");",
                source = "Unity UI Toolkit Guide"
            });

            // 7. WORLD STREAMING & LOD
            AddChunk(new KnowledgeChunk
            {
                id = "world_streaming_lod",
                topic = "Additive Scene Loading & LOD Groups",
                category = "WORLD",
                unityVersionMin = "2021.3",
                api = "UnityEngine.SceneManagement.SceneManager",
                tags = new[] { "lod", "streaming", "additive", "openworld", "culling" },
                summary = "Open-world titles split maps into grid chunks loaded via SceneManager.LoadSceneAsync(..., LoadSceneMode.Additive).",
                bestPractice = "Combine LODGroup components with Occlusion Culling for massive performance gains.",
                codeExample = "SceneManager.LoadSceneAsync(\"Chunk_0_1\", LoadSceneMode.Additive);",
                source = "Unity Optimization Guide"
            });

            // 8. PERFORMANCE & GC ALLOCATIONS
            AddChunk(new KnowledgeChunk
            {
                id = "perf_gc_optimization",
                topic = "Garbage Collection & Struct Allocations",
                category = "PERFORMANCE",
                unityVersionMin = "2021.3",
                api = "UnityEngine.Profiling.Profiler",
                tags = new[] { "gc", "garbagecollection", "allocations", "string", "struct", "profiler" },
                summary = "String concatenation in Update() causes heavy GC pauses. Use StringBuilder or string interpolation sparingly.",
                bestPractice = "Use NonAlloc physics methods like Physics.RaycastNonAlloc and Physics.OverlapSphereNonAlloc.",
                codeExample = "RaycastHit[] hits = new RaycastHit[10];\nint count = Physics.RaycastNonAlloc(ray, hits, 100f);",
                source = "Unity Performance Manual"
            });
        }

        private static void AddChunk(KnowledgeChunk chunk)
        {
            KnowledgeStore.Add(chunk);
        }

        public static List<KnowledgeChunk> QueryKnowledge(string query, string category = null)
        {
            string currentUnityVer = Application.unityVersion;
            var tokens = query.ToLowerInvariant().Split(new[] { ' ', ',', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

            var matched = KnowledgeStore.Where(k =>
                (string.IsNullOrEmpty(category) || k.category.Equals(category, StringComparison.OrdinalIgnoreCase)) &&
                (tokens.Any(t => k.topic.ToLowerInvariant().Contains(t) || k.tags.Any(tag => tag.Contains(t)) || k.summary.ToLowerInvariant().Contains(t)))
            ).ToList();

            return matched.Count > 0 ? matched : KnowledgeStore.Take(4).ToList();
        }
    }
}
