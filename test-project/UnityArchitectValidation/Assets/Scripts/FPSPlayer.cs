#pragma warning disable CS0618, CS0619
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5.5f;
    public float sprintSpeed = 9.5f;
    public float crouchSpeed = 2.8f;
    public float jumpHeight = 1.4f;
    public float gravity = -20f;

    [Header("Camera & Look")]
    public Camera playerCamera;
    public float mouseSensitivity = 2.5f;
    public float maxLookAngle = 85f;

    [Header("Interaction")]
    public float interactionDistance = 3.5f;
    public LayerMask interactableLayer = ~0;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isGrounded;
    private IInteractable currentInteractable;
    private bool inVehicle = false;

    public IInteractable CurrentInteractable => currentInteractable;
    public bool IsInVehicle => inVehicle;
    public CharacterController Controller => controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.stepOffset = 0.3f;
        controller.slopeLimit = 45f;
        controller.skinWidth = 0.05f;
        controller.minMoveDistance = 0f;

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (inVehicle) return;

        // 1. Mouse Look
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
        transform.Rotate(Vector3.up * mouseX);

        // 2. Ground & Movement
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(h, 0f, v).normalized;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);
        float speed = isSprinting ? sprintSpeed : (isCrouching ? crouchSpeed : walkSpeed);

        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.z;

        // 3. Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 4. Combined Gravity & Movement (Single smooth controller.Move call)
        velocity.y += gravity * Time.deltaTime;
        Vector3 finalMovement = (moveDir * speed + velocity) * Time.deltaTime;
        controller.Move(finalMovement);

        // 5. Interaction Raycast
        CheckInteractionRaycast();

        // 6. Interact on [E]
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }

        // Always enforce locked and invisible cursor
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void CheckInteractionRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer, QueryTriggerInteraction.Collide))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            currentInteractable = interactable;
        }
        else
        {
            currentInteractable = null;
        }
    }

    public void SetInVehicleState(bool insideVehicle, Transform vehicleTransform = null)
    {
        inVehicle = insideVehicle;
        if (controller != null) controller.enabled = !insideVehicle;

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = !insideVehicle;

        if (insideVehicle && vehicleTransform != null)
        {
            transform.SetParent(vehicleTransform);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            transform.SetParent(null);
        }
    }
}
