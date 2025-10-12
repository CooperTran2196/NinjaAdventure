using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class W_Melee : W_Base
{
    Vector2 attackDir;
    int currentComboIndex = 0; // Track which combo attack this is

    // Track hits to avoid multiple hits on same target during one attack
    readonly HashSet<int> alreadyHit = new HashSet<int>();

    // Override attack method
    public override void Attack(Vector2 aimDir)
    {
        attackDir = aimDir.normalized;
        
        // Get combo index from owner's attack state (if player)
        var playerAttackState = owner?.GetComponent<P_State_Attack>();
        if (playerAttackState != null)
        {
            currentComboIndex = playerAttackState.GetComboIndex();
        }
        else
        {
            currentComboIndex = 0; // Default for non-player entities
        }
        
        StartCoroutine(Hit());
    }

    // Coroutine for handling the hit process with combo patterns
    IEnumerator Hit()
    {
        // Clear hit tracker for this attack
        alreadyHit.Clear();

        // Get combo-specific showTime
        float showTime = weaponData.comboShowTimes[currentComboIndex];
        
        // Calculate base angle for the attack direction
        // Using UP as 0°, RIGHT as 90°: angle = atan2(x, y) in degrees
        // Negate X to fix left/right reversal (Unity's coordinate system)
        float baseAngle = Mathf.Atan2(-attackDir.x, attackDir.y) * Mathf.Rad2Deg;
        
        // Different movement pattern based on combo index
        switch (currentComboIndex)
        {
            case 0: // Slash Down (arc sweeps downward)
                {
                    float halfArc = weaponData.slashArcDegrees * 0.5f;
                    float startAngle = baseAngle - halfArc;  // Start counter-clockwise
                    float endAngle = baseAngle + halfArc;    // End clockwise
                    
                    // Calculate initial position for visual start (radar arm at start angle)
                    // IMPORTANT: Negate X to match the position calculation in ArcSlashOverTime
                    float startAngleRad = startAngle * Mathf.Deg2Rad;
                    Vector3 startPos = new Vector3(
                        -Mathf.Sin(startAngleRad) * weaponData.offsetRadius,  // Negate X
                        Mathf.Cos(startAngleRad) * weaponData.offsetRadius,
                        0f
                    );
                    
                    BeginVisual(startPos, startAngle, enableHitbox: true);
                    yield return ArcSlashOverTime(attackDir, startAngle, endAngle, showTime);
                }
                break;
                
            case 1: // Slash Up (arc sweeps upward - reversed direction)
                {
                    float halfArc = weaponData.slashArcDegrees * 0.5f;
                    float startAngle = baseAngle + halfArc;  // Start clockwise
                    float endAngle = baseAngle - halfArc;    // End counter-clockwise
                    
                    // Calculate initial position for visual start (radar arm at start angle)
                    // IMPORTANT: Negate X to match the position calculation in ArcSlashOverTime
                    float startAngleRad = startAngle * Mathf.Deg2Rad;
                    Vector3 startPos = new Vector3(
                        -Mathf.Sin(startAngleRad) * weaponData.offsetRadius,  // Negate X
                        Mathf.Cos(startAngleRad) * weaponData.offsetRadius,
                        0f
                    );
                    
                    BeginVisual(startPos, startAngle, enableHitbox: true);
                    yield return ArcSlashOverTime(attackDir, startAngle, endAngle, showTime);
                }
                break;
                
            case 2: // Thrust Finisher (forward thrust)
            default:
                {
                    Vector3 localPosition = GetPolarPosition(attackDir);
                    float thrustAngle = GetPolarAngle(attackDir);
                    
                    BeginVisual(localPosition, thrustAngle, enableHitbox: true);
                    yield return ThrustOverTime(attackDir, showTime, weaponData.thrustDistance);
                }
                break;
        }

        // End visuals and restore parent
        EndVisual();
    }

    // Hit detection with combo support
    void OnTriggerStay2D(Collider2D targetCollider)
    {
        // Check if the collider is a valid target
        var (targetHealth, root) = TryGetTarget(targetCollider);
        if (!targetHealth) return;
        
        // Check if already hit this target in this specific attack
        if (!alreadyHit.Add(root.GetInstanceID())) return;
        
        // For player attacks, also check attack state hit tracking
        var playerAttackState = owner?.GetComponent<P_State_Attack>();
        if (playerAttackState != null)
        {
            if (playerAttackState.WasTargetHitThisAttack(targetHealth)) return;
            playerAttackState.MarkTargetHit(targetHealth);
        }
        
        // Apply hit effects with combo index for damage/stun multipliers
        ApplyHitEffects(c_Stats, weaponData, targetHealth, attackDir, targetCollider, currentComboIndex);
    }
}
