# Building a Five-Quest Custom Trader Chain in SPT 4.0.13 with WTT-CommonLib

## TL;DR

- **It is fully achievable in SPT 4.0.13.** Drop a `db/CustomQuests/6a1ac8598933e3f023895bd3/` folder (with `Quests/`, `QuestAssort/`, `Locales/`, `Images/` subfolders) into your bundled C# mod, call `await wttCommon.CustomQuestService.CreateCustomQuests(assembly)` from an `IOnLoad` running at `OnLoadOrder.PostDBModLoader + 2`, and the library registers your quests, locales, images, and quest-gated assorts automatically.
- **Quest chaining, kill/find/handover objectives, and all reward types you need exist natively**: chaining via a `Quest`-type `AvailableForStart` condition (`status:[4]` = previous quest Success), kills via `CounterCreator`, item turn-in via `HandoverItem`/`FindItem`, and rewards for XP (`Experience`), trader rep (`TraderStanding`), items (`Item`, including backported weapons by template id), and trader-shop unlocks (`AssortmentUnlock`).
- **The two hard requirements**: (1) per the WTT-CommonLib README, "Quest .json files MUST MATCH BSG QUEST MODELS exactly. Invalid quest data will throw errors and prevent loading"; (2) every quest `_id` and every condition `id` needs matching locale keys (`<id> name`, `<id> description`, plus a key per objective) so Mae's dialogue renders instead of raw IDs.

## Key Findings

1. **Loader confirmed.** WTT-ServerCommonLib (latest published version on NuGet is 2.0.20: `<PackageReference Include="WTT-ServerCommonLib" Version="2.0.20" />`; paired with the WTT-ClientCommonLib package) exposes `CustomQuestService.CreateCustomQuests(assembly)` and `CreateCustomQuests(assembly, path)`. It discovers quests by walking a trader-keyed folder tree and is the supported path for SPT 4.0.13 custom quests.
2. **WTT-ContentBackport is real, current, and WTT-CommonLib-dependent.** The Forge lists "WTT - Content Backport" 1.0.7 marked **SPT 4.0.13 Compatible** ("Download Latest Version (1.0.7) 3.25 GB ... Updated May 14, 9:34 AM ... Created by GrooveypenguinX · 269.5K Downloads"), GUID `com.wtt.contentbackport`, source at github.com/WelcomeToTarkov/Tarkov-1.0-Backport ("A port of the items added in Tarkov 1.0 to SPT 4.0"). The Forge page lists its dependency as "WTT - CommonLib · Requires v2.0.20." **Source-vs-Forge divergence to note:** the GitHub `WTTContentBackport.cs` `ModMetadata` declares `ModDependencies { "com.wtt.commonlib", new Range("~2.0.16") }` and `SptVersion new("~4.0.1")`, and lists `Author = "GrooveypenguinX"` with `Contributors = null` (the EpicRangeTime/Tron credits appear on the Forge listing, not in the code metadata). The repo ships separate sub-projects including `WTT-ContentBackport-Mainmenu` (the "Peaceful Sky" main-menu background swap) and the AS VAL MOD.4; the 1.0.7 version notes state "val 4 sounds have been updated to their new sounds."
3. **SPT 4.0 uses the flattened `conditionType` condition format**, not the legacy `_parent`/`_props` wrapper, though the server still accepts the older wrapped form.
4. **AssortmentUnlock is the mechanism** for "trader sells item X only after quest Y." It carries `traderId`, `loyaltyLevel`, and an `items[]` template, and is paired with `QuestAssort/*.json` gating.

## Details

### 1. Mod scaffold and load call

```csharp
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.yourname.maetrader";
    public override string Name { get; init; } = "Mae Trader Quests";
    public override string Author { get; init; } = "You";
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override Range SptVersion { get; init; } = new("~4.0.13");
    public override string License { get; init; } = "MIT";
    public override bool? IsBundleMod { get; init; } = true;
    public override Dictionary<string, Range>? ModDependencies { get; init; } = new()
        { { "com.wtt.commonlib", new Range("~2.0.0") } };
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class MaeMod(WTTServerCommonLib.WTTServerCommonLib wttCommon) : IOnLoad
{
    public async Task OnLoad()
    {
        var assembly = Assembly.GetExecutingAssembly();
        await wttCommon.CustomQuestService.CreateCustomQuests(assembly);
        await Task.CompletedTask;
    }
}
```

