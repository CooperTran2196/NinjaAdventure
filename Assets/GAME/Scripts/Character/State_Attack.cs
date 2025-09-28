using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class State_Attack : MonoBehaviour
{
    [Header("Animation States")]
    public string idleState   = "Idle";
    public string attackState = "Attack"; // clip name (no bools)
    public LayerMask playerLayer;

    [Header("Timing")]
    public float attackCooldown = 0.80f;
    public float attackDuration = 0.45f;
    public float hitDelay       = 0.15f;

    // Ranges are injected by controller
    float detectionRange = 3f; // not used for logic here, but kept for gizmos if you want
    float attackRange    = 1.2f;

    [Header("Weapon")]
    public W_Base activeWeapon;

    [Header("Knockback")]
    public float knockbackRecovery = 30f;

    // Cache
    Rigidbody2D rb;
    Animator anim;

    // Runtime
    Transform target;
    Vector2 knockback, lastFace = Vector2.down;
    float cooldownTimer;
    bool isAttacking;
    string lastPlayed;

    public bool IsAttacking => isAttacking;

    void Awake()
    {
        rb   ??= GetComponent<Rigidbody2D>();
        anim ??= GetComponent<Animator>();
        activeWeapon ??= GetComponentInChildren<W_Base>();
    }

    void OnEnable()
    {
        lastPlayed = null;
        PlayIfChanged(idleState);
        StartCoroutine(AttackLoop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        rb.linearVelocity = Vector2.zero;
        lastPlayed = null;
    }

    public void SetTarget(Transform t) => target = t;
    public void SetRanges(float detect, float atk) { detectionRange = detect; attackRange = atk; }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        // No movement here; controller handles Chase. We only face & strike.
    }

    void FixedUpdate()
    {
        if (knockback.sqrMagnitude > 0f)
        {
            float step = knockbackRecovery * Time.fixedDeltaTime;
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, step);
            rb.linearVelocity = knockback;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    IEnumerator AttackLoop()
    {
        var wait = new WaitForSeconds(0.06f); // light polling

        while (true)
        {
            if (target)
            {
                // Use collider-based ring test so "touching" the ring counts
                bool inInner = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);

                Vector2 to = (Vector2)target.position - (Vector2)transform.position;
                float d = to.magnitude;
                Vector2 dir = d > 0.0001f ? to.normalized : lastFace;

                // keep facing with idle floats while we wait
                UpdateIdleFacing(dir);

                // START ATTACK immediately on touch (if off cooldown)
                if (!isAttacking && inInner && cooldownTimer <= 0f)
                    StartCoroutine(AttackRoutine(dir));
            }

            yield return wait;
        }
    }



    IEnumerator AttackRoutine(Vector2 dirAtStart)
    {
        isAttacking = true;

        // Lock attack facing into atkX/atkY once at the start
        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);

        // play full clip (non-interruptible)
        PlayIfChanged(attackState);

        // movement stays zero while striking; just hold idle facing
        UpdateIdleFacing(lastFace);

        yield return new WaitForSeconds(hitDelay);
        activeWeapon?.Attack(lastFace);

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitDelay));
        cooldownTimer = attackCooldown;
        isAttacking = false;

        // fall back to idle pose; controller will decide next state
        PlayIfChanged(idleState);
    }

    void UpdateIdleFacing(Vector2 faceDir)
    {
        // moveX/moveY are zero in attack state; idleX/idleY carry facing
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);

        Vector2 f = faceDir.sqrMagnitude > 0f ? faceDir.normalized : lastFace;
        anim.SetFloat("idleX", f.x);
        anim.SetFloat("idleY", f.y);
    }

    void PlayIfChanged(string stateName)
    {
        if (lastPlayed == stateName) return;
        anim.Play(stateName);
        lastPlayed = stateName;
    }

    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.65f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
