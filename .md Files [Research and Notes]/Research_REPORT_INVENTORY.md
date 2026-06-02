# SPT 4.0.13 Bot TYPE FILE: Deterministic Loot, Stacks, Meds & Mags — Mechanics & Fixes

## TL;DR
- In the C# server (sp-tarkov/server-csharp), a bot TYPE FILE has two relevant blocks: `inventory.items` (five tpl→weight pools: `Backpack`, `Pockets`, `TacticalVest`, `SecuredContainer`, `SpecialLoot`) and `generation.items` (per-category `{weights, whitelist}` where `weights` is a count→weight map and `whitelist` is a tpl→weight dictionary). To make a bot carry meds/mags/grenades you must set BOTH the count `weights` in `generation.items` AND supply the tpls (curated `whitelist`, or the matching `inventory.items` pool).
- The zero-stack GP-coin bug is NOT in the type file. Bot money stack SIZE is set by `RandomiseMoneyStackSize`, which reads `currencyStackSize[botRole][currencyTpl]` from `configs/bot.json`. There is a fallback on botRole→`"default"` but NO fallback on currency tpl, so a GP-coin tpl missing from the table (or a `"0"` weighted entry) yields an empty/zero stack. Fix: add the GP-coin tpl with a weighted stack table under `default` (or your bot role) in bot.json.
- For near-deterministic loot, use peaked `weights` (e.g. `{"1":1}` to force exactly one) plus tiny curated `whitelist`s; use the WTT loadout service for gear/mods/ammo. An empty `whitelist` combined with an empty `inventory.items` pool produces nothing for that category even when count weights ask for 1–2 — which is exactly why your faction spawns zero meds.

## Key Findings

### 1. Bot type `inventory` structure (confirmed 4.0.x C#)
The C# model `BotTypeInventory` (Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotType.cs) contains `equipment`, `ammo`, `items`, and `mods`. The `items` sub-object (`ItemPools`) has exactly five keys:
- `Backpack`
- `Pockets`
- `TacticalVest`
- `SecuredContainer`
- `SpecialLoot`

Each is a `Dictionary<MongoId, double>` — a **tpl→weight dictionary, NOT a flat array of tpl strings**. Example:
```json
"items": {
  "Backpack":        { "<tpl>": 12, "<tpl>": 4 },
  "Pockets":         { "<tpl>": 8 },
  "TacticalVest":    { "<tpl>": 6 },
  "SecuredContainer":{ "<tpl>": 2 },
  "SpecialLoot":     { "<tpl>": 1 }
}
```
There is no `Eyewear` key here (eyewear lives under `inventory.equipment`, keyed by equipment slot; `inventory.equipment` is `Dictionary<EquipmentSlots, Dictionary<MongoId, double>>`, and `inventory.ammo` is `Dictionary<string, Dictionary<MongoId, double>>` keyed by caliber).

These pools are consumed by `BotLootCacheService` (DeepWiki "Bot Loot & Caching"), which classifies each tpl into typed category caches: `Special, Backpack, Pocket, Vest, Secure, Combined, HealingItems, GrenadeItems, DrugItems, FoodItems, DrinkItems, CurrencyItems, StimItems`. The container caches filter out inappropriate types: **Backpack/Pocket/Vest exclude magazines, bullets, grenades, medical, food, drink, and currency; Secure excludes only magazines and bullets.** This means meds, grenades, food, drink, and currency tpls placed in any pool are routed into their own typed caches (e.g. HealingItems, CurrencyItems) regardless of which container key they sit under — the container keys mainly govern generic loot items, while consumable categories are picked up by classification.

How items are picked: the generator (`BotLootGenerator.GenerateLoot`) reads a per-category count from `generation.items.<cat>.weights` (weighted random number of items), then calls `AddLootFromPool` against the matching typed cache, doing weighted-random tpl selection up to that count.

