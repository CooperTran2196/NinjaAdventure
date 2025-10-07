using UnityEngine;

[DisallowMultipleComponent]
public class P_State_Movement : MonoBehaviour
{
    // Cache
    Animator anim;
    C_Stats c_Stats;
    P_Controller controller;

    // Runtime
    Vector2 moveAxis;

    void Awake()
    {
        anim = GetComponent<Animator>();
        c_Stats = GetComponent<C_Stats>();
        controller = GetComponent<P_Controller>();

        if (!anim) Debug.LogError("P_State_Movement: missing Animator");
        if (!c_Stats) Debug.LogError("P_State_Movement: missing C_Stats");
        if (!controller) Debug.LogError("P_State_Movement: missing P_Controller");
    }

    void OnDisable()
    {
        controller.SetDesiredVelocity(Vector2.zero);
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
    }

    // Move with given axis (called by controller)
    public void Move(Vector2 axis)
    {
        moveAxis = axis; // Controller already normalized this
    }

    void Update()
    {
        // Calculate and apply movement velocity
        Vector2 vel = moveAxis * c_Stats.MS;
        controller.SetDesiredVelocity(vel);

        // Set movement animation
        anim.SetFloat("moveX", moveAxis.x);
        anim.SetFloat("moveY", moveAxis.y);
    }
}
