# FSO: Norvinsk Section 1 — Polish & Tuning Roadmap

**Purpose:** master checklist of everything to do *after* the quest pipeline is built, so the anniversary gift (target: **June 1, 2026**) ships as polished as possible. Nothing here blocks the quest chain — these are the finishing passes.

**Status legend:** ⬜ not started · 🟦 in progress · ✅ done

---

## CURRENT FOCUS (not in this doc)
> **Quest pipeline + 5-quest chain (Q1–Q5).** This is the active workstream. This roadmap covers everything that comes *after* the quests are proven and built. See `Research_REPORT_QUESTCHAIN.md` for the quest build reference.

### Quest-phase note: custom quest icons
For Q1–Q5 (not the throwaway test quest), make **FSO-branded quest icons** — fits the office aesthetic, Damjan will notice. Loader wants them in the trader's `Images/` folder as `.png`/`.jpg`, auto-routed by filename to `/files/quest/icon/<filename>`, referenced in each quest's `image` field as `/files/quest/icon/<filename>`. Missing image = harmless default icon (confirmed via test quest), so this is pure polish, not a blocker.

---

## 1. ⬜ Faction Hostility — Kaban & Kollontay (and all bosses)
**Problem observed live:** FSO killed Reshala on sight, but *ignored / very slowly engaged* Kaban and Kollontay and their guards on Streets. They eventually fought after a long delay, so the relation exists but is weak/unreliable.

**Goal:** reliable, immediate aggression between FSO and all hostile boss factions — Kaban especially (personal priority — neutralize him more often than not).

**Root cause (confirmed via SPT FEATURES.md):** our hostility was wired to a *category* (`scavbosses`) that catches Reshala (`bossBully`) but does NOT auto-expand to every individual boss + follower type. Each boss and each follower is its own `WildSpawnType`.

**The fix:** in `Mod.cs` faction relations, explicitly name every boss + follower type as hostile, and make the relation **mutual** (FSO→boss AND boss→FSO) so they actually shoot.

**Exact target list (names from SPT source):**
- **Bosses:** `bossBully` (Reshala), `bossKilla`, `bossGluhar`, `bossKojaniy` (Shturman), `bossSanitar`, `bossTagilla`, `bossKnight`, `bossZryachiy`, `bossBoar` (**Kaban**), `bossBoarSniper`, `bosskolontay` (**Kollontay**), `bossPartisan`
- **Followers/guards:** `followerBully`, `followerGluharAssault`, `followerGluharSecurity`, `followerGluharScout`, `followerGluharSnipe`, `followerKojaniy`, `followerSanitar`, `followerTagilla`, `followerBigPipe`, `followerBirdEye`, `followerZryachiy`, `followerBoar` (**Kaban's guards**), `followerkolontayassault`, `followerkolontaysecurity` (**Kollontay's guards**)
- **Cultists:** `sectantPriest`, `sectantWarrior`

**Needs:** paste the faction-relations block from `Mod.cs` when we get here, to see current format and add the full list (mutual).

**>>> DIAGNOSIS CONFIRMED (from Mod.cs, deadline run):** `WireFactionRelationships()` wires FSO hostile to ONLY three factions: `scavs`, `scavbosses`, `sectants` (all mutual, via `factionService.AddEnemyByFaction`). FSO friendly to `usec`/`bear`/`rogues`. **BD and RUAF are NOT in the list at all** — never wired. So FSO has NO relationship with BD/RUAF → neutral → they ignore each other / "sit idly / chat" (NOT an aggression-tuning problem — a MISSING-RELATIONSHIP problem; once enemies, the AI engages). Header comment describes intent (hostile scavs/bosses/cultists, ignore Goons) but BD/RUAF never made it into code. **THE FIX:** add `factionService.AddEnemyByFaction(FactionName, "<bd_faction>")` + reverse, same for RUAF, in `WireFactionRelationships()`. EASY once we have faction names.

**⚠️ BLOCKER — need BD's faction registration:** `AddEnemyByFaction` works on **faction names** in MoreBotsServer's `FactionService` (FSO registers via `factionService.Factions.Add(...)` + maps ints→names via `customBotTypeService.AddCustomWildSpawnTypeNames`). For BD to be enemy THIS way, BD must ALSO be a faction in that same FactionService. NEED FROM BLACKDIV MOD: (1) BD's faction name + how it registers (its Mod.cs equivalent) — confirms if `AddEnemyByFaction` works or needs another method; (2) BD's role string/int (its WildSpawnTypePatch equiv) — for Q4/Q5 kill objectives. SAME info unblocks hostility AND kill placeholders. Also need RUAF's faction name.

**⚠️ Q5 BLOCKER FOUND (from Mod.cs):** FSO spawn rules cover streets/sandbox(GZ)/customs/shoreline/lighthouse — **NO laboratory spawns wired at all.** Q5 finale needs Inner Circle (708304) spawning on LABS. Currently impossible — must add a Labs spawn rule (an `AddLabsSpawns()` method + `MapLaboratory` const, modeled on the others, using `BuildInnerCircleRecon`/`BuildEliteStrike` compositions). Without it, the Q5 "Inner Circle deploys" narrative + the Labs battle won't happen. (Labs map internal name likely `laboratory` — verify; this also ties to the Q5 quest `location` field.)

