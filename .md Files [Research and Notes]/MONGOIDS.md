# FSO MongoID Reference

A flat lookup table for every MongoID used in FSO bot configurations.
Updated as Phase 2e progresses. **Single source of truth** — when in doubt, check here.

---

## Presets Rule

**Always use preset IDs for FSO weapons, NOT base weapon IDs.**

Base IDs spawn the weapon with whatever default attachments EFT picks, which can
include unsuppressed configurations even when design intent requires suppression
(e.g., MP5 base ID vs MP5SD preset ID — they share a base receiver but differ in
attached parts).

### Finding presets in vanilla SPT (globals.json)

When searching globals.json for a preset's outer key:

- The OUTER key (what we use in bot data) is the line above `"_changeWeaponName"`
  and `"_id"`, formatted as `"<24_hex>": {`
- Internal `_id` fields inside `_items` are inner-item IDs (the attachments),
  NOT the preset itself
- The base weapon's tpl is in `_tpl` of the first inner item
- The preset's display name is in `_name` near the end of the entry
- `_parent` field points to the inner-item ID of the base weapon WITHIN the
  preset, NOT a category parent

### Finding presets in modded JSONs (WTT-ContentBackport, Combat Arsenal, etc.)

Modded weapons usually nest their presets inside a `weaponPresets: [...]` array
within their main item config file. In this structure, the `_id` field on the
preset object IS the canonical preset ID (no outer dictionary key — it's stored
in an array, not a dictionary).

### Verification commands

```powershell
# Vanilla SPT presets:
Select-String -Path "C:\Games\SPT\SPT\SPT_Data\database\globals.json" -Pattern "<PRESET_ID>" -Context 0,4

# Modded weapon presets:
Get-Content "C:\Games\SPT\SPT\user\mods\<MOD>\db\CustomItems\<WEAPON>_config.json" | Select-Object -Skip <N> -First 20
```

### FSO preset inventory

- MP5SD: `59411abb86f77478f702b5d2`
- MP7 DEVGRU: `5bd05f1186f774572f181678`
- Staccato XC modified Glock: `68452d6389e25020b667ae1e`
- AS VAL Mod.4: `68d502e016a16a91e200bbbc`
- M249-SAW (Combat Arsenal upgrade): `66e71a1b735261b9f6efb5cf`
- MIKOR M32A1: TBD (verify preset when authoring Inner Circle)

---

## Weapons

### SMGs (Rookie tier)

- **MP5SD** preset: `59411abb86f77478f702b5d2` ⭐
  - Base weapon (informational, included in preset): `5926bb2186f7744b1c6c6e60`
  - Internal attachments (informational, included in preset):
    - `5926f2e086f7745aae644231` (SD upper receiver — integrated suppressor)
    - `5926d33d86f77410de68ebc0` (SD sound suppressor)
    - `5926f34786f77469195bfe92` (SD polymer handguard)
    - `5926d2be86f774134d668e4e` (MP5 drum rear sight)
    - `5926d40686f7740f152b6b7e` (MP5 A3 old model stock)
    - `5926c32286f774616e42de99` (MP5 cocking handle)

- **MP7 DEVGRU** preset: `5bd05f1186f774572f181678` ⭐
  - Base weapon (informational, included in preset): `5ba26383d4351e00334c93d9` (MP7A1)
  - Loadout: suppressed + scope + tactical lights + stock (matches FSO Rookie design intent)

### Carbines (Operative tier)

- **AS VAL Mod.4** preset: `68d502e016a16a91e200bbbc` ⭐ (from WTT-ContentBackport)
  - Base weapon (informational, included in preset): `6871284e9a353bb50606f3ed`
  - Vanilla AS VAL reference (informational): `57c44b372459772d2b39b8ce` (the AS VAL the Mod.4 is a clone of)
  - Parent category (informational): `5447b5fc4bdc2d87278b4567` (Assault Rifle)
  - Handbook category (informational): `5b5f78e986f77447ed5636b1` (9x39 ARs section)
  - **Note:** integral barrel suppressor, always suppressed regardless of attachments

### DMRs (Specialist Marksman)

