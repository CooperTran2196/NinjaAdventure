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
        anim        = GetComponent<Animator>();
        c_Stats     = GetComponent<C_Stats>();
        controller  = GetComponent<P_Controller>();

        if (!anim)          Debug.LogError("P_State_Movement: missing Animator");
        if (!c_Stats)       Debug.LogError("P_State_Movement: missing C_Stats");
        if (!controller)    Debug.LogError("P_State_Movement: missing P_Controller");
    }

    void OnEnable()
    {
        anim.SetBool("isMoving", true);
    }

    void OnDisable()
    {
        controller.SetDesiredVelocity(Vector2.zero);
        anim.SetBool("isMoving", false);
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
    }

    void Update()
    {
        // Calculate and apply movement velocity
        controller.SetDesiredVelocity(moveAxis * c_Stats.MS);

        // Set movement animation
        anim.SetFloat("moveX", moveAxis.x);
        anim.SetFloat("moveY", moveAxis.y);
    }

    // Move with given axis
    public void SetMoveAxis(Vector2 moveAxis)
    {
        this.moveAxis = moveAxis; // Controller already normalized this
    }
}
