using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class IntroUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Button startButton;

    [Header("Timing")]
    [SerializeField] private float showDelay = 2f;   // realtime seconds

    private Coroutine showRoutine;

    void Awake()
    {
        cg ??= GetComponent<CanvasGroup>();
        startButton ??= GetComponentInChildren<Button>(true);
    }

    void OnEnable()
    {
        // hidden + non-interactive at start
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        showRoutine = StartCoroutine(ShowAfterDelay());
    }

    void OnDisable()
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }
    }

    private IEnumerator ShowAfterDelay()
    {
        // wait using unscaled time (immune to pauses/fade)
        float t = showDelay;
        while (t > 0f)
        {
            yield return null;
            t -= Time.unscaledDeltaTime;
        }

        // pause world while intro is visible
        Time.timeScale = 0f;

        // show + take input
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // focus button for keyboard/controller submit
        yield return null;
        EventSystem.current?.SetSelectedGameObject(startButton.gameObject);
    }

    // Hook to Start Button OnClick
    public void StartClicked()
    {
        Time.timeScale = 1f;                 // resume
        cg.interactable = false;
        cg.blocksRaycasts = false;
        Destroy(gameObject);                  // one-time use
    }
}
