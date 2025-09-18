// using UnityEngine;
// using System;
// using System.Collections.Generic; // Required for List

// [DisallowMultipleComponent]
// public class P_StatsChanged : MonoBehaviour
// {
//     [Header("Independent component to manage Player's stats changed by Skill Tree")]
//     [Header("References")]
//     public C_Stats c_Stats;
//     C_Health c_Health;

//     // Baseline snapshot
//     int baseAD, baseAP, baseMaxHP, baseAR, baseMR;
//     float baseMS, baseKR;

//     // Upgrades
//     int upAD, upAP, upHP, upAR, upMR;
//     float upMS, upKR;

//     // Lifesteal
//     public bool lifestealUnlocked = false;
//     [Range(0f, 1f)] public float lifestealPercent = 0f;

//     public event Action OnStatsRecalculated;

//     void Awake()
//     {
//         c_Stats ??= GetComponent<C_Stats>();
//         c_Health ??= GetComponent<C_Health>();

//         if (!c_Stats) Debug.LogError($"{name}: C_Stats missing in P_Skills.", this);
//         if (!c_Health) Debug.LogError($"{name}: C_Health missing in P_Skills.", this);

//         baseAD = c_Stats.AD;
//         baseAP = c_Stats.AP;
//         baseMaxHP = c_Stats.maxHP;
//         baseAR = c_Stats.AR;
//         baseMR = c_Stats.MR;
//         baseMS = c_Stats.MS;
//         baseKR = c_Stats.KR;

//         Recalculate();
//     }

//     // Called by SkillManager when a node upgrades
//     public void ApplyStat(ST_SkillSO.Stat stat, int delta)
//     {
//         switch (stat)
//         {
//             case ST_SkillSO.Stat.AD: upAD += delta; break;
//             case ST_SkillSO.Stat.AP: upAP += delta; break;
//             case ST_SkillSO.Stat.MaxHP: upHP += delta; break;
//             case ST_SkillSO.Stat.AR: upAR += delta; break;
//             case ST_SkillSO.Stat.MR: upMR += delta; break;
//             case ST_SkillSO.Stat.MS: upMS += delta; break;
//             case ST_SkillSO.Stat.KR: upKR += delta; break;
//         }
//         Recalculate();
//     }

//     public void SetLifestealPercent(float percent)
//     {
//         lifestealUnlocked = percent > 0f;
//         lifestealPercent = percent;
//     }

//     void Recalculate()
//     {
//         // Recalculate all stats based on base values + upgrades
//         c_Stats.AD = baseAD + upAD;
//         c_Stats.AP = baseAP + upAP;
//         c_Stats.maxHP = baseMaxHP + upHP;
//         c_Stats.AR = baseAR + upAR;
//         c_Stats.MR = baseMR + upMR;
//         c_Stats.MS = baseMS + upMS;
//         c_Stats.KR = baseKR + upKR;

//         // Announce that stats have been updated
//         OnStatsRecalculated?.Invoke();
//     }

//     // Heal when the PLAYER deals final damage (wire from weapon/hit pipeline)
//     public void OnDealtDamage(int finalDamage)
//     {
//         if (!lifestealUnlocked || finalDamage <= 0) return;
//         int heal = Mathf.RoundToInt(finalDamage * lifestealPercent);
//         if (heal > 0) c_Health.ChangeHealth(heal);
//     }
// }