### 2. Item stack counts — the zero GP-coin bug (root cause identified)
Two separate mechanisms:
- The **NUMBER** of currency items a bot carries = `generation.items.currency.weights` (count→weight), drawn via `AddLootFromPool` (the currency call passes `itemSpawnLimits: null`, so spawn limits do NOT cap currency).
- The **stack SIZE** of each money item is set in `BotGeneratorHelper.AddRequiredChildItemsToParent` → `RandomiseMoneyStackSize(botRole, itemTemplate, moneyItem)`:
```csharp
if (!BotConfig.CurrencyStackSize.TryGetValue(botRole, out var currencyWeights))
    currencyWeights = BotConfig.CurrencyStackSize["default"];
var currencyWeight = currencyWeights[moneyItem.Template];   // indexes by currency tpl — NO per-tpl fallback
moneyItem.AddUpd();
moneyItem.Upd.StackObjectsCount =
    int.Parse(weightedRandomHelper.GetWeightedValue(currencyWeight));
```
The config field is **`currencyStackSize`** in `configs/bot.json`, typed `Dictionary<string, Dictionary<string, Dictionary<string, double>>>` = `currencyStackSize[botRole][currencyTpl][stackSizeString] = weight`. GP coins inherit `BaseClasses.MONEY`, so they go through this exact path.

**Causes of a 0/empty stack on a bot-carried GP coin / currency:**
1. **Most likely:** the GP-coin tpl is absent from `currencyStackSize` both for your bot role AND for `default`. Because `currencyWeights[moneyItem.Template]` is a raw dictionary index with **no per-currency fallback**, no valid stack value is assigned — the item is left with its minimal default (effectively empty/zero). GP coins were historically omitted from currency stack tables.
2. The weight table for that currency contains a `"0"` stack-size key — the weighted roll can literally return a 0 stack.
3. The item isn't recognized as `BaseClasses.MONEY` (custom or misparented item) so the money branch never runs; `GenerateExtraPropertiesForItem` only forces `StackObjectsCount ??= 1` when `forceStackObjectsCount` is true.

Ammo stack size is a different path: `RandomiseAmmoStackSize` → `ItemHelper.GetRandomisedAmmoStackSize`, which reads the ammo template `_props.StackMaxRandom`/`StackMinRandom` (a missing/zero pair gives an empty ammo stack). Wallet contents use a separate `walletLoot.stackSizeWeight` system.

**Fix (server-mod bot.json patch):** add the GP-coin tpl under `currencyStackSize["default"]` (and/or your custom role) with a non-zero weighted table, e.g.:
```json
"currencyStackSize": {
  "default": {
    "5d235b4d86f7742e017bc88a": { "1": 6, "2": 3, "3": 1 }   // GP coin, no "0" key
  }
}
```
Verify no `"0"` keys exist anywhere in your currency tables. A correct non-zero GP-coin/bitcoin stack is achieved here, not in the type file.

### 3. `generation.items` weight tables
Model `GenerationWeightingItems` (BotType.cs) has these exact keys, each a `GenerationData`:
`grenades, healing, drugs, food, drink, currency, stims, backpackLoot, pocketLoot, vestLoot, magazines, specialItems, looseLoot`.

`GenerationData` shape:
```csharp
public Dictionary<double,double> Weights { get; set; }      // count -> weight (how MANY spawn)
[JsonConverter(typeof(ArrayToObjectFactoryConverter))]
public Dictionary<MongoId,double> Whitelist { get; set; }   // tpl -> weight
```
**Critical 4.0.x finding:** `GenerationData` has only `weights` and `whitelist` — **there is NO `blacklist` key** (unlike older expectations). `weights` keys are item COUNTS (e.g. `{"0":5,"1":10,"2":3}` = weighted choice of how many to spawn). `whitelist` is a tpl→weight dict; it commonly ships as an empty array `[]` in vanilla JSON and is deserialized to an empty dict via `ArrayToObjectFactoryConverter`.

`GenerateLoot` requires `.Weights` to be present for BackpackLoot, PocketLoot, VestLoot, SpecialItems, Healing, Drugs, Food, Drink, Currency, Stims, and Grenades, or it logs `bot-unable_to_generate_bot_loot`.

**Empty-whitelist behaviour (your bug):** the tpls for healing/drugs/stims/grenades/specialItems come EITHER from the category `whitelist` OR, when that is empty, from the bot's `inventory.items` pools as classified by `BotLootCacheService`. If BOTH the category whitelist AND the relevant `inventory.items` source are empty, the category produces nothing even when count weights ask for 1–2 items. That is precisely why your custom faction (empty whitelists for healing/drugs/stims/magazines and presumably no med tpls in the inventory pools) spawns zero meds and unreliable mags.

### 4. Meds / healing / stims — exact JSON to set
You must set BOTH (a) non-zero count weights and (b) a tpl source. Two valid, equivalent approaches:

