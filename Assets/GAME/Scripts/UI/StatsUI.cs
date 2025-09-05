using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [Header("References")]
    public C_Stats p_Stats;
    public CanvasGroup statsCanvas;
    public GameObject[] statsSlots;

    private P_InputActions input;
    private bool statsOpen = false;

    void Awake()
    {
        input = new P_InputActions();
        input.UI.ToggleStats.Enable();

        // start closed
        statsCanvas.alpha = 0;
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
            "Damage: " + p_Stats.AD;
    }
    public void UpdateAP()
    {
        statsSlots[1].GetComponentInChildren<TMP_Text>().text =
            "Ability Power: " + p_Stats.AP;
    }
    public void UpdateMS()
    {
        statsSlots[2].GetComponentInChildren<TMP_Text>().text =
            "Move Speed: " + p_Stats.MS;
    }

    public void UpdateMaxHealth()
    {
        statsSlots[3].GetComponentInChildren<TMP_Text>().text =
            "Max Health: " + p_Stats.maxHP;
    }
    public void UpdateAR()
    {
        statsSlots[4].GetComponentInChildren<TMP_Text>().text =
            "Armor: " + p_Stats.AR;
    }
    public void UpdateMR()
    {
        statsSlots[5].GetComponentInChildren<TMP_Text>().text =
            "Magic Res: " + p_Stats.MR;
    }
        public void UpdateKR()
    {
        statsSlots[6].GetComponentInChildren<TMP_Text>().text =
            "Knockback Res: " + p_Stats.KR;
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
