using UnityEngine;
using System.Collections;
using System;

public class Enemy_Health : MonoBehaviour
{
    
    [Header("Boss Settings")]
    [SerializeField] private bool isFinalBoss = false;

    public int expReward = 3;

    public delegate void MonsterDefeated(int exp);
    public static event MonsterDefeated OnMonsterDefeated;
    public static System.Action OnFinalBossDefeated;

    public int currentHealth;
    public int maxHealth;

    [Header("Hit flash")]
    public float flashTime = 0.1f;
    SpriteRenderer sr;


    public void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        else if (currentHealth <= 0)
        {
            OnMonsterDefeated?.Invoke(expReward);
            if (isFinalBoss)
            {
                OnFinalBossDefeated?.Invoke();
            }
            Destroy(gameObject);
        }
    }

    public void TakeHit(float damage, Transform attacker, float knockbackForce, float stunTime)
    {
        ChangeHealth(-Mathf.RoundToInt(damage));

        GetComponent<Enemy_Knockback>()?.Knockback(attacker, knockbackForce, 0.5f, stunTime);


        if (sr != null)
        {
            GetComponentInChildren<Enemy_Healthbar>()?.Show();
            StartCoroutine(Flash());
        }
    }

    IEnumerator Flash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        sr.color = Color.white;
    }
}
