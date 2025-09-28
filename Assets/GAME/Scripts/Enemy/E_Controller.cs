using UnityEngine;

[DisallowMultipleComponent]
public class E_Controller : MonoBehaviour
{
    public enum EState { Idle, Wander, Attack }

    [Header("States")]
    public EState defaultState = EState.Idle;     // you said default Idle for all; can override in Inspector
    public State_Idle   idle;                     // shared with NPC
    public State_Wander wander;                   // shared with NPC
    public State_Attack attack;                   // new merged chase+combat

    EState current;

    void Awake()
    {
        idle   ??= GetComponent<State_Idle>();
        wander ??= GetComponent<State_Wander>();
        attack ??= GetComponent<State_Attack>();

        if (!idle)   Debug.LogError($"{name}: State_Idle missing for E_Controller.");
        if (!wander) Debug.LogWarning($"{name}: State_Wander not found (optional).");
        if (!attack) Debug.LogError($"{name}: State_Attack missing for E_Controller.");
    }

    void OnEnable()
    {
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        if (idle)   idle.enabled   = false;
        if (wander) wander.enabled = false;
        if (attack) attack.enabled = false;
    }

    void Update()
    {
        if (!attack) return;

        // 1) If NOT in Attack, scan outer circle and enter Attack on sight
        if (current != EState.Attack)
        {
            var col = Physics2D.OverlapCircle((Vector2)transform.position, attack.detectionRadius, attack.playerLayer);
            if (col)
            {
                attack.SetTarget(col.transform);
                SwitchState(EState.Attack);
            }
            return;
        }

        // 2) While in Attack, leave only when target left outer ring AND attack isn't mid-clip
        if (!attack.IsAttacking)
        {
            var col = Physics2D.OverlapCircle((Vector2)transform.position, attack.detectionRadius, attack.playerLayer);
            if (!col) SwitchState(defaultState);
        }
    }

    public void SwitchState(EState newState)
    {
        if (current == newState) return;
        current = newState;

        if (idle)   idle.enabled   = (newState == EState.Idle);
        if (wander) wander.enabled = (newState == EState.Wander);
        if (attack) attack.enabled = (newState == EState.Attack);
    }
}
