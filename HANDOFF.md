# FSO: Norvinsk Section 1 — Session Handoff

Last updated: May 30, 2026

## Quick context for picking-up Claude

I'm Mae, building a private SPT 4.0.13 mod called **FSO: Norvinsk Section 1**
as an anniversary/birthday gift for my boyfriend Damjan. Deadline:
**June 1, 2026**. It's a Project Moon-themed custom faction mod: allied fixer
bots across 5 tiers, a custom trader named Mae (my VTuber OC), a 5-quest
contract chain, and an engraved/renamed Roler Submariner watch as the final
reward.

**The mod is a surprise — Damjan doesn't know.**

- Project: `C:\Dev\FSO-NorvinskSection1\`
- SPT client/BepInEx: `C:\Games\SPT\`
- SPT server + `user/mods`: `C:\Games\SPT\SPT\`

## Where we are: Phase 3 (Trader), end of 3a — ONE fix pending

Two build axes are in play.

### Axis 1 — Bots (Phase 2): DONE

- All 5 FSO bot tiers have full custom name pools — 92 names total.
- Build pipeline works, deploy auto-fires, MoreBotsAPI registers all 5 tiers
  on every boot.
- Tiers: Rookie (32) / Operative (25) / Specialist (15) / Lead (12) /
  Inner Circle (8).
- Enum range 708300–708304 (within reserved block 708300–708399).

### Axis 2 — Trader (Phase 3): IN PROGRESS

- Found and used **SPT Scaffold** (viniHNS, forge mod 2633) to validate the
  project's `.csproj` / `Mod.cs` shape and set up auto-deploy.
- Locked the trader registration pattern by studying **WTT-Artem** (FSO
  already deps `com.wtt.commonlib`, so we mirror its proven pattern).
- **3a (trader skeleton) is written and COMPILES.** Three files:
  - `FsoTrader.cs` — `[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)] : IOnLoad`
  - `FsoTraderHelper.cs` — `[Injectable]`, port of WTTArtemHelper
  - `db/trader/assort.json` — empty (`{"items":[],"barter_scheme":{},"loyal_level_items":{}}`)
  - Trader id: `6a1ac8598933e3f023895bd3`
  - Reads `db/trader/base.json`, routes portrait from `db/trader/res/<id>.jpg`,
    restock 1–2h, `_ragfairConfig.Traders.TryAdd`, registers empty assort,
    sets locale strings, overwrites assort.
  - Success log line: `[FSO] Trader 'Mae' registered — the Office is open. Coffee's hot.`

### >>> THE ONE PENDING FIX — DO THIS FIRST <<<

During a "clean build" pass I refactored `FsoTrader` to inject `TraderConfig`
and `RagfairConfig` **directly into the constructor** (intending to
future-proof for SPT 4.1, where `ConfigServer` is removed).

**This is wrong for 4.0.13.** It compiles, but the server **crashes at boot**
with a `Microsoft.Extensions.DependencyInjection` resolution stack trace,
because 4.0.13's DI container does NOT hand out config objects as constructor
params. The deprecation warning is misleading — direct injection is a 4.1 thing.

**Fix (revert `FsoTrader` to pull configs from `ConfigServer`):**

- inject `ConfigServer configServer` as a ctor param
- re-add `using SPTarkov.Server.Core.Servers;`
- `_traderConfig = configServer.GetConfig<TraderConfig>();`
- `_ragfairConfig = configServer.GetConfig<RagfairConfig>();`
- remove the direct `TraderConfig` / `RagfairConfig` ctor params

This returns to the version that built. The ~4 `ConfigServer`-deprecation
warnings come back — harmless; ignore until/unless we actually move to 4.1.
(Corrected `FsoTrader.cs` is provided alongside this handoff.)

After the revert: `dotnet build` → restart → expect the "Coffee's hot." line
plus Mae's card in the trader list with her portrait. **Empty stock is
expected** — filling it is 3b.

## What's next (roadmap)

