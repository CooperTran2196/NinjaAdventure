using UnityEngine;

[DisallowMultipleComponent]
public class P_State_Idle : MonoBehaviour
{
    // Cache
    Animator anim;
    P_Controller controller;

    // Runtime - track last movement for idle facing
    Vector2 lastFaceDirection = Vector2.down;

    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<P_Controller>();

        if (!anim) Debug.LogError("P_State_Idle: missing Animator");
        if (!controller) Debug.LogError("P_State_Idle: missing P_Controller");
    }

    void OnEnable()
    {
        // Ensure no movement when idle
        controller.SetDesiredVelocity(Vector2.zero);
        
        // Set idle animation parameters
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
        anim.SetBool("isAttacking", false);
        anim.SetBool("isDodging", false);
        
        // Use the last movement direction for idle facing
        // This should come from the controller's lastMove
        SetIdleFacing(lastFaceDirection);
    }

    void OnDisable()
    {
        controller.SetDesiredVelocity(Vector2.zero);
    }

    // Set the idle facing direction
    public void SetIdleFacing(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            lastFaceDirection = direction.normalized;
            anim.SetFloat("idleX", lastFaceDirection.x);
            anim.SetFloat("idleY", lastFaceDirection.y);
        }
    }

    void Update()
    {
        // Ensure we stay at zero velocity while idle
        controller.SetDesiredVelocity(Vector2.zero);
    }
}