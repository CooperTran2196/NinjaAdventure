using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]

public class State_Attack : MonoBehaviour
{
    [Header("Animation States")]
    public LayerMask playerLayer;

    [Header("Timing")]
    public float attackCooldown = 0.80f;
    public float attackDuration = 0.45f;
    public float hitDelay       = 0.15f;

    // Ranges are injected by controller
    float attackRange    = 1.2f;

    [Header("Weapon")]
    public W_Base activeWeapon;

    [Header("Knockback")]
    public float knockbackRecovery = 30f;

    // Cache
    Rigidbody2D rb;
    Animator anim;

    // Runtime variables
    Transform target;
    Vector2 knockback, lastFace = Vector2.down;
    float cooldownTimer;
    bool isAttacking;

    public bool IsAttacking => isAttacking;

    void Awake()
    {
        rb           = GetComponent<Rigidbody2D>();
        anim         = GetComponent<Animator>();
        activeWeapon = GetComponentInChildren<W_Base>();

        if (!rb) Debug.LogError($"{name}: Rigidbody2D missing on State_Attack.");
        if (!anim) Debug.LogError($"{name}: Animator missing on State_Attack.");
    }

    void OnEnable()
    {
        // anim.SetBool("isAttacking", false);
    }

    void OnDisable()
    {
        // anim.SetBool("isAttacking", false);
        isAttacking = false;
        rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        
        // Process attack logic only when we have a target
        if (target)
        {
            // Inner ring test (collider-based so edge contact counts)
            bool inInner = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);

            Vector2 to = (Vector2)target.position - (Vector2)transform.position;
            float d = to.magnitude;
            Vector2 dir = d > 0.0001f ? to.normalized : lastFace;

            // Continuously update idle facing (even during attack so idle pose rotates, atkX/atkY stay locked)
            UpdateIdleFacing(isAttacking ? lastFace : dir);

            // Start attack immediately upon entering inner ring and off cooldown
            if (!isAttacking && inInner && cooldownTimer <= 0f)
                StartCoroutine(AttackRoutine(dir));
        }
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

    public void SetTarget(Transform t) => target = t;
    public void SetRanges(float attackRange) => this.attackRange = attackRange;

    // NON-INTERUPTIBLE ATTACK ROUTINE
    // lock facing, play clip, wait for hit, wait for end, cooldown
    IEnumerator AttackRoutine(Vector2 dirAtStart)
    {
        isAttacking = true;
        anim.SetBool("isAttacking", true);

        // Lock attack facing into atkX/atkY once at the start
        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);

        // Movement stays zero while striking; just update idle facing
        UpdateIdleFacing(lastFace);

        // Wait for hitDelay then start the hit
        yield return new WaitForSeconds(hitDelay);
        activeWeapon?.Attack(lastFace);
        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitDelay));

        // Reset the attack cooldown
        cooldownTimer = attackCooldown;
        isAttacking = false;
        anim.SetBool("isAttacking", false);
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

    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;

}
