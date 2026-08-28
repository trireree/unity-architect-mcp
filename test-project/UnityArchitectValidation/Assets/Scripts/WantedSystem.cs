using UnityEngine;
using System;

public class WantedSystem : MonoBehaviour
{
    [Range(0, 5)] public int wantedLevel = 0;
    public event Action<int> OnWantedLevelChanged;

    public void AddStars(int stars)
    {
        wantedLevel = Mathf.Clamp(wantedLevel + stars, 0, 5);
        OnWantedLevelChanged?.Invoke(wantedLevel);
    }

    public void ClearWanted()
    {
        wantedLevel = 0;
        OnWantedLevelChanged?.Invoke(0);
    }
}