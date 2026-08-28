#pragma warning disable CS0618, CS0619
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 5.5f;
    public float sprintSpeed = 9.0f;
    public float crouchSpeed = 3.0f;
    public float jumpHeight = 1.4f;
    public float gravity = -20.0f;

    [Header("Look Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 2.5f;
    public float upLookLimit = 85f;
    public float downLookLimit = 85f;

    [Header("Head Bobbing")]
    public float bobFrequency = 1.8f;
    public float bobHorizontalAmplitude = 0.05f;
    public float bobVerticalAmplitude = 0.05f;
    public float headBobSmoothing = 8f;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isGrounded;
    private Vector3 cameraInitialLocalPos;
    private float bobTimer = 0f;

    public bool IsMoving { get; private set; }
    public bool IsSprinting { get; private set; }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        if (playerCamera != null)
        {
            cameraInitialLocalPos = playerCamera.localPosition;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Mouse Look
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -upLookLimit, downLookLimit);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
        transform.Rotate(Vector3.up * mouseX);

        // 2. Movement
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(h, 0f, v).normalized;

        IsMoving = moveInput.magnitude > 0.1f && isGrounded;
        IsSprinting = IsMoving && Input.GetKey(KeyCode.LeftShift);

        float currentSpeed = IsSprinting ? sprintSpeed : (Input.GetKey(KeyCode.LeftControl) ? crouchSpeed : walkSpeed);
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.z;
        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        // 3. Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 4. Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 5. Head Bobbing
        HandleHeadBob();

        // Always enforce locked, invisible cursor
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleHeadBob()
    {
        if (playerCamera == null) return;

        if (IsMoving)
        {
            float speedMult = IsSprinting ? 1.5f : 1f;
            bobTimer += Time.deltaTime * bobFrequency * speedMult * 6f;

            Vector3 targetBobPos = cameraInitialLocalPos + new Vector3(
                Mathf.Cos(bobTimer / 2f) * bobHorizontalAmplitude,
                Mathf.Sin(bobTimer) * bobVerticalAmplitude,
                0f
            );

            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetBobPos, Time.deltaTime * headBobSmoothing);
        }
        else
        {
            bobTimer = 0f;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, cameraInitialLocalPos, Time.deltaTime * headBobSmoothing);
        }
    }
}
