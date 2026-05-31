# Making Custom Trader "Mae" Sell Quest-Locked Clothing in SPT 4.0.13 with WTT-CommonLib

## TL;DR
- The correct tool is **`wttCommon.CustomClothingService.CreateCustomClothing(assembly)`** (NOT `CustomCustomizationService`, which is a distinct service for hideout walls/decor and shooting-range marks). It reads JSON from your mod's **`db/CustomClothing/`** folder and, per the official WTT-CommonLib README, "will attempt to automatically add the services tab to traders that do not already have it" — which is exactly what stops Mae's `GetTraderSuits` crash loop.
- Each clothing JSON both DEFINES a suit (suiteId/outfitId/topId/handsId) AND registers it as a trader offer via a `traderId` field plus a requirements block including `questRequirements`. Set `traderId` to Mae's MongoID `6a1ac8598933e3f023895bd3` and `questRequirements: ["217e6ba5ff7f1752ff218687"]` to gate the Notch Lapel suit behind the quest.
- The crash is caused by Mae having no suits collection; registering even one clothing entry to her gives `GetTraderSuits(traderId)` a non-empty list and ends the "Unable to get suits from trader" loop. Register clothing AFTER the trader exists, at `OnLoadOrder.PostDBModLoader + 2`.

## Key Findings

### 1. Which service to use
WTT-ServerCommonLib 2.0.20 (confirmed published on NuGet at nuget.org/packages/WTT-ServerCommonLib/) exposes both relevant services on the master `WTTServerCommonLib.WTTServerCommonLib` object, and they are explicitly distinct:
- **`CustomClothingService` (`WTTCustomClothingService`)** — "Adds custom clothing sets (tops, bottoms) for players." Registration method: `CreateCustomClothing(assembly)`. This is the service that creates wearable suits AND wires them to a trader's customization/services tab.
- **`CustomCustomizationService` (`WTTCustomCustomizationService`)** — "Registers custom hideout decorations, customization items, icons, and shooting range mark textures." Registration method: `CreateCustomCustomizations(assembly)`. WelcomeToTarkov's own Tarkov-1.0-Backport mod (`WTTContentBackport.cs`) calls these two as separate, unrelated services — confirming `CustomCustomizationService` is the WRONG tool here. It handles hideout walls/ceilings/floors and shooting-range textures, not trader clothing offers.

### 2. The CustomClothingService API
- **Method signature**: `Task CreateCustomClothing(Assembly assembly)` with an overload `Task CreateCustomClothing(Assembly assembly, string relativePath)`.
- **Call pattern** (identical to `CustomQuestService.CreateCustomQuests` / `CustomLocaleService.CreateCustomLocales`, which also take an `Assembly`):
  ```csharp
  var assembly = Assembly.GetExecutingAssembly();
  await wttCommon.CustomClothingService.CreateCustomClothing(assembly);
  // or custom path:
  await wttCommon.CustomClothingService.CreateCustomClothing(assembly, Path.Join("db", "MyCustomClothingFolder"));
  ```
- **Folder**: default `db/CustomClothing/` (exact casing: `CustomClothing`, PascalCase, single folder directly under `db/`). One or more JSON files inside.

### 3. Clothing JSON structure (verbatim from the official README)
```json
{
  "type": "top",
  "suiteId": "PUT A UNIQUE MONGOID HERE",
  "outfitId": "PUT A UNIQUE MONGOID HERE",
  "topId": "PUT A UNIQUE MONGOID HERE",
  "handsId": "PUT A UNIQUE MONGOID HERE",
  "locales": {
    "en": { "name": "Lara's Tattered Tank Top", "description": "Women's Upper" }
  },
  "topBundlePath": "clothing/lara_top.bundle",
  "handsBundlePath": "clothing/lara_hands.bundle",
  "traderId": "RAGMAN",
  "loyaltyLevel": 1,
  "profileLevel": 1,
  "standing": 0,
  "currencyId": "ROUBLES",
  "price": 150,
  "achievementRequirements": [],
  "questRequirements": []
}
```
The README's default example keys the suit to `"traderId": "RAGMAN"` with `"questRequirements": []`, confirming that `traderId` + `questRequirements` is the documented gating field combination. The fields map directly onto SPT's trader-suit offer: `traderId` associates the suit to a trader (accepts a trader name like `RAGMAN` or a trader MongoID), and `loyaltyLevel`/`profileLevel`/`standing`/`currencyId`/`price`/`achievementRequirements`/`questRequirements` populate the suit's requirements block.

