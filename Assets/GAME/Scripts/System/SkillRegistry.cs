using System.Collections.Generic;
using UnityEngine;

public static class SkillRegistry
{
    private static Dictionary<string, ST_SkillSO> byId;

    public static void Init()
    {
        if (byId != null) return;

        byId = new Dictionary<string, ST_SkillSO>();
        var all = Resources.LoadAll<ST_SkillSO>("Skills"); // Assets/Resources/Skills/**
        foreach (var so in all)
        {
            if (!so) continue;
            if (string.IsNullOrEmpty(so.id))
            {
                Debug.LogWarning($"[SkillRegistry] Skill '{so.name}' has empty id. Set it for save/load.");
                continue;
            }
            if (byId.ContainsKey(so.id))
            {
                Debug.LogWarning($"[SkillRegistry] Duplicate skill id '{so.id}'. First wins.");
                continue;
            }
            byId[so.id] = so;
        }
    }

    public static ST_SkillSO Get(string id)
    {
        if (byId == null) Init();
        if (string.IsNullOrEmpty(id)) return null;
        byId.TryGetValue(id, out var so);
        return so;
    }
}
