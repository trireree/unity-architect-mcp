using System;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Handlers;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Templates
{
    public static class GameSystemTemplates
    {
        public static McpResponse ScaffoldSystem(string systemName, string targetPath = "Assets/Scripts")
        {
            string scriptPath = null;
            string content = null;

            switch (systemName?.ToLowerInvariant())
            {
                case "player":
                case "thirdperson":
                case "thirdpersoncontroller":
                    scriptPath = $"{targetPath}/PlayerController.cs";
                    content = GetPlayerControllerScript();
                    break;

                case "vehicle":
                case "car":
                    scriptPath = $"{targetPath}/SimpleCarController.cs";
                    content = GetVehicleControllerScript();
                    break;

                case "weapon":
                case "gun":
                case "combat":
                    scriptPath = $"{targetPath}/WeaponController.cs";
                    content = GetWeaponScript();
                    break;

                case "health":
                case "damage":
                    scriptPath = $"{targetPath}/HealthSystem.cs";
                    content = GetHealthScript();
                    break;

                case "inventory":
                    scriptPath = $"{targetPath}/InventoryManager.cs";
                    content = GetInventoryScript();
                    break;

                case "enemy":
                case "ai":
                    scriptPath = $"{targetPath}/EnemyAI.cs";
                    content = GetEnemyAIScript();
                    break;

                case "interaction":
                    scriptPath = $"{targetPath}/InteractableSystem.cs";
                    content = GetInteractionScript();
                    break;

                case "saveload":
                case "save":
                    scriptPath = $"{targetPath}/SaveLoadManager.cs";
                    content = GetSaveLoadScript();
                    break;

                case "objectpool":
                case "pool":
                    scriptPath = $"{targetPath}/ObjectPooler.cs";
                    content = GetObjectPoolScript();
                    break;

                case "daynight":
                    scriptPath = $"{targetPath}/DayNightCycle.cs";
                    content = GetDayNightScript();
                    break;

                case "police":
                case "wanted":
                    scriptPath = $"{targetPath}/WantedSystem.cs";
                    content = GetWantedSystemScript();
                    break;

                case "traffic":
                    scriptPath = $"{targetPath}/TrafficSpawner.cs";
                    content = GetTrafficSpawnerScript();
                    break;

                default:
                    return McpResponse.Error($"Unknown game system '{systemName}'. Supported: player, vehicle, weapon, health, inventory, enemy, interaction, saveload, objectpool, daynight, police, traffic.");
            }

            return ScriptAndCompilationHandler.CreateOrUpdateScript(scriptPath, content);
        }

        public static string GetPlayerControllerScript()
        {
            return @"using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header(""Movement Parameters"")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 1.6f;
    public float gravity = -18f;
    public float turnSmoothTime = 0.1f;

    [Header(""References"")]
    public Transform cameraTransform;
    private CharacterController controller;
    private Vector3 velocity;
    private float turnSmoothVelocity;
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
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }

        if (Input.GetButtonDown(""Jump"") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}";
        }

        public static string GetVehicleControllerScript()
        {
            return @"using UnityEngine;

public class SimpleCarController : MonoBehaviour
{
    [Header(""Wheel Colliders"")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header(""Vehicle Settings"")]
    public float maxMotorTorque = 1500f;
    public float maxSteeringAngle = 30f;
    public float brakeTorque = 3000f;
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.centerOfMass += centerOfMassOffset;
        }
    }

    void FixedUpdate()
    {
        float motor = maxMotorTorque * Input.GetAxis(""Vertical"");
        float steering = maxSteeringAngle * Input.GetAxis(""Horizontal"");
        bool braking = Input.GetKey(KeyCode.Space);

        if (frontLeftWheel) frontLeftWheel.steerAngle = steering;
        if (frontRightWheel) frontRightWheel.steerAngle = steering;

        if (rearLeftWheel) rearLeftWheel.motorTorque = braking ? 0 : motor;
        if (rearRightWheel) rearRightWheel.motorTorque = braking ? 0 : motor;

        float appliedBrake = braking ? brakeTorque : 0;
        if (frontLeftWheel) frontLeftWheel.brakeTorque = appliedBrake;
        if (frontRightWheel) frontRightWheel.brakeTorque = appliedBrake;
        if (rearLeftWheel) rearLeftWheel.brakeTorque = appliedBrake;
        if (rearRightWheel) rearRightWheel.brakeTorque = appliedBrake;
    }
}";
        }

        public static string GetWeaponScript()
        {
            return @"using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header(""Gun Stats"")]
    public float damage = 25f;
    public float fireRate = 0.15f;
    public float range = 100f;
    public int maxAmmo = 30;
    public int currentAmmo = 30;

    private float nextTimeToFire = 0f;
    public Camera playerCamera;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetButton(""Fire1"") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentAmmo = maxAmmo;
        }
    }

    void Shoot()
    {
        currentAmmo--;
        if (playerCamera == null) return;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, range))
        {
            if (hit.transform.TryGetComponent<HealthSystem>(out var health))
            {
                health.TakeDamage(damage);
            }
        }
    }
}";
        }

        public static string GetHealthScript()
        {
            return @"using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            gameObject.SetActive(false);
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }
}";
        }

        public static string GetInventoryScript()
        {
            return @"using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItem
{
    public string itemId;
    public string itemName;
    public int quantity;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public List<InventoryItem> items = new List<InventoryItem>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(string id, string name, int amount = 1)
    {
        var existing = items.Find(i => i.itemId == id);
        if (existing != null) existing.quantity += amount;
        else items.Add(new InventoryItem { itemId = id, itemName = name, quantity = amount });
    }

    public bool RemoveItem(string id, int amount = 1)
    {
        var existing = items.Find(i => i.itemId == id);
        if (existing != null && existing.quantity >= amount)
        {
            existing.quantity -= amount;
            if (existing.quantity == 0) items.Remove(existing);
            return true;
        }
        return false;
    }
}";
        }

        public static string GetEnemyAIScript()
        {
            return @"using UnityEngine;
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
}";
        }

        public static string GetInteractionScript()
        {
            return @"using UnityEngine;

public class InteractableSystem : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactLayer;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && cam != null)
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactRange, interactLayer))
            {
                Debug.Log($""Interacted with: {hit.collider.gameObject.name}"");
            }
        }
    }
}";
        }

        public static string GetSaveLoadScript()
        {
            return @"using UnityEngine;
using System.IO;

public static class SaveLoadManager
{
    public static void SaveData<T>(string fileName, T data)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public static T LoadData<T>(string fileName) where T : new()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        return new T();
    }
}";
        }

        public static string GetObjectPoolScript()
        {
            return @"using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize = 20;
    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject SpawnFromPool(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0) return null;
        var obj = pool.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}";
        }

        public static string GetDayNightScript()
        {
            return @"using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0, 24)] public float timeOfDay = 12f;
    public float dayDurationInSeconds = 120f;
    public Light sunLight;

    void Update()
    {
        timeOfDay += (Time.deltaTime / dayDurationInSeconds) * 24f;
        if (timeOfDay >= 24f) timeOfDay = 0f;

        if (sunLight != null)
        {
            float angle = (timeOfDay / 24f) * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(angle, 170f, 0f);
        }
    }
}";
        }

        public static string GetWantedSystemScript()
        {
            return @"using UnityEngine;
using System;

public class WantedSystem : MonoBehaviour
{
    [Range(0, 5)] public int wantedLevel = 0;
    public event Action<int> OnWantedLevelChanged;

    public void AddStars(int stars)
    {
        wantedLevel = Mathf.Clamp(wantedLevel + stars, 0, 5);
        OnWantedLevelChanged?.Invoke(wantedLevel);
    }

    public void ClearWanted()
    {
        wantedLevel = 0;
        OnWantedLevelChanged?.Invoke(0);
    }
}";
        }

        public static string GetTrafficSpawnerScript()
        {
            return @"using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    public GameObject[] vehiclePrefabs;
    public Transform[] spawnPoints;
    public float spawnInterval = 5f;
    private float nextSpawnTime;

    void Update()
    {
        if (Time.time >= nextSpawnTime && vehiclePrefabs.Length > 0 && spawnPoints.Length > 0)
        {
            nextSpawnTime = Time.time + spawnInterval;
            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var prefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
            Instantiate(prefab, point.position, point.rotation);
        }
    }
}";
        }
    }
}
