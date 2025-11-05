# Guild - General References
**Purpose:** Living documentation for quick reference and rechecking  
**For:** Both human developers and AI assistants  
**Updated:** November 4, 2025

---

## 📚 What's in Guild Folder

This folder contains **general reference documents** that don't belong to any specific week. These are "living docs" that get updated frequently and are used for quick lookups.

---

## 🎯 **PRIMARY REFERENCE: GAME_BALANCE.md** ⭐

**[GAME_BALANCE.md](GAME_BALANCE.md)** - **USE THIS FIRST!**
**Complete game balance in ONE file. Everything you need.**

**Contains:**
- ✅ Player stats & progression (Level 1 → 23)
- ✅ All 9 weapon stats with exact Unity changes needed
- ✅ Complete enemy tables for all 3 maps (organized by no-weapon/weapon/boss)
- ✅ XP distribution (2,400 / 3,000 / 3,045 XP)
- ✅ Skill tree path (80% coverage, 44 SP)
- ✅ Implementation checklist
- ✅ Testing procedures

**Quick Facts:**
- 681 total enemies
- 8,445 total XP (80%+ guaranteed if kill most)
- 60-75 minute playtime
- Final Boss drops nothing (just end game)

**Weapon Changes Summary:**
- Stick: +3 attack speed
- Katana: 0 attack speed (baseline)
- Sword2: +1 attack speed, AD 4 (all-around for normal players)
- Lance: AD 6, -5 speed (2nd highest damage, spacing/skill weapon)
- Axe: AD 8, -6 speed (highest damage, brute force tank weapon)

**Player Types:**
- Tank/Brute Force → Axe (slow, highest damage, build AR/MR/HP)
- Skilled/Spacing → Lance (2nd damage, long range, positioning)
- Normal/Balanced → Sword2 (good damage, less move penalty, reliable)

---

## 📊 Previous Balance References (Archive)

**[BALANCE_QUICK_REF.md](BALANCE_QUICK_REF.md)**  
Quick stat lookups. (Older version, use GAME_BALANCE.md instead)

**[COMPLETE_BALANCE_v2.md](COMPLETE_BALANCE_v2.md)**  
Full balance specifications. (Older version, use GAME_BALANCE.md instead)

**[ENEMY_BALANCE_GUIDE.md](ENEMY_BALANCE_GUIDE.md)**  
Enemy design templates. (Older version, use GAME_BALANCE.md instead)

**[WEAPON_SKILL_BONUSES_GUIDE.md](WEAPON_SKILL_BONUSES_GUIDE.md)**  
Skill tree bonuses, weapon stat modifiers.

---

### **WEAPON_BALANCE_CHANGES.md** � **QUICK FIX GUIDE**
**Exact weapon changes needed in Unity (5 minutes to apply)**
- 6 weapons requiring AD updates: Sword2, Axe, Lance, Bow, Shuriken, CFG_Katana
- Copy-paste values for Unity Inspector
- Before/after damage comparisons
- Testing procedures

**Use this first:** Apply these changes immediately to fix weapon progression

---

### **ENEMY_STATS_REFERENCE.md** 📋 **COPY-PASTE READY**
**Enemy prefab creation guide (2-3 hours)**
- Complete stats for all 12 enemy types
- Copy-paste values for E_Stats, E_Collision, E_Controller, E_Reward
- Enemy distribution per map
- Movement speed rules (MS 2.5 vs 2.0)
- Implementation checklist

**Use this for:** Creating enemy prefabs in Unity

---

### **GAME_PROGRESSION_FLOWCHART.md** 📈 **VISUAL GUIDE**
**High-level game flow visualization**
- Map-by-map progression flowchart
- Weapon unlock timeline
- Player power curve graphs
- Enemy difficulty progression
- XP distribution charts
- Recommended skill build paths

**Use this for:** Understanding game pacing & player experience

---

## 📊 Previous Balance References (Archive)

**[BALANCE_QUICK_REF.md](BALANCE_QUICK_REF.md)**  
Quick stat lookups for player, enemies, weapons, items. (Older version)

**[COMPLETE_BALANCE_v2.md](COMPLETE_BALANCE_v2.md)**  
Full balance specifications with formulas, stat interactions, level curves. (Older version)

**[ENEMY_BALANCE_GUIDE.md](ENEMY_BALANCE_GUIDE.md)**  
Enemy design templates, AI behavior stats, difficulty scaling. (Older version)

**[WEAPON_SKILL_BONUSES_GUIDE.md](WEAPON_SKILL_BONUSES_GUIDE.md)**  
Skill tree bonuses, weapon stat modifiers, progression systems.

---

## 🛠️ Code & Structure References

**[CODING_STYLE_GUIDE.md](CODING_STYLE_GUIDE.md)**  
Code conventions, naming patterns, P_InputActions usage, architecture rules.

**[HIERARCHY.md](HIERARCHY.md)**  
Complete Unity scene hierarchy for Level1. All GameObjects, components, UI structure, data flow.  
**Updated:** October 15, 2025

---

## 📈 Progress & Comparisons

**[GODOT_VS_UNITY_COMPARISON.md](GODOT_VS_UNITY_COMPARISON.md)**  
Progress assessment comparing Unity codebase vs original Godot prototype.

---

## 🎯 When to Use Guild Docs

**Use Guild folder for:**
- ✅ Quick stat lookups during development
- ✅ Checking code conventions before writing
- ✅ Understanding scene hierarchy
- ✅ Balance validation during playtesting
- ✅ General "how things work" questions
- ✅ Docs that get updated across multiple weeks

**Don't use Guild folder for:**
- ❌ Week-specific implementation guides
- ❌ Tutorial/how-to docs for new features
- ❌ System documentation (goes in week folders)
- ❌ Work-in-progress docs (stay in main Docs/ until done)

---

## 📝 Updating Guild Docs

**These docs are LIVING references:**
- Update them when balance changes
- Update them when code patterns evolve
- Update them when hierarchy changes
- Keep them accurate and current
- Date stamp major updates

**Don't:**
- Create new versions (update existing files)
- Move to week folders (they stay here)
- Let them get outdated

---

**Status:** 📚 7 reference documents | 🔄 Updated frequently  
**Back to:** [Main Docs README](../README.md)
