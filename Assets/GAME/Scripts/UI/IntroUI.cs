using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IntroUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Button startButton;

    [Header("Timing (realtime seconds)")]
    [SerializeField] private float showDelay = 2f;

    void Awake()
    {
        cg ??= GetComponent<CanvasGroup>();
        startButton ??= GetComponentInChildren<Button>();
    }

    void OnEnable()
    {
        // hidden + non-interactive at start
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        StartCoroutine(ShowAfterDelay());
    }

    private IEnumerator ShowAfterDelay()
    {
        // Wait for showDelay seconds in real time
        yield return new WaitForSecondsRealtime(showDelay);
        // pause world + show + take input
        Time.timeScale = 0f; 
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    // Hook to Start Button OnClick
    public void StartClicked()
    {
        Time.timeScale = 1f; // unpause world
        cg.interactable = false;
        cg.blocksRaycasts = false;
        Destroy(gameObject); // one time use
    }
}
