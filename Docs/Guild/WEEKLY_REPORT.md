CS 4395 – Weekly Progress Report Template
Student Name:
Student ID:
________________
Cooper Tran
__________________________
2106638
_________________
____________________________
Advisor Name:
__________________
Ling Xu
________________________
Week Number:
2
Date Range ():
________________
__________________________
_______
Aug 18 - Augus 29
__________________
1. Summary of Work Completed This Week
Unity setup:
I set up a clean Unity project with the new Input System for Move (WASD) and Attack
(Space). I imported the asset pack, created the main GameObjects, and set up animations for
the player and the samurai enemy.
LDtk mapping:
I created an LDtk project and started drawing Level 1. I exported it to Unity and confirmed
the sorting layers and collisions look correct.
Player scripts:
● P
_
Stats.cs: holds player stats like HP and max HP .
● P
_
Movement.cs: reads input (WASD), moves the player, updates facing/animator.
● P
_
Combat.cs: handles TakeDamage/Heal, red flash on hit, fade and destroy on death,
basic attack timing/cooldown, and a debug hotkey to take damage.
Enemy scripts:
● E
_
Stats.cs: holds enemy stats like HP and max HP .
● E
_
Movement.cs: detects the player, starts/stops chasing, applies velocity, updates
facing/animator; shows gizmo circles for detection range.
● E
_
Combat.cs: plays attack, applies damage, red flash on hit, fade and destroy on
death; shows gizmo for attack range.
Overall:
Core gameplay loop (move, detect, chase, attack, damage/heal, death) is working for
Player/Enemy with a clean and minimal code style.
I also built a simple enemy AI that:
● Detects the player inside a radius.
● Faces the player and chases until in attack range.
● Attacks only when close enough and facing the correct direction, playing the correct
animation.
● Stops chasing and returns to idle when the player leaves the detection range.
● Keeps animation logic clear using stage flags: isIdle, isMoving, isAttacking (only one
true at a time).
2. Challenges or Issues Encountered
Contact damage “tick” behavior:
At first, touching the enemy sometimes dealt damage twice and then stopped while I was
idle. I fixed this by handling contact damage in one place and adding a short internal
cooldown to avoid double hits from physics steps.
Clear animation stages:
I added simple bools for each stage on both player and enemy: isIdle, isMoving, isAttacking.
This keeps them in only one stage at a time, so my animator logic is much clearer and less
confusing.
3. Plans for Next Week
Weapon system:
● Create a ScriptableObject for weapon data (sprite, damage, offsets, thrust distance,
etc.).
● Make a W
_
Manager to hold an inspector-tunable list for the Player (future UI
hotkeys 1–9).
● Focus on wiring & animation events only (spawn/position next to the character
based on facing).
4. Feedback from Advisor (if any)
I will present my project around mid-November.
She suggested I stop coding and wrap up by the end of October so I have time for slides,
demo, and documentation.
I planned 3 levels. She advised me to finish them one by one, not in parallel.
She said my project coding and setup are clear right now.
Student Signature:
Advisor Signature:
_____________________________
____________________________
Date:
___________
Date:
___________


