using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(C_Stats))]
[DisallowMultipleComponent]
public class State_Wander : MonoBehaviour
{
    [Header("References")]
    Rigidbody2D rb;
    Animator anim;
    C_Stats c_Stats;
    E_Controller controller;

    [Header("Wander Area")]
    public Vector2 startCenter;
    public float width = 6f;
    public float height = 4f;

    [Header("Movement")]
    public float pauseDuration = 1f;

    [Header("Animation")]
    public string idleState = "Idle";
    public string walkState = "Walk";

    // runtime
    Vector2 destination;
    Vector2 dir;
    Vector2 lastMove;
    bool isWandering;
    const float MIN_DISTANCE = 0.1f;

    void Awake()
    {
        rb         = GetComponent<Rigidbody2D>();
        anim       = GetComponentInChildren<Animator>();
        c_Stats    = GetComponent<C_Stats>();
        controller = GetComponent<E_Controller>();

        if (!rb) Debug.LogError($"{name}: Rigidbody2D missing in State_Wander.");
        if (!c_Stats) Debug.LogError($"{name}: C_Stats missing in State_Wander.");
        if (!anim) Debug.LogError($"{name}: Animator (in children) missing in State_Wander.");

        if (startCenter == Vector2.zero) startCenter = (Vector2)transform.position;
    }

    void OnEnable()
    {
        isWandering = true;
        anim.SetBool("isWandering", true);
        rb.bodyType = RigidbodyType2D.Dynamic;
        StopAllCoroutines();
        StartCoroutine(PauseAndPickNewDestination());
    }

    void OnDisable()
    {
        anim.SetBool("isWandering", false);
        StopAllCoroutines();
        isWandering = false;
        controller?.SetDesiredVelocity(Vector2.zero);
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (!isWandering)
        {
            controller?.SetDesiredVelocity(Vector2.zero);
            return;
        }

        if (Vector2.Distance(transform.position, destination) < MIN_DISTANCE)
        {
            StopAllCoroutines();
            StartCoroutine(PauseAndPickNewDestination());
            controller?.SetDesiredVelocity(Vector2.zero);
            return;
        }

        dir = (destination - (Vector2)transform.position).normalized;
        if (dir.sqrMagnitude > 0f) lastMove = dir;

        // Animator floats
        anim.SetFloat("moveX", dir.x);
        anim.SetFloat("moveY", dir.y);
        anim.SetFloat("idleX", lastMove.x);
        anim.SetFloat("idleY", lastMove.y);

        // Send intent to controller
        controller?.SetDesiredVelocity(dir * c_Stats.MS);
    }

    IEnumerator PauseAndPickNewDestination()
    {
        isWandering = false;
        controller?.SetDesiredVelocity(Vector2.zero);
        rb.linearVelocity = Vector2.zero;
        anim?.Play(idleState);

        yield return new WaitForSeconds(pauseDuration);

        destination = GetRandomEdgePoint();
        isWandering = true;
        anim?.Play(walkState);
    }

    Vector2 GetRandomEdgePoint()
    {
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: return new Vector2(startCenter.x - halfW, Random.Range(startCenter.y - halfH, startCenter.y + halfH));
            case 1: return new Vector2(startCenter.x + halfW, Random.Range(startCenter.y - halfH, startCenter.y + halfH));
            case 2: return new Vector2(Random.Range(startCenter.x - halfW, startCenter.x + halfW), startCenter.y - halfH);
            case 3: return new Vector2(Random.Range(startCenter.x - halfW, startCenter.x + halfW), startCenter.y + halfH);
        }
        return startCenter;
    }

    void OnCollisionEnter2D(Collision2D _) { if (isWandering) StartCoroutine(PauseAndPickNewDestination()); }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        var size = new Vector3(width, height, 0f);
        var center = Application.isPlaying ? (Vector3)startCenter : transform.position;
        Gizmos.DrawWireCube(center, size);
    }
}
