using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class C_FX
{
    public static IEnumerator Flash(SpriteRenderer sprite, float duration, Color baseColor)
    {
        float a = sprite.color.a;                         // keep current alpha
        sprite.color = new Color(1f, 0.3f, 0.3f, a);      // tint red
        yield return new WaitForSeconds(duration);
        sprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, a); // restore
    }

    // unchanged
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