CS 4395 – Weekly Progress Report Template
Student Name:
Student ID:
________________
Cooper Tran
__________________________
2106638
_________________
____________________________
Advisor Name:
__________________
Ling Xu
________________________
Week Number:
3
Date Range ():
________________
__________________________
_______
Aug 29 - Sep 7, 2025
__________________
1. Summary of Work Completed This Week
Core refactor:
● Introduced C
_
Stats and migrated Player and Enemy to use this single component
(replacing separate P
_
Stats/E
_
Stats).
● Simplified health and damage code paths by reading from one stats source; removed
many null/ternary checks in hot paths and keep checks in Awake only.
● Introduced C
_
FX for quick hit/death effects: simple red flase for damaged, green
flash for healing coroutine and FadeAndDestroy() (alpha 1→0 over deathFadeTime).
This will replace old Flash function that lived inside Health class.
Visual and feedback utilities:
● Added C
_
Anim helper to keep animator booleans and directional floats consistent
(e.g., isMoving/isAttacking), used by both player and enemy to avoid desync.
Movement/dodge system:
● Added C
_
Dodge: dash distance/speed/cooldown with a ForcedVelocity that
movement respects during the dash. Works with current “lock during attack” valve
logic.
● Added C
_
AfterimageSpawner: lightweight afterimage trail (spawn+fade) triggered
during dodge/fast actions to improve motion feedback.
2. Challenges or Issues Encountered
Flash sticking: earlier red-flash sometimes lingered; simplified C
_
FX.Flash() timing to
ensure tint always restores after WaitForSeconds.
Animator state clarity: mixed writes from different scripts caused occasional desync;
consolidated updates through C
_
Anim so one place controls booleans/direction.
Dodge vs. movement valve: ensured ForcedVelocity cleanly overrides base velocity during
the dash, then hands control back smoothly, so no more stutter and queued extra inputs.
Afterimage tuning: adjusted lifetime and spawn cadence so the trail reads clearly without
visual spam; confirmed sorting order so it renders behind the character.
Weapon classes: weapon classes introduced many errors due to character’s facing sync in
4/8 directions in Player and Enemy movement classes. I need to introduce new class that
control all character states to sync them all in 1 place before doing anything related to
attacking state.. Will be introduce next week.
3. Plans for Next Week
Weapon classes:
● Create W
_
SO: This will hold all stats from current weapon that attached to a
character.
● Create W
Base + W
_
_
Melee swing: enable sprite/collider during window; thrust
along facing; simple per-swing de-dup.
● Create: W
Knockback and W
_
_
Stun scripts and hook into current animator attack
timing (hitDelay / attackDuration).
● Keep scope tight: focus on clean wiring and consistency with C
_
Stats/C
_
Anim/C
_
FX.
4. Feedback from Advisor (if any)
No meeting this week.
Student Signature:
Date:
Advisor Signature:
_____________________________
____________________________
Date:
___________
___________


CS 4395 – Weekly Progress Report Template
Student Name:
Student ID:
Advisor Name:
Week Number:
Date Range ():
________________
Cooper Tran
__________________________
2106638
_________________
____________________________
__________________
Ling Xu
________________________
3
________________
__________________________
_______
Sep 8 - Sep 12, 2025
__________________
1. Summary of Work Completed This Week
Weapons:
● Switched from 8-direction snap to free aim. Now I compute a clean vector from the
owner to the target (mouse for player / player for enemies), convert to an angle, and
drive all weapons with that).
● Centralized helpers in W
_
Base: polar offset placement around the owner, sprite
angle, and a short melee thrust motion.
● Unified target filtering: ignore owner, ignore weapons, respect layers. So melee and
projectiles behave the same.
● Fixed melee double-hit by tracking per-swing hits and projectile pierce so arrows
stop exactly when they should
● Standardized effect order: ApplyDamage -> Knockback -> optional Stun
● Locked attack facing for the duration of a swing so animation and aim matched.
● One shared pipeline for melee and ranged -> simpler maintenance and consistent
feel
Combat UI:
● Enemy Health UI: small world-space health bar that follows each enemy and updates
on damage.
● Player Health UI: simple slider showing current HP / max HP and wired to health
events.
● Player Stats UI: clean stats panel that auto-updates on stat changes.
2. Challenges or Issues Encountered
● The 8-direction snap felt imprecise, especially on diagonals and quick turns.
Switching to free aim solved alignment and made the game feel smoother.
● Small tuning on melee thrust/pierce counts and the enemy health bar follow
behavior.
3. Plans for Next Week
● Skill Tree UI & Scripts: build the Skill Tree panel. So upgrades truly change gameplay.
● Implement one Lifesteal skill and basic stat upgrades.
● When upgrading MaxHP , update Health UI immediately; when upgrading
attack/defense/speed, update Stats UI immediately.
4. Feedback from Advisor (if any)
● Everything is going well now. I was stuck, but I made it through;
● If I get stuck too long next time, I’ll move on or choose a simpler approach first.
Student Signature:
Advisor Signature:
Date:
_____________________________
___________
____________________________
Date:
___________


