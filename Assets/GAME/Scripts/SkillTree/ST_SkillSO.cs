using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "SkillTree")]
public class ST_SkillSO : ScriptableObject
{
    public enum Kind { Stat, Lifesteal }
    public enum Stat { AD, AP, MS, MaxHP, AR, MR, KR }

    [Header("Meta")]
    public string skillName = "Auto Filled";
    public Sprite skillIcon;
    [Min(1)] public int maxLevel = 1;

    [Header("Effect")]
    public Kind kind = Kind.Stat;

    [Header("Only Used when Kind = Stat")]
    public Stat stat = Stat.AD;

    [Header("Added to the chosen stat per level")]
    public int pointPerLv = 1;

    [Header("ONLY Used when Kind = Lifesteal (total % at current level)")]
    [Range(0f, 1f)] public float lifestealPercentPerLevel = 0f;

    private void OnValidate()
    {
        if (skillName != name)
            skillName = name;
    }
}
