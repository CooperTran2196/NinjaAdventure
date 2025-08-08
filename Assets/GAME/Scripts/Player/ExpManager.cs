using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ExpManager : MonoBehaviour
{
    public int level;
    public int totalKills;
    public float playTime;
    public int currentExp;
    public int expToLevel = 10;
    public float expMultilier = 1.2f;
    public Slider expSlider;
    public TMP_Text currentLevelText;
    public static ExpManager Instance;


    public static event Action<int> OnLevelUp;

    void Start()
    {
        UpdateUI();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GainExperience(1);
        }
        playTime += Time.deltaTime;
    }

    private void OnEnable()
    {
        Enemy_Health.OnMonsterDefeated += GainExperience;
    }
    private void OnDisable()
    {
        Enemy_Health.OnMonsterDefeated -= GainExperience;
    }

    
    public void GainExperience(int amount)
    {
        totalKills++;
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
        expToLevel = Mathf.RoundToInt(expToLevel * expMultilier);
        OnLevelUp?.Invoke(1);
    }

    public void UpdateUI()
    {
        expSlider.maxValue = expToLevel;
        expSlider.value = currentExp;
        currentLevelText.text = "Level: " + level;

    }
}