**>>> ALL FACTION DATA CONFIRMED (from BD/RUAF mod files — deadline run):**
- **BD faction = `"blackdiv"`** (lowercase). BD bot role names: `848420 blackDivLead`, `848421 blackDivAssault`, `848422 blackDivBreacher`, `848423 blackDivSupport` (camelCase EXACT). Client lists a 5th `848424` not in server faction (boss/special — SKIP). **BD ALREADY hostile to savage/rogues/usec/bear/infected (+ ruaf if present) but NOT to `fso`.** So Damjan (usec/bear) IS attacked by BD already; we just need FSO↔BD.
- **RUAF faction = `"ruaf"`** (lowercase), subfaction `"remnant"`. RUAF bots 848400-848405, remnant 848406.
- **✅ HOSTILITY FIX WRITTEN:** see `Mod_cs_hostility_patch.md` in outputs — expanded `WireFactionRelationships()` adding `blackdiv`+`ruaf`+`remnant` as mutual enemies (mirrors BD's own `AddEnemyByFaction` pattern). **Q4 + Q5 BD kill objectives ALSO updated** (placeholders replaced with the 4 real `blackDiv*` role names — both quest files now functional).
- **✅ Q5 LABS SPAWN — pattern confirmed:** BD's SpawnController shows the working approach: `locations.Laboratory.Base.BossLocationSpawn.Add(new BossLocationSpawn{...})` with `BossZone = "BotZoneFloor2,BotZoneFloor1,BotZoneBasement"` (+ gate zones `BotZoneGate1/Gate2`). **BD already spawns on Labs**, so Damjan WILL meet BD there for Q5. For FSO Inner Circle on Labs: add `AddLabsSpawns()` to Mod.cs. NOTE: Mod.cs uses the `CustomWaves.Boss[map]` config approach; BD uses `locations.Laboratory.Base` directly — need to confirm CustomWaves accepts a `laboratory`/`labs` key, OR switch to the direct-location approach for Labs. Labs internal name = `laboratory`. (Also new 1.0 map `labyrinth` exists.)

**>>> EARLIER FINDINGS (live testing, deadline run):**
- ⚠️ **RUAF NOT being fought** (confirmed live — Mae had to fight 3 RUAF that FSO ignored). **→ FIXED by the hostility patch above (adds `ruaf`).** The design doc hostility table says FSO should be HOSTILE to RUAF. So RUAF's role string(s) are missing from the hostility wiring. Add RUAF roles to the mutual-hostility list. (RUAF likely spawns via the RUAFComeHome mod with its own role string — need to confirm the exact string, but it's a faction FSO must engage.)
- ⚠️ **FRIENDLY-FIRE LEG DAMAGE** (confirmed live, nearly fatal): stepping too close to FSO fixers, they shoot the player's legs — almost killed Mae. So the "friendly to player" relation isn't fully protecting the player at close range. This is a relation/behavior bug in the FSO→player friendliness (possibly a proximity/collision-triggered defensive behavior, or the friendly relation not being absolute). HIGH PRIORITY — a gift faction that can kill the player is bad. Needs investigation: is it the SAIN brain's close-range behavior, or an incomplete friendly relation? May relate to the `layersToRemove` config in WildSpawnTypePatch.cs.

