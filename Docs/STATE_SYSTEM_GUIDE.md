# 🎮 State & Animation System Guide

Complete guide to player state management, concurrent states, transitions, and animation priority.

---

## Overview

The state system enables:
1. **Concurrent States** - Attack + Movement work together
2. **Clean Transitions** - Auto-restore proper state after actions
3. **Animation Priority** - Attack always overrides movement
4. **State Cleanup** - Proper disable when actions finish

---

## 1. Concurrent States (Attack + Movement)

### Problem:
Originally, Attack state disabled when player tried to move, causing:
- Player locked in place during attacks
- Movement input ignored
- Poor game feel

### Solution:
**Allow Attack and Movement states to coexist**.

### Implementation:

**P_Controller.cs:**
```csharp
void ProcessInputs()
{
    // ... other checks ...
    
    // MOVEMENT - Can happen during attack!
    Vector2 moveInput = playerInput.Player.Move.ReadValue<Vector2>();
    bool wantsToMove = moveInput.sqrMagnitude > 0.01f;
    
    if (wantsToMove)
    {
        // Don't disable attack state - just enable movement
        if (!isMoving)
        {
            SwitchState(PState.Movement, enableConcurrent: true);
        }
    }
    else if (isMoving && !isAttacking)
    {
        // Only go to idle if not attacking
        SwitchState(PState.Idle);
    }
    
    // ATTACK - Can happen during movement!
    if (attackInput)
    {
        if (isAttacking)
        {
            attack.QueueComboInput();  // Queue next combo
        }
        else
        {
            TriggerAttack();  // Start attack (movement stays enabled)
        }
    }
}
```

**Key Change:**
```csharp
// OLD (wrong):
SwitchState(PState.Movement);  // Would disable attack

// NEW (correct):
SwitchState(PState.Movement, enableConcurrent: true);  // Keeps attack enabled
```

### Benefits:
- ✅ Move during attacks (with penalty)
- ✅ Better game feel
- ✅ Strategic positioning
- ✅ Combo flow not interrupted

---

## 2. State Transitions

### Problem:
After attack/dodge finished, states didn't transition properly:
- Could get stuck in Idle when should be Moving
- Attack state stayed enabled even when not attacking

### Solution:
**Auto-restore proper state based on current input**.

### Implementation:

**P_State_Attack.cs:**
```csharp
IEnumerator AttackRoutine()
{
    // ... perform attack ...
    
    // Attack finished - restore appropriate state
    yield return null;  // Wait one frame
    
    // Let controller handle state restoration based on current input
    // If player is holding move input, controller will enable Movement
    // If player is idle, controller will enable Idle
}

void OnDisable()
{
    // Clean up when attack ends
    ResetCombo();
    
    // Stop all coroutines
    StopAllCoroutines();
}
```

**P_Controller.cs:**
```csharp
void Update()
{
    // Always check input and restore correct state
    ProcessInputs();
}
```

### State Flow:

```
Attack starts:
  Attack.enabled = true
  Movement.enabled = true (if moving)

Attack ends (OnDisable):
  Attack.enabled = false
  
Next frame (Update):
  ProcessInputs() checks move input
  → If moving: Movement stays enabled
  → If idle: Switch to Idle

Result: Smooth transition!
```

---

## 3. Animation Priority

### Problem:
When both `isAttacking` and `isMoving` were true, animator didn't know which to play:
- Sometimes showed movement during attack
- Inconsistent visuals

### Solution:
**Attack animation always has priority over movement**.

### Implementation:

**Player Animator:**
```
Attack Layer (Base Layer):
  - Has priority
  - isAttacking = true → Play attack animation
  
Movement Layer (Lower Priority):
  - Only plays if isAttacking = false
  - isMoving = true → Play move animation
```

**P_State_Attack.cs:**
```csharp
void OnEnable()
{
    anim.SetBool("isAttacking", true);  // HIGH PRIORITY
}

void OnDisable()
{
    anim.SetBool("isAttacking", false);  // Release priority
}
```

**P_State_Movement.cs:**
```csharp
void Update()
{
    // Only plays if attack layer not active
    anim.SetBool("isMoving", moveInput.sqrMagnitude > 0.01f);
}
```

### Priority Order:
```
1. Attack (highest)
2. Dodge
3. Movement
4. Idle (lowest/default)
```

---

## 4. State Cleanup

### Problem:
`isAttacking` animator bool never turned off:
- State disabled but animator bool stayed true
- Animation stuck in attack pose

### Solution:
**Disable state component when attack finishes, triggers OnDisable**.

### Implementation:

**P_State_Attack.cs:**
```csharp
IEnumerator AttackRoutine()
{
    // ... attack logic ...
    
    // Attack complete - disable self (triggers OnDisable)
    enabled = false;
}

void OnDisable()
{
    // CRITICAL: Turn off animator bool
    if (anim != null)
    {
        anim.SetBool("isAttacking", false);
    }
    
    // Reset combo state
    ResetCombo();
    
    // Stop all coroutines
    StopAllCoroutines();
}
```

### Cleanup Flow:
```
1. AttackRoutine finishes
2. Sets enabled = false
3. OnDisable() called automatically
4. Animator bool turned off
5. Combo reset
6. Coroutines stopped
7. Clean state!
```

---

