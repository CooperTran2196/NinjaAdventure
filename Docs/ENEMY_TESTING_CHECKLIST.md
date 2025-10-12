# Enemy System Testing Checklist

## Pre-Testing Setup

### 1. Weapon Configuration
Create or configure test weapons with different profiles:

#### Fast Weapon (Dagger)
```
Name: Test_Dagger
AD: 3
AP: 0
showTime: 0.3f
attackMovePenalty: 0.8f (80% speed - very mobile)
thrustDistance: 0.2f
```

#### Medium Weapon (Sword)
```
Name: Test_Sword
AD: 5
AP: 0
showTime: 0.5f
attackMovePenalty: 0.5f (50% speed - balanced)
thrustDistance: 0.3f
```

#### Slow Weapon (Greatsword)
```
Name: Test_Greatsword
AD: 15
AP: 0
showTime: 1.5f
attackMovePenalty: 0.2f (20% speed - very slow)
thrustDistance: 0.4f
```

### 2. Enemy Setup
Create test enemy prefabs:

#### Fast Enemy
```
C_Stats:
  MS: 5.0
  attackCooldown: 0.8f

Weapon: Test_Dagger (child object)

E_Controller:
  detectionRange: 5f
  attackRange: 1.5f
  attackStartBuffer: 0.2f
  defaultState: Idle
```

#### Slow Enemy
```
C_Stats:
  MS: 2.0
  attackCooldown: 2.0f

Weapon: Test_Greatsword (child object)

E_Controller:
  detectionRange: 4f
  attackRange: 1.8f
  attackStartBuffer: 0.3f
  defaultState: Idle
```

---

## Core Functionality Tests

### Test 1: Basic Attack (Standing)
**Steps:**
1. Place enemy in scene
2. Enter play mode
3. Approach enemy to trigger detection
4. Stay within attack range but don't move

**Expected Results:**
- [ ] Enemy detects player
- [ ] Enemy switches to Chase state
- [ ] Enemy enters attack range
- [ ] Attack animation plays
- [ ] `isAttacking` becomes true
- [ ] Weapon appears and thrusts
- [ ] Attack completes
- [ ] Enemy returns to Chase or Idle

**Look For:**
- ✅ Smooth state transitions
- ✅ No stuck states
- ✅ Animation plays fully
- ❌ No console errors

---

### Test 2: Attack While Chasing
**Steps:**
1. Place enemy in scene
2. Enter play mode
3. Approach enemy to trigger chase
4. Move around while enemy is in attack range
5. Let enemy attack while you're moving

**Expected Results:**
- [ ] Enemy chases at full speed (MS)
- [ ] Enemy enters attack range
- [ ] Attack triggered
- [ ] **Enemy still moves during attack** ✅
- [ ] Enemy moves slower (weapon penalty applied)
- [ ] Attack animation takes priority over movement
- [ ] Attack completes
- [ ] Full chase speed restored

**Look For:**
- ✅ Enemy repositions during attack (slow)
- ✅ No animation conflicts
- ✅ Speed changes are smooth
- ❌ Enemy doesn't freeze in place

---

### Test 3: ShowTime Animation Lock (Slow Weapon)
**Steps:**
1. Give enemy Test_Greatsword (showTime: 1.5f)
2. Trigger attack
3. Watch animation closely

**Expected Results:**
- [ ] Attack animation plays (0.45s)
- [ ] Animation reaches final frame
- [ ] **Animation freezes** at final frame ✅
- [ ] Enemy still moving slowly (20% speed)
- [ ] Freeze lasts ~1.05s (1.5f - 0.45f)
- [ ] Animation speed restores to 1.0
- [ ] Attack completes normally

**Look For:**
- ✅ Smooth freeze (not jarring)
- ✅ Movement continues during freeze
- ✅ Animation speed restored
- ❌ No stuck frozen animations

---

### Test 4: ShowTime No Lock (Fast Weapon)
**Steps:**
1. Give enemy Test_Dagger (showTime: 0.3f)
2. Trigger attack
3. Watch animation

**Expected Results:**
- [ ] Attack animation plays normally
- [ ] No animation freeze (showTime < animDuration)
- [ ] Attack completes at 0.45s (animation duration)
- [ ] No lockout period

**Look For:**
- ✅ No freeze occurs
- ✅ Natural animation flow
- ❌ No premature animation end

---

### Test 5: Movement Penalty (Visual)
**Steps:**
1. Create two enemies side-by-side
2. Enemy A: Test_Dagger (penalty: 0.8)
3. Enemy B: Test_Greatsword (penalty: 0.2)
4. Trigger both to chase and attack simultaneously

**Expected Results:**
- [ ] Both chase at their MS speed
- [ ] Both attack when in range
- [ ] Enemy A moves faster during attack (80% vs 20%)
- [ ] Enemy B moves much slower during attack
- [ ] Visual difference is clear

**Look For:**
- ✅ Noticeable speed difference
- ✅ Fast weapon = mobile combat
- ✅ Slow weapon = locked down combat

---

### Test 6: State Restoration (Target Exists)
**Steps:**
1. Enemy chasing player
2. Trigger attack
3. Stay in detection range
4. Let attack complete

