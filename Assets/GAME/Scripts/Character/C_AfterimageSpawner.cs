using System.Collections;
using UnityEngine;

public class C_AfterimageSpawner : MonoBehaviour
{
    [Header("Trail")]
    public float spawnInterval = 0.035f;
    public float ghostLifetime = 0.20f;
    public Color ghostTint = new Color(0.7f, 0.7f, 0.7f, 0.7f);
    public int   sortingOrderOffset = -1;

    SpriteRenderer sr;

    void Awake()
    {
        sr ??= GetComponent<SpriteRenderer>();

        if (!sr) Debug.LogWarning("C_AfterimageSpawner: SpriteRenderer missing on this GameObject.");
    }

    // Burst where every ghost uses the SAME locked sprite & flips (captured at dodge start)
    public void StartBurst(float duration, Sprite lockedSprite, bool lockedFlipX, bool lockedFlipY)
    {
        StartCoroutine(BurstRoutine(duration, lockedSprite, lockedFlipX, lockedFlipY));
    }

    IEnumerator BurstRoutine(float duration, Sprite lockedSprite, bool lockedFlipX, bool lockedFlipY)
    {
        // Spawn one immediately so the trail starts right away
        SpawnGhost(lockedSprite, lockedFlipX, lockedFlipY);

        float t = 0f;
        while (t < duration)
        {
            yield return new WaitForSeconds(spawnInterval);
            t += spawnInterval;
            SpawnGhost(lockedSprite, lockedFlipX, lockedFlipY);
        }
    }

    void SpawnGhost(Sprite sprite, bool flipX, bool flipY)
    {
        // Assumes sr exists & sprite provided (per your style: Inspector preconditions are correct)
        var g = new GameObject("Afterimage");
        g.transform.SetPositionAndRotation(transform.position, transform.rotation);
        g.transform.localScale = transform.localScale;

        var gsr = g.AddComponent<SpriteRenderer>();
        gsr.sprite          = sprite;
        gsr.flipX           = flipX;
        gsr.flipY           = flipY;
        gsr.sortingLayerID  = sr.sortingLayerID;
        gsr.sortingOrder    = sr.sortingOrder + sortingOrderOffset;
        gsr.color           = ghostTint;

        StartCoroutine(FadeAndDestroy(gsr));
    }

    IEnumerator FadeAndDestroy(SpriteRenderer gsr)
    {
        float t = 0f;
        Color start = gsr.color;
        while (t < ghostLifetime)
        {
            float a = Mathf.Lerp(start.a, 0f, t / ghostLifetime);
            gsr.color = new Color(start.r, start.g, start.b, a);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(gsr.gameObject);
    }
}