- **SR-25**: `5df8ce05b11454561e39243b` (vanilla SPT — Mae's pick over DVL-10)
- **VSS Vintorez**: `57838ad32459774a17445cd2`

### Shotguns (Specialist Breacher)

- **AA-12 Gen.2**: `67124dcfa3541f2a1f0e788b` (Gen.2 has more attachment compatibility than Gen.1)

### LMGs (Specialist Suppression, Inner Circle)

- **M249-SAW (Combat Arsenal)** preset: `66e71a1b735261b9f6efb5cf` ⭐
  - Base weapon (informational, included in preset): `66e718dc498d978477e0ba75`
  - Vanilla M249 reference (informational): `6513ef33e06849f06c0957ca`
  - Parent category (informational): `5447bed64bdc2d97278b4568` (Machinegun)
  - Handbook category (informational): `5b5f79a486f77409407a7f94` (LMG section)

### Grenade Launchers (Inner Circle)

- **MIKOR M32A1**: `6275303a9f372d6ea97f9ec7`
  - Preset verification pending — check when authoring Inner Circle tier

### Sidearms (all tiers)

- **Staccato XC modified Glock** preset: `68452d6389e25020b667ae1e` ⭐
  - User-validated as preset rather than base item — preferred for bot data

---

## Armor

### Vests

- **LBT-6094A Slick Plate Carrier (Black)**: `5e4abb5086f77406975c9342` (Rookie tier)
- **NFM THOR Integrated Carrier body armor (full set)**: `60a283193cb70855c43a381d` (Operative+ tier)

### Plates

- **KITECO SC-IV SA ballistic plate (front)**: `656fafe3498d1b7e3e071da4`
- **KITECO SC-IV SA ballistic plate (back)**: `656fafe3498d1b7e3e071da4`
- **ESBI level IV ballistic plate (side, left/right)**: `64afdb577bb3bfe8fe03fd1d`

### Helmets

- **Diamond Age Bastion helmet (Black)**: `5ea17ca01412a1425304d1c0` (Inner Circle tier)
  - Level 6 forehead plate attachment: `5ea18c84ecf1982c7712d9a2`
  - **Clipping note:** may need to remove the forehead plate when mounting T-7 thermals,
    otherwise the helmet may render bugged

### Face Masks

- **ATOMIC Defense CQCM Mk.2 (Black, Level 6)**: `69bed4d13540cfcdd5012628`
  - From Combat Arsenal (Salco's Arsenal)
  - Uniform across ALL FSO tiers (the "FSO face")

### Eyewear / Mounted Optics

- **T-7 Thermal Goggles**: `5c110624d174af029e69734c` (Inner Circle tier — mounted on helmet)
  - **Note:** may need to omit the helmet's L6 forehead plate for proper rendering

### Earpieces

- **Proflex earbuds**: `69413241b1ce1e5fbb09ed0a` (uniform across all FSO tiers)

---

## Ammo

### Suppressed SMG (Rookie / Operative backup)

- **9x19 RIP**: `5c0d56a986f774449d5de529` (annihilates unarmored; armored via flesh damage)
- **4.6x30mm AP SX (MP7)**: `5ba26835d4351e0035628ff5`

### High-pen (Operative+)

- **M995A1 (5.56)** from Combat Arsenal: `69c021a559546892578a1aa0` (upgraded form of M995)
- **9x19 RIP**: `5c0d56a986f774449d5de529`
- **9x39 BP-gs (AS VAL)**: `5c0d688c86f77413ae3407b2` (Mae's pick over SP-6, "feels right for FSO")

### Specialist Breacher (AA-12)

- **12/70 FRAG-12**: `699358094978fa2d65f4dcf3` (small-radius grenade shells)
- **12/70 Hellfire**: `698924bf6dcd41ac313f5921` (portable flamethrower)

### Inner Circle 40mm

- **AP-MERS 40mm**: `5ede475339ee016e8c534742`

---

## Grenades

### Throwables

- **Model 7290 Flash Bang grenade**: `619256e5f8af2c1a4e1f5d92`
- **M67 fragmentation**: `58d3db5386f77426186285a0`

---

## Containers / Loot

### Backpacks

- **Tasmanian Tiger Modular Pack 45 Plus - MultiCam Black** (Rookie): `68947a8ce4bf255d1b0ca759`
- **Mystery Ranch 2 Day Assault Pack - Black** (Operative): `68947ab5a733b1602007e2fe`
- **6Sh118 raid backpack - Black** (Specialist+ leans): `6673b1ac5cae0610f1079d71`

### Rigs (Tactical Vests)

- **Haley Strategic D3CRX Chest Harness - Black** (Rookie): `5d5d85c586f774279a21cbdb`
- **Siege-R Armored Rig** (mid tiers — repurposed Black Division rig): `68947a4be4bf255d1b0ca746`

---

## Medical

- **Golden Vaseline Balm** (upgraded form): `69c0189dc89909925a0eadb9`
- **Salco Industries: UFAK** (higher-tier medkit, 700 charges): `69c01b37875a5d501e3647ae`
- **Surv12 Mk.2** (higher tiers, more charges + faster surgery): `69c01af0cba59524d1645772`
- **eTG-change regenerative stimulant** (x2): `5c0e534186f7747fa1419867`

---

## Inventory infrastructure (UNTARGH defaults — re-used as-is)

- **Pockets** (default vanilla bot pockets): `557ffd194bdc2d28148b457f`
- **Secured Container** (vanilla bot gamma equivalent): `5c0a794586f77461c458f892`
- **Scabbard** (UNTARGH pattern): empty — no melee weapon equipped

---

## Slots intentionally left empty in FSO bot data

- **ArmBand**: pending custom FSO armband item (future content phase)
- **Eyewear**: clean look — mask covers face anyway
- **Headwear** (Rookie / Operative / Specialist / Lead): bare-headed (Chris Redfield hair shows)
- **Scabbard**: no melee
- **SecondPrimaryWeapon**: Rookie/Operative are SMG/carbine-only tiers
