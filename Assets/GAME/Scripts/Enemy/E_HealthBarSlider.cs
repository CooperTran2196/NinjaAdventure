using UnityEngine;
using UnityEngine.UI;

public class E_HealthBarSlider : MonoBehaviour
{
    [Header("References")]
    CanvasGroup cg;
    C_Health    e_Health;
    C_Stats     e_Stats;
    Slider      slider;

    [Header("World Positioning")]
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Visibility")]
    public float visibleTime = 2f;

    float hideTimer;

    void Awake()
    {
        cg       ??= GetComponent<CanvasGroup>();
        e_Health ??= GetComponentInParent<C_Health>();
        e_Stats  ??= GetComponentInParent<C_Stats>();
        slider   ??= GetComponentInChildren<Slider>();

        if (!e_Health) { Debug.LogError($"{name}: C_Health is missing!", this); return; }
        if (!e_Stats)  { Debug.LogError($"{name}: C_Stats is missing!", this); return; }
        if (!slider)   { Debug.LogError($"{name}: Slider is missing!", this); return; }
    }

    void OnEnable()
    {
        slider.maxValue = e_Stats.maxHP;
        slider.value    = e_Stats.currentHP;

        cg.alpha = 0f;

        e_Health.OnDamaged += OnDamaged;
        e_Health.OnHealed  += OnHealed;
        e_Health.OnDied    += OnDied;
    }

    void OnDisable()
    {
        e_Health.OnDamaged -= OnDamaged;
        e_Health.OnHealed  -= OnHealed;
        e_Health.OnDied    -= OnDied;
    }

    // LateUpdate to ensure it runs after all movement
    void LateUpdate()
    {
        // follow owner
        transform.position = e_Health.transform.position + worldOffset;

        // handle fade-out timer
        if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f) cg.alpha = 0f;
        }
    }

    // Event handlers
    void OnDamaged(int amount)
    {
        slider.value = e_Stats.currentHP;
        Show();
    }

    void OnHealed(int amount)
    {
        slider.value = e_Stats.currentHP;
        Show();
    }

    void OnDied()
    {
        cg.alpha = 0f; // death visuals handled elsewhere
    }

    void Show()
    {
        cg.alpha = 1f;
        hideTimer = visibleTime;
    }
}
