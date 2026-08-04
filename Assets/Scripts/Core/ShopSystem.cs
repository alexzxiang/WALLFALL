using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wallfall
{
    /// <summary>
    /// One shared unit pool for both players (spec §3). Handles rolls, buying, selling, star-ups.
    /// </summary>
    public class ShopSystem
    {
        readonly Dictionary<string, int> _pool = new Dictionary<string, int>();
        readonly System.Random _rng;

        public ShopSystem(int seed)
        {
            _rng = new System.Random(seed);
            foreach (var def in UnitCatalog.All)
                _pool[def.Id] = GameConfig.PoolCopies[def.Cost];
        }

        public void RollShop(PlayerState p)
        {
            // return unbought shop units to pool
            for (int i = 0; i < p.Shop.Length; i++)
            {
                if (p.Shop[i] != null) _pool[p.Shop[i]]++;
                p.Shop[i] = null;
            }

            int[] odds = GameConfig.ShopOdds[Mathf.Clamp(p.Level, 1, GameConfig.MaxLevel)];
            for (int i = 0; i < p.Shop.Length; i++)
            {
                int tier = PickTier(odds);
                var candidates = UnitCatalog.All.Where(d => d.Cost == tier && _pool[d.Id] > 0).ToList();
                // fall back down tiers if the rolled tier is drained
                while (candidates.Count == 0 && tier > 1)
                {
                    tier--;
                    candidates = UnitCatalog.All.Where(d => d.Cost == tier && _pool[d.Id] > 0).ToList();
                }
                if (candidates.Count == 0) continue;
                var pick = candidates[_rng.Next(candidates.Count)];
                _pool[pick.Id]--;
                p.Shop[i] = pick.Id;
            }
        }

        int PickTier(int[] odds)
        {
            int roll = _rng.Next(100), acc = 0;
            for (int t = 0; t < 5; t++)
            {
                acc += odds[t];
                if (roll < acc) return t + 1;
            }
            return 1;
        }

        /// <summary>Buy shop slot into bench. visibleLane: the lane on screen (-1 bench-only, -2 all lanes/bot).</summary>
        public UnitInstance Buy(PlayerState p, int slot, int visibleLane = -2)
        {
            string id = p.Shop[slot];
            if (id == null) return null;
            var def = UnitCatalog.Get(id);
            if (p.Gold < def.Cost) return null;
            if (p.Bench.Count >= GameConfig.BenchSize && !WouldMerge(p, def, visibleLane)) return null;

            p.Gold -= def.Cost;
            p.Shop[slot] = null;
            var unit = new UnitInstance(def);
            p.Bench.Add(unit);
            TryStarUp(p, def, visibleLane);
            return unit;
        }

        bool WouldMerge(PlayerState p, UnitDef def, int visibleLane)
        {
            int count = p.Bench.Count(u => u.Def == def && u.Star == 1);
            if (visibleLane >= 0)
                count += p.Lanes[visibleLane].Units.Count(pl => pl.Unit.Def == def && pl.Unit.Star == 1);
            return count + 1 >= GameConfig.CopiesToStarUp;
        }

        /// <summary>
        /// Merge 3 same-star copies when they're all VISIBLE together: on the bench plus the
        /// lane currently on screen. Copies parked on a lane you're not looking at never merge —
        /// only when you focus that lane do they combine (TryStarUp is re-run on lane focus).
        /// visibleLane: >=0 that lane + bench; -1 bench only; -2 every lane + bench (the bot).
        /// </summary>
        public bool TryStarUp(PlayerState p, UnitDef def, int visibleLane = -2)
        {
            bool any = false;
            bool merged = true;
            while (merged)
            {
                merged = false;
                for (int star = 1; star <= 2 && !merged; star++)
                {
                    int laneFrom = visibleLane == -2 ? 0 : visibleLane;
                    int laneTo = visibleLane == -2 ? GameConfig.LaneCount - 1 : visibleLane;

                    for (int lane = laneFrom; lane <= laneTo && !merged; lane++)
                    {
                        var pool = new List<UnitInstance>();
                        // bench copies first — they're the preferred fuel
                        pool.AddRange(p.Bench.Where(u => u.Def == def && u.Star == star));
                        if (lane >= 0)
                            pool.AddRange(p.Lanes[lane].Units.Where(pl => pl.Unit.Def == def && pl.Unit.Star == star).Select(pl => pl.Unit));

                        if (pool.Count < GameConfig.CopiesToStarUp) continue;

                        // keep the board copy when there is one (its position survives), else a bench copy
                        var keep = pool.FirstOrDefault(u => p.LaneOf(u) >= 0) ?? pool[0];
                        int removed = 0;
                        foreach (var u in pool)
                        {
                            if (u == keep || removed >= GameConfig.CopiesToStarUp - 1) continue;
                            RemoveUnit(p, u);
                            removed++;
                        }
                        keep.Star = star + 1;
                        merged = true;
                        any = true;
                    }
                }
            }
            return any;
        }

        public void Sell(PlayerState p, UnitInstance u)
        {
            p.Gold += u.SellValue;
            _pool[u.Def.Id] += (int)Mathf.Pow(GameConfig.CopiesToStarUp, u.Star - 1);
            RemoveUnit(p, u);
        }

        static void RemoveUnit(PlayerState p, UnitInstance u)
        {
            if (p.Bench.Remove(u)) return;
            foreach (var lane in p.Lanes)
            {
                var placed = lane.Units.FirstOrDefault(pl => pl.Unit == u);
                if (placed != null) { lane.Units.Remove(placed); return; }
            }
        }
    }
}
