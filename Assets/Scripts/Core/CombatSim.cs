using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wallfall
{
    public enum FightResult { SideAWins, SideBWins, Draw }

    /// <summary>Per-side, per-fight economy/game context read by traits and signatures.</summary>
    public class CombatContext
    {
        public int Gold, Iron, Diamonds, Emeralds;
        public int DeadLanes;   // destroyed lanes on the whole map
        public int FightIndex;  // fights already resolved this round
        public int PotGold;     // gold sitting in all lane pots
        public int BedHp;       // this side's bed HP on this lane
        public int LaneIndex;
        public static readonly CombatContext Empty = new CombatContext();
    }

    /// <summary>A unit inside a running fight.</summary>
    public class CombatUnit
    {
        public UnitInstance Source;
        public int Side;
        public Vector2 Pos;           // continuous board-local position — hexes only set the START
        public bool IsEcho;           // Omnipresent copies & summons: no traits, no economy credit

        public float Hp, MaxHp, Ad, AttackSpeed, MoveSpeed;
        public int Range, Armor, Mana, ManaMax, ManaPerAttack;
        public float AbilityPower = 1f;
        public float DamageAmp = 1f;  // Gilded etc.
        public float Lifesteal;
        public float Thorns;
        public float CritChance;
        public float Shield;

        // statuses
        public float StunUntil, BlindUntil;
        public CombatUnit Taunter; public float TauntUntil;
        public float DotAmount, DotUntil;
        public int ShreddedArmor;

        // mechanics
        public int EmpowerLeft; public float EmpowerBonus; public int EmpowerShred;
        public int AttackCounter;
        public int DuelistStacks;
        public float StormTimer; public int StormStacks; public float StormGain;
        public float RegenPct;
        public bool HeraldRider; public float HeraldShieldPct; public bool FirstCastDone;
        public bool ReviveOnce;
        public int KillCount;
        public float SniperPerHex;
        public float BuffAsUntil; public float BuffAsAmt;
        public float BuffAdUntil; public float BuffAdAmt;
        public float PermAsBonus;

        public float AttackCd, MoveCd;
        public bool Alive => Hp > 0;

        // view hooks
        public bool JustAttacked, JustCast, JustHit;
    }

    /// <summary>Deterministic fixed-tick autobattle on one lane's 6x5 hex board.</summary>
    public class CombatSim
    {
        public readonly List<CombatUnit> Units = new List<CombatUnit>();
        public float Time { get; private set; }
        public bool Finished { get; private set; }
        public FightResult Result { get; private set; }
        public int WinnerSurvivors { get; private set; }
        public bool LoserWiped { get; private set; }
        public int DeathsA { get; private set; }
        public int DeathsB { get; private set; }

        // signature/economy outputs read by MatchController at resolve
        public bool MinedDiamondA { get; private set; }
        public bool MinedDiamondB { get; private set; }
        public bool MotherlodeA { get; private set; }
        public bool MotherlodeB { get; private set; }

        readonly System.Random _rng = new System.Random(12907);
        readonly CombatContext[] _ctx = new CombatContext[2];
        readonly bool[] _prospectTried = new bool[2];
        readonly int[] _prospectTier = new int[2];
        readonly int[] _duelistCap = new int[2];
        readonly float[] _gunnerBonus = new float[2];
        float _dt;

        public CombatSim(List<PlacedUnit> sideA, List<PlacedUnit> sideB,
                         CombatMods modsA = null, CombatMods modsB = null,
                         CombatContext ctxA = null, CombatContext ctxB = null,
                         List<UnitInstance> echoesA = null, List<UnitInstance> echoesB = null)
        {
            _dt = 1f / GameConfig.CombatTickRate;
            modsA = modsA ?? CombatMods.None;
            modsB = modsB ?? CombatMods.None;
            _ctx[0] = ctxA ?? CombatContext.Empty;
            _ctx[1] = ctxB ?? CombatContext.Empty;

            foreach (var p in sideA) AddSpawn(Spawn(p.Unit, 0, p.Col, p.Row, modsA, 1f, false));
            foreach (var p in sideB) AddSpawn(Spawn(p.Unit, 1, GameConfig.BoardCols - 1 - p.Col, GameConfig.BoardRowsPerSide * 2 - 1 - p.Row, modsB, 1f, false));
            if (echoesA != null) foreach (var e in echoesA) AddSpawn(Spawn(e, 0, 2, 0, modsA, 0.35f, true));
            if (echoesB != null) foreach (var e in echoesB) AddSpawn(Spawn(e, 1, 2, GameConfig.BoardRowsPerSide * 2 - 1, modsB, 0.35f, true));

            ApplyTraits(0);
            ApplyTraits(1);
            FightStartPositioning();
        }

        // ------------------------------------------------------------------
        // Spawning & traits
        // ------------------------------------------------------------------

        void AddSpawn(CombatUnit cu)
        {
            Units.Add(cu); // overlap is resolved continuously by separation
        }

        /// <summary>Distance in hex-widths between two fighters (continuous).</summary>
        static float HexDist(CombatUnit a, CombatUnit b) =>
            Vector2.Distance(a.Pos, b.Pos) / HexUtil.Width;

        CombatUnit Spawn(UnitInstance u, int side, int col, int row, CombatMods mods, float extraScale, bool echo)
        {
            var d = u.Def;
            float s = u.StatScale * extraScale;
            var cu = new CombatUnit
            {
                Source = u, Side = side, Pos = HexUtil.ToWorld(col, row), IsEcho = echo,
                MaxHp = d.Hp * s * mods.HpMult,
                Ad = (d.Ad * s + u.BonusAd) * mods.AdMult,
                AttackSpeed = d.AttackSpeed * mods.AsMult,
                MoveSpeed = d.MoveSpeed,
                Range = d.Range, Armor = d.Armor,
                Mana = d.ManaStart, ManaMax = d.ManaMax,
                ManaPerAttack = GameConfig.ManaPerAttack,
                AbilityPower = mods.ApMult
            };
            foreach (var item in u.Items)
            {
                cu.MaxHp *= item.HpMult;
                cu.Ad *= item.AdMult;
                cu.AttackSpeed *= item.AsMult;
                cu.AbilityPower *= item.ApMult;
                cu.Lifesteal += item.Lifesteal;
                cu.Thorns += item.Thorns;
                cu.Range += item.RangeBonus;
                cu.ManaPerAttack += item.ManaBonus;
            }
            cu.Hp = cu.MaxHp;
            return cu;
        }

        int OriginCount(int side, Origin o)
        {
            int c = 0;
            foreach (var u in Units)
            {
                if (u.Side != side || u.IsEcho) continue;
                var d = u.Source.Def;
                if (d.Origin == o || d.Origin2 == o)
                    c += (o == Origin.Sylvan) ? d.SylvanWeight : 1;
            }
            return c;
        }

        int ClassCount(int side, UnitClass k)
        {
            int c = 0;
            foreach (var u in Units)
                if (u.Side == side && !u.IsEcho && u.Source.Def.Class == k) c++;
            return c;
        }

        void ApplyTraits(int side)
        {
            var ctx = _ctx[side];
            var mine = Units.Where(u => u.Side == side).ToList();
            if (mine.Count == 0) return;

            int fTier = TraitInfo.Tier(OriginCount(side, Origin.Foundry), TraitInfo.Breakpoints(Origin.Foundry));
            int pTier = TraitInfo.Tier(OriginCount(side, Origin.Prospect), TraitInfo.Breakpoints(Origin.Prospect));
            int sTier = TraitInfo.Tier(OriginCount(side, Origin.Sylvan), TraitInfo.Breakpoints(Origin.Sylvan));
            int gTier = TraitInfo.Tier(OriginCount(side, Origin.Gilded), TraitInfo.Breakpoints(Origin.Gilded));
            int wTier = TraitInfo.Tier(OriginCount(side, Origin.Wallguard), TraitInfo.Breakpoints(Origin.Wallguard));
            int rTier = TraitInfo.Tier(OriginCount(side, Origin.Ruinborn), TraitInfo.Breakpoints(Origin.Ruinborn));
            int stTier = TraitInfo.Tier(OriginCount(side, Origin.Stormcaller), TraitInfo.Breakpoints(Origin.Stormcaller));
            int cTier = TraitInfo.Tier(OriginCount(side, Origin.Caravan), TraitInfo.Breakpoints(Origin.Caravan));
            _prospectTier[side] = pTier;

            foreach (var u in mine)
            {
                if (u.IsEcho) continue;
                var d = u.Source.Def;
                bool Has(Origin o) => d.Origin == o || d.Origin2 == o;

                if (fTier > 0 && Has(Origin.Foundry)) u.Armor += new[] { 0, 15, 35, 60 }[fTier];
                if (pTier > 0 && Has(Origin.Prospect)) u.AbilityPower *= 1f + new[] { 0f, .15f, .35f }[pTier];
                if (sTier > 0 && Has(Origin.Sylvan)) u.RegenPct = new[] { 0f, 1.5f, 3f, 5f }[sTier];
                if (gTier > 0 && Has(Origin.Gilded))
                {
                    float cap = new[] { 0f, 15f, 20f, 28f, 40f }[gTier];
                    u.DamageAmp *= 1f + Mathf.Min(cap, ctx.Gold * 0.5f) / 100f;
                }
                if (wTier > 0 && Has(Origin.Wallguard))
                {
                    u.MaxHp *= 1f + new[] { 0f, .10f, .22f, .40f }[wTier];
                    u.Hp = u.MaxHp;
                }
                if (rTier > 0 && Has(Origin.Ruinborn))
                {
                    float per = new[] { 0f, .14f, .30f }[rTier];
                    float mult = 1f + per * Mathf.Min(3, ctx.DeadLanes);
                    u.Ad *= mult; u.AbilityPower *= mult;
                }
                if (stTier > 0 && Has(Origin.Stormcaller))
                {
                    u.StormGain = new[] { 0f, .12f, .25f }[stTier];
                    u.StormStacks = ctx.FightIndex;
                }
                if (cTier > 0 && Has(Origin.Caravan))
                    u.Shield += u.MaxHp * new[] { 0f, .20f, .40f }[cTier];
            }

            int bulT = TraitInfo.Tier(ClassCount(side, UnitClass.Bulwark), TraitInfo.Breakpoints(UnitClass.Bulwark));
            int jugT = TraitInfo.Tier(ClassCount(side, UnitClass.Juggernaut), TraitInfo.Breakpoints(UnitClass.Juggernaut));
            int dueT = TraitInfo.Tier(ClassCount(side, UnitClass.Duelist), TraitInfo.Breakpoints(UnitClass.Duelist));
            int assT = TraitInfo.Tier(ClassCount(side, UnitClass.Assassin), TraitInfo.Breakpoints(UnitClass.Assassin));
            int sniT = TraitInfo.Tier(ClassCount(side, UnitClass.Sniper), TraitInfo.Breakpoints(UnitClass.Sniper));
            int arcT = TraitInfo.Tier(ClassCount(side, UnitClass.Arcanist), TraitInfo.Breakpoints(UnitClass.Arcanist));
            int herT = TraitInfo.Tier(ClassCount(side, UnitClass.Herald), TraitInfo.Breakpoints(UnitClass.Herald));
            int gunT = TraitInfo.Tier(ClassCount(side, UnitClass.Gunner), TraitInfo.Breakpoints(UnitClass.Gunner));

            float laneAp = arcT > 0 ? new[] { 0f, .15f, .30f, .50f }[arcT] : 0f;

            foreach (var u in mine)
            {
                if (u.IsEcho) continue;
                var k = u.Source.Def.Class;
                if (laneAp > 0f) u.AbilityPower *= 1f + laneAp * (k == UnitClass.Arcanist ? 2f : 1f);
                if (bulT > 0 && k == UnitClass.Bulwark) { u.MaxHp += new[] { 0, 250, 550, 900 }[bulT]; u.Hp = u.MaxHp; }
                if (jugT > 0 && k == UnitClass.Juggernaut) u.Lifesteal += new[] { 0f, .12f, .25f }[jugT];
                if (assT > 0 && k == UnitClass.Assassin) u.CritChance += new[] { 0f, .20f, .45f }[assT];
                if (sniT > 0 && k == UnitClass.Sniper) { u.Range += 1; u.SniperPerHex = new[] { 0f, .08f, .18f }[sniT]; }
                if (herT > 0 && k == UnitClass.Herald) { u.Mana += 20; u.HeraldRider = true; u.HeraldShieldPct = new[] { 0f, .25f, .45f }[herT]; }
            }
            _duelistCap[side] = new[] { 0, 8, 12, 16 }[dueT];
            _gunnerBonus[side] = new[] { 0f, .8f, 1.6f }[gunT];

            // ---- signatures ----
            bool lyra = mine.Any(u => !u.IsEcho && u.Source.Def.Id == "lyra");
            bool bram = mine.Any(u => !u.IsEcho && u.Source.Def.Id == "bram");
            foreach (var u in mine)
            {
                if (u.IsEcho) continue;
                switch (u.Source.Def.Sig)
                {
                    case Signature.Heartbound:
                        if (lyra && bram)
                        {
                            u.MaxHp *= 1.25f; u.Hp = u.MaxHp; u.Ad *= 1.25f; u.AbilityPower *= 1.25f;
                            u.ReviveOnce = true;
                        }
                        break;
                    case Signature.GoldenToll:
                        u.Ad += Mathf.Min(40, ctx.Gold / 5);
                        break;
                    case Signature.Dragonsoul:
                        u.AbilityPower *= 1f + Mathf.Min(0.4f, ctx.Emeralds * 0.08f);
                        break;
                    case Signature.PerfectStorm:
                        u.AbilityPower *= 1f + 0.2f * ctx.FightIndex;
                        u.AttackSpeed *= 1f + 0.1f * ctx.FightIndex;
                        break;
                    case Signature.LivingWall:
                        u.MaxHp += 2 * ctx.BedHp; u.Hp = u.MaxHp;
                        break;
                    case Signature.Potluck:
                        float amp = 1f + Mathf.Min(0.45f, 0.03f * ctx.PotGold);
                        u.MaxHp *= amp; u.Hp = u.MaxHp; u.Ad *= amp;
                        break;
                }
            }
        }

        void FightStartPositioning()
        {
            // Bulwark taunt is part of the 2-piece trait — a lone Bulwark doesn't taunt
            foreach (var bul in Units.Where(x => x.Source.Def.Class == UnitClass.Bulwark))
            {
                if (TraitInfo.Tier(ClassCount(bul.Side, UnitClass.Bulwark), TraitInfo.Breakpoints(UnitClass.Bulwark)) < 1)
                    continue;
                foreach (var enemy in Units.Where(o => o.Side != bul.Side))
                    if (HexDist(bul, enemy) <= 1.2f)
                    { enemy.Taunter = bul; enemy.TauntUntil = 3f; }
            }
        }

        // ------------------------------------------------------------------
        // Tick
        // ------------------------------------------------------------------

        float OvertimeAmp =>
            Time > GameConfig.OvertimeStartSeconds
                ? 1f + (Time - GameConfig.OvertimeStartSeconds) * GameConfig.OvertimeRampPerSecond
                : 1f;

        bool _assassinsLeaped;

        void DoAssassinLeaps()
        {
            // the leap is part of the 2-piece Assassin trait — a lone Assassin fights normally
            foreach (var u in Units.Where(x => x.Alive && x.Source.Def.Class == UnitClass.Assassin).ToList())
            {
                if (TraitInfo.Tier(ClassCount(u.Side, UnitClass.Assassin), TraitInfo.Breakpoints(UnitClass.Assassin)) < 1)
                    continue;
                int targetRow = u.Side == 0 ? GameConfig.BoardRowsPerSide * 2 - 1 : 0;
                float backY = HexUtil.ToWorld(0, targetRow).y;
                u.Pos = new Vector2(u.Pos.x, backY); // dash to the enemy back row; separation untangles
            }
        }

        public void Tick()
        {
            if (Finished) return;
            if (!_assassinsLeaped) { _assassinsLeaped = true; DoAssassinLeaps(); }
            Time += _dt;

            foreach (var u in Units) { u.JustAttacked = false; u.JustCast = false; u.JustHit = false; }

            foreach (var u in Units.Where(x => x.Alive).OrderBy(x => x.Source.Id).ToList())
            {
                if (!u.Alive) continue;

                if (u.RegenPct > 0f) u.Hp = Mathf.Min(u.MaxHp, u.Hp + u.MaxHp * u.RegenPct / 100f * _dt);
                if (u.DotUntil > Time && u.DotAmount > 0f) ApplyDamage(u, u.DotAmount * _dt);
                if (!u.Alive) continue;
                if (u.StormGain > 0f)
                {
                    u.StormTimer += _dt;
                    if (u.StormTimer >= 3f) { u.StormTimer -= 3f; u.StormStacks++; }
                }
                if (u.BuffAsUntil > 0f) u.BuffAsUntil -= _dt;
                if (u.BuffAdUntil > 0f) u.BuffAdUntil -= _dt;

                if (u.StunUntil > Time) continue;

                u.AttackCd -= _dt;

                var target = PickTarget(u);
                if (target == null) continue;

                float dist = HexDist(u, target);
                if (dist <= u.Range + 0.15f)
                {
                    if (u.Mana >= u.ManaMax) Cast(u, target);
                    else if (u.AttackCd <= 0f) Attack(u, target);
                }
                else
                {
                    // smooth march: MoveSpeed is in hexes/second
                    Vector2 dir = (target.Pos - u.Pos).normalized;
                    u.Pos += dir * (u.MoveSpeed * HexUtil.Width * _dt);
                }
            }

            ResolveSeparation();
            CheckEnd();
        }

        CombatUnit PickTarget(CombatUnit u)
        {
            if (u.Taunter != null && u.Taunter.Alive && u.TauntUntil > Time) return u.Taunter;
            return Nearest(u);
        }

        CombatUnit Nearest(CombatUnit u)
        {
            CombatUnit best = null; float bestDist = float.MaxValue;
            foreach (var o in Units)
            {
                if (!o.Alive || o.Side == u.Side) continue;
                float d = HexDist(u, o);
                if (d < bestDist) { bestDist = d; best = o; }
            }
            return best;
        }

        /// <summary>Keep fighters from stacking: gently push apart any overlapping pair.</summary>
        void ResolveSeparation()
        {
            float minSep = HexUtil.Width * 0.8f;
            for (int i = 0; i < Units.Count; i++)
            {
                var a = Units[i];
                if (!a.Alive) continue;
                for (int j = i + 1; j < Units.Count; j++)
                {
                    var b = Units[j];
                    if (!b.Alive) continue;
                    Vector2 delta = b.Pos - a.Pos;
                    float d = delta.magnitude;
                    if (d >= minSep) continue;
                    Vector2 push = d > 0.001f ? delta / d : new Vector2(1f, 0f);
                    float overlap = (minSep - d) * 0.5f;
                    a.Pos -= push * overlap;
                    b.Pos += push * overlap;
                }
            }
        }

        // ------------------------------------------------------------------
        // Attacks & abilities
        // ------------------------------------------------------------------

        float EffectiveAs(CombatUnit u) =>
            Mathf.Min(3.5f, u.AttackSpeed
                * (1f + 0.05f * u.DuelistStacks + u.StormGain * u.StormStacks + u.PermAsBonus)
                * (u.BuffAsUntil > 0f ? 1f + u.BuffAsAmt : 1f));

        float EffectiveAd(CombatUnit u) =>
            u.Ad * (u.BuffAdUntil > 0f ? 1f + u.BuffAdAmt : 1f);

        void Attack(CombatUnit u, CombatUnit target)
        {
            u.AttackCd = 1f / EffectiveAs(u);
            u.JustAttacked = true;

            u.Mana = Mathf.Min(u.ManaMax, u.Mana + u.ManaPerAttack);
            if (u.BlindUntil > Time) return; // swing misses

            float raw = EffectiveAd(u);
            if (u.SniperPerHex > 0f)
                raw *= 1f + u.SniperPerHex * Mathf.Min(3f, HexDist(u, target));

            u.AttackCounter++;
            if (_gunnerBonus[u.Side] > 0f && u.Source.Def.Class == UnitClass.Gunner && u.AttackCounter % 4 == 0)
                raw *= 1f + _gunnerBonus[u.Side];

            if (u.EmpowerLeft > 0)
            {
                raw *= 1f + u.EmpowerBonus;
                if (u.EmpowerShred > 0) target.ShreddedArmor += u.EmpowerShred;
                u.EmpowerLeft--;
            }

            if (u.CritChance > 0f && _rng.NextDouble() < u.CritChance) raw *= 1.4f;

            int cap = _duelistCap[u.Side];
            if (cap > 0 && u.Source.Def.Class == UnitClass.Duelist && u.DuelistStacks < cap) u.DuelistStacks++;

            DealDamage(u, target, raw, isAttack: true);
        }

        List<CombatUnit> SelectTargets(CombatUnit u, CombatUnit current, AbilitySpec spec)
        {
            var enemies = Units.Where(o => o.Alive && o.Side != u.Side);
            IEnumerable<CombatUnit> ordered;
            switch (spec.Mode)
            {
                case TargetMode.LowestHp: ordered = enemies.OrderBy(o => o.Hp / o.MaxHp); break;
                case TargetMode.Farthest: ordered = enemies.OrderByDescending(o => HexDist(u, o)); break;
                case TargetMode.Backline: ordered = enemies.OrderByDescending(o => u.Side == 0 ? o.Pos.y : -o.Pos.y); break;
                case TargetMode.Nearest: ordered = enemies.OrderBy(o => HexDist(u, o)); break;
                default: return current != null ? new List<CombatUnit> { current } : new List<CombatUnit>();
            }
            return ordered.Take(Mathf.Max(1, spec.Targets)).ToList();
        }

        void Cast(CombatUnit u, CombatUnit current)
        {
            u.Mana = 0;
            u.JustCast = true;
            var spec = u.Source.Def.Ability;
            float ap = u.AbilityPower;

            // Prospect: first cast each fight may mine a diamond
            if (!u.IsEcho && !_prospectTried[u.Side] && _prospectTier[u.Side] > 0 &&
                (u.Source.Def.Origin == Origin.Prospect || u.Source.Def.Origin2 == Origin.Prospect))
            {
                _prospectTried[u.Side] = true;
                float chance = new[] { 0f, .3f, .6f }[_prospectTier[u.Side]] + (_ctx[u.Side].LaneIndex == 1 ? .2f : 0f);
                if (_rng.NextDouble() < chance)
                {
                    if (u.Side == 0) MinedDiamondA = true; else MinedDiamondB = true;
                }
            }

            if (spec.BlinkBehind && current != null)
            {
                Vector2 dir = (current.Pos - u.Pos).normalized;
                u.Pos = current.Pos + dir * HexUtil.Width * 0.9f; // land behind the target
            }

            float totalDealt = 0f;
            if (spec.AdRatio > 0f || spec.Magic > 0f)
            {
                var targets = SelectTargets(u, current, spec);

                if (spec.Pierce && targets.Count > 0)
                {
                    var prim = targets[0];
                    var beyond = Units.Where(o => o.Alive && o.Side != u.Side && !targets.Contains(o))
                        .OrderBy(o => HexDist(prim, o))
                        .FirstOrDefault();
                    if (beyond != null) targets.Add(beyond);
                }
                if (spec.Splash && targets.Count > 0)
                {
                    var prim = targets[0];
                    foreach (var o in Units.Where(o => o.Alive && o.Side != u.Side && !targets.Contains(o)).ToList())
                        if (HexDist(prim, o) <= 1.15f)
                            targets.Add(o);
                }

                foreach (var t in targets.ToList())
                {
                    float dmg = spec.AdRatio * EffectiveAd(u) + spec.Magic * ap;
                    totalDealt += DealDamage(u, t, dmg, isAttack: false);
                    if (spec.StunDur > 0f) t.StunUntil = Time + spec.StunDur;
                    if (spec.BlindDur > 0f) t.BlindUntil = Time + spec.BlindDur;
                    if (spec.ArmorShred > 0) t.ShreddedArmor += spec.ArmorShred;
                    if (spec.DotDamage > 0f) { t.DotAmount = spec.DotDamage / Mathf.Max(0.5f, spec.DotDur); t.DotUntil = Time + spec.DotDur; }
                    if (spec.ResetManaOnKill && !t.Alive) u.Mana = 50;
                    if (spec.SummonOnKill && !t.Alive) SummonServant(u);
                }
            }

            if (spec.ShieldFlat > 0f || spec.ShieldSelfPct > 0f)
            {
                float amount = (spec.ShieldFlat + spec.ShieldSelfPct * u.MaxHp) * ap;
                if (spec.Targets > 1 && spec.AdRatio == 0f && spec.Magic == 0f)
                {
                    foreach (var ally in Units.Where(o => o.Alive && o.Side == u.Side && o != u)
                             .OrderBy(o => o.Hp / o.MaxHp).Take(spec.Targets).ToList())
                        ally.Shield += amount;
                }
                else
                {
                    u.Shield += amount;
                    if (spec.ShieldAdjacent)
                        foreach (var ally in Units.Where(o => o.Alive && o.Side == u.Side && o != u &&
                                 HexDist(u, o) <= 1.2f).ToList())
                            ally.Shield += amount * 0.6f;
                }
            }
            if (spec.HealLowest > 0f)
            {
                var ally = Units.Where(o => o.Alive && o.Side == u.Side).OrderBy(o => o.Hp / o.MaxHp).FirstOrDefault();
                if (ally != null) ally.Hp = Mathf.Min(ally.MaxHp, ally.Hp + spec.HealLowest * ap);
            }
            if (spec.HealAllLane > 0f)
                foreach (var ally in Units.Where(o => o.Alive && o.Side == u.Side).ToList())
                    ally.Hp = Mathf.Min(ally.MaxHp, ally.Hp + spec.HealAllLane * ap);
            if (spec.HealSelf > 0f) u.Hp = Mathf.Min(u.MaxHp, u.Hp + spec.HealSelf * ap);
            if (spec.HealSelfPctDamage > 0f) u.Hp = Mathf.Min(u.MaxHp, u.Hp + totalDealt * spec.HealSelfPctDamage);

            if (spec.SelfAsPct > 0f)
            {
                if (spec.BuffPermanent) u.PermAsBonus += spec.SelfAsPct;
                else { u.BuffAsAmt = spec.SelfAsPct; u.BuffAsUntil = spec.BuffDur; }
            }
            if (spec.SelfAdPct > 0f) { u.BuffAdAmt = spec.SelfAdPct; u.BuffAdUntil = spec.BuffDur; }
            if (spec.EmpowerAttacks > 0) { u.EmpowerLeft = spec.EmpowerAttacks; u.EmpowerBonus = spec.EmpowerBonusPct; u.EmpowerShred = spec.ArmorShred; }
            if (spec.TauntTarget && current != null) { current.Taunter = u; current.TauntUntil = Time + 3f; }

            if (u.HeraldRider && !u.FirstCastDone)
            {
                u.FirstCastDone = true;
                var ally = Units.Where(o => o.Alive && o.Side == u.Side).OrderBy(o => o.Hp / o.MaxHp).FirstOrDefault();
                if (ally != null) ally.Shield += u.MaxHp * u.HeraldShieldPct;
            }
        }

        void SummonServant(CombatUnit caster)
        {
            var inst = new UnitInstance(UnitCatalog.Get("scavver"));
            var servant = new CombatUnit
            {
                Source = inst, Side = caster.Side, Pos = caster.Pos + new Vector2(0.3f, 0f), IsEcho = true,
                MaxHp = 500, Hp = 500, Ad = 40, AttackSpeed = .7f, MoveSpeed = 1.6f,
                Range = 1, Armor = 20, ManaMax = 999
            };
            AddSpawn(servant);
        }

        // ------------------------------------------------------------------
        // Damage & death
        // ------------------------------------------------------------------

        float DealDamage(CombatUnit from, CombatUnit to, float raw, bool isAttack)
        {
            raw *= from.DamageAmp;
            int armor = Mathf.Max(0, to.Armor - to.ShreddedArmor + JuggArmorBonus(to));
            float reduced = raw * (100f / (100f + armor)) * OvertimeAmp;
            ApplyDamage(to, reduced);

            if (from.Lifesteal > 0f && from.Alive)
                from.Hp = Mathf.Min(from.MaxHp, from.Hp + reduced * from.Lifesteal);
            if (isAttack && to.Thorns > 0f && from.Alive)
                ApplyDamage(from, reduced * to.Thorns);
            if (!to.Alive) from.KillCount++;
            return reduced;
        }

        int JuggArmorBonus(CombatUnit u)
        {
            if (u.Source.Def.Class != UnitClass.Juggernaut || u.Hp <= u.MaxHp * 0.5f) return 0;
            int t = TraitInfo.Tier(ClassCount(u.Side, UnitClass.Juggernaut), TraitInfo.Breakpoints(UnitClass.Juggernaut));
            return new[] { 0, 20, 40 }[t];
        }

        void ApplyDamage(CombatUnit to, float amount)
        {
            if (!to.Alive) return;
            to.JustHit = true;
            if (to.Shield > 0f)
            {
                float absorbed = Mathf.Min(to.Shield, amount);
                to.Shield -= absorbed;
                amount -= absorbed;
            }
            to.Hp -= amount;
            if (to.Hp <= 0f) OnDeath(to);
        }

        void OnDeath(CombatUnit dead)
        {
            // Heartbound revive (before anything else — the death may not stick)
            if (dead.Source.Def.Sig == Signature.Heartbound && dead.ReviveOnce)
            {
                var partner = Units.FirstOrDefault(o => o.Alive && o.Side == dead.Side &&
                    o.Source.Def.Sig == Signature.Heartbound && o != dead);
                if (partner != null)
                {
                    dead.ReviveOnce = false;
                    if (partner.ReviveOnce) partner.ReviveOnce = false;
                    dead.Hp = dead.MaxHp * 0.4f;
                    return;
                }
            }

            if (!dead.IsEcho)
            {
                if (dead.Side == 0) DeathsA++; else DeathsB++;
            }

            if (dead.Source.Def.Sig == Signature.Motherlode)
            {
                bool already = dead.Side == 0 ? MotherlodeA : MotherlodeB;
                if (!already)
                {
                    if (dead.Side == 0) MotherlodeA = true; else MotherlodeB = true;
                    foreach (var o in Units.Where(o => o.Alive && o.Side != dead.Side &&
                             HexDist(dead, o) <= 2.2f).ToList())
                        ApplyDamage(o, dead.Ad * 2.5f);
                }
            }
        }

        // ------------------------------------------------------------------
        // Movement & end
        // ------------------------------------------------------------------

        void CheckEnd()
        {
            int aliveA = Units.Count(u => u.Alive && u.Side == 0);
            int aliveB = Units.Count(u => u.Alive && u.Side == 1);

            if (aliveA > 0 && aliveB > 0 && Time < GameConfig.FightMaxSeconds) return;

            Finished = true;
            if (aliveA > aliveB) { Result = FightResult.SideAWins; WinnerSurvivors = aliveA; LoserWiped = aliveB == 0; }
            else if (aliveB > aliveA) { Result = FightResult.SideBWins; WinnerSurvivors = aliveB; LoserWiped = aliveA == 0; }
            else
            {
                float hpA = 0f, maxA = 0f, hpB = 0f, maxB = 0f;
                foreach (var u in Units)
                {
                    if (!u.Alive) continue;
                    if (u.Side == 0) { hpA += u.Hp; maxA += u.MaxHp; }
                    else { hpB += u.Hp; maxB += u.MaxHp; }
                }
                float fracA = maxA > 0f ? hpA / maxA : 0f;
                float fracB = maxB > 0f ? hpB / maxB : 0f;
                if (fracA > fracB + 0.05f) { Result = FightResult.SideAWins; WinnerSurvivors = aliveA; LoserWiped = false; }
                else if (fracB > fracA + 0.05f) { Result = FightResult.SideBWins; WinnerSurvivors = aliveB; LoserWiped = false; }
                else Result = FightResult.Draw;
            }
        }

        /// <summary>Alive, non-echo survivors with a given origin (for economy payouts).</summary>
        public bool AnySurvivor(int side, Origin origin) =>
            Units.Any(u => u.Alive && u.Side == side && !u.IsEcho &&
                (u.Source.Def.Origin == origin || u.Source.Def.Origin2 == origin));

        public bool AnySurvivorSig(int side, Signature sig) =>
            Units.Any(u => u.Alive && u.Side == side && !u.IsEcho && u.Source.Def.Sig == sig);

        public int KillsBySig(int side, Signature sig) =>
            Units.Where(u => u.Side == side && u.Source.Def.Sig == sig).Sum(u => u.KillCount);
    }
}
