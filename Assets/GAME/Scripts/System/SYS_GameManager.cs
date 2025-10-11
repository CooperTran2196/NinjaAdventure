using UnityEngine;

public class SYS_GameManager : MonoBehaviour
{
    public static SYS_GameManager Instance;

    [Header("References")]
    public D_Manager         d_Manager;
    public D_HistoryTracker  d_HistoryTracker;
    public SYS_Fader         sys_Fader;
    public SHOP_Manager      shop_Manager;

    [Header("Restart")]
    [SerializeField] private string initialSceneName = "Level1";
    [SerializeField] private string initialSpawnId = "Start";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

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
        audioSource ??= GetComponent<AudioSource>();

        if (!d_Manager)         Debug.LogWarning("SYS_GameManager: Dialogue Manager is missing.");
        if (!d_HistoryTracker)  Debug.LogWarning("SYS_GameManager: Dialogue History Tracker is missing.");
        if (!shop_Manager)      Debug.LogWarning("SYS_GameManager: Shop Manager is missing.");
        if (!sys_Fader)         Debug.LogWarning("SYS_GameManager: Fader is missing.");
        if (!audioSource)       Debug.LogWarning("SYS_GameManager: AudioSource is missing.");

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

    // Play music clip centrally
    public void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
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
