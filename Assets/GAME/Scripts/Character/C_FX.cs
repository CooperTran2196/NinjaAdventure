using System.Collections;
using UnityEngine;

public class C_FX : MonoBehaviour
{
    [Header("References")]
    SpriteRenderer sprite;

    [Header("Flash")]
    public float flashDuration = 0.1f;
    public Color healTint   = new Color(0.3f, 1f, 0.3f, 1f);
    public Color damageTint = new Color(1f, 0.3f, 0.3f, 1f);

    [Header("Death")]
    public float deathFadeTime = 1.5f;

    Color baseRGB;

    void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();
        if (!sprite) Debug.LogError($"{name}: SpriteRenderer in C_FX missing.", this);

        baseRGB = sprite.color;
    }

    public void FlashOnDamaged() => StartCoroutine(Flash(damageTint));
    public void FlashOnHealed()  => StartCoroutine(Flash(healTint));

    IEnumerator Flash(Color tint)
    {
        float a = sprite.color.a;
        sprite.color = new Color(tint.r, tint.g, tint.b, a);
        yield return new WaitForSeconds(flashDuration);
        sprite.color = new Color(baseRGB.r, baseRGB.g, baseRGB.b, a);
    }

    public IEnumerator FadeAndDestroy(GameObject go)
    {
        float t = 0f;
        var c = sprite.color;
        while (t < deathFadeTime)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / deathFadeTime);
            sprite.color = new Color(c.r, c.g, c.b, k);
            yield return null;
        }
        Destroy(go);
    }
}