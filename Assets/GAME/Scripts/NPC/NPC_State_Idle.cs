using UnityEngine;

public class NPC_State_Idle : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator characterAnimator;

    void Awake()
    {
        rb                ??= GetComponent<Rigidbody2D>();
        characterAnimator ??= GetComponentInChildren<Animator>();
        if (!rb) Debug.LogError($"{name}: Rigidbody2D missing in NPC_State_Idle.");
    }

    void OnEnable()
    {
        rb.linearVelocity = Vector2.zero;
        characterAnimator?.Play("Idle");
    }
}
