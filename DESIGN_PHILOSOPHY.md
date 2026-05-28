# FSO Design Philosophy

Notes on the *why* behind FSO's design decisions — the narrative threads that
connect data to intent. Every gear choice, name pick, and faction relationship
in this mod is downstream of these principles.

This document is descriptive (capturing decisions we've made), not prescriptive
(it doesn't constrain future decisions). If future-Mae overrides something here,
update this doc.

---

## Core principle: FSO is the organization Yan should have been

In Library of Ruina, the fixer Finn dies because Yan sent him into the Library
unprepared, with no equipment matching the threat. Yan didn't respect Finn —
didn't believe he was worth investing in. Finn died.

FSO operates by the inverse principle: **equipment is our strength**. We don't
send anyone in like Finn — not even Rookies. If a fixer wears the FSO armband,
they get FSO-grade gear, full stop.

This is encoded directly in data:

- All FSO Rookies receive L6 ceramic plates (KITECO SC-IV), not default Steel
- Every tier wears the ATOMIC CQCM Mk.2 mask (Level 6 face protection)
- Every fixer carries the Salco UFAK medkit (700-charge "portable field hospital")
- Every tier has the Proflex earpiece for full comms integration

Probationary status doesn't reduce kit quality. *FSO believes you're worth it.*

This principle drives the question Mae would ask Damjan (the player) implicitly
through the contract chain: **what does it mean to be valued by your employer?**

---

## The BD-intercept narrative

FSO is in active conflict with Black Division. Equipment "trickle-down" runs
through the organization based on this conflict:

- **Lead and Inner Circle** wear **Siege-R Armored Rigs** in matte black
- These rigs are canonically Black Division equipment
- FSO acquired them by *intercepting BD supply convoys* during ongoing operations

Mae's trader dialogue will acknowledge this in a tone of dry humor:
> "Don't ask where I got them. Let's just say BD has been having... supply issues."

This narrative serves multiple functions:

1. Justifies why Lead/Inner Circle's armor differs from Rookie/Operative's Slick
2. Builds anticipation for Quest 5 ("BD operations" — the climax)
3. Gives Mae personality as a fixer who appreciates a good intercept
4. Worldbuilds the broader Norvinsk in-fiction conflict

---

## Tier roles, encoded in gear

Each FSO tier's gear *physically expresses* their combat role. This is
mechanical AND aesthetic AND narrative simultaneously.

| tier | gear philosophy | role in combat |
|---|---|---|
| **Rookie** | Slick + L6 plates + Haley rig | maneuverable, agile, can flex anywhere |
| **Operative** | Slick + L6 plates + Haley rig + AS VAL Mod.4 | trusted workhorse, signature suppressed carbine |
| **Specialist** | THOR full armor + LBT-1961A (LMG-friendly slots) + 6Sh118 raid pack | role-specialist (Marksman/Breacher/Suppression) |
| **Lead** | Siege-R BD-trophy rig (full L6) | commands from flanks/rear, mobility matters |
| **Inner Circle** | Siege-R + Bastion helmet + T-7 thermals + Sam Fisher head | elite gear, full tactical sensors |

The Slick on Rookie/Operative has *zero movement/turn/ergonomics penalty*
because they're meant to be agile. The THOR on Specialist trades mobility
for full-body coverage because Specialists *anchor positions*. The Siege-R
on Lead/Inner Circle is the trophy of FSO's enemies, fit for the highest tier.

This is not gear-power escalation. It's role-fit escalation. Every tier is
*good at their job* — they're just good at *different* jobs.

---

## Restraint as design discipline

Some design tools are kept rare on purpose. Their power comes from their scarcity:

- **Color Fixer names** (Red Mist, Black Silence) → Inner Circle ONLY
- **Bastion helmet + T-7 thermals** → Inner Circle ONLY
- **Sam Fisher head model** → Inner Circle ONLY
- **MIKOR M32A1 grenade launcher** → Inner Circle ONLY
- **Personal-reference names** (Bong-bong, Forehead, The Mae of Hatred) → carefully placed in *specific* tiers, never spread thin

If everything is special, nothing is. Restraint is what makes the rare tiers
*feel* rare when Damjan encounters them in raid.

The same principle applies to weapon variety: Rookie has 2 SMG presets
(MP5SD, MP7 DEVGRU). Operative has 1 (AS VAL Mod.4). Inner Circle gets
the *whole* exotic pool because they're the climactic encounter.

---

## Equipment as characterization

Every gear choice encodes a layer of character:

- **The CQCM Mk.2 mask** on every tier = visual unity, "this is one organization"
- **Chris Redfield head model** on tiers 1-4 = corporate uniform, anonymity
- **Sam Fisher head on Inner Circle** = "this is a different kind of fixer"
- **Big Pipe + Birdeye voices on Lead** = the moment of "wait, the Goons?!" then "oh, allies"
- **LBT-1961A's 4x4 slots on Specialist** = LMG drums fit, physical justification for the role
- **Salco UFAK on every fixer** = "we keep our people alive"

The data IS the characterization. Damjan won't read these notes — but he'll
*feel* them when he plays, because every system points the same direction.

---

## Names as relationship cartography

92 names across 5 tiers. Most carry weight for Damjan specifically:

- **Rookie (32 names)**: L Corp default employees with Bong-bong and Yum-yum
  - The mundane base — these are *the staff*
- **Operative (25 names)**: agents from your shared no-deaths LobCorp run + Damjan's friends
  - The personal layer — these are *your history together*
- **Specialist (15 names)**: Phantom Thieves codenames (including the "Forehead" inside joke)
  - The shared-fiction layer — Damjan loves Persona 5
- **Lead (12 names)**: Sephirot + Library figures + Sinners (Don Quixote + Ryōshū as the plushie pair)
  - The Project Moon layer — the universe FSO inhabits
- **Inner Circle (8 names)**: Custom Color Fixers, each one a deep cut for Damjan
  - The most personal layer — every name a love letter

The principle: **rarer tiers carry denser personal weight**. Inner Circle's
8 names should each cause a moment of recognition. Lower tiers can include
"texture" names alongside personal ones because there are more slots.

---

## Mae as section manager, not bot

Mae is intentionally NOT in any bot pool. She doesn't spawn in raid as
a fixer. She exists as:

- The trader (the in-game contact who runs FSO from the office)
- The voice in trader dialogue
- The signature on the engraved Roler Submariner reward
- The *referenced* Color Fixer in Inner Circle's pool ("The Mae of Hatred")
  — but as a *title*, not a spawning entity

This protects Mae's narrative weight. She runs the operation; she doesn't
patrol it. The "Mae of Hatred" cameo is a *legend*, not a *bot*.

Coffee is Mae's recurring motif. The 67-coffee marked keys barter at LL4
is — per the design doc — "the soul of the mod." Mae's trader dialogue
will reference Sojiro (Persona 5) and Chesed (LobCorp) as "the three
great coffee shrines of fiction," tying coffee to her core identity.

---

## What this all serves

Damjan plays this mod. Every spawned fixer name, every weapon they carry,
every armor configuration, every faction relationship — all of it points
back at one thing:

**"I see you, and I made this for you."**

Bong-bong matters because we played LobCorp together. The Hollow Knight
matters because Damjan loves that game. The Final Curtain matters because
it's musical and theatrical and final, like the gift itself. The BD-intercept
narrative matters because Damjan will fight BD in Quest 5, and the gear he
fights *alongside* will be the gear FSO took *from* them.

The gift isn't the mod. The mod is the medium. The gift is the time spent
designing, naming, encoding care into data, and the *recognition* Damjan
feels when he plays it.
