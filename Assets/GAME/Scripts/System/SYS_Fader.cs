using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SYS_Fader : MonoBehaviour
{
    public Animator animator;
    public float fadeTime = 0.5f;

    void Awake()
    {
        animator ??= GetComponentInChildren<Animator>(true);
        if (!animator) Debug.LogError("SYS_Fader: Animator missing.");
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(DoFadeToScene(sceneName));
    }

    IEnumerator DoFadeToScene(string sceneName)
    {
        animator.Play("FadeOut");
        yield return new WaitForSeconds(fadeTime);
        // Load the scene
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        yield return new WaitForSeconds(fadeTime);
        animator.Play("FadeIn");      // reveal
    }
}
