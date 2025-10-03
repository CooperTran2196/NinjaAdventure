using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C_SceneTeleport : MonoBehaviour
{
    [Header("References")]
    public string sceneToLoad = "The Name of the Scene to Load";
    public Animator fadeAnimator;   // Animator on the FadeCanvas Image
    public float fadeTime = 0.6f;
    public Vector2 newPlayerPosition;
    private Transform player;

    [Header("Target Scene")]
    string playerTag = "Player";

    void Awake()
    {        
        if (!fadeAnimator) Debug.LogError($"{name}: fadeAnimator not set.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        player = other.transform;
        fadeAnimator?.Play("FadeToWhite");       // play FadeOut

        StartCoroutine(LoadRoutine(other.transform));
    }

    IEnumerator LoadRoutine(Transform player)
    {
        yield return new WaitForSeconds(fadeTime);     // wait for fade
        player.position = newPlayerPosition; // move player to new position
        SceneManager.LoadScene(sceneToLoad);           // load the new scene
    }
}
