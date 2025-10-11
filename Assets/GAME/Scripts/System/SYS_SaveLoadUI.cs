using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class SYS_SaveLoadUI : MonoBehaviour
{
    [Header("Optional")]
    [SerializeField] private TMP_InputField slotNameInput; // e.g., "slot1.json"
    [SerializeField] private TMP_Text feedback;            // small status label

    [Header("Defaults")]
    [SerializeField] private string defaultSlot = "slot1.json";
    [SerializeField] private CanvasGroup panel;   // the Save/Load panel root
    P_InputActions input;
    bool isOpen;

    void Awake()
    {
        panel ??= GetComponentInChildren<CanvasGroup>();
        input = new P_InputActions();
        input.UI.ToggleSetting.Enable();

        // start closed
        if (panel)
        {
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }
    }


    void OnDisable()
    {
        input?.UI.Disable();
        input?.Dispose();
    }


    void Update()
    {
        // If dialog is open, let Esc end the dialog only (don’t also open settings)
        var dm = SYS_GameManager.Instance ? SYS_GameManager.Instance.d_Manager : null;
        if (dm && dm.isDialogActive) return;

        if (input.UI.ToggleSetting.WasPressedThisFrame())
            SetOpen(!isOpen);
    }

public void SetOpen(bool open)
{
    isOpen = open;
    if (panel)
    {
        panel.alpha = open ? 1f : 0f;
        panel.interactable = open;
        panel.blocksRaycasts = open;
    }
    Time.timeScale = open ? 0f : 1f;
}

// Optional close hook for an “X” button
public void OnClickClose() => SetOpen(false);

    private string SlotOrDefault()
    {
        var s = slotNameInput ? slotNameInput.text : null;
        return string.IsNullOrWhiteSpace(s) ? defaultSlot : s.Trim();
    }

    // Wire these to your buttons:
    public void OnClickSave()
    {
        var fn = SlotOrDefault();
        SYS_SaveSystem.Instance.SaveGame(fn);
        if (feedback) feedback.SetText($"Saved {fn}");
    }

    public void OnClickLoad()
    {
        var fn = SlotOrDefault();
        bool ok = SYS_SaveSystem.Instance.LoadGameAndTravel(fn);
        if (feedback) feedback.SetText(ok ? $"Loading {fn}" : $"Load failed: {fn}");
    }

    public void OnClickLoadAutosave()
    {
        bool ok = SYS_SaveSystem.Instance.ReplayFromAutosave();
        if (feedback) feedback.SetText(ok ? "Loading autosave" : "Autosave not found");
    }

    public void OnClickRestartInitial()
    {
        bool ok = SYS_SaveSystem.Instance.RestartFromInitial();
        if (feedback) feedback.SetText(ok ? "Restarting…" : "Initial save missing");
    }
}
