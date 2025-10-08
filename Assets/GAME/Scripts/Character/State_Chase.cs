using UnityEngine;

[DisallowMultipleComponent]
public class State_Chase : MonoBehaviour
{
    [Header("Tuning")]
    [Min(0f)] public float stopBuffer = 0.10f; // used by controller’s axis calc

    // cache
    Animator     anim;
    C_Stats      c_Stats;
    E_Controller controller;

    // runtime
    Vector2 moveAxis;
    Vector2 lastMove = Vector2.down;

    void Awake()
    {
        anim        = GetComponent<Animator>();
        c_Stats     = GetComponent<C_Stats>();
        controller  = GetComponent<E_Controller>();
    }

    void OnEnable()
    {
        // Style-mirror P_State_Movement: state just publishes intent
        // (We won’t force isMoving here; floats + axis drive anims cleanly.)
    }

    void OnDisable()
    {
        controller.SetDesiredVelocity(Vector2.zero);
        anim.SetFloat("moveX", 0f);
        anim.SetFloat("moveY", 0f);
        anim.SetBool("isMoving", false);
    }

    void Update()
    {
        // Calculate and apply movement velocity
        controller.SetDesiredVelocity(moveAxis * c_Stats.MS);

        // Set movement animation
        anim.SetFloat("moveX", moveAxis.x);
        anim.SetFloat("moveY", moveAxis.y);

        if (moveAxis.sqrMagnitude > 0f) lastMove = moveAxis;
        anim.SetFloat("idleX", lastMove.x);
        anim.SetFloat("idleY", lastMove.y);

        // Optional: derive isMoving from axis (keeps old visuals)
        bool moving = moveAxis.sqrMagnitude > 0f;
        anim.SetBool("isMoving", moving);
    }

    // Move with given axis, ontroller already normalized this
    public void SetMoveAxis(Vector2 moveAxis) => this.moveAxis = moveAxis;
}
