using UnityEngine;

public class GS2_State_Chase : MonoBehaviour
{
    GS2_Controller controller;
    C_Stats        c_Stats;

    [Header("Retreat Settings")]
    public float retreatSpeedMultiplier = 0.7f;

    Transform target;
    bool      hasTarget;
    Vector2   moveVector;

    void Awake()
    {
        controller = GetComponent<GS2_Controller>();
        c_Stats    = GetComponent<C_Stats>();
    }

    void Update()
    {
        if (!hasTarget) return;

        // PHASE 2: Check retreat behavior
        if (controller.IsRetreating())
        {
            // Retreat: move away from player
            moveVector = ((Vector2)transform.position - (Vector2)target.position).normalized;
            controller.SetDesiredVelocity(moveVector * c_Stats.MS * retreatSpeedMultiplier);
        }
        else if (controller.IsInRetreatCooldown())
        {
            // Cooldown window: stop moving (vulnerable)
            controller.SetDesiredVelocity(Vector2.zero);
        }
        else
        {
            // Normal chase: move toward player
            moveVector = ((Vector2)target.position - (Vector2)transform.position).normalized;
            controller.SetDesiredVelocity(moveVector * c_Stats.MS);
        }
    }

    public void SetTarget(Transform t)
    {
        target = t;
        hasTarget = (t != null);
    }
}
