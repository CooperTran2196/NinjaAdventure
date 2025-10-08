using System.Collections;
using UnityEngine;

public class State_Attack : MonoBehaviour
{
    [Header("References")]
    public W_Base activeWeapon;          // Will get this from children
    public Vector2 attackDir;            // Will calculate this internally

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
        activeWeapon    = GetComponentInChildren<W_Base>();
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
    }

    void Update()
    {
        // Keep idle facing towards attack direction 
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
        anim.SetFloat("idleX", attackDir.x);
        anim.SetFloat("idleY", attackDir.y);
    }

    IEnumerator AttackRoutine()
    {
        // Set animator direction
        anim.SetFloat("atkX", attackDir.x);
        anim.SetFloat("atkY", attackDir.y);

        yield return new WaitForSeconds(hitDelay);
        activeWeapon.Attack(attackDir);
        yield return new WaitForSeconds(attackDuration - hitDelay);

        controller.SetAttacking(false); // Normal finish
    }


}
