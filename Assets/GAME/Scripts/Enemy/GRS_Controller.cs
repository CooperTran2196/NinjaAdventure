using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_Health))]
[RequireComponent(typeof(State_Idle))]
[RequireComponent(typeof(State_Wander))]
[RequireComponent(typeof(GRS_State_Chase))]
[RequireComponent(typeof(GRS_State_Attack))]
public class GRS_Controller : MonoBehaviour, I_Controller
{
    public enum GRSState { Idle, Wander, Chase, Attack }

    [Header("References")]
    Rigidbody2D rb;
    C_Stats     c_Stats;
    C_Health    c_Health;

    [Header("States")]
    State_Idle         idle;
    State_Wander       wander;
    GRS_State_Chase    chase;
    GRS_State_Attack   attack;

    [Header("Detection")]
                public GRSState  defaultState = GRSState.Idle;
    [Min(0.1f)] public float     detectionRange = 10f;
    [Min(0.1f)] public float     attackRange = 1.6f;
                public LayerMask playerLayer;
                public float     attackStartBuffer = 0.20f;

    // Runtime state
    Vector2   desiredVelocity;
    Transform target;
    float     inRangeTimer;
    GRSState  current;

    void Awake()
    {
        rb       ??= GetComponent<Rigidbody2D>();
        c_Stats  ??= GetComponent<C_Stats>();
        c_Health ??= GetComponent<C_Health>();
        idle     ??= GetComponent<State_Idle>();
        wander   ??= GetComponent<State_Wander>();
        chase    ??= GetComponent<GRS_State_Chase>();
        attack   ??= GetComponent<GRS_State_Attack>();
    }

    void OnEnable()
    {
        desiredVelocity = Vector2.zero;
        if (rb) rb.linearVelocity = Vector2.zero;

        chase?.SetRanges(attackRange);
        attack?.SetRanges(attackRange);
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

    // I_CONTROLLER

    public void SetDesiredVelocity(Vector2 v) => desiredVelocity = v;

    void FixedUpdate()
    {
        if (!rb) return;
        rb.linearVelocity = desiredVelocity;
    }

    // STATE MACHINE

    void Update()
    {
        Vector2 pos = transform.position;

        var cAtk = Physics2D.OverlapCircle(pos, attackRange, playerLayer);
        var cDet = cAtk ?? Physics2D.OverlapCircle(pos, detectionRange, playerLayer);

        bool inAttack = cAtk;
        bool inDetect = cDet;

        if (inDetect) target = cDet.transform;

        inRangeTimer = inAttack ? inRangeTimer + Time.deltaTime : 0f;
        bool readyMelee = inAttack && inRangeTimer >= attackStartBuffer;

        bool readySpecial = attack && target && attack.CanSpecialNow(transform.position, target.position);
        bool attackingNow = attack && attack.IsAttacking;

        var desired =
            attackingNow ? GRSState.Attack :
            readySpecial ? GRSState.Attack :
            readyMelee   ? GRSState.Attack :
            inDetect     ? GRSState.Chase  :
                           defaultState;

        if (desired != current) SwitchState(desired);
    }

    void SwitchState(GRSState s)
    {
        current = s;

        if (idle)   idle.enabled   = (s == GRSState.Idle);
        if (wander) wander.enabled = (s == GRSState.Wander);
        if (chase)  chase.enabled  = (s == GRSState.Chase);
        if (attack) attack.enabled = (s == GRSState.Attack);

        if (s == GRSState.Chase)
        {
            chase?.SetTarget(target);
            chase?.SetRanges(attackRange);
            desiredVelocity = Vector2.zero;
        }
        else if (s == GRSState.Attack)
        {
            attack?.SetTarget(target);
            attack?.SetRanges(attackRange);
            desiredVelocity = Vector2.zero;
        }
        else desiredVelocity = Vector2.zero;
    }

    // GIZMOS

    void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position;

        // Detection range (orange)
        Gizmos.color = new Color(1f, 0.65f, 0f);
        Gizmos.DrawWireSphere(p, detectionRange);

        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p, attackRange);
    }
}
