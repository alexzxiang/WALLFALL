# WALLFALL — Systems & UI Design Spec (v0.1)

*Companion to `WALLFALL_design.md`. That doc is the thesis; this doc is the buildable spec: concrete numbers for every system, plus the complete UI/UX design mapped to the itch.io assets in `Assets/itchio`. All numbers are starting points for playtest tuning — the tuning dials from the design doc still rule.*

---

# PART I — SYSTEMS

## 1. Match Timeline

| Phase | Rounds | Planning | Fights | Notes |
|---|---|---|---|---|
| **The Walls** | 1–5 | 45s (round 1: 60s) | 4 PvE waves, resolved simultaneously (~10s) | Opponent fully hidden. Kits picked round 1. |
| **Walls Drop** | start of 6 | — | — | 5s reveal cinematic, both boards shown |
| **The War** | 6+ | 30s | 4 sequential fights × ~15s, gold lane last | Blind planning → reveal (3s) → fights → resolve |

- Round cadence in The War: **Income → Planning (blind) → Reveal → Fights (iron → diamond → emerald → gold) → Resolve** ≈ 100–110s per round.
- Expected match length: 14–20 rounds ≈ 22–30 min. If playtests drag: speed fights up first (design doc rule), then trim planning to 25s.
- Fight hard cap 15s; at cap, the fight is a **draw**: no lane damage either way, and the lane's resource bounty **rolls over** into next round's pot (a lane worth double is a drama magnet, not a whimper).

## 2. Economy

### Gold (the floor + interest)
- **Floor:** +4 gold every round, unconditional, forever. Never modified by anything.
- **Interest:** +1 per 10 banked gold, capped at +5.
- **Gold lane win:** +5 gold bounty, and XP purchases cost −1 this round (the "soft perk").
- No streaks. Ever. (Design invariant.)

### The four currencies

| Currency | Win bounty | Trickle | Uncontested auto-bank | Sinks (exclusive) |
|---|---|---|---|---|
| **Iron** | 3 | +1/round while lane 1 lives | 3/round | Transfers (2i), consumables (2–4i) |
| **Diamond** | 2 | — | 2/round | Items (3d), Diamond Duplicate (5d) |
| **Emerald** | 1 | — | 1/round | Powers (1–3e) |
| **Gold bonus** | +5g, cheap XP | — | 5g/round | (feeds normal gold sinks) |

Nothing converts to anything else. Draws roll bounties over (see above).

### Iron sinks (logistics & consumables)
| Item | Cost | Effect |
|---|---|---|
| **Transfer** | 2 iron | Move one unit between your lanes at planning. **Cap: 2 transfers/round** (the central dial). |
| **Bed Plating** | 4 iron | +2 lane HP, permanent, cannot exceed starting max. |
| **War Horn** | 3 iron | One lane: +15% attack speed this round only. |
| **Field Rations** | 2 iron | One lane: units start the fight with +20% HP this round. |

### Diamond sinks (items & permanence)
- **Items cost 3 diamonds**, completed on purchase (no TFT component algebra — complexity budget). A unit holds **1 item** (Forgemaster lane: 2). Items are permanent and transfer with the unit.
- Launch catalog (8): Whetstone Blade (+35% AD) · Focus Crystal (+35% AP) · Tower Plate (+30% HP) · Swift Boots (+20% AS) · Vampire Fang (20% lifesteal) · Mana Spring (+1 mana/attack) · Thorn Shell (reflect 15%) · Sniper Scope (+1 range).
- **Diamond Duplicate** — 5 diamonds: add one copy of a 1★ unit you own (cost ≤3). The diamond lane's late-game identity: buy star-ups directly.

