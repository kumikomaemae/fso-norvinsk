# FSO: Norvinsk Section 1 — Dialogue & Narrative Companion

**Purpose:** the unified writing reference for all of Mae's quest dialogue. Derived from the Design Doc v2 canon. Use this so every quest's text hits the right lore beat, surfaces the right voice traits, and pays off the LCCB → Golden Bough → TerraGroup → Black Division → Savior throughline consistently. This is the "how Mae talks and what each quest must accomplish" doc; the Design Doc is the "what's true" doc.

**Companion to:** `FSO_Norvinsk_Section_1_Design_Doc_v2.md` (canon) + `FSO_POLISH_ROADMAP.md` (build tracking).

---

## PART 1 — MAE'S VOICE (the unified vision)

### The core principle (locked)
Mae is a **real fixer-office manager giving real briefings.** No meta-commentary, no fourth-wall breaks — the immersion holds completely. But woven into the professional voice are turns of phrase, tiny warmths, and references that Damjan would *notice* — not "haha, a reference," but *"...wait, that's how **she** talks."* The mask never drops on purpose. He just slowly sees who's behind it.

### Voice traits (from Design Doc §3, with how to express each)
| Trait | How it shows in text |
|---|---|
| **Always tired** | coffee's cold, long days, "another day," weariness as baseline not complaint |
| **Deadpan professional** | clipped briefings, dry understatement, treats absurdity as routine |
| **Secretly silly** | a non-sequitur, an oddly specific aside, a joke buried in a flat delivery |
| **Coffee-dependent (Chesed)** | the running coffee thread; *occasionally* pitched so a PM-head catches the Chesed nod |
| **Names client only as "the LCCB"** | BUT not until Q3 (see reveal timing). Pre-Q3 = "the client," deliberately cagey |
| **Tarkov chaos = "Tuesday"** | bureaucratic calm about violence/horror; compares it unfavorably to City paperwork |
| **Warmth through tiny gestures** | an offhand kindness, quiet approval, an inside observation — never gushing |
| **When emotionally affected → MORE clipped** | Q4/Q5: as stakes rise, she gets *shorter*, not softer. Brittleness = restraint |

