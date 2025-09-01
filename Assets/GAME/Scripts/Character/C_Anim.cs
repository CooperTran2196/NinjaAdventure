using UnityEngine;

public static class C_Anim
{
    // Shared for Player + Enemy
    public static void ApplyMoveIdle(Animator a, bool isAttacking, Vector2 moveAxis, Vector2 lastMove, float MIN_DISTANCE)
    {
        if (isAttacking)
        {
            a.SetBool("isMoving", false);
            a.SetFloat("idleX", lastMove.x);
            a.SetFloat("idleY", lastMove.y);
            return;
        }

        bool moving = moveAxis.sqrMagnitude > MIN_DISTANCE;
        a.SetBool("isMoving", moving);

        if (moving)
        {
            a.SetFloat("moveX", moveAxis.x);
            a.SetFloat("moveY", moveAxis.y);
        }

        a.SetFloat("idleX", lastMove.x);
        a.SetFloat("idleY", lastMove.y);
    }
}
