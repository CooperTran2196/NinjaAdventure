using UnityEngine;
using System.Collections.Generic; // Required for List

[CreateAssetMenu(fileName = "NewSkill", menuName = "SkillTree")]
public class ST_SkillSO : ScriptableObject
{
    [Header("Meta")]
    public string skillName = "Auto Filled";
    public string id;

    public Sprite skillIcon;
    [Min(1)] public int maxLevel = 1;

    [Header("Effects Per Level")]
    public List<P_StatEffect> StatEffectList;

    private void OnValidate()
    {
        if (skillName != name)
            skillName = name;
    }
}