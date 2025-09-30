using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class State_Attack : MonoBehaviour
{
    [Header("Animation States")]
    public LayerMask playerLayer;

    [Header("Timing")]
    public float attackCooldown = 0.80f;
    public float attackDuration = 0.45f;
    public float hitDelay       = 0.15f;

    float attackRange = 1.2f;

    [Header("Weapon")]
    public W_Base activeWeapon;

    // Cache
    Rigidbody2D rb;
    Animator anim;
    E_Controller controller;

    // Runtime
    Transform target;
    Vector2 lastFace = Vector2.down;
    bool isAttacking;

    public bool IsAttacking => isAttacking;

    void Awake()
    {
        rb           = GetComponent<Rigidbody2D>();
        anim         = GetComponent<Animator>();
        controller   = GetComponent<E_Controller>();
        activeWeapon = GetComponentInChildren<W_Base>();

        if (!rb) Debug.LogError($"{name}: Rigidbody2D missing on State_Attack.");
        if (!anim) Debug.LogError($"{name}: Animator missing on State_Attack.");
    }

    void OnDisable()
    {
        isAttacking = false;
        controller.SetDesiredVelocity(Vector2.zero);
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isAttacking", false);
    }

    void Update()
    {
        // No movement while in attack state; controller still applies knockback globally
        controller.SetDesiredVelocity(Vector2.zero);

        if (!target) return;

        bool inInner = Physics2D.OverlapCircle((Vector2)transform.position, attackRange, playerLayer);

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        float d = to.magnitude;
        Vector2 dir = d > 0.0001f ? to.normalized : lastFace;

        UpdateIdleFacing(isAttacking ? lastFace : dir);

        if (!isAttacking && inInner && controller.GetAttackCooldown <= 0f)
            StartCoroutine(AttackRoutine(dir));
    }

    public void SetTarget(Transform t) => target = t;
    public void SetRanges(float attackRange) => this.attackRange = attackRange;

    IEnumerator AttackRoutine(Vector2 dirAtStart)
    {
        isAttacking = true;
        anim.SetBool("isAttacking", true);

        if (dirAtStart.sqrMagnitude > 0f) lastFace = dirAtStart.normalized;
        anim.SetFloat("atkX", lastFace.x);
        anim.SetFloat("atkY", lastFace.y);

        UpdateIdleFacing(lastFace);

        yield return new WaitForSeconds(hitDelay);
        activeWeapon?.Attack(lastFace);
        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitDelay));

        controller.SetAttackCooldown(attackCooldown);
        isAttacking = false;
        anim.SetBool("isAttacking", false);
    }

    void UpdateIdleFacing(Vector2 faceDir)
    {
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
        Vector2 f = faceDir.sqrMagnitude > 0f ? faceDir.normalized : lastFace;
        anim.SetFloat("idleX", f.x);
        anim.SetFloat("idleY", f.y);
    }
}
