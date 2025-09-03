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
    public C_ChangeHealth health;

    [Header("AI")]
    public LayerMask playerLayer;
    public float attackRange = 0.9f;
    public float thinkInterval = 0.5f;

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
    Color baseColor;

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

        baseColor = sprite.color;

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

        if (health != null)
        {
            health.OnDamaged += a => OnDamaged?.Invoke(a);
            health.OnHealed += a => OnHealed?.Invoke(a);
            health.OnDied += () => { OnDied?.Invoke(); Die(); };
        }
    }


    void OnDisable()
    {
        debugTakeDamageAction?.Disable();

        if (health != null)
        {
            health.OnDamaged -= a => OnDamaged?.Invoke(a);
            health.OnHealed -= a => OnHealed?.Invoke(a);
            health.OnDied -= () => { OnDied?.Invoke(); Die(); };
        }
    }



    void Update()
    {
        if (autoKill) { autoKill = false; health?.ChangeHealth(-stats.maxHP); }
        if (debugTakeDamageAction.triggered) health?.ChangeHealth(-debugHotkeyDamage);

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

        var playerHealth = c.collider.GetComponent<C_ChangeHealth>();
        if (playerHealth == null || !playerHealth.IsAlive) return; // Only damage a live player
        playerHealth.ChangeHealth(-stats.collisionDamage); //Apply damage

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

        cooldownTimer = stats.attackCooldown;
    }

    void Die()
    {
        movement?.SetDisabled(true);
        animator?.SetTrigger(deathTrigger);
        StartCoroutine(C_FX.FadeAndDestroy(sprite, deathFadeTime, gameObject));
        OnDied?.Invoke();
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.9f); // red attack ring
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    /// DEBUG
    public void Kill() => health?.ChangeHealth(-stats.maxHP);

}
