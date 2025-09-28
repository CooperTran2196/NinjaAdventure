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

        if (!idle)   Debug.LogError($"{name}: State_Idle is missing in E_Controller.");
        if (!wander) Debug.LogWarning($"{name}: State_Wander is missing in E_Controller.");
        if (!attack) Debug.LogError($"{name}: State_Attack is missing in E_Controller.");
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
        // Compute detection once per frame and reuse below
        var target = Physics2D.OverlapCircle((Vector2)transform.position, attack.detectionRadius, attack.playerLayer);

        // 1) If NOT in Attack, enter Attack on sight
        if (current != EState.Attack)
        {
            if (target)
            {
                attack.SetTarget(target.transform);
                SwitchState(EState.Attack);
            }
            return;
        }

        // 2) Only switch out of Attack State if the attack is done
        if (!attack.IsAttacking)
        {
            if (!target) SwitchState(defaultState);
        }
    }

    public void SwitchState(EState newState)
    {
        if (current == newState) return;
        current = newState;

        idle.enabled   = (newState == EState.Idle);
        wander.enabled = (newState == EState.Wander);
        attack.enabled = (newState == EState.Attack);
    }
}
