# NinjaAdventure - Documentation
**Last Updated:** October 17, 2025  
**Current Week:** Week 10 (Oct 14-18, 2025)

---

## 📋 Quick Navigation

- **[Week 10 - Current Work](Week_10_Oct14-18/README.md)** 🚧 **IN PROGRESS**
- **[Week 9 - All Completed Systems](Week_9_Oct14-18/README.md)** ⭐ **COMPLETE**
- **[Current Work in Progress](#-work-in-progress)** 🚧
- **[TODO - Future Features](#-todo-incomplete-features)** 🚧
- **[Balance References](#-balance-references)** 📊

---

## 🚧 Work in Progress

**Current Week:** Week 10 (Oct 14-18, 2025)

**Recently Completed:**
- ✅ Destructible objects with particle effects (Week 10)

**Active Work:**
- 🚧 More Week 10 features coming...

**Previous Completions:**
- ✅ Week 9: All 10 combat & inventory systems complete!
  - Item Info Popup system (hover tooltips)
  - Fixed ExpUI, ManaUI, HealthUI null reference errors
  - Added hover delay system (1s) for inventory tooltips
  - Removed weaponNameText from WeaponUI (cleaner display)
  - Added aspect ratio preservation for weapons in inventory

---

## ✅ What's Complete

### **Week 10 (Oct 14-18, 2025)** - Environment & Polish
**Location:** `Week_10_Oct14-18/` folder

**Systems Completed:**
1. Destructible Objects & Particle System

👉 **[Read Week 10 README](Week_10_Oct14-18/README.md)** for details

### **Week 9 (Oct 14-18, 2025)** - Combat & Inventory
**Location:** `Week_9_Oct14-18/` folder

**10 Production-Ready Systems:**
1. Combo System (3-hit melee)
2. Combo Code Explanation
3. State System (concurrent states)
4. Weapon System (anchoring, penalties)
5. Enemy AI (full state machine)
6. Enemy Fixed Attack Patterns
7. Enemy Random Attack Patterns
8. Coordinate System Reference
9. Inventory System (items + weapons, drag/drop)
10. Item Info Popup (hover tooltips with smart delays)

👉 **[Read Week 9 README](Week_9_Oct14-18/README.md)** for details

---

## 🚧 TODO (Incomplete Features)

**[ULTIMATE_SKILLS_SYSTEM.md](ULTIMATE_SKILLS_SYSTEM.md)** - Ultimate abilities (E/R keys)
- Status: 🚧 Planning only - NOT implemented
- Dagger Barrage (E key) - Projectile attack
- Divine Intervention (R key) - Invulnerability
- Priority: Low

---

## 📊 Balance References

**Living docs - updated as balance changes:**

- **[BALANCE_QUICK_REF.md](BALANCE_QUICK_REF.md)** - Quick stat lookups
- **[COMPLETE_BALANCE_v2.md](COMPLETE_BALANCE_v2.md)** - Full balance specs
- **[ENEMY_BALANCE_GUIDE.md](ENEMY_BALANCE_GUIDE.md)** - Enemy design
- **[WEAPON_SKILL_BONUSES_GUIDE.md](WEAPON_SKILL_BONUSES_GUIDE.md)** - Skill bonuses

---

## 🗂️ Structure References

**Game hierarchy and organization:**

- **[HIERARCHY.md](HIERARCHY.md)** - Complete Unity scene hierarchy
  - All GameObjects, components, and prefabs
  - UI system structure (PlayerCanvas, InventoryCanvas, ShopCanvas, etc.)
  - Player/Enemy hierarchies
  - System connections and data flow
  - **Updated:** October 15, 2025

---

## 📂 Folder Structure

```
Docs/
├── README.md                           ← You are here
├── Week_10_Oct14-18/                   🚧 Current work (environment systems)
├── Week_9_Oct14-18/                    ✅ Complete (10 combat/inventory systems)
│
├── HIERARCHY.md                        📊 Complete Unity hierarchy (Level1 scene)
├── CODING_STYLE_GUIDE.md               📊 Code conventions & P_InputActions patterns
│
├── BALANCE_QUICK_REF.md                📊 Balance references
├── COMPLETE_BALANCE_v2.md              📊 (4 files total)
├── ENEMY_BALANCE_GUIDE.md              📊
├── WEAPON_SKILL_BONUSES_GUIDE.md       📊
│
└── ULTIMATE_SKILLS_SYSTEM.md           🚧 Future TODO
```

---

## 📜 Documentation Rules (For AI)

**CRITICAL - Read this EVERY time:**

1. **ALL new docs go in `Docs/` folder** - NEVER create docs in workspace root
2. **Week folders only created when user says so** - Don't auto-create new weeks
3. **Current week continues** until user explicitly says "start Week X"
4. **Work-in-progress docs** stay in main `Docs/` folder (not in week folder yet)
5. **When work completes** - User decides if/when to move to week folder

**Weekly folder rules:**

1. **Weekly Folders:** `Week_X_[MonthDay-Day]/` for COMPLETED work only
2. **File Numbers:** Prefix with `1_`, `2_`, `3_`... (order completed)
3. **Week README:** Each week needs summary of all systems
4. **Dates:** Include created/completed dates in week docs
5. **Status:** ✅ Complete | 📊 Reference | 🚧 TODO | ⭐ Latest
6. **Main Clean:** Only week folders + balance refs + WIP docs in main Docs/
7. **Consolidate:** Merge similar docs, delete old scattered files
8. **Short READMEs:** Navigation only - technical details in week folders
9. **Complete TODOs:** Move to week folder when done (only when user says so)

**Before creating ANY new file:**
- ✅ Check if we're still in current week (default: yes)
- ✅ Put it in `Docs/` folder
- ✅ Update main README to show it as WIP
- ❌ DON'T create new week folders without user permission

---

## 🚀 Quick Start

**New developer?**
1. Read [Week 10 README](Week_10_Oct14-18/README.md) for current work
2. Read [Week 9 README](Week_9_Oct14-18/README.md) for production systems
3. Check [Work in Progress](#-work-in-progress) for current tasks
4. Use balance refs for stats

**Need to...**
- See current work? → [Week 10](Week_10_Oct14-18/README.md)
- Learn production systems? → [Week 9](Week_9_Oct14-18/README.md)
- Understand hierarchy? → [HIERARCHY.md](HIERARCHY.md)
- Check code style? → [CODING_STYLE_GUIDE.md](CODING_STYLE_GUIDE.md)
- Find TODOs? → [ULTIMATE_SKILLS_SYSTEM.md](ULTIMATE_SKILLS_SYSTEM.md)
- Check stats? → [Balance refs above](#-balance-references)

---

**Status:** 🚧 Week 10 IN PROGRESS | ✅ Week 9 COMPLETE (10 systems) | 🗂️ Hierarchy Documented  
**Last Updated:** October 17, 2025
