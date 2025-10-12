using UnityEngine;

[DisallowMultipleComponent]
public class P_State_Movement : MonoBehaviour
{
    // Cache
    Animator     anim;
    C_Stats      c_Stats;
    P_Controller controller;
    P_State_Attack attackState;

    // Runtime
    Vector2      moveAxis;

    void Awake()
    {
        anim        = GetComponent<Animator>();
        c_Stats     = GetComponent<C_Stats>();
        controller  = GetComponent<P_Controller>();
        attackState = GetComponent<P_State_Attack>();
    }

    void OnEnable()
    {
        // Only set isMoving if not attacking (attack animation takes priority)
        if (controller.currentState != P_Controller.PState.Attack)
        {
            anim.SetBool("isMoving", true);
        }
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
        // Calculate base movement velocity
        float speed = c_Stats.MS;
        
        // Apply attack movement penalty if attacking
        if (controller.currentState == P_Controller.PState.Attack)
        {
            // Get the active weapon and its movement penalty
            W_Base activeWeapon = attackState.GetActiveWeapon();
            if (activeWeapon != null && activeWeapon.weaponData != null)
            {
                speed *= activeWeapon.weaponData.attackMovePenalty;
            }
            
            // Turn off isMoving animation while attacking (attack animation has priority)
            anim.SetBool("isMoving", false);
        }
        else
        {
            // Normal movement - show movement animation
            anim.SetBool("isMoving", true);
        }
        
        controller.SetDesiredVelocity(moveAxis * speed);

        // Set movement animation parameters (always update for directional info)
        anim.SetFloat("moveX", moveAxis.x);
        anim.SetFloat("moveY", moveAxis.y);
    }

    // Move with given axis, ontroller already normalized this
    public void SetMoveAxis(Vector2 moveAxis) => this.moveAxis = moveAxis;
}
