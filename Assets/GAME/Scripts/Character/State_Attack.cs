using System.Collections;
using UnityEngine;

public class State_Attack : MonoBehaviour
{
    [Header("References")]
    public W_Base activeWeapon;          // controller passes this; assume Inspector is correct
    public Vector2 attackDir;            // controller passes a normalized dir

    [Header("Attack Timings")]
    float attackDuration = 0.45f;
    float hitDelay       = 0.15f;

    // cache
    Animator     anim;
    E_Controller controller;

    void Awake()
    {
        anim            = GetComponentInChildren<Animator>();
        controller      = GetComponent<E_Controller>();
    }

    void OnEnable()
    {
        anim.SetBool("isAttacking", true); // animator enter
    }

    void OnDisable()
    {
        StopAllCoroutines(); // If enemy dies -> stop attack immediately
        anim.SetBool("isAttacking", false); // Exit Attack animation by bool
        controller?.SetAttacking(false); // Normal finish
    }

    void Update()
    {
        // Keep idle facing towards attack direction 
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
        anim.SetFloat("idleX", attackDir.x);
        anim.SetFloat("idleY", attackDir.y);
    }
    
    // Attack with weapon and direction (called by controller)
    public void StartAttack(W_Base activeWeapon, Vector2 attackDir)
    {
        this.activeWeapon = activeWeapon;
        this.attackDir = attackDir; // controller already normalized

        anim.SetFloat("atkX", attackDir.x);
        anim.SetFloat("atkY", attackDir.y);

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(hitDelay);
        activeWeapon.Attack(attackDir);
        yield return new WaitForSeconds(attackDuration - hitDelay);

        controller.SetAttacking(false); // Interrupted by Dead
    }


}
