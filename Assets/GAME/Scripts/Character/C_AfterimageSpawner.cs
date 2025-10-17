using System.Collections;
using UnityEngine;

public class C_AfterimageSpawner : MonoBehaviour
{
    [Header("References")]
    SpriteRenderer sr;

    [Header("Trail Settings")]
    public float spawnInterval      = 0.035f;
    public float ghostLifetime      = 0.20f;
    public Color ghostTint          = new Color(0.7f, 0.7f, 0.7f, 0.7f);
    public int   sortingOrderOffset = -1;

    void Awake()
    {
        sr ??= GetComponent<SpriteRenderer>();

        if (!sr) { Debug.LogError($"{name}: SpriteRenderer is missing!", this); return; }
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
        // Create new GameObject for afterimage
        var g = new GameObject("Afterimage");
        g.transform.SetPositionAndRotation(transform.position, transform.rotation);
        g.transform.localScale = transform.localScale;

        // Setup sprite renderer with same properties as original
        var gsr = g.AddComponent<SpriteRenderer>();
        gsr.sprite         = sprite;
        gsr.flipX          = flipX;
        gsr.flipY          = flipY;
        gsr.sortingLayerID = sr.sortingLayerID;
        gsr.sortingOrder   = sr.sortingOrder + sortingOrderOffset;
        gsr.color          = ghostTint;

        // Start fade-out coroutine
        StartCoroutine(FadeAndDestroy(gsr));
    }

    IEnumerator FadeAndDestroy(SpriteRenderer gsr)
    {
        float t = 0f;
        Color start = gsr.color;
        
        // Gradually fade out over ghostLifetime
        while (t < ghostLifetime)
        {
            // Lerp alpha from start to 0 (fully transparent)
            float a = Mathf.Lerp(start.a, 0f, t / ghostLifetime);
            gsr.color = new Color(start.r, start.g, start.b, a);
            
            t += Time.deltaTime;
            yield return null;
        }
        
        // Destroy afterimage GameObject after fade completes
        Destroy(gsr.gameObject);
    }
}
