using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SYS_Fader : MonoBehaviour
{
    [Header("References")]
    Animator anim;

    [Header("Fade Settings")]
    public float fadeTime = 0.5f;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        
        // Make animator work during Time.timeScale = 0 (for resurrection/pause effects)
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    // Fade to a new scene with fade out/in transition
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(DoFadeToScene(sceneName));
    }

    // Fade out, load the scene, then fade in
    IEnumerator DoFadeToScene(string sceneName)
    {
        anim.Play("FadeOut");
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        yield return new WaitForSeconds(fadeTime);
        anim.Play("FadeIn");      // reveal
    }

    // Play fade animation (works even during Time.timeScale = 0)
    public void PlayFade(string animationName)
    {
        anim.Play(animationName);
    }
}
