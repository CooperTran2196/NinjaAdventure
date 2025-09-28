using UnityEngine;

public class NPC_State_Talk : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator characterAnimator;   // NPC sprite animator (Idle/Walk graph)
    public Animator interactAnimator;    // icon animator with states: Idle (default), WantToTalk

    // Facing direction set by the controller
    Vector2 facingDir; 

    void Awake()
    {
        rb                ??= GetComponent<Rigidbody2D>();
        characterAnimator ??= GetComponentInChildren<Animator>();

        if (!rb) Debug.LogError($"{name}: Rigidbody2D missing in NPC_State_Talk.");
    }

    void OnEnable()
    {
        // Stop movement and swallow any external forces
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Face a direction
        characterAnimator?.SetFloat("idleX", facingDir.x);
        characterAnimator?.SetFloat("idleY", facingDir.y);

        // Play animations
        characterAnimator?.Play("Idle");       // idle while talking
        interactAnimator?.Play("WantToTalk");  // open talk icon
    }

    void OnDisable()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;  // restore normal physics
        interactAnimator?.Play("Idle");        // close talk icon
        facingDir = Vector2.zero;           // clear after use
    }

    // API for controller
    public void FaceTarget(Transform target)
    {
        facingDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
    }
}
