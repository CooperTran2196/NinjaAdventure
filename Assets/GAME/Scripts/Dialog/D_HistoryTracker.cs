// <summary>
// Tracks NPCs the player has spoken to in the dialogue system.
// </summary>

using UnityEngine;
using System.Collections.Generic;

public class D_HistoryTracker : MonoBehaviour
{
    public static D_HistoryTracker Instance;

    // Using HashSet to avoid duplicate entries/ Don't care about order
    public readonly HashSet<D_ActorSO> spokenNPCs = new();

    // Singleton pattern
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Add the NPC to spkenNPCs if not already present
    public void RecordNPC(D_ActorSO actorSO)
    {
        if (spokenNPCs.Add(actorSO))
        {
            Debug.Log($"Spoken with: {actorSO.actorName}");
        }
    }

    // Check if we've spoken to this NPC before
    public bool HasSpokenWith(D_ActorSO actorSO)
    {
        return spokenNPCs.Contains(actorSO);
    }
}
