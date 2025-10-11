using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]

public class C_FX : MonoBehaviour
{
    [Header("References")]
    SpriteRenderer sr;

    [Header("Flash")]
    public float flashDuration = 0.1f;
    public Color healTint   = new Color(0.3f, 1f, 0.3f, 1f);
    public Color damageTint = new Color(1f, 0.3f, 0.3f, 1f);

    [Header("Death")]
    public float deathFadeTime = 1.5f;
    public bool destroySelfOnDeath = true;
    Color baseRGB;

    void Awake()
    {
        sr ??= GetComponent<SpriteRenderer>();
        if (!sr) Debug.LogError($"{name}: SpriteRenderer is missing in C_FX");

        baseRGB = sr.color;
    }

    public void FlashOnDamaged() => StartCoroutine(Flash(damageTint));
    public void FlashOnHealed()  => StartCoroutine(Flash(healTint));

    IEnumerator Flash(Color tint)
    {
        float a = sr.color.a;
        sr.color = new Color(tint.r, tint.g, tint.b, a);
        yield return new WaitForSeconds(flashDuration);
        sr.color = new Color(baseRGB.r, baseRGB.g, baseRGB.b, a);
    }

    public IEnumerator FadeAndDestroy(GameObject go)
    {
        float t = 0f;
        var c = sr.color;
        while (t < deathFadeTime)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / deathFadeTime);
            sr.color = new Color(c.r, c.g, c.b, k);
            yield return null;
        }
        if (destroySelfOnDeath) Destroy(go);
        else
        {
            // Player path: restore full alpha so it's ready when re-enabled on restart
            sr.color = new Color(c.r, c.g, c.b, 1f);
            go.SetActive(false);         // keep GO for Ending UI → RestartGame to re-enable
        }
    
    }
}