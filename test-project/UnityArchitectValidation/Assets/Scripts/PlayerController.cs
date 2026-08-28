#pragma warning disable CS0618, CS0619
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 1.6f;
    public float gravity = -18f;

    [Header("Camera & Vehicle")]
    public Transform cameraTransform;
    public float interactionRadius = 3.5f;
    public LayerMask vehicleLayer;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isInVehicle = false;

    public bool IsInVehicle => isInVehicle;

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
        if (isInVehicle) return;

        // Ground Check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // WASD Input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + (cameraTransform ? cameraTransform.eulerAngles.y : 0);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Enter Vehicle (E Key)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryEnterNearestVehicle();
        }
    }

    private void TryEnterNearestVehicle()
    {
        var vehicles = Object.FindObjectsByType<VehicleSeat>(FindObjectsSortMode.None);
        foreach (var seat in vehicles)
        {
            if (Vector3.Distance(transform.position, seat.transform.position) <= interactionRadius && !seat.isOccupied)
            {
                seat.EnterVehicle(this);
                break;
            }
        }
    }

    public void SetInVehicleState(bool inVehicle)
    {
        isInVehicle = inVehicle;
        if (controller != null) controller.enabled = !inVehicle;
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = !inVehicle;
    }
}