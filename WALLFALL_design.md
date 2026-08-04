# WALLFALL — Game Design Doc
*Working title — rename freely. A separate project from BLINDSIDE; they share a genre and nothing else.*

## Logline
A 1v1 auto-battler fought across **four lanes at once**. Each lane is its own small board with its own kit, its own bed, and its own resource — iron, diamonds, emeralds, and the gold lane everyone wants. Every round you decide where your strength goes. Simultaneous-move Colonel Blotto dressed as a Bedwars autobattler.

---

## The Thesis
Auto-battlers are the only genre that can run a true multi-front war, because you don't pilot the fights — you **allocate** before them. Piloting four fronts at once is humanly impossible in an action game; *allocating* across four fronts is completely natural in a genre where combat resolves itself. WALLFALL exploits that: the strategic core is never "is my board strong?" but **"where does my strength go?"** — which lane to reinforce, which to hold, which to quietly concede this round while your opponent secretly makes the same four bets.

This is Colonel Blotto, the classic game-theory allocation duel with no dominant strategy — pure prediction and misdirection — finally playable as a video game. Everything is visible (no hidden stats, no deduction machinery); the only fog is **time**: you commit your moves before seeing your opponent's. Wide, not deep. Fast, aggressive, readable.

## Design Pillars
1. **Four fronts, one army.** ~12 units total, partitioned across four small lanes. The game is the partition.
2. **Allocation is the skill.** Buying, transferring, and conceding are the verbs. The fights execute your bets.
3. **Simultaneous commitment.** Both players plan blind each round; the walls drop and the bets reveal. All the mind-games live in that delay.
4. **Losing a lane bends you, it doesn't break you.** Bedwars' arc — bed break → consolidation → all-in — is mechanical, not just emotional. And no lane loss, not even the gold lane, is a hidden elimination.
5. **Every fight is watchable.** Lanes resolve one at a time. No split-screen soup.
6. **Complexity budget is sacred.** One shared shop, small lanes, four currencies with strictly disjoint jobs. The load must stay at "TFT plus an allocation layer," never "four TFTs."

---

## Match Structure

### Phase 1 — The Walls (rounds 1–5)
Both players build behind opaque walls: **no scouting the opponent at all** during this phase. You fight light PvE rounds in each lane to farm starting resources, you **choose a kit for each of your four lanes**, and you shape your opening partition. This front-loads the strategy — five rounds of pure preparation, betting on what the enemy is building without a single look — and gives the match its first dramatic beat:

**The walls drop at round 6.** Both setups reveal simultaneously. From here on it's PvP — and because it's a 1v1, from this point you can **always see your opponent's boards** (as of the last fight; see The Fog below).

### Phase 2 — The War (round 6 onward)
Standard round loop, repeated until a player's four lanes are all dead:
1. **Income** — the unconditional gold floor, interest, plus each living lane's resource (won or uncontested-banked).
2. **Planning (blind)** — buy from the shared shop, assign and transfer units, spend resources, level, reposition. You see the opponent's boards **as of the previous fight**, not their current moves.
3. **Reveal** — this round's changes become visible on both sides. The mini walls-drop, every round.
4. **Sequential fights** — lanes resolve one at a time (~15 seconds each), **gold lane last** so the round builds to its richest prize.
5. **Resolve** — lane HP damage, wither checks, resource awards, lane deaths and consolidation.

## The Fog (this replaces hidden information)
There are no hidden stats anywhere. The only concealment is **commitment delay**: during planning you see the opponent's lanes frozen at their last-fight state; your purchases and transfers reveal only at the round's reveal step, simultaneously with theirs. That single rule creates the entire yomi layer — feints, baits, over-commits, reads — with zero extra machinery. (During the Walls phase the delay is total: five rounds of no information at all.)

## Lanes & Boards
- Four lanes per player; each lane is a small board of **3 unit slots** (may grow slightly with level — but four lanes means lane size stays minimal to protect the complexity budget). Total army ~12 units — comparable to one TFT board, partitioned.
- Each lane has its own **HP pool** and its own **Bed** (see below).
- Lanes are paired: your lane 1 always fights their lane 1, etc.
- **Positioning within a lane** is simple (front/back rows) — the deep positioning game is *between* lanes, not within them.

