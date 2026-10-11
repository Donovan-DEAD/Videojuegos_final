using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MadnessDreams
{
    /// <summary>
    /// Ventana del tileset: elegís el <b>sheet</b> de piso/muro y el de objetos, marcás con el pincel
    /// qué sprite es <em>Piso</em>, <em>Muro</em>, <em>Decor</em>, <em>Barrera</em> o <em>Ignorar</em>,
    /// y construís ahí mismo el catálogo, los bloques y los props. Todo se escribe en <b>las carpetas
    /// que elijas</b>, de modo que se puedan mantener varios mundos separados sin sobrescribir nada
    /// fuera de esas carpetas.
    ///
    /// Importante: los <b>bloques se eligen por tamaño</b> en el mapa (el tag NO filtra bloques).
    /// El <b>tag</b> del cuarto se sortea en runtime desde los temas del mundo y decide qué TEMA
    /// (enemigos, props, barreras) se usa en ese cuarto.
    /// </summary>
    public class TilesetToolsWindow : EditorWindow
    {
        private enum Paint
        {
            Ignore = 0,
            Floor = 1,
            Wall = 2
        }

        private enum ObjectPaint
        {
            Ignore = 0,
            Decor = 1,
            Barrier = 2
        }

        private const float Cell = 44f;

        // Catálogo / tiles
        private TilesetCatalogSO catalog;
        private string tilesFolder = "Assets/Tiles/Generated";
        private string catalogFolder = "Assets/ScriptableObjects/Tileset";
        private string sheetPath = "Assets/Art/Tilesets/walls_floor.png";
        private int sheetColumns = 13;
        private Paint paint = Paint.Floor;

        private Sprite[] sprites = new Sprite[0];
        private Texture2D sheetTexture;
        private readonly List<int> floors = new List<int>();
        private readonly List<int> walls = new List<int>();
        private Vector2 scroll;

        // Objetos
        private string objectSheetPath = "Assets/Art/Tilesets/Objects.png";
        private ObjectPaint objectPaint = ObjectPaint.Decor;
        private Sprite[] objectSprites = new Sprite[0];
        private Texture2D objectTexture;
        private readonly List<int> decor = new List<int>();
        private readonly List<int> barriers = new List<int>();
        private Vector2 objectScroll;

        // Bloques
        private string roomsFolder = "Assets/ScriptableObjects/Rooms";
        private string blockSizes = "1x1, 2x1, 1x2, 2x2, 3x3";
        private BlockBankSO bank;
        private int varietySeed = 20240;

        // Props
        private string prefabsFolder = "Assets/Prefabs/Props";
        private string propDefsFolder = "Assets/ScriptableObjects/Props";
        private ZoneThemeSO theme;
        private int decorCount = 8;
        private int barrierCount = 6;

        private bool selectionLoaded;
        private Vector2 mainScroll;
        private int spawnGrid = 3;

        [MenuItem("Tools/Madness Dreams/Tileset Builder")]
        public static void Open()
        {
            GetWindow<TilesetToolsWindow>("Tileset Builder");
        }

        private void OnEnable()
        {
            minSize = new Vector2(560f, 600f);
            ReloadSelection();
        }

        private void ReloadSelection()
        {
            selectionLoaded = true;

            floors.Clear();
            walls.Clear();
            decor.Clear();
            barriers.Clear();

            if (catalog != null)
            {
                sheetPath = catalog.RoomSheetPath;
                sheetColumns = catalog.RoomSheetColumns;
                objectSheetPath = catalog.ObjectSheetPath;
                floors.AddRange(catalog.FloorIndices);
                walls.AddRange(catalog.WallIndices);
                decor.AddRange(catalog.DecorIndices);
                barriers.AddRange(catalog.BarrierIndices);
            }

            if (floors.Count == 0) floors.AddRange(TilesetTools.DefaultFloorIndices);
            if (walls.Count == 0) walls.AddRange(TilesetTools.DefaultWallIndices);

            LoadSheets();
        }

        private void LoadSheets()
        {
            sprites = TilesetTools.LoadSheetSprites(sheetPath, sheetColumns, out sheetTexture);
            objectSprites = TilesetTools.LoadSheetSprites(objectSheetPath, 1, out objectTexture);
        }

        private void OnGUI()
        {
            if (!selectionLoaded) ReloadSelection();

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);

            EditorGUILayout.HelpBox(
                "Marcá con el pincel: Piso/Muro (sheet de piso-muro) y Decor/Barrera (sheet de objetos).\n" +
                "Construí el catálogo para guardar la selección y los assets. Después: bloques y props.\n" +
                "Los bloques se eligen por TAMAÑO en el mapa; el TAG solo decide el TEMA (enemigos/props) y\n" +
                "se configura en el WorldConfigSO (varios temas por mundo, con peso).",
                MessageType.Info);

            catalog = (TilesetCatalogSO)EditorGUILayout.ObjectField("Catálogo", catalog, typeof(TilesetCatalogSO), false);
            if (GUILayout.Button("Cargar selección del catálogo")) ReloadSelection();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("1) Tiles de piso y muro", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sheet", GUILayout.Width(140f));
            sheetPath = EditorGUILayout.TextField(sheetPath);
            if (GUILayout.Button("…", GUILayout.Width(24f)))
            {
                string p = EditorUtility.OpenFilePanel("Sheet de piso/muro", Application.dataPath, "png");
                if (!string.IsNullOrEmpty(p)) sheetPath = ToProjectPath(p) ?? sheetPath;
            }

            EditorGUILayout.EndHorizontal();
            sheetColumns = Mathf.Max(1, EditorGUILayout.IntField("Columnas", sheetColumns));
            paint = (Paint)GUILayout.Toolbar((int)paint, new[] { "Ignorar", "Piso", "Muro" });
            DrawGrid(sprites, sheetTexture, sheetColumns, IsTileSelected, ref scroll, ToggleTile);
            EditorGUILayout.LabelField("Piso: " + floors.Count + "   |   Muro: " + walls.Count);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("2) Objetos (props y barreras)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sheet", GUILayout.Width(140f));
            objectSheetPath = EditorGUILayout.TextField(objectSheetPath);
            if (GUILayout.Button("…", GUILayout.Width(24f)))
            {
                string p = EditorUtility.OpenFilePanel("Sheet de objetos", Application.dataPath, "png");
                if (!string.IsNullOrEmpty(p)) objectSheetPath = ToProjectPath(p) ?? objectSheetPath;
            }

            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Recargar sheet de objetos")) LoadSheets();
            objectPaint = (ObjectPaint)GUILayout.Toolbar((int)objectPaint, new[] { "Ignorar", "Decor", "Barrera" });
            DrawGrid(objectSprites, objectTexture, 8, IsObjectSelected, ref objectScroll, ToggleObject);
            EditorGUILayout.LabelField("Decor: " + decor.Count + "   |   Barreras: " + barriers.Count);

            EditorGUILayout.Space();
            if (GUILayout.Button("Guardar selección en el catálogo")) SaveSelection();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("3) Construir", EditorStyles.boldLabel);
            FolderField("Carpeta de tiles", ref tilesFolder);
            FolderField("Carpeta del catálogo", ref catalogFolder);
            if (GUILayout.Button("3a) Catálogo (tiles + objetos)"))
            {
                SaveSelection();
                catalog = TilesetTools.BuildCatalog(catalog, catalogFolder, tilesFolder, sheetPath, sheetColumns);
            }

            EditorGUILayout.Space();
            FolderField("Carpeta de bloques", ref roomsFolder);
            blockSizes = EditorGUILayout.TextField("Tamaños (1x1, 2x1, ...)", blockSizes);
            bank = (BlockBankSO)EditorGUILayout.ObjectField("Banco a cablear", bank, typeof(BlockBankSO), false);
            varietySeed = EditorGUILayout.IntField("Semilla de variedad (tiles del bloque)", varietySeed);
            using (new EditorGUI.DisabledScope(catalog == null))
            {
                if (GUILayout.Button("3b) Bloques"))
                {
                    TilesetTools.BuildRooms(catalog, roomsFolder, bank, varietySeed, TilesetTools.ParseSizes(blockSizes));
                }
            }

            EditorGUILayout.Space();
            FolderField("Carpeta de prefabs", ref prefabsFolder);
            FolderField("Carpeta de definiciones", ref propDefsFolder);
            theme = (ZoneThemeSO)EditorGUILayout.ObjectField("Tema a cablear", theme, typeof(ZoneThemeSO), false);
            decorCount = Mathf.Max(0, EditorGUILayout.IntField("Props decorativos", decorCount));
            barrierCount = Mathf.Max(0, EditorGUILayout.IntField("Barreras", barrierCount));
            using (new EditorGUI.DisabledScope(catalog == null))
            {
                if (GUILayout.Button("3c) Props y barreras"))
                {
                    TilesetTools.BuildProps(catalog, prefabsFolder, propDefsFolder, theme, decorCount, barrierCount);
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Validar mundos (1 Spawn + 1 Boss)")) TilesetTools.ValidateWorlds();

            EditorGUILayout.LabelField("4) Spawn points (respaldo)", EditorStyles.boldLabel);
            spawnGrid = Mathf.Clamp(EditorGUILayout.IntField("Grilla (NxN)", spawnGrid), 1, 8);
            if (GUILayout.Button("4) Generar spawn points en los bloques de la carpeta"))
            {
                TilesetTools.GenerateSpawnsIn(roomsFolder, spawnGrid);
            }

            EditorGUILayout.EndScrollView();
        }

        private bool IsTileSelected(int i)
        {
            return floors.Contains(i) || walls.Contains(i);
        }

        private bool IsObjectSelected(int i)
        {
            return decor.Contains(i) || barriers.Contains(i);
        }

        private void ToggleTile(int i)
        {
            switch (paint)
            {
                case Paint.Floor:
                    walls.Remove(i);
                    if (!floors.Contains(i)) floors.Add(i);
                    break;
                case Paint.Wall:
                    floors.Remove(i);
                    if (!walls.Contains(i)) walls.Add(i);
                    break;
                default:
                    floors.Remove(i);
                    walls.Remove(i);
                    break;
            }
        }

        private void ToggleObject(int i)
        {
            switch (objectPaint)
            {
                case ObjectPaint.Decor:
                    barriers.Remove(i);
                    if (!decor.Contains(i)) decor.Add(i);
                    break;
                case ObjectPaint.Barrier:
                    decor.Remove(i);
                    if (!barriers.Contains(i)) barriers.Add(i);
                    break;
                default:
                    decor.Remove(i);
                    barriers.Remove(i);
                    break;
            }
        }

        private void DrawGrid(Sprite[] list, Texture2D texture, int columns, Func<int, bool> selected, ref Vector2 scrollPos, Action<int> onClick)
        {
            if (list == null || list.Length == 0 || texture == null)
            {
                EditorGUILayout.HelpBox("No se encontraron sprites. Revisá la ruta del sheet.", MessageType.Warning);
                return;
            }

            int cols = Mathf.Max(1, columns);
            int rows = Mathf.CeilToInt(list.Length / (float)cols);
            float step = Cell + 4f;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(260f));
            Rect area = GUILayoutUtility.GetRect(cols * step, rows * (step + 12f));

            for (int i = 0; i < list.Length; i++)
            {
                int cx = i % cols;
                int cy = i / cols;
                Rect cell = new Rect(area.x + cx * step, area.y + cy * (step + 12f), Cell, Cell);

                Color prev = GUI.color;
                GUI.color = selected(i) ? new Color(0.35f, 1f, 0.4f, 0.45f) : new Color(0f, 0f, 0f, 0.2f);
                GUI.DrawTexture(cell, EditorGUIUtility.whiteTexture);
                GUI.color = prev;

                DrawSpriteFit(cell, list[i], texture);

                if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) onClick(i);
                GUI.Label(new Rect(cell.x, cell.yMax, Cell, 12f), i.ToString(), EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawSpriteFit(Rect cell, Sprite sprite, Texture2D texture)
        {
            if (sprite == null || texture == null) return;

            Rect tr = sprite.textureRect;
            float scale = Mathf.Min(cell.width / tr.width, cell.height / tr.height);
            float w = tr.width * scale;
            float h = tr.height * scale;
            Rect r = new Rect(cell.x + (cell.width - w) * 0.5f, cell.y + (cell.height - h) * 0.5f, w, h);

            GUI.DrawTextureWithTexCoords(r, texture,
                new Rect(tr.x / texture.width, tr.y / texture.height, tr.width / texture.width, tr.height / texture.height));
        }

        private void SaveSelection()
        {
            if (catalog == null)
            {
                Debug.LogWarning("Tileset Builder: asigná o construí un catálogo antes de guardar la selección.");
                return;
            }

            catalog.SetRoomSheet(sheetPath, sheetColumns);
            catalog.SetObjectSheet(objectSheetPath);
            catalog.SetSelection(floors, walls);
            catalog.SetObjectSelection(decor, barriers);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private static void FolderField(string label, ref string value)
        {
            EditorGUILayout.BeginHorizontal();
            value = EditorGUILayout.TextField(label, value);
            if (GUILayout.Button("…", GUILayout.Width(24f)))
            {
                string picked = EditorUtility.OpenFolderPanel(label, Application.dataPath, string.Empty);
                if (!string.IsNullOrEmpty(picked)) value = ToProjectPath(picked) ?? value;
            }

            EditorGUILayout.EndHorizontal();
        }

        private static string ToProjectPath(string absolute)
        {
            string project = System.IO.Path.GetDirectoryName(Application.dataPath).Replace('\\', '/');
            string normalized = absolute.Replace('\\', '/');
            if (normalized.StartsWith(project + "/"))
            {
                return normalized.Substring(project.Length + 1);
            }

            EditorUtility.DisplayDialog("Fuera del proyecto", "Elegí una carpeta o archivo dentro del proyecto (Assets/...).", "OK");
            return null;
        }
    }

    /// <summary>
    /// Constructores de assets del tileset. Los invoca la ventana con la selección y las carpetas
    /// elegidas; <b>crean o actualizan</b> (nunca borran) y no tocan nada fuera de esas carpetas.
    /// </summary>
    public static class TilesetTools
    {
        private const int TilesPerCell = 16;

        /// <summary>Selección por defecto (índices sobre <c>walls_floor.png</c> con 13 columnas).</summary>
        public static readonly int[] DefaultFloorIndices = { 1, 0, 12, 13, 11, 14 };
        public static readonly int[] DefaultWallIndices = { 128, 137, 138, 154 };

        private static readonly Vector2Int[] FallbackRoomSizes =
        {
            new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2),
            new Vector2Int(2, 2), new Vector2Int(3, 3)
        };

        /// <summary>Parsea un texto tipo "1x1, 2x1, 1x2" a una lista de tamaños en celdas.</summary>
        public static List<Vector2Int> ParseSizes(string text)
        {
            var result = new List<Vector2Int>();
            if (string.IsNullOrEmpty(text)) return result;

            foreach (string part in text.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] xy = part.ToLowerInvariant().Split('x');
                if (xy.Length != 2) continue;

                int w, h;
                if (int.TryParse(xy[0], out w) && int.TryParse(xy[1], out h) && w > 0 && h > 0)
                {
                    result.Add(new Vector2Int(w, h));
                }
            }

            if (result.Count == 0)
            {
                result.AddRange(FallbackRoomSizes);
            }

            return result;
        }

        /// <summary>
        /// Carga los sprites del sheet ordenados por fila/columna (según el nombre <c>&lt;sheet&gt;_col_row</c>).
        /// El <b>índice</b> que usa el catálogo es la posición en este arreglo.
        /// </summary>
        public static Sprite[] LoadSheetSprites(string path, int columns, out Texture2D texture)
        {
            texture = null;
            var list = new List<Sprite>();
            if (string.IsNullOrEmpty(path)) return list.ToArray();

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite s) list.Add(s);
            }

            int cols = Mathf.Max(1, columns);
            bool allParsed = list.Count > 0;
            foreach (var s in list)
            {
                if (ParseIndex(s.name, cols) < 0) { allParsed = false; break; }
            }

            if (allParsed)
            {
                list.Sort((a, b) => ParseIndex(a.name, cols).CompareTo(ParseIndex(b.name, cols)));
            }
            else
            {
                list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            }

            if (list.Count > 0) texture = list[0].texture;
            return list.ToArray();
        }

        private static int ParseIndex(string name, int columns)
        {
            int last = name.LastIndexOf('_');
            if (last <= 0) return -1;
            int prev = name.LastIndexOf('_', last - 1);
            if (prev < 0) return -1;

            int col, row;
            if (int.TryParse(name.Substring(prev + 1, last - prev - 1), out col) &&
                int.TryParse(name.Substring(last + 1), out row))
            {
                return row * columns + col;
            }

            return -1;
        }

        /// <summary>
        /// Grilla <paramref name="grid"/>$\times$<paramref name="grid"/> de puntos de spawn dentro del
        /// interior de un bloque (en tiles locales). Ej.: 3$\times$3 = 9 puntos.
        /// </summary>
        public static EnemySpawnPoint[] BuildSpawnGrid(Vector2Int sizeTiles, int grid)
        {
            int n = Mathf.Clamp(grid, 1, 8);
            var points = new List<EnemySpawnPoint>();
            int x0 = 4;
            int y0 = 4;
            int x1 = Mathf.Max(x0, sizeTiles.x - 5);
            int y1 = Mathf.Max(y0, sizeTiles.y - 5);

            for (int iy = 0; iy < n; iy++)
            {
                for (int ix = 0; ix < n; ix++)
                {
                    float tx = n == 1 ? 0.5f : ix / (float)(n - 1);
                    float ty = n == 1 ? 0.5f : iy / (float)(n - 1);
                    points.Add(new EnemySpawnPoint
                    {
                        localPosition = new Vector2Int(
                            Mathf.RoundToInt(Mathf.Lerp(x0, x1, tx)),
                            Mathf.RoundToInt(Mathf.Lerp(y0, y1, ty))),
                        weight = 1
                    });
                }
            }

            return points.ToArray();
        }

        /// <summary>
        /// Genera spawn points en todos los bloques de una carpeta (sin tocar sus tiles). Se saltean los
        /// cuartos de rol Start y Boss. Devuelve cuántos bloques modificó.
        /// </summary>
        public static int GenerateSpawnsIn(string folder, int grid)
        {
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning("TilesetTools: carpeta inválida para generar spawns: " + folder);
                return 0;
            }

            int changed = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:MapChunkSO", new[] { folder }))
            {
                var room = AssetDatabase.LoadAssetAtPath<MapChunkSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (room == null || room.Role == CellRole.Start || room.Role == CellRole.Boss)
                {
                    continue;
                }

                room.SetEnemySpawns(BuildSpawnGrid(room.Size, grid));
                EditorUtility.SetDirty(room);
                changed++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Spawn points ({grid}x{grid}) generados en {changed} bloque(s) de '{folder}'.");
            return changed;
        }

        /// <summary>
        /// Crea/actualiza los assets <see cref="Tile"/> y los sprites de objetos según la selección del
        /// catálogo, y llena sus listas. Devuelve el catálogo usado (puede haberlo creado).
        /// </summary>
        public static TilesetCatalogSO BuildCatalog(TilesetCatalogSO catalog, string catalogFolder, string tilesFolder, string sheetPath, int sheetColumns)
        {
            if (string.IsNullOrEmpty(tilesFolder)) tilesFolder = "Assets/Tiles/Generated";
            if (string.IsNullOrEmpty(catalogFolder)) catalogFolder = "Assets/ScriptableObjects/Tileset";
            EnsureFolder(tilesFolder);
            EnsureFolder(catalogFolder);

            if (catalog == null)
            {
                string catalogPath = catalogFolder + "/TilesetCatalog.asset";
                catalog = AssetDatabase.LoadAssetAtPath<TilesetCatalogSO>(catalogPath);
                if (catalog == null)
                {
                    catalog = ScriptableObject.CreateInstance<TilesetCatalogSO>();
                    AssetDatabase.CreateAsset(catalog, catalogPath);
                }
            }

            if (string.IsNullOrEmpty(sheetPath)) sheetPath = catalog.RoomSheetPath;
            if (sheetColumns <= 0) sheetColumns = catalog.RoomSheetColumns;
            catalog.SetRoomSheet(sheetPath, sheetColumns);

            Texture2D texture;
            Sprite[] sheet = LoadSheetSprites(sheetPath, sheetColumns, out texture);

            List<int> floorIdx = catalog.FloorIndices.Count > 0 ? new List<int>(catalog.FloorIndices) : new List<int>(DefaultFloorIndices);
            List<int> wallIdx = catalog.WallIndices.Count > 0 ? new List<int>(catalog.WallIndices) : new List<int>(DefaultWallIndices);

            var floors = new List<TileBase>();
            foreach (int idx in floorIdx)
            {
                var tile = CreateTile(sheet, tilesFolder, idx, "floor_" + idx, false);
                if (tile != null) floors.Add(tile);
            }

            var walls = new List<TileBase>();
            foreach (int idx in wallIdx)
            {
                var tile = CreateTile(sheet, tilesFolder, idx, "wall_" + idx, true);
                if (tile != null) walls.Add(tile);
            }

            // Objetos: si hay selección se usa tal cual; si no, se cae al heurístico por área.
            Texture2D objectTexture;
            Sprite[] objects = LoadSheetSprites(catalog.ObjectSheetPath, 1, out objectTexture);
            var decor = new List<Sprite>();
            var barriers = new List<Sprite>();

            if (catalog.DecorIndices.Count > 0 || catalog.BarrierIndices.Count > 0)
            {
                foreach (int idx in catalog.DecorIndices)
                {
                    if (idx >= 0 && idx < objects.Length) decor.Add(objects[idx]);
                }

                foreach (int idx in catalog.BarrierIndices)
                {
                    if (idx >= 0 && idx < objects.Length) barriers.Add(objects[idx]);
                }
            }
            else
            {
                foreach (var sprite in objects)
                {
                    float area = sprite.rect.width * sprite.rect.height;
                    if (area >= 450f) barriers.Add(sprite);
                    else if (area >= 150f) decor.Add(sprite);
                }
            }

            var so = new SerializedObject(catalog);
            Fill(so, "floorTiles", floors);
            Fill(so, "wallTiles", walls);
            Fill(so, "decorSprites", decor);
            Fill(so, "barrierSprites", barriers);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            Debug.Log($"Catálogo '{catalog.name}': {floors.Count} de piso, {walls.Count} de muro, {decor.Count} props, {barriers.Count} barreras.", catalog);
            return catalog;
        }

        /// <summary>
        /// Genera/actualiza los bloques (por tamaño) más Spawn y Boss, y los cablea en el banco.
        /// Los bloques normales salen con <see cref="RoomTag.None"/>: el tag se sortea en runtime.
        /// </summary>
        public static void BuildRooms(TilesetCatalogSO catalog, string roomsFolder, BlockBankSO bank, int varietySeed, List<Vector2Int> sizes)
        {
            if (catalog == null)
            {
                Debug.LogError("TilesetTools: falta el catálogo (construilo primero).");
                return;
            }

            if (string.IsNullOrEmpty(roomsFolder)) roomsFolder = "Assets/ScriptableObjects/Rooms";
            EnsureFolder(roomsFolder);

            if (sizes == null || sizes.Count == 0) sizes = ParseSizes(null);

            var created = new List<MapChunkSO>();
            int seed = varietySeed;
            foreach (var cells in sizes)
            {
                created.Add(BuildRoom(catalog, roomsFolder, cells, RoomTag.None, CellRole.Normal, new System.Random(seed++)));
            }

            var start = BuildRoom(catalog, roomsFolder, new Vector2Int(1, 1), RoomTag.None, CellRole.Start, new System.Random(seed++));
            var boss = BuildRoom(catalog, roomsFolder, new Vector2Int(3, 3), RoomTag.None, CellRole.Boss, new System.Random(seed++));

            if (bank != null)
            {
                var bso = new SerializedObject(bank);
                var list = bso.FindProperty("normalChunks");
                list.arraySize = created.Count;
                for (int i = 0; i < created.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = created[i];
                bso.FindProperty("startRoom").objectReferenceValue = start;
                bso.FindProperty("bossArena").objectReferenceValue = boss;
                bso.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bank);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Bloques en '{roomsFolder}': {created.Count} normales + Spawn + Boss. Banco: {bank}", bank);
        }

        /// <summary>Crea/actualiza un prefab y su <see cref="PropDefinitionSO"/> por sprite (decor o barrera).</summary>
        public static void BuildProps(TilesetCatalogSO catalog, string prefabsFolder, string propDefsFolder, ZoneThemeSO theme, int decorCount, int barrierCount)
        {
            if (catalog == null)
            {
                Debug.LogError("TilesetTools: falta el catálogo.");
                return;
            }

            if (string.IsNullOrEmpty(prefabsFolder)) prefabsFolder = "Assets/Prefabs/Props";
            if (string.IsNullOrEmpty(propDefsFolder)) propDefsFolder = "Assets/ScriptableObjects/Props";
            EnsureFolder(prefabsFolder);
            EnsureFolder(propDefsFolder);

            var decorDefs = new List<PropDefinitionSO>();
            int i = 0;
            foreach (var sprite in catalog.DecorSprites)
            {
                if (i >= decorCount) break;
                i++;
                var def = BuildProp(sprite, false, prefabsFolder, propDefsFolder, "Prop", i);
                if (def != null) decorDefs.Add(def);
            }

            var barrierDefs = new List<PropDefinitionSO>();
            i = 0;
            foreach (var sprite in catalog.BarrierSprites)
            {
                if (i >= barrierCount) break;
                i++;
                var def = BuildProp(sprite, true, prefabsFolder, propDefsFolder, "Barrier", i);
                if (def != null) barrierDefs.Add(def);
            }

            if (theme != null)
            {
                var tso = new SerializedObject(theme);
                FillWeightedProp(tso, "propBank", decorDefs, 2);
                FillWeightedProp(tso, "barrierBank", barrierDefs, 2);
                tso.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(theme);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Props: {decorDefs.Count} decorativos + {barrierDefs.Count} barreras. Tema: {theme}", theme);
        }

        /// <summary>Comprueba que cada <see cref="WorldConfigSO"/> genere exactamente 1 cuarto de Spawn y 1 de Boss.</summary>
        public static void ValidateWorlds()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:WorldConfigSO"))
            {
                var config = AssetDatabase.LoadAssetAtPath<WorldConfigSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (config == null) continue;

                var rooms = new List<RoomPlacement>();
                var connections = new List<RoomConnection>();
                RoomLayout.Build(config, new System.Random(config.Seed), rooms, connections);

                int starts = 0, bosses = 0;
                for (int i = 0; i < rooms.Count; i++)
                {
                    if (rooms[i].role == CellRole.Start) starts++;
                    if (rooms[i].role == CellRole.Boss) bosses++;
                }

                bool ok = rooms.Count > 0 && starts == 1 && bosses == 1;
                Debug.Log($"[Worlds] {config.name}: cuartos={rooms.Count} start={starts} boss={bosses} -> {(ok ? "OK" : "REVISAR")}", config);
            }
        }

        private static MapChunkSO BuildRoom(TilesetCatalogSO catalog, string folder, Vector2Int cells, RoomTag tag, CellRole role, System.Random rng)
        {
            int w = cells.x * TilesPerCell;
            int h = cells.y * TilesPerCell;
            var tiles = new TileBase[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool border = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                    tiles[y * w + x] = border ? catalog.PickWall(rng) : catalog.PickFloor(rng);
                }
            }

            var points = new List<EnemySpawnPoint>();
            if (role == CellRole.Normal)
            {
                points.AddRange(BuildSpawnGrid(new Vector2Int(w, h), 3));
            }

            string name = role == CellRole.Start ? "Room_Start"
                : role == CellRole.Boss ? "Room_Boss"
                : "Room_" + cells.x + "x" + cells.y;
            string path = folder + "/" + name + ".asset";

            var room = AssetDatabase.LoadAssetAtPath<MapChunkSO>(path);
            if (room == null)
            {
                room = ScriptableObject.CreateInstance<MapChunkSO>();
                AssetDatabase.CreateAsset(room, path);
            }

            // Setters directos: se persisten siempre, incluso actualizando un asset existente.
            room.SetSize(new Vector2Int(w, h));
            room.SetTileData(tiles);
            room.SetSizeInCells(cells);
            room.SetTag(tag);
            room.SetRole(role);
            room.SetEnemySpawns(points.ToArray());

            EditorUtility.SetDirty(room);
            AssetDatabase.SaveAssets();
            return room;
        }

        private static PropDefinitionSO BuildProp(Sprite sprite, bool barrier, string prefabsFolder, string defsFolder, string prefix, int index)
        {
            if (sprite == null) return null;

            int tw = Mathf.CeilToInt(sprite.rect.width / (float)TilesPerCell);
            int th = Mathf.CeilToInt(sprite.rect.height / (float)TilesPerCell);
            string name = prefix + "_" + tw + "x" + th + "_" + index;

            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = barrier ? 4 : 2;

            if (barrier)
            {
                int walls = LayerMask.NameToLayer("Walls");
                if (walls >= 0) go.layer = walls;
                var box = go.AddComponent<BoxCollider2D>();
                box.size = new Vector2(sprite.rect.width / (float)TilesPerCell, sprite.rect.height / (float)TilesPerCell);
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabsFolder + "/" + name + ".prefab");
            UnityEngine.Object.DestroyImmediate(go);

            string defPath = defsFolder + "/" + name + ".asset";
            var def = AssetDatabase.LoadAssetAtPath<PropDefinitionSO>(defPath);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<PropDefinitionSO>();
                AssetDatabase.CreateAsset(def, defPath);
            }

            var dso = new SerializedObject(def);
            dso.FindProperty("prefab").objectReferenceValue = prefab;
            dso.FindProperty("kind").enumValueIndex = barrier ? 1 : 0;
            dso.FindProperty("footprint").vector2IntValue = new Vector2Int(tw, th);
            dso.FindProperty("weight").intValue = 1;
            dso.FindProperty("avoidPassages").boolValue = true;
            dso.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(def);
            return def;
        }

        private static Tile CreateTile(Sprite[] sheet, string tilesFolder, int index, string name, bool isWall)
        {
            if (index < 0 || index >= sheet.Length || sheet[index] == null)
            {
                Debug.LogWarning($"TilesetTools: índice {index} fuera del sheet ({sheet.Length} sprites). Se omite.");
                return null;
            }

            string path = tilesFolder + "/" + name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.sprite = sheet[index];
            // El TilemapCollider2D solo colisiona con tiles cuyo ColliderType != None:
            // así el piso es transitable y el muro bloquea, en el mismo Tilemap.
            tile.colliderType = isWall ? Tile.ColliderType.Grid : Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static void FillWeightedProp(SerializedObject so, string field, List<PropDefinitionSO> props, int weight)
        {
            var list = so.FindProperty(field);
            list.arraySize = props.Count;
            for (int i = 0; i < props.Count; i++)
            {
                var el = list.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("prop").objectReferenceValue = props[i];
                el.FindPropertyRelative("weight").intValue = weight;
            }
        }

        private static void Fill<T>(SerializedObject so, string field, List<T> items) where T : UnityEngine.Object
        {
            var prop = so.FindProperty(field);
            prop.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
