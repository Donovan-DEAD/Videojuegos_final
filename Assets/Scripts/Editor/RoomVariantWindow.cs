using UnityEditor;
using UnityEngine;

namespace MadnessDreams
{
    /// <summary>
    /// Ventana para generar variantes de cuartos con aperturas garantizadas y pintado por rol,
    /// renderizar previews y validar los chunks.
    /// </summary>
    public class RoomVariantWindow : EditorWindow
    {
        private TilesetCatalogSO catalog;
        private TilePainterProfileSO profile;
        private string templatesFolder = "Assets/ScriptableObjects/Rooms/Sea";
        private string outputFolder = "Assets/ScriptableObjects/Rooms/Sea/Generated";
        private string bankPath = "Assets/ScriptableObjects/Sample/Sea/Bank_Sea_Generated.asset";
        private WorldConfigSO world;
        private int seed = 20241;
        private int variantsPerSize = 3;
        private string sizes = "1x1, 2x1, 1x2, 2x2, 3x3";
        private string previewFolder = "Assets/Art/Previews/Rooms/Sea";
        private Vector2 scroll;

        [MenuItem("Tools/Madness Dreams/Room Variants")]
        public static void Open()
        {
            GetWindow<RoomVariantWindow>("Room Variants");
        }

        private void OnEnable()
        {
            minSize = new Vector2(540f, 500f);
            if (catalog == null) catalog = AssetDatabase.LoadAssetAtPath<TilesetCatalogSO>("Assets/ScriptableObjects/Tileset/Tileset_Sea.asset");
            if (profile == null) profile = AssetDatabase.LoadAssetAtPath<TilePainterProfileSO>("Assets/ScriptableObjects/Tileset/TilePainterProfile_Sea.asset");
            if (world == null) world = AssetDatabase.LoadAssetAtPath<WorldConfigSO>("Assets/ScriptableObjects/Sample/World_Sea.asset");
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.HelpBox(
                "Genera variantes sin sobrescribir los cuartos base.\n" +
                "Cada celda de 16 tiles lleva aperturas de 4 tiles (6..9, incluye 8 y 9) en cada borde.\n" +
                "El pintado usa el perfil de roles: piso centro/borde y muro cuerpo/remate.",
                MessageType.Info);

            catalog = (TilesetCatalogSO)EditorGUILayout.ObjectField("Catálogo", catalog, typeof(TilesetCatalogSO), false);
            profile = (TilePainterProfileSO)EditorGUILayout.ObjectField("Perfil de pintado", profile, typeof(TilePainterProfileSO), false);
            FolderField("Carpeta de plantillas", ref templatesFolder);
            FolderField("Carpeta de salida", ref outputFolder);
            bankPath = EditorGUILayout.TextField("Banco a cablear", bankPath);
            world = (WorldConfigSO)EditorGUILayout.ObjectField("Mundo a cablear", world, typeof(WorldConfigSO), false);

            EditorGUILayout.Space();
            seed = EditorGUILayout.IntField("Semilla", seed);
            variantsPerSize = Mathf.Max(1, EditorGUILayout.IntField("Variantes por tamaño", variantsPerSize));
            sizes = EditorGUILayout.TextField("Tamaños (1x1, 2x1, ...)", sizes);
            FolderField("Carpeta de previews", ref previewFolder);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(catalog == null || profile == null))
            {
                if (GUILayout.Button("Generar variantes", GUILayout.Height(30f)))
                {
                    RoomVariantGenerator.Generate(
                        catalog,
                        profile,
                        templatesFolder,
                        outputFolder,
                        bankPath,
                        world,
                        seed,
                        variantsPerSize,
                        TilesetTools.ParseSizes(sizes));
                }
            }

            if (GUILayout.Button("Renderizar previews de la carpeta de salida"))
            {
                int count = RoomPreviewRenderer.RenderFolder(outputFolder, previewFolder, 8);
                Debug.Log($"RoomVariantWindow: {count} preview(s) en '{previewFolder}'.");
            }

            if (GUILayout.Button("Validar chunks (anillo/tamaño)"))
            {
                MapChunkTools.ValidateChunks();
            }

            EditorGUILayout.EndScrollView();
        }

        private static void FolderField(string label, ref string value)
        {
            EditorGUILayout.BeginHorizontal();
            value = EditorGUILayout.TextField(label, value);
            if (GUILayout.Button("…", GUILayout.Width(24f)))
            {
                string picked = EditorUtility.OpenFolderPanel(label, Application.dataPath, string.Empty);
                if (!string.IsNullOrEmpty(picked))
                {
                    string project = System.IO.Path.GetDirectoryName(Application.dataPath).Replace('\\', '/');
                    string normalized = picked.Replace('\\', '/');
                    if (normalized.StartsWith(project + "/")) value = normalized.Substring(project.Length + 1);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
