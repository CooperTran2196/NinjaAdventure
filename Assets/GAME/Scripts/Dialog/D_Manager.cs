using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class D_Manager : MonoBehaviour
{
    // ---- Singleton ----
    public static D_Manager Instance;

    // ---- UI refs ----
    [Header("UI")]
    public Image portrait;
    public TMP_Text actorNameText;
    public TMP_Text dialogueText;
    public CanvasGroup canvasGroup;

    // ---- State ----
    public D_SO currentDialogue;
    int dialogueIndex;
    public bool isDialogueActive;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // start hidden
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // Show current line then advance index
    void ShowDialogue()
    {
        var line = currentDialogue.lines[dialogueIndex++];
        portrait.sprite        = line.speaker.portrait;
        actorNameText.text     = line.speaker.actorName;
        dialogueText.text      = line.text;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // Called by NPC to begin a conversation
    public void StartDialogue(D_SO dialogue)
    {
        currentDialogue = dialogue;
        dialogueIndex = 0;
        isDialogueActive = true;
        ShowDialogue();
    }

    // Called when player presses Interact during an active convo
    public void AdvanceDialogue()
    {
        if (dialogueIndex < currentDialogue.lines.Length)
        {
            ShowDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    // Hide + reset
    public void EndDialogue()
    {
        dialogueIndex = 0;
        isDialogueActive = false;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
