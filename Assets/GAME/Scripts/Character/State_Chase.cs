using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(C_Stats))]
[DisallowMultipleComponent]
public class State_Chase : MonoBehaviour
{
    [Header("Animation States")]
    public string idleState = "Idle";
    public string walkState = "Move";  // name in your Animator

    [Header("Tuning")]
    public float stopBuffer = 0.10f;   // hover just outside the inner ring

    // Ranges are injected by controller
    float detectionRange = 3f;
    float attackRange    = 1.2f;

    // Cache
    Rigidbody2D rb;
    Animator anim;
    C_Stats stats;

    // Runtime
    Transform target;
    Vector2 velocity, knockback, lastMove = Vector2.down;
    string lastPlayed;

    void Awake()
    {
        rb    ??= GetComponent<Rigidbody2D>();
        anim  ??= GetComponent<Animator>();
        stats ??= GetComponent<C_Stats>();

        if (!stats) Debug.LogError($"{name}: C_Stats missing on State_Chase.");
    }

    void OnEnable()
    {
        lastPlayed = null;
        PlayIfChanged(idleState);
    }

    void OnDisable()
    {
        velocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        lastPlayed = null;
    }

    public void SetTarget(Transform t) => target = t;
    public void SetRanges(float detect, float atk) { detectionRange = detect; attackRange = atk; }

    void Update()
    {
        if (!target)
        {
            velocity = Vector2.zero;
            UpdateFloats(Vector2.zero);
            PlayIfChanged(idleState);
            return;
        }

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        float   d  = to.magnitude;
        Vector2 dir = d > 0.0001f ? to.normalized : lastMove;

        // Move toward target while outside inner ring
        velocity = (d > (attackRange + stopBuffer)) ? dir * stats.MS : Vector2.zero;

        // Anim
        UpdateFloats(velocity);
        PlayIfChanged(velocity.sqrMagnitude > 0f ? walkState : idleState);
    }

    void FixedUpdate()
    {
        Vector2 final = velocity + knockback;
        rb.linearVelocity = final;

        if (knockback.sqrMagnitude > 0f)
        {
            float step = stats.KR * Time.fixedDeltaTime;
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, step);
        }
    }

    void UpdateFloats(Vector2 move)
    {
        if (move.sqrMagnitude > 0f) lastMove = move.normalized;
        anim.SetFloat("moveX", move.x);
        anim.SetFloat("moveY", move.y);
        anim.SetFloat("idleX", lastMove.x);
        anim.SetFloat("idleY", lastMove.y);
    }

    void PlayIfChanged(string stateName)
    {
        if (lastPlayed == stateName) return;
        anim.Play(stateName);
        lastPlayed = stateName;
    }

    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;
}
