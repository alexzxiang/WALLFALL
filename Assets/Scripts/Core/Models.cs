using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wallfall
{
    /// <summary>A owned unit (persistent between rounds).</summary>
    public class UnitInstance
    {
        static int _nextId;
        public readonly int Id;
        public UnitDef Def;
        public int Star = 1;
        public List<ItemDef> Items = new List<ItemDef>();
        public float BonusAd;                 // Caravan stacks, Potluck permanents

        public UnitInstance(UnitDef def) { Def = def; Id = ++_nextId; }

        public float StatScale => Mathf.Pow(GameConfig.StarStatMultiplier, Star - 1);
        public int SellValue => Def.Cost * (int)Mathf.Pow(GameConfig.CopiesToStarUp, Star - 1);
    }

    /// <summary>A unit standing on a lane board (position in your-half coords: rows 0..2).</summary>
    public class PlacedUnit
    {
        public UnitInstance Unit;
        public int Col, Row;
        public PlacedUnit(UnitInstance u, int col, int row) { Unit = u; Col = col; Row = row; }
    }

    public class LaneState
    {
        public int Index;
        public int Hp;
        public int MaxHp;
        public bool Alive => Hp > 0;
        public List<PlacedUnit> Units = new List<PlacedUnit>();

        public Kit Kit = Kit.None;

        // round-scoped (cleared at income)
        public float RoundAsMult = 1f;      // War Horn / Frenzy
        public float RoundHpMult = 1f;      // Field Rations / Overgrowth
        public float RoundAdMult = 1f;      // Sharpen / Sylvan power-amp
        public float RoundApMult = 1f;      // Sylvan power-amp
        public float RoundEnemyAsMult = 1f; // Torch (slows the enemy in THIS lane's fight)
        public bool Wallback;               // no bed damage this round

        public LaneState(int index)
        {
            Index = index;
            MaxHp = GameConfig.LaneStartHp(index);
            Hp = MaxHp;
        }

        public void SetKit(Kit kit)
        {
            if (Kit == Kit.Architect && kit != Kit.Architect) { MaxHp -= GameConfig.ArchitectBedBonus; Hp = Mathf.Min(Hp, MaxHp); }
            if (kit == Kit.Architect && Kit != Kit.Architect) { MaxHp += GameConfig.ArchitectBedBonus; Hp += GameConfig.ArchitectBedBonus; }
            Kit = kit;
        }

        public void ResetRoundMods()
        {
            RoundAsMult = 1f;
            RoundHpMult = 1f;
            RoundAdMult = 1f;
            RoundApMult = 1f;
            RoundEnemyAsMult = 1f;
            Wallback = false;
        }

        public PlacedUnit At(int col, int row) => Units.FirstOrDefault(p => p.Col == col && p.Row == row);
    }

    public class PlayerState
    {
        public string Name;
        public bool IsBot;

        // Currencies
        public int Gold;
        public int Iron;
        public int Diamonds;
        public int Emeralds;

        // Level
        public int Level = 1;
        public int Xp;

        // Round-scoped
        public int TransfersUsed;
        public int FreeTransfers;            // Supply Drop
        public bool XpDiscount;              // won gold lane last round
        public bool RallyArmed;              // Rally power waiting for a lost fight
        public bool RallyActive;             // Rally triggered; buffs later lanes this round
        public bool WallbackUsed;            // once per match

        public bool KitsChosen
        {
            get
            {
                foreach (var l in Lanes) if (l.Kit == Kit.None) return false;
                return true;
            }
        }

        public bool HasAliveKit(Kit kit)
        {
            foreach (var l in Lanes) if (l.Alive && l.Kit == kit) return true;
            return false;
        }

        public List<UnitInstance> Bench = new List<UnitInstance>();
        public LaneState[] Lanes;
        public string[] Shop = new string[GameConfig.ShopSlots]; // def ids, null = bought/empty

        public PlayerState(string name, bool isBot)
        {
            Name = name; IsBot = isBot;
            Gold = GameConfig.StartingGold;
            Lanes = new LaneState[GameConfig.LaneCount];
            for (int i = 0; i < GameConfig.LaneCount; i++) Lanes[i] = new LaneState(i);
        }

        public bool Defeated => Lanes.All(l => !l.Alive);
        public int UnitCap => GameConfig.UnitCapPerLane(Level);

        public void GainXp(int amount)
        {
            if (Level >= GameConfig.MaxLevel) return;
            Xp += amount;
            while (Level < GameConfig.MaxLevel && Xp >= GameConfig.XpForLevel[Level + 1])
                Level++;
        }

        public int XpToNext => Level >= GameConfig.MaxLevel ? 0 : GameConfig.XpForLevel[Level + 1] - Xp;

        public IEnumerable<UnitInstance> AllOwnedUnits()
        {
            foreach (var u in Bench) yield return u;
            foreach (var lane in Lanes)
                foreach (var p in lane.Units)
                    yield return p.Unit;
        }

        /// <summary>Which lane a unit is on, or -1 for bench / not found.</summary>
        public int LaneOf(UnitInstance u)
        {
            for (int i = 0; i < Lanes.Length; i++)
                if (Lanes[i].Units.Any(p => p.Unit == u)) return i;
            return -1;
        }
    }
}
