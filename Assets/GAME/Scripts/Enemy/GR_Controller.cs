using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_Health))]
[RequireComponent(typeof(C_FX))]
[RequireComponent(typeof(State_Idle))]
[RequireComponent(typeof(State_Wander))]
[RequireComponent(typeof(GR_State_Chase))]
[RequireComponent(typeof(GR_State_Attack))]

public class GR_Controller : MonoBehaviour, I_Controller
{
    public enum GRState { Idle, Wander, Chase, Attack }

    [Header("References")]
    Rigidbody2D     rb;
    Animator        anim;
    C_Stats         c_Stats;
    C_Health        c_Health;
    C_FX            c_FX;
    State_Idle      idle;
    State_Wander    wander;
    GR_State_Chase  chase;
    GR_State_Attack attack;

    [Header("Detection")]
    public GRState   defaultState       = GRState.Idle;
    public float     detectionRange     = 16f;   // Chase + special attack range
    public float     attackRange        = 5f;    // Normal attack trigger range
    public LayerMask playerLayer;
    public float     attackStartBuffer  = 0.20f;

    // Runtime state
    Vector2   desiredVelocity;
    Transform target;
    float     inRangeTimer;
    float     contactTimer;   // Collision damage cooldown
    GRState   current;

    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        anim     = GetComponent<Animator>();
        c_Stats  = GetComponent<C_Stats>();
        c_Health = GetComponent<C_Health>();
        c_FX     = GetComponent<C_FX>();
        idle     = GetComponent<State_Idle>();
        wander   = GetComponent<State_Wander>();
        chase    = GetComponent<GR_State_Chase>();
        attack   = GetComponent<GR_State_Attack>();
    }

    void OnEnable()
    {
        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        c_Health.OnDied += OnDiedHandler;

        chase.SetRanges(attackRange);
        attack.SetRanges(attackRange, detectionRange);
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        c_Health.OnDied -= OnDiedHandler;

        idle.enabled = false;
        wander.enabled = false;
        chase.enabled = false;
        attack.enabled = false;

        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    void OnDiedHandler()
    {
        idle.StopAllCoroutines();
        wander.StopAllCoroutines();
        chase.StopAllCoroutines();
        attack.StopAllCoroutines();

        idle.enabled   = false;
        wander.enabled = false;
        chase.enabled  = false;
        attack.enabled = false;

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
        if (!c_Health.IsAlive) return;

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
        bool recovering   = attack && attack.IsRecovering;

        var desired =
            attackingNow ? GRState.Attack :
            recovering   ? GRState.Idle   :  // Vulnerable window after attack
            readySpecial ? GRState.Attack :
            readyMelee   ? GRState.Attack :
            inDetect     ? GRState.Chase  :
                           defaultState;

        if (desired != current) SwitchState(desired);
    }

    void SwitchState(GRState s)
    {
        current = s;

        idle.enabled = (s == GRState.Idle);
        wander.enabled = (s == GRState.Wander);
        chase.enabled = (s == GRState.Chase);
        attack.enabled = (s == GRState.Attack);

        if (s == GRState.Chase)
        {
            chase.SetTarget(target);
            chase.SetRanges(attackRange);
            desiredVelocity = Vector2.zero;
        }
        else if (s == GRState.Attack)
        {
            attack.SetTarget(target);
            attack.SetRanges(attackRange, detectionRange);
            desiredVelocity = Vector2.zero;
        }
        else desiredVelocity = Vector2.zero;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!c_Health.IsAlive) return;
        if (attack && attack.IsAttacking) return;  // No contact damage during attacks

        if ((playerLayer.value & (1 << collision.collider.gameObject.layer)) == 0) return;

        C_Health playerHealth = collision.collider.GetComponent<C_Health>();
        if (!playerHealth || !playerHealth.IsAlive) return;

        if (contactTimer <= 0f)
        {
            playerHealth.ChangeHealth(-c_Stats.collisionDamage);
            contactTimer = c_Stats.collisionTick;
        }
        else
        {
            contactTimer -= Time.fixedDeltaTime;
        }
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
