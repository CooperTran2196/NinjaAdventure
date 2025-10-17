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
        anim ??= GetComponentInChildren<Animator>(true);

        if (!anim) { Debug.LogError($"{name}: Animator is missing!", this); return; }
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
}
