using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    public Image bar;             // drag Health UI image
    public TMP_Text hpText;       // drag HP Text

    void LateUpdate()
    {
        float ratio = (float)StatsManager.Instance.currentHealth / StatsManager.Instance.maxHealth;
        bar.fillAmount = ratio;   // 1 = full, 0 = empty
        hpText.text = $"HP: {StatsManager.Instance.currentHealth}/{StatsManager.Instance.maxHealth}";
    }
}
