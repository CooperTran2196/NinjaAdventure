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

    [Header("Option Buttons (max 3)")]
    public Button[] optionButtons;

    D_SO currentDialog;
    int dialogIndex;
    public bool isDialogActive;

    void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // hide all option buttons at boot
        foreach (var b in optionButtons)
            b.gameObject.SetActive(false);
    }

    public void StartDialog(D_SO dialog)
    {
        currentDialog = dialog;
        dialogIndex = 0;
        isDialogActive = true;
        ClearOptions();
        ShowDialog();
    }

    void ShowDialog()
    {
        // Clear previous optionList
        ClearOptions();
        // advancing a line hides old choices, if any
        var line = currentDialog.lines[dialogIndex++]; 
        // Record NPC spoken to
        SYS_GameManager.Instance.d_HistoryTracker.RecordNPC(line.speaker);
        
        // Play random NPC talk sound
        SYS_GameManager.Instance.sys_SoundManager.PlayNPCTalk();
        
        // Update UI
        avatar.sprite           = line.speaker.avatar;
        characterNameText.text  = line.speaker.characterName;
        dialogText.text         = line.text;
        // Show UI
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // Called to advance dialog or show choices
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
        // Clear previous optionList
        ClearOptions();
        var optionList = currentDialog.optionList;

        // Show available option buttons, or a simple End button if none
        if (optionList != null && optionList.Length > 0)
        {   
            // Show as many optionList as we have, up to the max buttons available (3)
            int count = Mathf.Min(optionList.Length, optionButtons.Length);
            for (int i = 0; i < count; i++)
            {
                var button = optionButtons[i];
                var option = optionList[i]; // capture for lambda

                button.GetComponentInChildren<TMP_Text>().text = option.optionButtonText;
                button.onClick.AddListener(() => ChooseOption(option.nextDialog));
                button.gameObject.SetActive(true);
            }
        }
        else
        {
            // No optionList -> show a simple End button on slot 0
            var endButton = optionButtons[0];
            endButton.GetComponentInChildren<TMP_Text>().text = "End";
            endButton.onClick.AddListener(EndDialog_WithRewards);
            endButton.gameObject.SetActive(true);
        }
    }

    // Called when player selects an option button
    void ChooseOption(D_SO nextDialog)
    {
        if (nextDialog == null)
        {
            EndDialog_WithRewards();
            return;
        }
        
        // Start next dialog
        StartDialog(nextDialog);
    }

    // Grant auto rewards if any
    void GrantAutoRewards(D_SO dialog)
    {
        var rewards = dialog.autoRewards;
        if (rewards == null || rewards.Length == 0)
        {
            Debug.Log($"[D_Manager] No autoRewards for dialog: {dialog.name}");
            return;
        }

        Debug.Log($"[D_Manager] Granting {rewards.Length} autoReward(s) from dialog: {dialog.name}");

        // Get INV_Manager through GameManager for proper scene lifecycle handling
        var inv_Manager = SYS_GameManager.Instance.inv_Manager;
        if (!inv_Manager)
        {
            Debug.LogError($"[D_Manager] INV_Manager not found in GameManager! Cannot grant rewards.");
            return;
        }

        // Grant each reward
        for (int i = 0; i < rewards.Length; i++)
        {
            var reward = rewards[i];
            
            // Grant weapon if specified
            if (reward.weaponSO != null)
            {
                Debug.Log($"[D_Manager] Granting weapon: {reward.weaponSO.id}");
                bool success = inv_Manager.AddWeapon(reward.weaponSO);
                Debug.Log($"[D_Manager] Weapon grant {(success ? "SUCCESS" : "FAILED - inventory full")}");
            }
            // Otherwise grant item if specified
            else if (reward.itemSO != null)
            {
                Debug.Log($"[D_Manager] Granting item: {reward.itemSO.itemName} x{reward.quantity}");
                inv_Manager.AddItem(reward.itemSO, reward.quantity);
            }
        }
    }

    // Call this when the dialog ends with rewards
    public void EndDialog_WithRewards()
    {
        // first grant rewards if any
        GrantAutoRewards(currentDialog);

        // then end dialog
        EndDialog();
    }

    // Call this for a simple end, no rewards
    public void EndDialog()
    {
        dialogIndex = 0;
        isDialogActive = false;
        ClearOptions();

        // Play dialog end sound
        SYS_GameManager.Instance.sys_SoundManager.PlayDialogEnd();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // Clear all option buttons and their listeners
    void ClearOptions()
    {
        foreach (var button in optionButtons)
        {
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }
}
