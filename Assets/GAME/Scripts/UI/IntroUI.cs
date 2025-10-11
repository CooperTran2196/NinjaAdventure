using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Button      startButton;

    [Header("Timing (realtime seconds)")]
    [SerializeField] private float showDelay = 2f;

    void Awake()
    {
        cg          ??= GetComponent<CanvasGroup>();
        startButton ??= GetComponentInChildren<Button>();
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

    public void StartClicked()
    {
        Time.timeScale    = 1f;
        cg.interactable   = false;
        cg.blocksRaycasts = false;
        Destroy(gameObject);
    }
}
