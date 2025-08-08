using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroScreen : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Canvas containing the Intro Screen UI")]
    [SerializeField] private Canvas introCanvas;
    [Tooltip("Button to start the game")]
    [SerializeField] private Button startButton;
    [Tooltip("Image displaying the input tutorial")]
    [SerializeField] private Image tutorialImage;

    [Header("Settings")]
    [Tooltip("Scene to load when Start Game is clicked")]
    [SerializeField] private string gameSceneName = "MainGameScene";

    private bool isGameStarted = false;

    private void Awake()
    {
        // Ensure canvas is enabled at start
        if (introCanvas != null)
        {
            introCanvas.enabled = true;
        }
        else
        {
            Debug.LogError("IntroScreen: IntroCanvas is not assigned.");
        }

        // Ensure Time.timeScale is paused to prevent gameplay
        Time.timeScale = 0f;
    }

    private void Start()
    {
        // Assign button listener
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogError("IntroScreen: StartButton is not assigned.");
        }
    }

    private void Update()
    {
        // Reopen intro screen with F12
        if (Input.GetKeyDown(KeyCode.F12) && isGameStarted)
        {
            ShowIntroScreen();
        }
    }

    private void OnStartButtonClicked()
    {
        // Resume gameplay and hide intro screen
        isGameStarted = true;
        Time.timeScale = 1f;
        if (introCanvas != null)
        {
            introCanvas.enabled = false;
        }

        // Load main game scene if not already in it
        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    private void ShowIntroScreen()
    {
        // Pause game and show intro screen
        Time.timeScale = 0f;
        if (introCanvas != null)
        {
            introCanvas.enabled = true;
        }
    }

    private void OnEnable()
    {
        // Ensure button listener is added
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void OnDisable()
    {
        // Clean up listener to prevent memory leaks
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }
}