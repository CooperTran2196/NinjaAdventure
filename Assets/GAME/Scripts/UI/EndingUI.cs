using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndingUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Canvas for the ending screen")]
    [SerializeField] private Canvas endingCanvas;

    [Tooltip("Panel for stats (hidden on game over)")]
    [SerializeField] private GameObject statsPanel;

    [Tooltip("Text for title: Win or Game Over")]
    [SerializeField] private TMP_Text endingPanelText;

    [Tooltip("Text for level display")]
    [SerializeField] private TMP_Text levelText;

    [Tooltip("Text for XP display")]
    [SerializeField] private TMP_Text xpText;

    [Tooltip("Text for kills display")]
    [SerializeField] private TMP_Text killsText;

    [Tooltip("Text for completion time display")]
    [SerializeField] private TMP_Text timeText;
    
    [Tooltip("Button to replay the game")]
    [SerializeField] private Button replayButton;
    [SerializeField] private TMP_Text replayButtonText;


    private bool isWin = false;

    private void Start()
    {
        // Hide canvas at start
        if (endingCanvas != null)
        {
            endingCanvas.enabled = false;
        }

        // Assign button listener
        if (replayButton != null)
        {
            replayButton.onClick.AddListener(OnReplayButtonClicked);
        }
        else
        {
            Debug.LogError("EndingUI: ReplayButton is not assigned.");
        }
    }

    public void ShowEndingScreen(bool winCondition)
    {
        isWin = winCondition;
        Time.timeScale = 0f; // Pause game

        if (endingCanvas != null)
        {
            endingCanvas.enabled = true;
        }

        // Update title
        if (endingPanelText != null)
        {
            endingPanelText.text = winCondition ? "Victory!" : "Game Over";
        }

        // Show/hide stats panel
        if (statsPanel != null)
        {
            statsPanel.SetActive(winCondition);
        }

        // Update stats if win
        if (winCondition)
        {
            if (levelText != null) levelText.text = $"Level: {ExpManager.Instance.level}";
            if (xpText != null) xpText.text = $"XP: {ExpManager.Instance.currentExp}";
            if (killsText != null) killsText.text = $"Kills: {ExpManager.Instance.totalKills}";
            if (timeText != null) timeText.text = $"Time: {FormatTime(ExpManager.Instance.playTime)}";
        }
    }

    private void OnReplayButtonClicked()
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:D2}:{seconds:D2}";
    }

    private void OnEnable()
    {
        // Subscribe to events
        Enemy_Health.OnFinalBossDefeated += CheckBossDeath;
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent leaks
        Enemy_Health.OnFinalBossDefeated -= CheckBossDeath;
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
    }

    private void CheckBossDeath()
    {
        // Temporary bypass until final boss detection is refined
            ShowEndingScreen(true);

    }

    private void HandlePlayerDeath()
    {
        ShowEndingScreen(false);
    }
}