## State Machine Architecture

### State Components:

**P_State_Idle.cs:**
```csharp
void OnEnable()
{
    // Set idle animation
}
```

**P_State_Movement.cs:**
```csharp
void Update()
{
    // Read move input
    // Apply movement penalty if attacking
    // Set velocity
    // Update animator
}
```

**P_State_Attack.cs:**
```csharp
void Attack()
{
    // Start attack routine
    // Set animator bool
}

IEnumerator AttackRoutine()
{
    // Open input window
    // Perform attack
    // Check for combo input
    // Disable when done
}

void OnDisable()
{
    // Cleanup
}
```

**P_State_Dodge.cs:**
```csharp
void Dodge()
{
    // Cancel combo
    // Perform dodge
    // Disable when done
}
```

### Controller (P_Controller.cs):

```csharp
void Update()
{
    ProcessInputs();  // Check input every frame
}

void ProcessInputs()
{
    // Priority order:
    // 1. Dodge (cancel everything)
    // 2. Attack (queue or trigger)
    // 3. Movement (can coexist with attack)
    // 4. Idle (fallback)
}

void SwitchState(PState newState, bool enableConcurrent = false)
{
    if (!enableConcurrent)
    {
        // Disable all states
        idle.enabled = false;
        movement.enabled = false;
        attack.enabled = false;
        dodge.enabled = false;
    }
    
    // Enable requested state
    switch (newState)
    {
        case PState.Idle: idle.enabled = true; break;
        case PState.Movement: movement.enabled = true; break;
        case PState.Attack: attack.enabled = true; break;
        case PState.Dodge: dodge.enabled = true; break;
    }
}
```

---

## Enemy AI Integration

**Same patterns applied to enemies:**

**E_Controller.cs:**
- Chase + Attack concurrent
- Animation priority
- State cleanup

**E_State_Attack.cs:**
- Same OnDisable cleanup
- Same animation lock
- Same weapon penalties

**E_State_Chase.cs:**
- Can run while attacking
- Movement penalty applied

---

## Animation Setup

### Animator Parameters:

**Player:**
```
bool isAttacking  // Priority 1
bool isDodging    // Priority 2
bool isMoving     // Priority 3
float moveX, moveY   // Movement direction
float idleX, idleY   // Idle facing
float atkX, atkY     // Attack direction
```

### Layer Setup:

```
Base Layer:
  - Attack State (isAttacking = true)
  - Dodge State (isDodging = true)
  - Movement State (isMoving = true, isAttacking = false)
  - Idle State (default)
```

### Transitions:

```
Any State → Attack (when isAttacking = true)
Any State → Dodge (when isDodging = true)
Attack → Movement (when isAttacking = false && isMoving = true)
Attack → Idle (when isAttacking = false && isMoving = false)
Movement → Idle (when isMoving = false)
```

---

## Common Patterns

### Pattern 1: Trigger Action
```csharp
public void TriggerAction()
{
    if (!enabled) return;  // Safety check
    
    StartCoroutine(ActionRoutine());
}

IEnumerator ActionRoutine()
{
    // Set animator
    anim.SetBool("isActing", true);
    
    // Perform action
    yield return new WaitForSeconds(duration);
    
    // Cleanup
    enabled = false;  // Triggers OnDisable
}

void OnDisable()
{
    anim.SetBool("isActing", false);
    StopAllCoroutines();
}
```

### Pattern 2: Concurrent State
```csharp
// In controller:
if (wantsAction && canDoAction)
{
    SwitchState(State.Action, enableConcurrent: true);
}
```

### Pattern 3: Cancel Action
```csharp
// In controller:
if (cancelInput)
{
    action.CancelAction();  // Calls enabled = false internally
    SwitchState(State.Idle);
}
```

---

## Troubleshooting

### Player stuck in attack animation:
**Cause:** `isAttacking` bool not turned off  
**Fix:** Ensure `OnDisable()` sets `anim.SetBool("isAttacking", false)`

### Can't move during attack:
**Cause:** Movement state disabled when attack starts  
**Fix:** Use `SwitchState(Movement, enableConcurrent: true)`

### Animation plays wrong:
**Cause:** Animation priority not set  
**Fix:** Set attack layer as highest priority in Animator

### State doesn't transition after action:
**Cause:** Not checking input in `Update()`  
**Fix:** Always call `ProcessInputs()` in controller `Update()`

### Combo resets unexpectedly:
**Cause:** State disabled multiple times  
**Fix:** Check `if (enabled)` before disabling

---

## Best Practices

### State Components:
1. **Single Responsibility** - Each state does one thing
2. **OnEnable** - Setup (set animator bools)
3. **Update/FixedUpdate** - Continuous logic
4. **OnDisable** - Cleanup (reset bools, stop coroutines)

### Controller:
1. **Update** - Always process input
2. **ProcessInputs** - Check priority order
3. **SwitchState** - Handle concurrent flag
4. **Never** manually enable/disable states outside controller

### Animator:
1. **Bool per state** - `isAttacking`, `isMoving`, etc.
2. **Priority order** - Attack > Dodge > Movement > Idle
3. **Any State transitions** - For high-priority states
4. **Exit Time** - Disable for responsive actions

---

**Status:** ✅ All Systems Implemented  
**Version:** Final
