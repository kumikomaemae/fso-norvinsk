# Bot JSON Schema Notes

Reference notes captured during Phase 2a/2b research, May 23, 2026.
Sources: decompiled `BotType.cs` and `BotTypeInventory.cs` from `SPTarkov.Server.Core.dll` v4.0.13,
plus cross-validation against UNTARGH's `followeruntar.json` and RUAF's `ruaf_remnant.jsonc`.

---

## Top-level BotType structure

Every bot JSON has exactly these 10 keys at the root, in this order:

| JSON key | C# property | Type | Notes |
|---|---|---|---|
| `appearance` | `BotAppearance` | `Appearance` | small — weighted dicts of body/feet/hands/head/voice MongoIds |
| `chances` | `BotChances` | `Chances` | 3 sub-dicts: equipment, equipmentMods, weaponMods |
| `difficulty` | `BotDifficulty` | `Dictionary<string, DifficultyCategories>` | massive — keyed by "easy"/"normal"/"hard"/"impossible" |
| `experience` | `BotExperience` | `Experience` | tiny — level range, kill rewards |
| `firstName` *(singular!)* | `FirstNames` | `List<string>` | name pool |
| `generation` | `BotGeneration` | `Generation` | item-count distributions |
| `health` | `BotHealth` | `BotTypeHealth` | body part HP per difficulty |
| `inventory` | `BotInventory` | `BotTypeInventory` | THE BIG ONE — gear, ammo, items, mods |
| `lastName` *(singular!)* | `LastNames` | `IEnumerable<string>` | name pool |
| `skills` | `BotSkills` | `BotDbSkills` | Common skills + Mastering per-weapon |

⚠️ JSON keys are singular ("firstName"/"lastName") but the C# fields are plural —
the values are List<string>. BSG convention, preserved by SPT.

---

## BotTypeInventory — the four sub-keys

| JSON key | Type | Plain English |
|---|---|---|
| `equipment` | `Dictionary<EquipmentSlots, Dictionary<MongoId, double>>` | per-slot weighted item pools |
| `ammo` | `Dictionary<string, Dictionary<MongoId, double>>` | per-caliber weighted ammo pools |
| `items` | `ItemPools` | container loot (pockets/backpack/vest/secure) |
| `mods` | `Dictionary<MongoId, Dictionary<string, HashSet<MongoId>>>` | weapon attachment compatibility tree |

The `mods` tree is what makes bot JSONs huge (every weapon × every slot × every valid attachment).
We mostly leave it alone — UNTARGH already authored a valid Western/NATO mod tree.

---

## chances vs inventory — how they collaborate

The two structures answer different questions:

- **chances** answers "IF" (does a slot get filled? does a weapon have a scope?)
- **inventory** answers "WHAT" (which specific item fills the slot?)

| chances entry | inventory entry | combined behavior |
|---|---|---|
| `chances.equipment.FirstPrimaryWeapon: 100` | `inventory.equipment.FirstPrimaryWeapon: {...}` | 100% chance to spawn with primary; pick weighted from pool |
| `chances.weaponMods.mod_scope: 68` | `inventory.mods[weaponId].mod_scope: [...]` | 68% chance the weapon has a scope; if yes, pick from valid scope list |
| `chances.equipmentMods.front_plate: 100` | (handled in inventory.mods for armor) | 100% chance armor has a front plate insert |

---

## Difficulty section — the dragon we don't slay

The `difficulty` section is ~2000 lines (4 difficulties × ~14 sub-objects each: Aiming, Boss, 
Change, Core, Cover, Grenade, Hearing, Lay, Look, Mind, Move, Patrol, Scattering, Shoot).

**We mostly leave this alone because:**
1. UNTARGH's defaults are sensible PMC-tier values, already tested.
2. SAIN intercepts most behaviors at runtime via the SAINSettings we configured in the prepatcher.

Touch only if specific behaviors feel wrong in testing.

---

## Our edit targets for Phase 2c

The two real leverage points for FSO bot personality are:

1. **`inventory.equipment`** — which weapons/armor/helmets/etc. spawn, with what weights
2. **`inventory.ammo`** — caliber → ammo type weights (controls penetration tier)

Secondary (smaller but still meaningful):
- `appearance` — WTT-Artem suit MongoIds, hair, voice
- `firstName` / `lastName` — FSO callsigns
- `health` — body part HP scaling per tier
- `generation` — how much loot they carry
- `skills.Common` — health regen, endurance, etc.
- `chances.*` — tune % chances per tier

Mostly leave alone:
- `inventory.mods` — UNTARGH's mod tree is fine for Western/NATO weapons
- `inventory.items` — generic loot pools
- `difficulty.*` — see above

---

## Naming conventions in vanilla data

- bot type **filenames** and **DB keys**: all lowercase (`assault`, `fsofixerrookie`)
- `difficulty.*.Mind.REVENGE_BOT_TYPES` references: camelCase (`exUsec`, `bossKnight`, `followerBigPipe`) — preserved from BSG vanilla
- `chances.equipment` keys: PascalCase enum values (`FirstPrimaryWeapon`, `ArmorVest`, etc.)
- `chances.weaponMods` keys: snake_case (`mod_muzzle`, `mod_scope_001`, etc.)
- ammo caliber keys: PascalCase (`Caliber556x45NATO`, `Caliber762x39`)

When in doubt, look at vanilla JSONs in `C:\Games\SPT\SPT\SPT_Data\database\bots\types\`
for ground-truth examples.

---

## File extensions

- `.json` and `.jsonc` are both accepted by MoreBotsAPI's `CreateCustomBotTypes` glob (`*.json*`)
- SPT's JsonUtil is configured to skip JSON comments, so `.jsonc` files work fine
- Our FSO files currently use `.json`; renaming to `.jsonc` to add inline documentation
  is a zero-risk change whenever useful