# FSO: Norvinsk Section 1 — Polish & Tuning Roadmap

**Purpose:** master checklist of everything to do *after* the quest pipeline is built, so the anniversary gift (target: **June 1, 2026**) ships as polished as possible. Nothing here blocks the quest chain — these are the finishing passes.

**Status legend:** ⬜ not started · 🟦 in progress · ✅ done

---

## CURRENT FOCUS (not in this doc)
> **Quest pipeline + 5-quest chain (Q1–Q5).** This is the active workstream. This roadmap covers everything that comes *after* the quests are proven and built. See `Research_REPORT_QUESTCHAIN.md` for the quest build reference.

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

---

## 2. ⬜ FSO Bot Loadout / Inventory Overhaul
**Problem observed live (multiple flags):**
- **GP coins reading 0** — a broken/empty stack instead of a real count (this is a *bug*, not just tuning)
- **5 TerraGroup Labs green keycards** on a single bot — WAY too many rare items, economy-breaking (these are the Q5 reward items leaking into bot inventories — present but massively over-distributed)
- **No medkits** — a fixer was bleeding out / coughing / gasping, had no meds
- **Too few magazines** — they run dry
- **Random/ill-fitting camos** — see section 4 (visual identity)

**Goal:** FSO fixers carry sensible, fitting loadouts — proper mags, medkits, correct rare-item rarity (rare = rare), GP coins as a real count.

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

---

## 5. ⬜ Kill-Feed Role Locale Fix
**Problem observed live:** killing FSO fixers shows `SCAVROLE/FS` (truncated locale key) in the kill feed instead of a clean faction/role name. (Names like "FSO Violet", "FSO Queen" render fine — it's specifically the **role/type display** that's broken.)

**Root cause:** the custom FSO `WildSpawnType` role names have **no registered locale string**, so the game falls back to showing the raw key fragment.

**The fix:** register a locale entry mapping FSO role types → clean display text (e.g. "Full-Stop Office" or "FSO Fixer"). Same locale-key mechanism as quest text. Quick fix once located.

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
- **Unlock:** Q1 completion → `AssortmentUnlock` reward + `QuestAssort/success` gate.
- Status: build during quest chain (Q1) — listed here for completeness.

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
