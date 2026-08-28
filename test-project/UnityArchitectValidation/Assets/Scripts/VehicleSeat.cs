using UnityEngine;

public class VehicleSeat : MonoBehaviour
{
    [Header("Seat Settings")]
    public Transform exitAnchor;
    public VehicleController vehicle;
    public bool isOccupied = false;

    private PlayerController currentPlayer;
    private SmoothFollowCamera mainCam;

    void Start()
    {
        if (vehicle == null) vehicle = GetComponent<VehicleController>();
        var camGo = GameObject.FindWithTag("MainCamera");
        if (camGo != null) mainCam = camGo.GetComponent<SmoothFollowCamera>();
    }

    void Update()
    {
        if (isOccupied && Input.GetKeyDown(KeyCode.E))
        {
            ExitVehicle();
        }
    }

    public void EnterVehicle(PlayerController player)
    {
        if (isOccupied || player == null) return;

        currentPlayer = player;
        isOccupied = true;

        // Attach player to car & hide mesh
        player.transform.SetParent(transform);
        player.transform.localPosition = Vector3.zero;
        player.SetInVehicleState(true);

        if (vehicle != null) vehicle.isPlayerControlled = true;

        if (mainCam != null)
        {
            mainCam.SetTarget(transform, true);
        }
    }

    public void ExitVehicle()
    {
        if (!isOccupied || currentPlayer == null) return;

        Vector3 exitPos = exitAnchor != null ? exitAnchor.position : transform.position + transform.right * -2.5f + Vector3.up * 0.5f;

        currentPlayer.transform.SetParent(null);
        currentPlayer.transform.position = exitPos;
        currentPlayer.SetInVehicleState(false);

        if (vehicle != null) vehicle.isPlayerControlled = false;

        if (mainCam != null)
        {
            mainCam.SetTarget(currentPlayer.transform, false);
        }

        isOccupied = false;
        currentPlayer = null;
    }
}