**Approach A — curated whitelist (most deterministic):**
```json
"generation": {
  "items": {
    "healing": { "weights": { "1": 3, "2": 1 },
      "whitelist": { "590c678286f77426c9660122": 5, "590c657e86f77412b013051d": 1 } },
    "drugs":   { "weights": { "0": 2, "1": 1 },
      "whitelist": { "5755383e24597772cb798966": 3 } },
    "stims":   { "weights": { "0": 3, "1": 1 },
      "whitelist": { "5c0e534186f7747fa1419867": 1, "5c0e530286f7747fa1419862": 1 } }
  }
}
```

**Approach B — inventory pool:** add the med/stim tpls to `inventory.items.Pockets`/`TacticalVest`/`SpecialLoot`; the loot cache classifies them into HealingItems/DrugItems/StimItems automatically, and the `generation.items.<cat>.weights` count governs how many are drawn.

**Verified med/stim TPLs (independently confirmed):** IFAK `590c678286f77426c9660122` (confirmed via Tarkov Database: "IFAK is a personal medical kit issued to soldiers in service"); AFAK `60098ad7c2240c0fe85c570a` (confirmed via Tarkov Database: "A more advanced version of the IFAK individual first aid kit"); Grizzly `590c657e86f77412b013051d` (confirmed via Tarkov Database: "Sportsman Series Grizzly Medical Kit is considered one of the best first aid kits"). Standard EFT IDs (verify against your installed SPT_Data/templates/items or the `ItemTpl` enum): Salewa `544fb45d4bdc2dee738b4568`; Car med kit `590c661e86f7741e566b646a`; CALOK-B `5e8488fa988a8701445df1e4`; Surv12 `5d02797c86f774203f38e30a`; AI-2 `5755383e24597772cb798966`; eTG-c `5c0e534186f7747fa1419867`; Propital `5c0e530286f7747fa1419862`; SJ6 `5c0e531286f7747fa54205c2`; Zagustin `5c0e533786f7747fa23f4d42`; Adrenaline `5c10c8fd86f7743d7d706df3`.

### 5. Magazines & grenades
Spare-magazine count is governed by `generation.items.magazines.weights`. **Magazine selection is tied to the bot's equipped weapon, not to inventory.items** — the generator finds magazines compatible with the bot's weapon and "generates extra magazines or bullets (if magazine is internal) and adds them to TacticalVest and Pockets; additionally, adds extra bullets to SecuredContainer" (SPT server API docs). The Backpack/Pocket/Vest loot caches explicitly exclude magazines, so an empty `magazines.whitelist` does NOT break mags — the bot still draws weapon-compatible mags. Unreliable mag counts therefore stem from low/zeroed `magazines.weights`, not the empty whitelist. Grenade count = `generation.items.grenades.weights`, with tpls from `grenades.whitelist` or the GrenadeItems cache. To carry ~2–3 spare mags and 1–2 grenades:
```json
"magazines": { "weights": { "2": 3, "3": 2 }, "whitelist": [] },
"grenades":  { "weights": { "1": 3, "2": 1 }, "whitelist": { "<frag_tpl>": 3, "<smoke_tpl>": 1 } }
```
Ensure the bot has a valid primary weapon so compatible mags can be found.

### 6. Best-practice / near-deterministic loot
Vanilla bot types use broad pools with smooth count distributions; for a tightly controlled "fixer" PMC do the opposite:
- **Peaked count weights** to force a fixed amount, e.g. `{"1":1}` = always exactly one.
- **Tiny curated whitelists** for the valuable/barter/intel items so only your chosen tpls can appear.
- **Low currency count** (e.g. `currency.weights {"1":8,"2":2}`) and rely on bot.json `currencyStackSize` to set sensible per-stack roubles, not too high.
- **Keep keycards/bitcoin out** of whitelists, or give them very small weights, so they appear rarely.
- Use `specialItems.whitelist` for curated valuables rather than dumping high-value tpls into Backpack.

### 7. Loadout service choice (gear side)
Prefer the **WTT WTTCustomBotLoadoutService** (db/CustomBotLoadouts/ — merges Chances + Inventory{Equipment, Mods, Ammo} + Appearance) over the MoreBotsAPI LoadoutService (db/bots/loadouts/ — equipment + mods only), because the WTT service additionally carries ammo and chances, giving you control over weapon-mod/equipment spawn probabilities. The WTT service is sound for MoreBotsAPI-registered bots: BlackDiv (also MoreBotsAPI-based, built for SPT 4.0.13, depending on MoreBotsAPI 2.0.1 + WTT-CommonLib 2.0.20) ships using it. No known blocking issues running the WTT loadout service for MoreBotsAPI bots.

