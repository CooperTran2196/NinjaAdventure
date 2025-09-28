using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class State_Attack : MonoBehaviour
{
    [Header("Detection (two rings)")]
    public LayerMask playerLayer;
    [Min(0.1f)] public float detectionRadius = 3f; // outer ring
    [Min(0.1f)] public float attackRange     = 1.2f; // inner ring

    [Header("Chase")]
    public float moveSpeed   = 2.2f;
    public float stopBuffer  = 0.10f; // hover just outside inner ring

    [Header("Attack")]
    public float thinkInterval   = 0.08f;  // polling cadence
    public float attackCooldown  = 0.80f;
    public float attackDuration  = 0.45f;  // full, uninterruptible clip length
    public float hitDelay        = 0.15f;  // hit timing inside the clip
    public string attackTrigger  = "Attack";

    [Header("Weapon (auto-cached)")]
    public W_Base activeWeapon; // any W_Base (melee/ranged)

    [Header("Knockback")]
    public float knockbackRecovery = 30f;  // units/sec back to zero

    // Cached
    Rigidbody2D rb;
    Animator anim;

    // Runtime
    Transform target;
    Vector2 velocity, knockback;
    Vector2 lastMove;       // remembered for idleX/idleY
    float cooldownTimer;
    bool isAttacking;

    public bool IsAttacking => isAttacking;

    void Awake()
    {
        rb   ??= GetComponent<Rigidbody2D>();
        anim ??= GetComponent<Animator>();
        activeWeapon ??= GetComponentInChildren<W_Base>();

        if (!rb)   Debug.LogError($"{name}: Rigidbody2D missing.");
        if (!anim) Debug.LogError($"{name}: Animator missing.");
        if (!activeWeapon) Debug.LogWarning($"{name}: No weapon found; attacks will be animation-only.");
    }

    void OnEnable()
    {
        StartCoroutine(ThinkLoop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        velocity    = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        // Animator floats are set inside ThinkLoop each tick; nothing extra here.
    }

    void FixedUpdate()
    {
        // Apply motion + knockback
        Vector2 final = velocity + knockback;
        rb.linearVelocity = final;

        if (knockback.sqrMagnitude > 0f)
        {
            float step = knockbackRecovery * Time.fixedDeltaTime;
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, step);
        }
    }

    public void SetTarget(Transform t) => target = t;

    IEnumerator ThinkLoop()
    {
        var wait = new WaitForSeconds(thinkInterval);

        while (true)
        {
            // Acquire/validate target using outer ring if missing
            if (!target)
            {
                var col = Physics2D.OverlapCircle((Vector2)transform.position, detectionRadius, playerLayer);
                target = col ? col.transform : null;
            }

            if (target)
            {
                Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
                float   dist     = toTarget.magnitude;
                bool inOuter = dist <= detectionRadius;
                bool inInner = dist <= attackRange;

                if (!isAttacking)
                {
                    if (inOuter && !inInner)
                    {
                        // CHASE
                        Vector2 dir = toTarget.normalized;
                        velocity = dir * moveSpeed;

                        // stop just before entering the strike ring to avoid jitter
                        if (dist <= (attackRange + stopBuffer))
                            velocity = Vector2.zero;

                        UpdateAnimFloats(velocity);
                    }
                    else
                    {
                        // HOLD (either inner on cooldown, or lost outer)
                        velocity = Vector2.zero;
                        UpdateAnimFloats(Vector2.zero);
                    }

                    // START STRIKE
                    if (inInner && cooldownTimer <= 0f)
                        StartCoroutine(AttackRoutine(toTarget.normalized));
                }
                else
                {
                    // Attacking: do not move, but keep idle facing consistent
                    velocity = Vector2.zero;
                    UpdateAnimFloats(Vector2.zero);
                }

                // If not attacking and lost outer ring, controller will flip us out
            }
            else
            {
                // No target: stand still; controller will exit this state
                velocity = Vector2.zero;
                UpdateAnimFloats(Vector2.zero);
            }

            yield return wait;
        }
    }

    IEnumerator AttackRoutine(Vector2 dirAtStart)
    {
        isAttacking = true;

        // Face once at the start of the clip; animation plays fully (no bools)
        lastMove = dirAtStart.sqrMagnitude > 0f ? dirAtStart.normalized : lastMove;
        anim?.SetTrigger(attackTrigger);

        // Lock movement during the clip
        velocity = Vector2.zero;
        UpdateAnimFloats(Vector2.zero); // keeps idleX/idleY = lastMove

        // Hit timing
        yield return new WaitForSeconds(hitDelay);
        activeWeapon?.Attack(dirAtStart);

        // Finish clip
        float remain = Mathf.Max(0f, attackDuration - hitDelay);
        yield return new WaitForSeconds(remain);

        cooldownTimer = attackCooldown;
        isAttacking   = false;
    }

    void UpdateAnimFloats(Vector2 move)
    {
        // moveX/moveY reflect current motion; idleX/idleY remember last facing
        if (move.sqrMagnitude > 0f) lastMove = move.normalized;

        anim?.SetFloat("moveX", move.x);
        anim?.SetFloat("moveY", move.y);
        anim?.SetFloat("idleX", lastMove.x);
        anim?.SetFloat("idleY", lastMove.y);
    }

    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;
}
