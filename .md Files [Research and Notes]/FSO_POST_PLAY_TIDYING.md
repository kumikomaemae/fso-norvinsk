# FSO — Post-Play Tidying List

Cosmetic / non-urgent cleanup to do AFTER the first playthrough with Damjan (during his week off).
None of these affect gameplay, the build, or the Savior ending — they're polish + lint.

## Code lint (harmless warnings, safe to clean later)

### 1. CS0414 — unused field `_registeredThisRaid`
- **File:** `FSO.NorvinskSection1.Plugin/FsoAllyPatches.cs` (line ~113)
- **Warning:** `The field 'FsoHuntRegistrar._registeredThisRaid' is assigned but its value is never used`
- **Why it exists:** Intended as an "already registered hunt roles this raid?" guard. It's written
  (line ~160 `= true`, line ~194 `ResetForNewRaid() => ... = false`) but never READ. The call site
  (`FsoMatchStartedPatch.Postfix`, ~line 214-215) always calls `ResetForNewRaid()` then `Register()`
  back-to-back once per raid, so the flag is currently redundant.
- **Fix A (recommended):** add a guard at the top of `Register()`:
  ```csharp
  public static void Register()
  {
      if (_registeredThisRaid) return;   // already registered this raid
      try { ...
  ```
  Makes the field meaningful (idempotent registration) + silences the warning. One line, zero risk.
- **Fix B (alternative):** remove the field entirely — delete the declaration (~113), delete
  `_registeredThisRaid = true;` (~160), and empty `ResetForNewRaid()`'s body to a no-op stub
  (KEEP the method — it's called at ~line 214) OR remove the method + its call site together.
- **Note:** editing this file recompiles the PLUGIN and touches the hunt/FF system — that's why
  we deferred it. Do it carefully, rebuild, confirm hunting + friendly-fire still work.

### 2. Other ignorable warnings (no action needed unless desired)
- **CS8602 / CS8604 "possible null dereference"** — nullable-reference-type lint. SPT code triggers
  many of these. Harmless; ignore (or add `?.` / null-checks if you ever want a clean build).
- **SPT 4.1 ConfigServer deprecation warning** — about a ConfigServer usage pattern changing in a
  future SPT version. Harmless on 4.0.13. Revisit only if/when upgrading SPT.

## Gameplay polish (optional, post-first-run — from the roadmap)
- Kill-feed role locale (ScavRole/FSO display name in the kill feed)
- WTT Voice service (Mae's custom audio lines for the fixers)
- SAIN combat profiles for the FSO tiers (smarter AI behavior)
- Camo / visual polish on loadouts
- Achievement icon: if it didn't render, drop the `.png` from the achievement's `imageUrl`
  (`/files/achievement/fso_savior_icon` instead of `...fso_savior_icon.png`)
- `FSOGameStartHook.cs` — currently excluded from compilation (`<Compile Remove>` in the .csproj);
  revisit if it was meant to do something.
- Version sync: `.csproj <Version>` (0.2.0) vs `ModMetadata.Version` (0.5.1) — harmless mismatch,
  align if you like tidiness.

---
*Everything here is deliberately deferred. The gift's emotional core (quests, faction, Savior
ending, the engraved watch) + the gameplay-essential layer (loadouts, HP) come first. This list is
just for the relaxed polish pass after you and Damjan have played.*