CS 4395 – Weekly Progress Report Template
Student Name:
Student ID:
Advisor Name:
Week Number:
Date Range ():
________________
Cooper Tran
__________________________
2106638
_________________
____________________________
__________________
Ling Xu
________________________
5
________________
__________________________
_______
Sep 15 - Sep 19, 2025
__________________
1. Summary of Work Completed This Week
Central Stats Manager:
● Refactored old stats touched in many places code into one Stats Manager.
● It is now the single entry point for changing stats: permanent, instant, and overtime
effect.
● After any change, it recalculates all stats and sends one update event so the UI
refreshes cleanly.
● When Max Health changes, it clamps current HP to the new max to keep health valid.
Skill Tree:
● Built the full Skill Tree UI with nodes/slots, levels, and prerequisite unlocks.
● SkillSO design: each skill asset declares its kind (Stat or Lifesteal) and the per-level
value.
● For Stat skills (Attack, Defense, Move Speed, Max HP), each upgrade adds its amount
to the manager.
● For Lifesteal, total lifesteal = currentLevel * lifestealPerLevel, on dealt damage,
healing is applied automatically.
● Immediate recalculation: when a skill upgrades, the manager applies the delta and
recomputes totals right away.
● Opening/closing the Skill Tree uses a pause/unpause pattern so players can upgrade
cleanly.
2. Challenges or Issues Encountered
Max HP upgrades breaking current HP
● When MaxHP increased, the player’s current HP could briefly sit above the new
bounds (or show odd numbers during rapid changes).
● Fix: After any MaxHP change, I clamp current HP during the same recompute pass,
so the Health UI always shows a valid value on the very next frame.
Prerequisite/unlock edge cases
● Skills with parents could become “half-unlocked” if I upgraded quickly in an odd
order, leaving a child clickable when it shouldn’t be.
● Fix: On every relevant event (upgrade/maxed), I run a quick CanUnlock sweep
across the tree and flip lock states accordingly, so children only enable when all
parents meet their max-level gates.
Keeping the Skill Tree and Stats UI in perfect sync
● There were moments where the Stats UI showed the old numbers for one frame
right after an upgrade.
● Fix: I ensured the recalculation + event happens before unpausing/closing the panel,
so the UI has already updated when control returns to gameplay.
Central Stats Manager
Problems
● Old problem: stats lived everywhere, so numbers went stale.
● Different systems: combat, upgrades, temporary buffs were editing stats directly and
at different times. Some paths forgot to recompute totals or notify the UI.
I introduced a single Stats Manager as the only way to change stats:
● Inputs normalized: every change in skill upgrades, consumables, temporary buffs
comes in as a small “effect” like what stat, how much, for how long.
● Deterministic recompute: I apply all active effects in a fixed order and recalculate
totals once.
● Safety built-in: after MaxHP changes, I clamp HP immediately so values can’t go out
of bounds, lifesteal hooks into the final damage dealt so healing is correct.
● Timed effects registry: start/stop timers in one place, when a timer ends, remove its
modifier and recompute.
● Single event: after recompute, I fire one stats-updated event so UI refreshes exactly
once.
3. Plans for Next Week
● Inventory: basic item data, hotbar slots, gold display.
● Item Pickup: world loot that fires events and fills inventory, stacking and simple
drop.
● Shop Manager: shop UI with offers, buy/sell flow hooked into wallet and inventory.
4. Feedback from Advisor (if any)
● Everything is going well now. I was stuck, but I made it through.
● If I get stuck too long next time, I’ll move on or choose a simpler approach first.
Student Signature:
Advisor Signature:
_____________________________
Date:
Date:
___________
____________________________
___________


