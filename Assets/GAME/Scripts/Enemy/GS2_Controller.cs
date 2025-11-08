using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_Health))]
[RequireComponent(typeof(C_FX))]
[RequireComponent(typeof(State_Idle))]
[RequireComponent(typeof(State_Wander))]
[RequireComponent(typeof(GS2_State_Chase))]

public class GS2_Controller : MonoBehaviour, I_Controller
{
    public enum GS2State { Idle, Wander, Chase }

    [Header("References")]
    Rigidbody2D      rb;
    Animator         anim;
    C_Stats          c_Stats;
    C_Health         c_Health;
    C_FX             c_FX;
    State_Idle       idle;
    State_Wander     wander;
    GS2_State_Chase  chase;

    [Header("Detection")]
    public GS2State  defaultState   = GS2State.Idle;
    public float     detectionRange = 12f;
    public LayerMask playerLayer;

    [Header("Special Attack (Summon)")]
    public GameObject enemyPrefab;              // Regular enemy prefab (no weapon)
    public int        normalSummonCount = 3;    // 2-3 enemies per summon
    public int        emergencySummonCount = 5; // 4-5 enemies at phase 2 start
    public float      specialCooldown   = 12f;
    public float      spawnSpreadRadius = 2.5f;

    [Header("Phase 2 Settings")]
    [Range(0f, 1f)] public float phase2Threshold = 0.20f; // 20% HP
    public float retreatDistance = 4f;   // Start retreating when player < 4 units
    public float retreatDuration = 3f;   // Retreat for 3 seconds
    public float retreatCooldown = 2f;   // Stop for 2 seconds (vulnerable window)

