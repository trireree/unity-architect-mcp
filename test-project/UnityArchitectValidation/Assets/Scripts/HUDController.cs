#pragma warning disable CS0618, CS0619
using UnityEngine;

public class HUDController : MonoBehaviour
{
    private FPSPlayer player;
    private float fps = 60f;
    private float fpsTimer = 0.4f;

    void Start()
    {
        player = Object.FindAnyObjectByType<FPSPlayer>();
    }

    void Update()
    {
        fpsTimer -= Time.deltaTime;
        if (fpsTimer <= 0f)
        {
            fps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            fpsTimer = 0.3f;
        }
    }

    void OnGUI()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        // 1. Center Crosshair (+)
        GUI.color = new Color(1f, 1f, 1f, 0.9f);
        GUI.skin.label.fontSize = 16;
        GUI.Label(new Rect(cx - 6, cy - 12, 20, 20), "+");

        if (player != null)
        {
            if (player.IsInVehicle)
            {
                var currentCar = player.GetComponentInParent<Vehicle>();
                if (currentCar != null)
                {
                    // Bottom-Right Vehicle Cluster
                    float rw = Screen.width - 270;
                    float rh = Screen.height - 145;
                    GUI.Box(new Rect(rw, rh, 255, 130), $"🚗 {currentCar.vehicleName.ToUpper()}");

                    // Engine Status
                    if (!currentCar.IsEngineRunning)
                    {
                        GUI.color = Color.red;
                        GUI.skin.label.fontSize = 16;
                        GUI.Label(new Rect(rw + 15, rh + 28, 230, 24), "⛔ ENGINE OFF");
                        GUI.color = Color.yellow;
                        GUI.skin.label.fontSize = 13;
                        GUI.Label(new Rect(rw + 15, rh + 55, 230, 22), "[ F ] Start Engine");
                        GUI.color = Color.white;
                        GUI.Label(new Rect(rw + 15, rh + 80, 230, 22), "[ E ] Exit Vehicle");
                    }
                    else
                    {
                        GUI.color = Color.green;
                        GUI.skin.label.fontSize = 14;
                        GUI.Label(new Rect(rw + 15, rh + 25, 230, 22), "⚡ ENGINE ON (Automatic)");

                        GUI.color = Color.yellow;
                        GUI.skin.label.fontSize = 18;
                        GUI.Label(new Rect(rw + 15, rh + 48, 230, 28), $"SPEED  {currentCar.SpeedKmh:000} KM/H");

                        GUI.color = Color.cyan;
                        GUI.skin.label.fontSize = 12;
                        GUI.Label(new Rect(rw + 15, rh + 78, 230, 20), "[ H ] Horn | [ F ] Stop Engine");
                        GUI.color = Color.white;
                        GUI.Label(new Rect(rw + 15, rh + 98, 230, 20), "[ E ] Exit Vehicle");
                    }
                }
            }
            else if (player.CurrentInteractable != null)
            {
                // Centered Interactive Prompt
                string prompt = player.CurrentInteractable.InteractionPrompt;
                GUI.color = Color.yellow;
                GUI.skin.label.fontSize = 14;
                GUI.Box(new Rect(cx - 130, cy + 35, 260, 38), "");
                GUI.Label(new Rect(cx - 120, cy + 42, 240, 26), $"<b>{prompt}</b>");
                GUI.color = Color.white;
            }
        }

        // Top-Left Info Box
        GUI.skin.label.fontSize = 13;
        GUI.Box(new Rect(15, 15, 310, 85), "🏙️ KENNEY ASSET-FIRST OPEN WORLD");
        GUI.color = Color.green;
        GUI.Label(new Rect(25, 36, 290, 20), "On Foot: WASD = Move | Shift = Run | Space = Jump");
        GUI.color = Color.yellow;
        GUI.Label(new Rect(25, 54, 290, 20), "Car: [E] Enter/Exit | [F] Start Engine | [H] Horn");
        GUI.color = Color.white;

        // Top-Right FPS Counter
        float sw = Screen.width;
        GUI.Box(new Rect(sw - 115, 15, 100, 40), "");
        GUI.color = fps >= 55f ? Color.green : Color.yellow;
        GUI.Label(new Rect(sw - 105, 24, 85, 22), $"⚡ {fps:F0} FPS");
        GUI.color = Color.white;
    }
}
