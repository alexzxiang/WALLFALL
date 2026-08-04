# WALLFALL Unit Compendium — Set 1: "The Four Fronts"

*The full 60-unit roster: 13/13/13/13/8 across costs 1–5. This document supersedes the 24-unit
prototype roster in the current build (see §7 for migration notes). Numbers are 1★ values;
each star multiplies unit stats ×1.8. Balancing takes TFT as its baseline, then bends around
WALLFALL's two unique facts: traits activate PER LANE, and there are four currencies to play with.*

---

## 1. Set Design Pillars

**Lane-local traits, small breakpoints.** A lane board holds `level` units (2–8). So breakpoints
are **2/4/6** (a few utility traits use 2/3, Gilded climbs 2/3/4/5). A 6-piece is a deliberate
"this lane is my identity" commitment — one board, most of your cap, telegraphed to the enemy at
reveal. That's the WALLFALL version of TFT's vertical flex: verticals are *spatial*.

**The currency web.** TFT has one resource; WALLFALL has four, each with an exclusive sink.
Traits touch them in exactly three ways, and every economy trait picks ONE:
- **Generators** make small, capped amounts of a currency (Foundry → iron, Prospect → diamonds).
- **Scalers** read a currency you're holding and convert it to combat stats without spending it
  (Gilded reads banked gold, Dragonsoul reads emeralds). Hoarding becomes a build.
- **Amplifiers** make a currency's *sink* better (Sylvan improves emerald powers, Caravan
  converts iron income into permanent stats). Spending becomes a build.

**Attunement (the regional layer).** Each economy origin has a home lane — Foundry↔L1 Iron,
Prospect↔L2 Diamond, Sylvan↔L3 Emerald, Gilded↔L4 Gold. Fielding the origin on its home lane adds
a small rider (listed per trait). This creates map texture — "Foundry belongs on iron" — while
staying legal anywhere, so the Blotto game (play the origin OFF-lane as a bluff) stays alive.

**Front/back discipline.** Classes split cleanly: **Bulwark / Juggernaut** are frontline,
**Sniper / Arcanist / Gunner** are backline, **Duelist / Assassin** are divers, **Herald** is
utility glue. Every unit is one origin + one class (signature units add a third).

**Balance guardrails (hard caps, tune here first):**

| Guardrail | Cap |
|---|---|
| Bonus iron from traits | ≤ 4/round total |
| Bonus diamonds from traits | ≤ 1/round total |
| Bonus emeralds from traits | ≤ 1/round total (currently only via Motherlode) |
| Bonus gold from traits | ≤ 6/round total |
| Currency-scaling stat bonuses | every scaler lists a hard cap |
| Ruinborn/comeback scaling | caps at 3 dead lanes |
| Echo units (Omnipresent) | contribute no traits, count as survivors only for wither checks |

---

## 2. Origins (8) — economy & region

**⛏ FOUNDRY** — *iron generator, frontline-leaning* — (2) / (4) / (6)
Foundry units gain **+15 / +35 / +60 Armor**. At fight end, if a Foundry unit survived, **bank
+1 / +2 / +3 iron** (per lane, once per round). *Attuned L1: survivors bank +1 extra iron.*
> The iron engine. Pairs with Caravan (which spends the identity) and with heavy consumable use.

**◆ PROSPECT** — *diamond generator, caster-leaning* — (2) / (4)
Prospectors gain **+15% / +35% ability power**. Once per fight, the first Prospector cast has a
**30% / 60% chance to mine +1 diamond**. *Attuned L2: +20% mine chance.*
> Diamonds are the scarcest currency (items are permanent), so generation is one die roll per
> fight, hard-capped by the guardrail. 4-Prospect is an item-economy engine.

**🌿 SYLVAN** — *emerald amplifier, sustain* — (2) / (4) / (6)
Sylvans regenerate **1.5% / 3% / 5% max HP per second**. Any **emerald power cast on this lane**
also grants Sylvans **+15% AD & AP** this round. *Attuned L3: lane-targeted emerald powers cost
−1 (min 1) when this lane holds 4+ Sylvans.*
> The spender's origin: makes Overgrowth/Frenzy double-dip. 6-Sylvan on the emerald lane is the
> self-feeding forest — wins emeralds to cast emeralds.

