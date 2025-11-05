using UnityEngine;
using System.Collections;

public class ENV_Trap : MonoBehaviour
{
    [Header("Environmental trap - damages player at regular intervals when active")]
    [Header("References")]
    Collider2D trapCollider;
    Animator   anim;

    [Header("Settings")]
                public float  startDelay        = 0f;     // Delay before trap starts cycling
                public int    damage            = 10;
                public float  collisionTickRate = 0.5f;
                public float  idleTime          = 2f;     // Time before extending
                public float  extendTime        = 1f;     // Extend animation duration (damages during this)
                public float  retractTime       = 1f;     // Retract animation duration (damages during this)
                public string extendTrigger     = "Extend";
                public string retractTrigger    = "Retract";

    // Runtime state
    bool  isActive;
    float tickTimer;
    
    void Awake()
    {
        trapCollider ??= GetComponent<Collider2D>();
        anim         ??= GetComponent<Animator>();

        if (!trapCollider) { Debug.LogError($"{name}: Collider2D is missing!", this); return; }

        trapCollider.isTrigger = true;
    }

    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        if (startDelay > 0)
        {
            yield return new WaitForSeconds(startDelay);
        }
        
        StartCoroutine(TrapCycle());
    }

    IEnumerator TrapCycle()
    {
        while (true)
        {
            // Idle phase - trap inactive
            isActive = false;
            yield return new WaitForSeconds(idleTime);

            // Extend (damages during animation)
            anim?.SetTrigger(extendTrigger);
            isActive = true;
            yield return new WaitForSeconds(extendTime);

            // Retract (damages during animation)
            anim?.SetTrigger(retractTrigger);
            // Keep isActive = true so retract damages too
            yield return new WaitForSeconds(retractTime);
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
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
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            tickTimer = 0f;
        }
    }
}
