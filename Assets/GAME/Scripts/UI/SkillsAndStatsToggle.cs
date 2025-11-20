using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillsAndStatsToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image buttonImage;
    [SerializeField] P_Exp p_Exp;

    [Header("Blink Settings")]
    [SerializeField] float blinkSpeed = 1f;        // Seconds per fade cycle (1.0 = slow, 0.5 = fast)
    [SerializeField] float minAlpha   = 0.5f;      // Minimum alpha during fade
    [SerializeField] float maxAlpha   = 1f;        // Maximum alpha during fade

    Coroutine blinkCoroutine;
    Color     originalColor;

    void Awake()
    {
        buttonImage ??= GetComponentInChildren<Image>();
        p_Exp       ??= FindFirstObjectByType<P_Exp>();

        if (!buttonImage) Debug.LogError("SkillsAndStatsToggle: buttonImage is missing!", this);
        if (!p_Exp)       Debug.LogError("SkillsAndStatsToggle: p_Exp is missing!", this);

        if (buttonImage) originalColor = buttonImage.color;
    }

    void OnEnable()
    {
        if (p_Exp != null)
        {
            p_Exp.OnSPChanged += HandleSPChanged;
            
            // Initialize blink state based on current SP
            HandleSPChanged(p_Exp.skillPoints);
        }
    }

    void OnDisable()
    {
        if (p_Exp != null)
            p_Exp.OnSPChanged -= HandleSPChanged;

        StopBlinking();
    }

    void HandleSPChanged(int sp)
    {
        if (sp > 0)
            StartBlinking();
        else
            StopBlinking();
    }

    void StartBlinking()
    {
        if (blinkCoroutine != null) return; // Already blinking
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // Reset to original color
        if (buttonImage)
            buttonImage.color = originalColor;
    }

    IEnumerator BlinkRoutine()
    {
        float time = 0f;
        while (true)
        {
            time += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(time / blinkSpeed, 1f));
            buttonImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    }
}
