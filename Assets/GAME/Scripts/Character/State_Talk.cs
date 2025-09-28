using UnityEngine;

public class State_Talk : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator characterAnim;   // NPC sprite animator (Idle/Walk graph)
    public Animator interactAnim;    // icon animator with states: Idle (default), WantToTalk

    // Facing direction set by the controller
    Vector2 facingDir; 

    void Awake()
    {
        rb                ??= GetComponent<Rigidbody2D>();
        characterAnim      ??= GetComponentInChildren<Animator>();

        if (!rb) Debug.LogError($"{name}: Rigidbody2D missing in NPC_State_Talk.");
    }

    void OnEnable()
    {
        // Stop movement and swallow any external forces
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Face a direction
        characterAnim?.SetFloat("idleX", facingDir.x);
        characterAnim?.SetFloat("idleY", facingDir.y);

        // Play animations
        characterAnim?.Play("Idle");       // idle while talking
        interactAnim?.Play("WantToTalk");  // open talk icon
    }

    void OnDisable()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;  // restore normal physics
        interactAnim?.Play("Idle");        // close talk icon
        facingDir = Vector2.zero;           // clear after use
    }

    // API for controller
    public void FaceTarget(Transform target)
    {
        facingDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
    }
}