### Style rules (locked, from prior work)
- **lowercase**, no capital "i"
- **signoff: `-m`** (short dash, lowercase m, no period) — every Mae message
- **NO em-dashes (—) anywhere** in her text. Use commas, periods, sentence breaks. The only dash is the `-m` signoff.
- player-response lines (`acceptPlayerMessage` etc.) are short, capitalized normally (they're the PLAYER, not Mae): "Understood." / "Not yet." / "Both intact. As asked."
- moderate PM-reference density: subtle, earned, never a parade

### The recurring threads (plant + pay off across the chain)
1. **Coffee (Chesed).** Status-tracks across quests: cold → out of beans/instant → proper espresso setup → "peak capacity." A small ongoing gag that humanizes her. Tie to Chesed at least once catchably.
2. **Norvinsk-as-freedom.** Fixers are *thrilled* here: ammo's more common than food, no Head's taxes on rounds, no City firearm laws. Her unbothered-ness about Tarkov comes partly from this. "back home you'd file paperwork to carry what you're holding."
3. **The slow reveal.** "the client" (Q1–Q2) → "the LCCB" + Golden Bough hint (Q3) → BD = TerraGroup reveal + "are you still in?" (Q4) → endgame authorization (Q5).
4. **"You're becoming one of us."** Quiet through-line: the player starts as an outside asset and earns Mae's recognition. Q1 "you're starting to look like one" → culminates in Q5/the watch ("off-contract").
5. **The City as "back home."** Fixers reference it offhand. Never explained, just lived-in.

### Easter eggs (subtle, for Damjan specifically)
- **Oct 25, 2025** — the "first day" framing in Q1 nods to Damjan surprising Mae with Tarkov two days after her Oct 23 birthday (he'd researched it first). Keep it *in-fiction* — a line about first days that only lands different if you know.
- **Chesed** — the coffee thread (Lobotomy Corp's coffee-vending Sephirah).
- **The Phantom Thieves** — crew codename (Persona nod); can surface as a fixer in-joke.
- **67 coffees → Marked Keys** — the barter gag (lives in assort, not dialogue, but Mae can wink at "people pay me in coffee").
- **"We did it, chat!"** energy — NOT literally (that's the Ruina boss mod), but the warmth-after-a-clutch-moment tone can echo in Q5.

---

## PART 2 — PER-QUEST DIALOGUE GOALS

For each quest: the **lore beat** it must establish (from canon), the **voice notes** to surface, the **coffee status**, and any **easter egg**. Write the actual lines against these.

### Q1 — "Welcome to Section 1" *(Mae's title; was "First Day on the Job")*
**Lore beat:** introduce FSO + Mae as Section Manager; establish "the Office takes work, finishes work"; the player is vouched-for-but-not-one-of-us; the flash drives are "client correspondence" (DON'T name LCCB yet); seed "something larger in Norvinsk, can't confirm yet."
**Voice notes:** the wry "you actually showed up" opener (player was expected); deadpan competence; the first "you're starting to look like one" warmth on completion; "i'll know. i always know" (quiet authority).
**Coffee status:** cold. ("it's always cold.")
**Easter egg:** "everyone gets a first day. mine went worse than this one will." (the Oct-25 nod, fully in-fiction).
**Reveal level:** "the client." Cagey. No LCCB, no Golden Bough.
**Suit handoff:** on completion, point to the Fixer suit ("a fixer should look like one"). *(Mechanism TBD — Artem-rep or Mae-direct; dialogue works either way.)*

### Q2 — "Routine Maintenance"
**Lore beat:** "half this work is just clearing space"; the contract needs reduced hostile interference in operational zones; hint "the actual contract" is more interesting than this grunt work.
**Voice notes:** boredom-as-flavor (this is beneath her, mild dry impatience); the "Tuesday" energy lands well here (clearing hostiles = mundane chore); secretly-silly room.
**Coffee status:** ran out of beans, switched to instant. (mild suffering.)
**Reveal level:** still "the client." Teasing that there's more.
**Reward flavor:** suppressed weapon w/ custom preset name → **Lobotomy Corporation playthrough reference** (decide the name; a PM deep-cut). Ammo stockpile (Norvinsk-abundance flavor — she can remark ammo's everywhere here).

### Q3 — "Mutual Interest"
**Lore beat (THE FIRST BIG REVEAL):** client is **the LCCB** (first explicit naming — the cageyness ends here); they've been "following something"; they think **TerraGroup acquired an artifact**; first mention of the **Golden Bough** (Mae admits she doesn't fully know what it is, just what LCCB thinks); Section 1 has been getting "unusual interference."
**Voice notes:** this is where she trusts the player with real information — a subtle shift from transactional to confiding. "i don't ask too many questions. they pay on time." (deadpan over something ominous). The Golden Bough mention should feel like she's choosing her words.
**Coffee status:** found a proper espresso setup, "operating at peak capacity."
**Reveal level:** **LCCB named. Golden Bough introduced.** Use "the LCCB" consistently from here.
**Item:** TerraGroup "Blue Folders" Materials, FIR retrieval on Streets.

### Q4 — "Hostile Workplace"
**Lore beat (THE SECOND REVEAL + COMMITMENT):** confirm **Black Division is TerraGroup's internal special-ops force**, dispatched to clean up investigators (i.e. them). Section 1 has taken casualties. The player's narrative commitment moment: **"are you still in?"**
**Voice notes:** ⚠️ KEY — *when emotionally affected, she gets MORE clipped, not less.* She isn't panicked. She's **quieter**. Professionalism gets *brittle* — shorter sentences, more restraint, the warmth pulled tighter. This is the emotional hinge of the chain; write it with restraint, not melodrama. "we knew this contract was risky. it's now confirmed risky. are you still in?"
**Coffee status:** someone in the office found her a proper espresso setup again, "operating at peak capacity" — but note the contrast: the coffee gag continues even as things get grim. (the mundane persisting through the serious = very PM.)
**Reveal level:** BD = TerraGroup, full. Stakes named.
**Item/reward:** kill BD; Labs keycards (Black + standard) → LL3. Tension into Q5.

### Q5 — "Closing the Office" ✨ FINALE
**Lore beat (THE STAKES + RESOLUTION):** this is the endgame. The Golden Bough is real, TerraGroup has it, and if used → the EFT "bad ending" (TerraGroup over a nuked world). Mae authorizes **Inner Circle** deployment ("they were on call. they're on the way."). Completion = LCCB's contract fulfilled, the Bough recovered, **humanity preserved → the Savior ending.**
**Voice notes:** pre-quest = acknowledges the endgame, authorizes the no-quarter team (the gravity shows through clipped professionalism). Post-quest = **brief warmth-through-deadpan**, the mask almost slips, then she plays it off ("this operation's got me feeling exhausted, need more coffee"). This is the payoff of the whole "you're becoming one of us" thread. The watch + mail carry the real emotional weight (see below).
**Coffee status:** the gag resolves into the warmth — coffee as the thing she deflects to when feeling something.
**Reveal level:** everything. The Savior framing.
**The Watch + Mail:** delivered via mail (subject "Off-contract.", closes "Read the back."). The inscription + mail body are **Mae's to write by hand** (Design Doc §6 — left deliberately blank, no time pressure, the heart of the gift). The watch's case-back: FSO crown engraving, signoff "off-contract. don't ask. -m". *(Note: design doc shows "— M." but per locked style use the lowercase `-m` to match her voice everywhere else. Mae's call.)*

---

## PART 3 — CONTINUITY GUARDRAILS (so dialogue stays consistent)
1. **Client naming:** "the client" in Q1–Q2; "the LCCB" from Q3 on. Never "Limbus" generically. Never name LCCB early.
2. **Golden Bough:** first mentioned Q3, never before. Mae stays vague on what it *is* (she genuinely doesn't fully know).
3. **Black Division:** their TerraGroup-internal nature is a Q4 reveal. Q1–Q3 can reference "interference"/"casualties" without naming who.
4. **Emotional escalation = MORE clipped.** Never write Mae getting *wordier* or softer under stress. Brittleness shows as restraint.
5. **Coffee is the pressure valve.** It tracks across all 5 quests (cold → instant → espresso → peak → deflection). It's how she handles feeling things.
6. **Player arc:** outside asset (Q1) → trusted with info (Q3) → asked to commit (Q4) → recognized as one of them (Q5/watch). Each quest should nudge this one notch.
7. **Style:** lowercase, no capital i, `-m` signoff, NO em-dashes, moderate PM density. Every time.
8. **Fixers stay competent in-game.** Silliness lives ONLY in framing (Mae's text), never in fixer combat behavior.

---

## PART 4 — Q1 ADAPTATION (current working version + proposed refinements)

**Mae's current locale (her latest edit) is strong and canon-consistent.** "Welcome to Section 1" + "you actually showed up" + "the client" (correctly cagey) + "i'll know. i always know." all land. The `-m` signoff and no-em-dash rule are applied.

**Optional deepenings to consider** (pull from canon, take what resonates — NOT corrections):
- **Norvinsk-freedom seed:** a dry aside that she/the office is oddly at home here because, unlike back in the City, nobody taxes the ammo or files paperwork for a rifle. Plants Thread #2 early.
- **Chesed-catchable coffee:** pitch one coffee line so a PM-head clocks the Chesed nod, rather than generic "coffee's cold."
- **"Tuesday" energy:** one line treating Tarkov's lethality as utterly mundane.
- These are *optional* — Q1 is already good. Add only what feels like her.

*(The actual Q1 line-rewrite, if Mae wants it, gets drafted against this companion — building on her "Welcome to Section 1" version, not replacing it.)*