## The Bed & The Wither
Each lane's HP pool is anchored by its **Bed**.
- Lose a lane's fight → that lane loses HP, scaled by margin of defeat.
- **The Wither rule:** a *lopsided* loss (e.g. wiped while 3+ enemy units survive) triggers bonus HP loss — getting crushed is punished harder than losing close. Conceding a lane is viable; getting massacred in it is not free.
- A lane at 0 HP is **dead**: its bed breaks, and it stops producing its resource for you permanently.
- **All four lanes dead = you lose the match.**
- **The gold lane's bed is tankier by default** — killing the enemy's economy front is a long campaign, never a cheese rush.

### Consolidation (the bedless all-in)
When your lane dies, **its surviving units return to you for free redistribution** into your remaining lanes at the next planning step. You lost the income; you did not lose the army. Three fat lanes versus four medium ones is a real fighting position — the Bedwars bedless-rush arc, mechanized. This is the primary anti-death-spiral valve and it is not optional.

### Uncontested lanes
When your lane is dead, the opponent's opposing lane is uncontested. **An uncontested lane deals no lane damage** — it simply auto-banks its resource for its owner each round. The pressure of losing a lane is economic (you lose income *and* concede income), never a free execution. The opponent's real choice is whether to *keep* units parked in an uncontested lane (safe income) or consolidate them into contested lanes (pressure) — conceding a lane doesn't end the allocation game, it sharpens it.

