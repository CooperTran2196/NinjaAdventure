using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class W_Melee : W_Base
{
    Vector2 attackDir;
    int currentComboIndex = 0; // Track which combo attack this is

    // Track hits to avoid multiple hits on same target during one attack
    readonly HashSet<int> alreadyHit = new HashSet<int>();

    // Player attack (reads combo index from P_State_Attack)
    public override void Attack(Vector2 aimDir)
    {
        attackDir = aimDir.normalized;
        
        // Read combo index from player's attack state
        var playerAttackState = owner?.GetComponent<P_State_Attack>();
        if (playerAttackState != null)
            currentComboIndex = playerAttackState.GetComboIndex();
        else
            currentComboIndex = 0;
        
        // Play combo slash sound for player
        SYS_GameManager.Instance.sys_SoundManager.PlayComboSlash(currentComboIndex);
        
        StartCoroutine(Hit());
    }

    // Enemy attack (directly sets combo index)
    public void AttackAsEnemy(Vector2 aimDir, int comboAttackIndex)
    {
        attackDir = aimDir.normalized;
        currentComboIndex = Mathf.Clamp(comboAttackIndex, 0, 2);
        
        // Play combo slash sound for enemy (quieter)
        SYS_GameManager.Instance.sys_SoundManager.PlayComboSlash_Enemy(currentComboIndex);
        
        StartCoroutine(Hit());
    }

    // Returns attack angles based on combo index: 0=SlashDown, 1=SlashUp, 2=Thrust
    (float startAngle, float endAngle, bool isThrust) GetComboPattern(float baseAngle, int index)
    {
        if (index == 2) return (0, 0, true);  // Thrust
        
        // Apply slash arc bonus from player stats
        float finalArcDegrees = weaponData.slashArcDegrees + c_Stats.slashArcBonus;
        float halfArc = finalArcDegrees * 0.5f;
        bool reverseArc = (index == 1);  // SlashUp reverses arc direction
        
        float startAngle = baseAngle + (reverseArc ? halfArc : -halfArc);
        float endAngle = baseAngle + (reverseArc ? -halfArc : halfArc);
        
        return (startAngle, endAngle, false);  // Arc slash
    }

    // Execute attack: thrust forward OR arc slash based on combo index
    IEnumerator Hit()
    {
        alreadyHit.Clear();

        float showTime = weaponData.comboShowTimes[currentComboIndex];
        
        // Angle from attack direction (UP=0°, negated X fixes left/right)
        float baseAngle = Mathf.Atan2(-attackDir.x, attackDir.y) * Mathf.Rad2Deg;
        
        var pattern = GetComboPattern(baseAngle, currentComboIndex);
        
        if (pattern.isThrust)
        {
            // Forward thrust with distance bonus (1 = 1% increase)
            Vector3 localPosition = GetPolarPosition(attackDir);
            float thrustAngle = GetPolarAngle(attackDir);
            float finalThrustDistance = weaponData.thrustDistance * (1f + c_Stats.thrustDistanceBonus / 100f);
            
            BeginVisual(localPosition, thrustAngle, enableHitbox: true);
            yield return ThrustOverTime(attackDir, showTime, finalThrustDistance);
        }
        else
        {
            // Arc slash (ArcSlashOverTime sets position on first frame)
            BeginVisual(Vector3.zero, pattern.startAngle, enableHitbox: true);
            yield return ArcSlashOverTime(attackDir, pattern.startAngle, pattern.endAngle, showTime);
        }

        EndVisual();
    }

    // Trigger contact → apply combo damage/stun/knockback
    void OnTriggerStay2D(Collider2D targetCollider)
    {
        var (targetHealth, root) = TryGetTarget(targetCollider);
        if (!targetHealth) return;
        
        // Prevent double-hit on same target
        if (!alreadyHit.Add(root.GetInstanceID())) return;
        
        // Player: also check attack state tracking
        var playerAttackState = owner?.GetComponent<P_State_Attack>();
        if (playerAttackState != null)
        {
            if (playerAttackState.WasTargetHitThisAttack(targetHealth)) return;
            playerAttackState.MarkTargetHit(targetHealth);
        }
        
        // Apply combo effects (damage/stun/knockback scale with currentComboIndex)
        ApplyHitEffects(c_Stats, weaponData, targetHealth, attackDir, targetCollider, currentComboIndex);
    }
}
