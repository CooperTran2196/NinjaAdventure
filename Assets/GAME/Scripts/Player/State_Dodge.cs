using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class State_Dodge : MonoBehaviour
{
    [Header("References")]
    public C_Stats c_Stats;
    public C_State c_State;
    public Animator animator;
    public C_AfterimageSpawner afterimage;

    [Header("State (read-only)")]
    public bool IsDodging { get; private set; }
    public Vector2 ForcedVelocity => IsDodging ? forcedVelocity : Vector2.zero;

    float cooldownTimer;
    Vector2 forcedVelocity;

    void Awake()
    {
        c_Stats    ??= GetComponent<C_Stats>();
        c_State    ??= GetComponent<C_State>();
        animator   ??= GetComponent<Animator>();
        afterimage ??= GetComponent<C_AfterimageSpawner>();

        if (!c_Stats) Debug.LogWarning($"{name}: C_Stats missing on State_Dodge");
        if (!c_State) Debug.LogWarning($"{name}: C_State missing on State_Dodge");
        if (!animator) Debug.LogWarning($"{name}: Animator missing on State_Dodge");
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public void RequestDodge(Vector2 dir)
    {
        if (c_State != null && c_State.lockDodge && c_State.Is(C_State.ActorState.Attack)) return;
        if (IsDodging) return;
        if (cooldownTimer > 0f) return;

        Vector2 ndir = (dir.sqrMagnitude > 0f) ? dir.normalized : Vector2.down;

        var sr = GetComponent<SpriteRenderer>();
        var lockedSprite = sr ? sr.sprite : null;
        bool flipX = sr ? sr.flipX : false;
        bool flipY = sr ? sr.flipY : false;

        float speed = c_Stats.dodgeSpeed;
        float distance = c_Stats.dodgeDistance;
        float duration = (speed > 0f) ? (distance / speed) : 0f;

        IsDodging = true;
        forcedVelocity = ndir * speed;

        if (duration > 0f)
            afterimage?.StartBurst(duration, lockedSprite, flipX, flipY);

        animator?.SetBool("isDodging", true);
        StartCoroutine(DodgeRoutine(duration));
    }

    IEnumerator DodgeRoutine(float duration)
    {
        if (duration > 0f) yield return new WaitForSeconds(duration);

        IsDodging = false;
        forcedVelocity = Vector2.zero;
        animator?.SetBool("isDodging", false);

        cooldownTimer = c_Stats.dodgeCooldown;
    }
}
