using UnityEngine;

public static class C_Anim
{
    // Shared for Player + Enemy
    public static void ApplyMoveIdle(
        Animator animator,
        bool isAttacking,
        Vector2 moveAxis,
        Vector2 lastMove,
        float MIN_DISTANCE)
    {
        if (isAttacking)
        {
            animator.SetBool("isMoving", false);
            animator.SetFloat("idleX", lastMove.x);
            animator.SetFloat("idleY", lastMove.y);
            return;
        }

        bool moving = moveAxis.sqrMagnitude > MIN_DISTANCE;
        animator.SetBool("isMoving", moving);

        if (moving)
        {
            animator.SetFloat("moveX", moveAxis.x);
            animator.SetFloat("moveY", moveAxis.y);
        }

        animator.SetFloat("idleX", lastMove.x);
        animator.SetFloat("idleY", lastMove.y);
    }
}
