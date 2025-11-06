using System.Collections;
using UnityEngine;

public class SYS_GameManager : MonoBehaviour
{
    public static SYS_GameManager Instance;

    [Header("Central game manager - handles singletons, scene, audio")]
    [Header("References")]
    public SYS_Fader        sys_Fader;
    public SYS_SoundManager sys_SoundManager;
    public D_Manager        d_Manager;
    public D_HistoryTracker d_HistoryTracker;
    public SHOP_Manager     shop_Manager;
    public INV_ItemInfo     inv_ItemInfo;
    public AudioSource      audioSource;

    [Header("Restart Settings")]
    public string initialSceneName = "Level0";
    public string initialSpawnId   = "Start";

    [Header("Audio Settings")]
    public float fadeDuration = 1.5f;

    [Header("MUST wire MANUALLY in Inspector")]
    public GameObject[] persistentObjects;

    Coroutine currentFade;

    void Awake()
    {
        // If an instance already exists, destroy this one
        if (Instance != null)
        {
            CleanUpAndDestroy();
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        MarkPersistentObjects();

        d_Manager        ??= FindFirstObjectByType<D_Manager>();
        d_HistoryTracker ??= FindFirstObjectByType<D_HistoryTracker>();
        shop_Manager     ??= FindFirstObjectByType<SHOP_Manager>();
        sys_Fader        ??= FindFirstObjectByType<SYS_Fader>();
        sys_SoundManager ??= FindFirstObjectByType<SYS_SoundManager>();
        inv_ItemInfo     ??= FindFirstObjectByType<INV_ItemInfo>();
        audioSource      ??= GetComponent<AudioSource>();

        if (!d_Manager)        { Debug.LogWarning($"{name}: D_Manager is missing!", this); }
        if (!d_HistoryTracker) { Debug.LogWarning($"{name}: D_HistoryTracker is missing!", this); }
        if (!shop_Manager)     { Debug.LogWarning($"{name}: SHOP_Manager is missing!", this); }
        if (!sys_Fader)        { Debug.LogWarning($"{name}: SYS_Fader is missing!", this); }
        if (!sys_SoundManager) { Debug.LogWarning($"{name}: SYS_SoundManager is missing!", this); }
        if (!inv_ItemInfo)     { Debug.LogWarning($"{name}: INV_ItemInfo is missing!", this); }
        if (!audioSource)      { Debug.LogWarning($"{name}: AudioSource is missing!", this); }
        
        inv_ItemInfo?.Hide();

        if (audioSource)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f;
        }
    }

    // Restart the game by clearing all DDOL objects and loading the initial scene
    public void FreshBoot()
    {
        Time.timeScale = 1f;

        var temp = new GameObject("DDOL_Cleaner");
        DontDestroyOnLoad(temp);
        var ddol = temp.scene;
        Object.Destroy(temp);
        foreach (var root in ddol.GetRootGameObjects())
            Object.Destroy(root);

        UnityEngine.SceneManagement.SceneManager.LoadScene(initialSceneName);
    }

    // Play music clip instantly without fading
    public void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Fade smoothly from current music to new clip, stops any existing fade
    public void PlayMusicWithFade(AudioClip newClip)
    {
        // Don't fade if already playing this clip
        if (audioSource.clip == newClip && audioSource.isPlaying)
            return;

        // Stop current fade to prevent conflicts
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
            currentFade = null;
        }

        currentFade = StartCoroutine(FadeToNewMusic(newClip));
    }

    IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        float elapsed      = 0f;
        float halfDuration = fadeDuration * 0.5f;
        float startVolume  = audioSource.volume;

        // Fade out current music
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t       = Mathf.Clamp01(elapsed / halfDuration);
            float smoothT = t * t * (3f - 2f * t);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, smoothT);
            yield return null;
        }

        // Swap clips
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in new music
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t       = Mathf.Clamp01(elapsed / halfDuration);
            float smoothT = t * t * (3f - 2f * t);
            audioSource.volume = Mathf.Lerp(0f, startVolume, smoothT);
            yield return null;
        }

        audioSource.volume = startVolume;
        currentFade        = null;
    }

    void MarkPersistentObjects()
    {
        foreach (GameObject obj in persistentObjects)
            if (obj)
                DontDestroyOnLoad(obj);
    }

    void CleanUpAndDestroy()
    {
        foreach (GameObject obj in persistentObjects)
            Destroy(obj);
        Destroy(gameObject);
    }
}
