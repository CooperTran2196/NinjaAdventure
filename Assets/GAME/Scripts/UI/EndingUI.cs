using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[DisallowMultipleComponent]
public class EndingUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Button replayButton;
    [SerializeField] private TMP_Text titleText;     // "Victory!" / "Game Over"
    [SerializeField] private GameObject statsPanel;  // now shown on BOTH win/lose
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text timeText;

    [Header("End Triggers (optional)")]
    [SerializeField] private C_Health playerHealth;     // show Game Over on player death (with delay)
    [SerializeField] private C_Health finalBossHealth;  // show Victory on boss death (no delay)

    [Header("Next Scene (optional)")]
    [Tooltip("If empty, reloads current scene; otherwise loads this scene (e.g., \"Level2\").")]
    [SerializeField] private string nextSceneName = "";

    [Header("Timing")]
    [Tooltip("Delay after player death before the Ending UI appears (realtime seconds).")]
    [SerializeField] private float gameOverDelay = 2f;  // realtime seconds

    [Header("Data")]
    [SerializeField] private P_Exp playerExp;   // provides level/currentExp/totalKills/playTime
private bool isWin;

    private bool shown;

    void Awake()
    {
        cg ??= GetComponent<CanvasGroup>();
        replayButton ??= GetComponentInChildren<Button>(true);
        playerExp ??= FindObjectOfType<P_Exp>(true);

        if (!cg) Debug.LogError("[EndingUI] Missing CanvasGroup.");
        if (!replayButton) Debug.LogError("[EndingUI] Missing Replay Button.");
        if (!titleText) Debug.LogError("[EndingUI] Missing titleText.");
        if (!statsPanel) Debug.LogError("[EndingUI] Missing statsPanel.");
        if (!levelText) Debug.LogError("[EndingUI] Missing levelText.");
        if (!expText) Debug.LogError("[EndingUI] Missing expText.");
        if (!killsText) Debug.LogError("[EndingUI] Missing killsText.");
        if (!timeText) Debug.LogError("[EndingUI] Missing timeText.");
        if (!playerExp) Debug.LogWarning("[EndingUI] P_Exp not found; stats will be blank.");
    }

    void OnEnable()
    {
        // start hidden & non-interactive
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        if (playerHealth) playerHealth.OnDied += OnPlayerDied;
        if (finalBossHealth) finalBossHealth.OnDied += OnBossDied;
        // Button OnClick -> OnClickReplay() should be wired in Inspector (mouse only; no keyboard handling)
    }

    void OnDisable()
    {
        if (playerHealth) playerHealth.OnDied -= OnPlayerDied;
        if (finalBossHealth) finalBossHealth.OnDied -= OnBossDied;
    }

    // Optional public triggers (e.g., from a portal)
    public void ShowVictory()  => Show(true);
    public void ShowGameOver() => Show(false);

    private void OnBossDied()   => Show(true);

    private void OnPlayerDied()
    {
        if (shown) return;
        StartCoroutine(ShowGameOverAfterDelay());
    }

    private System.Collections.IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSecondsRealtime(gameOverDelay);
        Show(false);
    }

    private void Show(bool win)
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

        // Reveal modal
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private string FormatTime(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int h = Mathf.FloorToInt(seconds / 3600f);
        int m = Mathf.FloorToInt((seconds % 3600f) / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return h > 0 ? $"{h:D2}:{m:D2}:{s:D2}" : $"{m:D2}:{s:D2}";
    }

    // Hook this to the Replay button OnClick in the Inspector (mouse click only)
public void OnClickReplay()
{
    if (!shown) return;

    // Stop taking input first
    cg.interactable = false;
    cg.blocksRaycasts = false;

    // Unpause before traveling
    Time.timeScale = 1f;

    if (isWin)
    {
        // Optional: ensure boss-death spawn override is cleared on victory
        SYS_SaveSystem.Instance.ClearBossDeathReturnSpawn();

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // Victory + next scene: teleport straight to that scene (start spawn)
            SYS_SaveSystem.Instance.AdvanceToNextScene(nextSceneName, "Start");
        }
        else
        {
            // Victory + no next scene: restart from initial save (Level1 start)
            SYS_SaveSystem.Instance.RestartFromInitial();
        }
    }
    else
    {
        // Game Over: replay from autosave (boss-return spawn beats checkpoint if set)
        SYS_SaveSystem.Instance.ReplayFromAutosave();
    }
}

}
