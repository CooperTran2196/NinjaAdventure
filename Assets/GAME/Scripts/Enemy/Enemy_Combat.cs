using UnityEngine;
using System.Collections;

public class Enemy_Combat : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 2;
    public float attackRadius = 1f;
    public LayerMask playerLayer;
    public Transform attackPoint;

    private Enemy_Movement movement;     

    void Awake()
    {
        movement = GetComponent<Enemy_Movement>();
        if (attackPoint == null) attackPoint = transform;
    }

    public void Attack()
    {             
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Changehealth(-damage);  // Apply damage
                Debug.Log("Enemy dealt " + damage + " damage to player!");
                
                // Apply knockback only if player is still alive after damage
                if (StatsManager.Instance.currentHealth > 0)
                {
                    hit.GetComponent<PlayerMovement>()?.Knockback(transform, movement.attackRange * 2f, 0.5f);
                }
            }
        }

        StartCoroutine(ResetStateAfterAttack(0.5f));
    }

    IEnumerator ResetStateAfterAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        movement.ChangeState(EnemyState.Chasing);  
    }
}