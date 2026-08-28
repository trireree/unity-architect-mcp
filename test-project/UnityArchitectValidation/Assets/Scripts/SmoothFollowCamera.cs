using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    [Header("Target Tracking")]
    public Transform target;
    public Vector3 playerOffset = new Vector3(0f, 2.5f, -5f);
    public Vector3 vehicleOffset = new Vector3(0f, 3.5f, -7.5f);
    public float smoothSpeed = 0.12f;
    public float mouseSensitivity = 3f;

    private float currentYaw = 0f;
    private float currentPitch = 15f;
    private Vector3 currentVelocity;
    private bool isFollowingVehicle = false;

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Mouse Orbit (Hold Right-Click or Free Look)
        currentYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentPitch = Mathf.Clamp(currentPitch, -15f, 65f);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 activeOffset = isFollowingVehicle ? vehicleOffset : playerOffset;
        Vector3 desiredPosition = target.position + rotation * activeOffset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothSpeed);
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }

    public void SetTarget(Transform newTarget, bool isVehicle)
    {
        target = newTarget;
        isFollowingVehicle = isVehicle;
    }
}