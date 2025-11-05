using UnityEngine;

public class ENV_Drowning : MonoBehaviour
{
    [Header("Drowning zone - damages player continuously while in water")]
    [Header("References")]
    Collider2D waterCollider;

    [Header("Settings")]
                public int    damage            = 5;
                public float  collisionTickRate = 0.5f;
                public string drowningTrigger   = "isDrowning";

    // Runtime state
    Animator playerAnim;
    float    tickTimer;
    bool     playerInWater;
    
    void Awake()
    {
        waterCollider ??= GetComponent<Collider2D>();

        if (!waterCollider) { Debug.LogError($"{name}: Collider2D is missing!", this); return; }

        waterCollider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInWater = true;
            playerAnim = other.GetComponent<Animator>();
            playerAnim?.SetBool(drowningTrigger, true);
            tickTimer = 0f;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!playerInWater || !other.CompareTag("Player")) return;

        tickTimer += Time.deltaTime;

        if (tickTimer >= collisionTickRate)
        {
            C_Health playerHealth = other.GetComponent<C_Health>();
            if (playerHealth != null)
            {
                playerHealth.ApplyDamage(damage, 0, 0, 0, 0, 0);  // Pure physical damage
            }

            tickTimer = 0f;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInWater = false;
            playerAnim?.SetBool(drowningTrigger, false);
            playerAnim = null;
            tickTimer = 0f;
        }
    }
}
