# NinjaAdventure - Documentation
**Last Updated:** October 15, 2025  
**Current Week:** Week 9 (Oct 14-18, 2025)

---

## 📋 Quick Navigation

- **[Week 9 - All Completed Systems](Week_9_Oct14-18/README.md)** ⭐ **START HERE**
- **[Current Work in Progress](#-work-in-progress)** 🚧
- **[TODO - Future Features](#-todo-incomplete-features)** 🚧
- **[Balance References](#-balance-references)** 📊

---

## 🚧 Work in Progress

**Current work (still in Week 9):**

- **[ITEM_INFO_POPUP_PLAN.md](ITEM_INFO_POPUP_PLAN.md)** - Shared tooltip popup
  - Status: ✅ Code Complete - Unity Setup Pending
  - Shows item/weapon stats on hover
  - Quick Unity steps: [UNITY_SETUP_GUIDE.md](UNITY_SETUP_GUIDE.md)

---

## ✅ What's Complete (Week 9)

**Location:** `Week_9_Oct14-18/` folder

**9 Production-Ready Systems:**
1. Combo System (3-hit melee)
2. State System (concurrent states)
3. Weapon System (anchoring, penalties)
4. Enemy AI (full state machine)
5. Enemy Attack Patterns
6. Inventory System (items + weapons, drag/drop)
7. Supporting documentation

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

## 📂 Folder Structure

```
Docs/
├── README.md                           ← You are here
├── Week_9_Oct14-18/                    ✅ All completed work (9 systems)
├── ITEM_INFO_POPUP_PLAN.md             🚧 Current WIP (Item popup)
├── UNITY_SETUP_GUIDE.md                🚧 Unity instructions for popup
├── INVENTORY_UI_HIERARCHY.md           📊 UI structure reference
├── BALANCE_*.md                        📊 Balance references (4 files)
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
1. Read [Week 9 README](Week_9_Oct14-18/README.md) for production systems
2. Check [Work in Progress](#-work-in-progress) for current tasks
3. Check files 1-9 in Week 9 folder (in order)
4. Use balance refs for stats

**Need to...**
- See current work? → [Work in Progress](#-work-in-progress)
- Learn systems? → [Week 9](Week_9_Oct14-18/README.md)
- See what's done? → Check `Week_9_Oct14-18/` folder
- Find TODOs? → [ULTIMATE_SKILLS_SYSTEM.md](ULTIMATE_SKILLS_SYSTEM.md)
- Check stats? → [Balance refs above](#-balance-references)

---

**Status:** ✅ Week 9 Complete (9 systems) | 🚧 Item Popup WIP (still Week 9)
