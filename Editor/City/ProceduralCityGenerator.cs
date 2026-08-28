using System;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Transaction;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.City
{
    [Serializable]
    public class CityConfig
    {
        public int seed = 48392;
        public int gridWidth = 4;
        public int gridHeight = 4;
        public float blockWidth = 20f;
        public float roadWidth = 8f;
        public float minBuildingHeight = 8f;
        public float maxBuildingHeight = 28f;
    }

    public static class ProceduralCityGenerator
    {
        public static McpResponse GenerateProceduralCity(CityConfig config = null)
        {
            config = config ?? new CityConfig();
            var random = new System.Random(config.seed);

            var batchReq = new BatchRequestDto
            {
                transactionId = $"city_gen_{config.seed}_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                autoRollbackOnError = true
            };

            // 1. City Root & Main Ground
            float totalWidth = config.gridWidth * (config.blockWidth + config.roadWidth);
            float totalHeight = config.gridHeight * (config.blockWidth + config.roadWidth);

            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "City_Root",
                position = new[] { 0f, 0f, 0f }
            });

            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "City_Asphalt_Ground",
                primitiveType = "Plane",
                scale = new[] { totalWidth / 10f, 1f, totalHeight / 10f },
                position = new[] { 0f, 0f, 0f },
                parent = "City_Root"
            });

            // 2. Generate Grid Blocks and Buildings
            int buildingId = 1;
            float startX = -(totalWidth / 2f) + (config.blockWidth / 2f);
            float startZ = -(totalHeight / 2f) + (config.blockWidth / 2f);

            for (int x = 0; x < config.gridWidth; x++)
            {
                for (int z = 0; z < config.gridHeight; z++)
                {
                    float posX = startX + x * (config.blockWidth + config.roadWidth);
                    float posZ = startZ + z * (config.blockWidth + config.roadWidth);

                    float buildingHeight = (float)(config.minBuildingHeight + random.NextDouble() * (config.maxBuildingHeight - config.minBuildingHeight));
                    string buildingName = $"Building_B{buildingId++}_Seed{config.seed}";

                    batchReq.actions.Add(new BatchActionItem
                    {
                        action = "gameobject_create",
                        name = buildingName,
                        primitiveType = "Cube",
                        scale = new[] { config.blockWidth * 0.85f, buildingHeight, config.blockWidth * 0.85f },
                        position = new[] { posX, buildingHeight / 2f, posZ },
                        parent = "City_Root"
                    });
                }
            }

            // 3. Player Spawn Point
            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "Player_Spawn_Anchor",
                position = new[] { 0f, 0.5f, 0f },
                parent = "City_Root"
            });

            return BatchExecutor.ExecuteBatch(batchReq, r => UnityMcpBridge.ExecuteAction(r));
        }
    }
}
