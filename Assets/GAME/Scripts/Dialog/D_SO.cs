// <summary>
// Used to define a dialogue node in the dialogue system.
// </summary>

using Unity.AppUI.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSO", menuName = "Dialogue/DialogueNode")]
public class D_SO : ScriptableObject
{
    public D_LineSO[] lines;
    public D_Option[] options;

    [Header("Conditional Requirements (OPTIONAL)")]
    [Header("Must have spoken to these NPCs to see this dialogue")]
    public D_ActorSO[] requiredNPCs;

    [Header("Must have visited these locations to see this dialogue")]
    public D_LocationSO[] requiredLocations;

    // Try to prove FALSE by condition checks, else return true
    public bool IsConditionMet()
    {
        // 1/ NPC gate
        if (requiredNPCs.Length > 0)
        {
            foreach (var npc in requiredNPCs)
            {
                if (!D_HistoryTracker.Instance.HasSpokenWith(npc))
                    return false;
            }
        }

        // 2/ LOCATION gate
        if (requiredLocations.Length > 0)
        {
            foreach (var location in requiredLocations)
            {
                if (!D_LocationHistoryTracker.Instance.HasVisited(location))
                    return false;
            }
        }

        return true;
    }
}

[System.Serializable]
public class D_LineSO
{
    public D_ActorSO speaker;

    [TextArea(3, 5)]
    public string text;
}

[System.Serializable]
public class D_Option
{
    public string optionText;
    public D_SO nextDialogue;
}