CS 4395 – Weekly Progress Report Template
Student Name:
Student ID:
Advisor Name:
Week Number:
Date Range ():
________________
Cooper Tran
__________________________
2106638
_________________
____________________________
__________________
Ling Xu
________________________
6
________________
__________________________
_______
Sep 22 - Sep 26, 2025
__________________
1. Summary of Work Completed This Week
Inventory system is working end to end:
● I built pickup, stacking, use, and drop. Loot in the world now raises an event when
the player touches it, then the item is added to inventory and the pickup anim plays.
Gold is a special case that adds straight to the wallet. Stacking fills partial stacks first,
then empty slots, and any overflow is dropped at the player’s feet.
Left and right mouse clicks now do real actions:
● Left click uses the item and applies all its configured effects through the stats
manager. Right click drops one item when the shop is closed. When the shop is open,
right click sells one item instead. The slot UI updates after every change.
Item effects apply to the player and stats recalc happens on time:
● Items use a shared effect format with stat name, value, and duration. The stats
manager handles permanent and instant effects in one pass, and it runs timers for
temporary effects. Over time healing ticks once per second. After any change it
recalculates final stats and clamps current health if Max Health changed, then raises
one update event for the UIs.
Shopping UI supports hover details and buying:
● Shop slots show name, icon, and price. Hover shows a floating info panel that lists all
stat effects and follows the mouse. The Buy button checks gold and space and then
adds the item to inventory.
Multiple shopkeepers work with categories and pause:
● Each shopkeeper holds its own item lists for Items, Weapons, and Armor. When the
player is in range and presses the toggle key, the shop opens, the game pauses, and
the correct category fills the grid. Closing resumes time. A global event tells
inventory slots whether the shop is open so right click sells instead of dropping.
2. Challenges or Issues Encountered
Making inventory clicks actually change gameplay:
● It was tricky to make left click and right click feel natural and safe. I had to route left
click into the inventory manager and then into the stats manager to apply every
effect on the item. I also had to prevent wasting clicks when the item had no effects.
Right click needed to drop one when the shop was closed, but sell one when the
shop was open.
● I solved this by listening to a shop state event in the slots and switching behavior
based on that state. The slot now updates itself after every action so the UI never
gets out of sync.
Building a real item effect system that talks to player stats:
● At first, item effects changed numbers but the UI lagged and temporary effects did
not end cleanly.
● I created a single entry point where items submit their list of effects. The stats
manager now applies permanent and instant changes in one pass and puts timed
effects in a list with timers. Over time healing ticks once per second and then stops.
After every update it recalculates all stats, clamps health if the max changed, and
fires one event that the UIs listen to. This removed flicker and stopped stale values.
Showing item info in the shop without clutter:
● The challenge was to generate readable lines for many different stats and keep the
panel near the cursor without blocking clicks.
● I built a small UI that clears and rebuilds lines for each stat and shows clean
messages like Heal, Max HP , or Lifesteal. It appears on hover, follows the mouse, and
hides on exit.
Supporting many shopkeepers with their own stock:
● I needed a way for each NPC to open the same shop UI but with different item lists.
● I solved it by letting the keeper call into the manager to populate slots with its lists
for Items, Weapons, or Armor. The keeper toggles the canvas, pauses the game, and
announces open or close with an event so other systems react. This made the
inventory UI sell mode work only while the shop is open.
Making pickup and stacking feel smooth:
● It was easy to fill empty slots but harder to merge partially filled stacks first and
handle overflow.
● I now add to existing stacks up to the stack size, then fill empty slots, and finally
drop the overflow near the player so nothing is lost. The gold path is special and
updates the wallet text directly.
NPC life is still missing:
● The keeper stands still and just opens the UI.
● I need movement, idles, and simple interaction so the world feels alive. There are
also small bugs in the shopping loop that I must fix before putting a real moving
keeper into the game.
3. Plans for Next Week
I will add a basic NPC system for shopkeepers with movement and idle animations. I will
add a simple interaction layer so they greet and respond. I will fix the remaining bugs in
buying and selling, including edge cases with stacking and space checks. I will add small
polish to the shop UI and inventory to make the flow clean.
4. Feedback from Advisor (if any)
No meeting this week.
Student Signature:
Date:
Advisor Signature:
_____________________________
Date:
____________________________
___________
___________


