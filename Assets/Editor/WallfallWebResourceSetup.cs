using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wallfall.EditorTools
{
    /// <summary>
    /// Builds the ignored, local-only Resources bundle from the ignored itch.io source packs.
    /// The third-party files are intentionally not committed to the repository.
    /// </summary>
    public static class WallfallWebResourceSetup
    {
        const string SourceRoot = "Assets/itchio/";
        const string ResourceRoot = "Assets/Resources/Wallfall/itchio/";
        const string RuntimeRoot = "Wallfall/itchio/";

        static readonly string[] DirectAssets =
        {
            // Unit animation sheets.
            "Tiny RPG Character Asset Pack 01 v2.0 -Free Soldier&Orc/Characters(100x100 split)/Soldier/Soldier/Soldier_Idle.png",
            "Tiny RPG Character Asset Pack 01 v2.0 -Free Soldier&Orc/Characters(100x100 split)/Orc/Orc/Orc_Idle.png",
            "Tiny RPG Character Asset Pack 02 -Free Demon_A&Blood Monster_A/Characters(100x100 split)/Demon_A/Demon_A/Demon_A_Idle.png",
            "Tiny RPG Character Asset Pack 02 -Free Demon_A&Blood Monster_A/Characters(100x100 split)/Blood Monster_A/Blood Monster_A/Blood Monster_A_Idle.png",
            "Monsters_Creatures_Fantasy/Goblin/Idle.png",
            "Monsters_Creatures_Fantasy/Skeleton/Idle.png",
            "Monsters_Creatures_Fantasy/Mushroom/Idle.png",
            "Monsters_Creatures_Fantasy/Flying eye/Flight.png",
            "free-pixel-art-tiny-hero-sprites/1 Pink_Monster/Pink_Monster_Idle_4.png",
            "free-pixel-art-tiny-hero-sprites/2 Owlet_Monster/Owlet_Monster_Idle_4.png",
            "free-pixel-art-tiny-hero-sprites/3 Dude_Monster/Dude_Monster_Idle_4.png",
            "Cute_Fantasy_Free/Enemies/Slime_Green.png",
            "Cute_Fantasy_Free/Enemies/Skeleton.png",
            "Cute_Fantasy_Free/Player/Player.png",
            "2dpixeldungeon/Enemy_Animations_Set/enemies-skeleton1_idle.png",
            "2dpixeldungeon/Enemy_Animations_Set/enemies-skeleton2_idle.png",
            "2dpixeldungeon/Enemy_Animations_Set/enemies-vampire_idle.png",

            // Board and menu backgrounds.
            "backgrounds/free-city-backgrounds-pixel-art/city 3/7.png",
            "Nature Landscapes Free Pixel Art/nature_6/orig.png",
            "Nature Landscapes Free Pixel Art/nature_3/orig.png",
            "MountainDuskGodot/MountainsLayers/sky.png",
            "MountainDuskGodot/MountainsLayers/far-clouds.png",
            "MountainDuskGodot/MountainsLayers/far-mountains.png",
            "MountainDuskGodot/MountainsLayers/mountains.png",
            "MountainDuskGodot/MountainsLayers/trees.png",

            // Projectile, dust, and UI atlases.
            "Tiny RPG Character Asset Pack 01 v2.0 -Free Soldier&Orc/Arrow(Projectile)/Arrow01(32x32).png",
            "free-pixel-art-tiny-hero-sprites/1 Pink_Monster/Double_Jump_Dust_5.png",
            "Pixel UI pack 3/00.png",
            "Pixel UI pack 3/02.png",

            // Trait, ability, and market icons.
            "28_Pixel Art_Skill/sprite_png/Ice skill icon 2.png",
            "28_Pixel Art_Skill/sprite_png/dragon roar skill icon.png",
            "28_Pixel Art_Skill/sprite_png/plant skill icon.png",
            "28_Pixel Art_Skill/sprite_png/healing skill icon 2.png",
            "28_Pixel Art_Skill/sprite_png/dragon charges skill icon.png",
            "28_Pixel Art_Skill/sprite_png/dragon wing skill icon.png",
            "28_Pixel Art_Skill/sprite_png/dragon tail icon.png",
            "28_Pixel Art_Skill/sprite_png/plant skill icon 4.png",
            "28_Pixel Art_Skill/sprite_png/slash skill icon 4.png",
            "28_Pixel Art_Skill/sprite_png/fire skill icon.png",
            "28_Pixel Art_Skill/sprite_png/plant skill icon 2.png",
            "28_Pixel Art_Skill/sprite_png/Ice skill icon 3.png",
            "28_Pixel Art_Skill/sprite_png/slash skill icon.png",
            "28_Pixel Art_Skill/sprite_png/fire skill icon 3.png",
            "28_Pixel Art_Skill/sprite_png/Ice skill icon 4.png",
            "28_Pixel Art_Skill/sprite_png/slash skill icon 3.png",
            "28_Pixel Art_Skill/sprite_png/Poison skill icon.png",
            "28_Pixel Art_Skill/sprite_png/healing skill icon 3.png",
            "28_Pixel Art_Skill/sprite_png/plant skill icon 3.png",
            "28_Pixel Art_Skill/sprite_png/fire skill icon 2.png",
            "28_Pixel Art_Skill/sprite_png/healing skill icon 4.png",
            "28_Pixel Art_Skill/sprite_png/Ice skill icon.png",
            "28_Pixel Art_Skill/sprite_png/slash skill icon 2.png",
            "28_Pixel Art_Skill/sprite_png/healing skill icon.png",

            // UI audio.
            "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - Cursor - 1.ogg",
            "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - Cursor - 2.ogg",
            "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - Cursor - 3.ogg",
            "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - Cursor - 5.ogg",
            "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - Select - 1.ogg",
            "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - Select - 2.ogg",
            "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - Error - 1.ogg",
            "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - Popup Open - 1.ogg",
            "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - Popup Close - 1.ogg",
            "JDSherbert - Ultimate UI SFX Pack (FREE)/Mono/ogg/JDSherbert - Ultimate UI SFX Pack - Swipe - 1.ogg",

            // Font and soundtrack.
            "pokemon-ds-font.otf/pokemon-ds-font.otf",
            "MountainDuskGodot/Music/summer nights.ogg",
        };

        static readonly string[] MythicIds =
        {
            "001", "002", "003", "004", "005", "006", "007", "008", "009", "021", "023"
        };

        [MenuItem("WALLFALL/Web/Prepare Curated Resources")]
        public static void Prepare()
        {
            var assets = CollectAssets();
            var missingSources = new List<string>();
            var copied = 0;
            var existing = 0;

            foreach (var relative in assets)
            {
                var source = SourceRoot + relative;
                var destination = ResourceRoot + relative;
                if (AssetDatabase.LoadMainAssetAtPath(source) == null)
                {
                    missingSources.Add(source);
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? ResourceRoot);
            }

            if (missingSources.Count > 0)
                throw new InvalidOperationException("Missing WALLFALL source assets:\n" + string.Join("\n", missingSources));

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var relative in assets)
            {
                var source = SourceRoot + relative;
                var destination = ResourceRoot + relative;
                if (AssetDatabase.LoadMainAssetAtPath(destination) != null)
                {
                    existing++;
                    continue;
                }

                if (!AssetDatabase.CopyAsset(source, destination))
                    throw new InvalidOperationException($"Could not copy {source} to {destination}");
                copied++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ApplyRuntimeImportSettings(assets);
            Validate();
            Debug.Log($"WALLFALL WEB RESOURCES: ready ({assets.Count} assets, {copied} copied, {existing} already present).");
        }

        [MenuItem("WALLFALL/Web/Validate Curated Resources")]
        public static void Validate()
        {
            var assets = CollectAssets();
            var missing = new List<string>();
            foreach (var relative in assets)
            {
                var runtimePath = WithoutExtension(RuntimeRoot + relative);
                var extension = Path.GetExtension(relative).ToLowerInvariant();
                UnityEngine.Object loaded;
                switch (extension)
                {
                    case ".png": loaded = Resources.Load<Texture2D>(runtimePath); break;
                    case ".ogg": loaded = Resources.Load<AudioClip>(runtimePath); break;
                    case ".otf": loaded = Resources.Load<Font>(runtimePath); break;
                    default: loaded = Resources.Load(runtimePath); break;
                }
                if (loaded == null) missing.Add(runtimePath);
            }

            if (missing.Count > 0)
                throw new InvalidOperationException("Missing WALLFALL Resources:\n" + string.Join("\n", missing));

            Debug.Log($"WALLFALL WEB RESOURCES: validation passed ({assets.Count}/{assets.Count}).");
        }

        static List<string> CollectAssets()
        {
            var assets = new List<string>(DirectAssets);
            const string mythicDir = "Free Mythic Monsters/Free Mythic Monsters/Transparent/1x Size/";
            foreach (var id in MythicIds)
            {
                var found = 0;
                for (var frame = 1; frame <= 8; frame++)
                {
                    var relative = $"{mythicDir}{id}_{frame}.png";
                    if (AssetDatabase.LoadMainAssetAtPath(SourceRoot + relative) == null) break;
                    assets.Add(relative);
                    found++;
                }
                if (found == 0) throw new InvalidOperationException($"No source frames found for mythic unit {id}.");
            }

            return assets;
        }

        static void ApplyRuntimeImportSettings(IEnumerable<string> assets)
        {
            foreach (var relative in assets)
            {
                var destination = ResourceRoot + relative;
                if (AssetImporter.GetAtPath(destination) is TextureImporter texture)
                {
                    texture.textureType = TextureImporterType.Default;
                    texture.filterMode = FilterMode.Point;
                    texture.mipmapEnabled = false;
                    texture.textureCompression = TextureImporterCompression.Uncompressed;
                    texture.alphaIsTransparency = true;
                    texture.SaveAndReimport();
                }
                else if (AssetImporter.GetAtPath(destination) is AudioImporter audio)
                {
                    var settings = audio.defaultSampleSettings;
                    settings.loadType = relative.EndsWith("summer nights.ogg", StringComparison.Ordinal)
                        ? AudioClipLoadType.CompressedInMemory
                        : AudioClipLoadType.DecompressOnLoad;
                    settings.preloadAudioData = true;
                    audio.defaultSampleSettings = settings;
                    audio.SaveAndReimport();
                }
            }
            AssetDatabase.SaveAssets();
        }

        static string WithoutExtension(string path)
        {
            var slash = path.LastIndexOf('/');
            var dot = path.LastIndexOf('.');
            return dot > slash ? path.Substring(0, dot) : path;
        }
    }
}
