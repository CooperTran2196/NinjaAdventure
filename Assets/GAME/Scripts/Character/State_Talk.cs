using System.Collections.Generic;
using UnityEngine;

public class State_Talk : MonoBehaviour
{
    [Header("References")]
    public Animator interactAnim; // Talk icon animator
    public D_SO currentDialog;
    [Header("All possible dialogs for this NPC, order matters")]
    [Header("1 time > Completion > Repeatable > Default")]
    public List<D_SO> dialogs;

    Rigidbody2D rb;
    Animator characterAnim;   // NPC sprite animator
    I_Controller controller;
    P_InputActions input;

    // runtime
    Vector2 facingDir; 
    Transform target;

    void Awake()
    {
        rb            ??= GetComponent<Rigidbody2D>();
        characterAnim ??= GetComponentInChildren<Animator>();
        input         ??= new P_InputActions();

        controller      = (I_Controller)(GetComponent<E_Controller>() ??
                            (Component)GetComponent<NPC_Controller>());

        if (!rb) Debug.LogError($"{name}: Rigidbody2D is missing in State_Talk");
    }

    // When enabled, stop physics and movement, face the player, show talk icon
    void OnEnable()
    {
        input.Enable();
        // Stop movement and swallow any external forces
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Face a direction
        characterAnim?.SetFloat("idleX", facingDir.x);
        characterAnim?.SetFloat("idleY", facingDir.y);

        // Play animations
        characterAnim.Play("Idle");       // idle while talking
        interactAnim.Play("WantToTalk");  // open talk icon
    }

    // When disabled, restore physics and movement, hide talk icon
    void OnDisable()
    {
        input.Disable();
        target = null;
        rb.bodyType = RigidbodyType2D.Dynamic;  // restore normal physics
        interactAnim?.Play("Idle");        // close talk icon
        facingDir = Vector2.zero;           // clear after use
    }

    void Update()
    {
        // NPCs don’t move while talking
        controller.SetDesiredVelocity(Vector2.zero);

        if (!target) return;

        Vector2 to = (Vector2)target.position - (Vector2)transform.position;

        if (to.sqrMagnitude > 0.0001f)
        {
            facingDir = to.normalized;
            characterAnim.SetFloat("idleX", facingDir.x);
            characterAnim.SetFloat("idleY", facingDir.y);
        }

        var dm = SYS_GameManager.Instance.d_Manager; // after D below
        if (input.UI.EndConvo.WasPressedThisFrame())
        {
            if (dm.isDialogActive)
                dm.EndDialog();
            return;
        }

        // Interact (F) to advance / start
        if (input.Player.Interact.WasPressedThisFrame())
        {
            if (dm.isDialogActive)
            {
                dm.AdvanceDialog();
            }
            else
            {
                CheckForNewDialog();
                dm.StartDialog(currentDialog);
            }
        }

    }

    void CheckForNewDialog()
    {
        // scanning from start (your current priority order)
        for (int i = 0; i < dialogs.Count; i++)
        {
            var dialog = dialogs[i];
            if (dialog != null && dialog.IsConditionMet())
            {
                // Promote to current
                currentDialog = dialog;

                // Remove stale dialogs this one obsoletes
                if (dialog.removeTheseOnPlay != null && dialog.removeTheseOnPlay.Count > 0)
                {
                    for (int r = dialogs.Count - 1; r >= 0; r--)
                    {
                        if (dialogs[r] != null && dialog.removeTheseOnPlay.Contains(dialogs[r]))
                            dialogs.RemoveAt(r);
                    }
                }

                // Remove this dialog only if one-time
                if (dialog.removeAfterPlay)
                {
                    // find current index again safely (list may have shifted)
                    int idx = dialogs.IndexOf(dialog);
                    if (idx >= 0) dialogs.RemoveAt(idx);
                }

                break; // stop at first valid
            }
        }
    }

    // API for controller
    public void SetTarget(Transform t)
    {
        target = t;
        if (!target) return;
        var to = (Vector2)target.position - (Vector2)transform.position;
        if (to.sqrMagnitude > 0.0001f) facingDir = to.normalized;
    }
}
