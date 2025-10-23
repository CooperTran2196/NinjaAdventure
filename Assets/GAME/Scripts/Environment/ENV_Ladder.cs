using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ENV_Ladder : MonoBehaviour
{
    [Header("References")]
    BoxCollider2D trigger;

    [Header("Ladder Setting")]
                        public Vector2 climbDirection = Vector2.up;
    [Range(0.1f, 1.0f)] public float climbUpMultiplier   = 0.6f;
    [Range(1.0f, 2.0f)] public float climbDownMultiplier = 1.3f;
                        public float ladderGravityScale = 0f;

    // Runtime state
    Vector2 normalizedClimbDir;

    void Awake()
    {
        trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        normalizedClimbDir = climbDirection.normalized;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Support both player and enemies
        P_Controller player = other.GetComponent<P_Controller>();
        if (player)
        {
            player.EnterLadder(this);
            return;
        }
        
        E_Controller enemy = other.GetComponent<E_Controller>();
        if (enemy) enemy.EnterLadder(this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Support both player and enemies
        P_Controller player = other.GetComponent<P_Controller>();
        if (player)
        {
            player.ExitLadder();
            return;
        }
        
        E_Controller enemy = other.GetComponent<E_Controller>();
        if (enemy) enemy.ExitLadder();
    }

    // Returns modified velocity based on movement direction relative to ladder
    public Vector2 ApplyLadderSpeed(Vector2 inputVelocity)
    {
        if (inputVelocity.sqrMagnitude < 0.01f) return Vector2.zero;

        // Dot product: positive = climbing, negative = sliding
        float alignment = Vector2.Dot(inputVelocity.normalized, normalizedClimbDir);

        float speedMult;
        if (alignment > 0.1f)       speedMult = climbUpMultiplier;   // Climbing up
        else if (alignment < -0.1f) speedMult = climbDownMultiplier; // Sliding down
        else                        speedMult = 1f;                   // Perpendicular

        return inputVelocity * speedMult;
    }
}
