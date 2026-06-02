# FSO BOT SYSTEM — COMPLETE INVENTORY MANIFEST
*(extracted from db/bots/types/*.json + config/*.jsonc via the live repo)*

## ARCHITECTURE CONFIRMED
- **config `.jsonc`** = generation TUNING only (durability 100%, NVG/laser/light activation chances, currency stack sizes, forceStock, forceOnlyArmoredRigWhenNoArmor). NO gear list.
- **type `.json`** = the actual gear: `inventory` (equipment/Ammo/mods/items) + `chances` + `generation` + AI `difficulty` + `health` + `skills` + names.

---

## PER-TIER EQUIPMENT (FirstPrimaryWeapon weighted)

### ROOKIE (fsofixerrookie)
- ArmorVest: `5e4abb5086f77406975c9342` (2-plate: Front/Back + soft front/back)
- TacticalVest: `5d5d85c586f774279a21cbdb`
- Backpack: `68947a8ce4bf255d1b0ca759`, Earpiece: `69413241b1ce1e5fbb09ed0a`, FaceCover: `69bed4d13540cfcdd5012628`, Holster: `68452c3da87156b67d9ec538`, Pockets: `557ffd194bdc2d28148b457f`, SecuredContainer: `5c0a794586f77461c458f892`
- NO Headwear, NO Eyewear
- FirstPrimary: `5926bb2186f7744b1c6c6e60`:5, `5bd70322209c4d00d7167b8f`:3
- Ammo: 9x19 `5c0d56a986f774449d5de529`, 46x30 `5ba26835d4351e0035628ff5`

### OPERATIVE (fsofixeroperative)
- ArmorVest: `5e4abb5086f77406975c9342` (same 2-plate)
- TacticalVest: `5d5d85c586f774279a21cbdb`
- Backpack: `68947ab5a733b1602007e2fe`, rest same accessories as rookie
- NO Headwear, NO Eyewear
- FirstPrimary: `6871284e9a353bb50606f3ed`:1
- Ammo: 9x19 `5c0d56a986f774449d5de529`, 9x39 `5c0d688c86f77413ae3407b2`

### SPECIALIST (fsofixerspecialist)
- ArmorVest: `60a283193cb70855c43a381d` (FULL plate rig: front/back/L/R side + soft armor + collar + shoulders + groin)
- TacticalVest: `5e9db13186f7742f845ee9d3`
- Backpack: `6673b1ac5cae0610f1079d71`, rest same accessories
- NO Headwear, NO Eyewear
- FirstPrimary: `5df8ce05b11454561e39243b`:3, `67124dcfa3541f2a1f0e788b`:3, `66e718dc498d978477e0ba75`:1
- Ammo: 9x19, 556x45 `69c021a559546892578a1aa0`, 12g `699358094978fa2d65f4dcf3` + `698924bf6dcd41ac313f5921`

### LEAD (fsofixerlead)
- ArmorVest: (empty) — armor is the TacticalVest (armored rig)
- TacticalVest: `68947a4be4bf255d1b0ca746` (FULL plate rig: front/back/L/R + soft + collar + shoulders + groin)
- Backpack: `6673b1ac5cae0610f1079d71`, rest same accessories
- NO Headwear, NO Eyewear
- FirstPrimary: `6871284e9a353bb50606f3ed`:3, `66e718dc498d978477e0ba75`:1
- Ammo: 9x19, 9x39, 556x45

### INNER CIRCLE (fsofixerinnercircle)
- ArmorVest: (empty) — armor is the TacticalVest (armored rig)
- TacticalVest: `68947a4be4bf255d1b0ca746` (FULL plate rig, same as Lead)
- Headwear: `5ea17ca01412a1425304d1c0` (HELMET — with Helmet_top `657f9a55c6679fefb3051e19` + Helmet_back `657f9a94ada5fadd1f07a589`)
- Eyewear: `5c110624d174af029e69734c` (ONLY tier with eyewear)
- Backpack: `6673b1ac5cae0610f1079d71`, rest same accessories
- FirstPrimary: `6871284e9a353bb50606f3ed`:3, `66e718dc498d978477e0ba75`:2, MIKOR `6275303a9f372d6ea97f9ec7`:1
- Ammo: 9x19, 9x39, 556x45, 40x46 AP-MERS `5ede475339ee016e8c534742`

---

## ITEMS (loot placed in slots) — weighted; ⚠️ NO MEDS IN ANY TIER
Common across tiers: coffee `694c6d5568b849f7bb05b7ac`, GP coin `5d235b4d86f7742e017bc88a`, Labs keycard `5c94bbff86f7747ee735c08f`, bitcoin `59faff1d86f7746c51718c9c`, intel/docs items (`69c01af0cba59524d1645772`, `5c1d0f49...`, `5c1d0efb...`, `5c1d0dc5...`, `5c1d0d6d...`, `6656560053eaaa7a23349c86`), GPU `6656560053...`.
- The escalation: higher tiers carry more coffee/GP + more valuable intel/bitcoin.
- **CRITICAL GAP: not a single medical item (no IFAK/AFAK/grizzly/salewa/CMS/surv12) in any slot, any tier.**

---

## 🚨 ROOT CAUSE OF THE GEAR ISSUES

### NO MEDS (all 5 tiers)
- `inventory.items` pools contain NO medical tpls.
- `generation.items.healing` = whitelist EMPTY, weights {0:1, 1:2, 2:1} (wants to place 0-2 healing).
- `generation.items.drugs` = whitelist EMPTY, weights {0:1, 1:2}.
- `generation.items.stims` = whitelist EMPTY, weights {0:2, 1:1}.
- → The generator is told to place healing items but has an empty pool → bots spawn with zero meds → **can't heal mid/post-fight.** CONFIRMED `CAN_USE_MEDS: true` in the AI block, so it's purely an inventory gap.
- **FIX (mechanism TBD — verify against loadout format):** populate a medical pool (UFAK `69c01b37875a5d501e3647ae`, grizzly, IFAK, salewa, etc.) so the existing healing weights have something to draw.

### MAGAZINES
- `generation.items.magazines` = whitelist EMPTY, weights {0:0, 1:0, 2:1, 3:3, 4:1} (wants 2-4 mags).
- Empty whitelist → SPT *should* fall back to mags compatible with the equipped weapon. Need to confirm whether the empty whitelist is why conventional weapons run dry, OR whether it's working and the issue was elsewhere. (MILKOR is fine — integrated cylinder, not detachable mags.)

### ARMOR PLATES (the "Bastion missing plate" report)
- Plates ARE defined in `inventory.mods` for every armored tier:
  - rookie/operative vest `5e4abb50...`: Front_plate + Back_plate (+ soft)
  - specialist vest `60a28319...`: full plate set
  - lead/IC rig `68947a4b...`: full plate set (front/back `656fafe3498d1b7e3e071da4`, sides `64afdb577bb3bfe8fe03fd1d`, soft, collar, shoulders, groin)
  - IC helmet `5ea17ca0...`: Helmet_top `657f9a55...` + Helmet_back `657f9a94...`
- BUT — `chances.equipmentMods` (IC) lists: front_plate/back_plate/left_side_plate/right_side_plate = 100, mod_nvg 4, mod_equipment_001 4, mod_equipment_000 2. **NO `Helmet_top`/`Helmet_back` entry in the chances block** → the helmet's armor plates may roll at a non-100 default → possibly why the helmet appears without its plate.
- ALSO note: chances uses lowercase plate slot names (`front_plate`) while mods uses capitalized (`Front_plate`) — verify SPT slot-name normalization isn't dropping them.

---

## NAMES (all firstName ["FSO"])
- ROOKIE lastName: Bong-bong, Yum-yum, Joshua, Emilia, Tim, Mason, Juliet, Daniel, Hunter, Cooper, Joy, London, Anthony, Parker, Julian, Ria, Angel, Cedric, Oliver, Owen, Adam, Max, Quinn, Harry, Xavier, Ray, Haru, Ryn, Noah, Emma, Bella, Piper
- OPERATIVE: Poussey, Miho, Dexter, Katya, Gabriella, Mr. Black, Vincent, John LobCorp, Bongbong2-4, Tino, Fabio, Danny, Philipp, Klara, Alex, Mattias, Wetherby, Igoree, LaVerne, Firenze, Maximin, Camille, Anastasia
- SPECIALIST (Phantom Thieves): Joker, Skull, Panther, Mona, Fox, Queen, Oracle, Forehead, Crow, Violet, Lavenza, Igor, Arsene, Takemi, Becky
- LEAD (LobCorp/Limbus Sephirah + sinners): Binah, Hokma, Gebura, Hod, Angela, Roland, Angelica, Xiao, Don Quixote, Ryoshu, Faust, Yi Sang
- INNER CIRCLE (E.G.O. titles): The Mae of Hatred, The White Prince, The Red Rebellion, The Pink Idol, The Purple Robin, The Hollow Knight, The Distorted Saint, The Final Curtain

---

## 🔴 FRIENDLY-FIRE CLUE (the #1 blocking essential)
From IC type file `difficulty.normal.Mind`:
- `DEFAULT_USEC_BEHAVIOUR: "Warn"`, `DEFAULT_BEAR_BEHAVIOUR: "Warn"`, `DEFAULT_SAVAGE_BEHAVIOUR: "Warn"`
- `CAN_RECEIVE_PLAYER_REQUESTS_USEC: true` (normal; false in easy/hard)
- `CHANCE_SHOOT_WHEN_WARN_PLAYER_100: 40` (normal), `CHANCE_TO_STAY_WHEN_WARN_PLAYER_100: 80`
- `WARN_BOT_TYPES: ["assault","assaultGroup","pmcUSEC"]`
- Boss-style warn block present: `BOSS_DIST_TO_WARNING_USEC: 20`, `TOTAL_TIME_KILL_AFTER_WARN: 10`, `WARN_PLAYER_PERIOD: 30`
- **INTERPRETATION:** FSO bots have boss-like "warn the player, then kill after the warn period" behavior toward USEC/BEAR. This is almost certainly WHY they shoot the player at close range — the Mind behavior treats the player as warn-then-engage, independent of (or overriding) the faction friendliness we set in Mod.cs.
- **NEEDS RESEARCH:** what values can DEFAULT_USEC_BEHAVIOUR / DEFAULT_BEAR_BEHAVIOUR take (is there "Friend"/"Ignore"/"None"?); how the faction relationship interacts with Mind behavior; whether MoreBotsAPI/SPT has a clean "friendly to player" path.

---

## HEALTH (per body part) — IC example
Head 30-35, Chest 80-85, Stomach 70, arms 60, legs 65. (Lower-tier likely scaled down.)

## SKILLS — IC: Endurance/Health/RecoilControl/Strength/StressResistance/Vitality all 5100 (maxed), BotReload 100-500.
