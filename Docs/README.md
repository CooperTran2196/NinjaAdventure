# NinjaAdventure - Documentation
**Last Updated:** October 17, 2025  
**Current Week:** Week 10 (Oct 21-25, 2025)

---

## 📋 Quick Navigation

- **[Week 10 Audio - Latest](Week_10_Oct21-25/README.md)** ⭐ **COMPLETE**
- **[Week 10 Destructibles](Week_10_Oct14-18/README.md)** ✅ **COMPLETE**
- **[Week 9 - All Core Systems](Week_9_Oct14-18/README.md)** ✅ **COMPLETE**
- **[Guild - References](Guild/)** � **LIVING DOCS**
- **[TODO - Future Features](#-todo-incomplete-features)** 🚧

---

## ✅ What's Complete

### **Week 10 (Oct 21-25, 2025)** - Audio System ⭐ **LATEST**
**Location:** `Week_10_Oct21-25/` folder

**Systems Completed:**
1. Complete Audio System (41 sounds, 17 files integrated)
2. SYS_SoundManager with AudioSource pooling

👉 **[Read Week 10 Audio README](Week_10_Oct21-25/README.md)** for details

### **Week 10 (Oct 14-18, 2025)** - Environment & Polish
**Location:** `Week_10_Oct14-18/` folder

**Systems Completed:**
1. Destructible Objects & Particle System

👉 **[Read Week 10 Destructibles README](Week_10_Oct14-18/README.md)** for details

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

## � Guild - General References

**Location:** `Guild/` folder - Living docs for quick reference (updated frequently)

**Balance & Design:**
- **[BALANCE_QUICK_REF.md](Guild/BALANCE_QUICK_REF.md)** - Quick stat lookups
- **[COMPLETE_BALANCE_v2.md](Guild/COMPLETE_BALANCE_v2.md)** - Full balance specs
- **[ENEMY_BALANCE_GUIDE.md](Guild/ENEMY_BALANCE_GUIDE.md)** - Enemy design
- **[WEAPON_SKILL_BONUSES_GUIDE.md](Guild/WEAPON_SKILL_BONUSES_GUIDE.md)** - Skill bonuses

**Code & Structure:**
- **[CODING_STYLE_GUIDE.md](Guild/CODING_STYLE_GUIDE.md)** - Code conventions & patterns
- **[HIERARCHY.md](Guild/HIERARCHY.md)** - Complete Unity scene hierarchy (Updated: Oct 15, 2025)

**Comparisons:**
- **[GODOT_VS_UNITY_COMPARISON.md](Guild/GODOT_VS_UNITY_COMPARISON.md)** - Progress assessment vs Godot codebase

---

## 📂 Folder Structure

```
Docs/
├── README.md                           ← You are here (rules + navigation)
│
├── Week_10_Oct21-25/                   ✅ Complete (audio system)
├── Week_10_Oct14-18/                   ✅ Complete (destructibles)
├── Week_9_Oct14-18/                    ✅ Complete (10 combat/inventory systems)
│
├── Guild/                              � General references (living docs)
│   ├── BALANCE_QUICK_REF.md            📊 Quick stat lookups
│   ├── COMPLETE_BALANCE_v2.md          📊 Full balance specs
│   ├── ENEMY_BALANCE_GUIDE.md          📊 Enemy design
│   ├── WEAPON_SKILL_BONUSES_GUIDE.md   📊 Skill bonuses
│   ├── CODING_STYLE_GUIDE.md           📊 Code conventions
│   ├── HIERARCHY.md                    📊 Unity scene hierarchy
│   └── GODOT_VS_UNITY_COMPARISON.md    📊 Progress assessment
│
└── ULTIMATE_SKILLS_SYSTEM.md           🚧 Future TODO (not implemented)
```

---

## 📜 Documentation Rules (For AI)

**CRITICAL - Read this EVERY time:**

### **📂 Folder Structure Rules**

1. **ALL new docs go in `Docs/` folder** - NEVER create docs in workspace root
2. **Week folders only created when user says so** - Don't auto-create new weeks
3. **Current week continues** until user explicitly says "start Week X"
4. **Work-in-progress docs** stay in main `Docs/` folder (not in week folder yet)
5. **When work completes** - User decides if/when to move to week folder

### **📁 Guild Folder (General References)**

**Location:** `Docs/Guild/`  
**Purpose:** Keep general reference docs that I or AI need to recheck later

**What goes in Guild folder:**
- ✅ Balance references (BALANCE_QUICK_REF.md, COMPLETE_BALANCE_v2.md, etc.)
- ✅ Code style guides (CODING_STYLE_GUIDE.md)
- ✅ System hierarchies (HIERARCHY.md)
- ✅ Enemy design guides (ENEMY_BALANCE_GUIDE.md)
- ✅ Weapon/skill guides (WEAPON_SKILL_BONUSES_GUIDE.md)
- ✅ Any "living docs" that get updated frequently
- ✅ Quick reference sheets for both human and AI

**What does NOT go in Guild folder:**
- ❌ Week-specific implementation guides
- ❌ Tutorial/how-to docs for specific features
- ❌ Completed system documentation
- ❌ WIP docs (stay in main Docs/ until done)

### **📅 Weekly Folder Rules**

1. **Weekly Folders:** `Week_X_[MonthDay-Day]/` for COMPLETED work only
2. **File Numbers:** Prefix with `1_`, `2_`, `3_`... (order completed)
3. **One File Per System:** Consolidate related docs into single comprehensive file
4. **Week README:** Each week needs summary of all systems
5. **Dates:** Include created/completed dates in week docs
6. **Status:** ✅ Complete | 📊 Reference | 🚧 TODO | ⭐ Latest

### **🧹 Documentation Quality Rules**

1. **Consolidate:** Merge similar/related docs into ONE comprehensive file
   - Example: Audio system had 3 separate files → consolidated into 1
   - Avoid: "Part 1", "Part 2", "Setup Guide" as separate files
   - Prefer: Single file with all sections (Overview, Setup, API, Testing)

2. **Main Docs/ Clean:** Only these types of files allowed in main `Docs/`:
   - Week folders (Week_9_Oct14-18/, Week_10_Oct21-25/)
   - Guild folder (general references)
   - README.md (this file)
   - WIP docs currently being worked on
   - Future TODO planning docs (ULTIMATE_SKILLS_SYSTEM.md)

3. **Short READMEs:** Navigation only - technical details in week folders
   - Main README: Links to weeks + quick status
   - Week README: Links to systems + brief descriptions
   - System files: Full technical implementation details

4. **Delete Old Files:** When consolidating or updating:
   - Delete superseded versions
   - Delete scattered/duplicate files
   - Keep only the latest consolidated version

### **✍️ Before Creating ANY New File:**

- ✅ Check if we're still in current week (default: yes)
- ✅ Is this a general reference? → Put in `Docs/Guild/`
- ✅ Is this WIP for current week? → Put in `Docs/` (main folder)
- ✅ Update main README to show it as WIP
- ✅ Can this be merged with existing doc? → Consolidate instead of creating new
- ❌ DON'T create new week folders without user permission
- ❌ DON'T create separate files for setup/implementation/testing → Make it one file

---

## 🚀 Quick Start

**New developer?**
1. Read [Week 10 README](Week_10_Oct14-18/README.md) for current work
2. Read [Week 9 README](Week_9_Oct14-18/README.md) for production systems
3. Check [Work in Progress](#-work-in-progress) for current tasks
4. Use balance refs for stats

**Need to...**
- See latest work? → [Week 10 Audio](Week_10_Oct21-25/README.md)
- Learn production systems? → [Week 9](Week_9_Oct14-18/README.md)
- Understand hierarchy? → [Guild/HIERARCHY.md](Guild/HIERARCHY.md)
- Check code style? → [Guild/CODING_STYLE_GUIDE.md](Guild/CODING_STYLE_GUIDE.md)
- Find TODOs? → [ULTIMATE_SKILLS_SYSTEM.md](ULTIMATE_SKILLS_SYSTEM.md)
- Check stats? → [Guild folder references](#-guild---general-references)

---

**Status:** 🚧 Week 10 IN PROGRESS | ✅ Week 9 COMPLETE (10 systems) | 🗂️ Hierarchy Documented  
**Last Updated:** October 17, 2025
