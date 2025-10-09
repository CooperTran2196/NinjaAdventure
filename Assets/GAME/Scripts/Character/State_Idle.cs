using UnityEngine;

public class State_Idle : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    Animator anim;

    I_Controller controller;
    C_Stats stats;

    void Awake()
    {
        rb     = GetComponent<Rigidbody2D>();
        anim   = GetComponent<Animator>();
        stats  = GetComponent<C_Stats>();

        // Make this work for Player, Enemy, or NPC

        controller = (I_Controller)(GetComponent<E_Controller>() ?? (Component)GetComponent<NPC_Controller>());

        if (!rb) Debug.LogError($"{name}: Rigidbody2D is missing in State_Idle");
    }

    void OnEnable()
    {
        rb.linearVelocity = Vector2.zero;
        controller?.SetDesiredVelocity(Vector2.zero);
        anim.SetBool("isMoving", false);
        anim.SetBool("isWandering", false);
        anim.SetBool("isAttacking", false);
    }

    // Idle: no movement intent; controller handles knockback for all states
    void Update()
    {
        // controller?.SetDesiredVelocity(Vector2.zero);
    }
}
