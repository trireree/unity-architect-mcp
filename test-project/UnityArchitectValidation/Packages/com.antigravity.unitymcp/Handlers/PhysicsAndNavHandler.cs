#pragma warning disable CS0618, CS0619
using System;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class PhysicsAndNavHandler
    {
        public static McpResponse SetupRigidbody(string targetGameObject, float mass, float drag, float angularDrag, bool useGravity, bool isKinematic)
        {
            var go = SceneHandler.FindGameObject(targetGameObject);
            if (go == null) return McpResponse.Error($"GameObject '{targetGameObject}' not found.");

            var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
            Undo.RecordObject(rb, "Setup Rigidbody");

            rb.mass = mass;
            rb.linearDamping = drag;
            rb.angularDamping = angularDrag;
            rb.useGravity = useGravity;
            rb.isKinematic = isKinematic;

            return McpResponse.Success($"Configured Rigidbody on '{go.name}'.");
        }

        public static McpResponse SetupCollider(string targetGameObject, string colliderType, bool isTrigger, float[] center, float[] size)
        {
            var go = SceneHandler.FindGameObject(targetGameObject);
            if (go == null) return McpResponse.Error($"GameObject '{targetGameObject}' not found.");

            Collider col = null;
            switch (colliderType?.ToLowerInvariant())
            {
                case "box":
                    var box = go.GetComponent<BoxCollider>() ?? go.AddComponent<BoxCollider>();
                    if (center != null && center.Length == 3) box.center = new Vector3(center[0], center[1], center[2]);
                    if (size != null && size.Length == 3) box.size = new Vector3(size[0], size[1], size[2]);
                    col = box;
                    break;
                case "sphere":
                    var sphere = go.GetComponent<SphereCollider>() ?? go.AddComponent<SphereCollider>();
                    if (center != null && center.Length == 3) sphere.center = new Vector3(center[0], center[1], center[2]);
                    if (size != null && size.Length > 0) sphere.radius = size[0];
                    col = sphere;
                    break;
                case "capsule":
                    var cap = go.GetComponent<CapsuleCollider>() ?? go.AddComponent<CapsuleCollider>();
                    if (center != null && center.Length == 3) cap.center = new Vector3(center[0], center[1], center[2]);
                    if (size != null && size.Length >= 2)
                    {
                        cap.radius = size[0];
                        cap.height = size[1];
                    }
                    col = cap;
                    break;
                case "mesh":
                    var mesh = go.GetComponent<MeshCollider>() ?? go.AddComponent<MeshCollider>();
                    col = mesh;
                    break;
                default:
                    col = go.GetComponent<Collider>() ?? go.AddComponent<BoxCollider>();
                    break;
            }

            Undo.RecordObject(col, "Setup Collider");
            col.isTrigger = isTrigger;

            return McpResponse.Success($"Configured {col.GetType().Name} on '{go.name}'.");
        }

        public static McpResponse BakeNavMesh()
        {
            try
            {
                UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
                return McpResponse.Success("NavMesh baked successfully.");
            }
            catch (Exception ex)
            {
                return McpResponse.Error($"Failed to bake NavMesh: {ex.Message}");
            }
        }
    }
}
