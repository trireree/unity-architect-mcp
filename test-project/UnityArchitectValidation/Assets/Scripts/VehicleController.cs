using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Vehicle Settings")]
    public float maxMotorTorque = 1800f;
    public float maxSteeringAngle = 32f;
    public float brakeTorque = 4000f;
    public Vector3 centerOfMassOffset = new Vector3(0, -0.6f, 0);

    public bool isPlayerControlled = false;
    private Rigidbody rb;

    public float CurrentSpeedKmh => rb != null ? rb.linearVelocity.magnitude * 3.6f : 0f;

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
        if (!isPlayerControlled)
        {
            ApplyBrakes(brakeTorque);
            return;
        }

        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        bool braking = Input.GetKey(KeyCode.Space);

        if (frontLeftWheel) frontLeftWheel.steerAngle = steering;
        if (frontRightWheel) frontRightWheel.steerAngle = steering;

        if (rearLeftWheel) rearLeftWheel.motorTorque = braking ? 0 : motor;
        if (rearRightWheel) rearRightWheel.motorTorque = braking ? 0 : motor;

        float appliedBrake = braking ? brakeTorque : 0;
        ApplyBrakes(appliedBrake);
    }

    private void ApplyBrakes(float brakePower)
    {
        if (frontLeftWheel) frontLeftWheel.brakeTorque = brakePower;
        if (frontRightWheel) frontRightWheel.brakeTorque = brakePower;
        if (rearLeftWheel) rearLeftWheel.brakeTorque = brakePower;
        if (rearRightWheel) rearRightWheel.brakeTorque = brakePower;
    }
}
