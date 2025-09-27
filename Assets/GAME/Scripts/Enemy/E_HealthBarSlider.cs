// File: E_HealthBarSlider.cs  (enemy-only, green-only)
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]

public class E_HealthBarSlider : MonoBehaviour
{
    [Header("References")]
    CanvasGroup cg;

    public C_Health e_Health; // Health in parent
    public Slider   slider;   // slider in child

    [Header("World Positioning")]
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Visibility")]
    public float visibleTime = 2f;   // auto-hide delay
    public bool  startHidden = true; // start invisible until first event

    float hideTimer;

    void Awake()
    {
        cg       ??= GetComponent<CanvasGroup>();
        e_Health ??= GetComponentInParent<C_Health>();

        if (!e_Health) Debug.LogError($"{name}: C_Health in E_HealthBarSlider is missing.", this);
        if (!slider)   Debug.LogError($"{name}: Slider in E_HealthBarSlider is missing.", this);
    }

    void OnEnable()
    {
        var s           = e_Health.c_Stats;      // current/max HP live here
        slider.maxValue = s.maxHP;     // make slider track the true max
        slider.value    = s.currentHP; // initialize to current

        if (startHidden) cg.alpha = 0f;

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
        slider.value = e_Health.c_Stats.currentHP;
        Show();
    }

    void OnHealed(int amount)
    {
        slider.value = e_Health.c_Stats.currentHP;
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
