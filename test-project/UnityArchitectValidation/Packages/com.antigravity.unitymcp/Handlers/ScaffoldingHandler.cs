#pragma warning disable CS0618, CS0619
using System;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class ScaffoldingHandler
    {
        public static McpResponse ScaffoldThirdPersonPlayer(string characterName = "PlayerCharacter")
        {
            // Idempotency: Check if player already exists in scene
            var existing = SceneHandler.FindGameObject(characterName);
            GameObject player = existing;

            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = characterName;
                player.transform.position = new Vector3(0, 1, 0);

                var col = player.GetComponent<Collider>();
                if (col != null) UnityEngine.Object.DestroyImmediate(col);

                var cc = player.AddComponent<CharacterController>();
                cc.height = 2f;
                cc.radius = 0.5f;
                cc.center = new Vector3(0, 0, 0);

                Undo.RegisterCreatedObjectUndo(player, "Scaffold Player");
            }

            // Write and attach PlayerController script
            string scriptPath = "Assets/Scripts/PlayerController.cs";
            string scriptContent = @"using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxisRaw(""Horizontal"");
        float vertical = Input.GetAxisRaw(""Vertical"");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + (cameraTransform ? cameraTransform.eulerAngles.y : 0);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        if (Input.GetButtonDown(""Jump"") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}";
            ScriptAndCompilationHandler.CreateOrUpdateScript(scriptPath, scriptContent);

            Selection.activeGameObject = player;
            return McpResponse.Success($"Scaffolded Third Person Player '{player.name}' (Idempotent reuse: {existing != null}) and created '{scriptPath}'.");
        }

        public static McpResponse ScaffoldEnemyAI(string enemyName = "EnemyAI")
        {
            var existing = SceneHandler.FindGameObject(enemyName);
            GameObject enemy = existing;

            if (enemy == null)
            {
                enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = enemyName;
                enemy.transform.position = new Vector3(5, 1, 5);

                var agent = enemy.AddComponent<NavMeshAgent>();
                agent.speed = 3.5f;

                Undo.RegisterCreatedObjectUndo(enemy, "Scaffold Enemy AI");
            }

            string scriptPath = "Assets/Scripts/EnemyAI.cs";
            string scriptContent = @"using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public Transform targetPlayer;
    public float chaseRadius = 15f;
    public float attackRadius = 2f;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (targetPlayer == null)
        {
            var p = GameObject.FindWithTag(""Player"");
            if (p != null) targetPlayer = p.transform;
        }
    }

    void Update()
    {
        if (targetPlayer == null) return;

        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        if (distance <= chaseRadius)
        {
            agent.SetDestination(targetPlayer.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}";
            ScriptAndCompilationHandler.CreateOrUpdateScript(scriptPath, scriptContent);

            Selection.activeGameObject = enemy;
            return McpResponse.Success($"Scaffolded Enemy AI '{enemy.name}' (Idempotent reuse: {existing != null}) with NavMeshAgent and created '{scriptPath}'.");
        }

        public static McpResponse ScaffoldThirdPersonCamera(string targetName = "PlayerCharacter")
        {
            var cam = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            
            string scriptPath = "Assets/Scripts/SmoothFollowCamera.cs";
            string scriptContent = @"using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public float smoothSpeed = 0.125f;
    public float rotationSpeed = 3f;

    private float currentYaw = 0f;
    private float currentPitch = 15f;

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(1))
        {
            currentYaw += Input.GetAxis(""Mouse X"") * rotationSpeed;
            currentPitch -= Input.GetAxis(""Mouse Y"") * rotationSpeed;
            currentPitch = Mathf.Clamp(currentPitch, -20f, 60f);
        }

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}";
            ScriptAndCompilationHandler.CreateOrUpdateScript(scriptPath, scriptContent);

            return McpResponse.Success($"Scaffolded SmoothFollowCamera and created '{scriptPath}'.");
        }
    }
}
