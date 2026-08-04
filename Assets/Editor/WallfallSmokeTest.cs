using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wallfall.EditorTools
{
    /// <summary>Headless logic check: run with -executeMethod Wallfall.EditorTools.WallfallSmokeTest.Run</summary>
    public static class WallfallSmokeTest
    {
        public static void Run()
        {
            try
            {
                TestShop();
                TestCombat();
                TestEmptyLaneFight();
                TestKitsAndItems();
                Debug.Log("WALLFALL SMOKE: ALL PASS");
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("WALLFALL SMOKE FAILED: " + e);
                EditorApplication.Exit(1);
            }
        }

        static void Assert(bool cond, string msg)
        {
            if (!cond) throw new System.Exception("Assert failed: " + msg);
        }

        static void TestShop()
        {
            var shop = new ShopSystem(12345);
            var p = new PlayerState("T", false);
            p.Gold = 100;

            shop.RollShop(p);
            Assert(p.Shop.Count(s => s != null) == GameConfig.ShopSlots, "shop fills 5 slots");

            // buy everything affordable, check gold decreases
            int before = p.Gold;
            for (int i = 0; i < GameConfig.ShopSlots && p.Bench.Count < GameConfig.BenchSize; i++)
                shop.Buy(p, i);
            Assert(p.Gold < before, "buying costs gold");
            Assert(p.Bench.Count > 0, "bought to bench");

            // force a star-up: 3 recruits
            var recruit = UnitCatalog.Get("gatepup");
            p.Bench.Clear();
            p.Bench.Add(new UnitInstance(recruit));
            p.Bench.Add(new UnitInstance(recruit));
            p.Bench.Add(new UnitInstance(recruit));
            shop.TryStarUp(p, recruit);
            Assert(p.Bench.Count == 1 && p.Bench[0].Star == 2, "3 copies merge to one 2-star");

            // xp/level
            p.GainXp(22);
            Assert(p.Level == 5, $"22 xp -> level 5 (got {p.Level})");
            Assert(p.UnitCap == 5, "level 5 cap = 5 (cap follows level)");
            Debug.Log("WALLFALL SMOKE: shop ok");
        }

        static void TestCombat()
        {
            var a = new System.Collections.Generic.List<PlacedUnit>
            {
                new PlacedUnit(new UnitInstance(UnitCatalog.Get("gatepup")), 1, 2),
                new PlacedUnit(new UnitInstance(UnitCatalog.Get("fletch")), 2, 0),
                new PlacedUnit(new UnitInstance(UnitCatalog.Get("filch")), 3, 2),
            };
            var b = new System.Collections.Generic.List<PlacedUnit>
            {
                new PlacedUnit(new UnitInstance(UnitCatalog.Get("thorn")), 2, 2),
            };

            var sim = new CombatSim(a, b);
            int maxTicks = (int)(GameConfig.FightMaxSeconds * GameConfig.CombatTickRate) + 10;
            int ticks = 0;
            while (!sim.Finished && ticks++ < maxTicks) sim.Tick();

            Assert(sim.Finished, "fight terminates");
            Assert(sim.Result == FightResult.SideAWins, $"3v1 should win (got {sim.Result})");
            Assert(sim.WinnerSurvivors >= 1, "survivors counted");
            Assert(sim.LoserWiped, "1-unit side wiped");
            Debug.Log($"WALLFALL SMOKE: combat ok in {ticks} ticks, {sim.WinnerSurvivors} survivors, t={sim.Time:F1}s");
        }

        static void TestKitsAndItems()
        {
            var p = new PlayerState("T", false);

            // Architect adds bed HP; swapping it away removes it
            p.Lanes[0].SetKit(Kit.Architect);
            Assert(p.Lanes[0].MaxHp == GameConfig.LaneMaxHp + GameConfig.ArchitectBedBonus, "architect bed bonus");
            p.Lanes[0].SetKit(Kit.Merchant);
            Assert(p.Lanes[0].MaxHp == GameConfig.LaneMaxHp, "architect bonus removed on swap");
            Assert(p.HasAliveKit(Kit.Merchant), "kit query works");

            // items change combat stats
            var plain = new UnitInstance(UnitCatalog.Get("gatepup"));
            var armed = new UnitInstance(UnitCatalog.Get("gatepup"));
            armed.Items.Add(ItemCatalog.All.Find(i => i.Id == "blade"));
            var simPlain = new CombatSim(
                new System.Collections.Generic.List<PlacedUnit> { new PlacedUnit(plain, 2, 2) },
                new System.Collections.Generic.List<PlacedUnit>());
            var simArmed = new CombatSim(
                new System.Collections.Generic.List<PlacedUnit> { new PlacedUnit(armed, 2, 2) },
                new System.Collections.Generic.List<PlacedUnit>());
            Assert(simArmed.Units[0].Ad > simPlain.Units[0].Ad * 1.3f, "blade raises AD");

            // combat mods apply
            var modded = new CombatSim(
                new System.Collections.Generic.List<PlacedUnit> { new PlacedUnit(plain, 2, 2) },
                new System.Collections.Generic.List<PlacedUnit>(),
                new CombatMods { HpMult = 1.25f }, null);
            Assert(modded.Units[0].MaxHp > simPlain.Units[0].MaxHp * 1.2f, "hp mult applies");

            Debug.Log("WALLFALL SMOKE: kits & items ok");
        }

        static void TestEmptyLaneFight()
        {
            var a = new System.Collections.Generic.List<PlacedUnit>
            {
                new PlacedUnit(new UnitInstance(UnitCatalog.Get("filch")), 2, 2),
            };
            var empty = new System.Collections.Generic.List<PlacedUnit>();

            var sim = new CombatSim(a, empty);
            sim.Tick();
            Assert(sim.Finished && sim.Result == FightResult.SideAWins, "units vs empty = instant win");

            var sim2 = new CombatSim(empty, new System.Collections.Generic.List<PlacedUnit>());
            sim2.Tick();
            Assert(sim2.Finished && sim2.Result == FightResult.Draw, "empty vs empty = draw");
            Debug.Log("WALLFALL SMOKE: edge cases ok");
        }
    }
}
