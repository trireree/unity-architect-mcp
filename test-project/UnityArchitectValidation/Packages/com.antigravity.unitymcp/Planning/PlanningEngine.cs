using System;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Templates;
using Antigravity.UnityMCP.Editor.Transaction;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Planning
{
    [Serializable]
    public class PlanStepDto
    {
        public int stepNumber;
        public string title;
        public string action;
        public string target;
        public string description;
        public List<string> dependsOn = new List<string>();
        public BatchActionItem batchItem;
    }

    [Serializable]
    public class SystemPlanDto
    {
        public string intent;
        public string systemType;
        public int totalSteps;
        public List<PlanStepDto> steps = new List<PlanStepDto>();
    }

    public static class PlanningEngine
    {
        public static SystemPlanDto GeneratePlan(string intent)
        {
            var plan = new SystemPlanDto { intent = intent };
            string lower = intent.ToLowerInvariant();

            if (lower.Contains("player") || lower.Contains("character") || lower.Contains("third person"))
            {
                plan.systemType = "ThirdPersonPlayerSystem";
                BuildPlayerPlan(plan);
            }
            else if (lower.Contains("car") || lower.Contains("vehicle") || lower.Contains("drivable"))
            {
                plan.systemType = "VehiclePhysicsSystem";
                BuildVehiclePlan(plan);
            }
            else if (lower.Contains("enemy") || lower.Contains("npc") || lower.Contains("ai"))
            {
                plan.systemType = "EnemyAISystem";
                BuildEnemyPlan(plan);
            }
            else if (lower.Contains("inventory") || lower.Contains("item"))
            {
                plan.systemType = "InventorySystem";
                BuildInventoryPlan(plan);
            }
            else if (lower.Contains("weapon") || lower.Contains("gun") || lower.Contains("combat"))
            {
                plan.systemType = "WeaponCombatSystem";
                BuildWeaponPlan(plan);
            }
            else
            {
                plan.systemType = "GenericGameObjectSystem";
                BuildGenericPlan(plan, intent);
            }

            plan.totalSteps = plan.steps.Count;
            return plan;
        }

        private static void BuildPlayerPlan(SystemPlanDto plan)
        {
            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 1,
                title = "Create Player Controller Script",
                action = "script_create",
                description = "Generate C# CharacterController movement script with sprint and jump physics.",
                batchItem = new BatchActionItem
                {
                    action = "script_create",
                    path = "Assets/Scripts/PlayerController.cs",
                    content = GameSystemTemplates.GetPlayerControllerScript()
                }
            });

            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 2,
                title = "Spawn Player Capsule",
                action = "gameobject_create",
                description = "Instantiate player GameObject at origin.",
                batchItem = new BatchActionItem
                {
                    action = "gameobject_create",
                    name = "PlayerCharacter",
                    primitiveType = "Capsule",
                    position = new[] { 0f, 1f, 0f },
                    tag = "Player"
                }
            });

            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 3,
                title = "Setup Follow Camera",
                action = "scaffold_camera",
                description = "Bind smooth follow camera to PlayerCharacter.",
                batchItem = new BatchActionItem
                {
                    action = "scaffold_camera",
                    target = "PlayerCharacter"
                }
            });
        }

        private static void BuildVehiclePlan(SystemPlanDto plan)
        {
            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 1,
                title = "Create Vehicle Controller Script",
                action = "script_create",
                description = "Generate 4-wheel vehicle physics script with motorTorque and steering.",
                batchItem = new BatchActionItem
                {
                    action = "script_create",
                    path = "Assets/Scripts/SimpleCarController.cs",
                    content = GameSystemTemplates.GetVehicleControllerScript()
                }
            });

            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 2,
                title = "Create Car Chassis",
                action = "gameobject_create",
                description = "Instantiate vehicle chassis with Rigidbody (1200kg).",
                batchItem = new BatchActionItem
                {
                    action = "gameobject_create",
                    name = "Car_Chassis",
                    primitiveType = "Cube",
                    scale = new[] { 2f, 1f, 4f },
                    position = new[] { 0f, 1f, 0f }
                }
            });

            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 3,
                title = "Configure Car Rigidbody",
                action = "physics_setup_rigidbody",
                description = "Set mass to 1200kg with 0.05 drag.",
                batchItem = new BatchActionItem
                {
                    action = "physics_setup_rigidbody",
                    target = "Car_Chassis",
                    mass = 1200f,
                    drag = 0.05f
                }
            });
        }

        private static void BuildEnemyPlan(SystemPlanDto plan)
        {
            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 1,
                title = "Create Enemy AI Script",
                action = "script_create",
                description = "Generate NavMeshAgent chase and patrol logic.",
                batchItem = new BatchActionItem
                {
                    action = "script_create",
                    path = "Assets/Scripts/EnemyAI.cs",
                    content = GameSystemTemplates.GetEnemyAIScript()
                }
            });

            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 2,
                title = "Spawn Enemy NPC",
                action = "gameobject_create",
                description = "Create Enemy capsule with NavMeshAgent.",
                batchItem = new BatchActionItem
                {
                    action = "scaffold_enemy",
                    name = "Enemy_Patrol"
                }
            });
        }

        private static void BuildInventoryPlan(SystemPlanDto plan)
        {
            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 1,
                title = "Create Inventory Manager Script",
                action = "script_create",
                description = "Generate Inventory and Item data structures.",
                batchItem = new BatchActionItem
                {
                    action = "script_create",
                    path = "Assets/Scripts/InventoryManager.cs",
                    content = GameSystemTemplates.GetInventoryScript()
                }
            });
        }

        private static void BuildWeaponPlan(SystemPlanDto plan)
        {
            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 1,
                title = "Create Weapon System Script",
                action = "script_create",
                description = "Generate Raycast / Projectile gun shooting and ammo script.",
                batchItem = new BatchActionItem
                {
                    action = "script_create",
                    path = "Assets/Scripts/WeaponController.cs",
                    content = GameSystemTemplates.GetWeaponScript()
                }
            });
        }

        private static void BuildGenericPlan(SystemPlanDto plan, string name)
        {
            plan.steps.Add(new PlanStepDto
            {
                stepNumber = 1,
                title = $"Create {name}",
                action = "gameobject_create",
                description = $"Create empty GameObject for {name}.",
                batchItem = new BatchActionItem
                {
                    action = "gameobject_create",
                    name = string.IsNullOrEmpty(name) ? "NewSystemObject" : name
                }
            });
        }

        public static McpResponse ExecutePlan(SystemPlanDto plan)
        {
            if (plan == null || plan.steps == null || plan.steps.Count == 0)
            {
                return McpResponse.Error("Invalid or empty execution plan.");
            }

            var batchReq = new BatchRequestDto
            {
                transactionId = $"plan_{plan.systemType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                autoRollbackOnError = true
            };

            foreach (var step in plan.steps)
            {
                if (step.batchItem != null)
                {
                    batchReq.actions.Add(step.batchItem);
                }
            }

            return BatchExecutor.ExecuteBatch(batchReq, r => UnityMcpBridge.ExecuteAction(r));
        }
    }
}
