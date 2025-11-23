using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "SkillTree")]
public class ST_SkillSO : ScriptableObject
{
    [Header("Meta")]
    public string id;
    public string skillName = "Auto Filled";
    public Sprite skillIcon;

    [Min(1)] public int maxLevel = 1;

    [Header("Effects Per Level")]
    public List<P_StatEffect> StatEffectList;

    void OnValidate()
    {
        if (skillName != name)
            skillName = name;
    }
}