**🪙 GILDED** — *gold scaler, greed* — (2) / (3) / (4) / (5)
Winning a Gilded lane's fight pays **+1 / +2 / +3 / +5 gold**. Gilded units deal **+1% damage per
2 gold banked**, capped at **+15% / +20% / +28% / +40%**. *Attuned L4: the win bounty also grants
+1 XP.*
> TFT-Fortune energy tuned for a game where the gold lane is already the richest prize — Gilded
> on L4 makes the map's hottest fight even hotter, which is exactly the Blotto bait we want.

**🧱 WALLGUARD** — *bed defense, regional-defensive* — (2) / (4) / (6)
Wallguards gain **+10% / +22% / +40% max HP**. When this lane LOSES a fight, the bed takes
**−2 / −4 / −8 damage** (applies before Wither; a loss always deals at least 1).
> The turtle origin. No currency hook — its "currency" is bed HP, WALLFALL's fifth resource.
> Natural home of the Architect kit and the Wallback power.

**🔥 RUINBORN** — *comeback, scaling with destruction* — (2) / (4)
Ruinborn gain **+14% / +30% AD & ability power per destroyed lane** on the map (yours or the
enemy's, cap 3).
> Mechanized bedless-rush flavor: the board decaying literally feeds them. Deliberately the only
> trait that gets stronger as you lose territory — the consolidation army's backbone.

**⚡ STORMCALLER** — *tempo, sequence-aware* — (2) / (4)
Every 3 seconds of combat, Stormcallers gain **+12% / +25% attack speed** (stacking). They start
the fight with **1 pre-stacked bonus per fight already resolved this round**.
> Uses WALLFALL's sequential fights: Stormcallers on the gold lane (fights last) start hot.
> The overtime ramp makes their late-fight spike real.

**🐪 CARAVAN** — *iron spender/scaler, permanent growth* — (2) / (4)
Caravans start fights with a **shield equal to 20% / 40% max HP**. Whenever you **gain iron**,
each Caravan permanently gains **+1 AD** (cap +30 / +60).
> Foundry's mirror: Foundry makes iron, Caravan eats it into forever-stats. The two together are
> the "iron kingdom" comp — and a reason to fight for L1 all game.

---

## 3. Classes (8) — combat roles

| Class | BP | Effect | Role |
|---|---|---|---|
| **🛡 BULWARK** | 2/4/6 | +250 / +550 / +900 bonus HP; at fight start, taunt adjacent enemies for 3s | Frontline anchor |
| **🪓 JUGGERNAUT** | 2/4 | 12% / 25% omnivamp; +20 / +40 Armor while above half HP | Frontline bruiser |
| **⚔ DUELIST** | 2/4/6 | Attacks grant +5% AS, stacking up to 8 / 12 / 16 stacks | Sustained melee carry |
| **🗡 ASSASSIN** | 2/4 | Leap to the enemy backline at fight start; +20% / +45% crit chance (crits ×1.4) | Backline diver |
| **🎯 SNIPER** | 2/4 | +1 range; +8% / +18% damage per hex to target (max 3 hexes) | Long-range carry |
| **🔮 ARCANIST** | 2/4/6 | Lane allies gain +15% / +30% / +50% AP; Arcanists gain double | Magic battery |
| **📯 HERALD** | 2/3 | Heralds get +20 starting mana; their first cast also shields the lowest-HP ally for 25% / 45% of the Herald's max HP | Utility glue |
| **💥 GUNNER** | 2/4 | Every 4th attack deals +80% / +160% AD bonus physical damage | Attack-reset carry |

Frontline traits: Bulwark, Juggernaut. Backline: Sniper, Arcanist, Gunner. Divers: Duelist,
Assassin. Utility: Herald. Every lane wants roughly 1 frontliner per 2 backliners — the hex
halves are only 3 rows deep, so taunt and Assassin leaps matter.

---

## 4. Signature Traits (uniques)

| Signature | Holder(s) | Effect |
|---|---|---|
| **💞 HEARTBOUND** | Lyra (2c) + Bram (4c) | While both are on the same lane and alive: they split damage taken and gain +25% all stats. Once per fight, when one dies, the other revives them at 40% HP after 2s. *(Lucian/Senna-style pair — the shop pities the missing half: if you own one, the other's shop odds triple.)* |
| **🔨 BREACHER** | Wallbreaker (5c) | If this lane's fight is won with him alive: +6 bonus bed damage. Enemies he personally kills add +1 bed damage each (max +3). *The only unit that attacks the macro-game directly.* |
| **👑 GOLDEN TOLL** | Aurelia (5c) | +1 AD per 5 gold banked (cap +40). Her kills mint +1 gold (max 3/round). *Greed made flesh — she IS your interest.* |
| **🐉 DRAGONSOUL** | Verdanth (5c) | Counts as **3 Sylvan**. +8% ability power per emerald you hold — unspent — up to +40%. *Shyvana-style trait weight plus a hoarder's dilemma: cast your emeralds or feed the dragon.* |
| **⛰ MOTHERLODE** | Karst (5c) | Counts as Foundry **and** Prospect. First death each round: erupts for 250% AD in 2 hexes and banks +1 iron & +1 diamond. *A walking economy that pays out when it breaks.* |
| **👥 OMNIPRESENT** | Mirrormarch (5c) | At fight start, a spectral echo (35% stats, no traits, no bed-damage credit) joins **every other living allied lane's** fight this round. *Only possible in a four-front game — the one unit that fights everywhere.* |
| **🌀 PERFECT STORM** | Vessa (5c) | +20% AP and +10% AS per fight already resolved this round (0–3). *The gold-lane finisher: last fight of the round, she's a hurricane.* |
| **🧱 LIVING WALL** | Mortar (5c) | Your bed on his lane cannot take Wither bonus damage. He gains +2 max HP per point of bed HP remaining (snapshot at fight start). *A 5-cost who scales off the thing you're defending.* |
| **🎲 POTLUCK** | Pothound (3c) | +3% all stats per gold sitting in any lane pot. Whenever any fight draws, Pothound banks +1 gold and gains +5 AD permanently. *Draws feed the dog. The only unit that wants stalemates.* |

Non-signature 5-cost (deliberately, like every TFT set): **Gravekeeper** — a plain, huge, honest stat-check.

---

## 5. The Roster

Armor baselines by class: Bulwark/Juggernaut 45, Duelist/Assassin 30, others 20. Mana is
`start/max`. Damage types: ability damage is magic unless marked (phys).

### 1-Costs (13)

| Unit | Traits | HP | AD | AS | Rng | Mana | Ability |
|---|---|---|---|---|---|---|---|
| Smelt | Foundry · Juggernaut | 650 | 50 | .60 | 1 | 0/70 | **Mule Kick** — 180% AD (phys) to target, brief 1-hex knockback |
| Sparks | Foundry · Gunner | 500 | 52 | .70 | 3 | 0/80 | **Rivet Gun** — next 3 attacks deal +40% AD and shred 5 Armor |
| Shard | Prospect · Arcanist | 480 | 40 | .65 | 3 | 10/60 | **Glint** — 190 magic to target; +30 more if it's below half HP |
| Sprout | Sylvan · Herald | 520 | 42 | .65 | 2 | 20/70 | **Sprout** — heal the lowest-HP ally 180 |
| Thorn | Sylvan · Duelist | 560 | 48 | .75 | 1 | 0/90 | **Bramble Swipe** — 170% AD (phys); target bleeds 60 over 3s |
| Filch | Gilded · Assassin | 500 | 50 | .70 | 1 | 0/80 | **Pickpocket** — 160% AD (phys); the first cast each fight taunts nobody but flips a coin: heads +1 gold on win |
| Bellhop | Gilded · Herald | 520 | 40 | .65 | 2 | 20/60 | **Room Service** — shield an ally 200 for 4s |
| Gatepup | Wallguard · Bulwark | 700 | 42 | .60 | 1 | 0/60 | **Guard Stance** — 300 self-shield, taunts target |
| Rampart | Wallguard · Sniper | 470 | 52 | .70 | 4 | 0/90 | **Pot Shot** — 200% AD (phys) to the farthest enemy in range |
| Ember | Ruinborn · Assassin | 490 | 52 | .75 | 1 | 0/85 | **Cinder Step** — blink behind target, 170% AD (phys) |
| Gale | Stormcaller · Duelist | 540 | 46 | .80 | 1 | 0/90 | **Tailwind** — +40% AS for 4s |
| Dune | Caravan · Juggernaut | 640 | 48 | .65 | 1 | 0/75 | **Sandslam** — 180% AD (phys) in a 1-hex cleave |
| Packmule | Caravan · Bulwark | 690 | 40 | .60 | 1 | 0/65 | **Overloaded** — 280 self-shield; +80 per 10 iron you hold (cap +160) |

### 2-Costs (13)

| Unit | Traits | HP | AD | AS | Rng | Mana | Ability |
|---|---|---|---|---|---|---|---|
| Golem | Foundry · Bulwark | 850 | 55 | .55 | 1 | 0/75 | **Molten Core** — 350 shield; nearby enemies take 90 magic over 3s |
| Rivet | Foundry · Duelist | 700 | 58 | .75 | 1 | 0/90 | **Nail Flurry** — 3 rapid hits of 70% AD (phys) each |
| Glimmer | Prospect · Herald | 600 | 48 | .65 | 3 | 20/70 | **Dazzle** — 220 magic; blinds target (misses) for 1.5s |
| Mole | Prospect · Gunner | 620 | 60 | .70 | 3 | 0/85 | **Drill Bolt** — 210% AD (phys), pierces one unit behind |
| Fletch | Sylvan · Sniper | 580 | 62 | .75 | 4 | 0/90 | **Seeker Arrow** — 230% AD (phys) to lowest-HP enemy |
| Moss | Sylvan · Juggernaut | 820 | 58 | .60 | 1 | 0/80 | **Maul** — 200% AD (phys); heals self 50% of damage dealt |
| Scrapper | Gilded · Duelist | 720 | 60 | .80 | 1 | 0/95 | **Crowd Pleaser** — 190% AD (phys); +30% if any pot has gold in it |
| Toll | Gilded · Assassin | 640 | 62 | .75 | 1 | 0/85 | **Shakedown** — 200% AD (phys); marked target takes +10% damage from all sources |
| Brick | Wallguard · Bulwark | 900 | 50 | .55 | 1 | 0/70 | **Hold the Line** — 380 shield split with the ally behind him |
| **Lyra** | Wallguard · Herald · **Heartbound** | 620 | 50 | .70 | 3 | 20/70 | **Lifeline** — heal lowest ally 260; if that ally is Bram, he also gains +20% AS |
| Scavver | Ruinborn · Gunner | 610 | 62 | .70 | 3 | 0/90 | **Bone Shrapnel** — 220% AD (phys) cone behind target |
| Harrier | Stormcaller · Sniper | 590 | 64 | .75 | 4 | 0/95 | **Dive Bolt** — 240% AD (phys); +20% per Stormcaller AS stack held |
| Sirocco | Caravan · Arcanist | 610 | 50 | .65 | 3 | 10/75 | **Mirage** — 240 magic to 2 nearest enemies |

### 3-Costs (13)

| Unit | Traits | HP | AD | AS | Rng | Mana | Ability |
|---|---|---|---|---|---|---|---|
| Anvil | Foundry · Bulwark | 1050 | 65 | .60 | 1 | 0/80 | **Anvil Drop** — 280 magic in 1-hex ring, stuns 1s |
| Slag | Foundry · Arcanist | 750 | 60 | .65 | 3 | 15/80 | **Slag Spray** — 300 magic cone; hit enemies lose 10 Armor |
| Facet | Prospect · Arcanist | 760 | 62 | .65 | 3 | 15/75 | **Refraction** — 320 magic split among up to 3 enemies; single-target if alone |
| Lode | Prospect · Juggernaut | 950 | 70 | .65 | 1 | 0/85 | **Headlamp Rush** — charge the farthest enemy within 3 hexes, 240% AD (phys) |
| Willow | Sylvan · Arcanist | 800 | 58 | .60 | 3 | 20/85 | **Rootgrasp** — 280 magic to 2 enemies, roots them 1.5s |
| Briar | Sylvan · Assassin | 780 | 74 | .80 | 1 | 0/90 | **Thorn Ambush** — 230% AD (phys); resets to 50 mana on kill |
| Gavel | Gilded · Herald | 740 | 55 | .65 | 3 | 25/80 | **Going Once** — shield 2 allies 280 each; +40 per 10 gold banked (cap +120) |
| **Pothound** | Gilded · Juggernaut · **Potluck** | 980 | 72 | .65 | 1 | 0/90 | **Beg** — 480 self-heal; if any pot holds gold, also +25% AD for 5s |
| Murus | Wallguard · Juggernaut | 1000 | 70 | .60 | 1 | 0/85 | **Battering Charge** — 250% AD (phys); +50% vs shielded targets |
| Strix | Wallguard · Sniper | 700 | 76 | .70 | 4 | 0/95 | **Overwatch** — 260% AD (phys); range +1 while within 1 hex of your back row |
| Cinder | Ruinborn · Arcanist | 770 | 62 | .65 | 3 | 15/85 | **Ashfall** — 300 magic in 2-hex line; burns 90 over 3s |
| Tempest | Stormcaller · Duelist | 820 | 74 | .85 | 1 | 0/95 | **Lightning Rounds** — next 4 attacks chain 60 magic to a nearby enemy |
| Convoy | Caravan · Gunner | 750 | 76 | .70 | 3 | 0/90 | **Convoy Volley** — 240% AD (phys); +15% per 10 iron you hold (cap +45%) |

### 4-Costs (13)

| Unit | Traits | HP | AD | AS | Rng | Mana | Ability |
|---|---|---|---|---|---|---|---|
| Furnace | Foundry · Gunner | 950 | 90 | .70 | 3 | 0/100 | **Overheat** — next 6 attacks deal +70% AD and splash 1 hex (phys) |
| Matriarch | Foundry · Juggernaut | 1250 | 85 | .65 | 1 | 0/90 | **Foundry's Embrace** — 500 shield to self and adjacent allies; enemies who strike the shields take 60 magic |
| Prisma | Prospect · Arcanist | 900 | 78 | .65 | 3 | 20/90 | **Prism Lance** — 480 magic piercing line; +25% per diamond held (cap +75%) |
| Auger | Prospect · Assassin | 920 | 95 | .80 | 1 | 0/95 | **Burrow Strike** — untargetable 1s, emerges at backline for 280% AD (phys) |
| Oakheart | Sylvan · Bulwark | 1400 | 75 | .55 | 1 | 0/85 | **Deep Roots** — 600 shield; while shielded, regenerates 4% max HP/s |
| Fury | Sylvan · Duelist | 1000 | 92 | .85 | 1 | 0/100 | **Wildwrath** — 260% AD (phys); each cast this fight adds +10% AS permanently |
| Magnate | Gilded · Gunner | 950 | 96 | .75 | 3 | 0/100 | **Money Shot** — 300% AD (phys); costs nothing, but if you bank ≥20 gold it crits |
| Duchess | Gilded · Arcanist | 880 | 76 | .65 | 3 | 20/95 | **Compound Interest** — 380 magic; repeats after 2s at +50% if the target still lives |
| Bastion | Wallguard · Herald | 920 | 70 | .65 | 3 | 25/85 | **Sanctify** — heal all lane allies 220; allies within 1 hex of your bed row heal double |
| **Bram** | Wallguard · Bulwark · **Heartbound** | 1450 | 82 | .60 | 1 | 0/90 | **Vow** — 550 shield; if Lyra is on this lane, she gains the same shield |
| Vestige | Ruinborn · Herald | 890 | 72 | .65 | 3 | 25/90 | **Eulogy** — 350 magic to 3 enemies; allies gain +10% AD per destroyed lane |
| Eyewall | Stormcaller · Sniper | 860 | 100 | .75 | 5 | 0/110 | **Hurricane Bolt** — 320% AD (phys) to farthest enemy; gains all Stormcaller stacks twice |
| Horizon | Caravan · Duelist | 1050 | 94 | .85 | 1 | 0/100 | **Long Haul** — 270% AD (phys) dash through target; shield 20% max HP |

### 5-Costs (8)

| Unit | Traits | HP | AD | AS | Rng | Mana | Ability |
|---|---|---|---|---|---|---|---|
| **Wallbreaker** | Ruinborn · Juggernaut · **Breacher** | 1750 | 120 | .70 | 1 | 0/100 | **Demolish** — 350% AD (phys) in a 2-hex line; structures weep: see Breacher |
| **Aurelia** | Gilded · Sniper · **Golden Toll** | 1300 | 125 | .80 | 5 | 0/110 | **Golden Volley** — 5 shots of 110% AD (phys) split among enemies; each kill = Golden Toll mint |
| **Verdanth** | Sylvan(×3) · **Dragonsoul** | 1650 | 110 | .70 | 2 | 30/100 | **Emerald Breath** — 550 magic cone; Sylvans hit by the breath (allies) are healed instead |
| **Karst** | Foundry · Prospect · Bulwark · **Motherlode** | 1800 | 100 | .60 | 1 | 0/90 | **Tectonic Slam** — 400 magic 2-hex ring, 1.5s stun; see Motherlode on death |
| **Mirrormarch** | Caravan · Assassin · **Omnipresent** | 1350 | 118 | .85 | 1 | 0/95 | **Phantom Waltz** — blink + 300% AD (phys); echoes on other lanes cast a 35% version simultaneously |
| **Vessa** | Stormcaller · Arcanist · **Perfect Storm** | 1400 | 105 | .75 | 3 | 25/105 | **Tempest Crown** — 500 magic to 4 enemies; at 3 Perfect Storm stacks it hits ALL enemies |
| **Gravekeeper** | Ruinborn · Arcanist | 1500 | 110 | .70 | 3 | 20/100 | **Last Rites** — 650 magic to target; if it kills, the grave spawns a 500-HP Bone Servant for the rest of the fight |
| **Mortar** | Wallguard · Bulwark · **Living Wall** | 1900 | 108 | .55 | 1 | 0/95 | **Mortar Wall** — 700 shield to self and adjacent allies; see Living Wall |

---

## 6. Comp Archetypes (what the set wants you to discover)

- **Iron Kingdom** (L1): Foundry 4/6 + Caravan — generate iron, eat it into permanent AD, buy
  every consumable. The lane nobody can dislodge by round 10.
- **Diamond Cartel** (L2): Prospect 4 + Prisma carry — mine diamonds, stack items on one
  board, Forgemaster kit. Slow, inevitable.
- **The Feeding Forest** (L3): Sylvan 6 + Dragon — win emeralds, spend emeralds on yourself,
  Dragonsoul hoards the rest. Attunement discount makes powers nearly free.
- **Gold Rush** (L4): Gilded 4/5 + Aurelia + Merchant kit — all-in on the richest lane. Huge
  bounty, huge target; classic Blotto bait.
- **The Bedless Horde**: Ruinborn 4 + Wallbreaker — deliberately concede a lane, consolidate,
  and let destruction scaling + Breacher end the war before economy does.
- **Sequence Storm**: Stormcaller 4 + Vessa parked on gold lane — sacrifice early-lane tempo,
  win the last and richest fight every round.
- **The Vow**: Lyra + Bram + Wallguard 4 — an unkillable duo holding one bed forever.
- **Everywhere Army**: Mirrormarch + thin, wide boards — the echoes make every lane fight 1 unit
  bigger, warping wither math across the whole map.

---

## 7. Implementation & Migration Notes

- **IMPLEMENTED**: the full 60-unit roster, origins/classes with counted breakpoints, the
  ability framework, and all nine signatures now ship in the build (`UnitCatalog`, `Traits.cs`,
  `CombatSim`). Names are one-word per the set's naming rule.
- **Sprites**: the 24 mapped itch.io sprites cover the low costs; the remaining 36 need either
  the pixel-crawler pack (once it extracts), recolors via the existing `Tint` system, or new packs.
- **Currency hooks**: trait generators route through `MatchController.AwardBounty`-adjacent code
  and must respect §1 guardrail caps — implement the caps as `GameConfig` constants first.
- **Echo units** (Omnipresent) need a `CombatUnit.IsEcho` flag: excluded from trait counts, bed
  damage credit, and Scavenger death counting; included in "survivors" for wither only.
- **Shop pity for Heartbound**: owning one of the pair triples the other's roll weight — small
  addition to `ShopSystem.RollShop`.
- Roll-out order: origins/classes system → 1-3 cost roster → 4-costs → signatures (each 5-cost
  is its own mini-feature; Omnipresent and Breacher touch match rules, do them last).

*Tuning creed: generators are capped, scalers have ceilings, amplifiers only improve what you
already paid for. When in doubt, nerf the economy half of a trait before the combat half —
combat power self-corrects through fights; economy power compounds silently.*
