#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Core;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Architect
{
    [Serializable]
    public class CompiledIntentDto
    {
        public string rawPrompt;
        public string genre; // "open_world_crime", "survival", "rpg", "arcade"
        public string perspective; // "third_person", "first_person", "top_down"
        public string setting; // "urban_coastal", "post_apocalyptic", "sci_fi"
        public string targetPlatform;
        public List<string> requiredSystems = new List<string>();
        public List<string> coreGameLoops = new List<string>();
    }

    [Serializable]
    public class ArchitectureLayerDto
    {
        public string layerName;
        public List<string> components = new List<string>();
        public List<string> dependencies = new List<string>();
        public string estimatedComplexity;
    }

    [Serializable]
    public class GameArchitecturePlanDto
    {
        public string title;
        public CompiledIntentDto intent;
        public List<ArchitectureLayerDto> layers = new List<ArchitectureLayerDto>();
        public List<string> executionOrder = new List<string>();
        public int totalTasks;
    }

    [Serializable]
    public class DecomposedTaskDto
    {
        public string id;
        public string title;
        public string layer;
        public int priority;
        public List<string> dependencies = new List<string>();
        public string action;
        public string validationCriteria;
    }

    [Serializable]
    public class DecomposedPlanDto
    {
        public string gameTitle;
        public int totalTasks;
        public List<DecomposedTaskDto> tasks = new List<DecomposedTaskDto>();
    }

    public static class GameArchitecturePlanner
    {
        public static CompiledIntentDto CompileIntent(string prompt)
        {
            var p = prompt?.ToLowerInvariant() ?? "";
            var intent = new CompiledIntentDto
            {
                rawPrompt = prompt,
                genre = p.Contains("crime") || p.Contains("gta") ? "open_world_crime" : (p.Contains("survival") ? "survival" : "action_adventure"),
                perspective = p.Contains("first") ? "first_person" : "third_person",
                setting = p.Contains("post") ? "post_apocalyptic" : (p.Contains("coastal") ? "modern_coastal" : "modern_urban"),
                targetPlatform = "StandaloneWindows64"
            };

            intent.requiredSystems.AddRange(new[] { "Player", "Camera", "Vehicle", "NPC_Pedestrians", "Traffic_AI", "Police_Wanted", "Weapons", "Missions", "DayNight", "Audio", "UI_HUD", "SaveLoad" });
            intent.coreGameLoops.AddRange(new[] { "Free Roam & Exploration", "Vehicle Driving & Traffic Interaction", "Crime & Police Pursuit", "Mission Completion & Rewards", "Save/Load State" });

            return intent;
        }

        public static GameArchitecturePlanDto GenerateArchitecture(string prompt)
        {
            var intent = CompileIntent(prompt);
            var plan = new GameArchitecturePlanDto
            {
                title = $"Autonomous Architecture: {intent.genre} ({intent.setting})",
                intent = intent
            };

            // 1. WORLD LAYER
            plan.layers.Add(new ArchitectureLayerDto
            {
                layerName = "WORLD",
                components = new List<string> { "Procedural Road Network", "Commercial & Residential Districts", "Modular Building Blocks", "Street Lighting & Props", "Player & Vehicle Spawn Points" },
                dependencies = new List<string>(),
                estimatedComplexity = "MEDIUM"
            });

            // 2. GAMEPLAY LAYER
            plan.layers.Add(new ArchitectureLayerDto
            {
                layerName = "GAMEPLAY",
                components = new List<string> { "Third-Person CharacterController", "Smooth Follow & Orbit Camera", "Drivable WheelCollider Vehicles", "Weapon & Hit Detection", "Inventory & Economy", "Mission Sequence Engine" },
                dependencies = new List<string> { "WORLD" },
                estimatedComplexity = "HIGH"
            });

            // 3. AI & SIMULATION LAYER
            plan.layers.Add(new ArchitectureLayerDto
            {
                layerName = "AI_SIMULATION",
                components = new List<string> { "NavMesh Pedestrian Simulation", "Object-Pooled Road Traffic", "5-Star Police Pursuit & Search State", "Dynamic NPC Schedules" },
                dependencies = new List<string> { "WORLD", "GAMEPLAY" },
                estimatedComplexity = "HIGH"
            });

            // 4. PRESENTATION LAYER
            plan.layers.Add(new ArchitectureLayerDto
            {
                layerName = "PRESENTATION",
                components = new List<string> { "Canvas HUD & Minimap Hook", "Day/Night Lighting & Sky Cycle", "Dynamic Audio Mixer & Engine SFX", "Particle Sparks & Muzzle Flash" },
                dependencies = new List<string> { "GAMEPLAY" },
                estimatedComplexity = "MEDIUM"
            });

            // 5. TECHNICAL LAYER
            plan.layers.Add(new ArchitectureLayerDto
            {
                layerName = "TECHNICAL",
                components = new List<string> { "Object Pooling System", "LOD Group Management", "Quality Gate & Performance Profiling", "Atomic Checkpoint & Rollback" },
                dependencies = new List<string> { "WORLD", "GAMEPLAY", "AI_SIMULATION" },
                estimatedComplexity = "MEDIUM"
            });

            plan.executionOrder.AddRange(new[] { "WORLD", "GAMEPLAY", "AI_SIMULATION", "PRESENTATION", "TECHNICAL" });
            plan.totalTasks = 20;

            return plan;
        }

        public static DecomposedPlanDto DecomposeGame(string prompt)
        {
            var arch = GenerateArchitecture(prompt);
            var dec = new DecomposedPlanDto
            {
                gameTitle = arch.title
            };

            int taskId = 100;
            // World Tasks
            dec.tasks.Add(new DecomposedTaskDto { id = $"TASK_{taskId++}", title = "Generate Procedural City World Grid", layer = "WORLD", priority = 1, action = "generate_world", validationCriteria = "City root exists with ground plane and > 10 buildings" });
            dec.tasks.Add(new DecomposedTaskDto { id = $"TASK_{taskId++}", title = "Setup Lighting & Day/Night Cycle", layer = "PRESENTATION", priority = 2, dependencies = new List<string> { "TASK_100" }, action = "scaffold_daynight", validationCriteria = "Directional light with DayNightCycle attached" });
            // Gameplay Tasks
            dec.tasks.Add(new DecomposedTaskDto { id = $"TASK_{taskId++}", title = "Scaffold Third Person Player & Camera", layer = "GAMEPLAY", priority = 3, dependencies = new List<string> { "TASK_100" }, action = "create_character", validationCriteria = "Player with CharacterController and Camera target" });
            dec.tasks.Add(new DecomposedTaskDto { id = $"TASK_{taskId++}", title = "Scaffold Drivable Vehicle with WheelColliders", layer = "GAMEPLAY", priority = 4, dependencies = new List<string> { "TASK_100" }, action = "create_vehicle", validationCriteria = "Vehicle with Rigidbody and 4 WheelColliders" });
            dec.tasks.Add(new DecomposedTaskDto { id = $"TASK_{taskId++}", title = "Setup Police Wanted Pursuit System", layer = "AI_SIMULATION", priority = 5, dependencies = new List<string> { "TASK_102" }, action = "scaffold_police", validationCriteria = "WantedSystem script compiled and attached" });
            dec.tasks.Add(new DecomposedTaskDto { id = $"TASK_{taskId++}", title = "Setup Traffic Spawner with Pooling", layer = "AI_SIMULATION", priority = 6, dependencies = new List<string> { "TASK_100" }, action = "scaffold_traffic", validationCriteria = "Traffic spawner created with road paths" });
            dec.tasks.Add(new DecomposedTaskDto { id = $"TASK_{taskId++}", title = "Create HUD & Mission Objective Tracker", layer = "PRESENTATION", priority = 7, dependencies = new List<string> { "TASK_102" }, action = "create_ui", validationCriteria = "Canvas HUD with Health and Wanted stars" });
            dec.tasks.Add(new DecomposedTaskDto { id = $"TASK_{taskId++}", title = "Execute Quality Gate & Compile Validation", layer = "TECHNICAL", priority = 8, dependencies = new List<string> { "TASK_100", "TASK_101", "TASK_102", "TASK_103", "TASK_104", "TASK_105", "TASK_106" }, action = "quality_gate", validationCriteria = "0 compile errors and Quality Score >= 80" });

            dec.totalTasks = dec.tasks.Count;
            return dec;
        }
    }
}
