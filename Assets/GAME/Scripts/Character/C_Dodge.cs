using UnityEngine;
using System.Collections;

public class C_Dodge : MonoBehaviour
{
    [Header("References")]
    Animator animator;
    P_InputActions input;

    public C_Stats c_Stats;
    public C_State c_State;
    public P_Movement p_Movement; // read lastMove for direction
    public E_Movement e_Movement;
    C_AfterimageSpawner afterimage;

    [Header("Config")]
    public bool useInput = true; // Player = true; Enemy can set false and call RequestDodge(dir) manually

    [Header("State (read-only)")]
    public bool IsDodging { get; private set; }
    public Vector2 ForcedVelocity => IsDodging ? forcedVelocity : Vector2.zero;

    float cooldownTimer;
    Vector2 forcedVelocity;

    void Awake()
    {
        animator ??= GetComponent<Animator>();
        c_Stats ??= GetComponent<C_Stats>();
        c_State ??= GetComponent<C_State>();
        p_Movement ??= GetComponent<P_Movement>();
        afterimage ??= GetComponent<C_AfterimageSpawner>();

        input ??= new P_InputActions();

        if (!animator) Debug.LogError($"{name}: Animator in C_Dodge missing.");
        if (!c_Stats) Debug.LogError($"{name}: C_Stats in C_Dodge missing.");
        if (!p_Movement && !e_Movement) Debug.LogError($"{name}: *_Movement in C_Dodge missing.");
        if (!afterimage)  Debug.LogError($"{name}: C_AfterimageSpawner in C_Dodge missing.");
    }

    void OnEnable()
    {
        if (useInput) input.Player.Dodge.Enable();
    }

    void OnDisable()
    {
        if (useInput) input.Player.Dodge.Disable();
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (!useInput) return;

        if (input.Player.Dodge.WasPressedThisFrame())
        {
            // Read facing from movement; fallback down if zero
            Vector2 dir = (p_Movement && p_Movement.lastMove != Vector2.zero) ? p_Movement.lastMove.normalized : Vector2.down;
            RequestDodge(dir);
        }
    }

    // External API for AI/other scripts
    public void RequestDodge(Vector2 dir)
    {
        if (c_State && c_State.lockDodge && c_State.Is(C_State.ActorState.Attack)) return;
        if (IsDodging) return;
        if (cooldownTimer > 0f) return;

        // Lock the CURRENT sprite BEFORE we switch to the Dodge state
        var sr = GetComponent<SpriteRenderer>();
        var lockedSprite = sr ? sr.sprite : null;
        bool lockedFlipX = sr ? sr.flipX : false;
        bool lockedFlipY = sr ? sr.flipY : false;

        // Animation-cancel on purpose

        // Duration is derived from distance & speed
        float duration = (c_Stats.dodgeSpeed > 0f) ? (c_Stats.dodgeDistance / c_Stats.dodgeSpeed) : 0f;

        // Enter dodge
        IsDodging = true;

        forcedVelocity = dir.normalized * c_Stats.dodgeSpeed;

        // Spawn trail using the locked sprite for the whole dodge
        afterimage?.StartBurst(duration, lockedSprite, lockedFlipX, lockedFlipY);

        StartCoroutine(DodgeRoutine(duration));
    }

    public IEnumerator DodgeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        forcedVelocity = Vector2.zero;
        IsDodging = false;
        cooldownTimer = c_Stats.dodgeCooldown;
    }
}
