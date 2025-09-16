using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [Header("References")]
    public C_Stats p_Stats;
    public P_StatsChanged p_StatsChanged;
    public CanvasGroup statsCanvas;
    public GameObject[] statsSlots;

    private P_InputActions input;
    private bool statsOpen = false;

    void Awake()
    {
        input = new P_InputActions();
        input.UI.ToggleStats.Enable();

        p_Stats ??= FindFirstObjectByType<C_Stats>();
        p_StatsChanged ??= FindFirstObjectByType<P_StatsChanged>();

        // start closed
        statsCanvas.alpha = 0;
    }
    
    void OnEnable()
    {
        if (p_StatsChanged != null)
            p_StatsChanged.OnStatsRecalculated += UpdateAllStats;  // <-- add
    }

    void OnDisable()
    {
        if (p_StatsChanged != null)
            p_StatsChanged.OnStatsRecalculated -= UpdateAllStats;  // <-- add
    }

    void Start()
    {
        UpdateAllStats();
    }

    void Update()
    {
        if (!input.UI.ToggleStats.WasPressedThisFrame()) return;

        if (statsOpen) // Close
        {
            Time.timeScale  = 1;
            statsCanvas.alpha = 0;
            statsOpen = false;
        }
        else // Open
        {
            UpdateAllStats();
            Time.timeScale  = 0;
            statsCanvas.alpha = 1;
            statsOpen = true;
        }
    }

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
