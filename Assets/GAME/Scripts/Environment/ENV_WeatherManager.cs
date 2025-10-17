using UnityEngine;

/// <summary>
/// Central manager for controlling weather particle systems.
/// Allows toggling weather effects on/off and switching between weather states.
/// Attach to a GameObject in scene (e.g., "WeatherController").
/// </summary>
public class ENV_WeatherManager : MonoBehaviour
{
    [Header("Weather Systems")]
    [SerializeField] private ParticleSystem clouds;
    [SerializeField] private ParticleSystem rain;
    [SerializeField] private ParticleSystem rainSplash;
    [SerializeField] private ParticleSystem leaves;
    [SerializeField] private ParticleSystem smoke;
    [SerializeField] private ParticleSystem fog;
    [SerializeField] private ParticleSystem spark;
    [SerializeField] private ParticleSystem snow;
    
    [Header("Static Effects")]
    [SerializeField] private GameObject raylight;
    
    [Header("Weather State")]
    [SerializeField] private bool enableClouds = true;
    [SerializeField] private bool enableRain = false;
    [SerializeField] private bool enableLeaves = false;
    [SerializeField] private bool enableSmoke = false;
    [SerializeField] private bool enableFog = false;
    [SerializeField] private bool enableSpark = false;
    [SerializeField] private bool enableSnow = false;
    [SerializeField] private bool enableRaylight = false;
    
    void Start()
    {
        ApplyWeatherState();
    }
    
    void OnValidate()
    {
        // Allow Inspector changes during Play mode to toggle effects in real-time
        if (Application.isPlaying)
        {
            ApplyWeatherState();
        }
    }
    
    /// <summary>
    /// Set all weather effects at once.
    /// </summary>
    public void SetWeather(bool clouds, bool rain, bool leaves, bool smoke, bool fog, bool spark, bool snow, bool raylight)
    {
        enableClouds = clouds;
        enableRain = rain;
        enableLeaves = leaves;
        enableSmoke = smoke;
        enableFog = fog;
        enableSpark = spark;
        enableSnow = snow;
        enableRaylight = raylight;
        ApplyWeatherState();
    }
    
    /// <summary>
    /// Preset: Clear sky with clouds only.
    /// </summary>
    public void SetClearWeather()
    {
        SetWeather(clouds: true, rain: false, leaves: false, smoke: false, fog: false, spark: false, snow: false, raylight: true);
    }
    
    /// <summary>
    /// Preset: Rainy autumn day.
    /// </summary>
    public void SetRainyWeather()
    {
        SetWeather(clouds: true, rain: true, leaves: true, smoke: false, fog: false, spark: false, snow: false, raylight: false);
    }
    
    /// <summary>
    /// Preset: Snowy winter.
    /// </summary>
    public void SetSnowyWeather()
    {
        SetWeather(clouds: true, rain: false, leaves: false, smoke: true, fog: true, spark: false, snow: true, raylight: false);
    }
    
    /// <summary>
    /// Preset: Magical/mystical atmosphere.
    /// </summary>
    public void SetMagicalWeather()
    {
        SetWeather(clouds: false, rain: false, leaves: false, smoke: false, fog: true, spark: true, snow: false, raylight: true);
    }
    
    void ApplyWeatherState()
    {
        ToggleParticleSystem(clouds, enableClouds);
        ToggleParticleSystem(rain, enableRain);
        ToggleParticleSystem(rainSplash, enableRain); // Linked to rain
        ToggleParticleSystem(leaves, enableLeaves);
        ToggleParticleSystem(smoke, enableSmoke);
        ToggleParticleSystem(fog, enableFog);
        ToggleParticleSystem(spark, enableSpark);
        ToggleParticleSystem(snow, enableSnow);
        
        if (raylight) raylight.SetActive(enableRaylight);
    }
    
    void ToggleParticleSystem(ParticleSystem ps, bool enable)
    {
        if (!ps) return;
        
        if (enable && !ps.isPlaying) 
        {
            ps.Play();
        }
        else if (!enable && ps.isPlaying) 
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    
    /// <summary>
    /// Toggle individual weather effect by name (for external calls).
    /// </summary>
    public void ToggleWeather(string weatherName, bool enable)
    {
        switch (weatherName.ToLower())
        {
            case "clouds": enableClouds = enable; ToggleParticleSystem(clouds, enable); break;
            case "rain": enableRain = enable; ToggleParticleSystem(rain, enable); ToggleParticleSystem(rainSplash, enable); break;
            case "leaves": enableLeaves = enable; ToggleParticleSystem(leaves, enable); break;
            case "smoke": enableSmoke = enable; ToggleParticleSystem(smoke, enable); break;
            case "fog": enableFog = enable; ToggleParticleSystem(fog, enable); break;
            case "spark": enableSpark = enable; ToggleParticleSystem(spark, enable); break;
            case "snow": enableSnow = enable; ToggleParticleSystem(snow, enable); break;
            case "raylight": enableRaylight = enable; if (raylight) raylight.SetActive(enable); break;
            default: Debug.LogWarning($"Unknown weather type: {weatherName}"); break;
        }
    }
    
    /// <summary>
    /// Toggle individual effect without knowing the name (just flip current state).
    /// </summary>
    public void ToggleClouds() => ToggleWeather("clouds", !enableClouds);
    public void ToggleRain() => ToggleWeather("rain", !enableRain);
    public void ToggleLeaves() => ToggleWeather("leaves", !enableLeaves);
    public void ToggleSmoke() => ToggleWeather("smoke", !enableSmoke);
    public void ToggleFog() => ToggleWeather("fog", !enableFog);
    public void ToggleSpark() => ToggleWeather("spark", !enableSpark);
    public void ToggleSnow() => ToggleWeather("snow", !enableSnow);
    public void ToggleRaylight() => ToggleWeather("raylight", !enableRaylight);
    
    /// <summary>
    /// Stop all weather effects immediately.
    /// </summary>
    public void StopAllWeather()
    {
        SetWeather(clouds: false, rain: false, leaves: false, smoke: false, fog: false, spark: false, snow: false, raylight: false);
    }
    
    /// <summary>
    /// Get current state of a weather effect.
    /// </summary>
    public bool IsWeatherEnabled(string weatherName)
    {
        return weatherName.ToLower() switch
        {
            "clouds" => enableClouds,
            "rain" => enableRain,
            "leaves" => enableLeaves,
            "smoke" => enableSmoke,
            "fog" => enableFog,
            "spark" => enableSpark,
            "snow" => enableSnow,
            "raylight" => enableRaylight,
            _ => false
        };
    }
}
