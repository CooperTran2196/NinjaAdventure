# Enemy System Architecture - Visual Diagrams

## 1. State Component Hierarchy

```
Enemy GameObject
├── E_Controller (AI & State Management)
├── Rigidbody2D (Physics)
├── Animator (Visuals)
├── C_Stats (Attributes)
├── C_Health (HP & Damage)
├── Collider2D (Detection & Combat)
│
├── State Components (Enable/Disable based on AI)
│   ├── State_Idle ✅ (shared with NPCs)
│   ├── State_Wander ✅ (shared with NPCs)
│   ├── State_Chase ✅ (enemy-specific)
│   └── State_Attack ✅ (enemy-specific)
│
└── Weapon GameObject (child)
    └── W_Base (or W_Melee/W_Ranged)
```

---

## 2. State Machine Flow

```
┌─────────────────────────────────────────────────────────────┐
│                      E_Controller                           │
│                                                              │
│  ProcessAI() runs every Update:                            │
│  1. Check for death                                        │
│  2. Detect player (Physics2D.OverlapCircle)               │
│  3. Update currentTarget                                   │
│  4. Make decision:                                         │
│     • No target → defaultState (Idle/Wander)              │
│     • Target detected → Chase                              │
│     • Target in attack range → TriggerAttack()           │
└─────────────────────────────────────────────────────────────┘
                            ↓
        ┌───────────────────┴────────────────────┐
        │                                        │
        ↓                                        ↓
  ┌──────────┐                            ┌──────────┐
  │  IDLE    │                            │  WANDER  │
  │          │                            │          │
  │ Standing │                            │  Patrol  │
  │  still   │                            │   area   │
  └────┬─────┘                            └─────┬────┘
       │                                        │
       └───────────────┬────────────────────────┘
                       │
                       ↓ (Player detected)
                 ┌──────────┐
                 │  CHASE   │
                 │          │
                 │  Follow  │
                 │  player  │
                 └────┬─────┘
                      │
                      ↓ (Enter attack range)
                ┌──────────┐
                │  ATTACK  │
                │          │
                │ Attack + │
                │  Chase   │ ← Concurrent!
                └────┬─────┘
                     │
                     ↓ (Attack completes)
            ┌────────┴─────────┐
            │                  │
            ↓                  ↓
    (Target exists)    (Target lost)
    ┌──────────┐      ┌──────────┐
    │  CHASE   │      │IDLE/WANDER│
    └──────────┘      └──────────┘
```

---

## 3. Concurrent States During Attack

```
BEFORE ATTACK:
┌─────────────────────┐
│   E_Controller      │
│   currentState:     │
│   Chase             │
└──────────┬──────────┘
           │
           ↓
    ┌──────────────┐
    │ State_Chase  │
    │ enabled: ✅  │
    └──────────────┘

DURING ATTACK:
┌─────────────────────┐
│   E_Controller      │
│   currentState:     │
│   Attack            │
│   isAttacking: true │
└──────────┬──────────┘
           │
           ├─────────────────┬──────────────┐
           ↓                 ↓              ↓
    ┌──────────────┐  ┌─────────────┐ ┌───────────┐
    │ State_Chase  │  │State_Attack │ │ Weapon    │
    │ enabled: ✅  │  │ enabled: ✅ │ │ Active ✅ │
    │              │  │             │ └───────────┘
    │ Provides:    │  │ Provides:   │
    │ • Movement   │  │ • Animation │
    │ • Direction  │  │ • Timing    │
    │ • Penalty    │  │ • Weapon    │
    └──────────────┘  └─────────────┘

AFTER ATTACK:
┌─────────────────────┐
│   E_Controller      │
│   currentState:     │
│   Chase             │
│   isAttacking: false│
└──────────┬──────────┘
           │
           ↓
    ┌──────────────┐
    │ State_Chase  │
    │ enabled: ✅  │
    └──────────────┘
```

---

## 4. Attack Routine Timeline

