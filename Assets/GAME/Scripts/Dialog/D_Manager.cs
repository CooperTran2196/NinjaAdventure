// <summary>
// Manages dialogue flow, UI updates, and player choices.
// </summary>

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class D_Manager : MonoBehaviour
{
    public static D_Manager Instance { get; private set; }

    [Header("UI")]
    public Image portrait;
    public TMP_Text actorNameText;
    public TMP_Text dialogueText;
    public CanvasGroup canvasGroup;

    [Header("Choices")]
    public Button[] choiceButtons;

    D_SO currentDialogue;
    int dialogueIndex;
    public bool isDialogueActive { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // hide all option buttons at boot
        foreach (var b in choiceButtons)
            b.gameObject.SetActive(false);
    }

    void ShowDialogue()
    {
        // advancing a line hides old choices, if any
        ClearChoices();

        var line = currentDialogue.lines[dialogueIndex++]; 

        D_HistoryTracker.Instance.RecordNPC(line.speaker);
        portrait.sprite    = line.speaker.portrait;
        actorNameText.text = line.speaker.actorName;
        dialogueText.text  = line.text;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void StartDialogue(D_SO dialogue)
    {
        currentDialogue = dialogue;
        dialogueIndex = 0;
        isDialogueActive = true;
        ClearChoices();
        ShowDialogue();
    }

    public void AdvanceDialogue()
    {
        if (dialogueIndex < currentDialogue.lines.Length)
        {
            ShowDialogue();
        }
        else
        {
            ShowChoices();
        }
    }

    void ShowChoices()
    {
        ClearChoices();
        var opts = currentDialogue.options;

        if (opts != null && opts.Length > 0)
        {
            int count = Mathf.Min(opts.Length, choiceButtons.Length);
            for (int i = 0; i < count; i++)
            {
                var btn = choiceButtons[i];
                var opt = opts[i]; // capture for lambda

                btn.GetComponentInChildren<TMP_Text>(true).text = opt.optionText;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ChooseOption(opt.nextDialogue));
                btn.gameObject.SetActive(true);
            }
        }
        else
        {
            // no options → show a simple End button on slot 0
            var endBtn = choiceButtons[0];
            endBtn.GetComponentInChildren<TMP_Text>(true).text = "End";
            endBtn.onClick.RemoveAllListeners();
            endBtn.onClick.AddListener(EndDialogue);
            endBtn.gameObject.SetActive(true);
        }
    }

    void ChooseOption(D_SO nextDialogue)
    {
        ClearChoices();

        if (nextDialogue == null)
        {
            EndDialogue();
            return;
        }

        StartDialogue(nextDialogue);
    }

    public void EndDialogue()
    {
        dialogueIndex = 0;
        isDialogueActive = false;
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
