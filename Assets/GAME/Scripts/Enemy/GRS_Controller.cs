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

    Vector2   desiredVelocity;
    Transform target;
    float     inRangeTimer;
    float     contactTimer;
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

        // Play death animation AND fade simultaneously
        anim.SetTrigger("Die");
        StartCoroutine(c_FX.FadeOut());
        
        // Wait for fade to complete
        yield return new WaitForSeconds(c_FX.deathFadeTime);

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

        inRangeTimer     = inAttack ? inRangeTimer + Time.deltaTime : 0f;
        bool readyMelee  = inAttack && inRangeTimer >= attackStartBuffer;
        bool readySpecial = attack && target && attack.CanSpecialNow(transform.position, target.position);
        bool attackingNow = attack && attack.IsAttacking;
        bool recovering   = attack && attack.IsRecovering;

        var desired =
            attackingNow ? GRSState.Attack :
            recovering   ? GRSState.Idle   :
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

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!c_Health.IsAlive) return;
        if (attack && attack.IsAttacking) return;

        if ((playerLayer.value & (1 << collision.collider.gameObject.layer)) == 0)
            return;

        var playerHealth = collision.collider.GetComponent<C_Health>();
        if (!playerHealth || !playerHealth.IsAlive) return;

        if (contactTimer <= 0f)
        {
            playerHealth.ChangeHealth(-c_Stats.collisionDamage);
            SYS_GameManager.Instance.sys_SoundManager.PlayPlayerHit();
            contactTimer = c_Stats.collisionTick;
        }
        else
        {
            contactTimer -= Time.fixedDeltaTime;
        }
    }
}
