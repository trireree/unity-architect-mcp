#pragma warning disable CS0618, CS0619
using UnityEngine;

[RequireComponent(typeof(Vehicle))]
public class TrafficVehicleController : MonoBehaviour
{
    [Header("Traffic Navigation")]
    public float driveSpeed = 9f;
    public float sensorDistance = 7f;
    public Vector3 targetLaneDirection = Vector3.forward;

    private Vehicle vehicle;
    private Rigidbody rb;

    void Start()
    {
        vehicle = GetComponent<Vehicle>();
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Kinematic for clean, stable road traffic flow
        }
    }

    void Update()
    {
        if (vehicle != null && vehicle.IsPlayerDriving)
        {
            enabled = false;
            return;
        }

        // Obstacle detection ahead
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
        bool obstacleAhead = Physics.Raycast(ray, sensorDistance, ~0, QueryTriggerInteraction.Ignore);

        if (!obstacleAhead)
        {
            transform.Translate(Vector3.forward * driveSpeed * Time.deltaTime, Space.Self);
        }

        // City Boundary Looping (300m x 300m range)
        float bound = 280f;
        Vector3 pos = transform.position;
        if (pos.z > bound) transform.position = new Vector3(pos.x, pos.y, -bound);
        else if (pos.z < -bound) transform.position = new Vector3(pos.x, pos.y, bound);
        if (pos.x > bound) transform.position = new Vector3(-bound, pos.y, pos.z);
        else if (pos.x < -bound) transform.position = new Vector3(bound, pos.y, pos.z);
    }
}
