using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Visual")]
    public int facingDirection = 1;          // 1 = right, -1 = left

    [Header("Runtime")]
    public Vector2 lastMove = Vector2.right; // where the body is facing

    Rigidbody2D rb;
    Animator    anim;

    Vector2 input;           // latest raw WASD input
    bool    isKnockedBack;

    const float deadZone = 0.001f;   // tiny stick drift / tap filter

    /* --------------------------------------------------- */
    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    /* -------- read input, drive animator, face sprite -------- */
    void Update()
    {
        if (isKnockedBack) return;

        /* 1. Read WASD / arrow keys (no smoothing) */
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical"));

        bool moving = input.sqrMagnitude > deadZone;

        if (moving)
            lastMove = input.normalized;

        /* 2. Animator parameters */
        anim.SetBool ("isMoving", moving);
        anim.SetFloat("moveX",   moving ? input.normalized.x : 0);
        anim.SetFloat("moveY",   moving ? input.normalized.y : 0);
        anim.SetFloat("idleX",  !moving ? lastMove.x : 0);
        anim.SetFloat("idleY",  !moving ? lastMove.y : 0);

        /* 3. Flip sprite on X axis */
        if (input.x > 0 && transform.localScale.x < 0 ||
            input.x < 0 && transform.localScale.x > 0)
            Flip();
    }

    /* -------- apply physics at a steady 50 Hz -------- */
    void FixedUpdate()
    {
        if (isKnockedBack) return;

        rb.linearVelocity = input.normalized * StatsManager.Instance.speed;
    }

    /* ---------------- helpers ---------------- */
    void Flip()
    {
        facingDirection *= -1;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    /* ---------- Knock-back ---------- */
    public void Knockback(Transform enemy, float force, float stunTime)
    {
        isKnockedBack = true;

        Vector2 dir = ((Vector2)transform.position - (Vector2)enemy.position).normalized;
        rb.linearVelocity = dir * force;

        StartCoroutine(KnockbackCounter(stunTime));
    }

    IEnumerator KnockbackCounter(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.linearVelocity   = Vector2.zero;
        isKnockedBack = false;
    }
}
