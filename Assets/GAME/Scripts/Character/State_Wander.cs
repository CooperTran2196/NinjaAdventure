// NPC_State_Wander.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(C_Stats))]
[DisallowMultipleComponent]

// Exclusive wander state for NPCs (tutorial-style).
// Does NOT depend on C_State. Single writer of rb.linearVelocity while enabled.
public class State_Wander : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    Animator anim;  // NPC sprite animator (Idle/Walk graph)
    C_Stats c_Stats;

    [Header("Wander Area")]
    public Vector2 startCenter;
    public float width = 6f;
    public float height = 4f;

    [Header("Movement")]
    public float pauseDuration = 1f; // idle time at edges / on bump

    [Header("Animation")]
    public string idleState = "Idle";
    public string walkState = "Walk";

    // runtime
    Vector2 destination;
    Vector2 dir;
    Vector2 lastMove;
    bool isWandering;
    const float MIN_DISTANCE = 0.1f;
    Vector2 knockback;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        c_Stats = GetComponent<C_Stats>();

        if (!rb) Debug.LogError($"{name}: Rigidbody2D missing in NPC_State_Wander.");
        if (!c_Stats) Debug.LogError($"{name}: C_Stats missing in NPC_State_Wander.");
        if (!anim) Debug.LogError($"{name}: Animator (in children) missing in NPC_State_Wander.");

        // Use current spawn as center by default
        if (startCenter == Vector2.zero) startCenter = (Vector2)transform.position;
    }

    void OnEnable()
    {
        isWandering = true;
        anim.SetBool("isWandering", true);
        rb.bodyType = RigidbodyType2D.Dynamic;
        StopAllCoroutines();
        StartCoroutine(PauseAndPickNewDestination());
    }

    void OnDisable()
    {
        anim.SetBool("isWandering", false);
        StopAllCoroutines();
        isWandering = false;
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (!isWandering) return;

        if (Vector2.Distance(transform.position, destination) < MIN_DISTANCE)
        {
            StopAllCoroutines();
            StartCoroutine(PauseAndPickNewDestination());
            return;
        }

        dir = (destination - (Vector2)transform.position).normalized;

        if (dir.sqrMagnitude > 0f) lastMove = dir;

        // Update animator floats to keep facing consistent
        anim.SetFloat("moveX", dir.x);
        anim.SetFloat("moveY", dir.y);
        anim.SetFloat("idleX", lastMove.x);
        anim.SetFloat("idleY", lastMove.y);
    }

    void FixedUpdate()
    {
        if (!isWandering)
        {
            if (rb.linearVelocity != Vector2.zero) rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 final = (dir * c_Stats.MS) + knockback;
        rb.linearVelocity = final;

        if (knockback.sqrMagnitude > 0f)
        {
            float step = c_Stats.KR * Time.fixedDeltaTime;
            knockback = Vector2.MoveTowards(knockback, Vector2.zero, step);
        }
    
    }

    IEnumerator PauseAndPickNewDestination()
    {
        // pause
        isWandering = false;
        rb.linearVelocity = Vector2.zero;
        anim?.Play(idleState);

        yield return new WaitForSeconds(pauseDuration);

        // new target + resume
        destination = GetRandomEdgePoint();
        isWandering = true;
        anim?.Play(walkState);
    }

    Vector2 GetRandomEdgePoint()
    {
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        int edge = Random.Range(0, 4); // 0=Left, 1=Right, 2=Bottom, 3=Top
        switch (edge)
        {
            case 0: // Left
                return new Vector2(startCenter.x - halfW,
                    Random.Range(startCenter.y - halfH, startCenter.y + halfH));
            case 1: // Right
                return new Vector2(startCenter.x + halfW,
                    Random.Range(startCenter.y - halfH, startCenter.y + halfH));
            case 2: // Bottom
                return new Vector2(
                    Random.Range(startCenter.x - halfW, startCenter.x + halfW),
                    startCenter.y - halfH);
            case 3: // Top
                return new Vector2(
                    Random.Range(startCenter.x - halfW, startCenter.x + halfW),
                    startCenter.y + halfH);
        }
        return startCenter;
    }

    void OnCollisionEnter2D(Collision2D _)
    {
        if (isWandering) StartCoroutine(PauseAndPickNewDestination());
    }
    public void ReceiveKnockback(Vector2 impulse) => knockback += impulse;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        var size = new Vector3(width, height, 0f);
        var center = Application.isPlaying ? (Vector3)startCenter : transform.position;
        Gizmos.DrawWireCube(center, size);
    }
}
