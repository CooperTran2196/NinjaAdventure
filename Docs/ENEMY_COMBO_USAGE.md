# 🗡️ Enemy Combo Attack System

## Overview

Enemies can now use any of the 3 combo attacks (Slash Down, Slash Up, Thrust) **without chaining them**. Each enemy simply picks ONE attack pattern and uses it every time.

**Status:** ✅ Ready to Use  
**Simplicity:** Enemies don't combo — they just pick one attack pattern

---

## Quick Setup in Unity Inspector

1. **Select your enemy GameObject** in the scene/prefab
2. **Find the `State_Attack` component** in the Inspector
3. **Set "Selected Combo Attack"** (0-2):
   - `0` = Slash Down (arc sweeps downward)
   - `1` = Slash Up (arc sweeps upward)
   - `2` = Thrust (forward stab with knockback)

That's it! Your enemy will now use that specific attack pattern.

---

## Attack Patterns Explained

### 0 - Slash Down 🔽
- Arc sweeps **downward** (counter-clockwise → clockwise)
- Damage: `1.0x` base
- Stun: `0.1s`
- Movement Penalty: `60%`
- **Best for:** Fast, aggressive enemies

### 1 - Slash Up 🔼
- Arc sweeps **upward** (clockwise → counter-clockwise)
- Damage: `1.2x` base
- Stun: `0.2s`
- Movement Penalty: `50%`
- **Best for:** Medium enemies with balanced attacks

### 2 - Thrust ➡️
- Forward **stab** with knockback
- Damage: `2.0x` base
- Stun: `0.5s`
- Movement Penalty: `30%`
- **Only attack that applies knockback!**
- **Best for:** Heavy/boss enemies, finisher move

---

## Example Enemy Configurations

### Fast Goblin (Slash Down)
```
Selected Combo Attack: 0
Attack Cooldown: 1.0s
Attack Range: 1.2
```
- Rapid weak slashes, high attack frequency

### Standard Skeleton (Slash Up)
```
Selected Combo Attack: 1
Attack Cooldown: 1.5s
Attack Range: 1.5
```
- Balanced damage and frequency

### Heavy Knight/Boss (Thrust)
```
Selected Combo Attack: 2
Attack Cooldown: 2.5s
Attack Range: 1.8
```
- Devastating thrust attacks with knockback, slower but deadly

---

## Technical Details

### Code Changes

**State_Attack.cs:**
- Added `selectedComboAttack` field (Inspector-editable)
- Reads `comboShowTimes[selectedComboAttack]` for timing
- Calls `W_Melee.AttackAsEnemy(attackDir, selectedComboAttack)` for melee weapons

**W_Melee.cs:**
- Added `AttackAsEnemy(Vector2 aimDir, int comboAttackIndex)` method
- Allows enemies to bypass player combo state and directly specify attack index
- Reuses all existing combo pattern logic (arc slash, thrust, damage multipliers)

### No Changes Needed For:
- ✅ Enemy movement/chase/idle states
- ✅ Weapon ScriptableObjects (same W_SO used by player)
- ✅ Hit detection/damage calculation
- ✅ Animation system

---

## Weapon SO Configuration

Enemies use the **same** combo configuration as the player's weapon:

```
W_SO:
  slashArcDegrees: 45°
  comboShowTimes: [0.3, 0.3, 0.5]
  comboDamageMultipliers: [1.0, 1.2, 2.0]
  comboStunTimes: [0.1, 0.2, 0.5]
  onlyThrustKnocksBack: true
```

This ensures **consistency** — if you tune weapon values, both player and enemies update automatically.

---

## Testing Workflow

1. **Create enemy prefab** with melee weapon (W_Melee component as child)
2. **Set selectedComboAttack** on State_Attack component
3. **Play test** and observe the attack pattern
4. **Adjust** attackCooldown and attackRange to balance difficulty
5. **Tune** damage multipliers in W_SO if needed

---

## Design Variety Tips

Mix different attack patterns across enemy types for variety:

- **Trash Mobs:** Slash Down (0) — fast, weak
- **Standard Enemies:** Slash Up (1) — balanced
- **Elite/Mini-Boss:** Thrust (2) — slow, heavy-hitting with knockback
- **Boss Phases:** Start with Slash Up, switch to Thrust in second phase

You can even have **same enemy type with different attacks** for unpredictability!

---

## Future Extensions (Not Implemented Yet)

Possible enhancements:
- **Random attack selection** per attack (enemy picks 0-2 randomly)
- **Attack pattern AI** (enemy chooses based on distance/health)
- **Boss combo chains** (special enemies that DO chain attacks like player)

For now, keep it simple: **one enemy = one attack pattern**.

---

## FAQ

**Q: Can enemies chain combos like the player?**  
A: No, this system is intentionally simple. Enemies pick ONE attack and use it every time.

**Q: Can I change the attack pattern at runtime?**  
A: Not currently implemented, but you could script it by modifying `State_Attack.selectedComboAttack` via code.

**Q: Do I need separate weapons for each attack pattern?**  
A: No! Same weapon works for all patterns. Just change `selectedComboAttack` value.

**Q: What if my enemy uses ranged weapons?**  
A: The system falls back to standard `Attack()` for non-melee weapons. Only W_Melee uses combo patterns.

---

## Summary

✅ **Simple:** Just set one number in Inspector (0, 1, or 2)  
✅ **Consistent:** Uses same weapon data as player  
✅ **Variety:** Mix different attacks across enemy types  
✅ **No Chaining:** Enemies don't combo (keeps AI simple)  
✅ **Battle Tested:** Reuses proven player combo code

**Ready to use — no additional setup required!** 🎮
