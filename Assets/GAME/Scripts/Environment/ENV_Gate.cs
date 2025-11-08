using System.Collections;
using UnityEngine;

public class ENV_Gate : MonoBehaviour
{
    [Header("Gate fades out and self-destructs when target dies")]
    [Header("MUST wire MANUALLY in Inspector")]
    public C_Health targetHealth;

    [Header("Settings")]
    public float fadeDuration = 1.5f;

    // Runtime state
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (!targetHealth) { Debug.LogError($"{name}: targetHealth is missing!", this); return; }
        if (!sr)           { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
    }

    void OnEnable()  => targetHealth.OnDied += DestroyGate;
    void OnDisable() => targetHealth.OnDied -= DestroyGate;

    void DestroyGate()
    {
        AudioClip breakSound = SYS_GameManager.Instance.sys_SoundManager.GetRandomObjectBreak();
        AudioSource.PlayClipAtPoint(breakSound, transform.position);

        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        float elapsed = 0f;
        Color originalColor = sr.color;

        // Fade out sprite
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeDuration);

            Color newColor = originalColor;
            newColor.a = alpha;
            sr.color = newColor;

            yield return null;
        }

        // Ensure fully transparent
        Color finalColor = originalColor;
        finalColor.a = 0f;
        sr.color = finalColor;

        Destroy(gameObject);
    }
}
