# 🤖 Enemy AI System Guide

Complete guide to enemy AI architecture, state machine, behaviors, and testing.

---

## Overview

**Enemy AI Features:**
- ✅ State-based behavior (Idle, Wander, Chase, Attack, Dead)
- ✅ Modular state components (enable/disable)
- ✅ Detection system (player tracking)
- ✅ Combat integration (weapons, damage, death)
- ✅ Configurable parameters (speeds, ranges, cooldowns)
- ✅ Rewards on death (XP, loot)

**Design Philosophy:**
Enemy system mirrors player architecture but uses AI decision-making instead of input.

---

## 1. Enemy Architecture

### Core Components:

**E_Controller.cs** - Central AI brain
- Manages state transitions
- Coordinates all components
- Handles state switching logic

**State Components:**
- `E_State_Idle.cs` - Stand still, face last direction
- `E_State_Wander.cs` - Random movement
- `E_State_Chase.cs` - Follow player
- `E_State_Attack.cs` - Execute attacks
- `E_State_Dead.cs` - Death state (future expansion)

**Supporting Systems:**
- `E_Detection.cs` - Player tracking (vision range)
- `E_Combat.cs` - Attack timing & execution
- `E_Movement.cs` - Velocity control
- `E_Reward.cs` - XP/loot on death

**Shared Systems (Player + Enemy):**
- `C_Stats.cs` - Health, damage, speed
- `C_Health.cs` - Damage/death handling
- `W_Base.cs` - Weapon system

---

## 2. State Machine

### State Enum:
```csharp
public enum EState
{
    Idle,
    Wander,
    Chase,
    Attack,
    Dead
}
```

### State Transitions:

```
Idle → Wander (timer expires)
Wander → Chase (player detected)
Chase → Attack (in range)
Attack → Chase (out of range)
Chase → Wander (player lost)
Wander → Idle (wander timer expires)
Any → Dead (health <= 0)
```

### Controller (E_Controller.cs):

```csharp
public class E_Controller : MonoBehaviour
{
    // State components
    [SerializeField] E_State_Idle idle;
    [SerializeField] E_State_Wander wander;
    [SerializeField] E_State_Chase chase;
    [SerializeField] E_State_Attack attack;
    
    // Detection
    [SerializeField] E_Detection detection;
    
    // Movement
    [SerializeField] E_Movement movement;
    
    // Current state
    EState currentState = EState.Idle;
    
    void Start()
    {
        SwitchState(EState.Idle);
    }
    
    public void SwitchState(EState newState)
    {
        if (currentState == newState) return;
        
        // Disable all states
        idle.enabled = false;
        wander.enabled = false;
        chase.enabled = false;
        attack.enabled = false;
        
        // Enable new state
        currentState = newState;
        switch (newState)
        {
            case EState.Idle: idle.enabled = true; break;
            case EState.Wander: wander.enabled = true; break;
            case EState.Chase: chase.enabled = true; break;
            case EState.Attack: attack.enabled = true; break;
        }
    }
    
    // API for states to request transitions
    public void RequestWander() => SwitchState(EState.Wander);
    public void RequestChase() => SwitchState(EState.Chase);
    public void RequestAttack() => SwitchState(EState.Attack);
    public void RequestIdle() => SwitchState(EState.Idle);
}
```

---

## 3. State Behaviors

### Idle State (E_State_Idle.cs)

**Purpose:** Stand still, wait before wandering

```csharp
public class E_State_Idle : MonoBehaviour
{
    [SerializeField] float idleDuration = 2f;
    
    E_Controller controller;
    E_Detection detection;
    
    float idleTimer;
    
    void Awake()
    {
        controller = GetComponent<E_Controller>();
        detection = GetComponent<E_Detection>();
    }
    
    void OnEnable()
    {
        idleTimer = idleDuration;
    }
    
    void Update()
    {
        // Check for player first
        if (detection.PlayerDetected)
        {
            controller.RequestChase();
            return;
        }
        
        // Count down idle timer
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            controller.RequestWander();
        }
    }
}
```

**Behavior:**
- Stands still for `idleDuration` seconds
- Checks for player every frame
- Switches to Chase if player detected
- Switches to Wander when timer expires

---

### Wander State (E_State_Wander.cs)

**Purpose:** Random movement in area

