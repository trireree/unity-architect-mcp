#pragma warning disable CS0618, CS0619
using UnityEngine;

public class FPSGameHUD : MonoBehaviour
{
    private float fps = 60f;
    private float fpsTimer = 0.5f;

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
        // 1. Dynamic Center Crosshair (Soft dot + ring)
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        // Center dot
        GUI.color = new Color(1f, 1f, 1f, 0.85f);
        GUI.Box(new Rect(cx - 3, cy - 3, 6, 6), "");

        // 2. Info Box (Top-Left)
        GUI.skin.label.fontSize = 13;
        GUI.Box(new Rect(15, 15, 290, 85), "✨ STYLIZED FPS CONTROLLER");
        GUI.color = Color.cyan;
        GUI.Label(new Rect(25, 40, 270, 22), "Move: WASD | Sprint: Shift | Jump: Space");
        GUI.color = Color.yellow;
        GUI.Label(new Rect(25, 62, 270, 22), "Left Click (LMB): Punch / Interact");
        GUI.color = Color.white;

        // 3. FPS Badge (Top-Right)
        float sw = Screen.width;
        GUI.Box(new Rect(sw - 120, 15, 105, 40), "");
        GUI.color = fps >= 55f ? Color.green : Color.yellow;
        GUI.Label(new Rect(sw - 110, 24, 90, 22), $"⚡ {fps:F0} FPS");
        GUI.color = Color.white;
    }
}
