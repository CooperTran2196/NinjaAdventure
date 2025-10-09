using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class P_State_Dodge : MonoBehaviour
{
    // Cache
    Animator anim;
    C_Stats stats;
    P_Controller controller;
    C_AfterimageSpawner afterimage;

    // Runtime
    Vector2 dodgeDir;
    float dodgeDuration;

    void Awake()
    {
        anim        ??= GetComponent<Animator>();
        stats       ??= GetComponent<C_Stats>();
        controller  ??= GetComponent<P_Controller>();
        afterimage  ??= GetComponent<C_AfterimageSpawner>();

        if (!anim)       Debug.LogError("State_Dodge: Missing Animator");
        if (!stats)      Debug.LogError("State_Dodge: Missing C_Stats");
        if (!controller) Debug.LogError("State_Dodge: Missing P_Controller");
        if (!afterimage) Debug.LogError("State_Dodge: Missing C_AfterimageSpawner");
    }

    void OnEnable()
    {
        anim.SetBool("isDodging", true);
    }

    void OnDisable()
    {
        StopAllCoroutines(); // If player dies -> stop attack immediately
        anim.SetBool("isDodging", false); // Exit Attack animation by bool
        controller.SetDodging(false); // Normal finish
    }

    public void Dodge(Vector2 dir)
    {
        dodgeDir = dir;

        // Compute duration from distance & speed
        float speed    = stats.dodgeSpeed;
        float distance = stats.dodgeDistance;
        dodgeDuration  = distance / speed; // assume valid per your inspector preconditions

        // One-time velocity set; controller keeps applying desiredVelocity each FixedUpdate
        controller.SetDesiredVelocity(dodgeDir * speed);

        // Lock current sprite for the whole trail burst
        var sr          = GetComponent<SpriteRenderer>();
        var lockedSprite= sr ? sr.sprite : null;
        bool flipX      = sr && sr.flipX;
        bool flipY      = sr && sr.flipY;

        afterimage?.StartBurst(dodgeDuration, lockedSprite, flipX, flipY);

        StartCoroutine(DodgeRoutine());
    }

    IEnumerator DodgeRoutine()
    {
        yield return new WaitForSeconds(dodgeDuration);

        controller.SetDodging(false); // Interrupted by Dead
    }
}
