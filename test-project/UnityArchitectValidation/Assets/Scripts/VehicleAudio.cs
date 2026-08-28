#pragma warning disable CS0618, CS0619
using UnityEngine;

public class VehicleAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip engineStartClip;
    public AudioClip engineIdleClip;
    public AudioClip lowSpeedClip;
    public AudioClip highRpmClip;
    public AudioClip tireRoadClip;
    public AudioClip hardBrakeClip;
    public AudioClip lightImpactClip;
    public AudioClip heavyImpactClip;
    public AudioClip doorClip;
    public AudioClip hornClip;

    private AudioSource idleSource;
    private AudioSource lowSpeedSource;
    private AudioSource highRpmSource;
    private AudioSource tireSource;
    private AudioSource sfxSource;
    private AudioSource impactSource;

    private Vehicle vehicle;
    private float lastImpactTime = 0f;
    private float hornTimer = 0f;
    private Transform mainCamTransform;

    void Awake()
    {
        vehicle = GetComponent<Vehicle>();
        SetupAudioSources();
        AutoLoadAudioClips();
    }

    void Start()
    {
        if (Camera.main != null) mainCamTransform = Camera.main.transform;
    }

    private void SetupAudioSources()
    {
        idleSource = Create3DAudioSource("Audio_EngineIdle", true);
        lowSpeedSource = Create3DAudioSource("Audio_LowSpeed", true);
        highRpmSource = Create3DAudioSource("Audio_HighRpm", true);
        tireSource = Create3DAudioSource("Audio_TireRoad", true);
        sfxSource = Create3DAudioSource("Audio_SFX", false);
        impactSource = Create3DAudioSource("Audio_Impact", false);
    }

    private AudioSource Create3DAudioSource(string name, bool isLoop)
    {
        var child = new GameObject(name);
        child.transform.SetParent(transform);
        child.transform.localPosition = Vector3.zero;

        var src = child.AddComponent<AudioSource>();
        src.spatialBlend = 1.0f; // 100% 3D Spatial Audio
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 1.5f;
        src.maxDistance = 28.0f;
        src.dopplerLevel = 0.3f;
        src.loop = isLoop;
        src.playOnAwake = false;
        src.volume = 0f;
        return src;
    }

    public void AutoLoadAudioClips()
    {
#if UNITY_EDITOR
        string sPath = "Assets/sesler/";
        if (engineStartClip == null) engineStartClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(sPath + "Realistic_modern_gas_#4-1787927392359.mp3");
        if (engineIdleClip == null) engineIdleClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(sPath + "Realistic_gasoline_c_#3-1787927482942.mp3");
        if (lowSpeedClip == null) lowSpeedClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(sPath + "Realistic_low-speed__#2-1787927581495.mp3");
        if (highRpmClip == null) highRpmClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(sPath + "Realistic_performanc_#4-1787927461489.mp3");
        if (tireRoadClip == null) tireRoadClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(sPath + "Realistic_car_drivin_#2-1787927429094.mp3");
        if (hardBrakeClip == null) hardBrakeClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(sPath + "Realistic_car_hard_b_#4-1787927532688.mp3");
        if (lightImpactClip == null) lightImpactClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(sPath + "Realistic_modern_gas_#4-1787927706497.mp3");
        if (heavyImpactClip == null) heavyImpactClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(sPath + "Realistic_high-impac_#3-1787927598238.mp3");
        if (doorClip == null) doorClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(sPath + "Realistic_modern_car_#4-1787927638421.mp3");
        if (hornClip == null) hornClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(sPath + "Realistic_modern_car_horn#2-1787927774911.mp3");
#endif

        if (idleSource != null && engineIdleClip != null) idleSource.clip = engineIdleClip;
        if (lowSpeedSource != null && lowSpeedClip != null) lowSpeedSource.clip = lowSpeedClip;
        if (highRpmSource != null && highRpmClip != null) highRpmSource.clip = highRpmClip;
        if (tireSource != null && tireRoadClip != null) tireSource.clip = tireRoadClip;
    }

    void Update()
    {
        if (vehicle == null) return;

        bool engineOn = vehicle.IsEngineRunning;
        float speedKmh = vehicle.SpeedKmh;
        float speedRatio = Mathf.Clamp01(speedKmh / 90f);

        if (mainCamTransform == null && Camera.main != null) mainCamTransform = Camera.main.transform;

        // Proximity optimization: Only process audio if within 35m of camera
        float distToCam = mainCamTransform != null ? Vector3.Distance(transform.position, mainCamTransform.position) : 0f;
        if (distToCam > 35f && !vehicle.IsPlayerDriving)
        {
            if (idleSource.isPlaying) idleSource.Stop();
            if (lowSpeedSource.isPlaying) lowSpeedSource.Stop();
            if (highRpmSource.isPlaying) highRpmSource.Stop();
            if (tireSource.isPlaying) tireSource.Stop();
            return;
        }

        if (engineOn)
        {
            if (!idleSource.isPlaying && idleSource.clip != null) idleSource.Play();
            if (!lowSpeedSource.isPlaying && lowSpeedSource.clip != null) lowSpeedSource.Play();
            if (!highRpmSource.isPlaying && highRpmClip != null) highRpmSource.Play();
            if (!tireSource.isPlaying && tireSource.clip != null) tireSource.Play();

            // Smooth Multi-Layer Balancing
            float targetIdleVol = Mathf.Clamp01(1.0f - speedRatio * 1.5f) * 0.75f;
            idleSource.volume = Mathf.Lerp(idleSource.volume, targetIdleVol, Time.deltaTime * 5f);
            idleSource.pitch = Mathf.Lerp(idleSource.pitch, 0.95f + speedRatio * 0.25f, Time.deltaTime * 4f);

            float targetLowVol = Mathf.Clamp01(speedKmh / 30f) * (1.0f - Mathf.Clamp01((speedKmh - 35f) / 25f)) * 0.7f;
            lowSpeedSource.volume = Mathf.Lerp(lowSpeedSource.volume, targetLowVol, Time.deltaTime * 5f);
            lowSpeedSource.pitch = Mathf.Lerp(lowSpeedSource.pitch, 0.85f + speedRatio * 0.45f, Time.deltaTime * 4f);

            float targetHighVol = Mathf.Clamp01((speedKmh - 25f) / 40f) * 0.85f;
            highRpmSource.volume = Mathf.Lerp(highRpmSource.volume, targetHighVol, Time.deltaTime * 6f);
            highRpmSource.pitch = Mathf.Lerp(highRpmSource.pitch, 0.8f + speedRatio * 0.6f, Time.deltaTime * 5f);

            float targetTireVol = Mathf.Clamp01(speedKmh / 25f) * 0.6f;
            tireSource.volume = Mathf.Lerp(tireSource.volume, targetTireVol, Time.deltaTime * 5f);
        }
        else
        {
            idleSource.volume = Mathf.Lerp(idleSource.volume, 0f, Time.deltaTime * 8f);
            lowSpeedSource.volume = Mathf.Lerp(lowSpeedSource.volume, 0f, Time.deltaTime * 8f);
            highRpmSource.volume = Mathf.Lerp(highRpmSource.volume, 0f, Time.deltaTime * 8f);
            tireSource.volume = Mathf.Lerp(tireSource.volume, 0f, Time.deltaTime * 8f);
        }

        // Hard braking sound trigger
        if (engineOn && speedKmh > 25f && Input.GetKey(KeyCode.Space) && vehicle.IsPlayerDriving)
        {
            PlayHardBrake();
        }

        // Horn
        hornTimer -= Time.deltaTime;
        if (vehicle.IsPlayerDriving && Input.GetKeyDown(KeyCode.H))
        {
            PlayHorn();
        }
    }

    public void PlayEngineStart()
    {
        if (sfxSource != null && engineStartClip != null)
        {
            sfxSource.PlayOneShot(engineStartClip, 0.95f);
        }
    }

    public void PlayDoor()
    {
        if (sfxSource != null && doorClip != null)
        {
            sfxSource.PlayOneShot(doorClip, 0.9f);
        }
    }

    public void PlayHorn()
    {
        if (hornTimer <= 0f && sfxSource != null && hornClip != null)
        {
            sfxSource.PlayOneShot(hornClip, 1.0f);
            hornTimer = 0.8f;
        }
    }

    public void PlayHardBrake()
    {
        if (!sfxSource.isPlaying && hardBrakeClip != null)
        {
            sfxSource.PlayOneShot(hardBrakeClip, 0.65f);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (Time.time - lastImpactTime < 0.4f) return; // Cooldown
        
        // Ignore road/ground plane collisions (only trigger on actual building / obstacle / car impacts)
        if (col.gameObject.name.Contains("Ground") || col.gameObject.name.Contains("road") || col.gameObject.name.Contains("Road"))
            return;

        float impactMagnitude = col.relativeVelocity.magnitude;
        if (impactMagnitude > 6.0f)
        {
            lastImpactTime = Time.time;
            if (impactSource != null && heavyImpactClip != null)
            {
                impactSource.PlayOneShot(heavyImpactClip, Mathf.Clamp01(impactMagnitude / 18f));
            }
        }
        else if (impactMagnitude > 2.8f)
        {
            lastImpactTime = Time.time;
            if (impactSource != null && lightImpactClip != null)
            {
                impactSource.PlayOneShot(lightImpactClip, Mathf.Clamp01(impactMagnitude / 10f));
            }
        }
    }
}