**Expected Results:**
- [ ] Attack completes
- [ ] `SetAttacking(false)` called
- [ ] `currentTarget` still exists
- [ ] Enemy switches back to Chase ✅
- [ ] Enemy resumes chasing at full speed

**Look For:**
- ✅ Smooth transition back to chase
- ✅ No idle frame between attack and chase
- ❌ No stuck attack state

---

### Test 7: State Restoration (Target Lost)
**Steps:**
1. Enemy chasing player
2. Trigger attack
3. **Teleport player far away** during attack
4. Let attack complete

**Expected Results:**
- [ ] Attack completes
- [ ] `SetAttacking(false)` called
- [ ] `currentTarget` becomes null (out of detection range)
- [ ] Enemy switches to defaultState (Idle/Wander) ✅
- [ ] Enemy returns to idle/patrol behavior

**Look For:**
- ✅ Smooth transition to default behavior
- ✅ No chase after target lost
- ❌ No stuck states

---

### Test 8: Attack Interrupted by Death
**Steps:**
1. Enemy starts attack
2. Deal lethal damage mid-attack
3. Watch state cleanup

**Expected Results:**
- [ ] Attack interrupted
- [ ] `OnDisable()` called on State_Attack
- [ ] `anim.speed` restored to 1.0 ✅
- [ ] `isAttacking` set to false
- [ ] Death animation plays
- [ ] No errors in console

**Look For:**
- ✅ Clean cleanup on death
- ✅ Death animation plays normally
- ❌ No frozen animation
- ❌ No console errors

---

### Test 9: Concurrent States (Chase + Attack)
**Steps:**
1. Place enemy far from player
2. Trigger detection and chase
3. Enter attack range
4. Use Debug/Inspector to watch component state

**Expected Results:**
- [ ] Detection → `chase.enabled = true`
- [ ] Attack range → `TriggerAttack()` called
- [ ] `attack.enabled = true`
- [ ] **`chase.enabled` stays true** ✅
- [ ] Both components active simultaneously
- [ ] Attack completes → `attack.enabled = false`
- [ ] `chase.enabled` still true

**Look For:**
- ✅ Both states enabled during attack
- ✅ Chase provides movement
- ✅ Attack provides animation
- ❌ Chase not disabled during attack

---

### Test 10: Animation Priority
**Steps:**
1. Enemy chasing player
2. Trigger attack while enemy is moving

**Expected Results:**
- [ ] Chase: `isMoving = true`
- [ ] Attack triggers
- [ ] `State_Chase.Update()` sets `isMoving = false` ✅
- [ ] Attack animation plays (priority)
- [ ] Movement animation **does not** play
- [ ] Attack completes
- [ ] `isMoving = true` again (if still chasing)

**Look For:**
- ✅ Attack animation never interrupted
- ✅ No blended animations
- ❌ Movement animation doesn't override attack

---

## Edge Case Tests

### Test 11: Rapid Attack Spam
**Steps:**
1. Set `attackCooldown = 0.1f` (very low)
2. Stay in attack range
3. Let enemy spam attacks

**Expected Results:**
- [ ] Attacks trigger rapidly
- [ ] Each attack completes fully
- [ ] No overlap (one attack at a time)
- [ ] State restoration works every time
- [ ] No stuck states

**Look For:**
- ✅ Clean attack cycles
- ❌ No double-attacks
- ❌ No state corruption

---

### Test 12: Long ShowTime Spam
**Steps:**
1. Set weapon `showTime = 3.0f` (very long)
2. Set `attackCooldown = 0.5f` (shorter than showTime)
3. Trigger attack

**Expected Results:**
- [ ] Attack starts
- [ ] Animation freezes
- [ ] Lockout period starts
- [ ] **Cannot trigger second attack** (isAttacking = true) ✅
- [ ] Full 3.0s completes
- [ ] State restored
- [ ] Next attack can trigger

**Look For:**
- ✅ No attack overlap
- ✅ ShowTime fully respected
- ❌ No spam attacks during lockout

---

### Test 13: Multiple Enemies
**Steps:**
1. Place 3-5 enemies in scene
2. Trigger all to attack simultaneously

**Expected Results:**
- [ ] Each enemy manages own state
- [ ] No state interference between enemies
- [ ] All showTime locks work independently
- [ ] All movement penalties work independently
- [ ] All state restorations work

**Look For:**
- ✅ Independent behavior
- ❌ No shared state bugs
- ❌ No performance issues

---

### Test 14: Attack Cooldown Enforcement
**Steps:**
1. Trigger attack
2. Immediately move out and back into range
3. Try to trigger second attack before cooldown

**Expected Results:**
- [ ] First attack completes
- [ ] `attackCooldown` timer starts
- [ ] Re-enter attack range
- [ ] **Second attack doesn't trigger** ✅
- [ ] Wait for cooldown
- [ ] Second attack triggers normally

**Look For:**
- ✅ Cooldown properly enforced
- ❌ No instant re-attacks

---

### Test 15: Knockback During Attack
**Steps:**
1. Enemy starts attack
2. Deal damage with knockback weapon
3. Watch behavior

