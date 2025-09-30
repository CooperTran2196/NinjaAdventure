using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]

[DisallowMultipleComponent]

public class E_Combat : MonoBehaviour
{
    [Header("References")]
    SpriteRenderer sr;
    Animator anim;

    C_Stats c_Stats;
    C_State c_State;
    E_Movement e_Movement;
    C_Health e_Health;
    public W_Base activeWeapon;

    [Header("AI")]
    public LayerMask playerLayer;
    [Min(1.3f)] public float attackRange = 1.3f;
    [Min(0.5f)] public float thinkInterval = 0.5f;

    [Header("Attack")]
    [Header("ALWAYS matched full clip length (0.45)")]
    public float attackDuration = 0.45f;
    [Header("ALWAYS set when the hit happens (0.15)")]
    public float hitDelay = 0.15f;

    [Header("Debug")]
    [SerializeField] bool autoKill;

    // Quick state check
    public bool isAttacking { get; private set; }
    // Removed IsAlive; use c_State.Is(ActorState.Dead) as single source of truth
    const float MIN_DISTANCE = 0.0001f;
    float contactTimer;   // for collision damage
    float cooldownTimer;  // for attacking cooldown

    void Awake()
    {
        sr           ??= GetComponent<SpriteRenderer>();
        anim         ??= GetComponent<Animator>();

        c_Stats      ??= GetComponent<C_Stats>();
        c_State      ??= GetComponent<C_State>();
        e_Movement   ??= GetComponent<E_Movement>();
        e_Health     ??= GetComponent<C_Health>();
        activeWeapon ??= GetComponentInChildren<W_Melee>();


        if (!sr)            Debug.LogError($"{name}: SpriteRenderer is missing in E_Combat");
        if (!anim)          Debug.LogError($"{name}: Animator is missing in E_Combat");

        if (!c_Stats)       Debug.LogError($"{name}: C_Stats is missing in E_Combat");
        if (!e_Movement)    Debug.LogError($"{name}: E_Movement is missing in E_Combat");
        if (!e_Health)      Debug.LogError($"{name}: C_Health is missing in E_Combat");
        if (!activeWeapon)  Debug.LogError($"{name}: W_Melee is missing in E_Combat");
        // c_Wander optional: only log verbose if enemy expected to wander? (skip error)
    }

    void OnEnable()
    {
        StartCoroutine(ThinkLoop());
    }

    void Update()
    {
        if (autoKill) { autoKill = false; e_Health.ChangeHealth(-c_Stats.maxHP); }
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    IEnumerator ThinkLoop()
    {
        var wait = new WaitForSeconds(thinkInterval);

        while (true)
        {
            if (c_State.Is(C_State.ActorState.Dead)) yield break;

            if (!isAttacking)
            {
                // Check if player is in range
                bool inAttackRange = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);

                // Decide if should hold position (idle) or chase
                bool shouldHold = inAttackRange && cooldownTimer > 0f && c_State.lockMoveWhileAttacking;
                e_Movement.SetHoldInRange(shouldHold);

                // If close enough and off cooldown, begin an attack
                if (inAttackRange && cooldownTimer <= 0f)
                    StartCoroutine(AttackRoutine());
            }
            yield return wait;
        }
    }

    // Collision Damage
    void OnCollisionStay2D(Collision2D collision)
    {
        if (c_State.Is(C_State.ActorState.Dead)) return;

        // Filter to only player layer
        if ((playerLayer.value & (1 << collision.collider.gameObject.layer)) == 0)
            return;
        // Count down using physics timestep
        if (contactTimer > 0f)
        {
            contactTimer -= Time.fixedDeltaTime;
            return;
        }
        var playerHealth = collision.collider.GetComponent<C_Health>();
        if (playerHealth == null || !playerHealth.IsAlive) return; // Only damage a live player
        playerHealth.ChangeHealth(-c_Stats.collisionDamage); //Apply damage
        contactTimer = c_Stats.collisionTick; // reset tick window
    }


    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // CONTINUOUS facing from player position at attack start (no lastMove / no snap)
        Vector2 dir = ReadAimToPlayer();

        // Set facing direction
        c_State.SetAttackDirection(dir);

        // Delay -> Attack -> recover
        yield return new WaitForSeconds(hitDelay);
        // Placeholder
        activeWeapon?.Attack(dir);
        yield return new WaitForSeconds(attackDuration - hitDelay);

        // Decide keep chasing/idle based on lockDuringAttack (MODE)
        bool lockMoveFlag = (c_State && c_State.lockMoveWhileAttacking);
        bool stillInRange = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
        e_Movement.SetHoldInRange(lockMoveFlag && stillInRange);

        isAttacking = false;

        cooldownTimer = c_Stats.attackCooldown;
    }

    Vector2 ReadAimToPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return Vector2.down;
        Vector2 d = (Vector2)(player.transform.position - transform.position);
        return (d.sqrMagnitude > MIN_DISTANCE) ? d.normalized : Vector2.down;
    }

    // Debug: show attack range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.9f); // red attack ring
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
