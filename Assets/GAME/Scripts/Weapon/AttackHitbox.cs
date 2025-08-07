using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public WeaponSO weaponSO;

    void Start()        // runs once when the prefab spawns
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger     = true;
        if (weaponSO != null && weaponSO.spriteInHand != null)
        {
            Vector2 size = weaponSO.spriteInHand.bounds.size;
            // scale collider
            col.size = size;                    
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy"))
        {
            return;
        }

        Enemy_Health enemyHealth = other.GetComponent<Enemy_Health>();
        if (enemyHealth == null)
        {
            return;
        }

        float totalDamage = StatsManager.Instance.baseDamage * weaponSO.weaponDamage;
        enemyHealth.TakeHit(totalDamage, transform, weaponSO.knockbackForce, weaponSO.stunTime);
    }
}