### 8. The `.sptids` file
The `.sptids` file beside loadout JSONs holds generated in-game item IDs and is auto-managed by the WTT loadout pipeline. It is **safe to ignore when hand-authoring loadout JSON** — you do not need to create or maintain it manually; the service regenerates/uses it as needed.

## Details — 3.x (TypeScript) vs 4.0.x (C#)
The data SHAPES are largely identical between the TS 3.x server and the C# 4.0.x server: `inventory.items` pools and `generation.items` weight tables exist in both, and the magazine/bullet generation behaviour (extra mags/bullets to TacticalVest + Pockets, extra bullets to SecuredContainer) is the same. Differences confirmed for 4.0.x:
- `GenerationData` exposes only `weights` + `whitelist` — **no `blacklist`** on the per-category generation object.
- Currency stack size is config-driven via **`currencyStackSize`** in `configs/bot.json` (`botRole → currencyTpl → stackSizeString → weight`), with a `default` role fallback but no per-currency fallback. The legacy TS `randomisedLootItemStackCount`/`minMaxLootValue` concept is gone.
- `inventory.items` containers and `generation.items.whitelist` are tpl→weight dictionaries deserialized from JSON arrays via `ArrayToObjectFactoryConverter` (so vanilla `[]` becomes an empty dict).
- Currency item-count is not capped by `itemSpawnLimits` (the currency `AddLootFromPool` call passes `null`).

## Recommendations
1. **Fix the GP-coin zero stack first.** Add a server-mod bot.json patch inserting the GP-coin tpl (`5d235b4d86f7742e017bc88a`) into `currencyStackSize["default"]` (and/or your bot role) with `{ "1": 6, "2": 3, "3": 1 }`. Audit every currency table for stray `"0"` keys. Do the same for bitcoin if the bot should carry a set number.
2. **Make meds reliable.** Set `generation.items.healing.whitelist` to a curated med tpl→weight dict and `healing.weights` to `{"1":3,"2":1}`; repeat for `drugs` and `stims`. Confirm both the count weights are non-zero AND the whitelist (or an inventory.items source) is non-empty.
3. **Fix mags/grenades.** Set `magazines.weights` peaked at 2–3 and `grenades.weights` at 1–2; leave `magazines.whitelist` empty (mags come from weapon-compatible pool) but populate `grenades.whitelist` with your chosen frag/smoke tpls. Ensure the bot has a valid primary weapon.
4. **Curate valuables narrowly.** Use small `specialItems`/pool whitelists with peaked weights; keep keycards/bitcoin out or near-zero.
5. **Keep gear on the WTT loadout service; ignore `.sptids`.**

Thresholds that change the plan: if meds still don't spawn after setting whitelist+weights, check the loot-cache classification (item baseclass) and bot.json equipment/loot blacklists; if currency still zeroes, the tpl isn't `MONEY` baseclass or is still missing from `currencyStackSize`; if mags are still unreliable, the bot's weapon may have no compatible mag in the database for the chosen ammo/caliber.

## Caveats
- I could not quote literal currency tpl→weight values from a vanilla `assault.json`/`bear.json` this session; the structure (keys, dict shapes, code paths) is confirmed from the C# model classes (`BotType.cs`, `BotConfig.cs`), `BotLootGenerator.cs`/`BotGeneratorHelper.cs`, and DeepWiki, which are authoritative for shapes. Confirm exact stack-weight values by inspecting your installed `configs/bot.json`.
- `ItemHelper.GetRandomisedAmmoStackSize` body was not directly fetched; its role (reading `_props.StackMaxRandom/StackMinRandom`) is inferred from the calling method `RandomiseAmmoStackSize`.
- IFAK, AFAK, and Grizzly TPLs were independently confirmed via Tarkov Database; the remaining med/stim TPLs are standard EFT IDs that should be verified against your installed `SPT_Data/templates/items` or the `ItemTpl` enum before use.
- DeepWiki text at one point references `BotConfig.ItemSpawnLimits` for currency, but the actual C# code passes `itemSpawnLimits: null` to the currency `AddLootFromPool` call — so spawn limits do not cap currency; stack size is purely `currencyStackSize`. Treat the code path as authoritative over the prose.