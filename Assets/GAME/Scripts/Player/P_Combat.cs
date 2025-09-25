using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class P_Combat : MonoBehaviour
{
    [Header("References")]
    SpriteRenderer sprite;
    Animator animator;
    P_InputActions input;

    C_Stats     c_Stats;
    C_State     c_State;
    P_Movement  p_Movement;
    C_Health    c_Health;
    
    [Header("Weapons (Player can hold 3)")]
    public W_Melee  meleeWeapon;
    public W_Ranged rangedWeapon;
    public W_Base   magicWeapon;
    
    [Header("Attack")]
    [Header("ALWAYS matched full clip length (0.45)")]
    public float attackDuration = 0.45f;
    [Header("ALWAYS set when the hit happens (0.15)")]
    public float hitDelay = 0.15f;

    [Header("Debug")]
    [SerializeField] bool autoKill;

    Vector2 attackDir = Vector2.down; // default when starting the game
    const float MIN_DISTANCE = 0.0001f;
    float cooldownTimer;
    public bool isAttacking { get; private set; }

    // Quick state check
    public bool IsAlive => c_Stats.currentHP > 0;

    void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();
        animator ??= GetComponent<Animator>();
        input ??= new P_InputActions();
        
        c_Stats ??= GetComponent<C_Stats>();
        c_State ??= GetComponent<C_State>();
        p_Movement ??= GetComponent<P_Movement>();
        c_Health ??= GetComponent<C_Health>();

        meleeWeapon  ??= GetComponentInChildren<W_Melee>();
        rangedWeapon ??= GetComponentInChildren<W_Ranged>();
        magicWeapon  ??= null; // placeholder only

        
        if (!sprite) Debug.LogWarning($"{name}: SpriteRenderer in P_Combat missing.");
        if (!animator) Debug.LogError($"{name}: Animator in P_Combat missing.");

        if (!c_Stats) Debug.LogError($"{name}: P_Stats in P_Combat missing.");
        
        if (!p_Movement) Debug.LogError($"{name}: P_Movement in P_Combat missing.");
        if (!c_Health) Debug.LogError($"{name}: C_Health in P_Combat missing.");
    }

    void OnEnable()
    {
        input?.Enable();
    }

    void OnDisable()
    {
        input?.Disable();
    }

    void Update()
    {
        // Read aim from MOUSE
        Vector2 mouseAim = ReadMouseAim();
        if (mouseAim.sqrMagnitude > MIN_DISTANCE) attackDir = mouseAim;

        // Inputs: Left Mouse  -> Melee, Right Mouse -> Ranged
        if (input.Player.MeleeAttack.triggered)  RequestAttack(meleeWeapon);
        if (input.Player.RangedAttack.triggered) RequestAttack(rangedWeapon);

        if (autoKill) { autoKill = false; c_Health.ChangeHealth(-c_Stats.maxHP); }

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    public void RequestAttack(W_Base weapon)
    {
        if (!IsAlive || cooldownTimer > 0f) return;

        cooldownTimer = c_Stats.attackCooldown;

        // Face once at attack start
        c_State.SetAttackDirection(attackDir);

        
        StartCoroutine(AttackRoutine(weapon));
    }

    IEnumerator AttackRoutine(W_Base weapon)
    {
        // STATE: Attack START
        isAttacking = true;

        // Delay -> Attack -> Recover
        yield return new WaitForSeconds(hitDelay);
        weapon.Attack(attackDir);
        yield return new WaitForSeconds(attackDuration - hitDelay);

        // STATE: Attack END
        isAttacking = false;
    }

    Vector2 ReadMouseAim()
    {
        // return previous aim if no camera/mouse
        if (!Camera.main || Mouse.current == null)
        {
            Debug.LogError("P_Combat: No main camera or mouse found for aiming.");
            return attackDir;
        }

        // screen-space mouse position
            Vector2 m = Mouse.current.position.ReadValue();

        // z distance from camera to actor for ScreenToWorldPoint
        float z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);

        // mouse position in world space at actor depth
        Vector3 mw = Camera.main.ScreenToWorldPoint(new Vector3(m.x, m.y, z));

        // vector from actor to mouse
        Vector2 d = (Vector2)(mw - transform.position);

        // return normalized direction if significant, else preserve aim
        return (d.sqrMagnitude > MIN_DISTANCE) ? d.normalized : attackDir;
    }
}