```csharp
public class E_State_Wander : MonoBehaviour
{
    [SerializeField] float wanderSpeed = 1f;
    [SerializeField] float wanderDuration = 3f;
    [SerializeField] float changeDirectionInterval = 1f;
    
    E_Controller controller;
    E_Detection detection;
    E_Movement movement;
    Animator anim;
    
    Vector2 wanderDirection;
    float wanderTimer;
    float directionTimer;
    
    void Awake()
    {
        controller = GetComponent<E_Controller>();
        detection = GetComponent<E_Detection>();
        movement = GetComponent<E_Movement>();
        anim = GetComponent<Animator>();
    }
    
    void OnEnable()
    {
        wanderTimer = wanderDuration;
        directionTimer = 0f;
        PickRandomDirection();
    }
    
    void Update()
    {
        // Check for player first
        if (detection.PlayerDetected)
        {
            controller.RequestChase();
            return;
        }
        
        // Count down timers
        wanderTimer -= Time.deltaTime;
        directionTimer -= Time.deltaTime;
        
        // Change direction periodically
        if (directionTimer <= 0f)
        {
            directionTimer = changeDirectionInterval;
            PickRandomDirection();
        }
        
        // Move in current direction
        movement.SetDesiredVelocity(wanderDirection * wanderSpeed);
        
        // Update animation
        if (wanderDirection.sqrMagnitude > 0.01f)
        {
            anim.SetFloat("moveX", wanderDirection.x);
            anim.SetFloat("moveY", wanderDirection.y);
        }
        
        // Return to idle when wander expires
        if (wanderTimer <= 0f)
        {
            controller.RequestIdle();
        }
    }
    
    void PickRandomDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
}
```

**Behavior:**
- Picks random direction every `changeDirectionInterval` seconds
- Moves at `wanderSpeed`
- Wanders for `wanderDuration` seconds total
- Always checks for player (high priority)
- Returns to Idle when done

---

### Chase State (E_State_Chase.cs)

**Purpose:** Follow player

```csharp
public class E_State_Chase : MonoBehaviour
{
    [SerializeField] float chaseSpeed = 2.5f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float losePlayerDelay = 2f;
    
    E_Controller controller;
    E_Detection detection;
    E_Movement movement;
    Animator anim;
    
    float lostPlayerTimer;
    
    void Awake()
    {
        controller = GetComponent<E_Controller>();
        detection = GetComponent<E_Detection>();
        movement = GetComponent<E_Movement>();
        anim = GetComponent<Animator>();
    }
    
    void OnEnable()
    {
        lostPlayerTimer = 0f;
    }
    
    void Update()
    {
        Transform player = detection.PlayerTransform;
        
        // Player lost - start lose timer
        if (!detection.PlayerDetected)
        {
            lostPlayerTimer += Time.deltaTime;
            if (lostPlayerTimer >= losePlayerDelay)
            {
                controller.RequestWander();
                return;
            }
        }
        else
        {
            lostPlayerTimer = 0f;  // Reset timer if player re-detected
        }
        
        if (player == null) return;
        
        // Check attack range
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer <= attackRange)
        {
            controller.RequestAttack();
            return;
        }
        
        // Chase player
        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        movement.SetDesiredVelocity(dirToPlayer * chaseSpeed);
        
        // Update animation
        anim.SetFloat("moveX", dirToPlayer.x);
        anim.SetFloat("moveY", dirToPlayer.y);
    }
}
```

**Behavior:**
- Moves toward player at `chaseSpeed`
- Switches to Attack when within `attackRange`
- Tracks player for `losePlayerDelay` seconds after losing detection
- Returns to Wander if player lost for too long

---

### Attack State (E_State_Attack.cs)

**Purpose:** Execute weapon attacks

```csharp
public class E_State_Attack : MonoBehaviour
{
    [SerializeField] float attackCooldown = 1.5f;
    
    E_Controller controller;
    E_Detection detection;
    E_Combat combat;
    Animator anim;
    
    float cooldownTimer;
    
    void Awake()
    {
        controller = GetComponent<E_Controller>();
        detection = GetComponent<E_Detection>();
        combat = GetComponent<E_Combat>();
        anim = GetComponent<Animator>();
    }
    
    void OnEnable()
    {
        cooldownTimer = 0f;
        
        // Face player
        Transform player = detection.PlayerTransform;
        if (player != null)
        {
            Vector2 dirToPlayer = (player.position - transform.position).normalized;
            anim.SetFloat("idleX", dirToPlayer.x);
            anim.SetFloat("idleY", dirToPlayer.y);
        }
    }
    
    void Update()
    {
        Transform player = detection.PlayerTransform;
        
        // Player out of range or lost
        if (player == null || !detection.PlayerDetected)
        {
            controller.RequestChase();
            return;
        }
        
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer > combat.AttackRange)
        {
            controller.RequestChase();
            return;
        }
        
        // Attack cooldown
        cooldownTimer += Time.deltaTime;
        if (cooldownTimer >= attackCooldown)
        {
            cooldownTimer = 0f;
            combat.TryAttack();
        }
    }
}
```