    // Runtime state
    Vector2   desiredVelocity;
    Transform target;
    GS2State  current;
    float     contactTimer, nextSpecialTime, retreatEndTime, retreatCooldownEndTime;
    bool      isPhase2, hasTriggeredPhase2, isRetreating;

    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        anim     = GetComponent<Animator>();
        c_Stats  = GetComponent<C_Stats>();
        c_Health = GetComponent<C_Health>();
        c_FX     = GetComponent<C_FX>();
        idle     = GetComponent<State_Idle>();
        wander   = GetComponent<State_Wander>();
        chase    = GetComponent<GS2_State_Chase>();
    }

    void OnEnable()
    {
        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        c_Health.OnDied += OnDiedHandler;

        SwitchState(defaultState);
    }

    void OnDisable()
    {
        c_Health.OnDied -= OnDiedHandler;

        idle.enabled   = false;
        wander.enabled = false;
        chase.enabled  = false;

        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    void OnDiedHandler()
    {
        idle.StopAllCoroutines();
        wander.StopAllCoroutines();
        chase.StopAllCoroutines();

        idle.enabled   = false;
        wander.enabled = false;
        chase.enabled  = false;

        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders) col.enabled = false;

        anim.SetTrigger("Die");

        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(c_FX.FadeOut());

        Destroy(gameObject);
    }

    // I_CONTROLLER
    public void SetDesiredVelocity(Vector2 v) => desiredVelocity = v;

    void FixedUpdate()
    {
        rb.linearVelocity = desiredVelocity;
    }

    // STATE MACHINE
    void Update()
    {
        // Check for phase 2 transition
        CheckPhaseTransition();

        // Handle retreat timing (Phase 2 only)
        if (isPhase2)
        {
            UpdateRetreatBehavior();
        }

        // Check for special attack (summon)
        if (target && CanSpecialNow())
        {
            TriggerSpecial();
        }

        // Normal state logic
        Vector2 pos = transform.position;
        var cDet = Physics2D.OverlapCircle(pos, detectionRange, playerLayer);

        if (cDet) target = cDet.transform;

        // Determine desired state
        GS2State desired = cDet ? GS2State.Chase : defaultState;

        if (desired != current) SwitchState(desired);
    }

    void SwitchState(GS2State s)
    {
        current = s;

        idle.enabled   = (s == GS2State.Idle);
        wander.enabled = (s == GS2State.Wander);
        chase.enabled  = (s == GS2State.Chase);

        if (s == GS2State.Chase)
        {
            chase.SetTarget(target);
            desiredVelocity = Vector2.zero;
        }
        else
        {
            desiredVelocity = Vector2.zero;
        }
    }

    // PHASE SYSTEM
    void CheckPhaseTransition()
    {
        if (hasTriggeredPhase2) return;

        float hpPercent = (float)c_Stats.currentHP / c_Stats.maxHP;
        if (hpPercent <= phase2Threshold)
        {
            TriggerPhase2();
        }
    }

    void TriggerPhase2()
    {
        hasTriggeredPhase2 = true;
        isPhase2 = true;

        // Emergency summon (bonus enemies)
        int emergencyCount = Random.Range(emergencySummonCount - 1, emergencySummonCount + 1);
        SpawnEnemies(emergencyCount);

        // Start retreat cycle
        StartRetreat();

        Debug.Log($"{name}: Entered Phase 2! Emergency summon triggered.");
    }

    void UpdateRetreatBehavior()
    {
        float now = Time.time;

        if (isRetreating)
        {
            // Currently retreating
            if (now >= retreatEndTime)
            {
                // End retreat, start cooldown (vulnerable window)
                isRetreating = false;
                retreatCooldownEndTime = now + retreatCooldown;
                desiredVelocity = Vector2.zero; // Stop moving
            }
        }
        else
        {
            // In cooldown window
            if (now >= retreatCooldownEndTime)
            {
                // Check if player is still close
                if (target && Vector2.Distance(transform.position, target.position) < retreatDistance)
                {
                    StartRetreat();
                }
            }
        }
    }

    void StartRetreat()
    {
        isRetreating = true;
        retreatEndTime = Time.time + retreatDuration;
    }

    public bool IsRetreating() => isPhase2 && isRetreating;
    public bool IsInRetreatCooldown() => isPhase2 && !isRetreating && Time.time < retreatCooldownEndTime;

    // SPECIAL ATTACK (SUMMON)
    bool CanSpecialNow()
    {
        if (Time.time < nextSpecialTime) return false;
        return true;
    }

    void TriggerSpecial()
    {
        nextSpecialTime = Time.time + specialCooldown;
        int spawnCount = Random.Range(normalSummonCount - 1, normalSummonCount + 1);
        SpawnEnemies(spawnCount);
    }

    public void SpawnEnemies(int count)
    {
        if (!enemyPrefab)
        {
            Debug.LogWarning($"{name}: enemyPrefab is not assigned!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            // Circular spread pattern
            float angle = (360f / count) * i + Random.Range(-20f, 20f);
            float radius = Random.Range(1.5f, spawnSpreadRadius);

            Vector2 offset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );

            Vector2 spawnPos = (Vector2)transform.position + offset;
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }

        Debug.Log($"{name}: Spawned {count} enemies.");
    }

    // COLLISION DAMAGE
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!c_Health.IsAlive) return;

        if ((playerLayer.value & (1 << collision.collider.gameObject.layer)) == 0)
            return;

        if (contactTimer > 0f)
        {
            contactTimer -= Time.fixedDeltaTime;
            return;
        }

        var playerHealth = collision.collider.GetComponent<C_Health>();
        if (!playerHealth || !playerHealth.IsAlive) return;

        playerHealth.ChangeHealth(-c_Stats.collisionDamage);
        contactTimer = c_Stats.collisionTick;
    }

    // GETTERS
    public Transform GetTarget() => target;

    // GIZMOS
    void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position;

        // Detection range (orange)
        Gizmos.color = new Color(1f, 0.65f, 0f);
        Gizmos.DrawWireSphere(p, detectionRange);

        // Phase 2: Retreat distance (cyan)
        if (isPhase2)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, retreatDistance);
        }

        // Enemy spawn radius (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, spawnSpreadRadius);
    }
}
