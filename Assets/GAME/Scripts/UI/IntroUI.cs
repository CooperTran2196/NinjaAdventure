using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Button      normalButton;
    [SerializeField] private Button      easyButton;
    [SerializeField] private P_Exp       p_Exp;
    [SerializeField] private INV_Manager inv_Manager;

    void Awake()
    {
        cg ??= GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
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
        SYS_GameManager.Instance.ApplyEasyModeBonuses(p_Exp, inv_Manager);
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
