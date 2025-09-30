using UnityEngine;

[DisallowMultipleComponent]
public class NPC_Controller : MonoBehaviour, I_Controller
{
    public enum NPCState { Idle, Wander, Talk }

    [Header("States")]
    public NPCState defaultState = NPCState.Wander;
    public State_Wander wander;
    public State_Talk talk;        // new
    public State_Idle idle;        // new

    NPCState current;

    // --- Movement (controller is the single velocity writer) ---
    Rigidbody2D rb;
    C_Stats stats;
    Vector2 desiredVelocity;

    void Awake()
    {
        wander ??= GetComponent<State_Wander>();
        talk   ??= GetComponent<State_Talk>();
        idle   ??= GetComponent<State_Idle>();

        rb    ??= GetComponent<Rigidbody2D>();
        stats ??= GetComponent<C_Stats>();

        if (!wander) Debug.LogError($"{name}: State_Wander is missing in NPC_Controller");
        if (!talk)   Debug.LogError($"{name}: State_Talk is missing in NPC_Controller");
        if (!idle)   Debug.LogError($"{name}: State_Idle is missing in NPC_Controller");
        if (!rb)     Debug.LogError($"{name}: Rigidbody2D is missing in NPC_Controller");
    }

    void OnEnable()
    {
        desiredVelocity = Vector2.zero;
        if (rb) rb.linearVelocity = Vector2.zero;
        SwitchState(defaultState);
    }

    void OnDisable()
    {
        if (wander) wander.enabled = false;
        if (talk)   talk.enabled   = false;
        if (idle)   idle.enabled   = false;

        desiredVelocity = Vector2.zero;
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    // States publish intent; controller applies in FixedUpdate
    public void SetDesiredVelocity(Vector2 v) => desiredVelocity = v;

    void FixedUpdate()
    {
        if (!rb) return;
        rb.linearVelocity = desiredVelocity;
    }

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Make Talk face the player before switching
        if (talk) talk.SetTarget(other.transform);

        SwitchState(NPCState.Talk);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        SwitchState(defaultState);
    }
}
