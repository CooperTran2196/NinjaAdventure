using UnityEngine;

public class SYS_GameManager : MonoBehaviour
{
    public static SYS_GameManager Instance;

    [Header("References")]
    public D_Manager d_Manager;
    public D_HistoryTracker d_HistoryTracker;
    public SYS_Fader sys_Fader; // Centralized reference to Fader

    [Header("Persistent Objects")]
    public GameObject[] persistentObjects; // Objects to persist across scenes

    // Ensure only one instance of the GameManager exists
    void Awake()
    {
        // If an instance already exists, destroy this one
        if (Instance != null)
        {
            CleanUpAndDestroy();
            return;
        }
        else
        {
            // Set the instance and mark this object to not be destroyed on load
            Instance = this;
            DontDestroyOnLoad(gameObject);
            MarkPersistentObjects();

            // Fallback
            sys_Fader ??= FindFirstObjectByType<SYS_Fader>(FindObjectsInactive.Include);
            if (!sys_Fader) Debug.LogWarning("SYS_GameManager: Fader reference not set. Scene transitions will be immediate.");

        }
    }

    // Mark specified objects to not be destroyed on scene load
    void MarkPersistentObjects()
    {
        foreach (var obj in persistentObjects)
        {
            if (obj)
            {
                DontDestroyOnLoad(obj);
            }
        }
    }

    // Clean up persistent objects and destroy this instance
    void CleanUpAndDestroy()
    {
        foreach (GameObject obj in persistentObjects)
        {
            Destroy(obj);
        }
        Destroy(gameObject);
    }
}
