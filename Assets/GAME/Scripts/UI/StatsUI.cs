using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [Header("References")]
    public C_Stats p_Stats;
    public C_StatsManager p_StatsManager;
    public CanvasGroup statsCanvas;
    public GameObject[] statsSlots;

    P_InputActions input;
    bool statsOpen = false;

    void Awake()
    {
        input = new P_InputActions();
        input.UI.ToggleStats.Enable();

        p_Stats ??= FindFirstObjectByType<C_Stats>();
        p_StatsManager ??= FindFirstObjectByType<C_StatsManager>();

        // start closed
        statsCanvas.alpha = 0;
    }
    
    void OnEnable()
    {
        if (p_StatsManager != null)
            p_StatsManager.OnStatsChanged += UpdateAllStats;
    }

    void OnDisable()
    {
        if (p_StatsManager != null)
            p_StatsManager.OnStatsChanged -= UpdateAllStats;
    }

    // Initialize all stats display
    void Start()
    {
        UpdateAllStats();
    }

    // Toggle stats UI with input
    void Update()
    {
        if (input.UI.ToggleStats.WasPressedThisFrame())
            SetOpen(!statsOpen);
    }

    //  Open/close the stats UI
    void SetOpen(bool open)
    {
        statsOpen = open;

        Time.timeScale = open ? 0f : 1f;
        statsCanvas.alpha = open ? 1f : 0f;
    }

    // Update individual stat displays
    public void UpdateAD()
    {
        statsSlots[0].GetComponentInChildren<TMP_Text>().text =
            "AD: " + p_Stats.AD;
    }
    public void UpdateAP()
    {
        statsSlots[1].GetComponentInChildren<TMP_Text>().text =
            "AP: " + p_Stats.AP;
    }
    public void UpdateMS()
    {
        statsSlots[2].GetComponentInChildren<TMP_Text>().text =
            "MS: " + p_Stats.MS;
    }

    public void UpdateMaxHealth()
    {
        statsSlots[3].GetComponentInChildren<TMP_Text>().text =
            "HP: " + p_Stats.maxHP;
    }
    public void UpdateAR()
    {
        statsSlots[4].GetComponentInChildren<TMP_Text>().text =
            "AR: " + p_Stats.AR;
    }
    public void UpdateMR()
    {
        statsSlots[5].GetComponentInChildren<TMP_Text>().text =
            "MR: " + p_Stats.MR;
    }
        public void UpdateKR()
    {
        statsSlots[6].GetComponentInChildren<TMP_Text>().text =
            "KR: " + p_Stats.KR;
    }

    public void UpdateAllStats()
    {
        UpdateAD();
        UpdateAP();
        UpdateMS();

        UpdateMaxHealth();
        UpdateAR();
        UpdateMR();
        UpdateKR();
    }
}
