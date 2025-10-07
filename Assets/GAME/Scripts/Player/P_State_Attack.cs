using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class P_State_Attack : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public C_Stats c_Stats;

    [Header("Weapons")]
    public W_Melee  meleeWeapon;
    public W_Ranged rangedWeapon;
    public W_Base   magicWeapon;

    [Header("Timings")]
    [Min(0f)] public float attackDuration = 0.45f;
    [Min(0f)] public float hitDelay = 0.15f;

    public bool IsAttacking { get; private set; }

    float cooldownTimer;
    Vector2 attackDir = Vector2.down;

    void Awake()
    {
        animator ??= GetComponent<Animator>();
        c_Stats  ??= GetComponent<C_Stats>();

        if (!animator) Debug.LogWarning($"{name}: Animator missing on State_Attack_Player");
        if (!c_Stats) Debug.LogWarning($"{name}: C_Stats missing on State_Attack_Player");
    }

    public void SetDisabled(bool v)
    {
        enabled = !v;
        if (v)
        {
            IsAttacking = false;
            animator?.SetBool("isAttacking", false);
        }
    }

    public void ForceStop()
    {
        StopAllCoroutines();
        IsAttacking = false;
        animator?.SetBool("isAttacking", false);
    }

    public void RequestAttack(Vector2 dir, W_Base weapon)
    {
        if (weapon == null) return;
        if (IsAttacking) return;
        if (cooldownTimer > 0f) return;

        attackDir = dir.sqrMagnitude > 0f ? dir.normalized : Vector2.down;
        StartCoroutine(AttackRoutine(weapon));
    }

    IEnumerator AttackRoutine(W_Base weapon)
    {
        IsAttacking = true;
        animator?.SetBool("isAttacking", true);
        animator?.SetFloat("atkX", attackDir.x);
        animator?.SetFloat("atkY", attackDir.y);

        yield return new WaitForSeconds(hitDelay);
        weapon.Attack(attackDir);

        float rest = Mathf.Max(0f, attackDuration - hitDelay);
        if (rest > 0f) yield return new WaitForSeconds(rest);

        IsAttacking = false;
        animator?.SetBool("isAttacking", false);
        cooldownTimer = c_Stats.attackCooldown;
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }
}
