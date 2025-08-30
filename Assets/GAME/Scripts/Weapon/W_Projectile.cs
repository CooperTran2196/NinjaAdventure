using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
public class W_Projectile : MonoBehaviour
{
    public Rigidbody2D rb;     // optional
    public float speed = 8f;
    public float life  = 2f;

    int damage;
    LayerMask targets;
    GameObject owner;
    Vector2 dir;
    float timer;

    void Awake()
    {
        rb ??= GetComponent<Rigidbody2D>();
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
        timer = life;
    }

    public void Fire(Vector2 direction, int dmg, LayerMask targetMask, float speedOverride, float lifeOverride, GameObject ownerGO)
    {
        dir     = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.down;
        damage  = Mathf.Max(0, dmg);
        targets = targetMask;
        owner   = ownerGO;
        if (speedOverride > 0f) speed = speedOverride;
        if (lifeOverride  > 0f) life  = lifeOverride;
        timer = life;

        if (rb) rb.linearVelocity = dir * speed;
    }

    void Update()
    {
        if (rb == null) transform.position += (Vector3)(dir * speed * Time.deltaTime);

        timer -= Time.deltaTime;
        if (timer <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (owner && other.transform.IsChildOf(owner.transform)) return;
        if (((1 << other.gameObject.layer) & targets) == 0) return;

        var pc = other.GetComponent<P_Combat>();
        if (pc != null) { pc.ChangeHealth(-damage); Destroy(gameObject); return; }  // 【turn16file11†P_Combat.cs†L58-L66】

        var ec = other.GetComponent<E_Combat>();
        if (ec != null) { ec.ChangeHealth(-damage); Destroy(gameObject); return; }     // 【turn16file7†AllEnemyScripts.txt†L68-L76】
    }
}
