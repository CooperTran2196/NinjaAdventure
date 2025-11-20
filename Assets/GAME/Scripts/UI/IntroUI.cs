using UnityEngine;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Button      normalButton;
    [SerializeField] private Button      easyButton;

    static bool hasShown = false;

    // Reset the static flag (called by SYS_GameManager.FreshBoot)
    public static void Reset()
    {
        hasShown = false;
    }

    void Awake()
    {
        cg ??= GetComponent<CanvasGroup>();
        
        // If already shown -> disable this GameObject
        if (hasShown)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    void OnEnable()
    {
        cg.alpha          = 1f;
        cg.interactable   = true;
        cg.blocksRaycasts = true;
    }

    public void OnNormalClicked()
    {
        SYS_GameManager.Instance.SetDifficulty(Difficulty.Normal);
        StartGame();
    }

    public void OnEasyClicked()
    {
        SYS_GameManager.Instance.SetDifficulty(Difficulty.Easy);
        SYS_GameManager.Instance.ApplyEasyModeBonuses();
        StartGame();
    }

    void StartGame()
    {
        cg.interactable   = false;
        cg.blocksRaycasts = false;
        
        // Mark as shown and disable immediately
        hasShown = true;
        gameObject.SetActive(false);
    }
}
