using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(C_Stats))]
[RequireComponent(typeof(C_Health))]
[RequireComponent(typeof(C_FX))]
[RequireComponent(typeof(State_Idle))]
[RequireComponent(typeof(State_Wander))]
[RequireComponent(typeof(GRS_State_Chase))]
[RequireComponent(typeof(GRS_State_Attack))]

public class GRS_Controller : MonoBehaviour, I_Controller
{
    public enum GRSState { Idle, Wander, Chase, Attack }

    [Header("References")]
    Rigidbody2D        rb;
    Animator           anim;
    C_Stats            c_Stats;
    C_Health           c_Health;
    C_FX               c_FX;
    State_Idle         idle;
    State_Wander       wander;
    GRS_State_Chase    chase;
    GRS_State_Attack   attack;

    [Header("Detection")]
    public GRSState  defaultState       = GRSState.Idle;
    public float     detectionRange     = 10f;
    public float     attackRange        = 1.6f;
    public LayerMask playerLayer;
    public float     attackStartBuffer  = 0.20f;

    // Runtime state
    Vector2   desiredVelocity;
    Transform target;
    float     inRangeTimer;
    float     contactTimer;   // Collision damage cooldown
    GRSState  current;

    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        anim     = GetComponent<Animator>();
        c_Stats  = GetComponent<C_Stats>();
        c_Health = GetComponent<C_Health>();
        c_FX     = GetComponent<C_FX>();
        idle     = GetComponent<State_Idle>();
        wander   = GetComponent<State_Wander>();
        chase    = GetComponent<GRS_State_Chase>();
        attack   = GetComponent<GRS_State_Attack>();
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

        idle.enabled   = false;
        wander.enabled = false;
        chase.enabled  = false;
        attack.enabled = false;

        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    void OnDiedHandler()
    {
        // Immediately stop all coroutines in states (prevents dash continuation)
        idle.StopAllCoroutines();
        wander.StopAllCoroutines();
        chase.StopAllCoroutines();
        attack.StopAllCoroutines();

        // Immediately disable all states to prevent any further actions
        idle.enabled   = false;
        wander.enabled = false;
        chase.enabled  = false;
        attack.enabled = false;

        StartCoroutine(HandleDeath());
    }
    
    IEnumerator HandleDeath()
    {
        // Stop all movement
        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        // Disable colliders (stop collision damage)
        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders) col.enabled = false;
        
        // Play death animation
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

        idle.enabled   = (s == GRSState.Idle);
        wander.enabled = (s == GRSState.Wander);
        chase.enabled  = (s == GRSState.Chase);
        attack.enabled = (s == GRSState.Attack);

        if (s == GRSState.Chase)
        {
            chase.SetTarget(target);
            chase.SetRanges(attackRange);
            desiredVelocity = Vector2.zero;
        }
        else if (s == GRSState.Attack)
        {
            attack.SetTarget(target);
            attack.SetRanges(attackRange, detectionRange);
            desiredVelocity = Vector2.zero;
        }
        else desiredVelocity = Vector2.zero;
    }

    // COLLISION DAMAGE
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!c_Health.IsAlive) return;

        // Filter to only player layer
        if ((playerLayer.value & (1 << collision.collider.gameObject.layer)) == 0)
            return;

        // Cooldown using physics timestep
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
