using UnityEngine;

public static class C_Anim
{
    // Writes only the directional floats on the Animator
    public static void UpdateAnimDirections(
        Animator animator,
        bool busy,
        Vector2 moveAxis,
        Vector2 lastMove,
        float MIN_DISTANCE)
    {
        // Only write move floats when not locked by another state
        if (!busy && moveAxis.sqrMagnitude > MIN_DISTANCE)
        {
            animator.SetFloat("moveX", moveAxis.x);
            animator.SetFloat("moveY", moveAxis.y);
        }

        // Always refresh idle facing so the character remembers direction
        animator.SetFloat("idleX", lastMove.x);
        animator.SetFloat("idleY", lastMove.y);
    }
}
