using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
public class ExpUI : MonoBehaviour
{
    public int level;
    public int currentExp;
    public int expToLevel = 10;
    public float expGrowMultiplier = 1.2f;
    public Slider expSlider;
    public TMP_Text currentLevelText;

    private P_InputActions input;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (input.Debug.GainExp.WasPressedThisFrame())
        {
            GainExperience(2);
        }
    }
    public void GainExperience(int amount)
    {
        currentExp += amount;
        if (currentExp >= expToLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    private void LevelUp()
    {
        level++;
        currentExp -= expToLevel;
        expToLevel = Mathf.RoundToInt(expToLevel * expGrowMultiplier);
    }

    public void UpdateUI()
    {
        expSlider.maxValue = expToLevel;
        expSlider.value = currentExp;
        currentLevelText.text = "Level: " + level;
    }
}