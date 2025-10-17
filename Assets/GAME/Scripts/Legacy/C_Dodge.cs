using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

[DisallowMultipleComponent]

public class C_Dodge : MonoBehaviour
{
    [Header("References")]
    C_Stats c_Stats;
    C_State c_State;

    Animator animator;
    P_InputActions input;

    [Header("Only choose one")]
    public P_Movement p_Movement;
    public E_Movement e_Movement;
    C_AfterimageSpawner afterimage;

    [Header("Player = true, Enemy = false")]
    public bool usePlayerInput = true;

    [Header("State (read-only)")]
    public bool IsDodging { get; private set; }
    public Vector2 ForcedVelocity => IsDodging ? forcedVelocity : Vector2.zero;

    float cooldownTimer;
    Vector2 forcedVelocity;

    void Awake()
    {
        animator    ??= GetComponent<Animator>();
        c_Stats     ??= GetComponent<C_Stats>();
        c_State     ??= GetComponent<C_State>();
        p_Movement  ??= GetComponent<P_Movement>();
        e_Movement  ??= GetComponent<E_Movement>();
        afterimage  ??= GetComponent<C_AfterimageSpawner>();

        input ??= new P_InputActions();

        if (!animator)                   Debug.LogError($"{name}: Animator is missing in C_Dodge");
        if (!c_Stats)                    Debug.LogError($"{name}: C_Stats is missing in C_Dodge");
        if (!p_Movement && !e_Movement)  Debug.LogError($"{name}: *_Movement is missing in C_Dodge");
        if (!afterimage)                 Debug.LogError($"{name}: C_AfterimageSpawner is missing in C_Dodge");
    }

    void OnEnable()
    {
        if (usePlayerInput) input.Player.Dodge.Enable();
    }

    void OnDisable()
    {
        if (usePlayerInput)
        {
            input.Player.Dodge.Disable();
        }
    }

    void OnDestroy()
    {
        input?.Dispose();
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (!usePlayerInput) return;

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
        if (c_State.lockDodge && c_State.Is(C_State.ActorState.Attack)) return;
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
