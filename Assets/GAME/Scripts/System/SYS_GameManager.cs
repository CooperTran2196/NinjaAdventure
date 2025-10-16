using System.Collections;
using UnityEngine;

public class SYS_GameManager : MonoBehaviour
{
    public static SYS_GameManager Instance;

    [Header("References")]
    public D_Manager         d_Manager;
    public D_HistoryTracker  d_HistoryTracker;
    public SYS_Fader         sys_Fader;
    public SHOP_Manager      shop_Manager;
    public INV_ItemInfo      itemInfoPopup;  // Shared info popup for Shop and Inventory

    [Header("Restart")]
    [SerializeField] private string initialSceneName = "Level1";
    [SerializeField] private string initialSpawnId = "Start";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeDuration = 1.5f; // Tunable fade time

    // Internal fade state
    private Coroutine currentFade;

    [Header("Persistent Objects")]
    public GameObject[] persistentObjects; // Objects to persist across scenes

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

        d_Manager ??= FindFirstObjectByType<D_Manager>();
        d_HistoryTracker ??= FindFirstObjectByType<D_HistoryTracker>();
        shop_Manager ??= FindFirstObjectByType<SHOP_Manager>();
        sys_Fader    ??= FindFirstObjectByType<SYS_Fader>();
        itemInfoPopup ??= FindFirstObjectByType<INV_ItemInfo>();
        audioSource ??= GetComponent<AudioSource>();

        if (!d_Manager)         Debug.LogWarning("SYS_GameManager: Dialogue Manager is missing.");
        if (!d_HistoryTracker)  Debug.LogWarning("SYS_GameManager: Dialogue History Tracker is missing.");
        if (!shop_Manager)      Debug.LogWarning("SYS_GameManager: Shop Manager is missing.");
        if (!sys_Fader)         Debug.LogWarning("SYS_GameManager: Fader is missing.");
        if (!itemInfoPopup)     Debug.LogWarning("SYS_GameManager: ItemInfoPopup is missing.");
        if (!audioSource)       Debug.LogWarning("SYS_GameManager: AudioSource is missing.");

        if (itemInfoPopup) itemInfoPopup.Hide(); // Start hidden

        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    // Restart the game by loading the initial scene and resetting time scale
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

    // Play music clip centrally (instant switch, no fade)
    public void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Fade smoothly from current music to new clip.
    /// Stops any existing fade before starting new one.
    /// </summary>
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

    private IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        float elapsed = 0f;
        float halfDuration = fadeDuration * 0.5f;
        float startVolume = audioSource.volume;

        // Fade out current music (ease-out curve using smoothstep)
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float smoothT = t * t * (3f - 2f * t); // Smoothstep
            audioSource.volume = Mathf.Lerp(startVolume, 0f, smoothT);
            yield return null;
        }

        // Swap clips at silent point
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in new music (ease-in curve using smoothstep)
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float smoothT = t * t * (3f - 2f * t); // Smoothstep
            audioSource.volume = Mathf.Lerp(0f, startVolume, smoothT);
            yield return null;
        }

        audioSource.volume = startVolume; // Ensure final volume is exact
        currentFade = null; // Clear reference
    }

    // Mark specified objects to not be destroyed on scene load
    void MarkPersistentObjects()
    {
        foreach (var obj in persistentObjects)
            if (obj)
                DontDestroyOnLoad(obj);
    }

    // Clean up persistent objects and destroy this instance
    void CleanUpAndDestroy()
    {
        foreach (GameObject obj in persistentObjects)
            Destroy(obj);
        Destroy(gameObject);
    }
}
