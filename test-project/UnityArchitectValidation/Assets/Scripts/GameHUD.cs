using UnityEngine;

public class GameHUD : MonoBehaviour
{
    private float fps = 60f;
    private float fpsUpdateTimer = 0.5f;
    private int wantedLevel = 1;

    private PlayerController player;
    private VehicleController currentVehicle;

    void Start()
    {
        player = Object.FindAnyObjectByType<PlayerController>();
        currentVehicle = Object.FindAnyObjectByType<VehicleController>();
    }

    void Update()
    {
        fpsUpdateTimer -= Time.deltaTime;
        if (fpsUpdateTimer <= 0f)
        {
            fps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            fpsUpdateTimer = 0.3f;
        }
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 14;

        // 1. TOP-LEFT: Wanted Level & Game Info
        GUI.Box(new Rect(15, 15, 320, 110), "🏙️ CITY PROTOTYPE HUD");
        
        string wantedStars = "";
        for (int i = 0; i < wantedLevel; i++) wantedStars += "★ ";
        for (int i = wantedLevel; i < 5; i++) wantedStars += "☆ ";

        GUI.color = Color.yellow;
        GUI.Label(new Rect(25, 45, 300, 25), $"Wanted Level: {wantedStars}");
        GUI.color = Color.white;

        bool inCar = player != null && player.IsInVehicle;
        if (inCar && currentVehicle != null)
        {
            GUI.color = Color.cyan;
            GUI.Label(new Rect(25, 70, 300, 25), $"State: [🚗 DRIVING] Speed: {currentVehicle.CurrentSpeedKmh:F0} km/h");
            GUI.Label(new Rect(25, 92, 300, 25), "Controls: WASD (Drive), Space (Brake), E (Exit)");
        }
        else
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(25, 70, 300, 25), "State: [🚶 ON FOOT]");
            GUI.color = Color.white;
            GUI.Label(new Rect(25, 92, 300, 25), "Controls: WASD (Move), Shift (Sprint), Space (Jump), E (Enter Car)");
        }

        // 2. TOP-RIGHT: FPS Counter
        float screenW = Screen.width;
        GUI.Box(new Rect(screenW - 135, 15, 120, 45), "");
        GUI.color = fps >= 55f ? Color.green : (fps >= 30f ? Color.yellow : Color.red);
        GUI.Label(new Rect(screenW - 125, 25, 100, 25), $"⚡ FPS: {fps:F0}");
        GUI.color = Color.white;
    }
}
