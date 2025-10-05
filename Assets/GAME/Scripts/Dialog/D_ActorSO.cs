// <summary>
// Used to define an actor (NPC) in the dialogue system.
// </summary>

using UnityEngine;

[CreateAssetMenu(fileName = "ActorSO", menuName = "Dialogue/ActorSO")]
public class D_ActorSO : ScriptableObject
{
    public string characterName;
    public Sprite avatar;
}
