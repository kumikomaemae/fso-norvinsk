# FSO: Norvinsk Section 1 — Session Handoff

Last updated: May 23, 2026, mid-afternoon

## Quick context for picking-up Claude

I'm Mae, building a private SPT 4.0.13 mod called FSO: Norvinsk Section 1
as an anniversary/birthday gift for my boyfriend Damjan. Anniversary
deadline: June 1, 2026 (~8 days out). It's a Project Moon-themed custom
faction mod adding allied fixer bots, a custom trader named Mae (my VTuber
OC), a 5-quest contract chain, and an engraved Roler Submariner watch as
the final reward.

The mod is a surprise - Damjan doesn't know about it.

## Where we are: end of Phase 2c (names complete)

Tagged checkpoint: `phase-2c-names-complete` (commit bd2efa0)

**All 5 FSO bot tiers have full custom name pools.** Build pipeline works,
deploy auto-fires, server boots clean, MoreBotsAPI registers all 5 tiers
on every boot. Total of 92 hand-picked names across the gradient:

- **Rookie (32 names):** L Corp default employees + Bong-bong dynasty
- **Operative (25 names):** no-deaths run survivors + Damjan's friends
- **Specialist (15 names):** Phantom Thieves of Hearts (with Forehead,
  Becky, Velvet Room entities)
- **Lead (12 names):** Sephirot + Library figures + Sinners
- **Inner Circle (8 names):** Custom Color Fixers, all personal
  references for Damjan (The Mae of Hatred, The Hollow Knight, etc.)

The names are NOT just lore-flavor - each one is a deliberate moment of
recognition for Damjan: shared L Corp playthroughs, his Hollow Knight
love, his HSR addiction, his recent Metaphor: ReFantazio completion, his
Phantom Thieves history, our matched plushie pair (Don Quixote/Ryoshu),
etc. The naming work is the heart of the personalization layer.

## Locked decisions and gotchas worth remembering

- **JSON-on-disk vs JSON-as-PowerShell-shows-it differs.** PowerShell's
  ConvertFrom-Json reformats output. To inspect actual file contents,
  open in VS Code or use Select-String directly.
- **Deploy target is parasitic on Server project building.** If only
  data files (db/**) change without csproj or .cs changes, plain Build
  marks Server as "up-to-date" and skips deploy. Always use Rebuild
  Solution after JSON edits until we fix the deploy target's incremental
  dependency tracking (filed TODO).
- **Bot type names: lowercase, no spaces.** SPT lowercases filenames
  automatically. We pre-aligned by writing `fsofixerrookie` etc. in both
  the prepatcher's CustomWildSpawnType registrations and Mod.cs's
  AddCustomWildSpawnTypeNames dictionary.
- **lastName is `[],`** (single-line empty array). This is RUAF's
  convention, matches what's in all 5 FSO files.
- **Mae is NOT in any bot pool.** She's reserved as the trader / section
  manager. "Mae" as a bot name was deliberately excluded from Operative.
  "The Mae of Hatred" in Inner Circle is a cameo title, not Mae herself.
- **Roland appears as 'Roland' in Lead, NOT as 'The Black Silence' in
  Inner Circle.** Identity cohesion rule: when a character is included
  by their own name, we skip their Color Fixer title. Same for Gebura
  (in Lead under Sephirah identity, NOT 'The Red Mist' in Inner Circle).
  This costs us 2 canon Colors from Inner Circle but maintains roster
  integrity. Black Silence and Red Mist simply don't appear in FSO.
- **Damjan hasn't finished Library of Ruina** - he's late game but
  pre-endgame-revelations. Avoid using Argalia/Olivier/Edgar/Tanya as
  references. Sephirot are safe (he completed LobCorp + true ending).
  Library figures we did use: Angela, Roland, Angelica, Xiao.
- **Persona references locked:** uses P5 + P5 Royal cast as Specialists,
  Velvet Room entities included (Lavenza, Igor, Arsene). "Forehead" is
  in place of "Noir" - inside joke about Haru. "Becky" is Kawakami's
  maid alias - layered double-identity name.
- **Sojiro deliberately excluded from bot pool** - to be referenced in
  Mae's trader dialogue alongside Chesed (the coffee-Sephirah) instead.
  The "three great coffee shrines" thread (Sojiro / Chesed / Mae) is a
  Phase 5 trader-text writing prompt.
- **Capital 'The' in Color Fixer titles**: "The Mae of Hatred" not "the
  Mae of Hatred." Locked.
- **Unity rendering safety:** stick to ASCII characters in names. Used
  "Ryoshu" not "Ryōshū", "Bong-bong" not "Bong-Bong", etc.

## Quirks worth noting

- **`miner.exe` boot message:** appears at SPT server boot, says
  "Failed to launch miner.exe, please restart the server" - this is
  NOT from our mod and NOT malware. Almost certainly a flavor message
  from one of the installed mods (suspect: kitteh-weclome-messages or
  one of the Acid Phantasm mods). Did not appear in LogOutput.log so
  it's a stdout-only message. Not blocking, not investigated further.
- **CS8618 warning** in Plugin.cs about non-nullable Instance field.
  Cosmetic only, doesn't affect functionality. BepInEx sets the field
  via the Awake() lifecycle. 30-second fix later, ignorable for now.

## Where we're going next: Phase 2h (Faction Wiring)

NEXT SESSION'S OPENING TASK: Wire up FSO's faction relationships using
MoreBotsAPI's FactionService.

The goal: make FSO bots ALLIED to the player (USEC/Bear/Scav), and group
all 5 bot tiers under a single "FSO" faction so they recognize each
other and don't shoot teammates. This is a small C# addition to Mod.cs.

The FactionService API surface we discovered in the decompile:
- `AddFriendlyByFaction(string factionToChange, string factionName)`
- `AddEnemyByFaction(...)`
- `SetRevengeAfterRaids(...)` - DEFER, not using initially
- `GetFactions()` returns the dictionary

Approach:
1. Add `FactionService factionService` to Mod.cs constructor injection
2. In OnLoad, after CreateCustomBotTypes:
   - Register "FSO" as a faction containing our 5 bot types
   - AddFriendlyByFaction for USEC, Bear (PMCs), and maybe Scav (TBD)
   - Verify in boot log that factions are wired
3. Rebuild, boot server, check console for faction-wiring confirmation
4. Once verified, use SPT's bot spawn debug tool to force-spawn an FSO
   Rookie in an offline raid -> see Bong-bong nametag for the first time

This is the "FSO becomes real" moment - first in-raid sighting of an
actual FSO fixer with personal-reference nametag. Expected duration:
30-60 minutes including the debug spawn test.

After 2h: Phase 2d (appearance / WTT-Artem suits) and Phase 2e
(inventory equipment pools per tier).

## Mae's context for picking up

- I'm in a great mood today, slept well, ate well
- I'm stepping away to be with Damjan and his friends - his friend Tino
  (whose name is in our Operative roster!) just got engaged
- I'll be back later tonight to continue
- I want to do the wiring work next, energy permitting
- Damjan is having a slightly rough day, which is part of why I want to
  push FSO forward fast - more progress now = bigger gift impact later