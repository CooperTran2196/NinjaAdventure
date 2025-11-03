using UnityEngine;

/// <summary>
/// Simple component for individual weather particle control.
/// Attach to individual weather prefabs for standalone enable/disable.
/// Supports camera-following for infinite world scrolling.
/// </summary>
public class ENV_ParticleWeather : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Start playing particles when scene loads")]
    [SerializeField] private bool playOnStart = true;
    
    [Tooltip("Follow camera position (for scrolling levels)")]
    [SerializeField] private bool followCamera = false;
    
    [Header("References")]
    [SerializeField] private ParticleSystem[] particleSystems;
    
    private Camera mainCamera;
    private Vector3 cameraOffset;
    
    void Awake()
    {
        // Auto-detect particle systems if not assigned
        if (particleSystems == null || particleSystems.Length == 0)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }
        
        if (followCamera)
        {
            mainCamera = Camera.main;
            if (mainCamera)
            {
                cameraOffset = transform.position - mainCamera.transform.position;
            }
            else
            {
                Debug.LogWarning($"{name}: followCamera enabled but no main camera found!");
            }
        }
    }
    
    void Start()
    {
        if (playOnStart)
        {
            PlayWeather();
        }
        else
        {
            StopWeather();
        }
    }
    
    void Update()
    {
        if (followCamera && mainCamera)
        {
            transform.position = mainCamera.transform.position + cameraOffset;
        }
    }
    
    /// <summary>
    /// Start all particle systems.
    /// </summary>
    public void PlayWeather()
    {
        foreach (var ps in particleSystems)
        {
            if (ps && !ps.isPlaying) ps.Play();
        }
    }
    
    /// <summary>
    /// Stop all particle systems and clear particles.
    /// </summary>
    public void StopWeather()
    {
        foreach (var ps in particleSystems)
        {
            if (ps && ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    
    /// <summary>
    /// Toggle weather on/off.
    /// </summary>
    public void ToggleWeather()
    {
        if (particleSystems.Length > 0 && particleSystems[0].isPlaying)
        {
            StopWeather();
        }
        else
        {
            PlayWeather();
        }
    }
}
