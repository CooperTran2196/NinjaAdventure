using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class State_Dodge : MonoBehaviour
{
    // Cache
    Animator anim;
    C_Stats c_Stats;
    P_Controller controller;
    C_AfterimageSpawner afterimage;

    // Runtime
    Vector2 dodgeDir;

    void Awake()
    {
        anim = GetComponent<Animator>();
        c_Stats = GetComponent<C_Stats>();
        controller = GetComponent<P_Controller>();
        afterimage = GetComponent<C_AfterimageSpawner>();

        if (!anim) Debug.LogError("State_Dodge: missing Animator");
        if (!c_Stats) Debug.LogError("State_Dodge: missing C_Stats");
        if (!controller) Debug.LogError("State_Dodge: missing P_Controller");
    }

    void OnDisable()
    {
        controller.SetDesiredVelocity(Vector2.zero);
        anim.SetBool("isDodging", false);
    }

    // Dodge with given direction (called by controller)
    public void Dodge(Vector2 dir)
    {
        dodgeDir = dir; // Controller already normalized this
        
        var sr = GetComponent<SpriteRenderer>();
        var lockedSprite = sr ? sr.sprite : null;
        bool flipX = sr ? sr.flipX : false;
        bool flipY = sr ? sr.flipY : false;

        float speed = c_Stats.dodgeSpeed;
        float distance = c_Stats.dodgeDistance;
        float duration = (speed > 0f) ? (distance / speed) : 0f;

        Vector2 dodgeVelocity = dodgeDir * speed;
        controller.SetDesiredVelocity(dodgeVelocity);

        if (duration > 0f)
            afterimage?.StartBurst(duration, lockedSprite, flipX, flipY);

        anim.SetBool("isDodging", true);
        StartCoroutine(DodgeRoutine(duration));
    }

    IEnumerator DodgeRoutine(float duration)
    {
        if (duration > 0f) yield return new WaitForSeconds(duration);

        // OnDisable will handle cleanup when controller switches states
        controller.SetDesiredVelocity(Vector2.zero);
        anim.SetBool("isDodging", false);
    }
}
