using UnityEngine;

public class Enemy_HealthBarManager : MonoBehaviour
{
    [Tooltip("Check for bosses; unchecked = normal mob")]
    public bool isBoss = false;

    [Header("Prefabs")]
    public GameObject mobBarPrefab;
    public GameObject bossBarPrefab;

    void Start()
    {
        /* choose prefab */
        GameObject prefab = isBoss ? bossBarPrefab : mobBarPrefab;
        if (prefab == null) { Debug.LogWarning($"{name} missing health-bar prefab"); return; }

        /* spawn bar as child */
        GameObject bar = Instantiate(prefab, transform);
        bar.transform.localPosition = Vector3.zero;      // centred on enemy
    }
}
