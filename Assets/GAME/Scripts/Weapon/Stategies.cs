using UnityEngine;
using System.Collections;

/// <summary>Holds every run-time reference a strategy needs.</summary>
public readonly struct WeaponContext
{
    public readonly Transform owner;            // player or enemy root
    public readonly WeaponSO data;        // stats, icon, prefab
    public readonly WeaponSpriteHelper vis;     // sprite helper
    public readonly Animator bodyAnim;          // drive attack clip
    public readonly LayerMask enemyMask;        // who to hit

    public WeaponContext(
        Transform owner,
        WeaponSO data,
        WeaponSpriteHelper vis,
        Animator bodyAnim,
        LayerMask enemyMask)
    {
        this.owner     = owner;
        this.data      = data;
        this.vis       = vis;
        this.bodyAnim  = bodyAnim;
        this.enemyMask = enemyMask;
    }
}


public interface IWeaponStrategy
{

    void Use(WeaponContext ctx, Vector2 dir);
}


public static class StrategyFactory
{
    static readonly IWeaponStrategy melee = new MeleeStrategy();
    static readonly IWeaponStrategy projectile = new ProjectileStrategy();
    static readonly IWeaponStrategy barehand = new BarehandStrategy();

    public static IWeaponStrategy Get(AttackStyle s) =>
        s == AttackStyle.Melee ? melee :
        s == AttackStyle.Projectile ? projectile : barehand;
}


/// Static helper called by every IWeaponStrategy to drive the body attack animation
/// and reset the “isAttacking” flag when the clip finishes.

public static class AttackAnimHelper
{
    public static void Play(WeaponContext ctx, Vector2 dir, float clipTime)
    {
        Vector2 snapped = Snap(dir);
        Animator anim   = ctx.bodyAnim;

        anim.SetFloat("attackX",  snapped.x);
        anim.SetFloat("attackY",  snapped.y);
        anim.SetBool ("isAttacking", true);
        anim.SetTrigger("Attack");

        // Use an existing MonoBehaviour (WeaponManager) to run the coroutine.
        ctx.owner.GetComponent<WeaponManager>()
                 .StartCoroutine(ResetAttackFlag(anim, clipTime));
    }

    
    static Vector2 Snap(Vector2 d)
    {
        if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
            return Mathf.Abs(d.x) > 0.1f ? new Vector2(Mathf.Sign(d.x), 0) : Vector2.down;
        else
            return Mathf.Abs(d.y) > 0.1f ? new Vector2(0, Mathf.Sign(d.y)) : Vector2.down;
    }

    static IEnumerator ResetAttackFlag(Animator anim, float t)
    {
        yield return new WaitForSeconds(t);
        anim.SetBool("isAttacking", false);
    }
}


public class MeleeStrategy : IWeaponStrategy
{
    float cooldownTimer;
    public void Use(WeaponContext ctx, Vector2 dir)
    {
        if (Time.time < cooldownTimer) return;
        cooldownTimer = Time.time + ctx.data.cooldown;

        AttackAnimHelper.Play(ctx, dir, 0.25f);
        // spawn hitbox 
        WeaponManager wm = ctx.owner.GetComponent<WeaponManager>();
        if (wm == null || wm.meleeHitboxPrefab == null) return;

        Transform pivot = wm.attackPoint != null ? wm.attackPoint : ctx.owner;
        float reach = ctx.data.offsetDistance > 0 ? ctx.data.offsetDistance : 0.6f;

        Vector3 pos = pivot.position + (Vector3)(dir.normalized * reach);
        GameObject hb  = Object.Instantiate(wm.meleeHitboxPrefab,
                                            pos, 
                                            Quaternion.identity);

        hb.GetComponent<AttackHitbox>().weaponSO = ctx.data;

        float life = 0.05f;
        Object.Destroy(hb, life);
    }
}


public class ProjectileStrategy : IWeaponStrategy
{
    float cooldownTimer;

    public void Use(WeaponContext ctx, Vector2 dir)
    {
        if (Time.time < cooldownTimer) return;
        cooldownTimer = Time.time + ctx.data.cooldown;

        AttackAnimHelper.Play(ctx, dir, 0.25f);

        //spawn projectile 
        if (ctx.data.projectilePrefab == null) return;

        GameObject go = Object.Instantiate(ctx.data.projectilePrefab,
                                           ctx.owner.position,
                                           Quaternion.identity);

        Arrow arrow          = go.GetComponent<Arrow>();
        arrow.direction       = dir.normalized;
        arrow.damage          = Mathf.RoundToInt(StatsManager.Instance.baseDamage
                                                 * ctx.data.weaponDamage);
        arrow.knockbackForce  = ctx.data.knockbackForce;
        arrow.knockbackTime   = 0.05f;
        arrow.stunTime        = ctx.data.stunTime;
    }
}



public class BarehandStrategy : IWeaponStrategy
{
    float cooldownTimer;
    public void Use(WeaponContext ctx, Vector2 dir)
    {
        if (Time.time < cooldownTimer) return;
        cooldownTimer = Time.time + ctx.data.cooldown;          // reuse stat

        AttackAnimHelper.Play(ctx, dir, 0.25f);                 // new utility
        Vector2 where = ctx.owner.position + (Vector3)(dir * .3f);
        foreach (var hit in Physics2D.OverlapCircleAll(where, 0.4f, ctx.enemyMask))
        {
            Enemy_Health enemyHealth = hit.GetComponent<Enemy_Health>();
            if (enemyHealth != null)
            {
                float damage = Mathf.RoundToInt(StatsManager.Instance.baseDamage * ctx.data.weaponDamage);
                enemyHealth.TakeHit(damage, ctx.owner, ctx.data.knockbackForce, ctx.data.stunTime);
            }
        }
    }
}