CS 4395 – Weekly Progress Report Template
Student Name:
________________
Cooper Tran
__________________________
Student ID:
2106638
_________________
____________________________
Advisor Name:
__________________
Ling Xu
________________________
Week Number:
7
________________
__________________________
Date Range ():
_______
Sep 29 - Oct 3, 2025
__________________
1. Summary of Work Completed This Week
Systems and Refactors:
● NPC and enemy characters now use the same controller facade. Each character runs
exactly one active state at a time, which makes behavior clear and prevents scripts
from fighting each other. States publish intent, and the controller applies movement
in a stable update, so timing is consistent.
● Added a new wander state for background life. The wander state tells the controller
where to move and the controller handles the actual motion. This same wander state
works for both NPC and enemy after the refactor, so adding ambient motion no
longer needs two separate implementations.
● When an NPC detects a talk request, control hands off cleanly to the talk state. The
NPC faces the player during the talk and returns to the old facing when the talk ends.
This flow is now consistent for all characters that can talk.
Dialog and Conversation:
● A player can approach an NPC and press the F key to start a talk. The NPC looks
toward the player and locks idle or movement during the talk. When the talk
finishes, the NPC restores the old facing and back to default state.
● The system decides which dialog to show. It can run one time or repeat. It can check
simple conditions like whether the player has talked to someone before or visited a
location. Dialog can set the next dialog to unlock and can grant a small reward. The
manager holds the data and the UI shows the speaker name, the line, and up to three
choices. The panel closes when the line or choices end.
● Interaction prompts appear when the player enters an NPC talk area and go away
when leaving. This keeps the screen clean and teaches the input without extra text.
Scene Transition and Persistence:
● Added a teleport script that works like a gate and also spawns the player in the next
scene. When the player enters the gate, the screen fades out, the target scene loads,
and the player appears at a set spawn point in the new scene. This keeps the
transition smooth and never shows a half loaded map.
● Kept core managers alive between scenes. A single game manager marks important
objects to not destroy on load and cleans up any extra copies. This removes many
null checks and keeps history and dialog tracking consistent across the whole game.
● On each scene load, the camera confiner binds to the new level bounds
automatically. This prevents the camera from drifting into empty space.
2. Challenges or Issues Encountered
State engine for all character
● Wander was added first for NPC, but the old enemy setup could not use it.
● Fix: Moving enemies to the same controller facade allowed Idle, Wander, Chase,
Attack, and Talk to be shared across all characters. This reduced code paths,
improved consistency, and made future changes safer.
UI clicks blocked by the fade overlay
● The full-screen FadeCanvas image intercepted raycasts while idle, so Inventory and
Dialog buttons could not be pressed.
● Fix: Raycasts are disabled when not fading and enabled only during the fade
transition, so gameplay UI remains clickable.
Teleporter retriggering and scene loop risk
● teleporter could fire again right after a load, causing a ping-pong between scenes or
a double teleport during the same entry.
● Fix: The flow was enforced as enter -> fade out -> set spawn -> load -> fade in. The
teleporter only reacts to a fresh player entry, and the sequence runs once per
transition.
Movement applied in two places after the refactor
● Some enemy logic still wrote velocity directly while the new controller also applied
movement, which led to conflicts and jitter.
● States now publish intent only, and the controller is the single authority that applies
velocity during fixed update, removing duplicate writes.
Talk state facing did not update
● During conversations the NPC kept the initial facing direction, which looked wrong
when the player moved around.
● Fix: While Talk is active, the character continuously turns to face the player and
restores the default state on exit.
3. Plans for Next Week
Dialog to build the story
● Write a first pass of the Level 1 story arc.
● Attach dialog to key NPCs and shopkeepers.
● Use simple conditions to unlock new lines as progress is made.
Music theme, intro, and end screen
● Select or compose a Level 1 music theme and set correct loop points.
● Add an intro screen with a short fade into gameplay and an end screen that appears
after Level 1 is cleared.
Final boss for Level 1
● Implement two or more moves with simple tells so players can read them.
● Add hit reactions, damage windows, and a short invulnerability after taking damage.
● Trigger end screen and rewards when the boss is defeated.
4. Feedback from Advisor (if any)
No meeting this week.
Student Signature:
Date:
Advisor Signature:
_____________________________
___________
____________________________
Date:
___________


CS 4395 – Weekly Progress Report Template
Student Name: ________________Cooper Tran__________________________
Student ID: _________________2106638____________________________
Advisor Name: __________________Ling Xu________________________
Week Number: ________________8__________________________
Date Range (): _______Oct 6 - Oct 10, 2025 __________________

