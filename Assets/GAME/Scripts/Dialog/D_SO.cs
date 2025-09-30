using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSO", menuName = "Dialogue/DialogueNode")]
public class D_SO : ScriptableObject
{
    public D_LineSO[] lines;
}

[System.Serializable]
public class D_LineSO
{
    public D_ActorSO speaker;

    [TextArea(3, 5)]
    public string text;
}
