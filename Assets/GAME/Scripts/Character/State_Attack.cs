using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class State_Attack : MonoBehaviour
{
    [Header("Detection (two rings)")]
    public LayerMask playerLayer;
    public C_Stats c_Stats;
    [Min(3f)] public float detectionRange = 3f; // outer ring
    [Min(1.2f)] public float attackRange   = 1.2f; // inner ring

    [Header("Chase")]
    // public float moveSpeed   = 2.2f;
    public float stopBuffer  = 0.10f; // hover just outside inner ring

    [Header("Attack")]
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
        // AI logic is now in Update()
    }

    void OnDisable()
    {
        StopAllCoroutines(); // Stop any running AttackRoutine
        isAttacking = false;
        velocity    = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    // Counter for attack cooldown & AI logic
    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        // --- AI Logic (from former ThinkLoop) ---
        if (target)
        {
            Vector2 toTargetVector = (Vector2)target.position - (Vector2)transform.position;
            float   distance       = toTargetVector.magnitude;
            Vector2 toTargetDir    = toTargetVector.normalized;

            bool inOuter = distance <= detectionRange;
            bool inInner = distance <= attackRange;

            // If not attacking, can move
            if (!isAttacking)
            {
                // If in outer ring, chase; else hold
                if (inOuter && !inInner)
                {
                    // CHASE
                    velocity = toTargetDir * c_Stats.MS; // use C_Stats move speed

                    // stop just before entering the strike ring to avoid jitter
                    if (distance <= (attackRange + stopBuffer))
                        velocity = Vector2.zero;

                    UpdateAnimFloats(velocity);
                }
                else
                {
                    // HOLD (either attack on cooldown, or outside outer ring)
                    velocity = Vector2.zero;
                    UpdateAnimFloats(Vector2.zero);
                }

                // START ATTACK
                if (inInner && cooldownTimer <= 0f)
                    StartCoroutine(AttackRoutine(toTargetDir));
            }
            else
            {
                // While Attacking: do not move, but keep idle facing consistent
                velocity = Vector2.zero;
                UpdateAnimFloats(Vector2.zero);
            }
        }
        else
        {
            // No target: stand still; controller will exit this state
            velocity = Vector2.zero;
            UpdateAnimFloats(Vector2.zero);
        }
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
        yield return new WaitForSeconds(attackDuration - hitDelay);

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