The WTT-CommonLib README confirms this exact pattern verbatim — `[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]`, `await wttCommon.CustomQuestService.CreateCustomQuests(assembly);`, with the guidance "Set TypePriority = OnLoadOrder.PostDBModLoader + 2 to load after the database." This ensures the database (and your trader) exist before quests load.

### 2. Folder structure (exact names and casing)

``
db/CustomQuests/
├── QuestTimeData.json          # optional, seasonal windows
├── QuestSideData.json          # optional, BEAR/USEC-only quests
└── 6a1ac8598933e3f023895bd3/   # YOUR trader's MongoID (a trader name also works)
    ├── Quests/
    │   └── mae_quests.json      # array of quest objects
    ├── QuestAssort/
    │   └── assort.json          # quest→assort gating map
    ├── Locales/
    │   ├── en.json
    │   └── ru.json
    └── Images/
        └── mae_q1.png           # quest icons, 314x177 .png/.jpg
``

The folder may be named either the trader's MongoID (`6a1ac8598933e3f023895bd3`) or a known trader alias. The casing of the four subfolders (`Quests`, `QuestAssort`, `Locales`, `Images`) is as shown. Per the README, "QuestTimeData.json and QuestSideData.json are optional and can be placed in the root CustomQuests/ folder," and "Images must be in standard formats (.png, .jpg, etc)."

**Hard rules straight from the WTT-CommonLib README:**

- "Quest .json files MUST MATCH BSG QUEST MODELS exactly. Invalid quest data will throw errors and prevent loading."
- "If a quest is outside its time window, it will not be loaded into the database."
- "Locales fall back to English if a translation is missing for a specific language."

### 3. The quest object schema

Top-level fields of a single quest object (matching `SPTarkov.Server.Core.Models.Eft.Common.Tables.Quest`):

```json
{
  "_id": "mae_q1_arrival",
  "QuestName": "Arrival",
  "trader": "6a1ac8598933e3f023895bd3",
  "traderId": "6a1ac8598933e3f023895bd3",
  "location": "any",
  "image": "/files/quest/icon/mae_q1.png",
  "type": "Completion",
  "isKey": false,
  "restartable": false,
  "instantComplete": false,
  "secretQuest": false,
  "canShowNotificationsInGame": true,
  "side": "Pmc",
  "QuestType": "Completion",
  "conditions": {
    "AvailableForStart": [],
    "AvailableForFinish": [],
    "Fail": []
  },
  "rewards": {
    "Started": [],
    "Success": [],
    "Fail": []
  }
}
```

- `_id` is the quest's MongoID; reuse it across the locale keys.
- `trader`/`traderId` both point at your trader's MongoID `6a1ac8598933e3f023895bd3`.
- `location` accepts `any` or a map id (`bigmap`=Customs, `factory4_day`, `factory4_night`, `interchange`, `laboratory`, `lighthouse`, `rezervbase`, `shoreline`, `woods`, `tarkovstreets`, `sandbox`/`sandbox_high`=Ground Zero).
- `type`/`QuestType` are decorative bucket labels: `Completion`, `PickUp`, `Elimination`, `Loyalty`, `Discover`.
- `side` is `Pmc` (or `Savage`).

### 4. Conditions (objectives)

SPT 4.0 stores conditions in a **flattened format** with a `conditionType` string field. (The legacy `{"_parent":"...","_props":{...}}` wrapper is still accepted; the flattened form is recommended for new quests.)

**Quest chaining — `AvailableForStart`, `Quest` condition** (this is how quest 2 unlocks after quest 1):

