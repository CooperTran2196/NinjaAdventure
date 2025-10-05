// <summary>
// Manages dialog flow, UI updates, and player choices.
// </summary>

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class D_Manager : MonoBehaviour
{
    [Header("UI")]
    public Image avatar;
    public TMP_Text characterNameText;
    public TMP_Text dialogText;
    public CanvasGroup canvasGroup;

    [Header("Choices")]
    public Button[] choiceButtons;

    D_SO currentDialog;
    int dialogIndex;
    public bool isDialogActive { get; private set; }

    void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // hide all option buttons at boot
        foreach (var b in choiceButtons)
            b.gameObject.SetActive(false);
    }
    
    void ShowDialog()
    {
        // Clear previous choices
        ClearChoices();
        // advancing a line hides old choices, if any
        var line = currentDialog.lines[dialogIndex++]; 

        SYS_GameManager.Instance.d_HistoryTracker?.RecordNPC(line.speaker);
        avatar.sprite      = line.speaker.avatar;
        characterNameText.text = line.speaker.actorName;
        dialogText.text  = line.text;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void StartDialog(D_SO dialog)
    {
        currentDialog = dialog;
        dialogIndex = 0;
        isDialogActive = true;
        ClearChoices();
        ShowDialog();
    }

    public void AdvanceDialog()
    {
        if (dialogIndex < currentDialog.lines.Length)
        {
            ShowDialog();
        }
        else
        {
            ShowChoices();
        }
    }

    void ShowChoices()
    {
        ClearChoices();
        var opts = currentDialog.options;

        if (opts != null && opts.Length > 0)
        {
            int count = Mathf.Min(opts.Length, choiceButtons.Length);
            for (int i = 0; i < count; i++)
            {
                var btn = choiceButtons[i];
                var opt = opts[i]; // capture for lambda

                btn.GetComponentInChildren<TMP_Text>(true).text = opt.optionButtonText;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ChooseOption(opt.nextDialog));
                btn.gameObject.SetActive(true);
            }
        }
        else
        {
            // no options → show a simple End button on slot 0
            var endBtn = choiceButtons[0];
            endBtn.GetComponentInChildren<TMP_Text>(true).text = "End";
            endBtn.onClick.RemoveAllListeners();
            endBtn.onClick.AddListener(EndDialog);
            endBtn.gameObject.SetActive(true);
        }
    }

    void ChooseOption(D_SO nextDialog)
    {
        ClearChoices();

        if (nextDialog == null)
        {
            EndDialog();
            return;
        }

        StartDialog(nextDialog);
    }

    public void EndDialog()
    {
        dialogIndex = 0;
        isDialogActive = false;
        ClearChoices();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void ClearChoices()
    {
        foreach (var b in choiceButtons)
        {
            b.onClick.RemoveAllListeners();
            b.gameObject.SetActive(false);
        }
    }
}
