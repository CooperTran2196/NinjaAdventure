using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Enemy_Healthbar : MonoBehaviour
{
    public Slider slider;
    public bool   anchorTop = true;
    public float  visibleTime = 2f;
    public float yOffset = 0.6f;

    Enemy_Health enemy;
    CanvasGroup  cg;
    float        timer;

    void Awake()
    {
        enemy = GetComponentInParent<Enemy_Health>();
        cg    = GetComponent<CanvasGroup>();
        cg.alpha = 0;                         // start hidden
        slider.maxValue = enemy.maxHealth;
    }

    public void Show()                       // called from TakeHit()
    {
        timer    = visibleTime;
        cg.alpha = 1;
    }

    void Update()
    {
        /*  update the fill */
        slider.value = enemy.currentHealth;

        /*  position the bar */
        float sign = anchorTop ? 1f : -1f;
        transform.localPosition = new Vector3(0f, sign * yOffset, 0f);

        /*  handle fade-out */
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0) cg.alpha = 0;
        }
    }
}
