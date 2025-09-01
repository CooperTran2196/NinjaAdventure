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
    Color baseColor;
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
        baseColor = sprite.color;
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
        if (!IsAlive || cooldownTimer > 0f) return;

        cooldownTimer = stats.attackCooldown;

        // Face once at attack start
        animator.SetFloat("atkX", aimDir.x);
        animator.SetFloat("atkY", aimDir.y);
        StartCoroutine(AttackRoutine());
    }


    IEnumerator AttackRoutine()
    {
        animator.SetBool("isAttacking", true);

        // Delay -> Attack -> Recover
        yield return new WaitForSeconds(hitDelay);
        activeWeapon?.Attack();
        yield return new WaitForSeconds(attackDuration - hitDelay);

        animator.SetBool("isAttacking", false);
    }


    public void ChangeHealth(int amount)
    {
        if (!IsAlive) return;

        if (amount < 0) OnDamaged?.Invoke(-amount);
        else if (amount > 0) OnHealed?.Invoke(amount);

        stats.currentHP = Mathf.Clamp(stats.currentHP + amount, 0, stats.maxHP);

        if (stats.currentHP == 0)
            Die(); // call the death sequence in ONE place
    }

    void Die()
    {
        movement?.SetDisabled(true);
        animator?.SetTrigger(deathTrigger);
        StartCoroutine(C_FX.FadeAndDestroy(sprite, deathFadeTime, gameObject));
        OnDied?.Invoke();
    }

    /// DEBUG
    public void Kill() => ChangeHealth(-stats.maxHP);
}
