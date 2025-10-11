using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SYS_ConfinerFinder : MonoBehaviour
{
    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // When a new scene is loaded, find the confiner object and assign it to the CinemachineConfiner2D
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CinemachineConfiner2D confiner = GetComponent<CinemachineConfiner2D>();
        confiner.BoundingShape2D = GameObject.FindWithTag("Confiner").GetComponent<PolygonCollider2D>();
    }
}