```json
{
  "conditionType": "Quest",
  "id": "mae_q2_req_q1",
  "index": 0,
  "parentId": "",
  "dynamicLocale": false,
  "globalQuestCounterId": "",
  "availableAfter": 0,
  "dispersion": 0,
  "visibilityConditions": [],
  "target": "mae_q1_arrival",
  "status": [4]
}
```

- `target` = the previous quest's `_id`.
- `status` is an array of QuestStatus integers: **0=Locked, 1=AvailableForStart, 2=Started, 3=AvailableForFinish, 4=Success, 5=Fail, 6=FailRestartable, 7=MarkedAsFailed, 8=Expired, 9=AvailableAfter.** Use `[4]` for "previous quest completed."
- `availableAfter` adds a delay (seconds) after the target reaches that status.

**Player level gate — `Level`:**

```json
{ "conditionType": "Level", "id": "mae_q1_lvl", "index": 0, "parentId": "",
  "dynamicLocale": false, "compareMethod": ">=", "value": 5, "globalQuestCounterId": "" }
```

**Trader loyalty gate — `TraderLoyalty`:**

```json
{ "conditionType": "TraderLoyalty", "id": "mae_q3_loy", "index": 0, "parentId": "",
  "dynamicLocale": false, "compareMethod": ">=", "value": 2,
  "target": "6a1ac8598933e3f023895bd3" }
```

**Kill objective — `CounterCreator`** (nested counter with `Kills` + optional `Location`):

```json
{
  "conditionType": "CounterCreator",
  "id": "mae_q2_kill_obj",
  "index": 0,
  "parentId": "",
  "value": 15,
  "type": "Elimination",
  "dynamicLocale": false,
  "oneSessionOnly": false,
  "doNotResetIfCounterCompleted": false,
  "counter": {
    "id": "mae_q2_counter",
    "conditions": [
      {
        "conditionType": "Kills",
        "id": "mae_q2_kills",
        "target": "Savage",
        "compareMethod": ">=",
        "value": 1,
        "weapon": [],
        "bodyPart": [],
        "distance": { "compareMethod": ">=", "value": 0 },
        "savageRole": [],
        "daytime": { "from": 0, "to": 0 }
      },
      {
        "conditionType": "Location",
        "id": "mae_q2_loc",
        "target": ["bigmap", "tarkovstreets"]
      }
    ]
  }
}
```

- `value` (on the CounterCreator) = number of kills required.
- `Kills.target`: `Savage` (Scav), `AnyPmc`, `Usec`, `Bear`, `Any`, plus boss/follower roles via `savageRole`.
- Filter by `weapon` (array of weapon template ids), `bodyPart` (e.g. `["Head"]`), `distance` (compareMethod + value in meters), `daytime`, and map via a nested `Location` condition. This `CounterCreator` + nested `Kills`/`Location` structure is corroborated by real vanilla quest dumps (e.g. Qazwar/Enut `quest_list.json`) and the QuestsExtended modding guide.

**Item turn-in — `HandoverItem`** (in `AvailableForFinish`):

```json
{
  "conditionType": "HandoverItem",
  "id": "mae_q1_handover",
  "index": 1,
  "parentId": "",
  "dynamicLocale": false,
  "target": ["590c645c86f77412b01304d9"],
  "value": "2",
  "onlyFoundInRaid": true,
  "dogtagLevel": 0,
  "maxDurability": 100,
  "minDurability": 0,
  "visibilityConditions": []
}
```

- `target` = array of accepted item template ids.
- `value` = count; `onlyFoundInRaid` = FiR requirement; `dogtagLevel` for dogtag turn-ins; `min/maxDurability` for weapon/armor condition filters.

**`FindItem`** is structurally identical to `HandoverItem` (just "find in raid" rather than "hand to trader") and is usually paired with a HandoverItem.

**Place item at a map location — `LeaveItemAtLocation` / `PlaceBeacon`:**

