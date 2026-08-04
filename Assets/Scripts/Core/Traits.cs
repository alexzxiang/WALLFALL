using System.Collections.Generic;

namespace Wallfall
{
    /// <summary>Economy/region origins — lane-local, breakpoints 2/4/6 (Gilded 2/3/4/5).</summary>
    public enum Origin { Foundry, Prospect, Sylvan, Gilded, Wallguard, Ruinborn, Stormcaller, Caravan }

    /// <summary>Combat classes — lane-local, breakpoints 2/4(/6), Herald 2/3.</summary>
    public enum UnitClass { Bulwark, Juggernaut, Duelist, Assassin, Sniper, Arcanist, Herald, Gunner }

    /// <summary>One-off signature traits (mostly 5-costs).</summary>
    public enum Signature { None, Heartbound, Breacher, GoldenToll, Dragonsoul, Motherlode, Omnipresent, PerfectStorm, LivingWall, Potluck }

    public static class TraitInfo
    {
        public static readonly Origin[] AllOrigins = (Origin[])System.Enum.GetValues(typeof(Origin));
        public static readonly UnitClass[] AllClasses = (UnitClass[])System.Enum.GetValues(typeof(UnitClass));

        public static int[] Breakpoints(Origin o) =>
            o == Origin.Gilded ? new[] { 2, 3, 4, 5 }
            : (o == Origin.Prospect || o == Origin.Ruinborn || o == Origin.Stormcaller || o == Origin.Caravan)
                ? new[] { 2, 4 } : new[] { 2, 4, 6 };

        public static int[] Breakpoints(UnitClass c) =>
            c == UnitClass.Herald ? new[] { 2, 3 }
            : (c == UnitClass.Bulwark || c == UnitClass.Duelist || c == UnitClass.Arcanist)
                ? new[] { 2, 4, 6 } : new[] { 2, 4 };

        /// <summary>Tier reached for a count against breakpoints (0 = inactive).</summary>
        public static int Tier(int count, int[] bps)
        {
            int tier = 0;
            foreach (var bp in bps) if (count >= bp) tier++;
            return tier;
        }

        public static string Describe(Origin o)
        {
            switch (o)
            {
                case Origin.Foundry: return "+15/35/60 Armor. If a Foundry unit survives the fight, bank 1/2/3 iron. Home lane L1: +1 more.";
                case Origin.Prospect: return "+15/35% ability power. First Prospector cast each fight: 30/60% chance to mine 1 diamond. Home lane L2: +20% chance.";
                case Origin.Sylvan: return "Regenerate 1.5/3/5% max HP per second. Emerald powers cast on this lane also grant Sylvans +15% AD & AP this round.";
                case Origin.Gilded: return "Winning this lane pays +1/2/3/5 gold. Gilded deal +1% damage per 2 gold banked (capped by tier).";
                case Origin.Wallguard: return "+10/22/40% max HP. Losing this lane's fight deals 2/4/8 less anchor damage.";
                case Origin.Ruinborn: return "+14/30% AD & AP per destroyed lane on the map (max 3).";
                case Origin.Stormcaller: return "Every 3s of combat: +12/25% attack speed (stacking). Start with 1 stack per fight already resolved this round.";
                case Origin.Caravan: return "Start fights with a 20/40% max-HP shield. Whenever you gain iron, Caravans permanently gain +1 AD (capped).";
                default: return "";
            }
        }

        public static string Describe(UnitClass c)
        {
            switch (c)
            {
                case UnitClass.Bulwark: return "+250/550/900 HP; taunts adjacent enemies at fight start.";
                case UnitClass.Juggernaut: return "12/25% omnivamp; +20/40 Armor while above half HP.";
                case UnitClass.Duelist: return "Attacks grant +5% attack speed, up to 8/12/16 stacks.";
                case UnitClass.Assassin: return "Leap to the enemy backline; +20/45% crit chance (crits x1.4).";
                case UnitClass.Sniper: return "+1 range; +8/18% damage per hex to target (max 3).";
                case UnitClass.Arcanist: return "Lane allies +15/30/50% ability power; Arcanists get double.";
                case UnitClass.Herald: return "+20 starting mana; first cast also shields the lowest ally 25/45% of the Herald's max HP.";
                case UnitClass.Gunner: return "Every 4th attack deals +80/160% AD bonus damage.";
                default: return "";
            }
        }

