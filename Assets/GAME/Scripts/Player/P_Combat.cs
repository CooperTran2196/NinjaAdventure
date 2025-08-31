using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]

public class P_Combat : MonoBehaviour
{
    [Header("References")]
    public P_Stats stats;
    public P_Movement movement;
    public SpriteRenderer sprite;
    public Animator animator;
    public W_Base activeWeapon; // Placeholder

    [Header("Attack")]
    public float attackDuration = 0.45f; // ALWAYS matched full clip length
    public float hitDelay = 0.15f; // ALWAYS set when the hit happens
    public float attackCooldown = 1.5f; // cooldown between attacks
    public bool lockDuringAttack = true; // read by E_Movement valve

    [Header("FX Timings")]
    public float flashDuration = 0.1f; // How long the red flash lasts
    public float deathFadeTime = 1.5f; // How long to fade before destroy

    [Header("Keyword to Trigger Die Animation")]
    public string deathTrigger = "Die";

    [Header("Debug")]
    [SerializeField] private bool autoKill;
    public Key debugHotkey = Key.F1;
    public int debugHotkeyDamage = 1;
    InputAction debugTakeDamageAction;

    P_InputActions input;
    Vector2 aimDir = Vector2.down;
    const float MIN_DISTANCE = 0.0001f;

    // Placeholder for Events
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;
    public event Action OnDied;

    // Quick state check
    public bool IsAlive => stats.currentHP > 0;

    bool isAttacking;
    float cooldownTimer;

    void Awake()
    {
        animator ??= GetComponent<Animator>();
        sprite ??= GetComponent<SpriteRenderer>();
        stats ??= GetComponent<P_Stats>();
        movement ??= GetComponent<P_Movement>();
        input ??= new P_InputActions();

        if (animator == null) Debug.LogError($"{name}: Animator missing.");
        if (sprite == null) Debug.LogWarning($"{name}: SpriteRenderer missing.");
        if (stats == null) Debug.LogError($"{name}: P_Stats missing.");
        if (movement == null) Debug.LogError($"{name}: P_Movement missing.");

        animator?.SetFloat("atkX", 0f);
        animator?.SetFloat("atkY", -1f);

        debugTakeDamageAction = new InputAction(
            "DebugTakeDamage",
            InputActionType.Button,
            $"<Keyboard>/{debugHotkey.ToString().ToLower()}"
        );

    }

    void OnEnable()
    {
        input?.Enable();
        debugTakeDamageAction?.Enable();
    }

    void OnDisable()
    {
        input?.Disable();
        debugTakeDamageAction?.Disable();
    }

    void Update()
    {
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();
        if (raw.sqrMagnitude > MIN_DISTANCE) aimDir = raw.normalized;
        if (input.Player.Attack.triggered) RequestAttack();

        if (autoKill) { autoKill = false; ChangeHealth(-stats.maxHP); }
        if (debugTakeDamageAction.triggered) ChangeHealth(-debugHotkeyDamage);

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public void RequestAttack()
    {
        if (!IsAlive) return;
        if (cooldownTimer > 0f) return;

        cooldownTimer = attackCooldown;

        // Face once at attack start
        var dir = aimDir;
        animator.SetFloat("atkX", dir.x);
        animator.SetFloat("atkY", dir.y);

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetBool("isAttacking", true);

        // Delay -> Attack -> Recover
        yield return new WaitForSeconds(hitDelay);
        activeWeapon?.Attack();

        yield return new WaitForSeconds(attackDuration - hitDelay);

        isAttacking = false;
        animator.SetBool("isAttacking", false);

    }

    public void ChangeHealth(int amount)
    {
        if (!IsAlive) return;
        int newHP = stats.currentHP + amount;

        // Death path
        if (newHP <= 0)
        {
            if (amount < 0) OnDamaged?.Invoke(-amount); 
            stats.currentHP = 0;
            Die();
            return;
        }

        // Overheal cap
        if (newHP > stats.maxHP)
        {
            if (amount > 0) OnHealed?.Invoke(amount);
            stats.currentHP = stats.maxHP;
            return;
        }

        // Normal mid-range change
        stats.currentHP = newHP;
        if (amount < 0)
        {
            OnDamaged?.Invoke(-amount);
            StartCoroutine(C_FX.Flash(sprite, flashDuration));
        }
        else if (amount > 0)
        {
            OnHealed?.Invoke(amount);
        }

    }


    void Die()
    {
        movement?.SetDisabled(true);
        animator?.SetTrigger(deathTrigger);
        StartCoroutine(C_FX.FadeAndDestroy(sprite, deathFadeTime, gameObject));
        OnDied?.Invoke();
    }


    /// Briefly tints the sprite red, then restores original color
    public IEnumerator Flash()
    {
        // Save original color so we can restore it after the flash
        Color original = sprite.color;

        // Set sprite to pure red immediately
        sprite.color = Color.red;

        // Wait for 'flashDuration' seconds
        yield return new WaitForSeconds(flashDuration);
        sprite.color = original;
    }

    /// Lowers sprite alpha from 1 to 0 over 'deathFadeTime', then destroys the GameObject
    public IEnumerator FadeAndDestroy()
    {
        Color baseColor = sprite.color;   // keep RGB, only change alpha
        float elapsed = 0f;
        float invDur = 1f / deathFadeTime; // avoids a divide each frame

        while (elapsed < deathFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed * invDur); // 0 → 1 over the duration
            Color temp = baseColor;  // copy (Color is a struct)
            temp.a = 1f - t;         // fade alpha from 1 to 0
            sprite.color = temp;     // assign back to the SpriteRenderer

            yield return null;    // wait one frame
        }

        Destroy(gameObject);
    }


    /// DEBUG
    public void Kill()
    {
        if (!IsAlive) return;
        ChangeHealth(-stats.maxHP);
    }
}
