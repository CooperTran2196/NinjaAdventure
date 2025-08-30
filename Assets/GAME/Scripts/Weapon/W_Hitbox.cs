using UnityEngine;

[DisallowMultipleComponent]
public class W_Hitbox : MonoBehaviour
{
    public Collider2D col;

    int damage;
    LayerMask targets;
    GameObject owner;
    float timeLeft;

    void Awake()
    {
        col ??= GetComponent<Collider2D>();
        if (col == null) Debug.LogError($"{name}: Collider2D missing.");
        else col.isTrigger = true;

        enabled = false;
        if (col) col.enabled = false;
    }

    public void Arm(int dmg, LayerMask targetMask, GameObject ownerGO, float duration)
    {
        damage   = Mathf.Max(0, dmg);
        targets  = targetMask;
        owner    = ownerGO;
        timeLeft = duration;

        enabled = true;
        if (col) col.enabled = true;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            if (col) col.enabled = false;
            enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (owner && other.transform.IsChildOf(owner.transform)) return;
        if (((1 << other.gameObject.layer) & targets.value) == 0) return;

        // Player uses ChangeHealth; Enemy uses TakeDamage
        var pc = other.GetComponent<P_Combat>();
        if (pc != null) { pc.ChangeHealth(-damage); return; }   // 【turn16file11†P_Combat.cs†L58-L66】

        var ec = other.GetComponent<E_Combat>();
        if (ec != null) { ec.ChangeHealth(-damage); return; }      // 【turn16file7†AllEnemyScripts.txt†L68-L76】
    }
}
