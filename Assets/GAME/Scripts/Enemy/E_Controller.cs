using UnityEngine;

[DisallowMultipleComponent]
public class E_Controller : MonoBehaviour
{
    public enum EState { Idle, Wander, Chase, Attack }

    [Header("Ranges")]
    public LayerMask playerLayer;
    [Min(0.1f)] public float detectionRange = 3f;   // outer ring
    [Min(0.1f)] public float attackRange    = 1.2f; // inner ring

    [Header("States")]
    public EState defaultState = EState.Idle;  // you can change per prefab
    public State_Idle   idle;                  // shared with NPC
    public State_Wander wander;                // shared with NPC (optional)
    public State_Chase  chase;                 // new
    public State_Attack attack;                // new

    EState current;
    Transform currentTarget;

    void Awake()
    {
        idle   ??= GetComponent<State_Idle>();
        wander ??= GetComponent<State_Wander>();
        chase  ??= GetComponent<State_Chase>();
        attack ??= GetComponent<State_Attack>();

        if (!idle)   Debug.LogError($"{name}: Missing State_Idle.");
        if (!chase)  Debug.LogError($"{name}: Missing State_Chase.");
        if (!attack) Debug.LogError($"{name}: Missing State_Attack.");
    }

    void OnEnable() => SwitchState(defaultState);

    void OnDisable()
    {
        if (idle)   idle.enabled   = false;
        if (wander) wander.enabled = false;
        if (chase)  chase.enabled  = false;
        if (attack) attack.enabled = false;
    }

    void Update()
    {
        // Always check INNER first so inner implies outer
        Collider2D innerHit = Physics2D.OverlapCircle((Vector2)transform.position, attackRange,    playerLayer);
        Collider2D outerHit = innerHit ? innerHit
                                       : Physics2D.OverlapCircle((Vector2)transform.position, detectionRange, playerLayer);

        bool hitInner = innerHit;
        bool hitOuter = outerHit; // includes inner

        if (hitOuter) currentTarget = outerHit.transform;
        // Pass ranges to whoever is active (keeps states in sync with inspector)
        if (chase  && chase.enabled)  chase.SetRanges(detectionRange, attackRange);
        if (attack && attack.enabled) attack.SetRanges(detectionRange, attackRange);

        switch (current)
        {
            case EState.Idle:
            case EState.Wander:
                if (hitOuter)
                {
                    chase.SetTarget(currentTarget);
                    SwitchState(EState.Chase);
                }
                break;

            case EState.Chase:
                if (!hitOuter)
                {
                    currentTarget = null;
                    SwitchState(defaultState);
                }
                else if (hitInner)
                {
                    attack.SetTarget(currentTarget);
                    SwitchState(EState.Attack);
                }
                else
                {
                    // keep feeding target/ranges while chasing
                    chase.SetTarget(currentTarget);
                }
                break;

            case EState.Attack:
                // Never interrupt the clip. Only switch once it's finished.
                if (!hitOuter && !attack.IsAttacking)
                {
                    currentTarget = null;
                    SwitchState(defaultState);
                }
                else if (!hitInner && !attack.IsAttacking)
                {
                    // Still in outer ring but out of inner -> chase again
                    chase.SetTarget(currentTarget);
                    SwitchState(EState.Chase);
                }
                else
                {
                    // stay attacking; controller does nothing mid-clip
                }
                break;
        }
    }

    public void SwitchState(EState s)
    {
        if (current == s) return;
        current = s;

        if (idle)   idle.enabled   = (s == EState.Idle);
        if (wander) wander.enabled = (s == EState.Wander);
        if (chase)  chase.enabled  = (s == EState.Chase);
        if (attack) attack.enabled = (s == EState.Attack);

        // On enter, provide context to newly enabled state
        if (s == EState.Chase && chase)
        {
            chase.SetTarget(currentTarget);
            chase.SetRanges(detectionRange, attackRange);
        }
        else if (s == EState.Attack && attack)
        {
            attack.SetTarget(currentTarget);
            attack.SetRanges(detectionRange, attackRange);
        }
    }
}
