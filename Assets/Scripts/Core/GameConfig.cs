using UnityEngine;

namespace Wallfall
{
    /// <summary>All tuning dials in one place. Numbers mirror WALLFALL_systems_and_ui.md §1–§7.</summary>
    public static class GameConfig
    {
        // Lanes
        public const int LaneCount = 4;
        public const int BoardCols = 9;          // hex columns per lane board (TFT-style)
        public const int BoardRowsPerSide = 4;   // your half = 4 rows x 9 cols
        public const int GoldLaneIndex = 3;

        // Lane HP — TFT-scale: ~5-7 losses kill a lane, crushes hurt much more
        public const int LaneMaxHp = 100;
        public const int GoldLaneMaxHp = 150;
        public const int LaneDamageBase = 8;          // + per-survivor damage
        public const int LaneDamagePerSurvivor = 5;
        public const int WitherBonusDamage = 15;
        public const int WitherSurvivorThreshold = 3; // wiped vs 3+ survivors
        public const int PvELossDamage = 5;           // Walls-phase wave losses (rounds 3-5)
        public const int ArchitectBedBonus = 30;
        public const int ArchitectCloseLossReduction = 5;
        public const int BedRepairAmount = 30;        // emerald power
        public const int BedPlatingAmount = 15;       // iron consumable

        // Economy
        public const int GoldFloor = 4;
        public const int InterestPer = 10;
        public const int InterestCap = 5;
        public const int StartingGold = 12;

        // Lane bounties (indexed by lane)
        public static readonly int[] IronBounty = { 3, 0, 0, 0 };
        public static readonly int[] DiamondBounty = { 0, 2, 0, 0 };
        public static readonly int[] EmeraldBounty = { 0, 0, 1, 0 };
        public static readonly int[] GoldBounty = { 0, 0, 0, 5 };
        public const int IronTrickle = 1;             // while lane 1 lives

        // Transfers — the design doc's central dial, currently DISABLED per playtest feedback
        // (moves between lanes are free and uncapped; restore 2/2 to re-enable the mechanic)
        public const int TransferIronCost = 0;
        public const int TransfersPerRound = 99;

        // Shop
        public const int ShopSlots = 5;
        public const int RerollCost = 2;
        public const int XpCost = 4;
        public const int XpPerBuy = 4;
        public const int XpPerRound = 2;
        public const int BenchSize = 6;

        // Level -> total XP required (index = level, level 1 at 0 XP)
        public static readonly int[] XpForLevel = { 0, 0, 2, 6, 12, 22, 36, 52, 76 };
        public const int MaxLevel = 8;

        // Level -> unit cap per lane: TFT-style, your level IS the number of units
        // each lane's board can field (min 2 so the opening rounds aren't starved).
        public static int UnitCapPerLane(int level) => Mathf.Clamp(level, 2, 8);

        // Level -> shop odds per cost tier (percent, tiers 1..5)
        // TFT-style odds per level (tiers 1..5)
        public static readonly int[][] ShopOdds =
        {
            null,
            new[] { 100, 0, 0, 0, 0 },   // 1
            new[] { 100, 0, 0, 0, 0 },   // 2
            new[] { 75, 25, 0, 0, 0 },   // 3
            new[] { 55, 30, 15, 0, 0 },  // 4
            new[] { 45, 33, 20, 2, 0 },  // 5
            new[] { 30, 40, 25, 5, 0 },  // 6
            new[] { 19, 30, 40, 10, 1 }, // 7
            new[] { 17, 24, 32, 24, 3 }  // 8
        };

        // Pool copies per cost tier (1..5)
        public static readonly int[] PoolCopies = { 0, 10, 8, 6, 4, 3 };

        // Star-up
        public const int CopiesToStarUp = 3;
        public const float StarStatMultiplier = 1.8f;

        // Combat
        public const float FightMaxSeconds = 28f;      // hard cap; overtime should end fights before this
        public const float OvertimeStartSeconds = 14f; // after this, damage ramps so fights CONCLUDE
        public const float OvertimeRampPerSecond = 0.12f;
        public const float CombatTickRate = 20f; // ticks per second
        public const int ManaPerAttack = 10;
        public const int ManaPerAttackRoyals = 12;

        // Rounds
        public const float PlanningSeconds = 30f;
        public const float RevealSeconds = 1.5f;
        public const float PostFightSeconds = 1.6f;

        public static int LaneStartHp(int lane) => lane == GoldLaneIndex ? GoldLaneMaxHp : LaneMaxHp;
    }
}
