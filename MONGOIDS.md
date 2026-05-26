# FSO MongoID Reference

A flat lookup table for every MongoID used in FSO bot configurations.
Updated as Phase 2e progresses. **Single source of truth** — when in doubt, check here.

## Weapons

### SMGs (Rookie tier)

- **MP5SD** preset: `59411abb86f77478f702b5d2`
  - Base weapon: `5926bb2186f7744b1c6c6e60` (informational — included in preset)
  - Attachments listed below are baked into the preset; do not need separate handling:
    - `5926f2e086f7745aae644231` (SD upper receiver)
    - `5926d33d86f77410de68ebc0` (SD sound suppressor)
    - ... (etc — keep for reference)

- **MP7 DEVGRU** preset: `5bd05f1186f774572f181678`
  - Base weapon: `5ba26383d4351e00334c93d9` (informational — included in preset)
  - Loadout: suppressed + scope + tactical lights + stock (matches FSO Rookie design intent)

## Presets rule

**Always use preset IDs for FSO weapons, NOT base weapon IDs.**

When searching globals.json for a preset's outer key:

- The OUTER key (what we use) is on the same line as the preset's opening `{`
- Internal `_id` fields are inner-item IDs of the preset's attachments
- The base weapon's tpl is in `_tpl` of the first inner item
- The preset's display name is in `_name` near the end of the entry
- The `_parent` field points to the inner-item ID of the base weapon WITHIN the preset, NOT the preset's parent

To find an outer key when searching globals.json:

1. Search for either the preset's `_name` or the base weapon's tpl
2. Look UPWARD from the match for the dictionary key (line starting with `"<24_hex>":`)
3. Verify by checking that line is immediately followed by `"_changeWeaponName"`, `"_encyclopedia"`, or `"_id"`

### Carbines (Operative tier)

- **AS VAL Mod.4**: - *it could not find the AS VAL Mod.4 - so i went into WTT - Content backport and found it myself, found 3 ids at the top, any ideas on which is the main one?*
"itemTplToClone": `57c44b372459772d2b39b8ce`
"parentId": `5447b5fc4bdc2d87278b4567`
"handbookParentId": `5b5f78e986f77447ed5636b1`

### DMRs (Specialist Marksman)

- **SR-25**: `5df8ce05b11454561e39243b` (i dislike the DVL-10, SR-25 instead :P)
- **VSS**: `57838ad32459774a17445cd2`

### Shotguns (Specialist Breacher)

- **AA-12 Gen.2**: `67124dcfa3541f2a1f0e788b` (Gen.2 has more attachment compatibility)

### LMGs (Specialist Suppression, Inner Circle)

- **M249-SAW**: *same as for the AS VAL Mod.4, unsure which is main ID*
    "itemTplToClone": `6513ef33e06849f06c0957ca`
    "parentId": `5447bed64bdc2d97278b4568`
    "handbookParentId": `5b5f79a486f77409407a7f94`

### Grenade Launchers (Inner Circle)

- **MIKOR M32A1**: `6275303a9f372d6ea97f9ec7`

### Sidearms (all tiers)