        public static string Describe(Signature s)
        {
            switch (s)
            {
                case Signature.Heartbound: return "Lyra & Bram: together on a lane they gain +25% all stats; once per fight the survivor revives the fallen at 40% HP.";
                case Signature.Breacher: return "Winning this lane with Wallbreaker alive deals +6 anchor damage.";
                case Signature.GoldenToll: return "+1 AD per 5 gold banked (cap +40). Kills mint +1 gold (max 3/round).";
                case Signature.Dragonsoul: return "Counts as 3 Sylvan. +8% ability power per emerald held (cap +40%).";
                case Signature.Motherlode: return "Counts as Foundry AND Prospect. First death each round: erupts and banks +1 iron & +1 diamond.";
                case Signature.Omnipresent: return "At fight start, a 35%-strength echo joins every other living allied lane's fight this round.";
                case Signature.PerfectStorm: return "+20% AP and +10% AS per fight already resolved this round.";
                case Signature.LivingWall: return "Your anchor here can't take crush bonus damage. +2 max HP per anchor HP at fight start.";
                case Signature.Potluck: return "+3% all stats per gold in any lane pot. Any drawn fight: banks +1 gold and gains +5 AD permanently.";
                default: return "";
            }
        }

        // icon keys into the skill-icon pack (via SpriteBank.TraitIcon)
        public static string IconFile(Origin o)
        {
            switch (o)
            {
                case Origin.Foundry: return "slash skill icon 4.png";
                case Origin.Prospect: return "Ice skill icon 4.png";
                case Origin.Sylvan: return "plant skill icon.png";
                case Origin.Gilded: return "healing skill icon 4.png";
                case Origin.Wallguard: return "Ice skill icon 2.png";
                case Origin.Ruinborn: return "fire skill icon.png";
                case Origin.Stormcaller: return "dragon wing skill icon.png";
                case Origin.Caravan: return "plant skill icon 4.png";
                default: return null;
            }
        }

        public static string IconFile(UnitClass c)
        {
            switch (c)
            {
                case UnitClass.Bulwark: return "Ice skill icon.png";
                case UnitClass.Juggernaut: return "dragon charges skill icon.png";
                case UnitClass.Duelist: return "slash skill icon.png";
                case UnitClass.Assassin: return "slash skill icon 2.png";
                case UnitClass.Sniper: return "fire skill icon 2.png";
                case UnitClass.Arcanist: return "fire skill icon 3.png";
                case UnitClass.Herald: return "healing skill icon.png";
                case UnitClass.Gunner: return "slash skill icon 3.png";
                default: return null;
            }
        }
    }

    // ------------------------------------------------------------------
    // Ability framework: one parameterized spec covers the whole set.
    // ------------------------------------------------------------------

    public enum TargetMode { Current, Nearest, LowestHp, Farthest, Backline }

    public class AbilitySpec
    {
        public string Name = "";
        public string Desc = "";

        // damage
        public float AdRatio;          // % of AD as physical (1.8 = 180%)
        public float Magic;            // flat magic damage, scaled by AbilityPower
        public int Targets = 1;
        public TargetMode Mode = TargetMode.Current;
        public bool Pierce;            // also hits the enemy behind the target
        public bool Splash;            // also hits neighbors of the target
        public float StunDur;
        public float BlindDur;
        public float DotDamage;        // over DotDur
        public float DotDur;
        public int ArmorShred;

        // defense / support
        public float ShieldSelfPct;    // % of caster max HP
        public float ShieldFlat;
        public bool ShieldAdjacent;    // Matriarch: also shields adjacent allies
        public float HealLowest;       // flat heal on lowest-HP ally
        public float HealAllLane;      // flat heal on all allies
        public float HealSelf;         // flat self-heal
        public float HealSelfPctDamage;// lifedrain: heal % of ability damage dealt

        // self buffs
        public float SelfAsPct;
        public float SelfAdPct;
        public float BuffDur = 4f;
        public bool BuffPermanent;     // Fury: stacks forever

        // attack modifiers
        public int EmpowerAttacks;     // next N attacks...
        public float EmpowerBonusPct;  // ...deal +X% AD

        // movement / misc
        public bool BlinkBehind;
        public bool TauntTarget;
        public bool ResetManaOnKill;
        public bool SummonOnKill;      // Gravekeeper's Bone Servant
    }
}
