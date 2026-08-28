using System;
using System.IO;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.State;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Antigravity.UnityMCP.Editor.Graph
{
    public static class ProjectGraphBuilder
    {
        private static ProjectStateGraph _cachedGraph;

        public static ProjectStateGraph GetOrBuildGraph(bool forceRebuild = false)
        {
            if (_cachedGraph != null && !forceRebuild)
            {
                return _cachedGraph;
            }

            _cachedGraph = BuildFullGraph();
            return _cachedGraph;
        }

        public static ProjectStateGraph BuildFullGraph()
        {
            var graph = new ProjectStateGraph();
            var activeScene = SceneManager.GetActiveScene();

            // 1. Scene Node
            string sceneNodeId = string.IsNullOrEmpty(activeScene.path) ? "scene_active" : activeScene.path;
            graph.AddNode(new GraphNodeDto
            {
                id = sceneNodeId,
                name = string.IsNullOrEmpty(activeScene.name) ? "UntitledScene" : activeScene.name,
                type = GraphNodeType.SCENE.ToString(),
                path = activeScene.path,
                hash = StateHasher.ComputeHash(activeScene.name + activeScene.path)
            });

            // 2. GameObjects and Components in Scene
            var rootObjects = activeScene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                TraverseGameObject(graph, root, sceneNodeId, null);
            }

            // 3. Project Scripts and Asset Indexing
            var scriptGuids = AssetDatabase.FindAssets("t:MonoScript");
            foreach (var guid in scriptGuids)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                if (scriptPath.StartsWith("Assets/"))
                {
                    string scriptName = Path.GetFileNameWithoutExtension(scriptPath);
                    string scriptId = $"guid:{guid}";
                    graph.AddNode(new GraphNodeDto
                    {
                        id = scriptId,
                        name = scriptName,
                        type = GraphNodeType.SCRIPT.ToString(),
                        path = scriptPath,
                        hash = StateHasher.ComputeFileHash(scriptPath)
                    });
                }
            }

            // 4. Prefabs
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                if (prefabPath.StartsWith("Assets/"))
                {
                    string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
                    string prefabId = $"guid:{guid}";
                    graph.AddNode(new GraphNodeDto
                    {
                        id = prefabId,
                        name = prefabName,
                        type = GraphNodeType.PREFAB.ToString(),
                        path = prefabPath,
                        hash = StateHasher.ComputeFileHash(prefabPath)
                    });
                }
            }

            graph.ComputeAndCacheGraphHash();
            ProjectStateGraph.SaveToDisk(graph);

            return graph;
        }

        private static void TraverseGameObject(ProjectStateGraph graph, GameObject go, string sceneNodeId, string parentNodeId)
        {
            if (go == null) return;

            string stableId = GetStableId(go);
            string goHash = StateHasher.ComputeGameObjectHash(go);

            graph.AddNode(new GraphNodeDto
            {
                id = stableId,
                name = go.name,
                type = GraphNodeType.GAMEOBJECT.ToString(),
                path = GetHierarchyPath(go),
                hash = goHash
            });

            if (!string.IsNullOrEmpty(parentNodeId))
            {
                graph.AddEdge(parentNodeId, stableId, GraphRelationType.PARENT_OF);
            }
            else
            {
                graph.AddEdge(sceneNodeId, stableId, GraphRelationType.CONTAINS);
            }

            // Components
            var components = go.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp == null) continue;

                string compName = comp.GetType().Name;
                string compId = $"{stableId}#{compName}_{EntityIdHelper.GetIdString(comp)}";

                graph.AddNode(new GraphNodeDto
                {
                    id = compId,
                    name = compName,
                    type = GraphNodeType.COMPONENT.ToString(),
                    path = $"{GetHierarchyPath(go)}/{compName}",
                    hash = StateHasher.ComputeHash(compName + EntityIdHelper.GetIdString(comp))
                });

                graph.AddEdge(stableId, compId, GraphRelationType.HAS_COMPONENT);
            }

            // Child GameObjects
            for (int i = 0; i < go.transform.childCount; i++)
            {
                TraverseGameObject(graph, go.transform.GetChild(i).gameObject, sceneNodeId, stableId);
            }
        }

        public static string GetStableId(GameObject go)
        {
            if (go == null) return string.Empty;

            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(go);
            if (!string.IsNullOrEmpty(globalId.ToString()) && !globalId.ToString().Contains("00000000000000000000000000000000"))
            {
                return globalId.ToString();
            }

            string scenePath = go.scene.path;
            if (string.IsNullOrEmpty(scenePath)) scenePath = "ActiveScene";
            return $"{scenePath}:{GetHierarchyPath(go)}";
        }

        public static string GetHierarchyPath(GameObject go)
        {
            if (go == null) return string.Empty;
            string path = go.name;
            Transform current = go.transform.parent;
            while (current != null)
            {
                path = $"{current.name}/{path}";
                current = current.parent;
            }
            return path;
        }

        public static ProjectSummaryDto BuildSummary()
        {
            var graph = GetOrBuildGraph(false);
            var activeScene = SceneManager.GetActiveScene();

            var summary = new ProjectSummaryDto
            {
                projectName = Application.productName,
                unityVersion = Application.unityVersion,
                activeScene = string.IsNullOrEmpty(activeScene.name) ? "UntitledScene" : activeScene.name,
                currentHash = graph.graphHash,
                gameObjectCount = graph.nodes.Values.Count(n => n.type == GraphNodeType.GAMEOBJECT.ToString()),
                scriptCount = graph.nodes.Values.Count(n => n.type == GraphNodeType.SCRIPT.ToString()),
                prefabCount = graph.nodes.Values.Count(n => n.type == GraphNodeType.PREFAB.ToString()),
                materialCount = graph.nodes.Values.Count(n => n.type == GraphNodeType.MATERIAL.ToString()),
                textureCount = graph.nodes.Values.Count(n => n.type == GraphNodeType.TEXTURE.ToString()),
                compileErrors = EditorUtility.scriptCompilationFailed ? 1 : 0
            };

            var roots = activeScene.GetRootGameObjects();
            summary.keyObjects = roots.Take(10).Select(r => r.name).ToList();

            return summary;
        }
    }
}