```
TIME:     0s        0.15s       0.45s              1.5s        
          │          │           │                  │           
ACTION:   │          │           │                  │           
          │          │           │                  │           
TRIGGER─┬─┘          │           │                  │           
        │            │           │                  │           
        ↓            ↓           ↓                  ↓           
     START      HIT DELAY    ANIM END         SHOWTIME END
        │            │           │                  │           
        │            │           │                  │           
ANIM:  ┌────────────┼───────────┼──────────────────┤           
       │  Playing   │  Playing  │   FROZEN ❄️      │           
       │ speed=1.0  │ speed=1.0 │   speed=0.0      │           
       └────────────┴───────────┴──────────────────┘           
                                                    │           
WEAPON:              │                              │           
       ──────────────┼─────────────[ VISIBLE ]──────┤           
                     │                              │           
                  Attack()                      EndVisual()
                     
SPEED:               
       ──────────────┼──────────────────────────────┤           
       Full Speed    │    Penalty Applied (20%)     │  Full
       MS = 3.0      │    MS × 0.2 = 0.6           │  3.0
                     │                              │           
                                                                
PHASES:   Phase 1    │   Phase 2  │    Phase 3     │  Phase 4  
        ─────────────┼────────────┼────────────────┼──────────
         Init +      │  Weapon    │  Anim freeze   │ Cleanup +
         Anim start  │  attack    │  + lockout     │  Restore
```

---

## 5. Movement Penalty Application

```
┌─────────────────────────────────────────────────────────┐
│              State_Chase.Update()                       │
│                                                          │
│  1. Calculate chase direction                          │
│     Vector2 moveAxis = ComputeChaseDir()               │
│                                                          │
│  2. Get base speed                                     │
│     float speed = c_Stats.MS  (e.g., 3.0)             │
│                                                          │
│  3. Check if attacking                                 │
│     if (controller.currentState == Attack)             │
│     {                                                   │
│        4. Get active weapon                            │
│           W_Base weapon = attackState.GetActiveWeapon()│
│                                                          │
│        5. Apply penalty                                │
│           speed *= weapon.weaponData.attackMovePenalty │
│           // e.g., 3.0 × 0.2 = 0.6                    │
│     }                                                   │
│                                                          │
│  6. Apply velocity                                     │
│     controller.SetDesiredVelocity(moveAxis × speed)    │
└─────────────────────────────────────────────────────────┘
```

---

## 6. State Restoration Logic

```
Attack Completes:
┌──────────────────────────────────────────┐
│   State_Attack.AttackRoutine()           │
│   (coroutine finishes)                   │
└────────────────┬─────────────────────────┘
                 │
                 ↓
┌──────────────────────────────────────────┐
│   controller.SetAttacking(false)         │
│   enabled = false                        │
└────────────────┬─────────────────────────┘
                 │
                 ├─────────────────┬────────
                 ↓                 ↓        
┌──────────────────────┐  ┌────────────────┐
│  State_Attack        │  │ E_Controller   │
│  .OnDisable()        │  │ .SetAttacking()│
│                      │  │                │
│  • Stop coroutines   │  │ Check target:  │
│  • Clear anim bool   │  │                │
│  • Restore anim      │  │ if (exists)    │
│    speed = 1.0       │  │   → Chase      │
│  • Re-enable chase   │  │ else           │
│    (if target)       │  │   → Default    │
└──────────────────────┘  └────────────────┘
```

---

## 7. Animation Priority System

```
┌────────────────────────────────────────────────────┐
│             Animator Controller                    │
│                                                     │
│  Parameters:                                       │
│  • isAttacking (bool)  ← Attack animation          │
│  • isMoving (bool)     ← Movement animation        │
│  • moveX, moveY        ← Direction                 │
│  • idleX, idleY        ← Idle facing               │
│                                                     │
│  Priority (highest to lowest):                     │
│  1. isAttacking = true  → Attack animation         │
│  2. isMoving = true     → Movement animation       │
│  3. Default             → Idle animation           │
└────────────────────────────────────────────────────┘
                           ↑
                           │
        ┌──────────────────┴─────────────────┐
        │                                    │
┌───────┴──────────┐              ┌──────────┴────────┐
│  State_Attack    │              │  State_Chase      │
│                  │              │                   │
│  Sets:           │              │  During attack:   │
│  isAttacking=true│              │  isMoving = false │
│                  │              │  (priority!)      │
│  Priority: ⭐⭐⭐│              │                   │
└──────────────────┘              └───────────────────┘
```

---

## 8. Physics & Velocity Flow

