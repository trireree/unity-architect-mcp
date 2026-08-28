using System;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Handlers;
using Antigravity.UnityMCP.Editor.Planning;
using Antigravity.UnityMCP.Editor.Templates;
using Antigravity.UnityMCP.Editor.Transaction;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Architect
{
    public static class GameArchitectEngine
    {
        public static McpResponse ArchitectFullGamePrototype(string gameGenre = "OpenWorldCrime")
        {
            var batchReq = new BatchRequestDto
            {
                transactionId = $"architect_{gameGenre}_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                autoRollbackOnError = true
            };

            // 1. Core Scripts (Player, Car, Weapon, Health, Wanted, DayNight)
            batchReq.actions.Add(new BatchActionItem { action = "script_create", path = "Assets/Scripts/PlayerController.cs", content = GameSystemTemplates.GetPlayerControllerScript() });
            batchReq.actions.Add(new BatchActionItem { action = "script_create", path = "Assets/Scripts/SimpleCarController.cs", content = GameSystemTemplates.GetVehicleControllerScript() });
            batchReq.actions.Add(new BatchActionItem { action = "script_create", path = "Assets/Scripts/WeaponController.cs", content = GameSystemTemplates.GetWeaponScript() });
            batchReq.actions.Add(new BatchActionItem { action = "script_create", path = "Assets/Scripts/HealthSystem.cs", content = GameSystemTemplates.GetHealthScript() });
            batchReq.actions.Add(new BatchActionItem { action = "script_create", path = "Assets/Scripts/WantedSystem.cs", content = GameSystemTemplates.GetWantedSystemScript() });
            batchReq.actions.Add(new BatchActionItem { action = "script_create", path = "Assets/Scripts/DayNightCycle.cs", content = GameSystemTemplates.GetDayNightScript() });

            // 2. City Environment (Ground, Road Grid, Buildings)
            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "City_Ground",
                primitiveType = "Plane",
                scale = new[] { 20f, 1f, 20f },
                position = new[] { 0f, 0f, 0f }
            });

            // Scaffold 4 City Blocks
            float[] offsets = new float[] { -25f, 25f };
            int buildingId = 1;
            foreach (float x in offsets)
            {
                foreach (float z in offsets)
                {
                    batchReq.actions.Add(new BatchActionItem
                    {
                        action = "gameobject_create",
                        name = $"Building_Block_{buildingId++}",
                        primitiveType = "Cube",
                        scale = new[] { 15f, UnityEngine.Random.Range(8f, 22f), 15f },
                        position = new[] { x, 5f, z }
                    });
                }
            }

            // 3. Player Character & Follow Camera
            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "PlayerCharacter",
                primitiveType = "Capsule",
                position = new[] { 0f, 1f, 0f },
                tag = "Player"
            });
            batchReq.actions.Add(new BatchActionItem
            {
                action = "scaffold_camera",
                target = "PlayerCharacter"
            });

            // 4. Drivable Police Car
            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "Police_Car",
                primitiveType = "Cube",
                scale = new[] { 2f, 1f, 4f },
                position = new[] { 5f, 0.5f, 5f }
            });
            batchReq.actions.Add(new BatchActionItem
            {
                action = "physics_setup_rigidbody",
                target = "Police_Car",
                mass = 1200f
            });

            // 5. UI Canvas (HUD)
            batchReq.actions.Add(new BatchActionItem { action = "ui_create_canvas", renderMode = "ScreenSpaceOverlay" });
            batchReq.actions.Add(new BatchActionItem
            {
                action = "ui_create_element",
                elementType = "text",
                name = "HUD_Wanted_Stars",
                text = "WANTED: [☆☆☆☆☆]",
                posX = 20,
                posY = -20,
                width = 200,
                height = 40
            });

            // 6. Day/Night Sun Light
            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "Sun_DirectionalLight",
                rotation = new[] { 50f, -30f, 0f }
            });

            return BatchExecutor.ExecuteBatch(batchReq, r => UnityMcpBridge.ExecuteAction(r));
        }
    }
}