**Behavior:**
- Stands still and attacks
- Uses `E_Combat` to trigger weapon
- Returns to Chase if player moves out of range
- Attacks every `attackCooldown` seconds

---

## 4. Detection System

### E_Detection.cs

```csharp
public class E_Detection : MonoBehaviour
{
    [SerializeField] float detectionRadius = 5f;
    [SerializeField] LayerMask playerLayer;
    
    Transform playerTransform;
    bool playerDetected;
    
    public Transform PlayerTransform => playerTransform;
    public bool PlayerDetected => playerDetected;
    
    void Update()
    {
        DetectPlayer();
    }
    
    void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position, 
            detectionRadius, 
            playerLayer
        );
        
        if (hit != null)
        {
            playerTransform = hit.transform;
            playerDetected = true;
        }
        else
        {
            playerDetected = false;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = playerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
```

**Features:**
- Circular detection range
- Updates every frame
- Visual gizmo for debugging
- Caches player transform

---

## 5. Combat System

### E_Combat.cs

```csharp
public class E_Combat : MonoBehaviour
{
    [SerializeField] W_Base weapon;
    [SerializeField] float attackRange = 1.5f;
    
    E_Detection detection;
    Animator anim;
    
    public float AttackRange => attackRange;
    
    void Awake()
    {
        detection = GetComponent<E_Detection>();
        anim = GetComponent<Animator>();
    }
    
    public void TryAttack()
    {
        Transform player = detection.PlayerTransform;
        if (player == null) return;
        
        // Calculate attack direction
        Vector2 attackDir = (player.position - transform.position).normalized;
        
        // Set animator
        anim.SetFloat("atkX", attackDir.x);
        anim.SetFloat("atkY", attackDir.y);
        anim.SetBool("isAttacking", true);
        
        // Execute weapon attack
        weapon.Attack(attackDir);
        
        // Reset animator (use animation event or timer)
        StartCoroutine(ResetAttackAnim());
    }
    
    IEnumerator ResetAttackAnim()
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetBool("isAttacking", false);
    }
}
```

---

## 6. Movement System

### E_Movement.cs

```csharp
public class E_Movement : MonoBehaviour
{
    Rigidbody2D rb;
    Vector2 desiredVelocity;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void SetDesiredVelocity(Vector2 velocity)
    {
        desiredVelocity = velocity;
    }
    
    void FixedUpdate()
    {
        rb.linearVelocity = desiredVelocity;
    }
}
```

**Simple and clean:**
- States request velocity via `SetDesiredVelocity()`
- Controller applies in `FixedUpdate()`
- No direct velocity manipulation in states

---

## 7. Reward System

### E_Reward.cs

```csharp
public class E_Reward : MonoBehaviour
{
    [SerializeField] int expReward = 10;
    
    C_Health health;
    
    void Awake()
    {
        health = GetComponent<C_Health>();
    }
    
    void OnEnable()
    {
        health.OnDied += GrantReward;
    }
    
    void OnDisable()
    {
        health.OnDied -= GrantReward;
    }
    
    void GrantReward()
    {
        // Find player exp component
        P_Exp playerExp = FindFirstObjectByType<P_Exp>();
        if (playerExp != null)
        {
            playerExp.AddXP(expReward);
        }
    }
}
```

---

## 8. Player vs Enemy Comparison

| Feature | Player | Enemy |
|---------|--------|-------|
| **Control** | Input (WASD, Mouse) | AI (State Machine) |
| **States** | Idle, Movement, Attack, Dodge | Idle, Wander, Chase, Attack |
| **Movement** | P_State_Movement | E_Movement (passive) |
| **Combat** | P_State_Attack | E_State_Attack + E_Combat |
| **Decision Making** | Player input | Detection + AI logic |
| **Shared Systems** | C_Stats, C_Health, W_Base | Same! |

**Key Insight:** Shared components (`C_*`, `W_*`) mean weapons, health, and stats work identically for both player and enemies!

