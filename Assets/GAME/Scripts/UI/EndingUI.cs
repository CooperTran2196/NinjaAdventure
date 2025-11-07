using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class EndingUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Button      replayButton;
    [SerializeField] private TMP_Text    titleText;
    [SerializeField] private GameObject  statsPanel;
    [SerializeField] private TMP_Text    levelText;
    [SerializeField] private TMP_Text    expText;
    [SerializeField] private TMP_Text    killsText;
    [SerializeField] private TMP_Text    timeText;

    [Header("End Triggers (optional)")]
    [SerializeField] private C_Health playerHealth;     // show Game Over on player death (with delay)
    [SerializeField] private C_Health finalBossHealth;  // show Victory on boss death (no delay)

    [Header("Data")]
    [SerializeField] private P_Exp playerExp;

    private bool isWin;
    private bool shown;

    void Awake()
    {
        cg           ??= GetComponent<CanvasGroup>();
        replayButton ??= GetComponentInChildren<Button>(true);
        playerExp    ??= FindFirstObjectByType<P_Exp>();

        var label = replayButton?.GetComponentInChildren<TMP_Text>(true);
        if (label) label.text = "Restart";

        if (!cg)           Debug.LogError("EndingUI: Missing CanvasGroup.");
        if (!replayButton) Debug.LogError("EndingUI: Missing Replay Button.");
        if (!titleText)    Debug.LogError("EndingUI: Missing titleText.");
        if (!statsPanel)   Debug.LogError("EndingUI: Missing statsPanel.");
        if (!levelText)    Debug.LogError("EndingUI: Missing levelText.");
        if (!expText)      Debug.LogError("EndingUI: Missing expText.");
        if (!killsText)    Debug.LogError("EndingUI: Missing killsText.");
        if (!timeText)     Debug.LogError("EndingUI: Missing timeText.");
        if (!playerExp)    Debug.LogWarning("EndingUI: P_Exp not found; stats will be blank.");
    }

    void OnEnable()
    {
        cg.alpha          = 0f;
        cg.interactable   = false;
        cg.blocksRaycasts = false;

        // Only subscribe to boss death (player death handled by GameManager)
        if (finalBossHealth) finalBossHealth.OnDied += OnBossDied;
    }

    void OnDisable()
    {
        if (finalBossHealth) finalBossHealth.OnDied -= OnBossDied;
    }



    void OnBossDied() => Show(true);

    // Called directly by GameManager (no delay, no event)
    public void Show(bool win)
    {
        if (shown) return;
        shown = true;
        isWin = win;
        // Pause world while the ending screen is visible
        Time.timeScale = 0f;

        titleText.text = win ? "Victory!" : "Game Over";

        // Stats now show regardless of outcome
        statsPanel.SetActive(true);
        if (playerExp)
        {
            levelText.text = $"Level {playerExp.level}";
            expText.text   = $"{playerExp.currentExp} XP";
            killsText.text = playerExp.totalKills.ToString();
            timeText.text  = FormatTime(playerExp.playTime);
        }

        cg.alpha          = 1f;
        cg.interactable   = true;
        cg.blocksRaycasts = true;
    }

    string FormatTime(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int h = Mathf.FloorToInt(seconds / 3600f);
        int m = Mathf.FloorToInt((seconds % 3600f) / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return h > 0 ? $"{h:D2}:{m:D2}:{s:D2}" : $"{m:D2}:{s:D2}";
    }

    public void OnClickReplay()
    {
        if (!shown) return;
        cg.interactable   = false;
        cg.blocksRaycasts = false;
        Time.timeScale = 1f;

        SYS_GameManager.Instance.FreshBoot();
    }
}
