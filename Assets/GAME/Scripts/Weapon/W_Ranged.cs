using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class W_Ranged : W_Base
{
    [Header("Sprite (visual only)")]
    public SpriteRenderer weaponSprite;
    public float showTime = 0.12f;
    public bool previewPose = false;

    [Header("Projectile")]
    public W_Projectile projectilePrefab;
    public float projectileSpeed = 8f;
    public float projectileLife  = 2f;

    protected override void Awake()
    {
        base.Awake();
        weaponSprite ??= GetComponentInChildren<SpriteRenderer>();
        if (weaponSprite) weaponSprite.enabled = false;
    }

    void Update()
    {
        if (previewPose && weaponSprite)
        {
            weaponSprite.enabled = true;
            weaponSprite.transform.position = WorldPosAtOffset();
            weaponSprite.transform.rotation = Quaternion.Euler(0, 0, FacingAngleDeg());
        }
    }

    public override void Attack()
    {
        // Show the bow/wand sprite briefly (no thrust, no damage by sprite)
        if (weaponSprite)
        {
            weaponSprite.enabled = true;
            weaponSprite.transform.position = WorldPosAtOffset();
            weaponSprite.transform.rotation = Quaternion.Euler(0, 0, FacingAngleDeg());
            StartCoroutine(HideAfter(showTime));
        }

        // Fire the projectile (this applies damage)
        if (projectilePrefab)
        {
            var proj = Instantiate(
                projectilePrefab,
                WorldPosAtOffset(),
                Quaternion.Euler(0, 0, FacingAngleDeg())
            );
            proj.Fire(FacingDir, FinalDamage, targetMask, projectileSpeed, projectileLife, owner ? owner.gameObject : null);
        }
    }

    IEnumerator HideAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (weaponSprite) weaponSprite.enabled = false;
    }
}
