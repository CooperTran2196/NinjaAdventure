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

        // Always refresh idle facing sso the character remembers direction
        animator.SetFloat("idleX", lastMove.x);
        animator.SetFloat("idleY", lastMove.y);
    }

    // Single place to write attack-facing
    public static void SetAttackDirection(Animator animator, Vector2 dir)
    {
        animator.SetFloat("atkX", dir.x);
        animator.SetFloat("atkY", dir.y);
    }

    // Read back the attack facing used by Movement while attacking
    public static Vector2 GetAttackDirection(Animator animator)
    {
        return new Vector2(animator.GetFloat("atkX"), animator.GetFloat("atkY"));
    }

    // Read idle facing
    public static Vector2 GetIdleDirection(Animator animator)
    {
        return new Vector2(animator.GetFloat("idleX"), animator.GetFloat("idleY"));
    }
}