---

## 9. Setup Checklist

### Creating a New Enemy:

1. **Create GameObject:**
   - Add `SpriteRenderer`
   - Add `Animator`
   - Add `Rigidbody2D` (Kinematic or Dynamic)
   - Add `Collider2D`

2. **Add Core Components:**
   - `E_Controller`
   - `E_Movement`
   - `E_Detection`
   - `E_Combat`
   - `E_Reward`

3. **Add State Components:**
   - `E_State_Idle`
   - `E_State_Wander`
   - `E_State_Chase`
   - `E_State_Attack`

4. **Add Shared Components:**
   - `C_Stats`
   - `C_Health`

5. **Assign Weapon:**
   - Create or assign `W_SO` ScriptableObject
   - Reference in `E_Combat`

6. **Configure Inspector:**
   - Set detection radius
   - Set speeds (wander, chase)
   - Set attack range
   - Set timers (idle, wander, cooldown)
   - Set exp reward

7. **Setup Animator:**
   - Add parameters: `moveX`, `moveY`, `idleX`, `idleY`, `atkX`, `atkY`, `isAttacking`
   - Create animations: Idle, Walk, Attack, Death
   - Setup transitions

8. **Assign Layers:**
   - Enemy layer
   - Player detection layer mask

---

## 10. Testing Guide

### Test Scenarios:

**1. Idle → Wander:**
- [ ] Enemy stands still for `idleDuration`
- [ ] Transitions to Wander
- [ ] Picks random direction

**2. Wander → Chase:**
- [ ] Enemy wanders randomly
- [ ] Detects player when in range
- [ ] Transitions to Chase immediately

**3. Chase → Attack:**
- [ ] Enemy moves toward player
- [ ] Stops at attack range
- [ ] Transitions to Attack

**4. Attack Behavior:**
- [ ] Attacks every `attackCooldown` seconds
- [ ] Faces player
- [ ] Weapon triggers correctly

**5. Player Lost:**
- [ ] Enemy continues chasing for `losePlayerDelay`
- [ ] Returns to Wander after delay

**6. Death:**
- [ ] Grants XP to player
- [ ] Triggers death animation
- [ ] Stops all behaviors

### Debug Tools:

**Gizmos:**
```csharp
void OnDrawGizmosSelected()
{
    // Detection range
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectionRadius);
    
    // Attack range
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, attackRange);
}
```

**Console Logging:**
```csharp
void SwitchState(EState newState)
{
    Debug.Log($"[{gameObject.name}] {currentState} → {newState}");
    // ... state switch logic ...
}
```

---

## 11. Tuning Guide

### Passive Enemy:
```
detectionRadius = 3f
wanderSpeed = 0.5f
chaseSpeed = 1.5f
attackRange = 1.0f
attackCooldown = 2.0f
```

### Aggressive Enemy:
```
detectionRadius = 7f
wanderSpeed = 1.5f
chaseSpeed = 3.5f
attackRange = 2.0f
attackCooldown = 0.8f
```

### Boss Enemy:
```
detectionRadius = 10f
wanderSpeed = 0f (no wander, always idle)
chaseSpeed = 2.0f
attackRange = 3.0f
attackCooldown = 1.5f
C_Stats: High HP, high damage
```

---

## 12. Common Issues

### Enemy doesn't detect player:
- Check `playerLayer` in `E_Detection`
- Verify player has correct layer
- Check detection radius (gizmo)

### Enemy doesn't move:
- Verify `Rigidbody2D` not frozen
- Check `E_Movement.SetDesiredVelocity()` being called
- Ensure state is enabled

### Enemy doesn't attack:
- Verify weapon assigned in `E_Combat`
- Check attack range vs detection range
- Ensure `E_State_Attack` enabled

### State transitions broken:
- Check `E_Controller.SwitchState()` disables all states
- Verify states call `controller.Request*()` not direct enable/disable
- Check priority order (player detection always first)

---

## Best Practices

1. **Always check player detection first** in every state
2. **Use controller requests** (`RequestChase()`) not direct state changes
3. **Set timers in OnEnable** for clean state entry
4. **Use gizmos** for visual debugging
5. **Keep states simple** - one responsibility per state
6. **Share systems** - use `C_*` and `W_*` for parity with player

---

**Status:** ✅ Fully Implemented  
**Architecture:** Modular state machine  
**Integration:** Works with player systems seamlessly
