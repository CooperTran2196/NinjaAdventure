using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
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
    Rigidbody2D rb;
    C_Stats     c_Stats;
    C_Health    c_Health;
    C_FX        c_FX;

    [Header("States")]
    State_Idle      idle;
    State_Wander    wander;
    GR_State_Chase  chase;
    GR_State_Attack attack;

    [Header("Detection")]
                public GRState   defaultState = GRState.Idle;
    [Min(0.1f)] public float     detectionRange = 10f;   // Chase + special attack range
    [Min(0.1f)] public float     attackRange = 3f;       // Normal attack trigger range
                public LayerMask playerLayer;
                public float     attackStartBuffer = 0.20f;

    // Runtime state
    Vector2   desiredVelocity;
    Transform target;
    float     inRangeTimer;
    float     contactTimer;   // Collision damage cooldown
    GRState   current;

    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
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
        if (rb) rb.linearVelocity = Vector2.zero;

        // Subscribe to death event
        if (c_Health) c_Health.OnDied += OnDiedHandler;

        chase?.SetRanges(attackRange);
        attack?.SetRanges(attackRange, detectionRange);
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        // Unsubscribe from death event
        if (c_Health) c_Health.OnDied -= OnDiedHandler;

        if (idle)   idle.enabled   = false;
        if (wander) wander.enabled = false;
        if (chase)  chase.enabled  = false;
        if (attack) attack.enabled = false;

        desiredVelocity = Vector2.zero;
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    void OnDiedHandler()
    {
        // Handle death: disable states, play animation, fade, then destroy
        if (idle)   idle.enabled   = false;
        if (wander) wander.enabled = false;
        if (chase)  chase.enabled  = false;
        if (attack) attack.enabled = false;
        
        desiredVelocity = Vector2.zero;
        if (rb) rb.linearVelocity = Vector2.zero;
        
        StartCoroutine(HandleDeath());
    }
    
    System.Collections.IEnumerator HandleDeath()
    {
        // Play death animation (assuming animator has "Die" trigger)
        var anim = GetComponent<Animator>();
        if (anim) anim.SetTrigger("Die");
        
        yield return new WaitForSeconds(1.5f);
        
        // Fade out sprite
        yield return StartCoroutine(c_FX.FadeOut());
        
        // Destroy boss
        Destroy(gameObject);
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
            attackingNow ? GRState.Attack :
            readySpecial ? GRState.Attack :
            readyMelee   ? GRState.Attack :
            inDetect     ? GRState.Chase  :
                           defaultState;

        if (desired != current) SwitchState(desired);
    }

    void SwitchState(GRState s)
    {
        current = s;

        if (idle)   idle.enabled   = (s == GRState.Idle);
        if (wander) wander.enabled = (s == GRState.Wander);
        if (chase)  chase.enabled  = (s == GRState.Chase);
        if (attack) attack.enabled = (s == GRState.Attack);

        if (s == GRState.Chase)
        {
            chase?.SetTarget(target);
            chase?.SetRanges(attackRange);
            desiredVelocity = Vector2.zero;
        }
        else if (s == GRState.Attack)
        {
            attack?.SetTarget(target);
            attack?.SetRanges(attackRange, detectionRange);
            desiredVelocity = Vector2.zero;
        }
        else desiredVelocity = Vector2.zero;
    }

    // COLLISION DAMAGE
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!c_Health || !c_Health.IsAlive) return;

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

        if (!c_Stats) return;

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
