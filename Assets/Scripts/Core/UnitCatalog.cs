using System.Collections.Generic;
using UnityEngine;

namespace Wallfall
{
    public class UnitDef
    {
        public string Id, Name;
        public int Cost;
        public Origin Origin;
        public Origin? Origin2;      // Karst counts as two origins
        public UnitClass Class;
        public Signature Sig = Signature.None;
        public int SylvanWeight = 1; // Verdanth counts as 3 Sylvan

        public float Hp, Ad, AttackSpeed, MoveSpeed = 1.6f;
        public int Range, Armor, ManaStart, ManaMax;
        public AbilitySpec Ability;

        public string SpriteKey;     // base sheet in SpriteBank
        public Color Tint = Color.white;

        public UnitDef(string id, string name, int cost, Origin origin, UnitClass cls,
                       float hp, float ad, float atkSpd, int range, int manaStart, int manaMax,
                       string spriteKey, string tintHex, AbilitySpec ability)
        {
            Id = id; Name = name; Cost = cost; Origin = origin; Class = cls;
            Hp = hp; Ad = ad; AttackSpeed = atkSpd; Range = range;
            ManaStart = manaStart; ManaMax = manaMax;
            SpriteKey = spriteKey;
            Ability = ability;
            Armor = (cls == UnitClass.Bulwark || cls == UnitClass.Juggernaut) ? 45
                  : (cls == UnitClass.Duelist || cls == UnitClass.Assassin) ? 30 : 20;
            if (!string.IsNullOrEmpty(tintHex)) ColorUtility.TryParseHtmlString(tintHex, out Tint);
        }
    }

    /// <summary>Set 1 "The Four Fronts" — 60 units. See WALLFALL_units.md for the design doc.</summary>
    public static class UnitCatalog
    {
        static AbilitySpec A(string name, string desc) => new AbilitySpec { Name = name, Desc = desc };

        public static readonly List<UnitDef> All = BuildAll();

