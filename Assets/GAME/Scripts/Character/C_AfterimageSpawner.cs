using UnityEngine;
using System.Collections;

public class C_AfterimageSpawner : MonoBehaviour
{
    [Header("Trail")]
    public float spawnInterval = 0.035f;
    public float ghostLifetime = 0.20f;
    public Color ghostTint = new Color(0.7f, 0.7f, 0.7f, 0.7f);
    public int sortingOrderOffset = -1; // draw behind player

    // Cached
    SpriteRenderer srcSR;

    void Awake()
    {
        srcSR ??= GetComponent<SpriteRenderer>();
        if (srcSR == null) Debug.LogWarning("C_AfterimageSpawner: missing SpriteRenderer on the same GameObject.");
    }

    // Spawns a quick burst of ghosts for 'duration' seconds using the given sprite
    public void StartBurst(float duration, Sprite sprite)
    {
        StartCoroutine(BurstRoutine(duration, sprite));
    }

    IEnumerator BurstRoutine(float duration, Sprite sprite)
    {
        float t = 0f;
        while (t < duration)
        {
            SpawnGhost(sprite);
            yield return new WaitForSeconds(spawnInterval);
            t += spawnInterval;
        }
    }

    void SpawnGhost(Sprite sprite)
    {
        if (srcSR == null || sprite == null) return;

        var g = new GameObject("Afterimage");
        g.transform.position = transform.position;
        g.transform.rotation = transform.rotation;
        g.transform.localScale = transform.localScale;

        var gsr = g.AddComponent<SpriteRenderer>();
        gsr.sprite = sprite;
        gsr.flipX = srcSR.flipX;
        gsr.flipY = srcSR.flipY;
        gsr.sortingLayerID = srcSR.sortingLayerID;
        gsr.sortingOrder = srcSR.sortingOrder + sortingOrderOffset;
        gsr.color = ghostTint;

        StartCoroutine(FadeAndDestroy(gsr));
    }

    IEnumerator FadeAndDestroy(SpriteRenderer gsr)
    {
        float t = 0f;
        var start = gsr.color;
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