```json
{
  "conditionType": "LeaveItemAtLocation",
  "id": "mae_q4_place",
  "index": 0,
  "parentId": "",
  "zoneId": "mae_zone_streets_1",
  "target": ["5a29357286f77409c705e025"],
  "value": "1",
  "plantTime": 5,
  "onlyFoundInRaid": false
}
```

- `zoneId` references a quest zone. Custom zones are created with WTT's `CustomQuestZoneService` (`db/CustomQuestZones/`, with fields `ZoneId`, `ZoneName`, `ZoneLocation`, `ZoneType` ("placeitem"), `Position`/`Rotation`/`Scale`), positioned visually with the in-game F12 editor (WTT-ClientCommonLib). On Streets of Tarkov set `ZoneLocation` to `tarkovstreets`; on Labs use `laboratory`.

**Visit a place — `VisitPlace`/`Exploration`**, **`Skill`** (reach a skill level), and **`WeaponAssembly`** (build a weapon to spec) follow the same flattened pattern with `conditionType`, `id`, `value`, and type-specific fields.

### 5. Rewards

`rewards` has three arrays — `Started`, `Success`, `Fail`. Each reward shares base fields `id`, `index`, `type`, `value`, optional `target`, and type-specific extras. The `type` string values (from the C# `RewardType` enum) are: `Experience`, `Skill`, `TraderStanding`, `TraderUnlock`, `Item`, `AssortmentUnlock`, `Achievement`, `StashRows`, `ProductionScheme`, `Pockets`, `CustomizationDirect`, `NotificationPopup`, `ExtraDailyQuest`.

**Experience:**

```json
{ "id": "mae_q1_xp", "index": 0, "type": "Experience", "value": "3300", "target": "" }
```

**TraderStanding** (`target` = trader MongoID, `value` = rep delta, may be negative):

```json
{ "id": "mae_q1_rep", "index": 1, "type": "TraderStanding",
  "target": "6a1ac8598933e3f023895bd3", "value": "0.05" }
```

**Item** (give an item or assembled preset; `target` must match `items[0]._id`):

```json
{
  "id": "mae_q5_itemreward",
  "index": 0,
  "type": "Item",
  "target": "mae_q5_root",
  "value": "1",
  "findInRaid": true,
  "items": [
    { "_id": "mae_q5_root", "_tpl": "<AS_VAL_MOD4_TPL>",
      "upd": { "StackObjectsCount": 1, "SpawnedInSession": true } }
  ]
}
```

To grant a **backported ending weapon**, set `_tpl` to that item's template id from WTT-ContentBackport (the mod must be installed; you reference its template ids, you do not redefine the item). For a multi-part preset, list the child mods in `items[]` with `parentId`/`slotId` referencing the root.

**AssortmentUnlock** (unlock a trader-shop item after quest completion — used for the Artem suit after quest 1 and the watch after quest 5):

```json
{
  "id": "mae_q1_unlock_suit",
  "index": 2,
  "type": "AssortmentUnlock",
  "target": "mae_assort_artemsuit",
  "value": "1",
  "loyaltyLevel": 1,
  "traderId": "6a1ac8598933e3f023895bd3",
  "items": [
    { "_id": "mae_assort_artemsuit", "_tpl": "<ARTEM_SUIT_TPL>",
      "upd": { "UnlimitedCount": true, "StackObjectsCount": 1 } }
  ]
}
```

- `traderId` = your trader; `loyaltyLevel` = the level at which the unlocked entry becomes purchasable; `items[0]._id` = the assort entry id that gets enabled; `target` mirrors that id.

**TraderUnlock** (`target` = trader id, `value` "1"), **Skill** (`target` = skill name, `value` = points), **StashRows** (`value` = rows), and **ProductionScheme** (hideout recipe unlock, matched by `questId`/area) round out the set.

### 6. The quest→assort unlock mechanism in detail

Two systems cooperate:

- **The trader's own `assort.json`** holds `items[]` (each with `parentId:"hideout"`, `slotId:"hideout"`), `barter_scheme` (price/barter per item id), and `loyal_level_items` (item id → required loyalty level). This defines *what* the trader can sell and at what loyalty.
- **`QuestAssort/assort.json`** (WTT) gates specific assort item ids behind quest IDs. Its shape:

```json
{
  "started": {},
  "success": {
    "mae_assort_artemsuit": "mae_q1_arrival",
    "mae_assort_watch": "mae_q5_finale"
  },
  "fail": {}
}
```

The key is the **assort item id**, the value is the **quest id** whose completion unlocks it. The server's `AssortHelper.StripLockedQuestAssort` greys out (does not delete) quest-locked assort items until the gating quest reaches the right status; once the quest is `Success`, the item becomes buyable. (Confirmed by the C# server documentation: "The StripLockedQuestAssort method marks items that are locked behind quest completion without removing them (they appear greyed out in the client)," `AssortHelper.cs`. The companion `StripLockedLoyaltyAssort` removes items above the player's loyalty level.)

