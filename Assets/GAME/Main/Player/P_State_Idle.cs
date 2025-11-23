using UnityEngine;

[DisallowMultipleComponent]
public class P_State_Idle : MonoBehaviour
{
    // Cache
    Animator anim;
    P_Controller controller;

    // Runtime
    public Vector2 lastFaceDirection = Vector2.down;

    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<P_Controller>();

        if (!anim) Debug.LogError("P_State_Idle: missing Animator");
        if (!controller) Debug.LogError("P_State_Idle: missing P_Controller");
    }

    // Set the idle facing direction
    public void SetIdleFacing(Vector2 direction)
    {
        // Controller guarantees normalized input per your rule.
        lastFaceDirection = direction;
        anim.SetFloat("idleX", lastFaceDirection.x);
        anim.SetFloat("idleY", lastFaceDirection.y);
    }
}