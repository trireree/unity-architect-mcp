#pragma warning disable CS0618, CS0619
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Vehicle : MonoBehaviour, IInteractable
{
    [Header("Vehicle Identity")]
    public string vehicleName = "Sedan";
    public bool isParked = true;

    [Header("Engine State (Automatic)")]
    public bool isEngineRunning = false;

    [Header("Driving Physics (Arcade Rigidbody)")]
    public float acceleration = 22f;
    public float maxSpeedKmh = 95f;
    public float reverseSpeedKmh = 35f;
    public float turnSpeed = 65f;
    public float brakeStrength = 35f;
    public float downforce = 50f;

    [Header("3rd Person Camera & Anchors")]
    public Transform thirdPersonCamAnchor;
    public Transform exitAnchor;
    public float cameraFollowSmooth = 12f;
    public float mouseLookSensitivity = 2.5f;

    private Rigidbody rb;
    private FPSPlayer driver;
    private bool isPlayerDriving = false;
    private Camera playerCam;
    private float defaultCamFov = 60f;
    private float camYaw = 0f;
    private float camPitch = 14f;
    private VehicleAudio vehicleAudio;
    private bool isGrounded = true;

    public string InteractionPrompt => isPlayerDriving ? "[ E ] Exit Vehicle" : $"[ E ] Enter {vehicleName}";
    public bool IsPlayerDriving => isPlayerDriving;
    public bool IsEngineRunning => isEngineRunning;
    public float SpeedKmh => rb != null ? rb.linearVelocity.magnitude * 3.6f : 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = 1400f;
            rb.centerOfMass = new Vector3(0f, -0.4f, 0f);
            rb.linearDamping = 1.0f;
            rb.angularDamping = 4.0f;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Prevent car flipping
        }

        vehicleAudio = GetComponent<VehicleAudio>();
        if (vehicleAudio == null)
        {
            vehicleAudio = gameObject.AddComponent<VehicleAudio>();
        }

        if (thirdPersonCamAnchor == null)
        {
            var anchorGo = new GameObject("3rdPerson_CamAnchor");
            anchorGo.transform.SetParent(transform);
            anchorGo.transform.localPosition = new Vector3(0f, 2.2f, -5.6f);
            anchorGo.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
            thirdPersonCamAnchor = anchorGo.transform;
        }

        if (exitAnchor == null)
        {
            var exit = new GameObject("Exit_Anchor");
            exit.transform.SetParent(transform);
            exit.transform.localPosition = new Vector3(-2.0f, 0.2f, 0f);
            exitAnchor = exit.transform;
        }
    }

    void Update()
    {
        // Enforce locked & invisible mouse cursor
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (isPlayerDriving)
        {
            // 1. Exit Vehicle on [E]
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExitVehicle();
                return;
            }

            // 2. Engine Start / Stop on [F]
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleEngine();
            }

            // 3. 3rd Person Orbit Mouse Look & Speed FOV
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseLookSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseLookSensitivity;

            camYaw += mouseX;
            camPitch = Mathf.Clamp(camPitch - mouseY, 0f, 45f);

            if (Mathf.Abs(mouseX) < 0.05f && SpeedKmh > 5f)
            {
                camYaw = Mathf.Lerp(camYaw, 0f, Time.deltaTime * 3f);
            }

            if (playerCam != null)
            {
                float targetFov = defaultCamFov + Mathf.Clamp(SpeedKmh * 0.22f, 0f, 28f);
                playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFov, Time.deltaTime * 4f);

                Quaternion orbitRot = transform.rotation * Quaternion.Euler(camPitch, camYaw, 0f);
                Vector3 targetCamPos = transform.position + orbitRot * new Vector3(0f, 2.2f, -5.6f);

                playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, targetCamPos, Time.deltaTime * cameraFollowSmooth);
                playerCam.transform.LookAt(transform.position + Vector3.up * 1.2f);
            }
        }
    }

    void FixedUpdate()
    {
        // Ground check
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 1.2f, ~0, QueryTriggerInteraction.Ignore);

        if (isGrounded)
        {
            // Apply gentle downforce to keep car glued to road
            rb.AddForce(Vector3.down * downforce, ForceMode.Acceleration);
        }

        if (!isPlayerDriving || !isEngineRunning || !isGrounded)
        {
            return;
        }

        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");
        bool isBraking = Input.GetKey(KeyCode.Space);

        // Drive Forward / Reverse
        if (vInput > 0.05f && SpeedKmh < maxSpeedKmh)
        {
            rb.AddForce(transform.forward * acceleration * vInput, ForceMode.Acceleration);
        }
        else if (vInput < -0.05f && SpeedKmh < reverseSpeedKmh)
        {
            rb.AddForce(transform.forward * acceleration * 0.6f * vInput, ForceMode.Acceleration);
        }

        // Steering
        if (Mathf.Abs(hInput) > 0.05f && SpeedKmh > 1.0f)
        {
            float steerDir = vInput < -0.05f ? -hInput : hInput;
            float rotAmount = steerDir * turnSpeed * Time.fixedDeltaTime;
            transform.Rotate(0f, rotAmount, 0f);
        }

        // Handbrake
        if (isBraking)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * brakeStrength * 0.2f);
        }
    }

    public void ToggleEngine()
    {
        isEngineRunning = !isEngineRunning;
        if (isEngineRunning)
        {
            if (vehicleAudio != null) vehicleAudio.PlayEngineStart();
        }
    }

    public void Interact(FPSPlayer player)
    {
        if (isPlayerDriving || player == null) return;
        EnterVehicle(player);
    }

    public void EnterVehicle(FPSPlayer player)
    {
        driver = player;
        isPlayerDriving = true;
        isParked = false;
        isEngineRunning = false;
        camYaw = 0f;
        camPitch = 14f;

        if (vehicleAudio != null) vehicleAudio.PlayDoor();

        var trafficAi = GetComponent<TrafficVehicleController>();
        if (trafficAi != null) trafficAi.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        player.SetInVehicleState(true, transform);

        if (player.playerCamera != null)
        {
            playerCam = player.playerCamera;
            defaultCamFov = playerCam.fieldOfView;
            playerCam.transform.SetParent(null);
        }
    }

    public void ExitVehicle()
    {
        if (!isPlayerDriving || driver == null) return;

        if (vehicleAudio != null) vehicleAudio.PlayDoor();

        Vector3 safeExitPos = exitAnchor != null ? exitAnchor.position : transform.position + transform.right * -2f + Vector3.up * 0.5f;

        if (playerCam != null)
        {
            playerCam.transform.SetParent(driver.transform);
            playerCam.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            playerCam.transform.localRotation = Quaternion.identity;
            playerCam.fieldOfView = defaultCamFov;
            playerCam = null;
        }

        driver.transform.position = safeExitPos;
        driver.SetInVehicleState(false);

        isPlayerDriving = false;
        driver = null;
    }
}
