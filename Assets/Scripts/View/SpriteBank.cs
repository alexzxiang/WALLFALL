using System.Collections.Generic;
using UnityEngine;

namespace Wallfall
{
    /// <summary>
    /// Loads the curated runtime art bundle and slices it at runtime. The source packs remain
    /// outside the player; only assets copied beneath Assets/Resources/Wallfall ship in builds.
    /// Every accessor is null-safe so procedural placeholders remain a diagnostic fallback.
    /// </summary>
    public static class SpriteBank
    {
        const string Root = "Wallfall/itchio/";

        class UnitArt
        {
            public string Path;              // idle sheet path (or frame pattern with {0})
            public int FrameW, FrameH;       // frame size in pixels (0 = whole texture)
            public float WorldHeight;        // world units the FULL frame spans (character ~0.95)
            public float PivotX = 0.5f;      // measured character center within the frame
            public float PivotY = 0.5f;
            public int Frames;               // 0 = autodetect columns (some sheets have empty tail frames)
            public Color Tint = Color.white; // recolor variants
            public UnitArt(string path, int fw, int fh, float wh, float px = 0.5f, float py = 0.5f, int frames = 0)
            { Path = path; FrameW = fw; FrameH = fh; WorldHeight = wh; PivotX = px; PivotY = py; Frames = frames; }
        }

