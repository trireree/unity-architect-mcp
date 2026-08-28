using System;
using System.Collections.Generic;
using System.Linq;

namespace Antigravity.UnityMCP.Editor.Knowledge
{
    [Serializable]
    public class KnowledgeSnippet
    {
        public string topic;
        public string category;
        public string[] tags;
        public string summary;
        public string codeExample;
        public string bestPractice;
    }

    public static class UnityKnowledgeIndex
    {
        private static readonly List<KnowledgeSnippet> Snippets = new List<KnowledgeSnippet>();

        static UnityKnowledgeIndex()
        {
            InitializeKnowledgeBase();
        }

        private static void InitializeKnowledgeBase()
        {
            // 1. WheelCollider & Vehicle Physics
            Snippets.Add(new KnowledgeSnippet
            {
                topic = "WheelCollider Vehicle Physics",
                category = "Physics",
                tags = new[] { "vehicle", "car", "wheel", "physics", "rigidbody", "wheelcollider" },
                summary = "WheelColliders require a parent Rigidbody with low center of mass (centerOfMass = new Vector3(0, -0.5f, 0)). Never attach visual mesh directly to WheelCollider; update visual wheel rotation/position in Update using wheelCollider.GetWorldPose.",
                bestPractice = "Configure motorTorque on drive wheels and steerAngle on front wheels. Apply brakeTorque to all wheels when stopping.",
                codeExample = "wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);\nwheelTransform.position = pos;\nwheelTransform.rotation = rot;"
            });

            // 2. Modern Unity C# API Standards
            Snippets.Add(new KnowledgeSnippet
            {
                topic = "Modern Unity Scripting API (2022+ / 2023 / 6)",
                category = "Scripting",
                tags = new[] { "api", "findobject", "getcomponent", "performance", "csharp" },
                summary = "Replace deprecated 'FindObjectOfType' with 'Object.FindFirstObjectByType<T>()' or 'Object.FindAnyObjectByType<T>()'. Always use 'TryGetComponent<T>(out var comp)' to prevent GC allocations.",
                bestPractice = "Cache all component references in Awake/Start. Never call FindFirstObjectByType in Update() loop.",
                codeExample = "if (TryGetComponent<Rigidbody>(out var rb)) {\n    rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);\n}"
            });

            // 3. Universal Render Pipeline (URP) Material & Shader
            Snippets.Add(new KnowledgeSnippet
            {
                topic = "URP Material Configuration",
                category = "Rendering",
                tags = new[] { "urp", "shader", "material", "color", "texture", "lighting" },
                summary = "In URP Lit shader, the main color property is '_BaseColor' (not '_Color') and main texture is '_BaseMap' (not '_MainTex').",
                bestPractice = "Use Shader.Find('Universal Render Pipeline/Lit') for standard objects. If URP is not installed, fallback to Shader.Find('Standard').",
                codeExample = "Material mat = new Material(Shader.Find(\"Universal Render Pipeline/Lit\"));\nmat.SetColor(\"_BaseColor\", Color.red);\nmat.SetFloat(\"_Smoothness\", 0.5f);"
            });

            // 4. CharacterController Grounding & Movement
            Snippets.Add(new KnowledgeSnippet
            {
                topic = "CharacterController Jump and Gravity",
                category = "Gameplay",
                tags = new[] { "player", "charactercontroller", "jump", "gravity", "movement" },
                summary = "CharacterController.isGrounded can be jittery when falling. Keep velocity.y at -2f when grounded. Calculate jump velocity as Mathf.Sqrt(jumpHeight * -2f * gravity).",
                bestPractice = "Call controller.Move(velocity * Time.deltaTime) once per frame at the end of Update.",
                codeExample = "if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;\nif (Input.GetButtonDown(\"Jump\") && controller.isGrounded) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);\nvelocity.y += gravity * Time.deltaTime;\ncontroller.Move(velocity * Time.deltaTime);"
            });

            // 5. NavMesh Navigation & Obstacles
            Snippets.Add(new KnowledgeSnippet
            {
                topic = "NavMeshAgent AI Pathfinding",
                category = "AI",
                tags = new[] { "navmesh", "agent", "pathfinding", "ai", "enemy", "patrol" },
                summary = "NavMeshAgent requires baked NavMesh data. For moving objects that block enemies, attach NavMeshObstacle with 'Carve = true'. Check remainingDistance <= stoppingDistance to detect destination reached.",
                bestPractice = "Set agent.updateRotation = true for automatic facing, or false if controlling rotation via Animator root motion.",
                codeExample = "agent.SetDestination(target.position);\nif (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) {\n    // Reached destination\n}"
            });

            // 6. Object Pooling & Memory Optimization
            Snippets.Add(new KnowledgeSnippet
            {
                topic = "Object Pooling Pattern",
                category = "Optimization",
                tags = new[] { "pool", "objectpool", "memory", "gc", "bullets", "particles" },
                summary = "Never use Instantiate / Destroy repeatedly during combat for bullets, projectiles, or particle effects. Use UnityEngine.Pool.ObjectPool<GameObject> (built into Unity 2021+).",
                bestPractice = "Pre-warm the pool with initial capacity and release objects back to pool on collision/lifetime expiry.",
                codeExample = "var pool = new UnityEngine.Pool.ObjectPool<GameObject>(\n    createFunc: () => Instantiate(prefab),\n    actionOnGet: go => go.SetActive(true),\n    actionOnRelease: go => go.SetActive(false),\n    actionOnDestroy: Destroy,\n    defaultCapacity: 20\n);"
            });
        }

        public static List<KnowledgeSnippet> SearchKnowledge(string query, string category = null)
        {
            if (string.IsNullOrEmpty(query)) return Snippets.Take(5).ToList();

            var terms = query.ToLowerInvariant().Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);

            return Snippets.Where(s =>
                (string.IsNullOrEmpty(category) || s.category.Equals(category, StringComparison.OrdinalIgnoreCase)) &&
                (terms.Any(t => s.topic.ToLowerInvariant().Contains(t) || s.tags.Any(tag => tag.Contains(t)) || s.summary.ToLowerInvariant().Contains(t)))
            ).ToList();
        }
    }
}