## Economy — the floor, the interest, and four fronts
**There are no streaks.** Round-level win/loss is ambiguous across four lanes (what's a "streak" at 2–2?), so streaks are deleted rather than patched — the comeback valves live elsewhere (consolidation, uncontested-lane rules, margin-based wither, the income floor).

**The gold floor:** every player receives a small, unconditional base income every round, no matter what happened. This is the survival floor — never zero, never contested. A player who lost every lane fight can still buy. **Interest** applies as usual (+1 per 10 banked, capped) — unambiguous and the economy's greed dial.

On top of the floor, each lane is a resource front. Distinct faucet **and** distinct sink for every currency, or it doesn't exist. Nothing is convertible.

| Lane | Currency | Faucet | Sink (exclusive) |
|---|---|---|---|
| 1 | **Iron** | Winning lane 1 (plus a small trickle) | **Consumables & logistics** — most importantly **unit transfers between lanes** |
| 2 | **Diamonds** | Winning lane 2 | **Items & permanent unit upgrades** |
| 3 | **Emeralds** | Winning lane 3 | **Rare, match-bending powers** (big one-round buffs, a bed repair, an extra transfer window) |
| 4 | **Gold bonus** | Winning lane 4 | Adds to your gold (units/XP), plus a soft perk that round (e.g. discounted XP) |

- **The gold lane is first among equals, not a win condition.** Winning it is a meaningful bounty and plausibly the most contested prize on the map — but losing it means poverty, not paralysis, because the floor and interest never turn off. This is deliberate: gold buys units and XP (all fundamental power), so fully lane-gating it would be an existential snowball — lose lane 4, can't buy, lose everything. The floor removes the trap while keeping the fantasy of *fighting over the economy itself*.
- The resource lanes create built-in asymmetry: iron→diamond→emerald→gold is Bedwars' risk ladder, expressed as *which lane you choose to win*.
- **Balance dials:** emerald powers are the most dangerous thing in the economy — start weak and rare. And watch the gold-lane bounty: too big and both players' allocation collapses into lane 4, flattening the Blotto game; the mind-game is richest when the fronts stay comparable and *players* decide which matters this round.

## The Shared Shop
One shop, TFT-style (5 slots, gold to buy, gold to reroll, refreshes each round). You buy a unit **then assign it to a lane** — one decision stream, not four. This is the single most important complexity-control decision in the game: four separate shops would quadruple the cognitive load and kill it. Units are drawn from one shared pool; costs 1–5 by tier.

## Transfers — the central dial
Moving a unit between lanes costs **iron** and is capped (start: 2 transfers per round). This dial sets the game's entire character:
- Free/unlimited transfers → pure round-by-round Blotto: maximum mind-games, zero commitment.
- No transfers → four parallel solitaire games: zero mind-games.
- **Costed and capped (the target):** commitments are real but not coffins. Reads matter because reacting is expensive.

Tune this dial before anything else in playtesting.

## Kits
During the Walls phase you pick **one kit per lane** (from a starting pool of ~6; opponent's picks hidden until the walls drop). Kits are **utility identities, never stat sticks** — they change how a lane plays, not how hard it hits:

- **Forgemaster** — this lane's units can hold an extra item; diamond costs reduced here.
- **Quartermaster** — transfers in/out of this lane are cheaper; +iron trickle.
- **Warlord** — this lane's wither threshold is harsher for the *enemy* (crush them harder); small on-win bonus.
- **Architect** — this lane's bed has bonus HP and cheap repairs; loses HP slower on close losses.
- **Merchant** — winning this lane yields a small extra gold bounty; shop rerolls slightly cheaper while it lives. *(Stacked on the gold lane, this is the all-in economy build — legal, greedy, and a huge target.)*
- **Scavenger** — when any of *your* lanes' units die, this lane collects a small resource bounty; thrives in attrition.

Kit-per-lane (not per-player) is what makes the choice spatial: *which lane gets which identity* is itself an allocation bet made blind, before you've seen the enemy. Balance burden is kits × lanes, so launch small and add later.

## Sequential Fights
Lanes resolve **one at a time**, in a fixed order ending on the gold lane, ~15 seconds each. Non-negotiable, for three reasons: every fight is actually watchable; the round acquires a narrative arc (and always climaxes on the richest prize); and sequencing leaves design space for later mechanics that react to earlier lane results (a rally buff after losing lane 1, emerald powers triggered mid-round). **Four fights ≈ 60 seconds of resolution per round — this is at the ceiling.** Keep individual fights short (small lanes help); if rounds drag in playtests, speed up resolution before touching anything else. Combat itself is standard auto-battler fare: HP/attack/speed/range/mana, abilities on full mana, nearest-target default — deliberately conventional, because this game's identity lives *between* the boards, not inside them.

## Death Spiral Audit (design invariant)
The multi-front structure doubles down on losses, so every valve below is load-bearing — check them after any rules change:
1. **The gold floor** — unconditional base income; the broke player always keeps buying.
2. **Consolidation** — lane death returns survivors for redeployment.
3. **Uncontested lanes deal no damage** — pressure is economic only.
4. **Wither is margin-based** — conceding thin is cheap; only massacres are expensive, so tactical concession stays a *strategy* rather than a spiral.
5. **The gold lane is a bounty, not a lifeline** — losing it must never read as "game over."
A player down to two lanes with a consolidated army and the floor income should be dangerous, not dead. If playtests show lane loss = match loss, these valves are mistuned.

## What This Game Is Not
- **Not BLINDSIDE.** No hidden stats, no deduction UI, no scouting economy. The only fog is commitment delay.
- **Not four TFTs.** Tiny lanes, one shop, one army. If a playtest feels like managing four games, shrink lane size or slow the round timer — the complexity budget is the identity.

## Scope & Build Order
1. **Core slice:** one lane, shared shop, gold floor + interest, standard combat — i.e., a minimal single-board autobattler that works.
2. **The partition:** four lanes, unit assignment, transfers + iron, sequential resolution, lane HP + wither + consolidation. *This is the milestone where the game's thesis is testable — playtest hard here.*
3. **The fog:** blind planning + simultaneous reveal; the Walls phase.
4. **Resources & kits:** diamond/emerald/gold-lane sinks, the kit pool.
5. **Polish:** bed-break moments, walls-drop reveal drama, bot AI that allocates legibly.

## Open Tuning Questions
- Lane size (3 slots assumed) and total army cap.
- Transfer cost/cap — the central dial.
- Wither threshold and margin scaling.
- Gold-lane bounty size (big enough to fight over, small enough not to collapse allocation) and its bed's extra HP.
- Emerald power budget (start weak).
- Walls phase length (5 rounds assumed) and PvE difficulty.
- Round length — four sequential fights sit at the watchability ceiling; tune fight speed first.
- Whether later lanes may react to earlier results in-round (defer; sequencing allows it).
