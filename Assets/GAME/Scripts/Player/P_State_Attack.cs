using System.Collections;
using UnityEngine;

public class P_State_Attack : MonoBehaviour
{
    [Header("References")]
    public W_Base  activeWeapon;
    public Vector2 attackDir;

    [Header("Attack Timings")]
    float attackAnimDuration = 0.45f; // How long the attack animation actually is
    float hitDelay       = 0.15f;

    // Cache
    Animator anim;
    P_Controller controller;

    void Awake()
    {
        anim        = GetComponent<Animator>();
        controller  = GetComponent<P_Controller>();
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

        StartCoroutine(AttackRoutine());
    }
    
    // Public getter for movement state to access current weapon
    public W_Base GetActiveWeapon() => activeWeapon;

    // Handles animation timing + weapon showTime lockout
    IEnumerator AttackRoutine()
    {
        // Get weapon's showTime (how long the weapon is active/visible)
        float weaponShowTime = activeWeapon.weaponData.showTime;
        
        // Wait for hit delay, then trigger weapon attack
        yield return new WaitForSeconds(hitDelay);
        activeWeapon.Attack(attackDir);
        
        // Calculate how long until animation finishes
        float remainingAnimTime = attackAnimDuration - hitDelay;
        
        // If weapon showTime is longer than animation, we need to freeze and wait
        if (weaponShowTime > attackAnimDuration)
        {
            // Let animation play normally until it finishes
            yield return new WaitForSeconds(remainingAnimTime);
            
            // Freeze animation at final frame (speed = 0)
            anim.speed = 0f;
            
            // Wait for the remaining weapon showTime (lockout period)
            float lockoutTime = weaponShowTime - attackAnimDuration;
            yield return new WaitForSeconds(lockoutTime);
            
            // Restore animation speed
            anim.speed = 1f;
        }
        else
        {
            // Weapon showTime is shorter than or equal to animation
            // Just wait for the remaining animation time
            yield return new WaitForSeconds(remainingAnimTime);
        }

        controller.SetAttacking(false);
        
        // Disable this state component to trigger OnDisable() and clean up animation
        enabled = false;
    }
}
