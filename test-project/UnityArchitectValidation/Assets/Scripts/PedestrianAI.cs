#pragma warning disable CS0618, CS0619
using UnityEngine;

public enum PedestrianState { Idle, Walk, Avoid, Flee }

public class PedestrianAI : MonoBehaviour
{
    [Header("Pedestrian Settings")]
    public float walkSpeed = 2.2f;
    public float fleeSpeed = 5.0f;
    public float changeTargetInterval = 6.0f;

    private PedestrianState state = PedestrianState.Walk;
    private Vector3 moveDirection;
    private float stateTimer = 0f;
    private Transform playerTransform;

    void Start()
    {
        PickNewDirection();
        var p = GameObject.FindWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        // Player / Vehicle proximity reaction
        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist < 3.0f)
            {
                state = PedestrianState.Avoid;
                Vector3 away = (transform.position - playerTransform.position).normalized;
                away.y = 0;
                moveDirection = away;
            }
            else if (stateTimer <= 0f)
            {
                PickNewDirection();
            }
        }

        // Movement
        if (state == PedestrianState.Walk || state == PedestrianState.Avoid)
        {
            transform.Translate(Vector3.forward * (state == PedestrianState.Avoid ? fleeSpeed : walkSpeed) * Time.deltaTime, Space.Self);
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }
        }

        // Boundary Looping
        float bound = 280f;
        Vector3 pos = transform.position;
        if (pos.z > bound) transform.position = new Vector3(pos.x, pos.y, -bound);
        else if (pos.z < -bound) transform.position = new Vector3(pos.x, pos.y, bound);
        if (pos.x > bound) transform.position = new Vector3(-bound, pos.y, pos.z);
        else if (pos.x < -bound) transform.position = new Vector3(bound, pos.y, pos.z);
    }

    private void PickNewDirection()
    {
        float[] angles = new float[] { 0f, 90f, 180f, 270f };
        float chosenAngle = angles[Random.Range(0, angles.Length)];
        moveDirection = Quaternion.Euler(0, chosenAngle, 0) * Vector3.forward;
        state = Random.value > 0.2f ? PedestrianState.Walk : PedestrianState.Idle;
        stateTimer = Random.Range(4f, changeTargetInterval);
    }
}