- **Staccato XC modified Glock -Mod Weapon**: `68452d6389e25020b667ae1e` (this is a preset id, i'm not using the ids at the top anymore, is this more correct than the alternative ids i used for modded weapons before?)

## Armor

### Vests

- **Slick plate carrier**: `TBD`
- **NFM THOR Integrated Carrier body armor (full set)**: `60a283193cb70855c43a381d`

### Plates

- **KITECO SC-IV SA ballistic plate (front)**: `656fafe3498d1b7e3e071da4`
- **KITECO SC-IV SA ballistic plate (back)**: `656fafe3498d1b7e3e071da4`
- **ESBI level IV ballistic plate (Side)** -*note it can be used for left and right side*: `64afdb577bb3bfe8fe03fd1d`

### Helmets

- **Diamond Age Bastion helmet (Black)** -*a helmet we are using instead, as no THOR helmet exists, i didn't clarify that for you*: `5ea17ca01412a1425304d1c0` + `5ea18c84ecf1982c7712d9a2` <- *this id is for the level 6 forehead plate for the helmet, perfect for inner circle*

### Face Masks

- **ATOMIC Defense CQCM Mk.2 (black)**: `69bed4d13540cfcdd5012628` -*found through combat arsenal (salco's arsenal)* -this id was at the VERY top, before the 3 other ids i used previously for modded items, the itemtpltoclone, parentid, and handbookparentid, so i assume this one is the actual, real id?

### Eyewear / Optics (worn)

- **T-7 Thermal Goggles**: `5c110624d174af029e69734c` -*note, we may need to remove the armor plate from the bastion helmet for the T-7 mount to actually spawn correctly, otherwise the helmet would be bugged*

### Earpieces

- **Proflex earbuds**: `69413241b1ce1e5fbb09ed0a`

## Ammo

### Suppressed SMG (Rookie/Operative)

- **9x19 RIP (suppressed-friendly)** -*we're using rip since it is meant to annihilate unarmored targets/armored targets through flesh damage*: `5c0d56a986f774449d5de529`
- **4.6x30mm AP SX (MP7)**: `5ba26835d4351e0035628ff5`

### High-pen (Operative+)

- **M995A1 (5.56)** -*custom ammo from combat arsenal, upgraded form of M995*: `69c021a559546892578a1aa0`
- **9x19 RIP**: `5c0d56a986f774449d5de529`
- **9x39 BP-gs (AS VAL)** -*better ammo than SP-6, feels right for FSO*: `5c0d688c86f77413ae3407b2`

### Specialist Breacher (AA-12)

- **12/70 FRAG-12** -*a even better form of HE slugs for shotguns, basically fires small-radius grenades*: `699358094978fa2d65f4dcf3`
- **12/70 Hellfire**: -*turns a shotgun into a portable flamethrower, basically* `698924bf6dcd41ac313f5921`

### Inner Circle 40mm

- **AP-MERS 40mm**: `5ede475339ee016e8c534742`

## Grenades

### Throwables

- **Model 7290 Flash Bang grenade**: `"619256e5f8af2c1a4e1f5d92`
- **M67 fragmentation**: `58d3db5386f77426186285a0`

### Backpacks

- **Tasmanian Tiger Modular Pack 45 Plus - MultiCam Black (Rookie)**: `68947a8ce4bf255d1b0ca759`
- **Mystery Ranch 2 Day Assault Pack - Black (Operative)** -*we might switch specialist+ and this one, personally*: `68947ab5a733b1602007e2fe`
- **6Sh118 raid backpack - Black (Specialist+)** -*a massive, heavy pack, meant for carrying many items, agree with this choice? or do we do lighter equipment?*: `6673b1ac5cae0610f1079d71`

### Rigs (Tactical Vests)

- **Haley Strategic D3CRX Chest Harness - Black (Rookie)**: `5d5d85c586f774279a21cbdb`
- **Siege-R Armored Rig (mid tiers)** -*repurposed black division rig, very defensive, VERY heavy*: `68947a4be4bf255d1b0ca746`

## Medical

- **Golden Vaseline Balm** -*a upgraded form of vaseline balm: `69c0189dc89909925a0eadb9`
- **Salco Industries: UFAK** -*a higher-tier medkit with 700 charges, basically a portable field hospital on every FSO fixer*: `69c01b37875a5d501e3647ae`
- **Surv12 Mk.2 (higher tiers)** -*upgraded Surv-12 with more charges and faster surgery speed*: `69c01af0cba59524d1645772`
- **eTG-change regenerative stimulant injector** (*x2*) -*stimulant that regenerates health extremely fast* - `5c0e534186f7747fa1419867`
