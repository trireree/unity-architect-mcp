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
    public class LiveIdeContextDto
    {
        public string activeScene;
        public bool isCompiling;
        public bool isPlaying;
        public string selectedObjectName;
        public string selectedObjectId;
        public Vector3 selectedObjectPosition;
        public string[] selectedObjectComponents;
        public int sceneObjectCount;
    }

    public static class BiDirectionalIdeSyncHandler
    {
        public static McpResponse GetLiveEditorContext()
        {
            var activeGo = Selection.activeGameObject;
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            var dto = new LiveIdeContextDto
            {
                activeScene = !string.IsNullOrEmpty(activeScene.name) ? activeScene.name : "Untitled Scene",
                isCompiling = EditorApplication.isCompiling,
                isPlaying = EditorApplication.isPlaying,
                selectedObjectName = activeGo != null ? activeGo.name : "None",
                selectedObjectId = activeGo != null ? EntityIdHelper.GetIdString(activeGo) : "",
                selectedObjectPosition = activeGo != null ? activeGo.transform.position : Vector3.zero,
                selectedObjectComponents = activeGo != null ? activeGo.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name).ToArray() : new string[0],
                sceneObjectCount = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length
            };

            return McpResponse.Success("Live Unity Editor context streamed.", JsonUtility.ToJson(dto, true));
        }
    }
}
