using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public static System.Action OnPlayerDeath;
    public TMP_Text healthText;

    [Header("Hit Flash")] 
    public float flashTime = 0.1f;
    private SpriteRenderer sr;

    private void Start()
    {
        healthText.text = StatsManager.Instance.currentHealth + " / " + StatsManager.Instance.maxHealth;
        
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) 
        {
            Debug.LogWarning("PlayerHealth: No SpriteRenderer found on " + gameObject.name + ". Flash effect won't work.");
        }
    }

    public void Changehealth(int amount)
    {
        StatsManager.Instance.currentHealth += amount;
        healthText.text = StatsManager.Instance.currentHealth + " / " + StatsManager.Instance.maxHealth;

        Debug.Log($"Player health changed by {amount}. Current: {StatsManager.Instance.currentHealth}/{StatsManager.Instance.maxHealth}");

        // Only flash if taking damage amount < 0
        if (amount < 0 && sr != null)
        {
            StartCoroutine(Flash());
        }

        if (StatsManager.Instance.currentHealth <= 0)
        {
            Debug.Log("Player health <= 0. Invoking OnPlayerDeath event.");
            OnPlayerDeath?.Invoke();

            // Disable specific components instead of deactivating the entire GameObject
            if (sr != null) sr.enabled = false;
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = false;
            // Add similar disables for other player components if needed (e.g., Animator, custom combat scripts)
        }
    }

    private IEnumerator Flash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        sr.color = Color.white;
    }
}