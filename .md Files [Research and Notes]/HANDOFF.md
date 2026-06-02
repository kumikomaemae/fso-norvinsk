# FSO: Norvinsk Section 1 — Session Handoff

Last updated: May 30, 2026 — **SERVER BOOTS CLEAN** ✅

## Quick context (project identity)

Private SPT 4.0.13 mod, an anniversary/birthday gift for Damjan. **Deadline: June 1, 2026.**
Surprise — Damjan doesn't know. A Project Moon-themed allied faction (the Full-Stop
Office) deployed to Tarkov: 5 fixer bot tiers, a custom trader (Mae, her VTuber OC),
a 5-quest contract chain, and an engraved Roler Submariner as the finale gift.

- Mod GUID: `com.mae.fso.norvinsksection1` · version `0.4.0` · SPT `~4.0.0` · MIT
- Project: `C:\Dev\FSO-NorvinskSection1\`
- SPT client/BepInEx: `C:\Games\SPT\` · server + `user/mods`: `C:\Games\SPT\SPT\`
- Server mod folder: `user/mods/FSO.NorvinskSection1.Server/`
- Tone: PM Office bureaucracy meets Tarkov grit. Deadpan-professional, secretly silly, runs on coffee.

## Current state — what's LIVE (confirmed booting clean this session)

**Bots (Phase 2): done.** 5 tiers, 92 hand-picked names total.

- `708300 fsofixerrookie` / `708301 fsofixeroperative` / `708302 fsofixerspecialist` /
  `708303 fsofixerlead` / `708304 fsofixerinnercircle`
- Registered via MoreBotsAPI (`AddCustomWildSpawnTypeNames` + `CreateCustomBotTypes` +
  `LoadCustomBotConfigs`). Tier gear is baked into the type JSONs (db/bots/types/*).

**Faction: wired.** friendly with `usec`/`bear`/`rogues`; hostile to `scavs`/`scavbosses`/`sectants`;
mutual-ignore with the Goons; no warn behavior (professionals, no posturing).

**Spawns: wired.** 9 squad-composition templates (patrol_alpha/bravo, light/sweep patrol,
heavy_element, assault_team, command_element, elite_strike, ic_recon) across 5 maps —
Streets 8, Ground Zero (both tiers) 10, Customs 4, Shoreline 4, Lighthouse 2.
Spawn chance 40%, `IgnoreMaxBots = true`. Inner Circle reserved for the Q5 Labs finale.

**Trader Mae (Phase 3): registered + stocked.**

- `base.json`: id `6a1ac8598933e3f023895bd3`, USD currency, $500k float, `unlockedByDefault`,
  4 loyalty levels gated by standing (0 / 0.2 / 0.4 / 0.6), buy-price coef 60→45.
- `FsoTrader.cs` + `FsoTraderHelper.cs`: registers her, routes portrait from
  `db/trader/res/<id>.jpg`, restock 1–2h, lists on flea, applies assort.
- `assort.json`: **validated — 128 items, 31 sellable roots, 31 barters, 31 loyalty mappings.**
  Coffee ladder intact: RGN impact 1 → flash drive 3 → intel 5 → Blue Folders 8 →
  bitcoin 10 → **Dorm 314 marked key = 67 coffee** (the soul of the mod).

## The bug we just killed (documented so it never bites again)

**On SPT 4.0.13, config objects CANNOT be constructor-injected — pull them from
`ConfigServer.GetConfig<T>()`.** Injecting them directly compiles, then crashes the server
at boot with a `Microsoft.Extensions.DependencyInjection` resolution error. This bit us in
**two** files:

- `FsoTrader.cs` — was injecting `TraderConfig` / `RagfairConfig`. Fixed → ConfigServer.
- `Mod.cs` — was injecting `LocationConfig`. Fixed → ConfigServer (`_locationConfig` field).

The ~handful of "ConfigServer is obsolete" deprecation warnings are **harmless on 4.0.13** —
direct injection only becomes valid in 4.1. Leave them.

Also fixed this session: `FsoTraderHelper.cs` got accidentally overwritten with a duplicate
trader during a fresh-chat handoff; restored intact from git (`phase-3-assortment`).

## Known-harmless boot noise (don't panic next time)

- `[MoreBotsAPI] ERROR: Directory for custom loadouts not found ...db\bots\loadouts` —
  **EXPECTED / IGNORE.** Loadouts are intentionally unused (gear is baked into the bot type
  files; a loadouts system would override that). The `LoadLoadouts` call is a deliberate
  no-op. Optional silence: create an empty `db/bots/loadouts/` folder.
- `miner.exe` boot message — not ours, not malware; flavor message from another mod.
- `CS8618` in `Plugin.cs` — cosmetic nullable warning; BepInEx sets the field via Awake().

## What's next — remaining phases (per design doc)

1. **WTT-CommonLib wiring** — the gate to everything below (see next section).
2. **Items phase** — the FSO armband (Q1-era, "dress as a Fixer") + the engraved Roler watch
   (Q5 reward), as WTT custom items (`CreateCustomItems`).
3. **Quests 1–4** (Phase 5) — definitions + locales + quest-assort LL unlocks.
4. **Quest 5 + the finale** (Phase 6) — Labs raid, Inner Circle weapons-free, the Savior package.
5. **Anniversary writing** (Phase 7) — Mae's voice pass on all dialog, the mail body, the
   watch inscription (Mae writes these when ready, no time pressure).
6. **Polish + secret solo playtest** (Phase 8), then install on Damjan's client June 1.

## WTT-CommonLib wiring (prerequisite for items + quests)

Currently deferred — the trader/bots/spawns are pure SPT core + MoreBotsAPI. To build items
and quests, wire WTT-ServerCommonLib (the proven WTT-Artem pattern FSO already mirrors):

- Add `com.wtt.commonlib` `~2.0.0` to `ModMetadata.ModDependencies`.
- Add `WTT-ServerCommonLib` PackageReference (2.0.0) to the Server `.csproj`.
- Inject `WTTServerCommonLib` into `FsoTrader`, and in `OnLoad` call
  `CreateCustomItems`, `CreateCustomQuests` (+ `CreateCustomQuestZones`). These auto-load by
  folder convention from `db/CustomItems/` and `db/CustomQuests/<maeId>/`.

## Quest chain (the spine) — to build

- **Q1 "First Day on the Job"** (LL1 auto) — Streets find+handover. Rewards USD, XP, +Charisma,
  +USEC Negotiations (SkillsExtended, optional-guarded), unlocks the Artem suit+suitpants.
  Subtle nod to their first raid together (Oct 23, 2025).
- **Q2 "Routine Maintenance"** — eliminate hostiles → unlocks LL2. Suppressed-weapon reward
  (custom preset name = a LobCorp reference, finalize in writing phase).
- **Q3 "Mutual Interest"** — retrieve TerraGroup Blue Folders. First explicit reveal: client is
  the **LCCB**; first mention of the **Golden Bough**.
- **Q4 "Hostile Workplace"** — kill Black Division. THE REVEAL: BD is TerraGroup's internal force.
  Section 1 takes casualties; Mae gets quieter ("are you still in?"). Rewards Labs keycards → LL3.
- **Q5 "Closing the Office"** ✨ — Labs raid. Inner Circle (708304) deploys, **weapons-free except
  the player/contract roster**. Eliminate raiders + BD, extract → LL4 + the Savior finale.

QuestAssort success-map gates LL items; TraderStanding rewards cross base.json minStanding
0.2 / 0.4 / 0.6. Quest folder: `db/CustomQuests/<6a1ac8598933e3f023895bd3>/{Quests,QuestAssort,Locales,Images}`.

## The Savior finale (the last stone, after Q1–Q5 exist)

The mod is an EFT **Savior / "For Humanity" ending** story — and it maps beat-for-beat:
Q3 Blue Folders (secrets), Q4 BD-is-TerraGroup (the mask), Q5 Closing the Office (the strike).
Damjan *becomes* the Savior. (The Fixers already carry the Savior ending's own backported
reward weapons — the arsenal was foreshadowing the whole time.)

Split implementation:

- **Server (native Q5 rewards):** the Savior package — Savior dogtag, "For Humanity" armband,
  the arsenal, the dollars, the cases. Pure quest-reward JSON.
- **Client (a small BepInEx/Harmony plugin, same family as the faction patch):**
  (1) play **"Minutes to Midnight"** (Nikita's Savior soundtrack) on Q5 complete via AudioSource —
  *emotionally central; the violin part carries personal meaning for Mae*;
  (2) swap to the **"Peaceful Sky"** menu background after Q5;
  (3) the **"Savior"** achievement pop.
  Assets live on Mae's GitHub: `majinjdawes-hub/FSO-for-SPT`
  (Peaceful_Sky_main_menu_background.webp, Savior_icon.webp).

## The anniversary moment

Engraved **Roler Submariner**: case-back engraved with the FSO crown, gold-leafed on brushed
steel. Inscription = **Mae writes when ready** (the heart of the gift; no time pressure),
signed *"— off-contract. don't ask. — M."* Delivered via Mae's mail — subject **"Off-contract."**,
closing line *"Read the back."*

## Key IDs & paths

- Mae trader MongoID: `6a1ac8598933e3f023895bd3`
- coffee tpl: `694c6d5568b849f7bb05b7ac` · USD tpl: `5696686a4bdc2da3298b456a`
- FSO arsenal tpls (also the Savior reward weapons — watch boot log for "item not found"):
  AS VAL Mod.4 `6871284e…`, PKP belt-fed `66e718dc…`, premium rifle `67124dcf…`,
  Staccato `68452c3d…`, Siege-R `68947a4b…`
- Roler watch tpl: `59faf7ca86f7740dbe19f6c2` *(verify against items before use)*
- Blue Folders + Dorm 314 key: exact tpls are already in `assort.json` — read them from there
- Trader files: `db/trader/{base.json, assort.json, res/<id>.jpg}`

## Locked decisions / gotchas (carry forward)

- **Config = `ConfigServer.GetConfig<T>()` on 4.0.13**, never constructor injection.
- WTT-CommonLib deferred until the items/quests phases.
- Avatar `res/` lives INSIDE `db/trader/` (not at mod root).
- Deploy is parasitic on the Server project building — **Rebuild after JSON-only edits**
  (`.cs` changes build + deploy normally).
- Bot type names: lowercase, no spaces.
- Mae is NOT in any bot pool (trader / section manager). "The Mae of Hatred" = Inner Circle
  cameo *title*, not her.
- Roland appears as "Roland" (Lead), not "The Black Silence"; Gebura as Gebura (Lead), not
  "The Red Mist." Those two Colors don't appear in FSO.
- Damjan hasn't finished Library of Ruina — avoid Argalia / Olivier / Edgar / Tanya. Sephirot safe.
- ASCII-only in names ("Ryoshu", "Bong-bong").
- Loadouts intentionally unused (the directory-not-found error is expected/benign).
- "Minutes to Midnight" (Savior soundtrack) is the finale's emotional core — honor it in wiring.

## Git checkpoints

`phase-3-presets-100` (bots) · `phase-3-trader-skeleton` · `phase-3-assortment` (5a07807) ·
`phase-3-trader-live` (this session — full backbone boots clean).
