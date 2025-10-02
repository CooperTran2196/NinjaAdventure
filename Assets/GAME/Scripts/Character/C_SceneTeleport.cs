using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C_SceneTeleport : MonoBehaviour
{
    [Header("References")]
    public string sceneToLoad = "The Name of the Scene to Load";
    public Animator fadeAnimator;   // Animator on the FadeCanvas Image
    public float fadeTime = 0.6f;
    Collider2D col;

    [Header("Target Scene")]
    string playerTag = "Player";



    void Awake()
    {
        col ??= GetComponent<Collider2D>();
        Debug.LogError($"{name}: needs a Collider2D set as Trigger.");
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

        fadeAnimator?.Play("FadeToWhite", 0, 0f);       // play FadeOut
        yield return new WaitForSeconds(fadeTime);     // wait for fade

        SceneManager.LoadScene(sceneToLoad);           // load the new scene
    }
}
