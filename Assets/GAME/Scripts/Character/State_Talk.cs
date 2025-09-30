using UnityEngine;

public class State_Talk : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    Animator characterAnim;   // NPC sprite animator (Idle/Walk graph)
    public Animator interactAnim;    // icon animator with states: Idle (default), WantToTalk
    I_Controller controller;

    // runtime
    Vector2 facingDir; 
    Transform target;

    void Awake()
    {
        rb            ??= GetComponent<Rigidbody2D>();
        characterAnim ??= GetComponentInChildren<Animator>();
        controller      = (I_Controller)(GetComponent<E_Controller>() ??
                            (Component)GetComponent<NPC_Controller>());

    if (!rb) Debug.LogError($"{name}: Rigidbody2D is missing in State_Talk");
    }
    void Update()
    {
        // NPCs don’t move while talking; keep intent at zero if you’re using controllers for NPCs too.
        controller?.SetDesiredVelocity(Vector2.zero);

        if (!target) return;

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;
        if (to.sqrMagnitude > 0.0001f)
        {
            facingDir = to.normalized;
            characterAnim?.SetFloat("idleX", facingDir.x);
            characterAnim?.SetFloat("idleY", facingDir.y);
        }
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
        target = null;
        rb.bodyType = RigidbodyType2D.Dynamic;  // restore normal physics
        interactAnim?.Play("Idle");        // close talk icon
        facingDir = Vector2.zero;           // clear after use
    }

    // API for controller
    public void SetTarget(Transform t)
    {
        target = t;
        if (!target) return;
        var to = (Vector2)target.position - (Vector2)transform.position;
        if (to.sqrMagnitude > 0.0001f) facingDir = to.normalized;
    }
}