### Emerald sinks (match-benders — start weak, per design doc)
| Power | Cost | Effect |
|---|---|---|
| **Bed Repair** | 2 | +5 HP to a living lane (cannot revive dead lanes). |
| **Rally** | 1 | After your next lost fight this round, remaining lanes get +12% stats for the rest of the round. (Uses the sequential-fight order.) |
| **Supply Drop** | 1 | 2 free transfers this planning phase (doesn't count against cap). |
| **Overgrowth** | 2 | One lane's units get +25% HP this round. |
| **Wallback** | 3 | One of your lanes cannot take lane damage this round (fight still happens; resource still contested). Max once per match. |

## 3. Shared Shop, Leveling, Rerolls

- **One shop, 5 slots**, refreshes each round, reroll **2 gold**. Buy a unit → it goes to the **bench** → assign to a lane (free while on bench; moving lane→lane is a Transfer).
- **Bench: 6 slots.** Bench units don't fight and don't count toward lane slots.
- **XP:** 4 gold → 4 XP (gold-lane win round: 3 gold). Auto +2 XP per round.

| Level | Total XP | Unit cap per lane | Army max | 1c / 2c / 3c / 4c / 5c odds |
|---|---|---|---|---|
| 2 | 2 | 2 | 8 | 75 / 25 / 0 / 0 / 0 |
| 3 | 6 | 2 | 8 | 60 / 35 / 5 / 0 / 0 |
| 4 | 12 | 2 | 8 | 45 / 38 / 15 / 2 / 0 |
| 5 | 22 | **3** | 12 | 30 / 40 / 25 / 5 / 0 |
| 6 | 36 | 3 | 12 | 20 / 33 / 33 / 12 / 2 |
| 7 | 52 | 3 | 12 | 14 / 25 / 35 / 20 / 6 |
| 8 | 76 | **4** | 16 | 10 / 18 / 30 / 30 / 12 |

- The unit cap is per lane and uniform across lanes (no per-lane leveling — complexity budget). Units may stand on any of the 15 hexes of your half; the cap limits bodies, not positions.
- **Star-up:** 3 copies → 2★ (+80% stats), 9 → 3★ (+80% again). 3★ is a real chase but rare in 1v1 pool sizes.
- **Pool (1v1 sizing):** 1-cost ×10 copies, 2c ×8, 3c ×6, 4c ×4, 5c ×3. Sell-back returns copies to pool at full copy count, refunds cost (1★) as usual.

## 4. Units — Launch Roster (24) & Bonds

**Bonds replace TFT traits.** Every unit has exactly one Bond. A Bond activates when **2+ units sharing it stand in the same lane** — synergy is spatial and lane-local, so partition decisions are synergy decisions. With 2–4 slots per lane, Bonds are pairs by design: readable at a glance.

| Bond | 2-piece effect (lane-local) |
|---|---|
| **Shieldline** | Front row +25% HP |
| **Wildpack** | +15% AS, +10% move speed |
| **Hexbound** | Abilities +25% power |
| **Longshot** | Back row +20% AD |
| **Gravewalk** | First ally death: others gain +20% AS |
| **Forgeborn** | Item holders +15% all stats |
| **Venom** | Attacks apply 3% max-HP poison/s |
| **Royals** | +1 mana per attack, +10% ability power |

Roster mapped to owned sprite packs (side-view, animated — Idle/Run/Attack/Hit/Death all exist in these packs):

| Cost | Unit | Bond | Role | Sprite source |
|---|---|---|---|---|
| 1 | Recruit | Shieldline | Tank | Tiny RPG Soldier |
| 1 | Goblin | Wildpack | Melee DPS | Monsters_Creatures_Fantasy/Goblin |
| 1 | Mushling | Venom | Melee | Monsters_Creatures_Fantasy/Mushroom |
| 1 | Slime | Gravewalk | Tank | Cute_Fantasy_Free/Slime_Green |
| 1 | Wisp Eye | Longshot | Ranged | Monsters_Creatures_Fantasy/Flying eye |
| 1 | Pink Scamp | Royals | Support | tiny-hero/Pink_Monster |
| 1 | Farmboy | Forgeborn | Bruiser | Mana Seed Farmer |
| 2 | Skeleton | Gravewalk | Melee DPS | Monsters_Creatures_Fantasy/Skeleton |
| 2 | Orc | Wildpack | Bruiser | Tiny RPG Orc |
| 2 | Owlet Sage | Hexbound | Caster | tiny-hero/Owlet_Monster |
| 2 | Dude Brawler | Shieldline | Tank | tiny-hero/Dude_Monster |
| 2 | Archer | Longshot | Ranged | Tiny RPG Soldier recolor + Arrow projectile |
| 2 | Bone Hound | Venom | Melee | Cute_Fantasy/Skeleton |
| 3 | Demon | Hexbound | Caster DPS | Tiny RPG Demon_A |
| 3 | Blood Ooze | Venom | Tank | Tiny RPG Blood Monster_A |
| 3 | Knight-Captain | Shieldline | Tank | Pixel Crawler pack |
| 3 | Dungeon Mage | Royals | Caster | 2D Pixel Dungeon pack |
| 3 | Ranger | Longshot | Ranged carry | Pixel Crawler pack |
| 3 | Grave Priest | Gravewalk | Support | 2D Pixel Dungeon pack |
| 4 | Wyvern | Wildpack | Assassin | Free Mythic Monsters |
| 4 | Frost Colossus | Shieldline | Super-tank | Free Mythic Monsters |
| 4 | Warlock | Hexbound | AoE caster | Free Mythic Monsters |
| 4 | Royal Guard | Royals | Bruiser | Pixel Crawler / Portraits pack |
| 5 | **The Dragon** | (all Bonds count it as a partner) | Flex carry | Free Mythic Monsters + dragon skill icons |

*(Exact pack-to-unit fit gets adjusted when sprites are imported; the shape — 7/6/6/4/1 across costs 1–5 — is the spec.)*

**Abilities:** every unit has 1 ability cast on full mana (standard autobattler). 1-costs get stat-simple abilities (a shield, a stab); 4–5 costs get board-benders (AoE, stun). The `28_Pixel Art_Skill` icon set covers ability icons (ice, poison, dragon, etc.).

## 5. Combat Spec (deliberately conventional)

- Stats: HP / AD / AP / Attack Speed / Range / Armor / Mana. Mana fills on attack (+10) and on damage taken (+1 per 2% HP lost). Full mana → cast, mana to 0.
- Targeting: nearest enemy by hex distance (no targeting UI — identity lives between boards, not inside them).
- **Lane geometry: each lane is a real hex board, TFT-style.** Your half is a **3-row × 5-column** pointy-top hex grid (15 hexes); the enemy half mirrors it across a center seam — a full 6×5 lane board. Positioning is genuine: corner-bunking a carry, front-line walls, flank assassins all work per-lane. Four lanes × 15 hexes keeps the complexity budget: each board is small enough to read in a second, and unit caps (2–4 per lane) keep placement decisions sharp rather than exhaustive.
- Units pathfind hex-to-hex toward nearest target, attack when in range. Movement/attack cadence standard autobattler.
- Fight ends on wipe or 15s (draw rules above).

## 6. Lane HP, Wither, Consolidation

- **Lane HP:** 15. **Gold lane: 25** (the tankier bed, per design doc).
- **Lane damage on loss:** `2 + (surviving enemy units)` → 3–6 typical, up to 2+4=6 at level 8 lanes.
- **Wither:** if you were **wiped** and the enemy kept **3+ units alive**, +3 bonus damage. A close loss costs ~3; a massacre costs ~9. Conceding thin is a strategy; getting crushed is a wound.
- **Lane death at 0 HP:** bed breaks (big moment — see UI Part), lane stops producing forever, survivors return to bench at next planning as **free consolidation moves** (they don't consume transfers or iron, must be placed that planning phase, bench overflow allowed for that phase only).
- **Uncontested lanes:** deal no lane damage; auto-bank their resource for the owner. Owner may keep units parked there (safe income) or pull them out (normal transfer rules).
- **Loss condition:** all four lanes dead. A 2-lane player with a consolidated army and floor income must still be dangerous — death-spiral audit applies to every tuning pass.

## 7. Kits (picked one per lane in round 1, hidden until walls drop)

| Kit | Effect (utility identity, never a stat stick) |
|---|---|
| **Forgemaster** | Units in this lane hold 2 items; items for this lane cost 2 diamonds. |
| **Quartermaster** | Transfers in/out cost 1 iron; +1 iron trickle/round. |
| **Warlord** | Enemy wither threshold vs this lane: 2+ survivors instead of 3+; +1 gold on winning this lane. |
| **Architect** | +5 lane HP; Bed Plating costs 2 iron here; close losses (enemy ≤1 survivor) deal −1 damage. |
| **Merchant** | Winning this lane: +2 bonus gold; while it lives, rerolls cost 1. *(On the gold lane = the greedy all-in build; legal, and a huge target.)* |
| **Scavenger** | Any of your units dying anywhere pays this lane's bank +1 iron per 3 deaths (rounds down, paid at resolve). |

No duplicate kits across your four lanes. Six kits at launch; the balance surface is kits × lanes, so grow slowly.

## 8. Walls Phase PvE

- Rounds 1–5: each lane fights a small creep wave simultaneously (they're PvE; watchability rule applies to PvP only). Creeps use the Cute Fantasy / dungeon enemy sprites.
- Creep drops: R1: 2 iron · R2: 1 diamond · R3: 4 gold · R4: 1 emerald · R5: **choice of any one** (2i / 1d / 1e / 4g) per lane won.
- Losing a PvE wave costs no lane HP in R1–2, 1 HP in R3–5 (teaches the stakes gently).
- Round 1 planning (60s): pick all four kits, assign starting units (you start with 4 gold + a free 1-cost unit per lane).
- **Walls drop at round 6**: full-screen reveal moment (see UI).

---

# PART II — UI / UX DESIGN

## 9. Art Direction

**Pitch: "a plush war-room."** Cute pixel armies fighting a deadly-serious allocation game. Everything readable in one glance from across the room; everything soft, chunky, and tactile up close. TFT's information hierarchy in a 16-bit skin.

- **Style:** clean 16-bit pixel art. Chunky 2–4px UI borders, square corners (or 2-step "pixel-rounded" corners), zero gradients-as-decoration, hard drop shadows (2–4px, 0 blur). `image-rendering: pixelated` everywhere; all UI on a consistent virtual pixel grid (render at 640×360 or 960×540 logical, integer-scale up).
- **The one rule:** *game state is never conveyed by color alone.* Every resource has a unique gem silhouette (the Pixel UI pack's diamonds/gems are already shape-distinct), every lane has an icon + biome, wither/damage always pairs color with an icon and a number.

### 9.1 Palette (derived from Sprout Lands pastels + Pixel UI pack)

| Token | Hex | Use |
|---|---|---|
| `bg-deep` | `#1A1826` | Outer background, fight-screen vignette |
| `bg-plum` | `#2A2438` | Screen background, dead-lane overlay |
| `panel-cream` | `#F4E9D0` | Panels, shop cards, tooltips |
| `panel-wood` | `#8B5E3C` | Panel borders, frames (Pixel UI pack wood buttons) |
| `ink` | `#3A3048` | Text on cream |
| `paper-text` | `#F4E9D0` | Text on plum |
| `iron` | `#B8C0CC` | Iron currency, lane 1 accents |
| `diamond` | `#6EE7F0` | Diamond currency, lane 2 accents |
| `emerald` | `#6EDB78` | Emerald currency, lane 3 accents |
| `gold` | `#FFD447` | Gold currency, lane 4 accents, CTA buttons |
| `hp-red` | `#E85D5D` | Lane HP hearts, damage numbers |
| `xp-blue` | `#5D9EE8` | XP bar, level |
| `accent-pink` | `#F27EA9` | Star-ups, celebrations, "cute" moments |
| `danger` | `#C93A3A` | Wither warnings, destructive confirms |
| `frozen` | `#7A8BA8` @ 60% desat | "Last seen" enemy-board tint |

Contrast: `ink` on `panel-cream` = 9.8:1; `paper-text` on `bg-plum` = 11:1; `gold` on `bg-plum` = 9:1. All currency colors get dark 1px outlines so they hold on any biome background.

### 9.2 Typography

- **Display / logo:** `Planes_ValMore.ttf` (owned, in `free-pixel-art-tiny-hero-sprites/Font`) — round names, "WALLS DROP", victory screens.
- **UI body & numbers:** a readable pixel font at native size — recommend **m5x7 / m3x6** (Daniel Linssen, free) or **Pixel Operator**; numbers must be tabular so HP/gold don't jitter.
- Minimum text size = 1 font native size at integer scale (never fractional scaling — blurry pixels are the cardinal sin).
- Damage numbers: display font, colored by type, 1px black outline.

### 9.3 Asset map (what each itch.io pack is for)

| Pack | Role |
|---|---|
| **Pixel UI pack 3** | The HUD backbone: buttons, hearts (lane HP), stars (unit stars), progress bars (mana/HP/timer), **the 4 gem sprites = the 4 currencies**, frames, badges (kit emblems base) |
| **28_Pixel Art_Skill** | Ability icons + emerald power icons (ice/poison/dragon sets) |
| **Cursors_v2** | Custom cursor (Light set on dark bg; swap to grab-cursor while dragging units) |
| **Free 39 Portraits** | Kit portraits, player avatars, bot opponents |
| **Tiny RPG (Soldier/Orc/Demon/Blood)** | Core unit sprites (100×100, full anim sets) + arrow projectile |
| **Monsters_Creatures_Fantasy** | Units: goblin/skeleton/mushroom/flying eye (Idle/Run/Attack/Hit/Death ✓) |
| **tiny-hero-sprites** | Units: Pink/Owlet/Dude monsters (the "cute" anchor units) |
| **Free Mythic Monsters** | 4–5 cost units (outlined set for readability) |
| **Cute_Fantasy_Free, Mana Seed Farmer, Sprout Lands** | Units, PvE creeps, props, decoration; Sprout palette = UI pastel reference |
| **2D Pixel Dungeon, Pixel Crawler** | Props: torches, chests, tiles; 3-cost units; **bed = "Heart Crystal"** built from UI-pack gem + dungeon pedestal |
| **Backgrounds** | Per-lane biomes: **Iron = parallax-industrial**, **Diamond = Glacial mountains**, **Emerald = parallax forest**, **Gold = desert/city**; ocean/sky/space for menus |
| **Wenrexa kits (both)** | ⚠️ Sleek vector style — clashes with pixel art. Prototype/greybox only; do not ship in-game. |

## 10. Screen Map

```
Main Menu → Lobby/Matchmaking → MATCH
  MATCH:
   Walls Phase planning (rounds 1–5, enemy side = giant wall)
   → Walls Drop cinematic (round 6)
   → War loop: Planning → Reveal → Fight×4 → Resolve (round summary strip)
   → Bed Break moment (as triggered) / Consolidation planning
   → Victory / Defeat
```

One layout for planning across both phases (the Walls phase just replaces the enemy half with wall art). Fight view is the same strip, zoomed. Minimal screen count, maximum reuse.

## 11. The Planning Screen (the core screen)

**One focused lane board at a time + a persistent lane rail.** Each lane is a full hex board now, so all four can't be on screen at full fidelity — and that's fine: the rail keeps the other three lanes' vitals one glance (and one keypress) away.

Reference layout at 16:9 (design at 960×540 logical, ship at integer scales):

```
┌──────────────────────────────────────────────────────────────────────┐
│ TOP BAR  round 8 · WAR   ⏱ 0:24          🧑opponent    ⛏12 💎4 ✦2 🪙23 │ 56px
├──────┬───────────────────────────────────────────────────────────────┤
│ LANE │            ENEMY HALF (frozen · LAST SEEN R7)                 │
│ RAIL │         ⬡ ⬡ ⬡ ⬡ ⬡   ← 3 rows × 5 hexes, scanline tint        │
│ [L1] │         ⬡ ⬡ ⬡ ⬡ ⬡      their bed ♥9 top-right               │
│ [L2]*│  ═══════ center seam · lane bounty ⛏3 ═══════                 │
│ [L3] │         ⬡ ⬡ ⬡ ⬡ ⬡      YOUR HALF                             │
│ [L4] │         ⬡ ⬡ ⬡ ⬡ ⬡      your bed ♥12 bottom-left             │
├──────┴───────────────────────────────────────────────────────────────┤
│ BENCH ▫▫▫▫▫▫   transfers 1/2 ⛏ │ SHOP [c][c][c][c][c] 🔄2g  XP 4g  L6│ ~120px
└──────────────────────────────────────────────────────────────────────┘
```

**The focused board (the heart of it):**
- TFT orientation: your 3×5 hex half at the bottom, the enemy's frozen half at the top, beds anchored at opposite corners. The lane's **biome backdrop** (muted ~40% so sprites pop) fills behind the board.
- Hexes: pointy-top, chunky pixel outlines; empty = dashed outline, hover = bright, drag-over = snap highlight with a 1-frame "thunk." Unit cap per lane (2/3/4 by level) shown as a pip row on the seam — placement freedom is the full 15 hexes, the cap is on bodies, not positions.
- **Enemy frozen half:** desaturated `frozen` tint + "LAST SEEN · R7" tag + scanline shimmer. Must read as stale at a glance — it's the game's only fog. On reveal, color floods back in a 12-frame pixel-dissolve.
- **Switching lanes:** keys 1–4, rail click, or Q/E cycling. Switch is a 150ms slide in lane order (spatial continuity — lanes have a fixed geography).

**The lane rail (left edge, always visible):**
- Four mini lane cards, top-to-bottom L1→L4 (gold last, biggest). Each shows: resource gem + bounty (rollover pots pulse), bed HP heart-strip both sides, unit-count pips, kit emblem, and a change-marker dot when the opponent's last-seen state differs from two rounds ago.
- **Dead lane card:** cracked, dimmed, "UNCONTESTED — banking ⛏3/rd." Still clickable — a dead lane is information, not absence.
- During fights the rail doubles as the result rail (✔/✘/pending per lane).
- A **quad-view toggle (Tab)** shows all four boards zoomed out, read-only, for partition-level thinking; any click dives back into that lane.

**Bottom bar (pure TFT muscle memory):**
- Bench (6 sockets) left; **transfer counter** ("1/2 ⛏") beside it — the central dial deserves permanent HUD presence.
- Shop: 5 cards. Card = cream panel, unit sprite (idle anim plays on hover), name, Bond icon + name, cost in gold gem. Owned-elsewhere copies get a pink "▲2/3" star-progress pip (TFT's best invention — keep it).
- Right cluster: Reroll (🔄 2g), Buy XP (4g), level + XP pixel progress bar, shop odds row (tap/hover to expand).
- Buying: click card → unit hops to bench with a squash-and-stretch land. Assigning: drag bench → socket. **Transfers:** drag a unit from lane A to lane B → an iron cost chip (⛏2) rides the cursor; on drop, chip flies to the wallet and the counter ticks. If out of transfers/iron: socket flashes `danger`, "no transfers left" toast.
- Selling: drag to shop area (turns into a gold "sell for 2g" trough) — standard TFT gesture.

**Top bar:** round + phase name, planning countdown (turns `danger` and ticks audibly at 5s), opponent portrait, wallet (four gem counters, always in the same order — iron/diamond/emerald/gold — matching lane order top-to-bottom).

**Kit & power surfaces:**
- Kit emblem sits on each lane nameplate; hover → tooltip with exact kit text.
- Emerald powers live in a compact **spellbook button** next to the wallet (badge shows affordable count). Opens a 5-card modal; casting targets a lane by clicking its strip (strips glow emerald during targeting).
- Iron consumables: right-click (or long-press) a lane nameplate → small radial of 3 consumables with iron costs.

**Blind-planning honesty:** during planning, *your own* uncommitted changes render at 80% opacity with a small "hidden until reveal 👁" watermark on newly placed units — teaching that the opponent can't see them yet. At reveal, everything snaps to full opacity on both sides simultaneously.

## 12. The Fight Screen

- The active lane's **hex board fills the screen** (shared-element transition from its rail card, 200ms, ease-out) with its full-brightness biome; the other three lanes live as **result chips** on the rail: `L1 ⛏ ✔ +3` / `L2 💎 …` (pending = grey, current = pulsing).
- Fight order rail (iron→diamond→emerald→gold) always visible; the gold chip is drawn bigger — the round's climax is legible from the first second.
- Units: your side enters left with a 200ms stagger run-in (30ms/unit), enemy right. HP bars (2px, colored by side) + mana bars (1px, blue) above heads; damage numbers pop with 1px outlines; ability casts flash the skill icon briefly above the caster.
- **Speed controls:** 1×/2× toggle; both players' fights are identical playback so no advantage.
- **Resolve banner** per fight: "LANE 2 — VICTORY · 💎2" slams in with 2-frame screen shake; **wither losses** get a distinct heavy purple "WITHERED −9♥" stamp and 4-frame shake (players must *feel* the difference between losing and getting crushed).
- After lane 4: round summary strip (4 chips + income line: `+4 floor · +2 interest · +5 gold lane`) for 4s, then back to planning.

## 13. Signature Moments (the drama budget)

1. **The Walls (rounds 1–5):** the enemy half of every strip is one continuous wall sprite (Pixel Dungeon bricks, torch-lit). It looms. Round counter says "WALLS DROP IN 2…"
2. **Walls Drop (round 6):** 2s rumble + dust, wall crumbles bottom-up in a pixel-shatter, revealing all four enemy boards + kit emblems at once. Single loudest audio+visual beat in the match. (This is the game's title. Spend here.)
3. **Reveal (every War round):** both sides' ghosted changes pixel-dissolve to solid simultaneously — a mini walls-drop every ~100 seconds. 400ms, never skippable, always the same sting sound (the yomi heartbeat).
4. **Bed Break:** Heart Crystal shatters (8-frame), lane floods plum, hearts scatter as pixel particles, "BED DESTROYED" in display font; next planning opens with survivors hopping onto the bench under a green "CONSOLIDATE — free moves" banner. The comeback mechanic gets celebration treatment, not funeral treatment — it should feel like a rally.
5. **Gold-lane finale:** gold lane fights get golden vignette edges + the biggest resolve banner. When a match-point lane fight starts (someone can die this round), the timer bar turns `danger` and the music drops to heartbeat.

## 14. Motion & Feel Rules

- UI transitions 150–250ms, ease-out in / ease-in out; **sprite work is frame-by-frame** (8–12 fps) — never tween a sprite's scale smoothly, it breaks the pixel grid. Squash-and-stretch via swapped frames.
- Screen shake budget: 2 frames (hits) / 4 frames (wither, bed break) / 8 frames (walls drop). Never during planning.
- Hover states on everything clickable (1px lift + brighten); pressed = 1px sink. Cursor swaps (Cursors_v2) for default/hover/drag.
- Every animation interruptible; input never blocked. Reduced-motion setting: no shake, dissolves become cuts, parallax stills.
- Tooltips: 150ms delay on hover, instant on click/tap; every icon in the game has one (gems, kits, Bonds, powers, wither, frozen-tag).

## 15. UX Checklist (applied, not aspirational)

- [ ] Color-never-alone: currencies = unique gem shapes; lanes = icon + name + biome; frozen = tint + ⏱ tag + scanlines; wither = color + icon + word.
- [ ] Contrast ≥4.5:1 for all text (verified for palette above); currency icons ≥3:1 with outlines on all biomes.
- [ ] All interactive targets ≥ 40×40 logical px (sockets are 48×48); drag has an 8px threshold before starting.
- [ ] Tabular numbers for HP/gold/timer — zero layout jitter.
- [ ] Keyboard: R = reroll, X = buy XP, 1–4 = focus lane, E = spellbook, Esc = close modal; full planning phase playable mouse-only or keyboard+mouse.
- [ ] Timer urgency is visual + audio, and planning auto-commits current state (never punishes with a null turn).
- [ ] No emoji as icons — the mockup glyphs above are placeholders for the Pixel UI pack sprites.
- [ ] Integer scaling only; letterbox with `bg-deep` on odd resolutions.
- [ ] Colorblind sim pass (deuteranopia) on the four currency colors each art review.

## 16. Build Order Hooks (UI slices matching the design doc's milestones)

1. **Core slice:** one lane strip + shop bar + wallet — the whole bottom-bar UX ships here.
2. **The partition:** 4 strips, drag-transfer flow + iron chips, sequential fight zoom + result rail.
3. **The fog:** ghost-until-reveal rendering, frozen tint, reveal dissolve; Walls wall art + drop cinematic.
4. **Resources & kits:** spellbook, consumable radial, kit emblems + pick screen (round 1: four portrait cards per lane, drag kit → lane).
5. **Polish:** bed break, wither stamps, gold-lane vignette, music layers, bot avatars.
