# Enemy State System - Player Architecture Applied

## Overview
Refactored the enemy AI system to mirror the **player's state management architecture**, bringing all the same benefits: concurrent states, weapon movement penalty, showTime animation locking, and automatic state restoration.

## Changes Applied

### 1. **State_Attack.cs** - Complete Refactor

#### Added Features:
✅ **ShowTime Animation Lock** (like player)
- Animation freezes at final frame if `weaponShowTime > attackAnimDuration`
- `anim.speed = 0` during lockout period
- Smooth restoration of `anim.speed = 1` in `OnDisable()`

✅ **Component Lifecycle Management**
- `enabled = false` at end of `AttackRoutine()`
- Triggers `OnDisable()` automatically for cleanup
- Restores animation speed and chase state

✅ **State Restoration on Attack End**
- If target still exists → re-enable `State_Chase`
- Automatic handling via `OnDisable()`

✅ **Weapon Getter for Movement Penalty**
- `GetActiveWeapon()` method for `State_Chase` to read weapon data
- Enables per-weapon movement speed during attack

#### Code Changes:
```csharp
// NEW: Variable renamed for clarity
float attackAnimDuration = 0.45f; // OLD: attackDuration

// NEW: Added references
State_Chase chaseState;

// NEW: ShowTime animation lock logic
IEnumerator AttackRoutine()
{
    // ... hit delay + attack ...
    
    // Phase 3: Wait for animation to complete
    float animRemaining = attackAnimDuration - hitDelay;
    yield return new WaitForSeconds(animRemaining);

    // Phase 4: If showTime > animation, freeze and wait
    if (weaponShowTime > attackAnimDuration)
    {
        anim.speed = 0f;  // Freeze animation
        float lockoutDuration = weaponShowTime - attackAnimDuration;
        yield return new WaitForSeconds(lockoutDuration);
        anim.speed = 1f;  // Restore
    }

    // Phase 5: Disable component
    controller.SetAttacking(false);
    enabled = false; // ✅ Triggers OnDisable()
}

// NEW: Enhanced OnDisable with state restoration
void OnDisable()
{
    StopAllCoroutines();
    anim.SetBool("isAttacking", false);
    controller.SetAttacking(false);
    
    // Restore animation speed
    anim.speed = 1f;

    // Restore chase if target still exists
    if (chaseState != null && controller.GetTarget() != null && !chaseState.enabled)
    {
        chaseState.enabled = true;
    }
}

// NEW: Weapon getter for movement penalty
public W_Base GetActiveWeapon() => activeWeapon;
```

---

### 2. **State_Chase.cs** - Movement Penalty Support

#### Added Features:
✅ **Per-Weapon Movement Penalty** (like player)
- Reads `attackMovePenalty` from active weapon during attack
- Speed reduction applied: `speed *= weapon.attackMovePenalty`
- Full speed when not attacking

✅ **Animation Priority System**
- Attack animation takes priority over movement
- `isMoving = false` during attack state
- Prevents animation conflicts

✅ **Concurrent State Support**
- Chase state stays enabled during attack
- Applies movement velocity even while attacking
- Smooth transition when attack ends

#### Code Changes:
```csharp
// NEW: Added attack state reference
State_Attack attackState;

void Awake()
{
    // ... other components ...
    attackState = GetComponent<State_Attack>();
}

// NEW: OnEnable checks for attack state
void OnEnable()
{
    if (controller.currentState != E_Controller.EState.Attack)
    {
        anim.SetBool("isMoving", true);
    }
}

// NEW: Apply weapon movement penalty
void Update()
{
    Vector2 moveAxis = ComputeChaseDir();
    float speed = c_Stats.MS;

    // Apply penalty if attacking
    if (controller.currentState == E_Controller.EState.Attack)
    {
        W_Base activeWeapon = attackState.GetActiveWeapon();
        if (activeWeapon != null && activeWeapon.weaponData != null)
        {
            speed *= activeWeapon.weaponData.attackMovePenalty;
        }
        
        // Don't override attack animation
        anim.SetBool("isMoving", false);
    }
    else
    {
        // Show movement animation if moving
        bool moving = moveAxis.sqrMagnitude > 0f;
        anim.SetBool("isMoving", moving);
    }

    controller.SetDesiredVelocity(moveAxis * speed);
    // ... animation parameters ...
}
```

---

### 3. **E_Controller.cs** - Concurrent States & State Restoration

#### Added Features:
✅ **TriggerAttack() Method** (like player)
- Triggers attack without disabling other states
- Enables attack component directly
- Sets state tracking variables

✅ **Concurrent Chase + Attack States**
- Attack state preserved during chase
- Chase provides movement during attack
- Both states run simultaneously

✅ **State Restoration Logic**
- `SetAttacking(false)` restores proper state
- Checks if target still exists → Chase or Default
- Automatic state management

✅ **Protected Attack State**
- `!isAttacking` guard in `SwitchState()`
- Attack component only disabled when safe
- Prevents premature attack cancellation

#### Code Changes:

**New TriggerAttack() Method:**
```csharp
void TriggerAttack()
{
    if (isAttacking) return; // Already attacking

    // Enable attack state component
    attack.enabled = true;
    
    // Update state tracking
    currentState = EState.Attack;
    isAttacking = true;
    attackCooldown = c_Stats.attackCooldown;
}
```

**Modified ProcessAI():**
```csharp
void ProcessAI()
{
    // ... death check ...
    
    // No target handling
    if (currentTarget == null)
    {
        // Don't switch away from attack if attacking
        if (currentState != defaultState && !isAttacking)
        {
            SwitchState(defaultState);
        }
        attackInRangeTimer = 0f;
        return;
    }

    // ... timer logic ...

    // Check if can attack
    bool canAttack = targetInAttackRange 
                  && attackInRangeTimer >= attackStartBuffer 
                  && attackCooldown <= 0f
                  && !isAttacking; // Don't interrupt active attack

    if (canAttack)
    {
        TriggerAttack(); // ✅ New method
    }
    else if (currentTarget != null && currentState != EState.Attack)
    {
        // Don't switch away from Attack if currently attacking
        SwitchState(EState.Chase);
    }
}
```

**Modified SwitchState():**
```csharp
public void SwitchState(EState state)
{
    if (currentState == state) return;
    currentState = state;

    // Disable all states first (except attack if active)
    idle.enabled = wander.enabled = chase.enabled = false;
    
    // Only disable attack if we're not currently attacking
    if (!isAttacking) attack.enabled = false;

    switch (state)
    {
        case EState.Dead:
            // ... death logic ...
            attack.enabled = false; // Force disable on death
            break;

        case EState.Attack:
            // Attack handled via TriggerAttack()
            // Empty case for state tracking
            break;

        // ... other states ...
    }
}
```

**Enhanced SetAttacking():**
```csharp
public void SetAttacking(bool value)
{
    isAttacking = value;

    // When attack finishes, restore appropriate state
    if (!value)
    {
        // Check if still has a target
        if (currentTarget != null)
        {
            // Return to chase state
            SwitchState(EState.Chase);
        }
        else
        {
            // No target, return to default behavior
            SwitchState(defaultState);
        }
    }
}
```

---

## State Behavior Matrix

### Enemy State Component Status

| Situation | Idle | Wander | Chase | Attack | Current State | isAttacking |
|-----------|------|--------|-------|--------|---------------|-------------|
| No target (default=Idle) | ✅ | ❌ | ❌ | ❌ | Idle | false |
| No target (default=Wander) | ❌ | ✅ | ❌ | ❌ | Wander | false |
| Target detected | ❌ | ❌ | ✅ | ❌ | Chase | false |
| **Attack triggered** | ❌ | ❌ | ✅ | ✅ | Attack | true |
| **Attack ends (target exists)** | ❌ | ❌ | ✅ | ❌ | Chase | false |
| **Attack ends (no target)** | varies | varies | ❌ | ❌ | default | false |
| Dead | ❌ | ❌ | ❌ | ❌ | Dead | false |

### Animation State During Attack + Chase

| Phase | isAttacking | isMoving | Animation Shown |
|-------|-------------|----------|-----------------|
| Chasing only | false | true | Walk/Run |
| Attack starts | true | **false** | **Attack** |
| Attack + moving | true | **false** | **Attack** (priority) |
| Attack ends → still chasing | false | true | Walk/Run |
| Attack ends → no target | false | false | Idle |

---

## Complete Flow Examples

### Scenario 1: Attack While Chasing → Attack Ends → Keep Chasing

```
Frame 1-10: Chasing
  → currentState = Chase
  → chase.enabled = true
  → attack.enabled = false
  → isAttacking = false
  → Moving at full speed

Frame 11: Enter attack range
  → TriggerAttack()
  → currentState = Attack
  → chase.enabled = true (stays enabled!)
  → attack.enabled = true
  → isAttacking = true

Frame 12-40: Attack + Chase concurrent
  → Both components enabled
  → Chase provides movement (with penalty)
  → Attack plays animation (priority)
  → Moving at reduced speed (weapon penalty)

Frame 41: Attack finishes
  → AttackRoutine() completes
  → controller.SetAttacking(false)
  → enabled = false → State_Attack.OnDisable()
  → attack.enabled = false
  
Frame 42: SetAttacking(false) called
  → currentTarget exists
  → SwitchState(EState.Chase)
  → chase already enabled (no change)

Frame 43+: Back to chasing
  → currentState = Chase
  → chase.enabled = true
  → attack.enabled = false
  → Full speed restored
  → Walk animation resumes
```

### Scenario 2: Attack Ends → Target Lost → Return to Default

```
Frame 1-30: Attacking

Frame 31: Attack ends
  → AttackRoutine() completes
  → controller.SetAttacking(false)
  → enabled = false

Frame 32: SetAttacking(false) called
  → currentTarget = null (lost target)
  → SwitchState(defaultState) // e.g., Idle or Wander

Frame 33+: Default behavior
  → currentState = Idle/Wander
  → attack.enabled = false
  → chase.enabled = false
  → Idle or Wander animation
```

