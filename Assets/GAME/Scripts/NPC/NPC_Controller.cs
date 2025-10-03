using UnityEngine;

[DisallowMultipleComponent]
public class NPC_Controller : MonoBehaviour, I_Controller
{
    public enum NPCState { Idle, Wander, Talk }

    [Header("References")]
    public NPCState defaultState = NPCState.Wander;
    public State_Wander wander;
    public State_Talk talk;
    public State_Idle idle;
    Rigidbody2D rb;
    C_Stats stats;

    NPCState current;
    Vector2 desiredVelocity;

    void Awake()
    {
        wander ??= GetComponent<State_Wander>();
        talk ??= GetComponent<State_Talk>();
        idle ??= GetComponent<State_Idle>();

        rb ??= GetComponent<Rigidbody2D>();
        stats ??= GetComponent<C_Stats>();

        if (!wander) Debug.LogError($"{name}: State_Wander is missing in NPC_Controller");
        if (!talk) Debug.LogError($"{name}: State_Talk is missing in NPC_Controller");
        if (!idle) Debug.LogError($"{name}: State_Idle is missing in NPC_Controller");
        if (!rb) Debug.LogError($"{name}: Rigidbody2D is missing in NPC_Controller");
        if (!stats) Debug.LogError($"{name}: C_Stats is missing in NPC_Controller");
    }

    void OnEnable()
    {
        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        if (wander) wander.enabled = false;
        if (talk)   talk.enabled   = false;
        if (idle)   idle.enabled   = false;

        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    // States publish intent, controller applies in FixedUpdate
    public void SetDesiredVelocity(Vector2 desiredVelocity) => this.desiredVelocity = desiredVelocity;

    // Apply movement in FixedUpdate for consistent physics
    void FixedUpdate()
    {
        rb.linearVelocity = desiredVelocity;
    }

    // Switch state and enable/disable relevant components
    public void SwitchState(NPCState newState)
    {
        if (current == newState) return;
        current = newState;

        // Enable exactly one state
        if (idle)   idle.enabled   = newState == NPCState.Idle;
        if (wander) wander.enabled = newState == NPCState.Wander;
        if (talk)   talk.enabled   = newState == NPCState.Talk;

        // Reset movement intent on enter
        desiredVelocity = Vector2.zero;
    }

    // Triggered Talk state when player enters trigger collider (ONLY if NPC has Talk state)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Make Talk face the player before switching
        if (talk)
        {
            talk.SetTarget(other.transform);
            SwitchState(NPCState.Talk);
        }
        else return;
    }

    // Revert to default state when player exits trigger collider
    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !talk) return;
        SwitchState(defaultState);
    }
}
