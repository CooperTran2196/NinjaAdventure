using UnityEngine;

/// <summary>
/// Simple ping-pong movement for decorative clouds.
/// Moves horizontally left/right within a configurable distance.
/// </summary>
public class ENV_MovingCloud : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed (slower than player's 90)")]
    [SerializeField] private float moveSpeed = 15f;

    [Tooltip("Maximum distance to travel before reversing")]
    [SerializeField] private float maxDistance = 5f;

    [Tooltip("Starting direction (1 = right, -1 = left)")]
    [SerializeField] private float startDirection = 1f;

    // Runtime state
    private Vector2 startPosition;
    private float direction;
    private float distanceTraveled;

    void Start()
    {
        startPosition = transform.position;
        direction = Mathf.Sign(startDirection); // Normalize to 1 or -1
        distanceTraveled = 0f;
    }

    void Update()
    {
        // Move horizontally
        float movement = direction * moveSpeed * Time.deltaTime;
        transform.Translate(movement, 0f, 0f);

        // Track distance
        distanceTraveled += Mathf.Abs(movement);

        // Reverse at boundary
        if (distanceTraveled >= maxDistance)
        {
            direction *= -1f; // Flip direction
            distanceTraveled = 0f; // Reset counter
        }
    }

    // Gizmos to visualize boundaries in editor
    void OnDrawGizmosSelected()
    {
        Vector2 origin = Application.isPlaying ? startPosition : (Vector2)transform.position;

        // Draw movement range
        Gizmos.color = Color.cyan;
        Vector2 leftBound = origin - Vector2.right * maxDistance;
        Vector2 rightBound = origin + Vector2.right * maxDistance;

        // Draw boundary lines
        Gizmos.DrawLine(leftBound + Vector2.up * 0.5f, leftBound - Vector2.up * 0.5f);
        Gizmos.DrawLine(rightBound + Vector2.up * 0.5f, rightBound - Vector2.up * 0.5f);

        // Draw movement path
        Gizmos.DrawLine(leftBound, rightBound);

        // Draw current position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}
