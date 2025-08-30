using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]

public class E_Combat : MonoBehaviour
{
    [Header("References")]
    public E_Stats stats;
    public E_Movement movement;
    public SpriteRenderer sprite;
    public Animator animator;
    public W_Base activeWeapon;   // Placeholder

    [Header("AI")]
    public LayerMask playerLayer;
    public float attackRange = 1.2f;
    public float thinkInterval = 0.2f;

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
    [SerializeField] bool autoKill;
    public Key debugHotkey = Key.F2;
    public int debugHotkeyDamage = 1;
    InputAction debugTakeDamageAction;

    // Placeholder for Events
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;
    public event Action OnDied;

    // Quick state check
    public bool IsAlive => stats.currentHP > 0;

    bool isAttacking;
    float contactTimer; // for collision damage
    float cooldownTimer; // for attacking cooldown

    void Awake()
    {
        animator ??= GetComponent<Animator>();
        sprite ??= GetComponent<SpriteRenderer>();
        stats ??= GetComponent<E_Stats>();
        movement ??= GetComponent<E_Movement>();

        if (animator == null) Debug.LogError($"{name}: Animator missing.");
        if (sprite == null) Debug.LogError($"{name}: SpriteRenderer missing.");
        if (stats == null) Debug.LogError($"{name}: E_Stats missing.");
        if (movement == null) Debug.LogError($"{name}: E_Movement missing.");


        debugTakeDamageAction = new InputAction(
            "DebugTakeDamage",
            InputActionType.Button,
            $"<Keyboard>/{debugHotkey.ToString().ToLower()}"
        );
    }

    void OnEnable()
    {
        debugTakeDamageAction.Enable();
        StartCoroutine(ThinkLoop());
    }
    void OnDisable() => debugTakeDamageAction?.Disable();

    void Update()
    {
        if (autoKill) { autoKill = false; ChangeHealth(-stats.maxHP); }
        if (debugTakeDamageAction.triggered) ChangeHealth(-debugHotkeyDamage);

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    IEnumerator ThinkLoop()
    {
        var wait = new WaitForSeconds(thinkInterval);

        while (true)
        {
            if (!IsAlive) yield break;

            if (!isAttacking)
            {
                bool inAttackRange = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);

                if (lockDuringAttack)
                {
                    // Idle in place if player is still in range
                    bool shouldHold = inAttackRange && cooldownTimer > 0f;
                    movement.SetHoldInRange(shouldHold);
                }
                else
                {
                    // Hard mode: never hold between attack
                    movement.SetHoldInRange(false);
                }

                if (inAttackRange && cooldownTimer <= 0f)
                    StartCoroutine(AttackRoutine());
            }

            yield return wait;
        }
    }

    void OnCollisionStay2D(Collision2D c)
    {
        if (!IsAlive) return;

        // Count down using physics timestep
        if (contactTimer > 0f)
        {
            contactTimer -= Time.fixedDeltaTime;
            return;
        }

        var player = c.collider.GetComponent<P_Combat>();
        if (player == null || !player.IsAlive) return; // only damage a live player

        player.ChangeHealth(-stats.collisionDamage);
        contactTimer = stats.collisionTick; // reset tick window
    }


    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetBool("isAttacking", true);

        // Aim the directional attack
        var dir = movement.lastMove;
        animator.SetFloat("atkX", dir.x);
        animator.SetFloat("atkY", dir.y);

        // Delay -> Attack -> recover
        yield return new WaitForSeconds(hitDelay);
        // Placeholder
        activeWeapon?.Attack();
        yield return new WaitForSeconds(attackDuration - hitDelay);

        // Decide stance for cooldown
        if (lockDuringAttack)
        {
            bool stillInRange = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);
            movement.SetHoldInRange(stillInRange);
        }
        else
        {
            movement.SetHoldInRange(false);
        }

        isAttacking = false;
        animator.SetBool("isAttacking", false);

        cooldownTimer = attackCooldown;
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
            StartCoroutine(Flash());
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
        StartCoroutine(FadeAndDestroy());
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.9f); // red attack ring
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    // DEBUG
    public void Kill()
    {
        if (!IsAlive) return;
        ChangeHealth(-stats.maxHP);
    }
}