1. Summary of Work Completed This Week
Item system
●	Built world pickups that add items into inventory after a short pickup animation. Overflow never disappears. When inventory is full the extra items drop back to the ground near the player. Stacking fills partial stacks first and then uses empty slots so bags stay tidy.
●	Added use and drop from inventory. Left click applies all item effects and then reduces the stack by one. Right click drops a single item to the world with a brief delay so it cannot be picked up in the same moment. Hover shows a simple info panel that lists effects in plain text.
●	Gold is handled in the same flow. Gold updates the wallet and the HUD as soon as it changes.
Enemy drops and rewards
●	Shopkeeper circles open and close the shop. Time pauses while the shop is open. The grid fills from the keeper’s lists so different keepers sell different things. Closing the shop resumes the game.
●	Buy checks gold and checks space before adding to inventory. Right click sells from inventory while the shop is open and returns to drop when the shop is closed. Slots listen to a shared open state so sell mode is always correct.
●	Shop slots show name, icon, price, and a hover card that follows the mouse. The card lists all effects in short lines so the panel stays readable.
Game manager system
●	Kept one game manager alive across scenes and used it as the single place for shared systems. It owns dialog, shop, fader, and music so the game does not spawn duplicates. If a second copy appears it cleans itself up and removes extra persistent objects.
●	Centralized input listening in the game manager to avoid scattered subscriptions. Other scripts now receive clean signals from one place instead of subscribing on their own.
●	Added music triggers in the world that switch the current track when the player enters a zone. The game manager plays the clip and keeps the loop stable.
Dialog system
●	Built a simple loop. Interact near an NPC to start. The NPC faces the player during the talk. The panel shows avatar, name, line, and up to three choices. Ending the talk restores control and hides the panel.
●	Dialog rules decide which line to show. Rules can depend on past talks, visited places, or items. Dialog can be set to one time or repeat. Dialog can unlock the next node and grant a small reward when it ends.
●	Added a clear list for quest steps. When the quest is finished the system skips all pre quest lines and goes straight to the completed dialog for that NPC.
Scene transition and persistence
●	A single teleporter acts as the gate in the current scene and also saves the next spawn id. The fader runs fade out and then loads the target scene.
●	A spawn point in the next scene reads the saved id and places the player at the matching marker on load. This keeps the handoff reliable and easy to set up.
●	The camera confiner binds to the new level bounds when a scene loads so the camera never drifts outside the map.
●	Core data now persists across scenes through the game manager so level, gold, items, and stats remain intact.
Music trigger
●	Added a location based music system that plays a theme based on where the player is on the map.
●	Each area defines a theme name and loop info. When the player enters the area, the system tells the game manager to switch to that theme.
●	If the same theme is already playing, it keeps playing without a restart. If the theme changes, it fades to the new clip and then loops cleanly.
2. Challenges or Issues Encountered
Input spread across many scripts
●	Multiple scripts subscribed to input. Missed unsubscriptions caused leaks and odd behavior.
●	Fix: Moved input handling into the game manager and broadcast simple signals to listeners. Subscriptions are now centralized and easy to reason about.
NPC still showing pre quest dialog after completion
●	The same pre quest lines could appear even after finishing the quest.
●	Fix: Added a clear list tied to quest completion. When the condition is met the system skips pre quest lines and jumps to the completed dialog.
Data lost on scene change
●	Level state, gold, items, and stats reset when moving between scenes.
●	Fix: Created a persistent game manager that keeps consistent objects alive and removes duplicates. Data stays valid across scene loads.
UI clicks blocked by the fade overlay
●	The full screen fade image intercepted raycasts while idle so buttons in all UI could not be pressed.
●	Fix: Disabled raycasts when not fading and enabled them only during the transition.
Teleporter retriggering and scene loop risk
●	A teleporter could fire again right after load and cause a ping pong between scenes.
●	Fix: Enforced a strict sequence. Enter the gate, fade out, set spawn, load, fade in. The teleporter reacts to a fresh entry only.
Movement applied in two places after the refactor
●	Some enemy logic set velocity while the controller also applied movement which caused jitter.
●	Fix: States now publish intent only. The controller is the single place that applies velocity during fixed update.
Talk state facing did not update
●	During a conversation the NPC kept the first facing direction while the player moved.
●	Fix: While Talk is active the character turns to face the player each frame and restores the default state on exit.
3. Plans for Next Week
Final boss for Level 1 with special moves
●	Define boss states for intro, idle, chase, light attack, heavy attack, stagger, and death. Create clear wind up and recovery. Implement two or more special moves with readable tells. Trigger end screen and rewards on defeat. 
Intro and end screen for Level 1
●	Add an intro screen with a short fade into gameplay. Add an end screen after Level 1 is cleared with options for retry, continue, and return to menu.
4. Feedback from Advisor (if any)
Everything is going great, keep it up.





Student Signature: _____________________________     Date: ___________
Advisor Signature: ____________________________     Date: ___________


CS 4395 – Weekly Progress Report Template
Student Name: ________________Cooper Tran__________________________
Student ID: _________________2106638____________________________
Advisor Name: __________________Ling Xu________________________
Week Number: ________________9__________________________
Date Range (): _______Oct 14 - Oct 18, 2025 __________________

