using UnityEngine;

public class SimpleCarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Vehicle Settings")]
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
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
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
}