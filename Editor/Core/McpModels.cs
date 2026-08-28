#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;

namespace Antigravity.UnityMCP.Editor.Core
{
    [Serializable]
    public class McpRequest
    {
        public string action;
        public string payload;
    }

    [Serializable]
    public class McpResponse
    {
        public bool success;
        public string message;
        public string data;
        public string error;
        public string transactionId;
        public List<string> errors;
        public List<string> warnings;

        public static McpResponse Success(string message, string data = null, string txId = null)
        {
            return new McpResponse { success = true, message = message, data = data, transactionId = txId };
        }

        public static McpResponse Error(string error, string data = null, List<string> errors = null, string txId = null)
        {
            return new McpResponse { success = false, error = error, data = data, errors = errors, transactionId = txId };
        }
    }

    [Serializable]
    public class LogEntry
    {
        public string type;
        public string condition;
        public string stackTrace;
        public string timestamp;
    }

    [Serializable]
    public class HierarchyNode
    {
        public int instanceId;
        public string stableId;
        public string name;
        public string tag;
        public string layer;
        public bool activeSelf;
        public bool activeInHierarchy;
        public float[] position;
        public float[] rotation;
        public float[] scale;
        public List<string> components = new List<string>();
        public List<HierarchyNode> children = new List<HierarchyNode>();
    }

    public enum GraphNodeType
    {
        PROJECT,
        SCENE,
        GAMEOBJECT,
        COMPONENT,
        PREFAB,
        SCRIPT,
        MATERIAL,
        TEXTURE,
        MESH,
        ANIMATION,
        AUDIO,
        PACKAGE
    }

    public enum GraphRelationType
    {
        CONTAINS,
        HAS_COMPONENT,
        REFERENCES,
        DEPENDS_ON,
        INSTANTIATES,
        USES_MATERIAL,
        USES_TEXTURE,
        USES_SCRIPT,
        PARENT_OF
    }

    [Serializable]
    public class GraphNodeDto
    {
        public string id;
        public string name;
        public string type;
        public string path;
        public string hash;
        public List<string> metadataList = new List<string>();
    }

    [Serializable]
    public class GraphEdgeDto
    {
        public string sourceId;
        public string targetId;
        public string relation;
    }

    [Serializable]
    public class ProjectSummaryDto
    {
        public string projectName;
        public string unityVersion;
        public int sceneCount;
        public int gameObjectCount;
        public int scriptCount;
        public int prefabCount;
        public int materialCount;
        public int textureCount;
        public int compileErrors;
        public int warnings;
        public string currentHash;
        public string activeScene;
        public List<string> keyObjects = new List<string>();
    }

    [Serializable]
    public class StateDiffDto
    {
        public string previousHash;
        public string currentHash;
        public int addedCount;
        public int removedCount;
        public int modifiedCount;
        public int unchangedCount;
        public List<string> added = new List<string>();
        public List<string> removed = new List<string>();
        public List<string> modified = new List<string>();
    }

    [Serializable]
    public class ValidationErrorDto
    {
        public string type;
        public string target;
        public string message;
        public string severity;
    }

    [Serializable]
    public class ValidationReportDto
    {
        public bool isValid;
        public int errorCount;
        public int warningCount;
        public List<ValidationErrorDto> issues = new List<ValidationErrorDto>();
    }

    [Serializable]
    public class BatchActionItem
    {
        public string action;
        public string target;
        public string name;
        public string path;
        public string content;
        public string code;
        public string primitiveType;
        public string componentType;
        public string propertyName;
        public string propertyValue;
        public string shaderName;
        public string colorHex;
        public string filter;
        public string colliderType;
        public string renderMode;
        public string elementType;
        public string text;
        public string motionPath;
        public string paramType;
        public int width = 160;
        public int height = 30;
        public float mass = 1f;
        public float drag = 0f;
        public float angularDrag = 0.05f;
        public bool useGravity = true;
        public bool isKinematic = false;
        public bool isTrigger = false;
        public float posX;
        public float posY;
        public bool? active;
        public float[] position;
        public float[] rotation;
        public float[] scale;
        public float[] center;
        public float[] size;
        public string parent;
        public string tag;
        public string layer;
    }

    [Serializable]
    public class BatchRequestDto
    {
        public string transactionId;
        public bool autoRollbackOnError = true;
        public List<BatchActionItem> actions = new List<BatchActionItem>();
    }
}
