using UnityEngine;
using System.Collections;
using NUnit.Framework;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(C_State))]
[DisallowMultipleComponent]

public class C_Wander : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    C_State c_State;

    [Header("Wander Area")]
    public float wanderingSpeed = 1f;
    public float width = 6f;
    public float height = 4f;
    Vector2 startCenter;

    [Header("Idle time at edges / on bump")]
    public float pauseDuration = 1f;

    [Header("Phase flag: true = moving to the destination/ false = paused")]
    public bool isWandering;

    Vector2 destination;
    Vector2 dir;
    Vector2 lastMove; // preserves facing during pauses

    const float MIN_DISTANCE = 0.1f;

    void Awake()
    {
        rb      ??= GetComponent<Rigidbody2D>();
        c_State ??= GetComponent<C_State>();

        if (!rb)        Debug.LogError($"{name}: Rigidbody2D is missing in C_Wander.");
        if (!c_State)   Debug.LogError($"{name}: C_State is missing in C_Wander.");

        // Setup wander area at start
        startCenter = (Vector2)transform.position;
    }

    // Start wandering when enabled
    void OnEnable()
    {
        if (c_State.canWander) StartCoroutine(StopAndPickNewDestination());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isWandering = false;
        rb.linearVelocity = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if wandering is allowed in C_State
        if (!c_State.canWander) return;
        // Check if currently wandering
        if (!isWandering) return;

        // If reached destination, pause and pick a new one
        if (Vector2.Distance(transform.position, destination) < MIN_DISTANCE)
        {
            StopAllCoroutines();
            StartCoroutine(StopAndPickNewDestination());
            return;
        }

        dir = (destination - (Vector2)transform.position).normalized;

        if (dir.sqrMagnitude > 0f) lastMove = dir;
        c_State.UpdateAnimDirections(dir, lastMove); // facing via C_State
    }

    // Apply movement in FixedUpdate
    void FixedUpdate()
    {
        // Check if wandering is allowed in C_State or currently wandering
        if (!c_State.canWander || !isWandering)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Move towards destination
        rb.linearVelocity = dir * wanderingSpeed;
    }

    // Stop movement then pick a new destination
    IEnumerator StopAndPickNewDestination()
    {
        // Stop movement
        isWandering = false;
        rb.linearVelocity = Vector2.zero;

        // Stop for a bit
        yield return new WaitForSeconds(pauseDuration);

        // Pick new destination
        destination = GetRandomEdgePoint();
        isWandering = true;
    }

    // Pick a random point on the edge of the rectangle
    Vector2 GetRandomEdgePoint()
    {
        // Get half dimensions
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        // 0=Left, 1=Right, 2=Bottom, 3=Top
        int edge = Random.Range(0, 4); 

        // Pick a random point on the edge
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

    // On bump, pause and pick a new destination
    void OnCollisionEnter2D(Collision2D _)
    {
        if (c_State.canWander && isWandering)
            StartCoroutine(StopAndPickNewDestination());
    }


    // Visualize wander area in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        var size = new Vector3(width, height, 0f);
        var center = Application.isPlaying ? (Vector3)startCenter : transform.position;
        Gizmos.DrawWireCube(center, size);
    }
}
