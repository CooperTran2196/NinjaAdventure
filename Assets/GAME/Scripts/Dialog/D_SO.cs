using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSO", menuName = "Dialogue/DialogueNode")]
public class D_SO : ScriptableObject
{
    public D_LineSO[] lines;
    public D_Option[] options;  // NEW
}

[System.Serializable]
public class D_LineSO
{
    public D_ActorSO speaker;

    [TextArea(3, 5)]
    public string text;
}

[System.Serializable]
public class D_Option   // NEW
{
    public string optionText;
    public D_SO nextDialogue;   // load this conversation when chosen (can be null to End)
}