        static List<UnitDef> BuildAll()
        {
            var list = new List<UnitDef>();

            // ============ 1-COSTS (13) ============
            list.Add(new UnitDef("smelt", "Smelt", 1, Origin.Foundry, UnitClass.Juggernaut, 650, 50, .60f, 1, 0, 70, "orc", "#E0955F",
                new AbilitySpec { Name = "Mule Kick", Desc = "180% AD to target", AdRatio = 1.8f }));
            list.Add(new UnitDef("sparks", "Sparks", 1, Origin.Foundry, UnitClass.Gunner, 500, 52, .70f, 3, 0, 80, "playercute", "#FFD68A",
                new AbilitySpec { Name = "Rivet Gun", Desc = "Next 3 attacks: +40% AD and shred 5 Armor", EmpowerAttacks = 3, EmpowerBonusPct = 0.4f, ArmorShred = 5 }));
            list.Add(new UnitDef("shard", "Shard", 1, Origin.Prospect, UnitClass.Arcanist, 480, 40, .65f, 3, 10, 60, "eye", "#9FE8F0",
                new AbilitySpec { Name = "Glint", Desc = "190 magic; +30 if target below half HP", Magic = 190 }));
            list.Add(new UnitDef("sprout", "Sprout", 1, Origin.Sylvan, UnitClass.Herald, 520, 42, .65f, 2, 20, 70, "mushroom", "",
                new AbilitySpec { Name = "Sprout", Desc = "Heal the lowest-HP ally 180", HealLowest = 180 }));
            list.Add(new UnitDef("thorn", "Thorn", 1, Origin.Sylvan, UnitClass.Duelist, 560, 48, .75f, 1, 0, 90, "slime", "",
                new AbilitySpec { Name = "Bramble Swipe", Desc = "170% AD; bleeds 60 over 3s", AdRatio = 1.7f, DotDamage = 60, DotDur = 3 }));
            list.Add(new UnitDef("filch", "Filch", 1, Origin.Gilded, UnitClass.Assassin, 500, 50, .70f, 1, 0, 80, "goblin", "",
                new AbilitySpec { Name = "Pickpocket", Desc = "160% AD to target", AdRatio = 1.6f }));
            list.Add(new UnitDef("bellhop", "Bellhop", 1, Origin.Gilded, UnitClass.Herald, 520, 40, .65f, 2, 20, 60, "pink", "",
                new AbilitySpec { Name = "Room Service", Desc = "Shield the lowest ally 200", ShieldFlat = 200 }));
            list.Add(new UnitDef("gatepup", "Gatepup", 1, Origin.Wallguard, UnitClass.Bulwark, 700, 42, .60f, 1, 0, 60, "soldier", "",
                new AbilitySpec { Name = "Guard Stance", Desc = "300 self-shield, taunts target", ShieldSelfPct = 0, ShieldFlat = 300, TauntTarget = true }));
            list.Add(new UnitDef("rampart", "Rampart", 1, Origin.Wallguard, UnitClass.Sniper, 470, 52, .70f, 4, 0, 90, "bonecute", "",
                new AbilitySpec { Name = "Pot Shot", Desc = "200% AD to the farthest enemy", AdRatio = 2.0f, Mode = TargetMode.Farthest }));
            list.Add(new UnitDef("ember", "Ember", 1, Origin.Ruinborn, UnitClass.Assassin, 490, 52, .75f, 1, 0, 85, "demon", "",
                new AbilitySpec { Name = "Cinder Step", Desc = "Blink behind target, 170% AD", AdRatio = 1.7f, BlinkBehind = true }));
            list.Add(new UnitDef("gale", "Gale", 1, Origin.Stormcaller, UnitClass.Duelist, 540, 46, .80f, 1, 0, 90, "dungeon2", "#BFE8FF",
                new AbilitySpec { Name = "Tailwind", Desc = "+40% attack speed for 4s", SelfAsPct = 0.4f, BuffDur = 4 }));
            list.Add(new UnitDef("dune", "Dune", 1, Origin.Caravan, UnitClass.Juggernaut, 640, 48, .65f, 1, 0, 75, "dude", "",
                new AbilitySpec { Name = "Sandslam", Desc = "180% AD splashing to neighbors", AdRatio = 1.8f, Splash = true }));
            list.Add(new UnitDef("packmule", "Packmule", 1, Origin.Caravan, UnitClass.Bulwark, 690, 40, .60f, 1, 0, 65, "blood", "#C89A6A",
                new AbilitySpec { Name = "Overloaded", Desc = "280 self-shield", ShieldFlat = 280 }));

            // ============ 2-COSTS (13) ============
            list.Add(new UnitDef("golem", "Golem", 2, Origin.Foundry, UnitClass.Bulwark, 850, 55, .55f, 1, 0, 75, "myth021", "",
                new AbilitySpec { Name = "Molten Core", Desc = "350 shield; nearby enemies burn 90 over 3s", ShieldFlat = 350, DotDamage = 90, DotDur = 3, Splash = true }));
            list.Add(new UnitDef("rivet", "Rivet", 2, Origin.Foundry, UnitClass.Duelist, 700, 58, .75f, 1, 0, 90, "playercute", "#C8D2E8",
                new AbilitySpec { Name = "Nail Flurry", Desc = "3 rapid hits of 70% AD", AdRatio = 2.1f }));
            list.Add(new UnitDef("glimmer", "Glimmer", 2, Origin.Prospect, UnitClass.Herald, 600, 48, .65f, 3, 20, 70, "owlet", "",
                new AbilitySpec { Name = "Dazzle", Desc = "220 magic; blinds target 1.5s", Magic = 220, BlindDur = 1.5f }));
            list.Add(new UnitDef("mole", "Mole", 2, Origin.Prospect, UnitClass.Gunner, 620, 60, .70f, 3, 0, 85, "myth002", "",
                new AbilitySpec { Name = "Drill Bolt", Desc = "210% AD, pierces behind", AdRatio = 2.1f, Pierce = true }));
            list.Add(new UnitDef("fletch", "Fletch", 2, Origin.Sylvan, UnitClass.Sniper, 580, 62, .75f, 4, 0, 90, "playercute", "#9CD489",
                new AbilitySpec { Name = "Seeker Arrow", Desc = "230% AD to lowest-HP enemy", AdRatio = 2.3f, Mode = TargetMode.LowestHp }));
            list.Add(new UnitDef("moss", "Moss", 2, Origin.Sylvan, UnitClass.Juggernaut, 820, 58, .60f, 1, 0, 80, "blood", "#6B9E4E",
                new AbilitySpec { Name = "Maul", Desc = "200% AD; heals half the damage", AdRatio = 2.0f, HealSelfPctDamage = 0.5f }));
            list.Add(new UnitDef("scrapper", "Scrapper", 2, Origin.Gilded, UnitClass.Duelist, 720, 60, .80f, 1, 0, 95, "goblin", "#E8C86A",
                new AbilitySpec { Name = "Crowd Pleaser", Desc = "190% AD to target", AdRatio = 1.9f }));
            list.Add(new UnitDef("toll", "Toll", 2, Origin.Gilded, UnitClass.Assassin, 640, 62, .75f, 1, 0, 85, "goblin", "#B08AE0",
                new AbilitySpec { Name = "Shakedown", Desc = "200% AD; shreds 10 Armor", AdRatio = 2.0f, ArmorShred = 10 }));
            list.Add(new UnitDef("brick", "Brick", 2, Origin.Wallguard, UnitClass.Bulwark, 900, 50, .55f, 1, 0, 70, "dungeon1", "",
                new AbilitySpec { Name = "Hold the Line", Desc = "380 shield, split with the ally behind", ShieldFlat = 380, ShieldAdjacent = true }));
            list.Add(new UnitDef("lyra", "Lyra", 2, Origin.Wallguard, UnitClass.Herald, 620, 50, .70f, 3, 20, 70, "pink", "#FFD1E8",
                new AbilitySpec { Name = "Lifeline", Desc = "Heal the lowest ally 260", HealLowest = 260 }) { Sig = Signature.Heartbound });
            list.Add(new UnitDef("scavver", "Scavver", 2, Origin.Ruinborn, UnitClass.Gunner, 610, 62, .70f, 3, 0, 90, "skeleton", "",
                new AbilitySpec { Name = "Bone Shrapnel", Desc = "220% AD splashing behind target", AdRatio = 2.2f, Splash = true }));
            list.Add(new UnitDef("harrier", "Harrier", 2, Origin.Stormcaller, UnitClass.Sniper, 590, 64, .75f, 4, 0, 95, "eye", "#A8C8FF",
                new AbilitySpec { Name = "Dive Bolt", Desc = "240% AD to target", AdRatio = 2.4f }));
            list.Add(new UnitDef("sirocco", "Sirocco", 2, Origin.Caravan, UnitClass.Arcanist, 610, 50, .65f, 3, 10, 75, "slime", "#E8D8A0",
                new AbilitySpec { Name = "Mirage", Desc = "240 magic to 2 nearest enemies", Magic = 240, Targets = 2, Mode = TargetMode.Nearest }));

            // ============ 3-COSTS (13) ============
            list.Add(new UnitDef("anvil", "Anvil", 3, Origin.Foundry, UnitClass.Bulwark, 1050, 65, .60f, 1, 0, 80, "soldier", "#AEB8CC",
                new AbilitySpec { Name = "Anvil Drop", Desc = "280 magic around target, stuns 1s", Magic = 280, Splash = true, StunDur = 1f }));
            list.Add(new UnitDef("slag", "Slag", 3, Origin.Foundry, UnitClass.Arcanist, 750, 60, .65f, 3, 15, 80, "demon", "#FF8A5F",
                new AbilitySpec { Name = "Slag Spray", Desc = "300 magic to 3 enemies; shreds 10 Armor", Magic = 300, Targets = 3, ArmorShred = 10 }));
            list.Add(new UnitDef("facet", "Facet", 3, Origin.Prospect, UnitClass.Arcanist, 760, 62, .65f, 3, 15, 75, "eye", "#D8F0FF",
                new AbilitySpec { Name = "Refraction", Desc = "320 magic split among 3 enemies", Magic = 320, Targets = 3 }));
            list.Add(new UnitDef("lode", "Lode", 3, Origin.Prospect, UnitClass.Juggernaut, 950, 70, .65f, 1, 0, 85, "blood", "#8AB8D8",
                new AbilitySpec { Name = "Headlamp Rush", Desc = "240% AD to the farthest enemy in reach", AdRatio = 2.4f, Mode = TargetMode.Farthest, BlinkBehind = true }));
            list.Add(new UnitDef("willow", "Willow", 3, Origin.Sylvan, UnitClass.Arcanist, 800, 58, .60f, 3, 20, 85, "mushroom", "#A8E0A0",
                new AbilitySpec { Name = "Rootgrasp", Desc = "280 magic to 2 enemies, stuns 1.5s", Magic = 280, Targets = 2, StunDur = 1.5f }));
            list.Add(new UnitDef("briar", "Briar", 3, Origin.Sylvan, UnitClass.Assassin, 780, 74, .80f, 1, 0, 90, "myth003", "#8FCF7A",
                new AbilitySpec { Name = "Thorn Ambush", Desc = "230% AD; refunds mana on kill", AdRatio = 2.3f, ResetManaOnKill = true }));
            list.Add(new UnitDef("gavel", "Gavel", 3, Origin.Gilded, UnitClass.Herald, 740, 55, .65f, 3, 25, 80, "vampire", "#E8C87A",
                new AbilitySpec { Name = "Going Once", Desc = "Shield 2 allies 280 each", ShieldFlat = 280, Targets = 2 }));
            list.Add(new UnitDef("pothound", "Pothound", 3, Origin.Gilded, UnitClass.Juggernaut, 980, 72, .65f, 1, 0, 90, "blood", "#FFD447",
                new AbilitySpec { Name = "Beg", Desc = "480 self-heal and +25% AD for 5s", HealSelf = 480, SelfAdPct = 0.25f, BuffDur = 5 }) { Sig = Signature.Potluck });
            list.Add(new UnitDef("murus", "Murus", 3, Origin.Wallguard, UnitClass.Juggernaut, 1000, 70, .60f, 1, 0, 85, "orc", "#9AA6B8",
                new AbilitySpec { Name = "Battering Charge", Desc = "250% AD; +50% vs shields", AdRatio = 2.5f }));
            list.Add(new UnitDef("strix", "Strix", 3, Origin.Wallguard, UnitClass.Sniper, 700, 76, .70f, 4, 0, 95, "eye", "#E8E0C8",
                new AbilitySpec { Name = "Overwatch", Desc = "260% AD to target", AdRatio = 2.6f }));
            list.Add(new UnitDef("cinder", "Cinder", 3, Origin.Ruinborn, UnitClass.Arcanist, 770, 62, .65f, 3, 15, 85, "demon", "#B88AA0",
                new AbilitySpec { Name = "Ashfall", Desc = "300 magic to 2 enemies, burns 90 over 3s", Magic = 300, Targets = 2, DotDamage = 90, DotDur = 3 }));
            list.Add(new UnitDef("tempest", "Tempest", 3, Origin.Stormcaller, UnitClass.Duelist, 820, 74, .85f, 1, 0, 95, "dungeon2", "#8AB8FF",
                new AbilitySpec { Name = "Lightning Rounds", Desc = "Next 4 attacks chain 60 magic", EmpowerAttacks = 4, EmpowerBonusPct = 0.8f }));
            list.Add(new UnitDef("convoy", "Convoy", 3, Origin.Caravan, UnitClass.Gunner, 750, 76, .70f, 3, 0, 90, "dude", "#E0B87A",
                new AbilitySpec { Name = "Convoy Volley", Desc = "240% AD to target", AdRatio = 2.4f }));

            // ============ 4-COSTS (13) ============
            list.Add(new UnitDef("furnace", "Furnace", 4, Origin.Foundry, UnitClass.Gunner, 950, 90, .70f, 3, 0, 100, "myth004", "",
                new AbilitySpec { Name = "Overheat", Desc = "Next 6 attacks: +70% AD, splash", EmpowerAttacks = 6, EmpowerBonusPct = 0.7f }));
            list.Add(new UnitDef("matriarch", "Matriarch", 4, Origin.Foundry, UnitClass.Juggernaut, 1250, 85, .65f, 1, 0, 90, "myth001", "",
                new AbilitySpec { Name = "Foundry's Embrace", Desc = "500 shield to self and adjacent allies", ShieldFlat = 500, ShieldAdjacent = true }));
            list.Add(new UnitDef("prisma", "Prisma", 4, Origin.Prospect, UnitClass.Arcanist, 900, 78, .65f, 3, 20, 90, "myth006", "",
                new AbilitySpec { Name = "Prism Lance", Desc = "480 magic piercing line", Magic = 480, Pierce = true }));
            list.Add(new UnitDef("auger", "Auger", 4, Origin.Prospect, UnitClass.Assassin, 920, 95, .80f, 1, 0, 95, "vampire", "#9FD8E8",
                new AbilitySpec { Name = "Burrow Strike", Desc = "Blink to backline, 280% AD", AdRatio = 2.8f, BlinkBehind = true, Mode = TargetMode.Backline }));
            list.Add(new UnitDef("oakheart", "Oakheart", 4, Origin.Sylvan, UnitClass.Bulwark, 1400, 75, .55f, 1, 0, 85, "myth008", "",
                new AbilitySpec { Name = "Deep Roots", Desc = "600 shield; regenerates while shielded", ShieldFlat = 600 }));
            list.Add(new UnitDef("fury", "Fury", 4, Origin.Sylvan, UnitClass.Duelist, 1000, 92, .85f, 1, 0, 100, "myth003", "",
                new AbilitySpec { Name = "Wildwrath", Desc = "260% AD; +10% AS permanently per cast", AdRatio = 2.6f, SelfAsPct = 0.1f, BuffPermanent = true }));
            list.Add(new UnitDef("magnate", "Magnate", 4, Origin.Gilded, UnitClass.Gunner, 950, 96, .75f, 3, 0, 100, "vampire", "#FFD447",
                new AbilitySpec { Name = "Money Shot", Desc = "300% AD; crits if you bank 20+ gold", AdRatio = 3.0f }));
            list.Add(new UnitDef("duchess", "Duchess", 4, Origin.Gilded, UnitClass.Arcanist, 880, 76, .65f, 3, 20, 95, "owlet", "#E8C8FF",
                new AbilitySpec { Name = "Compound Interest", Desc = "380 magic, repeats at +50% if target lives", Magic = 380, Targets = 1 }));
            list.Add(new UnitDef("bastion", "Bastion", 4, Origin.Wallguard, UnitClass.Herald, 920, 70, .65f, 3, 25, 85, "soldier", "#FFF0C8",
                new AbilitySpec { Name = "Sanctify", Desc = "Heal all lane allies 220", HealAllLane = 220 }));
            list.Add(new UnitDef("bram", "Bram", 4, Origin.Wallguard, UnitClass.Bulwark, 1450, 82, .60f, 1, 0, 90, "dungeon1", "#FFD1A8",
                new AbilitySpec { Name = "Vow", Desc = "550 shield; Lyra shares it", ShieldFlat = 550, ShieldAdjacent = true }) { Sig = Signature.Heartbound });
            list.Add(new UnitDef("vestige", "Vestige", 4, Origin.Ruinborn, UnitClass.Herald, 890, 72, .65f, 3, 25, 90, "bonecute", "#D8CFE8",
                new AbilitySpec { Name = "Eulogy", Desc = "350 magic to 3 enemies", Magic = 350, Targets = 3 }));
            list.Add(new UnitDef("eyewall", "Eyewall", 4, Origin.Stormcaller, UnitClass.Sniper, 860, 100, .75f, 5, 0, 110, "myth009", "",
                new AbilitySpec { Name = "Hurricane Bolt", Desc = "320% AD to the farthest enemy", AdRatio = 3.2f, Mode = TargetMode.Farthest }));
            list.Add(new UnitDef("horizon", "Horizon", 4, Origin.Caravan, UnitClass.Duelist, 1050, 94, .85f, 1, 0, 100, "dude", "#8FE8D8",
                new AbilitySpec { Name = "Long Haul", Desc = "270% AD dash; shields 20% max HP", AdRatio = 2.7f, ShieldSelfPct = 0.2f, BlinkBehind = true }));

            // ============ 5-COSTS (8) ============
            list.Add(new UnitDef("wallbreaker", "Wallbreaker", 5, Origin.Ruinborn, UnitClass.Juggernaut, 1750, 120, .70f, 1, 0, 100, "orc", "#E05F5F",
                new AbilitySpec { Name = "Demolish", Desc = "350% AD in a line through the target", AdRatio = 3.5f, Pierce = true }) { Sig = Signature.Breacher });
            list.Add(new UnitDef("aurelia", "Aurelia", 5, Origin.Gilded, UnitClass.Sniper, 1300, 125, .80f, 5, 0, 110, "pink", "#FFE089",
                new AbilitySpec { Name = "Golden Volley", Desc = "5 shots of 110% AD among enemies", AdRatio = 1.1f, Targets = 5, Mode = TargetMode.Nearest }) { Sig = Signature.GoldenToll });
            list.Add(new UnitDef("verdanth", "Verdanth", 5, Origin.Sylvan, UnitClass.Arcanist, 1650, 110, .70f, 2, 30, 100, "myth023", "",
                new AbilitySpec { Name = "Emerald Breath", Desc = "550 magic to 3 enemies", Magic = 550, Targets = 3 }) { Sig = Signature.Dragonsoul, SylvanWeight = 3 });
            list.Add(new UnitDef("karst", "Karst", 5, Origin.Foundry, UnitClass.Bulwark, 1800, 100, .60f, 1, 0, 90, "myth021", "#C8E8F0",
                new AbilitySpec { Name = "Tectonic Slam", Desc = "400 magic around target, 1.5s stun", Magic = 400, Splash = true, StunDur = 1.5f }) { Sig = Signature.Motherlode, Origin2 = Origin.Prospect });
            list.Add(new UnitDef("mirrormarch", "Mirrormarch", 5, Origin.Caravan, UnitClass.Assassin, 1350, 118, .85f, 1, 0, 95, "myth005", "",
                new AbilitySpec { Name = "Phantom Waltz", Desc = "Blink + 300% AD", AdRatio = 3.0f, BlinkBehind = true }) { Sig = Signature.Omnipresent });
            list.Add(new UnitDef("vessa", "Vessa", 5, Origin.Stormcaller, UnitClass.Arcanist, 1400, 105, .75f, 3, 25, 105, "myth007", "",
                new AbilitySpec { Name = "Tempest Crown", Desc = "500 magic to 4 enemies", Magic = 500, Targets = 4 }) { Sig = Signature.PerfectStorm });
            list.Add(new UnitDef("gravekeeper", "Gravekeeper", 5, Origin.Ruinborn, UnitClass.Arcanist, 1500, 110, .70f, 3, 20, 100, "skeleton", "#A08AC8",
                new AbilitySpec { Name = "Last Rites", Desc = "650 magic; kills raise a Bone Servant", Magic = 650, SummonOnKill = true }));
            list.Add(new UnitDef("mortar", "Mortar", 5, Origin.Wallguard, UnitClass.Bulwark, 1900, 108, .55f, 1, 0, 95, "dungeon1", "#F0E8D0",
                new AbilitySpec { Name = "Mortar Wall", Desc = "700 shield to self and adjacent allies", ShieldFlat = 700, ShieldAdjacent = true }) { Sig = Signature.LivingWall });

            return list;
        }

        static Dictionary<string, UnitDef> _byId;
        public static UnitDef Get(string id)
        {
            if (_byId == null)
            {
                _byId = new Dictionary<string, UnitDef>();
                foreach (var d in All) _byId[d.Id] = d;
            }
            return _byId[id];
        }
    }
}
