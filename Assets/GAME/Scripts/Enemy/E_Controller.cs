using UnityEngine;

[RequireComponent(typeof(State_Idle))]
[RequireComponent(typeof(State_Wander))]
[RequireComponent(typeof(State_Chase))]
[RequireComponent(typeof(State_Attack))]
[DisallowMultipleComponent]

public class E_Controller : MonoBehaviour
{
    public enum EState { Idle, Wander, Chase, Attack }

    [Header("Main controller for enemy AI states")]
    [Header("Ranges and Layers")]
    [Min(3f)] public float detectionRange = 3f;
    [Min(1.2f)] public float attackRange = 1.2f;
    public LayerMask playerLayer;

    [Header("States")]
    public EState defaultState = EState.Idle;
    EState currentState;
    public State_Idle idle;
    public State_Wander wander;
    public State_Chase chase;
    public State_Attack attack;

    // Runtime variables
    Transform currentTarget;

    void Awake()
    {
        idle = GetComponent<State_Idle>();
        wander = GetComponent<State_Wander>();
        chase = GetComponent<State_Chase>();
        attack = GetComponent<State_Attack>();

        if (!idle) Debug.LogError($"{name}: Missing State_Idle in E_Controller.");
        if (!wander) Debug.LogError($"{name}: Missing State_Wander in E_Controller.");
        if (!chase) Debug.LogError($"{name}: Missing State_Chase in E_Controller.");
        if (!attack) Debug.LogError($"{name}: Missing State_Attack in E_Controller.");
    }

    void OnEnable() => SwitchState(defaultState);

    void OnDisable()
    {
        idle.enabled   = false;
        wander.enabled = false;
        chase.enabled  = false;
        attack.enabled = false;
    }

    void Update()
    {
        // Always check attackCircle first. If this hits the player -> the player also inside the detectionCircle
        Collider2D attackCircle = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
        // If attackCircle not null, reuse it. Otherwise check the detectionCircle
        Collider2D detectionCircle = attackCircle ?? Physics2D.OverlapCircle((Vector2)transform.position, detectionRange, playerLayer);
        // Check true/false for each circle depending on player location
        bool targetInsideAttackCircle = attackCircle;
        bool targetInsideDetectionCircle = detectionCircle;

        if (targetInsideDetectionCircle) currentTarget = detectionCircle.transform;

        // Optional: the current state reacts immediately without forcing a state change
        // if (chase.enabled) chase.SetRanges(attackRange);
        // if (attack.enabled) attack.SetRanges(attackRange);

        // Decide desired state from ring position
        EState desiredState =
        targetInsideAttackCircle    ? EState.Attack :
        targetInsideDetectionCircle ? EState.Chase  :
                                      defaultState;

        // Never interrupt an active attack clip
        if (currentState == EState.Attack && attack.IsAttacking && desiredState != EState.Attack) return;
        // If nothing changed just return
        if (desiredState == currentState) return;

        // Apply the change
        switch (desiredState)
        {
            case EState.Attack:
                attack.SetTarget(currentTarget);
                SwitchState(EState.Attack);
                break;

            case EState.Chase:
                chase.SetTarget(currentTarget);
                SwitchState(EState.Chase);
                break;

            default:
                currentTarget = null;
                SwitchState(defaultState);
                break;
        }
    }

    public void SwitchState(EState s)
    {
        if (currentState == s) return;
        currentState = s;

        idle.enabled = (s == EState.Idle);
        wander.enabled = (s == EState.Wander);
        chase.enabled = (s == EState.Chase);
        attack.enabled = (s == EState.Attack);

        // On enter, provide context to newly enabled state
        if (s == EState.Chase)
        {
            chase.SetTarget(currentTarget);
            chase.SetRanges(attackRange);
        }
        else if (s == EState.Attack)
        {
            attack.SetTarget(currentTarget);
            attack.SetRanges(attackRange);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.65f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
