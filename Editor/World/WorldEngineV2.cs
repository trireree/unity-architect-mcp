#pragma warning disable CS0618, CS0619
using System;
using Antigravity.UnityMCP.Editor.City;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Handlers;
using Antigravity.UnityMCP.Editor.Transaction;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.World
{
    [Serializable]
    public class WorldGenerationConfig
    {
        public int seed = 48392;
        public string theme = "modern_urban"; // "modern_urban", "coastal", "industrial"
        public int districts = 4;
        public int districtSize = 3;
        public float blockSize = 24f;
        public float roadWidth = 10f;
    }

    public static class WorldEngineV2
    {
        public static McpResponse GenerateFullWorld(WorldGenerationConfig config = null)
        {
            config = config ?? new WorldGenerationConfig();
            var random = new System.Random(config.seed);

            var batchReq = new BatchRequestDto
            {
                transactionId = $"world_gen_{config.seed}_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                autoRollbackOnError = true
            };

            float fullDimension = config.districtSize * (config.blockSize + config.roadWidth) * 2;

            // 1. World Root
            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "World_Root",
                position = new[] { 0f, 0f, 0f }
            });

            // 2. Asphalt Road Ground
            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "World_Ground_Asphalt",
                primitiveType = "Plane",
                scale = new[] { fullDimension / 10f, 1f, fullDimension / 10f },
                position = new[] { 0f, 0f, 0f },
                parent = "World_Root"
            });

            // 3. Generate Districts and Diverse Buildings
            int buildingIdx = 1;
            float halfDim = fullDimension / 2f;
            float step = config.blockSize + config.roadWidth;

            for (float x = -halfDim + step / 2f; x < halfDim; x += step)
            {
                for (float z = -halfDim + step / 2f; z < halfDim; z += step)
                {
                    float height = (float)(10f + random.NextDouble() * 35f);
                    string bName = $"Building_D{(int)((x + halfDim) / step)}_{buildingIdx++}";

                    batchReq.actions.Add(new BatchActionItem
                    {
                        action = "gameobject_create",
                        name = bName,
                        primitiveType = "Cube",
                        scale = new[] { config.blockSize * 0.82f, height, config.blockSize * 0.82f },
                        position = new[] { x, height / 2f, z },
                        parent = "World_Root"
                    });
                }
            }

            // 4. Player & Traffic Spawners
            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "Player_Spawn_Zone",
                position = new[] { 0f, 1f, 0f },
                parent = "World_Root"
            });

            batchReq.actions.Add(new BatchActionItem
            {
                action = "gameobject_create",
                name = "Traffic_Network_Root",
                position = new[] { 0f, 0.5f, 0f },
                parent = "World_Root"
            });

            return BatchExecutor.ExecuteBatch(batchReq, r => UnityMcpBridge.ExecuteAction(r));
        }
    }
}
