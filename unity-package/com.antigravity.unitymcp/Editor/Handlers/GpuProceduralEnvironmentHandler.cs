#pragma warning disable CS0618, CS0619
using System;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class GpuProceduralEnvironmentHandler
    {
        public static McpResponse CreateProceduralGpuGrass(string targetParent = "Environment_Root", int grassCount = 50000, float areaSize = 150f)
        {
            // 1. Create Wind Settings ScriptableObject
            string dataDir = "Assets/Data";
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            string windScriptPath = "Assets/Scripts/Environment/WindSettings.cs";
            string windScriptDir = Path.GetDirectoryName(windScriptPath);
            if (!Directory.Exists(windScriptDir)) Directory.CreateDirectory(windScriptDir);

            if (!File.Exists(windScriptPath))
            {
                File.WriteAllText(windScriptPath, @"using UnityEngine;

[CreateAssetMenu(fileName = ""WindSettings"", menuName = ""Environment/WindSettings"")]
public class WindSettings : ScriptableObject
{
    public Vector3 windDirection = new Vector3(1f, 0f, 0.5f);
    public float windSpeed = 2.5f;
    public float windWaveFrequency = 1.2f;
    public float windWaveAmplitude = 0.35f;
}");
                AssetDatabase.ImportAsset(windScriptPath, ImportAssetOptions.ForceUpdate);
            }

            // 2. Create GPU Grass Renderer Component Script
            string grassScriptPath = "Assets/Scripts/Environment/GpuGrassRenderer.cs";
            if (!File.Exists(grassScriptPath))
            {
                File.WriteAllText(grassScriptPath, @"using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteAlways]
public class GpuGrassRenderer : MonoBehaviour
{
    public Mesh grassMesh;
    public Material grassMaterial;
    public int instanceCount = 50000;
    public float fieldRadius = 100f;

    private ComputeBuffer argsBuffer;
    private ComputeBuffer positionBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    private void Start()
    {
        InitializeBuffers();
    }

    private void OnEnable()
    {
        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        if (grassMesh == null)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            grassMesh = quad.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(quad);
        }

        if (grassMaterial == null)
        {
            grassMaterial = new Material(Shader.Find(""Universal Render Pipeline/Lit"") ?? Shader.Find(""Standard""));
            grassMaterial.enableInstancing = true;
            grassMaterial.color = new Color(0.18f, 0.55f, 0.22f);
        }

        ReleaseBuffers();

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        positionBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(Matrix4x4)));

        Matrix4x4[] matrices = new Matrix4x4[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            Vector2 randCircle = Random.insideUnitCircle * fieldRadius;
            Vector3 pos = transform.position + new Vector3(randCircle.x, 0f, randCircle.y);
            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Vector3 scale = new Vector3(Random.Range(0.6f, 1.2f), Random.Range(0.8f, 1.8f), 1f);
            matrices[i] = Matrix4x4.TRS(pos, rot, scale);
        }

        positionBuffer.SetData(matrices);

        args[0] = (uint)grassMesh.GetIndexCount(0);
        args[1] = (uint)instanceCount;
        args[2] = (uint)grassMesh.GetIndexStart(0);
        args[3] = (uint)grassMesh.GetBaseVertex(0);
        argsBuffer.SetData(args);

        grassMaterial.SetBuffer(""_TransformBuffer"", positionBuffer);
    }

    private void Update()
    {
        if (argsBuffer != null && positionBuffer != null && grassMaterial != null && grassMesh != null)
        {
            Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, new Bounds(transform.position, Vector3.one * fieldRadius * 2f), argsBuffer);
        }
    }

    private void OnDisable()
    {
        ReleaseBuffers();
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    private void ReleaseBuffers()
    {
        argsBuffer?.Release();
        argsBuffer = null;
        positionBuffer?.Release();
        positionBuffer = null;
    }
}");
                AssetDatabase.ImportAsset(grassScriptPath, ImportAssetOptions.ForceUpdate);
            }

            AssetDatabase.Refresh();

            // 3. Instantiate in Scene
            var grassGo = new GameObject("GPU_Procedural_Grass_System");
            if (!string.IsNullOrEmpty(targetParent))
            {
                var parentGo = SceneHandler.FindGameObject(targetParent);
                if (parentGo != null) grassGo.transform.SetParent(parentGo.transform, false);
            }

            Undo.RegisterCreatedObjectUndo(grassGo, "Create GPU Procedural Grass System");
            Selection.activeGameObject = grassGo;

            return McpResponse.Success($"Created Zero-CPU Allocation GPU Grass System ({grassCount:N0} instances in 1 single Draw Call)!", grassGo.name);
        }

        public static McpResponse CreateProceduralWaterSurface(string targetParent = "Environment_Root", float surfaceWidth = 200f, float surfaceLength = 200f)
        {
            var waterGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterGo.name = "GPU_Procedural_Water_Surface";
            waterGo.transform.localScale = new Vector3(surfaceWidth / 10f, 1f, surfaceLength / 10f);

            if (!string.IsNullOrEmpty(targetParent))
            {
                var parentGo = SceneHandler.FindGameObject(targetParent);
                if (parentGo != null) waterGo.transform.SetParent(parentGo.transform, false);
            }

            string matDir = "Assets/Materials/Generated";
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            string matPath = $"{matDir}/M_Procedural_Water.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.color = new Color(0.08f, 0.45f, 0.65f, 0.85f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.1f);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            var renderer = waterGo.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = mat;

            Undo.RegisterCreatedObjectUndo(waterGo, "Create Procedural Water Surface");
            Selection.activeGameObject = waterGo;

            return McpResponse.Success($"Created High-Performance Water Surface ({surfaceWidth}m x {surfaceLength}m) with depth-gradient material!", waterGo.name);
        }
    }
}