1. Summary of Work Completed This Week
Combo system for player and enemies
●	Built a 3-hit melee combo chain with progressive damage scaling. The sequence runs Slash Down, Slash Up, and Thrust. Each hit in the chain deals more damage than the last using multipliers of 1x, 1.2x, and 2x. Input buffering allows the next attack to queue during the current swing so the combo flows smoothly.
●	The weapon sprite uses a bottom pivot setup so it rotates like a radar arm. The handle stays anchored near the player and the blade sweeps through an arc. This required switching all weapon sprites to bottom pivot in the sprite editor and setting pointsUp to true in the weapon data.
●	Enemies use the same combo system but select one fixed attack pattern on start. They can chain the same combo type consistently or pick randomly each time they attack. This creates variety without breaking the shared weapon code.
●	Movement penalties apply during each combo step. Speed drops to 60% on the first slash, 50% on the second, and 30% on the thrust. This forces commitment and prevents players from freely dodging mid combo.
New weapon UI
●	Added separate UI slots for equipped melee and ranged weapons. The slots display the weapon icon and update whenever the player equips a new weapon from inventory. This gives instant visual feedback for weapon swaps.
●	The UI listens to weapon change events from the player controller. When a weapon changes the icon refreshes and the old slot clears. This keeps the UI synced with the actual equipped state without manual updates.
Drag and drop inventory UI
●	Unified items and weapons into a single 9 slot inventory. Both types share the same slots and use the same drag logic.
●	Drag creates a floating icon that follows the mouse. Drop swaps all data between the source and target slots including type, item or weapon reference, and quantity. Empty slots accept dragged items. Full slots swap their contents cleanly.
●	Hotbar input with number keys 1 through 9 triggers the slot action. Items are consumed and weapons are equipped. Right click drops one item or weapon to the world. When the shop is open right click sells instead.
●	Left click on a weapon slot swaps it with the currently equipped weapon of the same type. Melee weapons swap with the melee slot and ranged weapons swap with the ranged slot. This keeps the inventory flowing without extra menus.
Item info popup
●	Built a shared hover tooltip that works in both inventory and shop. The popup shows item name, description, and stat effects for items. For weapons it lists attack damage, attack power, knockback, stun time, and other combat stats formatted by weapon type.
●	Inventory uses a 1 second hover delay to avoid accidental popups during quick clicks. Shop uses instant display so players can compare items quickly. The same popup instance is reused by both systems through the game manager.
Intro and ending UI
●	Added an intro screen that pauses the game after a short delay and waits for the player to press Start. The intro uses realtime timing so it displays even when time scale is zero. Clicking Start unpauses the game and removes the intro panel.
●	Built an ending screen that triggers on player death or boss defeat. The Game Over screen appears after a configurable delay when the player dies. The Victory screen shows instantly when the final boss health reaches zero. Both screens pause the game and display level, exp, kills, and time stats.
●	The ending UI includes a Restart button that reloads the current scene or loads the next scene if a scene name is provided. This keeps progression flowing from level to level or allows quick retry on failure.

2. Challenges or Issues Encountered
Bottom pivot sprite setup for combo arcs
●	The combo arc rotation looked wrong when weapon sprites used center pivot. The blade and handle rotated in opposite directions and the visual broke.
●	Fix: Changed all weapon sprites to bottom pivot at 0.5, 0 in the sprite editor. Set pointsUp to true in the weapon data. Now the weapon rotates cleanly like a radar arm with the handle anchored and the blade sweeping.
Unified inventory merging items and weapons
●	Items and weapons were separate before. Merging them into one inventory required tracking slot type, dual references for item or weapon data, and different behaviors for use versus equip.
●	Fix: Added a SlotType enum with Empty, Item, and Weapon states. Each slot holds both itemSO and weaponSO references but only one is active based on type. Left click checks the type and routes to use or equip. Drag and drop swaps all fields together so no data is lost.
Drag icon cleanup on cancel
●	When dragging outside the inventory and releasing the mouse, the floating icon stayed visible and blocked clicks.
●	Fix: OnEndDrag destroys the drag icon and restores the cursor to normal even if the drop target is invalid. This keeps the UI state clean.
Weapon swap not updating UI
●	Equipping a weapon from inventory did not refresh the equipped weapon UI slots immediately.
●	Fix: Added OnMeleeWeaponChanged and OnRangedWeaponChanged events in the player controller. The weapon UI subscribes to these events and updates the icon as soon as the weapon changes.
Item popup showing stale data
●	The popup sometimes displayed old item info when hovering quickly between different slots.
●	Fix: The popup clears all text fields before populating new data. Each Show call rebuilds the content from scratch so no old lines remain.

