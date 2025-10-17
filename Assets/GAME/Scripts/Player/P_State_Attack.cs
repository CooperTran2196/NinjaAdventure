using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_State_Attack : MonoBehaviour
{
    [Header("References")]
    public W_Base  activeWeapon;
    public Vector2 attackDir;

    [Header("Attack Timings")]
    float attackAnimDuration = 0.45f; // How long the attack animation actually is
    float hitDelay       = 0.15f;

    [Header("Combo System")]
    [SerializeField] int comboIndex = 0;           // 0=slash down, 1=slash up, 2=thrust
    [SerializeField] float comboInputWindow = 1.0f; // Time window to press next attack (VERY forgiving)
    [SerializeField] float comboWindowTimer = 0f;   // Countdown timer
    [SerializeField] bool comboInputQueued = false; // Input buffering flag
    
    // Hit tracking per attack (prevents double-hits on same target in one attack)
    HashSet<C_Health> hitTargetsThisAttack = new HashSet<C_Health>();

    // Cache
    Animator anim;
    P_Controller controller;
    C_Stats c_Stats;

    void Awake()
    {
        anim        = GetComponent<Animator>();
        controller  = GetComponent<P_Controller>();
        c_Stats     = GetComponent<C_Stats>();
    }

    void OnEnable()
    {
        anim.SetBool("isAttacking", true); // animator enter
    }

    void OnDisable()
    {
        StopAllCoroutines(); // If player dies -> stop attack immediately
        anim.SetBool("isAttacking", false); // Exit Attack animation by bool
        
        // Resume animation speed if it was frozen
        anim.speed = 1f;
        
        controller.SetAttacking(false); // Normal finish
        
        // Reset combo on disable
        ResetCombo();
        
        // Re-enable movement animation if player is moving
        var moveState = GetComponent<P_State_Movement>();
        if (moveState != null && moveState.enabled)
        {
            anim.SetBool("isMoving", true);
        }
    }

    void Update()
    {
        // OPTIONAL: Keep idle facing towards attack direction - else use last movement direction
        // anim.SetFloat("moveX", 0f);
        // anim.SetFloat("moveY", 0f);
        // anim.SetFloat("idleX", attackDir.x);
        // anim.SetFloat("idleY", attackDir.y);
    }

    // Attack with weapon and direction (called by controller)
    public void Attack(W_Base activeWeapon, Vector2 attackDir)
    {
        this.activeWeapon = activeWeapon;
        this.attackDir = attackDir; // Controller already normalized

        anim.SetFloat("atkX", attackDir.x);
        anim.SetFloat("atkY", attackDir.y);
        
        // Restart animation from beginning for combo visual consistency
        anim.Play("Attack", -1, 0f);

        StartCoroutine(AttackRoutine());
    }
    
    // Public getter for movement state to access current weapon
    public W_Base GetActiveWeapon() => activeWeapon;
    
    // Get current combo index for movement penalty
    public int GetComboIndex() => comboIndex;
    
    // Get current movement penalty based on combo stage (with reduction bonus: 1 = 1%)
    public float GetCurrentMovePenalty()
    {
        if (activeWeapon?.weaponData == null) return 0.5f; // Fallback
        
        float basePenalty = activeWeapon.weaponData.comboMovePenalties[comboIndex];
        
        // Apply reduction: 1 = 1% less penalty
        // Example: basePenalty = 0.6 (40% penalty), reduction = 10 (10% less penalty)
        // Result: 0.6 + (1 - 0.6) * (10/100) = 0.64 (36% penalty)
        float reduction = c_Stats.movePenaltyReduction / 100f;
        return basePenalty + (1f - basePenalty) * reduction;
    }
    
    // Queue combo input during attack (called by controller)
    public void QueueComboInput()
    {
        // Only queue if within input window and not at final combo attack
        if (comboWindowTimer > 0f && comboIndex < 2)
        {
            comboInputQueued = true;
        }
    }
    
    // Reset combo state (called on cancel, timeout, or completion)
    public void ResetCombo()
    {
        comboIndex = 0;
        comboWindowTimer = 0f;
        comboInputQueued = false;
        hitTargetsThisAttack.Clear();
    }
    
    // Check if target was already hit in this attack
    public bool WasTargetHitThisAttack(C_Health target)
    {
        return hitTargetsThisAttack.Contains(target);
    }
    
    // Mark target as hit in this attack
    public void MarkTargetHit(C_Health target)
    {
        hitTargetsThisAttack.Add(target);
    }

    // Handles combo chain, animation timing + weapon showTime lockout
    IEnumerator AttackRoutine()
    {
        // Get combo-specific showTime
        float weaponShowTime = activeWeapon.weaponData.comboShowTimes[comboIndex];
        
        // IMMEDIATELY open input window (allows button mashing)
        comboWindowTimer = comboInputWindow; // 1.0s window
        
        // Phase 1: Wait for hit delay, then trigger weapon attack
        yield return new WaitForSeconds(hitDelay);
        activeWeapon.Attack(attackDir);
        
        // Phase 2: Calculate remaining animation time
        float remainingAnimTime = attackAnimDuration - hitDelay;
        
        // Phase 3: Wait for animation/showTime while checking for input
        float elapsed = 0f;
        
        while (elapsed < weaponShowTime)
        {
            // Countdown input window timer
            if (comboWindowTimer > 0f)
            {
                comboWindowTimer -= Time.deltaTime;
            }
            
            elapsed += Time.deltaTime;
            
            // Check if animation should freeze (showTime > animation duration)
            if (weaponShowTime > attackAnimDuration && elapsed >= remainingAnimTime && anim.speed > 0f)
            {
                anim.speed = 0f; // Freeze at final frame
            }
            
            yield return null;
        }
        
        // Restore animation speed if it was frozen
        anim.speed = 1f;
        
        // Phase 4: Determine next state (combo chain or finish)
        if (comboInputQueued && comboIndex < 2)
        {
            // Chain to next combo attack
            comboIndex++;
            comboInputQueued = false;
            hitTargetsThisAttack.Clear(); // Reset hit tracking for next attack
            
            // Restart attack routine with new combo index
            // Don't disable component - keep attacking
            StartCoroutine(AttackRoutine());
        }
        else
        {
            // Combo complete or timed out
            controller.SetAttacking(false);
            
            // Disable this state component to trigger OnDisable() and clean up
            enabled = false;
        }
    }
}
