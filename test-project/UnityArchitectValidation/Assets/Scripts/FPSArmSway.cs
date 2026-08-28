#pragma warning disable CS0618, CS0619
using UnityEngine;

public class FPSArmSway : MonoBehaviour
{
    [Header("Mouse Sway")]
    public float swayAmount = 1.8f;
    public float maxSway = 6.0f;
    public float swaySmooth = 8.0f;

    [Header("Idle Breathing")]
    public float breatheSpeed = 2.0f;
    public float breatheAmount = 0.015f;

    [Header("Walk Bobbing")]
    public float walkBobSpeed = 10.0f;
    public float walkBobAmount = 0.035f;

    [Header("Punch / Interact Action")]
    public Transform rightArm;
    public Transform leftArm;
    public float punchSpeed = 14f;

    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private FPSController playerController;
    private float breatheTimer = 0f;
    private float walkTimer = 0f;
    private float punchProgress = 0f;
    private bool isPunching = false;

    void Start()
    {
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
        playerController = GetComponentInParent<FPSController>();
    }

    void Update()
    {
        // 1. Mouse Sway
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        float moveX = Mathf.Clamp(-mouseX * swayAmount, -maxSway, maxSway);
        float moveY = Mathf.Clamp(-mouseY * swayAmount, -maxSway, maxSway);

        Quaternion targetSwayRot = Quaternion.Euler(moveY, moveX, -moveX * 0.5f) * initialLocalRot;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetSwayRot, Time.deltaTime * swaySmooth);

        // 2. Idle & Walk Positional Movement
        breatheTimer += Time.deltaTime * breatheSpeed;
        Vector3 targetPos = initialLocalPos;

        targetPos.y += Mathf.Sin(breatheTimer) * breatheAmount;

        if (playerController != null && playerController.IsMoving)
        {
            float speedMult = playerController.IsSprinting ? 1.4f : 1.0f;
            walkTimer += Time.deltaTime * walkBobSpeed * speedMult;
            targetPos.x += Mathf.Cos(walkTimer / 2f) * walkBobAmount;
            targetPos.y += Mathf.Abs(Mathf.Sin(walkTimer)) * walkBobAmount;
        }

        // 3. Punch Action (LMB)
        if (Input.GetMouseButtonDown(0) && !isPunching)
        {
            isPunching = true;
            punchProgress = 0f;
        }

        if (isPunching)
        {
            punchProgress += Time.deltaTime * punchSpeed;
            float punchOffset = Mathf.Sin(punchProgress * Mathf.PI) * 0.35f;
            targetPos.z += punchOffset;

            if (punchProgress >= 1f)
            {
                isPunching = false;
                punchProgress = 0f;
            }
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 10f);
    }
}