**Expected Results:**
- [ ] Attack animation continues
- [ ] Enemy is knocked back (physics)
- [ ] **Attack still completes** ✅
- [ ] State restoration works
- [ ] Enemy returns to chase

**Look For:**
- ✅ Attack not cancelled by knockback
- ✅ Smooth recovery
- ❌ No stuck states

---

### Test 16: Stun During Attack
**Steps:**
1. Enemy starts attack
2. Apply stun effect
3. Watch behavior

**Expected Results:**
- [ ] Attack animation may continue (depending on implementation)
- [ ] Enemy velocity = 0 (stunned)
- [ ] Stun duration completes
- [ ] Attack completes
- [ ] State restoration works

**Look For:**
- ✅ Clean stun handling
- ✅ State restoration after stun
- ❌ No state corruption

---

## Compatibility Tests (NPCs & Shared States)

### Test 17: NPC Idle State
**Steps:**
1. Place NPC with `State_Idle` enabled
2. Enter play mode

**Expected Results:**
- [ ] NPC stands still
- [ ] Idle animation plays
- [ ] No errors
- [ ] Works as before (unchanged)

**Look For:**
- ✅ No regression
- ❌ No enemy-specific code breaking NPCs

---

### Test 18: NPC Wander State
**Steps:**
1. Place NPC with `State_Wander` enabled
2. Configure wander area
3. Enter play mode

**Expected Results:**
- [ ] NPC wanders in area
- [ ] Pauses at destinations
- [ ] Picks new random points
- [ ] Works as before (unchanged)

**Look For:**
- ✅ No regression
- ❌ No changes to behavior

---

### Test 19: NPC Talk State
**Steps:**
1. Place NPC with `State_Talk`
2. Approach with player
3. Press interact key

**Expected Results:**
- [ ] Talk icon appears
- [ ] Dialog starts
- [ ] NPC faces player
- [ ] Works as before (unchanged)

**Look For:**
- ✅ No regression
- ❌ No enemy code affecting NPCs

---

## Visual Polish Tests

### Test 20: Animation Smoothness
**Steps:**
1. Watch enemy attack animation closely
2. Test all weapon types (fast, medium, slow)

**Expected Results:**
- [ ] Smooth animation transitions
- [ ] No jarring freezes (except intended showTime lock)
- [ ] Natural movement during attack
- [ ] Clean return to idle/chase

**Look For:**
- ✅ Polished visual feel
- ❌ No animation pops

---

### Test 21: Combat "Feel" Test
**Steps:**
1. Fight fast enemy (dagger)
2. Fight slow enemy (greatsword)
3. Compare experience

**Expected Results:**
- [ ] Fast enemy feels aggressive and mobile
- [ ] Slow enemy feels powerful but committed
- [ ] Clear gameplay difference
- [ ] Both feel intentional and balanced

**Look For:**
- ✅ Distinct combat archetypes
- ✅ Tactical depth (can exploit slow enemy)

---

## Performance Tests

### Test 22: Many Enemies Performance
**Steps:**
1. Spawn 20+ enemies
2. Trigger all to chase and attack
3. Monitor framerate

**Expected Results:**
- [ ] No significant framerate drop
- [ ] All state management works
- [ ] No stuttering
- [ ] No memory leaks

**Look For:**
- ✅ Stable performance
- ❌ No GC spikes
- ❌ No freezing

---

## Final Checklist Summary

### Core Features
- [ ] Basic attack works
- [ ] Attack + chase concurrent states work
- [ ] ShowTime animation lock works (slow weapons)
- [ ] No showTime lock for fast weapons
- [ ] Movement penalty applied correctly
- [ ] State restoration (target exists)
- [ ] State restoration (target lost)
- [ ] Death cleanup works

### Edge Cases
- [ ] Attack spam handled
- [ ] Long showTime handled
- [ ] Multiple enemies work independently
- [ ] Cooldown enforced
- [ ] Knockback during attack works
- [ ] Stun during attack works

### Compatibility
- [ ] NPC Idle state still works
- [ ] NPC Wander state still works
- [ ] NPC Talk state still works
- [ ] No regressions in shared states

### Polish
- [ ] Animations smooth
- [ ] Combat feels good
- [ ] Fast vs slow weapons feel different
- [ ] Performance acceptable

---

## Bug Report Template

If you find issues, document them like this:

```
**Bug:** [Short description]

**Steps to Reproduce:**
1. ...
2. ...
3. ...

**Expected:** [What should happen]

**Actual:** [What actually happens]

**Console Errors:** [Copy errors if any]

**Files Involved:** [E_Controller.cs, State_Attack.cs, etc.]

**Screenshot/Video:** [If applicable]
```

---

## Success Criteria

✅ **System is working correctly if:**
- All core functionality tests pass
- No console errors during normal gameplay
- Animations look smooth and intentional
- Enemy behavior mirrors player quality
- NPCs still work correctly
- Performance is acceptable

---

**Testing Duration:** ~30-45 minutes for full checklist  
**Priority Tests:** 1-10 (core functionality)  
**Optional Tests:** 11-22 (edge cases, polish, performance)
