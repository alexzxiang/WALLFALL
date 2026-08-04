using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wallfall
{
    public enum Phase { Planning, Reveal, Fighting, Resolve, GameOver }

    /// <summary>What you can see of an opponent's lane: last-fight snapshot during planning.</summary>
    public struct SnapUnit
    {
        public UnitDef Def; public int Star; public int Col; public int Row;
    }

    /// <summary>
    /// The match: Walls phase (rounds 1-5, PvE, opponent hidden) then the War
    /// (blind planning -> reveal -> sequential fights, gold lane last -> resolve).
    /// Owns kit effects, all four currency sinks, wither/consolidation/uncontested rules.
    /// </summary>
    public class MatchController : MonoBehaviour
    {
        public PlayerState You { get; private set; }
        public PlayerState Foe { get; private set; }
        public ShopSystem Shop { get; private set; }
        public BotPlayer Bot { get; private set; }

        public Phase Phase { get; private set; } = Phase.Planning;
        public int Round { get; private set; }
        public float PlanningTimeLeft { get; private set; }
        public int FightingLane { get; private set; } = -1;
        public CombatSim CurrentFight { get; private set; }
        public int FightSpeed { get; private set; } = 1;

        /// <summary>The lane currently on screen (set by the presenter). Star-ups use bench + this lane.</summary>
        public int VisibleLane = 0;

        /// <summary>Never merge units out of the lane that is actively fighting.</summary>
        int SafeVisibleLane =>
            (Phase == Phase.Fighting && VisibleLane == FightingLane) ? -1 : VisibleLane;

        /// <summary>Focusing a lane brings its copies into view — visible triples combine now.</summary>
        public void OnLaneViewed(int lane)
        {
            VisibleLane = lane;
            if (Phase != Phase.Planning) return;
            bool merged = false;
            var defs = You.Bench.Select(u => u.Def)
                .Concat(You.Lanes[lane].Units.Select(pl => pl.Unit.Def))
                .Distinct().ToList();
            foreach (var def in defs)
                merged |= Shop.TryStarUp(You, def, lane);
            if (merged) StateChanged?.Invoke();
        }

        /// <summary>Rounds 1-5: no information at all. The walls drop at round 6.</summary>
        public bool WallsUp => Round <= 5;
        public bool IsWallsPhase => WallsUp;

        /// <summary>During planning the enemy renders from this last-fight snapshot, not live state.</summary>
        public List<SnapUnit>[] FoeSnapshot { get; private set; }
        public int SnapshotRound { get; private set; }

        /// <summary>True when foe boards should render live (reveal/fights/resolve).</summary>
        public bool FoeBoardsLive => Phase != Phase.Planning;

        /// <summary>Your units placed/moved this planning render ghosted (hidden from foe until reveal).</summary>
        public HashSet<int> MovedThisPlanning { get; } = new HashSet<int>();

        public int[] LanePots { get; private set; } = new int[GameConfig.LaneCount];
        public FightResult?[] RoundResults { get; private set; } = new FightResult?[GameConfig.LaneCount];

        public event System.Action StateChanged;
        public event System.Action<int> FightStarted;
        public event System.Action<int, string> Announcement;
        public event System.Action<int, bool> LaneDied;
        public event System.Action BigMoment; // walls drop, bed break -> camera shake

        bool _playerReady;
        readonly int[] _yourDeathsThisRound = new int[1]; // boxed for closure-free accumulation
        int _foeDeathsThisRound;
        int _fightIndexThisRound;
        readonly Dictionary<PlayerState, int> _traitIron = new Dictionary<PlayerState, int>();
        readonly Dictionary<PlayerState, int> _traitGold = new Dictionary<PlayerState, int>();
        readonly Dictionary<PlayerState, int> _traitDia = new Dictionary<PlayerState, int>();

        public void Begin()
        {
            You = new PlayerState("You", false);
            Foe = new PlayerState("Rival", true);
            Shop = new ShopSystem(Random.Range(int.MinValue, int.MaxValue));
            Bot = new BotPlayer(Foe, Shop);

            FoeSnapshot = new List<SnapUnit>[GameConfig.LaneCount];
            for (int i = 0; i < GameConfig.LaneCount; i++) FoeSnapshot[i] = new List<SnapUnit>();

            SeedStartingUnits(You);
            SeedStartingUnits(Foe);

            StartCoroutine(RunMatch());
        }

        void SeedStartingUnits(PlayerState p)
        {
            var oneCosts = UnitCatalog.All.Where(d => d.Cost == 1).ToList();
            for (int i = 0; i < 3; i++)
            {
                var def = oneCosts[Random.Range(0, oneCosts.Count)];
                p.Lanes[i].Units.Add(new PlacedUnit(new UnitInstance(def), 1 + i % 3, 2));
            }
        }

        IEnumerator RunMatch()
        {
            while (true)
            {
                Round++;
                Income(You);
                Income(Foe);
                Shop.RollShop(You);
                Shop.RollShop(Foe);
                for (int i = 0; i < GameConfig.LaneCount; i++) RoundResults[i] = null;

                // --- Planning (blind) ---
                Phase = Phase.Planning;
                _playerReady = false;
                MovedThisPlanning.Clear();
                PlanningTimeLeft = Round == 1 ? 60f : GameConfig.PlanningSeconds;

                if (Round == 6)
                {
                    Announcement?.Invoke(-1, "THE WALLS DROP!");
                    BigMoment?.Invoke();
                }

                Bot.Plan(Round);
                StateChanged?.Invoke();

                while (PlanningTimeLeft > 0f && !_playerReady)
                {
                    PlanningTimeLeft -= UnityEngine.Time.deltaTime;
                    yield return null;
                }

                if (Round == 1) AutoAssignKits(You);

                // --- Reveal ---
                Phase = Phase.Reveal;
                MovedThisPlanning.Clear();
                StateChanged?.Invoke();
                yield return new WaitForSeconds(GameConfig.RevealSeconds);

                // --- Fights ---
                Phase = Phase.Fighting;
                You.XpDiscount = false;
                Foe.XpDiscount = false;
                _yourDeathsThisRound[0] = 0;
                _foeDeathsThisRound = 0;
                _fightIndexThisRound = 0;
                _traitIron[You] = 0; _traitIron[Foe] = 0;
                _traitGold[You] = 0; _traitGold[Foe] = 0;
                _traitDia[You] = 0; _traitDia[Foe] = 0;

                if (WallsUp) yield return RunWallsFights();
                else yield return RunWarFights();

                FightingLane = -1;

                // --- Resolve ---
                Phase = Phase.Resolve;
                AwardScavenger(You, _yourDeathsThisRound[0]);
                AwardScavenger(Foe, _foeDeathsThisRound);
                You.RallyActive = false;
                Foe.RallyActive = false;
                You.RallyArmed = false;
                Foe.RallyArmed = false;
                Consolidate(You);
                Consolidate(Foe);
                CaptureFoeSnapshot();
                StateChanged?.Invoke();

                if (You.Defeated || Foe.Defeated)
                {
                    Phase = Phase.GameOver;
                    Announcement?.Invoke(-1, You.Defeated ? "DEFEAT — ALL ANCHORS LOST" : "VICTORY — WALLFALL!");
                    StateChanged?.Invoke();
                    yield break;
                }
                yield return new WaitForSeconds(0.8f);
            }
        }

        // ------------------------------------------------------------------
        // Fights
        // ------------------------------------------------------------------

        IEnumerator RunWarFights()
        {
            for (int lane = 0; lane < GameConfig.LaneCount; lane++)
            {
                if (!You.Lanes[lane].Alive || !Foe.Lanes[lane].Alive) continue;

                var modsYou = BuildMods(You, lane);
                var modsFoe = BuildMods(Foe, lane);
                // Torch: each side's lane debuff slows the OPPOSING units in this fight
                modsYou.AsMult *= Foe.Lanes[lane].RoundEnemyAsMult;
                modsFoe.AsMult *= You.Lanes[lane].RoundEnemyAsMult;
                var sim = new CombatSim(You.Lanes[lane].Units, Foe.Lanes[lane].Units, modsYou, modsFoe,
                                        BuildContext(You, lane), BuildContext(Foe, lane),
                                        BuildEchoes(You, lane), BuildEchoes(Foe, lane));
                yield return WatchFight(lane, sim);
                ResolveWarFight(lane, sim);
                PayTraitEconomy(lane, sim);
                _fightIndexThisRound++;
                _yourDeathsThisRound[0] += sim.DeathsA;
                _foeDeathsThisRound += sim.DeathsB;
                StateChanged?.Invoke();
                yield return new WaitForSeconds(GameConfig.PostFightSeconds);
            }
        }

        IEnumerator RunWallsFights()
        {
            // your lanes fight visible PvE waves; the bot's resolve off-screen
            for (int lane = 0; lane < GameConfig.LaneCount; lane++)
            {
                if (You.Lanes[lane].Alive)
                {
                    var sim = new CombatSim(You.Lanes[lane].Units, BuildCreepWave(Round),
                                            BuildMods(You, lane), null, BuildContext(You, lane), null);
                    yield return WatchFight(lane, sim);
                    ResolvePvE(You, lane, sim.Result == FightResult.SideAWins);
                    PayTraitEconomySide(You, 0, lane, sim);
                    _fightIndexThisRound++;
                    _yourDeathsThisRound[0] += sim.DeathsA;
                    StateChanged?.Invoke();
                    yield return new WaitForSeconds(GameConfig.PostFightSeconds * 0.6f);
                }

                if (Foe.Lanes[lane].Alive)
                {
                    var botSim = new CombatSim(Foe.Lanes[lane].Units, BuildCreepWave(Round),
                                               BuildMods(Foe, lane), null, BuildContext(Foe, lane), null);
                    int guard = 0;
                    while (!botSim.Finished && guard++ < 400) botSim.Tick();
                    ResolvePvE(Foe, lane, botSim.Result == FightResult.SideAWins);
                    _foeDeathsThisRound += botSim.DeathsA;
                }
            }
        }

        IEnumerator WatchFight(int lane, CombatSim sim)
        {
            FightingLane = lane;
            CurrentFight = sim;
            FightStarted?.Invoke(lane);
            StateChanged?.Invoke();

            // let the camera arrive and both formations stand in place before the clash
            yield return new WaitForSeconds(0.9f);

            while (!sim.Finished)
            {
                sim.Tick();
                yield return new WaitForSeconds(1f / (GameConfig.CombatTickRate * FightSpeed));
            }
            CurrentFight = null;
        }

        CombatMods BuildMods(PlayerState p, int lane)
        {
            var l = p.Lanes[lane];
            var mods = new CombatMods
            {
                HpMult = l.RoundHpMult,
                AsMult = l.RoundAsMult,
                AdMult = l.RoundAdMult,
                ApMult = l.RoundApMult
            };
            if (p.RallyActive)
            {
                mods.HpMult *= 1.12f;
                mods.AsMult *= 1.12f;
                mods.AdMult *= 1.12f;
            }
            return mods;
        }

        CombatContext BuildContext(PlayerState p, int lane)
        {
            int dead = You.Lanes.Count(l => !l.Alive) + Foe.Lanes.Count(l => !l.Alive);
            int pots = 0; foreach (var g in LanePots) pots += g;
            return new CombatContext
            {
                Gold = p.Gold, Iron = p.Iron, Diamonds = p.Diamonds, Emeralds = p.Emeralds,
                DeadLanes = dead, FightIndex = _fightIndexThisRound, PotGold = pots,
                BedHp = p.Lanes[lane].Hp, LaneIndex = lane
            };
        }

        /// <summary>Omnipresent: Mirrormarch on any OTHER living lane sends a 35% echo into this fight.</summary>
        List<UnitInstance> BuildEchoes(PlayerState p, int lane)
        {
            List<UnitInstance> echoes = null;
            for (int l = 0; l < GameConfig.LaneCount; l++)
            {
                if (l == lane || !p.Lanes[l].Alive) continue;
                foreach (var placed in p.Lanes[l].Units)
                    if (placed.Unit.Def.Sig == Signature.Omnipresent)
                    {
                        var echo = new UnitInstance(placed.Unit.Def) { Star = placed.Unit.Star };
                        (echoes = echoes ?? new List<UnitInstance>()).Add(echo);
                    }
            }
            return echoes;
        }

        void PayTraitEconomy(int lane, CombatSim sim)
        {
            PayTraitEconomySide(You, 0, lane, sim);
            PayTraitEconomySide(Foe, 1, lane, sim);
        }

        void PayTraitEconomySide(PlayerState p, int side, int lane, CombatSim sim)
        {
            // Foundry: survivors bank iron by tier (guardrail: <=4 bonus iron/round)
            int fCount = p.Lanes[lane].Units.Count(u => u.Unit.Def.Origin == Origin.Foundry || u.Unit.Def.Origin2 == Origin.Foundry);
            int fTier = TraitInfo.Tier(fCount, TraitInfo.Breakpoints(Origin.Foundry));
            if (fTier > 0 && sim.AnySurvivor(side, Origin.Foundry))
            {
                int amt = new[] { 0, 1, 2, 3 }[fTier] + (lane == 0 ? 1 : 0);
                amt = Mathf.Min(amt, 4 - _traitIron[p]);
                if (amt > 0) { _traitIron[p] += amt; GainIron(p, amt); }
            }

            // Prospect: mined diamond (guardrail: <=1/round)
            bool mined = side == 0 ? sim.MinedDiamondA : sim.MinedDiamondB;
            if (mined && _traitDia[p] < 1) { _traitDia[p]++; p.Diamonds++; }

            // Motherlode eruption bank
            bool lode = side == 0 ? sim.MotherlodeA : sim.MotherlodeB;
            if (lode)
            {
                if (_traitIron[p] < 4) { _traitIron[p]++; GainIron(p, 1); }
                if (_traitDia[p] < 1) { _traitDia[p]++; p.Diamonds++; }
            }

            // Golden Toll: Aurelia's kills mint gold (<=3/round, inside gold guardrail)
            int mint = Mathf.Min(3, sim.KillsBySig(side, Signature.GoldenToll));
            if (mint > 0)
            {
                mint = Mathf.Min(mint, 6 - _traitGold[p]);
                if (mint > 0) { _traitGold[p] += mint; p.Gold += mint; }
            }
        }

        /// <summary>Central iron faucet: Caravans permanently convert iron income into AD.</summary>
        public void GainIron(PlayerState p, int amount)
        {
            if (amount <= 0) return;
            p.Iron += amount;
            foreach (var u in p.AllOwnedUnits())
                if (u.Def.Origin == Origin.Caravan || u.Def.Origin2 == Origin.Caravan)
                    u.BonusAd = Mathf.Min(60f, u.BonusAd + amount);
        }

        List<PlacedUnit> BuildCreepWave(int round)
        {
            string[][] waves =
            {
                new[] { "thorn" },
                new[] { "filch", "thorn" },
                new[] { "filch", "sprout" },
                new[] { "scavver", "thorn", "filch" },
                new[] { "scavver", "smelt", "sprout" },
            };
            var ids = waves[Mathf.Clamp(round - 1, 0, waves.Length - 1)];
            var list = new List<PlacedUnit>();
            for (int i = 0; i < ids.Length; i++)
                list.Add(new PlacedUnit(new UnitInstance(UnitCatalog.Get(ids[i])), 1 + i, 1));
            return list;
        }

        void ResolvePvE(PlayerState p, int lane, bool won)
        {
            if (won)
            {
                switch (Round)
                {
                    case 1: GainIron(p, 2); break;
                    case 2: p.Diamonds += 1; break;
                    case 3: p.Gold += 4; break;
                    case 4: p.Emeralds += 1; break;
                    default: GainIron(p, 2); p.Diamonds += 1; p.Emeralds += 1; break; // R5 "choice", simplified
                }
                if (p == You) RoundResults[lane] = FightResult.SideAWins;
            }
            else
            {
                if (Round >= 3)
                {
                    p.Lanes[lane].Hp = Mathf.Max(0, p.Lanes[lane].Hp - GameConfig.PvELossDamage);
                    if (!p.Lanes[lane].Alive) { LaneDied?.Invoke(lane, p == You); BigMoment?.Invoke(); }
                }
                if (p == You) RoundResults[lane] = FightResult.SideBWins;
            }
            if (p == You)
                Announcement?.Invoke(lane, won ? $"LANE {lane + 1} — WAVE CLEARED" : $"LANE {lane + 1} — WAVE LOST");
        }

        void ResolveWarFight(int lane, CombatSim sim)
        {
            RoundResults[lane] = sim.Result;

            if (sim.Result == FightResult.Draw)
            {
                LanePots[lane] += GameConfig.IronBounty[lane] + GameConfig.DiamondBounty[lane]
                                + GameConfig.EmeraldBounty[lane] + GameConfig.GoldBounty[lane];
                PayPotluck(You);
                PayPotluck(Foe);
                Announcement?.Invoke(lane, $"LANE {lane + 1} — DRAW · POT GROWS");
                return;
            }

            PlayerState winner = sim.Result == FightResult.SideAWins ? You : Foe;
            PlayerState loser = winner == You ? Foe : You;
            int winnerSide = winner == You ? 0 : 1;

            AwardBounty(winner, lane, includePot: true);
            if (winner.Lanes[lane].Kit == Kit.Merchant) winner.Gold += 2;
            if (winner.Lanes[lane].Kit == Kit.Warlord) winner.Gold += 1;

            // Gilded: winning a Gilded lane pays bonus gold by tier (guardrail <=6/round)
            int gCount = winner.Lanes[lane].Units.Count(u => u.Unit.Def.Origin == Origin.Gilded || u.Unit.Def.Origin2 == Origin.Gilded);
            int gTier = TraitInfo.Tier(gCount, TraitInfo.Breakpoints(Origin.Gilded));
            if (gTier > 0)
            {
                int amt = Mathf.Min(new[] { 0, 1, 2, 3, 5 }[gTier], 6 - _traitGold[winner]);
                if (amt > 0) { _traitGold[winner] += amt; winner.Gold += amt; }
            }

            // Rally: a loss arms the buff for this player's remaining lanes this round
            if (loser.RallyArmed) { loser.RallyActive = true; loser.RallyArmed = false; }

            int damage = GameConfig.LaneDamageBase + sim.WinnerSurvivors * GameConfig.LaneDamagePerSurvivor;

            // Breacher: Wallbreaker alive on the winning side batters the bed directly
            if (sim.AnySurvivorSig(winnerSide, Signature.Breacher)) damage += 6;

            int witherThreshold = winner.Lanes[lane].Kit == Kit.Warlord ? 2 : GameConfig.WitherSurvivorThreshold;
            bool wither = sim.LoserWiped && sim.WinnerSurvivors >= witherThreshold;
            // Living Wall: Mortar's bed cannot take Wither bonus damage
            if (wither && loser.Lanes[lane].Units.Any(u => u.Unit.Def.Sig == Signature.LivingWall)) wither = false;
            if (wither) damage += GameConfig.WitherBonusDamage;

            // Wallguard: the lane's bed takes less damage on a loss
            int wCount = loser.Lanes[lane].Units.Count(u => u.Unit.Def.Origin == Origin.Wallguard || u.Unit.Def.Origin2 == Origin.Wallguard);
            int wTier = TraitInfo.Tier(wCount, TraitInfo.Breakpoints(Origin.Wallguard));
            if (wTier > 0) damage = Mathf.Max(1, damage - new[] { 0, 2, 4, 8 }[wTier]);

            // Architect: close losses (winner kept <=1 unit) hurt less
            if (loser.Lanes[lane].Kit == Kit.Architect && sim.WinnerSurvivors <= 1)
                damage = Mathf.Max(1, damage - GameConfig.ArchitectCloseLossReduction);

            var laneState = loser.Lanes[lane];
            if (laneState.Wallback)
            {
                damage = 0;
                Announcement?.Invoke(lane, $"LANE {lane + 1} — WALLBACK! ANCHOR PROTECTED");
            }

            laneState.Hp = Mathf.Max(0, laneState.Hp - damage);

            if (damage > 0)
            {
                string who = winner == You ? "VICTORY" : "DEFEAT";
                Announcement?.Invoke(lane, wither
                    ? $"LANE {lane + 1} — {who} · ANCHOR CRUSHED -{damage}"
                    : $"LANE {lane + 1} — {who} · -{damage}");
                if (wither) BigMoment?.Invoke();
            }

            if (!laneState.Alive)
            {
                LaneDied?.Invoke(lane, loser == You);
                BigMoment?.Invoke();
            }
        }

        // ------------------------------------------------------------------
        // Economy & resolve
        // ------------------------------------------------------------------

        void Income(PlayerState p)
        {
            p.Gold += GameConfig.GoldFloor;
            p.Gold += Mathf.Min(GameConfig.InterestCap, p.Gold / GameConfig.InterestPer);
            p.GainXp(GameConfig.XpPerRound);
            if (p.Lanes[0].Alive) GainIron(p, GameConfig.IronTrickle);
            if (p.HasAliveKit(Kit.Quartermaster)) GainIron(p, 1);

            p.TransfersUsed = 0;
            p.FreeTransfers = 0;
            p.RallyActive = false;
            foreach (var l in p.Lanes) l.ResetRoundMods();

            // uncontested lanes auto-bank (War only; everyone is alive during the Walls)
            if (!WallsUp)
            {
                var enemy = p == You ? Foe : You;
                for (int lane = 0; lane < GameConfig.LaneCount; lane++)
                    if (p.Lanes[lane].Alive && !enemy.Lanes[lane].Alive)
                        AwardBounty(p, lane, includePot: false);
            }
        }

        void AwardBounty(PlayerState p, int lane, bool includePot)
        {
            GainIron(p, GameConfig.IronBounty[lane]);
            p.Diamonds += GameConfig.DiamondBounty[lane];
            p.Emeralds += GameConfig.EmeraldBounty[lane];
            p.Gold += GameConfig.GoldBounty[lane];
            if (lane == GameConfig.GoldLaneIndex) p.XpDiscount = true;
            if (includePot)
            {
                p.Gold += LanePots[lane];
                LanePots[lane] = 0;
            }
        }

        void AwardScavenger(PlayerState p, int deaths)
        {
            if (deaths >= 3 && p.HasAliveKit(Kit.Scavenger))
                GainIron(p, deaths / 3);
        }

        void Consolidate(PlayerState p)
        {
            foreach (var lane in p.Lanes)
            {
                if (lane.Alive || lane.Units.Count == 0) continue;
                foreach (var placed in lane.Units.ToList())
                    p.Bench.Add(placed.Unit);
                lane.Units.Clear();
            }
        }

        void CaptureFoeSnapshot()
        {
            SnapshotRound = Round;
            for (int lane = 0; lane < GameConfig.LaneCount; lane++)
            {
                FoeSnapshot[lane].Clear();
                foreach (var p in Foe.Lanes[lane].Units)
                    FoeSnapshot[lane].Add(new SnapUnit { Def = p.Unit.Def, Star = p.Unit.Star, Col = p.Col, Row = p.Row });
            }
        }

        void PayPotluck(PlayerState p)
        {
            foreach (var laneState in p.Lanes)
                foreach (var placed in laneState.Units)
                    if (placed.Unit.Def.Sig == Signature.Potluck && _traitGold[p] < 6)
                    {
                        _traitGold[p]++;
                        p.Gold++;
                        placed.Unit.BonusAd += 5f;
                        return; // once per draw
                    }
        }

        void AutoAssignKits(PlayerState p)
        {
            var free = KitInfo.AllKits.Where(k => p.Lanes.All(l => l.Kit != k)).ToList();
            foreach (var lane in p.Lanes)
            {
                if (lane.Kit != Kit.None) continue;
                var k = free[Random.Range(0, free.Count)];
                free.Remove(k);
                lane.SetKit(k);
            }
        }

        // ------------------------------------------------------------------
        // Player actions
        // ------------------------------------------------------------------

        public bool CanAct => Phase == Phase.Planning;

        /// <summary>Shop economy stays open through fights, TFT-style (board edits do not).</summary>
        public bool CanShop => Phase != Phase.GameOver;

        /// <summary>Selling: anything during planning; bench-only mid-battle; never a unit in the running fight.</summary>
        public bool CanSell(UnitInstance u)
        {
            if (Phase == Phase.GameOver) return false;
            if (CurrentFight != null && CurrentFight.Units.Any(cu => cu.Source == u)) return false;
            if (Phase != Phase.Planning && You.LaneOf(u) >= 0) return false;
            return true;
        }

        public void PlayerReady() { if (CanAct) _playerReady = true; }

        public void ToggleFightSpeed()
        {
            FightSpeed = FightSpeed == 1 ? 2 : 1;
            StateChanged?.Invoke();
        }

        public int RerollCost => You.HasAliveKit(Kit.Merchant) ? 1 : GameConfig.RerollCost;

        public bool BuyFromShop(int slot)
        {
            if (!CanShop) return false;
            bool ok = Shop.Buy(You, slot, SafeVisibleLane) != null;
            if (ok) StateChanged?.Invoke();
            return ok;
        }

        public bool Reroll()
        {
            if (!CanShop || You.Gold < RerollCost) return false;
            You.Gold -= RerollCost;
            Shop.RollShop(You);
            StateChanged?.Invoke();
            return true;
        }

        public bool BuyXp()
        {
            int cost = You.XpDiscount ? GameConfig.XpCost - 1 : GameConfig.XpCost;
            if (!CanShop || You.Gold < cost || You.Level >= GameConfig.MaxLevel) return false;
            You.Gold -= cost;
            You.GainXp(GameConfig.XpPerBuy);
            StateChanged?.Invoke();
            return true;
        }

        public bool SellUnit(UnitInstance u)
        {
            if (!CanSell(u)) return false;
            Shop.Sell(You, u);
            StateChanged?.Invoke();
            return true;
        }

        public bool SelectKit(int lane, Kit kit)
        {
            if (!CanAct || Round != 1) return false;
            if (You.Lanes.Any(l => l.Index != lane && l.Kit == kit)) return false;
            You.Lanes[lane].SetKit(kit);
            StateChanged?.Invoke();
            return true;
        }

        int TransferCost(int srcLane, int destLane)
        {
            if (GameConfig.TransferIronCost == 0) return 0; // dial disabled
            bool qm = (srcLane >= 0 && You.Lanes[srcLane].Kit == Kit.Quartermaster)
                   || (destLane >= 0 && You.Lanes[destLane].Kit == Kit.Quartermaster);
            return qm ? 1 : GameConfig.TransferIronCost;
        }

        public bool MoveUnit(UnitInstance u, int destLane, int col, int row)
        {
            if (!CanAct) return false;
            int srcLane = You.LaneOf(u);

            bool isTransfer = srcLane >= 0 && destLane != srcLane;
            bool useFree = isTransfer && You.FreeTransfers > 0;
            int ironCost = isTransfer && !useFree ? TransferCost(srcLane, destLane) : 0;

            if (isTransfer && !useFree)
            {
                if (You.TransfersUsed >= GameConfig.TransfersPerRound) return false;
                if (You.Iron < ironCost) return false;
            }

            if (destLane >= 0)
            {
                var lane = You.Lanes[destLane];
                if (!lane.Alive) return false;
                if (row < 0 || row >= GameConfig.BoardRowsPerSide || col < 0 || col >= GameConfig.BoardCols) return false;

                var occupant = lane.At(col, row);
                if (occupant != null && occupant.Unit != u)
                {
                    // SWAP: the dropped unit takes the cell; the occupant takes the dropped
                    // unit's old spot (board cell or bench). Counts never change, so no cap check.
                    int oldCol = 0, oldRow = 0;
                    if (srcLane >= 0)
                    {
                        var placedSrc = You.Lanes[srcLane].Units.First(pl => pl.Unit == u);
                        oldCol = placedSrc.Col; oldRow = placedSrc.Row;
                        You.Lanes[srcLane].Units.Remove(placedSrc);
                    }
                    else You.Bench.Remove(u);

                    lane.Units.Remove(occupant);
                    lane.Units.Add(new PlacedUnit(u, col, row));
                    MovedThisPlanning.Add(u.Id);

                    if (srcLane >= 0)
                    {
                        You.Lanes[srcLane].Units.Add(new PlacedUnit(occupant.Unit, oldCol, oldRow));
                        MovedThisPlanning.Add(occupant.Unit.Id);
                    }
                    else You.Bench.Add(occupant.Unit);

                    Shop.TryStarUp(You, u.Def, SafeVisibleLane);
                    Shop.TryStarUp(You, occupant.Unit.Def, SafeVisibleLane);
                    StateChanged?.Invoke();
                    return true;
                }
                if (srcLane != destLane && lane.Units.Count >= You.UnitCap) return false;
            }
            else if (You.Bench.Count >= GameConfig.BenchSize && srcLane >= 0)
            {
                return false;
            }

            if (srcLane >= 0)
            {
                var placed = You.Lanes[srcLane].Units.First(p => p.Unit == u);
                You.Lanes[srcLane].Units.Remove(placed);
            }
            else You.Bench.Remove(u);

            if (destLane >= 0)
            {
                You.Lanes[destLane].Units.Add(new PlacedUnit(u, col, row));
                MovedThisPlanning.Add(u.Id);
            }
            else You.Bench.Add(u);

            // bringing copies together (visible set) can complete a star-up
            Shop.TryStarUp(You, u.Def, SafeVisibleLane);

            if (isTransfer)
            {
                if (useFree) You.FreeTransfers--;
                else { You.TransfersUsed++; You.Iron -= ironCost; }
            }
            StateChanged?.Invoke();
            return true;
        }

        // ---- sinks ----

        public bool BuyConsumable(ConsumableKind kind, int lane)
        {
            if (!CanAct || lane < 0 || !You.Lanes[lane].Alive) return false;
            var l = You.Lanes[lane];
            int cost = ConsumableInfo.Cost(kind, l.Kit);
            if (You.Iron < cost) return false;

            switch (kind)
            {
                case ConsumableKind.BedPlating:
                    if (l.Hp >= l.MaxHp) return false;
                    l.Hp = Mathf.Min(l.MaxHp, l.Hp + GameConfig.BedPlatingAmount);
                    break;
                case ConsumableKind.WarHorn:
                    l.RoundAsMult = 1.15f;
                    break;
                case ConsumableKind.FieldRations:
                    l.RoundHpMult = Mathf.Max(l.RoundHpMult, 1.2f);
                    break;
                case ConsumableKind.Sharpen:
                    l.RoundAdMult = Mathf.Max(l.RoundAdMult, 1.12f);
                    break;
                case ConsumableKind.Torch:
                    l.RoundEnemyAsMult = Mathf.Min(l.RoundEnemyAsMult, 0.88f);
                    break;
            }
            You.Iron -= cost;
            StateChanged?.Invoke();
            return true;
        }

        public bool BuyPower(PowerKind kind, int lane)
        {
            if (!CanAct) return false;
            int cost = PowerInfo.Cost(kind);
            if (You.Emeralds < cost) return false;
            if (PowerInfo.TargetsLane(kind) && (lane < 0 || !You.Lanes[lane].Alive)) return false;

            switch (kind)
            {
                case PowerKind.BedRepair:
                    You.Lanes[lane].Hp = Mathf.Min(You.Lanes[lane].MaxHp, You.Lanes[lane].Hp + GameConfig.BedRepairAmount);
                    break;
                case PowerKind.Rally:
                    if (You.RallyArmed) return false;
                    You.RallyArmed = true;
                    break;
                case PowerKind.Frenzy:
                    You.Lanes[lane].RoundAsMult = Mathf.Max(You.Lanes[lane].RoundAsMult, 1.2f);
                    break;
                case PowerKind.Windfall:
                    You.Gold += 6;
                    break;
                case PowerKind.Tutor:
                    You.GainXp(3);
                    break;
                case PowerKind.Overgrowth:
                    You.Lanes[lane].RoundHpMult = Mathf.Max(You.Lanes[lane].RoundHpMult, 1.25f);
                    break;
                case PowerKind.Wallback:
                    if (You.WallbackUsed) return false;
                    You.WallbackUsed = true;
                    You.Lanes[lane].Wallback = true;
                    break;
            }
            You.Emeralds -= cost;

            // Sylvan: emerald powers cast on a Sylvan lane empower it this round
            if (PowerInfo.TargetsLane(kind))
            {
                int sCount = You.Lanes[lane].Units.Sum(u =>
                    (u.Unit.Def.Origin == Origin.Sylvan || u.Unit.Def.Origin2 == Origin.Sylvan) ? u.Unit.Def.SylvanWeight : 0);
                if (TraitInfo.Tier(sCount, TraitInfo.Breakpoints(Origin.Sylvan)) > 0)
                {
                    You.Lanes[lane].RoundAdMult = Mathf.Max(You.Lanes[lane].RoundAdMult, 1.15f);
                    You.Lanes[lane].RoundApMult = Mathf.Max(You.Lanes[lane].RoundApMult, 1.15f);
                }
            }
            StateChanged?.Invoke();
            return true;
        }

        public bool BuyAndEquipItem(ItemDef item, UnitInstance unit)
        {
            if (!CanAct) return false;
            int lane = You.LaneOf(unit);
            bool onBench = lane < 0 && You.Bench.Contains(unit);
            if (lane < 0 && !onBench) return false;

            bool forge = lane >= 0 && You.Lanes[lane].Kit == Kit.Forgemaster;
            int slots = forge ? 2 : 1;
            int cost = forge ? ItemDef.ForgeCost : ItemDef.DiamondCost;
            if (unit.Items.Count >= slots || You.Diamonds < cost) return false;

            You.Diamonds -= cost;
            unit.Items.Add(item);
            StateChanged?.Invoke();
            return true;
        }

        public bool DiamondDuplicate(UnitInstance unit)
        {
            if (!CanAct || You.Diamonds < ItemCatalog.DuplicateCost) return false;
            if (unit.Star != 1 || unit.Def.Cost > 3) return false;
            if (You.Bench.Count >= GameConfig.BenchSize) return false;

            You.Diamonds -= ItemCatalog.DuplicateCost;
            You.Bench.Add(new UnitInstance(unit.Def));
            Shop.TryStarUp(You, unit.Def, SafeVisibleLane);
            StateChanged?.Invoke();
            return true;
        }
    }
}
