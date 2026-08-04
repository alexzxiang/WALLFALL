# WALLFALL

**A four-lane autobattler where the economy is the battlefield.**

Draft units, hold four lanes at once, and survive the moment the walls drop.
Teamfight Tactics meets Bedwars: every lane mines a different resource, every
anchor you crush starves your opponent, and every fight you skip is gold you kept.

**▶ Play in your browser:** *(itch.io link coming soon)*

---

## The game

You and an AI opponent each hold **four lanes** — Iron, Diamond, Emerald, and
Gold — and each lane is a full hex board with its own army, its own **anchor**
(destroy it and the lane starts bleeding), and its own resource stream:

- **Gold** runs your shop: reroll, level up, buy units (TFT-style odds and a shared pool).
- **Iron** buys lane consumables — anchor plating, war horns, rations, torches.
- **Diamonds** buy permanent items you forge onto units.
- **Emeralds** buy swingy one-shot powers — repairs, rallies, frenzies, windfalls.

For the first five rounds the **walls are up**: you fight PvE creeps, build your
economy, and can't see the enemy boards. At round six the walls drop, the boards
reveal, and all four lanes fight sequentially every round. Lose a fight, your
anchor takes damage. Lose your anchor, the lane starts dying for good.

### Set 1 — The Four Fronts

**60 units** across five cost tiers, with 8 origins, 8 classes, and 9 one-off
signature traits — all counted **per lane**, so every board is its own puzzle.
Traits plug directly into the four-currency economy: Foundry units bank iron by
surviving, Prospectors mine diamonds mid-fight, Gilded armies mint gold off wins,
and signatures like Motherlode and Potluck bend the economy in stranger ways.

Other systems: per-lane **kits** (permanent lane identities picked at start),
star-ups scoped to the units you can actually see, a fog-of-war scouting layer
with last-seen snapshots, and an overtime system so fights always conclude.

## Controls

| Input | Action |
|---|---|
| Drag | Move / deploy units (drop on an occupied hex to swap; drop on the shop to sell) |
| Right-click | Unit details sidebar |
| Scroll wheel / `1–4` / `Q`,`E` | Switch lanes |
| `R` / `X` | Reroll shop / buy XP |
| `T` | Open the market |
| `S` | Sell hovered unit |
| `F` | Toggle fight speed |
| `Enter` | Ready |

## Tech

- **Unity 6** (URP 2D), C#, new Input System.
- The entire game — boards, UI, menus, VFX — is **built procedurally at runtime**
  from a single bootstrap component. No scenes to author, no prefabs.
- Deterministic fixed-tick combat sim (20 tps) with continuous unit movement,
  separation solving, and a single parameterized ability interpreter that covers
  all 60 units' spells.
- Custom UI toolkit on UGUI: translucent "glass" surfaces with animated
  chroma-gradient borders drawn by a bespoke mesh graphic.

## Building from source

Open the project in Unity 6000.3+ and press Play — **but note:** the third-party
art, audio, and font packs are licensed for use in the built game, not for
redistribution, so they are **not in this repository**. A fresh clone compiles
and runs with procedural placeholder graphics and no audio. To restore the full
presentation, download the packs listed in [CREDITS.md](CREDITS.md) into
`Assets/itchio/` and run **WALLFALL → Web → Configure**, then the resource setup
tool regenerates the runtime bundle. WebGL builds ship via
**WALLFALL → Web → Build Release**.

## Credits

All third-party asset attributions live in [CREDITS.md](CREDITS.md).
WALLFALL is a free, non-commercial fan-scale project.

Game design & programming: **Alex Xiang**.