        static readonly Dictionary<string, UnitArt> Units = new Dictionary<string, UnitArt>
        {
            // Keys are sprite BASE sheets; many units share a base with different tints (UnitDef.Tint).
            ["soldier"]    = new UnitArt(Root + "Tiny RPG Character Asset Pack 01 v2.0 -Free Soldier&Orc/Characters(100x100 split)/Soldier/Soldier/Soldier_Idle.png", 100, 100, 4.5f, 0.49f, 0.51f),
            ["orc"]        = new UnitArt(Root + "Tiny RPG Character Asset Pack 01 v2.0 -Free Soldier&Orc/Characters(100x100 split)/Orc/Orc/Orc_Idle.png", 100, 100, 6.0f, 0.55f, 0.51f),
            ["demon"]      = new UnitArt(Root + "Tiny RPG Character Asset Pack 02 -Free Demon_A&Blood Monster_A/Characters(100x100 split)/Demon_A/Demon_A/Demon_A_Idle.png", 100, 100, 4.8f, 0.53f, 0.51f),
            ["blood"]      = new UnitArt(Root + "Tiny RPG Character Asset Pack 02 -Free Demon_A&Blood Monster_A/Characters(100x100 split)/Blood Monster_A/Blood Monster_A/Blood Monster_A_Idle.png", 100, 100, 6.0f, 0.52f, 0.51f),
            ["goblin"]     = new UnitArt(Root + "Monsters_Creatures_Fantasy/Goblin/Idle.png", 150, 150, 3.95f, 0.50f, 0.45f),
            ["skeleton"]   = new UnitArt(Root + "Monsters_Creatures_Fantasy/Skeleton/Idle.png", 150, 150, 2.8f, 0.55f, 0.50f),
            ["mushroom"]   = new UnitArt(Root + "Monsters_Creatures_Fantasy/Mushroom/Idle.png", 150, 150, 3.8f, 0.50f, 0.45f),
            ["eye"]        = new UnitArt(Root + "Monsters_Creatures_Fantasy/Flying eye/Flight.png", 150, 150, 4.5f, 0.52f, 0.49f),
            ["pink"]       = new UnitArt(Root + "free-pixel-art-tiny-hero-sprites/1 Pink_Monster/Pink_Monster_Idle_4.png", 32, 32, 1.08f, 0.48f, 0.44f),
            ["owlet"]      = new UnitArt(Root + "free-pixel-art-tiny-hero-sprites/2 Owlet_Monster/Owlet_Monster_Idle_4.png", 32, 32, 1.15f, 0.42f, 0.41f),
            ["dude"]       = new UnitArt(Root + "free-pixel-art-tiny-hero-sprites/3 Dude_Monster/Dude_Monster_Idle_4.png", 32, 32, 1.15f, 0.48f, 0.41f),
            ["slime"]      = new UnitArt(Root + "Cute_Fantasy_Free/Enemies/Slime_Green.png", 64, 64, 3.4f, 0.51f, 0.48f, 4),
            ["bonecute"]   = new UnitArt(Root + "Cute_Fantasy_Free/Enemies/Skeleton.png", 32, 32, 1.5f, 0.52f, 0.53f, 4),
            ["playercute"] = new UnitArt(Root + "Cute_Fantasy_Free/Player/Player.png", 32, 32, 1.5f, 0.48f, 0.53f, 4),
            ["dungeon1"]   = new UnitArt(Root + "2dpixeldungeon/Enemy_Animations_Set/enemies-skeleton1_idle.png", 32, 32, 2.0f, 0.45f, 0.33f),
            ["dungeon2"]   = new UnitArt(Root + "2dpixeldungeon/Enemy_Animations_Set/enemies-skeleton2_idle.png", 32, 32, 2.0f, 0.50f, 0.33f),
            ["vampire"]    = new UnitArt(Root + "2dpixeldungeon/Enemy_Animations_Set/enemies-vampire_idle.png", 32, 32, 1.9f, 0.41f, 0.34f),
            ["myth001"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/001_{0}.png", 0, 0, 1.15f),
            ["myth002"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/002_{0}.png", 0, 0, 1.05f),
            ["myth003"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/003_{0}.png", 0, 0, 1.05f),
            ["myth004"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/004_{0}.png", 0, 0, 1.1f),
            ["myth005"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/005_{0}.png", 0, 0, 1.1f),
            ["myth006"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/006_{0}.png", 0, 0, 1.1f),
            ["myth007"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/007_{0}.png", 0, 0, 1.15f),
            ["myth008"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/008_{0}.png", 0, 0, 1.2f),
            ["myth009"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/009_{0}.png", 0, 0, 1.15f),
            ["myth021"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/021_{0}.png", 0, 0, 1.2f),
            ["myth023"]    = new UnitArt(Root + "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/023_{0}.png", 0, 0, 1.35f),
        };

        // lane -> (far layer, near layer)
        static readonly string[][] LaneBackgrounds =
        {
            new[] { Root + "backgrounds/free-city-backgrounds-pixel-art/city 3/7.png" },
            new[] { Root + "Nature Landscapes Free Pixel Art/nature_6/orig.png" },
            new[] { Root + "Nature Landscapes Free Pixel Art/nature_3/orig.png" },
            new[] { Root + "MountainDuskGodot/MountainsLayers/sky.png",
                    Root + "MountainDuskGodot/MountainsLayers/far-mountains.png",
                    Root + "MountainDuskGodot/MountainsLayers/mountains.png" },
        };

        static readonly string[] MenuBackgroundPaths =
        {
            Root + "MountainDuskGodot/MountainsLayers/sky.png",
            Root + "MountainDuskGodot/MountainsLayers/far-clouds.png",
            Root + "MountainDuskGodot/MountainsLayers/far-mountains.png",
            Root + "MountainDuskGodot/MountainsLayers/mountains.png",
            Root + "MountainDuskGodot/MountainsLayers/trees.png",
        };

        const string ArrowPath = Root + "Tiny RPG Character Asset Pack 01 v2.0 -Free Soldier&Orc/Arrow(Projectile)/Arrow01(32x32).png";
        const string DustPath = Root + "free-pixel-art-tiny-hero-sprites/1 Pink_Monster/Double_Jump_Dust_5.png";
        const string SkillIconDir = Root + "28_Pixel Art_Skill/sprite_png/";
        const string MusicPath = Root + "MountainDuskGodot/Music/summer nights.ogg";
        const string FontPath = Root + "pokemon-ds-font.otf/pokemon-ds-font.otf";

        static readonly Dictionary<string, Sprite[]> _frameCache = new Dictionary<string, Sprite[]>();
        static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        static Texture2D[] _menuBackgrounds;

        static string ResourcePath(string path)
        {
            int slash = path.LastIndexOf('/');
            int dot = path.LastIndexOf('.');
            return dot > slash ? path.Substring(0, dot) : path;
        }

        static Texture2D LoadTex(string path)
        {
            var tex = Resources.Load<Texture2D>(ResourcePath(path));
            if (tex != null) tex.filterMode = FilterMode.Point;
            return tex;
        }

        static Sprite MakeSprite(Texture2D tex, Rect rect, float worldHeight, float pivotX, float pivotY)
        {
            float ppu = rect.height / worldHeight;
            return Sprite.Create(tex, rect, new Vector2(pivotX, pivotY), ppu, 0, SpriteMeshType.FullRect);
        }

        /// <summary>Idle animation frames for a unit, or null (fallback: procedural blob).</summary>
        public static Sprite[] UnitFrames(string unitId)
        {
            if (_frameCache.TryGetValue(unitId, out var cached)) return cached;
            Sprite[] result = null;

            if (Units.TryGetValue(unitId, out var art))
            {
                if (art.Path.Contains("{0}"))
                {
                    // numbered single-frame files, autodetect count
                    var frames = new List<Sprite>();
                    for (int i = 1; i <= 8; i++)
                    {
                        var tex = LoadTex(string.Format(art.Path, i));
                        if (tex == null) break;
                        frames.Add(MakeSprite(tex, new Rect(0, 0, tex.width, tex.height), art.WorldHeight, art.PivotX, art.PivotY));
                    }
                    if (frames.Count > 0) result = frames.ToArray();
                }
                else
                {
                    var tex = LoadTex(art.Path);
                    if (tex != null)
                    {
                        int cols = Mathf.Max(1, tex.width / art.FrameW);
                        int count = art.Frames > 0 ? Mathf.Min(art.Frames, cols) : Mathf.Min(cols, 8);
                        var frames = new Sprite[count];
                        float top = tex.height - art.FrameH; // row 0 = top row of the sheet
                        for (int i = 0; i < count; i++)
                            frames[i] = MakeSprite(tex, new Rect(i * art.FrameW, top, art.FrameW, art.FrameH), art.WorldHeight, art.PivotX, art.PivotY);
                        result = frames;
                    }
                }
            }

            _frameCache[unitId] = result;
            return result;
        }

        public static Color UnitTint(string unitId) => Color.white; // tints now live on UnitDef.Tint

        /// <summary>
        /// First idle frame CROPPED to the character (many sheets are mostly transparent
        /// padding, which rendered tiny portraits). Crop size derives from the manifest's
        /// measured pivot + world height: charPixels ≈ 0.95 / WorldHeight * frameH.
        /// </summary>
        public static Sprite UnitPortrait(string unitId)
        {
            string key = "portrait" + unitId;
            if (_spriteCache.TryGetValue(key, out var cached)) return cached;

            Sprite result = null;
            var frames = UnitFrames(unitId);
            if (frames != null && frames.Length > 0)
            {
                var f = frames[0];
                if (Units.TryGetValue(unitId, out var art) && art.FrameW > 0)
                {
                    float charPix = 0.95f / art.WorldHeight * art.FrameH;
                    float size = Mathf.Clamp(charPix * 1.45f, 16f, Mathf.Min(art.FrameW, art.FrameH));
                    float cx = f.rect.x + art.PivotX * art.FrameW;
                    float cy = f.rect.y + art.PivotY * art.FrameH;
                    float x = Mathf.Clamp(cx - size * 0.5f, f.rect.x, f.rect.xMax - size);
                    float y = Mathf.Clamp(cy - size * 0.5f, f.rect.y, f.rect.yMax - size);
                    result = Sprite.Create(f.texture, new Rect(x, y, size, size),
                        new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
                }
                else result = f;
            }
            _spriteCache[key] = result;
            return result;
        }

        public static Sprite[] LaneBackground(int lane)
        {
            string key = $"lanebg{lane}";
            if (_frameCache.TryGetValue(key, out var cached)) return cached;
            var list = new List<Sprite>();
            foreach (var path in LaneBackgrounds[Mathf.Clamp(lane, 0, 3)])
            {
                var tex = LoadTex(path);
                if (tex != null)
                    list.Add(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.height / 9.5f, 0, SpriteMeshType.FullRect));
            }
            var result = list.Count > 0 ? list.ToArray() : null;
            _frameCache[key] = result;
            return result;
        }

        public static Texture2D[] MenuBackgroundLayers()
        {
            if (_menuBackgrounds != null) return _menuBackgrounds;
            var layers = new List<Texture2D>();
            foreach (var path in MenuBackgroundPaths)
            {
                var tex = LoadTex(path);
                if (tex != null) layers.Add(tex);
            }
            _menuBackgrounds = layers.ToArray();
            return _menuBackgrounds;
        }

        public static Sprite Arrow()
        {
            if (_spriteCache.TryGetValue("arrow", out var c)) return c;
            var tex = LoadTex(ArrowPath);
            var s = tex == null ? null : Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.height / 0.5f, 0, SpriteMeshType.FullRect);
            _spriteCache["arrow"] = s;
            return s;
        }

        public static Sprite[] DustFrames()
        {
            if (_frameCache.TryGetValue("dust", out var c)) return c;
            Sprite[] result = null;
            var tex = LoadTex(DustPath);
            if (tex != null)
            {
                int fw = tex.height; // square frames in a horizontal strip
                int count = Mathf.Max(1, tex.width / fw);
                result = new Sprite[count];
                for (int i = 0; i < count; i++)
                    result[i] = Sprite.Create(tex, new Rect(i * fw, 0, fw, tex.height), new Vector2(0.5f, 0.5f), fw / 0.9f, 0, SpriteMeshType.FullRect);
            }
            _frameCache["dust"] = result;
            return result;
        }


        // ---------- Pixel UI pack 3 atlas (sliced at exact measured rects) ----------

        const string UiAtlas00 = Root + "Pixel UI pack 3/00.png";
        const string UiAtlas02 = Root + "Pixel UI pack 3/02.png";

        /// <summary>Slice a sub-rect (top-left pixel coords) out of an atlas as a 9-sliced sprite.</summary>
        static Sprite Slice(string path, int px, int py, int w, int h, float border, string cacheKey)
        {
            if (_spriteCache.TryGetValue(cacheKey, out var c)) return c;
            var tex = LoadTex(path);
            Sprite s = null;
            if (tex != null)
            {
                var rect = new Rect(px, tex.height - py - h, w, h);
                s = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 24f, 0, SpriteMeshType.FullRect,
                    new Vector4(border, border, border, border));
            }
            _spriteCache[cacheKey] = s;
            return s;
        }

        /// <summary>White 24x24 cell frame with accent corners — cards, portraits, slots. Tintable.</summary>
        public static Sprite UiCellFrame() => Slice(UiAtlas02, 4, 20, 24, 24, 6, "cellframe");
        /// <summary>Blue-ringed pill — primary buttons.</summary>
        public static Sprite UiButton() => Slice(UiAtlas00, 0, 85, 48, 22, 8, "btn");
        /// <summary>Dark pill — secondary buttons / list rows.</summary>
        public static Sprite UiButtonDark() => Slice(UiAtlas00, 64, 85, 48, 22, 8, "btnDark");
        /// <summary>Brown rounded wood panel — chips and small bars.</summary>
        public static Sprite UiWoodPanel() => Slice(UiAtlas00, 128, 85, 62, 39, 13, "wood");

        /// <summary>Silver heart from the UI pack — the bed/life icon. Tints with lane colors.</summary>
        public static Sprite UiHeart() => Slice(UiAtlas00, 195, 30, 18, 16, 0, "heart");

        /// <summary>Gem buttons from the UI pack: 0 iron (tinted grey), 1 diamond, 2 emerald, 3 gold.</summary>
        public static Sprite GemIcon(int currency)
        {
            switch (currency)
            {
                case 0: return Slice(UiAtlas00, 218, 3, 18, 13, 0, "gemCyan");   // tint grey for iron
                case 1: return Slice(UiAtlas00, 218, 3, 18, 13, 0, "gemCyan");
                case 2: return Slice(UiAtlas00, 218, 51, 18, 13, 0, "gemGreen");
                default: return Slice(UiAtlas00, 218, 19, 18, 13, 0, "gemGold");
            }
        }

        public static Color GemTint(int currency) =>
            currency == 0 ? new Color(0.78f, 0.78f, 0.85f) : Color.white;

        static readonly Dictionary<string, string> MarketIconFiles = new Dictionary<string, string>
        {
            ["BedPlating"] = "Ice skill icon 2.png",
            ["WarHorn"] = "dragon roar skill icon.png",
            ["FieldRations"] = "plant skill icon.png",
            ["BedRepair"] = "healing skill icon 2.png",
            ["Rally"] = "dragon charges skill icon.png",
            ["Frenzy"] = "dragon wing skill icon.png",
            ["Windfall"] = "dragon tail icon.png",
            ["Tutor"] = "plant skill icon 4.png",
            ["Sharpen"] = "slash skill icon 4.png",
            ["Torch"] = "fire skill icon.png",
            ["Overgrowth"] = "plant skill icon 2.png",
            ["Wallback"] = "Ice skill icon 3.png",
            ["blade"] = "slash skill icon.png",
            ["crystal"] = "fire skill icon 3.png",
            ["plate"] = "Ice skill icon 4.png",
            ["boots"] = "slash skill icon 3.png",
            ["fang"] = "Poison skill icon.png",
            ["spring"] = "healing skill icon 3.png",
            ["thorn"] = "plant skill icon 3.png",
            ["scope"] = "fire skill icon 2.png",
            ["duplicate"] = "healing skill icon 4.png",
        };

        /// <summary>Generic 32x32 icon from the skill pack by filename (traits, abilities).</summary>
        public static Sprite TraitIcon(string file)
        {
            if (file == null) return null;
            string key = "ticon" + file;
            if (_spriteCache.TryGetValue(key, out var c)) return c;
            Sprite s = null;
            var tex = LoadTex(SkillIconDir + file);
            if (tex != null)
                s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 24f, 0, SpriteMeshType.FullRect);
            _spriteCache[key] = s;
            return s;
        }

        /// <summary>World-space cast icon chosen from what the ability actually does.</summary>
        public static Sprite SkillIcon(AbilitySpec spec)
        {
            string file =
                spec.HealLowest > 0f || spec.HealAllLane > 0f || spec.HealSelf > 0f ? "healing skill icon.png"
                : spec.ShieldFlat > 0f || spec.ShieldSelfPct > 0f ? "Ice skill icon.png"
                : spec.Magic > 0f ? "fire skill icon.png"
                : "slash skill icon.png";
            string key = "wskill" + file;
            if (_spriteCache.TryGetValue(key, out var c)) return c;
            Sprite s = null;
            var tex = LoadTex(SkillIconDir + file);
            if (tex != null)
                s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.height / 0.55f, 0, SpriteMeshType.FullRect);
            _spriteCache[key] = s;
            return s;
        }

        /// <summary>32x32 skill icon for a market entry key (consumable/power/item id).</summary>
        public static Sprite MarketIcon(string key)
        {
            string cacheKey = "mkt" + key;
            if (_spriteCache.TryGetValue(cacheKey, out var c)) return c;
            Sprite s = null;
            if (MarketIconFiles.TryGetValue(key, out var file))
            {
                var tex = LoadTex(SkillIconDir + file);
                if (tex != null)
                    s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 24f, 0, SpriteMeshType.FullRect);
            }
            _spriteCache[cacheKey] = s;
            return s;
        }

        const string SfxDir = Root + "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - ";
        static readonly Dictionary<string, AudioClip> _sfxCache = new Dictionary<string, AudioClip>();

        /// <summary>UI sound from the JDSherbert pack, e.g. "Select - 1", "Popup Open - 1", "Error - 1".</summary>
        public static AudioClip Sfx(string name)
        {
            if (_sfxCache.TryGetValue(name, out var c)) return c;
            var clip = Resources.Load<AudioClip>(ResourcePath(SfxDir + name + ".ogg"));
            _sfxCache[name] = clip;
            return clip;
        }

        static AudioClip _music;
        static bool _musicSearched;

        public static AudioClip MusicClip()
        {
            if (_musicSearched) return _music;
            _musicSearched = true;
            _music = Resources.Load<AudioClip>(ResourcePath(MusicPath));
            return _music;
        }

        static Font _uiFont;
        static bool _uiFontSearched;

        /// <summary>The Pokemon DS pixel font from itchio, or null (callers fall back to LegacyRuntime).</summary>
        public static Font UiFont()
        {
            if (_uiFontSearched) return _uiFont;
            _uiFontSearched = true;
            _uiFont = Resources.Load<Font>(ResourcePath(FontPath));
            return _uiFont;
        }

        /// <summary>Skill icon as a UI sprite (default PPU) for panels.</summary>
        public static Sprite PoisonIcon()
        {
            if (_spriteCache.TryGetValue("poison", out var c)) return c;
            var tex = LoadTex(SkillIconDir + "Poison skill icon.png");
            var s = tex == null ? null : Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.height / 0.55f, 0, SpriteMeshType.FullRect);
            _spriteCache["poison"] = s;
            return s;
        }
    }
}
