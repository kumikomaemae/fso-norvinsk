# FSO: Norvinsk Section 1
## Design Document v2.0 — LOCKED

**Project Type:** Private SPT mod (gift, never published)
**Author:** Mae
**Recipient:** Damjan
**Target Date:** June 1, 2026 (anniversary)
**SPT Version:** 4.0.13
**Fika Compatibility:** Required
**Enum Range:** 708300–708399
**Crew Codename:** The Phantom Thieves

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Canon & Lore Bible](#2-canon--lore-bible)
3. [Mae — Trader Profile](#3-mae--trader-profile)
4. [Bot Types (All 5 Tiers)](#4-bot-types-all-5-tiers)
5. [Quest Chain](#5-quest-chain)
6. [The Anniversary Moment](#6-the-anniversary-moment)
7. [Technical File Structure](#7-technical-file-structure)
8. [Implementation Phases](#8-implementation-phases)
9. [Dependencies & Stack Notes](#9-dependencies--stack-notes)
10. [Open Items / TODOs](#10-open-items--todos)

---

## 1. Project Overview

A custom faction mod for Single-Player Tarkov, adding **FSO: Norvinsk Section 1** — a temporary deployment of the Full-Stop Office (a Project Moon fixer organization) to the Tarkov region. The faction operates as an event-style allied presence: their fixers spawn as patrols on certain maps and are friendly to PMCs (specifically the player and their FIKA partner), hostile to scavs, raiders, RUAF, BlackDiv, and cultists.

**Mae** — Section Manager of Norvinsk Section 1 — serves as a custom trader with a custom assortment, a 5-quest contract chain, and a final anniversary reward (a renamed Roler Submariner with personal engraving). The mod includes a custom faction armband as a v1 deliverable.

**Tonal target:** Project Moon Office bureaucracy meets Tarkov grit. Deadpan-professional with subtle silliness. Earnest at the right moments.

---

## 2. Canon & Lore Bible

### Full-Stop Office (canon, lifted from Library of Ruina)

- A Fixer Office in the City specializing in **ranged assassinations using firearms**
- Rare among Offices for the firearm focus — firearms are restricted by the Head's laws, ammunition is heavily taxed, and proper firearm training is uncommon among Fixers
- Full-Stop's operatives prefer firearms for their **lethality and range** — most people in the City can't dodge bullets
- Operates under the broader Office system within the General Working Association

### Why Norvinsk

In Norvinsk, ammunition is more common than food. For a Full-Stop fixer, the region is a paradise: no Head's taxes on rounds, no City-restrictive laws on firearm carry, abundant supply. This is the *real* reason Section 1 took the deployment — not just the contract money, but the chance to operate without the home turf's constraints.

### The Contract (the hidden lore that unfolds across the quest chain)

**LCCB (Limbus Company — B Team / Scouting Division)** has been running wide sweeps across multiple regions, monitoring for **distortion phenomena** that could indicate displaced E.G.O. artifacts. During one such sweep, LCCB picked up a signature consistent with a **Golden Bough** displacement event in or around the Norvinsk region.

Rather than commit their own operatives to a hostile foreign zone, LCCB sub-contracted the Full-Stop Office to handle investigation. The Office sent Norvinsk Section 1 — a small detachment under Section Manager Mae.

### The Player's Relationship to FSO

The player (and their FIKA partner) are **ex-USEC PMCs** who never fully left Norvinsk after the EMP blast (1.0 lore). They possess insider knowledge of TerraGroup's structure and operational patterns. LCCB, via FSO, sub-contracted them as an information asset and operational partner. The player isn't a random PMC — they're on the contract roster, which is why FSO treats them as a recognized friendly.

### Black Division (corrected from earlier draft)

**Black Division is TerraGroup's internal special operations force**, not a hired contractor. They're called in to eliminate threats to TerraGroup's research — including any investigators sniffing around the Labs. Once TerraGroup becomes aware of LCCB's investigation, BD is dispatched to clean up.

### The Stakes (Quest 5 reveal)

The Golden Bough is real. TerraGroup acquired it. If TerraGroup uses it, the outcome is the EFT canon "bad ending" — TerraGroup reigning over a nuked world. The mod's quest chain culminates in the **Savior ending** (canon to 1.0 / WTT-Content Backport): the player completes LCCB's contract, the Bough is recovered, humanity is preserved.

### Lore Touchpoints (subtle references the player will encounter)

- **The City** — referenced as "back home" by fixers / Mae
- **Distortion phenomena** — LCCB's monitoring system, mentioned in passing
- **The Head's laws** — referenced as the reason fixers are *thrilled* to be in Norvinsk
- **Mirror worlds** — mentioned as part of LCCB's monitoring scope, no mechanical implementation
- **Other Offices** — possible passing reference (e.g., "Hana Association doesn't take these contracts")
- **Chesed (coffee)** — Mae's coffee obsession, an Easter egg for Damjan

---

## 3. Mae — Trader Profile

### Identity

- **Display Name:** Mae
- **Subtitle:** FSO Section Manager
- **Internal Trader ID:** `mae_fso_section_manager` (subject to adjustment during build)
- **Portrait:** Custom artwork (City-cyberpunk background, Mae with headset and rifle, looking off-camera)
- **Armband logo:** Full-Stop Office crown insignia (gold-on-black)
- **Voice:** Text-only for v1. (Voice acting framework integration is a future v2 update.)
- **Availability:** Always available at hideout — configured similarly to Fence's always-on availability. Lore: fixers operate via comms and couriers; they don't require you to come to them physically.
- **Currency accepted:** USD only (LCCB converts USD to City assets through their own channels)

### Voice Palette (locked)

- Always tired
- Deadpan professional
- Secretly silly
- Coffee-dependent (Chesed reference — for Damjan)
- Refers to client *only* as "the LCCB" — consistent terminology, never "Limbus" generically
- Treats Tarkov's chaos as "Tuesday" — bureaucratic calm about absurdity
- Real warmth shown through tiny gestures (a non-sequitur observation, an offhand kindness, an inside reference)
- When emotionally affected, gets *more* clipped and professional, not less

### Sample Voice Tone

> *"Good. You're alive. Coffee's still hot if you want some — figuratively, since I'm here and you're there. The LCCB wants eyes on something. We're going to give them eyes. Standard rates. Don't die."*

### Loyalty Levels

- **LL1** — Default, available immediately. Basic assortment.
- **LL2** — Unlocks after Quest 2. Expanded assortment, first tier of coffee barters.
- **LL3** — Unlocks after Quest 4. Top-tier fixer gear, BD gear listings (high USD price).
- **LL4** — Unlocks after Quest 5. **The Watch (final anniversary reward).** Exclusive premium items.

### Assortment Philosophy

Heavy Western/NATO emphasis. Modded weapons preferred where they're more characterful than vanilla equivalents. Suppressed weapons featured prominently. Weapon presets with custom titles. Roughly 30–40 line items total across all loyalty levels (mix of specialty + general-utility).

### Featured Weapon Family

- **AS VAL Mod.4** — standard issue across all fixer tiers, available for player purchase at LL2+. Integrated suppressor, capable of high-pen ammo.
- Suppressed pistols (USP, Glock 17 suppressed)
- Suppressed SMGs (MP5SD, MP7)
- DMRs at higher LL (DVL-10, VSS)
- BD-recovered gear at LL4 (high price)

### Coffee Barter Ladder (LOCKED IN CANON FOREVER)

| Loyalty Level | Barter |
|---|---|
| LL2 | Impact grenades for 1 coffee |
| LL3 | Military flash drive OR intelligence folder for coffee |
| LL4 | Bitcoin for 10 coffees |
| **LL4 + 67 coffees** | **Marked Keys** ✨ |

The 67-coffee marked keys barter is non-negotiable. It is the soul of the mod.

---

## 4. Bot Types (All 5 Tiers)

All tiers share visual identity: **dark suit, dark tie, white shirt, dark slacks** (using Artem trader's suit + suitpants). All tiers wear the **Atomic Defense CQCM Mk.2 ballistic mask** (Level 6 face protection, black). All tiers use **Proflex earbuds** (squad-wide comms aesthetic).

### 708300 — `fsoFixerRookie` (Fledgling Fixer)

- **Role:** Entry-level operative, disorient + volume-of-fire support
- **Spawn weight:** Most common in patrols
- **Armor:** Slick plate carrier + Level 6 plates (front/back only — clean visual)
- **Mask:** CQCM Mk.2 (black, Level 6)
- **Comms:** Proflex earbuds
- **Weapon:** SMGs (MP5SD, MP7) with suppressors
- **Equipment:** Flashbang grenades (M67 frags possible)
- **AI Brain:** Tuned-down Raider brain (grenade priority enabled)
- **Skill tier:** Average
- **Lore:** New hires, training on the job. Haven't earned the formal suit yet.

### 708301 — `fsoFixerOperative` (Office Fixer)

- **Role:** Standard career fixer, backbone of patrols
- **Spawn weight:** Common in patrols
- **Armor:** Slick + Level 6 plates (or upgraded to black THOR if balance allows)
- **Mask:** CQCM Mk.2
- **Comms:** Proflex earbuds
- **Weapon:** Suppressed carbine (AS VAL Mod.4) or suppressed SMG, sidearm
- **Equipment:** M67 fragmentation grenades, high-pen ammo (M995, RIP rounds, etc.)
- **AI Brain:** Raider brain (high skill tier)
- **Skill tier:** High
- **Lore:** Career fixers. Years of contract work. Knows where the bodies are.

### 708302 — `fsoFixerSpecialist` (Field Specialist)

- **Role:** Randomized specialist — patrol gets 1–2 of these
- **Spawn weight:** Less common, 1–2 per patrol
- **Armor:** Black THOR set (full coverage — shoulders, groin, sides) with Level 6 plates
- **Mask:** CQCM Mk.2
- **Comms:** Proflex earbuds
- **Weapon (randomized per spawn):**
  - **Marksman variant:** DMR (DVL-10, VSS) with high-pen ammo
  - **Breacher variant:** AA-12 with HE / incendiary rounds
  - **Suppression variant:** M249-SAW (LMG)
- **AI Brain:** Raider brain, high skill, slower TTR for marksmen
- **Skill tier:** High
- **Lore:** Specialists pulled in for specific contract needs. Mae rotates them based on intel.

### 708303 — `fsoFixerLead` (Senior Fixer)

- **Role:** Patrol leader — always at least one per patrol
- **Spawn weight:** Guaranteed as patrol lead
- **Armor:** Black THOR set with Level 6 plates
- **Mask:** CQCM Mk.2 (possibly custom-textured Lead variant if asset work feasible — fallback: vanilla black Mk.2)
- **Comms:** Proflex earbuds
- **Weapon:** Top-tier AS VAL Mod.4 (custom preset, "Section Manager's Loaner" or similar named flavor)
- **Equipment:** Frags, smokes, advanced med
- **AI Brain:** Boss-tier Raider brain, leads followers
- **Skill tier:** Maximum
- **Lore:** Section 1's middle management. Mae trusts them to call shots in the field.

### 708304 — `fsoFixerInnerCircle` (Inner Circle) ✨

- **Role:** Mae's personal team — THE LABS BOTS
- **Spawn:** **Quest 5 ONLY**, elevated rates on Labs
- **Armor:** Full black THOR set + helmet
- **Mask:** CQCM Mk.2
- **Comms:** Proflex earbuds
- **Helmet/Optics:** **T-7 Thermal Goggles** mounted on helmet
- **Weapon Loadouts (randomized):**
  - Custom AS VAL Mod.4 preset (FSO-named) with thermal optic + best ammo
  - MIKOR grenade launcher with **AP-MERS rounds ONLY** (armor-piercing buckshot — no incendiary or HE, to prevent accidental player kills)
  - At least one team member per spawn carries an **LMG** for suppression
- **AI Brain:** Glukhar boss-brain (very aggressive, uses cover, pushes objectives)
- **Skill tier:** Maximum
- **Hostility:** **WEAPONS FREE.** Hostile to everyone except players in the active contract (Mae's authorized contract roster = the player + their FIKA partner). Distinguishes player from BD, raiders, anyone else.
- **Lore:** Mae's hand-picked team. Have been with her since before Norvinsk. The "no quarter" team. When Mae authorizes deployment, the rules of engagement have changed permanently.

### Patrol Composition (rough guidelines)

Standard patrol on Streets:
- 1× Lead (guaranteed)
- 2–3× Operatives
- 1–2× Rookies
- 0–1× Specialist (rolls per spawn)

Labs raid (Quest 5 active):
- Multiple Inner Circle deployments
- 3–5 Inner Circle per breach group
- 1+ LMG carrier per group
- 1+ MIKOR carrier per group

### Hostility Tables (summary)

| Faction | Standard Fixers (Q1–Q4) | Inner Circle (Q5) |
|---|---|---|
| Player (PMC) | Friendly | Friendly (contract recognition) |
| Other PMCs | Friendly (professional courtesy) | Hostile |
| Scavs | Hostile | Hostile |
| Raiders | Hostile | Hostile |
| Cultists | Hostile | Hostile |
| **Black Division** | **Hostile (primary target)** | **Hostile (primary target)** |
| **RUAF** | **Hostile** | Hostile |
| UNTAR | Neutral (professional respect for peacekeepers) | Hostile |

---

## 5. Quest Chain

### Quest 1 — "First Day on the Job"

**Giver:** Mae (auto-unlocks at LL1)
**Type:** Find + Hand Over (dead drop retrieval)
**Map:** Streets of Tarkov
**Objective:** Locate a marked container, retrieve sealed envelope, hand over to Mae

**Narrative:**
- Mae introduces herself as Section Manager
- Establishes the Office is in Norvinsk on contract
- Envelope contains "client correspondence" — she doesn't reveal who from
- Drops dry remark hinting at "something larger in Norvinsk, can't confirm yet"
- First coffee complaint

**Rewards:**
- Sizable USD payout
- Meaningful XP
- **+1 Charisma** (vanilla skill)
- **+1 USEC Negotiations** (SkillsExtended skill)
- Office cufflinks (flavor item — placeholder, finalize during build)
- Begins LL2 unlock progression

**Easter Egg Potential:** "First day on the job" framing — subtle nod to their first SPT raid together (October 23, 2025) via a dry Mae line.

---

### Quest 2 — "Routine Maintenance"

**Giver:** Mae
**Type:** Eliminate (faction-flexible target)
**Maps:** Any (no map restriction since raiders don't spawn naturally on Streets)
**Objective:** Eliminate X hostile faction members (RUAF, BD, goons, OR raiders count — player's choice of where/who)
- Example: 8 of any listed faction, with optional bonus tier for RUAF specifically

**Narrative:**
- "Half this work is just clearing space."
- The Office's contract requires reduced hostile interference in operational zones
- Mae hints "the actual contract" is more interesting than this
- Coffee complaint: ran out of beans, switched to instant

**Rewards:**
- Boosted USD payout
- High XP
- Suppressed weapon (custom preset name — **placeholder for Lobotomy Corporation playthrough reference**, finalize during writing phase)
- Possible custom sticker (if Custom Stickers mod compatibility confirmed)
- Substantial ammunition stockpile (Norvinsk lore: ammo as abundant supply)
- LL2 unlocked

---

### Quest 3 — "Mutual Interest"

**Giver:** Mae (now LL2)
**Type:** Find + Hand Over (intel retrieval, FIR required)
**Map:** Streets of Tarkov (or flexible)
**Objective:** Find and retrieve **TerraGroup "Blue Folders" Materials** from a Streets location, survive raid, hand over

**Narrative:**
- Mae reveals: client is **LCCB** (first explicit PM lore drop)
- "The LCCB has been following something. They think TerraGroup might have acquired an artifact. I don't ask too many questions. They pay on time."
- First mention of the **Golden Bough** — Mae says she doesn't fully know what it is, just what LCCB thinks it might be
- Mae mentions Section 1 has been getting "unusual interference"
- Coffee update: found a proper espresso setup, "operating at peak capacity"

**Rewards:**
- Boosted USD
- Mid-to-high tier gear
- First coffee-themed barter unlock at LL2
- Hint at LL3 unlock condition

---

### Quest 4 — "Hostile Workplace"

**Giver:** Mae (LL2 → unlocks LL3)
**Type:** Eliminate + Optional Retrieve
**Maps:** Any
**Objective:** Eliminate X Black Division members (any map). Optional: retrieve a BD armor/rig item (WTT-Content Backport BD piece)

**Narrative:**
- **THE REVEAL:** Mae confirms BD is TerraGroup's internal force, dispatched to clean up investigators
- Section 1 has taken casualties
- Mae isn't panicked. She's *quieter*. Professionalism gets brittle.
- "We knew this contract was risky. It's now confirmed risky. **Are you still in?**"
- The "are you still in?" beat is the player's narrative commitment moment
- Heavy coffee reference: someone in the office found her a proper espresso setup, she's "operating at peak capacity again"

**Rewards:**
- High USD
- High-tier gear (likely top-tier suppressed weapon)
- **Black Labs keycard + standard Labs access keycard** (plus possibly a third special keycard)
- LL3 unlocked
- Tension/anticipation buildup for Quest 5

---

### Quest 5 — "Closing the Office" ✨ THE FINALE

**Giver:** Mae (LL3 → unlocks LL4)
**Type:** Reach + Eliminate + Survive (Labs raid)
**Map:** Labs ONLY
**Objective:**
1. Reach the keycard room matching the Quest 4 keycard
2. Eliminate **10–20 Raiders + 5 Black Division soldiers**
3. Survive and extract
- Progress saves across raid attempts (SPT default behavior for kill counters)

**Spawn Conditions (quest-active):**
- Inner Circle (708304) spawns at elevated rates on Labs
- BD spawn rate boosted on Labs (via BlackDiv mod's existing Labs config)
- Inner Circle hostility = WEAPONS FREE except for player/contract roster

**Narrative:**
- Mae's pre-quest dialog: acknowledges this is the endgame
- Authorizes Inner Circle deployment: "they were on call. They're on the way."
- The fight is **intense** — fixers and BD clashing around the player while objectives are completed
- Post-quest dialog: brief warmth-through-deadpan moment, played off as "this operation's got me feeling exhausted, need more coffee"

**Rewards:**
- Massive USD payout
- Top-tier gear stockpile (LEDXs, Far-Forward Signal Amplifier Unit, multiple Bitcoins, multiple Labs keycards)
- LL4 unlocked
- **🎁 The Watch** — delivered via Mae's anniversary mail
- **Savior ending content unlocks** from WTT-Content Backport (giving the chain a sense of finality EFT 1.0's actual playthrough didn't quite deliver)

---

## 6. The Anniversary Moment

### The Gift: Engraved Roler Submariner

**Base item:** Roler Submariner (vanilla EFT high-value watch)
**Custom locale name:** *To be decided in writing phase* (placeholder: "Roler Submariner — Inscribed")
**Custom description structure:**

```
[Front-of-watch flavor — what someone looking at it sees]
A Roler Submariner in pristine condition. The case-back is engraved
with the Full-Stop Office crown, gold-leafed against brushed steel.

[The inscription — Mae writes this in the writing phase]
"[poetic message about time and love — written by Mae when she's ready,
in a separate text file, with no pressure on timing]"

[The signoff — Mae's framing]
— off-contract. don't ask. — M.
```

### The Mail Message

**Subject:** "Off-contract."
**Sender:** Mae
**Body structure:**
- Acknowledges what the Labs op took out of everyone
- Confirms the contract is complete; LCCB is satisfied
- Pivots — drops the professional voice briefly
- References the Norvinsk-year (subtle anniversary callout)
- Hands over the watch
- Closes with characteristic deadpan: *"Read the back."*

**Attachment:** The Engraved Roler

### Writing Phase Notes

The inscription text and the mail body are **deliberately left blank** in this design doc. Both should be written by Mae in a quiet moment, ideally in a separate text file outside this conversation, with as much time as the words need. These are the heart of the gift and shouldn't be drafted under time pressure.

The structure is locked. The exact words are yours to write when ready.

---

## 7. Technical File Structure

```
FSO-NorvinskSection1/
├── BepInEx/
│   ├── patchers/
│   │   └── FSO.NorvinskSection1.Prepatch/
│   │       └── FSO.NorvinskSection1.Prepatch.dll
│   └── plugins/
│       └── FSO.NorvinskSection1.Plugin/    (optional — only if custom client behavior needed)
│           └── FSO.NorvinskSection1.Plugin.dll
└── user/
    └── mods/
        └── FSO.NorvinskSection1.Server/
            ├── FSO.NorvinskSection1.Server.dll
            ├── package.json
            └── db/
                ├── bots/
                │   ├── types/
                │   │   ├── 708300.json    (Rookie)
                │   │   ├── 708301.json    (Operative)
                │   │   ├── 708302.json    (Specialist)
                │   │   ├── 708303.json    (Lead)
                │   │   └── 708304.json    (Inner Circle)
                │   └── config/
                │       └── spawnconfig.json
                ├── traders/
                │   └── mae_fso/
                │       ├── base.json
                │       ├── assort.json
                │       ├── questassort.json
                │       └── avatar.jpg     (Mae portrait)
                ├── quests/
                │   ├── quest1_first_day.json
                │   ├── quest2_routine.json
                │   ├── quest3_mutual.json
                │   ├── quest4_hostile.json
                │   └── quest5_closing.json
                ├── items/
                │   ├── armband.json       (FSO armband custom item)
                │   └── watch.json         (Inscribed Roler — locale override)
                ├── locales/
                │   └── en.json            (all dialog, descriptions, names)
                └── mail/
                    └── anniversary_mail.json
```

Total expected: ~15 files to write/configure across all phases.

---

## 8. Implementation Phases

### Phase 1 — Skeleton Scaffold
- Empty mod folder structure created
- Prepatcher project with stub bot registration (one bot, no behavior yet)
- Server mod with basic load
- **Goal:** SPT boots clean with mod installed but no functional content
- **Deliverable:** "Hello, world" mod that doesn't crash

### Phase 2 — Bots
- All 5 bot types implemented via prepatcher + JSON
- Hostility configured per the table above
- Test by manually setting spawn chance to 100% on one map
- **Goal:** Fixers spawn, look right, behave correctly per ROE rules
- **Deliverable:** Fixers patrolling Streets, friendly to player, hostile to scavs

### Phase 3 — Trader
- Mae as custom trader
- Portrait, basic assortment (10–15 items), dialog scaffolding
- Always-available config
- **Goal:** Mae shows up in trader list, basic buy/sell works
- **Deliverable:** Functional trader with placeholder dialog

### Phase 4 — Armband
- Custom FSO armband item
- Available via Mae's assortment at LL2
- Tested in player inventory
- **Goal:** Armband works as wearable, displays correctly
- **Deliverable:** v1 armband shipped

### Phase 5 — Quests 1–4
- All four quest definitions
- Locale entries for dialog
- Quest assort entries for LL unlocks
- **Goal:** Player can complete quest chain through Quest 4
- **Deliverable:** 80% of the gameplay loop functional

### Phase 6 — The Finale (Quest 5 + Inner Circle)
- Quest 5 definition with custom objectives
- Inner Circle bot configured (708304)
- Labs spawn config for quest-active state
- Hostility override for quest-active state
- **Goal:** Labs raid plays as designed
- **Deliverable:** Climactic finale functional

### Phase 7 — Anniversary Writing
- Mae's writing-phase pass on all dialog (final voice)
- The mail message body
- The Watch description (with inscription written separately by Mae)
- All locale entries finalized
- **Goal:** Every word in the mod is the right word
- **Deliverable:** The love letter is complete

### Phase 8 — Polish + Secret Playtest
- Full solo playthrough by Mae (NOT with Damjan present)
- Bug fixes, balance tweaks, dialog polish
- Final test of the complete arc
- **Goal:** Mod is gift-ready
- **Deliverable:** v1.0 stable

### Day-Of (June 1, 2026)
- Install on Damjan's client
- He logs in, sees Mae in his trader list
- The journey begins

---

## 9. Dependencies & Stack Notes

### Required (must be installed on both player and Damjan)
- **MoreBotsAPI** (already installed) — bot framework
- **SkillsExtended** (already installed) — USEC Negotiations skill for rewards
- **DrakiaXYZ-Waypoints** (already installed) — required for proper bot navmesh

### Compatible (already in stack, used as reference / not modified)
- **TacticalToaster-UNTARGH** — reference implementation
- **BlackDiv** — quest target faction (uses its existing Labs config)
- **RUAFComeHome** — hostility target
- **WTT-ContentBackport** — Savior ending content unlocks at Quest 5
- **Raid Overhaul** — coexists, no integration needed
- **Fika** (2.2.5+) — multiplayer sync via standard bot spawn system

### Optional / Future
- Voice Acting Framework — Mae voice lines (v2 update)
- Custom Stickers — Q2 reward sticker (if available in stack)

### Development Environment
- Visual Studio Community 2022 (confirmed installed)
- .NET 9 SDK (check during Phase 1)
- dotnet CLI (confirmed installed)
- Reference repos for code patterns:
  - `https://github.com/TacticalToaster/MoreBotsAPI`
  - `https://github.com/TacticalToaster/MoreBotsAPI-Example`
  - `https://github.com/TacticalToaster/TacticalToasterUNTARGH`

---

## 10. Open Items / TODOs

These are decisions deferred to their natural implementation phase, not blockers:

- [ ] **Custom preset name for Quest 2 weapon reward** — Lobotomy Corporation reference, decide during writing phase
- [ ] **Quest 1 cufflinks item** — pick existing flavor item from mod stack or skip if not available
- [ ] **Custom sticker for Quest 2 weapon** — depends on Custom Stickers mod availability
- [ ] **Lead Fixer custom mask texture** — attempt during Phase 2; fallback to vanilla black Mk.2 if asset work isn't feasible
- [ ] **The Watch locale name** — finalize during writing phase
- [ ] **The Watch inscription text** — Mae writes in separate file, no time pressure
- [ ] **Mae's anniversary mail body** — Mae writes in writing phase
- [ ] **Quest 1 "first day" Easter egg line** — subtle reference to October 23, 2025
- [ ] **Quest 5 progress persistence** — verify SPT's default kill-counter persistence handles this; alternative path needed if not

---

## Document Status

**v2.0 — Locked.** All major design decisions confirmed by Mae across two design sessions. No structural changes anticipated.

**Next action:** Phase 1 — Skeleton Scaffold.

---

*This document is a living reference. Update as decisions are refined during implementation. Original conception: Mae for Damjan, May 2026. 💛*
