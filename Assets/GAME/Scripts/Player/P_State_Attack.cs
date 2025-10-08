using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class P_State_Attack : MonoBehaviour
{
    [Header("References")]
    public W_Base weaponToUse;
    public Vector2 attackDir;

    [Header("Attack Timings")]
    float attackDuration = 0.45f;
    float hitDelay = 0.15f;

    // Cache
    Animator anim;
    P_Controller controller;

    void Awake()
    {
        anim        = GetComponent<Animator>();
        controller  = GetComponent<P_Controller>();

        if (!anim) Debug.LogError("P_State_Attack: missing Animator");
        if (!controller) Debug.LogError("P_State_Attack: missing P_Controller");
    }

    void OnEnable()
    {
        anim.SetBool("isAttacking", true); // drives animator transitions
    }

    void OnDisable()
    {
        StopAllCoroutines(); // If player dies -> stop attack immediately
        anim.SetBool("isAttacking", false); // Exit Attack animation by bool
        controller.SetAttacking(false); // Normal finish
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
    public void Attack(W_Base weapon, Vector2 attackDir)
    {
        weaponToUse = weapon;
        this.attackDir = attackDir; // Controller already normalized this

        anim.SetFloat("atkX", attackDir.x);
        anim.SetFloat("atkY", attackDir.y);

        StartCoroutine(AttackRoutine());
    }

    // Just handles animation timing
    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(hitDelay);
        weaponToUse.Attack(attackDir);
        yield return new WaitForSeconds(attackDuration - hitDelay);

        controller.SetAttacking(false); // Interrupted by Dead
    }
}
