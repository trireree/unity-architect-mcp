using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0, 24)] public float timeOfDay = 12f;
    public float dayDurationInSeconds = 120f;
    public Light sunLight;

    void Update()
    {
        timeOfDay += (Time.deltaTime / dayDurationInSeconds) * 24f;
        if (timeOfDay >= 24f) timeOfDay = 0f;

        if (sunLight != null)
        {
            float angle = (timeOfDay / 24f) * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(angle, 170f, 0f);
        }
    }
}