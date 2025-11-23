using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SYS_ConfinerFinder : MonoBehaviour
{
    [Header("References")]
    CinemachineConfiner2D confiner;

    void Awake()
    {
        confiner ??= GetComponent<CinemachineConfiner2D>();

        if (!confiner) { Debug.LogError($"{name}: CinemachineConfiner2D is missing!", this); return; }
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // When a new scene is loaded, find the confiner object and assign it to the CinemachineConfiner2D
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject confinerObj = GameObject.FindWithTag("Confiner");
        if (confinerObj)
        {
            confiner.BoundingShape2D = confinerObj.GetComponent<PolygonCollider2D>();
        }
        else
        {
            Debug.LogWarning($"{name}: No GameObject with tag 'Confiner' found in scene {scene.name}!", this);
        }
    }
}
