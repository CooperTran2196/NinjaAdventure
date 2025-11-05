using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class MB_Controller : MonoBehaviour, I_Controller
{
    public enum MBState { Idle, Wander, Chase, Attack }

    [Header("States")]
    public MBState defaultState = MBState.Idle;
    public State_Idle          idle;
    public State_Wander        wander;
    public State_Chase_MBlv2   chase;
    public State_Attack_MBlv2  attack;

    [Header("Detection")]
    [Min(0.1f)] public float detectionRange = 8f;
    [Min(0.1f)] public float chargeRange    = 5f;    // Max range for charge attack
    [Min(0.1f)] public float jumpRange      = 8f;    // Max range for jump attack
    public LayerMask playerLayer;
    public float attackStartBuffer = 0.20f;

    Rigidbody2D rb;
    Vector2 desiredVelocity;

    Transform target;
    float inRangeTimer;
    MBState current;

    void Awake()
    {
        idle   ??= GetComponent<State_Idle>();
        wander ??= GetComponent<State_Wander>();
        chase  ??= GetComponent<State_Chase_MBlv2>();
        attack ??= GetComponent<State_Attack_MBlv2>();
        rb     ??= GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        desiredVelocity = Vector2.zero;
        if (rb) rb.linearVelocity = Vector2.zero;

        chase?.SetRanges(chargeRange);
        attack?.SetRanges(chargeRange, jumpRange);
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        if (idle)   idle.enabled   = false;
        if (wander) wander.enabled = false;
        if (chase)  chase.enabled  = false;
        if (attack) attack.enabled = false;

        desiredVelocity = Vector2.zero;
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    // -------- I_Controller --------
    public void SetDesiredVelocity(Vector2 v) => desiredVelocity = v;

    void FixedUpdate()
    {
        if (!rb) return;
        rb.linearVelocity = desiredVelocity;
    }

    void Update()
    {
        Vector2 pos = transform.position;

        var cDet = Physics2D.OverlapCircle(pos, detectionRange, playerLayer);
        bool inDetect = cDet;

        if (inDetect) target = cDet.transform;

        // Check if in attack range (either charge or jump)
        float distToTarget = target ? Vector2.Distance(pos, target.position) : float.MaxValue;
        bool inAttackRange = distToTarget <= jumpRange;

        inRangeTimer = inAttackRange ? inRangeTimer + Time.deltaTime : 0f;
        bool readyAttack = inAttackRange && inRangeTimer >= attackStartBuffer;

        bool attackingNow = attack && attack.IsAttacking;

        var desired =
            attackingNow  ? MBState.Attack :
            readyAttack   ? MBState.Attack :
            inDetect      ? MBState.Chase  :
                            defaultState;

        if (desired != current) SwitchState(desired);
    }

    public void SwitchState(MBState s)
    {
        current = s;

        if (idle)   idle.enabled   = (s == MBState.Idle);
        if (wander) wander.enabled = (s == MBState.Wander);
        if (chase)  chase.enabled  = (s == MBState.Chase);
        if (attack) attack.enabled = (s == MBState.Attack);

        if (s == MBState.Chase)
        {
            chase?.SetTarget(target);
            chase?.SetRanges(chargeRange);
            desiredVelocity = Vector2.zero;
        }
        else if (s == MBState.Attack)
        {
            attack?.SetTarget(target);
            attack?.SetRanges(chargeRange, jumpRange);
            desiredVelocity = Vector2.zero;
        }
        else desiredVelocity = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position;

        // Detection range (orange)
        Gizmos.color = new Color(1f, 0.65f, 0f);
        Gizmos.DrawWireSphere(p, detectionRange);

        // Charge attack range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, chargeRange);

        // Jump attack range (cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(p, jumpRange);
    }
}
