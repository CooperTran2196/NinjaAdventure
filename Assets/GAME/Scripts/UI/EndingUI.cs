using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndingUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Canvas for the ending screen")]
    [SerializeField] Canvas endingCanvas;

    [Tooltip("Panel for stats (hidden on game over)")]
    [SerializeField] GameObject statsPanel;
    [SerializeField] TMP_Text endingPanelText; // "Victory!" or "Game Over"
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text expText;
    [SerializeField] TMP_Text totalKillsText;
    [SerializeField] TMP_Text totalTimeText;
    [SerializeField] Button replayButton;

    [Header("System References")]
    [Tooltip("Drag the Final Boss GameObject here")]
    [SerializeField] C_Health finalBossHealth;

    private P_Exp p_Exp;
    private C_Health playerHealth;
    private bool isGameOver = false;

    private void Awake()
    {
        // Auto-wire components
        p_Exp = FindFirstObjectByType<P_Exp>();
        
        // Find player health component
        P_Controller player = FindFirstObjectByType<P_Controller>();
        if (player != null)
        {
            playerHealth = player.GetComponent<C_Health>();
        }

        // Null checks
        if (!p_Exp) Debug.LogError("EndingUI: P_Exp is missing in the scene.");
        if (!playerHealth) Debug.LogError("EndingUI: Player's C_Health component not found.");
        if (!finalBossHealth) Debug.LogError("EndingUI: Final Boss Health is not assigned in the inspector.");
        if (!endingCanvas) Debug.LogError("EndingUI: EndingCanvas is not assigned.");
    }

    private void Start()
    {
        // Hide canvas at start
        endingCanvas.enabled = false;


        // Assign button listener
        if (replayButton != null)
        {
            replayButton.onClick.AddListener(OnReplayButtonClicked);
        }
    }

    private void OnEnable()
    {
        // Subscribe to events
        if (finalBossHealth != null)
        {
            finalBossHealth.OnDied += HandleFinalBossDefeated;
        }
        if (playerHealth != null)
        {
            playerHealth.OnDied += HandlePlayerDeath;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent leaks
        if (finalBossHealth != null)
        {
            finalBossHealth.OnDied -= HandleFinalBossDefeated;
        }
        if (playerHealth != null)
        {
            playerHealth.OnDied -= HandlePlayerDeath;
        }
    }

    public void ShowEndingScreen(bool winCondition)
    {
        if (isGameOver) return; // Prevent showing screen multiple times
        isGameOver = true;

        Time.timeScale = 0f; // Pause game
        endingCanvas.enabled = true;

        // Update title
        endingPanelText.text = winCondition ? "Victory!" : "Game Over";

        // Show/hide stats panel
        statsPanel.SetActive(winCondition);

        // Update stats if win
        if (winCondition && p_Exp != null)
        {
            levelText.text = $"Level: {p_Exp.level}";
            expText.text = $"XP: {p_Exp.currentExp}";
            totalKillsText.text = $"Kills: {p_Exp.totalKills}";
            totalTimeText.text = $"Time: {FormatTime(p_Exp.playTime)}";
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

    private void HandleFinalBossDefeated()
    {
        ShowEndingScreen(true);
    }

    private void HandlePlayerDeath()
    {
        ShowEndingScreen(false);
    }
}