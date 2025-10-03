using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SYS_SceneTeleport : MonoBehaviour
{
    [Header("References")]
    public string sceneToLoad = "The Name of the Scene to Load";
    public Animator fadeAnimator;   // Animator on the FadeCanvas Image
    public float fadeTime = 0.6f;
    public Vector2 newPlayerPosition;
    private Transform player;

    void Awake()
    {        
        if (!fadeAnimator) Debug.LogError($"{name}: fadeAnimator not set.");
    }

    // Triggered when player enters the teleport trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        player = other.transform;
        fadeAnimator?.Play("FadeOut");       // play FadeOut
        StartCoroutine(LoadRoutine(other.transform));
    }

    // Coroutine to handle the fade and scene loading
    IEnumerator LoadRoutine(Transform player)
    {
        yield return new WaitForSeconds(fadeTime);
        // fadeAnimator?.Play("FadeFromWhite"); // play FadeIn
        player.position = newPlayerPosition; // move player to new position
        SceneManager.LoadScene(sceneToLoad); // load the new scene
    }
}
