# Black Division v1.1.1 — bug report: BD doesn't spawn (SPT 4.0.13)

Hi! Love the mod. Ran into the "BD doesn't spawn" issue a few others have mentioned, and while
digging through the decompiled code I think I found a null-handling bug that would abort all
spawning. Sharing in case it's useful. 🙂

Environment: SPT 4.0.13, with WTT-ContentBackport installed (1.0 content backported onto the
4.0.13 base). Also running RUAF Come Home (works fine), MoreBotsAPI, WTT CommonLib, Fika.

## TL;DR
`SpawnController.AdjustAllSpawns()` appears to throw **at the top** (the Labs gate section)
before it spawns anything — so BD never appears on ANY map. The trigger looks like an
`Add(null)` in the `else` branch when a Labs EXFIL spawn isn't found.

## Where it looks like it happens
In `SpawnController.AdjustAllSpawns()`, the Labs gate setup:

```csharp
BossLocationSpawn bossLocationSpawn1 = laboratory.Base.BossLocationSpawn
    .Find(x => x.TriggerId == "autoId_00014_EXFIL");
if (bossLocationSpawn1 != null)
    this.gate1 = bossLocationSpawn1;
else
    laboratory.Base.BossLocationSpawn.Add(this.gate1);   // <-- gate1 is null here

BossLocationSpawn bossLocationSpawn2 = laboratory.Base.BossLocationSpawn
    .Find(x => x.TriggerId == "autoId_00632_EXFIL");
if (bossLocationSpawn2 != null)
    this.gate2 = bossLocationSpawn2;
else
    laboratory.Base.BossLocationSpawn.Add(this.gate2);   // <-- gate2 is null here
```

The branch looks inverted: when the EXFIL spawn **isn't** found (`== null`), the `else` adds
`this.gate1` / `this.gate2` — but those fields haven't been assigned at this point, so they're
`null`. Adding `null` into the `BossLocationSpawn` list (and/or processing a list that contains
a null later) would throw. Because this is near the very start of `AdjustAllSpawns()`, the whole
method aborts before reaching the per-map hunt loop, so BD ends up spawning **nowhere**.

## Evidence
- BD loads fine (ModValidator shows v1.1.1 loaded).
- `OnLoad` does call `spawnController.AdjustAllSpawns()`.
- But **none** of the SpawnController's `logger.Info(...)` lines ever appear in the server log
  — e.g. "Adding Black Division spawn to Labs", "Adjusting Black Division spawns for {map}",
  "Enabling Black Division hunt for {map}". It seems to die before the first log call.
- RUAF Come Home (same framework) works fine in the same setup, so it's isolated to this path.

## What I'm NOT sure about (so I'm not guessing)
I'm not certain **why** `Find(x => x.TriggerId == "autoId_00014_EXFIL")` returns null in this
environment — whether SPT 4.0.13's Labs data doesn't include those exfil TriggerIds as
BossLocationSpawn entries, whether ContentBackport's Labs changes affect it, or whether those IDs
were tied to a different target version. That part would need your eyes. BUT — the `Add(null)` in
the `else` is a problem **regardless** of why the Find misses: even when the IDs aren't present,
the code probably shouldn't add a null gate.

## Suggested fix (defensive, independent of the root cause)
1) Don't add a null — only assign when found, and skip otherwise:
```csharp
var existingGate1 = laboratory.Base.BossLocationSpawn
    .Find(x => x.TriggerId == "autoId_00014_EXFIL");
if (existingGate1 != null)
    this.gate1 = existingGate1;
// else: nothing to add (or construct a real BossLocationSpawn if that was the intent)
```

2) And/or wrap the Labs gate section so a miss can't abort the rest of the method (the hunt
   spawns on other maps would still apply even if Labs gate setup is skipped):
```csharp
try { /* Labs gate + spawn setup */ }
catch (Exception e) { logger.Warning($"Labs gate setup skipped: {e.Message}"); }
```

3) If the gate-tie to those specific exfil TriggerIds was intentional, it may just need the IDs
   that exist in the current target environment (whatever Labs exfil spawns are present there).

Guarding the null + not letting the Labs section abort the per-map loop should get BD spawning
again across all maps.

Thanks for the mod — happy to test a patched build if that'd help! 🙂
