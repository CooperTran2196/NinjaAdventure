using UnityEngine;

public class State_Chase : MonoBehaviour
{
    [Header("Tuning")]
    [Min(0f)] public float stopBuffer = 0.10f; // used by controller’s axis calc

    // cache
    Animator     anim;
    C_Stats      c_Stats;
    E_Controller controller;
    State_Attack attackState;

    // runtime
    Vector2 lastMove = Vector2.down;

    void Awake()
    {
        anim        = GetComponent<Animator>();
        c_Stats     = GetComponent<C_Stats>();
        controller  = GetComponent<E_Controller>();
        attackState = GetComponent<State_Attack>();
    }

    void OnEnable()
    {
        // Only set isMoving if not attacking (attack animation takes priority)
        if (controller.currentState != E_Controller.EState.Attack)
        {
            anim.SetBool("isMoving", true);
        }
    }

    void OnDisable()
    {
        controller.SetDesiredVelocity(Vector2.zero);
        anim.SetBool("isMoving", false);
    }

    void Update()
    {
        // Calculate chase direction
        Vector2 moveAxis = ComputeChaseDir();

        // Calculate base speed
        float speed = c_Stats.MS;

        // Apply weapon movement penalty if attacking
        if (controller.currentState == E_Controller.EState.Attack)
        {
            // Get active weapon from attack state
            W_Base activeWeapon = attackState.GetActiveWeapon();
            if (activeWeapon != null && activeWeapon.weaponData != null)
            {
                // Use combo-specific penalty for melee, fallback for ranged
                if (activeWeapon is W_Melee meleeWeapon)
                {
                    // Enemy uses random combo attack - get penalty from State_Attack
                    int comboIndex = attackState.GetComboIndex();
                    speed *= activeWeapon.weaponData.comboMovePenalties[comboIndex];
                }
                else
                {
                    // Ranged weapons use simple attackMovePenalty
                    speed *= activeWeapon.weaponData.attackMovePenalty;
                }
            }

            // During attack, don't override attack animation
            anim.SetBool("isMoving", false);
        }
        else
        {
            // Not attacking - show movement animation if moving
            bool moving = moveAxis.sqrMagnitude > 0f;
            anim.SetBool("isMoving", moving);
        }

        // Apply movement velocity
        Vector2 velocity = moveAxis * speed;
        
        // Apply ladder modifiers if on ladder
        velocity = controller.ApplyLadderModifiers(velocity);
        
        controller.SetDesiredVelocity(velocity);

        // Set movement animation parameters
        anim.SetFloat("moveX", moveAxis.x);
        anim.SetFloat("moveY", moveAxis.y);

        // Continue to face the last direction it was moving in
        if (moveAxis.sqrMagnitude > 0f) lastMove = moveAxis;
        anim.SetFloat("idleX", lastMove.x);
        anim.SetFloat("idleY", lastMove.y);
    }

    Vector2 ComputeChaseDir()
    {
        Transform target = controller.GetTarget();
        if (!target) return Vector2.zero;

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        float dist = to.magnitude;
        if (dist <= 0.000001f) return Vector2.zero;

        float stop = controller.GetAttackRange() + stopBuffer;
        return (dist > stop) ? (to / dist) : Vector2.zero; // normalized or zero if within stop band
    }
}
