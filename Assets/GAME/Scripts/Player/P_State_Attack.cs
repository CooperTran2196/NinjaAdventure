using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class P_State_Attack : MonoBehaviour
{
    [Header("Attack Timings")]
    [Min(0f)] public float attackDuration = 0.45f;
    [Min(0f)] public float hitDelay = 0.15f;

    // Cache
    Animator anim;
    P_Controller controller;

    // Runtime
    W_Base weaponToUse;
    Vector2 attackDir;

    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<P_Controller>();

        if (!anim) Debug.LogError("P_State_Attack: missing Animator");
        if (!controller) Debug.LogError("P_State_Attack: missing P_Controller");
    }

    void OnDisable()
    {
        controller.SetAttacking(false); // Attack state only turns OFF
        controller.SetDesiredVelocity(Vector2.zero);
        anim.SetBool("isAttacking", false);
    }

    void Update()
    {
        // No movement while in attack state; controller still applies knockback globally
        controller.SetDesiredVelocity(Vector2.zero);

        // Update facing for idle state (always use last attack direction)
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
        anim.SetFloat("idleX", attackDir.x);
        anim.SetFloat("idleY", attackDir.y);
    }

    // Attack with weapon and direction (called by controller)
    public void Attack(W_Base weapon, Vector2 attackDir)
    {
        weaponToUse = weapon;
        this.attackDir = attackDir; // Controller already normalized this
        
        if (weaponToUse != null) // Only check null since controller might pass null
        {
            StartCoroutine(AttackRoutine());
        }
    }

    // Attack coroutine (simplified - just handles animation timing)
    IEnumerator AttackRoutine()
    {
        anim.SetBool("isAttacking", true);

        // Set attack direction for animation
        anim.SetFloat("atkX", attackDir.x);
        anim.SetFloat("atkY", attackDir.y);

        yield return new WaitForSeconds(hitDelay);
        weaponToUse.Attack(attackDir);
        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitDelay));

        // OnDisable will handle controller.SetAttacking(false) when state switches
        anim.SetBool("isAttacking", false);
    }

}
