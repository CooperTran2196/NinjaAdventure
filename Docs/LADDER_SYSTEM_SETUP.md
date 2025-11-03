# 🪜 Ladder System Setup Guide

## Overview
Realistic ladder climbing with:
- **Slower ascent** (harder to climb up against gravity)
- **Faster descent** (gravity assists going down)
- **Limited horizontal movement** while on ladder

## Setup in Unity

### 1. Create Ladder GameObject
1. Create empty GameObject: `Ladder`
2. Add component: `ENV_Ladder`
3. Add component: `BoxCollider2D`
   - Set to **Trigger** (automatically set by script)
   - Adjust size to cover climbable area

### 2. Configure Ladder Settings

**Default Settings (Good starting point):**
```
Climb Up Multiplier:    0.6   (60% speed going up - feels challenging)
Climb Down Multiplier:  1.3   (130% speed going down - gravity assist)
Horizontal Multiplier:  0.5   (50% horizontal movement while climbing)
Ladder Gravity Scale:   0     (No gravity while on ladder)
```

**Tweaking Tips:**
- **Slower climb up:** Lower `climbUpMultiplier` (0.4 - 0.6)
- **Faster slide down:** Increase `climbDownMultiplier` (1.3 - 1.8)
- **More horizontal freedom:** Increase `horizontalMultiplier` (0.5 - 0.8)
- **Keep slight gravity:** Set `ladderGravityScale` to 0.1-0.3

### 3. Layer Setup (Optional but Recommended)
- Create layer: `Ladder`
- Assign layer to ladder GameObjects
- Helps with organization and collision filtering

## How It Works

### Physics
- **On ladder:** Gravity disabled, movement controlled by input
- **Vertical movement:** Multiplied based on direction (up/down)
- **Horizontal movement:** Reduced to simulate limited side-to-side freedom
- **Off ladder:** Normal gravity and movement restored

### Code Flow
```
Player enters ladder trigger
  ↓
ENV_Ladder.OnTriggerEnter2D() calls P_Controller.EnterLadder()
  ↓
P_Controller stores ladder reference, disables gravity
  ↓
P_State_Movement.Update() applies ladder modifiers to velocity
  ↓
- Vertical: slower up (0.6x), faster down (1.3x)
- Horizontal: limited (0.5x)
  ↓
Player exits ladder trigger
  ↓
ENV_Ladder.OnTriggerExit2D() calls P_Controller.ExitLadder()
  ↓
Normal gravity and movement restored
```

## Visual Feedback (Scene View)
- Orange transparent box shows ladder climb zone
- Yellow wireframe shows exact trigger bounds
- Visible when ladder GameObject is selected

## Testing Checklist
- [ ] Player can climb up ladder (slower than normal movement)
- [ ] Player can climb down ladder (faster than normal movement)
- [ ] Horizontal movement is limited while on ladder
- [ ] Player doesn't fall while idle on ladder
- [ ] Gravity restores when exiting ladder
- [ ] Works with player's normal state system (attack, dodge, etc.)

## Example Scenarios

### Standard Ladder (balanced)
```
climbUpMultiplier:   0.6
climbDownMultiplier: 1.3
horizontalMultiplier: 0.5
```

### Rope Ladder (harder, more realistic)
```
climbUpMultiplier:   0.4   // Very slow climb
climbDownMultiplier: 1.6   // Fast slide
horizontalMultiplier: 0.3  // Very limited horizontal
```

### Metal Ladder (easier)
```
climbUpMultiplier:   0.8   // Easier climb
climbDownMultiplier: 1.1   // Slight speed boost down
horizontalMultiplier: 0.7  // More horizontal freedom
```

## Files Modified
- `ENV_Ladder.cs` - New ladder component
- `P_Controller.cs` - Added ladder enter/exit/modifier methods
- `P_State_Movement.cs` - Applies ladder velocity modifiers

## Notes
- Ladder doesn't prevent attacking/dodging (handled by state system)
- Multiple ladders can exist; player tracks current ladder
- Gravity scale restoration ensures no permanent physics changes
- System integrates with existing controller/state architecture

🎯 **Result:** Realistic ladder climbing that feels responsive and physically accurate!