**>>> SAIN RECONSIDERATION (deadline run):** Mae does NOT currently have SAIN installed (intentionally — "SAIN bots are cracked/too lethal"). Reconsidering it because: (1) it gives real control over aggression via MIND settings (easier to tune than base EFT, and WildSpawnTypePatch.cs ALREADY has the SAINSettings written — Section "FSO", BrainsToApply [PMC,ExUsec], etc. — so adding SAIN is low-friction), (2) it can fix/tune the leg-damage friendly-fire via SAIN's fire-discipline settings, (3) the Q5 Inner Circle SHOULD be terrifyingly competent (SAIN makes that happen). RISK: SAIN bots are lethally accurate — could make leg-damage WORSE if untuned, OR make the gift faction too deadly to the player generally. **PLAN: test the hostility patch VANILLA first (no SAIN). If FSO still feel passive/don't push aggressively, install SAIN as the fix (it's the right tool for aggression + fire discipline). Strong "polish phase" candidate. Don't add mid-build — it's a new variable; lock the chain first.** Also possible SAIN conflicts with things, so test carefully when added.

---

## 2. ⬜ FSO Bot Loadout / Inventory Overhaul
**Problem observed live (multiple flags):**
- **GP coins reading 0** — a broken/empty stack instead of a real count (this is a *bug*, not just tuning)
- **5 TerraGroup Labs green keycards** on a single bot — WAY too many rare items, economy-breaking (these are the Q5 reward items leaking into bot inventories — present but massively over-distributed)
- **No medkits** — a fixer was bleeding out / coughing / gasping, had no meds
- **Too few magazines** — they run dry
- **Random/ill-fitting camos** — see section 4 (visual identity)

**Goal:** FSO fixers carry sensible, fitting loadouts — proper mags, medkits, correct rare-item rarity (rare = rare), GP coins as a real count.

**>>> NEW FINDINGS (live testing, deadline run):**
- **Boot-log "item not found in database" errors** (confirmed live): tpls `66489a0f8b2d733829c2a848`, `664aa07d00fc82b15f32c52b`, `5a1539825682a7fa06dc4243`, `2f5f355a48a470aeb12452b1` "does not exist in the item database" — repeated cache-regeneration failures. These are items referenced (in bot loadouts or assort) whose definitions aren't loaded. Need to trace which are FSO-referenced vs other-mod, and either remove the references or ensure the defining mod is present. (Some may be from other mods in the stack, not FSO — triage by source.)
- **MIKOR/Milkor camora fill failure** (confirmed live): "Unable to fill weapons camora (chamber) slots for: 627bce33f21bc425b06ab967 - mag_msgl_milkor_cylinder_mag_std_40x46_6. The mod pool for it was empty, attempting to generate dynamically" — the MIKOR grenade launcher (Inner Circle weapon, Q5) has an empty mod pool for its cylinder mag, so the server scrambles to fill it dynamically. Ties to the bot loadout / weapon preset work (section 4 + 7d) — the Inner Circle's MIKOR needs a proper ammo/mod pool defined.
- All the above + GP coins/keycards/meds/mags = the inventory overhaul (this section). Mae confirms it NEEDS doing — flagged repeatedly in testing.

**Method (Mae's call):** **deep research** (research toggle) on SPT 4.0.13 bot loadout generation — the chance/weighting structure, how `db/bots/types/*` define inventory pools, and *why* a 0-count stack or 5-of-a-rare-key happens (smells like a generation-weight or stack-count misconfig). Get certainty on generation structure before rewriting, so we fix root cause not symptoms.

**Files:** `db/bots/types/*` (the 5 FSO tier files: fsofixerrookie/operative/specialist/lead/innercircle).

**Note:** the Labs keycards leaking in is actually a *good* sign narratively (Q5 content is present) — just needs rarity correction so they don't spawn on every bot.

---

## 3. ⬜ Savior Ending Content + Unlock (Q5 finale payload)
**Goal:** the Q5 completion delivers the EFT 1.0 "Savior / For Humanity" ending package as the emotional climax.

**Confirmed available (research):** **WTT-ContentBackport 1.0.7** (installed, SPT 4.0.13 compatible, GUID `com.wtt.contentbackport`) already contains:
- the Savior ending **weapons/gear** (AS VAL Mod.4 / "val 4", PKP, premium weapons, Staccato, Siege-R, AA-12)
- the **"Peaceful Sky" main menu background** (ships as `WTT-ContentBackport-Mainmenu` sub-project)

**So:** we **reference existing item template IDs** in Q5's `Item` rewards — we do NOT build the items. Need to read exact MongoIDs from the installed mod's `db/CustomItems`.

**FSO bots already carry the backported Savior-tier reward weapons** (confirmed: Tim soloed Reshala with endgame gear) — those are the same items, just need rarity/loadout balance (section 2).

**The music hook ("Minutes to Midnight"):**
- Nikita's Savior soundtrack — the violin holds personal meaning for Mae; **emotionally central** to the gift.
- **TBD whether ContentBackport already plays it** on the ending trigger. If yes → free. If no → it's the *one* custom piece we build, and we build it **carefully + standalone** (NOT by hijacking core systems — lesson from the GameCallbacks hook saga).
- Plays on Q5 completion, alongside Peaceful Sky menu swap + "Savior" achievement.

**Assets on Mae's GitHub** (`majinjdawes-hub/FSO-for-SPT`): `Peaceful_Sky_main_menu_background.webp`, `Savior_icon.webp` (may be redundant if ContentBackport provides them).

---

## 4. ⬜ FSO Edition Weapons & Gear (Camos + Renaming)
**Goal:** Mae sells visibly non-default, FSO-branded gear so it's clearly *FSO equipment* — and so it ties to the faction's visual identity. Like the Black Division "Siege-R" edition.

**Two distinct sub-tasks (important to separate):**

### 4a. ⬜ Rename to `[FSO Edition]`
- Mechanism: **cloned items** with custom locale names + preset baked on (same as Siege-R = "Black Division Edition").
- **Current state:** only the LL1 guns are FSO-ified clones (they show preset names — `XC fso-stac`, `MP5 fso-mp5sd`). The MP7A2, SR-25, and AS VAL Mod.4 are still *stock* items (no preset names).
- **To do:** clone the remaining weapons into FSO Editions (rename + preset), append `[FSO Edition]` to names.

### 4b. ⬜ Apply Camos
- Tool: the **Weapon Camo & Stickers mod** (changes equipment/weapons/items via paint/material/stickers, has a preset system).
- Apply FSO camos/colors so weapons + armor *look* FSO (storefront AND bot loadouts).
- **Scope:** mostly **weapons and armors**. Maybe **clothing** too (the mod can camo clothing).
- **Open question:** how to apply camo presets to *bots* (so they don't spawn with random ugly camos). Mae + previous Claude worked on this before — needs a dig on chances/generation structure (overlaps with section 2).

**Outcome wanted:** feels unique, clearly FSO, non-default gear being sold.

### 4c. ⬜ E.G.O. QUEST-REWARD WEAPONS (Project Moon weapon flavor, separate from FSO Edition store gear)
These are named, camo'd weapons given as specific QUEST rewards (distinct from the FSO Edition store items above). Built in the dedicated weapon phase; quests reference them as placeholders until built.
- **"Magic Bullet"** (Q2 reward) — engraved **KAR98** (chosen: German weapon = Damjan's heritage; single weapon = clean build; modded rifle already in stack), **blue + gold** camo via Weapon Camo & Stickers mod, modified to fit a bit better. CLEAN single-preset build. **Q2 uses "Magic Bullet" as a named placeholder now; real camo'd KAR98 built in weapon phase, swapped in later.**
- **"Solemn Lament"** 🦋 (RESERVED for a later special/finale reward slot — too ceremonial for Q2) — Funeral of the Dead Butterflies' E.G.O. from Library of Ruina: a matched set of dual pistols, one BLACK + one WHITE, with fancy engravings. EFT has no dual-wield, so represent as TWO separate pistol presets (a pair). Build: white one via Reshala's Golden TT + white camo; engravings depend on available attachment mods with engraved slides. More complex (2 presets + camo + attachment hunting) — give it the ceremony it deserves in a later/finale reward, not Q2.


---

## 5. ✅ Kill-Feed Role Locale Fix — SOLVED (built, pending in-raid confirm)
**Problem observed live:** killing FSO fixers shows `scavrole/FSO` (truncated as "scavrole/fs-...") in the kill feed instead of a clean faction name.

**ROOT CAUSE CONFIRMED:** the `WildSpawnTypePatch.cs` registers all 5 tiers with the **third constructor param `"FSO"`** as the role/faction string (e.g. `new CustomWildSpawnType(708300, "fsofixerrookie", "FSO", ...)`). The kill feed looks up locale key `ScavRole/FSO` — which had no registered string, so it showed the raw key (truncated). The "scavrole/fs-" cutoff was just the visual truncation of `scavrole/FSO`.

**THE FIX (built):** reference pattern from RUAFComeHomeServer (`db/CustomLocales/en.json` = `{"ScavRole/RUAF": "RUAF", "ScavRole/REMNANT": "REMNANT"}`). All 5 FSO tiers share `"FSO"` so it's ONE line: `{"ScavRole/FSO": "FSO"}`. Created `db/CustomLocales/en.json` + wired `await wttCommon.CustomLocaleService.CreateCustomLocales(Assembly.GetExecutingAssembly());` as OnLoad step 9 (after clothing). The `db/**` csproj glob already deploys CustomLocales/ — no csproj change. Confirmed `CustomLocaleService.CreateCustomLocales(assembly)` reads `db/CustomLocales/` per WTT docs. **Pending: in-raid confirm the feed shows "FSO" (will see it during the Q2 boss hunt when a fixer dies).** Display name currently "FSO" — can change to "Full-Stop Office" / "FSO Fixer" anytime (just edit the value).

---

## 5b. ✅ Q1 Location Label Fix — SOLVED
**Problem:** Q1 showed "tarkovstreets Name" (broken locale label) top-right. **Cause:** `location: "tarkovstreets"` (lowercase) didn't match SPT's DB Id "TarkovStreets" (capitalized), so the label didn't resolve. **Fix:** changed `location` to `"any"` in q1_first_day.json — better design anyway (Q1 is a find-and-handover, no need to map-lock it). Shows "Anywhere" instead of the broken label.

---

## 6. ⬜ FSO Spawn Locations at Boss Bases — **DESIGN DECISION: KEEP**
**Observed:** FSO spawns at Kaban's base and Kollontay's base (Klimov shopping mall + Lexos) on Streets — felt a bit awkward at first.

**Decision (Mae):** **KEEP IT.** Reframed thematically — FSO contesting territory at raid start, more tension, whoever wins holds it for that raid. Not a bug; a flavor choice.

**Action:** none required unless we later want to tune spawn weighting. Logged so we don't "accidentally fix" it.

---

## 7. ⬜ WTT Services Phase (the fun toys — all confirmed available on WTT-CommonLib 2.0.20)
> All of these services exist on the `WTTServerCommonLib` master class (confirmed via dotPeek). Same library we wired for quests. Build **one at a time, measure twice** — especially anything client-side (GameCallbacks hook lesson).

### 7a. ⬜ `WTTCustomVoiceService` — **HIGH PRIORITY / BIGGEST GIFT POTENTIAL** 🎙️
- **Mae's own voice as a selectable PLAYER VOICE for Damjan.** Every voiceline is structured (`follow me`, `spread out`, etc.) — ambitious but hugely meaningful ("Damjan hears *her* voice call out in raid"). He'd freak out (happily). 🥹
- **AND/OR custom Korean voices for the FSO fixers** — true Project Moon experience. Mae is experienced with UABEA / Asset Studio on Ruina files, so sourcing/extracting audio is in her wheelhouse.
- This is worth real, dedicated time. Possibly the emotional centerpiece *alongside* the watch.

### 7b. ⬜ `WTTCustomAchievementService` — custom FSO achievements
- Including the "Savior" achievement for Q5 (ties to section 3).
- Other FSO-flavored achievements as desired.

### 7c. ⬜ `WTTCustomAudioService` — silly Mae audio
- Lower priority / "nice to have." Fun Mae sound effects/stingers.

### 7d. ⬜ `WTTCustomWeaponPresetService` — bot weapon presets
- Ties into section 4 (FSO Edition weapons / consistent bot camos). Helps ensure bots spawn with the *right* presets, not random.

### 7e. ⬜ (IDEA, not required) Mae "Services" menu entries — **FSO Assistance**
- Concept: special FSO-themed entries in the trader **Services** menu (like insurance/repair are services). Something like "FSO Assistance" — a special offering with flavor.
- Still being conceived. Not required for the gift. Logged as a stretch idea.
- Possible mechanism: trader services / custom service entries (needs research on how the Services tab is populated for custom traders).

### 7f. ⬜ Mae Repair service (+ repair kits) — **EASY, thematic, recommended**
- **Repair: EASY** — it's a `base.json` config (`repair` object: `availability`, `quality`, `currency`, `currency_coefficient`, `excluded_id_list`, cost multiplier). A fixer office maintaining gear makes complete sense. Low-risk, high-flavor. **Recommended for inclusion.**
- **Insurance: DEFERRED / harder** — more than base.json: involves the `insurance` settings PLUS server-side return-logic (return chance, time windows, insurance controller). Layered system, more failure surface. Separate later evaluation, not v1.
- **Repair kits in assort (LL3/LL4):** sell **body armor repair kits + weapon repair kits** gated at LL3/LL4 — just assort items (known mechanism) behind loyalty. On-brand: "the Office keeps you running." Nice touch for later.

### Cross-trader textbox dialogue (maybe-later polish)
- The mechanism where accepting one quest pops a *different* trader's message box (e.g. Skier's Chemical Part 4 → Prapor/Therapist appear). Real, but finicky — would require the other trader to "know" about the quest, which risks touching Artem. **Not worth the risk for v1.** Mae mentioning Artem in her own dialogue (done in Q1) achieves the narrative tie-in safely without it.

---

## 8. ⬜ The Watch (final anniversary reward — Q5)
**Item:** engraved Roler Submariner (tpl `59faf7ca86f7740dbe19f6c2` — **verify**), renamed, FSO crown engraving.
- **Inscription:** Mae writes when ready, signed `— off-contract. don't ask. — M.`
- **Delivery:** via mail, subject "Off-contract." / "Read the back."
- **Unlock:** Q5 completion → `AssortmentUnlock` reward + `QuestAssort/success` gate so Mae sells it only after the final quest.
- Status: design locked, build during quest chain (Q5) — listed here for completeness.

---

## 9. ⬜ The Artem Suit (Q1 reward)
**Goal:** completing Q1 unlocks the Artem suit in Mae's assort, so Damjan can dress as a Fixer.
- **Unlock:** Q1 completion → reward + gating. **CAVEAT (important):** clothing is NOT a normal assort item — it's a customization/wardrobe item keyed by `suiteId`, unlocked through the trader's clothing/customization offer list, NOT the weapon-style `assort.json` + `AssortmentUnlock` path. So the unlock mechanism needs its own focused dig: likely a `CustomizationDirect` reward, or WTT `CustomClothingService`, or adding the suite to the trader's `customization_*` list. Do NOT assume `AssortmentUnlock` works for clothing — verify first.
- **Artem IDs (from `WTT-Artem/db/CustomClothing/Artem Clothes.json`):**
  - **Notch Lapel Suit (Black) [top]:** `suiteId 6753b531d39e76c118c7456a`, outfitId `6753b531cb1138e1ee94c8f7`, topId `6753b532ab6de13791c4e840`, handsId `6753b53239d8af90f97f62e9`
  - **Notch Lapel Suit Pants [bottom]:** `suiteId 674db5974b5effbbf51fd756`, outfitId `674db5965d278c45de950fd3`, bottomId `674db596713fafc1c16e99b3`
- **CONFIRMED CLOTHING MECHANISM (cross-referenced Drip Out quest + Ragman suits.json + QuestAssort):** Clothing unlocks do NOT live in quest rewards. They live in **`suits.json`** (the trader's customization offer list). Each suit entry has `suiteId`, `requiredTid`, and a `requirements` block with `loyaltyLevel`/`profileLevel`/**`questRequirements: [questId]`**/`itemRequirements` (price). A suit unlocks when its `questRequirements` quest is complete AND the player pays. Drip Out works this way: suit entries in Ragman's suits.json list Drip Out's quest ID in `questRequirements`. This is trader-agnostic — any trader can sell a suit gated by any quest. (Drip Out's own quest `rewards` arrays are completely EMPTY — confirmed.)
- **TWO INDEPENDENT PROBLEMS with Artem:** (1) Artem's customization is currently BROKEN in-game — ALL his suits throw `GetTraderSuits` → "Unable to get suits from trader: %s" on purchase. This is a general Artem-on-4.0.13 bug affecting the whole game, not caused by FSO. Suits worked earlier in Mae's game, so a **reinstall may restore them** (back up base.json, re-apply salesSum-0 after). (2) How Q1 grants the suit.
- **DECISION TREE:** **First, reinstall Artem** (fixes problem #1 for the whole game; real chance of working since suits worked before). Then: **if reinstall fixes Artem** → Q1's existing Artem-rep +0.25 reward already unlocks the Notch Lapel at his LL2 (salesSum now 0), so **Q1 is already done, no changes needed** — the suit was visible at his LL2, just unbuyable due to the broader bug; fix bug → buying works. **If reinstall does NOT fix Artem** → go Mae-direct: add Notch Lapel suit entries to MAE's own customization offers via WTT `CustomClothingService`, gated by `questRequirements: ["217e6ba5ff7f1752ff218687"]` (Q1 ID), `requiredTid` = Mae. More thematic ("Fixer office issues the uniform"), fully sidesteps Artem. Requires setting up Mae's suits registration (focused task — she has no customization data yet).
- **Artem IDs (from `WTT-Artem/db/CustomClothing/Artem Clothes.json`):**
  - **Notch Lapel Suit (Black) [top]:** `suiteId 6753b531d39e76c118c7456a`, outfitId `6753b531cb1138e1ee94c8f7`, topId `6753b532ab6de13791c4e840`, handsId `6753b53239d8af90f97f62e9`
  - **Notch Lapel Suit Pants [bottom]:** `suiteId 674db5974b5effbbf51fd756`, outfitId `674db5965d278c45de950fd3`, bottomId `674db596713fafc1c16e99b3`
- Status: Q1 quest core DONE + proven (all rewards land). Suit unlock pending Artem reinstall test.

**>>> DIAGNOSIS CORRECTED (via full log analysis — supersedes the two bullets above):**
- The `GetTraderSuits` "Unable to get suits from trader: %s" crash is **MAE's, NOT Artem's.** Log proof: `/client/trading/customization/6a1ac8598933e3f023895bd3/usec/offers` (Mae) throws `[Fatal]` and loops (22x in log); Artem's own `customization/66bf757f.../offers` returns `[Info]` cleanly. **Artem is FINE** (reinstall confirmed working). The earlier "all Artem suits broken" reads were actually the client hitting MAE's endpoint.
- **Root cause:** SPT 4.0.13's customization UI polls EVERY trader's clothing-offers endpoint. Mae is a trader with NO suits/customization registration, so `GetTraderSuits(Mae)` finds nothing and crashes, then the client retries forever. Any custom trader needs an (even empty) suits registration so that endpoint returns `[]` instead of throwing.
- **THE FIX = THE FEATURE (one task):** WTT-CommonLib `CustomClothingService` registers a trader's suits. Setting up Mae's clothing registration (a) STOPS the crash loop, AND (b) is where we add the **Notch Lapel Fixer suit gated by `questRequirements: ["217e6ba5ff7f1752ff218687"]` (Q1)** so **Mae issues the suit directly** — the thematic "Fixer office issues the uniform" version. Go Mae-direct: fixes the bug AND delivers the suit in one move. Artem-rep approach abandoned (Artem works fine, but Mae-direct is cleaner + necessary to fix the crash).
- **NEXT for suit:** research WTT `CustomClothingService` usage (register a trader's suits + gate by quest), set up Mae's suits with Notch Lapel suit/pants (`suiteId 6753b531d39e76c118c7456a` + `674db5974b5effbbf51fd756`) gated by Q1. The Artem→Mae extract binding can stay (harmless) or be reverted (Artem works either way).

**>>> SUIT BUILD FINALIZED (file written: MaeNotchLapel.json):**
- **Approach: Mae-direct via `CustomClothingService`, Option B (self-contained bundles).** File `db/CustomClothing/MaeNotchLapel.json` written + validated (2 entries, top + bottom, zero ID collision with Artem confirmed).
- **Schema confirmed** from Artem's full `Artem Clothes.json`: TOP = type/suiteId/outfitId/topId/handsId/locales/topBundlePath/handsBundlePath/traderId/loyaltyLevel/profileLevel/standing/currencyId/price. BOTTOM = same minus topId/handsId/topBundlePath, plus bottomId/bottomBundlePath (NO hands on pants). `questRequirements` added (not in Artem's file but research-confirmed valid).
- **Mae's suit specifics:** 7 fresh unique MongoIDs (top: suiteId 80dbdc8d280202b5fbfaee92, outfitId 5ac78805d312d85327371d5a, topId fd05cff1c0051730bc3e75c6, handsId 768655f88377c14e6a9fea63; bottom: suiteId 1f1876ebfb5340677874f3fb, outfitId ae6e0b9c53e3f8ab3543fdf0, bottomId a25bccef14069956c00d6974). Named "FSO Fixer Suit (Black)" + "FSO Fixer Suit Pants". `traderId` = Mae (6a1ac8598933e3f023895bd3). `currencyId` = USD (5696686a4bdc2da3298b456a). **price = 67** (ties to the 67-coffee Marked Keys gag; cursed number for Damjan). LL1, profileLevel 1. `questRequirements: ["217e6ba5ff7f1752ff218687"]` (Q1).
- **BUNDLE PATHS resolve relative to the REGISTERING mod's `bundles/` root** (proven by Artem: JSON says "Tops/artem_top_10.bundle", file lives at WTT-Artem\bundles\Tops\). So **Option B: COPY 3 bundles into Mae's mod** — `Tops/artem_top_10.bundle`, `Hands/artem_atomhands.bundle`, `Pants/artem_pants_8.bundle` from WTT-Artem\bundles\ → FSO mod's bundles\ (keep subfolder names exact). Copying .bundle files is fine (no editing needed). Self-contained = guaranteed render, survives Artem uninstall.
- **Loader wiring:** add `await wttCommon.CustomClothingService.CreateCustomClothing(Assembly.GetExecutingAssembly());` as next OnLoad step after the quest call. Registering the suit STOPS the GetTraderSuits crash loop.
- **csproj:** DeployToSPT target must also copy `bundles/**` (currently only copies db/). Pending csproj review for exact line.
- Status: clothing file written + validated. Pending: copy bundles, drop JSON, wire loader, add bundles to csproj deploy, build+restart+verify (crash gone + suit wearable at 67 USD).

**>>> ✅✅✅ SUIT + Q1 COMPLETE (CONFIRMED IN-GAME):** Built, deployed (bundles confirmed on disk), restarted. RESULTS: (1) **crash loop DEAD** — customization screen opens, both pieces purchased, no GetTraderSuits spam. (2) **FSO Fixer Suit renders on the character model** (screenshot confirmed — full black suit/tie/mask, Option B bundles work). (3) both pieces buyable at **67 USD**, unlocked via Q1 (`questRequirements` retroactive gating confirmed via "OBTAINED"). (4) **Artem ALSO works** (bought shirt+pants to confirm — registering Mae's suits fixed the whole customization system; Artem was never broken, it was always Mae's missing registration). Descriptions finalized to "FSO: Section 1" (clean category label, both pieces a matched set). Icon not shown (custom clothing icon not registered — cosmetic, deferred, suit works fine without). **Q1 IS 100% DONE: voice + FSO logo icon + all rewards (XP/Mae rep/Artem rep/Charisma/USEC Negotiations/USD) + the Fixer uniform.** The hardest infrastructure (quest pipeline + clothing pipeline) is now PROVEN.

---

## CHARACTER / VOICE REFERENCE (locked — for all quest + mail + service text)
- **Voice = "(c) in-world recognition":** Mae is a real fixer-office manager giving real briefings. **No meta-commentary, no fourth-wall breaks** — immersion holds completely. But woven in are turns of phrase / small warmths Damjan would *notice* — not "haha a reference," but "...wait, that's how *she* talks." The mask never drops on purpose; he just starts seeing who's behind it. Subtle enough to never break, present enough to feel.
- **PM-reference density: moderate** — subtle lore drops, some Project Moon terminology as flavor; only HUGE events referenced, and even then subtly. Not a meme overload.
- **Writing style:** lowercase, no capital "I", casual — Mae's personal voice leaking through the professional register.
- **Fixers stay competent** in-game (professional, no clowning). Silliness lives only in framing (Mae's dialogue, mail, quest descriptions).
- **Worldview:** FSO has "seen worse" — Norvinsk is just another City district where the rules broke down. Mae's all-business about contracts; the person underneath leaks through phrasing.

---

## KEY TECHNICAL GUARDRAILS (hard-won lessons — do not repeat)
1. **On SPT 4.0.13, the `ConfigServer.GetConfig<T>()` obsolete warnings are CORRECT code — NOT a bug.** Do NOT "fix" them to direct config injection (that's a 4.1 pattern that **crashes the 4.0.13 server at boot**). This applies to `TraderConfig`, `RagfairConfig`, `LocationConfig`, etc.
2. **Quest JSON must match BSG models EXACTLY** or the loader throws and blocks loading. Build/test one quest at a time.
3. **Every quest `_id` and every condition `id` needs a matching locale key** or the UI shows raw GUIDs. (Same mechanism as the kill-feed fix #5.)
4. **Quest files are `Dictionary<MongoId, Quest>`** (keyed object `{ "questid": {...} }`), NOT a bare array. Quest IDs must be valid 24-char MongoIDs or the loader skips them.
5. **Config JSON changes → restart server. Code changes → rebuild then restart.** Server reads everything fresh at boot.
6. **Client-side hooks: build carefully + standalone.** Do NOT hijack/override core systems (the `GameCallbacks` override caused infinite-load; the fix was removing it). Applies to the music hook (#3) and any client plugin.
7. **`<ItemGroup>` blocks must be tag-balanced** in the .csproj — every `<ItemGroup>` needs its `</ItemGroup>`.
8. **WTT-CommonLib load order:** `[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]` so it runs after the database/trader exist.

---

## SUGGESTED POST-QUEST ORDER (rough)
1. **Kill-feed locale (#5)** — quick win, same mechanism we're already using for quests.
2. **Faction hostility (#1)** — the one real gameplay bug; makes raids feel right.
3. **Bot loadout overhaul (#2)** — deep-research-backed; fixes GP coins, rare over-spawn, medkits, mags.
4. **FSO Edition weapons + camos (#4, #7d)** — visual identity polish.
5. **Savior content wiring + music (#3)** — Q5 payload; the music gets careful standalone treatment.
6. **WTT Voice service (#7a)** — the big emotional toy; dedicated time.
7. **Achievements (#7b), audio (#7c), Services-menu idea (#7e)** — final flourishes.

*(Order flexible — #5 and #1 are the highest-value/lowest-risk early wins.)*

---

## LOOSE ENDS / CLEANUP (don't forget — tidiness)
- ⬜ **PURGE THE TEST QUEST** (the throwaway from proving the quest pipeline). Quest ID `06883fd4953de05404a82ebd`. Currently shows in the completed log as raw GUIDs ("06883fd4953de05404a82ebd name/description") — purely cosmetic clutter in MAE's (Mae's) test profile, NOT in Damjan's future fresh profile. To purge: (1) delete the test quest's `.json` from `db/CustomQuests/6a1ac8598933e3f023895bd3/Quests/` (the file whose `_id` is that GUID), (2) remove the completion record from the player profile (`6a00eb56a3e2c72f34f8bc4e.json`) — BACK UP PROFILE FIRST, then use a profile-editor tool (safer than hand-editing JSON) to find quest `06883fd4953de05404a82ebd` and remove it. Low stakes (cosmetic, own-profile-only), deferred to a cleanup pass. Recommended before final gift handoff for tidiness.
- ⬜ **Q1 minor polish** (if not already): Charisma reward currently +300 pts = "+3 levels"; fine. FSO icon confirmed working.


## ⚠️ BD SPAWN TIMING + VAGABOND RAID LENGTH (diagnosed, fixed in BD config)
BD minTime/maxTime are FRACTIONS of total raid time (0.3 = 30% in). Vagabond makes raids ~80 min
(open-world style), so the OLD values (min 0.3 / max 0.9) meant BD wouldnt even start spawning
until ~24 min in and could be missed entirely before extract/death -> likely why BD were no-shows
in testing. FIX (applied to BD config.jsonc): minTime 0.0 (spawn at raid start), maxTime 0.3
(through ~24 min). BD should now reliably appear early.
NOTE: BD config change is in BD's own mod, BUT Narconet syncs Mae(host/profile-owner) settings -> Damjan one-directionally, so the BD config propagates to Damjan automatically. Cross-install handled. (Narconet does NOT sync Damjan->Mae or override Mae's local settings.)


## ✅ BD SPAWN — CORRECTED DIAGNOSIS (BD is NOT broken)
EARLIER I (Claude) wrongly concluded BD v1.1.1 crashes on startup based on ZERO BD log lines.
WRONG: the mod has debug.logs:false by default, so it just wasn't logging. With debug.logs:true,
BD's AdjustAllSpawns() clearly RUNS FINE - logs "Adding Black Division spawn to Labs" + adds
timed hunts to all 10 maps. BD's spawner works. No bug report sent (would have been bogus).
LESSON: suspect a logging FLAG before concluding a crash from missing logs. Mae's instinct to
turn on debug logs first caught this.

REAL ISSUE: BD don't MATERIALIZE on Labs despite being registered (by BD AND, redundantly, by us).
Suspects: (1) our ~18 FSO Inner Circle with IgnoreMaxBots=true FLOOD Labs and hit the bot cap,
squeezing out BD if BD's Labs spawn respects the cap; (2) our redundant FSO BD spawn conflicting
with BD's own. NOTE BD hunts are TIMED (e.g. spawn ~18min into raid) - on hunt maps BD appear
partway through by design, NOT at raid start.
FIX (in progress): (a) REMOVE our FSO-side BD spawn entirely - let BD's working spawner handle it;
(b) REDUCE FSO Inner Circle count on Labs (less flooding); (c) make BD IGNORE the max-bot cap so
they materialize alongside FSO. Test after.

## ✅ BD ON LABS — FULLY FIXED + VERIFIED (boot log confirmed)
Triple-layer fix all confirmed working at boot:
1. Cap-raise: Labs bot cap (~19-24) -> +8 (RaiseLabsBotCap via BotConfig.MaxBotCap). The cap
   was the original bottleneck (~18 FSO ate the whole 19 cap, no room for BD).
2. FSO reduced: Labs Inner Circle 3 squads x6 (=18) -> 2 squads x5 (=10), less flooding.
3. BD cap-bypass: EnsureBlackDivisionIgnoresLabsCap flips IgnoreMaxBots on BD's Labs spawns.
   REQUIRED load-order fix: our TypePriority 400008 (raw) > BD's 400007, so we run AFTER BD
   and find its Labs spawns. Boot log confirms "set IgnoreMaxBots on 2 Black Division spawn(s)".
BD's OWN spawner works fine (NOT broken - the earlier "crash" theory was wrong; debug.logs was
just off). BD spawns BD; we only ensure they fit on Labs. NO bug report sent (BD isn't broken).
STILL TO CONFIRM IN-RAID: BD actually materialize on Labs + Q4/Q5 BD-kill tracking ticks.

## 💡 FUTURE CAPABILITY UNLOCKED (post-launch idea — Mae's spark)
The pattern we built (run AFTER other mods via high TypePriority + databaseService.GetLocation()
+ flip spawn properties like IgnoreMaxBots/BossChance) is a GENERAL lever to orchestrate ANY
faction mod's spawns from FSO's side WITHOUT editing their code. Post-launch/update ideas:
- HUGE FSO vs RUAF war on a map (force both factions' spawns high + hostile)
- Multi-faction battle royale set-pieces (FSO + BD + RUAF + Untar all forced onto one map)
- Tunable "event" raids / themed clashes
NOT for the gift build - a fun future-update direction once the gift ships + is polished.

## ★ THE PLAN / SEQUENCE (locked, per Mae) ★
1. [IN PROGRESS] Confirm BD materialize in-raid + Q4 completes (safe-skill reward proof).
2. [NEXT] FOLLOW-UP GIT COMMIT - capture all working fixes (BD cap/load-order, safe skills,
   valid MongoIDs, rewards retune) as a known-good point before Savior work.
3. SAVIOR DEEP RESEARCH FIRST (the "for humanity" ending) - then build it:
   - EndingsCommand + BackportQuestHelper (trigger), for_humanity dogtag + armband (items),
     menu background/cinematic (client), achievement. Materials gathered (see SAVIOR_ENDING
     _RESEARCH_BRIEF.md). Mail signature CONFIRMED (SendLocalisedNpcMessageToPlayer).
4. THE WATCH - finalize inscription (Mae's hand) + wire delivery via mail (with dogtag/armband
   + Mae's letter, on Q5 completion).
5. THEN ALL THE POLISH (don't forget these!):
   - Friendly-fire leg damage (FSO shooting player legs at close range - HIGH)
   - SAIN reconsideration (aggression/fire-discipline control; WildSpawnTypePatch SAIN-ready)
   - Bot loadout overhaul (medkits/UFAK, mags, GP-coin bug, keycard over-spawn, MIKOR ammo)
   - FSO Edition weapons + camos: MAGIC BULLET (Q2 KAR98, blue+gold, German=Damjan heritage),
     SOLEMN LAMENT (finale dual pistols, Funeral of the Dead Butterflies E.G.O.)
   - SAVIOR music hook ("Minutes to Midnight") - SEPARATE deep research after Savior ending
   - Purge test quest (leftover GUID 06883fd4953de05404a82ebd from own profile)
   - WTT Voice (Mae as player voice), achievements, Mae repair/medic polish
   - Verify coffee-ladder assort unlocks gate to right LL (LL2/LL3/LL4)
