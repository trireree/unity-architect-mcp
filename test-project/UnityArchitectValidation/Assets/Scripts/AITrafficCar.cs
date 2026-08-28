using UnityEngine;

public class AITrafficCar : MonoBehaviour
{
    [Header("AI Settings")]
    public float driveSpeed = 8f;
    public float sensorDistance = 5f;
    public Vector3 driveDirection = Vector3.forward;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Safe kinematic movement for road traffic
        }
    }

    void Update()
    {
        // Obstacle detection ahead
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
        bool obstacleAhead = Physics.Raycast(ray, sensorDistance);

        if (!obstacleAhead)
        {
            transform.Translate(Vector3.forward * driveSpeed * Time.deltaTime);
        }

        // Loop boundaries if out of city range
        if (transform.position.z > 60f) transform.position = new Vector3(transform.position.x, transform.position.y, -60f);
        else if (transform.position.z < -60f) transform.position = new Vector3(transform.position.x, transform.position.y, 60f);
        if (transform.position.x > 60f) transform.position = new Vector3(-60f, transform.position.y, transform.position.z);
        else if (transform.position.x < -60f) transform.position = new Vector3(60f, transform.position.y, transform.position.z);
    }
}
