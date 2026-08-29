#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [Serializable]
    public class RaycastHitResultDto
    {
        public bool hasHit;
        public Vector3 point;
        public Vector3 normal;
        public float distance;
        public string hitObjectName;
        public int hitLayer;
        public string hitTag;
    }

    [Serializable]
    public class SpatialContextDto
    {
        public string targetObject;
        public Vector3 position;
        public Vector3 boundingBoxSize;
        public List<NearbyObjectDto> nearbyObjects = new List<NearbyObjectDto>();
    }

    [Serializable]
    public class NearbyObjectDto
    {
        public string name;
        public float distance;
        public Vector3 direction;
        public bool hasLineOfSight;
    }

    public static class SpatialAndPhysicsProbeHandler
    {
        public static McpResponse Raycast(Vector3 origin, Vector3 direction, float maxDistance = 100f, int layerMask = ~0)
        {
            var dto = new RaycastHitResultDto();
            Ray ray = new Ray(origin, direction.normalized);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
                dto.hasHit = true;
                dto.point = hit.point;
                dto.normal = hit.normal;
                dto.distance = hit.distance;
                dto.hitObjectName = hit.collider.gameObject.name;
                dto.hitLayer = hit.collider.gameObject.layer;
                dto.hitTag = hit.collider.gameObject.tag;
                return McpResponse.Success($"Raycast HIT object '{dto.hitObjectName}' at distance {dto.distance:F2}m.", JsonUtility.ToJson(dto, true));
            }

            dto.hasHit = false;
            return McpResponse.Success($"Raycast MISSED (no colliders hit within {maxDistance}m).", JsonUtility.ToJson(dto, true));
        }

        public static McpResponse OverlapSphere(Vector3 center, float radius, int layerMask = ~0)
        {
            var colliders = Physics.OverlapSphere(center, radius, layerMask, QueryTriggerInteraction.Collide);
            var names = colliders.Select(c => c.gameObject.name).Distinct().ToList();
            return McpResponse.Success($"OverlapSphere at {center} (radius {radius}m) detected {colliders.Length} colliders.", JsonUtility.ToJson(new { count = colliders.Length, hitObjects = names }, true));
        }

        public static McpResponse GetSpatialContext(string targetObject, float searchRadius = 30f)
        {
            var go = SceneHandler.FindGameObject(targetObject);
            if (go == null) return McpResponse.Error($"Target GameObject '{targetObject}' not found.");

            var dto = new SpatialContextDto
            {
                targetObject = go.name,
                position = go.transform.position
            };

            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                dto.boundingBoxSize = bounds.size;
            }

            var allGos = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var other in allGos)
            {
                if (other == go || other.transform.IsChildOf(go.transform) || other.transform.parent != null) continue;
                float dist = Vector3.Distance(go.transform.position, other.transform.position);
                if (dist <= searchRadius)
                {
                    bool hasLos = !Physics.Linecast(go.transform.position + Vector3.up * 1.5f, other.transform.position + Vector3.up * 1.5f, ~0, QueryTriggerInteraction.Ignore);
                    dto.nearbyObjects.Add(new NearbyObjectDto
                    {
                        name = other.name,
                        distance = dist,
                        direction = (other.transform.position - go.transform.position).normalized,
                        hasLineOfSight = hasLos
                    });
                }
            }

            dto.nearbyObjects = dto.nearbyObjects.OrderBy(o => o.distance).Take(15).ToList();
            return McpResponse.Success($"Harvested spatial context for '{go.name}' ({dto.nearbyObjects.Count} nearby objects).", JsonUtility.ToJson(dto, true));
        }
    }
}
