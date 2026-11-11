#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MadnessDreams.EditorTools
{
    /// <summary>
    /// Genera prefabs, PropDefinitionSO y ZoneThemeSO a partir de los PNG individuales de
    /// Downloads/Props, organiza los props por tema (Forest/Sea/Cave/Ruins/Winter) y por
    /// categoría (Props/Barriers) y los cablea en el WorldConfig del mundo Sea para que
    /// aparezcan en la partida.
    /// Menú: Tools > Madness Dreams > Props > Build Prop Packs (idempotente).
    /// Batch: -executeMethod MadnessDreams.EditorTools.PropPackBuilder.BuildAll
    /// </summary>
    public static class PropPackBuilder
    {
        private const string SourceRoot = "C:/Users/donov/Downloads/Props";
        private const string ArtRoot = "Assets/Art/Props/Packs";
        private const string PrefabRoot = "Assets/Prefabs/Props/Packs";
        private const string SoRoot = "Assets/ScriptableObjects/Props/Packs";
        private const string ThemesRoot = "Assets/ScriptableObjects/Themes";
        private const string WorldSeaPath = "Assets/ScriptableObjects/Sample/World_Sea.asset";
        private const string DefaultSpriteMaterialGuid = "a97c105638bdf8b4a8650670310a4cd3";
        private const float MaxBarrierUnits = 4f;
        private const float PropZ = -0.5f;

        private sealed class Rule
        {
            public string Theme;
            public bool Barrier;
            public int Weight;
            public bool AllowWater;
            public string Pattern;
        }

        private static readonly Rule[] Rules =
        {
            // ---- Forest (tag 1) ----
            new Rule { Theme = "Forest", Barrier = true,  Weight = 2, Pattern = @"^Trees_texture_shadow/(Tree|Moss_tree|Flower_tree|Fruit_tree|Autumn_tree)\d*\.png$" },
            new Rule { Theme = "Forest", Barrier = true,  Weight = 1, Pattern = @"^Arboles/(Ent_man|Ent_woman|Mega_tree1|Mega_tree2|Light_balls_tree\d*|Living gazebo\d*)\.png$" },
            new Rule { Theme = "Forest", Barrier = false, Weight = 3, Pattern = @"^Arbustos_texture_shadow/Bush_simple\d_\d\.png$" },
            new Rule { Theme = "Forest", Barrier = false, Weight = 2, Pattern = @"^Arbustos_texture_shadow/Bush_(blue|orange|pink|red)_flowers\d\.png$" },
            new Rule { Theme = "Forest", Barrier = false, Weight = 2, Pattern = @"^Arbustos_texture_shadow/Fern[12]_\d\.png$" },
            new Rule { Theme = "Forest", Barrier = false, Weight = 2, Pattern = @"^Arbustos_texture_shadow/Autumn_bush\d\.png$" },
            new Rule { Theme = "Forest", Barrier = false, Weight = 1, Pattern = @"^Arboles/(White-red_mushroom|Beige_green_mushroom|Chanterelles)\d*\.png$" },
            new Rule { Theme = "Forest", Barrier = false, Weight = 2, Pattern = @"^Trees_texture_shadow/Broken_tree[1-6]\.png$" },
            new Rule { Theme = "Forest", Barrier = false, Weight = 2, Pattern = @"^Objects_separately/Rock[1-6]_grass_shadow\d\.png$" },

            // ---- Sea (tag 2) ----
            new Rule { Theme = "Sea", Barrier = true,  Weight = 2, Pattern = @"^Trees_texture_shadow/Palm_tree\d_\d\.png$" },
            new Rule { Theme = "Sea", Barrier = true,  Weight = 2, Pattern = @"^Arboles/(Willow|Blue-green_balls_tree|White_tree)\d*\.png$" },
            new Rule { Theme = "Sea", Barrier = false, Weight = 2, AllowWater = true, Pattern = @"^Objects_separately/Rock7_water_shadow\d\.png$" },
            new Rule { Theme = "Sea", Barrier = false, Weight = 1, AllowWater = true, Pattern = @"^Objects_separately/Rock7_water_shadow_dark\d\.png$" },

            // ---- Cave (tag 3) ----
            new Rule { Theme = "Cave", Barrier = true,  Weight = 2, Pattern = @"^Arboles/Curved_tree\d*\.png$" },
            new Rule { Theme = "Cave", Barrier = true,  Weight = 1, Pattern = @"^Arboles/Luminous_tree\d*\.png$" },
            new Rule { Theme = "Cave", Barrier = true,  Weight = 1, Pattern = @"^Arboles/Swirling tree\d*\.png$" },
            new Rule { Theme = "Cave", Barrier = false, Weight = 1, Pattern = @"^Objects_separately/Rock[1-6]_grass_shadow_dark\d\.png$" },
            new Rule { Theme = "Cave", Barrier = false, Weight = 1, Pattern = @"^Objects_separately/Rock8_ground_shadow_dark\d\.png$" },

            // ---- Ruins (tag 4) ----
            new Rule { Theme = "Ruins", Barrier = true,  Weight = 2, Pattern = @"^Trees_texture_shadow/(Broken_tree7|Burned_tree\d*)\.png$" },
            new Rule { Theme = "Ruins", Barrier = true,  Weight = 1, Pattern = @"^Arboles/Tree_idol_(deer|dragon|human|wolf)\.png$" },
            new Rule { Theme = "Ruins", Barrier = false, Weight = 2, Pattern = @"^Arbustos_texture_shadow/(Broken_tree|Burned_tree)\d\.png$" },
            new Rule { Theme = "Ruins", Barrier = false, Weight = 2, Pattern = @"^Objects_separately/Rock[1-8]_\d\.png$" },
            new Rule { Theme = "Ruins", Barrier = false, Weight = 1, Pattern = @"^Objects_separately/Rock8_ground_shadow\d\.png$" },

            // ---- Winter (tag 5, Snow) ----
            new Rule { Theme = "Winter", Barrier = true,  Weight = 2, Pattern = @"^Trees_texture_shadow/(Christmas_tree|Snow_christmass_tree|Snow_tree)\d\.png$" },
            new Rule { Theme = "Winter", Barrier = false, Weight = 2, Pattern = @"^Arbustos_texture_shadow/Cactus[12]_\d\.png$" },
            new Rule { Theme = "Winter", Barrier = false, Weight = 3, Pattern = @"^Arbustos_texture_shadow/Snow_bush\d\.png$" },
            new Rule { Theme = "Winter", Barrier = false, Weight = 2, Pattern = @"^Objects_separately/Rokc3_snow_shadow\d\.png$" },
            new Rule { Theme = "Winter", Barrier = false, Weight = 1, Pattern = @"^Objects_separately/Rokc3_snow_shadow_dark\d\.png$" },
        };

        private sealed class BuiltProp
        {
            public Rule Rule;
            public string Source;
            public string ArtPath;
            public string PrefabPath;
            public string SoPath;
            public PropDefinitionSO So;
            public int MaxPx;
        }

        [MenuItem("Tools/Madness Dreams/Props/Build Prop Packs")]
        public static void BuildAll()
        {
            Debug.Log("[PropPackBuilder] Inicio: " + DateTime.Now.ToLongTimeString());

            List<BuiltProp> built = new List<BuiltProp>();

            string[] sources = Directory.GetFiles(SourceRoot, "*.png", SearchOption.AllDirectories);
            foreach (string source in sources)
            {
                string rel = source.Substring(SourceRoot.Length + 1).Replace('\\', '/');
                Rule rule = MatchRule(rel);
                if (rule == null)
                {
                    continue;
                }

                BuiltProp prop = BuildProp(source, rel, rule);
                if (prop != null)
                {
                    built.Add(prop);
                }
            }

            Debug.Log("[PropPackBuilder] Props generados: " + built.Count);

            BuildThemes(built);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            WriteCatalog(built);

            Debug.Log("[PropPackBuilder] Listo: prefabs, SOs y temas generados y cableados en World_Sea.");
        }

        private static Rule MatchRule(string rel)
        {
            for (int i = 0; i < Rules.Length; i++)
            {
                if (Regex.IsMatch(rel, Rules[i].Pattern))
                {
                    return Rules[i];
                }
            }

            return null;
        }

        private static BuiltProp BuildProp(string source, string rel, Rule rule)
        {
            try
            {
                string fileName = Path.GetFileName(source);
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                string category = rule.Barrier ? "Barriers" : "Props";
                string safeName = baseName.Replace(' ', '_');

                string artDir = ArtRoot + "/" + rule.Theme + "/" + category;
                string prefabDir = PrefabRoot + "/" + rule.Theme + "/" + category;
                string soDir = SoRoot + "/" + rule.Theme + "/" + category;

                string artPath = artDir + "/" + fileName;
                string prefabPath = prefabDir + "/" + safeName + ".prefab";
                string soPath = soDir + "/" + safeName + ".asset";

                Directory.CreateDirectory(artDir);
                Directory.CreateDirectory(prefabDir);
                Directory.CreateDirectory(soDir);

                if (!File.Exists(artPath))
                {
                    File.Copy(source, artPath, overwrite: false);
                }

                AssetDatabase.ImportAsset(artPath, ImportAssetOptions.ForceSynchronousImport);
                ConfigureImporter(artPath);
                AssetDatabase.ImportAsset(artPath, ImportAssetOptions.ForceSynchronousImport);

                TextureImporter ti = AssetImporter.GetAtPath(artPath) as TextureImporter;
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(artPath);
                if (sprite == null || ti == null)
                {
                    Debug.LogWarning("[PropPackBuilder] No se pudo importar sprite: " + rel);
                    return null;
                }

                int maxPx = Mathf.Max(sprite.texture.width, sprite.texture.height);
                float units = maxPx / 16f;
                Vector2Int footprint = rule.Barrier
                    ? new Vector2Int(Mathf.Clamp(Mathf.CeilToInt(units), 2, 4), Mathf.Clamp(Mathf.CeilToInt(units), 2, 4))
                    : new Vector2Int(Mathf.Clamp(Mathf.CeilToInt(units), 1, 2), Mathf.Clamp(Mathf.CeilToInt(units), 1, 2));

                GameObject prefab = CreatePrefab(prefabPath, safeName, sprite, rule.Barrier ? Mathf.Min(1f, MaxBarrierUnits / units) : 1f, rule.Barrier);
                PropDefinitionSO so = CreatePropSo(soPath, safeName, prefab, rule, footprint);

                return new BuiltProp
                {
                    Rule = rule,
                    Source = rel,
                    ArtPath = artPath,
                    PrefabPath = prefabPath,
                    SoPath = soPath,
                    So = so,
                    MaxPx = maxPx,
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError("[PropPackBuilder] Error con " + rel + ": " + e.Message);
                return null;
            }
        }

        private static void ConfigureImporter(string path)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null)
            {
                return;
            }

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16f;
            ti.spritePivot = new Vector2(0.5f, 0.5f);
            TextureImporterSettings settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.Tight;
            ti.SetTextureSettings(settings);
            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.isReadable = false;
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
        }

        private static GameObject CreatePrefab(string prefabPath, string safeName, Sprite sprite, float scale, bool barrier)
        {
            Material material = null;
            string materialPath = AssetDatabase.GUIDToAssetPath(DefaultSpriteMaterialGuid);
            if (!string.IsNullOrEmpty(materialPath))
            {
                material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            }

            GameObject go = new GameObject(safeName);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            go.transform.position = new Vector3(0f, 0f, PropZ);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 4;
            if (material != null)
            {
                sr.sharedMaterial = material;
            }

            if (barrier)
            {
                go.layer = LayerMask.NameToLayer("Walls");
                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(sprite.bounds.size.x * scale, sprite.bounds.size.y * scale);
            }

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            UnityEngine.Object.DestroyImmediate(go);
            return saved;
        }

        private static PropDefinitionSO CreatePropSo(string soPath, string safeName, GameObject prefab, Rule rule, Vector2Int footprint)
        {
            PropDefinitionSO existing = AssetDatabase.LoadAssetAtPath<PropDefinitionSO>(soPath);
            PropDefinitionSO so = existing != null ? existing : ScriptableObject.CreateInstance<PropDefinitionSO>();
            so.name = safeName;

            SerializedObject s = new SerializedObject(so);
            s.FindProperty("prefab").objectReferenceValue = prefab;
            s.FindProperty("kind").intValue = rule.Barrier ? (int)PropKind.Barrier : (int)PropKind.Decor;
            s.FindProperty("footprint").vector2IntValue = footprint;
            s.FindProperty("weight").intValue = rule.Weight;
            s.FindProperty("avoidPassages").boolValue = true;
            s.FindProperty("allowWater").boolValue = rule.AllowWater;
            s.ApplyModifiedPropertiesWithoutUndo();

            if (existing == null)
            {
                AssetDatabase.CreateAsset(so, soPath);
            }
            else
            {
                EditorUtility.SetDirty(so);
            }

            return so;
        }

        private static void BuildThemes(List<BuiltProp> built)
        {
            ZoneThemeSO forest = LoadOrCreateTheme("ZoneTheme_Forest", RoomTag.Forest);
            ZoneThemeSO sea = LoadOrCreateTheme("ZoneTheme_Sea", RoomTag.Sea);
            ZoneThemeSO cave = LoadOrCreateTheme("ZoneTheme_Cave", RoomTag.Cave);
            ZoneThemeSO ruins = LoadOrCreateTheme("ZoneTheme_Ruins", RoomTag.Ruins);
            ZoneThemeSO winter = LoadOrCreateTheme("ZoneTheme_Winter", RoomTag.Snow);

            CopyEnemyBank(forest, cave);
            CopyEnemyBank(forest, ruins);
            CopyEnemyBank(forest, winter);

            ClearBank(forest, "propBank");
            ClearBank(forest, "barrierBank");

            // Idempotencia: quitar entradas de packs previas antes de volver a agregarlas al tema Sea.
            RemovePackEntries(sea, "propBank");
            RemovePackEntries(sea, "barrierBank");

            Dictionary<string, ZoneThemeSO> themes = new Dictionary<string, ZoneThemeSO>
            {
                { "Forest", forest },
                { "Sea", sea },
                { "Cave", cave },
                { "Ruins", ruins },
                { "Winter", winter },
            };

            foreach (BuiltProp prop in built)
            {
                ZoneThemeSO theme = themes[prop.Rule.Theme];
                AddBankEntry(theme, prop.Rule.Barrier ? "barrierBank" : "propBank", prop.So, prop.Rule.Weight);
            }

            SetDensities(forest, 4, 8, 1, 3);
            SetDensities(sea, 4, 8, 1, 3);
            SetDensities(cave, 4, 8, 1, 3);
            SetDensities(ruins, 4, 8, 1, 3);
            SetDensities(winter, 4, 8, 1, 3);

            EditorUtility.SetDirty(forest);
            EditorUtility.SetDirty(sea);
            EditorUtility.SetDirty(cave);
            EditorUtility.SetDirty(ruins);
            EditorUtility.SetDirty(winter);

            WireWorldThemes(sea, forest, cave, ruins, winter);
        }

        private static ZoneThemeSO LoadOrCreateTheme(string name, RoomTag tag)
        {
            string path = ThemesRoot + "/" + name + ".asset";
            ZoneThemeSO theme = AssetDatabase.LoadAssetAtPath<ZoneThemeSO>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ZoneThemeSO>();
                AssetDatabase.CreateAsset(theme, path);
            }

            theme.name = name;
            SerializedObject s = new SerializedObject(theme);
            s.FindProperty("tag").enumValueIndex = (int)tag;
            s.ApplyModifiedPropertiesWithoutUndo();
            return theme;
        }

        private static void CopyEnemyBank(ZoneThemeSO from, ZoneThemeSO to)
        {
            SerializedObject sf = new SerializedObject(from);
            SerializedObject st = new SerializedObject(to);
            SerializedProperty src = sf.FindProperty("enemyBank");
            SerializedProperty dst = st.FindProperty("enemyBank");
            dst.ClearArray();
            for (int i = 0; i < src.arraySize; i++)
            {
                dst.InsertArrayElementAtIndex(i);
                SerializedProperty d = dst.GetArrayElementAtIndex(i);
                SerializedProperty s = src.GetArrayElementAtIndex(i);
                d.FindPropertyRelative("archetype").objectReferenceValue = s.FindPropertyRelative("archetype").objectReferenceValue;
                d.FindPropertyRelative("weight").intValue = s.FindPropertyRelative("weight").intValue;
            }

            st.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ClearBank(ZoneThemeSO theme, string field)
        {
            SerializedObject s = new SerializedObject(theme);
            SerializedProperty bank = s.FindProperty(field);
            bank.ClearArray();
            s.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RemovePackEntries(ZoneThemeSO theme, string field)
        {
            SerializedObject s = new SerializedObject(theme);
            SerializedProperty bank = s.FindProperty(field);
            for (int i = bank.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty entry = bank.GetArrayElementAtIndex(i);
                UnityEngine.Object prop = entry.FindPropertyRelative("prop").objectReferenceValue;
                if (prop == null || AssetDatabase.GetAssetPath(prop).StartsWith(SoRoot))
                {
                    bank.DeleteArrayElementAtIndex(i);
                }
            }

            s.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddBankEntry(ZoneThemeSO theme, string field, PropDefinitionSO prop, int weight)
        {
            SerializedObject s = new SerializedObject(theme);
            SerializedProperty bank = s.FindProperty(field);
            int index = bank.arraySize;
            bank.InsertArrayElementAtIndex(index);
            SerializedProperty entry = bank.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("prop").objectReferenceValue = prop;
            entry.FindPropertyRelative("weight").intValue = weight;
            s.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetDensities(ZoneThemeSO theme, int propsMin, int propsMax, int barriersMin, int barriersMax)
        {
            SerializedObject s = new SerializedObject(theme);
            s.FindProperty("propsMin").intValue = propsMin;
            s.FindProperty("propsMax").intValue = propsMax;
            s.FindProperty("barriersMin").intValue = barriersMin;
            s.FindProperty("barriersMax").intValue = barriersMax;
            s.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireWorldThemes(ZoneThemeSO sea, ZoneThemeSO forest, ZoneThemeSO cave, ZoneThemeSO ruins, ZoneThemeSO winter)
        {
            WorldConfigSO world = AssetDatabase.LoadAssetAtPath<WorldConfigSO>(WorldSeaPath);
            if (world == null)
            {
                Debug.LogWarning("[PropPackBuilder] No se encontró " + WorldSeaPath + "; no se cablearon los temas.");
                return;
            }

            SerializedObject s = new SerializedObject(world);
            SerializedProperty list = s.FindProperty("themes");
            list.ClearArray();

            AddThemeEntry(list, RoomTag.Sea, sea, 4);
            AddThemeEntry(list, RoomTag.Forest, forest, 3);
            AddThemeEntry(list, RoomTag.Cave, cave, 1);
            AddThemeEntry(list, RoomTag.Ruins, ruins, 1);
            AddThemeEntry(list, RoomTag.Snow, winter, 1);

            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(world);
        }

        private static void AddThemeEntry(SerializedProperty list, RoomTag tag, ZoneThemeSO theme, int weight)
        {
            int index = list.arraySize;
            list.InsertArrayElementAtIndex(index);
            SerializedProperty entry = list.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("tag").enumValueIndex = (int)tag;
            entry.FindPropertyRelative("theme").objectReferenceValue = theme;
            entry.FindPropertyRelative("weight").intValue = weight;
        }

        private static void WriteCatalog(List<BuiltProp> built)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Vaciado de Props — Packs temáticos");
            sb.AppendLine();
            sb.AppendLine("Generado por `PropPackBuilder` a partir de `Downloads/Props`.");
            sb.AppendLine("Los props se dividen en **Props** (decorativos, arbustos/rocas) y **Barriers** (árboles y estructuras que bloquean el paso).");
            sb.AppendLine();
            sb.AppendLine("## Organización");
            sb.AppendLine();
            sb.AppendLine("- `Assets/Art/Props/Packs/<Tema>/<Barriers|Props>/` → PNGs");
            sb.AppendLine("- `Assets/Prefabs/Props/Packs/<Tema>/<Barriers|Props>/` → prefabs");
            sb.AppendLine("- `Assets/ScriptableObjects/Props/Packs/<Tema>/` → PropDefinitionSO");
            sb.AppendLine("- `Assets/ScriptableObjects/Themes/ZoneTheme_<Tema>.asset` → bancos ponderados");
            sb.AppendLine();

            string[] themes = { "Forest", "Sea", "Cave", "Ruins", "Winter" };
            string[] tagNames = { "Forest (1)", "Sea (2)", "Cave (3)", "Ruins (4)", "Winter/Snow (5)" };

            for (int t = 0; t < themes.Length; t++)
            {
                sb.AppendLine("## Tema: " + themes[t] + " — tag " + tagNames[t]);
                sb.AppendLine();
                sb.AppendLine("| Archivo | Categoría | Peso | Tamaño px |");
                sb.AppendLine("|---|---|---|---|");
                foreach (BuiltProp prop in built)
                {
                    if (prop.Rule.Theme == themes[t])
                    {
                        sb.Append("| ").Append(prop.Source).Append(" | ")
                          .Append(prop.Rule.Barrier ? "Barrier" : "Prop").Append(" | ")
                          .Append(prop.Rule.Weight).Append(" | ")
                          .Append(prop.MaxPx).Append("px |").AppendLine();
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine("## Notas");
            sb.AppendLine();
            sb.AppendLine("- Fuente de los PNG: `Downloads/Props` (pack craftpix). Se usan las variantes `*_texture_shadow` (mayor detalle) y se omiten las `*_no_shadow` (duplicados sin sombra).");
            sb.AppendLine("- Los `Barriers` validan conectividad al colocarse (no sellan el cuarto); su huella se limita a 4x4 celdas y su escala a 4 unidades.");
            sb.AppendLine("- Los `Barriers` llevan `BoxCollider2D` en layer `Walls` y `sortingOrder` 4, así el jugador no puede atravesarlos (igual que las barriers de granja).");
            sb.AppendLine("- Todos los prefabs se generan con `z = -0.5` y `sortingOrder 4`: se dibujan por encima del jugador (orden 3) y enemigos (orden 2), ocultando las piernas al pasar por detrás. Los proyectiles usan orden 5 para verse sobre los props.");
            sb.AppendLine("- `RoomContentSpawner` conserva la Z del prefab al instanciar (los props quedan más cerca de la cámara).");
            sb.AppendLine("- Los props de agua (`Rock7_water_*`) tienen `allowWater = true`.");
            sb.AppendLine("- `World_Sea` usa los 5 temas con pesos Sea 4, Forest 3, Cave 1, Ruins 1, Winter 1.");

            string catalogPath = ArtRoot + "/Vaciado_Props.md";
            File.WriteAllText(catalogPath, sb.ToString(), new UTF8Encoding(false));
            AssetDatabase.ImportAsset(catalogPath);
        }
    }
}
#endif