1. **Apply the config revert**, verify Mae registers in the trader list. ← immediate
2. **Phase 3b — fill the assortment:** coffee barters, the 67-coffee marked-keys
   joke (LL4 — "the soul of the mod"), the engraved Roler Submariner as the
   final reward, the faction armband. WTT-CommonLib comes back here via
   `CreateCustomItems` (armband + watch).
3. **Quests phase:** the 5-quest contract chain via WTT-CommonLib
   `CreateCustomQuests` (+ `CreateCustomQuestZones`).
4. **Faction wiring** (MoreBotsAPI `FactionService`): make FSO bots allied to
   USEC/Bear (+Scav TBD), grouped under one "FSO" faction so they don't shoot
   each other. **STATUS: verify whether this got done earlier this session; if
   not, it's a small Mod.cs addition** (`AddFriendlyByFaction`, etc.).
5. **Appearance / equipment pools per tier:** WTT-Artem suits + per-tier
   inventory loadouts per the design doc's gear-by-role table.
6. **Trader dialogue / locale text** in Mae's voice — incl. the "three great
   coffee shrines" thread (Sojiro / Chesed / Mae).
7. **Playtest with Damjan in Fika**, final polish for June 1.

## Locked decisions / gotchas (carry forward)

- **WTT-CommonLib is deferred.** The trader *skeleton* is pure SPT core
  (ModHelper / ImageRouter / ConfigServer / TimeUtil / DatabaseService /
  ICloner). WTT-CommonLib (`com.wtt.commonlib`) returns for the **items** and
  **quests** phases.
- **Config access on 4.0.13 = `ConfigServer.GetConfig<T>()`**, NOT direct
  constructor injection. (See pending fix above.)
- **Avatar path:** `db/trader/res/<id>.jpg` — `res/` is INSIDE `db/trader/`,
  not at mod root. (Divergence from WTT-Artem's mod-root `res/`.)
- **FsoTraderHelper uses the locale dictionary indexer (not `.Add`)** for
  re-run safety — intentional divergence from the decompile.
- **`OverwriteTraderAssort` takes a `MongoId`** (not `string`).
- **Deploy is parasitic on the Server project building.** If only `db/**` data
  files change, plain Build marks Server "up-to-date" and skips deploy. Use
  **Rebuild Solution** after JSON-only edits.
- **Bot type names: lowercase, no spaces** (`fsofixerrookie`, etc.) — SPT
  lowercases filenames; pre-aligned in prepatcher + Mod.cs.
- **Mae is NOT in any bot pool** — she's the trader / section manager. "The Mae
  of Hatred" in Inner Circle is a cameo *title*, not her.
- **Roland appears as 'Roland' (Lead), not 'The Black Silence'; Gebura as
  Gebura (Lead), not 'The Red Mist'.** Identity-cohesion rule — those two
  Colors don't appear in FSO.
- **Damjan hasn't finished Library of Ruina** — avoid Argalia / Olivier /
  Edgar / Tanya. Sephirot are safe.
- **ASCII only in names** (Unity rendering): "Ryoshu", "Bong-bong", etc.
- **Capital "The"** in Color Fixer titles: "The Mae of Hatred".

## Quirks (non-blocking)

- **`miner.exe` boot message** ("Failed to launch miner.exe, please restart the
  server") — NOT ours, NOT malware; flavor message from another mod (suspect
  kitteh-welcome-messages). stdout-only, not in LogOutput.log. Ignore.
- **CS8618 warning** in `Plugin.cs` (non-nullable Instance field) — cosmetic;
  BepInEx sets it via Awake(). Ignore.

## Note for the next picking-up Claude

This state was reconstructed via past-conversation search after a chat was lost
mid-fix. The code details here (file contents, signatures, trader id, paths)
are rebuilt from session notes, not a live file read — sanity-check the provided
`FsoTrader.cs` against the actual project before trusting it verbatim, and paste
any compile/runtime error with its line number.
