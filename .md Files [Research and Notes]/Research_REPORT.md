# Root Cause: SPT 4.0.13 Bot Generation Crash on Custom "fsofixerspecialist" Bot (Null `LastNames` → LINQ `ArgumentNullException`)

## TL;DR

- **The null collection is `BotType.LastNames`** (the JSON `lastName` array). The crash is thrown at one specific line in `BotNameService.GenerateUniqueBotNickname` in the SPT 4.0.13 C# server: `botJsonTemplate.LastNames.Any()` is called with no null guard, and calling the LINQ `.Any()` extension on a null `IEnumerable<string>` throws `System.ArgumentNullException` with `paramName = "source"` — exactly the server-side error you see.
- **The `mustHaveUniqueName: true` flag is NOT the trigger; the missing `lastName` field is.** The `.Any()` call runs for every non-PMC bot regardless of uniqueness; removing `lastName` from the FSO type JSONs (copied from BEAR but with the field deleted) leaves `LastNames` null, so every FSO bot fails name generation. The client-side `NullReferenceException` at `BotsPresets.CreateProfile` is the downstream symptom: the server fails to produce a valid bot profile, so the Fika host's client gets a null/incomplete profile object.
- **The fix:** give each FSO bot type a non-null `lastName` array. The simplest, dependency-free, guaranteed fix is to add `"lastName": ["...", "..."]` (any non-empty list of strings) directly into every FSO bot type JSON — copy BEAR's `lastName` array back in. UNTAR/RUAF avoid the crash because their custom types end up with a non-null last-name collection, not because of any uniqueness setting.

## Key Findings

### 1. The exact faulting code (verified from source)

In `Libraries/SPTarkov.Server.Core/Services/BotNameService.cs` (sp-tarkov/server-csharp, commit `c87cc3c6`, the indexed 4.0.x snapshot), `GenerateUniqueBotNickname` builds non-PMC bot names with this expression:

```csharp
var name = isPmc // Explicit handling of PMCs, all other bots will get "first_name last_name"
    ? botHelper.GetPmcNicknameOfMaxLength(BotConfig.BotNameLengthLimit, botGenerationDetails.Side)
    : $"{randomUtil.GetArrayValue(botJsonTemplate.FirstNames)} {(botJsonTemplate.LastNames.Any() ? randomUtil.GetArrayValue(botJsonTemplate.LastNames) : "")}";
```

Your FSO bots are custom WildSpawnTypes spawned through the boss spawn system, so `isPmc` is false and they take the `else` branch. `botJsonTemplate.LastNames.Any()` calls `System.Linq.Enumerable.Any<TSource>(IEnumerable<TSource> source)`. When `LastNames` is null, LINQ's argument validator (`ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source)`) throws `ArgumentNullException` with the parameter name `source` — precisely matching the log line **"Value cannot be null. (Parameter 'source')"**. The author intended `.Any()` as an empty-check, but `.Any()` does not tolerate a *null* source — only an *empty* one. This is the single unchecked LINQ call that produces your error.

### 2. Why the field is null (verified from source)

`Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotType.cs` maps the JSON like this:

```csharp
[JsonPropertyName("firstName")]
public List<string> FirstNames { get; set; }

[JsonPropertyName("lastName")]
public IEnumerable<string> LastNames { get; set; }
```

Note the JSON keys are **singular** — `firstName` and `lastName` — even though the C# properties are plural. `LastNames` has no default initializer. Since `lastName` was deleted from the FSO type files, the deserializer leaves the property as its default value, `null`. There is no post-load normalization that converts a missing `lastName` to an empty list. So the moment a FSO bot reaches `GenerateUniqueBotNickname`, the unchecked `.Any()` throws.

### 3. Why `mustHaveUniqueName: true` is a red herring

The uniqueness logic (`roleShouldBeUnique` / the `UsedNameCache` check) runs *after* the `name` string is already constructed. The `.Any()` call that crashes is in the name-construction step, which executes for every non-PMC bot on every attempt, whether or not the role is in `BotRolesThatMustHaveUniqueName`. Setting `mustHaveUniqueName: false` will **not** prevent the crash, because the null dereference happens before the uniqueness branch is reached. It would only matter if name generation got far enough to need the cache — with a null `LastNames` it never does.

### 4. Why UNTAR and RUAF work in the same raid

UNTAR ("TacticalToasterUNTARGH") and RUAF ("RUAFComeHome") use the same MoreBotsAPI/MoreBotsServer pipeline (per the MoreBotsAPI README, UNTAR Go Home uses enum range 1170–1179 and RUAF Come Home uses 848400–848419) and both spawn via the boss spawn system — the README explicitly notes that spawning is not provided by the API and modders must "either implement that using the boss spawn system (UNTAR Go Home uses this method) or create your own system." Despite also setting `mustHaveUniqueName: true`, they generate fine because their custom bot types end up with a **non-null `lastName` collection**. Your FSO mod deleted `lastName` entirely (modeled on Knight/`bossKnight`, which omits last names), which is what leaves the collection null.

