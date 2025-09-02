using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public P_Combat combat; // Reads currentHP/maxHP + raises events
    public RectTransform root;
    public Image healthPointPrefab;

    [Header("Sprites (index = HP in this slot)")]
    public Sprite[] healthPoint = new Sprite[4]; // 0 -> full

    readonly List<Image> health = new List<Image>(); // Instantiated icons
    int lastMaxHP = -1;

    // Events
    System.Action<int> onDamaged, onHealed;
    System.Action onDied;

    void OnEnable()
    {
        // Assign delegates
        onDamaged = _ => { chooseHealthSprite(); };
        onHealed  = _ => { chooseHealthSprite(); };
        onDied    = () => { chooseHealthSprite(); };

        combat.OnDamaged += onDamaged;
        combat.OnHealed  += onHealed;
        combat.OnDied    += onDied;

        UpdateUI();
        chooseHealthSprite();
    }

    void OnDisable()
    {
        combat.OnDamaged -= onDamaged;
        combat.OnHealed  -= onHealed;
        combat.OnDied    -= onDied;
    }

    // Build the correct number of icons from maxHP
    public void UpdateUI()
    {
        int maxHP = combat.stats.maxHP;
        if (maxHP == lastMaxHP) return;

        lastMaxHP = maxHP;
        int needed = Mathf.CeilToInt(maxHP / 3f);

        // Clear old children and list when increase maxHealth
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
        health.Clear();

        // Spawn health sprites
        for (int i = 0; i < needed; i++)
        {
            var img = Instantiate(healthPointPrefab, root);
            img.enabled = true;
            health.Add(img);
        }
    }

    // Pick which sprite to use based on remaining HP for that slot
    public void chooseHealthSprite()
    {
        int hp = combat.stats.currentHP;
        for (int i = 0; i < health.Count; i++)
        {
            int theHealthSpriteWanted = Mathf.Clamp(hp - i * 3, 0, 3);
            health[i].sprite = healthPoint[theHealthSpriteWanted];
        }
    }
}
