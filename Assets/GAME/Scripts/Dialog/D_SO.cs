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

    public bool IsConditionMet()
    {
        if (requiredNPCs != null && requiredNPCs.Length > 0)
        {
            foreach (var npc in requiredNPCs)
            {
                if (!D_HistoryTracker.Instance.HasSpokenWith(npc))
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
