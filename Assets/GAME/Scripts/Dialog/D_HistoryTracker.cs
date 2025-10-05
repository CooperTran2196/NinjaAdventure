// <summary>
// Tracks NPCs the player has spoken to in the dialogue system.
// </summary>

using UnityEngine;
using System.Collections.Generic;

public class D_HistoryTracker : MonoBehaviour
{
    // Using HashSet to avoid duplicate entries/ Don't care about order
    public readonly HashSet<D_ActorSO> spokenNPCs = new();

    // Add the NPC to spkenNPCs if not already present
    public void RecordNPC(D_ActorSO actorSO)
    {
        if (spokenNPCs.Add(actorSO))
        {
            Debug.Log($"Spoken with: {actorSO.characterName}");
        }
    }

    // Check if we've spoken to this NPC before
    public bool HasSpokenWith(D_ActorSO actorSO)
    {
        return spokenNPCs.Contains(actorSO);
    }
}