**Recommended pattern:** Put the suit/watch entries in your trader assort, list them in `QuestAssort/success` mapped to quest 1 and quest 5, AND add the matching `AssortmentUnlock` reward to those quests. Loyalty gating (`loyal_level_items`) and quest gating combine — the item must satisfy *both* the loyalty level and the quest unlock to appear for purchase.

### 7. Locale system

All quest text comes from locale key→string pairs merged into `locales/global/<lang>`. Each quest needs, at minimum:

```json
{
  "mae_q1_arrival name": "Arrival",
  "mae_q1_arrival description": "Mae's intro briefing text...",
  "mae_q1_arrival successMessage": "Good. You'll do.",
  "mae_q1_arrival startedMessageText": "Here's what I need.",
  "mae_q1_arrival successMessageText": "Take this — you've earned it.",
  "mae_q1_arrival failMessageText": "...",
  "mae_q1_handover": "Hand over 2 SSD drives",
  "mae_q2_kill_obj": "Eliminate 15 Scavs on Customs or Streets"
}
```

- The key format is `<questId> name`, `<questId> description`, and the message keys.
- **Critically, every objective condition's `id` is itself a locale key** mapping to the objective text shown to the player (e.g. the `id` `mae_q1_handover` above). This matches the long-standing VCQL/SPT rule: "Locales are key value pairs where the key is the quest or condition ID and the value is the string of text to be displayed to the user (e.g. `VCQ_1 name`: `Welcome To Tarkov`)." Miss these and the UI shows the raw GUID.
- The trader **mail** sent on completion uses these message templates; rewards are attached to that mail. The `successMessageText` is the body of the "quest complete" message that delivers the item rewards via the in-game messenger (handled by `MailSendService`), with `systemData` carrying the trader id.

### 8. WTT-ContentBackport content

"WTT - Content Backport" 1.0.7 (SPT 4.0.13 compatible, 3.25 GB, GUID `com.wtt.contentbackport`, 269.5K downloads) backports EFT 1.0 content into SPT 4.0 and depends on WTT-CommonLib (Forge: "Requires v2.0.20"; GitHub source declares `~2.0.16`). It is organized into `WTT-ContentBackport` (server), `WTT-ContentBackportClient`, `WTT-ContentBackportPatcher`, and `WTT-ContentBackport-Mainmenu` (the "Peaceful Sky" menu background). It adds the EFT 1.0 "Savior" ending weapons including the AS VAL MOD.4 ("val 4," with updated sounds in 1.0.7) and other premium weapons, plus streamer-case items integrated through WTT loot-container support. To grant these as quest rewards you reference their existing template ids in an `Item` reward `_tpl` (do not redefine them). The companion "Content Backport - Prestiges" mod (The Forge: v1.0.1, 21.23 MB, SPT 4.0.13, GUID `wtf.archangel.contentbackportprestiges`, dependencies "WTT - CommonLib · Requires v2.0.11" and "WTT - Content Backport · Requires v1.0.4," source github.com/ArchangelWTF/ContentBackportPrestiges) handles the prestige/achievement side. The exact MongoIDs for the AS VAL Mod.4, PKP, Staccato, Siege-R, and AA-12 must be read from the installed mod's item JSON (under its `db/CustomItems`).

