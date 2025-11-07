using UnityEngine;
using System.Collections;

public class ENV_Gate : MonoBehaviour
{
    [Header("Gate opens when entity dies, then self-destructs to reveal open gate underneath")]
    [Header("MUST wire MANUALLY in Inspector")]
    public C_Health targetHealth;

    [Header("Settings")]
    public float fadeDuration = 1.5f;

    // Runtime state
    SpriteRenderer[] spriteRenderers;

    void Awake()
    {
        if (!targetHealth) { Debug.LogError($"{name}: targetHealth is missing!", this); return; }

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    void OnEnable()  => targetHealth.OnDied += OpenGate;
    void OnDisable() => targetHealth.OnDied -= OpenGate;

    void OpenGate()
    {
        // Play gate break sound
        AudioClip breakSound = SYS_GameManager.Instance?.sys_SoundManager?.GetRandomObjectBreak();
        AudioSource.PlayClipAtPoint(breakSound, transform.position);

        Debug.Log($"{name}: Gate opening! Target defeated.");

        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        float elapsed = 0f;

        // Store original colors
        Color[] originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            originalColors[i] = spriteRenderers[i].color;

        // Fade out all sprites
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeDuration);

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                Color newColor = originalColors[i];
                newColor.a = alpha;
                spriteRenderers[i].color = newColor;
            }

            yield return null;
        }

        // Ensure fully transparent
        foreach (var sr in spriteRenderers)
        {
            Color color = sr.color;
            color.a = 0f;
            sr.color = color;
        }

        // Destroy this GameObject (reveals LDtk open gate underneath)
        Destroy(gameObject);
    }
}
