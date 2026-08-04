using System.Collections.Generic;

namespace Wallfall
{
    /// <summary>Kits — utility identities picked one per lane during the Walls phase (spec §7).</summary>
    public enum Kit { None, Forgemaster, Quartermaster, Warlord, Architect, Merchant, Scavenger }

    /// <summary>Emerald powers (spec §2, start weak).</summary>
    public enum PowerKind { BedRepair, Rally, Frenzy, Overgrowth, Wallback, Windfall, Tutor }

    /// <summary>Iron consumables (spec §2).</summary>
    public enum ConsumableKind { BedPlating, WarHorn, FieldRations, Sharpen, Torch }

    public static class KitInfo
    {
        public static readonly Kit[] AllKits =
            { Kit.Forgemaster, Kit.Quartermaster, Kit.Warlord, Kit.Architect, Kit.Merchant, Kit.Scavenger };

        public static string Describe(Kit k)
        {
            switch (k)
            {
                case Kit.Forgemaster: return "2 item slots here; items cost 2 diamonds";
                case Kit.Quartermaster: return "Consumables for this lane cost 1 less iron (min 1); +1 iron/round";
                case Kit.Warlord: return "Enemy anchor-crush triggers at 2+ survivors; +1g on win";
                case Kit.Architect: return "+30 anchor HP; plating 2 iron; softer close losses";
                case Kit.Merchant: return "+2g on win; rerolls cost 1 while alive";
                case Kit.Scavenger: return "Your deaths pay +1 iron per 3, banked here";
                default: return "";
            }
        }

        public static string Abbrev(Kit k) => k == Kit.None ? "" : k.ToString().Substring(0, 4).ToUpper();
    }

    public static class PowerInfo
    {
        public static int Cost(PowerKind p)
        {
            switch (p)
            {
                case PowerKind.BedRepair: return 2;
                case PowerKind.Rally: return 1;
                case PowerKind.Frenzy: return 2;
                case PowerKind.Overgrowth: return 2;
                case PowerKind.Wallback: return 3;
                case PowerKind.Windfall: return 2;
                case PowerKind.Tutor: return 1;
                default: return 99;
            }
        }

        public static bool TargetsLane(PowerKind p) =>
            p == PowerKind.BedRepair || p == PowerKind.Overgrowth || p == PowerKind.Wallback || p == PowerKind.Frenzy;

        public static string DisplayName(PowerKind p) =>
            p == PowerKind.BedRepair ? "ANCHOR REPAIR" : p.ToString().ToUpper();

        public static string Describe(PowerKind p)
        {
            switch (p)
            {
                case PowerKind.BedRepair: return "+30 HP to the focused lane's anchor";
                case PowerKind.Rally: return "After your next lost fight: +12% stats in later lanes";
                case PowerKind.Frenzy: return "Focused lane +20% attack speed this round";
                case PowerKind.Windfall: return "+6 gold, right now";
                case PowerKind.Tutor: return "+3 XP toward your next level";
                case PowerKind.Overgrowth: return "Focused lane +25% HP this round";
                case PowerKind.Wallback: return "The focused lane's anchor takes no damage this round (1/match)";
                default: return "";
            }
        }
    }

    public static class ConsumableInfo
    {
        public static int Cost(ConsumableKind c, Kit laneKit)
        {
            int cost;
            switch (c)
            {
                case ConsumableKind.BedPlating: cost = laneKit == Kit.Architect ? 2 : 4; break;
                case ConsumableKind.WarHorn: cost = 3; break;
                case ConsumableKind.FieldRations: cost = 2; break;
                case ConsumableKind.Sharpen: cost = 3; break;
                case ConsumableKind.Torch: cost = 3; break;
                default: return 99;
            }
            if (laneKit == Kit.Quartermaster) cost = UnityEngine.Mathf.Max(1, cost - 1);
            return cost;
        }

        public static string DisplayName(ConsumableKind c) =>
            c == ConsumableKind.BedPlating ? "ANCHOR PLATING" : c.ToString().ToUpper();

        public static string Describe(ConsumableKind c)
        {
            switch (c)
            {
                case ConsumableKind.BedPlating: return "+15 anchor HP (up to max)";
                case ConsumableKind.WarHorn: return "+15% attack speed this round";
                case ConsumableKind.FieldRations: return "+20% HP this round";
                case ConsumableKind.Sharpen: return "+12% attack damage this round";
                case ConsumableKind.Torch: return "Enemy units in this lane -12% attack speed this round";
                default: return "";
            }
        }
    }

    /// <summary>Completed diamond items — no component algebra (spec §2).</summary>
    public class ItemDef
    {
        public string Id, Name;
        public float AdMult = 1f, ApMult = 1f, HpMult = 1f, AsMult = 1f;
        public float Lifesteal, Thorns;
        public int RangeBonus, ManaBonus;

        public ItemDef(string id, string name) { Id = id; Name = name; }
        public const int DiamondCost = 3;
        public const int ForgeCost = 2;
    }

    public static class ItemCatalog
    {
        public static readonly List<ItemDef> All = new List<ItemDef>
        {
            new ItemDef("blade",  "Whetstone Blade") { AdMult = 1.35f },
            new ItemDef("crystal","Focus Crystal")   { ApMult = 1.35f },
            new ItemDef("plate",  "Tower Plate")     { HpMult = 1.30f },
            new ItemDef("boots",  "Swift Boots")     { AsMult = 1.20f },
            new ItemDef("fang",   "Vampire Fang")    { Lifesteal = 0.20f },
            new ItemDef("spring", "Mana Spring")     { ManaBonus = 5 },
            new ItemDef("thorn",  "Thorn Shell")     { Thorns = 0.15f },
            new ItemDef("scope",  "Sniper Scope")    { RangeBonus = 1 },
        };

        public const int DuplicateCost = 5; // Diamond Duplicate
    }

    /// <summary>Round-scoped combat modifiers for one side of one lane fight.</summary>
    public class CombatMods
    {
        public float HpMult = 1f;   // Field Rations, Overgrowth, Rally
        public float AsMult = 1f;   // War Horn, Rally
        public float AdMult = 1f;   // Rally / Sharpen / Sylvan amp
        public float ApMult = 1f;   // Sylvan amp
        public static readonly CombatMods None = new CombatMods();
    }
}