```
┌──────────────────────────────────────────────────────┐
│                  FixedUpdate()                       │
│                                                       │
│  1. Calculate base velocity                         │
│     baseVel = (isDead || isStunned) ?               │
│               Vector2.zero : desiredVelocity        │
│                                                       │
│  2. Apply to Rigidbody                              │
│     rb.linearVelocity = baseVel + knockback         │
│                                                       │
│  3. Decay knockback for next frame                  │
│     knockback = MoveTowards(knockback, zero, KR)    │
└──────────────────────────────────────────────────────┘
                        ↑
                        │
        ┌───────────────┴────────────────┐
        │                                │
┌───────┴────────┐             ┌─────────┴─────────┐
│  desiredVel    │             │    knockback      │
│  (from states) │             │  (from damage)    │
│                │             │                   │
│  Chase:        │             │  W_Base applies   │
│  moveAxis × MS │             │  via controller   │
│  × penalty     │             │  .ReceiveKnockback│
└────────────────┘             └───────────────────┘
```

---

## 9. Weapon Integration

```
Enemy GameObject
    └── Weapon GameObject (child)
        ├── W_Base (or subclass)
        │   ├── BeginVisual()
        │   ├── Attack(dir)
        │   ├── ThrustOverTime()
        │   └── EndVisual()
        │
        └── W_SO (ScriptableObject reference)
            ├── AD/AP (damage)
            ├── showTime (duration)
            ├── attackMovePenalty (movement speed)
            ├── thrustDistance
            ├── knockbackForce
            └── stunTime
            
Flow:
1. State_Attack calls activeWeapon.Attack(dir)
2. W_Base.Attack() starts coroutine
3. BeginVisual() shows weapon + parents to owner
4. ThrustOverTime() moves weapon
5. OnTriggerEnter2D() → ApplyHitEffects()
6. After showTime → EndVisual() hides weapon

Penalty Read:
State_Chase → attackState.GetActiveWeapon() → weaponData.attackMovePenalty
```

---

## 10. Complete System Data Flow

```
┌─────────────────────────────────────────────────────────┐
│                    PLAYER INPUT                         │
│              (WASD + Mouse + Click)                     │
└─────────────┬───────────────────────────────────────────┘
              │
              ↓
┌─────────────────────────────────────────────────────────┐
│                    ENEMY AI                             │
│           (Detection + Range Check)                     │
└─────────────┬───────────────────────────────────────────┘
              │
              ↓
┌─────────────────────────────────────────────────────────┐
│                 CONTROLLER                              │
│  • P_Controller (input) or E_Controller (AI)           │
│  • State management                                     │
│  • TriggerAttack()                                     │
│  • SwitchState()                                       │
└─────────────┬───────────────────────────────────────────┘
              │
              ├─────────────┬──────────────┬──────────────┐
              ↓             ↓              ↓              ↓
    ┌─────────────┐ ┌─────────────┐ ┌──────────┐ ┌──────────┐
    │State_Idle   │ │State_Move/  │ │State_    │ │State_    │
    │             │ │State_Chase  │ │Attack    │ │Dodge     │
    │• Zero vel   │ │• Calculate  │ │• Timing  │ │(player)  │
    │• Idle anim  │ │  direction  │ │• Weapon  │ └──────────┘
    └─────────────┘ │• Apply      │ │• Anim    │
                    │  penalty    │ │  lock    │
                    │• Movement   │ └─────┬────┘
                    │  anim       │       │
                    └──────┬──────┘       │
                           │              │
                           ↓              ↓
                    desiredVelocity   activeWeapon.Attack()
                           │              │
                           └──────┬───────┘
                                  ↓
                    ┌──────────────────────────┐
                    │     FixedUpdate()        │
                    │  rb.linearVelocity =     │
                    │  desiredVel + knockback  │
                    └────────────┬─────────────┘
                                 │
                                 ↓
                    ┌──────────────────────────┐
                    │    VISUAL OUTPUT         │
                    │  • Animator              │
                    │  • Transform position    │
                    │  • Weapon visuals        │
                    └──────────────────────────┘
```

---

## Summary

These diagrams show how the enemy system:
1. **Mirrors the player** in architecture
2. **Uses concurrent states** (Chase + Attack)
3. **Applies weapon penalties** during attacks
4. **Manages state transitions** automatically
5. **Handles animations** with proper priority
6. **Integrates weapons** seamlessly
7. **Maintains compatibility** with NPCs

The system is **modular**, **maintainable**, and **extensible** - exactly like the player system! ✨
