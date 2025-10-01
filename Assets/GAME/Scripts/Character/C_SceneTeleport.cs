using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C_SceneTeleport : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] string sceneToLoad = "level 1 room";
    [SerializeField] string playerTag    = "Player";

    [Header("Fade")]
    [SerializeField] Animator fadeAnimator;   // Animator on the FadeCanvas Image
    [SerializeField] string fadeOutState = "FadeOut";
    [SerializeField] float  fadeTime     = 0.6f; // set to your FadeOut clip length

    Collider2D col;

    void Awake()
    {
        col ??= GetComponent<Collider2D>();
        if (!col) Debug.LogError($"{name}: needs a Collider2D set as Trigger.");
        if (!fadeAnimator) Debug.LogError($"{name}: fadeAnimator not set.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        StartCoroutine(LoadRoutine(other.transform));
    }

    IEnumerator LoadRoutine(Transform player)
    {
        col.enabled = false;                            // avoid double-trigger
        player.GetComponent<P_Movement>()?.SetDisabled(true);

        fadeAnimator?.Play(fadeOutState, 0, 0f);       // play FadeOut
        yield return new WaitForSeconds(fadeTime);     // wait for fade

        SceneManager.LoadScene(sceneToLoad);           // load the new scene
    }
}