For a real-format ID reference (current README uses literal "PUT A UNIQUE MONGOID HERE" placeholders), the earlier WTT-ServerCommonLib 1.0.0/2.0.0 READMEs on NuGet show concrete sample values: `suiteId "6748037e298128d377dfffd0"`, `outfitId "67480381bd1eb568c78598df"`, `topId "67480383b253d50226f3becd"`, `handsId "67480396eda19f232a648533"` — all standard 24-character MongoIDs.

This matches the WTT-Artem reference, where each clothing entry carries `type`, `suiteId`, `outfitId`, `topId`, `handsId`, and `locales` (e.g. the Notch Lapel Suit Black entry: `"suiteId": "6753b531d39e76c118c7456a"`).

### 4. How the SPT 4.0.13 suits/crash mechanism works
- On SPT 4.0.13's C# server, the client polls `/client/trading/customization/<traderId>/usec/offers` for every trader. `CustomizationController.GetTraderSuits(traderId)` reads that trader's suits collection (associated by traderId).
- When a trader has no suits data, the lookup fails and the controller logs the "Unable to get suits from trader" error in a repeating loop. Registering at least one suit to Mae populates that collection and ends the loop. (In the legacy TypeScript server this same code path threw `TypeError: suits is not iterable` from `getTraderSuits`; the C# rewrite produces the equivalent "Unable to get suits from trader" message.)
- A suit's requirements include `questRequirements` (an array of quest IDs). When a quest ID is present, the suit becomes unlockable only after that quest is completed — the same mechanism vanilla uses for Ragman's quest-locked clothing.

### 5. Reusing Artem's suiteIds vs. redefining
Because `CreateCustomClothing` registers BOTH the suite definition and the trader offer from one JSON, the robust approach for Mae is to define your OWN clothing entries (your own unique suiteId/outfitId/topId/handsId MongoIDs) that point at the same Artem bundle paths, with `traderId` set to Mae. Re-using Artem's exact suiteIds (`6753b531d39e76c118c7456a`, `674db5974b5effbbf51fd756`) risks a duplicate-key collision if Artem has already registered them in the same database. If your only goal is the visually identical Notch Lapel suit sold by Mae, redefining with fresh IDs and the same bundle paths is the safest.

## Details

### Minimal crash fix (one entry)
Create `db/CustomClothing/MaeClothing.json` in your mod with a single valid clothing entry whose `traderId` is Mae (`6a1ac8598933e3f023895bd3`). That alone gives `GetTraderSuits("6a1ac8598933e3f023895bd3")` a non-empty result and ends the error loop, and the service auto-adds the services tab to Mae.

### Full working example (crash fix + quest-locked Notch Lapel suit)
`db/CustomClothing/MaeNotchLapel.json` — two entries (top and pants). Multiple suits can be placed as a JSON array, or one object per file:
```json
[
  {
    "type": "top",
    "suiteId": "<NEW_UNIQUE_MONGOID_1>",
    "outfitId": "<NEW_UNIQUE_MONGOID_2>",
    "topId": "<NEW_UNIQUE_MONGOID_3>",
    "handsId": "<NEW_UNIQUE_MONGOID_4>",
    "locales": { "en": { "name": "Notch Lapel Suit (Black)", "description": "Notch Lapel Suit" } },
    "topBundlePath": "<artem_top_bundle_path>",
    "handsBundlePath": "<artem_hands_bundle_path>",
    "traderId": "6a1ac8598933e3f023895bd3",
    "loyaltyLevel": 1,
    "profileLevel": 1,
    "standing": 0,
    "currencyId": "ROUBLES",
    "price": 50000,
    "achievementRequirements": [],
    "questRequirements": ["217e6ba5ff7f1752ff218687"]
  },
  {
    "type": "bottom",
    "suiteId": "<NEW_UNIQUE_MONGOID_5>",
    "outfitId": "<NEW_UNIQUE_MONGOID_6>",
    "bottomId": "<NEW_UNIQUE_MONGOID_7>",
    "locales": { "en": { "name": "Notch Lapel Suit Pants", "description": "Notch Lapel Suit Pants" } },
    "bottomBundlePath": "<artem_pants_bundle_path>",
    "traderId": "6a1ac8598933e3f023895bd3",
    "loyaltyLevel": 1,
    "profileLevel": 1,
    "standing": 0,
    "currencyId": "ROUBLES",
    "price": 30000,
    "achievementRequirements": [],
    "questRequirements": ["217e6ba5ff7f1752ff218687"]
  }
]
```
The exact field names for the "bottom"/pants variant (e.g. `bottomId`/`feetId`/`bottomBundlePath`) MUST be verified against Artem's actual `db/CustomClothing/Artem Clothes.json` (or the pants entry with suiteId `674db5974b5effbbf51fd756`), because the README documents only the "top" variant in full and the bottom field names above are inferred. Note Artem stores its clothing data under the mod's `/Resources` directory, so open the installed copy rather than expecting it in raw GitHub search.

### Registration / load order
```csharp
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class MaeMod(WTTServerCommonLib.WTTServerCommonLib wttCommon) : IOnLoad
{
    public async Task OnLoad()
    {
        var assembly = Assembly.GetExecutingAssembly();
        await wttCommon.CustomLocaleService.CreateCustomLocales(assembly);
        await wttCommon.CustomClothingService.CreateCustomClothing(assembly);
        await Task.CompletedTask;
    }
}
```
`TypePriority = OnLoadOrder.PostDBModLoader + 2` loads after the database. Clothing must be registered after the trader exists. If Mae is registered by a separate trader mod, ensure that mod loads first (or register Mae in the same mod before the clothing call). Add the dependency `{ "com.wtt.commonlib", new Range("~2.0.0") }` in your `ModMetadata.ModDependencies`.

## Recommendations
1. **Stop the crash first**: drop one valid clothing JSON keyed to Mae's MongoID into `db/CustomClothing/`. Restart the server and confirm the "Unable to get suits from trader" loop disappears and a Services tab appears on Mae.
2. **Add the quest-locked suit**: define the Notch Lapel top and pants with fresh unique MongoIDs, Artem's bundle paths, `traderId` = `6a1ac8598933e3f023895bd3`, and `questRequirements: ["217e6ba5ff7f1752ff218687"]`. Verify the suit appears greyed/locked until the quest completes, then becomes purchasable.
3. **Verify the bottom schema** by opening Artem's installed `Artem Clothes.json` (Resources folder) for the pants entry (suiteId `674db5974b5effbbf51fd756`) and copying its exact bottom field names and bundle paths.
4. **If reuse is required**: only reuse Artem's exact suiteIds if you confirm WTT-CommonLib de-duplicates rather than throwing on a duplicate suiteId; otherwise use new IDs pointing at the same bundles.

**Thresholds that change the plan**: if the crash persists after adding a valid suit, the cause is most likely (a) the `traderId` not exactly matching Mae's registered ID, or (b) clothing registering before the trader exists — fix the load order. If the suit is visible but never locks, the `questRequirements` quest ID is wrong or the quest itself isn't registered in the database.

## Caveats
- The exact verbatim text and localisation key of the "Unable to get suits from trader" error, and the exact C# field names/types of the SPT 4.0.13 `Suit` model, could not be confirmed from the server-csharp source in this research (the repo's `CustomizationController.cs` and `Suit.cs` were not directly retrievable). The mechanism — per-trader suits collection read by `GetTraderSuits(traderId)`, failing when empty, with a `Requirements` object containing `questRequirements` — is confirmed via the legacy server behavior and WTT-CommonLib's schema, but treat the precise model field list (`isActive`, `isHiddenInPVE`, `externalObtain`, `internalObtain`, `requiredTid`, etc.) as indicative rather than verbatim.
- The README's documented clothing JSON fully covers only the "top" type; the "bottom"/pants field names are inferred and should be verified against Artem's file.
- `currencyId` accepts `ROUBLES`; `traderId` accepts trader names (e.g. `RAGMAN`) or MongoIDs — using Mae's MongoID directly is recommended since she is a custom trader without a friendly-name alias.
- WTT-CommonLib also notes that custom JSON for these services "must match BSG's models exactly" for some services; clothing entries that fail validation are skipped, so malformed JSON can silently leave Mae with no suits and reproduce the crash.