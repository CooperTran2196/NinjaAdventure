using System.Collections;
using UnityEngine;

public class State_Attack : MonoBehaviour
{
    [Header("References")]
    public W_Base activeWeapon;          // Will get this from children
    public Vector2 attackDir;            // Will calculate this internally

    [Header("Attack Timings")]
    float attackAnimDuration = 0.45f; // How long the attack animation actually is
    float hitDelay       = 0.15f;

    // cache
    Animator     anim;
    E_Controller controller;
    State_Chase  chaseState;

    void Awake()
    {
        anim            = GetComponentInChildren<Animator>();
        controller      = GetComponent<E_Controller>();
        activeWeapon    = GetComponentInChildren<W_Base>();
        chaseState      = GetComponent<State_Chase>();
    }

    void OnEnable()
    {
        anim.SetBool("isAttacking", true); // animator enter

        // Calculate aim direction towards the target provided by the controller
        Transform target = controller.GetTarget();
        if (target)
        {
            attackDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        }
        else // Fallback if target is lost somehow
        {
            attackDir = new Vector2(anim.GetFloat("idleX"), anim.GetFloat("idleY"));
        }

        // Start the attack routine
        StartCoroutine(AttackRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines(); // If enemy dies -> stop attack immediately
        anim.SetBool("isAttacking", false); // Exit Attack animation by bool
        controller.SetAttacking(false); // Normal finish
        
        // Restore animation speed in case it was frozen
        anim.speed = 1f;

        // Restore chase state if still has target
        if (chaseState != null && controller.GetTarget() != null && !chaseState.enabled)
        {
            chaseState.enabled = true;
        }
    }

    void Update()
    {
        // Keep idle facing towards attack direction 
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
        anim.SetFloat("idleX", attackDir.x);
        anim.SetFloat("idleY", attackDir.y);
    }

    // Attack with weapon and direction (called by controller via OnEnable)
    public void Attack(W_Base weapon, Vector2 dir)
    {
        activeWeapon = weapon;
        attackDir = dir;
    }

    // Public getter for chase state to access current weapon (for movement penalty)
    public W_Base GetActiveWeapon() => activeWeapon;

    // Handles animation timing + weapon showTime lockout
    IEnumerator AttackRoutine()
    {
        // Set animator direction
        anim.SetFloat("atkX", attackDir.x);
        anim.SetFloat("atkY", attackDir.y);

        // Get weapon's showTime (how long the weapon is active/visible)
        float weaponShowTime = (activeWeapon && activeWeapon.weaponData) 
            ? activeWeapon.weaponData.showTime 
            : attackAnimDuration;

        // Phase 1: Wait for hit timing
        yield return new WaitForSeconds(hitDelay);
        
        // Phase 2: Execute weapon attack
        activeWeapon.Attack(attackDir);

        // Phase 3: Wait for animation to complete
        float animRemaining = attackAnimDuration - hitDelay;
        yield return new WaitForSeconds(animRemaining);

        // Phase 4: If showTime > animation duration, freeze animation and wait
        if (weaponShowTime > attackAnimDuration)
        {
            // Freeze animation on final frame
            anim.speed = 0f;

            // Calculate lockout period
            float lockoutDuration = weaponShowTime - attackAnimDuration;

            // Wait for lockout to complete
            yield return new WaitForSeconds(lockoutDuration);

            // Restore animation speed
            anim.speed = 1f;
        }

        // Phase 5: Attack complete - signal controller and disable this state
        controller.SetAttacking(false);
        enabled = false; // Triggers OnDisable() → cleanup
    }
}