3. Plans for Next Week
Final boss for Level 1
●	Build boss states for intro, idle, patrol, chase, light attack, heavy attack, special moves, stagger, and death. Add clear wind up animations and recovery windows. Implement at least two special moves with readable tells so players can react. Trigger the end screen and grant rewards when the boss is defeated.
Particle system for weather
●	Create a particle system for weather effects like rain and snow. Add control over intensity, wind direction, and spawn area. Hook the system into level zones so different areas can have different weather. Keep performance stable by pooling particles and limiting spawn rates.
Audio system
●	Build a centralized audio manager that handles music and sound effects. Add volume controls for music and SFX separately. Implement audio mixing so combat sounds do not drown out dialog or music. Create audio triggers that play ambient sounds based on player location. Add footstep sounds that sync with animation and surface type.

4. Feedback from Advisor (if any)
No meeting this week.





Student Signature: _____________________________     Date: ___________
Advisor Signature: ____________________________     Date: ___________


CS 4395 – Weekly Progress Report Template
Student Name: ________________Cooper Tran__________________________
Student ID: _________________2106638____________________________
Advisor Name: __________________Ling Xu________________________
Week Number: ________________10__________________________
Date Range (): _______Oct 21 - Oct 25, 2025 __________________

1. Summary of Work Completed This Week
Boss system
●	Built boss controller using the same interface as regular enemies with physics-based special attack that calculates exact distance for dynamic gap-closing. Special attack follows charge, dash, double hit sequence.
●	Implemented multi-gate decision logic combining Y-alignment checks, distance ranges, and cooldowns. Horizontal-first chase with gradual Y-alignment prevents attack range overlap.
●	Created "face spot" mechanic where boss stops short of player during dash with afterimage effects for counterplay window.

Weather particle system
●	Created weather particle component with camera-following option for scrolling levels and auto-detection for child particle systems.
●	Added playOnStart setting and runtime control methods. Weather effects like rain and snow follow the viewport smoothly.

Audio system
●	Finished sound manager with full coverage for combat, UI, healing, buffs, inventory, shop, dialog, and destructibles. Implemented AudioSource pooling with 8 sources to prevent clicks.
●	Added granular volume controls for master, combat, UI, and effects with separate enemy volume multiplier at 60%.
●	Wired animation events for frame-perfect combo sounds and integrated throughout codebase with hierarchical volume calculation.

Destructible objects
●	Created breakable objects like grass, vases, and crates. Each object has HP, triggers particle effects on break, and can drop loot.
●	Added type-specific break sounds using random selection. Objects fade and destroy after breaking with configurable timing.
●	Integrated with weapon damage system so melee and ranged attacks can break objects.

Ladder system
●	Built ladder component that modifies movement speed based on climb direction using dot product for directional alignment detection.
●	Climbing up is 0.6x slower and climbing down is 1.3x faster. Works at any angle including vertical, horizontal, and diagonal ladders.
●	Integrated with player and enemy controllers for seamless state transitions.

2. Challenges or Issues Encountered
Boss special attack prediction math
●	The original fixed-range dash felt unfair because the boss either overshot or stopped too far away.
●	Fix: Implemented dynamic range calculation using physics. TimeReach equals dashSpeed times the computed move window which is the total hit time minus special hit delay minus pre-hit stop bias. The boss now reaches the exact face spot consistently.

Boss chase entering attack range
●	The chase state sometimes pushed the boss into attack range before switching states which caused animation glitches.
●	Fix: Added stopBuffer of 0.10f. The chase velocity is set to zero when the player is within attack range plus the buffer. This creates a clean gap and prevents overlap.

Audio clicks and cutoff
●	Playing multiple sounds on the same AudioSource caused clicks and cut off previous sounds early.
●	Fix: Created a pool of 8 AudioSources. PlaySound cycles through the pool so each new sound gets a fresh source. Overlapping sounds now play cleanly without interference.

Weather particles not following camera
●	Static weather particle systems stayed in place when the camera moved which broke immersion in scrolling levels.
●	Fix: Added followCamera option that updates the particle system position to match the camera position plus the initial offset. This keeps rain and snow locked to the viewport.

3. Plans for Next Week
Level 2 design and mapping
●	Draw the full layout for Level 2 in LDtk including main path, side areas, secret rooms, and boss arena.
●	Export tilemap to Unity and configure all colliders, sorting layers, spawn points, and teleporters.
●	Add background layers and parallax scrolling for depth.

4. Feedback from Advisor (if any)
No meeting this week.





Student Signature: _____________________________     Date: ___________
Advisor Signature: ____________________________     Date: ___________
