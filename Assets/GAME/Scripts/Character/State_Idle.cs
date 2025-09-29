using UnityEngine;

public class State_Idle : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator characterAnimator;
    C_Stats stats;

    Vector2 knockback;

    void Awake()
    {
        rb ??= GetComponent<Rigidbody2D>();
        characterAnimator ??= GetComponentInChildren<Animator>();
        stats             ??= GetComponent<C_Stats>();
        if (!rb) Debug.LogError($"{name}: Rigidbody2D missing in NPC_State_Idle.");
    }

    void OnEnable()
    {
        rb.linearVelocity = Vector2.zero;
        var a = characterAnimator;
        if (!a) return;
        a.SetBool("isMoving", false);
        a.SetBool("isWandering", false);
        a.SetBool("isAttacking", false);
        // Optionally set idleX/idleY to keep the last facing if you want
    }
        void FixedUpdate()
    {
        // Idle has no intent velocity; only knockback moves it.
        if (knockback.sqrMagnitude > 0f)
        {
            float step = (stats ? stats.KR : 30f) * Time.fixedDeltaTime;
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, step);
            rb.linearVelocity = knockback;
        }
        else rb.linearVelocity = Vector2.zero;
    }

    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;

}