### 9. Common pitfalls

- **Quest doesn't appear:** wrong/nonexistent `trader`/`traderId`, an impossible `AvailableForStart` (e.g. `Level` 99), missing locale `name` key, or a JSON schema mismatch that made `CreateCustomQuests` throw (check server log).
- **Objective never completes:** the objective `id` is duplicated, the `Location` filter excludes the map you're on, or `onlyFoundInRaid:true` while you used a non-FiR item.
- **Chain won't advance:** the next quest's `Quest` condition `target` doesn't exactly match the prior quest's `_id`, or `status` isn't `[4]`. (The SPT C# server documents a real upstream chain-break bug — Ref's "To Great Heights" 1–3 present but not part 4 after Prestige 2 — illustrating how a single broken link kills a chain.)
- **Reward item missing:** `target` doesn't match `items[0]._id`, or the `_tpl` references a backport item that isn't installed.
- **Assort item never unlocks:** the id in `QuestAssort/success` doesn't match the assort entry `_id`, or the loyalty gate is higher than the player has reached.
- **IDs:** keep custom string IDs unique; older SPT had a client bug where non-MongoID quest ids needed `#` wrapping (e.g. `#mae_q1#`) — using true 24-character MongoIDs avoids this and is recommended.

## Recommendations

1. **Build the trader first** (the supplied id `6a1ac8598933e3f023895bd3`), confirm it loads with a working (even empty) assort, then add quests. Threshold to proceed: trader appears in-game with no boot errors.
2. **Author quest 1 only**, with a `Level` start gate, a `HandoverItem` finish objective, and `Experience` + `TraderStanding` rewards. Verify it appears and completes before adding more. Threshold: quest text (not GUIDs) shows, and completion delivers the rep/XP.
3. **Add the chain incrementally**: quests 2–5 each get a `Quest`/`status:[4]` start condition pointing at the previous quest's `_id`. Add quest 2's `CounterCreator` kill objective and confirm progress ticks in-raid.
4. **Wire the two AssortmentUnlocks last** (Artem suit on quest 1, watch on quest 5), backed by `QuestAssort/success` entries plus matching `AssortmentUnlock` rewards, and confirm the items flip from greyed-out to buyable in-game after completion.
5. **Validate locale coverage** by searching your `en.json` for every quest `_id` and every condition `id` — each must have a string. This is the single most common cause of "it works but shows gibberish."
6. **Decision thresholds:** if the server log shows a quest deserialization error on boot, the schema is wrong — fix before anything else. If quests load but text shows GUIDs, the locale keys are the problem. If the chain stalls, inspect the profile's quest status values against the `status` arrays.

## Caveats

- SPT 4.0.13 uses the **flattened `conditionType`** format; the `_parent`/`_props` examples found in older (3.x / raw-EFT) quest dumps still deserialize but should be migrated. Both forms place the prerequisite quest id in `target` and use the same `status` integer semantics.
- The exact backport item template MongoIDs (AS VAL Mod.4, PKP, Staccato, Siege-R, AA-12) and the Artem suit / watch template ids are **not reproduced here** — read them from the installed mods' item JSON, as guessing them risks mismatches.
- Trader MongoIDs cited for *vanilla* traders in source material were internally inconsistent and should be verified against `SPT_Data/database/traders/<id>/base.json`; this does not affect your custom trader id, which you supplied.
- A real version divergence exists for WTT-ContentBackport: the Forge listing says it requires WTT-CommonLib v2.0.20 and is SPT 4.0.13 compatible, while the GitHub `ModMetadata` declares `~2.0.16` / `~4.0.1`. Trust the Forge build for runtime, but expect the code metadata to lag.
- Reward `type` enum strings and the QuestStatus integers are verified against the SPT C# server model references; the specific illustrative `_id` hex values in the JSON examples are representative placeholders, not literal vanilla data. Replace them with freshly generated 24-character MongoIDs.
