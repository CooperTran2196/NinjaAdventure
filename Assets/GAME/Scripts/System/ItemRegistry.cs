using System.Collections.Generic;
using UnityEngine;

public static class ItemRegistry
{
    private static Dictionary<string, INV_ItemSO> byId;

    public static void Init()
    {
        if (byId != null) return;

        byId = new Dictionary<string, INV_ItemSO>();
        var all = Resources.LoadAll<INV_ItemSO>("Items"); // Assets/Resources/Items/**
        foreach (var so in all)
        {
            if (!so) continue;
            if (string.IsNullOrEmpty(so.id))
            {
                Debug.LogWarning($"[ItemRegistry] Item '{so.name}' has empty id. Set it for save/load.");
                continue;
            }
            if (byId.ContainsKey(so.id))
            {
                Debug.LogWarning($"[ItemRegistry] Duplicate item id '{so.id}'. First wins.");
                continue;
            }
            byId[so.id] = so;
        }
    }

    public static INV_ItemSO Get(string id)
    {
        if (byId == null) Init();
        if (string.IsNullOrEmpty(id)) return null;
        byId.TryGetValue(id, out var so);
        return so;
    }
}