### Scenario 3: ShowTime Lock Example (Slow Heavy Weapon)

```
Weapon: Heavy Axe
  showTime = 1.5f
  attackAnimDuration = 0.45f
  attackMovePenalty = 0.2f (20% speed)

Frame 1: Attack triggered
  → Animation starts
  → Moving at 20% speed

Frame 0.15s: Hit delay complete
  → activeWeapon.Attack(dir)
  → Weapon visuals appear

Frame 0.45s: Animation reaches final frame
  → anim.speed = 0 (freeze!)
  → Still moving at 20% speed
  → Weapon still visible

Frame 0.45s - 1.5s: Lockout period (1.05s)
  → Animation frozen on last frame
  → Still moving at 20% speed
  → Player can reposition but slow

Frame 1.5s: ShowTime complete
  → anim.speed = 1 (restore)
  → controller.SetAttacking(false)
  → enabled = false
  
Frame 1.51s+: Chase resumes
  → Full speed restored
  → Walk animation plays
```

---

## Key Benefits

### ✅ **Weapon Variety**
- Fast weapons (daggers): Low penalty (0.7-0.8), quick attacks, mobile
- Slow weapons (greatswords): High penalty (0.2-0.3), long lockout, powerful

### ✅ **Tactical Depth**
- Enemies with heavy weapons are slow and predictable
- Enemies with light weapons are fast and aggressive
- Player can exploit movement patterns

### ✅ **Consistent Architecture**
- Player and Enemy use same state patterns
- Same weapon behavior (anchoring, penalty, showTime)
- Easier to maintain and extend

### ✅ **Smooth Combat**
- Enemies can reposition during attack (slowly)
- No "stuck in place" feeling
- More dynamic combat encounters

### ✅ **State Restoration**
- Automatic return to chase or default
- No manual state management needed
- Clean transitions

---

## Compatibility Notes

### ✅ **State_Idle** - Still Works
- Used by NPCs and enemies (default state)
- No changes needed
- Works with `E_Controller` and `NPC_Controller`

### ✅ **State_Wander** - Still Works
- Used by NPCs and some enemies
- No changes needed
- Works with `E_Controller` and `NPC_Controller`

### ✅ **State_Talk** - Still Works (NPCs Only)
- Used exclusively by `NPC_Controller`
- No changes needed
- Not affected by enemy refactor

### ✅ **NPC_Controller** - Not Affected
- Still uses `State_Idle`, `State_Wander`, `State_Talk`
- No attack logic, so no conflicts
- Continues to work as before

---

## Testing Checklist

Enemy Combat:
- [ ] Enemy attacks while standing still (should work as before)
- [ ] Enemy attacks while chasing (should move at reduced speed)
- [ ] Enemy with fast weapon (showTime < 0.45s) - quick attacks
- [ ] Enemy with slow weapon (showTime > 0.45s) - animation freeze + lockout
- [ ] Attack ends while target exists → resume chase
- [ ] Attack ends while target lost → return to idle/wander
- [ ] Attack interrupted by death → proper cleanup

Movement Penalty:
- [ ] Light weapon enemy (0.7-0.8 penalty) - mobile during attack
- [ ] Heavy weapon enemy (0.2-0.3 penalty) - sluggish during attack
- [ ] Full speed after attack completes

Animation:
- [ ] Attack animation has priority over movement
- [ ] Movement animation resumes after attack
- [ ] No stuck animations on death
- [ ] ShowTime freeze works correctly

State Management:
- [ ] Chase + Attack concurrent states work
- [ ] State restoration after attack
- [ ] Idle/Wander states still work for NPCs
- [ ] Talk state still works for NPCs

---

## Configuration Examples

### Fast Assassin Enemy
```
Weapon: Dual Daggers
  AD = 3
  showTime = 0.3f
  attackMovePenalty = 0.75f
  thrustDistance = 0.2f

Result:
  → Quick attacks (no freeze)
  → Stays mobile (75% speed)
  → Hit-and-run combat style
```

### Balanced Knight Enemy
```
Weapon: Sword
  AD = 5
  showTime = 0.5f
  attackMovePenalty = 0.5f
  thrustDistance = 0.3f

Result:
  → Standard attacks (small freeze)
  → Moderate mobility (50% speed)
  → Balanced threat
```

### Heavy Brute Enemy
```
Weapon: Greatsword
  AD = 15
  showTime = 1.5f
  attackMovePenalty = 0.2f
  thrustDistance = 0.4f

Result:
  → Slow devastating attacks (long freeze)
  → Almost immobile (20% speed)
  → High risk/high reward for player
  → Telegraphed attacks, easy to dodge
```

---

## Future Enhancements

- [ ] Different AI personalities (aggressive vs cautious)
- [ ] Attack combos (sequential attacks)
- [ ] Special attacks (charged, area-of-effect)
- [ ] Movement patterns during attack (circle strafe, backpedal)
- [ ] Group tactics (one attacks, others flank)

---

**Last Updated:** October 11, 2025  
**Status:** Fully Implemented ✅  
**Compatibility:** Player system patterns fully applied
