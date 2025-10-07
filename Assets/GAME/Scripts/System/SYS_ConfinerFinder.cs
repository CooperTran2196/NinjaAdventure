// </summary>
// Automatically finds and assigns the confiner collider for CinemachineConfiner2D on scene load.
// </summary>

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SYS_ConfinerFinder : MonoBehaviour
{
    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // Update the confiner when a new scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CinemachineConfiner2D confiner = GetComponent<CinemachineConfiner2D>();
        confiner.BoundingShape2D = GameObject.FindWithTag("Confiner").GetComponent<PolygonCollider2D>();
    }
}
