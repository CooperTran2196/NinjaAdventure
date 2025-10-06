using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("SYS/Fader (Persistent)")]
public class SYS_Fader : MonoBehaviour
{
    public static SYS_Fader I;                  // simple singleton
    public Animator animator;                   // assign the Image's Animator
    public float fadeTime = 0.5f;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
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
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        yield return op;                        // stream in next scene
        yield return new WaitForSeconds(fadeTime);
        animator.Play("FadeIn");      // reveal
    }
}
