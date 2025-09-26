using UnityEngine;
using System.Collections;

public class NPC_Movement : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public C_Stats c_Stats;
    public C_State c_State;

    [Header("Wander Area")]
    public float width = 6f;
    public float height = 4f;
    private Vector2 startCenter;

    [Header("Movement")]
    public float pauseDuration = 1f;  // seconds to idle at edges / on bump

    Vector2 target;
    Vector2 dir;
    bool isPaused;

    const float REACH_EPS = 0.10f;

    void Awake()
    {
        rb       ??= GetComponent<Rigidbody2D>();
        animator ??= GetComponentInChildren<Animator>();
        c_Stats  ??= GetComponent<C_Stats>();
        c_State  ??= GetComponent<C_State>();

        if (!rb)       Debug.LogError($"{name}: Rigidbody2D missing in NPC_Wander.");
        if (!animator) Debug.LogError($"{name}: Animator (in children) missing in NPC_Wander.");
        if (!c_State)  Debug.LogError($"{name}: C_State missing in NPC_Wander.");
        if (!c_Stats)  Debug.LogError($"{name}: C_Stats missing in NPC_Wander.");

        startCenter = transform.position;
    }

    void OnEnable()
    {
        StartCoroutine(PauseAndPickNewLocation());
    }

    void Update()
    {
        if (isPaused) return;

        if (Vector2.Distance(transform.position, target) < REACH_EPS)
        {
            StartCoroutine(PauseAndPickNewLocation());
            return;
        }

        dir = (target - (Vector2)transform.position).normalized;

        // Facing via C_State
        // c_State?.SetFacing(dir);
    }

    void FixedUpdate()
    {
        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = dir * c_Stats.MS; // speed from stats directly
    }

    IEnumerator PauseAndPickNewLocation()
    {
        isPaused = true;
        rb.linearVelocity = Vector2.zero;
        animator?.Play("NPC_Idle");

        yield return new WaitForSeconds(pauseDuration);

        target = GetRamdomLocation();

        isPaused = false;
        animator?.Play("NPC_Wander");
    }

    Vector2 GetRamdomLocation()
    {
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        int edge = Random.Range(0, 4); // 0=Left, 1=Right, 2=Bottom, 3=Top

        switch (edge)
        {
            case 0: return new Vector2(startCenter.x - halfW, Random.Range(startCenter.y - halfH, startCenter.y + halfH)); // Left
            case 1: return new Vector2(startCenter.x + halfW, Random.Range(startCenter.y - halfH, startCenter.y + halfH)); // Right
            case 2: return new Vector2(Random.Range(startCenter.x - halfW, startCenter.x + halfW), startCenter.y - halfH); // Bottom
            case 3: return new Vector2(Random.Range(startCenter.x - halfW, startCenter.x + halfW), startCenter.y + halfH); // Top
        }
        return startCenter;
    }

    void OnCollisionEnter2D(Collision2D _)
    {
        if (!isPaused) StartCoroutine(PauseAndPickNewLocation());
    }

    public void SetDisabled(bool disabled)
    {
        isPaused = disabled;
        if (disabled)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        var size = new Vector3(width, height, 0f);
        var center = Application.isPlaying ? startCenter : (Vector2)transform.position;
        Gizmos.DrawWireCube(center, size);
    }
}
