// Assets/GAME/Scripts/UI/HealthUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public C_Health health;                 // drag the Player's C_Health here
    public RectTransform root;
    public Image healthPointPrefab;

    [Header("Sprites (index = HP in this slot)")]
    public Sprite[] healthPoint = new Sprite[4]; // 0 -> full

    readonly List<Image> hpIcons = new List<Image>(); // <— renamed
    int lastMaxHP = -1;

    System.Action<int> onDamaged, onHealed;
    System.Action onDied;

    void OnEnable()
    {
        onDamaged = _ => chooseHealthSprite();
        onHealed  = _ => chooseHealthSprite();
        onDied    = () => chooseHealthSprite();

        health.OnDamaged += onDamaged;   // <— subscribe to C_Health
        health.OnHealed  += onHealed;
        health.OnDied    += onDied;

        UpdateUI();
        chooseHealthSprite();
    }

    void OnDisable()
    {
        health.OnDamaged -= onDamaged;
        health.OnHealed  -= onHealed;
        health.OnDied    -= onDied;
    }

    public void UpdateUI()
    {
        int maxHP = health.pStats ? health.pStats.maxHP : health.eStats.maxHP;
        if (maxHP == lastMaxHP) return;

        lastMaxHP = maxHP;
        int needed = Mathf.CeilToInt(maxHP / 3f);

        for (int i = root.childCount - 1; i >= 0; i--) Destroy(root.GetChild(i).gameObject);
        hpIcons.Clear();

        for (int i = 0; i < needed; i++)
        {
            var img = Instantiate(healthPointPrefab, root);
            img.enabled = true;
            hpIcons.Add(img);
        }
    }

    public void chooseHealthSprite()
    {
        int hp = health.pStats ? health.pStats.currentHP : health.eStats.currentHP;
        for (int i = 0; i < hpIcons.Count; i++)
        {
            int idx = Mathf.Clamp(hp - i * 3, 0, 3);
            hpIcons[i].sprite = healthPoint[idx];
        }
    }
}
