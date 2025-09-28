// NPC_State_Wander.cs
using UnityEngine;
using System.Collections;

// Exclusive wander state for NPCs (tutorial-style).
// Does NOT depend on C_State. Single writer of rb.linearVelocity while enabled.
public class State_Wander : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator characterAnimator;  // NPC sprite animator (Idle/Walk graph)
    public C_Stats c_Stats;

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

    void Awake()
    {
        rb               ??= GetComponent<Rigidbody2D>();
        characterAnimator ??= GetComponentInChildren<Animator>();
        c_Stats          ??= GetComponent<C_Stats>();

        if (!rb)       Debug.LogError($"{name}: Rigidbody2D missing in NPC_State_Wander.");
        if (!c_Stats)  Debug.LogError($"{name}: C_Stats missing in NPC_State_Wander.");
        if (!characterAnimator) Debug.LogError($"{name}: Animator (in children) missing in NPC_State_Wander.");

        // Use current spawn as center by default
        if (startCenter == Vector2.zero) startCenter = (Vector2)transform.position;
    }

    void OnEnable()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        StopAllCoroutines();
        StartCoroutine(PauseAndPickNewDestination());
    }

    void OnDisable()
    {
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
        characterAnimator?.SetFloat("moveX", dir.x);
        characterAnimator?.SetFloat("moveY", dir.y);
        characterAnimator?.SetFloat("idleX", lastMove.x);
        characterAnimator?.SetFloat("idleY", lastMove.y);
    }

    void FixedUpdate()
    {
        if (!isWandering)
        {
            if (rb.linearVelocity != Vector2.zero) rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = dir * c_Stats.MS; // speed from stats directly
    }

    IEnumerator PauseAndPickNewDestination()
    {
        // pause
        isWandering = false;
        rb.linearVelocity = Vector2.zero;
        characterAnimator?.Play(idleState);

        yield return new WaitForSeconds(pauseDuration);

        // new target + resume
        destination = GetRandomEdgePoint();
        isWandering = true;
        characterAnimator?.Play(walkState);
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
        if (isWandering)
        {
            StopAllCoroutines();
            StartCoroutine(PauseAndPickNewDestination());
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        var size = new Vector3(width, height, 0f);
        var center = Application.isPlaying ? (Vector3)startCenter : transform.position;
        Gizmos.DrawWireCube(center, size);
    }
}
