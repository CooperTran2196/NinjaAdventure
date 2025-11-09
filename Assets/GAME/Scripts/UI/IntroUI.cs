using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Button      normalButton;
    [SerializeField] private Button      easyButton;

    [Header("Timing (realtime seconds)")]
    [SerializeField] private float showDelay = 2f;

    void Awake()
    {
        cg ??= GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        cg.alpha          = 0f;
        cg.interactable   = false;
        cg.blocksRaycasts = false;

        StartCoroutine(ShowAfterDelay());
    }

    IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSecondsRealtime(showDelay);

        Time.timeScale    = 0f;
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
        Time.timeScale    = 1f;
        cg.interactable   = false;
        cg.blocksRaycasts = false;
        Destroy(gameObject);
    }
}
