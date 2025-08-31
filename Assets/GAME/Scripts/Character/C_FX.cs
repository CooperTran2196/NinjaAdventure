using System.Collections;
using UnityEngine;

public static class C_FX
{
    // Tint red briefly then restore (scaled time)
    public static IEnumerator Flash(SpriteRenderer sprite, float duration)
    {
        var original = sprite.color;
        sprite.color = new Color(1f, 0.3f, 0.3f, original.a);
        yield return new WaitForSeconds(duration);
        sprite.color = original;
    }

    // Fade alpha 1 -> 0 then destroy
    public static IEnumerator FadeAndDestroy(SpriteRenderer sprite, float deathFadeTime, GameObject go)
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
        Object.Destroy(go);
    }
}
