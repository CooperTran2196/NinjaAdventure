using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_State_Attack : MonoBehaviour
{
    [Header("References")]
    Animator     anim;
    P_Controller controller;
    C_Stats      c_Stats;

    [Header("Attack Settings")]
    public W_Base  activeWeapon;
    public Vector2 attackDir;
    
    [Header("Timing")]
    public float attackAnimDuration = 0.45f;
    public float hitDelay           = 0.15f;

    [Header("Combo System")]
    [Range(0, 2)]   public int   comboIndex         = 0;
                    public float comboInputWindow   = 1.0f;
                    public float comboWindowTimer   = 0f;
                    public bool  comboInputQueued   = false;
    
    [Header("Options")]
    public bool lockIdleToAttackDir = false;
    
    // Hit tracking per attack (prevents double-hits on same target in one attack)
    HashSet<C_Health> hitTargetsThisAttack = new HashSet<C_Health>();

    void Awake()
    {
        anim       = GetComponent<Animator>();
        controller = GetComponent<P_Controller>();
        c_Stats    = GetComponent<C_Stats>();
    }

    void OnEnable()
    {
        anim.SetBool("isAttacking", true);
    }

    void OnDisable()
    {
        StopAllCoroutines();
        anim.SetBool("isAttacking", false);
        anim.speed = 1f;
        
        controller.SetAttacking(false);
        ResetCombo();
        
        // Re-enable movement animation if player is moving
        var moveState = GetComponent<P_State_Movement>();
        if (moveState?.enabled == true) anim.SetBool("isMoving", true);
    }

    void Update()
    {
        // Keep idle facing towards attack direction (optional)
        if (lockIdleToAttackDir)
        {
            anim.SetFloat("moveX", 0f);
            anim.SetFloat("moveY", 0f);
            anim.SetFloat("idleX", attackDir.x);
            anim.SetFloat("idleY", attackDir.y);
        }
    }

    // ATTACK SYSTEM

    // Attack with weapon and direction (called by controller)
    public void Attack(W_Base activeWeapon, Vector2 attackDir)
    {
        this.activeWeapon = activeWeapon;
        this.attackDir    = attackDir;

        anim.SetFloat("atkX", attackDir.x);
        anim.SetFloat("atkY", attackDir.y);
        anim.Play("Attack", -1, 0f);

        StartCoroutine(AttackRoutine());
    }

    // Handles combo chain, animation timing + weapon showTime lockout
    IEnumerator AttackRoutine()
    {
        // Get combo-specific showTime
        float weaponShowTime = activeWeapon.weaponData.comboShowTimes[comboIndex];
        
        // Calculate actual duration based on maxAttackSpeed
        // Combine weapon base speed + player speed bonus
        float totalSpeed           = activeWeapon.weaponData.maxAttackSpeed + c_Stats.attackSpeed;
        float speedMultiplier      = 1.0f - (totalSpeed * 0.1f); // Linear: 1 = 10% faster
        float actualWeaponDuration = weaponShowTime * speedMultiplier;
        
        // IMMEDIATELY open input window (allows button mashing)
        comboWindowTimer = comboInputWindow;
        
        // Phase 1: Wait for hit delay, then trigger weapon attack
        yield return new WaitForSeconds(hitDelay);
        
        // Update attack direction based on current mouse position (allows direction changes mid-combo)
        Vector2 currentMouseAim = controller.ReadMouseAim();
        if (currentMouseAim != Vector2.zero)
        {
            attackDir = currentMouseAim;
            anim.SetFloat("atkX", attackDir.x);
            anim.SetFloat("atkY", attackDir.y);
        }
        
        activeWeapon.Attack(attackDir);
        
        // Phase 2: Calculate remaining animation time
        float remainingAnimTime = attackAnimDuration - hitDelay;
        
        // Phase 3: Wait for animation/showTime while checking for input
        float elapsed = 0f;
        
        while (elapsed < actualWeaponDuration)
        {
            // Countdown input window timer
            if (comboWindowTimer > 0f) comboWindowTimer -= Time.deltaTime;
            
            elapsed += Time.deltaTime;
            
            // Freeze animation at final frame if weapon duration > animation duration
            if (actualWeaponDuration > attackAnimDuration && elapsed >= remainingAnimTime && anim.speed > 0f)
            {
                anim.speed = 0f;
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
            StartCoroutine(AttackRoutine());
        }
        else
        {
            // Combo complete or timed out
            controller.SetAttacking(false);
            enabled = false;
        }
    }

    // COMBO SYSTEM

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
        comboIndex       = 0;
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

    // PUBLIC API (Getters)

    public W_Base GetActiveWeapon() => activeWeapon;
    public int    GetComboIndex()   => comboIndex;

    // Get current movement penalty based on combo stage (with reduction bonus: 1 = 1%)
    public float GetCurrentMovePenalty()
    {
        if (activeWeapon?.weaponData == null) return 0.5f;
        
        float basePenalty = activeWeapon.weaponData.comboMovePenalties[comboIndex];
        float reduction   = c_Stats.movePenaltyReduction / 100f;
        
        // Apply reduction: basePenalty = 0.6 (40% penalty), reduction = 10 (10% less)
        // Result: 0.6 + (1 - 0.6) * 0.1 = 0.64 (36% penalty)
        return basePenalty + (1f - basePenalty) * reduction;
    }
}