Important nuance about the Knight comparison: vanilla follower/boss types that "omit" last names in practice either still deserialize to a non-null value in the shipped database or are PMC-flagged so they never hit the non-PMC branch. Copying that "omit lastNames" pattern by literally deleting the key from a *non-PMC, boss-spawned* custom type is what produces the null. (I could not retrieve raw `bossknight.json` to confirm whether it ships `"lastName": []` versus omitting the key; either way, the safe path for a custom type is to provide a non-empty array.)

### 5. The client-side NullReferenceException is a downstream effect

`BotsPresets.CreateProfile` (EFT client) consumes the profile the server generates for each bot. When the server throws inside `GenerateUniqueBotNickname`, that bot's generation fails and the server returns an error/empty result for bot #N. On the Fika host, `BossSpawnerClass.Spawn → BotCreationDataClass.Create → BotCreatorClass.GenerateProfile → BotsPresets.CreateProfile` then dereferences the missing/incomplete bot data, producing the `NullReferenceException` at offset 0x00232 and the "Rethrow as AggregateException". The client crash is a symptom; the server-side null `LastNames` is the cause.

## Details

### Where names fit in the generation pipeline

The C# `BotGenerator` calls `botNameService.GenerateUniqueBotNickname(botJsonTemplate, botGenerationDetails, botConfig.BotRolesThatMustHaveUniqueName)` and assigns the result to `bot.Info.Nickname`, then sets `bot.Info.LowerNickname`. If the name call throws, the whole bot record is abandoned and "Failed to generate bot #N (fsofixerspecialist)" is logged with the inner `ArgumentNullException`. (For reference, an unrelated quirk in the same class: `GetRandomPmcName()` pulls only `FirstNames` from `usec`/`bear`, so PMC nickname generation never touches per-type `lastName` — which is why only your non-PMC custom types are affected.)

### The correct, idiomatic fix (ranked)

**Fix A — Add `lastName` arrays directly to each FSO bot type JSON (recommended: simplest, verified, dependency-free).**
In every file under your mod's `db/bots/types/*.json`, restore a non-empty `lastName` array. Because the data was copied from `bear.json`, the cleanest fix is to paste BEAR's original `lastName` array back into each FSO type file:

```jsonc
{
  "firstName": ["Fixer", "Specialist", "..."],
  "lastName": ["Ivanov", "Petrov", "..."],   // <-- must be present and non-empty
  "appearance": { ... },
  ...
}
```

Any non-empty list of strings works; the names are cosmetic (they become the bot nickname). This removes the null and the crash for all difficulties at once. Note the **singular** key name `lastName` (matching `[JsonPropertyName("lastName")]`) — a typo like `lastNames` will be ignored by the deserializer and leave the property null.

**Fix B — Supply shared last names via MoreBotsServer (the UNTAR-style centralization).**
If you prefer not to duplicate name lists across files, you can centralize them. The **documented** MoreBotsServer loader is `LoadBots(Assembly.GetExecutingAssembly())`, called from an `IOnLoad` class with `TypePriority = OnLoadOrder.PostDBModLoader + 2`, injecting `MoreBotsServer.MoreBotsAPI`; it reads `db/bots/types` and `db/bots/config`. If your MoreBotsServer version exposes a `LoadBotTypeReplace(assembly, "lastnames", typeList)`-style helper that reads `db/bots/sharedTypes/lastnames.json` and merges it into each listed type's `lastName`, use that: create `db/bots/sharedTypes/lastnames.json` as a JSON array of name strings and call the replace method for your FSO type list. **Caveat:** the public MoreBotsAPI README (latest release 1.1.0, Nov 13 2025) documents only `LoadBots(assembly)` reading `db/bots/types` and `db/bots/config` — it does not document any `sharedTypes`/shared-names loader. So the exact `LoadBotTypeReplace` method name and the `lastnames.json` shape are **unverified** from public docs; confirm them against the MoreBotsServer DLL or UNTAR's source before relying on this path. Fix A does not depend on it and is the safe default.

**Fix C — Do NOT rely on `mustHaveUniqueName: false`.** As shown in Key Finding 3, it does not prevent the null dereference. Set it to whatever you want for gameplay; it is not the fix.

### Secondary null-collection risks to verify while you're in the files

Because the FSO types are hand-edited copies of BEAR, confirm no other required collection was deleted or emptied. Verify each of the following is present and non-null in every FSO type JSON:

