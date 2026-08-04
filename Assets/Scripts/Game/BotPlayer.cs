using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wallfall
{
    /// <summary>
    /// A simple, legible allocator: buys pair-completing units first, spreads strength across
    /// living lanes (weakest lane gets reinforcements), melee to the front row, ranged to the back.
    /// </summary>
    public class BotPlayer
    {
        readonly PlayerState _me;
        readonly ShopSystem _shop;

        public BotPlayer(PlayerState me, ShopSystem shop) { _me = me; _shop = shop; }

        public void Plan(int round)
        {
            if (round == 1) PickKits();
            SpendResources(round);

            // level up mid-game when rich
            while (_me.Gold >= GameConfig.XpCost + 8 && _me.Level < GameConfig.MaxLevel && round >= 3)
            {
                _me.Gold -= GameConfig.XpCost;
                _me.GainXp(GameConfig.XpPerBuy);
            }

            // buy: prefer units that complete a star-up, then strongest affordable
            for (int pass = 0; pass < 2; pass++)
            {
                for (int slot = 0; slot < _me.Shop.Length; slot++)
                {
                    string id = _me.Shop[slot];
                    if (id == null) continue;
                    var def = UnitCatalog.Get(id);
                    if (_me.Gold < def.Cost) continue;

                    bool completesPair = _me.AllOwnedUnits().Count(u => u.Def == def && u.Star == 1) >= 1;
                    if (pass == 0 && !completesPair) continue;
                    if (_me.Bench.Count >= GameConfig.BenchSize) break;
                    _shop.Buy(_me, slot);
                }
                if (pass == 0 && _me.Gold >= GameConfig.RerollCost && _me.Gold > 16)
                {
                    _me.Gold -= GameConfig.RerollCost;
                    _shop.RollShop(_me);
                }
            }

            Deploy();
        }

        void PickKits()
        {
            // a legible identity: forge the diamond lane, defend gold, spread the rest randomly
            var pool = KitInfo.AllKits.OrderBy(_ => Random.value).ToList();
            pool.Remove(Kit.Forgemaster); pool.Remove(Kit.Architect);
            _me.Lanes[1].SetKit(Kit.Forgemaster);
            _me.Lanes[GameConfig.GoldLaneIndex].SetKit(Kit.Architect);
            _me.Lanes[0].SetKit(pool[0]);
            _me.Lanes[2].SetKit(pool[1]);
        }

        void SpendResources(int round)
        {
            // emeralds: repair the most wounded living lane
            var hurt = _me.Lanes.Where(l => l.Alive && l.Hp <= l.MaxHp - GameConfig.BedRepairAmount)
                                .OrderBy(l => l.Hp).FirstOrDefault();
            if (hurt != null && _me.Emeralds >= 2)
            {
                _me.Emeralds -= 2;
                hurt.Hp = Mathf.Min(hurt.MaxHp, hurt.Hp + GameConfig.BedRepairAmount);
            }

            // diamonds: item the most expensive itemless board unit
            while (_me.Diamonds >= ItemDef.DiamondCost)
            {
                var target = _me.Lanes.SelectMany(l => l.Units)
                    .Where(p => p.Unit.Items.Count == 0)
                    .OrderByDescending(p => p.Unit.Def.Cost * p.Unit.StatScale)
                    .FirstOrDefault();
                if (target == null) break;
                int laneIdx = _me.LaneOf(target.Unit);
                bool forge = laneIdx >= 0 && _me.Lanes[laneIdx].Kit == Kit.Forgemaster;
                _me.Diamonds -= forge ? ItemDef.ForgeCost : ItemDef.DiamondCost;
                target.Unit.Items.Add(ItemCatalog.All[Random.Range(0, ItemCatalog.All.Count)]);
            }

            // iron: plate a wounded bed when flush (transfers cost is left for the player to exploit)
            var plate = _me.Lanes.Where(l => l.Alive && l.Hp < l.MaxHp).OrderBy(l => l.Hp).FirstOrDefault();
            if (plate != null && _me.Iron >= 6)
            {
                _me.Iron -= ConsumableInfo.Cost(ConsumableKind.BedPlating, plate.Kit);
                plate.Hp = Mathf.Min(plate.MaxHp, plate.Hp + GameConfig.BedPlatingAmount);
            }
        }

        void Deploy()
        {
            // strongest bench units out first
            foreach (var unit in _me.Bench.OrderByDescending(u => u.Def.Cost * u.StatScale).ToList())
            {
                var lane = _me.Lanes
                    .Where(l => l.Alive && l.Units.Count < _me.UnitCap)
                    .OrderBy(l => LaneStrength(l))
                    .FirstOrDefault();
                if (lane == null) break;

                var spot = FindSpot(lane, unit.Def.Range <= 1);
                if (spot == null) break;
                _me.Bench.Remove(unit);
                lane.Units.Add(new PlacedUnit(unit, spot.Value.x, spot.Value.y));
            }
        }

        float LaneStrength(LaneState lane) =>
            lane.Units.Sum(p => p.Unit.Def.Cost * p.Unit.StatScale);

        Vector2Int? FindSpot(LaneState lane, bool melee)
        {
            // your-half rows: 0 back, 2 front (front = nearest the seam)
            int[] rows = melee ? new[] { 2, 1, 0 } : new[] { 0, 1, 2 };
            int[] cols = { 2, 1, 3, 0, 4 };
            foreach (int r in rows)
                foreach (int c in cols)
                    if (lane.At(c, r) == null) return new Vector2Int(c, r);
            return null;
        }
    }
}
