using UnityEngine;
using System.Collections;

public class C_Dodge : MonoBehaviour
{
    [Header("References")]
    Animator animator;
    P_InputActions input;
    P_Stats stats;
    C_AfterimageSpawner afterimage;
    P_Movement movement; // read lastMove for direction

    [Header("Config")]
    public bool useInput = true; // Player = true; Enemy can set false and call RequestDodge(dir) manually

    [Header("State (read-only)")]
    public bool IsDodging { get; private set; }
    public Vector2 ForcedVelocity => IsDodging ? forcedVelocity : Vector2.zero;

    float cooldownTimer;
    Vector2 forcedVelocity;

    void Awake()
    {
        animator   ??= GetComponent<Animator>();
        stats      ??= GetComponent<P_Stats>();
        movement   ??= GetComponent<P_Movement>();
        afterimage ??= GetComponent<C_AfterimageSpawner>();
        input      ??= new P_InputActions();

        if (!animator) Debug.LogError($"{name}: Animator missing.");
        if (!stats)    Debug.LogError($"{name}: P_Stats missing.");
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
            Vector2 dir = (movement && movement.lastMove != Vector2.zero) ? movement.lastMove.normalized : Vector2.down;
            RequestDodge(dir);
        }
    }

    // --- External API for AI/other scripts ---
    public void RequestDodge(Vector2 dir)
    {
        if (IsDodging) return;
        if (cooldownTimer > 0f) return;

        // Lock the CURRENT sprite BEFORE we switch to the Dodge state
        var sr = GetComponent<SpriteRenderer>();
        var lockedSprite = sr ? sr.sprite : null;
        bool lockedFlipX = sr ? sr.flipX : false;
        bool lockedFlipY = sr ? sr.flipY : false;

        // Animation-cancel on purpose
        animator?.SetBool("isAttacking", false);
        animator?.SetBool("isMoving", false);

        // Duration is derived from distance & speed
        float duration = (stats.dodgeSpeed > 0f) ? (stats.dodgeDistance / stats.dodgeSpeed) : 0f;

        // Enter dodge
        IsDodging = true;
        animator?.SetBool("isDodging", true);

        forcedVelocity = dir.normalized * stats.dodgeSpeed;

        // Spawn trail using the locked sprite for the whole dodge
        afterimage?.StartBurst(duration, lockedSprite, lockedFlipX, lockedFlipY);

        StartCoroutine(DodgeRoutine(duration));
    }

    public IEnumerator DodgeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        forcedVelocity = Vector2.zero;
        IsDodging = false;
        animator?.SetBool("isDodging", false);
        cooldownTimer = stats.dodgeCooldown;
    }
}