- **`firstName`** — `randomUtil.GetArrayValue(FirstNames)` is called *unconditionally* on the non-PMC branch (no `.Any()` guard at all). If `firstName` was also removed, it would fault immediately — this is the single most likely next crash after you fix `lastName`. Make sure it is present and non-empty.
- **`appearance.body / head / feet / hands / voice`** — weighted pools consumed via `WeightedRandomHelper.GetWeightedValue`. `appearance.voice` is dereferenced directly (`bot.Info.Voice = getWeightedValue(botJsonTemplate.appearance.voice)`); a null/empty voice pool will throw. Keep BEAR's values. (In `BotType.cs` these are `Dictionary<MongoId, double>` maps; a deleted key → null map.)
- **`inventory.equipment` (per-slot pools), `inventory.Ammo`, `inventory.items`, `inventory.mods`** — consumed by `BotInventoryGenerator` / `BotWeaponGenerator` / `BotEquipmentModGenerator`. A missing `Ammo` caliber map or `equipment` slot dictionary is a common source of later null/`source` errors during weapon/mod generation. Keep the full BEAR structures.
- **`chances.equipment / weaponMods / equipmentMods`** — dictionaries read during equipment rolls; keep them.
- **`generation.items.*`** (grenades, healing, drugs, food, drink, currency, stims, backpackLoot, pocketLoot, vestLoot, magazines, specialItems, looseLoot) — each has `weights` and `whitelist`; `BotLootGenerator` reads these. Keep the full BEAR `generation` block.
- **`health.BodyParts`, `experience` (level/reward/standingForKill), `skills.Common`, `difficulty`** — keep BEAR's; missing `difficulty` tiers or `experience.level` can break level/health generation.

The practical rule: a custom PMC-style type should be a *complete* copy of `bear.json`/`usec.json` with only the AI/identity fields changed — **do not delete whole keys.** The single deletion of `lastName` is what broke FSO; auditing for any other deleted key prevents the next crash.

## Recommendations

1. **Immediate fix:** Add a non-empty `"lastName": [ ... ]` array (singular key) to every FSO bot type JSON in `db/bots/types/` — copy BEAR's `lastName` list. Restart the server and test a raid. This resolves both the server-side `ArgumentNullException ('source')` and the downstream client `NullReferenceException` at `BotsPresets.CreateProfile`.
2. **Confirm `firstName` is also present and non-empty** in each FSO type — it is dereferenced unconditionally and is the most likely next failure if it too was removed.
3. **Audit each FSO type against the BEAR template for any other deleted keys**, focusing on `appearance.voice`, `inventory.Ammo/equipment`, `generation.items`, and `chances`. Re-add anything missing.
4. **If you want centralized names**, migrate to the MoreBotsServer shared-names mechanism (Fix B) only after confirming the exact method name and `sharedTypes/lastnames.json` format from MoreBotsServer source or UNTAR's repo. Until confirmed, keep the inline `lastName` arrays.
5. **Do not toggle `mustHaveUniqueName` as a fix** — it does not affect this code path.
6. **Optional upstream report:** the unchecked `.Any()` on a nullable `IEnumerable` in `BotNameService.GenerateUniqueBotNickname` is a latent server bug. A `LastNames?.Any() ?? false` guard, or initializing `LastNames` to an empty list on deserialize, would make the server resilient to mods that omit `lastName`. Consider filing an issue on sp-tarkov/server-csharp.

**Benchmark that changes the recommendation:** If, after adding `lastName`, you still get a `Parameter 'source'` error for FSO bots, the null has moved to another deleted collection (most likely `firstName`, `appearance.voice`, or an `inventory` sub-map) — diff your FSO type JSON against `bear.json` key-by-key to find the remaining missing key.

## Caveats

- The faulting line and the `BotType` JSON mapping are quoted **directly** from sp-tarkov/server-csharp at commit `c87cc3c6` (the indexed 4.0.x snapshot) — I retrieved both `BotNameService.cs` and `BotType.cs` verbatim. The exact source-line offset can vary slightly between 4.0.x patch builds, but the `LastNames.Any()` pattern is the operative bug across the 4.0 line.
- I could **not** retrieve raw `bossknight.json`/`bear.json` JSON to verify whether vanilla omits `lastName` or ships it as `[]`. The recommendation (provide a non-empty array in your custom type) is safe regardless of the vanilla file's exact form.
- The MoreBotsServer shared-names API (`LoadBotTypeReplace` / `db/bots/sharedTypes/lastnames.json`) is **unverified** — public MoreBotsAPI docs only describe `LoadBots(assembly)` reading `db/bots/types` and `db/bots/config`. Treat Fix B's specifics as provisional; Fix A is the verified, dependency-free solution.
- The client-side mapping (server name failure → null/empty profile → `BotsPresets.CreateProfile` NRE at 0x00232) is inferred from your stack trace plus the confirmed server behavior. It is the consistent explanation, but the EFT client assembly is closed-source, so the precise field dereferenced at that offset cannot be named from public sources.
