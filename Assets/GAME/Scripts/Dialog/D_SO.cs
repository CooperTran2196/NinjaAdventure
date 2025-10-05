// <summary>
// Used to define a dialog node in the dialog system.
// </summary>

using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogSO", menuName = "Dialog/DialogNode")]
public class D_SO : ScriptableObject
{
    [Header("Lines of dialog in this dialog")]
    public D_LineSO[] lines;
    [Header("An option button at the end of a dialog")]
    public D_Option[] optionList;

    [Header("Conditional Requirements (OPTIONAL)")]
    [Header("Must have spoken to these NPCS to see this dialog")]
    public D_ActorSO[] requiredNPCs;

    [Header("Must have visited these LOCATIONS to see this dialog")]
    public D_LocationSO[] requiredLocations;

    [Header("Must have these ITEMS to see this dialog")]
    public INV_ItemSO[] requiredItems;

    [Header("Control Flags")]
    [Header("If TRUE, remove THIS DIALOG from the NPCs list")]
    [Header("ex: Quest dialogs, one-time use")]
    public bool removeAfterPlay;
    [Header("Also remove THESE DIALOGS from the NPCs list")]
    [Header("ex: Dialogs before quest")]
    public List<D_SO> removeTheseOnPlay = new();

    // Try to prove FALSE by condition checks, else return true
    public bool IsConditionMet()
    {
        // 1/ NPC gate
        if (requiredNPCs.Length > 0)
        {
            foreach (var npc in requiredNPCs)
            {
                if (!SYS_GameManager.Instance.d_HistoryTracker.HasSpokenWith(npc))
                    return false;
            }
        }

        // 2/ LOCATION gate
        if (requiredLocations.Length > 0)
        {
            foreach (var location in requiredLocations)
            {
                if (!SYS_GameManager.Instance.d_LocationHistoryTracker.HasVisited(location))
                    return false;
            }
        }

        // 3/ Items
        if (requiredItems.Length > 0)
        {
            foreach (var itemSO in requiredItems)
                if (!INV_Manager.Instance.HasItem(itemSO))
                    return false;
        }
        return true;
    }
}

// <summary>
// A single line of dialog spoken by an actor.
// Contains a D_ActorSO and the text they speak.
// </summary>
[System.Serializable]
public class D_LineSO
{
    public D_ActorSO speaker;

    [TextArea(3, 5)]
    public string text;
}

// <summary>
// An option presented to the player at the end of a dialog.
// Selecting this option leads to another dialog node (nextDialog).
// </summary>
[System.Serializable]
public class D_Option
{
    public string optionButtonText;
    public D_SO nextDialog;
}
