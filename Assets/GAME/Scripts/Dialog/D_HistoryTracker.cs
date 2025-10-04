using UnityEngine;
using System.Collections.Generic;

public class D_HistoryTracker : MonoBehaviour
{
    public static D_HistoryTracker Instance { get; private set; }

    // Who we've spoken to (unique ActorSOs)
    public readonly List<D_ActorSO> spokenNPCs = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RecordNPC(D_ActorSO actor)
    {
        // spkenNPC.Add(actor);
        if (actor == null) return;
        if (!spokenNPCs.Contains(actor)) spokenNPCs.Add(actor);
        Debug.Log($"Spoken with: {actor.actorName}");
    }

    public bool HasSpokenWith(D_ActorSO actor)
    {
        // return spkenNPC.Contains(actor);
        return actor != null && spokenNPCs.Contains(actor);
    }
}
