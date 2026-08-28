using System;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Validation;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Transaction
{
    public static class BatchExecutor
    {
        public static McpResponse ExecuteBatch(BatchRequestDto request, Func<BridgeRequest, McpResponse> singleActionRunner)
        {
            if (request == null || request.actions == null || request.actions.Count == 0)
            {
                return McpResponse.Error("Empty batch request.");
            }

            string txId = TransactionManager.BeginTransaction(request.transactionId);
            var executedResults = new List<string>();
            var errors = new List<string>();

            try
            {
                for (int i = 0; i < request.actions.Count; i++)
                {
                    var item = request.actions[i];
                    var bridgeReq = MapBatchItemToBridgeRequest(item);

                    var res = singleActionRunner(bridgeReq);
                    if (!res.success)
                    {
                        string errMsg = $"Action #{i + 1} ({item.action}) failed: {res.error}";
                        errors.Add(errMsg);

                        if (request.autoRollbackOnError)
                        {
                            TransactionManager.RollbackTransaction(txId);
                            return McpResponse.Error($"Batch aborted at step {i + 1}. Rolled back transaction '{txId}'.", string.Join("\n", errors), errors, txId);
                        }
                    }
                    else
                    {
                        executedResults.Add($"Step {i + 1} ({item.action}): {res.message}");
                    }
                }

                // Run validation
                var valReport = ValidationManager.ValidateScene();
                if (!valReport.isValid && request.autoRollbackOnError)
                {
                    TransactionManager.RollbackTransaction(txId);
                    return McpResponse.Error($"Batch validation failed with {valReport.errorCount} errors. Rolled back transaction '{txId}'.", JsonUtility.ToJson(valReport, true), null, txId);
                }

                TransactionManager.CommitTransaction(txId);
                return McpResponse.Success($"Batch executed successfully ({executedResults.Count} actions).", string.Join("\n", executedResults), txId);
            }
            catch (Exception ex)
            {
                if (request.autoRollbackOnError)
                {
                    TransactionManager.RollbackTransaction(txId);
                }
                return McpResponse.Error($"Batch fatal exception: {ex.Message}", null, null, txId);
            }
        }

        private static BridgeRequest MapBatchItemToBridgeRequest(BatchActionItem item)
        {
            return new BridgeRequest
            {
                action = item.action,
                target = item.target,
                name = item.name,
                path = item.path,
                content = item.content,
                code = item.code,
                primitiveType = item.primitiveType,
                componentType = item.componentType,
                propertyName = item.propertyName,
                propertyValue = item.propertyValue,
                shaderName = item.shaderName,
                colorHex = item.colorHex,
                filter = item.filter,
                colliderType = item.colliderType,
                renderMode = item.renderMode,
                elementType = item.elementType,
                text = item.text,
                motionPath = item.motionPath,
                paramType = item.paramType,
                mass = item.mass,
                drag = item.drag,
                angularDrag = item.angularDrag,
                useGravity = item.useGravity,
                isKinematic = item.isKinematic,
                isTrigger = item.isTrigger,
                posX = item.posX,
                posY = item.posY,
                active = item.active,
                position = item.position,
                rotation = item.rotation,
                scale = item.scale,
                center = item.center,
                size = item.size,
                parent = item.parent,
                tag = item.tag,
                layer = item.layer
            };
        }
    }
}
