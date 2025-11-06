using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class B_Controller : MonoBehaviour, I_Controller
{
    public enum BState { Idle, Wander, Chase, Attack }

    [Header("States")]
    public BState defaultState = BState.Idle;
    public State_Idle        idle;
    public State_Wander      wander;
    public State_Chase_Boss  chase;
    public State_Attack_Boss attack;

    [Header("Detection")]
    [Min(0.1f)] public float detectionRange = 5f;
    [Min(0.1f)] public float attackRange    = 1.6f;
    public LayerMask playerLayer;
    public float attackStartBuffer = 0.20f;

    Rigidbody2D rb;
    Vector2 desiredVelocity;

    Transform target;
    float inRangeTimer;
    BState current;

    void Awake()
    {
        idle   ??= GetComponent<State_Idle>();
        wander ??= GetComponent<State_Wander>();
        chase  ??= GetComponent<State_Chase_Boss>();
        attack ??= GetComponent<State_Attack_Boss>();
        rb     ??= GetComponent<Rigidbody2D>();
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

        var cAtk = Physics2D.OverlapCircle(pos, attackRange,    playerLayer);
        var cDet = cAtk ?? Physics2D.OverlapCircle(pos, detectionRange, playerLayer);

        bool inAttack = cAtk;
        bool inDetect = cDet;

        if (inDetect) target = cDet.transform;

        inRangeTimer = inAttack ? inRangeTimer + Time.deltaTime : 0f;
        bool readyMelee = inAttack && inRangeTimer >= attackStartBuffer;

        // ask the boss state if special is available now (uses timing reach, stop-short, y gate)
        bool readySpecial = false;
        if (attack && target)
            readySpecial = attack.CanSpecialNow(transform.position, target.position);

        bool attackingNow = attack && attack.IsAttacking;

        var desired =
            attackingNow   ? BState.Attack :
            readySpecial   ? BState.Attack :
            readyMelee     ? BState.Attack :
            inDetect       ? BState.Chase  :
                             defaultState;

        if (desired != current) SwitchState(desired);
    }

    public void SwitchState(BState s)
    {
        current = s;

        if (idle)   idle.enabled   = (s == BState.Idle);
        if (wander) wander.enabled = (s == BState.Wander);
        if (chase)  chase.enabled  = (s == BState.Chase);
        if (attack) attack.enabled = (s == BState.Attack);

        if (s == BState.Chase)
        {
            chase?.SetTarget(target);
            chase?.SetRanges(attackRange);
            desiredVelocity = Vector2.zero;
        }
        else if (s == BState.Attack)
        {
            attack?.SetTarget(target);
            attack?.SetRanges(attackRange);
            desiredVelocity = Vector2.zero;
        }
        else desiredVelocity = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.65f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
